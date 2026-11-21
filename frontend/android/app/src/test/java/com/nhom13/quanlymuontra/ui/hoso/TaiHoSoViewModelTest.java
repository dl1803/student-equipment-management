package com.nhom13.quanlymuontra.ui.hoso;

import static org.junit.Assert.assertEquals;
import static org.junit.Assert.assertFalse;
import static org.junit.Assert.assertNotNull;
import static org.junit.Assert.assertNull;
import static org.junit.Assert.assertTrue;

import androidx.annotation.NonNull;
import androidx.lifecycle.ViewModel;
import androidx.lifecycle.ViewModelProvider;
import androidx.lifecycle.ViewModelStore;
import com.nhom13.quanlymuontra.FakeCall;
import com.nhom13.quanlymuontra.HoTroLiveData;
import com.nhom13.quanlymuontra.HoTroSinhVienApi;
import com.nhom13.quanlymuontra.data.model.ApiError;
import com.nhom13.quanlymuontra.data.model.UiState;
import com.nhom13.quanlymuontra.data.repository.HoSoMuonRepository;
import com.nhom13.quanlymuontra.generated.model.SinhVienTheInfo;
import java.io.IOException;
import org.junit.After;
import org.junit.Before;
import org.junit.Test;
import retrofit2.Call;

/** Kiểm thử logic tải chung của SCR-SV-01 và SCR-SV-14 (FA 5.6, FC 6.2, 6.3, ĐX-FC-22, ĐX-UI-04). */
public class TaiHoSoViewModelTest {

    /** ViewModel thử: lớp nền không có việc riêng. */
    private static final class VmThu extends TaiHoSoViewModel {
        VmThu(HoSoMuonRepository kho) {
            super(kho);
        }
    }

    @Before
    public void chuanBi() {
        HoTroLiveData.batDau();
    }

    @After
    public void don() {
        HoTroLiveData.ketThuc();
    }

    private static VmThu taoVm(Call<?>... cacLan) {
        return new VmThu(new HoSoMuonRepository(HoTroSinhVienApi.taoApiSvToi(cacLan)));
    }

    private static FakeCall<SinhVienTheInfo> thanhCong(String tep) {
        return FakeCall.traVe(HoTroSinhVienApi.hoSoThanhCong(tep));
    }

    @Test
    public void FA56_LoadLanDau_DangTaiRoiThanhCong() {
        FakeCall<SinhVienTheInfo> goi = FakeCall.cho();
        VmThu vm = taoVm(goi);

        vm.load();

        UiState<SinhVienTheInfo> truoc = vm.getUiState().getValue();
        assertEquals(UiState.TrangThai.DANG_TAI, truoc.getTrangThai());
        assertNull(truoc.getDuLieu());

        goi.hoanTat(HoTroSinhVienApi.hoSoThanhCong("thanh-cong/sv-toi-bi-chan.json"));

        UiState<SinhVienTheInfo> sau = vm.getUiState().getValue();
        assertEquals(UiState.TrangThai.THANH_CONG, sau.getTrangThai());
        assertEquals("23520001", sau.getDuLieu().getMaSV());
    }

    @Test
    public void FC62_LoadLanHai_KhongTaiLaiKhiChuaDanhDauCanTaiLai() {
        FakeCall<SinhVienTheInfo> goi = thanhCong("thanh-cong/sv-toi-bi-chan.json");
        VmThu vm = taoVm(goi); // chỉ chuẩn bị một lần gọi: gọi lần hai sẽ làm test thất bại

        vm.load();
        vm.load();

        assertEquals(1, goi.soLanEnqueue());
    }

    @Test
    public void FC62_SauDanhDauCanTaiLai_LoadTaiLaiTuMayChu() {
        FakeCall<SinhVienTheInfo> goi = thanhCong("thanh-cong/sv-toi-bi-chan.json");
        VmThu vm = taoVm(goi, goi);

        vm.load();
        vm.danhDauCanTaiLai();
        vm.load();

        assertEquals(2, goi.soLanEnqueue());
    }

    @Test
    public void FC62_Refresh_TaiVoDieuKien() {
        FakeCall<SinhVienTheInfo> goi = thanhCong("thanh-cong/sv-toi-bi-chan.json");
        VmThu vm = taoVm(goi, goi);

        vm.load();
        vm.refresh();

        assertEquals(2, goi.soLanEnqueue());
    }

    @Test
    public void FA51_TaiLai_GiuDuLieuCuTrongKhiChoVaKhiLoi() {
        FakeCall<SinhVienTheInfo> lanMot = thanhCong("thanh-cong/sv-toi-bi-chan.json");
        FakeCall<SinhVienTheInfo> lanHai = FakeCall.cho();
        VmThu vm = taoVm(lanMot, lanHai);

        vm.load();
        vm.refresh();

        UiState<SinhVienTheInfo> dangCho = vm.getUiState().getValue();
        assertEquals(UiState.TrangThai.DANG_TAI, dangCho.getTrangThai());
        assertEquals("23520001", dangCho.getDuLieu().getMaSV());

        lanHai.that(new IOException("mất kết nối"));

        UiState<SinhVienTheInfo> loi = vm.getUiState().getValue();
        assertEquals(UiState.TrangThai.LOI, loi.getTrangThai());
        assertEquals(ApiError.Loai.MANG, loi.getLoi().getLoai());
        assertEquals("23520001", loi.getDuLieu().getMaSV());
    }

    @Test
    public void FA56_PhanHoiCuSauKhiTaiLai_BiBoQua() {
        FakeCall<SinhVienTheInfo> cu = FakeCall.cho();
        FakeCall<SinhVienTheInfo> moi = FakeCall.cho();
        VmThu vm = taoVm(cu, moi);

        vm.load();
        vm.refresh();
        moi.hoanTat(HoTroSinhVienApi.hoSoThanhCong("thanh-cong/sv-toi-binh-thuong.json"));
        cu.hoanTat(HoTroSinhVienApi.hoSoThanhCong("thanh-cong/sv-toi-bi-chan.json"));

        assertTrue(cu.isCanceled());
        assertEquals("23520002", vm.getUiState().getValue().getDuLieu().getMaSV());
    }

    @Test
    public void FA33_Loi500_ChuyenSangLoiKemApiError() {
        FakeCall<SinhVienTheInfo> goi = FakeCall.cho();
        VmThu vm = taoVm(goi);

        vm.load();
        goi.that(new IllegalStateException("lỗi giải mã"));

        UiState<SinhVienTheInfo> loi = vm.getUiState().getValue();
        assertEquals(UiState.TrangThai.LOI, loi.getTrangThai());
        assertNotNull(loi.getLoi());
        assertEquals(ApiError.Loai.HTTP, loi.getLoi().getLoai());
    }

    @Test
    public void FC63_OnCleared_HuyTruyVanDangChay() {
        FakeCall<SinhVienTheInfo> goi = FakeCall.cho();
        ViewModelStore kho = new ViewModelStore();
        VmThu vm =
                new ViewModelProvider(
                                kho,
                                new ViewModelProvider.Factory() {
                                    @NonNull
                                    @Override
                                    public <T extends ViewModel> T create(
                                            @NonNull Class<T> modelClass) {
                                        return modelClass.cast(taoVm(goi));
                                    }
                                })
                        .get(VmThu.class);

        vm.load();
        assertFalse(goi.isCanceled());
        kho.clear();

        assertTrue(goi.isCanceled());
    }
}
