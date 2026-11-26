package com.nhom13.quanlymuontra;

import static org.junit.Assert.assertNotNull;
import static org.junit.Assert.assertThrows;
import static org.junit.Assert.assertTrue;

import androidx.lifecycle.ViewModel;
import com.nhom13.quanlymuontra.data.repository.HoSoMuonRepository;
import com.nhom13.quanlymuontra.data.repository.TraCuuRepository;
import com.nhom13.quanlymuontra.data.repository.XacThucRepository;
import com.nhom13.quanlymuontra.data.session.SessionManager;
import com.nhom13.quanlymuontra.ui.dangnhap.DangNhapSinhVienViewModel;
import com.nhom13.quanlymuontra.ui.hoso.HoSoMuonViewModel;
import com.nhom13.quanlymuontra.ui.khoidong.KhoiDongViewModel;
import com.nhom13.quanlymuontra.ui.tracuu.ChiTietLoaiThietBiViewModel;
import com.nhom13.quanlymuontra.ui.tracuu.TraCuuThietBiViewModel;
import com.nhom13.quanlymuontra.ui.trangchu.TrangChuViewModel;
import java.util.Collections;
import org.junit.After;
import org.junit.Before;
import org.junit.Test;

public class AppViewModelFactoryTest {

    private AppViewModelFactory nhaMay;

    @Before
    public void chuanBi() {
        HoTroXacThuc.batDauLiveData();
        SessionManager phien = HoTroXacThuc.taoSessionManager();
        XacThucRepository kho =
                new XacThucRepository(HoTroXacThuc.taoApi(Collections.emptyMap()), phien);
        nhaMay =
                new AppViewModelFactory(
                        new AppContainer(
                                phien,
                                kho,
                                new HoSoMuonRepository(HoTroSinhVienApi.taoApiSvToi()),
                                new TraCuuRepository(HoTroSinhVienApi.taoApiSvToi())));
    }

    @After
    public void don() {
        HoTroXacThuc.ketThucLiveData();
    }

    @Test
    public void FA51_LopViewModelChuaDangKy_NemIllegalArgumentException() {
        IllegalArgumentException loi =
                assertThrows(IllegalArgumentException.class, () -> nhaMay.create(ViewModel.class));

        assertTrue(loi.getMessage().contains(ViewModel.class.getName()));
    }

    @Test
    public void FA51_KhoiDongViewModel_DuocTaoTuAppContainer() {
        assertNotNull(nhaMay.create(KhoiDongViewModel.class));
    }

    @Test
    public void FA51_DangNhapSinhVienViewModel_DuocTaoTuAppContainer() {
        assertNotNull(nhaMay.create(DangNhapSinhVienViewModel.class));
    }

    @Test
    public void FA51_TrangChuViewModel_DuocTaoTuAppContainer() {
        assertNotNull(nhaMay.create(TrangChuViewModel.class));
    }

    @Test
    public void FA51_HoSoMuonViewModel_DuocTaoTuAppContainer() {
        assertNotNull(nhaMay.create(HoSoMuonViewModel.class));
    }

    @Test
    public void FA51_TraCuuThietBiViewModel_DuocTaoTuAppContainer() {
        assertNotNull(nhaMay.create(TraCuuThietBiViewModel.class));
    }

    @Test
    public void FA51_ChiTietLoaiThietBiViewModel_DuocTaoTuAppContainer() {
        assertNotNull(nhaMay.create(ChiTietLoaiThietBiViewModel.class));
    }
}
