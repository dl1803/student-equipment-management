package com.nhom13.quanlymuontra;

import com.nhom13.quanlymuontra.generated.JSON;
import com.nhom13.quanlymuontra.generated.api.SinhVienApi;
import com.nhom13.quanlymuontra.generated.model.SinhVienTheInfo;
import java.lang.reflect.Proxy;
import java.util.Arrays;
import java.util.Iterator;
import retrofit2.Call;
import retrofit2.Response;

/**
 * Dụng cụ kiểm thử cho nhóm API sinh viên (D07-L): {@link SinhVienApi} giả dựng bằng {@link Proxy} theo
 * tên thao tác (không phụ thuộc chữ ký đầy đủ của mã sinh, FC 9.1), cùng cách {@link HoTroXacThuc}.
 */
public final class HoTroSinhVienApi {

    private HoTroSinhVienApi() {}

    /**
     * {@code SinhVienApi} giả chỉ có {@code svToi}: mỗi lần gọi trả lần lượt một {@link Call} trong {@code
     * cacLan}. Gọi quá số lần chuẩn bị, hoặc gọi thao tác khác, làm test thất bại.
     */
    public static SinhVienApi taoApiSvToi(Call<?>... cacLan) {
        Iterator<Call<?>> thuTu = Arrays.<Call<?>>asList(cacLan).iterator();
        return (SinhVienApi)
                Proxy.newProxyInstance(
                        SinhVienApi.class.getClassLoader(),
                        new Class<?>[] {SinhVienApi.class},
                        (doiTuong, phuongThuc, thamSo) -> {
                            if (!"svToi".equals(phuongThuc.getName())) {
                                throw new AssertionError(
                                        "Không mong đợi gọi " + phuongThuc.getName());
                            }
                            if (!thuTu.hasNext()) {
                                throw new AssertionError("svToi bị gọi nhiều hơn số lần chuẩn bị");
                            }
                            return thuTu.next();
                        });
    }

    /** Phản hồi 200 từ fixture {@code thanh-cong/...} chứa {@link SinhVienTheInfo}. */
    public static Response<SinhVienTheInfo> hoSoThanhCong(String tepFixture) {
        SinhVienTheInfo hoSo =
                new JSON().getGson().fromJson(Fixtures.doc(tepFixture), SinhVienTheInfo.class);
        return Response.success(hoSo);
    }
}
