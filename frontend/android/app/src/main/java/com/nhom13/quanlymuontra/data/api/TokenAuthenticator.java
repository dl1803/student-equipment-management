package com.nhom13.quanlymuontra.data.api;

import androidx.annotation.NonNull;
import androidx.annotation.Nullable;
import com.google.gson.Gson;
import com.google.gson.JsonParseException;
import com.nhom13.quanlymuontra.data.model.ApiError;
import com.nhom13.quanlymuontra.data.session.SessionManager;
import com.nhom13.quanlymuontra.data.session.TokenPair;
import com.nhom13.quanlymuontra.generated.JSON;
import com.nhom13.quanlymuontra.generated.model.LamMoiRequest;
import com.nhom13.quanlymuontra.generated.model.TokenResponse;
import java.io.IOException;
import okhttp3.Authenticator;
import okhttp3.HttpUrl;
import okhttp3.MediaType;
import okhttp3.OkHttpClient;
import okhttp3.Request;
import okhttp3.RequestBody;
import okhttp3.Response;
import okhttp3.ResponseBody;
import okhttp3.Route;

/**
 * Xử lý 401 của yêu cầu có token bằng đúng một lần làm mới phiên tại một thời điểm (FA 3.5, FA 5.4,
 * ĐX-FA-23). Refresh token xoay vòng và dùng lại token cũ thu hồi cả chuỗi, nên nhiều yêu cầu cùng
 * 401 không được mở nhiều lần làm mới.
 *
 * <p>Mọi yêu cầu 401 đi qua cùng một khối {@code synchronized}:
 *
 * <ol>
 *   <li>Yêu cầu thuộc bốn endpoint công khai, hoặc đã là lần gọi lại sau một 401: trả {@code null},
 *       không làm mới (không lặp).
 *   <li>Token mà yêu cầu đã gửi trùng token của lần làm mới vừa thất bại: ném {@link
 *       LamMoiPhienThatBaiException} cùng {@link ApiError} đó, không mở lần làm mới khác bằng
 *       token cũ.
 *   <li>Không còn phiên: trả {@code null} (phiên đã bị đăng xuất hoặc kết thúc trước đó).
 *   <li>Token hiện tại khác token đã gửi: một yêu cầu khác đã làm mới thành công, gọi lại với token
 *       hiện tại, không làm mới nữa.
 *   <li>Còn lại: gọi {@code POST /auth/lam-moi} bằng client riêng không có {@code Authenticator}.
 *       Thành công: ghi cặp token mới xuống kho lưu rồi mới gọi lại, đúng một lần. Thất bại (401,
 *       429, mạng, quá thời gian chờ, 500 hoặc thân không đọc được): kết thúc phiên kèm lý do và
 *       ném {@link LamMoiPhienThatBaiException}.
 * </ol>
 *
 * <p>Lớp này không ghi nhật ký và không đưa token vào thông điệp lỗi (FC 6.8).
 */
public final class TokenAuthenticator implements Authenticator {

    private static final Gson GSON = new JSON().getGson();
    private static final MediaType JSON_UTF8 = MediaType.get("application/json; charset=utf-8");
    private static final int MA_THANH_CONG = 200;

    @NonNull private final SessionManager sessionManager;
    @NonNull private final OkHttpClient clientLamMoi;
    @NonNull private final HttpUrl urlLamMoi;

    private final Object khoa = new Object();

    // Chỉ đọc và ghi trong khối synchronized (khoa). Ghi lại lần làm mới thất bại gần nhất theo
    // access token đã bị bỏ để những yêu cầu đang chờ cùng token đó nhận đúng lý do (FA 3.5).
    @Nullable private String tokenCuCuaLanThatBai;
    @Nullable private ApiError loiCuaLanThatBai;

    /**
     * @param clientLamMoi client riêng cho lần làm mới: không có {@code AuthInterceptor} và không
     *     có {@code Authenticator}, để lần làm mới không tự kích hoạt lại chính nó (FA 5.4)
     * @param diaChiGoc địa chỉ gốc của máy chủ, không có {@code /api/v1} (FA 5.8)
     */
    public TokenAuthenticator(
            @NonNull SessionManager sessionManager,
            @NonNull OkHttpClient clientLamMoi,
            @NonNull HttpUrl diaChiGoc) {
        this.sessionManager = sessionManager;
        this.clientLamMoi = clientLamMoi;
        this.urlLamMoi = diaChiGoc.newBuilder().encodedPath(DuongDanApi.LAM_MOI).build();
    }

    @Nullable
    @Override
    public Request authenticate(@Nullable Route route, @NonNull Response response)
            throws IOException {
        Request yeuCau = response.request();
        if (DuongDanApi.khongCanToken(yeuCau.url()) || daGoiLaiSau401(response)) {
            return null;
        }
        String tokenDaGui = layTokenDaGui(yeuCau);
        synchronized (khoa) {
            if (loiCuaLanThatBai != null
                    && tokenDaGui != null
                    && tokenDaGui.equals(tokenCuCuaLanThatBai)) {
                throw new LamMoiPhienThatBaiException(loiCuaLanThatBai);
            }
            TokenPair hienTai = sessionManager.getTokenPair();
            if (hienTai == null) {
                return null;
            }
            if (!hienTai.getAccessToken().equals(tokenDaGui)) {
                return goiLai(yeuCau, hienTai.getAccessToken());
            }
            return lamMoiRoiGoiLai(yeuCau, hienTai);
        }
    }

    // Gọi trong khối synchronized (khoa).
    @NonNull
    private Request lamMoiRoiGoiLai(@NonNull Request yeuCau, @NonNull TokenPair hienTai)
            throws LamMoiPhienThatBaiException {
        Request yeuCauLamMoi =
                new Request.Builder()
                        .url(urlLamMoi)
                        .header("Accept", "application/json")
                        .post(RequestBody.create(thanLamMoi(hienTai.getRefreshToken()), JSON_UTF8))
                        .build();
        TokenPair capMoi = null;
        ApiError loi = null;
        try (Response phanHoi = clientLamMoi.newCall(yeuCauLamMoi).execute()) {
            if (phanHoi.code() == MA_THANH_CONG) {
                capMoi = docCapToken(phanHoi);
                if (capMoi == null) {
                    loi = ApiErrorParser.loiHeThongChung();
                }
            } else {
                ResponseBody than = phanHoi.body();
                loi =
                        ApiErrorParser.tuPhanHoiLoi(
                                phanHoi.code(),
                                than == null ? null : than.contentType(),
                                than == null ? null : than.string());
            }
        } catch (IOException e) {
            loi = ApiErrorParser.tuNgoaiLe(false, e);
        }
        if (capMoi == null) {
            ApiError lyDo = loi == null ? ApiErrorParser.loiHeThongChung() : loi;
            tokenCuCuaLanThatBai = hienTai.getAccessToken();
            loiCuaLanThatBai = lyDo;
            sessionManager.ketThucPhien(lyDo);
            throw new LamMoiPhienThatBaiException(lyDo);
        }
        // save() ghi đồng bộ xuống kho lưu trước khi trả về, nên cặp token mới đã xuống đĩa trước
        // khi bất kỳ yêu cầu nào được gọi lại (FA 3.5). Ghi đĩa thất bại vẫn dùng được trong bộ
        // nhớ của lần chạy này, không phải lý do để dừng yêu cầu.
        sessionManager.save(capMoi);
        tokenCuCuaLanThatBai = null;
        loiCuaLanThatBai = null;
        return goiLai(yeuCau, capMoi.getAccessToken());
    }

    @NonNull
    private static String thanLamMoi(@NonNull String refreshToken) {
        LamMoiRequest yeuCau = new LamMoiRequest();
        yeuCau.setRefreshToken(refreshToken);
        return GSON.toJson(yeuCau);
    }

    @Nullable
    private static TokenPair docCapToken(@NonNull Response phanHoi) throws IOException {
        ResponseBody than = phanHoi.body();
        if (than == null) {
            return null;
        }
        try {
            TokenResponse token = GSON.fromJson(than.string(), TokenResponse.class);
            if (token == null
                    || laRong(token.getAccessToken())
                    || laRong(token.getRefreshToken())) {
                return null;
            }
            return new TokenPair(token.getAccessToken(), token.getRefreshToken());
        } catch (JsonParseException | IllegalArgumentException e) {
            return null;
        }
    }

    private static boolean laRong(@Nullable String chuoi) {
        return chuoi == null || chuoi.isEmpty();
    }

    @Nullable
    private static String layTokenDaGui(@NonNull Request yeuCau) {
        String tieuDe = yeuCau.header(AuthInterceptor.TIEU_DE_XAC_THUC);
        if (tieuDe == null || !tieuDe.startsWith(AuthInterceptor.TIEN_TO_BEARER)) {
            return null;
        }
        return tieuDe.substring(AuthInterceptor.TIEN_TO_BEARER.length());
    }

    @NonNull
    private static Request goiLai(@NonNull Request yeuCau, @NonNull String accessToken) {
        return yeuCau.newBuilder()
                .header(
                        AuthInterceptor.TIEU_DE_XAC_THUC,
                        AuthInterceptor.TIEN_TO_BEARER + accessToken)
                .build();
    }

    // Phản hồi của yêu cầu đã được gọi lại luôn mang theo phản hồi 401 trước đó (OkHttp gắn
    // priorResponse). Chỉ đếm 401 để các lần chuyển hướng không bị tính là lần gọi lại.
    private static boolean daGoiLaiSau401(@NonNull Response response) {
        Response truoc = response.priorResponse();
        while (truoc != null) {
            if (truoc.code() == 401) {
                return true;
            }
            truoc = truoc.priorResponse();
        }
        return false;
    }
}
