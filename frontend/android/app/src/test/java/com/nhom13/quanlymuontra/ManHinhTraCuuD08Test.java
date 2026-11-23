package com.nhom13.quanlymuontra;

import static org.junit.Assert.assertFalse;
import static org.junit.Assert.assertTrue;

import java.io.File;
import org.junit.Test;

/**
 * Bảo vệ cấu trúc SCR-SV-02 bằng đọc tài nguyên và mã nguồn (JVM): vùng của layout, chữ nguyên văn tài liệu, và quy
 * tắc mã của FC 6.2, 6.3, 6.6 cho gói tracuu. Hình thức, bố cục ở 320, 360, 412 dp, cỡ chữ 1,3 lần, TalkBack và hành
 * vi thật cần thiết bị hoặc emulator (chưa chạy).
 */
public class ManHinhTraCuuD08Test {

    private static final String MA = "src/main/java/com/nhom13/quanlymuontra/ui/tracuu";
    private static final String RES = "src/main/res";

    @Test
    public void UI52_Layout_CoKeoLamMoiVaDanhSachRecyclerView() {
        String xml = TaiNguyen.docChu(RES + "/layout/fragment_tra_cuu_thiet_bi.xml");

        for (String ma :
                new String[] {
                    "androidx.swiperefreshlayout.widget.SwipeRefreshLayout",
                    "androidx.recyclerview.widget.RecyclerView",
                    "@+id/srl_lam_moi",
                    "@+id/rv_ket_qua",
                }) {
            assertTrue(ma, xml.contains(ma));
        }
    }

    @Test
    public void UI52_DauDanhSach_CoOTimBoLocChipVaBaTrangThai() {
        String xml = TaiNguyen.docChu(RES + "/layout/view_tra_cuu_bo_loc.xml");

        for (String ma :
                new String[] {
                    "@style/Widget.Ds.SearchField",
                    "@+id/et_tim_kiem",
                    "@+id/sel_loai",
                    "@+id/sel_phong",
                    "@style/Widget.Ds.SegmentedControl",
                    "@+id/btn_tinh_trang_tat_ca",
                    "@+id/btn_tinh_trang_con_hang",
                    "@+id/btn_tinh_trang_het_hang",
                    "@+id/cg_bo_loc",
                    "@+id/btn_xoa_bo_loc",
                    "@+id/lv_ket_qua",
                    "@+id/ev_ket_qua",
                    "@+id/ev_rong",
                }) {
            assertTrue(ma, xml.contains(ma));
        }
    }

    @Test
    public void DS85_DongKetQua_DungStyleListRowVaCacThanhPhan() {
        String xml = TaiNguyen.docChu(RES + "/layout/item_tra_cuu_thiet_bi.xml");

        for (String ma :
                new String[] {
                    "@style/Widget.Ds.ListRow", "@+id/tv_ten_loai", "@+id/tv_phong", "@+id/tv_kha_dung", "@+id/sb_het_hang"
                }) {
            assertTrue(ma, xml.contains(ma));
        }
    }

    @Test
    public void UI52_Chu_NguyenVanTaiLieu() {
        String chu = TaiNguyen.docChu(RES + "/values/strings.xml");

        for (String dong :
                new String[] {
                    "name=\"tra_cuu_tim_kiem_nhan\">Tìm theo tên thiết bị<",
                    "name=\"tra_cuu_rong\">Không tìm thấy thiết bị phù hợp<",
                    "name=\"tra_cuu_xoa_bo_loc\">Xóa bộ lọc<",
                    "name=\"tra_cuu_khong_tai_them\">Không tải thêm được<",
                    "name=\"tra_cuu_tat_ca\">Tất cả<",
                    "name=\"tra_cuu_con_hang\">Còn hàng<",
                    "name=\"tra_cuu_het_hang\">Hết hàng<",
                    "name=\"tra_cuu_loc_loai\">Loại thiết bị<",
                    "name=\"tra_cuu_loc_phong\">Phòng thiết bị<",
                    "name=\"tra_cuu_loc_tinh_trang\">Tình trạng hàng<",
                }) {
            assertTrue(dong, chu.contains(dong));
        }
    }

    @Test
    public void FC66_MaGoiTracuu_KhongSnackbarToastLogDetailHayTuSapXep() {
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

    @Test
    public void FC62_Fragment_KhongGoiApiVaKhongTuTinhTonKho() {
        String ma = TaiNguyen.docChu(MA + "/TraCuuThietBiFragment.java");

        assertFalse(ma.contains("retrofit2"));
        assertFalse(ma.contains("Repository"));
        assertTrue(ma.contains("viewModel.load()"));
        assertTrue(ma.contains("danhDauCanTaiLai()"));
    }

    @Test
    public void FC61_ViewModel_KhongGiuViewContextHayHandler() {
        String ma = TaiNguyen.docChu(MA + "/TraCuuThietBiViewModel.java");

        for (String cam : new String[] {"android.view", "android.content.Context", "android.os.Handler"}) {
            assertFalse("ViewModel import " + cam, ma.contains("import " + cam));
        }
        assertTrue(ma.contains("HangSo.DO_TRE_GO_TIM_KIEM_MS"));
    }
}
