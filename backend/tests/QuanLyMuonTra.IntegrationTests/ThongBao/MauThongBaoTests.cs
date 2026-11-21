using QuanLyMuonTra.Modules.ThongBao.Application;
using QuanLyMuonTra.Modules.ThongBao.Contracts;

namespace QuanLyMuonTra.IntegrationTests.ThongBao;

public sealed class MauThongBaoTests
{
    private static readonly (string Loai, Dictionary<string, string> ThamSo)[] CacLoai =
    [
        (MaLoaiThongBao.NhacHanTra, new() { [ThamSoThongBao.MaPhieu] = "PM-1", [ThamSoThongBao.HanTra] = "14:30 20/11/2026" }),
        (MaLoaiThongBao.QuaHan, new() { [ThamSoThongBao.MaPhieu] = "PM-2", [ThamSoThongBao.HanTra] = "09:00 19/11/2026" }),
        (MaLoaiThongBao.ThietBiChoSanSang, new() { [ThamSoThongBao.TenLoai] = "Laptop", [ThamSoThongBao.HanGiuCho] = "15:00 20/11/2026" }),
        (MaLoaiThongBao.DuyetHoacTuChoi, new() { [ThamSoThongBao.MaYeuCau] = "YC-3", [ThamSoThongBao.KetQua] = "đã được duyệt", [ThamSoThongBao.ChiTiet] = "Thiết bị được giữ chỗ 30 phút." }),
        (MaLoaiThongBao.ViPhamPhiPhat, new() { [ThamSoThongBao.MaBienBan] = "BB-4", [ThamSoThongBao.TongTien] = "240.000 VNĐ" }),
        (MaLoaiThongBao.NhacBaoTri, new() { [ThamSoThongBao.MaThietBi] = "TB-5", [ThamSoThongBao.NgayDenHan] = "20/11/2026" })
    ];

    [Fact]
    public void MoiLoaiThongBao_CoMauDayDu_KhongConDauNgoacNhon()
    {
        foreach (var (loai, thamSo) in CacLoai)
        {
            var noiDung = MauThongBao.TaoNoiDungUngDung(loai, thamSo);

            Assert.False(string.IsNullOrWhiteSpace(MauThongBao.TieuDe(loai)));
            Assert.DoesNotContain("{", noiDung);
            Assert.DoesNotContain("}", noiDung);
            foreach (var giaTri in thamSo.Values)
            {
                Assert.Contains(giaTri, noiDung);
            }
        }
    }

    [Fact]
    public void ThieuThamSo_NemArgumentExceptionNeuTenThamSo()
    {
        var thamSo = new Dictionary<string, string> { [ThamSoThongBao.MaPhieu] = "PM-1" };

        var loi = Assert.Throws<ArgumentException>(() => MauThongBao.TaoNoiDungUngDung(MaLoaiThongBao.NhacHanTra, thamSo));

        Assert.Contains(ThamSoThongBao.HanTra, loi.Message);
    }

    [Fact]
    public void ThamSoRong_CoiNhuThieu()
    {
        var thamSo = new Dictionary<string, string> { [ThamSoThongBao.MaPhieu] = "PM-1", [ThamSoThongBao.HanTra] = "  " };

        Assert.Throws<ArgumentException>(() => MauThongBao.TaoNoiDungUngDung(MaLoaiThongBao.NhacHanTra, thamSo));
    }

    [Fact]
    public void LoaiKhongCoMau_NemArgumentException()
    {
        var thamSo = new Dictionary<string, string>();

        Assert.Throws<ArgumentException>(() => MauThongBao.TieuDe("NHAC_NHO_NV"));
        Assert.Throws<ArgumentException>(() => MauThongBao.TaoNoiDungUngDung("NHAC_NHO_NV", thamSo));
    }

    [Fact]
    public void EmailHtml_MaHoaKyTuDacBietCuaThamSo()
    {
        var thamSo = new Dictionary<string, string>
        {
            [ThamSoThongBao.MaPhieu] = "<b>PM-1</b>&",
            [ThamSoThongBao.HanTra] = "14:30 20/11/2026"
        };

        var html = MauThongBao.TaoNoiDungEmailHtml(MaLoaiThongBao.NhacHanTra, thamSo);

        Assert.StartsWith("<p>", html);
        Assert.EndsWith("</p>", html);
        Assert.Contains("&lt;b&gt;PM-1&lt;/b&gt;&amp;", html);
        Assert.DoesNotContain("<b>", html);
    }

    [Fact]
    public void ThamSoChuaDauNgoacNhon_KhongBiThayLanHai()
    {
        var thamSo = new Dictionary<string, string>
        {
            [ThamSoThongBao.MaPhieu] = "{hanTra}",
            [ThamSoThongBao.HanTra] = "14:30 20/11/2026"
        };

        var noiDung = MauThongBao.TaoNoiDungUngDung(MaLoaiThongBao.NhacHanTra, thamSo);

        Assert.Contains("Phiếu mượn {hanTra} sắp đến hạn", noiDung);
    }

    [Fact]
    public void EmailHtml_ThamSoCoXuongDong_ChuyenThanhTheBr()
    {
        var thamSo = new Dictionary<string, string>
        {
            [ThamSoThongBao.MaYeuCau] = "YC-1",
            [ThamSoThongBao.KetQua] = "tu choi",
            [ThamSoThongBao.ChiTiet] = "Dong 1\nDong 2\r\nDong 3"
        };

        var html = MauThongBao.TaoNoiDungEmailHtml(MaLoaiThongBao.DuyetHoacTuChoi, thamSo);

        Assert.Contains("Dong 1<br/>Dong 2<br/>Dong 3", html);
        Assert.DoesNotContain("\n", html);
        Assert.DoesNotContain("\r", html);
    }
}

