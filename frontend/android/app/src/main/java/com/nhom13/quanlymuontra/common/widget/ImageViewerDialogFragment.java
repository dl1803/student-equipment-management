package com.nhom13.quanlymuontra.common.widget;

import android.app.Dialog;
import android.graphics.Color;
import android.graphics.drawable.ColorDrawable;
import android.os.Bundle;
import android.view.LayoutInflater;
import android.view.View;
import android.view.ViewGroup;
import android.view.Window;
import android.view.WindowManager;
import androidx.annotation.NonNull;
import androidx.annotation.Nullable;
import androidx.fragment.app.DialogFragment;
import com.bumptech.glide.Glide;
import com.nhom13.quanlymuontra.R;
import com.nhom13.quanlymuontra.databinding.DialogXemAnhBinding;
import java.util.Objects;

/**
 * CMP-23 lớp xem ảnh lớn (DS CMP-23): phủ {@code color.overlay.scrim}, ảnh vừa khung nhìn, {@code shadow.dialog},
 * nút Đóng ở góc trên phải. Đóng bằng nút Đóng, chạm ra ngoài ảnh hoặc nút Quay lại hệ thống. Tên truy cập của ảnh là
 * "Ảnh <số thứ tự> của <tên thiết bị>". Ảnh tải bằng Glide, bộ nhớ đệm mặc định (ảnh danh mục là URL công khai);
 * không ghi URL vào nhật ký. Hiển thị bằng {@code tao(...).show(getChildFragmentManager(), TAG)} của màn hình.
 */
public class ImageViewerDialogFragment extends DialogFragment {

    private static final String ARG_URL = "url";
    private static final String ARG_SO_THU_TU = "soThuTu";
    private static final String ARG_TEN_THIET_BI = "tenThietBi";

    private DialogXemAnhBinding _binding;

    /**
     * @param url URL ảnh cần xem lớn
     * @param soThuTu số thứ tự của ảnh, tính từ 1, dùng cho tên truy cập
     * @param tenThietBi tên thiết bị, dùng cho tên truy cập
     */
    @NonNull
    public static ImageViewerDialogFragment tao(
            @NonNull String url, int soThuTu, @NonNull String tenThietBi) {
        Bundle thamSo = new Bundle();
        thamSo.putString(ARG_URL, url);
        thamSo.putInt(ARG_SO_THU_TU, soThuTu);
        thamSo.putString(ARG_TEN_THIET_BI, tenThietBi);
        ImageViewerDialogFragment hop = new ImageViewerDialogFragment();
        hop.setArguments(thamSo);
        return hop;
    }

    @NonNull
    @Override
    public Dialog onCreateDialog(@Nullable Bundle savedInstanceState) {
        Dialog hop = super.onCreateDialog(savedInstanceState);
        hop.setCanceledOnTouchOutside(true);
        return hop;
    }

    @NonNull
    private DialogXemAnhBinding binding() {
        return Objects.requireNonNull(_binding);
    }

    @Nullable
    @Override
    public View onCreateView(
            @NonNull LayoutInflater inflater,
            @Nullable ViewGroup container,
            @Nullable Bundle savedInstanceState) {
        _binding = DialogXemAnhBinding.inflate(inflater, container, false);
        return _binding.getRoot();
    }

    @Override
    public void onViewCreated(@NonNull View view, @Nullable Bundle savedInstanceState) {
        super.onViewCreated(view, savedInstanceState);
        Bundle thamSo = requireArguments();
        String url = Objects.requireNonNull(thamSo.getString(ARG_URL));
        int soThuTu = thamSo.getInt(ARG_SO_THU_TU);
        String tenThietBi = Objects.requireNonNull(thamSo.getString(ARG_TEN_THIET_BI));

        // Ảnh vừa khung nhìn: chiều cao tối đa là chiều cao màn hình trừ lề ds_space_6 hai phía.
        int le = getResources().getDimensionPixelSize(R.dimen.ds_space_6);
        binding().imgAnhLon.setMaxHeight(getResources().getDisplayMetrics().heightPixels - 2 * le);
        binding().imgAnhLon.setContentDescription(
                getString(R.string.anh_mo_ta_o_anh, soThuTu, tenThietBi));
        Glide.with(this).load(url).fitCenter().error(R.drawable.ic_broken_image).into(binding().imgAnhLon);

        binding().flNen.setOnClickListener(nut -> dismiss());
        binding().btnDong.setOnClickListener(nut -> dismiss());
    }

    @Override
    public void onStart() {
        super.onStart();
        Dialog hop = getDialog();
        if (hop == null || hop.getWindow() == null) {
            return;
        }
        // Lớp nền phủ do dialog_xem_anh.xml vẽ bằng color.overlay.scrim nên bỏ nền và độ mờ mặc định.
        Window cuaSo = hop.getWindow();
        cuaSo.setBackgroundDrawable(new ColorDrawable(Color.TRANSPARENT));
        cuaSo.clearFlags(WindowManager.LayoutParams.FLAG_DIM_BEHIND);
        cuaSo.setLayout(ViewGroup.LayoutParams.MATCH_PARENT, ViewGroup.LayoutParams.MATCH_PARENT);
    }

    @Override
    public void onDestroyView() {
        _binding = null;
        super.onDestroyView();
    }
}
