using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using QuanLyMuonTra.Infrastructure.Auditing;
using QuanLyMuonTra.Infrastructure.LuuTru;
using QuanLyMuonTra.Modules.DanhMuc.Contracts;
using QuanLyMuonTra.Modules.DanhMuc.Infrastructure;
using QuanLyMuonTra.Modules.MuonTra.Application;
using QuanLyMuonTra.Modules.MuonTra.Contracts;
using QuanLyMuonTra.Modules.MuonTra.Infrastructure;
using QuanLyMuonTra.Modules.SinhVien.Contracts;
using QuanLyMuonTra.Modules.SinhVien.Infrastructure;
using QuanLyMuonTra.Modules.TaiKhoan.Contracts;
using QuanLyMuonTra.Modules.TaiKhoan.Infrastructure;
using QuanLyMuonTra.Modules.ThongBao.Contracts;
using QuanLyMuonTra.Modules.ThongBao.Infrastructure;
using QuanLyMuonTra.Modules.ViPham.Infrastructure;
using QuanLyMuonTra.SharedKernel;
using QuanLyMuonTra.SharedKernel.Contracts;

namespace QuanLyMuonTra.IntegrationTests.MuonTra;

public class DiRuntimeKhongBanGiaTests
{
    [Fact]
    public void UC06_UC07_RuntimeDangKyBanThatChoMoiCongCuaDuyetTuChoiVaGiao()
    {
        using var factory = new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder =>
            {
                builder.UseSetting("ConnectionStrings:Default", "Host=localhost;Database=khong-dung;Username=khong-dung;Password=khong-dung");
                builder.UseSetting("Jwt:Secret", "khoa-kiem-thu-khong-phai-bi-mat-0123456789");
                builder.UseSetting("Jwt:Issuer", "kiem-thu");
                builder.UseSetting("Jwt:Audience", "kiem-thu");
            });

        using var phamVi = factory.Services.CreateScope();
        var dichVu = phamVi.ServiceProvider;

        Assert.IsType<ThongBaoDuyetOutbox>(dichVu.GetRequiredService<IThongBaoDuyetOutbox>());
        Assert.IsType<ThongBaoOutbox>(dichVu.GetRequiredService<IThongBaoOutbox>());
        Assert.IsType<QuyenPhongService>(dichVu.GetRequiredService<IQuyenPhongService>());
        Assert.IsType<AuditLogger>(dichVu.GetRequiredService<IAuditLogger>());
        Assert.IsType<AnhService>(dichVu.GetRequiredService<IAnhService>());
        Assert.IsType<AvailabilityService>(dichVu.GetRequiredService<IAvailabilityService>());
        Assert.IsType<AdvisoryLockService>(dichVu.GetRequiredService<IAdvisoryLockService>());
        Assert.IsType<MuonTraReader>(dichVu.GetRequiredService<IMuonTraReader>());
        Assert.IsType<ThietBiService>(dichVu.GetRequiredService<IThietBiService>());
        Assert.IsType<HoSoSinhVienReader>(dichVu.GetRequiredService<IHoSoSinhVienReader>());
        Assert.IsType<BienBanChuaNopReader>(dichVu.GetRequiredService<IBienBanChuaNopReader>());

        Assert.NotNull(dichVu.GetRequiredService<DuyetPhieuYeuCauMuonHandler>());
        Assert.NotNull(dichVu.GetRequiredService<TuChoiPhieuYeuCauMuonHandler>());
        Assert.NotNull(dichVu.GetRequiredService<GiaoThietBiHandler>());

        Assert.DoesNotContain(
            typeof(ThongBaoDuyetOutbox).Assembly.GetTypes(),
            kieu => kieu.Name.EndsWith("ChuaNoi", StringComparison.Ordinal));
    }
}
