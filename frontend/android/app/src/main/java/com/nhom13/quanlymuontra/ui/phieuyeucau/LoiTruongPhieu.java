package com.nhom13.quanlymuontra.ui.phieuyeucau;

import androidx.annotation.NonNull;
import androidx.annotation.Nullable;
import java.util.ArrayList;
import java.util.Collections;
import java.util.LinkedHashMap;
import java.util.List;
import java.util.Map;
import java.util.UUID;
import java.util.regex.Matcher;
import java.util.regex.Pattern;

/**
 * Lỗi theo trường của lỗi 400 {@code DU_LIEU_KHONG_HOP_LE} (FC 6.6, UI-Screens 5.5): hàm thuần ánh xạ khóa trong
 * {@code errors} về đúng ô. Khóa {@code dsLoaiVaSoLuong[i]...} (so khớp không phân biệt hoa thường) về dòng loại thứ
 * {@code i} của phiếu đã gửi; {@code thoiGianMuonDuKienGio} về ô thời gian; khóa khác vào {@link #getLoiKhac()}.
 */
public final class LoiTruongPhieu {

    private static final Pattern DONG_LOAI =
            Pattern.compile("^dsLoaiVaSoLuong\\[(\\d{1,9})\\](?:\\..+)?$", Pattern.CASE_INSENSITIVE);
    private static final String KHOA_THOI_GIAN = "thoiGianMuonDuKienGio";
    private static final LoiTruongPhieu RONG =
            new LoiTruongPhieu(
                    Collections.<UUID, String>emptyMap(), null, Collections.<String>emptyList());

    @NonNull private final Map<UUID, String> loiSoLuong;
    @Nullable private final String loiThoiGian;
    @NonNull private final List<String> loiKhac;

    private LoiTruongPhieu(
            @NonNull Map<UUID, String> loiSoLuong,
            @Nullable String loiThoiGian,
            @NonNull List<String> loiKhac) {
        this.loiSoLuong = loiSoLuong;
        this.loiThoiGian = loiThoiGian;
        this.loiKhac = loiKhac;
    }

    @NonNull
    public static LoiTruongPhieu rong() {
        return RONG;
    }

    /**
     * @param errors {@code errors} của phản hồi 400; có thể {@code null}
     * @param thuTuLoai mã loại theo đúng thứ tự dòng trong yêu cầu đã gửi
     */
    @NonNull
    public static LoiTruongPhieu phanTich(
            @Nullable Map<String, List<String>> errors, @NonNull List<UUID> thuTuLoai) {
        if (errors == null || errors.isEmpty()) {
            return RONG;
        }
        Map<UUID, String> loiSoLuong = new LinkedHashMap<>();
        String loiThoiGian = null;
        List<String> loiKhac = new ArrayList<>();
        for (Map.Entry<String, List<String>> muc : errors.entrySet()) {
            String chu = gop(muc.getValue());
            if (chu.isEmpty()) {
                continue;
            }
            String khoa = muc.getKey() == null ? "" : muc.getKey();
            if (KHOA_THOI_GIAN.equalsIgnoreCase(khoa)) {
                loiThoiGian = loiThoiGian == null ? chu : loiThoiGian + "\n" + chu;
                continue;
            }
            Matcher khop = DONG_LOAI.matcher(khoa);
            if (khop.matches()) {
                int chiSo = Integer.parseInt(khop.group(1));
                if (chiSo < thuTuLoai.size()) {
                    UUID ma = thuTuLoai.get(chiSo);
                    String cu = loiSoLuong.get(ma);
                    loiSoLuong.put(ma, cu == null ? chu : cu + "\n" + chu);
                    continue;
                }
            }
            loiKhac.add(chu);
        }
        return new LoiTruongPhieu(
                Collections.unmodifiableMap(loiSoLuong),
                loiThoiGian,
                Collections.unmodifiableList(loiKhac));
    }

    private static String gop(@Nullable List<String> cacThongDiep) {
        if (cacThongDiep == null) {
            return "";
        }
        StringBuilder sb = new StringBuilder();
        for (String t : cacThongDiep) {
            if (t == null || t.trim().isEmpty()) {
                continue;
            }
            if (sb.length() > 0) {
                sb.append(' ');
            }
            sb.append(t.trim());
        }
        return sb.toString();
    }

    /** Lỗi theo mã loại (dòng số lượng). */
    @NonNull
    public Map<UUID, String> getLoiSoLuong() {
        return loiSoLuong;
    }

    @Nullable
    public String getLoiThoiGian() {
        return loiThoiGian;
    }

    /** Thông điệp không gán được vào ô nào (hiển thị ở khối thông báo đầu SCR-SV-04). */
    @NonNull
    public List<String> getLoiKhac() {
        return loiKhac;
    }

    public boolean coLoiTruong() {
        return !loiSoLuong.isEmpty() || loiThoiGian != null;
    }
}
