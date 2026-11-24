using Microsoft.EntityFrameworkCore;
using QuanLyMuonTra.Modules.SinhVien.Application;
using QuanLyMuonTra.Modules.SinhVien.Contracts;

namespace QuanLyMuonTra.Modules.SinhVien.Infrastructure;

public sealed class XacThucSinhVien(ISchoolDirectory truong, DbContext db) : IXacThucSinhVien
{
    public async Task<string?> XacThucAsync(string mssv, string matKhau, CancellationToken ct)
    {
        var ketQua = await truong.XacThucAsync(mssv.Trim(), matKhau, ct);
        if (!ketQua.ThanhCong || ketQua.ThongTin is null)
        {
            return null;
        }

        var sv = ketQua.ThongTin;

        var moGiaoDich = db.Database.CurrentTransaction is null;
        var giaoDich = moGiaoDich ? await db.Database.BeginTransactionAsync(ct) : null;
        try
        {
            await db.Database.ExecuteSqlInterpolatedAsync($"""
                INSERT INTO sinh_vien.nhom_sinh_vien (ma_nhom, ten_nhom)
                VALUES ({sv.MaNhom}, COALESCE((SELECT ten_nhom FROM truong_gia_lap.nhom_sinh_vien_truong WHERE ma_nhom = {sv.MaNhom}), {sv.MaNhom}))
                ON CONFLICT (ma_nhom) DO NOTHING
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

            if (giaoDich is not null)
            {
                await giaoDich.CommitAsync(ct);
            }
        }
        finally
        {
            if (giaoDich is not null)
            {
                await giaoDich.DisposeAsync();
            }
        }

        return sv.MaSv;
    }
}
