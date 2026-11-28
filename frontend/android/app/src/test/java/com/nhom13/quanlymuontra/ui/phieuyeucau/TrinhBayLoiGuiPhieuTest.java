package com.nhom13.quanlymuontra.ui.phieuyeucau;

import static org.junit.Assert.assertEquals;
import static org.junit.Assert.assertFalse;
import static org.junit.Assert.assertNull;
import static org.junit.Assert.assertTrue;

import com.nhom13.quanlymuontra.R;
import com.nhom13.quanlymuontra.data.model.ApiError;
import com.nhom13.quanlymuontra.ui.phieuyeucau.TrinhBayLoiGuiPhieu.PhanLoai;
import org.junit.Test;

public class TrinhBayLoiGuiPhieuTest {

    private static ApiError http(int status, String maLoi) {
        return ApiError.http(status, maLoi, null, null, null);
    }

    @Test
    public void UI55_MaLoiNghiepVu_PhanNhanhTheoMaLoi() {
        assertEquals(PhanLoai.DU_LIEU_KHONG_HOP_LE, TrinhBayLoiGuiPhieu.phanLoai(http(400, "DU_LIEU_KHONG_HOP_LE")));
        assertEquals(PhanLoai.SO_LUONG_THAY_DOI, TrinhBayLoiGuiPhieu.phanLoai(http(409, "SO_LUONG_KHA_DUNG_THAY_DOI")));
        assertEquals(PhanLoai.VUOT_GIOI_HAN_THIET_BI, TrinhBayLoiGuiPhieu.phanLoai(http(422, "VUOT_GIOI_HAN_THIET_BI")));
        assertEquals(PhanLoai.VUOT_THOI_GIAN_MUON_TOI_DA, TrinhBayLoiGuiPhieu.phanLoai(http(422, "VUOT_THOI_GIAN_MUON_TOI_DA")));
        assertEquals(PhanLoai.THE_HET_HAN, TrinhBayLoiGuiPhieu.phanLoai(http(422, "SV_THE_HET_HAN")));
        assertEquals(PhanLoai.KHONG_CON_THEO_HOC, TrinhBayLoiGuiPhieu.phanLoai(http(422, "SV_KHONG_CON_THEO_HOC")));
        assertEquals(PhanLoai.BI_KHOA_QUYEN_MUON, TrinhBayLoiGuiPhieu.phanLoai(http(422, "SV_BI_KHOA_QUYEN_MUON")));
    }

    @Test
    public void UI55_MaLoiKhongBiAnhHuongBoiMaTrangThai() {
        assertEquals(PhanLoai.VUOT_GIOI_HAN_THIET_BI, TrinhBayLoiGuiPhieu.phanLoai(http(500, "VUOT_GIOI_HAN_THIET_BI")));
    }

    @Test
    public void UI55_QuaNhieuYeuCauVaLoiHeThong() {
        assertEquals(PhanLoai.QUA_NHIEU_YEU_CAU, TrinhBayLoiGuiPhieu.phanLoai(http(429, null)));
        assertEquals(PhanLoai.QUA_NHIEU_YEU_CAU, TrinhBayLoiGuiPhieu.phanLoai(http(429, "QUA_NHIEU_YEU_CAU")));
        assertEquals(PhanLoai.HE_THONG, TrinhBayLoiGuiPhieu.phanLoai(http(500, null)));
        assertEquals(PhanLoai.HE_THONG, TrinhBayLoiGuiPhieu.phanLoai(http(403, "KHONG_DU_QUYEN")));
        assertEquals(PhanLoai.DU_LIEU_KHONG_HOP_LE, TrinhBayLoiGuiPhieu.phanLoai(http(400, null)));
    }

    @Test
    public void FA33_MangQuaThoiGianHuy_LaChuaXacDinh() {
        assertEquals(PhanLoai.CHUA_XAC_DINH, TrinhBayLoiGuiPhieu.phanLoai(ApiError.mang()));
        assertEquals(PhanLoai.CHUA_XAC_DINH, TrinhBayLoiGuiPhieu.phanLoai(ApiError.quaThoiGian()));
        assertEquals(PhanLoai.CHUA_XAC_DINH, TrinhBayLoiGuiPhieu.phanLoai(ApiError.huy()));
    }

    @Test
    public void UI55_QuayVeSua_ChiBonNhom() {
        assertTrue(TrinhBayLoiGuiPhieu.quayVeSua(PhanLoai.DU_LIEU_KHONG_HOP_LE));
        assertTrue(TrinhBayLoiGuiPhieu.quayVeSua(PhanLoai.VUOT_GIOI_HAN_THIET_BI));
        assertTrue(TrinhBayLoiGuiPhieu.quayVeSua(PhanLoai.VUOT_THOI_GIAN_MUON_TOI_DA));
        assertTrue(TrinhBayLoiGuiPhieu.quayVeSua(PhanLoai.SO_LUONG_THAY_DOI));
        assertFalse(TrinhBayLoiGuiPhieu.quayVeSua(PhanLoai.THE_HET_HAN));
        assertFalse(TrinhBayLoiGuiPhieu.quayVeSua(PhanLoai.KHONG_CON_THEO_HOC));
        assertFalse(TrinhBayLoiGuiPhieu.quayVeSua(PhanLoai.BI_KHOA_QUYEN_MUON));
        assertFalse(TrinhBayLoiGuiPhieu.quayVeSua(PhanLoai.CHUA_XAC_DINH));
        assertFalse(TrinhBayLoiGuiPhieu.quayVeSua(PhanLoai.QUA_NHIEU_YEU_CAU));
        assertFalse(TrinhBayLoiGuiPhieu.quayVeSua(PhanLoai.HE_THONG));
    }

    @Test
    public void UI55_ThongDiep_MoiNhomCoChuRieng() {
        assertEquals(R.string.gui_the_het_han, TrinhBayLoiGuiPhieu.thongDiep(PhanLoai.THE_HET_HAN));
        assertEquals(R.string.gui_khong_con_theo_hoc, TrinhBayLoiGuiPhieu.thongDiep(PhanLoai.KHONG_CON_THEO_HOC));
        assertEquals(R.string.gui_chua_xac_dinh_noi_dung, TrinhBayLoiGuiPhieu.thongDiep(PhanLoai.CHUA_XAC_DINH));
        assertEquals(R.string.qua_nhieu_yeu_cau, TrinhBayLoiGuiPhieu.thongDiep(PhanLoai.QUA_NHIEU_YEU_CAU));
        assertEquals(R.string.loi_he_thong, TrinhBayLoiGuiPhieu.thongDiep(PhanLoai.HE_THONG));
    }

    @Test
    public void UI55_ChiTietThoiGian_ChiKhiMayChuCoTra() {
        assertEquals(
                "Tối đa 4 giờ.",
                TrinhBayLoiGuiPhieu.chiTietThoiGian(
                        ApiError.http(422, "VUOT_THOI_GIAN_MUON_TOI_DA", "  Tối đa 4 giờ.  ", null, null)));
        assertNull(TrinhBayLoiGuiPhieu.chiTietThoiGian(http(422, "VUOT_THOI_GIAN_MUON_TOI_DA")));
        assertNull(
                TrinhBayLoiGuiPhieu.chiTietThoiGian(
                        ApiError.http(422, "VUOT_THOI_GIAN_MUON_TOI_DA", "   ", null, null)));
        assertNull(TrinhBayLoiGuiPhieu.chiTietThoiGian(null));
    }
}
