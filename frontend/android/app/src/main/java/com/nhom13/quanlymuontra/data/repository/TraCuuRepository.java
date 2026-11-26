package com.nhom13.quanlymuontra.data.repository;

import androidx.annotation.NonNull;
import androidx.annotation.Nullable;
import com.nhom13.quanlymuontra.data.api.ApiErrorParser;
import com.nhom13.quanlymuontra.generated.api.SinhVienApi;
import com.nhom13.quanlymuontra.generated.model.LoaiThietBiTomTatPage;
import com.nhom13.quanlymuontra.generated.model.PhongThietBiPage;
import com.nhom13.quanlymuontra.generated.model.TraCuuThietBiChiTiet;
import com.nhom13.quanlymuontra.generated.model.TraCuuThietBiItemPage;
import java.util.UUID;
import retrofit2.Call;
import retrofit2.Callback;
import retrofit2.Response;

/**
 * Nhóm API tra cứu thiết bị của sinh viên (FA 5.2): {@code GET /sv/tra-cuu/thiet-bi}
 * ({@code svTraCuuThietBi}) và hai nguồn của bộ lọc {@code GET /sv/loai-thiet-bi} ({@code
 * svDsLoaiThietBi}), {@code GET /sv/phong-thiet-bi} ({@code svDsPhongThietBi}). Tên phương thức trùng
 * {@code operationId} (ĐX-FC-02). D09-L thêm {@code GET /sv/tra-cuu/thiet-bi/{maLoai}} ({@code svTraCuuThietBiChiTiet}) cho SCR-SV-03. Cả bốn đều là truy vấn đọc nên hủy im lặng được (FC 6.3); không có bộ
 * đệm (FA 3.6).
 *
 * <p>Lỗi trả qua {@link ApiCallback#onError}: 400, 401, 403, 404 (chỉ ở chi tiết), 500, mất kết nối, quá thời gian chờ, hủy. Phản
 * hồi 200 không có thân được coi là lỗi hệ thống chung.
 */
public final class TraCuuRepository {

    private final SinhVienApi api;

    public TraCuuRepository(@NonNull SinhVienApi api) {
        this.api = api;
    }

    /**
     * {@code GET /sv/tra-cuu/thiet-bi}. Tham số {@code null} không được gửi lên. Thứ tự kết quả do máy chủ
     * quyết định (tên loại, tên phòng); client không sắp xếp lại (FA 3.10).
     */
    @NonNull
    public Call<TraCuuThietBiItemPage> svTraCuuThietBi(
            @Nullable String ten,
            @Nullable UUID maLoai,
            @Nullable UUID maPhong,
            @Nullable Boolean conHang,
            int trang,
            int kichThuoc,
            @NonNull ApiCallback<TraCuuThietBiItemPage> callback) {
        return guiDi(api.svTraCuuThietBi(ten, maLoai, maPhong, conHang, trang, kichThuoc), callback);
    }

    /** {@code GET /sv/loai-thiet-bi}: nguồn của bộ lọc Loại thiết bị (không gồm loại đã ngưng sử dụng). */
    @NonNull
    public Call<LoaiThietBiTomTatPage> svDsLoaiThietBi(
            int trang, int kichThuoc, @NonNull ApiCallback<LoaiThietBiTomTatPage> callback) {
        return guiDi(api.svDsLoaiThietBi(trang, kichThuoc), callback);
    }

    /** {@code GET /sv/phong-thiet-bi}: nguồn của bộ lọc Phòng thiết bị. */
    @NonNull
    public Call<PhongThietBiPage> svDsPhongThietBi(
            int trang, int kichThuoc, @NonNull ApiCallback<PhongThietBiPage> callback) {
        return guiDi(api.svDsPhongThietBi(trang, kichThuoc), callback);
    }

    /**
     * {@code GET /sv/tra-cuu/thiet-bi/{maLoai}?maPhong=}: chi tiết một loại thiết bị tại một phòng (SCR-SV-03).
     * Số lượng khả dụng và ngày dự kiến có sẵn do máy chủ tính; client chỉ hiển thị (BR-16, BR-20, ĐX-KT-05).
     * Thêm lỗi 404 {@code KHONG_TIM_THAY} so với ba truy vấn còn lại.
     */
    @NonNull
    public Call<TraCuuThietBiChiTiet> svTraCuuThietBiChiTiet(
            @NonNull UUID maLoai,
            @NonNull UUID maPhong,
            @NonNull ApiCallback<TraCuuThietBiChiTiet> callback) {
        return guiDi(api.svTraCuuThietBiChiTiet(maLoai, maPhong), callback);
    }

    private static <T> Call<T> guiDi(@NonNull Call<T> call, @NonNull ApiCallback<T> callback) {
        call.enqueue(
                new Callback<T>() {
                    @Override
                    public void onResponse(@NonNull Call<T> goi, @NonNull Response<T> phanHoi) {
                        if (!phanHoi.isSuccessful()) {
                            callback.onError(ApiErrorParser.tuPhanHoiLoi(phanHoi));
                            return;
                        }
                        T thanPhanHoi = phanHoi.body();
                        if (thanPhanHoi == null) {
                            callback.onError(ApiErrorParser.loiHeThongChung());
                            return;
                        }
                        callback.onSuccess(thanPhanHoi);
                    }

                    @Override
                    public void onFailure(@NonNull Call<T> goi, @NonNull Throwable loi) {
                        callback.onError(ApiErrorParser.tuNgoaiLe(goi, loi));
                    }
                });
        return call;
    }
}
