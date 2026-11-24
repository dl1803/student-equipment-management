package com.nhom13.quanlymuontra.common.widget;

import android.content.Context;
import android.content.res.ColorStateList;
import android.util.AttributeSet;
import android.view.Gravity;
import android.view.LayoutInflater;
import android.view.View;
import android.widget.LinearLayout;
import androidx.annotation.ColorRes;
import androidx.annotation.DrawableRes;
import androidx.annotation.NonNull;
import androidx.annotation.Nullable;
import androidx.core.content.ContextCompat;
import androidx.core.widget.ImageViewCompat;
import com.nhom13.quanlymuontra.R;
import com.nhom13.quanlymuontra.databinding.ViewStateBinding;

/**
 * Nền chung của CMP-25 Trạng thái màn hình (DS 8): khối canh giữa gồm chỉ báo quay hoặc biểu tượng,
 * một câu thông điệp, dòng mã theo dõi tùy chọn và một hành động tùy chọn. Ba lớp công khai {@link
 * LoadingView}, {@link EmptyView}, {@link ErrorView} dùng chung bố cục {@code view_state.xml} và chỉ
 * khác nội dung. Trạng thái chỉ ảnh hưởng vùng chứa nó, không chiếm cả màn hình (ĐX-UI-04).
 */
abstract class StateViewBase extends LinearLayout {

    protected final ViewStateBinding binding;

    StateViewBase(@NonNull Context context, @Nullable AttributeSet attrs, int defStyleAttr) {
        super(context, attrs, defStyleAttr);
        setOrientation(VERTICAL);
        setGravity(Gravity.CENTER_HORIZONTAL);
        int doc = getResources().getDimensionPixelSize(R.dimen.ds_space_state);
        setPadding(0, doc, 0, doc);
        binding = ViewStateBinding.inflate(LayoutInflater.from(context), this);
    }

    protected void hienChiBaoQuay(boolean hien) {
        binding.piDangTai.setVisibility(hien ? View.VISIBLE : View.GONE);
    }

    protected void hienBieuTuong(@DrawableRes int hinh, @ColorRes int mau) {
        if (hinh == 0) {
            binding.imgBieuTuong.setVisibility(View.GONE);
            return;
        }
        binding.imgBieuTuong.setImageResource(hinh);
        ImageViewCompat.setImageTintList(
                binding.imgBieuTuong,
                ColorStateList.valueOf(ContextCompat.getColor(getContext(), mau)));
        binding.imgBieuTuong.setVisibility(View.VISIBLE);
    }

    protected void datThongDiep(@Nullable CharSequence thongDiep) {
        if (thongDiep == null || thongDiep.length() == 0) {
            binding.tvThongDiep.setVisibility(View.GONE);
        } else {
            binding.tvThongDiep.setText(thongDiep);
            binding.tvThongDiep.setVisibility(View.VISIBLE);
        }
    }

    protected void datMaTheoDoi(@Nullable String traceId) {
        if (traceId == null || traceId.isEmpty()) {
            binding.tvMaTheoDoi.setVisibility(View.GONE);
        } else {
            binding.tvMaTheoDoi.setText(getContext().getString(R.string.trang_thai_ma_theo_doi, traceId));
            binding.tvMaTheoDoi.setVisibility(View.VISIBLE);
        }
    }

    protected void datHanhDong(@Nullable CharSequence nhan, @Nullable OnClickListener khiNhan) {
        if (nhan == null || nhan.length() == 0) {
            binding.btnHanhDong.setOnClickListener(null);
            binding.btnHanhDong.setVisibility(View.GONE);
        } else {
            binding.btnHanhDong.setText(nhan);
            binding.btnHanhDong.setOnClickListener(khiNhan);
            binding.btnHanhDong.setVisibility(View.VISIBLE);
        }
    }
}
