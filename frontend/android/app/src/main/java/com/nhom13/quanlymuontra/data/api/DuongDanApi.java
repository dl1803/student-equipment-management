package com.nhom13.quanlymuontra.data.api;

import androidx.annotation.NonNull;
import okhttp3.HttpUrl;

/**
 * Đường dẫn API mà tầng mạng phải tự biết (FA 3.2: {@code /api/v1} là hằng số của client).
 *
 * <p>Địa chỉ gốc của máy chủ không có {@code /api/v1} (FA 5.8). Mỗi đường dẫn của client Retrofit
 * sinh từ OpenAPI đã chứa sẵn {@code api/v1/...}, nên không được nối thêm {@link #TIEN_TO} vào địa
 * chỉ gốc khi dựng Retrofit. Hằng số ở đây chỉ phục vụ nhận diện endpoint công khai và lần làm mới
 * token gọi tay trong {@link TokenAuthenticator}; {@code DuongDanApiTest} đối chiếu chúng với chú
 * thích của mã sinh để phát hiện lệch hợp đồng.
 */
public final class DuongDanApi {

    public static final String TIEN_TO = "/api/v1";

    public static final String DANG_NHAP_SINH_VIEN = TIEN_TO + "/auth/sinh-vien/dang-nhap";
    public static final String DANG_NHAP_NHAN_VIEN = TIEN_TO + "/auth/nhan-vien/dang-nhap";
    public static final String LAM_MOI = TIEN_TO + "/auth/lam-moi";
    public static final String DANG_XUAT = TIEN_TO + "/auth/dang-xuat";

    private DuongDanApi() {}

    /**
     * Bốn endpoint không yêu cầu xác thực (FA 3.5): đăng nhập sinh viên, đăng nhập nhân viên và
     * quản trị, làm mới, đăng xuất. Các yêu cầu này không gắn {@code Authorization} và không bao
     * giờ kích hoạt làm mới phiên.
     */
    public static boolean khongCanToken(@NonNull HttpUrl url) {
        String duongDan = url.encodedPath();
        return DANG_NHAP_SINH_VIEN.equals(duongDan)
                || DANG_NHAP_NHAN_VIEN.equals(duongDan)
                || LAM_MOI.equals(duongDan)
                || DANG_XUAT.equals(duongDan);
    }
}
