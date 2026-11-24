package com.nhom13.quanlymuontra;

import org.junit.Test;
import org.w3c.dom.Document;
import org.w3c.dom.Element;
import static com.nhom13.quanlymuontra.HoTroXml.cacPhanTu;
import static com.nhom13.quanlymuontra.HoTroXml.maId;
import static org.junit.Assert.assertEquals;
import static org.junit.Assert.assertFalse;
import static org.junit.Assert.assertNotNull;

/**
 * Bảo vệ cấu trúc layout SCR-CM-01 (UI-Screens 4.4, BR-08) bằng cách đọc tài nguyên (JVM).
 */
public class DangNhapD06Test {

    private static final String RES = "src/main/res";


    @Test
    public void UI44_DangNhap_KhongCoLienKetDangKyQuenHayDoiMatKhau() {
        String layout = TaiNguyen.docChu(RES + "/layout/fragment_dang_nhap_sinh_vien.xml").toLowerCase();
        String chu = TaiNguyen.docChu(RES + "/values/strings.xml").toLowerCase();

        assertFalse(layout.contains("dang_ky") || layout.contains("quen_mat_khau") || layout.contains("doi_mat_khau"));
        assertFalse(chu.contains("đăng ký tài khoản") || chu.contains("quên mật khẩu") || chu.contains("đổi mật khẩu"));
    }


    @Test
    public void UI44_DangNhap_NutVoHieuMacDinhVaMatKhauDuocChe() {
        Document tai = TaiNguyen.docXml(RES + "/layout/fragment_dang_nhap_sinh_vien.xml");
        Element nut = null;
        Element matKhau = null;
        for (Element e : cacPhanTu(tai, "com.google.android.material.button.MaterialButton")) {
            nut = e;
        }
        for (Element e : cacPhanTu(tai, "com.google.android.material.textfield.TextInputEditText")) {
            if (maId(e.getAttribute("android:id")).equals("et_mat_khau")) {
                matKhau = e;
            }
        }

        assertNotNull(nut);
        assertEquals("false", nut.getAttribute("android:enabled"));
        assertNotNull(matKhau);
        assertEquals("textPassword", matKhau.getAttribute("android:inputType"));
    }
}
