package com.nhom13.quanlymuontra.common.widget;

import android.content.Context;
import android.util.AttributeSet;
import androidx.annotation.DrawableRes;
import androidx.annotation.NonNull;
import androidx.annotation.Nullable;
import com.nhom13.quanlymuontra.R;

/**
 * CMP-25, kiểu Rỗng (DS 8, 9.3): vùng không có dữ liệu để hiển thị. Thông điệp do màn hình đặt vì
 * chữ phụ thuộc ngữ cảnh (ví dụ "Bạn chưa mượn thiết bị nào."); biểu tượng mặc định là hộp thư
 * đến, màn hình có thể đổi bằng {@link #datBieuTuong}. Chữ do màn hình cung cấp là chữ đã dựng
 * sẵn trong {@code strings.xml}, không ghép từ mã (FC 6.6).
 */
public class EmptyView extends StateViewBase {

    public EmptyView(@NonNull Context context) {
        this(context, null);
    }

    public EmptyView(@NonNull Context context, @Nullable AttributeSet attrs) {
        this(context, attrs, 0);
    }

    public EmptyView(@NonNull Context context, @Nullable AttributeSet attrs, int defStyleAttr) {
        super(context, attrs, defStyleAttr);
        datBieuTuong(R.drawable.ic_inbox);
    }

    /** Đổi biểu tượng; nhãn trung tính vì rỗng không phải lỗi (DS 9.3). */
    public void datBieuTuong(@DrawableRes int hinh) {
        hienBieuTuong(hinh, R.color.ds_color_tone_neutral_icon);
    }

    /**
     * Hiển thị trạng thái rỗng.
     *
     * @param thongDiep một câu mô tả vì sao rỗng
     * @param nhanHanhDong chữ của hành động tùy chọn (nút phụ); {@code null} thì không có nút
     */
    public void hienThi(
            @NonNull CharSequence thongDiep,
            @Nullable CharSequence nhanHanhDong,
            @Nullable OnClickListener khiNhan) {
        datThongDiep(thongDiep);
        datMaTheoDoi(null);
        datHanhDong(nhanHanhDong, khiNhan);
        setVisibility(VISIBLE);
    }
}
