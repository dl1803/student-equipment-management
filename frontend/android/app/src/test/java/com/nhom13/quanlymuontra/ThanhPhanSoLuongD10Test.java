package com.nhom13.quanlymuontra;

import static org.junit.Assert.assertEquals;
import static org.junit.Assert.assertFalse;
import static org.junit.Assert.assertTrue;

import org.junit.Test;

/**
 * Bảo vệ cấu trúc CMP-10 bằng cách đọc tài nguyên và mã nguồn (JVM). Không chứng minh hành vi chạm nút, vùng chạm
 * 48 dp hay TalkBack: cần thiết bị hoặc emulator (FC 9.4, TC-06), chưa chạy.
 */
public class ThanhPhanSoLuongD10Test {

    private static final String RES = "src/main/res";
    private static final String WIDGET = "src/main/java/com/nhom13/quanlymuontra/common/widget";

    @Test
    public void CMP10_Layout_CoHaiNutBieuTuongCoTenTruyCapVaGiaTri() {
        String xml = TaiNguyen.docChu(RES + "/layout/view_quantity_stepper.xml");

        for (String ma :
                new String[] {
                    "@+id/btn_giam",
                    "@+id/tv_gia_tri",
                    "@+id/btn_tang",
                    "@style/Widget.Ds.IconButton",
                    "@drawable/ic_remove",
                    "@drawable/ic_add",
                    "@string/so_luong_giam",
                    "@string/so_luong_tang",
                }) {
            assertTrue(ma, xml.contains(ma));
        }
    }

    @Test
    public void CMP10_Chu_TenTruyCapHaiNut() {
        String chu = TaiNguyen.docChu(RES + "/values/strings.xml");

        assertTrue(chu.contains("name=\"so_luong_giam\">Giảm số lượng<"));
        assertTrue(chu.contains("name=\"so_luong_tang\">Tăng số lượng<"));
    }

    @Test
    public void CMP10_Widget_KepGiaTriVaChiPhatSuKienKhiNguoiDungDoi() {
        String ma = TaiNguyen.docChu(WIDGET + "/QuantityStepperView.java");

        for (String phuongThuc :
                new String[] {"datGioiHan(", "datGiaTri(", "getGiaTri(", "datKhiDoi("}) {
            assertTrue(phuongThuc, ma.contains("public " + (phuongThuc.equals("getGiaTri(") ? "int " : "void ") + phuongThuc));
        }
        assertTrue(ma.contains("Math.max(toiThieu, Math.min(toiDa, x))"));
    }

    @Test
    public void FC66_MaWidgetSoLuong_KhongSnackbarToastLogDetailHayApi() {
        String ma = TaiNguyen.docChu(WIDGET + "/QuantityStepperView.java");

        for (String cam :
                new String[] {"Snackbar", "Toast", "System.out", "Log.", "getDetail(", "retrofit2", "Repository"}) {
            assertFalse("QuantityStepperView dùng " + cam, ma.contains(cam));
        }
        assertEquals(-1, ma.indexOf("parseColor"));
    }
}
