package com.nhom13.quanlymuontra.data.api;

import androidx.annotation.NonNull;
import androidx.annotation.Nullable;
import com.google.gson.Gson;
import com.google.gson.JsonParseException;
import com.nhom13.quanlymuontra.data.model.ApiError;
import com.nhom13.quanlymuontra.generated.JSON;
import com.nhom13.quanlymuontra.generated.model.ValidationProblemDetails;
import java.io.IOException;
import java.io.InterruptedIOException;
import okhttp3.MediaType;
import okhttp3.ResponseBody;
import retrofit2.Call;
import retrofit2.Response;

/**
 * Nơi duy nhất chuẩn hóa phản hồi lỗi và ngoại lệ mạng thành {@link ApiError} (FA 3.3, FA 5.5, FC
 * 6.4). Lớp phía trên {@code data/} không đọc phản hồi HTTP thô.
 *
 * <p>{@code maLoi} giữ nguyên chuỗi máy chủ trả, kể cả mã chưa biết (ĐX-FC-21); không đọc {@code
 * detail} để phân nhánh (ĐX-CV-02). Lớp này không ghi nhật ký.
 */
public final class ApiErrorParser {

    // Cùng bộ chuyển đổi Gson do bộ sinh tạo, đã cấu hình bỏ qua trường lạ (FA 3.2).
    private static final Gson GSON = new JSON().getGson();

    // Mã HTTP dùng khi không có phản hồi đọc được nhưng vẫn phải báo lỗi hệ thống chung.
    private static final int STATUS_LOI_HE_THONG = 500;

    private ApiErrorParser() {}

    /**
     * Lỗi hệ thống chung: loại {@code HTTP}, không có {@code maLoi} và {@code traceId}. Dùng khi
     * phản hồi đã nhận nhưng không đọc được hoặc ngoại lệ không thuộc nhóm mạng (FA 3.3 quy tắc 1).
     */
    @NonNull
    public static ApiError loiHeThongChung() {
        return ApiError.http(STATUS_LOI_HE_THONG, null, null, null, null);
    }

    /** Chuẩn hóa phản hồi Retrofit có mã không thuộc 2xx. */
    @NonNull
    public static ApiError tuPhanHoiLoi(@NonNull Response<?> phanHoi) {
        ResponseBody thanLoi = phanHoi.errorBody();
        if (thanLoi == null) {
            return tuPhanHoiLoi(phanHoi.code(), null, null);
        }
        try {
            return tuPhanHoiLoi(phanHoi.code(), thanLoi.contentType(), thanLoi.string());
        } catch (IOException e) {
            return tuNgoaiLe(false, e);
        }
    }

    /**
     * Chuẩn hóa một phản hồi lỗi đã có mã, kiểu nội dung và thân.
     *
     * <p>Chỉ thân {@code application/problem+json} đọc được mới cho {@code maLoi}, {@code
     * traceId}, {@code errors}; thân khác (ví dụ trang lỗi HTML của hạ tầng), thân rỗng hoặc hỏng
     * cú pháp cho lỗi {@code HTTP} không có {@code maLoi} (FA 3.3 quy tắc 1).
     */
    @NonNull
    public static ApiError tuPhanHoiLoi(
            int status, @Nullable MediaType loaiNoiDung, @Nullable String thanLoi) {
        if (!laProblemJson(loaiNoiDung) || thanLoi == null || thanLoi.isEmpty()) {
            return ApiError.http(status, null, null, null, null);
        }
        ValidationProblemDetails chiTiet;
        try {
            // ValidationProblemDetails là tập cha của ProblemDetails: thiếu `errors` thì là null.
            chiTiet = GSON.fromJson(thanLoi, ValidationProblemDetails.class);
        } catch (JsonParseException | IllegalArgumentException e) {
            return ApiError.http(status, null, null, null, null);
        }
        if (chiTiet == null) {
            return ApiError.http(status, null, null, null, null);
        }
        return ApiError.http(
                status,
                chiTiet.getMaLoi(),
                chiTiet.getDetail(),
                chiTiet.getTraceId(),
                chiTiet.getErrors());
    }

    /** Chuẩn hóa ngoại lệ của một {@link Call} Retrofit. */
    @NonNull
    public static ApiError tuNgoaiLe(@NonNull Call<?> call, @NonNull Throwable loi) {
        return tuNgoaiLe(call.isCanceled(), loi);
    }

    /**
     * Chuẩn hóa ngoại lệ mạng.
     *
     * <p>Thứ tự kiểm tra có chủ ý:
     *
     * <ol>
     *   <li>{@link LamMoiPhienThatBaiException}: trả nguyên {@link ApiError} của lần làm mới, không
     *       đổi thành {@code MANG} hay lỗi 401 gốc (FA 5.4).
     *   <li>{@link InterruptedIOException} (gồm {@code SocketTimeoutException} và hết {@code
     *       callTimeout}): {@code QUA_THOI_GIAN}. Phải đứng trước kiểm tra hủy vì OkHttp tự gọi
     *       {@code cancel()} khi hết {@code callTimeout}, nên {@code isCanceled()} cũng là {@code
     *       true} trong trường hợp này.
     *   <li>{@code yeuCauDaHuy}: {@code HUY}.
     *   <li>{@link IOException} khác: {@code MANG}.
     *   <li>Còn lại (ví dụ lỗi giải mã thân thành công): lỗi hệ thống chung.
     * </ol>
     *
     * <p>Việc hiển thị {@code HUY} phụ thuộc loại thao tác; thao tác ghi đã phát không được nuốt
     * lỗi này (FA 3.3 quy tắc 3, 4).
     */
    @NonNull
    public static ApiError tuNgoaiLe(boolean yeuCauDaHuy, @NonNull Throwable loi) {
        if (loi instanceof LamMoiPhienThatBaiException) {
            return ((LamMoiPhienThatBaiException) loi).getApiError();
        }
        if (loi instanceof InterruptedIOException) {
            return ApiError.quaThoiGian();
        }
        if (yeuCauDaHuy) {
            return ApiError.huy();
        }
        if (loi instanceof IOException) {
            return ApiError.mang();
        }
        return loiHeThongChung();
    }

    private static boolean laProblemJson(@Nullable MediaType loaiNoiDung) {
        return loaiNoiDung != null
                && "application".equalsIgnoreCase(loaiNoiDung.type())
                && "problem+json".equalsIgnoreCase(loaiNoiDung.subtype());
    }
}
