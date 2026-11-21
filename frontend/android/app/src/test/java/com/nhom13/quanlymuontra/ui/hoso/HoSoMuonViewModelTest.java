package com.nhom13.quanlymuontra.ui.hoso;

import static org.junit.Assert.assertEquals;
import static org.junit.Assert.assertNull;

import com.nhom13.quanlymuontra.FakeCall;
import com.nhom13.quanlymuontra.HoTroLiveData;
import com.nhom13.quanlymuontra.HoTroSinhVienApi;
import com.nhom13.quanlymuontra.HoTroXacThuc;
import com.nhom13.quanlymuontra.data.repository.HoSoMuonRepository;
import com.nhom13.quanlymuontra.data.repository.XacThucRepository;
import com.nhom13.quanlymuontra.data.session.SessionManager;
import com.nhom13.quanlymuontra.data.session.TokenPair;
import java.io.IOException;
import java.util.HashMap;
import java.util.Map;
import org.junit.After;
import org.junit.Before;
import org.junit.Test;
import retrofit2.Call;

/** Kiểm nút Đăng xuất của SCR-SV-14 (UF-CM-03, FA 3.11, FA 5.4). */
public class HoSoMuonViewModelTest {

    private SessionManager phien;
    private FakeCall<Void> goiDangXuat;
    private HoSoMuonViewModel vm;

    @Before
    public void chuanBi() {
        HoTroLiveData.batDau();
        phien = HoTroXacThuc.taoSessionManager();
        phien.save(new TokenPair("access-gia", "refresh-gia"));
        goiDangXuat = FakeCall.cho();
        Map<String, Call<?>> theoTen = new HashMap<>();
        theoTen.put("authDangXuat", goiDangXuat);
        XacThucRepository xacThuc = new XacThucRepository(HoTroXacThuc.taoApi(theoTen), phien);
        vm =
                new HoSoMuonViewModel(
                        new HoSoMuonRepository(HoTroSinhVienApi.taoApiSvToi()), xacThuc);
    }

    @After
    public void don() {
        HoTroLiveData.ketThuc();
    }

    @Test
    public void UFCM03_DangXuat_XoaPhienCucBoVaBatTrangThaiDangXuLy() {
        vm.dangXuat();

        assertNull(phien.getTokenPair());
        assertEquals(Boolean.TRUE, vm.getDangXuat().getValue());
        assertEquals(1, goiDangXuat.soLanEnqueue());
    }

    @Test
    public void FA311_BamDangXuatHaiLan_ChiGuiMotYeuCau() {
        vm.dangXuat();
        vm.dangXuat();

        assertEquals(1, goiDangXuat.soLanEnqueue());
    }

    @Test
    public void UFCM03_DangXuatLoiMang_KhongKhoiPhucPhienVaKhongNemLoi() {
        vm.dangXuat();

        goiDangXuat.that(new IOException("mất kết nối"));

        assertNull(phien.getTokenPair());
        assertEquals(1, goiDangXuat.soLanEnqueue());
    }
}
