package com.nhom13.quanlymuontra.data.repository;

import androidx.annotation.NonNull;
import com.nhom13.quanlymuontra.data.api.ApiErrorParser;
import com.nhom13.quanlymuontra.generated.api.SinhVienApi;
import com.nhom13.quanlymuontra.generated.model.SinhVienTheInfo;
import retrofit2.Call;
import retrofit2.Callback;
import retrofit2.Response;

/**
 * Nhóm API hồ sơ mượn của sinh viên (FA 5.2): {@code GET /sv/toi} ({@code svToi}). Mã sinh viên lấy từ
 * token ở máy chủ, client không gửi mã SV (User-Flows 3.2). Mỗi lần gọi là một yêu cầu mới: không có bộ
 * đệm, vì quyền mượn phải lấy tại thời điểm cần (FA 5.3 quy tắc 3, ĐX-UF-03, ĐX-UF-04).
 *
 * <p>Lỗi có thể trả qua {@link ApiCallback#onError}: 401 ({@code CHUA_XAC_THUC}, thường đã được
 * {@code TokenAuthenticator} xử lý trước), 403 ({@code KHONG_DU_QUYEN}), 500, mất kết nối, quá thời gian
 * chờ, hủy. Phản hồi 200 không có thân được coi là lỗi hệ thống chung.
 */
public final class HoSoMuonRepository {

    private final SinhVienApi api;

    public HoSoMuonRepository(@NonNull SinhVienApi api) {
        this.api = api;
    }

    /**
     * {@code GET /sv/toi}. Trả {@link Call} để {@code ViewModel} hủy được; đây là truy vấn đọc nên hủy
     * im lặng được (FC 6.3).
     */
    @NonNull
    public Call<SinhVienTheInfo> svToi(@NonNull ApiCallback<SinhVienTheInfo> callback) {
        Call<SinhVienTheInfo> call = api.svToi();
        call.enqueue(
                new Callback<SinhVienTheInfo>() {
                    @Override
                    public void onResponse(
                            @NonNull Call<SinhVienTheInfo> goi,
                            @NonNull Response<SinhVienTheInfo> phanHoi) {
                        if (!phanHoi.isSuccessful()) {
                            callback.onError(ApiErrorParser.tuPhanHoiLoi(phanHoi));
                            return;
                        }
                        SinhVienTheInfo hoSo = phanHoi.body();
                        if (hoSo == null) {
                            callback.onError(ApiErrorParser.loiHeThongChung());
                            return;
                        }
                        callback.onSuccess(hoSo);
                    }

                    @Override
                    public void onFailure(
                            @NonNull Call<SinhVienTheInfo> goi, @NonNull Throwable loi) {
                        callback.onError(ApiErrorParser.tuNgoaiLe(goi, loi));
                    }
                });
        return call;
    }
}
