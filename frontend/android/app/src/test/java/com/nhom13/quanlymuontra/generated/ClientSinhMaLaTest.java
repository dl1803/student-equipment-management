package com.nhom13.quanlymuontra.generated;

import static org.junit.Assert.assertEquals;
import static org.junit.Assert.assertThrows;
import static org.junit.Assert.assertTrue;

import com.google.gson.Gson;
import com.nhom13.quanlymuontra.Fixtures;
import com.nhom13.quanlymuontra.generated.model.BienBanViPham;
import com.nhom13.quanlymuontra.generated.model.ChiTietBienBan;
import com.nhom13.quanlymuontra.generated.model.ChiTietPhieuMuon;
import com.nhom13.quanlymuontra.generated.model.DangKyCho;
import com.nhom13.quanlymuontra.generated.model.LyDo;
import com.nhom13.quanlymuontra.generated.model.PhieuMuonTomTat;
import com.nhom13.quanlymuontra.generated.model.PhieuMuonTomTatPage;
import com.nhom13.quanlymuontra.generated.model.PhieuYeuCauMuon;
import com.nhom13.quanlymuontra.generated.model.ProblemDetails;
import com.nhom13.quanlymuontra.generated.model.SinhVienTheInfo;
import com.nhom13.quanlymuontra.generated.model.ThongBao;
import com.nhom13.quanlymuontra.generated.model.ThongTinToi;
import com.nhom13.quanlymuontra.generated.model.TokenResponse;
import com.nhom13.quanlymuontra.generated.model.ViPham;
import java.time.LocalDate;
import java.time.OffsetDateTime;
import java.util.UUID;
import org.junit.Test;

/**
 * Kiểm chứng cách client sinh từ OpenAPI giải mã phản hồi (ĐX-FC-21, FA 3.2, FA 5.5, FC 6.4).
 * Dùng chính {@link JSON} do bộ sinh tạo, đúng bộ chuyển đổi mà Retrofit sẽ dùng ở D04-L.
 */
public class ClientSinhMaLaTest {

    private static final Gson GSON = new JSON().getGson();
    private static final String MA_LA = "MA_CHUA_TUNG_THAY";

    // Mọi trường mã trạng thái, loại, kết quả, vai trò, maLoi trong hợp đồng của 21 thao tác. Gán
    // vào String để chính việc biên dịch chứng minh trường là String chứ không phải enum sinh ra.
    @Test
    public void DXFC21_MoiTruongMaTrangThai_GiaiMaMaLaVaGiuNguyenMa() {
        String phieuMuon = doc(PhieuMuonTomTat.class, "trangThai").getTrangThai();
        String phieuYeuCau = doc(PhieuYeuCauMuon.class, "trangThai").getTrangThai();
        String dangKyCho = doc(DangKyCho.class, "trangThai").getTrangThai();
        String bienBan = doc(BienBanViPham.class, "trangThai").getTrangThai();
        String loaiViPhamBienBan = doc(ChiTietBienBan.class, "loaiViPham").getLoaiViPham();
        String loaiViPham = doc(ViPham.class, "loaiViPham").getLoaiViPham();
        String loaiThongBao = doc(ThongBao.class, "loaiThongBao").getLoaiThongBao();
        String trangThaiHoc = doc(SinhVienTheInfo.class, "trangThaiHoc").getTrangThaiHoc();
        String quyenMuon =
                doc(SinhVienTheInfo.class, "trangThaiQuyenMuon").getTrangThaiQuyenMuon();
        String vaiTroThongTin = doc(ThongTinToi.class, "vaiTro").getVaiTro();
        String vaiTroToken = doc(TokenResponse.class, "vaiTro").getVaiTro();
        String maLoiProblem = doc(ProblemDetails.class, "maLoi").getMaLoi();
        String maLoiLyDo = doc(LyDo.class, "maLoi").getMaLoi();
        String ketQuaKiemTra = doc(ChiTietPhieuMuon.class, "ketQuaKiemTra").getKetQuaKiemTra();

        assertEquals("PhieuMuonTomTat.trangThai", MA_LA, phieuMuon);
        assertEquals("PhieuYeuCauMuon.trangThai", MA_LA, phieuYeuCau);
        assertEquals("DangKyCho.trangThai", MA_LA, dangKyCho);
        assertEquals("BienBanViPham.trangThai", MA_LA, bienBan);
        assertEquals("ChiTietBienBan.loaiViPham", MA_LA, loaiViPhamBienBan);
        assertEquals("ViPham.loaiViPham", MA_LA, loaiViPham);
        assertEquals("ThongBao.loaiThongBao", MA_LA, loaiThongBao);
        assertEquals("SinhVienTheInfo.trangThaiHoc", MA_LA, trangThaiHoc);
        assertEquals("SinhVienTheInfo.trangThaiQuyenMuon", MA_LA, quyenMuon);
        assertEquals("ThongTinToi.vaiTro", MA_LA, vaiTroThongTin);
        assertEquals("TokenResponse.vaiTro", MA_LA, vaiTroToken);
        assertEquals("ProblemDetails.maLoi", MA_LA, maLoiProblem);
        assertEquals("LyDo.maLoi", MA_LA, maLoiLyDo);
        assertEquals("ChiTietPhieuMuon.ketQuaKiemTra", MA_LA, ketQuaKiemTra);
    }

    @Test
    public void DXFC21_MaTrangThaiThuocHopDong_VanGiuNguyenMa() {
        PhieuMuonTomTat phieu = doc(PhieuMuonTomTat.class, "trangThai", "DANG_MUON");

        assertEquals("DANG_MUON", phieu.getTrangThai());
    }

    // Enum nào được sinh ra sẽ ném lỗi khi gặp mã lạ, nên 11 enum phải không tồn tại trong mã sinh.
    @Test
    public void DXFC21_KhongSinhLopEnumChoMaTrangThai() {
        String[] cacEnumCu = {
            "LoaiThongBao", "LoaiViPham", "LoaiViPhamBienBan", "MaLoi", "TrangThaiBienBan",
            "TrangThaiDangKyCho", "TrangThaiHoc", "TrangThaiPhieuMuon",
            "TrangThaiPhieuYeuCauMuon", "TrangThaiQuyenMuon", "VaiTro"
        };

        for (String ten : cacEnumCu) {
            assertThrows(
                    ten,
                    ClassNotFoundException.class,
                    () -> Class.forName("com.nhom13.quanlymuontra.generated.model." + ten));
        }
    }

    @Test
    public void FC64_KieuThoiGianVaUuid_SinhLaJavaTimeVaDocDungGiaTri() {
        SinhVienTheInfo sinhVien =
                GSON.fromJson(
                        Fixtures.doc("thanh-cong/sv-toi-bi-chan.json"), SinhVienTheInfo.class);
        PhieuMuonTomTatPage trang =
                GSON.fromJson(
                        Fixtures.doc("thanh-cong/sv-ds-phieu-muon.json"),
                        PhieuMuonTomTatPage.class);
        ThongBao thongBao =
                GSON.fromJson(
                        "{\"maThongBao\":\"3f2b8c1e-5a47-4d0a-9c1b-7e6a2d4f8b10\"}",
                        ThongBao.class);

        LocalDate hanThe = sinhVien.getHanThe();
        OffsetDateTime ngayGioMuon = trang.getDanhSach().get(0).getNgayGioMuon();
        UUID maThongBao = thongBao.getMaThongBao();

        assertEquals(LocalDate.of(2027, 9, 30), hanThe);
        assertTrue(OffsetDateTime.parse("2026-11-10T02:00:00Z").isEqual(ngayGioMuon));
        assertEquals(UUID.fromString("3f2b8c1e-5a47-4d0a-9c1b-7e6a2d4f8b10"), maThongBao);
    }

    @Test
    public void FA32_TruongLaTrongPhanHoi_BiBoQuaVaPhanConLaiVanDocDuoc() {
        String json =
                "{\"tongSo\":3,\"trang\":1,\"kichThuoc\":20,\"danhSach\":[],"
                        + "\"truongMoiCuaServer\":{\"a\":1}}";

        PhieuMuonTomTatPage trang = GSON.fromJson(json, PhieuMuonTomTatPage.class);

        assertEquals(Integer.valueOf(3), trang.getTongSo());
        assertTrue(trang.getDanhSach().isEmpty());
    }

    private static <T> T doc(Class<T> lop, String tenTruong) {
        return doc(lop, tenTruong, MA_LA);
    }

    private static <T> T doc(Class<T> lop, String tenTruong, String giaTri) {
        return GSON.fromJson("{\"" + tenTruong + "\":\"" + giaTri + "\"}", lop);
    }
}
