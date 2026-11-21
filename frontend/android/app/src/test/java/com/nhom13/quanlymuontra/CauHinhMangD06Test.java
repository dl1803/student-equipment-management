package com.nhom13.quanlymuontra;

import java.util.ArrayList;
import java.util.List;
import org.junit.Test;
import org.w3c.dom.Document;
import org.w3c.dom.Element;
import static com.nhom13.quanlymuontra.HoTroXml.cacPhanTu;
import static org.junit.Assert.assertEquals;
import static org.junit.Assert.assertFalse;
import static org.junit.Assert.assertTrue;

/**
 * Bảo vệ cấu hình mạng của D06-L bằng cách đọc tệp (JVM): bản debug chỉ được gọi http tới 10.0.2.2, bản release
 * không; địa chỉ API khai báo theo buildType (FA 5.5, 5.8, FC 6.8). Không chứng minh kết nối thật.
 */
public class CauHinhMangD06Test {


    @Test
    public void FC68_BanChinh_VanCamLuuLuongKhongMaHoaVaKhongCoCauHinhMang() {
        Element application =
                (Element) TaiNguyen.docXml("src/main/AndroidManifest.xml").getElementsByTagName("application").item(0);

        assertEquals("false", application.getAttribute("android:usesCleartextTraffic"));
        assertEquals("", application.getAttribute("android:networkSecurityConfig"));
    }


    @Test
    public void FC68_BanDebug_ChiChoPhepHttpToiDiaChiMayChuDevCuaEmulator() {
        Element application =
                (Element) TaiNguyen.docXml("src/debug/AndroidManifest.xml").getElementsByTagName("application").item(0);
        assertEquals("@xml/network_security_config_debug", application.getAttribute("android:networkSecurityConfig"));

        Document cauHinh = TaiNguyen.docXml("src/debug/res/xml/network_security_config_debug.xml");
        List<Element> mien = cacPhanTu(cauHinh, "domain");
        assertEquals(1, mien.size());
        assertEquals("10.0.2.2", mien.get(0).getTextContent().trim());
        assertEquals("false", mien.get(0).getAttribute("includeSubdomains"));
        List<Element> nhomMien = cacPhanTu(cauHinh, "domain-config");
        assertEquals(1, nhomMien.size());
        assertEquals("true", nhomMien.get(0).getAttribute("cleartextTrafficPermitted"));
        assertTrue("Không đặt base-config cho phép cleartext", cacPhanTu(cauHinh, "base-config").isEmpty());
    }


    @Test
    public void FC101_GitIgnoreAndroid_PhuDinhLaiThuMucSrcDebugBiMauDebugOGocBoQua() {
        // .gitignore gốc có mẫu [Dd]ebug/ bỏ qua mọi thư mục tên debug, trong đó có app/src/debug. Thiếu dòng phủ
        // định này, git bỏ qua cấu hình mạng debug mà không báo lỗi gì và máy khác không đăng nhập được khi chạy debug.
        String gitIgnore = TaiNguyen.docChu("../.gitignore");

        assertTrue(gitIgnore.contains("\n!/app/src/debug/\n"));
    }

    @Test
    public void FA58_DiaChiApi_KhaiBaoTheoBuildTypeKhongCoApiV1VaKetThucBangGach() {
        String gradle = TaiNguyen.docChu("build.gradle.kts");
        assertTrue(gradle.contains("buildConfig = true"));
        // Mỗi dòng có dạng: buildConfigField("String", "API_BASE_URL", "\"<địa chỉ>\"")
        List<String> diaChi = new ArrayList<>();
        for (String dong : gradle.split("\n")) {
            if (dong.contains("\"API_BASE_URL\"")) {
                diaChi.add(dong.substring(dong.indexOf("\\\"") + 2, dong.lastIndexOf("\\\"")));
            }
        }

        assertEquals("Khai báo đúng hai lần: debug rồi release", 2, diaChi.size());
        for (String d : diaChi) {
            assertTrue(d, d.endsWith("/"));
            assertFalse(d, d.contains("/api/v1"));
        }
        assertEquals("http://10.0.2.2:5283/", diaChi.get(0));
        assertTrue("Release chỉ dùng https: " + diaChi.get(1), diaChi.get(1).startsWith("https://"));
    }
}
