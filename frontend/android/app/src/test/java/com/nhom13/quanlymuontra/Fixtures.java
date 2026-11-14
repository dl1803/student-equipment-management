package com.nhom13.quanlymuontra;

import java.io.IOException;
import java.io.InputStream;
import java.io.UncheckedIOException;
import java.nio.charset.StandardCharsets;

/** Đọc fixture trong {@code src/test/resources/fixtures} làm chuỗi UTF-8. */
public final class Fixtures {

    private Fixtures() {}

    /**
     * @param duongDan đường dẫn tương đối trong thư mục {@code fixtures}, ví dụ {@code
     *     loi/401-chua-xac-thuc.json}
     */
    public static String doc(String duongDan) {
        try (InputStream luong =
                Fixtures.class.getClassLoader().getResourceAsStream("fixtures/" + duongDan)) {
            if (luong == null) {
                throw new IllegalStateException("Không tìm thấy fixture: " + duongDan);
            }
            return new String(luong.readAllBytes(), StandardCharsets.UTF_8);
        } catch (IOException e) {
            throw new UncheckedIOException(e);
        }
    }
}
