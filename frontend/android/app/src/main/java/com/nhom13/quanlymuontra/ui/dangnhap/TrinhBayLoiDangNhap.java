package com.nhom13.quanlymuontra.ui.dangnhap;

import androidx.annotation.NonNull;
import androidx.annotation.Nullable;
import androidx.annotation.StringRes;
import com.nhom13.quanlymuontra.R;
import com.nhom13.quanlymuontra.common.nhan.Tong;
import com.nhom13.quanlymuontra.data.model.ApiError;
import java.util.List;
import java.util.Map;

/**
 * Chọn nội dung, tông và vị trí hiển thị cho một {@link ApiError} ở SCR-CM-01 (UI-Screens 4.4, DS 9.4,
 * FA 3.3, 3.4 và 3.11). Dùng cho cả lỗi của lần đăng nhập lẫn lý do kết thúc phiên (làm mới thất bại) vì hai
 * trường hợp dùng chung bảng mã lỗi. Chỉ dựa vào {@code loai}, {@code status}, {@code errors} và {@code maLoi},
 * không dựa vào chữ của {@code detail}; mã không có xử lý riêng là lỗi hệ thống chung kèm {@code traceId}.
 */
final class TrinhBayLoiDangNhap {

    static final String MA_DANG_NHAP_SAI = "DANG_NHAP_SAI";
    static final String MA_CHUA_XAC_THUC = "CHUA_XAC_THUC";
    static final String MA_QUA_NHIEU_YEU_CAU = "QUA_NHIEU_YEU_CAU";
    private static final int STATUS_QUA_NHIEU_YEU_CAU = 429;

    /** Nơi hiển thị thông báo. */
    enum ViTri {
        /** Lỗi dưới ô mã số sinh viên (FA 3.11). */
        DUOI_O_MSSV,
        /** Lỗi dưới ô mật khẩu (UI-Screens 4.4, Wireframes, FA 3.11). */
        DUOI_O_MAT_KHAU,
        /** Khối thông báo CMP-24 ở đầu biểu mẫu. */
        KHOI_THONG_BAO
    }

    /** Kết quả chọn nội dung; {@code traceId} chỉ có ở lỗi hệ thống. */
    static final class NoiDung {
        @NonNull final ViTri viTri;
        @NonNull final Tong tong;
        @StringRes final int thongDiep;
        @Nullable final String thongDiepTuyChinh;
        @Nullable final String traceId;

        NoiDung(
                @NonNull ViTri viTri,
                @NonNull Tong tong,
                @StringRes int thongDiep,
                @Nullable String traceId) {
            this(viTri, tong, thongDiep, null, traceId);
        }

        NoiDung(
                @NonNull ViTri viTri,
                @NonNull Tong tong,
                @StringRes int thongDiep,
                @Nullable String thongDiepTuyChinh,
                @Nullable String traceId) {
            this.viTri = viTri;
            this.tong = tong;
            this.thongDiep = thongDiep;
            this.thongDiepTuyChinh = thongDiepTuyChinh;
            this.traceId = traceId;
        }
    }

    private TrinhBayLoiDangNhap() {}

    /** @return nội dung hiển thị, hoặc {@code null} khi không có gì để hiển thị (yêu cầu bị hủy) */
    @Nullable
    static NoiDung cho(@NonNull ApiError loi) {
        switch (loi.getLoai()) {
            case HUY:
                return null;
            case MANG:
            case QUA_THOI_GIAN:
                return new NoiDung(ViTri.KHOI_THONG_BAO, Tong.DANGER, R.string.loi_ket_noi_may_chu, null);
            case HTTP:
            default:
                break;
        }
        String maLoi = loi.getMaLoi();
        Integer status = loi.getStatus();
        if (status != null && status == 400) {
            if (!loi.getErrors().isEmpty()) {
                for (Map.Entry<String, List<String>> entry : loi.getErrors().entrySet()) {
                    String k = entry.getKey().toLowerCase();
                    List<String> ds = entry.getValue();
                    String tb = (ds != null && !ds.isEmpty()) ? ds.get(0) : null;
                    if (tb != null && !tb.isEmpty()) {
                        if (k.contains("mssv")) {
                            return new NoiDung(ViTri.DUOI_O_MSSV, Tong.DANGER, R.string.loi_he_thong, tb, null);
                        }
                        if (k.contains("matkhau")) {
                            return new NoiDung(ViTri.DUOI_O_MAT_KHAU, Tong.DANGER, R.string.loi_he_thong, tb, null);
                        }
                    }
                }
                for (List<String> ds : loi.getErrors().values()) {
                    if (ds != null && !ds.isEmpty() && ds.get(0) != null && !ds.get(0).isEmpty()) {
                        return new NoiDung(ViTri.KHOI_THONG_BAO, Tong.DANGER, R.string.loi_he_thong, ds.get(0), null);
                    }
                }
            }
            return new NoiDung(
                    ViTri.KHOI_THONG_BAO, Tong.DANGER, R.string.loi_he_thong, loi.getTraceId());
        }
        if (MA_DANG_NHAP_SAI.equals(maLoi)) {
            return new NoiDung(ViTri.DUOI_O_MAT_KHAU, Tong.DANGER, R.string.dang_nhap_sai, null);
        }
        if (MA_CHUA_XAC_THUC.equals(maLoi)) {
            return new NoiDung(ViTri.KHOI_THONG_BAO, Tong.INFO, R.string.phien_het_han, null);
        }
        if (MA_QUA_NHIEU_YEU_CAU.equals(maLoi)
                || (status != null && status == STATUS_QUA_NHIEU_YEU_CAU)) {
            return new NoiDung(ViTri.KHOI_THONG_BAO, Tong.WARNING, R.string.qua_nhieu_yeu_cau, null);
        }
        return new NoiDung(
                ViTri.KHOI_THONG_BAO, Tong.DANGER, R.string.loi_he_thong, loi.getTraceId());
    }

    /** @return {@code true} khi lỗi là sai thông tin đăng nhập, hiển thị dưới ô mật khẩu */
    static boolean laLoiDangNhapSai(@NonNull ApiError loi) {
        return MA_DANG_NHAP_SAI.equals(loi.getMaLoi());
    }
}
