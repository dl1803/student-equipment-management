using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using QuanLyMuonTra.Infrastructure;
using QuanLyMuonTra.Infrastructure.Persistence;
using QuanLyMuonTra.IntegrationTests.Database;
using QuanLyMuonTra.Modules.DanhMuc.Contracts;
using QuanLyMuonTra.Modules.QuyDinh.Contracts;
using QuanLyMuonTra.Modules.SinhVien.Contracts;
using QuanLyMuonTra.Modules.ViPham.Infrastructure;

namespace QuanLyMuonTra.IntegrationTests.ViPham;

public class ViPhamThamChieuTests
{
    [Fact]
    public async Task UC09_ReaderNo_BienBanChuaNopTinhVaoNoCuaDungSv()
    {
        await using var dichVu = TaoDichVu();
        await using var phamVi = dichVu.CreateAsyncScope();
        var db = phamVi.ServiceProvider.GetRequiredService<AppDbContext>();
        await using var giaoDich = await db.Database.BeginTransactionAsync();
        var du = await TaoBienBanAsync(db);
        var reader = new BienBanChuaNopReader(db);

        var cuaSv = await reader.LayDsChuaNopAsync(du.MaSv, CancellationToken.None);
        var cuaNguoiKhac = await reader.LayDsChuaNopAsync("KT03-KHONG-CO-NO", CancellationToken.None);

        var bienBan = Assert.Single(cuaSv);
        Assert.Equal(du.MaBienBan, bienBan.MaBienBan);
        Assert.Equal(240000L, bienBan.TongTien);
        Assert.Empty(cuaNguoiKhac);
    }

    [Fact]
    public async Task UC09_ReaderNo_MaSvChuaChuanHoa_VanTimRaNo()
    {
        await using var dichVu = TaoDichVu();
        await using var phamVi = dichVu.CreateAsyncScope();
        var db = phamVi.ServiceProvider.GetRequiredService<AppDbContext>();
        await using var giaoDich = await db.Database.BeginTransactionAsync();
        var du = await TaoBienBanAsync(db);
        var reader = new BienBanChuaNopReader(db);

        var cuaSv = await reader.LayDsChuaNopAsync($"  {du.MaSv.ToLowerInvariant()}  ", CancellationToken.None);

        var bienBan = Assert.Single(cuaSv);
        Assert.Equal(du.MaBienBan, bienBan.MaBienBan);
    }

    [Fact]
    public async Task UC09_ReaderNo_BienBanDaNopHoacDaHuy_KhongTinhLaNo()
    {
        await using var dichVu = TaoDichVu();
        await using var phamVi = dichVu.CreateAsyncScope();
        var db = phamVi.ServiceProvider.GetRequiredService<AppDbContext>();
        await using var giaoDich = await db.Database.BeginTransactionAsync();
        var du = await TaoBienBanAsync(db);
        var reader = new BienBanChuaNopReader(db);

        await db.Database.ExecuteSqlInterpolatedAsync(
            $"UPDATE vi_pham.bien_ban_vi_pham SET trang_thai = 'DA_NOP', thoi_diem_nop = now() WHERE ma_bien_ban = {du.MaBienBan}");
        var sauKhiNop = await reader.LayDsChuaNopAsync(du.MaSv, CancellationToken.None);
        await db.Database.ExecuteSqlInterpolatedAsync(
            $"UPDATE vi_pham.bien_ban_vi_pham SET trang_thai = 'DA_HUY', thoi_diem_nop = NULL WHERE ma_bien_ban = {du.MaBienBan}");
        var sauKhiHuy = await reader.LayDsChuaNopAsync(du.MaSv, CancellationToken.None);

        Assert.Empty(sauKhiNop);
        Assert.Empty(sauKhiHuy);
    }

    [Fact]
    public async Task UC12_ProviderDanhMuc_ThietBiCoTrongBienBan_TraTrueVaMaLaTraFalse()
    {
        await using var dichVu = TaoDichVu();
        await using var phamVi = dichVu.CreateAsyncScope();
        var db = phamVi.ServiceProvider.GetRequiredService<AppDbContext>();
        await using var giaoDich = await db.Database.BeginTransactionAsync();
        var du = await TaoBienBanAsync(db);
        var provider = new ThamChieuDanhMucViPhamProvider(db);

        var coThamChieu = await provider.CoThamChieuThietBiAsync(du.MaThietBi, CancellationToken.None);
        var khongThamChieu = await provider.CoThamChieuThietBiAsync("KT03-TB-KHONG-CO", CancellationToken.None);

        Assert.True(coThamChieu);
        Assert.False(khongThamChieu);
    }

    [Fact]
    public async Task UC13_ProviderKhungPhat_KhungCoTrongBienBan_TraTrueVaKhungLaTraFalse()
    {
        await using var dichVu = TaoDichVu();
        await using var phamVi = dichVu.CreateAsyncScope();
        var db = phamVi.ServiceProvider.GetRequiredService<AppDbContext>();
        await using var giaoDich = await db.Database.BeginTransactionAsync();
        var du = await TaoBienBanAsync(db);
        var provider = new ThamChieuKhungPhatViPhamProvider(db);

        var coThamChieu = await provider.CoThamChieuKhungAsync(du.MaKhung, CancellationToken.None);
        var khongThamChieu = await provider.CoThamChieuKhungAsync(Guid.CreateVersion7(), CancellationToken.None);

        Assert.True(coThamChieu);
        Assert.False(khongThamChieu);
    }

    [Fact]
    public void UC09_Runtime_CoBanHienThucThatCuaViPhamTrongDi()
    {
        using var factory = new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder =>
            {
                builder.UseSetting(
                    "ConnectionStrings:Default",
                    "Host=localhost;Database=khong-dung;Username=khong-dung;Password=khong-dung");
                builder.UseSetting("Jwt:Secret", "chuoi-khoa-jwt-kiem-thu-it-nhat-32-ky-tu-tro-len");
            });
        using var phamVi = factory.Services.CreateScope();

        var reader = phamVi.ServiceProvider.GetServices<IBienBanChuaNopReader>().Select(x => x.GetType()).ToList();
        var danhMuc = phamVi.ServiceProvider.GetServices<IThamChieuDanhMucProvider>().Select(x => x.GetType()).ToList();
        var khungPhat = phamVi.ServiceProvider.GetServices<IThamChieuKhungPhatProvider>().Select(x => x.GetType()).ToList();

        Assert.Contains(typeof(BienBanChuaNopReader), reader);
        Assert.Contains(typeof(ThamChieuDanhMucViPhamProvider), danhMuc);
        Assert.Contains(typeof(ThamChieuKhungPhatViPhamProvider), khungPhat);
    }

    private static ServiceProvider TaoDichVu() =>
        new ServiceCollection()
            .AddInfrastructure(KetNoiKiemThu.LayChuoiKetNoi(), [typeof(ViPhamThamChieuTests).Assembly])
            .BuildServiceProvider();

    private static async Task<DuLieuBienBan> TaoBienBanAsync(AppDbContext db)
    {
        var hau = Random.Shared.Next(100000, 999999);
        var maSv = $"KT03-SV-{hau}";
        var maNv = Guid.CreateVersion7();
        var maPhong = Guid.CreateVersion7();
        var maLoai = Guid.CreateVersion7();
        var maKhung = Guid.CreateVersion7();
        var maThietBi = $"KT03-TB-{hau}";
        var maPhieu = $"PM-999999-{hau}";
        var maBienBan = $"BB-999999-{hau}";
        var tenDangNhap = $"kt03-{hau}";

        await db.Database.ExecuteSqlInterpolatedAsync($"""
            INSERT INTO sinh_vien.sinh_vien (ma_sv, ho_ten, email, ma_nhom, han_the, trang_thai_hoc, hoc_ky)
            VALUES ({maSv}, 'SV kiem thu', 'kt03@example.com', 'DAI_HOC', DATE '2030-01-01', 'DANG_HOC', 'HK1')
            """);
        await db.Database.ExecuteSqlInterpolatedAsync($"""
            INSERT INTO tai_khoan.nguoi_dung_he_thong (ma_nguoi_dung, ten_dang_nhap, mat_khau_bam, ho_ten)
            VALUES ({maNv}, {tenDangNhap}, 'khong-dung', 'NV kiem thu')
            """);
        await db.Database.ExecuteSqlInterpolatedAsync($"""
            INSERT INTO tai_khoan.nhan_vien_quan_ly (ma_nguoi_dung, ma_nv) VALUES ({maNv}, {tenDangNhap})
            """);
        await db.Database.ExecuteSqlInterpolatedAsync($"""
            INSERT INTO danh_muc.phong_thiet_bi (ma_phong, ten_phong, co_so) VALUES ({maPhong}, 'Phong KT03', 'CS1')
            """);
        await db.Database.ExecuteSqlInterpolatedAsync($"""
            INSERT INTO danh_muc.loai_thiet_bi (ma_loai, ten_loai, chu_ky_bao_tri, phi_tre_moi_ngay)
            VALUES ({maLoai}, 'Loai KT03', 180, 0)
            """);
        await db.Database.ExecuteSqlInterpolatedAsync($"""
            INSERT INTO danh_muc.thiet_bi (ma_thiet_bi, ma_loai, ma_phong, ten_thiet_bi, ngay_nhap_kho)
            VALUES ({maThietBi}, {maLoai}, {maPhong}, 'Thiet bi KT03', DATE '2026-01-01')
            """);
        await db.Database.ExecuteSqlInterpolatedAsync($"""
            INSERT INTO quy_dinh.khung_phat (ma_khung, ten_khung, ty_le_phan_tram) VALUES ({maKhung}, 'Khung KT03', 50)
            """);
        await db.Database.ExecuteSqlInterpolatedAsync($"""
            INSERT INTO muon_tra.phieu_muon (ma_phieu, ma_sv, ma_nv_lap, ngay_gio_muon, ngay_gio_hen_tra)
            VALUES ({maPhieu}, {maSv}, {maNv}, now() - interval '2 days', now() - interval '1 day')
            """);
        await db.Database.ExecuteSqlInterpolatedAsync($"""
            INSERT INTO muon_tra.chi_tiet_phieu_muon (ma_chi_tiet, ma_phieu, ma_thiet_bi, tinh_trang_ban_dau, phi_tre_moi_ngay)
            VALUES ({Guid.CreateVersion7()}, {maPhieu}, {maThietBi}, 'Binh thuong', 0)
            """);
        await db.Database.ExecuteSqlInterpolatedAsync($"""
            INSERT INTO vi_pham.bien_ban_vi_pham (ma_bien_ban, ma_phieu_muon, ma_nv_lap, tong_tien)
            VALUES ({maBienBan}, {maPhieu}, {maNv}, 240000)
            """);
        await db.Database.ExecuteSqlInterpolatedAsync($"""
            INSERT INTO vi_pham.chi_tiet_bien_ban (ma_bien_ban, ma_phieu_muon, ma_thiet_bi, loai_vi_pham, so_tien, ma_khung)
            VALUES ({maBienBan}, {maPhieu}, {maThietBi}, 'HU_HONG', 240000, {maKhung})
            """);
        await db.Database.ExecuteSqlInterpolatedAsync($"""
            INSERT INTO vi_pham.vi_pham (ma_vi_pham, ma_sv, loai_vi_pham, hoc_ky, ma_bien_ban)
            VALUES ({Guid.CreateVersion7()}, {maSv}, 'HU_HONG', 'HK1', {maBienBan})
            """);

        return new DuLieuBienBan(maSv, maBienBan, maThietBi, maKhung);
    }

    private sealed record DuLieuBienBan(string MaSv, string MaBienBan, string MaThietBi, Guid MaKhung);
}

