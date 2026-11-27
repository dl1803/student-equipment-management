package com.nhom13.quanlymuontra;

import static com.nhom13.quanlymuontra.HoTroXml.cacPhanTu;
import static com.nhom13.quanlymuontra.HoTroXml.maId;
import static org.junit.Assert.assertEquals;
import static org.junit.Assert.assertFalse;
import static org.junit.Assert.assertTrue;

import java.io.File;
import java.util.HashMap;
import java.util.HashSet;
import java.util.Map;
import org.junit.Test;
import org.w3c.dom.Document;
import org.w3c.dom.Element;

/**
 * Bảo vệ cấu trúc SCR-SV-04 bằng cách đọc tài nguyên và mã nguồn (JVM): layout, chữ nguyên văn, nav graph con, quy tắc
 * mã của FC 6.2, 6.3, 6.6. Hình thức, bố cục ở 320/360/412 dp, cỡ chữ 1,3 lần, vùng chạm 48 dp, TalkBack, hộp xác nhận
 * đổi phòng, tải thêm khi cuộn và vòng đi–về SCR-SV-04 ↔ SCR-SV-05 cần thiết bị hoặc emulator: chưa chạy.
 */
public class ManHinhSoanPhieuD10Test {

    private static final String RES = "src/main/res";
    private static final String MA = "src/main/java/com/nhom13/quanlymuontra/ui/phieuyeucau";

    private static Document navGraph() {
        return TaiNguyen.docXml(RES + "/navigation/nav_graph.xml");
    }

    private static Element phanTuCoId(Document doc, String the, String ma) {
        for (Element e : cacPhanTu(doc, the)) {
            if (ma.equals(maId(e.getAttribute("android:id")))) {
                return e;
            }
        }
        throw new AssertionError("Không có " + the + " " + ma);
    }

    private static void coTatCa(String noiDung, String... mau) {
        for (String m : mau) {
            assertTrue(m, noiDung.contains(m));
        }
    }

    @Test
    public void FA53_NavGraphCon_ChuaSoanPhieuYeuCauVaHaiThamSoChuoiTuyChon() {
        Document doc = navGraph();
        Element graph = phanTuCoId(doc, "navigation", "soanPhieuGraph");
        Element diemDen = phanTuCoId(doc, "fragment", "soanPhieuYeuCauFragment");
        Map<String, Element> thamSoGraph = new HashMap<>();
        Map<String, Element> thamSo = new HashMap<>();
        for (Element e : cacPhanTu(doc, "argument")) {
            if (e.getParentNode() == graph) {
                thamSoGraph.put(e.getAttribute("android:name"), e);
            } else if (e.getParentNode() == diemDen) {
                thamSo.put(e.getAttribute("android:name"), e);
            }
        }

        assertEquals("@id/soanPhieuYeuCauFragment", graph.getAttribute("app:startDestination"));
        assertTrue(diemDen.getParentNode() == graph);
        assertEquals(
                "com.nhom13.quanlymuontra.ui.phieuyeucau.SoanPhieuYeuCauFragment",
                diemDen.getAttribute("android:name"));
        assertEquals("@string/soan_phieu_tieu_de", diemDen.getAttribute("android:label"));
        assertEquals(
                new HashSet<>(java.util.Arrays.asList("maPhong", "maLoai")), thamSoGraph.keySet());
        assertEquals(new HashSet<>(java.util.Arrays.asList("maPhong", "maLoai")), thamSo.keySet());
        for (Element e : thamSoGraph.values()) {
            assertEquals("string", e.getAttribute("app:argType"));
            assertEquals("true", e.getAttribute("app:nullable"));
            assertEquals("@null", e.getAttribute("android:defaultValue"));
        }
        for (Element e : thamSo.values()) {
            assertEquals("string", e.getAttribute("app:argType"));
            assertEquals("true", e.getAttribute("app:nullable"));
            assertEquals("@null", e.getAttribute("android:defaultValue"));
        }
    }

    @Test
    public void FA53_SoanPhieu_ChiCoHanhDongSangHoSoKhongXoaNganXep() {
        Document doc = navGraph();
        Element diemDen = phanTuCoId(doc, "fragment", "soanPhieuYeuCauFragment");
        Map<String, String> hanhDong = new HashMap<>();
        for (Element e : cacPhanTu(doc, "action")) {
            if (e.getParentNode() == diemDen) {
                hanhDong.put(maId(e.getAttribute("android:id")), maId(e.getAttribute("app:destination")));
                assertEquals("", e.getAttribute("app:popUpTo"));
            }
        }

        Map<String, String> kyVong = new HashMap<>();
        kyVong.put("action_soanPhieuYeuCauFragment_to_hoSoMuonFragment", "hoSoMuonFragment");
        assertEquals(kyVong, hanhDong);
    }

    @Test
    public void UI54_Layout_CoKeoLamMoiDanhSachVaNutXemLaiGhimMacDinhVoHieu() {
        String xml = TaiNguyen.docChu(RES + "/layout/fragment_soan_phieu_yeu_cau.xml");

        coTatCa(
                xml,
                "androidx.swiperefreshlayout.widget.SwipeRefreshLayout",
                "@+id/srl_lam_moi",
                "androidx.recyclerview.widget.RecyclerView",
                "@+id/rv_loai",
                "@+id/ll_ghim",
                "@+id/btn_xem_lai",
                "@style/Widget.Ds.Button.Primary",
                "android:enabled=\"false\"",
                "@string/soan_phieu_nut_xem_lai");
    }

    @Test
    public void UI54_ViewDauVaCuoi_CoOChonPhongGhiChuTrangThaiVaOThoiGian() {
        String dau = TaiNguyen.docChu(RES + "/layout/view_soan_phieu_dau.xml");
        String chan = TaiNguyen.docChu(RES + "/layout/view_soan_phieu_chan.xml");

        coTatCa(
                dau,
                "@+id/banner_loi_quyen",
                "@+id/sel_phong",
                "@+id/tv_ghi_chu_phong",
                "@+id/lv_loai",
                "@+id/ev_loai",
                "@+id/ev_rong");
        coTatCa(
                chan,
                "@+id/til_thoi_gian",
                "@+id/et_thoi_gian",
                "android:inputType=\"number\"",
                "android:labelFor=\"@+id/et_thoi_gian\"",
                "@style/Widget.Ds.TextField");
    }

    @Test
    public void UI54_DongLoai_CoBoChonSoLuongNhanHetHangVaLienKetDangKyCho() {
        String xml = TaiNguyen.docChu(RES + "/layout/item_soan_phieu_loai.xml");

        coTatCa(
                xml,
                "@style/Widget.Ds.ListRow",
                "@+id/qs_so_luong",
                "common.widget.QuantityStepperView",
                "@+id/sb_het_hang",
                "@+id/btn_dang_ky_cho",
                "@string/chi_tiet_hanh_dong_dang_ky_cho");
        assertFalse("Dòng có điều khiển không có chevron", xml.contains("ic_chevron_right"));
    }

    @Test
    public void UI54_Chu_NguyenVanTaiLieu() {
        String chu = TaiNguyen.docChu(RES + "/values/strings.xml");

        coTatCa(
                chu,
                "name=\"soan_phieu_tieu_de\">Đặt trước thiết bị<",
                "name=\"soan_phieu_nhan_phong\">Phòng thiết bị *<",
                "name=\"soan_phieu_so_luong_nhan\">Số lượng:<",
                "name=\"soan_phieu_thoi_gian_nhan\">Thời gian mượn dự kiến (giờ) *<",
                "name=\"soan_phieu_nut_xem_lai\">Xem lại<");
    }

    @Test
    public void FC63_Fragment_DungViewModelPhamViNavGraphConVaKhongGoiApi() {
        String ma = TaiNguyen.docChu(MA + "/SoanPhieuYeuCauFragment.java");

        assertFalse(ma.contains("retrofit2"));
        assertFalse(ma.contains("Repository"));
        coTatCa(
                ma,
                "getViewModelStoreOwner(R.id.soanPhieuGraph)",
                "SoanPhieuYeuCauFragmentArgs.fromBundle",
                "SoanPhieuYeuCauFragmentDirections",
                "viewModel.load()",
                "danhDauCanTaiLai()",
                "getViewLifecycleOwner()",
                "R.id.soanPhieuYeuCauFragment",
                "XacNhanDialogFragment",
                "setCheDoBiChan(true)");
    }

    @Test
    public void FC61_ViewModel_KhongGiuViewContextHayHandlerVaCoOnCleared() {
        String ma = TaiNguyen.docChu(MA + "/SoanPhieuViewModel.java");

        for (String cam : new String[] {"android.view", "android.content.Context", "android.os.Handler"}) {
            assertFalse("ViewModel import " + cam, ma.contains("import " + cam));
        }
        assertTrue(ma.contains("onCleared()"));
    }

    @Test
    public void UISV04_D10_KhongGuiDangKyChoKhongGuiPhieuVaKhongTuTinhGioiHan() {
        for (String tep :
                new String[] {
                    "SoanPhieuViewModel.java",
                    "SoanPhieuYeuCauFragment.java",
                    "SoanPhieuYeuCauAdapter.java",
                    "CongQuyenMuon.java"
                }) {
            String ma = TaiNguyen.docChu(MA + "/" + tep);
            assertFalse(tep, ma.contains("svDangKyCho"));
            assertFalse(tep, ma.contains("svGuiPhieuYeuCauMuon"));
            assertFalse(tep, ma.contains("getSoThietBiToiDa"));
            assertFalse(tep, ma.contains("getThoiGianMuonToiDa"));
        }
    }

    @Test
    public void FC66_MaGoiPhieuYeuCau_KhongSnackbarToastLogDetailHayTuSapXep() {
        for (File tep : TaiNguyen.cacTepDuoi(MA, ".java")) {
            String noiDung = TaiNguyen.docChu(tep);
            for (String cam :
                    new String[] {
                        "Snackbar", "Toast", "System.out", "Log.", "getDetail(", "Collections.sort", ".sort("
                    }) {
                assertFalse(tep.getName() + " dùng " + cam, noiDung.contains(cam));
            }
        }
    }
}
