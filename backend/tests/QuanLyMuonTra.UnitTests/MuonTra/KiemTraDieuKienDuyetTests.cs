using QuanLyMuonTra.Modules.MuonTra.Application;
using QuanLyMuonTra.Modules.MuonTra.Domain;
using QuanLyMuonTra.Modules.MuonTra.Infrastructure;
using QuanLyMuonTra.Modules.QuyDinh.Contracts;

namespace QuanLyMuonTra.UnitTests.MuonTra;

public class KiemTraDieuKienDuyetTests
{
    private static readonly DateOnly HomNay = new(2026, 11, 18);
    private static readonly Guid LoaiX = Guid.Parse("019c0000-0000-7000-8000-0000000d7c01");
    private static readonly QuyDinhNhomDto QuyDinh = new("D07-NHOM", 3, 5, []);

    private static DongHoSoSinhVien HoSo(DateOnly hanThe)
    {
        return new DongHoSoSinhVien
        {
            MaNhom = "D07-NHOM",
            HanThe = hanThe,
            TrangThaiHoc = "DANG_HOC",
            TrangThaiQuyenMuon = "BINH_THUONG"
        };
    }

    private static IReadOnlyList<LoaiVaSoLuongDto> Dong(int soLuong)
    {
        return [new LoaiVaSoLuongDto(LoaiX, "Loại X", soLuong)];
    }

    private static IReadOnlyList<KhaDungTheoLoaiDto> KhaDung(int soLuongYeuCau, int soLuongKhaDung)
    {
        return [new KhaDungTheoLoaiDto(LoaiX, "Loại X", soLuongYeuCau, soLuongKhaDung)];
    }

    [Fact]
    public void UC06_DuDieuKien_KhongCoLyDo()
    {
        var dsLyDo = KiemTraDieuKienDuyet.TongHopLyDo(HoSo(HomNay), HomNay, QuyDinh, Dong(1), 2, 0, KhaDung(1, 1));

        Assert.Empty(dsLyDo);
    }

    [Fact]
    public void UC06_TheHetHan_ChiCoLyDoSvTheHetHan()
    {
        var dsLyDo = KiemTraDieuKienDuyet.TongHopLyDo(HoSo(HomNay.AddDays(-1)), HomNay, QuyDinh, Dong(1), 2, 0, KhaDung(1, 1));

        var lyDo = Assert.Single(dsLyDo);
        Assert.Equal(MaLoi.SvTheHetHan, lyDo.MaLoi);
    }

    [Fact]
    public void UC06_NhieuLoi_TraDungThuTuSvThoiGianGioiHanKhaDung()
    {
        var dsLyDo = KiemTraDieuKienDuyet.TongHopLyDo(HoSo(HomNay.AddDays(-1)), HomNay, QuyDinh, Dong(1), 8, 3, KhaDung(1, 0));

        Assert.Equal(
            new[] { MaLoi.SvTheHetHan, MaLoi.VuotThoiGianMuonToiDa, MaLoi.VuotGioiHanThietBi, MaLoi.SoLuongKhaDungThayDoi },
            dsLyDo.Select(d => d.MaLoi));
    }

    [Fact]
    public void UC06_KhaDungBangSoLuongYeuCau_VanDuocDuyet()
    {
        var dsLyDo = KiemTraDieuKienDuyet.TongHopLyDo(HoSo(HomNay), HomNay, QuyDinh, Dong(2), 2, 0, KhaDung(2, 2));

        Assert.Empty(dsLyDo);
    }

    [Fact]
    public void UC06_KhaDungThieu_LyDoNeuTenLoaiVaSoLuong()
    {
        var dsLyDo = KiemTraDieuKienDuyet.TongHopLyDo(HoSo(HomNay), HomNay, QuyDinh, Dong(2), 2, 0, KhaDung(2, 1));

        var lyDo = Assert.Single(dsLyDo);
        Assert.Equal(MaLoi.SoLuongKhaDungThayDoi, lyDo.MaLoi);
        Assert.Contains("Loại X", lyDo.ThongDiep);
        Assert.Contains("còn 1", lyDo.ThongDiep);
    }

    [Fact]
    public void BR30_GuiCungNgay_ChuaHetHan()
    {
        var gui = new DateTimeOffset(2026, 11, 18, 8, 0, 0, TimeSpan.FromHours(7));
        var thoiDiem = new DateTimeOffset(2026, 11, 18, 16, 0, 0, TimeSpan.FromHours(7));

        Assert.False(KiemTraDieuKienDuyet.DaHetHanTheoNgay(gui, thoiDiem));
    }

    [Fact]
    public void BR30_GuiHomTruoc_DaHetHan()
    {
        var gui = new DateTimeOffset(2026, 11, 17, 23, 59, 0, TimeSpan.FromHours(7));
        var thoiDiem = new DateTimeOffset(2026, 11, 18, 0, 1, 0, TimeSpan.FromHours(7));

        Assert.True(KiemTraDieuKienDuyet.DaHetHanTheoNgay(gui, thoiDiem));
    }

    [Fact]
    public async Task OutboxChuaNoi_GhiAsync_KhongNemLoi()
    {
        var outbox = new ThongBaoDuyetOutboxChuaNoi();
        var thongBao = new ThongBaoDuyetHoacTuChoi("YC-1", "SV-1", true, null, null);

        await outbox.GhiAsync(thongBao, CancellationToken.None);
    }
}
