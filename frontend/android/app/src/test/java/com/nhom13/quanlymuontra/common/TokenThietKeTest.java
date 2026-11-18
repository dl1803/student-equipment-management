package com.nhom13.quanlymuontra.common;

import static org.junit.Assert.assertEquals;
import static org.junit.Assert.assertFalse;
import static org.junit.Assert.assertTrue;

import com.nhom13.quanlymuontra.TaiNguyen;
import java.io.File;
import java.util.HashMap;
import java.util.LinkedHashMap;
import java.util.Map;
import java.util.regex.Matcher;
import java.util.regex.Pattern;
import org.junit.Test;
import org.w3c.dom.Document;
import org.w3c.dom.Element;
import org.w3c.dom.NodeList;

/**
 * Kiểm token thiết kế Android theo DS 3 đến 5, Phụ lục C và FC 6.6, 8.4 (TC-01): giá trị token đúng
 * Design-System, lớp ngữ nghĩa chỉ tham chiếu token, và mã màu, kích thước thô chỉ nằm ở các tệp
 * token. Bảng kỳ vọng chép độc lập từ Design-System để đối chiếu với tệp tài nguyên.
 */
public class TokenThietKeTest {

    private static final String RES = "src/main/res";

    /** Tên token màu nguyên thủy và mã màu (DS 4.1). */
    private static final String[][] MAU_NGUYEN_THUY = {
        {"ds_color_gray_0", "#FFFFFF"},
        {"ds_color_gray_50", "#F6F7F9"},
        {"ds_color_gray_100", "#EEF0F3"},
        {"ds_color_gray_200", "#E1E4E9"},
        {"ds_color_gray_300", "#CBD0D8"},
        {"ds_color_gray_400", "#808998"},
        {"ds_color_gray_500", "#6B7482"},
        {"ds_color_gray_600", "#4F5764"},
        {"ds_color_gray_700", "#3A414D"},
        {"ds_color_gray_900", "#161A21"},
        {"ds_color_blue_50", "#EEF4FB"},
        {"ds_color_blue_100", "#D6E4F5"},
        {"ds_color_blue_500", "#2F69B0"},
        {"ds_color_blue_600", "#1F5499"},
        {"ds_color_blue_700", "#184380"},
        {"ds_color_blue_800", "#133466"},
        {"ds_color_green_50", "#E7F5EC"},
        {"ds_color_amber_50", "#FFF4DB"},
        {"ds_color_red_50", "#FDECEC"},
        {"ds_color_green_100", "#CDEBD8"},
        {"ds_color_amber_100", "#FFE5A8"},
        {"ds_color_red_100", "#F9D0D0"},
        {"ds_color_green_600", "#1B7A43"},
        {"ds_color_amber_600", "#B45F06"},
        {"ds_color_red_600", "#C62F2F"},
        {"ds_color_green_700", "#146236"},
        {"ds_color_amber_700", "#8A4A00"},
        {"ds_color_red_700", "#A12020"},
        {"ds_color_red_800", "#7F1818"},
    };

    /** Tông phản hồi: mỗi thuộc tính tham chiếu đúng bậc màu nguyên thủy (DS 5.2). */
    private static final String[][] TONG = {
        {"ds_color_tone_info_bg", "@color/ds_color_blue_50"},
        {"ds_color_tone_info_border", "@color/ds_color_blue_100"},
        {"ds_color_tone_info_text", "@color/ds_color_blue_700"},
        {"ds_color_tone_info_icon", "@color/ds_color_blue_600"},
        {"ds_color_tone_info_solid", "@color/ds_color_blue_600"},
        {"ds_color_tone_info_on_solid", "@color/ds_color_gray_0"},
        {"ds_color_tone_success_bg", "@color/ds_color_green_50"},
        {"ds_color_tone_success_border", "@color/ds_color_green_100"},
        {"ds_color_tone_success_text", "@color/ds_color_green_700"},
        {"ds_color_tone_success_icon", "@color/ds_color_green_600"},
        {"ds_color_tone_success_solid", "@color/ds_color_green_600"},
        {"ds_color_tone_success_on_solid", "@color/ds_color_gray_0"},
        {"ds_color_tone_warning_bg", "@color/ds_color_amber_50"},
        {"ds_color_tone_warning_border", "@color/ds_color_amber_100"},
        {"ds_color_tone_warning_text", "@color/ds_color_amber_700"},
        {"ds_color_tone_warning_icon", "@color/ds_color_amber_600"},
        {"ds_color_tone_warning_solid", "@color/ds_color_amber_600"},
        {"ds_color_tone_warning_on_solid", "@color/ds_color_gray_0"},
        {"ds_color_tone_danger_bg", "@color/ds_color_red_50"},
        {"ds_color_tone_danger_border", "@color/ds_color_red_100"},
        {"ds_color_tone_danger_text", "@color/ds_color_red_700"},
        {"ds_color_tone_danger_icon", "@color/ds_color_red_600"},
        {"ds_color_tone_danger_solid", "@color/ds_color_red_600"},
        {"ds_color_tone_danger_on_solid", "@color/ds_color_gray_0"},
        {"ds_color_tone_neutral_bg", "@color/ds_color_gray_100"},
        {"ds_color_tone_neutral_border", "@color/ds_color_gray_300"},
        {"ds_color_tone_neutral_text", "@color/ds_color_gray_700"},
        {"ds_color_tone_neutral_icon", "@color/ds_color_gray_600"},
        {"ds_color_tone_neutral_solid", "@color/ds_color_gray_700"},
        {"ds_color_tone_neutral_on_solid", "@color/ds_color_gray_0"},
    };

    /** Token kích thước dùng chung và riêng Android (DS 4.2 đến 4.4, 4.7). */
    private static final String[][] KICH_THUOC = {
        {"ds_space_1", "4dp"},
        {"ds_space_2", "8dp"},
        {"ds_space_3", "12dp"},
        {"ds_space_4", "16dp"},
        {"ds_space_6", "24dp"},
        {"ds_space_12", "48dp"},
        {"ds_radius_sm", "4dp"},
        {"ds_radius_md", "8dp"},
        {"ds_radius_lg", "12dp"},
        {"ds_radius_full", "9999dp"},
        {"ds_border_width_default", "1dp"},
        {"ds_border_width_strong", "2dp"},
        {"ds_border_width_accent", "4dp"},
        {"ds_size_touch_min", "48dp"},
        {"ds_size_button_height", "48dp"},
        {"ds_size_input_height", "56dp"},
        {"ds_size_chip_height", "32dp"},
        {"ds_size_list_row_min", "56dp"},
        {"ds_size_appbar_height", "56dp"},
        {"ds_size_bottomnav_height", "56dp"},
        {"ds_size_photo_thumb", "88dp"},
        {"ds_size_dialog_max", "560dp"},
        {"ds_size_icon_sm", "16dp"},
        {"ds_size_icon_md", "20dp"},
        {"ds_size_icon_lg", "24dp"},
        {"ds_size_badge_height", "24dp"},
        {"ds_size_state_max", "480dp"},
        {"ds_font_size_12", "12sp"},
        {"ds_font_size_14", "14sp"},
        {"ds_font_size_16", "16sp"},
        {"ds_font_size_20", "20sp"},
        {"ds_font_size_24", "24sp"},
        {"ds_font_size_28", "28sp"},
        {"ds_font_leading_16", "16sp"},
        {"ds_font_leading_20", "20sp"},
        {"ds_font_leading_24", "24sp"},
        {"ds_font_leading_28", "28sp"},
        {"ds_font_leading_32", "32sp"},
        {"ds_font_leading_36", "36sp"},
    };

    private static final Pattern MA_MAU_THO = Pattern.compile("#[0-9A-Fa-f]{3,8}\\b");
    // Trong Java chỉ bắt mã màu viết thành chuỗi hoặc hằng ARGB; không bắt "#" trong chú thích Javadoc
    // như {@code #ban} vì ban là các ký tự hệ mười sáu hợp lệ.
    private static final Pattern MA_MAU_THO_JAVA =
            Pattern.compile("\"#[0-9A-Fa-f]{3,8}\"|\\b0x[0-9A-Fa-f]{8}\\b");
    private static final Pattern DON_VI_THO = Pattern.compile("(\\d+(?:\\.\\d+)?)(dp|sp|dip|px)\\b");
    private static final Pattern MAU_NGUYEN_THUY_THAM_CHIEU =
            Pattern.compile("@color/ds_color_(gray|blue|green|amber|red)_\\d+");

    private static Map<String, String> docTheo(String tep, String the) {
        Document tai = TaiNguyen.docXml(tep);
        NodeList ds = tai.getElementsByTagName(the);
        Map<String, String> ketQua = new LinkedHashMap<>();
        for (int i = 0; i < ds.getLength(); i++) {
            Element e = (Element) ds.item(i);
            ketQua.put(e.getAttribute("name"), e.getTextContent().trim());
        }
        return ketQua;
    }

    @Test
    public void DXDS02_MauNguyenThuy_DungGiaTriDesignSystem() {
        Map<String, String> mau = docTheo(RES + "/values/ds_colors.xml", "color");
        for (String[] dong : MAU_NGUYEN_THUY) {
            assertEquals(dong[0], dong[1].toUpperCase(), String.valueOf(mau.get(dong[0])).toUpperCase());
        }
    }

    @Test
    public void DXDS02_MauNguHia_ChiThamChieuTokenKhongLapGiaTri() {
        Map<String, String> mau = docTheo(RES + "/values/ds_colors.xml", "color");
        Pattern nguyenThuy = Pattern.compile("ds_color_(gray|blue|green|amber|red)_\\d+");
        for (Map.Entry<String, String> m : mau.entrySet()) {
            assertTrue("Token màu phải bắt đầu bằng ds_color_: " + m.getKey(), m.getKey().startsWith("ds_color_"));
            boolean laNguyenThuy = nguyenThuy.matcher(m.getKey()).matches();
            boolean laDoMo = m.getKey().startsWith("ds_color_overlay_");
            if (!laNguyenThuy && !laDoMo) {
                assertTrue(
                        m.getKey() + " phải tham chiếu token khác, không chép mã màu: " + m.getValue(),
                        m.getValue().startsWith("@color/ds_color_"));
            }
        }
    }

    @Test
    public void DS5_Tong_NamTongSauThuocTinhDungBacNguyenThuy() {
        Map<String, String> mau = docTheo(RES + "/values/ds_colors.xml", "color");
        assertEquals(30, TONG.length);
        for (String[] dong : TONG) {
            assertEquals(dong[0], dong[1], mau.get(dong[0]));
        }
    }

    @Test
    public void DS4_KichThuoc_DungGiaTriDesignSystem() {
        Map<String, String> kichThuoc = docTheo(RES + "/values/ds_dimens.xml", "dimen");
        for (String[] dong : KICH_THUOC) {
            assertEquals(dong[0], dong[1], kichThuoc.get(dong[0]));
        }
    }

    @Test
    public void FC84_TenToken_BatDauBangDs() {
        for (String ten : docTheo(RES + "/values/ds_dimens.xml", "dimen").keySet()) {
            assertTrue(ten, ten.startsWith("ds_"));
        }
        Map<String, String> kieu = docTheo(RES + "/values/ds_type.xml", "style");
        for (String ten : kieu.keySet()) {
            assertTrue(ten, ten.startsWith("TextAppearance.Ds."));
        }
        assertEquals(10, kieu.size());
    }

    @Test
    public void TC01_TaiNguyenNgoaiTokenKhongChepThoMaMauHayKichThuoc() {
        for (File tep : TaiNguyen.cacTepDuoi(RES, ".xml")) {
            String ten = tep.getName();
            if (ten.equals("ds_colors.xml") || ten.equals("ds_dimens.xml")) {
                continue; // hai tệp token được chứa giá trị thô
            }
            String noiDung = TaiNguyen.docChu(tep);
            Matcher mau = MA_MAU_THO.matcher(noiDung);
            assertFalse(tep.getPath() + " chứa mã màu thô: " + (mau.find() ? mau.group() : ""), mau.reset().find());
            boolean laVector = tep.getParentFile().getName().equals("drawable");
            Matcher donVi = DON_VI_THO.matcher(noiDung);
            while (donVi.find()) {
                boolean laKhong = Double.parseDouble(donVi.group(1)) == 0.0 && !donVi.group(2).equals("px");
                // Chiều rộng, chiều cao nội tại của vector là kích thước vẽ, không phải token bố cục.
                if (laKhong || laVector && !donVi.group(2).equals("px")) {
                    continue;
                }
                throw new AssertionError(tep.getPath() + " chứa kích thước thô: " + donVi.group());
            }
        }
    }

    @Test
    public void TC01_TaiNguyenNgoaiTokenMauKhongThamChieuMauNguyenThuy() {
        for (File tep : TaiNguyen.cacTepDuoi(RES, ".xml")) {
            if (tep.getName().equals("ds_colors.xml")) {
                continue;
            }
            Matcher m = MAU_NGUYEN_THUY_THAM_CHIEU.matcher(TaiNguyen.docChu(tep));
            assertFalse(tep.getPath() + " dùng màu nguyên thủy: " + (m.find() ? m.group() : ""), m.reset().find());
        }
    }

    @Test
    public void FC66_MaJava_KhongCoMaMauThoPxHayParseColor() {
        Map<String, Integer> demTep = new HashMap<>();
        for (File tep : TaiNguyen.cacTepDuoi("src/main/java", ".java")) {
            String noiDung = TaiNguyen.docChu(tep);
            Matcher mau = MA_MAU_THO_JAVA.matcher(noiDung);
            assertFalse(tep.getPath() + " chứa mã màu thô: " + (mau.find() ? mau.group() : ""), mau.reset().find());
            assertFalse(tep.getPath() + " dùng parseColor", noiDung.contains("parseColor"));
            Matcher px = Pattern.compile("\\b\\d+(?:\\.\\d+)?px\\b").matcher(noiDung);
            assertFalse(tep.getPath() + " chứa kích thước px: " + (px.find() ? px.group() : ""), px.reset().find());
            demTep.put(tep.getName(), noiDung.length());
        }
        assertTrue("Phải quét được mã nguồn", demTep.size() > 10);
    }
}
