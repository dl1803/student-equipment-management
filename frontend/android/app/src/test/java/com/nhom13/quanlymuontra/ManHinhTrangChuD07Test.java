package com.nhom13.quanlymuontra;

import static org.junit.Assert.assertFalse;
import static org.junit.Assert.assertTrue;

import java.io.File;
import org.junit.Test;

/**
 * Bảo vệ cấu trúc SCR-SV-01 bằng cách đọc tài nguyên và mã nguồn (JVM): các vùng của layout, quy tắc mã của
 * FC 6.6 và 6.2 cho gói trangchu. Hình thức, bố cục và hành vi thật cần xem trên thiết bị (chưa chạy).
 */
public class ManHinhTrangChuD07Test {

    private static final String MA = "src/main/java/com/nhom13/quanlymuontra/ui/trangchu";

    @Test
    public void UI51_Layout_CoKeoLamMoiVungTheLoiVaDuLieuTheo() {
        String xml = TaiNguyen.docChu("src/main/res/layout/fragment_trang_chu.xml");

        for (String ma :
                new String[] {
                    "androidx.swiperefreshlayout.widget.SwipeRefreshLayout",
                    "@+id/srl_lam_moi",
                    "@+id/lv_the_quyen_muon",
                    "@+id/ev_the_quyen_muon",
                    "@+id/card_quyen_muon",
                    "@+id/btn_xem_ly_do",
                    "@+id/ll_loi_vao",
                }) {
            assertTrue(ma, xml.contains(ma));
        }
    }

    @Test
    public void FC66_MaNguonTrangChu_KhongSnackbarToastLogHayDetail() {
        for (File tep : TaiNguyen.cacTepDuoi(MA, ".java")) {
            String noiDung = TaiNguyen.docChu(tep);
            for (String cam : new String[] {"Snackbar", "Toast", "System.out", "Log.", "getDetail("}) {
                assertFalse(tep.getName() + " dùng " + cam, noiDung.contains(cam));
            }
        }
    }

    @Test
    public void UI51_Fragment_KhongGoiApiVaKhongTuTinhQuyenMuon() {
        String ma = TaiNguyen.docChu(MA + "/TrangChuFragment.java");

        assertFalse(ma.contains("retrofit2"));
        assertFalse(ma.contains("Repository"));
        assertTrue(ma.contains("QuyenMuon.laBinhThuong"));
    }
}
