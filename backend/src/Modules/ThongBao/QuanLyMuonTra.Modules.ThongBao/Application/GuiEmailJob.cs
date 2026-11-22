using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using QuanLyMuonTra.Modules.SinhVien.Contracts;
using QuanLyMuonTra.Modules.TaiKhoan.Contracts;
using QuanLyMuonTra.Modules.ThongBao.Domain;
using QuanLyMuonTra.SharedKernel;

namespace QuanLyMuonTra.Modules.ThongBao.Application;

public sealed class GuiEmailJob(
    IServiceScopeFactory nhaMayPhamVi,
    IEmailSender guiEmail,
    IClock dongHo,
    ILogger<GuiEmailJob> nhatKy) : IJob
{
    public const int SoDongToiDa = 50;

    public static readonly TimeSpan HanGiuQuyenXuLy = TimeSpan.FromMinutes(5);

    private const string DaGui = "DA_GUI";
    private const string Loi = "LOI";
    private const string LoiVinhVien = "LOI_VINH_VIEN";

    public string MaJob => "J7";

    public string TenJob => "GuiEmail";

    public TimeSpan ChuKy => TimeSpan.FromMinutes(1);

    public async Task<int> ChayAsync(CancellationToken ct)
    {
        await using var phamVi = nhaMayPhamVi.CreateAsyncScope();
        var db = phamVi.ServiceProvider.GetRequiredService<DbContext>();
        var emailSv = phamVi.ServiceProvider.GetRequiredService<ISinhVienEmailReader>();
        var emailNv = phamVi.ServiceProvider.GetRequiredService<INhanVienEmailReader>();

        await PhucHoiDangGuiBoDoAsync(db, ct);

        var dsDong = await ChonDongDenLuotAsync(db, ct);
        var soDaXuLy = 0;
        foreach (var dong in dsDong)
        {
            ct.ThrowIfCancellationRequested();
            if (await XuLyMotDongAsync(db, emailSv, emailNv, dong, ct))
            {
                soDaXuLy++;
            }
        }

        return soDaXuLy;
    }

    private async Task PhucHoiDangGuiBoDoAsync(DbContext db, CancellationToken ct)
    {
        var hanChot = dongHo.UtcNow - HanGiuQuyenXuLy;

        var soHetLanThu = await db.Database.ExecuteSqlInterpolatedAsync($"""
            UPDATE thong_bao.thong_bao
            SET trang_thai_gui = 'LOI_VINH_VIEN'
            WHERE kenh_gui = 'EMAIL' AND trang_thai_gui = 'DANG_GUI'
              AND lan_thu_cuoi < {hanChot} AND so_lan_thu >= {LichGuiLaiEmail.SoLanThuToiDa}
            """, ct);
        var soPhucHoi = await db.Database.ExecuteSqlInterpolatedAsync($"""
            UPDATE thong_bao.thong_bao
            SET trang_thai_gui = 'LOI'
            WHERE kenh_gui = 'EMAIL' AND trang_thai_gui = 'DANG_GUI'
              AND lan_thu_cuoi < {hanChot} AND so_lan_thu < {LichGuiLaiEmail.SoLanThuToiDa}
            """, ct);

        if (soHetLanThu > 0)
        {
            nhatKy.LogWarning(
                "{SoDong} dòng email DANG_GUI bị bỏ dở đã hết số lần thử, chuyển LOI_VINH_VIEN.",
                soHetLanThu);
        }

        if (soPhucHoi > 0)
        {
            nhatKy.LogWarning(
                "Đã phục hồi {SoDong} dòng email DANG_GUI bị bỏ dở quá {HanPhut} phút về LOI; có thể có email gửi lặp.",
                soPhucHoi,
                HanGiuQuyenXuLy.TotalMinutes);
        }
    }

    private async Task<List<DongEmailCho>> ChonDongDenLuotAsync(DbContext db, CancellationToken ct)
    {
        var bayGio = dongHo.UtcNow;
        var gianCach1 = LichGuiLaiEmail.GianCachSauLanThu(1);
        var gianCach2 = LichGuiLaiEmail.GianCachSauLanThu(2);
        var gianCach3 = LichGuiLaiEmail.GianCachSauLanThu(3);
        var gianCach4 = LichGuiLaiEmail.GianCachSauLanThu(4);

        return await db.Database.SqlQuery<DongEmailCho>($"""
                SELECT ma_thong_bao AS "MaThongBao", loai_thong_bao AS "LoaiThongBao", noi_dung AS "NoiDung",
                       ma_sv_nhan AS "MaSvNhan", ma_nv_nhan AS "MaNvNhan", so_lan_thu AS "SoLanThu"
                FROM thong_bao.thong_bao
                WHERE kenh_gui = 'EMAIL'
                  AND (trang_thai_gui = 'CHO_GUI'
                       OR (trang_thai_gui = 'LOI'
                           AND COALESCE(lan_thu_cuoi, thoi_diem_gui) + CASE
                                   WHEN so_lan_thu <= 1 THEN {gianCach1}
                                   WHEN so_lan_thu = 2 THEN {gianCach2}
                                   WHEN so_lan_thu = 3 THEN {gianCach3}
                                   ELSE {gianCach4}
                               END <= {bayGio}))
                ORDER BY thoi_diem_gui, ma_thong_bao
                LIMIT {SoDongToiDa}
                """)
            .ToListAsync(ct);
    }

    private async Task<bool> XuLyMotDongAsync(
        DbContext db,
        ISinhVienEmailReader emailSv,
        INhanVienEmailReader emailNv,
        DongEmailCho dong,
        CancellationToken ct)
    {
        if (!await ChiemAsync(db, dong.MaThongBao, ct))
        {
            return false;
        }

        var soLanThuMoi = dong.SoLanThu + 1;
        string trangThai;
        try
        {
            var diaChi = await TraDiaChiAsync(dong, emailSv, emailNv, ct);
            if (diaChi is null)
            {
                await GhiKetQuaAsync(db, dong.MaThongBao, LoiVinhVien);
                nhatKy.LogWarning(
                    "Email {MaThongBao} loại {LoaiThongBao} không còn địa chỉ nhận, chuyển LOI_VINH_VIEN.",
                    dong.MaThongBao,
                    dong.LoaiThongBao);

                return true;
            }

            await guiEmail.GuiAsync(diaChi, MauThongBao.TieuDe(dong.LoaiThongBao), dong.NoiDung, ct);
            trangThai = DaGui;
        }
        catch (OperationCanceledException) when (ct.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception loi)
        {
            trangThai = LichGuiLaiEmail.LoiVinhVien(soLanThuMoi) ? LoiVinhVien : Loi;
            nhatKy.LogWarning(
                "Email {MaThongBao} loại {LoaiThongBao} gửi lần {SoLanThu} lỗi ({KieuLoi}): {ThongDiepLoi}",
                dong.MaThongBao,
                dong.LoaiThongBao,
                soLanThuMoi,
                loi.GetType().Name,
                loi.Message);
        }

        await GhiKetQuaAsync(db, dong.MaThongBao, trangThai);

        if (trangThai == LoiVinhVien)
        {
            nhatKy.LogWarning(
                "Email {MaThongBao} loại {LoaiThongBao} lỗi vĩnh viễn sau {SoLanThu} lần thử.",
                dong.MaThongBao,
                dong.LoaiThongBao,
                soLanThuMoi);
        }

        return true;
    }

    private async Task<bool> ChiemAsync(DbContext db, Guid maThongBao, CancellationToken ct)
    {
        var bayGio = dongHo.UtcNow;
        var soDong = await db.Database.ExecuteSqlInterpolatedAsync($"""
            UPDATE thong_bao.thong_bao
            SET trang_thai_gui = 'DANG_GUI', so_lan_thu = so_lan_thu + 1, lan_thu_cuoi = {bayGio}
            WHERE ma_thong_bao = {maThongBao} AND trang_thai_gui IN ('CHO_GUI', 'LOI')
            """, ct);

        return soDong == 1;
    }

    private async Task GhiKetQuaAsync(DbContext db, Guid maThongBao, string trangThai)
    {
        var soDong = await db.Database.ExecuteSqlInterpolatedAsync($"""
            UPDATE thong_bao.thong_bao
            SET trang_thai_gui = {trangThai}
            WHERE ma_thong_bao = {maThongBao} AND trang_thai_gui = 'DANG_GUI'
            """, CancellationToken.None);

        if (soDong == 0)
        {
            nhatKy.LogWarning(
                "Email {MaThongBao} không còn ở DANG_GUI khi ghi kết quả {TrangThai}; bỏ qua.",
                maThongBao,
                trangThai);
        }
    }

    private static async Task<string?> TraDiaChiAsync(
        DongEmailCho dong,
        ISinhVienEmailReader emailSv,
        INhanVienEmailReader emailNv,
        CancellationToken ct)
    {
        if (dong.MaSvNhan is not null)
        {
            return await emailSv.LayEmailAsync(dong.MaSvNhan, ct);
        }

        if (dong.MaNvNhan is not null)
        {
            return await emailNv.LayEmailAsync(dong.MaNvNhan.Value, ct);
        }

        return null;
    }

    public sealed class DongEmailCho
    {
        public Guid MaThongBao { get; set; }

        public string LoaiThongBao { get; set; } = string.Empty;

        public string NoiDung { get; set; } = string.Empty;

        public string? MaSvNhan { get; set; }

        public Guid? MaNvNhan { get; set; }

        public int SoLanThu { get; set; }
    }
}

