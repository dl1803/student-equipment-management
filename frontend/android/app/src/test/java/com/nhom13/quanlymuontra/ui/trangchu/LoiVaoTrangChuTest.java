package com.nhom13.quanlymuontra.ui.trangchu;

import static org.junit.Assert.assertEquals;

import com.nhom13.quanlymuontra.R;
import com.nhom13.quanlymuontra.TaiNguyen;
import java.util.HashMap;
import java.util.Map;
import org.junit.Test;
import org.w3c.dom.Document;
import org.w3c.dom.Element;
import org.w3c.dom.NodeList;

/** Kiểm tám lối vào của SCR-SV-01: đúng thứ tự Wireframes, chữ nguyên văn UI-Screens 5.1. */
public class LoiVaoTrangChuTest {

    // {tên giá trị enum, khóa chuỗi, chữ nguyên văn UI-Screens 5.1}
    private static final String[][] BANG = {
        {"TRA_CUU", "trang_chu_loi_vao_tra_cuu", "Tra cứu thiết bị"},
        {"DAT_TRUOC", "trang_chu_loi_vao_dat_truoc", "Đặt trước thiết bị"},
        {"PHIEU_YEU_CAU", "trang_chu_loi_vao_phieu_yeu_cau", "Phiếu yêu cầu của tôi"},
        {"DANG_KY_CHO", "trang_chu_loi_vao_dang_ky_cho", "Đăng ký chờ của tôi"},
        {"LICH_SU", "trang_chu_loi_vao_lich_su", "Lịch sử mượn/trả"},
        {"BIEN_BAN", "trang_chu_loi_vao_bien_ban", "Biên bản và phí phạt"},
        {"VI_PHAM", "trang_chu_loi_vao_vi_pham", "Vi phạm"},
        {"HO_SO", "trang_chu_loi_vao_ho_so", "Hồ sơ mượn"},
    };

    private static Map<String, String> docChuoi() {
        Document tai = TaiNguyen.docXml("src/main/res/values/strings.xml");
        NodeList ds = tai.getElementsByTagName("string");
        Map<String, String> ketQua = new HashMap<>();
        for (int i = 0; i < ds.getLength(); i++) {
            Element e = (Element) ds.item(i);
            ketQua.put(e.getAttribute("name"), e.getTextContent().trim());
        }
        return ketQua;
    }

    @Test
    public void UI51_TamLoiVao_DungThuTuTenVaChuNguyenVan() throws Exception {
        LoiVaoTrangChu[] dsLoiVao = LoiVaoTrangChu.values();
        Map<String, String> chuoi = docChuoi();

        assertEquals(8, dsLoiVao.length);
        for (int i = 0; i < BANG.length; i++) {
            assertEquals(BANG[i][0], dsLoiVao[i].name());
            assertEquals(
                    BANG[i][1],
                    R.string.class.getField(BANG[i][1]).getInt(null),
                    dsLoiVao[i].getNhanResId());
            assertEquals(BANG[i][1], BANG[i][2], chuoi.get(BANG[i][1]));
        }
    }
}
