package com.nhom13.quanlymuontra;

import static org.junit.Assert.assertFalse;
import static org.junit.Assert.assertTrue;

import org.junit.Test;

/** Bảo vệ việc nối lối vào SCR-SV-04 từ SCR-SV-01 và SCR-SV-03 (đọc mã nguồn, JVM). */
public class ManHinhLoiVaoSoanPhieuD10Test {

    private static final String JAVA = "src/main/java/com/nhom13/quanlymuontra";

    @Test
    public void FA53_TrangChu_DatTruocChayCongRoiMoSoanPhieuHoacHoSoBiChan() {
        String ma = TaiNguyen.docChu(JAVA + "/ui/trangchu/TrangChuFragment.java");

        for (String m :
                new String[] {
                    "case DAT_TRUOC:",
                    "viewModel.chonDatTruoc()",
                    "actionTrangChuFragmentToSoanPhieuGraph",
                    "setCheDoBiChan(true)",
                    "setHanhDongDaChon(",
                    "R.id.trangChuFragment",
                    "QuyenMuon.laBinhThuong"
                }) {
            assertTrue(m, ma.contains(m));
        }
        assertFalse(ma.contains("Repository"));
        assertFalse(ma.contains("retrofit2"));
    }

    @Test
    public void FA53_ChiTietLoai_DatTruocMoSoanPhieuVoiMaPhongVaMaLoai() {
        String ma = TaiNguyen.docChu(JAVA + "/ui/tracuu/ChiTietLoaiThietBiFragment.java");

        for (String m :
                new String[] {
                    "actionChiTietLoaiThietBiFragmentToSoanPhieuGraph",
                    ".setMaPhong(thamSo.getMaPhong())",
                    ".setMaLoai(thamSo.getMaLoai())",
                    "HanhDong.DAT_TRUOC"
                }) {
            assertTrue(m, ma.contains(m));
        }
        assertFalse(ma.contains("svDangKyCho"));
    }

    @Test
    public void FA53_TrangChu_LayoutCoKhoiLoiQuyenVaViewModelDungCong() {
        String xml = TaiNguyen.docChu("src/main/res/layout/fragment_trang_chu.xml");
        String vm = TaiNguyen.docChu(JAVA + "/ui/trangchu/TrangChuViewModel.java");

        assertTrue(xml.contains("@+id/banner_loi_quyen"));
        assertTrue(vm.contains("CongQuyenMuon"));
        assertTrue(vm.contains("cong.huy()"));
    }
}
