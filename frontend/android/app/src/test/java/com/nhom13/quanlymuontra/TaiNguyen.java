package com.nhom13.quanlymuontra;

import java.io.File;
import java.io.IOException;
import java.io.UncheckedIOException;
import java.nio.charset.StandardCharsets;
import java.nio.file.Files;
import java.util.ArrayList;
import java.util.List;
import java.util.stream.Stream;
import javax.xml.parsers.DocumentBuilderFactory;
import org.w3c.dom.Document;

/**
 * Đọc tệp trong {@code src/main} của module app khi chạy kiểm thử trên JVM, để kiểm tài nguyên và mã
 * nguồn mà không cần thiết bị. Đường dẫn tính từ thư mục module; chạy từ thư mục gốc dự án (có
 * {@code app/}) cũng được.
 */
public final class TaiNguyen {

    private TaiNguyen() {}

    /** @param duongDanTuongDoi tương đối so với thư mục module, ví dụ {@code src/main/res/values} */
    public static File tep(String duongDanTuongDoi) {
        File tep = new File(duongDanTuongDoi);
        if (!tep.exists()) {
            tep = new File("app", duongDanTuongDoi);
        }
        if (!tep.exists()) {
            throw new IllegalStateException("Không tìm thấy: " + duongDanTuongDoi);
        }
        return tep;
    }

    public static String docChu(String duongDanTuongDoi) {
        try {
            return new String(Files.readAllBytes(tep(duongDanTuongDoi).toPath()), StandardCharsets.UTF_8);
        } catch (IOException e) {
            throw new UncheckedIOException(e);
        }
    }

    public static Document docXml(String duongDanTuongDoi) {
        try {
            return DocumentBuilderFactory.newInstance()
                    .newDocumentBuilder()
                    .parse(tep(duongDanTuongDoi));
        } catch (Exception e) {
            throw new IllegalStateException("Không đọc được XML: " + duongDanTuongDoi, e);
        }
    }

    /** Mọi tệp có đuôi {@code hauTo} dưới thư mục {@code thuMuc} (đệ quy), sắp theo đường dẫn. */
    public static List<File> cacTepDuoi(String thuMuc, String hauTo) {
        try (Stream<java.nio.file.Path> luong = Files.walk(tep(thuMuc).toPath())) {
            List<File> ketQua = new ArrayList<>();
            luong.filter(Files::isRegularFile)
                    .filter(p -> p.getFileName().toString().endsWith(hauTo))
                    .sorted()
                    .forEach(p -> ketQua.add(p.toFile()));
            return ketQua;
        } catch (IOException e) {
            throw new UncheckedIOException(e);
        }
    }

    public static String docChu(File tep) {
        try {
            return new String(Files.readAllBytes(tep.toPath()), StandardCharsets.UTF_8);
        } catch (IOException e) {
            throw new UncheckedIOException(e);
        }
    }
}
