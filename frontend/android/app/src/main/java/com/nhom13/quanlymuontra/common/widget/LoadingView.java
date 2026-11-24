package com.nhom13.quanlymuontra.common.widget;

import android.content.Context;
import android.util.AttributeSet;
import android.view.View;
import androidx.annotation.NonNull;
import androidx.annotation.Nullable;
import com.nhom13.quanlymuontra.R;
import com.nhom13.quanlymuontra.common.HangSo;

/**
 * CMP-25, kiểu Đang tải (DS 8, 9.3): chỉ báo quay trong vùng đang nạp dữ liệu. Nếu vẫn còn hiển thị
 * sau {@link HangSo#NGUONG_THONG_BAO_KHOI_DONG_GIAY} giây thì hiện thêm dòng "Hệ thống đang khởi
 * động, vui lòng chờ" vì máy chủ có thể đang thức dậy (ĐX-FA-04). Bộ đếm chỉ chạy khi view đang hiển
 * thị trên màn hình và tự đặt lại mỗi lần hiện lại. Hiện hoặc ẩn bằng {@code setVisibility}.
 */
public class LoadingView extends StateViewBase {

    private final Runnable hienThongBaoKhoiDong =
            () -> datThongDiep(getContext().getString(R.string.trang_thai_khoi_dong_cham));

    public LoadingView(@NonNull Context context) {
        this(context, null);
    }

    public LoadingView(@NonNull Context context, @Nullable AttributeSet attrs) {
        this(context, attrs, 0);
    }

    public LoadingView(@NonNull Context context, @Nullable AttributeSet attrs, int defStyleAttr) {
        super(context, attrs, defStyleAttr);
        hienChiBaoQuay(true);
        binding.piDangTai.setContentDescription(context.getString(R.string.trang_thai_dang_tai));
    }

    @Override
    protected void onAttachedToWindow() {
        super.onAttachedToWindow();
        capNhatBoDem();
    }

    @Override
    protected void onDetachedFromWindow() {
        removeCallbacks(hienThongBaoKhoiDong);
        super.onDetachedFromWindow();
    }

    @Override
    protected void onVisibilityChanged(@NonNull View changedView, int visibility) {
        super.onVisibilityChanged(changedView, visibility);
        capNhatBoDem();
    }

    private void capNhatBoDem() {
        // onVisibilityChanged có thể được gọi trong hàm khởi tạo của View, lúc trường chưa gán.
        if (hienThongBaoKhoiDong == null) {
            return;
        }
        removeCallbacks(hienThongBaoKhoiDong);
        datThongDiep(null);
        if (isAttachedToWindow() && isShown()) {
            postDelayed(hienThongBaoKhoiDong, HangSo.NGUONG_THONG_BAO_KHOI_DONG_GIAY * 1000L);
        }
    }
}
