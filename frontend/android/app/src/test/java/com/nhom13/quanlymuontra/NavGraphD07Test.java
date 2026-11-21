package com.nhom13.quanlymuontra;

import static com.nhom13.quanlymuontra.HoTroXml.cacPhanTu;
import static com.nhom13.quanlymuontra.HoTroXml.maId;
import static org.junit.Assert.assertEquals;
import static org.junit.Assert.assertNotNull;

import java.util.HashMap;
import java.util.HashSet;
import java.util.Map;
import java.util.Set;
import org.junit.Test;
import org.w3c.dom.Document;
import org.w3c.dom.Element;

/**
 * Bảo vệ cấu trúc điều hướng D07-L bằng cách đọc nav_graph.xml (JVM): ba tham số của SCR-SV-14 và hai hành
 * động mở chồng của Trang chủ. Không chứng minh hành vi điều hướng hay nút Quay lại trên thiết bị.
 */
public class NavGraphD07Test {

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
    public void FA53_HoSoMuon_BaThamSoCheDoCoMacDinh() {
        Document doc = navGraph();
        Element hoSo = diemDen(doc, "hoSoMuonFragment");
        Map<String, Element> thamSo = new HashMap<>();
        for (Element e : cacPhanTu(doc, "argument")) {
            if (e.getParentNode() == hoSo) {
                thamSo.put(e.getAttribute("android:name"), e);
            }
        }

        assertEquals(
                new HashSet<>(java.util.Arrays.asList("cheDoBiChan", "hanhDongDaChon", "coNutQuayLai")),
                thamSo.keySet());
        assertEquals("boolean", thamSo.get("cheDoBiChan").getAttribute("app:argType"));
        assertEquals("false", thamSo.get("cheDoBiChan").getAttribute("android:defaultValue"));
        assertEquals("string", thamSo.get("hanhDongDaChon").getAttribute("app:argType"));
        assertEquals("true", thamSo.get("hanhDongDaChon").getAttribute("app:nullable"));
        assertEquals("@null", thamSo.get("hanhDongDaChon").getAttribute("android:defaultValue"));
        assertEquals("boolean", thamSo.get("coNutQuayLai").getAttribute("app:argType"));
        assertEquals("false", thamSo.get("coNutQuayLai").getAttribute("android:defaultValue"));
    }

    @Test
    public void FA53_TrangChu_ChiNoiDichDaCoVaKhongXoaNganXep() {
        Document doc = navGraph();
        Element trangChu = diemDen(doc, "trangChuFragment");
        Map<String, String> hanhDong = new HashMap<>();
        for (Element e : cacPhanTu(doc, "action")) {
            if (e.getParentNode() == trangChu) {
                hanhDong.put(maId(e.getAttribute("android:id")), maId(e.getAttribute("app:destination")));
                assertEquals("", e.getAttribute("app:popUpTo"));
            }
        }

        Map<String, String> kyVong = new HashMap<>();
        kyVong.put("action_trangChuFragment_to_traCuuThietBiFragment", "traCuuThietBiFragment");
        kyVong.put("action_trangChuFragment_to_hoSoMuonFragment", "hoSoMuonFragment");
        assertEquals(kyVong, hanhDong);
    }

    @Test
    public void FA53_TabGoc_VanLaSauDiemDenVaKhongThemDiemDenMoi() {
        Document doc = navGraph();
        Set<String> ma = new HashSet<>();
        for (Element e : cacPhanTu(doc, "fragment")) {
            assertNotNull(e.getAttribute("android:id"));
            ma.add(maId(e.getAttribute("android:id")));
        }

        assertEquals(6, ma.size());
    }
}
