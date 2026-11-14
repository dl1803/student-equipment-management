package com.nhom13.quanlymuontra.data.model;

import androidx.annotation.NonNull;
import androidx.annotation.Nullable;
import java.util.ArrayList;
import java.util.Collections;
import java.util.LinkedHashMap;
import java.util.List;
import java.util.Map;

/**
 * Lỗi đã chuẩn hóa mà mọi lớp phía trên {@code data/} nhận thay cho phản hồi HTTP thô (FA 3.3).
 *
 * <p>Đối tượng bất biến, chỉ tạo bằng các phương thức tĩnh. {@code maLoi} giữ nguyên chuỗi máy chủ
 * trả, kể cả mã chưa biết (FA 2.2, ĐX-FC-21). Việc phân nhánh dựa vào {@code maLoi}, {@code status}
 * và {@code loai}; không đọc {@code detail} để phân nhánh (ĐX-CV-02).
 */
public final class ApiError {

    /** Loại lỗi theo FA 3.3. */
    public enum Loai {
        /** Có phản hồi HTTP. */
        HTTP,
        /** Mất kết nối. */
        MANG,
        /** Quá thời gian chờ (ĐX-FA-04). */
        QUA_THOI_GIAN,
        /** Yêu cầu bị hủy chủ động. */
        HUY
    }

    @NonNull private final Loai loai;
    @Nullable private final Integer status;
    @Nullable private final String maLoi;
    @Nullable private final String detail;
    @Nullable private final String traceId;
    @NonNull private final Map<String, List<String>> errors;

    private ApiError(
            @NonNull Loai loai,
            @Nullable Integer status,
            @Nullable String maLoi,
            @Nullable String detail,
            @Nullable String traceId,
            @NonNull Map<String, List<String>> errors) {
        this.loai = loai;
        this.status = status;
        this.maLoi = maLoi;
        this.detail = detail;
        this.traceId = traceId;
        this.errors = errors;
    }

    /** Lỗi có phản hồi HTTP; {@code errors} lấy từ {@code ValidationProblemDetails} nếu có. */
    @NonNull
    public static ApiError http(
            int status,
            @Nullable String maLoi,
            @Nullable String detail,
            @Nullable String traceId,
            @Nullable Map<String, List<String>> errors) {
        return new ApiError(Loai.HTTP, status, maLoi, detail, traceId, saoChepBatBien(errors));
    }

    /** Mất kết nối. */
    @NonNull
    public static ApiError mang() {
        return new ApiError(Loai.MANG, null, null, null, null, Collections.emptyMap());
    }

    /** Quá thời gian chờ. */
    @NonNull
    public static ApiError quaThoiGian() {
        return new ApiError(Loai.QUA_THOI_GIAN, null, null, null, null, Collections.emptyMap());
    }

    /** Yêu cầu bị hủy chủ động. */
    @NonNull
    public static ApiError huy() {
        return new ApiError(Loai.HUY, null, null, null, null, Collections.emptyMap());
    }

    @NonNull
    public Loai getLoai() {
        return loai;
    }

    /** Mã HTTP; {@code null} khi {@link Loai} khác {@link Loai#HTTP}. */
    @Nullable
    public Integer getStatus() {
        return status;
    }

    /** Mã lỗi nghiệp vụ nguyên văn; {@code null} khi phản hồi không có {@code maLoi}. */
    @Nullable
    public String getMaLoi() {
        return maLoi;
    }

    @Nullable
    public String getDetail() {
        return detail;
    }

    @Nullable
    public String getTraceId() {
        return traceId;
    }

    /** Lỗi theo trường (khóa dạng {@code dsLoaiVaSoLuong[0].soLuong}); rỗng nếu không có. */
    @NonNull
    public Map<String, List<String>> getErrors() {
        return errors;
    }

    // Sao chép để lỗi bất biến: người gọi sửa bản đồ gốc sau đó không làm đổi ApiError.
    @NonNull
    private static Map<String, List<String>> saoChepBatBien(
            @Nullable Map<String, List<String>> nguon) {
        if (nguon == null || nguon.isEmpty()) {
            return Collections.emptyMap();
        }
        Map<String, List<String>> ketQua = new LinkedHashMap<>();
        for (Map.Entry<String, List<String>> muc : nguon.entrySet()) {
            List<String> thongBao = muc.getValue();
            ketQua.put(
                    muc.getKey(),
                    thongBao == null
                            ? Collections.<String>emptyList()
                            : Collections.unmodifiableList(new ArrayList<>(thongBao)));
        }
        return Collections.unmodifiableMap(ketQua);
    }
}
