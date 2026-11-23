using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using QuanLyMuonTra.Modules.DanhMuc;
using QuanLyMuonTra.Modules.DanhMuc.Application;
using QuanLyMuonTra.Modules.DanhMuc.Domain;
using QuanLyMuonTra.SharedKernel;
using QuanLyMuonTra.SharedKernel.Contracts;

namespace QuanLyMuonTra.UnitTests.DanhMuc;

public sealed class NgungSuDungUngDungTests
{
    [Fact]
    public void UC12_ThietBi_NgungSuDungHoa_DatCoVaGiuNguyenTinhTrang()
    {
        var thietBi = ThietBi.NhapKho("TB-01", Guid.NewGuid(), Guid.NewGuid(), "Laptop 01", null, null, new DateOnly(2026, 1, 5), null, null, null);

        thietBi.NgungSuDungHoa();
        thietBi.NgungSuDungHoa();

        Assert.True(thietBi.NgungSuDung);
        Assert.Equal(TinhTrangThietBi.SanSang, thietBi.TinhTrang);
    }

    [Fact]
    public void UC12_Loai_NgungSuDungHoa_DatCo()
    {
        var loai = new LoaiThietBi("Laptop", null, null, 30, 10000);

        loai.NgungSuDungHoa();

        Assert.True(loai.NgungSuDung);
    }

    [Fact]
    public void UC12_DangKyModuleDanhMuc_HaiHandlerNgungSuDungGiaiQuyetDuoc()
    {
        using var dichVu = new ServiceCollection()
            .AddDbContext<NgungTestDbContext>(o => o.UseNpgsql("Host=khong-ket-noi;Database=khong-ket-noi"))
            .AddScoped<DbContext>(sp => sp.GetRequiredService<NgungTestDbContext>())
            .AddScoped<IAdvisoryLockService, KhoaGia>()
            .AddScoped<IAnhService, AnhServiceGia>()
            .AddScoped<IAvailabilityService, AvailabilityServiceGia>()
            .AddDanhMucModule()
            .BuildServiceProvider();
        using var phamVi = dichVu.CreateScope();

        Assert.NotNull(phamVi.ServiceProvider.GetRequiredService<NgungSuDungLoaiThietBiHandler>());
        Assert.NotNull(phamVi.ServiceProvider.GetRequiredService<NgungSuDungThietBiHandler>());
    }

    private sealed class NgungTestDbContext(DbContextOptions<NgungTestDbContext> options) : DbContext(options)
    {
    }

    private sealed class KhoaGia : IAdvisoryLockService
    {
        public Task LockPhongLoaiAsync(Guid maPhong, Guid maLoai, CancellationToken ct) => Task.CompletedTask;

        public Task LockDanhSachPhongLoaiAsync(IEnumerable<(Guid MaPhong, Guid MaLoai)> danhSach, CancellationToken ct) => Task.CompletedTask;
    }
}
