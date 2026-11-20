using Microsoft.EntityFrameworkCore;
using QuanLyMuonTra.Modules.DanhMuc.Application;

namespace QuanLyMuonTra.UnitTests.DanhMuc;

public sealed class XoaLoaiThietBiUngDungTests
{
    [Fact]
    public async Task UC12_XoaLoai_KhongCoProvider_TuChoiTruocKhiChamDb()
    {
        await using var db = new XoaLoaiKhongKetNoiDbContext(
            new DbContextOptionsBuilder<XoaLoaiKhongKetNoiDbContext>()
                .UseNpgsql("Host=khong-ket-noi;Database=khong-ket-noi")
                .Options);
        var handler = new XoaLoaiThietBiHandler(db, []);

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => handler.XuLyAsync(Guid.NewGuid(), CancellationToken.None));
    }

    private sealed class XoaLoaiKhongKetNoiDbContext(DbContextOptions<XoaLoaiKhongKetNoiDbContext> options) : DbContext(options)
    {
    }
}
