using QuanLyMuonTra.Modules.MuonTra.Application;
using QuanLyMuonTra.SharedKernel;
using static QuanLyMuonTra.IntegrationTests.MuonTra.BanGiaoTestSupport;
using static QuanLyMuonTra.IntegrationTests.MuonTra.GhiKetQuaKiemTraTestSupport;

namespace QuanLyMuonTra.IntegrationTests.MuonTra;

public class GhiKetQuaKiemTraGiaoDichTests
{
    private const string DkB = "DK-261126-915001";
    private const string DkA = "DK-261126-915002";

    private const string HaiNguoiCho = """
        INSERT INTO muon_tra.dang_ky_cho (ma_dang_ky, ma_sv, ma_phong, ma_loai, thoi_diem_dang_ky, so_thu_tu_cho, trang_thai)
        VALUES
            ('DK-261126-915001', 'D14-SV-B', '019c0000-0000-7000-8000-0000000d14b1', '019c0000-0000-7000-8000-0000000d14c2', TIMESTAMPTZ '2026-11-26 02:00:00+00', 1, 'DANG_CHO'),
            ('DK-261126-915002', 'D14-SV-A', '019c0000-0000-7000-8000-0000000d14b1', '019c0000-0000-7000-8000-0000000d14c2', TIMESTAMPTZ '2026-11-26 02:30:00+00', 2, 'DANG_CHO');
        """;

    [CanKetNoiTestFact]
    public async Task UC08_UC10_TraNguyenVenVaHuHongCungLoai_ChiCapLuotChoNguoiDauHangMotLan()
    {
        await using var moi = await TaoMoiTruongHoanTacAsync(QuyenP1P2);
        await moi.ThucThiSqlAsync(HaiNguoiCho);

        var phieu = await moi.Lay<GhiKetQuaKiemTraHandler>().XuLyAsync(Nv1, PmA, [NguyenVen(T3), HuHong(T4)], Ct);

        Assert.Equal("DANG_MUON", phieu.TrangThai);
        Assert.Equal("SAN_SANG", await DocTinhTrangThietBiAsync(moi.Db, T3));
        Assert.Equal("DANG_BAO_TRI_SUA_CHUA", await DocTinhTrangThietBiAsync(moi.Db, T4));
        var dauHang = await DuLieuD13.DocAsync(moi.Db, DkB);
        var keTiep = await DuLieuD13.DocAsync(moi.Db, DkA);
        Assert.Equal("DA_THONG_BAO", dauHang.TrangThai);
        Assert.NotNull(dauHang.HanGiuCho);
        Assert.True(dauHang.HanGiuCho > BayGio);
        Assert.Equal("DANG_CHO", keTiep.TrangThai);
        Assert.Equal(1, keTiep.SoThuTuCho);
        Assert.True(await DemAsync(moi.Db, $"""
            SELECT count(*)::integer AS "Value" FROM thong_bao.thong_bao WHERE ma_sv_nhan = 'D14-SV-B'
            """) > 0);
        Assert.Equal(0, await DemAsync(moi.Db, $"""
            SELECT count(*)::integer AS "Value" FROM thong_bao.thong_bao WHERE ma_sv_nhan = 'D14-SV-A'
            """));
    }

    [CanKetNoiTestFact]
    public async Task UC08_UC10_TraNguyenVenCaHaiThietBiCungLoai_CapLuotChoHaiNguoiTheoFifo()
    {
        await using var moi = await TaoMoiTruongHoanTacAsync(QuyenP1P2);
        await moi.ThucThiSqlAsync(HaiNguoiCho);

        await moi.Lay<GhiKetQuaKiemTraHandler>().XuLyAsync(Nv1, PmA, [NguyenVen(T3), NguyenVen(T4)], Ct);

        Assert.Equal("DA_THONG_BAO", (await DuLieuD13.DocAsync(moi.Db, DkB)).TrangThai);
        Assert.Equal("DA_THONG_BAO", (await DuLieuD13.DocAsync(moi.Db, DkA)).TrangThai);
    }

    [CanKetNoiTestFact]
    public async Task UC08_GhiLaiKetQuaDaXacNhan_Tra422VaGiuNguyenKetQuaCu()
    {
        await using var moi = await TaoMoiTruongHoanTacAsync(QuyenP1P2);
        var boXuLy = moi.Lay<GhiKetQuaKiemTraHandler>();
        await boXuLy.XuLyAsync(Nv1, PmA, [NguyenVen(T1)], Ct);
        var truoc = await DocChiTietAsync(moi.Db, T1);

        var loi = await Assert.ThrowsAsync<LoiNghiepVuException>(
            () => boXuLy.XuLyAsync(Nv1, PmA, [Mat(T1)], Ct));
        var sau = await DocChiTietAsync(moi.Db, T1);

        Assert.Equal("THIET_BI_KHONG_THUOC_PHIEU", loi.MaLoi);
        Assert.Equal(422, loi.MaHttp);
        Assert.Equal("NGUYEN_VEN", sau.KetQua);
        Assert.Equal(truoc.NgayGioTra, sau.NgayGioTra);
        Assert.Equal("SAN_SANG", await DocTinhTrangThietBiAsync(moi.Db, T1));
    }

    [CanKetNoiTestFact]
    public async Task UC08_LoiOThietBiThuHai_HoanTacToanBoLoVaGiaoDichKhongCommit()
    {
        await using var moi = await MoiTruongTichHopMuonTra.TaoAsync(Nv1, BayGio, false);

        try
        {
            await moi.ThucThiSqlAsync(XoaD14);
            await moi.NapDuLieuAsync(FileDuLieu, QuyenP1P2);
            IReadOnlyList<KetQuaKiemTraYeuCau> dsKetQua = [NguyenVen(T1), HuHong(T3, [Guid.CreateVersion7()])];

            var loi = await Assert.ThrowsAsync<LoiNghiepVuException>(
                () => moi.Lay<GhiKetQuaKiemTraHandler>().XuLyAsync(Nv1, PmA, dsKetQua, Ct));

            Assert.Equal("DU_LIEU_KHONG_HOP_LE", loi.MaLoi);
            Assert.Equal("DANG_DUOC_MUON", await DocTinhTrangThietBiAsync(moi.Db, T1));
            Assert.Equal("DANG_DUOC_MUON", await DocTinhTrangThietBiAsync(moi.Db, T3));
            Assert.Null((await DocChiTietAsync(moi.Db, T1)).KetQua);
            Assert.Null((await DocChiTietAsync(moi.Db, T3)).KetQua);
            Assert.Equal("DANG_MUON", await DocTrangThaiPhieuAsync(moi.Db, PmA));
            Assert.Equal(0, await DemAsync(moi.Db, $"""
                SELECT count(*)::integer AS "Value" FROM nhat_ky.nhat_ky_he_thong
                WHERE doi_tuong_id IN ('D14-TB-T1', 'D14-TB-T3')
                   OR (thao_tac = 'GHI_KET_QUA_KIEM_TRA' AND doi_tuong_id LIKE '019c0000-0000-7000-8000-0000000d14f%')
                """));
        }
        finally
        {
            await moi.ThucThiSqlAsync(XoaD14);
        }
    }
}
