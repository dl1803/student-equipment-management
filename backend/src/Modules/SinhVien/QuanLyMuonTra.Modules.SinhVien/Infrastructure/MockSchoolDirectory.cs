using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using QuanLyMuonTra.Modules.SinhVien.Application;

namespace QuanLyMuonTra.Modules.SinhVien.Infrastructure;

public sealed class MockSchoolDirectory(DbContext db) : ISchoolDirectory
{
    private static readonly PasswordHasher<object> BoBam = new();

    public async Task<KetQuaXacThucTruong> XacThucAsync(string mssv, string matKhau, CancellationToken ct)
    {
        var dong = await db.Database.SqlQuery<DongSinhVienTruong>($"""
                SELECT ma_sv AS "MaSv", mat_khau_bam AS "MatKhauBam", ho_ten AS "HoTen", lop AS "Lop",
                       khoa AS "Khoa", email AS "Email", ma_nhom AS "MaNhom", han_the AS "HanThe",
                       trang_thai_hoc AS "TrangThaiHoc", hoc_ky AS "HocKy"
                FROM truong_gia_lap.sinh_vien_truong
                WHERE ma_sv = {mssv}
                """)
            .FirstOrDefaultAsync(ct);

        if (dong is null)
        {
            return new KetQuaXacThucTruong(false, null);
        }

        var ketQua = BoBam.VerifyHashedPassword(null!, dong.MatKhauBam, matKhau);
        if (ketQua == PasswordVerificationResult.Failed)
        {
            return new KetQuaXacThucTruong(false, null);
        }

        var thongTin = new SinhVienTruongDto(
            dong.MaSv,
            dong.HoTen,
            dong.Lop,
            dong.Khoa,
            dong.Email,
            dong.MaNhom,
            dong.HanThe,
            dong.TrangThaiHoc,
            dong.HocKy);

        return new KetQuaXacThucTruong(true, thongTin);
    }

    public async Task<SinhVienTruongDto?> LayTheoMssvAsync(string mssv, CancellationToken ct)
    {
        var dong = await db.Database.SqlQuery<DongSinhVienThayDoi>($"""
                SELECT sv.ma_sv AS "MaSv", sv.ho_ten AS "HoTen", sv.lop AS "Lop", sv.khoa AS "Khoa",
                       sv.email AS "Email", sv.ma_nhom AS "MaNhom", nhom.ten_nhom AS "TenNhom",
                       sv.han_the AS "HanThe", sv.trang_thai_hoc AS "TrangThaiHoc", sv.hoc_ky AS "HocKy",
                       sv.cap_nhat_luc AS "CapNhatLuc"
                FROM truong_gia_lap.sinh_vien_truong sv
                JOIN truong_gia_lap.nhom_sinh_vien_truong nhom ON nhom.ma_nhom = sv.ma_nhom
                WHERE sv.ma_sv = {mssv}
                """)
            .FirstOrDefaultAsync(ct);

        return dong is null ? null : TaoDto(dong);
    }

    public async Task<IReadOnlyList<SinhVienTruongDto>> LayThayDoiTuAsync(DateTimeOffset? moc, CancellationToken ct)
    {
        var dsDong = await db.Database.SqlQuery<DongSinhVienThayDoi>($"""
                SELECT sv.ma_sv AS "MaSv", sv.ho_ten AS "HoTen", sv.lop AS "Lop", sv.khoa AS "Khoa",
                       sv.email AS "Email", sv.ma_nhom AS "MaNhom", nhom.ten_nhom AS "TenNhom",
                       sv.han_the AS "HanThe", sv.trang_thai_hoc AS "TrangThaiHoc", sv.hoc_ky AS "HocKy",
                       sv.cap_nhat_luc AS "CapNhatLuc"
                FROM truong_gia_lap.sinh_vien_truong sv
                JOIN truong_gia_lap.nhom_sinh_vien_truong nhom ON nhom.ma_nhom = sv.ma_nhom
                WHERE {moc}::timestamptz IS NULL OR sv.cap_nhat_luc > {moc}::timestamptz
                """)
            .OrderBy(dong => dong.CapNhatLuc)
            .ThenBy(dong => dong.MaSv)
            .ToListAsync(ct);

        return dsDong.Select(TaoDto).ToList();
    }

    private static SinhVienTruongDto TaoDto(DongSinhVienThayDoi dong) =>
        new(
            dong.MaSv,
            dong.HoTen,
            dong.Lop,
            dong.Khoa,
            dong.Email,
            dong.MaNhom,
            dong.HanThe,
            dong.TrangThaiHoc,
            dong.HocKy,
            dong.TenNhom,
            dong.CapNhatLuc);

    public sealed class DongSinhVienTruong
    {
        public string MaSv { get; set; } = string.Empty;

        public string MatKhauBam { get; set; } = string.Empty;

        public string HoTen { get; set; } = string.Empty;

        public string? Lop { get; set; }

        public string? Khoa { get; set; }

        public string Email { get; set; } = string.Empty;

        public string MaNhom { get; set; } = string.Empty;

        public DateOnly HanThe { get; set; }

        public string TrangThaiHoc { get; set; } = string.Empty;

        public string HocKy { get; set; } = string.Empty;
    }

    public sealed class DongSinhVienThayDoi
    {
        public string MaSv { get; set; } = string.Empty;

        public string HoTen { get; set; } = string.Empty;

        public string? Lop { get; set; }

        public string? Khoa { get; set; }

        public string Email { get; set; } = string.Empty;

        public string MaNhom { get; set; } = string.Empty;

        public string TenNhom { get; set; } = string.Empty;

        public DateOnly HanThe { get; set; }

        public string TrangThaiHoc { get; set; } = string.Empty;

        public string HocKy { get; set; } = string.Empty;

        public DateTimeOffset CapNhatLuc { get; set; }
    }
}
