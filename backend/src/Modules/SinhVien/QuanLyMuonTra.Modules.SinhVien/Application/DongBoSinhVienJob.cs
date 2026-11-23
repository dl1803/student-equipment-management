using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using QuanLyMuonTra.SharedKernel;

namespace QuanLyMuonTra.Modules.SinhVien.Application;

public sealed class DongBoSinhVienJob(DbContext db, ISchoolDirectory truong, ILogger<DongBoSinhVienJob> nhatKy) : IJob
{
    private const string TenDiemLuu = "dong_bo_mot_sinh_vien";

    public string MaJob => "J8";

    public string TenJob => "DongBoSinhVien";

    public TimeSpan ChuKy => TimeSpan.FromMinutes(30);

    public async Task<int> ChayAsync(CancellationToken ct)
    {
        var moc = await DocMocAsync(ct);
        var dsThayDoi = await truong.LayThayDoiTuAsync(moc, ct);
        if (dsThayDoi.Count == 0)
        {
            return 0;
        }

        var giaoDich = db.Database.CurrentTransaction;
        var soThanhCong = 0;
        var soLoi = 0;
        foreach (var sv in dsThayDoi)
        {
            ct.ThrowIfCancellationRequested();

            if (giaoDich is not null)
            {
                await giaoDich.CreateSavepointAsync(TenDiemLuu, ct);
            }

            try
            {
                await GhiMotSinhVienAsync(sv, ct);

                if (giaoDich is not null)
                {
                    await giaoDich.ReleaseSavepointAsync(TenDiemLuu, ct);
                }

                soThanhCong++;
            }
            catch (OperationCanceledException) when (ct.IsCancellationRequested)
            {
                throw;
            }
            catch (Exception loi)
            {
                soLoi++;

                if (giaoDich is not null)
                {
                    await giaoDich.RollbackToSavepointAsync(TenDiemLuu, CancellationToken.None);
                    await giaoDich.ReleaseSavepointAsync(TenDiemLuu, CancellationToken.None);
                }

                nhatKy.LogWarning(loi, "Đồng bộ sinh viên {MaSv} lỗi; bỏ qua sinh viên này và tiếp tục.", sv.MaSv);
            }
        }

        if (soLoi > 0)
        {
            nhatKy.LogWarning(
                "Đồng bộ sinh viên: {SoLoi} sinh viên lỗi, giữ nguyên mốc đồng bộ để kéo lại ở lần chạy sau.",
                soLoi);

            return soThanhCong;
        }

        var mocMoi = dsThayDoi.Max(sv => sv.CapNhatLuc);
        if (mocMoi is not null)
        {
            await GhiMocAsync(mocMoi.Value, ct);
        }

        return soThanhCong;
    }

    private async Task<DateTimeOffset?> DocMocAsync(CancellationToken ct)
    {
        var dong = await db.Database.SqlQuery<DongMocDongBo>($"""
                SELECT moc_dong_bo_gan_nhat AS "Moc" FROM sinh_vien.dong_bo_trang_thai WHERE id = 1
                """)
            .FirstOrDefaultAsync(ct);

        return dong?.Moc;
    }

    private async Task GhiMotSinhVienAsync(SinhVienTruongDto sv, CancellationToken ct)
    {
        var tenNhom = string.IsNullOrWhiteSpace(sv.TenNhom) ? sv.MaNhom : sv.TenNhom;

        await db.Database.ExecuteSqlInterpolatedAsync($"""
            INSERT INTO sinh_vien.nhom_sinh_vien AS nhom (ma_nhom, ten_nhom)
            VALUES ({sv.MaNhom}, {tenNhom})
            ON CONFLICT (ma_nhom) DO UPDATE SET ten_nhom = EXCLUDED.ten_nhom
            WHERE nhom.ten_nhom IS DISTINCT FROM EXCLUDED.ten_nhom
            """, ct);

        await db.Database.ExecuteSqlInterpolatedAsync($"""
            INSERT INTO sinh_vien.sinh_vien (ma_sv, ho_ten, lop, khoa, email, ma_nhom, han_the, trang_thai_hoc, hoc_ky)
            VALUES ({sv.MaSv}, {sv.HoTen}, {sv.Lop}::text, {sv.Khoa}::text, {sv.Email}, {sv.MaNhom}, {sv.HanThe}, {sv.TrangThaiHoc}, {sv.HocKy})
            ON CONFLICT (ma_sv) DO UPDATE SET
                ho_ten = EXCLUDED.ho_ten,
                lop = EXCLUDED.lop,
                khoa = EXCLUDED.khoa,
                email = EXCLUDED.email,
                ma_nhom = EXCLUDED.ma_nhom,
                han_the = EXCLUDED.han_the,
                trang_thai_hoc = EXCLUDED.trang_thai_hoc,
                hoc_ky = EXCLUDED.hoc_ky
            """, ct);
    }

    private async Task GhiMocAsync(DateTimeOffset moc, CancellationToken ct)
    {
        await db.Database.ExecuteSqlInterpolatedAsync($"""
            INSERT INTO sinh_vien.dong_bo_trang_thai AS dbtt (id, moc_dong_bo_gan_nhat)
            VALUES (1, {moc})
            ON CONFLICT (id) DO UPDATE SET moc_dong_bo_gan_nhat = EXCLUDED.moc_dong_bo_gan_nhat
            WHERE dbtt.moc_dong_bo_gan_nhat IS NULL OR dbtt.moc_dong_bo_gan_nhat < EXCLUDED.moc_dong_bo_gan_nhat
            """, ct);
    }

    public sealed class DongMocDongBo
    {
        public DateTimeOffset? Moc { get; set; }
    }
}

