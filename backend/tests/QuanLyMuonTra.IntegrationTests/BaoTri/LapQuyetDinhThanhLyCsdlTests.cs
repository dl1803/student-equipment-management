using QuanLyMuonTra.IntegrationTests.MuonTra;
using QuanLyMuonTra.Modules.BaoTri.Application;
using QuanLyMuonTra.SharedKernel;

namespace QuanLyMuonTra.IntegrationTests.BaoTri;

[Collection(ThanhLyDuLieu.TenBoSuuTap)]
public sealed class LapQuyetDinhThanhLyCsdlTests
{
    private static readonly CancellationToken Ct = CancellationToken.None;
    private static readonly DateOnly Ngay = new(2026, 11, 23);

    private static Task<QuyetDinhThanhLyDto> LapAsync(BaoTriMoiTruong mt, DateOnly ngay, params string[] dsMa) =>
        mt.Lay<LapQuyetDinhThanhLyHandler>().XuLyAsync(
            ThanhLyDuLieu.MaQtGuid,
            new LapQuyetDinhThanhLyRequest(ngay, dsMa),
            Ct);

    [CanKetNoiTestFact]
    public async Task UC15_LapNhieuThietBi_ChuyenDaThanhLy_GiuLichSuVaGhiNhatKy()
    {
        await using var mt = await BaoTriMoiTruong.TaoAsync(ThanhLyDuLieu.TenTep);
        var soBaoTriTruoc = await mt.DemAsync($"""
            SELECT count(*)::int AS "Value" FROM bao_tri.bao_tri_sua_chua WHERE ma_thiet_bi LIKE 'D16-TB-%'
            """);
        var soGiaTriTruoc = await mt.DemAsync($"""
            SELECT count(*)::int AS "Value" FROM danh_muc.lich_su_gia_tri WHERE ma_thiet_bi LIKE 'D16-TB-%'
            """);

        var dsMa = new[] { "D16-TB-01", "D16-TB-04", "D16-TB-05" };
        var nhatKyTruoc = new Dictionary<string, int>(StringComparer.Ordinal);
        foreach (var ma in dsMa)
        {
            nhatKyTruoc[ma] = await ThanhLyDuLieu.DemNhatKyAsync(mt, "danh_muc.thiet_bi", ma);
        }

        var ketQua = await LapAsync(mt, Ngay, dsMa);

        Assert.Matches(@"^QD-[0-9]{6}-[0-9]{6,}$", ketQua.MaQuyetDinh);
        Assert.Equal(ThanhLyDuLieu.MaQtGuid, ketQua.MaQTLap);
        Assert.Equal(Ngay, ketQua.NgayThanhLy);
        Assert.Equal(dsMa, ketQua.DsThietBi.Select(t => t.MaThietBi).ToArray());
        foreach (var ma in dsMa)
        {
            Assert.Equal("DA_THANH_LY", await ThanhLyDuLieu.TinhTrangAsync(mt, ma));
            Assert.Equal(nhatKyTruoc[ma] + 1, await ThanhLyDuLieu.DemNhatKyAsync(mt, "danh_muc.thiet_bi", ma));
        }

        Assert.Equal(3, await mt.DemAsync($"""
            SELECT count(*)::int AS "Value" FROM bao_tri.chi_tiet_thanh_ly WHERE ma_quyet_dinh = {ketQua.MaQuyetDinh}
            """));
        Assert.Equal(1, await mt.DemAsync($"""
            SELECT count(*)::int AS "Value" FROM bao_tri.quyet_dinh_thanh_ly
            WHERE ma_quyet_dinh = {ketQua.MaQuyetDinh} AND ngay_thanh_ly = {Ngay} AND ma_qt_lap = {ThanhLyDuLieu.MaQtGuid}
            """));
        Assert.Equal(1, await ThanhLyDuLieu.DemNhatKyAsync(mt, "bao_tri.quyet_dinh_thanh_ly", ketQua.MaQuyetDinh));
        Assert.Equal(soBaoTriTruoc, await mt.DemAsync($"""
            SELECT count(*)::int AS "Value" FROM bao_tri.bao_tri_sua_chua WHERE ma_thiet_bi LIKE 'D16-TB-%'
            """));
        Assert.Equal(soGiaTriTruoc, await mt.DemAsync($"""
            SELECT count(*)::int AS "Value" FROM danh_muc.lich_su_gia_tri WHERE ma_thiet_bi LIKE 'D16-TB-%'
            """));
    }

    [CanKetNoiTestFact]
    public async Task UC15_HaiDot_MoiQuyetDinhCoMaVaNgayRieng()
    {
        await using var mt = await BaoTriMoiTruong.TaoAsync(ThanhLyDuLieu.TenTep);

        var dot1 = await LapAsync(mt, Ngay, "D16-TB-04");
        var dot2 = await LapAsync(mt, Ngay.AddDays(1), "D16-TB-05");

        Assert.NotEqual(dot1.MaQuyetDinh, dot2.MaQuyetDinh);
        Assert.Equal(1, await mt.DemAsync($"""
            SELECT count(*)::int AS "Value" FROM bao_tri.quyet_dinh_thanh_ly
            WHERE ma_quyet_dinh = {dot1.MaQuyetDinh} AND ngay_thanh_ly = {Ngay}
            """));
        Assert.Equal(1, await mt.DemAsync($"""
            SELECT count(*)::int AS "Value" FROM bao_tri.quyet_dinh_thanh_ly
            WHERE ma_quyet_dinh = {dot2.MaQuyetDinh} AND ngay_thanh_ly = {Ngay.AddDays(1)}
            """));
    }

    [CanKetNoiTestFact]
    public async Task UC15_ThietBiRoiKhoiDanhSachCho_SauKhiCoQuyetDinh()
    {
        await using var mt = await BaoTriMoiTruong.TaoAsync(ThanhLyDuLieu.TenTep);
        var dsCho = mt.Lay<DsChoQuyetDinhThanhLyHandler>();

        await LapAsync(mt, Ngay, "D16-TB-01");

        var ketQua = await dsCho.XuLyAsync(1, 100, Ct);
        Assert.Equal(
            ["D16-TB-02"],
            ketQua.DanhSach.Where(d => d.MaThietBi.StartsWith("D16-TB-", StringComparison.Ordinal)).Select(d => d.MaThietBi).ToArray());
    }

    [CanKetNoiTestFact]
    public async Task UC15_ThietBiNgungSuDungDangSanSang_DuocThanhLyKhiKhaDungBangKhong()
    {
        await using var mt = await BaoTriMoiTruong.TaoAsync(ThanhLyDuLieu.TenTep);

        await LapAsync(mt, Ngay, "D16-TB-09");

        Assert.Equal("DA_THANH_LY", await ThanhLyDuLieu.TinhTrangAsync(mt, "D16-TB-09"));
    }

    [CanKetNoiTestFact]
    public async Task UC15_ThietBiDangMuonHoacDaBaoMatHoacDaThanhLy_Nem409VaKhongDoiTinhTrang()
    {
        await using var mt = await BaoTriMoiTruong.TaoAsync(ThanhLyDuLieu.TenTep);

        foreach (var (ma, tinhTrang) in new[]
        {
            ("D16-TB-06", "DANG_DUOC_MUON"),
            ("D16-TB-07", "DA_BAO_MAT"),
            ("D16-TB-08", "DA_THANH_LY"),
        })
        {
            var loi = await Assert.ThrowsAsync<LoiNghiepVuException>(() => LapAsync(mt, Ngay, ma));

            Assert.Equal("TRANG_THAI_KHONG_HOP_LE", loi.MaLoi);
            Assert.Equal(409, loi.MaHttp);
            Assert.Equal(tinhTrang, await ThanhLyDuLieu.TinhTrangAsync(mt, ma));
        }
    }

    [CanKetNoiTestFact]
    public async Task UC15_ThietBiDangThuocHanNgachGiuCho_Nem409KhaDungThayDoi()
    {
        await using var mt = await BaoTriMoiTruong.TaoAsync(ThanhLyDuLieu.TenTep);

        var loi = await Assert.ThrowsAsync<LoiNghiepVuException>(() => LapAsync(mt, Ngay, "D16-TB-03"));

        Assert.Equal("SO_LUONG_KHA_DUNG_THAY_DOI", loi.MaLoi);
        Assert.Equal(409, loi.MaHttp);
        Assert.Equal("SAN_SANG", await ThanhLyDuLieu.TinhTrangAsync(mt, "D16-TB-03"));
    }

    [CanKetNoiTestFact]
    public async Task UC15_ThietBiKhongTonTai_Nem400()
    {
        await using var mt = await BaoTriMoiTruong.TaoAsync(ThanhLyDuLieu.TenTep);

        var loi = await Assert.ThrowsAsync<LoiNghiepVuException>(() => LapAsync(mt, Ngay, "D16-TB-KHONG-CO"));

        Assert.Equal("DU_LIEU_KHONG_HOP_LE", loi.MaLoi);
        Assert.Equal(400, loi.MaHttp);
    }
}
