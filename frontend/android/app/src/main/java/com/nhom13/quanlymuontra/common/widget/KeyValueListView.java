package com.nhom13.quanlymuontra.common.widget;

import android.content.Context;
import android.util.AttributeSet;
import android.view.LayoutInflater;
import android.view.View;
import android.widget.LinearLayout;
import androidx.annotation.NonNull;
import androidx.annotation.Nullable;
import com.nhom13.quanlymuontra.R;
import com.nhom13.quanlymuontra.common.HangSo;
import com.nhom13.quanlymuontra.databinding.ViewKeyValueRowBinding;

/**
 * CMP-14 Danh sách khóa và giá trị (DS 8): mỗi dòng gồm tên trường (chú thích, màu phụ) phía trên và
 * giá trị phía dưới, xếp dọc nên vừa mọi chiều rộng và chữ dài tự xuống dòng (NT-04). Giá trị thiếu
 * hiển thị dấu gạch dài (ĐX-DS-12). Giá trị là trạng thái thì truyền {@link StatusBadgeView} vào
 * {@link #themDongView}. Khoảng cách giữa các dòng lấy từ token danh sách.
 */
public class KeyValueListView extends LinearLayout {

    public KeyValueListView(@NonNull Context context) {
        this(context, null);
    }

    public KeyValueListView(@NonNull Context context, @Nullable AttributeSet attrs) {
        this(context, attrs, 0);
    }

    public KeyValueListView(
            @NonNull Context context, @Nullable AttributeSet attrs, int defStyleAttr) {
        super(context, attrs, defStyleAttr);
        setOrientation(VERTICAL);
    }

    /** Xóa mọi dòng để nạp lại dữ liệu. */
    public void xoa() {
        removeAllViews();
    }

    /** Thêm dòng có giá trị chữ thường; {@code null} hoặc rỗng hiển thị dấu gạch dài. */
    public void themDong(@NonNull CharSequence ten, @Nullable CharSequence giaTri) {
        ViewKeyValueRowBinding dong = taoDong(ten);
        dong.tvGiaTri.setText(coChu(giaTri) ? giaTri : HangSo.GIA_TRI_THIEU);
        dong.tvGiaTri.setVisibility(View.VISIBLE);
    }

    /**
     * Thêm dòng có giá trị là mã (mã phiếu, mã thiết bị...): dùng kiểu chữ mã và không xuống dòng
     * (DS 9.7).
     */
    public void themDongMa(@NonNull CharSequence ten, @Nullable CharSequence ma) {
        ViewKeyValueRowBinding dong = taoDong(ten);
        dong.tvGiaTri.setTextAppearance(R.style.TextAppearance_Ds_Code);
        dong.tvGiaTri.setMaxLines(1);
        dong.tvGiaTri.setText(coChu(ma) ? ma : HangSo.GIA_TRI_THIEU);
        dong.tvGiaTri.setVisibility(View.VISIBLE);
    }

    /** Thêm dòng có giá trị là một view, ví dụ {@link StatusBadgeView}. */
    public void themDongView(@NonNull CharSequence ten, @NonNull View giaTri) {
        ViewKeyValueRowBinding dong = taoDong(ten);
        dong.flGiaTri.addView(giaTri);
        dong.flGiaTri.setVisibility(View.VISIBLE);
    }

    @NonNull
    private ViewKeyValueRowBinding taoDong(@NonNull CharSequence ten) {
        ViewKeyValueRowBinding dong =
                ViewKeyValueRowBinding.inflate(LayoutInflater.from(getContext()), this, false);
        dong.tvTen.setText(ten);
        LayoutParams thamSo = (LayoutParams) dong.getRoot().getLayoutParams();
        thamSo.topMargin =
                getChildCount() == 0
                        ? 0
                        : getResources().getDimensionPixelSize(R.dimen.ds_space_3);
        addView(dong.getRoot(), thamSo);
        return dong;
    }

    private static boolean coChu(@Nullable CharSequence chu) {
        return chu != null && chu.length() > 0;
    }
}
