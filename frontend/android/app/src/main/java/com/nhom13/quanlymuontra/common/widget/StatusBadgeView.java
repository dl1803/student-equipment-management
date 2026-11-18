package com.nhom13.quanlymuontra.common.widget;

import android.content.Context;
import android.content.res.ColorStateList;
import android.graphics.drawable.GradientDrawable;
import android.util.AttributeSet;
import android.view.Gravity;
import android.view.LayoutInflater;
import android.view.View;
import android.widget.LinearLayout;
import androidx.annotation.ColorRes;
import androidx.annotation.NonNull;
import androidx.annotation.Nullable;
import androidx.core.content.ContextCompat;
import androidx.core.widget.ImageViewCompat;
import com.nhom13.quanlymuontra.R;
import com.nhom13.quanlymuontra.common.nhan.HoTrangThai;
import com.nhom13.quanlymuontra.common.nhan.NhanPhatSinh;
import com.nhom13.quanlymuontra.common.nhan.NhanTrangThai;
import com.nhom13.quanlymuontra.common.nhan.ThongTinTrangThai;
import com.nhom13.quanlymuontra.common.nhan.Tong;
import com.nhom13.quanlymuontra.databinding.ViewStatusBadgeBinding;

/**
 * CMP-13 Nhãn trạng thái (DS 8): hiển thị trạng thái nghiệp vụ hoặc nhãn phát sinh bằng chữ, kèm
 * biểu tượng của tông (trừ tông trung tính và mã chưa biết), nên màu không phải kênh duy nhất
 * (NT-02). Nhãn nhạt: nền, viền, chữ và biểu tượng theo tông. Nhãn đậm: nền đặc, chữ đảo màu,
 * không viền. Mã chưa biết hiển thị nguyên mã bằng kiểu chữ mã, tông trung tính (DS 7.1). Chữ dài
 * tự xuống dòng, không cắt (NT-04).
 */
public class StatusBadgeView extends LinearLayout {

    private final ViewStatusBadgeBinding binding;

    public StatusBadgeView(@NonNull Context context) {
        this(context, null);
    }

    public StatusBadgeView(@NonNull Context context, @Nullable AttributeSet attrs) {
        this(context, attrs, 0);
    }

    public StatusBadgeView(
            @NonNull Context context, @Nullable AttributeSet attrs, int defStyleAttr) {
        super(context, attrs, defStyleAttr);
        setOrientation(HORIZONTAL);
        setGravity(Gravity.CENTER_VERTICAL);
        int ngang = getResources().getDimensionPixelSize(R.dimen.ds_space_2);
        int doc = getResources().getDimensionPixelSize(R.dimen.ds_space_1);
        setPadding(ngang, doc, ngang, doc);
        setMinimumHeight(getResources().getDimensionPixelSize(R.dimen.ds_size_badge_height));
        binding = ViewStatusBadgeBinding.inflate(LayoutInflater.from(context), this);
    }

    /** Hiển thị trạng thái của mã API {@code ma} trong họ {@code ho}; mã lạ hiển thị nguyên mã. */
    public void setTrangThai(@NonNull HoTrangThai ho, @Nullable String ma) {
        apDung(NhanTrangThai.thongTin(ho, ma));
    }

    /** Hiển thị nhãn phát sinh (Hết hàng, Hợp lệ, Tạm tính...). */
    public void setNhanPhatSinh(@NonNull NhanPhatSinh nhan) {
        apDung(nhan.thongTin());
    }

    private void apDung(@NonNull ThongTinTrangThai thongTin) {
        Tong tong = thongTin.getTong();
        boolean dam = thongTin.isNhanDam();

        // setTextAppearance đặt lại màu chữ nên phải gọi trước setTextColor.
        if (thongTin.isMaLa()) {
            binding.tvNhan.setTextAppearance(R.style.TextAppearance_Ds_Code);
            binding.tvNhan.setText(thongTin.getMaGoc());
        } else {
            binding.tvNhan.setTextAppearance(R.style.TextAppearance_Ds_LabelSm);
            binding.tvNhan.setText(thongTin.getNhanResId());
        }
        binding.tvNhan.setTextColor(mau(dam ? GiaoDienTong.chuTrenNenDac(tong) : GiaoDienTong.chu(tong)));

        int hinh = GiaoDienTong.hinhBieuTuong(tong);
        if (thongTin.isMaLa() || hinh == 0) {
            binding.imgBieuTuong.setVisibility(View.GONE);
        } else {
            binding.imgBieuTuong.setImageResource(hinh);
            ImageViewCompat.setImageTintList(
                    binding.imgBieuTuong,
                    ColorStateList.valueOf(
                            mau(dam ? GiaoDienTong.chuTrenNenDac(tong) : GiaoDienTong.bieuTuong(tong))));
            binding.imgBieuTuong.setVisibility(View.VISIBLE);
        }

        GradientDrawable nen = new GradientDrawable();
        nen.setShape(GradientDrawable.RECTANGLE);
        // Bán kính lớn hơn nửa chiều cao được GradientDrawable giới hạn nên thành hình viên thuốc.
        nen.setCornerRadius(getResources().getDimension(R.dimen.ds_radius_full));
        nen.setColor(mau(dam ? GiaoDienTong.dac(tong) : GiaoDienTong.nen(tong)));
        if (!dam) {
            nen.setStroke(
                    getResources().getDimensionPixelSize(R.dimen.ds_border_width_default),
                    mau(GiaoDienTong.vien(tong)));
        }
        setBackground(nen);
    }

    private int mau(@ColorRes int mauResId) {
        return ContextCompat.getColor(getContext(), mauResId);
    }
}
