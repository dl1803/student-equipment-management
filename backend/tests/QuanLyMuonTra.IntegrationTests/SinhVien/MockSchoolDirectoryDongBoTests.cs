using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using QuanLyMuonTra.Infrastructure.Persistence;
using QuanLyMuonTra.Modules.SinhVien.Infrastructure;

namespace QuanLyMuonTra.IntegrationTests.SinhVien;

public sealed class MockSchoolDirectoryDongBoTests
{
    [Fact]
    public async Task UC19_LayThayDoiTu_MocNull_TraSvKiemThuTheoThuTuCapNhatLuc()
    {
        await using var dichVu = DongBoSinhVienTestSupport.TaoDichVu();
        await using var phamVi = dichVu.CreateAsyncScope();
        var db = phamVi.ServiceProvider.GetRequiredService<AppDbContext>();
        await using var giaoDich = await db.Database.BeginTransactionAsync();
        var maNhom = DongBoSinhVienTestSupport.TaoMa("NHOM-D11");
        var maSvTruoc = DongBoSinhVienTestSupport.TaoMa("SV-D11-A");
        var maSvSau = DongBoSinhVienTestSupport.TaoMa("SV-D11-B");
        await DongBoSinhVienTestSupport.ThemNhomTruongAsync(db, maNhom, "Nhóm D11 kiểm thử");
        var moc = DongBoSinhVienTestSupport.MocGoc;
        await DongBoSinhVienTestSupport.ThemSvTruongAsync(db, maSvSau, "Sinh Viên Sau", moc.AddDays(2), maNhom);
        await DongBoSinhVienTestSupport.ThemSvTruongAsync(db, maSvTruoc, "Sinh Viên Trước", moc.AddDays(1), maNhom);

        var ds = await new MockSchoolDirectory(db).LayThayDoiTuAsync(null, CancellationToken.None);

        var cuaTest = ds.Where(sv => sv.MaSv == maSvTruoc || sv.MaSv == maSvSau).ToList();
        Assert.Equal(new[] { maSvTruoc, maSvSau }, cuaTest.Select(sv => sv.MaSv).ToArray());
        Assert.Equal("Nhóm D11 kiểm thử", cuaTest[0].TenNhom);
        Assert.Equal(moc.AddDays(1), cuaTest[0].CapNhatLuc);
        Assert.Equal(new DateOnly(2027, 12, 31), cuaTest[0].HanThe);
    }

    [Fact]
    public async Task UC19_LayThayDoiTu_ChiTraBanGhiLonHonMoc_KhongTraBanGhiBangMoc()
    {
        await using var dichVu = DongBoSinhVienTestSupport.TaoDichVu();
        await using var phamVi = dichVu.CreateAsyncScope();
        var db = phamVi.ServiceProvider.GetRequiredService<AppDbContext>();
        await using var giaoDich = await db.Database.BeginTransactionAsync();
        var moc = DongBoSinhVienTestSupport.MocGoc;
        var ma1 = DongBoSinhVienTestSupport.TaoMa("SV-D11-1");
        var ma2 = DongBoSinhVienTestSupport.TaoMa("SV-D11-2");
        var ma3 = DongBoSinhVienTestSupport.TaoMa("SV-D11-3");
        await DongBoSinhVienTestSupport.ThemSvTruongAsync(db, ma1, "SV 1", moc.AddDays(1));
        await DongBoSinhVienTestSupport.ThemSvTruongAsync(db, ma2, "SV 2", moc.AddDays(2));
        await DongBoSinhVienTestSupport.ThemSvTruongAsync(db, ma3, "SV 3", moc.AddDays(3));

        var ds = await new MockSchoolDirectory(db).LayThayDoiTuAsync(moc.AddDays(2), CancellationToken.None);

        var cuaTest = ds.Where(sv => sv.MaSv == ma1 || sv.MaSv == ma2 || sv.MaSv == ma3).ToList();
        Assert.Equal(ma3, Assert.Single(cuaTest).MaSv);
    }

    [Fact]
    public async Task UC19_LayTheoMssv_TraThongTinVaNullKhiKhongCo()
    {
        await using var dichVu = DongBoSinhVienTestSupport.TaoDichVu();
        await using var phamVi = dichVu.CreateAsyncScope();
        var db = phamVi.ServiceProvider.GetRequiredService<AppDbContext>();
        await using var giaoDich = await db.Database.BeginTransactionAsync();
        var ma = DongBoSinhVienTestSupport.TaoMa("SV-D11-T");
        await DongBoSinhVienTestSupport.ThemSvTruongAsync(db, ma, "SV Tra Theo Mssv", DongBoSinhVienTestSupport.MocGoc);
        var truong = new MockSchoolDirectory(db);

        var thay = await truong.LayTheoMssvAsync(ma, CancellationToken.None);
        var khongCo = await truong.LayTheoMssvAsync(DongBoSinhVienTestSupport.TaoMa("KHONG-CO"), CancellationToken.None);

        Assert.NotNull(thay);
        Assert.Equal("SV Tra Theo Mssv", thay.HoTen);
        Assert.Equal("DAI_HOC", thay.MaNhom);
        Assert.False(string.IsNullOrWhiteSpace(thay.TenNhom));
        Assert.Null(khongCo);
    }
}

