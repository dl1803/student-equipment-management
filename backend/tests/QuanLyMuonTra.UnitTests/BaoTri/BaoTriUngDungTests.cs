using Microsoft.EntityFrameworkCore;
using QuanLyMuonTra.Modules.BaoTri.Application;
using QuanLyMuonTra.Modules.TaiKhoan.Contracts;
using QuanLyMuonTra.SharedKernel;

namespace QuanLyMuonTra.UnitTests.BaoTri;

public sealed class BaoTriUngDungTests
{
    private static readonly Guid MaNv = Guid.Parse("019c0000-0000-7000-8000-0000000c1221");

    [Theory]
    [InlineData(0, 20)]
    [InlineData(-1, 20)]
    [InlineData(1, 0)]
    [InlineData(1, 101)]
    public async Task UC11_DsBaoTri_TrangHoacKichThuocNgoaiKhoang_Nem400TruocKhiTruyCapCsdl(int trang, int kichThuoc)
    {
        using var db = TaoDbContext();
        var quyenPhong = new QuyenPhongGia();
        var cacLenh = new Func<Task>[]
        {
            () => new DsThietBiDenHanBaoTriHandler(db, new DongHoGia(), quyenPhong).XuLyAsync(MaNv, null, trang, kichThuoc, CancellationToken.None),
            () => new DsChoTiepNhanSuaChuaHandler(db, quyenPhong).XuLyAsync(MaNv, null, trang, kichThuoc, CancellationToken.None),
        };

        foreach (var lenh in cacLenh)
        {
            var loi = await Assert.ThrowsAsync<LoiNghiepVuException>(lenh);

            Assert.Equal("DU_LIEU_KHONG_HOP_LE", loi.MaLoi);
            Assert.Equal(400, loi.MaHttp);
        }
    }

    [Theory]
    [InlineData("", "BAO_TRI_DINH_KY", null)]
    [InlineData("   ", "SUA_CHUA", null)]
    [InlineData("TB-01", "KHAC", null)]
    [InlineData("TB-01", "sua_chua", null)]
    [InlineData("TB-01", "BAO_TRI_DINH_KY", "PM-261123-991203")]
    [InlineData("TB-01", "SUA_CHUA", "   ")]
    public async Task UC11_LapBaoTri_DauVaoSai_Nem400TruocKhiTruyCapCsdl(string maThietBi, string loai, string? maPhieu)
    {
        using var db = TaoDbContext();
        var handler = new LapBaoTriHandler(db, null!, null!, null!, null!, null!, null!);

        var loi = await Assert.ThrowsAsync<LoiNghiepVuException>(
            () => handler.XuLyAsync(MaNv, new LapBaoTriRequest(maThietBi, loai, maPhieu), CancellationToken.None));

        Assert.Equal("DU_LIEU_KHONG_HOP_LE", loi.MaLoi);
        Assert.Equal(400, loi.MaHttp);
    }

    private static KhongKetNoiDbContext TaoDbContext() =>
        new(new DbContextOptionsBuilder<KhongKetNoiDbContext>()
            .UseNpgsql("Host=khong-ket-noi;Database=khong-ket-noi")
            .Options);

    private sealed class KhongKetNoiDbContext(DbContextOptions<KhongKetNoiDbContext> options) : DbContext(options)
    {
    }

    private sealed class QuyenPhongGia : IQuyenPhongService
    {
        public Task<IReadOnlyCollection<Guid>> LayDsMaPhongAsync(Guid maNguoiDung, CancellationToken ct) =>
            Task.FromResult<IReadOnlyCollection<Guid>>([]);

        public Task EnsureAsync(Guid maNguoiDung, Guid maPhong, CancellationToken ct) => Task.CompletedTask;
    }

    private sealed class DongHoGia : IClock
    {
        public DateTimeOffset UtcNow => new(2026, 11, 23, 5, 0, 0, TimeSpan.Zero);
    }
}
