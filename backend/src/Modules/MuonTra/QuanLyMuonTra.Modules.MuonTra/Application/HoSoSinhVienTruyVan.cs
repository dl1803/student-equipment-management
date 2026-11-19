using Microsoft.EntityFrameworkCore;

namespace QuanLyMuonTra.Modules.MuonTra.Application;

internal static class HoSoSinhVienTruyVan
{
    public static async Task<DongHoSoSinhVien> LayAsync(DbContext db, string maSv, CancellationToken ct)
    {
        return await db.Database.SqlQuery<DongHoSoSinhVien>($"""
                SELECT ma_nhom AS "MaNhom", han_the AS "HanThe", trang_thai_hoc AS "TrangThaiHoc",
                       trang_thai_quyen_muon AS "TrangThaiQuyenMuon"
                FROM sinh_vien.sinh_vien
                WHERE ma_sv = {maSv}
                """)
            .SingleOrDefaultAsync(ct)
            ?? throw new InvalidOperationException($"Không tìm thấy hồ sơ sinh viên {maSv} của phiếu yêu cầu.");
    }

    public static async Task KhoaAsync(DbContext db, string maSv, CancellationToken ct)
    {
        await db.Database.ExecuteSqlInterpolatedAsync($"""
            SELECT ma_sv FROM sinh_vien.sinh_vien WHERE ma_sv = {maSv} FOR UPDATE
            """, ct);
    }
}
