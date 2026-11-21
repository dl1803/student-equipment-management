package com.nhom13.quanlymuontra.ui.dangnhap;

import static org.junit.Assert.assertEquals;
import static org.junit.Assert.assertFalse;
import static org.junit.Assert.assertNotNull;
import static org.junit.Assert.assertNull;
import static org.junit.Assert.assertTrue;

import androidx.lifecycle.Observer;
import com.nhom13.quanlymuontra.FakeCall;
import com.nhom13.quanlymuontra.HoTroXacThuc;
import com.nhom13.quanlymuontra.data.model.ApiError;
import com.nhom13.quanlymuontra.data.repository.XacThucRepository;
import com.nhom13.quanlymuontra.data.session.SessionManager;
import com.nhom13.quanlymuontra.data.session.TokenPair;
import com.nhom13.quanlymuontra.generated.model.TokenResponse;
import java.io.IOException;
import java.util.HashMap;
import java.util.Map;
import org.junit.After;
import org.junit.Before;
import org.junit.Test;
import retrofit2.Call;

/** SCR-CM-01 / UF-CM-01: trạng thái gửi, lỗi hiển thị và sự kiện một lần, bằng API giả đồng bộ. */
public class DangNhapSinhVienViewModelTest {

    private SessionManager phien;
    private final Map<String, Call<?>> apiTheoTen = new HashMap<>();
    private final Observer<ApiError> quanSatLoi = loi -> {};
    private DangNhapSinhVienViewModel viewModel;

    @Before
    public void chuanBi() {
        HoTroXacThuc.batDauLiveData();
        phien = HoTroXacThuc.taoSessionManager();
    }

    @After
    public void don() {
        if (viewModel != null) {
            viewModel.getLoiHienThi().removeObserver(quanSatLoi);
        }
        HoTroXacThuc.ketThucLiveData();
    }

    /** Tạo ViewModel và quan sát lỗi như một Fragment đang hiển thị (nguồn của Mediator cần người quan sát). */
    private DangNhapSinhVienViewModel taoViewModel() {
        XacThucRepository kho = new XacThucRepository(HoTroXacThuc.taoApi(apiTheoTen), phien);
        viewModel = new DangNhapSinhVienViewModel(phien, kho);
        viewModel.getLoiHienThi().observeForever(quanSatLoi);
        return viewModel;
    }

    @Test
    public void UFCM01_DangNhapThanhCong_PhatSuKienVaXoaMatKhauVaGhiPhien() {
        FakeCall<TokenResponse> dangNhap = FakeCall.cho();
        apiTheoTen.put("authDangNhapSinhVien", dangNhap);
        DangNhapSinhVienViewModel vm = taoViewModel();

        vm.dangNhap("20201234", "mat-khau");

        assertEquals(Boolean.TRUE, vm.getDangGui().getValue());
        dangNhap.hoanTat(HoTroXacThuc.tokenThanhCong("thanh-cong/auth-dang-nhap-sinh-vien.json"));

        assertEquals(Boolean.FALSE, vm.getDangGui().getValue());
        assertNotNull(vm.getDaDangNhap().getValue().getContentIfNotHandled());
        assertNotNull(vm.getXoaMatKhau().getValue().getContentIfNotHandled());
        assertNull(vm.getLoiHienThi().getValue());
        TokenPair cap = phien.getTokenPair();
        assertEquals("access-dang-nhap", cap.getAccessToken());
    }

    @Test
    public void DXFA22_BamHaiLanKhiDangGui_ChiGuiMotYeuCau() {
        FakeCall<TokenResponse> dangNhap = FakeCall.cho();
        apiTheoTen.put("authDangNhapSinhVien", dangNhap);
        DangNhapSinhVienViewModel vm = taoViewModel();

        vm.dangNhap("20201234", "mat-khau");
        vm.dangNhap("20201234", "mat-khau");

        assertEquals(1, dangNhap.soLanEnqueue());
    }

    @Test
    public void UFCM01_ThieuTruongBatBuoc_KhongGuiYeuCau() {
        DangNhapSinhVienViewModel vm = taoViewModel();

        vm.dangNhap("", "mat-khau");
        vm.dangNhap("   ", "mat-khau");
        vm.dangNhap("20201234", "");

        assertEquals(Boolean.FALSE, vm.getDangGui().getValue());
        assertNull(vm.getDaDangNhap().getValue());
    }

    @Test
    public void UFCM01_SaiThongTin_HienLoiXoaMatKhauVaKhongPhatDangNhap() {
        apiTheoTen.put(
                "authDangNhapSinhVien",
                FakeCall.traVe(
                        HoTroXacThuc.<TokenResponse>phanHoiLoi(401, "loi/401-dang-nhap-sai.json")));
        DangNhapSinhVienViewModel vm = taoViewModel();

        vm.dangNhap("20201234", "sai");

        assertEquals(Boolean.FALSE, vm.getDangGui().getValue());
        assertEquals("DANG_NHAP_SAI", vm.getLoiHienThi().getValue().getMaLoi());
        assertNotNull("Mật khẩu bị xóa sau mỗi kết quả", vm.getXoaMatKhau().getValue());
        assertNull(vm.getDaDangNhap().getValue());
        assertNull(phien.getTokenPair());
    }

    @Test
    public void UFCM01_SuaOMatKhauSauKhiSai_GoLoiSaiThongTin() {
        apiTheoTen.put(
                "authDangNhapSinhVien",
                FakeCall.traVe(
                        HoTroXacThuc.<TokenResponse>phanHoiLoi(401, "loi/401-dang-nhap-sai.json")));
        DangNhapSinhVienViewModel vm = taoViewModel();
        vm.dangNhap("20201234", "sai");

        vm.xoaLoiDangNhapSai();

        assertNull(vm.getLoiHienThi().getValue());
    }

    @Test
    public void UFCM01_SuaOKhiLoiKhacSaiThongTin_GiuNguyenLoi() {
        apiTheoTen.put(
                "authDangNhapSinhVien",
                FakeCall.traVe(
                        HoTroXacThuc.<TokenResponse>phanHoiLoi(
                                429, "loi/429-qua-nhieu-yeu-cau.json")));
        DangNhapSinhVienViewModel vm = taoViewModel();
        vm.dangNhap("20201234", "x");

        vm.xoaLoiDangNhapSai();

        assertEquals("QUA_NHIEU_YEU_CAU", vm.getLoiHienThi().getValue().getMaLoi());
    }

    @Test
    public void UFCM01_MatMang_HienLoiMangVaChoThuLai() {
        FakeCall<TokenResponse> lanDau = FakeCall.nem(new IOException("mất mạng"));
        apiTheoTen.put("authDangNhapSinhVien", lanDau);
        DangNhapSinhVienViewModel vm = taoViewModel();

        vm.dangNhap("20201234", "mat-khau");

        assertEquals(ApiError.Loai.MANG, vm.getLoiHienThi().getValue().getLoai());
        assertEquals("Cho thử lại bằng tay (FA 3.11)", Boolean.FALSE, vm.getDangGui().getValue());
    }

    @Test
    public void UFCM01_GuiLaiSauLoi_GoLoiCuNgay() {
        apiTheoTen.put(
                "authDangNhapSinhVien",
                FakeCall.traVe(
                        HoTroXacThuc.<TokenResponse>phanHoiLoi(
                                429, "loi/429-qua-nhieu-yeu-cau.json")));
        DangNhapSinhVienViewModel vm = taoViewModel();
        vm.dangNhap("20201234", "x");
        FakeCall<TokenResponse> lanHai = FakeCall.cho();
        apiTheoTen.put("authDangNhapSinhVien", lanHai);

        vm.dangNhap("20201234", "x");

        assertNull("Thông báo cũ biến mất khi gửi lần mới", vm.getLoiHienThi().getValue());
        assertTrue(Boolean.TRUE.equals(vm.getDangGui().getValue()));
        assertFalse(lanHai.isCanceled());
    }

    @Test
    public void FA54_LyDoKetThucPhien_HienThiNhuLoiCuaManHinhDangNhap() {
        phien.ketThucPhien(ApiError.http(401, "CHUA_XAC_THUC", null, "00-trace", null));

        DangNhapSinhVienViewModel vm = taoViewModel();

        assertEquals("CHUA_XAC_THUC", vm.getLoiHienThi().getValue().getMaLoi());
    }

    @Test
    public void FA54_LyDoKetThucPhien_GiuQuaXoayManHinhVaChiTieuThuMotLan() {
        phien.ketThucPhien(ApiError.http(429, "QUA_NHIEU_YEU_CAU", null, null, null));
        DangNhapSinhVienViewModel vm = taoViewModel();
        // Xoay màn hình: người quan sát cũ bị gỡ, người quan sát mới đăng ký lại.
        vm.getLoiHienThi().removeObserver(quanSatLoi);
        vm.getLoiHienThi().observeForever(quanSatLoi);

        assertEquals("QUA_NHIEU_YEU_CAU", vm.getLoiHienThi().getValue().getMaLoi());

        // Màn hình đăng nhập dựng lại sau đó không được hiện lại lý do đã hiện.
        DangNhapSinhVienViewModel vmMoi =
                new DangNhapSinhVienViewModel(
                        phien,
                        new XacThucRepository(HoTroXacThuc.taoApi(apiTheoTen), phien));
        Observer<ApiError> quanSatMoi = loi -> {};
        vmMoi.getLoiHienThi().observeForever(quanSatMoi);
        assertNull(vmMoi.getLoiHienThi().getValue());
        vmMoi.getLoiHienThi().removeObserver(quanSatMoi);
    }

    @Test
    public void FA54_LyDoKetThucPhien_BiGoKhiBamDangNhapLanMoi() {
        phien.ketThucPhien(ApiError.http(401, "CHUA_XAC_THUC", null, null, null));
        FakeCall<TokenResponse> dangNhap = FakeCall.cho();
        apiTheoTen.put("authDangNhapSinhVien", dangNhap);
        DangNhapSinhVienViewModel vm = taoViewModel();

        vm.dangNhap("20201234", "mat-khau");

        assertNull(vm.getLoiHienThi().getValue());
    }

    @Test
    public void FA54_SuaOKhiDangHienLyDoHetPhien_KhongGoThongBao() {
        phien.ketThucPhien(ApiError.http(401, "CHUA_XAC_THUC", null, null, null));
        DangNhapSinhVienViewModel vm = taoViewModel();

        vm.xoaLoiDangNhapSai();

        assertEquals("CHUA_XAC_THUC", vm.getLoiHienThi().getValue().getMaLoi());
    }
}
