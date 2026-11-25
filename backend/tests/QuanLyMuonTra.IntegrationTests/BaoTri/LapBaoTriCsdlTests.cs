using QuanLyMuonTra.IntegrationTests.MuonTra;
using QuanLyMuonTra.Modules.BaoTri.Application;
using QuanLyMuonTra.SharedKernel;
using QuanLyMuonTra.SharedKernel.Contracts;

namespace QuanLyMuonTra.IntegrationTests.BaoTri;

public sealed class LapBaoTriCsdlTests
{
    private static readonly Guid MaNv = Guid.Parse(BaoTriTestSupport.MaNv);
    private static readonly Guid PhongP1 = Guid.Parse(BaoTriTestSupport.MaPhongP1);
    private static readonly Guid LoaiL1 = Guid.Parse(BaoTriTestSupport.MaLoaiL1);

    [CanKetNoiTestFact]
    public async Task UC11_LapDinhKy_ThietBiSanSang_ChuyenTinhTrangGiamKhaDungVaGhiNhatKy()
    {
        await using var mt = await BaoTriMoiTruong.TaoAsync();
        var khaDung = mt.Lay<IAvailabilityService>();
        var truoc = await khaDung.TinhKhaDungAsync(PhongP1, LoaiL1, CancellationToken.None);
        Assert.True(truoc >= 1);

        var ketQua = await mt.Lay<LapBaoTriHandler>().XuLyAsync(
            MaNv,
            new LapBaoTriRequest("D12-TB-01", "BAO_TRI_DINH_KY"),
            CancellationToken.None);

        Assert.Equal("BAO_TRI_DINH_KY", ketQua.LoaiThucHien);
        Assert.Equal(MaNv, ketQua.MaNVThucHien);
        Assert.Equal("Thiết bị D12 01", ketQua.TenThietBi);
        Assert.Null(ketQua.MaPhieuMuon);
        Assert.Null(ketQua.KetQua);
        Assert.Null(ketQua.NgayHoanThanh);
        Assert.Equal(BaoTriTestSupport.BayGio, ketQua.NgayLap);
        Assert.Equal("DANG_BAO_TRI_SUA_CHUA", Assert.Single(await mt.LayChuoiAsync(
            $"SELECT tinh_trang AS \"Value\" FROM danh_muc.thiet_bi WHERE ma_thiet_bi = {"D12-TB-01"}")));
        Assert.Equal(truoc - 1, await khaDung.TinhKhaDungAsync(PhongP1, LoaiL1, CancellationToken.None));
        Assert.Equal(1, await mt.DemAsync($"""
            SELECT count(*)::int AS "Value" FROM bao_tri.bao_tri_sua_chua
            WHERE ma_thiet_bi = {"D12-TB-01"} AND ket_qua IS NULL
            """));
        Assert.Equal(1, await mt.DemAsync($"""
            SELECT count(*)::int AS "Value" FROM nhat_ky.nhat_ky_he_thong
            WHERE thao_tac = 'THEM' AND doi_tuong = 'bao_tri.bao_tri_sua_chua' AND doi_tuong_id = {ketQua.MaBaoTri.ToString()}
            """));
        Assert.Equal(1, await mt.DemAsync($"""
            SELECT count(*)::int AS "Value" FROM nhat_ky.nhat_ky_he_thong
            WHERE thao_tac = 'SUA' AND doi_tuong = 'danh_muc.thiet_bi' AND doi_tuong_id = {"D12-TB-01"}
            """));
    }

    [CanKetNoiTestFact]
    public async Task UC11_LapDinhKy_ThietBiDangDuocMuon_Nem409VaKhongGhiGi()
    {
        await using var mt = await BaoTriMoiTruong.TaoAsync();

        var loi = await Assert.ThrowsAsync<LoiNghiepVuException>(() => mt.Lay<LapBaoTriHandler>().XuLyAsync(
            MaNv,
            new LapBaoTriRequest("D12-TB-05", "BAO_TRI_DINH_KY"),
            CancellationToken.None));

        Assert.Equal("TRANG_THAI_KHONG_HOP_LE", loi.MaLoi);
        Assert.Equal(409, loi.MaHttp);
        Assert.Equal("DANG_DUOC_MUON", Assert.Single(await mt.LayChuoiAsync(
            $"SELECT tinh_trang AS \"Value\" FROM danh_muc.thiet_bi WHERE ma_thiet_bi = {"D12-TB-05"}")));
        Assert.Equal(0, await mt.DemAsync($"""
            SELECT count(*)::int AS "Value" FROM bao_tri.bao_tri_sua_chua
            WHERE ma_thiet_bi = {"D12-TB-05"} AND ket_qua IS NULL
            """));
    }

    [CanKetNoiTestFact]
    public async Task UC11_LapDinhKy_GoiLanHai_Nem409KhongTaoHoSoTrung()
    {
        await using var mt = await BaoTriMoiTruong.TaoAsync();
        var handler = mt.Lay<LapBaoTriHandler>();
        await handler.XuLyAsync(MaNv, new LapBaoTriRequest("D12-TB-01", "BAO_TRI_DINH_KY"), CancellationToken.None);

        var loi = await Assert.ThrowsAsync<LoiNghiepVuException>(
            () => handler.XuLyAsync(MaNv, new LapBaoTriRequest("D12-TB-01", "BAO_TRI_DINH_KY"), CancellationToken.None));

        Assert.Equal("TRANG_THAI_KHONG_HOP_LE", loi.MaLoi);
        Assert.Equal(1, await mt.DemAsync($"""
            SELECT count(*)::int AS "Value" FROM bao_tri.bao_tri_sua_chua
            WHERE ma_thiet_bi = {"D12-TB-01"} AND ket_qua IS NULL
            """));
    }

    [CanKetNoiTestFact]
    public async Task UC11_LapDinhKy_KhaDungBangKhong_Nem409SoLuongKhaDungThayDoi()
    {
        await using var mt = await BaoTriMoiTruong.TaoAsync();
        await mt.ThucThiAsync("""
            INSERT INTO tai_khoan.nhan_vien_phong (ma_nguoi_dung, ma_phong)
            VALUES ('019c0000-0000-7000-8000-0000000c1221', '019c0000-0000-7000-8000-0000000c1202');
            INSERT INTO muon_tra.dang_ky_cho (ma_dang_ky, ma_sv, ma_phong, ma_loai, so_thu_tu_cho, trang_thai, han_giu_cho)
            VALUES ('DK-261123-991301', 'D12-SV-A', '019c0000-0000-7000-8000-0000000c1202',
                    '019c0000-0000-7000-8000-0000000c1211', NULL, 'DA_THONG_BAO', TIMESTAMPTZ '2026-11-23 06:00:00+00');
            """);

        var loi = await Assert.ThrowsAsync<LoiNghiepVuException>(() => mt.Lay<LapBaoTriHandler>().XuLyAsync(
            MaNv,
            new LapBaoTriRequest("D12-TB-12", "BAO_TRI_DINH_KY"),
            CancellationToken.None));

        Assert.Equal("SO_LUONG_KHA_DUNG_THAY_DOI", loi.MaLoi);
        Assert.Equal(409, loi.MaHttp);
        Assert.Equal("SAN_SANG", Assert.Single(await mt.LayChuoiAsync(
            $"SELECT tinh_trang AS \"Value\" FROM danh_muc.thiet_bi WHERE ma_thiet_bi = {"D12-TB-12"}")));
        Assert.Equal(0, await mt.DemAsync($"""
            SELECT count(*)::int AS "Value" FROM bao_tri.bao_tri_sua_chua WHERE ma_thiet_bi = {"D12-TB-12"}
            """));
    }

    [CanKetNoiTestFact]
    public async Task UC11_LapBaoTri_PhongChuaCapQuyen_Nem403()
    {
        await using var mt = await BaoTriMoiTruong.TaoAsync();

        var loi = await Assert.ThrowsAsync<LoiNghiepVuException>(() => mt.Lay<LapBaoTriHandler>().XuLyAsync(
            MaNv,
            new LapBaoTriRequest("D12-TB-12", "BAO_TRI_DINH_KY"),
            CancellationToken.None));

        Assert.Equal("PHONG_CHUA_CAP_QUYEN", loi.MaLoi);
        Assert.Equal(403, loi.MaHttp);
    }

    [CanKetNoiTestFact]
    public async Task UC11_LapBaoTri_ThietBiKhongTonTai_Nem400()
    {
        await using var mt = await BaoTriMoiTruong.TaoAsync();

        var loi = await Assert.ThrowsAsync<LoiNghiepVuException>(() => mt.Lay<LapBaoTriHandler>().XuLyAsync(
            MaNv,
            new LapBaoTriRequest("KHONG-CO-D13", "BAO_TRI_DINH_KY"),
            CancellationToken.None));

        Assert.Equal(400, loi.MaHttp);
    }

    [CanKetNoiTestFact]
    public async Task UC11_TiepNhanSuaChua_ThietBiHuTuPhieu_LienKetPhieuVaKhongDoiTinhTrang()
    {
        await using var mt = await BaoTriMoiTruong.TaoAsync();

        var ketQua = await mt.Lay<LapBaoTriHandler>().XuLyAsync(
            MaNv,
            new LapBaoTriRequest("D12-TB-08", "SUA_CHUA", "PM-261123-991203"),
            CancellationToken.None);

        Assert.Equal("SUA_CHUA", ketQua.LoaiThucHien);
        Assert.Equal("PM-261123-991203", ketQua.MaPhieuMuon);
        Assert.Equal("DANG_BAO_TRI_SUA_CHUA", Assert.Single(await mt.LayChuoiAsync(
            $"SELECT tinh_trang AS \"Value\" FROM danh_muc.thiet_bi WHERE ma_thiet_bi = {"D12-TB-08"}")));
        Assert.Equal(1, await mt.DemAsync($"""
            SELECT count(*)::int AS "Value" FROM bao_tri.bao_tri_sua_chua
            WHERE ma_thiet_bi = {"D12-TB-08"} AND ket_qua IS NULL AND ma_phieu_muon = {"PM-261123-991203"}
            """));
        var dsCho = await mt.Lay<DsChoTiepNhanSuaChuaHandler>().XuLyAsync(MaNv, null, 1, 20, CancellationToken.None);
        Assert.DoesNotContain(dsCho.DanhSach, d => d.MaThietBi == "D12-TB-08");
    }

    [CanKetNoiTestFact]
    public async Task UC11_TiepNhanSuaChua_PhieuKhongChuaThietBi_Nem400VaKhongTaoHoSo()
    {
        await using var mt = await BaoTriMoiTruong.TaoAsync();

        var loi = await Assert.ThrowsAsync<LoiNghiepVuException>(() => mt.Lay<LapBaoTriHandler>().XuLyAsync(
            MaNv,
            new LapBaoTriRequest("D12-TB-08", "SUA_CHUA", "PM-261123-991201"),
            CancellationToken.None));

        Assert.Equal("DU_LIEU_KHONG_HOP_LE", loi.MaLoi);
        Assert.Equal(0, await mt.DemAsync($"""
            SELECT count(*)::int AS "Value" FROM bao_tri.bao_tri_sua_chua
            WHERE ma_thiet_bi = {"D12-TB-08"} AND ket_qua IS NULL
            """));
    }

    [CanKetNoiTestFact]
    public async Task UC11_TiepNhanSuaChua_ThietBiSanSang_Nem409()
    {
        await using var mt = await BaoTriMoiTruong.TaoAsync();

        var loi = await Assert.ThrowsAsync<LoiNghiepVuException>(() => mt.Lay<LapBaoTriHandler>().XuLyAsync(
            MaNv,
            new LapBaoTriRequest("D12-TB-01", "SUA_CHUA"),
            CancellationToken.None));

        Assert.Equal("TRANG_THAI_KHONG_HOP_LE", loi.MaLoi);
    }

    [CanKetNoiTestFact]
    public async Task UC11_TiepNhanSuaChua_DaCoHoSoMo_Nem409()
    {
        await using var mt = await BaoTriMoiTruong.TaoAsync();

        var loi = await Assert.ThrowsAsync<LoiNghiepVuException>(() => mt.Lay<LapBaoTriHandler>().XuLyAsync(
            MaNv,
            new LapBaoTriRequest("D12-TB-06", "SUA_CHUA"),
            CancellationToken.None));

        Assert.Equal("TRANG_THAI_KHONG_HOP_LE", loi.MaLoi);
        Assert.Equal(409, loi.MaHttp);
    }

    [CanKetNoiTestFact]
    public async Task UC11_TiepNhanSuaChua_GoiLanHai_Nem409KhongTaoHoSoTrung()
    {
        await using var mt = await BaoTriMoiTruong.TaoAsync();
        var handler = mt.Lay<LapBaoTriHandler>();
        await handler.XuLyAsync(MaNv, new LapBaoTriRequest("D12-TB-08", "SUA_CHUA", "PM-261123-991203"), CancellationToken.None);

        var loi = await Assert.ThrowsAsync<LoiNghiepVuException>(
            () => handler.XuLyAsync(MaNv, new LapBaoTriRequest("D12-TB-08", "SUA_CHUA", "PM-261123-991203"), CancellationToken.None));

        Assert.Equal("TRANG_THAI_KHONG_HOP_LE", loi.MaLoi);
        Assert.Equal(1, await mt.DemAsync($"""
            SELECT count(*)::int AS "Value" FROM bao_tri.bao_tri_sua_chua
            WHERE ma_thiet_bi = {"D12-TB-08"} AND ket_qua IS NULL
            """));
    }
}
