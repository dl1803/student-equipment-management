using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using QuanLyMuonTra.Infrastructure;
using QuanLyMuonTra.Infrastructure.Persistence;
using QuanLyMuonTra.IntegrationTests.Database;
using QuanLyMuonTra.Modules.TaiKhoan.Domain;
using QuanLyMuonTra.Modules.TaiKhoan.Infrastructure;
using QuanLyMuonTra.SharedKernel;

namespace QuanLyMuonTra.IntegrationTests.TaiKhoan;

public class QuyenPhongTests
{
    [Fact]
    public async Task UC14_QuyenPhong_ChiTraVeCacPhongDuocCap()
    {
        await using var dichVu = TaoDichVu();
        await using var phamVi = dichVu.CreateAsyncScope();
        var db = phamVi.ServiceProvider.GetRequiredService<AppDbContext>();
        await using var giaoDich = await db.Database.BeginTransactionAsync();
        var maNv = await ThemNhanVienAsync(db, "HOAT_DONG");
        var phongDuocCap = await ThemPhongAsync(db);
        var phongKhac = await ThemPhongAsync(db);
        await CapQuyenAsync(db, maNv, phongDuocCap);
        var dichVuQuyen = TaoDichVuQuyen(db);

        var dsPhong = await dichVuQuyen.LayDsMaPhongAsync(maNv, CancellationToken.None);

        Assert.Equal(new[] { phongDuocCap }, dsPhong);
        await dichVuQuyen.EnsureAsync(maNv, phongDuocCap, CancellationToken.None);
        var loi = await Assert.ThrowsAsync<LoiNghiepVuException>(
            () => dichVuQuyen.EnsureAsync(maNv, phongKhac, CancellationToken.None));
        Assert.Equal("PHONG_CHUA_CAP_QUYEN", loi.MaLoi);
    }

    [Theory]
    [InlineData("HOAT_DONG")]
    [InlineData("TAM_KHOA_DANG_NHAP")]
    [InlineData("BI_KHOA")]
    public async Task UC19_TrangThaiTaiKhoanReader_TraDungTrangThaiDangLuu(string trangThai)
    {
        await using var dichVu = TaoDichVu();
        await using var phamVi = dichVu.CreateAsyncScope();
        var db = phamVi.ServiceProvider.GetRequiredService<AppDbContext>();
        await using var giaoDich = await db.Database.BeginTransactionAsync();
        var maNv = await ThemNhanVienAsync(db, trangThai);
        var boDoc = new TrangThaiTaiKhoanReader(db);

        Assert.Equal(trangThai, await boDoc.LayTrangThaiKhoaAsync(maNv, CancellationToken.None));
        Assert.Null(await boDoc.LayTrangThaiKhoaAsync(Guid.CreateVersion7(), CancellationToken.None));
    }

    [Fact]
    public async Task UC14_QuyenPhong_DemTrongBoNho_VaXoaDemThiDocLaiTuCsdl()
    {
        await using var dichVu = TaoDichVu();
        await using var phamVi = dichVu.CreateAsyncScope();
        var db = phamVi.ServiceProvider.GetRequiredService<AppDbContext>();
        await using var giaoDich = await db.Database.BeginTransactionAsync();
        var maNv = await ThemNhanVienAsync(db, "HOAT_DONG");
        var phong = await ThemPhongAsync(db);
        var dichVuQuyen = TaoDichVuQuyen(db);

        Assert.Empty(await dichVuQuyen.LayDsMaPhongAsync(maNv, CancellationToken.None));

        await CapQuyenAsync(db, maNv, phong);
        Assert.Empty(await dichVuQuyen.LayDsMaPhongAsync(maNv, CancellationToken.None));

        dichVuQuyen.Xoa(maNv);
        Assert.Equal(new[] { phong }, await dichVuQuyen.LayDsMaPhongAsync(maNv, CancellationToken.None));
    }

    private static QuyenPhongService TaoDichVuQuyen(AppDbContext db) =>
        new(db, new MemoryCache(new MemoryCacheOptions()));

    internal static async Task<Guid> ThemNhanVienAsync(AppDbContext db, string trangThaiKhoa)
    {
        var ma = Guid.CreateVersion7();
        var tenDangNhap = $"nv-kt-{ma:N}";
        var maNv = $"NV-KT-{ma:N}";
        var bam = new PasswordHasher<NguoiDungHeThong>().HashPassword(null!, "MatKhauKiemThu-1");
        var nv = NhanVienQuanLy.Tao(ma, tenDangNhap, bam, "Nhan vien kiem thu", maNv, null, trangThaiKhoa);
        db.Set<NhanVienQuanLy>().Add(nv);
        await db.SaveChangesAsync();
        return ma;
    }

    private static async Task<Guid> ThemPhongAsync(AppDbContext db)
    {
        var ma = Guid.CreateVersion7();
        var ten = $"Phong KT {ma:N}";
        await db.Database.ExecuteSqlInterpolatedAsync($"""
            INSERT INTO danh_muc.phong_thiet_bi (ma_phong, ten_phong, co_so) VALUES ({ma}, {ten}, 'CS1')
            """);
        return ma;
    }

    private static async Task CapQuyenAsync(AppDbContext db, Guid maNv, Guid maPhong)
    {
        var nvPhong = NhanVienPhong.Tao(maNv, maPhong);
        db.Set<NhanVienPhong>().Add(nvPhong);
        await db.SaveChangesAsync();
    }

    private static ServiceProvider TaoDichVu() =>
        new ServiceCollection()
            .AddInfrastructure(KetNoiKiemThu.LayChuoiKetNoi(), [typeof(NguoiDungHeThong).Assembly])
            .BuildServiceProvider();
}

