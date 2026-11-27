package com.nhom13.quanlymuontra.common.widget;

import android.content.Context;
import android.util.AttributeSet;
import android.view.LayoutInflater;
import android.view.View;
import android.widget.LinearLayout;
import androidx.annotation.ColorRes;
import androidx.annotation.NonNull;
import androidx.annotation.Nullable;
import androidx.core.content.ContextCompat;
import com.nhom13.quanlymuontra.R;
import com.nhom13.quanlymuontra.databinding.ViewQuantityStepperBinding;

/**
 * CMP-10 Bộ chọn số lượng (DS CMP-10): nút giảm, giá trị, nút tăng. Giá trị là số nguyên trong [tối thiểu, tối đa]; ở
 * SCR-SV-04 tối thiểu là 0 (0 nghĩa là không đưa loại vào phiếu, hiển thị chữ phụ) và tối đa là {@code
 * soLuongKhaDung} do máy chủ trả. Nút giảm vô hiệu ở mức tối thiểu, nút tăng vô hiệu ở mức tối đa (ĐX-DS-11). Lớp này
 * chỉ kẹp giá trị vào khoảng được đặt; mọi giới hạn nghiệp vụ khác do máy chủ kiểm (UI-Screens 5.4).
 *
 * <p>{@link #datKhiDoi} chỉ được gọi khi <b>người dùng</b> đổi giá trị; {@link #datGiaTri} và {@link #datGioiHan} không
 * phát sự kiện, để bên gọi gán lại dữ liệu khi bind danh sách mà không tạo vòng lặp.
 */
public class QuantityStepperView extends LinearLayout {

    /** Độ trong suốt của nút khi vô hiệu (mức nhấn mạnh vô hiệu của Material; DS chưa nêu giá trị riêng). */
    private static final float DO_TRONG_SUOT_VO_HIEU = 0.38f;

    /** Bên nhận giá trị mới do người dùng chọn. */
    public interface KhiDoi {
        void khiDoi(int giaTri);
    }

    private final ViewQuantityStepperBinding binding;
    private int toiThieu = 0;
    private int toiDa = 0;
    private int giaTri = 0;
    @Nullable private KhiDoi khiDoi;

    public QuantityStepperView(@NonNull Context context) {
        this(context, null);
    }

    public QuantityStepperView(@NonNull Context context, @Nullable AttributeSet attrs) {
        this(context, attrs, 0);
    }

    public QuantityStepperView(
            @NonNull Context context, @Nullable AttributeSet attrs, int defStyleAttr) {
        super(context, attrs, defStyleAttr);
        setOrientation(HORIZONTAL);
        setGravity(android.view.Gravity.CENTER_VERTICAL);
        binding = ViewQuantityStepperBinding.inflate(LayoutInflater.from(context), this);
        binding.btnGiam.setOnClickListener(nut -> nguoiDungDoi(giaTri - 1));
        binding.btnTang.setOnClickListener(nut -> nguoiDungDoi(giaTri + 1));
        hienThi();
    }

    /** Đặt khoảng [{@code tối thiểu}, {@code tối đa}]; tối đa nhỏ hơn tối thiểu được nâng lên bằng tối thiểu. */
    public void datGioiHan(int toiThieuMoi, int toiDaMoi) {
        toiThieu = toiThieuMoi;
        toiDa = Math.max(toiThieuMoi, toiDaMoi);
        giaTri = kep(giaTri);
        hienThi();
    }

    /** Đặt giá trị (kẹp vào khoảng); không phát sự kiện. */
    public void datGiaTri(int giaTriMoi) {
        giaTri = kep(giaTriMoi);
        hienThi();
    }

    public int getGiaTri() {
        return giaTri;
    }

    public void datKhiDoi(@Nullable KhiDoi khiDoiMoi) {
        khiDoi = khiDoiMoi;
    }

    @Override
    public void setEnabled(boolean enabled) {
        super.setEnabled(enabled);
        hienThi();
    }

    private void nguoiDungDoi(int moi) {
        int ketQua = kep(moi);
        if (ketQua == giaTri) {
            return;
        }
        giaTri = ketQua;
        hienThi();
        if (khiDoi != null) {
            khiDoi.khiDoi(ketQua);
        }
    }

    private int kep(int x) {
        return Math.max(toiThieu, Math.min(toiDa, x));
    }

    private void hienThi() {
        binding.tvGiaTri.setText(String.valueOf(giaTri));
        // DS CMP-10: giá trị 0 hiển thị chữ phụ.
        @ColorRes int mau = giaTri == 0 ? R.color.ds_color_text_secondary : R.color.ds_color_text_primary;
        binding.tvGiaTri.setTextColor(ContextCompat.getColor(getContext(), mau));
        datNut(binding.btnGiam, isEnabled() && giaTri > toiThieu);
        datNut(binding.btnTang, isEnabled() && giaTri < toiDa);
    }

    private static void datNut(@NonNull View nut, boolean bat) {
        nut.setEnabled(bat);
        nut.setAlpha(bat ? 1f : DO_TRONG_SUOT_VO_HIEU);
    }
}
