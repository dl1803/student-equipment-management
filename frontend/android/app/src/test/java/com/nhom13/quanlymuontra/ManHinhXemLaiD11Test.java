package com.nhom13.quanlymuontra;

import static com.nhom13.quanlymuontra.HoTroXml.cacPhanTu;
import static com.nhom13.quanlymuontra.HoTroXml.maId;
import static org.junit.Assert.assertEquals;
import static org.junit.Assert.assertFalse;
import static org.junit.Assert.assertTrue;

import java.util.HashMap;
import java.util.Map;
import org.junit.Test;
import org.w3c.dom.Document;
import org.w3c.dom.Element;

/**
 * Bảo vệ cấu trúc SCR-SV-05 bằng cách đọc tài nguyên và mã nguồn (JVM). Hình thức, bố cục ở 320/360/412 dp, cỡ chữ 1,3
 * lần, vùng chạm 48 dp, TalkBack, vòng đi–về SCR-SV-04 ↔ SCR-SV-05 và hành vi thật khi gửi cần thiết bị hoặc emulator: chưa
 * chạy.
 */
public class ManHinhXemLaiD11Test {

    private static final String RES = "src/main/res";
    private static final String MA = "src/main/java/com/nhom13/quanlymuontra/ui/phieuyeucau";

    private static void coTatCa(String noiDung, String... mau) {
        for (String m : mau) {
            assertTrue(m, noiDung.contains(m));
        }
    }

    private static Element phanTuCoId(Document doc, String the, String ma) {
        for (Element e : cacPhanTu(doc, the)) {
            if (ma.equals(maId(e.getAttribute("android:id")))) {
                return e;
            }
        }
        throw new AssertionError("Không có " + the + " " + ma);
    }

    @Test
    public void FA53_XemLai_ThuocNavGraphConCoNhanVaChiCoHanhDongSangHoSo() {
        Document doc = TaiNguyen.docXml(RES + "/navigation/nav_graph.xml");
        Element graph = phanTuCoId(doc, "navigation", "soanPhieuGraph");
        Element diemDen = phanTuCoId(doc, "fragment", "xemLaiPhieuYeuCauFragment");

        assertTrue(diemDen.getParentNode() == graph);
        assertEquals(
                "com.nhom13.quanlymuontra.ui.phieuyeucau.XemLaiPhieuYeuCauFragment",
                diemDen.getAttribute("android:name"));
        assertEquals("@string/xem_lai_tieu_de", diemDen.getAttribute("android:label"));
        Map<String, String> hanhDong = new HashMap<>();
        for (Element e : cacPhanTu(doc, "action")) {
            if (e.getParentNode() == diemDen) {
                hanhDong.put(maId(e.getAttribute("android:id")), maId(e.getAttribute("app:destination")));
                assertEquals("", e.getAttribute("app:popUpTo"));
            }
        }
        Map<String, String> kyVong = new HashMap<>();
        kyVong.put("action_xemLaiPhieuYeuCauFragment_to_hoSoMuonFragment", "hoSoMuonFragment");
        assertEquals(kyVong, hanhDong);
    }

    @Test
    public void UI55_Layout_CoBannerTheTomTatVaHaiNutGhim() {
        String xml = TaiNguyen.docChu(RES + "/layout/fragment_xem_lai_phieu_yeu_cau.xml");

        coTatCa(
                xml,
                "@+id/banner_ket_qua",
                "@+id/card_tom_tat",
                "common.widget.KeyValueListView",
                "@+id/kvl_tom_tat",
                "@+id/ll_ghim",
                "@+id/btn_sua",
                "@style/Widget.Ds.Button.Secondary",
                "@string/xem_lai_nut_sua",
                "@+id/btn_gui",
                "@style/Widget.Ds.Button.Primary",
                "@string/xem_lai_nut_gui");
    }

    @Test
    public void UI55_DauVaDongLoai_CoKhoiLoiGuiVaLoiSoLuong() {
        coTatCa(TaiNguyen.docChu(RES + "/layout/view_soan_phieu_dau.xml"), "@+id/banner_loi_gui");
        coTatCa(TaiNguyen.docChu(RES + "/layout/item_soan_phieu_loai.xml"), "@+id/tv_loi_so_luong");
    }

    @Test
    public void UI55_Chu_NguyenVanTaiLieuVaChuaCoChuCamTrongTaiLieu() {
        String chu = TaiNguyen.docChu(RES + "/values/strings.xml");

        coTatCa(
                chu,
                "name=\"xem_lai_tieu_de\">Xem lại phiếu<",
                "name=\"xem_lai_tom_tat_tieu_de\">Tóm tắt phiếu<",
                "name=\"gui_chua_xac_dinh_tieu_de\">Chưa xác định được kết quả<");
    }

    @Test
    public void FC63_Fragment_DungViewModelPhamViNavGraphConVaKhongGoiApi() {
        String ma = TaiNguyen.docChu(MA + "/XemLaiPhieuYeuCauFragment.java");

        assertFalse(ma.contains("retrofit2"));
        assertFalse(ma.contains("Repository"));
        coTatCa(
                ma,
                "getViewModelStoreOwner(R.id.soanPhieuGraph)",
                "XemLaiPhieuYeuCauFragmentDirections",
                "getViewLifecycleOwner()",
                "R.id.xemLaiPhieuYeuCauFragment",
                "NutDangXuLy.batDau",
                "setCheDoBiChan(true)");
    }

    @Test
    public void FC63_SoanPhieu_NutXemLaiCoDichVaKhongGoiApiGui() {
        String ma = TaiNguyen.docChu(MA + "/SoanPhieuYeuCauFragment.java");

        coTatCa(
                ma,
                "actionSoanPhieuYeuCauFragmentToXemLaiPhieuYeuCauFragment",
                "chuanBiXemLai()",
                "R.id.soanPhieuYeuCauFragment");
        assertFalse(ma.contains("svGuiPhieuYeuCauMuon"));
    }

    @Test
    public void FA311_ViewModel_KhongGiuLoiGoiGhiDeHuyKhiOnCleared() {
        String ma = TaiNguyen.docChu(MA + "/SoanPhieuViewModel.java");

        assertFalse("Không lưu Call của lần gửi để hủy", ma.contains("Call<PhieuYeuCauMuon>"));
        assertTrue(ma.contains("Cố ý không hủy lời gọi gửi phiếu"));
    }
}
