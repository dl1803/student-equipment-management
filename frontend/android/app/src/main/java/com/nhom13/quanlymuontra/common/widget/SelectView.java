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
import com.nhom13.quanlymuontra.common.HangSo;
import com.nhom13.quanlymuontra.data.model.ApiError;
import com.nhom13.quanlymuontra.databinding.ViewSelectBinding;

/**
 * CMP-06 Ô chọn một (DS CMP-06): nhãn phía trên, ô bấm hiển thị giá trị đang chọn kèm {@code expand_more}.
 * Danh sách lựa chọn mở bằng {@link ChonMotDialogFragment} do màn hình gọi trong {@link #datKhiMo}. Ba trạng
 * thái: giá trị (bấm được), đang tải (vô hiệu) và lỗi tải lựa chọn kèm "Thử lại" (DS CMP-06 "Lỗi tải lựa
 * chọn"); lỗi chỉ ảnh hưởng ô này, các điều khiển khác vẫn dùng được (UI-Screens 5.2). Nội dung lỗi chọn
 * bằng {@link TrinhBayLoi} như {@link ErrorView}.
 */
public class SelectView extends LinearLayout {

    private final ViewSelectBinding binding;

    public SelectView(@NonNull Context context) {
        this(context, null);
    }

    public SelectView(@NonNull Context context, @Nullable AttributeSet attrs) {
        this(context, attrs, 0);
    }

    public SelectView(@NonNull Context context, @Nullable AttributeSet attrs, int defStyleAttr) {
        super(context, attrs, defStyleAttr);
        setOrientation(VERTICAL);
        binding = ViewSelectBinding.inflate(LayoutInflater.from(context), this);
    }

    /** Nhãn nhìn thấy phía trên ô (không thay bằng gợi ý, UI-Screens 3.8). */
    public void datNhan(@NonNull CharSequence nhan) {
        binding.tvNhan.setText(nhan);
    }

    /** Xử lý khi bấm ô; chỉ có tác dụng khi ô đang ở trạng thái giá trị. */
    public void datKhiMo(@Nullable OnClickListener khiMo) {
        binding.llOChon.setOnClickListener(khiMo);
    }

    /** Trạng thái bình thường: hiển thị giá trị đang chọn (hoặc "Tất cả"), bấm được. */
    public void hienGiaTri(@NonNull CharSequence giaTri) {
        apDung(giaTri, R.color.ds_color_text_primary, true, false);
        binding.llLoi.setVisibility(View.GONE);
    }

    /** Đang tải lựa chọn: ô vô hiệu, chữ "Đang tải". */
    public void hienDangTai() {
        apDung(
                getContext().getString(R.string.trang_thai_dang_tai),
                R.color.ds_color_text_secondary,
                false,
                false);
        binding.llLoi.setVisibility(View.GONE);
    }

    /**
     * Lỗi tải lựa chọn: ô vô hiệu viền lỗi, thông điệp lỗi mạng hoặc lỗi hệ thống (kèm mã theo dõi) và nút Thử
     * lại. {@link ApiError.Loai#HUY} không hiển thị gì (truy vấn bị hủy chủ động, FA 3.3 quy tắc 4).
     *
     * @param thuLai xử lý khi bấm "Thử lại"; {@code null} thì nút không làm gì
     */
    public void hienLoi(@NonNull ApiError loi, @Nullable OnClickListener thuLai) {
        TrinhBayLoi.NoiDung noiDung = TrinhBayLoi.choVung(loi);
        if (noiDung == null) {
            return;
        }
        apDung(HangSo.GIA_TRI_THIEU, R.color.ds_color_text_secondary, false, true);
        binding.tvThongDiepLoi.setText(noiDung.thongDiepResId);
        if (noiDung.traceId == null) {
            binding.tvMaTheoDoi.setVisibility(View.GONE);
        } else {
            binding.tvMaTheoDoi.setText(
                    getContext().getString(R.string.trang_thai_ma_theo_doi, noiDung.traceId));
            binding.tvMaTheoDoi.setVisibility(View.VISIBLE);
        }
        binding.btnThuLai.setOnClickListener(thuLai);
        binding.llLoi.setVisibility(View.VISIBLE);
    }

    private void apDung(
            @NonNull CharSequence chu, @ColorRes int mauChu, boolean choPhepMo, boolean laLoi) {
        binding.tvGiaTri.setText(chu);
        binding.tvGiaTri.setTextColor(ContextCompat.getColor(getContext(), mauChu));
        binding.llOChon.setEnabled(choPhepMo);
        binding.llOChon.setActivated(laLoi);
    }
}
