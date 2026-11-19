using System.Text.Json;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using QuanLyMuonTra.Modules.DanhMuc;
using QuanLyMuonTra.Modules.DanhMuc.Application;
using QuanLyMuonTra.SharedKernel;
using QuanLyMuonTra.SharedKernel.Contracts;

namespace QuanLyMuonTra.UnitTests.DanhMuc;

public sealed class ThietBiUngDungTests
{
    private static readonly JsonSerializerOptions TuyChonJson = new(JsonSerializerDefaults.Web);
    private static readonly Guid MaLoai = Guid.Parse("019c0000-0000-7000-8000-000000000011");
    private static readonly Guid MaPhong = Guid.Parse("019c0000-0000-7000-8000-000000000001");

    private static ThietBiTaoMoiRequest YeuCauTao(
        string ma = "TB-01",
        string ten = "Laptop 01",
        long giaTri = 1_000_000,
        IReadOnlyList<Guid>? dsNcc = null,
        IReadOnlyList<Guid>? dsAnh = null) =>
        new(ma, MaLoai, MaPhong, ten, null, null, new DateOnly(2026, 1, 5), null, null, null, dsNcc, dsAnh, giaTri);

    private static ThietBiCapNhatRequest YeuCauSua(
        string ten = "Laptop 01",
        IReadOnlyList<Guid>? dsNcc = null,
        IReadOnlyList<Guid>? dsAnh = null) =>
        new(MaLoai, MaPhong, ten, null, null, new DateOnly(2026, 1, 5), null, null, null, dsNcc, dsAnh);

    private static List<Guid> TaoDanhSach(int soLuong) =>
        Enumerable.Range(1, soLuong).Select(_ => Guid.NewGuid()).ToList();

    [Fact]
    public void UC12_KiemTraTaoThietBi_DuLieuHopLe_Dat()
    {
        var ketQua = new ThietBiTaoMoiRequestValidator().Validate(YeuCauTao(dsNcc: TaoDanhSach(2), dsAnh: TaoDanhSach(5)));

        Assert.True(ketQua.IsValid);
    }

    [Theory]
    [InlineData("", "Laptop", 1L, "MaThietBi")]
    [InlineData("TB-01", "  ", 1L, "TenThietBi")]
    [InlineData("TB-01", "Laptop", -1L, "GiaTri")]
    [InlineData("TB-01", "Laptop", 100_000_000_000_000L, "GiaTri")]
    public void UC12_KiemTraTaoThietBi_DuLieuSai_BaoLoiDungTruong(string ma, string ten, long giaTri, string truongLoi)
    {
        var ketQua = new ThietBiTaoMoiRequestValidator().Validate(YeuCauTao(ma, ten, giaTri));

        Assert.False(ketQua.IsValid);
        Assert.Contains(ketQua.Errors, l => l.PropertyName == truongLoi);
    }

    [Fact]
    public void UC12_KiemTraTaoThietBi_NccTrung_BaoLoi()
    {
        var ma = Guid.NewGuid();

        var ketQua = new ThietBiTaoMoiRequestValidator().Validate(YeuCauTao(dsNcc: [ma, ma]));

        Assert.Contains(ketQua.Errors, l => l.PropertyName == "DsMaNCC");
    }

    [Fact]
    public void UC12_KiemTraTaoThietBi_QuaNamAnh_BaoLoi()
    {
        var ketQua = new ThietBiTaoMoiRequestValidator().Validate(YeuCauTao(dsAnh: TaoDanhSach(6)));

        Assert.Contains(ketQua.Errors, l => l.PropertyName == "DsMaAnh");
    }

    [Fact]
    public void UC12_KiemTraTaoThietBi_AnhTrung_BaoLoi()
    {
        var ma = Guid.NewGuid();

        var ketQua = new ThietBiTaoMoiRequestValidator().Validate(YeuCauTao(dsAnh: [ma, ma]));

        Assert.Contains(ketQua.Errors, l => l.PropertyName == "DsMaAnh");
    }

    [Fact]
    public void UC12_KiemTraSuaThietBi_DuLieuHopLe_Dat()
    {
        var ketQua = new ThietBiCapNhatRequestValidator().Validate(YeuCauSua(dsNcc: TaoDanhSach(1)));

        Assert.True(ketQua.IsValid);
    }

    [Fact]
    public void UC12_KiemTraSuaThietBi_TenRong_BaoLoi()
    {
        var ketQua = new ThietBiCapNhatRequestValidator().Validate(YeuCauSua(ten: string.Empty));

        Assert.Contains(ketQua.Errors, l => l.PropertyName == "TenThietBi");
    }

    [Fact]
    public void UC12_Json_TaoThietBiThieuMa_NemJsonException()
    {
        var json = $$"""{"maLoai":"{{MaLoai}}","maPhong":"{{MaPhong}}","tenThietBi":"A","ngayNhapKho":"2026-01-05","giaTri":1}""";

        Assert.Throws<JsonException>(() => JsonSerializer.Deserialize<ThietBiTaoMoiRequest>(json, TuyChonJson));
    }

    [Fact]
    public void UC12_Json_TaoThietBiThieuGiaTri_NemJsonException()
    {
        var json = $$"""{"maThietBi":"TB-01","maLoai":"{{MaLoai}}","maPhong":"{{MaPhong}}","tenThietBi":"A","ngayNhapKho":"2026-01-05"}""";

        Assert.Throws<JsonException>(() => JsonSerializer.Deserialize<ThietBiTaoMoiRequest>(json, TuyChonJson));
    }

    [Fact]
    public void UC12_Json_TaoThietBiToiThieu_DocDuocVaDanhSachNull()
    {
        var json = $$"""{"maThietBi":"TB-01","maLoai":"{{MaLoai}}","maPhong":"{{MaPhong}}","tenThietBi":"A","ngayNhapKho":"2026-01-05","giaTri":15000000}""";

        var yeuCau = JsonSerializer.Deserialize<ThietBiTaoMoiRequest>(json, TuyChonJson);

        Assert.NotNull(yeuCau);
        Assert.Equal("TB-01", yeuCau!.MaThietBi);
        Assert.Equal(new DateOnly(2026, 1, 5), yeuCau.NgayNhapKho);
        Assert.Equal(15_000_000L, yeuCau.GiaTri);
        Assert.Null(yeuCau.DsMaNCC);
        Assert.Null(yeuCau.DsMaAnh);
        Assert.Null(yeuCau.ThoiHanBaoHanh);
    }

    [Fact]
    public void UC12_Json_SuaThietBiThieuMaLoai_NemJsonException()
    {
        var json = $$"""{"maPhong":"{{MaPhong}}","tenThietBi":"A","ngayNhapKho":"2026-01-05"}""";

        Assert.Throws<JsonException>(() => JsonSerializer.Deserialize<ThietBiCapNhatRequest>(json, TuyChonJson));
    }

    [Fact]
    public void UC12_Json_GiaTriThieuGiaTri_NemJsonException()
    {
        Assert.Throws<JsonException>(() => JsonSerializer.Deserialize<GiaTriThietBiRequest>("{}", TuyChonJson));
    }

    [Theory]
    [InlineData("", 1L)]
    [InlineData("TB-01", -1L)]
    public async Task UC12_TaoThietBi_DuLieuSai_Nem400TruocKhiTruyCapCsdl(string ma, long giaTri)
    {
        using var db = TaoDbContext();
        var handler = new TaoThietBiHandler(db, new DongHoCoDinh());

        var loi = await Assert.ThrowsAsync<LoiNghiepVuException>(
            () => handler.XuLyAsync(YeuCauTao(ma: ma, giaTri: giaTri), CancellationToken.None));

        Assert.Equal("DU_LIEU_KHONG_HOP_LE", loi.MaLoi);
        Assert.Equal(400, loi.MaHttp);
    }

    [Fact]
    public async Task UC12_TaoThietBi_CoMaAnh_TuChoiAnToanTruocKhiTruyCapCsdl()
    {
        using var db = TaoDbContext();
        var handler = new TaoThietBiHandler(db, new DongHoCoDinh());

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => handler.XuLyAsync(YeuCauTao(dsAnh: TaoDanhSach(1)), CancellationToken.None));
    }

    [Fact]
    public async Task UC12_SuaThietBi_CoMaAnh_TuChoiAnToanTruocKhiTruyCapCsdl()
    {
        using var db = TaoDbContext();
        var handler = new SuaThietBiHandler(db, new KhoaGia());

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => handler.XuLyAsync("TB-01", YeuCauSua(dsAnh: TaoDanhSach(1)), CancellationToken.None));
    }

    [Fact]
    public void UC12_DangKyModuleDanhMuc_HandlerVaValidatorThietBiGiaiQuyetDuoc()
    {
        using var dichVu = new ServiceCollection()
            .AddDbContext<ThietBiKhongKetNoiDbContext>(o => o.UseNpgsql("Host=khong-ket-noi;Database=khong-ket-noi"))
            .AddScoped<DbContext>(sp => sp.GetRequiredService<ThietBiKhongKetNoiDbContext>())
            .AddSingleton<IClock, DongHoCoDinh>()
            .AddScoped<IAdvisoryLockService, KhoaGia>()
            .AddDanhMucModule()
            .BuildServiceProvider();
        using var phamVi = dichVu.CreateScope();
        var dv = phamVi.ServiceProvider;

        Assert.NotNull(dv.GetRequiredService<TaoThietBiHandler>());
        Assert.NotNull(dv.GetRequiredService<SuaThietBiHandler>());
        Assert.NotNull(dv.GetRequiredService<IValidator<ThietBiTaoMoiRequest>>());
        Assert.NotNull(dv.GetRequiredService<IValidator<ThietBiCapNhatRequest>>());
        Assert.NotNull(dv.GetRequiredService<IValidator<GiaTriThietBiRequest>>());
    }

    private static ThietBiKhongKetNoiDbContext TaoDbContext()
    {
        var options = new DbContextOptionsBuilder<ThietBiKhongKetNoiDbContext>()
            .UseNpgsql("Host=khong-ket-noi;Database=khong-ket-noi")
            .Options;
        return new ThietBiKhongKetNoiDbContext(options);
    }

    private sealed class ThietBiKhongKetNoiDbContext(DbContextOptions<ThietBiKhongKetNoiDbContext> options) : DbContext(options);

    private sealed class DongHoCoDinh : IClock
    {
        public DateTimeOffset UtcNow => new(2026, 11, 19, 0, 0, 0, TimeSpan.Zero);
    }

    private sealed class KhoaGia : IAdvisoryLockService
    {
        public Task LockPhongLoaiAsync(Guid maPhong, Guid maLoai, CancellationToken ct) => Task.CompletedTask;

        public Task LockDanhSachPhongLoaiAsync(IEnumerable<(Guid MaPhong, Guid MaLoai)> danhSach, CancellationToken ct) =>
            Task.CompletedTask;
    }
}
