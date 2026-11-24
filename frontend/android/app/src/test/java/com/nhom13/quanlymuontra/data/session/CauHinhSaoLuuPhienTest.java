package com.nhom13.quanlymuontra.data.session;

import static org.junit.Assert.assertEquals;
import static org.junit.Assert.assertTrue;

import java.io.File;
import java.io.IOException;
import javax.xml.parsers.DocumentBuilderFactory;
import javax.xml.parsers.ParserConfigurationException;
import org.junit.Test;
import org.w3c.dom.Document;
import org.w3c.dom.Element;
import org.w3c.dom.NodeList;
import org.xml.sax.SAXException;

/**
 * Kiểm cấu hình loại tệp phiên khỏi sao lưu và chuyển thiết bị (FA 5.4, FC 6.5, ĐX-FA-14) bằng cách
 * đọc thẳng manifest và hai tệp quy tắc trong {@code src/main}. Đây chỉ kiểm nội dung cấu hình;
 * việc hệ điều hành thật sự loại tệp khỏi sao lưu được kiểm thủ công trên thiết bị (FC 9.4).
 */
public class CauHinhSaoLuuPhienTest {

    private static final String NS_ANDROID = "http://schemas.android.com/apk/res/android";
    private static final String TEP_PHIEN = TokenStore.TEN_TEP + ".xml";

    @Test
    public void DXFA14_Manifest_TatAllowBackupVaTroDenHaiTepQuyTac() throws Exception {
        Element application =
                (Element)
                        docXml("src/main/AndroidManifest.xml")
                                .getElementsByTagName("application")
                                .item(0);

        assertEquals("false", application.getAttributeNS(NS_ANDROID, "allowBackup"));
        assertEquals(
                "@xml/backup_rules", application.getAttributeNS(NS_ANDROID, "fullBackupContent"));
        assertEquals(
                "@xml/data_extraction_rules",
                application.getAttributeNS(NS_ANDROID, "dataExtractionRules"));
    }

    @Test
    public void DXFA14_BackupRules_LoaiTepPhienKhoiSaoLuuTuDong() throws Exception {
        Document tai = docXml("src/main/res/xml/backup_rules.xml");

        Element goc = tai.getDocumentElement();

        assertEquals("full-backup-content", goc.getTagName());
        assertTrue(coLoaiTepPhien(goc));
    }

    @Test
    public void DXFA14_DataExtractionRules_LoaiTepPhienKhoiDamMayVaChuyenThietBi()
            throws Exception {
        Element goc = docXml("src/main/res/xml/data_extraction_rules.xml").getDocumentElement();

        assertEquals("data-extraction-rules", goc.getTagName());
        assertTrue(coLoaiTepPhien(phanTu(goc, "cloud-backup")));
        assertTrue(coLoaiTepPhien(phanTu(goc, "device-transfer")));
    }

    private static boolean coLoaiTepPhien(Element cha) {
        NodeList dsExclude = cha.getElementsByTagName("exclude");
        for (int i = 0; i < dsExclude.getLength(); i++) {
            Element exclude = (Element) dsExclude.item(i);
            if ("sharedpref".equals(exclude.getAttribute("domain"))
                    && TEP_PHIEN.equals(exclude.getAttribute("path"))) {
                return true;
            }
        }
        return false;
    }

    private static Element phanTu(Element cha, String ten) {
        NodeList ds = cha.getElementsByTagName(ten);
        assertEquals("Phải có đúng một phần tử " + ten, 1, ds.getLength());
        return (Element) ds.item(0);
    }

    // Gradle chạy kiểm thử với thư mục làm việc là thư mục module app; thử thêm "app/" để chạy từ
    // thư mục gốc dự án trong IDE.
    private static Document docXml(String duongDanTuongDoi)
            throws ParserConfigurationException, IOException, SAXException {
        File tep = new File(duongDanTuongDoi);
        if (!tep.isFile()) {
            tep = new File("app", duongDanTuongDoi);
        }
        assertTrue("Không tìm thấy tệp cấu hình: " + duongDanTuongDoi, tep.isFile());
        DocumentBuilderFactory nhaMay = DocumentBuilderFactory.newInstance();
        nhaMay.setNamespaceAware(true);
        return nhaMay.newDocumentBuilder().parse(tep);
    }
}
