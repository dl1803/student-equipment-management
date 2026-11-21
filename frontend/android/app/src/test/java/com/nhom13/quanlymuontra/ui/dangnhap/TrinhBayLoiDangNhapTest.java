package com.nhom13.quanlymuontra.ui.dangnhap;

import static org.junit.Assert.assertEquals;
import static org.junit.Assert.assertFalse;
import static org.junit.Assert.assertNull;
import static org.junit.Assert.assertTrue;

import com.nhom13.quanlymuontra.R;
import com.nhom13.quanlymuontra.common.nhan.Tong;
import com.nhom13.quanlymuontra.data.model.ApiError;
import java.util.Collections;
import java.util.List;
import java.util.Map;
import org.junit.Test;

/** Bảng ánh xạ lỗi của SCR-CM-01 (UI-Screens 4.4, DS 9.4, FA 3.3 và 3.4). */
public class TrinhBayLoiDangNhapTest {

    private static final String MA_TRUY_VET = "00-4bf92f3577b34da6a3ce929d0e0e4736-00f067aa0ba902b7-00";

    private static ApiError http(int status, String maLoi) {
        return ApiError.http(status, maLoi, null, MA_TRUY_VET, null);
    }

    @Test
    public void FA33_YeuCauBiHuy_KhongHienThiGi() {
        assertNull(TrinhBayLoiDangNhap.cho(ApiError.huy()));
    }

    @Test
    public void FA33_MatMangVaQuaThoiGian_LoiKetNoiDangDanger() {
        for (ApiError loi : new ApiError[] {ApiError.mang(), ApiError.quaThoiGian()}) {
            TrinhBayLoiDangNhap.NoiDung nd = TrinhBayLoiDangNhap.cho(loi);

            assertEquals(TrinhBayLoiDangNhap.ViTri.KHOI_THONG_BAO, nd.viTri);
            assertEquals(Tong.DANGER, nd.tong);
            assertEquals(R.string.loi_ket_noi_may_chu, nd.thongDiep);
            assertNull(nd.traceId);
        }
    }

    @Test
    public void UFCM01_DangNhapSai_HienDuoiOMatKhau() {
        TrinhBayLoiDangNhap.NoiDung nd = TrinhBayLoiDangNhap.cho(http(401, "DANG_NHAP_SAI"));

        assertEquals(TrinhBayLoiDangNhap.ViTri.DUOI_O_MAT_KHAU, nd.viTri);
        assertEquals(R.string.dang_nhap_sai, nd.thongDiep);
        assertTrue(TrinhBayLoiDangNhap.laLoiDangNhapSai(http(401, "DANG_NHAP_SAI")));
    }

    @Test
    public void FA54_ChuaXacThuc_PhienHetHanDangInfo() {
        TrinhBayLoiDangNhap.NoiDung nd = TrinhBayLoiDangNhap.cho(http(401, "CHUA_XAC_THUC"));

        assertEquals(TrinhBayLoiDangNhap.ViTri.KHOI_THONG_BAO, nd.viTri);
        assertEquals(Tong.INFO, nd.tong);
        assertEquals(R.string.phien_het_han, nd.thongDiep);
        assertFalse(TrinhBayLoiDangNhap.laLoiDangNhapSai(http(401, "CHUA_XAC_THUC")));
    }

    @Test
    public void FA54_QuaNhieuYeuCau_ChoRoiThuLaiDangWarning() {
        for (ApiError loi : new ApiError[] {http(429, "QUA_NHIEU_YEU_CAU"), http(429, null)}) {
            TrinhBayLoiDangNhap.NoiDung nd = TrinhBayLoiDangNhap.cho(loi);

            assertEquals(Tong.WARNING, nd.tong);
            assertEquals(R.string.qua_nhieu_yeu_cau, nd.thongDiep);
        }
    }

    @Test
    public void FA34_LoiHeThong_DangDangerKemTraceId() {
        TrinhBayLoiDangNhap.NoiDung nd = TrinhBayLoiDangNhap.cho(http(500, "LOI_HE_THONG"));

        assertEquals(Tong.DANGER, nd.tong);
        assertEquals(R.string.loi_he_thong, nd.thongDiep);
        assertEquals(MA_TRUY_VET, nd.traceId);
    }

    @Test
    public void UFCM01_Loi400_HienDuoiOInputNhuDangNhapSai() {
        for (ApiError loi : new ApiError[] {http(400, "DU_LIEU_KHONG_HOP_LE"), http(400, null)}) {
            TrinhBayLoiDangNhap.NoiDung nd = TrinhBayLoiDangNhap.cho(loi);

            assertEquals(TrinhBayLoiDangNhap.ViTri.DUOI_O_MAT_KHAU, nd.viTri);
            assertEquals(R.string.dang_nhap_sai, nd.thongDiep);
            assertTrue(TrinhBayLoiDangNhap.laLoiDangNhapSai(loi));
        }
    }

    @Test
    public void FA311_Loi400TheoTruong_HienDuoiOTuongUng() {
        ApiError loiMssv = ApiError.http(400, "DU_LIEU_KHONG_HOP_LE", null, null,
                Collections.singletonMap("Mssv", Collections.singletonList("MSSV không hợp lệ")));
        TrinhBayLoiDangNhap.NoiDung ndMssv = TrinhBayLoiDangNhap.cho(loiMssv);
        assertEquals(TrinhBayLoiDangNhap.ViTri.DUOI_O_MSSV, ndMssv.viTri);
        assertEquals("MSSV không hợp lệ", ndMssv.thongDiepTuyChinh);
        assertTrue(TrinhBayLoiDangNhap.laLoiDangNhapSai(loiMssv));

        ApiError loiMatKhau = ApiError.http(400, "DU_LIEU_KHONG_HOP_LE", null, null,
                Collections.singletonMap("MatKhau", Collections.singletonList("Mật khẩu không hợp lệ")));
        TrinhBayLoiDangNhap.NoiDung ndMatKhau = TrinhBayLoiDangNhap.cho(loiMatKhau);
        assertEquals(TrinhBayLoiDangNhap.ViTri.DUOI_O_MAT_KHAU, ndMatKhau.viTri);
        assertEquals("Mật khẩu không hợp lệ", ndMatKhau.thongDiepTuyChinh);
        assertTrue(TrinhBayLoiDangNhap.laLoiDangNhapSai(loiMatKhau));
    }

    @Test
    public void FA34_MaKhongCoXuLyRieng_LaLoiHeThongChungKhongPhaiPhienHetHan() {
        for (ApiError loi : new ApiError[] {http(401, null), http(422, "MA_LA"), http(403, "TAI_KHOAN_BI_KHOA")}) {
            TrinhBayLoiDangNhap.NoiDung nd = TrinhBayLoiDangNhap.cho(loi);

            assertEquals(Tong.DANGER, nd.tong);
            assertEquals(R.string.loi_he_thong, nd.thongDiep);
            assertEquals(MA_TRUY_VET, nd.traceId);
        }
    }
}
