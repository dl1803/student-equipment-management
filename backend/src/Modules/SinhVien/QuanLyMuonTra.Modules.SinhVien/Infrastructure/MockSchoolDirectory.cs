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
}

