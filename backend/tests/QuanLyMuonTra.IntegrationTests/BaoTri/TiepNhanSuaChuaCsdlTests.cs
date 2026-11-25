using QuanLyMuonTra.IntegrationTests.MuonTra;
using QuanLyMuonTra.Modules.BaoTri.Application;
using QuanLyMuonTra.SharedKernel;

namespace QuanLyMuonTra.IntegrationTests.BaoTri;

public sealed class TiepNhanSuaChuaCsdlTests
{
    private const string TepFixture = "D14-N-du-lieu-thu-sua-chua.sql";
    private static readonly Guid MaNv = Guid.Parse(BaoTriTestSupport.MaNv);

    [CanKetNoiTestFact]
    public async Task UC11_TiepNhanSuaChua_ConBaoHanhCoNcc_LienKetPhieuGhiNvVaGiuNguyenBaoHanhNcc()
    {
        await using var mt = await BaoTriMoiTruong.TaoAsync(TepFixture);

        var ketQua = await mt.Lay<LapBaoTriHandler>().XuLyAsync(
            MaNv,
            new LapBaoTriRequest("D14-TB-01", "SUA_CHUA", "PM-261123-991401"),
            CancellationToken.None);

        Assert.Equal("SUA_CHUA", ketQua.LoaiThucHien);
        Assert.Equal("PM-261123-991401", ketQua.MaPhieuMuon);
        Assert.Equal(MaNv, ketQua.MaNVThucHien);
        Assert.Equal(1, await mt.DemAsync($"""
            SELECT count(*)::int AS "Value" FROM bao_tri.bao_tri_sua_chua
            WHERE ma_thiet_bi = {"D14-TB-01"} AND ket_qua IS NULL
              AND ma_phieu_muon = {"PM-261123-991401"} AND ma_nv_thuc_hien = {MaNv}
            """));
        Assert.Equal("DANG_BAO_TRI_SUA_CHUA", Assert.Single(await mt.LayChuoiAsync(
            $"SELECT tinh_trang AS \"Value\" FROM danh_muc.thiet_bi WHERE ma_thiet_bi = {"D14-TB-01"}")));
        Assert.Equal("2027-06-30", Assert.Single(await mt.LayChuoiAsync(
            $"SELECT to_char(thoi_han_bao_hanh, 'YYYY-MM-DD') AS \"Value\" FROM danh_muc.thiet_bi WHERE ma_thiet_bi = {"D14-TB-01"}")));
        Assert.Equal(1, await mt.DemAsync($"""
            SELECT count(*)::int AS "Value" FROM danh_muc.thiet_bi_nha_cung_cap
            WHERE ma_thiet_bi = {"D14-TB-01"}
            """));
    }

    [CanKetNoiTestFact]
    public async Task UC11_TiepNhanSuaChua_HetBaoHanhThieuPhuKien_VanTiepNhanDuoc()
    {
        await using var mt = await BaoTriMoiTruong.TaoAsync(TepFixture);

        var ketQua = await mt.Lay<LapBaoTriHandler>().XuLyAsync(
            MaNv,
            new LapBaoTriRequest("D14-TB-02", "SUA_CHUA", "PM-261123-991402"),
            CancellationToken.None);

        Assert.Equal("PM-261123-991402", ketQua.MaPhieuMuon);
        Assert.Equal(1, await mt.DemAsync($"""
            SELECT count(*)::int AS "Value" FROM bao_tri.bao_tri_sua_chua
            WHERE ma_thiet_bi = {"D14-TB-02"} AND ket_qua IS NULL
            """));
    }

    [CanKetNoiTestFact]
    public async Task UC11_TiepNhanSuaChua_PhongChuaCapQuyen_Nem403VaKhongTaoHoSo()
    {
        await using var mt = await BaoTriMoiTruong.TaoAsync(TepFixture);

        var loi = await Assert.ThrowsAsync<LoiNghiepVuException>(() => mt.Lay<LapBaoTriHandler>().XuLyAsync(
            MaNv,
            new LapBaoTriRequest("D14-TB-03", "SUA_CHUA", "PM-261123-991403"),
            CancellationToken.None));

        Assert.Equal("PHONG_CHUA_CAP_QUYEN", loi.MaLoi);
        Assert.Equal(403, loi.MaHttp);
        Assert.Equal(0, await mt.DemAsync($"""
            SELECT count(*)::int AS "Value" FROM bao_tri.bao_tri_sua_chua WHERE ma_thiet_bi = {"D14-TB-03"}
            """));
    }

    [CanKetNoiTestFact]
    public async Task UC11_TiepNhanSuaChua_ThietBiDangDuocMuon_Nem409VaKhongTaoHoSo()
    {
        await using var mt = await BaoTriMoiTruong.TaoAsync(TepFixture);

        var loi = await Assert.ThrowsAsync<LoiNghiepVuException>(() => mt.Lay<LapBaoTriHandler>().XuLyAsync(
            MaNv,
            new LapBaoTriRequest("D12-TB-05", "SUA_CHUA", "PM-261123-991201"),
            CancellationToken.None));

        Assert.Equal("TRANG_THAI_KHONG_HOP_LE", loi.MaLoi);
        Assert.Equal(409, loi.MaHttp);
        Assert.Equal("DANG_DUOC_MUON", Assert.Single(await mt.LayChuoiAsync(
            $"SELECT tinh_trang AS \"Value\" FROM danh_muc.thiet_bi WHERE ma_thiet_bi = {"D12-TB-05"}")));
        Assert.Equal(0, await mt.DemAsync($"""
            SELECT count(*)::int AS "Value" FROM bao_tri.bao_tri_sua_chua WHERE ma_thiet_bi = {"D12-TB-05"} AND ket_qua IS NULL
            """));
    }

    [CanKetNoiTestFact]
    public async Task UC11_TiepNhanSuaChua_PhieuChiCoKetQuaNguyenVen_Nem400VaKhongTaoHoSo()
    {
        await using var mt = await BaoTriMoiTruong.TaoAsync(TepFixture);

        var loi = await Assert.ThrowsAsync<LoiNghiepVuException>(() => mt.Lay<LapBaoTriHandler>().XuLyAsync(
            MaNv,
            new LapBaoTriRequest("D12-TB-08", "SUA_CHUA", "PM-261123-991404"),
            CancellationToken.None));

        Assert.Equal("DU_LIEU_KHONG_HOP_LE", loi.MaLoi);
        Assert.Equal(400, loi.MaHttp);
        Assert.Equal(0, await mt.DemAsync($"""
            SELECT count(*)::int AS "Value" FROM bao_tri.bao_tri_sua_chua WHERE ma_thiet_bi = {"D12-TB-08"} AND ket_qua IS NULL
            """));
    }

    [CanKetNoiTestFact]
    public async Task UC11_TiepNhanSuaChua_PhieuKhongTonTai_Nem400VaKhongTaoHoSo()
    {
        await using var mt = await BaoTriMoiTruong.TaoAsync(TepFixture);

        var loi = await Assert.ThrowsAsync<LoiNghiepVuException>(() => mt.Lay<LapBaoTriHandler>().XuLyAsync(
            MaNv,
            new LapBaoTriRequest("D12-TB-08", "SUA_CHUA", "PM-261123-999999"),
            CancellationToken.None));

        Assert.Equal("DU_LIEU_KHONG_HOP_LE", loi.MaLoi);
        Assert.Equal(0, await mt.DemAsync($"""
            SELECT count(*)::int AS "Value" FROM bao_tri.bao_tri_sua_chua WHERE ma_thiet_bi = {"D12-TB-08"} AND ket_qua IS NULL
            """));
    }

    [CanKetNoiTestFact]
    public async Task UC11_TiepNhanSuaChua_KhongGuiPhieu_VanTaoHoSoVaGhiNvThucHien()
    {
        await using var mt = await BaoTriMoiTruong.TaoAsync(TepFixture);

        var ketQua = await mt.Lay<LapBaoTriHandler>().XuLyAsync(
            MaNv,
            new LapBaoTriRequest("D12-TB-08", "SUA_CHUA"),
            CancellationToken.None);

        Assert.Null(ketQua.MaPhieuMuon);
        Assert.Equal(1, await mt.DemAsync($"""
            SELECT count(*)::int AS "Value" FROM bao_tri.bao_tri_sua_chua
            WHERE ma_thiet_bi = {"D12-TB-08"} AND ket_qua IS NULL
              AND ma_phieu_muon IS NULL AND ma_nv_thuc_hien = {MaNv}
            """));
    }

    [CanKetNoiTestFact]
    public async Task UC11_DaTiepNhanSuaChua_LapDinhKyCungThietBi_Nem409KhongTaoHoSoThuHai()
    {
        await using var mt = await BaoTriMoiTruong.TaoAsync(TepFixture);
        var handler = mt.Lay<LapBaoTriHandler>();
        await handler.XuLyAsync(
            MaNv,
            new LapBaoTriRequest("D14-TB-01", "SUA_CHUA", "PM-261123-991401"),
            CancellationToken.None);

        var loi = await Assert.ThrowsAsync<LoiNghiepVuException>(() => handler.XuLyAsync(
            MaNv,
            new LapBaoTriRequest("D14-TB-01", "BAO_TRI_DINH_KY"),
            CancellationToken.None));

        Assert.Equal("TRANG_THAI_KHONG_HOP_LE", loi.MaLoi);
        Assert.Equal(409, loi.MaHttp);
        Assert.Equal(1, await mt.DemAsync($"""
            SELECT count(*)::int AS "Value" FROM bao_tri.bao_tri_sua_chua
            WHERE ma_thiet_bi = {"D14-TB-01"} AND ket_qua IS NULL
            """));
    }
}
