using Microsoft.Extensions.DependencyInjection;
using QuanLyMuonTra.Modules.MuonTra;
using QuanLyMuonTra.Modules.MuonTra.Domain;
using QuanLyMuonTra.SharedKernel;
using QuanLyMuonTra.SharedKernel.Contracts;

namespace QuanLyMuonTra.UnitTests.MuonTra;

public class BoTinhHanTraTests
{
    private static readonly TimeOnly GioDongCua = new(21, 0);

    private readonly BoTinhHanTra _boTinhHanTra = new();

    private static DateTimeOffset Vn(int ngay, int gio, int phut)
    {
        return new DateTimeOffset(2026, 11, ngay, gio, phut, 0, TimeSpan.FromHours(7));
    }

    [Theory]
    [InlineData(10, 2, 12)]
    [InlineData(10, 5, 15)]
    [InlineData(10, 10, 20)]
    [InlineData(15, 5, 20)]
    [InlineData(14, 10, 21)]
    [InlineData(18, 5, 21)]
    public void UC07_HanTraLaNhoHonCuaGioGiaoCongThoiGianMuonVaGioDongCua(int gioGiao, int thoiGianMuonGio, int gioHanTra)
    {
        var hanTra = _boTinhHanTra.TinhHanTra(Vn(16, gioGiao, 0), thoiGianMuonGio, GioDongCua);

        Assert.Equal(Vn(16, gioHanTra, 0), hanTra);
    }

    [Fact]
    public void UC07_GiaoTruocGioDongCuaMotPhut_HanTraLaGioDongCua()
    {
        var hanTra = _boTinhHanTra.TinhHanTra(Vn(16, 20, 59), 5, GioDongCua);

        Assert.Equal(Vn(16, 21, 0), hanTra);
    }

    [Theory]
    [InlineData(21, 0)]
    [InlineData(21, 1)]
    [InlineData(22, 30)]
    [InlineData(23, 59)]
    public void UC07_GiaoTuGioDongCuaTroDi_NemGiaoSauGioDongCua(int gio, int phut)
    {
        var loi = Assert.Throws<LoiNghiepVuException>(() => _boTinhHanTra.TinhHanTra(Vn(16, gio, phut), 5, GioDongCua));

        Assert.Equal("GIAO_SAU_GIO_DONG_CUA", loi.MaLoi);
        Assert.Equal(422, loi.MaHttp);
    }

    [Fact]
    public void UC07_ChuaCauHinhGioDongCua_NemChuaCauHinhGioQuay()
    {
        var loi = Assert.Throws<LoiNghiepVuException>(() => _boTinhHanTra.TinhHanTra(Vn(16, 10, 0), 5, null));

        Assert.Equal("CHUA_CAU_HINH_GIO_QUAY", loi.MaLoi);
        Assert.Equal(409, loi.MaHttp);
    }

    [Fact]
    public void UC07_GioGiaoODauNgayVnConLaNgayUtcTruoc_DungGioDongCuaCuaNgayVn()
    {
        var gioGiao = new DateTimeOffset(2026, 11, 15, 23, 30, 0, TimeSpan.Zero);

        var hanTra = _boTinhHanTra.TinhHanTra(gioGiao, 20, GioDongCua);

        Assert.Equal(Vn(16, 21, 0), hanTra);
    }

    [Fact]
    public void UC07_GioGiaoBieuDienOUtc_CungKetQuaVoiGioVietNam()
    {
        var gioGiaoUtc = Vn(16, 18, 0).ToUniversalTime();

        var hanTra = _boTinhHanTra.TinhHanTra(gioGiaoUtc, 5, GioDongCua);

        Assert.Equal(Vn(16, 21, 0), hanTra);
    }

    [Fact]
    public void UC07_HanTraTraVe_LuonODoLechUtc()
    {
        var hanTra = _boTinhHanTra.TinhHanTra(Vn(16, 10, 0), 2, GioDongCua);

        Assert.Equal(TimeSpan.Zero, hanTra.Offset);
    }

    [Fact]
    public void UC07_DangKyModuleMuonTra_GiaiMaDuocBoTinhHanTra()
    {
        using var dichVu = new ServiceCollection().AddMuonTraModule().BuildServiceProvider();
        using var phamVi = dichVu.CreateScope();

        var bo = phamVi.ServiceProvider.GetRequiredService<IBoTinhHanTra>();

        Assert.IsType<BoTinhHanTra>(bo);
    }
}
