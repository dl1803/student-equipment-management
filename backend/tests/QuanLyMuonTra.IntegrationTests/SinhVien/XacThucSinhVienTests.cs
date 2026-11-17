using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using QuanLyMuonTra.Infrastructure;
using QuanLyMuonTra.Infrastructure.Persistence;
using QuanLyMuonTra.IntegrationTests.Database;
using QuanLyMuonTra.Modules.SinhVien.Infrastructure;
using QuanLyMuonTra.Modules.TaiKhoan.Domain;

namespace QuanLyMuonTra.IntegrationTests.SinhVien;

public sealed class XacThucSinhVienTests
{
    private const string MatKhauDung = "MatKhauSinhVien-1";

    [Fact]
    public async Task UC19_MatKhauDung_TraMssvLamMoiHoSoVaGiuNguyenQuyenMuon()
    {
        await using var dichVu = TaoDichVu();
        await using var phamVi = dichVu.CreateAsyncScope();
        var db = phamVi.ServiceProvider.GetRequiredService<AppDbContext>();
        await using var giaoDich = await db.Database.BeginTransactionAsync();
        var mssv = $"SV-D05-{Guid.NewGuid():N}";
        await ThemSinhVienTruongAsync(db, mssv, "Ho Ten Moi", "CNPM");
        await db.Database.ExecuteSqlInterpolatedAsync($"""
            INSERT INTO sinh_vien.sinh_vien (ma_sv, ho_ten, email, ma_nhom, han_the, trang_thai_hoc, hoc_ky, trang_thai_quyen_muon)
            VALUES ({mssv}, 'Ho Ten Cu', 'cu@kiemthu.local', 'DAI_HOC', DATE '2020-01-01', 'DANG_HOC', 'HK-CU', 'TAM_KHOA')
            """);
        var boXacThuc = TaoBoXacThuc(db);

        var ketQua = await boXacThuc.XacThucAsync(mssv, MatKhauDung, CancellationToken.None);

        Assert.Equal(mssv, ketQua);
        var dong = await DocSinhVienAsync(db, mssv);
        Assert.Equal("Ho Ten Moi", dong.HoTen);
        Assert.Equal("CNPM", dong.Khoa);
        Assert.Equal("2027-12-31", dong.HanThe);
        Assert.Equal("HK1-2026", dong.HocKy);
        Assert.Equal("TAM_KHOA", dong.TrangThaiQuyenMuon);
    }

    [Fact]
    public async Task UC19_SinhVienMoiVaTruongRong_TaoHoSoVoiQuyenMuonBinhThuong()
    {
        await using var dichVu = TaoDichVu();
        await using var phamVi = dichVu.CreateAsyncScope();
        var db = phamVi.ServiceProvider.GetRequiredService<AppDbContext>();
        await using var giaoDich = await db.Database.BeginTransactionAsync();
        var mssv = $"SV-D05-{Guid.NewGuid():N}";
        await ThemSinhVienTruongAsync(db, mssv, "Sinh Vien Moi", null);
        var boXacThuc = TaoBoXacThuc(db);

        var ketQua = await boXacThuc.XacThucAsync($"  {mssv}  ", MatKhauDung, CancellationToken.None);

        Assert.Equal(mssv, ketQua);
        var dong = await DocSinhVienAsync(db, mssv);
        Assert.Equal("Sinh Vien Moi", dong.HoTen);
        Assert.Null(dong.Khoa);
        Assert.Equal("BINH_THUONG", dong.TrangThaiQuyenMuon);
    }

    [Fact]
    public async Task UC19_SaiMatKhau_TraNullVaKhongTaoHoSo()
    {
        await using var dichVu = TaoDichVu();
        await using var phamVi = dichVu.CreateAsyncScope();
        var db = phamVi.ServiceProvider.GetRequiredService<AppDbContext>();
        await using var giaoDich = await db.Database.BeginTransactionAsync();
        var mssv = $"SV-D05-{Guid.NewGuid():N}";
        await ThemSinhVienTruongAsync(db, mssv, "Sinh Vien Sai Mat Khau", "CNPM");
        var boXacThuc = TaoBoXacThuc(db);

        var ketQua = await boXacThuc.XacThucAsync(mssv, "mat-khau-sai", CancellationToken.None);

        Assert.Null(ketQua);
        Assert.Equal(0, await DemHoSoAsync(db, mssv));
    }

    [Fact]
    public async Task UC19_MssvKhongTonTai_TraNull()
    {
        await using var dichVu = TaoDichVu();
        await using var phamVi = dichVu.CreateAsyncScope();
        var db = phamVi.ServiceProvider.GetRequiredService<AppDbContext>();
        await using var giaoDich = await db.Database.BeginTransactionAsync();
        var boXacThuc = TaoBoXacThuc(db);

        var ketQua = await boXacThuc.XacThucAsync($"khong-co-{Guid.NewGuid():N}", MatKhauDung, CancellationToken.None);

        Assert.Null(ketQua);
    }

    [Fact]
    public async Task UC19_SinhVienCoNhomMoiChuaCoTrongBangNhom_TuDongThemNhomVaTaoHoSoThanhCong()
    {
        await using var dichVu = TaoDichVu();
        await using var phamVi = dichVu.CreateAsyncScope();
        var db = phamVi.ServiceProvider.GetRequiredService<AppDbContext>();
        await using var giaoDich = await db.Database.BeginTransactionAsync();
        var mssv = $"SV-D05-{Guid.NewGuid():N}";
        var maNhomMoi = $"NHOM_{Guid.NewGuid():N}";
        var bam = new PasswordHasher<object>().HashPassword(null!, MatKhauDung);
        await db.Database.ExecuteSqlInterpolatedAsync($"""
            INSERT INTO truong_gia_lap.nhom_sinh_vien_truong (ma_nhom, ten_nhom) VALUES ({maNhomMoi}, 'Nhom Truong Moi');
            INSERT INTO truong_gia_lap.sinh_vien_truong (ma_sv, mat_khau_bam, ho_ten, lop, khoa, email, ma_nhom, han_the, trang_thai_hoc, hoc_ky)
            VALUES ({mssv}, {bam}, 'Sinh Vien Nhom Moi', 'KTPM2024', null, 'nhommoi@truong.local', {maNhomMoi}, DATE '2027-12-31', 'DANG_HOC', 'HK1-2026');
            """);

        var boXacThuc = TaoBoXacThuc(db);
        var ketQua = await boXacThuc.XacThucAsync(mssv, MatKhauDung, CancellationToken.None);

        Assert.Equal(mssv, ketQua);
        var demNhom = await db.Database.SqlQuery<int>($"""
            SELECT count(*)::int AS "Value" FROM sinh_vien.nhom_sinh_vien WHERE ma_nhom = {maNhomMoi}
            """).SingleAsync();
        Assert.Equal(1, demNhom);
        var dong = await DocSinhVienAsync(db, mssv);
        Assert.Equal("Sinh Vien Nhom Moi", dong.HoTen);
    }

    private static XacThucSinhVien TaoBoXacThuc(AppDbContext db) => new(new MockSchoolDirectory(db), db);

    private static async Task ThemSinhVienTruongAsync(AppDbContext db, string mssv, string hoTen, string? khoa)
    {
        var bam = new PasswordHasher<object>().HashPassword(null!, MatKhauDung);
        await db.Database.ExecuteSqlInterpolatedAsync($"""
            INSERT INTO truong_gia_lap.sinh_vien_truong (ma_sv, mat_khau_bam, ho_ten, lop, khoa, email, ma_nhom, han_the, trang_thai_hoc, hoc_ky)
            VALUES ({mssv}, {bam}, {hoTen}, 'KTPM2024', {khoa}::text, 'kiemthu@truong.local', 'DAI_HOC', DATE '2027-12-31', 'DANG_HOC', 'HK1-2026')
            """);
    }

    private static Task<DongSinhVien> DocSinhVienAsync(AppDbContext db, string mssv) =>
        db.Database.SqlQuery<DongSinhVien>($"""
            SELECT ho_ten AS "HoTen", khoa AS "Khoa", han_the::text AS "HanThe", hoc_ky AS "HocKy",
                   trang_thai_quyen_muon AS "TrangThaiQuyenMuon"
            FROM sinh_vien.sinh_vien
            WHERE ma_sv = {mssv}
            """).SingleAsync();

    private static Task<int> DemHoSoAsync(AppDbContext db, string mssv) =>
        db.Database.SqlQuery<int>($"""
            SELECT count(*)::int AS "Value" FROM sinh_vien.sinh_vien WHERE ma_sv = {mssv}
            """).SingleAsync();

    private static ServiceProvider TaoDichVu() =>
        new ServiceCollection()
            .AddInfrastructure(KetNoiKiemThu.LayChuoiKetNoi(), [typeof(NguoiDungHeThong).Assembly])
            .BuildServiceProvider();

    public sealed class DongSinhVien
    {
        public string HoTen { get; set; } = string.Empty;

        public string? Khoa { get; set; }

        public string HanThe { get; set; } = string.Empty;

        public string HocKy { get; set; } = string.Empty;

        public string TrangThaiQuyenMuon { get; set; } = string.Empty;
    }
}

