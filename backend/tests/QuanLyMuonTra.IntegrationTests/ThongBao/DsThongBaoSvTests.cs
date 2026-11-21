using QuanLyMuonTra.IntegrationTests.MuonTra;
using QuanLyMuonTra.Modules.ThongBao.Application;
using QuanLyMuonTra.SharedKernel;

namespace QuanLyMuonTra.IntegrationTests.ThongBao;

public sealed class DsThongBaoSvTests
{
    private static readonly DateTimeOffset BayGio = new(2026, 11, 21, 3, 0, 0, TimeSpan.Zero);

    [CanKetNoiTestFact]
    public async Task UC17_DsThongBao_MoiNhatTruocChiKenhUngDungVaChiCuaMinh()
    {
        await using var moi = await MoiTruongThongBao.TaoAsync();
        var maSv = await moi.TaoSinhVienAsync();
        var maSvKhac = await moi.TaoSinhVienAsync();
        var khoa = $"KT-D09K-{Guid.NewGuid():N}";
        for (var i = 0; i < 3; i++)
        {
            await moi.TaoOutbox(BayGio.AddHours(i)).GhiAsync(
                MoiTruongThongBao.TaoYeuCau(maSv, null, $"{khoa}-{i}", $"YC-{i}"),
                CancellationToken.None);
        }

        await moi.TaoOutbox(BayGio).GhiAsync(
            MoiTruongThongBao.TaoYeuCau(maSvKhac, null, $"{khoa}-khac", "YC-KHAC"),
            CancellationToken.None);
        var boXuLy = new DsThongBaoSvHandler(moi.Db);

        var trang1 = await boXuLy.XuLyAsync(maSv, 1, 2, CancellationToken.None);
        var trang2 = await boXuLy.XuLyAsync(maSv, 2, 2, CancellationToken.None);
        var trang3 = await boXuLy.XuLyAsync(maSv, 3, 2, CancellationToken.None);

        Assert.Equal(3, trang1.TongSo);
        Assert.Equal(2, trang1.DanhSach.Count);
        Assert.Contains("YC-2", trang1.DanhSach[0].NoiDung);
        Assert.Contains("YC-1", trang1.DanhSach[1].NoiDung);
        Assert.Equal(BayGio.AddHours(2), trang1.DanhSach[0].ThoiDiemGui);
        Assert.Equal("DUYET_HOAC_TU_CHOI", trang1.DanhSach[0].LoaiThongBao);
        Assert.Equal("PHIEU_YEU_CAU", trang1.DanhSach[0].DoiTuongLoai);
        Assert.Equal("YC-2", trang1.DanhSach[0].DoiTuongId);
        var dongCuoi = Assert.Single(trang2.DanhSach);
        Assert.Contains("YC-0", dongCuoi.NoiDung);
        Assert.Equal(3, trang3.TongSo);
        Assert.Empty(trang3.DanhSach);
        Assert.DoesNotContain(trang1.DanhSach.Concat(trang2.DanhSach), d => d.NoiDung.Contains("YC-KHAC"));
        Assert.DoesNotContain(trang1.DanhSach.Concat(trang2.DanhSach), d => d.NoiDung.StartsWith("<p>"));
    }

    [CanKetNoiTestFact]
    public async Task DsThongBao_ChuaCoThongBao_TraRong()
    {
        await using var moi = await MoiTruongThongBao.TaoAsync();
        var maSv = await moi.TaoSinhVienAsync();

        var ketQua = await new DsThongBaoSvHandler(moi.Db).XuLyAsync(maSv, 1, 20, CancellationToken.None);

        Assert.Equal(0, ketQua.TongSo);
        Assert.Empty(ketQua.DanhSach);
    }

    [CanKetNoiTestFact]
    public async Task DsThongBao_PhanTrangSai_Tra400DuLieuKhongHopLe()
    {
        await using var moi = await MoiTruongThongBao.TaoAsync();
        var boXuLy = new DsThongBaoSvHandler(moi.Db);

        var loiTrang = await Assert.ThrowsAsync<LoiNghiepVuException>(() => boXuLy.XuLyAsync("SV-X", 0, 20, CancellationToken.None));
        var loiKichThuoc = await Assert.ThrowsAsync<LoiNghiepVuException>(() => boXuLy.XuLyAsync("SV-X", 1, 101, CancellationToken.None));

        Assert.Equal("DU_LIEU_KHONG_HOP_LE", loiTrang.MaLoi);
        Assert.Equal("DU_LIEU_KHONG_HOP_LE", loiKichThuoc.MaLoi);
    }
}

