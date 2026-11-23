package com.nhom13.quanlymuontra.ui.tracuu;

import static org.junit.Assert.assertEquals;
import static org.junit.Assert.assertFalse;
import static org.junit.Assert.assertNotNull;
import static org.junit.Assert.assertNull;
import static org.junit.Assert.assertTrue;

import com.nhom13.quanlymuontra.BoHenGioGia;
import com.nhom13.quanlymuontra.FakeCall;
import com.nhom13.quanlymuontra.HoTroLiveData;
import com.nhom13.quanlymuontra.HoTroTraCuuApi;
import com.nhom13.quanlymuontra.HoTroTraCuuApi.GoiApi;
import com.nhom13.quanlymuontra.common.HangSo;
import com.nhom13.quanlymuontra.data.model.ApiError;
import com.nhom13.quanlymuontra.data.model.UiState;
import com.nhom13.quanlymuontra.data.repository.TraCuuRepository;
import com.nhom13.quanlymuontra.generated.model.LoaiThietBiTomTatPage;
import com.nhom13.quanlymuontra.generated.model.PhongThietBiPage;
import com.nhom13.quanlymuontra.generated.model.TraCuuThietBiItem;
import com.nhom13.quanlymuontra.generated.model.TraCuuThietBiItemPage;
import java.io.IOException;
import java.util.List;
import java.util.UUID;
import org.junit.After;
import org.junit.Before;
import org.junit.Test;

/** Logic của SCR-SV-02 (UI-Screens 5.2, FA 3.10, 5.6, ĐX-FA-05) trên JVM với API giả. */
public class TraCuuThietBiViewModelTest {

    private static final String TRA_CUU = "svTraCuuThietBi";
    private static final String LOAI = "svDsLoaiThietBi";
    private static final String PHONG = "svDsPhongThietBi";
    private static final LuaChonBoLoc LOAI_A =
            LuaChonBoLoc.tao(UUID.fromString("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"), "Laptop", null);
    private static final LuaChonBoLoc PHONG_B =
            LuaChonBoLoc.tao(
                    UUID.fromString("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb"), "Phòng B2", "Cơ sở 2");

    private BoHenGioGia henGio;

    @Before
    public void chuanBi() {
        HoTroLiveData.batDau();
        henGio = new BoHenGioGia();
    }

    @After
    public void don() {
        HoTroLiveData.ketThuc();
    }

    private TraCuuThietBiViewModel tao(HoTroTraCuuApi api) {
        return new TraCuuThietBiViewModel(new TraCuuRepository(api.api()), henGio);
    }

    /** API mà mỗi lần tải (nguồn loại, nguồn phòng, danh sách trang 1) đều thành công. */
    private static HoTroTraCuuApi apiThanhCong(int soLan) {
        HoTroTraCuuApi api = new HoTroTraCuuApi();
        for (int i = 0; i < soLan; i++) {
            api.chuanBi(LOAI, FakeCall.traVe(HoTroTraCuuApi.trangLoai(2, 1, 2)));
            api.chuanBi(PHONG, FakeCall.traVe(HoTroTraCuuApi.trangPhong(1, 1, 1)));
            api.chuanBi(TRA_CUU, FakeCall.traVe(HoTroTraCuuApi.trangTraCuu(2, 1, 2)));
        }
        return api;
    }

    private static HoTroTraCuuApi apiNguonVaNhieuTraCuu(int soTraCuu) {
        HoTroTraCuuApi api = new HoTroTraCuuApi();
        api.chuanBi(LOAI, FakeCall.traVe(HoTroTraCuuApi.trangLoai(1, 1, 1)));
        api.chuanBi(PHONG, FakeCall.traVe(HoTroTraCuuApi.trangPhong(1, 1, 1)));
        for (int i = 0; i < soTraCuu; i++) {
            api.chuanBi(TRA_CUU, FakeCall.traVe(HoTroTraCuuApi.trangTraCuu(1, 1, 1)));
        }
        return api;
    }

    private static List<TraCuuThietBiItem> dong(TraCuuThietBiViewModel vm) {
        return vm.getDanhSach().getValue().getDuLieu();
    }

    @Test
    public void UISV02_MoManHinh_TaiTrangMotVaHaiNguonBoLoc() {
        HoTroTraCuuApi api = apiThanhCong(1);
        TraCuuThietBiViewModel vm = tao(api);

        vm.load();

        assertEquals(UiState.TrangThai.THANH_CONG, vm.getDanhSach().getValue().getTrangThai());
        assertEquals(2, dong(vm).size());
        GoiApi g = api.goiTheoTen(TRA_CUU).get(0);
        assertNull(g.thamSo[0]);
        assertEquals(Integer.valueOf(1), g.thamSo[4]);
        assertEquals(Integer.valueOf(HangSo.KICH_THUOC_TRANG), g.thamSo[5]);
        assertEquals(UiState.TrangThai.THANH_CONG, vm.getDsLoai().getValue().getTrangThai());
        assertEquals(2, vm.getDsLoai().getValue().getDuLieu().size());
        assertEquals(1, vm.getDsPhong().getValue().getDuLieu().size());
        assertEquals("Cơ sở 1", vm.getDsPhong().getValue().getDuLieu().get(0).getPhuDe());
    }

    @Test
    public void FC62_Load_ChiTaiMotLanChoDenKhiDanhDauCanTaiLai() {
        HoTroTraCuuApi api = apiThanhCong(2);
        TraCuuThietBiViewModel vm = tao(api);

        vm.load();
        vm.load();
        assertEquals(1, api.goiTheoTen(TRA_CUU).size());

        vm.danhDauCanTaiLai();
        vm.load();
        assertEquals(2, api.goiTheoTen(TRA_CUU).size());
        assertEquals(2, api.goiTheoTen(LOAI).size());
        assertEquals(2, api.goiTheoTen(PHONG).size());
    }

    @Test
    public void UISV02_TrangDauKhongCoDong_LaRongKhongPhaiLoi() {
        HoTroTraCuuApi api =
                new HoTroTraCuuApi()
                        .chuanBi(LOAI, FakeCall.traVe(HoTroTraCuuApi.trangLoai(0, 1, 0)))
                        .chuanBi(PHONG, FakeCall.traVe(HoTroTraCuuApi.trangPhong(0, 1, 0)))
                        .chuanBi(TRA_CUU, FakeCall.traVe(HoTroTraCuuApi.trangTraCuu(0, 1, 0)));
        TraCuuThietBiViewModel vm = tao(api);

        vm.load();

        assertEquals(UiState.TrangThai.RONG, vm.getDanhSach().getValue().getTrangThai());
        assertNull(vm.getDanhSach().getValue().getLoi());
    }

    @Test
    public void UISV02_TrangDauLoi_HienLoiVaKhongGiuDongCu() {
        HoTroTraCuuApi api =
                new HoTroTraCuuApi()
                        .chuanBi(LOAI, FakeCall.traVe(HoTroTraCuuApi.trangLoai(1, 1, 1)))
                        .chuanBi(PHONG, FakeCall.traVe(HoTroTraCuuApi.trangPhong(1, 1, 1)))
                        .chuanBi(TRA_CUU, FakeCall.nem(new IOException("mat mang")));
        TraCuuThietBiViewModel vm = tao(api);

        vm.load();

        UiState<List<TraCuuThietBiItem>> tt = vm.getDanhSach().getValue();
        assertEquals(UiState.TrangThai.LOI, tt.getTrangThai());
        assertNull(tt.getDuLieu());
        assertEquals(ApiError.Loai.MANG, tt.getLoi().getLoai());
    }

    @Test
    public void UISV02_LoiNguonLoai_KhongAnhHuongPhongVaDanhSachRoiThuLaiRieng() {
        HoTroTraCuuApi api =
                new HoTroTraCuuApi()
                        .chuanBi(
                                LOAI,
                                FakeCall.nem(new IOException("mat mang")),
                                FakeCall.traVe(HoTroTraCuuApi.trangLoai(2, 1, 2)))
                        .chuanBi(PHONG, FakeCall.traVe(HoTroTraCuuApi.trangPhong(1, 1, 1)))
                        .chuanBi(TRA_CUU, FakeCall.traVe(HoTroTraCuuApi.trangTraCuu(2, 1, 2)));
        TraCuuThietBiViewModel vm = tao(api);

        vm.load();

        assertEquals(UiState.TrangThai.LOI, vm.getDsLoai().getValue().getTrangThai());
        assertEquals(ApiError.Loai.MANG, vm.getDsLoai().getValue().getLoi().getLoai());
        assertEquals(UiState.TrangThai.THANH_CONG, vm.getDsPhong().getValue().getTrangThai());
        assertEquals(UiState.TrangThai.THANH_CONG, vm.getDanhSach().getValue().getTrangThai());

        vm.thuLaiNguonLoai();

        assertEquals(UiState.TrangThai.THANH_CONG, vm.getDsLoai().getValue().getTrangThai());
        assertEquals(2, api.goiTheoTen(LOAI).size());
        assertEquals(1, api.goiTheoTen(PHONG).size());
        assertEquals(1, api.goiTheoTen(TRA_CUU).size());
    }

    @Test
    public void UISV02_LoiNguonPhong_KhongAnhHuongLoaiVaDanhSach() {
        HoTroTraCuuApi api =
                new HoTroTraCuuApi()
                        .chuanBi(LOAI, FakeCall.traVe(HoTroTraCuuApi.trangLoai(1, 1, 1)))
                        .chuanBi(PHONG, FakeCall.nem(new IOException("mat mang")))
                        .chuanBi(TRA_CUU, FakeCall.traVe(HoTroTraCuuApi.trangTraCuu(1, 1, 1)));
        TraCuuThietBiViewModel vm = tao(api);

        vm.load();

        assertEquals(UiState.TrangThai.LOI, vm.getDsPhong().getValue().getTrangThai());
        assertEquals(UiState.TrangThai.THANH_CONG, vm.getDsLoai().getValue().getTrangThai());
        assertEquals(UiState.TrangThai.THANH_CONG, vm.getDanhSach().getValue().getTrangThai());
    }

    @Test
    public void UISV02_NguonBoLocNhieuTrang_TaiDuTungTrangVoiKichThuocToiDa() {
        HoTroTraCuuApi api =
                new HoTroTraCuuApi()
                        .chuanBi(
                                LOAI,
                                FakeCall.traVe(HoTroTraCuuApi.trangLoai(150, 1, 100)),
                                FakeCall.traVe(HoTroTraCuuApi.trangLoai(150, 2, 50)))
                        .chuanBi(PHONG, FakeCall.traVe(HoTroTraCuuApi.trangPhong(1, 1, 1)))
                        .chuanBi(TRA_CUU, FakeCall.traVe(HoTroTraCuuApi.trangTraCuu(1, 1, 1)));
        TraCuuThietBiViewModel vm = tao(api);

        vm.load();

        assertEquals(150, vm.getDsLoai().getValue().getDuLieu().size());
        List<GoiApi> goi = api.goiTheoTen(LOAI);
        assertEquals(2, goi.size());
        assertEquals(Integer.valueOf(1), goi.get(0).thamSo[0]);
        assertEquals(Integer.valueOf(100), goi.get(0).thamSo[1]);
        assertEquals(Integer.valueOf(2), goi.get(1).thamSo[0]);
        assertEquals(Integer.valueOf(100), goi.get(1).thamSo[1]);
    }

    @Test
    public void FA310_DoiBoLoc_VeTrangMotVaHuyYeuCauCu() {
        FakeCall<TraCuuThietBiItemPage> dau = FakeCall.cho();
        HoTroTraCuuApi api =
                new HoTroTraCuuApi()
                        .chuanBi(LOAI, FakeCall.traVe(HoTroTraCuuApi.trangLoai(1, 1, 1)))
                        .chuanBi(PHONG, FakeCall.traVe(HoTroTraCuuApi.trangPhong(1, 1, 1)))
                        .chuanBi(TRA_CUU, dau, FakeCall.traVe(HoTroTraCuuApi.trangTraCuu(1, 1, 1)));
        TraCuuThietBiViewModel vm = tao(api);
        vm.load();

        vm.chonTinhTrang(TinhTrangHang.CON_HANG);

        assertTrue(dau.isCanceled());
        GoiApi g = api.goiTheoTen(TRA_CUU).get(1);
        assertEquals(Boolean.TRUE, g.thamSo[3]);
        assertEquals(Integer.valueOf(1), g.thamSo[4]);
    }

    @Test
    public void FA310_KetQuaCuVeSau_KhongGhiDeKetQuaMoi() {
        FakeCall<TraCuuThietBiItemPage> dau = FakeCall.cho();
        HoTroTraCuuApi api =
                new HoTroTraCuuApi()
                        .chuanBi(LOAI, FakeCall.traVe(HoTroTraCuuApi.trangLoai(1, 1, 1)))
                        .chuanBi(PHONG, FakeCall.traVe(HoTroTraCuuApi.trangPhong(1, 1, 1)))
                        .chuanBi(TRA_CUU, dau, FakeCall.traVe(HoTroTraCuuApi.trangTraCuu(1, 1, 1)));
        TraCuuThietBiViewModel vm = tao(api);
        vm.load();
        vm.chonTinhTrang(TinhTrangHang.CON_HANG);
        assertEquals(1, dong(vm).size());

        dau.hoanTat(HoTroTraCuuApi.trangTraCuu(9, 1, 9));

        assertEquals(1, dong(vm).size());
        assertEquals(UiState.TrangThai.THANH_CONG, vm.getDanhSach().getValue().getTrangThai());
    }

    @Test
    public void DXFA05_GoLienTiep_ChiMotHenGioConHieuLucVaDoTre400() {
        HoTroTraCuuApi api = apiNguonVaNhieuTraCuu(2);
        TraCuuThietBiViewModel vm = tao(api);
        vm.load();

        vm.datTuKhoa("a", false);
        vm.datTuKhoa("ab", false);

        assertEquals(1, henGio.soViecConHieuLuc());
        assertEquals((long) HangSo.DO_TRE_GO_TIM_KIEM_MS, henGio.doTreCuoi());
        assertEquals(1, api.goiTheoTen(TRA_CUU).size());
        assertEquals("ab", vm.getTuKhoaHienThi());

        henGio.chayViecConHieuLuc();

        assertEquals(2, api.goiTheoTen(TRA_CUU).size());
        assertEquals("ab", api.goiTheoTen(TRA_CUU).get(1).thamSo[0]);
        assertEquals("ab", vm.getBoLoc().getValue().getTen());
    }

    @Test
    public void DXFA05_XacNhanBangPhimTim_GuiNgayVaHuyHenGio() {
        HoTroTraCuuApi api = apiNguonVaNhieuTraCuu(2);
        TraCuuThietBiViewModel vm = tao(api);
        vm.load();
        vm.datTuKhoa("a", false);

        vm.datTuKhoa("abc", true);

        assertEquals(0, henGio.soViecConHieuLuc());
        assertEquals(2, api.goiTheoTen(TRA_CUU).size());
        assertEquals("abc", api.goiTheoTen(TRA_CUU).get(1).thamSo[0]);
    }

    @Test
    public void DXFA05_TenChuaKhoangTrang_CatHaiDauVaKhongGuiLaiKhiKhongDoi() {
        HoTroTraCuuApi api = apiNguonVaNhieuTraCuu(2);
        TraCuuThietBiViewModel vm = tao(api);
        vm.load();

        vm.datTuKhoa("   ", true);
        assertEquals(1, api.goiTheoTen(TRA_CUU).size());

        vm.datTuKhoa("  laptop ", true);
        assertEquals(2, api.goiTheoTen(TRA_CUU).size());
        assertEquals("laptop", api.goiTheoTen(TRA_CUU).get(1).thamSo[0]);
    }

    @Test
    public void UISV02_DoiBoLocKhiDangGoDo_ChotTenVaChiGuiMotYeuCau() {
        HoTroTraCuuApi api = apiNguonVaNhieuTraCuu(2);
        TraCuuThietBiViewModel vm = tao(api);
        vm.load();
        vm.datTuKhoa("abc", false);

        vm.chonTinhTrang(TinhTrangHang.CON_HANG);

        assertEquals(0, henGio.soViecConHieuLuc());
        assertEquals(2, api.goiTheoTen(TRA_CUU).size());
        GoiApi g = api.goiTheoTen(TRA_CUU).get(1);
        assertEquals("abc", g.thamSo[0]);
        assertEquals(Boolean.TRUE, g.thamSo[3]);
    }

    @Test
    public void FA56_TaiThemDenKhiDuTongSo_RoiDung() {
        HoTroTraCuuApi api =
                new HoTroTraCuuApi()
                        .chuanBi(LOAI, FakeCall.traVe(HoTroTraCuuApi.trangLoai(1, 1, 1)))
                        .chuanBi(PHONG, FakeCall.traVe(HoTroTraCuuApi.trangPhong(1, 1, 1)))
                        .chuanBi(
                                TRA_CUU,
                                FakeCall.traVe(HoTroTraCuuApi.trangTraCuu(25, 1, 20)),
                                FakeCall.traVe(HoTroTraCuuApi.trangTraCuu(25, 2, 5)));
        TraCuuThietBiViewModel vm = tao(api);
        vm.load();
        assertEquals(20, dong(vm).size());

        vm.taiThem();

        assertEquals(25, dong(vm).size());
        assertEquals(Integer.valueOf(2), api.goiTheoTen(TRA_CUU).get(1).thamSo[4]);
        assertEquals(TraCuuThietBiViewModel.TrangThaiTaiThem.KHONG, vm.getTaiThem().getValue());

        vm.taiThem();

        assertEquals(2, api.goiTheoTen(TRA_CUU).size());
    }

    @Test
    public void FA56_LoiTaiThem_GiuDongCuVaThuLaiDungTrangDo() {
        HoTroTraCuuApi api =
                new HoTroTraCuuApi()
                        .chuanBi(LOAI, FakeCall.traVe(HoTroTraCuuApi.trangLoai(1, 1, 1)))
                        .chuanBi(PHONG, FakeCall.traVe(HoTroTraCuuApi.trangPhong(1, 1, 1)))
                        .chuanBi(
                                TRA_CUU,
                                FakeCall.traVe(HoTroTraCuuApi.trangTraCuu(40, 1, 20)),
                                FakeCall.nem(new IOException("mat mang")),
                                FakeCall.traVe(HoTroTraCuuApi.trangTraCuu(40, 2, 20)));
        TraCuuThietBiViewModel vm = tao(api);
        vm.load();

        vm.taiThem();

        assertEquals(TraCuuThietBiViewModel.TrangThaiTaiThem.LOI, vm.getTaiThem().getValue());
        assertEquals(20, dong(vm).size());
        assertEquals(UiState.TrangThai.THANH_CONG, vm.getDanhSach().getValue().getTrangThai());

        vm.thuLaiTaiThem();

        assertEquals(Integer.valueOf(2), api.goiTheoTen(TRA_CUU).get(2).thamSo[4]);
        assertEquals(40, dong(vm).size());
        assertEquals(TraCuuThietBiViewModel.TrangThaiTaiThem.KHONG, vm.getTaiThem().getValue());
    }

    @Test
    public void UISV02_XoaMotBoLocVaXoaTatCa_VeTrangMotMoiLan() {
        HoTroTraCuuApi api = apiNguonVaNhieuTraCuu(6);
        TraCuuThietBiViewModel vm = tao(api);
        vm.load();
        vm.chonLoai(LOAI_A);
        vm.chonPhong(PHONG_B);
        vm.chonTinhTrang(TinhTrangHang.CON_HANG);

        GoiApi g3 = api.goiTheoTen(TRA_CUU).get(3);
        assertEquals(LOAI_A.getMa(), g3.thamSo[1]);
        assertEquals(PHONG_B.getMa(), g3.thamSo[2]);
        assertEquals(Boolean.TRUE, g3.thamSo[3]);
        assertTrue(vm.getBoLoc().getValue().coBoLoc());

        vm.xoaLoai();

        assertNull(vm.getBoLoc().getValue().getLoai());
        assertEquals(PHONG_B, vm.getBoLoc().getValue().getPhong());
        GoiApi g4 = api.goiTheoTen(TRA_CUU).get(4);
        assertNull(g4.thamSo[1]);
        assertEquals(PHONG_B.getMa(), g4.thamSo[2]);
        assertEquals(Integer.valueOf(1), g4.thamSo[4]);

        vm.xoaBoLoc();

        assertEquals(BoLoc.MAC_DINH, vm.getBoLoc().getValue());
        assertFalse(vm.getBoLoc().getValue().coBoLoc());
        GoiApi g5 = api.goiTheoTen(TRA_CUU).get(5);
        assertNull(g5.thamSo[2]);
        assertNull(g5.thamSo[3]);
    }

    @Test
    public void FC63_OnCleared_HuyMoiYeuCauDangChayVaHenGio() {
        FakeCall<TraCuuThietBiItemPage> traCuu = FakeCall.cho();
        FakeCall<LoaiThietBiTomTatPage> loai = FakeCall.cho();
        FakeCall<PhongThietBiPage> phong = FakeCall.cho();
        HoTroTraCuuApi api =
                new HoTroTraCuuApi()
                        .chuanBi(LOAI, loai)
                        .chuanBi(PHONG, phong)
                        .chuanBi(TRA_CUU, traCuu);
        TraCuuThietBiViewModel vm = tao(api);
        vm.load();
        vm.datTuKhoa("x", false);

        vm.onCleared();

        assertTrue(traCuu.isCanceled());
        assertTrue(loai.isCanceled());
        assertTrue(phong.isCanceled());
        assertEquals(0, henGio.soViecConHieuLuc());
        assertNotNull(vm.getDanhSach().getValue());
    }
}
