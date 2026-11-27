package com.nhom13.quanlymuontra.ui.trangchu;

import static org.junit.Assert.assertEquals;
import static org.junit.Assert.assertFalse;
import static org.junit.Assert.assertNotNull;
import static org.junit.Assert.assertTrue;

import com.nhom13.quanlymuontra.FakeCall;
import com.nhom13.quanlymuontra.HoTroLiveData;
import com.nhom13.quanlymuontra.HoTroSinhVienApi;
import com.nhom13.quanlymuontra.HoTroTraCuuApi;
import com.nhom13.quanlymuontra.data.repository.HoSoMuonRepository;
import com.nhom13.quanlymuontra.ui.phieuyeucau.CongQuyenMuon;
import com.nhom13.quanlymuontra.ui.tracuu.ChiTietLoaiThietBiViewModel.HanhDong;
import org.junit.After;
import org.junit.Before;
import org.junit.Test;

/** Cổng quyền mượn của lối vào Đặt trước thiết bị ở SCR-SV-01 (FA 5.3 quy tắc 3, ĐX-UF-03). */
public class TrangChuViewModelTest {

    private static final String SV_TOI = "svToi";
    private static final String BINH_THUONG = "thanh-cong/sv-toi-binh-thuong.json";
    private static final String BI_CHAN = "thanh-cong/sv-toi-bi-chan.json";

    @Before
    public void chuanBi() {
        HoTroLiveData.batDau();
    }

    @After
    public void don() {
        HoTroLiveData.ketThuc();
    }

    @Test
    public void UFSV02_DatTruocQuyenBinhThuong_DuocPhep() {
        HoTroTraCuuApi ho =
                new HoTroTraCuuApi()
                        .chuanBi(SV_TOI, FakeCall.traVe(HoTroSinhVienApi.hoSoThanhCong(BINH_THUONG)));
        TrangChuViewModel vm = new TrangChuViewModel(new HoSoMuonRepository(ho.api()));

        vm.chonDatTruoc();

        CongQuyenMuon.KetQua kq = vm.getKetQuaCong().getValue().getContentIfNotHandled();
        assertNotNull(kq);
        assertTrue(kq.isDuocPhep());
        assertEquals(HanhDong.DAT_TRUOC, kq.getHanhDong());
    }

    @Test
    public void DXUF03_DatTruocQuyenBiChan_KhongDuocPhepVaMoiLanGoiSvToiMoi() {
        HoTroTraCuuApi ho =
                new HoTroTraCuuApi()
                        .chuanBi(
                                SV_TOI,
                                FakeCall.traVe(HoTroSinhVienApi.hoSoThanhCong(BI_CHAN)),
                                FakeCall.traVe(HoTroSinhVienApi.hoSoThanhCong(BINH_THUONG)));
        TrangChuViewModel vm = new TrangChuViewModel(new HoSoMuonRepository(ho.api()));

        vm.chonDatTruoc();
        assertFalse(vm.getKetQuaCong().getValue().getContentIfNotHandled().isDuocPhep());
        vm.chonDatTruoc();
        assertTrue(vm.getKetQuaCong().getValue().getContentIfNotHandled().isDuocPhep());

        assertEquals(2, ho.goiTheoTen(SV_TOI).size());
    }
}
