package com.nhom13.quanlymuontra.ui.tracuu;

import androidx.annotation.NonNull;
import androidx.annotation.Nullable;
import androidx.annotation.StringRes;
import com.nhom13.quanlymuontra.R;
import com.nhom13.quanlymuontra.common.format.FormatThoiGian;
import com.nhom13.quanlymuontra.data.model.ApiError;
import com.nhom13.quanlymuontra.generated.model.AnhRef;
import com.nhom13.quanlymuontra.ui.tracuu.ChiTietLoaiThietBiViewModel.HanhDong;
import java.time.OffsetDateTime;
import java.util.ArrayList;
import java.util.List;

/**
 * Quy tắc trình bày của SCR-SV-03 (UI-Screens 5.3). Hàm thuần, không giữ trạng thái, kiểm thử được trên JVM. Chỉ chọn
 * theo giá trị máy chủ trả về; client không tự tính còn hay hết hàng (FA 3.10).
 */
public final class ChiTietLoaiTrinhBay {

    private ChiTietLoaiTrinhBay() {}

    /**
     * Nút ghim theo số lượng khả dụng: lớn hơn 0 là Đặt trước, bằng 0 là Đăng ký chờ (UI-Screens 5.3).
     *
     * @return {@code null} khi số lượng thiếu hoặc âm (không có nút để hiển thị)
     */
    @Nullable
    public static HanhDong hanhDongChoSoLuong(@Nullable Integer soLuongKhaDung) {
        if (soLuongKhaDung == null || soLuongKhaDung < 0) {
            return null;
        }
        return soLuongKhaDung > 0 ? HanhDong.DAT_TRUOC : HanhDong.DANG_KY_CHO;
    }

    /** Chữ trên nút ghim theo hành động. */
    @StringRes
    public static int nhanNut(@NonNull HanhDong hanhDong) {
        return hanhDong == HanhDong.DAT_TRUOC
                ? R.string.chi_tiet_hanh_dong_dat_truoc
                : R.string.chi_tiet_hanh_dong_dang_ky_cho;
    }

    /** {@code true} khi máy chủ trả số lượng khả dụng bằng 0: hiện nhãn Hết hàng và ngày dự kiến. */
    public static boolean hetHang(@Nullable Integer soLuongKhaDung) {
        return soLuongKhaDung != null && soLuongKhaDung == 0;
    }

    /** {@code true} khi chuỗi có nội dung (không null, không chỉ gồm khoảng trắng): ẩn khối khi không có. */
    public static boolean coChu(@Nullable String chu) {
        return chu != null && !chu.trim().isEmpty();
    }

    /**
     * Ngày dự kiến có sẵn theo giờ Việt Nam (DS 9.7); không có dữ liệu thì dùng {@code chuaCoNgay} (UI-Screens 5.3).
     */
    @NonNull
    public static String ngayDuKien(@Nullable OffsetDateTime ngay, @NonNull String chuaCoNgay) {
        return ngay == null ? chuaCoNgay : FormatThoiGian.ngayGio(ngay);
    }

    /** URL các ảnh theo đúng thứ tự máy chủ trả; bỏ phần tử thiếu URL. */
    @NonNull
    public static List<String> dsUrlAnh(@Nullable List<AnhRef> dsAnh) {
        List<String> ketQua = new ArrayList<>();
        if (dsAnh == null) {
            return ketQua;
        }
        for (AnhRef anh : dsAnh) {
            Object url = anh == null ? null : anh.getUrl();
            if (url == null) {
                continue;
            }
            String chu = url.toString();
            if (!chu.isEmpty()) {
                ketQua.add(chu);
            }
        }
        return ketQua;
    }

    /** Các dòng thông số có nội dung, giữ thứ tự; rỗng thì ẩn khối thông số (UI-Screens 5.3). */
    @NonNull
    public static List<String> dsThongSo(@Nullable List<String> thongSo) {
        List<String> ketQua = new ArrayList<>();
        if (thongSo == null) {
            return ketQua;
        }
        for (String dong : thongSo) {
            if (coChu(dong)) {
                ketQua.add(dong);
            }
        }
        return ketQua;
    }

    /** {@code true} khi lỗi tải chi tiết là 404 {@code KHONG_TIM_THAY} (UI-Screens 3.2). */
    public static boolean laKhongTimThay(@NonNull ApiError loi) {
        return loi.getLoai() == ApiError.Loai.HTTP
                && loi.getStatus() != null
                && loi.getStatus() == 404;
    }

    /** Thông điệp của khối thông báo khi kiểm tra quyền mượn lỗi: mất kết nối hoặc quá thời gian chờ, còn lại là hệ thống. */
    @StringRes
    public static int thongDiepLoiQuyenMuon(@NonNull ApiError loi) {
        switch (loi.getLoai()) {
            case MANG:
            case QUA_THOI_GIAN:
                return R.string.loi_mang;
            case HTTP:
            case HUY:
            default:
                return R.string.loi_he_thong;
        }
    }

    /** Mã theo dõi ({@code traceId}) chỉ khi lỗi có phản hồi HTTP và có {@code traceId}; không thì {@code null}. */
    @Nullable
    public static String maTheoDoi(@NonNull ApiError loi) {
        if (loi.getLoai() != ApiError.Loai.HTTP) {
            return null;
        }
        String traceId = loi.getTraceId();
        return traceId == null || traceId.isEmpty() ? null : traceId;
    }
}
