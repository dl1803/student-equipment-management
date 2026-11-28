using QuanLyMuonTra.Modules.MuonTra.Application;
using QuanLyMuonTra.Modules.MuonTra.Contracts;
using QuanLyMuonTra.Modules.MuonTra.Infrastructure;
using QuanLyMuonTra.SharedKernel;
using static QuanLyMuonTra.IntegrationTests.MuonTra.BanGiaoTestSupport;
using static QuanLyMuonTra.IntegrationTests.MuonTra.GhiKetQuaKiemTraTestSupport;

namespace QuanLyMuonTra.IntegrationTests.MuonTra;

public class PortGhiNhanMatTests
{
    private static readonly Guid CtT2 = Guid.Parse("019c0000-0000-7000-8000-0000000d14f2");
    private static readonly Guid CtT3 = Guid.Parse("019c0000-0000-7000-8000-0000000d14f3");
    private static readonly Guid CtT4 = Guid.Parse("019c0000-0000-7000-8000-0000000d14f4");

    [CanKetNoiTestFact]
    public async Task UC08_Port_RuntimeDangKyBanThat()
    {
        await using var moi = await TaoMoiTruongHoanTacAsync(QuyenP1P2);

        Assert.IsType<PortGhiNhanMat>(moi.Lay<IPortGhiNhanMat>());
    }

    [CanKetNoiTestFact]
    public async Task UC08_Port_GhiMatMotChiTiet_NgayTraNullThietBiDaBaoMatPhieuGiuDangMuon()
    {
        await using var moi = await TaoMoiTruongHoanTacAsync(QuyenP1P2);

        var daGhi = await moi.Lay<IPortGhiNhanMat>().GhiNhanMatAsync(CtT1, Nv1, Ct);

        Assert.True(daGhi);
        var t1 = await DocChiTietAsync(moi.Db, T1);
        Assert.Equal("MAT", t1.KetQua);
        Assert.Null(t1.NgayGioTra);
        Assert.Equal(Nv1, t1.MaNv);
        Assert.Null(t1.MaKhung);
        Assert.Equal("DA_BAO_MAT", await DocTinhTrangThietBiAsync(moi.Db, T1));
        Assert.Equal("DANG_MUON", await DocTrangThaiPhieuAsync(moi.Db, PmA));
    }

    [CanKetNoiTestFact]
    public async Task UC08_Port_GhiMatMoiChiTiet_DongPhieuKhiMoiDongCoKetQua()
    {
        await using var moi = await TaoMoiTruongHoanTacAsync(QuyenP1P2);
        var port = moi.Lay<IPortGhiNhanMat>();

        await port.GhiNhanMatAsync(CtT1, Nv1, Ct);
        await port.GhiNhanMatAsync(CtT2, Nv1, Ct);
        await port.GhiNhanMatAsync(CtT3, Nv1, Ct);
        Assert.Equal("DANG_MUON", await DocTrangThaiPhieuAsync(moi.Db, PmA));
        await port.GhiNhanMatAsync(CtT4, Nv1, Ct);

        Assert.Equal("DA_TRA", await DocTrangThaiPhieuAsync(moi.Db, PmA));
    }

    [CanKetNoiTestFact]
    public async Task UC08_Port_ChiTietDaCoKetQua_Tra409VaKhongDoiKetQuaCu()
    {
        await using var moi = await TaoMoiTruongHoanTacAsync(QuyenP1P2);
        await moi.Lay<GhiKetQuaKiemTraHandler>().XuLyAsync(Nv1, PmA, [NguyenVen(T1)], Ct);

        var loi = await Assert.ThrowsAsync<LoiNghiepVuException>(
            () => moi.Lay<IPortGhiNhanMat>().GhiNhanMatAsync(CtT1, Nv1, Ct));

        Assert.Equal("TRANG_THAI_KHONG_HOP_LE", loi.MaLoi);
        Assert.Equal(409, loi.MaHttp);
        Assert.Equal("NGUYEN_VEN", (await DocChiTietAsync(moi.Db, T1)).KetQua);
        Assert.Equal("SAN_SANG", await DocTinhTrangThietBiAsync(moi.Db, T1));
    }

    [CanKetNoiTestFact]
    public async Task UC08_Port_KhongCoChiTiet_TraFalseVaKhongGhiGi()
    {
        await using var moi = await TaoMoiTruongHoanTacAsync(QuyenP1P2);

        var daGhi = await moi.Lay<IPortGhiNhanMat>().GhiNhanMatAsync(Guid.CreateVersion7(), Nv1, Ct);

        Assert.False(daGhi);
        Assert.Null((await DocChiTietAsync(moi.Db, T1)).KetQua);
    }

    [CanKetNoiTestFact]
    public async Task UC08_Port_KhongCoGiaoDich_NemInvalidOperation()
    {
        await using var moi = await MoiTruongTichHopMuonTra.TaoAsync(Nv1, BayGio, false);

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => moi.Lay<IPortGhiNhanMat>().GhiNhanMatAsync(Guid.CreateVersion7(), Nv1, Ct));
    }
}
