using QuanLyMuonTra.Modules.QuyDinh.Domain;
using QuanLyMuonTra.SharedKernel;

namespace QuanLyMuonTra.UnitTests.QuyDinh;

public sealed class CauHinhNhatKyTests
{
    [Fact]
    public void UC13_NhatKy_KhungPhatVaThamSoDanhDauIAuditable()
    {
        Assert.IsAssignableFrom<IAuditable>(new KhungPhat("Nhẹ", 15m));
        Assert.IsAssignableFrom<IAuditable>(new ThamSoHeThong(3, 30, 15, 2, 3, 5, null, null));
    }

    [Fact]
    public void UC13_GiaTriCauHinhSai_TraMaLoi422()
    {
        var loi = Assert.Throws<LoiNghiepVuException>(() => new KhungPhat("Khung", 150m));

        Assert.Equal("GIA_TRI_CAU_HINH_KHONG_HOP_LE", loi.MaLoi);
        Assert.Equal(422, loi.MaHttp);
    }
}
