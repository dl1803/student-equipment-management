package com.nhom13.quanlymuontra;

import java.util.ArrayList;
import java.util.Arrays;
import java.util.HashSet;
import java.util.List;
import java.util.Set;
import org.junit.Test;
import org.w3c.dom.Document;
import org.w3c.dom.Element;
import static com.nhom13.quanlymuontra.HoTroXml.cacPhanTu;
import static com.nhom13.quanlymuontra.HoTroXml.maId;
import static org.junit.Assert.assertEquals;
import static org.junit.Assert.assertTrue;

/**
 * Bảo vệ cấu trúc khung của D06-L bằng cách đọc tài nguyên và mã nguồn (JVM): thanh điều hướng dưới, khung chính,
 * MainActivity trong manifest. Chuyển tab giữ ngăn xếp riêng, ẩn hiện khung và giao diện tràn viền kiểm tay trên
 * emulator (xem tài liệu hướng dẫn D06-L, mục kiểm thử).
 */
public class KhungDieuHuongD06Test {

    private static final String RES = "src/main/res";


    @Test
    public void UI41_Menu_BonMucTrungMaBonTabGocTheoThuTu() {
        List<String> maMuc = new ArrayList<>();
        for (Element e : cacPhanTu(TaiNguyen.docXml(RES + "/menu/menu_bottom_nav.xml"), "item")) {
            maMuc.add(maId(e.getAttribute("android:id")));
            assertTrue(e.getAttribute("android:title").startsWith("@string/"));
        }

        assertEquals(NavGraphD06Test.TAB_GOC, maMuc);
    }


    @Test
    public void UI41_MainActivity_CungDanhSachTabGocVoiMenu() {
        String ma = TaiNguyen.docChu("src/main/java/com/nhom13/quanlymuontra/MainActivity.java");
        for (String tab : NavGraphD06Test.TAB_GOC) {
            assertTrue(tab, ma.contains("R.id." + tab));
        }
    }


    @Test
    public void UI41_KhungChinh_AnSanThanhTieuDeVaThanhDuoiDeKhongNhayKhiKhoiDong() {
        Document tai = TaiNguyen.docXml(RES + "/layout/activity_main.xml");
        Set<String> anSan = new HashSet<>();
        for (String the : new String[] {"com.google.android.material.appbar.MaterialToolbar",
                "com.google.android.material.bottomnavigation.BottomNavigationView", "View"}) {
            for (Element e : cacPhanTu(tai, the)) {
                if ("gone".equals(e.getAttribute("android:visibility"))) {
                    anSan.add(maId(e.getAttribute("android:id")));
                }
            }
        }

        assertEquals(
                new HashSet<>(Arrays.asList(
                        "tb_man_hinh", "bnv_dieu_huong", "v_duong_ke_duoi_tieu_de", "v_duong_ke_tren_dieu_huong")),
                anSan);
    }


    @Test
    public void FC3_KhungChinh_DungStyleDsVaNavHostVoiGraphGoc() {
        String xml = TaiNguyen.docChu(RES + "/layout/activity_main.xml");

        assertTrue(xml.contains("style=\"@style/Widget.Ds.TopBar\""));
        assertTrue(xml.contains("style=\"@style/Widget.Ds.BottomNav\""));
        assertTrue(xml.contains("android:name=\"androidx.navigation.fragment.NavHostFragment\""));
        assertTrue(xml.contains("app:navGraph=\"@navigation/nav_graph\""));
        assertTrue(xml.contains("app:defaultNavHost=\"true\""));
    }


    @Test
    public void FA55_MainActivity_LaDiemVaoDuyNhatVaDuocKhaiBaoTrongManifest() {
        Document tai = TaiNguyen.docXml("src/main/AndroidManifest.xml");
        List<Element> hoatDong = cacPhanTu(tai, "activity");

        assertEquals(1, hoatDong.size());
        assertEquals(".MainActivity", hoatDong.get(0).getAttribute("android:name"));
        assertEquals("true", hoatDong.get(0).getAttribute("android:exported"));
        assertEquals(1, cacPhanTu(tai, "action").size());
        assertEquals("android.intent.action.MAIN", cacPhanTu(tai, "action").get(0).getAttribute("android:name"));
        assertEquals(
                "android.intent.category.LAUNCHER", cacPhanTu(tai, "category").get(0).getAttribute("android:name"));
        Element application = (Element) tai.getElementsByTagName("application").item(0);
        assertEquals("@string/ung_dung_ten", application.getAttribute("android:label"));
    }
}
