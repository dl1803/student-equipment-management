package com.nhom13.quanlymuontra;

import android.graphics.drawable.Drawable;
import android.os.Bundle;
import android.view.View;
import androidx.annotation.Nullable;
import androidx.appcompat.app.AppCompatActivity;
import androidx.core.graphics.Insets;
import androidx.core.view.ViewCompat;
import androidx.core.view.WindowCompat;
import androidx.core.view.WindowInsetsCompat;
import androidx.navigation.NavController;
import androidx.navigation.NavDestination;
import androidx.navigation.NavOptions;
import androidx.navigation.fragment.NavHostFragment;
import com.nhom13.quanlymuontra.data.session.SessionManager;
import com.nhom13.quanlymuontra.data.session.TrangThaiPhien;
import com.nhom13.quanlymuontra.databinding.ActivityMainBinding;

/**
 * Activity duy nhất (FA 5.3): chứa {@code NavHostFragment}, thanh tiêu đề (CMP-28) và thanh điều hướng
 * dưới (CMP-27), và quan sát trạng thái phiên để đưa người dùng về đăng nhập khi phiên kết thúc.
 *
 * <p>Chuyển tab không dùng {@code NavigationUI.setupWithNavController}: hàm đó đưa ngăn xếp về điểm bắt đầu
 * của graph (KhoiDongFragment), mà KhoiDongFragment đã bị gỡ khỏi ngăn xếp sau khi khởi động. Ở đây dùng cùng
 * các tùy chọn (một bản duy nhất, lưu và khôi phục trạng thái) nhưng đưa ngăn xếp về {@code trangChuFragment}
 * (màn hình gốc luôn nằm ở đáy ngăn xếp sau khi khởi động/đăng nhập), giúp phím Back ở các tab phụ quay về
 * Trang chủ trước khi thoát ứng dụng (FA 5.3).
 */
public class MainActivity extends AppCompatActivity {

    /** Bốn màn hình gốc có thanh điều hướng dưới; mã trùng mã mục trong menu_bottom_nav.xml. */
    private static final int[] TAB_GOC = {
        R.id.trangChuFragment,
        R.id.traCuuThietBiFragment,
        R.id.thongBaoFragment,
        R.id.hoSoMuonFragment
    };

    private ActivityMainBinding binding;
    private NavController dieuHuong;
    private SessionManager sessionManager;

    @Override
    protected void onCreate(@Nullable Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);
        // targetSdk 36 bắt buộc giao diện tràn viền: tự đệm theo lề an toàn của hệ thống.
        WindowCompat.setDecorFitsSystemWindows(getWindow(), false);
        binding = ActivityMainBinding.inflate(getLayoutInflater());
        setContentView(binding.getRoot());
        ViewCompat.setOnApplyWindowInsetsListener(
                binding.getRoot(),
                (khung, lePhuHeThong) -> {
                    Insets le =
                            lePhuHeThong.getInsets(
                                    WindowInsetsCompat.Type.systemBars()
                                            | WindowInsetsCompat.Type.displayCutout()
                                            | WindowInsetsCompat.Type.ime());
                    khung.setPadding(le.left, le.top, le.right, le.bottom);
                    return WindowInsetsCompat.CONSUMED;
                });

        sessionManager = ((UngDung) getApplication()).getAppContainer().getSessionManager();
        NavHostFragment navHost =
                (NavHostFragment) getSupportFragmentManager().findFragmentById(R.id.fcv_dieu_huong);
        dieuHuong = navHost.getNavController();

        caiThanhTieuDe();
        caiThanhDieuHuong();
        dieuHuong.addOnDestinationChangedListener(
                (boDieuHuong, dich, thamSo) -> capNhatKhung(dich));

        // Tiến trình bị hệ điều hành hủy rồi dựng lại: ngăn xếp điều hướng được khôi phục nhưng
        // SessionManager mới còn trống. Đọc lại phiên; nếu không còn thì trạng thái KHONG_CO_PHIEN đưa
        // người dùng về đăng nhập ở bước quan sát bên dưới.
        if (savedInstanceState != null
                && sessionManager.getTrangThaiPhien().getValue() == TrangThaiPhien.CHUA_KHOI_PHUC) {
            sessionManager.restore();
        }
        sessionManager.getTrangThaiPhien().observe(this, this::khiTrangThaiPhienDoi);
    }

    private void caiThanhTieuDe() {
        binding.tbManHinh.setNavigationContentDescription(R.string.hanh_dong_quay_lai);
        binding.tbManHinh.setNavigationOnClickListener(nut -> dieuHuong.popBackStack());
    }

    private void caiThanhDieuHuong() {
        NavOptions tuyChonTab =
                new NavOptions.Builder()
                        .setLaunchSingleTop(true)
                        .setRestoreState(true)
                        .setPopUpTo(R.id.trangChuFragment, false, true)
                        .build();
        binding.bnvDieuHuong.setOnItemSelectedListener(
                muc -> {
                    dieuHuong.navigate(muc.getItemId(), null, tuyChonTab);
                    return true;
                });
        // Chạm lại tab đang chọn: về màn hình gốc của tab, không tải lại.
        binding.bnvDieuHuong.setOnItemReselectedListener(
                muc -> dieuHuong.popBackStack(muc.getItemId(), false));
    }

    /**
     * Phiên kết thúc (hết hạn, đăng xuất): về đăng nhập bằng action toàn cục, xóa ngăn xếp quay lại và
     * xóa trạng thái bốn tab đã lưu (FA 5.3 quy tắc 5, FA 3.5). Ở KhoiDongFragment và màn hình đăng nhập
     * không làm gì: KhoiDongFragment tự điều hướng theo kết quả của nó.
     */
    private void khiTrangThaiPhienDoi(TrangThaiPhien trangThai) {
        if (trangThai != TrangThaiPhien.KHONG_CO_PHIEN) {
            return;
        }
        NavDestination dich = dieuHuong.getCurrentDestination();
        if (dich == null
                || dich.getId() == R.id.khoiDongFragment
                || dich.getId() == R.id.dangNhapSinhVienFragment) {
            return;
        }
        dieuHuong.navigate(NavGraphDirections.actionGlobalToDangNhapSinhVienFragment());
        for (int tab : TAB_GOC) {
            dieuHuong.clearBackStack(tab);
        }
    }

    /** Bật hoặc ẩn thanh tiêu đề, thanh điều hướng dưới và nút Quay lại theo điểm đến (UI-Screens 4.1). */
    private void capNhatKhung(NavDestination dich) {
        int ma = dich.getId();
        boolean laManHinhChung = ma == R.id.khoiDongFragment || ma == R.id.dangNhapSinhVienFragment;
        boolean laTabGoc = laTabGoc(ma);

        int hienTieuDe = laManHinhChung ? View.GONE : View.VISIBLE;
        binding.tbManHinh.setVisibility(hienTieuDe);
        binding.vDuongKeDuoiTieuDe.setVisibility(hienTieuDe);
        binding.tbManHinh.setTitle(dich.getLabel());
        // Màn hình gốc không có nút Quay lại; màn hình mở chồng có.
        if (laTabGoc) {
            binding.tbManHinh.setNavigationIcon((Drawable) null);
        } else {
            binding.tbManHinh.setNavigationIcon(R.drawable.ic_arrow_back);
        }

        int hienDieuHuong = laTabGoc ? View.VISIBLE : View.GONE;
        binding.bnvDieuHuong.setVisibility(hienDieuHuong);
        binding.vDuongKeTrenDieuHuong.setVisibility(hienDieuHuong);
        if (laTabGoc) {
            binding.bnvDieuHuong.getMenu().findItem(ma).setChecked(true);
        }
    }

    private static boolean laTabGoc(int ma) {
        for (int tab : TAB_GOC) {
            if (tab == ma) {
                return true;
            }
        }
        return false;
    }
}
