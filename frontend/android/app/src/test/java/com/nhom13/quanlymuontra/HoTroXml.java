package com.nhom13.quanlymuontra;

import java.util.ArrayList;
import java.util.List;
import org.w3c.dom.Document;
import org.w3c.dom.Element;
import org.w3c.dom.NodeList;

/** Dụng cụ đọc XML dùng chung cho các test cấu trúc tài nguyên (D06-L). */
public final class HoTroXml {

    private HoTroXml() {}

    /** Mọi phần tử có tên thẻ {@code the} trong tài liệu. */
    public static List<Element> cacPhanTu(Document tai, String the) {
        NodeList ds = tai.getElementsByTagName(the);
        List<Element> ketQua = new ArrayList<>();
        for (int i = 0; i < ds.getLength(); i++) {
            ketQua.add((Element) ds.item(i));
        }
        return ketQua;
    }

    /** Bỏ tiền tố {@code @+id/} hoặc {@code @id/}. */
    public static String maId(String giaTri) {
        return giaTri.replace("@+id/", "").replace("@id/", "");
    }
}
