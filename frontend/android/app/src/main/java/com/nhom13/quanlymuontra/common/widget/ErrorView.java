package com.nhom13.quanlymuontra.common.widget;

import android.content.Context;
import android.util.AttributeSet;
import androidx.annotation.NonNull;
import androidx.annotation.Nullable;
import com.nhom13.quanlymuontra.R;
import com.nhom13.quanlymuontra.data.model.ApiError;

/**
 * CMP-25, kiểu Lỗi (DS 8, 9.4, 9.8): lỗi tải dữ liệu của một vùng, kèm nút "Thử lại". Nội dung chọn
 * theo {@link TrinhBayLoi}: mất kết nối hoặc quá thời gian chờ là "lỗi mạng"; mọi lỗi khác không có
 * xử lý riêng là "lỗi hệ thống" kèm mã theo dõi (traceId) để người dùng đọc cho nhân viên hỗ trợ.
 * Mỗi vùng dùng một ErrorView riêng nên lỗi một vùng không che các vùng khác (ĐX-UI-04).
 */
public class ErrorView extends StateViewBase {

    public ErrorView(@NonNull Context context) {
        this(context, null);
    }

    public ErrorView(@NonNull Context context, @Nullable AttributeSet attrs) {
        this(context, attrs, 0);
    }

    public ErrorView(@NonNull Context context, @Nullable AttributeSet attrs, int defStyleAttr) {
        super(context, attrs, defStyleAttr);
    }

    /**
     * Hiển thị lỗi của vùng tải dữ liệu (truy vấn). {@link ApiError.Loai#HUY} là truy vấn bị hủy
     * chủ động nên view tự ẩn. Không dùng cho thao tác ghi: {@code HUY} sau khi thao tác ghi đã phát
     * nghĩa là chưa biết kết quả và phải xử lý theo FA 3.3 quy tắc 3, không được ẩn đi.
     *
     * @param thuLai xử lý khi bấm "Thử lại"; {@code null} thì không hiện nút
     */
    public void hienThi(@NonNull ApiError loi, @Nullable OnClickListener thuLai) {
        TrinhBayLoi.NoiDung noiDung = TrinhBayLoi.choVung(loi);
        if (noiDung == null) {
            setVisibility(GONE);
            return;
        }
        if (noiDung.kieu == TrinhBayLoi.Kieu.MANG) {
            hienBieuTuong(R.drawable.ic_cloud_off, R.color.ds_color_tone_neutral_icon);
        } else {
            hienBieuTuong(R.drawable.ic_error, R.color.ds_color_tone_danger_icon);
        }
        datThongDiep(getContext().getString(noiDung.thongDiepResId));
        datMaTheoDoi(noiDung.traceId);
        datHanhDong(thuLai == null ? null : getContext().getString(R.string.hanh_dong_thu_lai), thuLai);
        setVisibility(VISIBLE);
    }
}
