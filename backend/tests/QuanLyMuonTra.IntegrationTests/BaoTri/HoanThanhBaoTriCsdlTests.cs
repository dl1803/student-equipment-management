using System.Globalization;
using QuanLyMuonTra.IntegrationTests.MuonTra;
using QuanLyMuonTra.Modules.BaoTri.Application;
using QuanLyMuonTra.Modules.QuyDinh.Contracts;
using QuanLyMuonTra.SharedKernel;

namespace QuanLyMuonTra.IntegrationTests.BaoTri;

[Collection(HoanThanhBaoTriDuLieu.TenBoSuuTap)]
public sealed class HoanThanhBaoTriCsdlTests
{
    private static readonly Guid MaNv = Guid.Parse(HoanThanhBaoTriDuLieu.MaNv);
    private static readonly CancellationToken Ct = CancellationToken.None;
    private static readonly DateTimeOffset NgayDinhKy = new(2026, 10, 20, 10, 0, 0, TimeSpan.FromHours(7));
    private static readonly DateTimeOffset NgaySuaChua = new(2026, 11, 22, 10, 0, 0, TimeSpan.FromHours(7));

    [CanKetNoiTestFact]
    public async Task UC11_HoanThanhDinhKy_VeSanSang_KhongNguoiCho_DongHoSoDatSanSangVaGhiNhatKy()
    {
        await using var mt = await BaoTriMoiTruong.TaoAsync(HoanThanhBaoTriDuLieu.TenTep);

        var ketQua = await HoanThanhAsync(
            mt, HoanThanhBaoTriDuLieu.BaoTri1, "Vệ sinh và kiểm tra tổng thể", "DUA_VE_SAN_SANG", NgayDinhKy);

        Assert.Equal(HoanThanhBaoTriDuLieu.BaoTri1, ketQua.MaBaoTri);
        Assert.Equal("D15-TB-01", ketQua.MaThietBi);
        Assert.Equal("BAO_TRI_DINH_KY", ketQua.LoaiThucHien);
        Assert.Equal("Vệ sinh và kiểm tra tổng thể", ketQua.NoiDungThucHien);
        Assert.Equal("DUA_VE_SAN_SANG", ketQua.KetQua);
        Assert.Equal(NgayDinhKy.ToUniversalTime(), ketQua.NgayHoanThanh);
        Assert.Equal("SAN_SANG", await TinhTrangAsync(mt, "D15-TB-01"));
        Assert.Equal(1, await mt.DemAsync($"""
            SELECT count(*)::int AS "Value" FROM bao_tri.bao_tri_sua_chua
            WHERE ma_bao_tri = {HoanThanhBaoTriDuLieu.BaoTri1} AND ket_qua = 'DUA_VE_SAN_SANG'
              AND noi_dung_thuc_hien = 'Vệ sinh và kiểm tra tổng thể'
              AND ngay_hoan_thanh = {NgayDinhKy.ToUniversalTime()}
            """));
        Assert.Equal(1, await DemNhatKyAsync(mt, "bao_tri.bao_tri_sua_chua", HoanThanhBaoTriDuLieu.BaoTri1.ToString()));
        Assert.Equal(1, await DemNhatKyAsync(mt, "danh_muc.thiet_bi", "D15-TB-01"));
        Assert.Equal(0, await mt.DemAsync($"""
            SELECT count(*)::int AS "Value" FROM thong_bao.thong_bao WHERE ma_sv_nhan LIKE 'D15-SV-%'
            """));
    }

    [CanKetNoiTestFact]
    public async Task UC10_UC11_HoanThanhVeSanSang_CoNguoiCho_CapLuotDauHangTrongCungGiaoDichKhongCanCron()
    {
        await using var mt = await BaoTriMoiTruong.TaoAsync(HoanThanhBaoTriDuLieu.TenTep);
        var thamSo = await mt.Lay<IQuyDinhReader>().LayThamSoAsync(Ct);
        var hanMongDoi = BaoTriTestSupport.BayGio.AddMinutes(thamSo.ThoiGianGiuChoPhut);
        var khoaGiuCho = $"GIU_CHO:{HoanThanhBaoTriDuLieu.Dk1}";

        await HoanThanhAsync(mt, HoanThanhBaoTriDuLieu.BaoTri2, "Thay pin", "DUA_VE_SAN_SANG", NgaySuaChua);

        Assert.Equal("SAN_SANG", await TinhTrangAsync(mt, "D15-TB-02"));
        Assert.Equal(1, await mt.DemAsync($"""
            SELECT count(*)::int AS "Value" FROM muon_tra.dang_ky_cho
            WHERE ma_dang_ky = {HoanThanhBaoTriDuLieu.Dk1} AND trang_thai = 'DA_THONG_BAO'
              AND so_thu_tu_cho IS NULL AND han_giu_cho = {hanMongDoi}
            """));
        Assert.Equal(1, await mt.DemAsync($"""
            SELECT count(*)::int AS "Value" FROM muon_tra.dang_ky_cho
            WHERE ma_dang_ky = {HoanThanhBaoTriDuLieu.Dk2} AND trang_thai = 'DANG_CHO' AND so_thu_tu_cho = 1
            """));
        Assert.Equal(2, await mt.DemAsync($"""
            SELECT count(*)::int AS "Value" FROM thong_bao.thong_bao
            WHERE khoa_trung_lap = {khoaGiuCho} AND ma_sv_nhan = 'D15-SV-A'
            """));
    }

    [CanKetNoiTestFact]
    public async Task UC11_HoanThanh_DeXuatThanhLy_GiuTinhTrangBaoTriVaKhongCapLuot()
    {
        await using var mt = await BaoTriMoiTruong.TaoAsync(HoanThanhBaoTriDuLieu.TenTep);

        var ketQua = await HoanThanhAsync(
            mt, HoanThanhBaoTriDuLieu.BaoTri4, "Hư hỏng nặng, không sửa được", "DE_XUAT_THANH_LY", NgaySuaChua);

        Assert.Equal("DE_XUAT_THANH_LY", ketQua.KetQua);
        Assert.Equal("DANG_BAO_TRI_SUA_CHUA", await TinhTrangAsync(mt, "D15-TB-04"));
        Assert.Equal(1, await mt.DemAsync($"""
            SELECT count(*)::int AS "Value" FROM bao_tri.bao_tri_sua_chua
            WHERE ma_bao_tri = {HoanThanhBaoTriDuLieu.BaoTri4} AND ket_qua = 'DE_XUAT_THANH_LY'
              AND ngay_hoan_thanh = {NgaySuaChua.ToUniversalTime()}
            """));
        Assert.Equal(1, await mt.DemAsync($"""
            SELECT count(*)::int AS "Value" FROM muon_tra.dang_ky_cho
            WHERE ma_dang_ky = {HoanThanhBaoTriDuLieu.Dk3} AND trang_thai = 'DANG_CHO' AND so_thu_tu_cho = 1
            """));
        Assert.Equal(0, await mt.DemAsync($"""
            SELECT count(*)::int AS "Value" FROM thong_bao.thong_bao WHERE ma_sv_nhan LIKE 'D15-SV-%'
            """));
    }

    [CanKetNoiTestFact]
    public async Task UC11_HoanThanhLanHai_Nem409VaKhongGhiDe()
    {
        await using var mt = await BaoTriMoiTruong.TaoAsync(HoanThanhBaoTriDuLieu.TenTep);
        await HoanThanhAsync(mt, HoanThanhBaoTriDuLieu.BaoTri3, "Thay linh kiện", "DUA_VE_SAN_SANG", NgaySuaChua);

        var loi = await Assert.ThrowsAsync<LoiNghiepVuException>(() => HoanThanhAsync(
            mt, HoanThanhBaoTriDuLieu.BaoTri3, "Nội dung khác", "DE_XUAT_THANH_LY", NgaySuaChua.AddDays(1)));

        Assert.Equal("TRANG_THAI_KHONG_HOP_LE", loi.MaLoi);
        Assert.Equal(409, loi.MaHttp);
        Assert.Equal(1, await mt.DemAsync($"""
            SELECT count(*)::int AS "Value" FROM bao_tri.bao_tri_sua_chua
            WHERE ma_bao_tri = {HoanThanhBaoTriDuLieu.BaoTri3} AND ket_qua = 'DUA_VE_SAN_SANG'
              AND noi_dung_thuc_hien = 'Thay linh kiện' AND ngay_hoan_thanh = {NgaySuaChua.ToUniversalTime()}
            """));
        Assert.Equal("SAN_SANG", await TinhTrangAsync(mt, "D15-TB-03"));
        Assert.Equal(1, await DemNhatKyAsync(mt, "bao_tri.bao_tri_sua_chua", HoanThanhBaoTriDuLieu.BaoTri3.ToString()));
    }

    [CanKetNoiTestFact]
    public async Task UC11_ChuKyBaoTri_ChiLayLanDinhKyHoanThanh_SuaChuaKhongDoiMoc()
    {
        await using var mt = await BaoTriMoiTruong.TaoAsync(HoanThanhBaoTriDuLieu.TenTep);
        await HoanThanhAsync(mt, HoanThanhBaoTriDuLieu.BaoTri1, "Bảo trì định kỳ", "DUA_VE_SAN_SANG", NgayDinhKy);
        await HoanThanhAsync(mt, HoanThanhBaoTriDuLieu.BaoTri3, "Sửa chữa", "DUA_VE_SAN_SANG", NgaySuaChua);

        var danhSach = await mt.Lay<DsThietBiDenHanBaoTriHandler>().XuLyAsync(MaNv, null, 1, 100, Ct);

        var cacMoc = danhSach.DanhSach
            .Where(d => d.MaThietBi is "D15-TB-01" or "D15-TB-03")
            .OrderBy(d => d.MaThietBi, StringComparer.Ordinal)
            .Select(d => d.MaThietBi + "|" + d.NgayDenHan.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture))
            .ToArray();
        Assert.Equal(["D15-TB-01|2026-11-19", "D15-TB-03|2026-01-31"], cacMoc);
    }

    [CanKetNoiTestFact]
    public async Task UC11_HoanThanh_PhongChuaCapQuyen_Nem403VaKhongThayDoi()
    {
        await using var mt = await BaoTriMoiTruong.TaoAsync(HoanThanhBaoTriDuLieu.TenTep);

        var loi = await Assert.ThrowsAsync<LoiNghiepVuException>(() => HoanThanhAsync(
            mt, HoanThanhBaoTriDuLieu.BaoTri6, "Đã sửa", "DUA_VE_SAN_SANG", NgaySuaChua));

        Assert.Equal("PHONG_CHUA_CAP_QUYEN", loi.MaLoi);
        Assert.Equal(403, loi.MaHttp);
        Assert.Equal("DANG_BAO_TRI_SUA_CHUA", await TinhTrangAsync(mt, "D15-TB-06"));
        Assert.Equal(1, await DemHoSoMoAsync(mt, HoanThanhBaoTriDuLieu.BaoTri6));
    }

    [CanKetNoiTestFact]
    public async Task UC11_HoanThanh_HoSoKhongTonTai_Nem404()
    {
        await using var mt = await BaoTriMoiTruong.TaoAsync(HoanThanhBaoTriDuLieu.TenTep);

        var loi = await Assert.ThrowsAsync<LoiNghiepVuException>(() => HoanThanhAsync(
            mt, Guid.CreateVersion7(), "Đã sửa", "DUA_VE_SAN_SANG", NgaySuaChua));

        Assert.Equal("KHONG_TIM_THAY", loi.MaLoi);
        Assert.Equal(404, loi.MaHttp);
    }

    [CanKetNoiTestFact]
    public async Task UC11_HoanThanh_DauVaoSai_Nem400VaKhongThayDoi()
    {
        await using var mt = await BaoTriMoiTruong.TaoAsync(HoanThanhBaoTriDuLieu.TenTep);
        (string NoiDung, string KetQua, DateTimeOffset Ngay)[] cacCa =
        [
            ("", "DUA_VE_SAN_SANG", NgaySuaChua),
            ("   ", "DUA_VE_SAN_SANG", NgaySuaChua),
            ("Đã kiểm tra", "KHAC", NgaySuaChua),
            ("Đã kiểm tra", "DUA_VE_SAN_SANG", NgaySuaChua.AddDays(-5)),
        ];

        foreach (var (noiDung, ketQua, ngay) in cacCa)
        {
            var loi = await Assert.ThrowsAsync<LoiNghiepVuException>(() => HoanThanhAsync(
                mt, HoanThanhBaoTriDuLieu.BaoTri3, noiDung, ketQua, ngay));

            Assert.Equal("DU_LIEU_KHONG_HOP_LE", loi.MaLoi);
            Assert.Equal(400, loi.MaHttp);
        }

        Assert.Equal("DANG_BAO_TRI_SUA_CHUA", await TinhTrangAsync(mt, "D15-TB-03"));
        Assert.Equal(1, await DemHoSoMoAsync(mt, HoanThanhBaoTriDuLieu.BaoTri3));
    }

    private static Task<BaoTriSuaChuaDto> HoanThanhAsync(
        BaoTriMoiTruong mt,
        Guid maBaoTri,
        string noiDung,
        string ketQua,
        DateTimeOffset ngay) =>
        mt.Lay<HoanThanhBaoTriHandler>().XuLyAsync(
            MaNv, maBaoTri, new HoanThanhBaoTriRequest(noiDung, ketQua, ngay), Ct);

    private static async Task<string> TinhTrangAsync(BaoTriMoiTruong mt, string maThietBi) =>
        Assert.Single(await mt.LayChuoiAsync($"""
            SELECT tinh_trang AS "Value" FROM danh_muc.thiet_bi WHERE ma_thiet_bi = {maThietBi}
            """));

    private static Task<int> DemHoSoMoAsync(BaoTriMoiTruong mt, Guid maBaoTri) =>
        mt.DemAsync($"""
            SELECT count(*)::int AS "Value" FROM bao_tri.bao_tri_sua_chua
            WHERE ma_bao_tri = {maBaoTri} AND ket_qua IS NULL
            """);

    private static Task<int> DemNhatKyAsync(BaoTriMoiTruong mt, string doiTuong, string doiTuongId) =>
        mt.DemAsync($"""
            SELECT count(*)::int AS "Value" FROM nhat_ky.nhat_ky_he_thong
            WHERE doi_tuong = {doiTuong} AND doi_tuong_id = {doiTuongId} AND thao_tac = 'SUA'
            """);
}
