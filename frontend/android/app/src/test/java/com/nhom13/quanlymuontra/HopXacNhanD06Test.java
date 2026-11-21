package com.nhom13.quanlymuontra;

import java.util.ArrayList;
import java.util.Arrays;
import java.util.List;
import org.junit.Test;
import org.w3c.dom.Document;
import org.w3c.dom.Element;
import static com.nhom13.quanlymuontra.HoTroXml.cacPhanTu;
import static com.nhom13.quanlymuontra.HoTroXml.maId;
import static org.junit.Assert.assertEquals;
import static org.junit.Assert.assertTrue;

/**
 * Bảo vệ cấu trúc CMP-26 Hộp xác nhận (DS 9.2) bằng cách đọc tài nguyên và mã nguồn (JVM). Đây là bảo vệ cấu trúc,
 * không chứng minh hành vi trên thiết bị (chạm ngoài, Back, tiêu điểm): các hành vi đó kiểm tay khi có màn hình đầu
 * tiên dùng hộp.
 */
public class HopXacNhanD06Test {

    private static final String RES = "src/main/res";


    @Test
    public void DS92_HopXacNhan_NutHuyDungTruocDeTieuDiemBanDauORuaHuy() {
        Document tai = TaiNguyen.docXml(RES + "/layout/dialog_xac_nhan.xml");
        List<String> thuTu = new ArrayList<>();
        for (Element e : cacPhanTu(tai, "com.google.android.material.button.MaterialButton")) {
            thuTu.add(maId(e.getAttribute("android:id")));
        }

        assertEquals(Arrays.asList("btn_huy", "btn_xac_nhan", "btn_xac_nhan_huy_bo"), thuTu);
    }


    @Test
    public void DS92_HopXacNhan_KhongDongKhiChamNgoaiVaKhoaBackKhiDangXuLy() {
        String ma =
                TaiNguyen.docChu("src/main/java/com/nhom13/quanlymuontra/common/widget/XacNhanDialogFragment.java");

        assertTrue(ma.contains("setCanceledOnTouchOutside(false)"));
        assertTrue(ma.contains("OnBackPressedCallback"));
        assertTrue(ma.contains("if (!dangXuLy)"));
        assertTrue(ma.contains("NutDangXuLy.batDau(nutXacNhan)"));
    }
}
