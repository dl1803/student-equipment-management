using Microsoft.Extensions.DependencyInjection;
using QuanLyMuonTra.Modules.MuonTra.Application;
using QuanLyMuonTra.Modules.QuyDinh.Contracts;
using QuanLyMuonTra.Modules.ThongBao.Contracts;
using QuanLyMuonTra.SharedKernel.Contracts;

namespace QuanLyMuonTra.IntegrationTests.MuonTra;

public class RoiDangKyChoThatTests
{
    private static readonly CancellationToken Ct = CancellationToken.None;

    private sealed class OutboxLoi : IThongBaoOutbox
    {
        public Task GhiAsync(YeuCauGhiThongBao yeuCau, CancellationToken ct) =>
            Task.FromException(new InvalidOperationException("Giả lập lỗi ghi outbox."));
    }

    [CanKetNoiTestFact]
    public async Task UC03A1_RoiKhiDangGiuCho_NguoiKeTiepDuocGiuChoVaThongBaoNgay_KhongViPham()
    {
        await using var moi = await DuLieuD13.TaoMoiTruongAsync(true);
        await moi.NapDuLieuAsync(DuLieuD13.TenTep, DuLieuD13.QuyenNv1P1);
        var thamSo = await moi.Lay<IQuyDinhReader>().LayThamSoAsync(Ct);
        var viPhamTruoc = await DuLieuD13.DemViPhamAsync(moi.Db);

        var ketQua = await moi.Lay<RoiDangKyChoHandler>().XuLyAsync(DuLieuD13.MaSvF, DuLieuD13.Dk006, Ct);

        Assert.Equal("DA_HUY", ketQua.TrangThai);
        Assert.Null(ketQua.SoThuTuCho);
        var nguoiKe = await DuLieuD13.DocAsync(moi.Db, DuLieuD13.Dk007);
        Assert.Equal("DA_THONG_BAO", nguoiKe.TrangThai);
        Assert.Null(nguoiKe.SoThuTuCho);
        Assert.Equal(DuLieuD13.BayGio.AddMinutes(thamSo.ThoiGianGiuChoPhut), nguoiKe.HanGiuCho);
        Assert.Equal(0, await moi.Lay<IAvailabilityService>().TinhKhaDungAsync(DuLieuD13.P1, DuLieuD13.LoaiW, Ct));
        var cacDong = await DuLieuD13.ThongBaoTheoKhoaAsync(moi.Db, $"GIU_CHO:{DuLieuD13.Dk007}");
        Assert.Equal(["EMAIL", "UNG_DUNG"], cacDong.Select(d => d.KenhGui).ToArray());
        Assert.All(cacDong, d => Assert.Equal(DuLieuD13.MaSvG, d.MaSvNhan));
        Assert.Equal(viPhamTruoc, await DuLieuD13.DemViPhamAsync(moi.Db));
    }

    [CanKetNoiTestFact]
    public async Task UC03A1_RoiKhiDangGiuChoVaKhongConAiCho_TraKhaDungVaKhongGhiThongBao()
    {
        await using var moi = await DuLieuD13.TaoMoiTruongAsync(true);
        await moi.NapDuLieuAsync(DuLieuD13.TenTep, DuLieuD13.QuyenNv1P1);
        Assert.Equal(0, await moi.Lay<IAvailabilityService>().TinhKhaDungAsync(DuLieuD13.P2, DuLieuD13.LoaiW, Ct));

        var ketQua = await moi.Lay<RoiDangKyChoHandler>().XuLyAsync(DuLieuD13.MaSvH, DuLieuD13.Dk008, Ct);

        Assert.Equal("DA_HUY", ketQua.TrangThai);
        Assert.Equal(1, await moi.Lay<IAvailabilityService>().TinhKhaDungAsync(DuLieuD13.P2, DuLieuD13.LoaiW, Ct));
        Assert.Equal(0, await DuLieuD13.DemThongBaoSvAsync(moi.Db));
    }

    [CanKetNoiTestFact]
    public async Task UC03A2_RoiKhiDangCho_KhongCapLuotDuCoThietBiTrongVaDanhSoLai()
    {
        await using var moi = await DuLieuD13.TaoMoiTruongAsync(true);
        await moi.NapDuLieuAsync(DuLieuD13.TenTep, DuLieuD13.QuyenNv1P1);

        var ketQua = await moi.Lay<RoiDangKyChoHandler>().XuLyAsync("D13-SV-A", DuLieuD13.Dk002, Ct);

        Assert.Equal("DA_HUY", ketQua.TrangThai);
        Assert.Equal("DANG_CHO", (await DuLieuD13.DocAsync(moi.Db, DuLieuD13.Dk001)).TrangThai);
        Assert.Equal(1, (await DuLieuD13.DocAsync(moi.Db, DuLieuD13.Dk001)).SoThuTuCho);
        Assert.Equal(2, (await DuLieuD13.DocAsync(moi.Db, DuLieuD13.Dk003)).SoThuTuCho);
        Assert.Equal(0, await DuLieuD13.DemThongBaoSvAsync(moi.Db));
    }

    [CanKetNoiTestFact]
    public async Task UC03A1_RoiKhiGiuCho_OutboxLoi_HoanTacToanBo()
    {
        await using var moi = await DuLieuD13.TaoMoiTruongAsync(
            false,
            dichVu => dichVu.AddScoped<IThongBaoOutbox, OutboxLoi>());

        try
        {
            await moi.ThucThiSqlAsync(DuLieuD13.XoaTatCa);
            await moi.NapDuLieuAsync(DuLieuD13.TenTep, DuLieuD13.QuyenNv1P1);

            await Assert.ThrowsAsync<InvalidOperationException>(
                () => moi.Lay<RoiDangKyChoHandler>().XuLyAsync(DuLieuD13.MaSvF, DuLieuD13.Dk006, Ct));

            var nguoiRoi = await DuLieuD13.DocAsync(moi.Db, DuLieuD13.Dk006);
            Assert.Equal("DA_THONG_BAO", nguoiRoi.TrangThai);
            Assert.NotNull(nguoiRoi.HanGiuCho);
            var nguoiKe = await DuLieuD13.DocAsync(moi.Db, DuLieuD13.Dk007);
            Assert.Equal("DANG_CHO", nguoiKe.TrangThai);
            Assert.Equal(1, nguoiKe.SoThuTuCho);
            Assert.Equal(0, await DuLieuD13.DemThongBaoSvAsync(moi.Db));
        }
        finally
        {
            await moi.ThucThiSqlAsync(DuLieuD13.XoaTatCa);
        }
    }
}
