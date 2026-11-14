package com.nhom13.quanlymuontra.generated;

import static org.junit.Assert.assertEquals;
import static org.junit.Assert.assertFalse;
import static org.junit.Assert.assertTrue;

import com.google.gson.Gson;
import com.google.gson.JsonArray;
import com.google.gson.JsonElement;
import com.google.gson.JsonObject;
import com.google.gson.JsonParser;
import com.nhom13.quanlymuontra.Fixtures;
import com.nhom13.quanlymuontra.generated.model.PhieuMuonTomTatPage;
import com.nhom13.quanlymuontra.generated.model.ProblemDetails;
import com.nhom13.quanlymuontra.generated.model.SinhVienTheInfo;
import com.nhom13.quanlymuontra.generated.model.ValidationProblemDetails;
import java.util.Map;
import org.junit.Test;

/**
 * Kiểm chứng bộ fixture đúng hợp đồng (D02-L): mỗi fixture giải mã được bằng mô hình sinh và không
 * có trường nào ngoài hợp đồng.
 */
public class FixtureHopDongTest {

    private static final Gson GSON = new JSON().getGson();
    private static final String MA_TRUY_VET =
            "00-4bf92f3577b34da6a3ce929d0e0e4736-00f067aa0ba902b7-00";

    private static final Object[][] CAC_FIXTURE_JSON = {
        {"thanh-cong/sv-toi-bi-chan.json", SinhVienTheInfo.class},
        {"thanh-cong/sv-ds-phieu-muon.json", PhieuMuonTomTatPage.class},
        {"rong/sv-ds-phieu-muon.json", PhieuMuonTomTatPage.class},
        {"loi/400-validation.json", ValidationProblemDetails.class},
        {"loi/401-chua-xac-thuc.json", ProblemDetails.class},
        {"loi/403-khong-du-quyen.json", ProblemDetails.class},
        {"loi/409-so-luong-kha-dung-thay-doi.json", ProblemDetails.class},
        {"loi/422-sv-bi-khoa-quyen-muon.json", ProblemDetails.class},
        {"loi/429-qua-nhieu-yeu-cau.json", ProblemDetails.class},
    };

    // Giải mã rồi mã hóa lại: trường nào không có trong mô hình sinh từ hợp đồng sẽ biến mất nên
    // hai cây JSON lệch nhau. Giá trị null và trường vắng mặt được coi là như nhau.
    @Test
    public void FC93_MoiFixtureJson_KhongCoTruongNgoaiHopDong() {
        for (Object[] muc : CAC_FIXTURE_JSON) {
            String tep = (String) muc[0];
            Class<?> lop = (Class<?>) muc[1];
            String goc = Fixtures.doc(tep);

            Object doiTuong = GSON.fromJson(goc, lop);
            JsonElement khoiPhuc = boNull(GSON.toJsonTree(doiTuong));

            assertEquals(tep, boNull(JsonParser.parseString(goc)), khoiPhuc);
        }
    }

    @Test
    public void FC93_FixtureThanhCong_GiaiMaDungGiaTri() {
        SinhVienTheInfo sinhVien =
                GSON.fromJson(
                        Fixtures.doc("thanh-cong/sv-toi-bi-chan.json"), SinhVienTheInfo.class);
        PhieuMuonTomTatPage trang =
                GSON.fromJson(
                        Fixtures.doc("thanh-cong/sv-ds-phieu-muon.json"),
                        PhieuMuonTomTatPage.class);

        assertEquals("23520001", sinhVien.getMaSV());
        assertEquals(Boolean.FALSE, sinhVien.getHopLe());
        assertEquals(1, sinhVien.getDsBienBanChuaNop().size());
        assertEquals("SV_BI_KHOA_QUYEN_MUON", sinhVien.getDsLyDoKhongHopLe().get(0).getMaLoi());
        assertEquals(Integer.valueOf(2), trang.getTongSo());
        assertEquals("DANG_MUON", trang.getDanhSach().get(0).getTrangThai());
        assertEquals("DA_TRA", trang.getDanhSach().get(1).getTrangThai());
    }

    @Test
    public void FC93_FixtureRong_DanhSachRongVaTongSoBangKhong() {
        PhieuMuonTomTatPage trang =
                GSON.fromJson(
                        Fixtures.doc("rong/sv-ds-phieu-muon.json"), PhieuMuonTomTatPage.class);

        assertEquals(Integer.valueOf(0), trang.getTongSo());
        assertTrue(trang.getDanhSach().isEmpty());
    }

    @Test
    public void FC93_FixtureLoi_GiaiMaThanhProblemDetailsGiuNguyenStatusVaMaLoi() {
        Object[][] cacLoi = {
            {"loi/401-chua-xac-thuc.json", 401, "CHUA_XAC_THUC"},
            {"loi/403-khong-du-quyen.json", 403, "KHONG_DU_QUYEN"},
            {"loi/409-so-luong-kha-dung-thay-doi.json", 409, "SO_LUONG_KHA_DUNG_THAY_DOI"},
            {"loi/422-sv-bi-khoa-quyen-muon.json", 422, "SV_BI_KHOA_QUYEN_MUON"},
            {"loi/429-qua-nhieu-yeu-cau.json", 429, "QUA_NHIEU_YEU_CAU"},
        };

        for (Object[] muc : cacLoi) {
            String tep = (String) muc[0];

            ProblemDetails loi = GSON.fromJson(Fixtures.doc(tep), ProblemDetails.class);

            assertEquals(tep, Integer.valueOf((Integer) muc[1]), loi.getStatus());
            assertEquals(tep, (String) muc[2], loi.getMaLoi());
            assertEquals(tep, MA_TRUY_VET, loi.getTraceId());
        }
    }

    @Test
    public void FC93_FixtureValidation_CoErrorsTheoKhoaTruong() {
        ValidationProblemDetails loi =
                GSON.fromJson(
                        Fixtures.doc("loi/400-validation.json"), ValidationProblemDetails.class);

        assertEquals("DU_LIEU_KHONG_HOP_LE", loi.getMaLoi());
        assertEquals(1, loi.getErrors().get("dsLoaiVaSoLuong[0].soLuong").size());
    }

    @Test
    public void FC93_FixtureKhongPhaiJson_KhongBatDauBangNgoacNhon() {
        String noiDung = Fixtures.doc("loi/khong-phai-json.html").trim();

        assertFalse(noiDung.startsWith("{"));
        assertTrue(noiDung.startsWith("<"));
    }

    private static JsonElement boNull(JsonElement phanTu) {
        if (phanTu.isJsonObject()) {
            JsonObject ketQua = new JsonObject();
            for (Map.Entry<String, JsonElement> muc : phanTu.getAsJsonObject().entrySet()) {
                if (!muc.getValue().isJsonNull()) {
                    ketQua.add(muc.getKey(), boNull(muc.getValue()));
                }
            }
            return ketQua;
        }
        if (phanTu.isJsonArray()) {
            JsonArray ketQua = new JsonArray();
            for (JsonElement con : phanTu.getAsJsonArray()) {
                ketQua.add(boNull(con));
            }
            return ketQua;
        }
        return phanTu;
    }
}
