using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using QuanLyMuonTra.Infrastructure.Persistence;
using QuanLyMuonTra.Modules.SinhVien;
using QuanLyMuonTra.Modules.SinhVien.Application;
using QuanLyMuonTra.Modules.SinhVien.Infrastructure;
using QuanLyMuonTra.SharedKernel;

namespace QuanLyMuonTra.IntegrationTests.SinhVien;

public sealed class DongBoSinhVienJobTests
{
    [Fact]
    public void UC19_J8_DangKyDungMaTenVaChuKy()
    {
        var dichVu = new ServiceCollection().AddSinhVienModule();

        Assert.Contains(
            dichVu,
            d => d.ServiceType == typeof(IJob) && d.ImplementationType == typeof(DongBoSinhVienJob));

        var job = new DongBoSinhVienJob(null!, null!, NullLogger<DongBoSinhVienJob>.Instance);
        Assert.Equal("J8", job.MaJob);
        Assert.Equal("DongBoSinhVien", job.TenJob);
        Assert.Equal(TimeSpan.FromMinutes(30), job.ChuKy);
    }

    [Fact]
    public async Task UC19_J8_UpsertSauMoc_GiuQuyenMuonVaTienMoc()
    {
        await using var dichVu = DongBoSinhVienTestSupport.TaoDichVu();
        await using var phamVi = dichVu.CreateAsyncScope();
        var db = phamVi.ServiceProvider.GetRequiredService<AppDbContext>();
        await using var giaoDich = await db.Database.BeginTransactionAsync();
        var moc = DongBoSinhVienTestSupport.MocGoc;
        await DongBoSinhVienTestSupport.DatMocAsync(db, moc);
        var maNhomMoi = DongBoSinhVienTestSupport.TaoMa("NHOM-D11");
        var maSvCu = DongBoSinhVienTestSupport.TaoMa("SV-D11-CU");
        var maSvMoi = DongBoSinhVienTestSupport.TaoMa("SV-D11-MOI");
        await DongBoSinhVienTestSupport.ThemNhomTruongAsync(db, maNhomMoi, "Nhóm D11 kiểm thử");
        await DongBoSinhVienTestSupport.ThemSvUngDungAsync(db, maSvCu, "Ho Ten Cu", "TAM_KHOA");
        await DongBoSinhVienTestSupport.ThemSvTruongAsync(db, maSvCu, "Ho Ten Moi", moc.AddDays(1));
        await DongBoSinhVienTestSupport.ThemSvTruongAsync(db, maSvMoi, "Sinh Vien Moi", moc.AddDays(2), maNhomMoi);

        var soDaXuLy = await TaoJob(db).ChayAsync(CancellationToken.None);

        Assert.Equal(2, soDaXuLy);
        var cu = await DongBoSinhVienTestSupport.DocSvUngDungAsync(db, maSvCu);
        Assert.NotNull(cu);
        Assert.Equal("Ho Ten Moi", cu.HoTen);
        Assert.Equal("2027-12-31", cu.HanThe);
        Assert.Equal("HK1-2026", cu.HocKy);
        Assert.Equal("TAM_KHOA", cu.TrangThaiQuyenMuon);
        var moi = await DongBoSinhVienTestSupport.DocSvUngDungAsync(db, maSvMoi);
        Assert.NotNull(moi);
        Assert.Equal("BINH_THUONG", moi.TrangThaiQuyenMuon);
        Assert.Equal(maNhomMoi, moi.MaNhom);
        var tenNhom = await db.Database.SqlQuery<string>($"""
            SELECT ten_nhom AS "Value" FROM sinh_vien.nhom_sinh_vien WHERE ma_nhom = {maNhomMoi}
            """).SingleAsync();
        Assert.Equal("Nhóm D11 kiểm thử", tenNhom);
        Assert.Equal(1, await DongBoSinhVienTestSupport.DemMocAsync(db, moc.AddDays(2)));
    }

    [Fact]
    public async Task UC19_J8_BanGhiBangMoc_KhongKeoLaiVaGiuNguyenMoc()
    {
        await using var dichVu = DongBoSinhVienTestSupport.TaoDichVu();
        await using var phamVi = dichVu.CreateAsyncScope();
        var db = phamVi.ServiceProvider.GetRequiredService<AppDbContext>();
        await using var giaoDich = await db.Database.BeginTransactionAsync();
        var moc = DongBoSinhVienTestSupport.MocGoc;
        await DongBoSinhVienTestSupport.DatMocAsync(db, moc);
        var ma = DongBoSinhVienTestSupport.TaoMa("SV-D11-BANG");
        await DongBoSinhVienTestSupport.ThemSvTruongAsync(db, ma, "SV Bang Moc", moc);

        var soDaXuLy = await TaoJob(db).ChayAsync(CancellationToken.None);

        Assert.Equal(0, soDaXuLy);
        Assert.Null(await DongBoSinhVienTestSupport.DocSvUngDungAsync(db, ma));
        Assert.Equal(1, await DongBoSinhVienTestSupport.DemMocAsync(db, moc));
    }

    [Fact]
    public async Task UC19_J8_ChayLai_Idempotent()
    {
        await using var dichVu = DongBoSinhVienTestSupport.TaoDichVu();
        await using var phamVi = dichVu.CreateAsyncScope();
        var db = phamVi.ServiceProvider.GetRequiredService<AppDbContext>();
        await using var giaoDich = await db.Database.BeginTransactionAsync();
        var moc = DongBoSinhVienTestSupport.MocGoc;
        await DongBoSinhVienTestSupport.DatMocAsync(db, moc);
        var ma = DongBoSinhVienTestSupport.TaoMa("SV-D11-LAI");
        await DongBoSinhVienTestSupport.ThemSvTruongAsync(db, ma, "SV Chay Lai", moc.AddDays(1));
        var job = TaoJob(db);

        var lanMot = await job.ChayAsync(CancellationToken.None);
        var truocLanHai = await DongBoSinhVienTestSupport.DocSvUngDungAsync(db, ma);
        var lanHai = await job.ChayAsync(CancellationToken.None);
        var sauLanHai = await DongBoSinhVienTestSupport.DocSvUngDungAsync(db, ma);

        Assert.Equal(1, lanMot);
        Assert.Equal(0, lanHai);
        Assert.NotNull(truocLanHai);
        Assert.NotNull(sauLanHai);
        Assert.Equal(truocLanHai.HoTen, sauLanHai.HoTen);
        Assert.Equal(1, await DongBoSinhVienTestSupport.DemMocAsync(db, moc.AddDays(1)));
    }

    [Fact]
    public async Task UC19_J8_MotSvLoi_KhongChanSvKhacVaTienMocTheoBanGhiThanhCong()
    {
        await using var dichVu = DongBoSinhVienTestSupport.TaoDichVu();
        await using var phamVi = dichVu.CreateAsyncScope();
        var db = phamVi.ServiceProvider.GetRequiredService<AppDbContext>();
        await using var giaoDich = await db.Database.BeginTransactionAsync();
        var moc = DongBoSinhVienTestSupport.MocGoc;
        await DongBoSinhVienTestSupport.DatMocAsync(db, moc);
        var maLoi = "   ";
        var maTot = DongBoSinhVienTestSupport.TaoMa("SV-D11-TOT");
        await DongBoSinhVienTestSupport.ThemSvTruongAsync(db, maLoi, "SV Ma Rong", moc.AddDays(1));
        await DongBoSinhVienTestSupport.ThemSvTruongAsync(db, maTot, "SV Tot", moc.AddDays(2));

        var soDaXuLy = await TaoJob(db).ChayAsync(CancellationToken.None);

        Assert.Equal(1, soDaXuLy);
        var tot = await DongBoSinhVienTestSupport.DocSvUngDungAsync(db, maTot);
        Assert.NotNull(tot);
        Assert.Equal("SV Tot", tot.HoTen);
        Assert.Null(await DongBoSinhVienTestSupport.DocSvUngDungAsync(db, maLoi));
        Assert.Equal(1, await DongBoSinhVienTestSupport.DemMocAsync(db, moc.AddDays(2)));
    }

    [Fact]
    public async Task UC19_J8_BanGhiThieuCapNhatLuc_BoQuaVaTienMocTheoBanGhiHopLe()
    {
        await using var dichVu = DongBoSinhVienTestSupport.TaoDichVu();
        await using var phamVi = dichVu.CreateAsyncScope();
        var db = phamVi.ServiceProvider.GetRequiredService<AppDbContext>();
        await using var giaoDich = await db.Database.BeginTransactionAsync();
        var moc = DongBoSinhVienTestSupport.MocGoc;
        await DongBoSinhVienTestSupport.DatMocAsync(db, moc);
        var maTot = DongBoSinhVienTestSupport.TaoMa("SV-D11-VALID");
        var maThieuMoc = DongBoSinhVienTestSupport.TaoMa("SV-D11-NOMOC");

        var svList = new List<SinhVienTruongDto>
        {
            new(maThieuMoc, "SV Thieu Moc", "Lop", "Khoa", "nomoc@truong.local", "DAI_HOC", new DateOnly(2027, 12, 31), "DANG_HOC", "HK1", "Dai Hoc", null),
            new(maTot, "SV Co Moc", "Lop", "Khoa", "valid@truong.local", "DAI_HOC", new DateOnly(2027, 12, 31), "DANG_HOC", "HK1", "Dai Hoc", moc.AddDays(3))
        };

        var truongMock = new TruongTuyChinh(svList);
        var job = new DongBoSinhVienJob(db, truongMock, NullLogger<DongBoSinhVienJob>.Instance);

        var soDaXuLy = await job.ChayAsync(CancellationToken.None);

        Assert.Equal(1, soDaXuLy);
        Assert.NotNull(await DongBoSinhVienTestSupport.DocSvUngDungAsync(db, maTot));
        Assert.Null(await DongBoSinhVienTestSupport.DocSvUngDungAsync(db, maThieuMoc));
        Assert.Equal(1, await DongBoSinhVienTestSupport.DemMocAsync(db, moc.AddDays(3)));
    }

    [Fact]
    public async Task UC19_J8_NguonTruongLoi_NemLoiVaKhongDoiDuLieuHoacMoc()
    {
        await using var dichVu = DongBoSinhVienTestSupport.TaoDichVu();
        await using var phamVi = dichVu.CreateAsyncScope();
        var db = phamVi.ServiceProvider.GetRequiredService<AppDbContext>();
        await using var giaoDich = await db.Database.BeginTransactionAsync();
        var moc = DongBoSinhVienTestSupport.MocGoc;
        await DongBoSinhVienTestSupport.DatMocAsync(db, moc);
        var ma = DongBoSinhVienTestSupport.TaoMa("SV-D11-NGUON");
        await DongBoSinhVienTestSupport.ThemSvUngDungAsync(db, ma, "Ho Ten Cu", "TAM_KHOA");
        var job = new DongBoSinhVienJob(db, new TruongLoi(), NullLogger<DongBoSinhVienJob>.Instance);

        await Assert.ThrowsAsync<InvalidOperationException>(() => job.ChayAsync(CancellationToken.None));

        var sau = await DongBoSinhVienTestSupport.DocSvUngDungAsync(db, ma);
        Assert.NotNull(sau);
        Assert.Equal("Ho Ten Cu", sau.HoTen);
        Assert.Equal("TAM_KHOA", sau.TrangThaiQuyenMuon);
        Assert.Equal(1, await DongBoSinhVienTestSupport.DemMocAsync(db, moc));
    }

    private static DongBoSinhVienJob TaoJob(AppDbContext db) =>
        new(db, new MockSchoolDirectory(db), NullLogger<DongBoSinhVienJob>.Instance);

    private sealed class TruongTuyChinh(IReadOnlyList<SinhVienTruongDto> ds) : ISchoolDirectory
    {
        public Task<KetQuaXacThucTruong> XacThucAsync(string mssv, string matKhau, CancellationToken ct) =>
            throw new NotSupportedException();

        public Task<SinhVienTruongDto?> LayTheoMssvAsync(string mssv, CancellationToken ct) =>
            throw new NotSupportedException();

        public Task<IReadOnlyList<SinhVienTruongDto>> LayThayDoiTuAsync(DateTimeOffset? moc, CancellationToken ct) =>
            Task.FromResult(ds);
    }

    private sealed class TruongLoi : ISchoolDirectory
    {
        public Task<KetQuaXacThucTruong> XacThucAsync(string mssv, string matKhau, CancellationToken ct) =>
            throw new NotSupportedException();

        public Task<SinhVienTruongDto?> LayTheoMssvAsync(string mssv, CancellationToken ct) =>
            throw new NotSupportedException();

        public Task<IReadOnlyList<SinhVienTruongDto>> LayThayDoiTuAsync(DateTimeOffset? moc, CancellationToken ct) =>
            throw new InvalidOperationException("Trường giả lập lỗi.");
    }
}
