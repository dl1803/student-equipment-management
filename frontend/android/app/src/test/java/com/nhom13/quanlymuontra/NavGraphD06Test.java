package com.nhom13.quanlymuontra;

import java.util.Arrays;
import java.util.HashSet;
import java.util.List;
import java.util.Set;
import org.junit.Test;
import org.w3c.dom.Document;
import org.w3c.dom.Element;
import org.w3c.dom.Node;
import static com.nhom13.quanlymuontra.HoTroXml.cacPhanTu;
import static com.nhom13.quanlymuontra.HoTroXml.maId;
import static org.junit.Assert.assertEquals;
import static org.junit.Assert.assertTrue;

/**
 * Bảo vệ cấu trúc nav graph của D06-L bằng cách đọc tài nguyên và mã nguồn (JVM): điểm bắt đầu, mã điểm đến và
 * hành động theo FC 3.3, quy tắc xóa ngăn xếp của FA 5.3. Không chứng minh hành vi điều hướng trên thiết bị.
 */
public class NavGraphD06Test {

    private static final String RES = "src/main/res";
    static final List<String> TAB_GOC =
            Arrays.asList("trangChuFragment", "traCuuThietBiFragment", "thongBaoFragment", "hoSoMuonFragment");

    private static String chuThuongDau(String ten) {
        return Character.toLowerCase(ten.charAt(0)) + ten.substring(1);
    }

    private static Document navGraph() {
        return TaiNguyen.docXml(RES + "/navigation/nav_graph.xml");
    }


    @Test
    public void FA53_NavGraph_DiemBatDauLaKhoiDongFragment() {
        Element goc = navGraph().getDocumentElement();

        assertEquals("@+id/nav_graph", goc.getAttribute("android:id"));
        assertEquals("@id/khoiDongFragment", goc.getAttribute("app:startDestination"));
    }


    @Test
    public void FC33_NavGraph_MaDiemDenTrungTenLopVaLopTonTai() {
        List<Element> diemDen = cacPhanTu(navGraph(), "fragment");

        // D09-L: thêm chiTietLoaiThietBiFragment (SCR-SV-03).
        assertEquals(7, diemDen.size());
        for (Element e : diemDen) {
            String ten = e.getAttribute("android:name");
            String tenLop = ten.substring(ten.lastIndexOf('.') + 1);
            assertEquals(chuThuongDau(tenLop), maId(e.getAttribute("android:id")));
            String tepNguon = "src/main/java/" + ten.replace('.', '/') + ".java";
            assertTrue(tepNguon, TaiNguyen.tep(tepNguon).isFile());
            String layout = e.getAttribute("tools:layout").replace("@layout/", "");
            assertTrue(layout, TaiNguyen.tep(RES + "/layout/" + layout + ".xml").isFile());
        }
    }


    @Test
    public void FC33_NavGraph_MaHanhDongTheoNguonVaDich() {
        Document tai = navGraph();
        for (Element hanhDong : cacPhanTu(tai, "action")) {
            String maHanhDong = maId(hanhDong.getAttribute("android:id"));
            Node cha = hanhDong.getParentNode();
            String nguon =
                    cha.getNodeName().equals("fragment")
                            ? maId(((Element) cha).getAttribute("android:id"))
                            : "global";
            String dich = maId(hanhDong.getAttribute("app:destination"));
            assertEquals("action_" + nguon + "_to_" + dich, maHanhDong);
        }
    }


    @Test
    public void FA53_NavGraph_HanhDongKhoiDongVaDangNhapXoaChinhNoKhoiNganXep() {
        Document tai = navGraph();
        Set<String> daThay = new HashSet<>();
        for (Element hanhDong : cacPhanTu(tai, "action")) {
            String ma = maId(hanhDong.getAttribute("android:id"));
            // D07-L: hành động mở chồng của Trang chủ không xóa ngăn xếp; cấu trúc của chúng do NavGraphD07Test kiểm.
            if (ma.startsWith("action_trangChuFragment_to_")) {
                continue;
            }
            // D09-L: hành động mở chồng của tra cứu và của chi tiết không xóa ngăn xếp; cấu trúc do NavGraphD09Test kiểm.
            if (ma.startsWith("action_traCuuThietBiFragment_to_")
                    || ma.startsWith("action_chiTietLoaiThietBiFragment_to_")) {
                continue;
            }
            if (ma.equals("action_global_to_dangNhapSinhVienFragment")) {
                // Quy tắc 5: phiên kết thúc xóa toàn bộ ngăn xếp (kể cả graph gốc).
                assertEquals("@id/nav_graph", hanhDong.getAttribute("app:popUpTo"));
            } else {
                String nguon = maId(((Element) hanhDong.getParentNode()).getAttribute("android:id"));
                assertEquals("@id/" + nguon, hanhDong.getAttribute("app:popUpTo"));
            }
            assertEquals("true", hanhDong.getAttribute("app:popUpToInclusive"));
            daThay.add(ma);
        }
        assertEquals(4, daThay.size());
    }


    @Test
    public void FA53_NavGraph_BonTabGocCoNhanLamTieuDeThanhTieuDe() {
        for (Element e : cacPhanTu(navGraph(), "fragment")) {
            String ma = maId(e.getAttribute("android:id"));
            // D09-L: màn hình mở chồng chiTietLoaiThietBiFragment có nhãn vì MainActivity lấy tiêu đề từ nhãn.
            if (TAB_GOC.contains(ma) || ma.equals("chiTietLoaiThietBiFragment")) {
                assertTrue(ma, e.getAttribute("android:label").startsWith("@string/"));
            } else {
                assertEquals("Màn hình chung không có tiêu đề: " + ma, "", e.getAttribute("android:label"));
            }
        }
    }
}
