package com.nhom13.quanlymuontra;

import static com.nhom13.quanlymuontra.HoTroXml.cacPhanTu;
import static com.nhom13.quanlymuontra.HoTroXml.maId;
import static org.junit.Assert.assertEquals;

import java.util.HashMap;
import java.util.HashSet;
import java.util.Map;
import org.junit.Test;
import org.w3c.dom.Document;
import org.w3c.dom.Element;

/**
 * Bảo vệ cấu trúc điều hướng D09-L bằng cách đọc nav_graph.xml (JVM): điểm đến SCR-SV-03 và hai hành động. Không
 * chứng minh hành vi điều hướng hay nút Quay lại trên thiết bị.
 */
public class NavGraphD09Test {

    private static Document navGraph() {
        return TaiNguyen.docXml("src/main/res/navigation/nav_graph.xml");
    }

    private static Element diemDen(Document doc, String ma) {
        for (Element e : cacPhanTu(doc, "fragment")) {
            if (ma.equals(maId(e.getAttribute("android:id")))) {
                return e;
            }
        }
        throw new AssertionError("Không có điểm đến " + ma);
    }

    @Test
    public void FA53_ChiTietLoai_CoNhanVaHaiThamSoChuoiBatBuoc() {
        Document doc = navGraph();
        Element chiTiet = diemDen(doc, "chiTietLoaiThietBiFragment");
        Map<String, Element> thamSo = new HashMap<>();
        for (Element e : cacPhanTu(doc, "argument")) {
            if (e.getParentNode() == chiTiet) {
                thamSo.put(e.getAttribute("android:name"), e);
            }
        }

        assertEquals("@string/chi_tiet_thiet_bi_tieu_de", chiTiet.getAttribute("android:label"));
        assertEquals(
                "com.nhom13.quanlymuontra.ui.tracuu.ChiTietLoaiThietBiFragment",
                chiTiet.getAttribute("android:name"));
        assertEquals(new HashSet<>(java.util.Arrays.asList("maLoai", "maPhong")), thamSo.keySet());
        for (Element e : thamSo.values()) {
            assertEquals("string", e.getAttribute("app:argType"));
            assertEquals("", e.getAttribute("android:defaultValue"));
        }
    }

    @Test
    public void FA53_TraCuuMoChiTiet_VaChiTietMoHoSo_KhongXoaNganXep() {
        Document doc = navGraph();
        Map<String, String> hanhDong = new HashMap<>();
        for (Element e : cacPhanTu(doc, "action")) {
            String ma = maId(e.getAttribute("android:id"));
            if (ma.startsWith("action_traCuuThietBiFragment_to_")
                    || ma.startsWith("action_chiTietLoaiThietBiFragment_to_")) {
                hanhDong.put(ma, maId(e.getAttribute("app:destination")));
                assertEquals(ma, "", e.getAttribute("app:popUpTo"));
            }
        }

        Map<String, String> kyVong = new HashMap<>();
        kyVong.put(
                "action_traCuuThietBiFragment_to_chiTietLoaiThietBiFragment",
                "chiTietLoaiThietBiFragment");
        kyVong.put("action_chiTietLoaiThietBiFragment_to_hoSoMuonFragment", "hoSoMuonFragment");
        // D10-L: Đặt trước ở chi tiết loại mở nav graph con soanPhieuGraph (màn đầu SCR-SV-04).
        kyVong.put(
                "action_chiTietLoaiThietBiFragment_to_soanPhieuGraph",
                "soanPhieuGraph");
        assertEquals(kyVong, hanhDong);
    }
}
