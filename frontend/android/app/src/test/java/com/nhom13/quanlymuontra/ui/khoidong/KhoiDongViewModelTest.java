package com.nhom13.quanlymuontra.ui.khoidong;

import static org.junit.Assert.assertEquals;
import static org.junit.Assert.assertNull;

import com.nhom13.quanlymuontra.FakeCall;
import com.nhom13.quanlymuontra.HoTroXacThuc;
import com.nhom13.quanlymuontra.data.model.ApiError;
import com.nhom13.quanlymuontra.data.repository.XacThucRepository;
import com.nhom13.quanlymuontra.data.session.SessionManager;
import com.nhom13.quanlymuontra.data.session.TokenPair;
import com.nhom13.quanlymuontra.generated.model.TokenResponse;
import java.io.IOException;
import java.time.Clock;
import java.time.Instant;
import java.time.ZoneOffset;
import java.util.HashMap;
import java.util.Map;
import org.junit.After;
import org.junit.Before;
import org.junit.Test;
import retrofit2.Call;

/** SCR-CM-03 / UF-CM-03: quyết định điều hướng khi khởi động, bằng API giả đồng bộ. */
public class KhoiDongViewModelTest {

    private static final Instant BAY_GIO = Instant.parse("2026-11-20T03:00:00Z");
    private static final Clock DONG_HO = Clock.fixed(BAY_GIO, ZoneOffset.UTC);

    private SessionManager phien;
    private final Map<String, Call<?>> apiTheoTen = new HashMap<>();

    @Before
    public void chuanBi() {
        HoTroXacThuc.batDauLiveData();
        phien = HoTroXacThuc.taoSessionManager();
    }

    @After
    public void don() {
        HoTroXacThuc.ketThucLiveData();
    }

    private KhoiDongViewModel taoViewModel() {
        XacThucRepository kho = new XacThucRepository(HoTroXacThuc.taoApi(apiTheoTen), phien);
        return new KhoiDongViewModel(phien, kho, DONG_HO);
    }

    /** Lưu một phiên vào kho; {@code khoiDong()} sẽ đọc lại phiên này bằng {@code restore()}. */
    private void coPhienDaLuu(String accessToken, String refreshToken) {
        phien.save(new TokenPair(accessToken, refreshToken));
    }

    private static KhoiDongViewModel.DichDen lay(KhoiDongViewModel viewModel) {
        return viewModel.getSuKienDieuHuong().getValue().getContentIfNotHandled();
    }

    @Test
    public void UFCM03_KhongCoPhien_VeDangNhapVaKhongGoiMang() {
        KhoiDongViewModel viewModel = taoViewModel();

        viewModel.khoiDong();

        assertEquals(KhoiDongViewModel.DichDen.DANG_NHAP, lay(viewModel));
    }

    @Test
    public void UFCM03_AccessTokenConHan_VaoTrangChuVaKhongGoiMang() {
        coPhienDaLuu(HoTroXacThuc.jwtCoExp(BAY_GIO.getEpochSecond() + 600), "refresh-1");
        KhoiDongViewModel viewModel = taoViewModel();

        viewModel.khoiDong();

        assertEquals(KhoiDongViewModel.DichDen.TRANG_CHU, lay(viewModel));
    }

    @Test
    public void UFCM03_AccessTokenHetHan_LamMoiThanhCongThiVaoTrangChu() {
        coPhienDaLuu(HoTroXacThuc.jwtCoExp(BAY_GIO.getEpochSecond() - 600), "refresh-cu");
        FakeCall<TokenResponse> lamMoi =
                FakeCall.traVe(HoTroXacThuc.tokenThanhCong("thanh-cong/auth-lam-moi.json"));
        apiTheoTen.put("authLamMoi", lamMoi);
        KhoiDongViewModel viewModel = taoViewModel();

        viewModel.khoiDong();

        assertEquals(KhoiDongViewModel.DichDen.TRANG_CHU, lay(viewModel));
        assertEquals("access-moi", phien.getTokenPair().getAccessToken());
        assertEquals(1, lamMoi.soLanEnqueue());
    }

    @Test
    public void UFCM03_AccessTokenHetHan_LamMoi401ThiVeDangNhapKemLyDo() {
        coPhienDaLuu(HoTroXacThuc.jwtCoExp(BAY_GIO.getEpochSecond() - 600), "refresh-cu");
        apiTheoTen.put(
                "authLamMoi",
                FakeCall.traVe(HoTroXacThuc.<TokenResponse>phanHoiLoi(401, "loi/401-chua-xac-thuc.json")));
        KhoiDongViewModel viewModel = taoViewModel();

        viewModel.khoiDong();

        assertEquals(KhoiDongViewModel.DichDen.DANG_NHAP, lay(viewModel));
        assertNull(phien.getTokenPair());
        ApiError lyDo = phien.getLyDoKetThucPhien().getValue().getContentIfNotHandled();
        assertEquals("CHUA_XAC_THUC", lyDo.getMaLoi());
    }

    @Test
    public void UFCM03_LamMoi429_VeDangNhapKemLyDoVaKhongTuLap() {
        coPhienDaLuu(HoTroXacThuc.jwtCoExp(BAY_GIO.getEpochSecond() - 600), "refresh-cu");
        FakeCall<TokenResponse> lamMoi =
                FakeCall.traVe(
                        HoTroXacThuc.<TokenResponse>phanHoiLoi(
                                429, "loi/429-qua-nhieu-yeu-cau.json"));
        apiTheoTen.put("authLamMoi", lamMoi);
        KhoiDongViewModel viewModel = taoViewModel();

        viewModel.khoiDong();

        assertEquals(KhoiDongViewModel.DichDen.DANG_NHAP, lay(viewModel));
        assertEquals(1, lamMoi.soLanEnqueue());
        assertEquals(
                "QUA_NHIEU_YEU_CAU",
                phien.getLyDoKetThucPhien().getValue().getContentIfNotHandled().getMaLoi());
    }

    @Test
    public void UFCM03_LamMoiMatMang_VeDangNhapKemLyDoMang() {
        coPhienDaLuu(HoTroXacThuc.jwtCoExp(BAY_GIO.getEpochSecond() - 600), "refresh-cu");
        apiTheoTen.put("authLamMoi", FakeCall.<TokenResponse>nem(new IOException("mất mạng")));
        KhoiDongViewModel viewModel = taoViewModel();

        viewModel.khoiDong();

        assertEquals(KhoiDongViewModel.DichDen.DANG_NHAP, lay(viewModel));
        assertEquals(
                ApiError.Loai.MANG,
                phien.getLyDoKetThucPhien().getValue().getContentIfNotHandled().getLoai());
    }

    @Test
    public void UFCM03_TokenKhongDocDuocExp_CoiNhuHetHanVaLamMoi() {
        coPhienDaLuu("access-khong-phai-jwt", "refresh-cu");
        FakeCall<TokenResponse> lamMoi =
                FakeCall.traVe(HoTroXacThuc.tokenThanhCong("thanh-cong/auth-lam-moi.json"));
        apiTheoTen.put("authLamMoi", lamMoi);
        KhoiDongViewModel viewModel = taoViewModel();

        viewModel.khoiDong();

        assertEquals(KhoiDongViewModel.DichDen.TRANG_CHU, lay(viewModel));
        assertEquals(1, lamMoi.soLanEnqueue());
    }

    @Test
    public void FA54_DangLamMoi_ChuaPhatSuKienChoToiKhiCoKetQua() {
        coPhienDaLuu(HoTroXacThuc.jwtCoExp(BAY_GIO.getEpochSecond() - 600), "refresh-cu");
        FakeCall<TokenResponse> lamMoi = FakeCall.cho();
        apiTheoTen.put("authLamMoi", lamMoi);
        KhoiDongViewModel viewModel = taoViewModel();

        viewModel.khoiDong();

        assertNull("Chưa có kết quả thì không điều hướng", viewModel.getSuKienDieuHuong().getValue());
        lamMoi.hoanTat(HoTroXacThuc.tokenThanhCong("thanh-cong/auth-lam-moi.json"));
        assertEquals(KhoiDongViewModel.DichDen.TRANG_CHU, lay(viewModel));
    }

    @Test
    public void FA54_KhoiDongHaiLan_ChiLamMoiMotLan() {
        coPhienDaLuu(HoTroXacThuc.jwtCoExp(BAY_GIO.getEpochSecond() - 600), "refresh-cu");
        FakeCall<TokenResponse> lamMoi = FakeCall.cho();
        apiTheoTen.put("authLamMoi", lamMoi);
        KhoiDongViewModel viewModel = taoViewModel();

        viewModel.khoiDong();
        viewModel.khoiDong();

        assertEquals(1, lamMoi.soLanEnqueue());
    }
}
