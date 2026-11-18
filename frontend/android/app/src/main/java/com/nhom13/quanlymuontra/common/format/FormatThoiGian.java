package com.nhom13.quanlymuontra.common.format;

import androidx.annotation.NonNull;
import androidx.annotation.Nullable;
import com.nhom13.quanlymuontra.common.HangSo;
import java.time.LocalDate;
import java.time.OffsetDateTime;
import java.time.format.DateTimeFormatter;
import java.util.Locale;
import java.util.regex.Pattern;

/**
 * Định dạng thời gian để hiển thị (DS 9.7, ĐX-UI-12, FC 7.2). Chỉ định dạng giá trị máy chủ trả về,
 * không tính toán gì (FA-07).
 *
 * <p>Mẫu dùng {@link Locale#ROOT} để chữ số và thứ tự phần tử không phụ thuộc vùng ngôn ngữ của
 * thiết bị. Giá trị thiếu hiển thị {@link HangSo#GIA_TRI_THIEU}.
 */
public final class FormatThoiGian {

    private static final DateTimeFormatter NGAY_GIO =
            DateTimeFormatter.ofPattern("dd/MM/yyyy HH:mm", Locale.ROOT);
    private static final DateTimeFormatter NGAY =
            DateTimeFormatter.ofPattern("dd/MM/yyyy", Locale.ROOT);
    // HH:mm hoặc HH:mm:ss (có thể kèm phần lẻ giây): giá trị giờ quầy đã là giờ Việt Nam.
    private static final Pattern GIO = Pattern.compile("^\\d{2}:\\d{2}(:.*)?$");

    private FormatThoiGian() {}

    /**
     * Thời điểm (UTC hoặc có độ lệch bất kỳ) thành {@code dd/MM/yyyy HH:mm} theo giờ Việt Nam, 24
     * giờ. 17:00 UTC là 00:00 ngày kế tiếp, không bao giờ là 24:00.
     */
    @NonNull
    public static String ngayGio(@Nullable OffsetDateTime thoiDiem) {
        if (thoiDiem == null) {
            return HangSo.GIA_TRI_THIEU;
        }
        return thoiDiem.atZoneSameInstant(HangSo.MUI_GIO_VN).format(NGAY_GIO);
    }

    /** Ngày thuần thành {@code dd/MM/yyyy}; không đổi múi giờ vì giá trị không có giờ. */
    @NonNull
    public static String ngay(@Nullable LocalDate ngay) {
        if (ngay == null) {
            return HangSo.GIA_TRI_THIEU;
        }
        return ngay.format(NGAY);
    }

    /**
     * Giờ quầy {@code HH:mm[:ss]} thành {@code HH:mm}, không đổi múi giờ. Chuỗi không đúng dạng được
     * trả nguyên văn thay vì gây lỗi (FA 2.2: giá trị lạ không làm vỡ giao diện).
     */
    @NonNull
    public static String gio(@Nullable String gio) {
        if (gio == null) {
            return HangSo.GIA_TRI_THIEU;
        }
        return GIO.matcher(gio).matches() ? gio.substring(0, 5) : gio;
    }
}
