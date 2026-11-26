package com.nhom13.quanlymuontra;

import static org.junit.Assert.assertFalse;
import static org.junit.Assert.assertTrue;

import java.io.File;
import org.junit.Test;

/**
 * Bảo vệ cấu trúc SCR-SV-03 và CMP-23 của D09-L bằng cách đọc tài nguyên và mã nguồn (JVM). Không chứng minh hình
 * thức thật, bố cục theo cỡ màn hình, tải ảnh thật bằng Glide, cuộn ngang, nút Back đóng ảnh lớn: cần thiết bị.
 */
public class ManHinhChiTietLoaiD09Test {

    private static final String JAVA = "src/main/java/com/nhom13/quanlymuontra";
    private static final String TRA_CUU = JAVA + "/ui/tracuu";
    private static final String WIDGET = JAVA + "/common/widget";
    private static final String RES = "src/main/res";

    private static void coTatCa(String noiDung, String... mau) {
        for (String m : mau) {
            assertTrue(m, noiDung.contains(m));
        }
    }

    @Test
    public void UI53_Layout_CoDayDuThanhPhan() {
        String xml = TaiNguyen.docChu(RES + "/layout/fragment_chi_tiet_loai_thiet_bi.xml");

        coTatCa(
                xml,
                "androidx.swiperefreshlayout.widget.SwipeRefreshLayout",
                "@+id/srl_lam_moi",
                "@+id/sv_noi_dung",
                "@+id/banner_loi_quyen",
                "@+id/banner_khong_tim_thay",
                "@+id/lv_tai",
                "@+id/ev_loi",
                "@+id/ll_noi_dung",
                "com.nhom13.quanlymuontra.common.widget.AnhGalleryView",
                "@+id/agv_anh",
                "@+id/tv_ten_loai",
                "@+id/tv_hang_phong_co_so",
                "@+id/ll_mo_ta",
                "@+id/tv_mo_ta",
                "@+id/ll_thong_so",
                "@+id/ll_ds_thong_so",
                "@+id/tv_so_luong",
                "@+id/sb_het_hang",
                "@+id/ll_ngay_du_kien",
                "@+id/tv_ngay_du_kien",
                "@+id/ll_ghim",
                "@+id/btn_hanh_dong",
                "@style/Widget.Ds.Button.Primary",
                "@style/TextAppearance.Ds.NumericLg");
    }

    @Test
    public void CMP23_ViewAnhGallery_CoDaiAnhVaKhoiLoi() {
        String xml = TaiNguyen.docChu(RES + "/layout/view_anh_gallery.xml");

        coTatCa(
                xml,
                "<merge",
                "@+id/hsv_dai_anh",
                "@+id/ll_dai_anh",
                "@+id/ll_loi_anh",
                "@+id/btn_tai_lai_anh",
                "@drawable/ic_broken_image",
                "@string/anh_khong_tai_duoc",
                "@string/anh_tai_lai",
                "@style/Widget.Ds.Button.Tertiary");
    }

    @Test
    public void CMP23_DialogXemAnh_CoLopPhuNutDongVaAnh() {
        String xml = TaiNguyen.docChu(RES + "/layout/dialog_xem_anh.xml");

        coTatCa(
                xml,
                "@+id/fl_nen",
                "@color/ds_color_overlay_scrim",
                "@+id/img_anh_lon",
                "@dimen/ds_shadow_dialog",
                "@+id/btn_dong",
                "@style/Widget.Ds.IconButton",
                "@string/hanh_dong_dong");
    }

    @Test
    public void CMP23_TaiNguyen_CoBieuTuongVaNenOAnh() {
        for (String tep :
                new String[] {
                    "/drawable/ic_image.xml", "/drawable/ic_broken_image.xml", "/drawable/ds_anh_o_nen.xml"
                }) {
            assertTrue(tep, TaiNguyen.tep(RES + tep).isFile());
        }
        assertTrue(
                TaiNguyen.docChu(RES + "/drawable/ds_anh_o_nen.xml")
                        .contains("@color/ds_color_surface_muted"));
    }

    @Test
    public void UI53_Chu_NguyenVanTaiLieu() {
        String chu = TaiNguyen.docChu(RES + "/values/strings.xml");

        coTatCa(
                chu,
                "name=\"chi_tiet_thiet_bi_tieu_de\">Chi tiết thiết bị<",
                "name=\"chi_tiet_mo_ta\">Mô tả<",
                "name=\"chi_tiet_thong_so\">Thông số kỹ thuật<",
                "name=\"chi_tiet_so_luong_kha_dung\">Số lượng khả dụng<",
                "name=\"chi_tiet_ngay_du_kien\">Ngày dự kiến có sẵn<",
                "name=\"chi_tiet_chua_co_ngay\">Chưa có ngày dự kiến<",
                "name=\"chi_tiet_hanh_dong_dat_truoc\">Đặt trước<",
                "name=\"chi_tiet_hanh_dong_dang_ky_cho\">Đăng ký chờ<",
                "name=\"chi_tiet_khong_tim_thay_tieu_de\">Không tìm thấy<",
                "name=\"anh_khong_tai_duoc\">Không tải được ảnh<",
                "name=\"anh_tai_lai\">Tải lại<",
                "name=\"anh_mo_ta_o_anh\">Ảnh %1$d của %2$s<");
    }

    @Test
    public void UI52_DongKetQua_BamDuocVaMoSCRSV03() {
        String item = TaiNguyen.docChu(RES + "/layout/item_tra_cuu_thiet_bi.xml");
        String adapter = TaiNguyen.docChu(TRA_CUU + "/TraCuuThietBiAdapter.java");
        String manHinh = TaiNguyen.docChu(TRA_CUU + "/TraCuuThietBiFragment.java");

        coTatCa(
                item,
                "@style/Widget.Ds.ListRow",
                "android:clickable=\"true\"",
                "android:focusable=\"true\"",
                "app:rippleColor=\"@color/ds_color_overlay_pressed\"");
        coTatCa(adapter, "Consumer<TraCuuThietBiItem>", "setOnClickListener", "khiChon.accept(dong)");
        coTatCa(
                manHinh,
                "actionTraCuuThietBiFragmentToChiTietLoaiThietBiFragment",
                "R.id.traCuuThietBiFragment");
    }

    @Test
    public void FA57_Glide_DaKhoaPhienBanVaThemVaoBuild() {
        String toml = TaiNguyen.docChu("../gradle/libs.versions.toml");
        String gradle = TaiNguyen.docChu("build.gradle.kts");

        coTatCa(
                toml,
                "glide = \"5.0.7\"",
                "glide = { module = \"com.github.bumptech.glide:glide\", version.ref = \"glide\" }");
        assertTrue(gradle.contains("implementation(libs.glide)"));
    }

    @Test
    public void FA57_WidgetAnh_AnhDanhMucDungBoNhoDemMacDinhVaKhongGhiNhatKy() {
        for (String ten : new String[] {"AnhGalleryView.java", "ImageViewerDialogFragment.java"}) {
            String ma = TaiNguyen.docChu(WIDGET + "/" + ten);
            for (String cam :
                    new String[] {
                        "DiskCacheStrategy", "Log.", "System.out", "Snackbar", "Toast", "retrofit2", "Repository"
                    }) {
                assertFalse(ten + " dùng " + cam, ma.contains(cam));
            }
            assertTrue(ten, ma.contains("Glide.with("));
        }
    }

    @Test
    public void FC62_Fragment_KhongGoiApiVaDungDirectionsCoKiemTraDiemDen() {
        String ma = TaiNguyen.docChu(TRA_CUU + "/ChiTietLoaiThietBiFragment.java");

        assertFalse(ma.contains("retrofit2"));
        assertFalse(ma.contains("Repository"));
        coTatCa(
                ma,
                "viewModel.load()",
                "danhDauCanTaiLai()",
                "ChiTietLoaiThietBiFragmentArgs.fromBundle",
                "ChiTietLoaiThietBiFragmentDirections",
                "setCheDoBiChan(true)",
                "setHanhDongDaChon(",
                "R.id.chiTietLoaiThietBiFragment");
    }

    @Test
    public void FC61_ViewModel_KhongGiuViewContextHayHandler() {
        String ma = TaiNguyen.docChu(TRA_CUU + "/ChiTietLoaiThietBiViewModel.java");

        for (String cam : new String[] {"android.view", "android.content.Context", "android.os.Handler"}) {
            assertFalse("ViewModel import " + cam, ma.contains("import " + cam));
        }
        assertTrue(ma.contains("onCleared()"));
    }

    @Test
    public void UISV03_D09_ChiLamCongKhongGuiDangKyChoVaChiDuaVaoTrangThaiQuyenMuon() {
        String viewModel = TaiNguyen.docChu(TRA_CUU + "/ChiTietLoaiThietBiViewModel.java");
        String fragment = TaiNguyen.docChu(TRA_CUU + "/ChiTietLoaiThietBiFragment.java");
        String repository = TaiNguyen.docChu(JAVA + "/data/repository/TraCuuRepository.java");

        for (String ma : new String[] {viewModel, fragment}) {
            assertFalse(ma.contains("svDangKyCho"));
            assertFalse(ma.contains("DangKyChoRepository"));
            assertFalse(ma.contains("HopLe("));
        }
        coTatCa(viewModel, "getTrangThaiQuyenMuon", "QuyenMuon.laBinhThuong", "svToi");
        assertTrue(repository.contains("svTraCuuThietBiChiTiet"));
        assertFalse(repository.contains("svDangKyCho"));
    }

    @Test
    public void UISV03_KhongCoChucNangTaiAnhLen() {
        // Kết quả D09-L: sinh viên không có chức năng upload ảnh (chỉ xem ảnh danh mục).
        String[] tep = {
            TRA_CUU + "/ChiTietLoaiThietBiFragment.java",
            TRA_CUU + "/ChiTietLoaiThietBiViewModel.java",
            WIDGET + "/AnhGalleryView.java",
            WIDGET + "/ImageViewerDialogFragment.java"
        };
        for (String duongDan : tep) {
            String ma = TaiNguyen.docChu(duongDan);
            for (String cam :
                    new String[] {
                        "ActivityResultContracts",
                        "PickVisualMedia",
                        "ACTION_PICK",
                        "ACTION_GET_CONTENT",
                        "MultipartBody"
                    }) {
                assertFalse(duongDan + " dùng " + cam, ma.contains(cam));
            }
        }
    }

    @Test
    public void FC66_MaMoiD09_KhongMaMauThoHayPx() {
        for (File tep : TaiNguyen.cacTepDuoi(TRA_CUU, ".java")) {
            String noiDung = TaiNguyen.docChu(tep);
            assertFalse(tep.getName(), noiDung.contains("parseColor"));
        }
    }
}
