package com.nhom13.quanlymuontra;

import static org.junit.Assert.assertFalse;
import static org.junit.Assert.assertTrue;

import java.io.File;
import org.junit.Test;

/**
 * Bảo vệ cấu trúc các thành phần CMP-05, 06, 08, 16, 17 của D08-L bằng đọc tài nguyên và mã nguồn (JVM). Hình
 * thức, bố cục thật và tiếp cận cần xem trên thiết bị (chưa chạy).
 */
public class ThanhPhanTraCuuD08Test {

    private static final String RES = "src/main/res";
    private static final String WIDGET = "src/main/java/com/nhom13/quanlymuontra/common/widget";

    @Test
    public void FC85_NamStyleThanhPhan_DuocKhaiBaoTheoPhuLucA() {
        String kieu = TaiNguyen.docChu(RES + "/values/ds_styles.xml");

        for (String ten :
                new String[] {
                    "Widget.Ds.SearchField",
                    "Widget.Ds.Select",
                    "Widget.Ds.SegmentedControl",
                    "Widget.Ds.SegmentedControl.Button",
                    "Widget.Ds.FilterChip",
                    "Widget.Ds.ListRow",
                }) {
            assertTrue(ten, kieu.contains("name=\"" + ten + "\""));
        }
    }

    @Test
    public void DS8_BieuTuongVaNenCuaThanhPhan_TonTai() {
        for (String tep :
                new String[] {
                    RES + "/drawable/ic_expand_more.xml",
                    RES + "/drawable/ic_search_off.xml",
                    RES + "/drawable/ic_close.xml",
                    RES + "/drawable/ic_check.xml",
                    RES + "/drawable/ds_o_chon_nen.xml",
                    RES + "/drawable/ds_lua_chon_nen.xml",
                    RES + "/color/ds_segment_bg.xml",
                    RES + "/color/ds_segment_stroke.xml",
                    RES + "/layout/view_select.xml",
                    RES + "/layout/dialog_chon_mot.xml",
                    RES + "/layout/item_lua_chon.xml",
                    RES + "/layout/view_chip_bo_loc.xml",
                }) {
            assertTrue(tep, TaiNguyen.tep(tep).isFile());
        }
    }

    @Test
    public void CMP06_SelectView_CoBaTrangThaiVaDungIdLayout() {
        String xml = TaiNguyen.docChu(RES + "/layout/view_select.xml");

        for (String ma :
                new String[] {
                    "@+id/tv_nhan", "@+id/ll_o_chon", "@+id/tv_gia_tri", "@+id/ll_loi", "@+id/btn_thu_lai"
                }) {
            assertTrue(ma, xml.contains(ma));
        }
        String ma = TaiNguyen.docChu(WIDGET + "/SelectView.java");
        for (String phuongThuc : new String[] {"hienGiaTri(", "hienDangTai(", "hienLoi("}) {
            assertTrue(phuongThuc, ma.contains("public void " + phuongThuc));
        }
    }

    @Test
    public void CMP06_ChonMotDialog_KhongDongKhiChamNgoaiVaTraKetQuaTheoMa() {
        String ma = TaiNguyen.docChu(WIDGET + "/ChonMotDialogFragment.java");

        assertTrue(ma.contains("setCanceledOnTouchOutside(false)"));
        assertTrue(ma.contains("setFragmentResult("));
        assertTrue(ma.contains("KEY_MA"));
    }

    @Test
    public void FC66_MaThanhPhanMoi_KhongSnackbarToastLogHayDetail() {
        for (String tep : new String[] {"SelectView.java", "ChonMotDialogFragment.java"}) {
            String noiDung = TaiNguyen.docChu(WIDGET + "/" + tep);
            for (String cam : new String[] {"Snackbar", "Toast", "System.out", "Log.", "getDetail("}) {
                assertFalse(tep + " dùng " + cam, noiDung.contains(cam));
            }
        }
    }
}
