package com.nhom13.quanlymuontra;

import static org.junit.Assert.assertFalse;
import static org.junit.Assert.assertTrue;

import java.io.File;
import org.junit.Test;

/**
 * Bảo vệ cấu trúc SCR-SV-14 bằng cách đọc tài nguyên và mã nguồn (JVM): các khối theo thứ tự Wireframes, không có
 * nút mở khóa, không hộp xác nhận khi đăng xuất, quy tắc mã của FC 6.6. Nút Quay lại và bố cục thật cần xem trên
 * thiết bị (chưa chạy).
 */
public class ManHinhHoSoMuonD07Test {

    private static final String MA = "src/main/java/com/nhom13/quanlymuontra/ui/hoso";

    @Test
    public void UI514_Layout_CacKhoiTheoThuTuWireframes() {
        String xml = TaiNguyen.docChu("src/main/res/layout/fragment_ho_so_muon.xml");
        String[] thuTu = {
            "@+id/banner_bi_chan",
            "@+id/card_thong_tin",
            "@+id/card_quyen_muon",
            "@+id/card_bien_ban",
            "@+id/card_ly_do",
            "@+id/card_huong_dan",
            "@+id/btn_dang_xuat",
        };

        int viTriTruoc = -1;
        for (String ma : thuTu) {
            int viTri = xml.indexOf(ma);
            assertTrue(ma, viTri > viTriTruoc);
            viTriTruoc = viTri;
        }
        assertTrue(xml.contains("androidx.swiperefreshlayout.widget.SwipeRefreshLayout"));
    }

    @Test
    public void UI514_Layout_KhongCoNutMoKhoaHayGoCam() {
        String xml = TaiNguyen.docChu("src/main/res/layout/fragment_ho_so_muon.xml").toLowerCase();

        for (String cam : new String[] {"mo_khoa", "go_cam", "unlock", "mokhoa"}) {
            assertFalse(cam, xml.contains(cam));
        }
    }

    @Test
    public void UI41_Fragment_DangXuatKhongHopXacNhan() {
        String ma = TaiNguyen.docChu(MA + "/HoSoMuonFragment.java");

        assertFalse(ma.contains("XacNhanDialogFragment"));
        assertTrue(ma.contains("viewModel.dangXuat()"));
    }

    @Test
    public void FC66_MaNguonHoSo_KhongSnackbarToastLogHayDetail() {
        for (File tep : TaiNguyen.cacTepDuoi(MA, ".java")) {
            String noiDung = TaiNguyen.docChu(tep);
            for (String cam : new String[] {"Snackbar", "Toast", "System.out", "Log.", "getDetail("}) {
                assertFalse(tep.getName() + " dùng " + cam, noiDung.contains(cam));
            }
        }
    }

    @Test
    public void FA53_MainActivity_DungThamSoSafeArgsVaTabHoSoKhongKhoiPhuc() {
        String ma = TaiNguyen.docChu("src/main/java/com/nhom13/quanlymuontra/MainActivity.java");

        assertTrue(ma.contains("HoSoMuonFragmentArgs"));
        assertTrue(ma.contains("getCoNutQuayLai()"));
        assertTrue(ma.contains("getCheDoBiChan()"));
        assertTrue(ma.contains("taoTuyChonTab(false)"));
    }
}
