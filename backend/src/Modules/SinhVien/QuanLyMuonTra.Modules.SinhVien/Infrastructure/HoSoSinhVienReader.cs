using Microsoft.EntityFrameworkCore;
using QuanLyMuonTra.Modules.SinhVien.Contracts;

namespace QuanLyMuonTra.Modules.SinhVien.Infrastructure;

public sealed class HoSoSinhVienReader(DbContext db) : IHoSoSinhVienReader
{
    public async Task<HoSoSinhVienDto?> LayTheoMaSvAsync(string maSv, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(maSv))
        {
            return null;
        }

        var maSvChuan = maSv.Trim().ToUpperInvariant();
        var dong = await db.Database.SqlQuery<DongHoSoSinhVien>($"""
                SELECT sv.ma_sv AS "MaSv", sv.ho_ten AS "HoTen", sv.lop AS "Lop", sv.khoa AS "Khoa",
                       sv.ma_nhom AS "MaNhom", nhom.ten_nhom AS "TenNhom", sv.han_the AS "HanThe",
                       sv.trang_thai_hoc AS "TrangThaiHoc", sv.trang_thai_quyen_muon AS "TrangThaiQuyenMuon"
                FROM sinh_vien.sinh_vien sv
                JOIN sinh_vien.nhom_sinh_vien nhom ON nhom.ma_nhom = sv.ma_nhom
                WHERE UPPER(sv.ma_sv) = {maSvChuan}
                """)
            .FirstOrDefaultAsync(ct);

        return dong is null
            ? null
            : new HoSoSinhVienDto(
                dong.MaSv,
                dong.HoTen,
                dong.Lop,
                dong.Khoa,
                dong.MaNhom,
                dong.TenNhom,
                dong.HanThe,
                dong.TrangThaiHoc,
                dong.TrangThaiQuyenMuon);
    }

    public sealed class DongHoSoSinhVien
    {
        public string MaSv { get; set; } = string.Empty;

        public string HoTen { get; set; } = string.Empty;

        public string? Lop { get; set; }

        public string? Khoa { get; set; }

        public string MaNhom { get; set; } = string.Empty;

        public string TenNhom { get; set; } = string.Empty;

        public DateOnly HanThe { get; set; }

        public string TrangThaiHoc { get; set; } = string.Empty;

        public string TrangThaiQuyenMuon { get; set; } = string.Empty;
    }
}

