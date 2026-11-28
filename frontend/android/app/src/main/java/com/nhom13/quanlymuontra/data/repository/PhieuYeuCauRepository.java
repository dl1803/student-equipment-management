package com.nhom13.quanlymuontra.data.repository;

import androidx.annotation.NonNull;
import com.nhom13.quanlymuontra.data.api.ApiErrorParser;
import com.nhom13.quanlymuontra.generated.api.SinhVienApi;
import com.nhom13.quanlymuontra.generated.model.GuiYeuCauMuonRequest;
import com.nhom13.quanlymuontra.generated.model.PhieuYeuCauMuon;
import java.io.IOException;
import retrofit2.Call;
import retrofit2.Callback;
import retrofit2.Response;

/**
 * Nhóm API phiếu yêu cầu mượn của sinh viên (FA 5.2): {@code POST /sv/phieu-yeu-cau-muon}
 * ({@code svGuiPhieuYeuCauMuon}). Mã sinh viên lấy từ token ở máy chủ (BR-06), client không gửi.
 *
 * <p>Đây là thao tác ghi: không tự lặp, không có bộ đệm. Lỗi trả qua {@link ApiCallback#onError}:
 * 400, 401, 403, 409, 422, 500, mất kết nối, quá thời gian chờ, hủy.
 *
 * <p>Hai trường hợp phản hồi không cho biết máy chủ đã tạo phiếu hay chưa, nên được chuẩn hóa để
 * {@code ViewModel} xử lý như "chưa xác định kết quả" (FA 3.3 quy tắc 3 và 4):
 *
 * <ul>
 *   <li>Phản hồi 2xx không có thân: gọi {@code onSuccess(null)}.
 *   <li>Ngoại lệ không thuộc nhóm mạng (ví dụ không giải mã được thân 2xx): đổi thành lỗi mạng,
 *       vì yêu cầu đã được phát đi.
 * </ul>
 */
public final class PhieuYeuCauRepository {

    private final SinhVienApi api;

    public PhieuYeuCauRepository(@NonNull SinhVienApi api) {
        this.api = api;
    }

    /**
     * {@code POST /sv/phieu-yeu-cau-muon}. Trả {@link Call} để bên gọi theo dõi; thao tác ghi đã phát
     * không được hủy im lặng khi rời màn hình (FA 3.11).
     */
    @NonNull
    public Call<PhieuYeuCauMuon> svGuiPhieuYeuCauMuon(
            @NonNull GuiYeuCauMuonRequest yeuCau, @NonNull ApiCallback<PhieuYeuCauMuon> callback) {
        Call<PhieuYeuCauMuon> call = api.svGuiPhieuYeuCauMuon(yeuCau);
        call.enqueue(
                new Callback<PhieuYeuCauMuon>() {
                    @Override
                    public void onResponse(
                            @NonNull Call<PhieuYeuCauMuon> goi,
                            @NonNull Response<PhieuYeuCauMuon> phanHoi) {
                        if (!phanHoi.isSuccessful()) {
                            callback.onError(ApiErrorParser.tuPhanHoiLoi(phanHoi));
                            return;
                        }
                        callback.onSuccess(phanHoi.body());
                    }

                    @Override
                    public void onFailure(
                            @NonNull Call<PhieuYeuCauMuon> goi, @NonNull Throwable loi) {
                        Throwable nguyenNhan = loi instanceof IOException ? loi : new IOException(loi);
                        callback.onError(ApiErrorParser.tuNgoaiLe(goi, nguyenNhan));
                    }
                });
        return call;
    }
}
