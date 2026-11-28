using QuanLyMuonTra.Modules.MuonTra.Application;
using QuanLyMuonTra.Modules.MuonTra.Domain;
using QuanLyMuonTra.SharedKernel;
using static QuanLyMuonTra.IntegrationTests.MuonTra.BanGiaoTestSupport;
using static QuanLyMuonTra.IntegrationTests.MuonTra.GhiKetQuaKiemTraTestSupport;

namespace QuanLyMuonTra.IntegrationTests.MuonTra;

public class GhiKetQuaKiemTraTests
{
    private const string PmKhongCo = "PM-261126-999999";

    [CanKetNoiTestFact]
    public async Task UC08_GhiKetQua_BonKetQuaTrongMotLo_GhiDungTungThietBiVaDongPhieu()
    {
        await using var moi = await TaoMoiTruongHoanTacAsync(QuyenP1P2);
        IReadOnlyList<KetQuaKiemTraYeuCau> dsKetQua = [NguyenVen(T1), HuHong(T2), ThieuPhuKien(T3), Mat(T4)];

        var phieu = await moi.Lay<GhiKetQuaKiemTraHandler>().XuLyAsync(Nv1, PmA, dsKetQua, Ct);

        Assert.Equal("DA_TRA", phieu.TrangThai);
        Assert.Equal(4, phieu.DsChiTiet.Count);

        var t1 = await DocChiTietAsync(moi.Db, T1);
        Assert.Equal("NGUYEN_VEN", t1.KetQua);
        Assert.Equal(BayGio, t1.NgayGioTra);
        Assert.Equal(Nv1, t1.MaNv);
        Assert.Null(t1.MaKhung);

        var t2 = await DocChiTietAsync(moi.Db, T2);
        Assert.Equal("HU_HONG", t2.KetQua);
        Assert.Equal("Vỡ màn hình", t2.TinhTrangKhiTra);
        Assert.Equal(Khung, t2.MaKhung);
        Assert.Equal(BayGio, t2.NgayGioTra);

        var t3 = await DocChiTietAsync(moi.Db, T3);
        Assert.Equal("THIEU_PHU_KIEN", t3.KetQua);
        Assert.Equal(Khung, t3.MaKhung);

        var t4 = await DocChiTietAsync(moi.Db, T4);
        Assert.Equal("MAT", t4.KetQua);
        Assert.Null(t4.NgayGioTra);
        Assert.Equal(Nv1, t4.MaNv);
        Assert.Null(t4.MaKhung);

        Assert.Equal("SAN_SANG", await DocTinhTrangThietBiAsync(moi.Db, T1));
        Assert.Equal("DANG_BAO_TRI_SUA_CHUA", await DocTinhTrangThietBiAsync(moi.Db, T2));
        Assert.Equal("DANG_BAO_TRI_SUA_CHUA", await DocTinhTrangThietBiAsync(moi.Db, T3));
        Assert.Equal("DA_BAO_MAT", await DocTinhTrangThietBiAsync(moi.Db, T4));
        Assert.Equal("DA_TRA", await DocTrangThaiPhieuAsync(moi.Db, PmA));

        Assert.Equal(4, await DemAsync(moi.Db, $"""
            SELECT count(*)::integer AS "Value" FROM nhat_ky.nhat_ky_he_thong
            WHERE thao_tac = 'GHI_KET_QUA_KIEM_TRA' AND doi_tuong = 'muon_tra.chi_tiet_phieu_muon'
              AND doi_tuong_id LIKE '019c0000-0000-7000-8000-0000000d14f%'
              AND ma_nguoi_dung = {Nv1}
            """));
    }

    [CanKetNoiTestFact]
    public async Task UC08_GhiKetQua_TraTungPhan_PhieuGiuDangMuonVaChiDoiThietBiDuocTra()
    {
        await using var moi = await TaoMoiTruongHoanTacAsync(QuyenP1P2);

        var phieu = await moi.Lay<GhiKetQuaKiemTraHandler>().XuLyAsync(Nv1, PmA, [NguyenVen(T1)], Ct);

        Assert.Equal("DANG_MUON", phieu.TrangThai);
        Assert.Equal("DANG_MUON", await DocTrangThaiPhieuAsync(moi.Db, PmA));
        Assert.Equal("NGUYEN_VEN", (await DocChiTietAsync(moi.Db, T1)).KetQua);
        Assert.Equal("SAN_SANG", await DocTinhTrangThietBiAsync(moi.Db, T1));
        foreach (var ma in new[] { T2, T3, T4 })
        {
            Assert.Null((await DocChiTietAsync(moi.Db, ma)).KetQua);
            Assert.Equal("DANG_DUOC_MUON", await DocTinhTrangThietBiAsync(moi.Db, ma));
        }
    }

    [CanKetNoiTestFact]
    public async Task UC08_GhiKetQua_PhieuHaiPhong_DongPhieuKhiMoiDongCoKetQua()
    {
        await using var moi = await TaoMoiTruongHoanTacAsync(QuyenP1P2);

        var phieu = await moi.Lay<GhiKetQuaKiemTraHandler>().XuLyAsync(Nv1, PmB, [NguyenVen(T5), NguyenVen(T6)], Ct);

        Assert.Equal("DA_TRA", phieu.TrangThai);
        Assert.Equal("SAN_SANG", await DocTinhTrangThietBiAsync(moi.Db, T5));
        Assert.Equal("SAN_SANG", await DocTinhTrangThietBiAsync(moi.Db, T6));
    }

    [CanKetNoiTestFact]
    public async Task UC08_GhiKetQua_NvChiCoQuyenMotPhong_Tra403VaKhongGhiGi()
    {
        await using var moi = await TaoMoiTruongHoanTacAsync(QuyenChiP1);
        var boXuLy = moi.Lay<GhiKetQuaKiemTraHandler>();

        var loi = await Assert.ThrowsAsync<LoiNghiepVuException>(
            () => boXuLy.XuLyAsync(Nv1, PmB, [NguyenVen(T5), NguyenVen(T6)], Ct));

        Assert.Equal("PHONG_CHUA_CAP_QUYEN", loi.MaLoi);
        Assert.Equal(403, loi.MaHttp);
        Assert.Null((await DocChiTietAsync(moi.Db, T5)).KetQua);
        Assert.Equal("DANG_DUOC_MUON", await DocTinhTrangThietBiAsync(moi.Db, T5));
    }

    [CanKetNoiTestFact]
    public async Task UC08_GhiKetQua_NvKhongCoQuyenPhongNaoCuaPhieu_Tra403()
    {
        await using var moi = await TaoMoiTruongHoanTacAsync(KhongQuyen);

        var loi = await Assert.ThrowsAsync<LoiNghiepVuException>(
            () => moi.Lay<GhiKetQuaKiemTraHandler>().XuLyAsync(Nv1, PmA, [NguyenVen(T1)], Ct));

        Assert.Equal("PHONG_CHUA_CAP_QUYEN", loi.MaLoi);
        Assert.Null((await DocChiTietAsync(moi.Db, T1)).KetQua);
    }

    [CanKetNoiTestFact]
    public async Task UC08_GhiKetQua_PhieuKhongTonTaiHoacMaSaiDinhDang_Tra404Va400()
    {
        await using var moi = await TaoMoiTruongHoanTacAsync(QuyenP1P2);
        var boXuLy = moi.Lay<GhiKetQuaKiemTraHandler>();

        var khongCo = await Assert.ThrowsAsync<LoiNghiepVuException>(
            () => boXuLy.XuLyAsync(Nv1, PmKhongCo, [NguyenVen(T1)], Ct));
        var saiMa = await Assert.ThrowsAsync<LoiNghiepVuException>(
            () => boXuLy.XuLyAsync(Nv1, "khong-phai-ma-phieu", [NguyenVen(T1)], Ct));

        Assert.Equal("KHONG_TIM_THAY", khongCo.MaLoi);
        Assert.Equal(404, khongCo.MaHttp);
        Assert.Equal("DU_LIEU_KHONG_HOP_LE", saiMa.MaLoi);
        Assert.Equal(400, saiMa.MaHttp);
    }

    [CanKetNoiTestFact]
    public async Task UC08_GhiKetQua_ThietBiKhongThuocPhieuHoacDaTra_Tra422VaKhongGhiGi()
    {
        await using var moi = await TaoMoiTruongHoanTacAsync(QuyenP1P2);
        var boXuLy = moi.Lay<GhiKetQuaKiemTraHandler>();

        var thietBiPhieuKhac = await Assert.ThrowsAsync<LoiNghiepVuException>(
            () => boXuLy.XuLyAsync(Nv1, PmA, [NguyenVen(T1), NguyenVen(T5)], Ct));
        var phieuDaTra = await Assert.ThrowsAsync<LoiNghiepVuException>(
            () => boXuLy.XuLyAsync(Nv1, PmDaTra, [NguyenVen(T7)], Ct));

        Assert.Equal("THIET_BI_KHONG_THUOC_PHIEU", thietBiPhieuKhac.MaLoi);
        Assert.Equal(422, thietBiPhieuKhac.MaHttp);
        Assert.Equal("THIET_BI_KHONG_THUOC_PHIEU", phieuDaTra.MaLoi);
        Assert.Null((await DocChiTietAsync(moi.Db, T1)).KetQua);
        Assert.Equal("DANG_DUOC_MUON", await DocTinhTrangThietBiAsync(moi.Db, T1));
    }

    [CanKetNoiTestFact]
    public async Task UC08_GhiKetQua_HuHongThieuMoTaHoacKhungPhat_Tra422VaKhongGhiGi()
    {
        await using var moi = await TaoMoiTruongHoanTacAsync(QuyenP1P2);
        var boXuLy = moi.Lay<GhiKetQuaKiemTraHandler>();
        var thieuKhung = new KetQuaKiemTraYeuCau(T1, KetQuaKiemTra.HuHong, "Vỡ", null, []);
        var thieuMoTa = new KetQuaKiemTraYeuCau(T1, KetQuaKiemTra.ThieuPhuKien, "  ", Khung, []);
        var khungRong = new KetQuaKiemTraYeuCau(T1, KetQuaKiemTra.HuHong, "Vỡ", Guid.Empty, []);

        foreach (var yeuCau in new[] { thieuKhung, thieuMoTa, khungRong })
        {
            var loi = await Assert.ThrowsAsync<LoiNghiepVuException>(() => boXuLy.XuLyAsync(Nv1, PmA, [yeuCau], Ct));

            Assert.Equal("THIEU_MO_TA_HOAC_KHUNG_PHAT", loi.MaLoi);
            Assert.Equal(422, loi.MaHttp);
        }

        Assert.Null((await DocChiTietAsync(moi.Db, T1)).KetQua);
        Assert.Equal("DANG_DUOC_MUON", await DocTinhTrangThietBiAsync(moi.Db, T1));
    }

    [CanKetNoiTestFact]
    public async Task UC08_GhiKetQua_KhungPhatKhongTonTaiHoacDaTat_Tra400VaKhongGhiGi()
    {
        await using var moi = await TaoMoiTruongHoanTacAsync(QuyenP1P2);
        var boXuLy = moi.Lay<GhiKetQuaKiemTraHandler>();
        var khungLa = new KetQuaKiemTraYeuCau(T1, KetQuaKiemTra.HuHong, "Vỡ", Guid.CreateVersion7(), []);

        var khongTonTai = await Assert.ThrowsAsync<LoiNghiepVuException>(() => boXuLy.XuLyAsync(Nv1, PmA, [khungLa], Ct));
        await moi.ThucThiSqlAsync("""
            UPDATE quy_dinh.khung_phat SET dang_su_dung = false
            WHERE ma_khung = '019c0000-0000-7000-8000-0000000d14d1';
            """);
        var daTat = await Assert.ThrowsAsync<LoiNghiepVuException>(() => boXuLy.XuLyAsync(Nv1, PmA, [HuHong(T1)], Ct));

        Assert.Equal("DU_LIEU_KHONG_HOP_LE", khongTonTai.MaLoi);
        Assert.Equal(400, khongTonTai.MaHttp);
        Assert.Equal("DU_LIEU_KHONG_HOP_LE", daTat.MaLoi);
        Assert.Null((await DocChiTietAsync(moi.Db, T1)).KetQua);
        Assert.Equal("DANG_DUOC_MUON", await DocTinhTrangThietBiAsync(moi.Db, T1));
    }

    [CanKetNoiTestFact]
    public async Task UC08_GhiKetQua_HuHongCoAnh_GanAnhKhiTraVaTraUrlKy()
    {
        await using var moi = await TaoMoiTruongHoanTacAsync(QuyenP1P2);
        var maAnh = await moi.Lay<IAnhService>().TaiLenAsync("KHI_TRA", new MemoryStream(AnhJpeg), Ct);

        var phieu = await moi.Lay<GhiKetQuaKiemTraHandler>().XuLyAsync(Nv1, PmA, [HuHong(T2, [maAnh])], Ct);

        var chiTietT2 = phieu.DsChiTiet.Single(c => c.MaThietBi == T2);
        Assert.Single(chiTietT2.DsAnhKhiTra);
        Assert.Equal(maAnh, chiTietT2.DsAnhKhiTra[0].MaAnh);
        Assert.Equal(1, await DemAsync(moi.Db, $"""
            SELECT count(*)::integer AS "Value" FROM luu_tru.anh_tai_len
            WHERE ma_anh = {maAnh} AND doi_tuong_loai = 'CHI_TIET_PHIEU_MUON'
              AND doi_tuong_id = '019c0000-0000-7000-8000-0000000d14f2' AND giai_doan = 'KHI_TRA'
            """));
    }
}
