package com.nhom13.quanlymuontra.common.format;

import androidx.annotation.NonNull;
import androidx.annotation.Nullable;
import com.nhom13.quanlymuontra.common.HangSo;
import java.text.DecimalFormat;
import java.text.DecimalFormatSymbols;
import java.util.Locale;

/**
 * Định dạng tiền VNĐ (DS 9.7, ĐX-UI-12, FC 7.2): số nguyên, dấu chấm phân cách hàng nghìn, một
 * khoảng trắng không ngắt U+00A0 rồi {@code VNĐ}, không có phần thập phân. Chỉ định dạng số máy chủ
 * trả về, không tính toán (FA-07).
 */
public final class FormatTien {

    private static final char DAU_NHOM = '.';
    // U+00A0 để số và đơn vị không bị tách thành hai dòng.
    private static final String KHOANG_TRANG_KHONG_NGAT = "\u00A0";
    // U+0110 là chữ Đ hoa; viết bằng mã để không phụ thuộc cách chuẩn hóa chữ của tệp nguồn.
    private static final String DON_VI = "VN\u0110";

    private FormatTien() {}

    /** Ví dụ {@code 240000} thành {@code 240.000 VNĐ} và {@code 0} thành {@code 0 VNĐ}. */
    @NonNull
    public static String tien(long soTien) {
        // Dấu nhóm đặt tường minh để kết quả không phụ thuộc locale của máy. DecimalFormat không an
        // toàn khi dùng chung giữa các luồng nên tạo mới ở mỗi lần gọi.
        DecimalFormatSymbols kyHieu = new DecimalFormatSymbols(Locale.ROOT);
        kyHieu.setGroupingSeparator(DAU_NHOM);
        DecimalFormat dinhDang = new DecimalFormat("#,##0", kyHieu);
        return dinhDang.format(soTien) + KHOANG_TRANG_KHONG_NGAT + DON_VI;
    }

    /** Như {@link #tien(long)}; {@code null} thành {@link HangSo#GIA_TRI_THIEU}. */
    @NonNull
    public static String tien(@Nullable Long soTien) {
        return soTien == null ? HangSo.GIA_TRI_THIEU : tien(soTien.longValue());
    }
}
