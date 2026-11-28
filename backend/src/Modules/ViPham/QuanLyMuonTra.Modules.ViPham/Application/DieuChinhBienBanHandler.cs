using Microsoft.EntityFrameworkCore;
using QuanLyMuonTra.Modules.ViPham.Domain;
using QuanLyMuonTra.SharedKernel;

namespace QuanLyMuonTra.Modules.ViPham.Application;

public sealed record KhoanDieuChinhYeuCau(string MaThietBi, string LoaiViPham, long SoTien);

public sealed record DieuChinhBienBanLenh(IReadOnlyList<KhoanDieuChinhYeuCau> DsKhoan);

internal sealed class DongKhoanHienCo
{
    public string MaThietBi { get; set; } = string.Empty;

    public string LoaiViPham { get; set; } = string.Empty;

    public long SoTien { get; set; }
}

public sealed class DieuChinhBienBanHandler(DbContext db, IAuditLogger nhatKy)
{
    private const long SoTienToiDa = 99_999_999_999_999;

    private static readonly string[] LoaiHopLe = [MaLoaiViPham.TraTre, MaLoaiViPham.HuHong, MaLoaiViPham.Mat];

    public async Task<BienBanViPhamDto> XuLyAsync(string maBienBan, DieuChinhBienBanLenh lenh, CancellationToken ct)
    {
        KiemTraDocViPham.KiemTraMaBienBan(maBienBan);
        KiemTraDauVao(lenh.DsKhoan);

        var maSv = (await BienBanTruyVan.LayDongAsync(db, maBienBan, ct)).MaSv;

        return await GiaoDichViPham.ChayAsync(db, async () =>
        {
            await GiaoDichViPham.KhoaDongSinhVienAsync(db, maSv, ct);

            var bienBan = await BienBanTruyVan.LayDongAsync(db, maBienBan, ct);
            if (bienBan.TrangThai != "CHUA_NOP")
            {
                throw new LoiNghiepVuException(
                    MaLoi.TrangThaiKhongHopLe,
                    $"Biên bản {maBienBan} đang ở trạng thái {bienBan.TrangThai}, chỉ điều chỉnh được biên bản CHUA_NOP.");
            }

            var dsKhoanCu = await db.Database.SqlQuery<DongKhoanHienCo>($"""
                    SELECT ma_thiet_bi AS "MaThietBi", loai_vi_pham AS "LoaiViPham", so_tien::bigint AS "SoTien"
                    FROM vi_pham.chi_tiet_bien_ban
                    WHERE ma_bien_ban = {maBienBan}
                    """)
                .ToListAsync(ct);

            foreach (var yeuCau in lenh.DsKhoan)
            {
                if (!dsKhoanCu.Any(k => k.MaThietBi == yeuCau.MaThietBi && k.LoaiViPham == yeuCau.LoaiViPham))
                {
                    throw new LoiNghiepVuException(
                        MaLoi.DuLieuKhongHopLe,
                        $"Biên bản không có khoản {yeuCau.LoaiViPham} của thiết bị {yeuCau.MaThietBi}.");
                }
            }

            var tongMoi = dsKhoanCu.Sum(k => SoTienSau(k, lenh.DsKhoan));
            if (tongMoi > SoTienToiDa)
            {
                throw new LoiNghiepVuException(MaLoi.DuLieuKhongHopLe, "Tổng tiền sau điều chỉnh vượt giới hạn cho phép.");
            }

            foreach (var yeuCau in lenh.DsKhoan)
            {
                await db.Database.ExecuteSqlInterpolatedAsync($"""
                    UPDATE vi_pham.chi_tiet_bien_ban
                    SET so_tien = {yeuCau.SoTien}
                    WHERE ma_bien_ban = {maBienBan}
                      AND ma_thiet_bi = {yeuCau.MaThietBi}
                      AND loai_vi_pham = {yeuCau.LoaiViPham}
                    """, ct);
            }

            await db.Database.ExecuteSqlInterpolatedAsync($"""
                UPDATE vi_pham.bien_ban_vi_pham
                SET tong_tien = {tongMoi}
                WHERE ma_bien_ban = {maBienBan} AND trang_thai = 'CHUA_NOP'
                """, ct);

            await nhatKy.GhiAsync(
                "DIEU_CHINH_BIEN_BAN_VI_PHAM",
                "vi_pham.bien_ban_vi_pham",
                maBienBan,
                new Dictionary<string, object?>
                {
                    ["TongTien"] = bienBan.TongTien,
                    ["DsKhoan"] = dsKhoanCu
                        .OrderBy(k => k.MaThietBi, StringComparer.Ordinal)
                        .ThenBy(k => k.LoaiViPham, StringComparer.Ordinal)
                        .Select(k => $"{k.MaThietBi}|{k.LoaiViPham}|{k.SoTien}")
                        .ToArray()
                },
                new Dictionary<string, object?>
                {
                    ["TongTien"] = tongMoi,
                    ["DsKhoan"] = dsKhoanCu
                        .OrderBy(k => k.MaThietBi, StringComparer.Ordinal)
                        .ThenBy(k => k.LoaiViPham, StringComparer.Ordinal)
                        .Select(k => $"{k.MaThietBi}|{k.LoaiViPham}|{SoTienSau(k, lenh.DsKhoan)}")
                        .ToArray()
                },
                ct);

            return await BienBanTruyVan.LayMotAsync(db, maBienBan, ct);
        }, ct);
    }

    private static long SoTienSau(DongKhoanHienCo khoanCu, IReadOnlyList<KhoanDieuChinhYeuCau> dsKhoan)
    {
        var yeuCau = dsKhoan.FirstOrDefault(
            y => y.MaThietBi == khoanCu.MaThietBi && y.LoaiViPham == khoanCu.LoaiViPham);

        return yeuCau?.SoTien ?? khoanCu.SoTien;
    }

    private static void KiemTraDauVao(IReadOnlyList<KhoanDieuChinhYeuCau> dsKhoan)
    {
        if (dsKhoan.Count == 0
            || dsKhoan.Select(k => (k.MaThietBi, k.LoaiViPham)).Distinct().Count() != dsKhoan.Count)
        {
            throw new LoiNghiepVuException(
                MaLoi.DuLieuKhongHopLe,
                "Danh sách khoản trống hoặc có khoản trùng thiết bị và loại vi phạm.");
        }

        if (dsKhoan.Any(k => !LoaiHopLe.Contains(k.LoaiViPham)))
        {
            throw new LoiNghiepVuException(MaLoi.DuLieuKhongHopLe, "Loại vi phạm phải là TRA_TRE, HU_HONG hoặc MAT.");
        }

        if (dsKhoan.Any(k => k.SoTien < 0 || k.SoTien > SoTienToiDa))
        {
            throw new LoiNghiepVuException(
                MaLoi.DuLieuKhongHopLe,
                "Số tiền phải là số nguyên từ 0 đến 99.999.999.999.999.");
        }
    }
}

