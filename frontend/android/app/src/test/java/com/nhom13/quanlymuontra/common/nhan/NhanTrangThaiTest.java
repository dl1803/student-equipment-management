package com.nhom13.quanlymuontra.common.nhan;

import static org.junit.Assert.assertEquals;
import static org.junit.Assert.assertFalse;
import static org.junit.Assert.assertTrue;

import com.nhom13.quanlymuontra.R;
import com.nhom13.quanlymuontra.TaiNguyen;
import java.text.Normalizer;
import java.util.ArrayList;
import java.util.HashMap;
import java.util.HashSet;
import java.util.List;
import java.util.Map;
import java.util.Set;
import org.junit.Test;
import org.w3c.dom.Document;
import org.w3c.dom.Element;
import org.w3c.dom.NodeList;

/**
 * Kiểm bảng nhãn trạng thái theo DS 7.1 đến 7.3 và FC 7.3, 9.3. Bảng kỳ vọng dưới đây chép độc lập
 * từ DS 7.2 và 7.3 để đối chiếu với mã nguồn; nhãn được đọc thẳng từ {@code strings.xml}. Độ phủ so
 * với hợp đồng được kiểm bằng cách đọc enum trong {@code app/openapi/openapi.yaml}.
 */
public class NhanTrangThaiTest {

    /** Họ, mã API, nhãn, tông, nhấn đậm (DS 7.2). */
    private static final String[][] BANG_KY_VONG = {
        {"TINH_TRANG_THIET_BI", "SAN_SANG", "Sẵn sàng cho mượn", "SUCCESS", "false"},
        {"TINH_TRANG_THIET_BI", "DANG_DUOC_MUON", "Đang được mượn", "INFO", "false"},
        {"TINH_TRANG_THIET_BI", "DANG_BAO_TRI_SUA_CHUA", "Đang bảo trì/sửa chữa", "WARNING", "false"},
        {"TINH_TRANG_THIET_BI", "DA_THANH_LY", "Đã thanh lý", "NEUTRAL", "false"},
        {"TINH_TRANG_THIET_BI", "DA_BAO_MAT", "Đã báo mất", "DANGER", "false"},
        {"PHIEU_YEU_CAU", "CHO_DUYET", "Chờ duyệt", "WARNING", "false"},
        {"PHIEU_YEU_CAU", "DA_DUYET", "Đã duyệt", "SUCCESS", "false"},
        {"PHIEU_YEU_CAU", "TU_CHOI", "Từ chối", "DANGER", "false"},
        {"PHIEU_YEU_CAU", "DA_HUY", "Đã hủy", "NEUTRAL", "false"},
        {"PHIEU_YEU_CAU", "HET_HAN", "Hết hạn", "NEUTRAL", "false"},
        {"PHIEU_YEU_CAU", "DA_NHAN", "Đã nhận", "SUCCESS", "false"},
        {"PHIEU_MUON", "DANG_MUON", "Đang mượn", "INFO", "false"},
        {"PHIEU_MUON", "QUA_HAN", "Quá hạn", "DANGER", "true"},
        {"PHIEU_MUON", "DA_TRA", "Đã trả", "SUCCESS", "false"},
        {"DANG_KY_CHO", "DANG_CHO", "Đang chờ", "INFO", "false"},
        {"DANG_KY_CHO", "DA_THONG_BAO", "Đã thông báo", "SUCCESS", "false"},
        {"DANG_KY_CHO", "DA_NHAN", "Đã nhận", "SUCCESS", "false"},
        {"DANG_KY_CHO", "DA_HUY", "Đã hủy", "NEUTRAL", "false"},
        {"DANG_KY_CHO", "HET_HAN", "Hết hạn", "NEUTRAL", "false"},
        {"KET_QUA_KIEM_TRA", "NGUYEN_VEN", "Nguyên vẹn", "SUCCESS", "false"},
        {"KET_QUA_KIEM_TRA", "HU_HONG", "Hư hỏng", "DANGER", "false"},
        {"KET_QUA_KIEM_TRA", "THIEU_PHU_KIEN", "Thiếu phụ kiện", "WARNING", "false"},
        {"KET_QUA_KIEM_TRA", "MAT", "Mất", "DANGER", "true"},
        {"BIEN_BAN", "CHUA_NOP", "Chưa nộp", "WARNING", "false"},
        {"BIEN_BAN", "DA_NOP", "Đã nộp", "SUCCESS", "false"},
        {"BIEN_BAN", "DA_HUY", "Đã hủy", "NEUTRAL", "false"},
        {"LOAI_VI_PHAM", "TRA_TRE", "Trả trễ", "WARNING", "false"},
        {"LOAI_VI_PHAM", "HU_HONG", "Hư hỏng", "DANGER", "false"},
        {"LOAI_VI_PHAM", "MAT", "Mất", "DANGER", "false"},
        {"LOAI_VI_PHAM", "KHONG_DEN_NHAN", "Không đến nhận", "WARNING", "false"},
        {"TRANG_THAI_HOC", "DANG_HOC", "Đang học", "SUCCESS", "false"},
        {"TRANG_THAI_HOC", "TOT_NGHIEP", "Tốt nghiệp", "NEUTRAL", "false"},
        {"TRANG_THAI_HOC", "THOI_HOC", "Thôi học", "DANGER", "false"},
        {"TRANG_THAI_HOC", "DINH_CHI", "Đình chỉ", "DANGER", "false"},
        {"QUYEN_MUON", "BINH_THUONG", "Bình thường", "SUCCESS", "false"},
        {"QUYEN_MUON", "TAM_KHOA", "Tạm khóa", "WARNING", "true"},
        {"QUYEN_MUON", "CAM_MUON_DAI_HAN", "Cấm mượn dài hạn", "DANGER", "true"},
        {"TAI_KHOAN", "HOAT_DONG", "Hoạt động", "SUCCESS", "false"},
        {"TAI_KHOAN", "TAM_KHOA_DANG_NHAP", "Tạm khóa đăng nhập", "WARNING", "false"},
        {"TAI_KHOAN", "BI_KHOA", "Bị khóa", "DANGER", "false"},
        {"LOAI_BAO_TRI", "BAO_TRI_DINH_KY", "Bảo trì định kỳ", "NEUTRAL", "false"},
        {"LOAI_BAO_TRI", "SUA_CHUA", "Sửa chữa", "NEUTRAL", "false"},
        {"KET_QUA_BAO_TRI", "DUA_VE_SAN_SANG", "Đưa về Sẵn sàng", "SUCCESS", "false"},
        {"KET_QUA_BAO_TRI", "DE_XUAT_THANH_LY", "Đề xuất thanh lý", "WARNING", "false"},
        {"NGUON_GIAO", "PHIEU_YEU_CAU", "Phiếu yêu cầu", "NEUTRAL", "false"},
        {"NGUON_GIAO", "DANG_KY_CHO", "Danh sách chờ", "NEUTRAL", "false"},
        {"NGUON_GIAO", "TRUC_TIEP", "Mượn trực tiếp", "NEUTRAL", "false"},
        {"LOAI_THONG_BAO", "NHAC_HAN_TRA", "Nhắc hạn trả", "WARNING", "false"},
        {"LOAI_THONG_BAO", "QUA_HAN", "Quá hạn", "DANGER", "false"},
        {"LOAI_THONG_BAO", "THIET_BI_CHO_SAN_SANG", "Thiết bị chờ sẵn sàng", "SUCCESS", "false"},
        {"LOAI_THONG_BAO", "DUYET_HOAC_TU_CHOI", "Duyệt hoặc từ chối", "INFO", "false"},
        {"LOAI_THONG_BAO", "VI_PHAM_PHI_PHAT", "Vi phạm, phí phạt", "DANGER", "false"},
        {"LOAI_THONG_BAO", "NHAC_NHO_NV", "Nhắc nhở của NV", "INFO", "false"},
        {"LOAI_THONG_BAO", "NHAC_BAO_TRI", "Nhắc bảo trì", "NEUTRAL", "false"},
    };

    /** Tên hằng NhanPhatSinh, nhãn, tông, nhấn đậm (DS 7.3). */
    private static final String[][] PHAT_SINH_KY_VONG = {
        {"HET_HANG", "Hết hàng", "DANGER", "false"},
        {"KHONG_DU", "Không đủ", "DANGER", "false"},
        {"HOP_LE", "Hợp lệ", "SUCCESS", "true"},
        {"KHONG_HOP_LE", "Không hợp lệ", "DANGER", "true"},
        {"DUOC_DUYET", "Được duyệt", "SUCCESS", "true"},
        {"KHONG_DUOC_DUYET", "Không được duyệt", "DANGER", "true"},
        {"DA_NGUNG_SU_DUNG", "Đã ngưng sử dụng", "NEUTRAL", "false"},
        {"DANG_SU_DUNG", "Đang sử dụng", "SUCCESS", "false"},
        {"DA_TAT", "Đã tắt", "NEUTRAL", "false"},
        {"CHUA_TRA", "Chưa trả", "NEUTRAL", "false"},
        {"BAO_TRI", "Bảo trì", "NEUTRAL", "false"},
        {"THANH_LY", "Thanh lý", "NEUTRAL", "false"},
        {"TAM_TINH", "Tạm tính", "WARNING", "false"},
    };

    /** Họ trạng thái và tên schema enum tương ứng trong openapi.yaml; NGUON_GIAO không có enum. */
    private static final String[][] HO_VA_SCHEMA = {
        {"TINH_TRANG_THIET_BI", "TinhTrangThietBi"},
        {"PHIEU_YEU_CAU", "TrangThaiPhieuYeuCauMuon"},
        {"PHIEU_MUON", "TrangThaiPhieuMuon"},
        {"DANG_KY_CHO", "TrangThaiDangKyCho"},
        {"KET_QUA_KIEM_TRA", "KetQuaKiemTra"},
        {"BIEN_BAN", "TrangThaiBienBan"},
        {"LOAI_VI_PHAM", "LoaiViPham"},
        {"TRANG_THAI_HOC", "TrangThaiHoc"},
        {"QUYEN_MUON", "TrangThaiQuyenMuon"},
        {"TAI_KHOAN", "TrangThaiKhoa"},
        {"LOAI_BAO_TRI", "LoaiThucHienBaoTri"},
        {"KET_QUA_BAO_TRI", "KetQuaBaoTri"},
        {"LOAI_THONG_BAO", "LoaiThongBao"},
    };

    private static String khoa(String ho, String ma) {
        return "nhan_" + ho.toLowerCase() + "_" + ma.toLowerCase();
    }

    private static String nfc(String chu) {
        return Normalizer.normalize(chu, Normalizer.Form.NFC);
    }

    /** Đọc mọi {@code <string>} của strings.xml thành bản đồ tên sang chữ (chuẩn hóa NFC). */
    private static Map<String, String> docStrings() {
        Document tai = TaiNguyen.docXml("src/main/res/values/strings.xml");
        NodeList ds = tai.getElementsByTagName("string");
        Map<String, String> ketQua = new HashMap<>();
        for (int i = 0; i < ds.getLength(); i++) {
            Element e = (Element) ds.item(i);
            String ten = e.getAttribute("name");
            assertFalse("Trùng tên chuỗi: " + ten, ketQua.containsKey(ten));
            ketQua.put(ten, nfc(e.getTextContent()));
        }
        return ketQua;
    }

    private static int idChuoi(String ten) throws Exception {
        return R.string.class.getField(ten).getInt(null);
    }

    /** Các giá trị enum của một schema trong openapi.yaml, theo đúng thứ tự khai báo. */
    private static List<String> docEnum(String schema) {
        String[] dong = TaiNguyen.docChu("openapi/openapi.yaml").split("\\R");
        List<String> ketQua = new ArrayList<>();
        boolean trongSchema = false;
        boolean trongEnum = false;
        for (String d : dong) {
            if (!trongSchema) {
                trongSchema = d.equals("    " + schema + ":");
                continue;
            }
            if (!trongEnum) {
                if (d.equals("      enum:")) {
                    trongEnum = true;
                } else if (d.matches("^    \\S.*")) {
                    break; // sang schema khác mà chưa thấy enum
                }
                continue;
            }
            if (d.startsWith("      - ")) {
                ketQua.add(d.substring("      - ".length()).trim());
            } else {
                break;
            }
        }
        return ketQua;
    }

    @Test
    public void DS72_MoiMa_DungNhanTongVaMucNhan() throws Exception {
        Map<String, String> strings = docStrings();
        for (String[] dong : BANG_KY_VONG) {
            String ten = khoa(dong[0], dong[1]);
            ThongTinTrangThai thongTin = NhanTrangThai.thongTin(HoTrangThai.valueOf(dong[0]), dong[1]);
            assertFalse(ten + " bị coi là mã lạ", thongTin.isMaLa());
            assertEquals(ten + ": tông", Tong.valueOf(dong[3]), thongTin.getTong());
            assertEquals(ten + ": mức nhấn", Boolean.parseBoolean(dong[4]), thongTin.isNhanDam());
            assertEquals(ten + ": nhãn trong strings.xml", nfc(dong[2]), strings.get(ten));
            assertEquals(ten + ": id chuỗi", idChuoi(ten), thongTin.getNhanResId());
        }
    }

    @Test
    public void DS72_BangNhan_KhongThuaKhongThieuSoVoiDesignSystem() {
        Map<HoTrangThai, Set<String>> kyVong = new HashMap<>();
        for (String[] dong : BANG_KY_VONG) {
            kyVong.computeIfAbsent(HoTrangThai.valueOf(dong[0]), k -> new HashSet<>()).add(dong[1]);
        }
        for (HoTrangThai ho : HoTrangThai.values()) {
            assertEquals("Họ " + ho, kyVong.get(ho), NhanTrangThai.maDaBiet(ho));
        }
    }

    @Test
    public void DS72_StringsXml_ChiCoNhanCuaBangKyVong() {
        Set<String> kyVong = new HashSet<>();
        for (String[] dong : BANG_KY_VONG) {
            kyVong.add(khoa(dong[0], dong[1]));
        }
        for (String[] dong : PHAT_SINH_KY_VONG) {
            kyVong.add("nhan_phat_sinh_" + dong[0].toLowerCase());
        }
        Set<String> thucTe = new HashSet<>();
        for (String ten : docStrings().keySet()) {
            if (ten.startsWith("nhan_")) {
                thucTe.add(ten);
            }
        }
        assertEquals(kyVong, thucTe);
    }

    @Test
    public void FC73_MoiMaTrongHopDongApi_DeuCoNhan() {
        for (String[] hoVaSchema : HO_VA_SCHEMA) {
            List<String> maApi = docEnum(hoVaSchema[1]);
            assertFalse("Không đọc được enum " + hoVaSchema[1], maApi.isEmpty());
            HoTrangThai ho = HoTrangThai.valueOf(hoVaSchema[0]);
            for (String ma : maApi) {
                assertFalse(
                        "Mã " + ma + " của " + hoVaSchema[1] + " chưa có nhãn",
                        NhanTrangThai.thongTin(ho, ma).isMaLa());
            }
            // Chiều ngược lại: bảng nhãn không chứa mã mà hợp đồng không có.
            assertEquals(hoVaSchema[1], new HashSet<>(maApi), NhanTrangThai.maDaBiet(ho));
        }
    }

    @Test
    public void DS71_MaLa_HienNguyenMaTrungTinhKhongBieuTuongKhongNemLoi() {
        ThongTinTrangThai thongTin =
                NhanTrangThai.thongTin(HoTrangThai.PHIEU_MUON, "TRANG_THAI_MOI_CUA_TUONG_LAI");
        assertTrue(thongTin.isMaLa());
        assertEquals("TRANG_THAI_MOI_CUA_TUONG_LAI", thongTin.getMaGoc());
        assertEquals(Tong.NEUTRAL, thongTin.getTong());
        assertFalse(thongTin.isNhanDam());
        assertEquals(0, thongTin.getNhanResId());
    }

    @Test
    public void DS71_MaVietThuong_KhongKhopVaDuocXemLaMaLa() {
        ThongTinTrangThai thongTin = NhanTrangThai.thongTin(HoTrangThai.PHIEU_MUON, "dang_muon");
        assertTrue(thongTin.isMaLa());
        assertEquals("dang_muon", thongTin.getMaGoc());
    }

    @Test
    public void DS71_MaDungHoKhac_LaMaLaTrongHoNay() {
        // SAN_SANG chỉ thuộc TINH_TRANG_THIET_BI; tra trong PHIEU_MUON phải là mã lạ.
        assertTrue(NhanTrangThai.thongTin(HoTrangThai.PHIEU_MUON, "SAN_SANG").isMaLa());
    }

    @Test
    public void DS71_MaNullHoacRong_HienGiaTriThieu() {
        ThongTinTrangThai khongCoMa = NhanTrangThai.thongTin(HoTrangThai.PHIEU_MUON, null);
        assertTrue(khongCoMa.isMaLa());
        assertEquals("\u2014", khongCoMa.getMaGoc());
        ThongTinTrangThai maRong = NhanTrangThai.thongTin(HoTrangThai.PHIEU_MUON, "");
        assertTrue(maRong.isMaLa());
        assertEquals("\u2014", maRong.getMaGoc());
    }

    @Test(expected = UnsupportedOperationException.class)
    public void FC73_MaDaBiet_TraTapKhongTheSua() {
        NhanTrangThai.maDaBiet(HoTrangThai.PHIEU_MUON).clear();
    }

    @Test
    public void DS71_CungMaKhacHo_MucNhanKhacNhau() {
        // Quá hạn của phiếu mượn nhấn đậm; của loại thông báo nhấn nhạt.
        assertTrue(NhanTrangThai.thongTin(HoTrangThai.PHIEU_MUON, "QUA_HAN").isNhanDam());
        assertFalse(NhanTrangThai.thongTin(HoTrangThai.LOAI_THONG_BAO, "QUA_HAN").isNhanDam());
        // Mất ở kết quả kiểm tra nhấn đậm; ở loại vi phạm nhấn nhạt.
        assertTrue(NhanTrangThai.thongTin(HoTrangThai.KET_QUA_KIEM_TRA, "MAT").isNhanDam());
        assertFalse(NhanTrangThai.thongTin(HoTrangThai.LOAI_VI_PHAM, "MAT").isNhanDam());
        // Cùng mã, cùng nhãn, cùng tông ở hai họ khác: DA_NHAN của phiếu yêu cầu và đăng ký chờ.
        assertEquals(
                NhanTrangThai.thongTin(HoTrangThai.PHIEU_YEU_CAU, "DA_NHAN").getTong(),
                NhanTrangThai.thongTin(HoTrangThai.DANG_KY_CHO, "DA_NHAN").getTong());
    }

    @Test
    public void DS73_NhanPhatSinh_DungNhanTongVaMucNhan() throws Exception {
        Map<String, String> strings = docStrings();
        assertEquals(PHAT_SINH_KY_VONG.length, NhanPhatSinh.values().length);
        for (String[] dong : PHAT_SINH_KY_VONG) {
            NhanPhatSinh nhan = NhanPhatSinh.valueOf(dong[0]);
            String ten = "nhan_phat_sinh_" + dong[0].toLowerCase();
            ThongTinTrangThai thongTin = nhan.thongTin();
            assertFalse(ten, thongTin.isMaLa());
            assertEquals(ten + ": tông", Tong.valueOf(dong[2]), thongTin.getTong());
            assertEquals(ten + ": mức nhấn", Boolean.parseBoolean(dong[3]), thongTin.isNhanDam());
            assertEquals(ten + ": nhãn", nfc(dong[1]), strings.get(ten));
            assertEquals(ten + ": id chuỗi", idChuoi(ten), thongTin.getNhanResId());
        }
    }

    @Test
    public void FC35_MuoiBonHoTrangThai_KhopPhuLucA() {
        assertEquals(14, HoTrangThai.values().length);
    }
}
