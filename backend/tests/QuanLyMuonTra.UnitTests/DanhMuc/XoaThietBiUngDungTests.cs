using Microsoft.EntityFrameworkCore;
using QuanLyMuonTra.Modules.DanhMuc.Application;
using QuanLyMuonTra.SharedKernel.Contracts;

namespace QuanLyMuonTra.UnitTests.DanhMuc;

public sealed class XoaThietBiUngDungTests
{
    [Fact]
    public async Task UC12_XoaThietBi_KhongCoProvider_TuChoiTruocKhiChamDb()
    {
        await using var db = new XoaThietBiKhongKetNoiDbContext(
            new DbContextOptionsBuilder<XoaThietBiKhongKetNoiDbContext>()
                .UseNpgsql("Host=khong-ket-noi;Database=khong-ket-noi")
                .Options);
        var handler = new XoaThietBiHandler(db, new KhoaGia(), [], new AnhServiceGia());

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => handler.XuLyAsync("TB-01", CancellationToken.None));
    }

    private sealed class XoaThietBiKhongKetNoiDbContext(DbContextOptions<XoaThietBiKhongKetNoiDbContext> options) : DbContext(options)
    {
    }

    private sealed class KhoaGia : IAdvisoryLockService
    {
        public Task LockPhongLoaiAsync(Guid maPhong, Guid maLoai, CancellationToken ct) => Task.CompletedTask;

        public Task LockDanhSachPhongLoaiAsync(IEnumerable<(Guid MaPhong, Guid MaLoai)> danhSach, CancellationToken ct) => Task.CompletedTask;
    }
}
