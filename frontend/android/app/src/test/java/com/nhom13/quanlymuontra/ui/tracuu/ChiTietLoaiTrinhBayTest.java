package com.nhom13.quanlymuontra.ui.tracuu;

import static org.junit.Assert.assertEquals;
import static org.junit.Assert.assertFalse;
import static org.junit.Assert.assertNull;
import static org.junit.Assert.assertTrue;

import com.nhom13.quanlymuontra.R;
import com.nhom13.quanlymuontra.data.model.ApiError;
import com.nhom13.quanlymuontra.generated.JSON;
import com.nhom13.quanlymuontra.generated.model.AnhRef;
import com.nhom13.quanlymuontra.ui.tracuu.ChiTietLoaiThietBiViewModel.HanhDong;
import java.time.OffsetDateTime;
import java.util.Arrays;
import java.util.Collections;
import java.util.List;
import org.junit.Test;

/** Quy tắc trình bày thuần của SCR-SV-03 (UI-Screens 5.3). */
public class ChiTietLoaiTrinhBayTest {

    @Test
    public void UISV03_SoLuongLonHonKhong_LaNutDatTruoc() {
        assertEquals(HanhDong.DAT_TRUOC, ChiTietLoaiTrinhBay.hanhDongChoSoLuong(1));
        assertEquals(HanhDong.DAT_TRUOC, ChiTietLoaiTrinhBay.hanhDongChoSoLuong(5));
    }

    @Test
    public void UISV03_SoLuongBangKhong_LaNutDangKyChoVaHetHang() {
        assertEquals(HanhDong.DANG_KY_CHO, ChiTietLoaiTrinhBay.hanhDongChoSoLuong(0));
        assertTrue(ChiTietLoaiTrinhBay.hetHang(0));
        assertFalse(ChiTietLoaiTrinhBay.hetHang(3));
    }

    @Test
    public void UISV03_SoLuongThieuHoacAm_KhongCoNut() {
        assertNull(ChiTietLoaiTrinhBay.hanhDongChoSoLuong(null));
        assertNull(ChiTietLoaiTrinhBay.hanhDongChoSoLuong(-1));
        assertFalse(ChiTietLoaiTrinhBay.hetHang(null));
    }

    @Test
    public void UISV03_NhanNut_NguyenVanTaiLieu() {
        assertEquals(
                R.string.chi_tiet_hanh_dong_dat_truoc, ChiTietLoaiTrinhBay.nhanNut(HanhDong.DAT_TRUOC));
        assertEquals(
                R.string.chi_tiet_hanh_dong_dang_ky_cho,
                ChiTietLoaiTrinhBay.nhanNut(HanhDong.DANG_KY_CHO));
    }

    @Test
    public void UISV03_HanhDong_MaThamSoKhopHoSoMuonTrinhBay() {
        assertEquals("DAT_TRUOC", HanhDong.DAT_TRUOC.getMaThamSo());
        assertEquals("DANG_KY_CHO", HanhDong.DANG_KY_CHO.getMaThamSo());
    }

    @Test
    public void UISV03_CoChu_AnKhiNullRongHoacChiKhoangTrang() {
        assertFalse(ChiTietLoaiTrinhBay.coChu(null));
        assertFalse(ChiTietLoaiTrinhBay.coChu(""));
        assertFalse(ChiTietLoaiTrinhBay.coChu("   "));
        assertTrue(ChiTietLoaiTrinhBay.coChu("Mô tả"));
    }

    @Test
    public void UISV03_NgayDuKien_GioVietNamHoacThongDiepChuaCoNgay() {
        OffsetDateTime ngay = OffsetDateTime.parse("2026-12-01T08:30:00Z");

        assertEquals("01/12/2026 15:30", ChiTietLoaiTrinhBay.ngayDuKien(ngay, "Chưa có ngày dự kiến"));
        assertEquals(
                "Chưa có ngày dự kiến", ChiTietLoaiTrinhBay.ngayDuKien(null, "Chưa có ngày dự kiến"));
    }

    @Test
    public void UISV03_DsUrlAnh_GiuThuTuVaBoPhanTuNullHoacUrlRong() {
        // Schema AnhRef bắt buộc maAnh và url nên phần tử "rỗng" được dựng bằng url rỗng, không bằng thiếu khóa.
        AnhRef[] dsAnh =
                new JSON()
                        .getGson()
                        .fromJson(
                                "[{\"maAnh\":\"33333333-3333-3333-3333-333333333331\","
                                        + "\"url\":\"https://anh.example.test/a.jpg\"},"
                                        + "{\"maAnh\":\"33333333-3333-3333-3333-333333333332\",\"url\":\"\"},"
                                        + "null,"
                                        + "{\"maAnh\":\"33333333-3333-3333-3333-333333333333\","
                                        + "\"url\":\"https://anh.example.test/c.jpg\"}]",
                                AnhRef[].class);

        List<String> ketQua = ChiTietLoaiTrinhBay.dsUrlAnh(Arrays.asList(dsAnh));

        assertEquals(
                Arrays.asList("https://anh.example.test/a.jpg", "https://anh.example.test/c.jpg"),
                ketQua);
        assertTrue(ChiTietLoaiTrinhBay.dsUrlAnh(null).isEmpty());
        assertTrue(ChiTietLoaiTrinhBay.dsUrlAnh(Collections.<AnhRef>emptyList()).isEmpty());
    }

    @Test
    public void UISV03_DsThongSo_BoDongRongGiuThuTu() {
        List<String> ketQua =
                ChiTietLoaiTrinhBay.dsThongSo(Arrays.asList("RAM 16 GB", "", null, "  ", "SSD 512 GB"));

        assertEquals(Arrays.asList("RAM 16 GB", "SSD 512 GB"), ketQua);
        assertTrue(ChiTietLoaiTrinhBay.dsThongSo(null).isEmpty());
    }

    @Test
    public void UI32_LaKhongTimThay_ChiKhiHttp404() {
        assertTrue(ChiTietLoaiTrinhBay.laKhongTimThay(ApiError.http(404, "KHONG_TIM_THAY", null, null, null)));
        assertFalse(ChiTietLoaiTrinhBay.laKhongTimThay(ApiError.http(500, null, null, null, null)));
        assertFalse(ChiTietLoaiTrinhBay.laKhongTimThay(ApiError.mang()));
    }

    @Test
    public void FA33_LoiQuyenMuon_ChonThongDiepVaMaTheoDoi() {
        assertEquals(R.string.loi_mang, ChiTietLoaiTrinhBay.thongDiepLoiQuyenMuon(ApiError.mang()));
        assertEquals(
                R.string.loi_mang, ChiTietLoaiTrinhBay.thongDiepLoiQuyenMuon(ApiError.quaThoiGian()));
        ApiError heThong = ApiError.http(500, null, null, "trace-1", null);
        assertEquals(R.string.loi_he_thong, ChiTietLoaiTrinhBay.thongDiepLoiQuyenMuon(heThong));
        assertEquals("trace-1", ChiTietLoaiTrinhBay.maTheoDoi(heThong));
        assertNull(ChiTietLoaiTrinhBay.maTheoDoi(ApiError.mang()));
        assertNull(ChiTietLoaiTrinhBay.maTheoDoi(ApiError.http(500, null, null, "", null)));
    }
}
