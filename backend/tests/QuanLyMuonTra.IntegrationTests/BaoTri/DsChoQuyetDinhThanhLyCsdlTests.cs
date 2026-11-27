using QuanLyMuonTra.IntegrationTests.MuonTra;
using QuanLyMuonTra.Modules.BaoTri.Application;

namespace QuanLyMuonTra.IntegrationTests.BaoTri;

[Collection(ThanhLyDuLieu.TenBoSuuTap)]
public sealed class DsChoQuyetDinhThanhLyCsdlTests
{
    private static readonly CancellationToken Ct = CancellationToken.None;

    [CanKetNoiTestFact]
    public async Task UC15_DsCho_ChiGomDeXuatGanNhatChuaCoQuyetDinh_SapTheoNgayHoanThanh()
    {
        await using var mt = await BaoTriMoiTruong.TaoAsync(ThanhLyDuLieu.TenTep);

        var ketQua = await mt.Lay<DsChoQuyetDinhThanhLyHandler>().XuLyAsync(1, 100, Ct);

        var cacDong = ketQua.DanhSach
            .Where(d => d.MaThietBi.StartsWith("D16-TB-", StringComparison.Ordinal))
            .ToList();
        Assert.Equal(["D16-TB-02", "D16-TB-01"], cacDong.Select(d => d.MaThietBi).ToArray());

        var tb02 = cacDong[0];
        Assert.Equal(ThanhLyDuLieu.BaoTriTb02, tb02.MaBaoTri);
        Assert.Equal("Không sửa được", tb02.NoiDungThucHien);
        Assert.Equal(new DateTimeOffset(2026, 11, 18, 10, 0, 0, TimeSpan.FromHours(7)), tb02.NgayHoanThanh);
        Assert.Equal(8000000L, tb02.GiaTriHienHanh);
        Assert.Equal("Phòng D16 P1", tb02.TenPhong);

        var tb01 = cacDong[1];
        Assert.Equal(ThanhLyDuLieu.BaoTriTb01, tb01.MaBaoTri);
        Assert.Equal(10000000L, tb01.GiaTriHienHanh);
    }

    [CanKetNoiTestFact]
    public async Task UC15_DsCho_PhanTrang_TongSoDungVaTrangVuotQuaRong()
    {
        await using var mt = await BaoTriMoiTruong.TaoAsync(ThanhLyDuLieu.TenTep);
        var boXuLy = mt.Lay<DsChoQuyetDinhThanhLyHandler>();

        var trangMot = await boXuLy.XuLyAsync(1, 1, Ct);
        var trangVuot = await boXuLy.XuLyAsync(1000000, 100, Ct);

        Assert.True(trangMot.TongSo >= 2);
        Assert.Single(trangMot.DanhSach);
        Assert.Equal(trangMot.TongSo, trangVuot.TongSo);
        Assert.Empty(trangVuot.DanhSach);
    }
}
