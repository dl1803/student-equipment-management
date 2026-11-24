package com.nhom13.quanlymuontra;

import com.nhom13.quanlymuontra.generated.JSON;
import com.nhom13.quanlymuontra.generated.api.SinhVienApi;
import com.nhom13.quanlymuontra.generated.model.LoaiThietBiTomTatPage;
import com.nhom13.quanlymuontra.generated.model.PhongThietBiPage;
import com.nhom13.quanlymuontra.generated.model.TraCuuThietBiItemPage;
import java.lang.reflect.Proxy;
import java.util.ArrayDeque;
import java.util.ArrayList;
import java.util.Arrays;
import java.util.Deque;
import java.util.HashMap;
import java.util.List;
import java.util.Map;
import retrofit2.Call;
import retrofit2.Response;

/**
 * Dụng cụ kiểm thử cho nhóm API tra cứu (D08-L): {@link SinhVienApi} giả, mỗi thao tác trả lần lượt các
 * {@link Call} đã chuẩn bị và ghi lại tham số mỗi lần gọi theo đúng thứ tự khai báo trong mã sinh.
 */
public final class HoTroTraCuuApi {

    /** Một lần gọi API: tên thao tác và tham số theo thứ tự chữ ký. */
    public static final class GoiApi {
        public final String thaoTac;
        public final Object[] thamSo;

        GoiApi(String thaoTac, Object[] thamSo) {
            this.thaoTac = thaoTac;
            this.thamSo = thamSo;
        }
    }

    private final Map<String, Deque<Call<?>>> hangDoi = new HashMap<>();
    private final List<GoiApi> nhatKy = new ArrayList<>();

    /** Chuẩn bị các {@link Call} sẽ trả lần lượt cho {@code thaoTac}. */
    public HoTroTraCuuApi chuanBi(String thaoTac, Call<?>... cacLan) {
        Deque<Call<?>> hang = hangDoi.get(thaoTac);
        if (hang == null) {
            hang = new ArrayDeque<>();
            hangDoi.put(thaoTac, hang);
        }
        hang.addAll(Arrays.<Call<?>>asList(cacLan));
        return this;
    }

    /** Gọi thao tác chưa chuẩn bị hoặc quá số lần chuẩn bị làm test thất bại. */
    public SinhVienApi api() {
        return (SinhVienApi)
                Proxy.newProxyInstance(
                        SinhVienApi.class.getClassLoader(),
                        new Class<?>[] {SinhVienApi.class},
                        (doiTuong, phuongThuc, thamSo) -> {
                            if (phuongThuc.getDeclaringClass() == Object.class) {
                                switch (phuongThuc.getName()) {
                                    case "hashCode":
                                        return System.identityHashCode(doiTuong);
                                    case "equals":
                                        return doiTuong == thamSo[0];
                                    default:
                                        return "HoTroTraCuuApi";
                                }
                            }
                            String ten = phuongThuc.getName();
                            Deque<Call<?>> hang = hangDoi.get(ten);
                            if (hang == null || hang.isEmpty()) {
                                throw new AssertionError(
                                        "Không mong đợi gọi " + ten + " (chưa chuẩn bị hoặc quá số lần)");
                            }
                            nhatKy.add(new GoiApi(ten, thamSo == null ? new Object[0] : thamSo));
                            return hang.poll();
                        });
    }

    /** Các lần gọi {@code thaoTac} theo thứ tự đã xảy ra. */
    public List<GoiApi> goiTheoTen(String thaoTac) {
        List<GoiApi> ketQua = new ArrayList<>();
        for (GoiApi g : nhatKy) {
            if (g.thaoTac.equals(thaoTac)) {
                ketQua.add(g);
            }
        }
        return ketQua;
    }

    private static String uuid(int so) {
        return String.format("00000000-0000-0000-0000-%012d", so);
    }

    /** Trang tra cứu có {@code soDong} dòng khả dụng 5; mã dòng duy nhất theo {@code trang}. */
    public static Response<TraCuuThietBiItemPage> trangTraCuu(int tongSo, int trang, int soDong) {
        StringBuilder sb = new StringBuilder();
        sb.append("{\"tongSo\":").append(tongSo).append(",\"trang\":").append(trang);
        sb.append(",\"kichThuoc\":20,\"danhSach\":[");
        for (int i = 0; i < soDong; i++) {
            int so = trang * 1000 + i;
            if (i > 0) {
                sb.append(',');
            }
            sb.append("{\"maLoai\":\"").append(uuid(so)).append("\",\"tenLoai\":\"Loại ").append(so);
            sb.append("\",\"hangSanXuat\":\"Hãng\",\"maPhong\":\"").append(uuid(so + 500));
            sb.append("\",\"tenPhong\":\"Phòng A\",\"coSo\":\"Cơ sở 1\",\"soLuongKhaDung\":5}");
        }
        sb.append("]}");
        return Response.success(
                new JSON().getGson().fromJson(sb.toString(), TraCuuThietBiItemPage.class));
    }

    public static Response<LoaiThietBiTomTatPage> trangLoai(int tongSo, int trang, int soDong) {
        StringBuilder sb = new StringBuilder();
        sb.append("{\"tongSo\":").append(tongSo).append(",\"trang\":").append(trang);
        sb.append(",\"kichThuoc\":100,\"danhSach\":[");
        for (int i = 0; i < soDong; i++) {
            int so = trang * 1000 + i;
            if (i > 0) {
                sb.append(',');
            }
            sb.append("{\"maLoai\":\"").append(uuid(so)).append("\",\"tenLoai\":\"Loại ").append(so);
            sb.append("\"}");
        }
        sb.append("]}");
        return Response.success(
                new JSON().getGson().fromJson(sb.toString(), LoaiThietBiTomTatPage.class));
    }

    public static Response<PhongThietBiPage> trangPhong(int tongSo, int trang, int soDong) {
        StringBuilder sb = new StringBuilder();
        sb.append("{\"tongSo\":").append(tongSo).append(",\"trang\":").append(trang);
        sb.append(",\"kichThuoc\":100,\"danhSach\":[");
        for (int i = 0; i < soDong; i++) {
            int so = trang * 1000 + i;
            if (i > 0) {
                sb.append(',');
            }
            sb.append("{\"maPhong\":\"").append(uuid(so + 500)).append("\",\"tenPhong\":\"Phòng ");
            sb.append(so).append("\",\"coSo\":\"Cơ sở 1\"}");
        }
        sb.append("]}");
        return Response.success(
                new JSON().getGson().fromJson(sb.toString(), PhongThietBiPage.class));
    }
}
