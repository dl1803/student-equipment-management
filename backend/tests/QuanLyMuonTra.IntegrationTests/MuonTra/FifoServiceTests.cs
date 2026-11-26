using QuanLyMuonTra.Modules.MuonTra.Application;
using QuanLyMuonTra.Modules.MuonTra.Contracts;
using QuanLyMuonTra.Modules.QuyDinh.Contracts;
using QuanLyMuonTra.SharedKernel;
using QuanLyMuonTra.SharedKernel.Contracts;

namespace QuanLyMuonTra.IntegrationTests.MuonTra;

public class FifoServiceTests
{
    private static readonly CancellationToken Ct = CancellationToken.None;

    [CanKetNoiTestFact]
    public async Task C9_CapLuot_DungDauHangTheoThoiDiemVaMaVaDanhSoLaiHangCho()
    {
        await using var moi = await DuLieuD13.TaoMoiTruongAsync(true);
        await moi.NapDuLieuAsync(DuLieuD13.TenTep, DuLieuD13.QuyenNv1P1);
        var thamSo = await moi.Lay<IQuyDinhReader>().LayThamSoAsync(Ct);
        var hanMongDoi = DuLieuD13.BayGio.AddMinutes(thamSo.ThoiGianGiuChoPhut);

        var ketQua = await moi.Lay<IFifoService>().CapLuotChoAsync(DuLieuD13.P1, DuLieuD13.LoaiX, Ct);

        Assert.NotNull(ketQua);
        Assert.Equal(DuLieuD13.Dk001, ketQua.MaDangKy);
        Assert.Equal("D13-SV-B", ketQua.MaSv);
        Assert.Equal(DuLieuD13.P1, ketQua.MaPhong);
        Assert.Equal(DuLieuD13.LoaiX, ketQua.MaLoai);
        Assert.Null(ketQua.SoThuTu);
        Assert.Equal(hanMongDoi, ketQua.HanGiuCho);
        Assert.Equal("DA_THONG_BAO", ketQua.TrangThai);

        var daCap = await DuLieuD13.DocAsync(moi.Db, DuLieuD13.Dk001);
        Assert.Equal("DA_THONG_BAO", daCap.TrangThai);
        Assert.Null(daCap.SoThuTuCho);
        Assert.Equal(hanMongDoi, daCap.HanGiuCho);
        Assert.Equal(1, (await DuLieuD13.DocAsync(moi.Db, DuLieuD13.Dk002)).SoThuTuCho);
        Assert.Equal(2, (await DuLieuD13.DocAsync(moi.Db, DuLieuD13.Dk003)).SoThuTuCho);
        Assert.Equal(0, await moi.Lay<IAvailabilityService>().TinhKhaDungAsync(DuLieuD13.P1, DuLieuD13.LoaiX, Ct));

        var cacDong = await DuLieuD13.ThongBaoTheoKhoaAsync(moi.Db, $"GIU_CHO:{DuLieuD13.Dk001}");
        Assert.Equal(["EMAIL", "UNG_DUNG"], cacDong.Select(d => d.KenhGui).ToArray());
        Assert.All(cacDong, d => Assert.Equal("D13-SV-B", d.MaSvNhan));
        Assert.Equal("CHO_GUI", cacDong[0].TrangThaiGui);
        Assert.Equal("DA_GUI", cacDong[1].TrangThaiGui);
        Assert.Contains("D13 Loai X", cacDong[1].NoiDung);
        Assert.Contains("giữ chỗ đến", cacDong[1].NoiDung);
    }

    [CanKetNoiTestFact]
    public async Task C9_HaiPhong_HangChoDocLapVaMoiLanChiCapMotLuot()
    {
        await using var moi = await DuLieuD13.TaoMoiTruongAsync(true);
        await moi.NapDuLieuAsync(DuLieuD13.TenTep, DuLieuD13.QuyenNv1P1);
        var fifo = moi.Lay<IFifoService>();

        var p1 = await fifo.CapLuotChoAsync(DuLieuD13.P1, DuLieuD13.LoaiX, Ct);

        Assert.Equal(DuLieuD13.Dk001, p1!.MaDangKy);
        Assert.Equal("DANG_CHO", (await DuLieuD13.DocAsync(moi.Db, DuLieuD13.Dk004)).TrangThai);
        Assert.Empty(await DuLieuD13.ThongBaoTheoKhoaAsync(moi.Db, $"GIU_CHO:{DuLieuD13.Dk004}"));

        var p2 = await fifo.CapLuotChoAsync(DuLieuD13.P2, DuLieuD13.LoaiX, Ct);

        Assert.Equal(DuLieuD13.Dk004, p2!.MaDangKy);
        Assert.Equal("D13-SV-D", p2.MaSv);
        Assert.Equal("DANG_CHO", (await DuLieuD13.DocAsync(moi.Db, DuLieuD13.Dk002)).TrangThai);
        Assert.Equal(1, (await DuLieuD13.DocAsync(moi.Db, DuLieuD13.Dk002)).SoThuTuCho);
        Assert.Equal(2, (await DuLieuD13.DocAsync(moi.Db, DuLieuD13.Dk003)).SoThuTuCho);

        var lanHai = await fifo.CapLuotChoAsync(DuLieuD13.P1, DuLieuD13.LoaiX, Ct);

        Assert.Null(lanHai);
        Assert.Equal("DANG_CHO", (await DuLieuD13.DocAsync(moi.Db, DuLieuD13.Dk002)).TrangThai);
    }

    [CanKetNoiTestFact]
    public async Task C9_KhongCapLuot_KhiHetKhaDungHoacKhongCoNguoiCho()
    {
        await using var moi = await DuLieuD13.TaoMoiTruongAsync(true);
        await moi.NapDuLieuAsync(DuLieuD13.TenTep, DuLieuD13.QuyenNv1P1);
        var fifo = moi.Lay<IFifoService>();

        var hetThietBi = await fifo.CapLuotChoAsync(DuLieuD13.P1, DuLieuD13.LoaiY, Ct);
        var dangGiuChoChiemKhaDung = await fifo.CapLuotChoAsync(DuLieuD13.P1, DuLieuD13.LoaiW, Ct);
        var khongAiCho = await fifo.CapLuotChoAsync(DuLieuD13.P2, DuLieuD13.LoaiY, Ct);

        Assert.Null(hetThietBi);
        Assert.Null(dangGiuChoChiemKhaDung);
        Assert.Null(khongAiCho);
        Assert.Equal("DANG_CHO", (await DuLieuD13.DocAsync(moi.Db, DuLieuD13.Dk005)).TrangThai);
        Assert.Equal("DA_THONG_BAO", (await DuLieuD13.DocAsync(moi.Db, DuLieuD13.Dk006)).TrangThai);
        Assert.Equal("DANG_CHO", (await DuLieuD13.DocAsync(moi.Db, DuLieuD13.Dk007)).TrangThai);
        Assert.Equal(0, await DuLieuD13.DemThongBaoSvAsync(moi.Db));
    }

    [CanKetNoiTestFact]
    public async Task C9_KhongCoGiaoDich_NemInvalidOperationVaKhongGhiGi()
    {
        await using var moi = await DuLieuD13.TaoMoiTruongAsync(false);

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => moi.Lay<IFifoService>().CapLuotChoAsync(DuLieuD13.P1, DuLieuD13.LoaiX, Ct));
    }

    [CanKetNoiTestFact]
    public async Task C9_SauCapLuot_GiaoTrucTiepThietBiDuocGiuBiChan409()
    {
        await using var moi = await DuLieuD13.TaoMoiTruongAsync(true);
        await moi.NapDuLieuAsync(DuLieuD13.TenTep, DuLieuD13.QuyenNv1P1);
        await moi.Lay<IFifoService>().CapLuotChoAsync(DuLieuD13.P1, DuLieuD13.LoaiX, Ct);

        var loi = await Assert.ThrowsAsync<LoiNghiepVuException>(() => moi.Lay<GiaoThietBiHandler>().GiaoTrucTiepAsync(
            DuLieuD13.Nv1,
            "D13-SV-I",
            DuLieuD13.P1,
            [new LoaiVaSoLuongYeuCau(DuLieuD13.LoaiX, 1)],
            [new ThietBiGiaoYeuCau("D13-TB-01", "Tốt", null)],
            Ct));

        Assert.Equal("SO_LUONG_KHA_DUNG_THAY_DOI", loi.MaLoi);
        Assert.Equal(409, loi.MaHttp);
        Assert.Equal("SAN_SANG", await DuLieuD13.TinhTrangThietBiAsync(moi.Db, "D13-TB-01"));
        Assert.Equal(0, await DuLieuD13.DemAsync(moi.Db, $"""
            SELECT count(*)::integer AS "Value" FROM muon_tra.phieu_muon WHERE ma_sv = 'D13-SV-I'
            """));
    }
}
