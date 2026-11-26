package com.nhom13.quanlymuontra.ui.tracuu;

import static org.junit.Assert.assertEquals;
import static org.junit.Assert.assertFalse;
import static org.junit.Assert.assertNotNull;
import static org.junit.Assert.assertNull;
import static org.junit.Assert.assertThrows;
import static org.junit.Assert.assertTrue;

import com.nhom13.quanlymuontra.FakeCall;
import com.nhom13.quanlymuontra.Fixtures;
import com.nhom13.quanlymuontra.HoTroLiveData;
import com.nhom13.quanlymuontra.HoTroSinhVienApi;
import com.nhom13.quanlymuontra.HoTroTraCuuApi;
import com.nhom13.quanlymuontra.HoTroXacThuc;
import com.nhom13.quanlymuontra.data.model.ApiError;
import com.nhom13.quanlymuontra.data.model.Event;
import com.nhom13.quanlymuontra.data.model.UiState;
import com.nhom13.quanlymuontra.data.repository.HoSoMuonRepository;
import com.nhom13.quanlymuontra.data.repository.TraCuuRepository;
import com.nhom13.quanlymuontra.generated.JSON;
import com.nhom13.quanlymuontra.generated.api.SinhVienApi;
import com.nhom13.quanlymuontra.generated.model.SinhVienTheInfo;
import com.nhom13.quanlymuontra.generated.model.TraCuuThietBiChiTiet;
import com.nhom13.quanlymuontra.ui.tracuu.ChiTietLoaiThietBiViewModel.HanhDong;
import com.nhom13.quanlymuontra.ui.tracuu.ChiTietLoaiThietBiViewModel.KetQuaCong;
import java.io.IOException;
import java.util.UUID;
import org.junit.After;
import org.junit.Before;
import org.junit.Test;
import retrofit2.Response;

/**
 * Logic của SCR-SV-03 (UI-Screens 5.3, UF-SV-01, FA 5.6): tải chi tiết, nhánh hàng vừa có lại, cổng kiểm tra quyền
 * mượn. {@code svDangKyCho} không được chuẩn bị ở bất kỳ test nào nên mọi nhánh cổng tự chứng minh không gửi đăng ký
 * chờ (kết quả D09-L).
 */
public class ChiTietLoaiThietBiViewModelTest {

    private static final UUID MA_LOAI = UUID.fromString("11111111-1111-1111-1111-111111111111");
    private static final UUID MA_PHONG = UUID.fromString("22222222-2222-2222-2222-222222222222");
    private static final String CHI_TIET = "svTraCuuThietBiChiTiet";
    private static final String CON_HANG = "thanh-cong/sv-tra-cuu-chi-tiet-con-hang.json";
    private static final String HET_HANG = "thanh-cong/sv-tra-cuu-chi-tiet-het-hang.json";
    private static final String QUYEN_BINH_THUONG = "thanh-cong/sv-toi-binh-thuong.json";
    private static final String QUYEN_BI_CHAN = "thanh-cong/sv-toi-bi-chan.json";

    @Before
    public void chuanBi() {
        HoTroLiveData.batDau();
    }

    @After
    public void don() {
        HoTroLiveData.ketThuc();
    }

    private static Response<TraCuuThietBiChiTiet> chiTiet(String tep) {
        return Response.success(
                new JSON().getGson().fromJson(Fixtures.doc(tep), TraCuuThietBiChiTiet.class));
    }

    private static Response<SinhVienTheInfo> hoSo(String tep) {
        return HoTroSinhVienApi.hoSoThanhCong(tep);
    }

    /** Hồ sơ lấy từ fixture bị chặn nhưng đổi mã quyền mượn thành {@code maQuyen}. */
    private static Response<SinhVienTheInfo> hoSoQuyen(String maQuyen) {
        String json = Fixtures.doc(QUYEN_BI_CHAN).replace("TAM_KHOA", maQuyen);
        return Response.success(new JSON().getGson().fromJson(json, SinhVienTheInfo.class));
    }

    private static ChiTietLoaiThietBiViewModel taoVm(HoTroTraCuuApi ho) {
        SinhVienApi api = ho.api();
        ChiTietLoaiThietBiViewModel vm =
                new ChiTietLoaiThietBiViewModel(
                        new TraCuuRepository(api), new HoSoMuonRepository(api));
        vm.khoiTao(MA_LOAI, MA_PHONG);
        return vm;
    }

    private static KetQuaCong ketQuaCong(ChiTietLoaiThietBiViewModel vm) {
        Event<KetQuaCong> suKien = vm.getKetQuaCong().getValue();
        assertNotNull("Chưa có kết quả cổng", suKien);
        return suKien.getContentIfNotHandled();
    }

    private static Integer soLuong(ChiTietLoaiThietBiViewModel vm) {
        return vm.getChiTiet().getValue().getDuLieu().getSoLuongKhaDung();
    }

    // ---------------------------------------------------------------- tải chi tiết

    @Test
    public void UISV03_TaiConHang_GoiDungMaVaThanhCongVoiNutDatTruoc() {
        HoTroTraCuuApi ho =
                new HoTroTraCuuApi().chuanBi(CHI_TIET, FakeCall.traVe(chiTiet(CON_HANG)));
        ChiTietLoaiThietBiViewModel vm = taoVm(ho);

        vm.load();

        UiState<TraCuuThietBiChiTiet> trangThai = vm.getChiTiet().getValue();
        assertEquals(UiState.TrangThai.THANH_CONG, trangThai.getTrangThai());
        assertEquals(Integer.valueOf(5), soLuong(vm));
        assertEquals(
                HanhDong.DAT_TRUOC,
                ChiTietLoaiTrinhBay.hanhDongChoSoLuong(trangThai.getDuLieu().getSoLuongKhaDung()));
        assertEquals(1, ho.goiTheoTen(CHI_TIET).size());
        assertEquals(MA_LOAI, ho.goiTheoTen(CHI_TIET).get(0).thamSo[0]);
        assertEquals(MA_PHONG, ho.goiTheoTen(CHI_TIET).get(0).thamSo[1]);
    }

    @Test
    public void UISV03_TaiHetHang_NutDangKyChoVaCoNgayDuKien() {
        HoTroTraCuuApi ho =
                new HoTroTraCuuApi().chuanBi(CHI_TIET, FakeCall.traVe(chiTiet(HET_HANG)));
        ChiTietLoaiThietBiViewModel vm = taoVm(ho);

        vm.load();

        TraCuuThietBiChiTiet duLieu = vm.getChiTiet().getValue().getDuLieu();
        assertEquals(Integer.valueOf(0), duLieu.getSoLuongKhaDung());
        assertTrue(ChiTietLoaiTrinhBay.hetHang(duLieu.getSoLuongKhaDung()));
        assertEquals(
                HanhDong.DANG_KY_CHO,
                ChiTietLoaiTrinhBay.hanhDongChoSoLuong(duLieu.getSoLuongKhaDung()));
        assertNotNull(duLieu.getNgayDuKienCoSan());
    }

    @Test
    public void FC22_LoadLanHai_KhongTaiLaiTuMayChu() {
        HoTroTraCuuApi ho =
                new HoTroTraCuuApi().chuanBi(CHI_TIET, FakeCall.traVe(chiTiet(CON_HANG)));
        ChiTietLoaiThietBiViewModel vm = taoVm(ho);

        vm.load();
        vm.load();

        assertEquals(1, ho.goiTheoTen(CHI_TIET).size());
    }

    @Test
    public void FC22_DanhDauCanTaiLai_LoadTaiLaiTuMayChu() {
        HoTroTraCuuApi ho =
                new HoTroTraCuuApi()
                        .chuanBi(
                                CHI_TIET,
                                FakeCall.traVe(chiTiet(CON_HANG)),
                                FakeCall.traVe(chiTiet(CON_HANG)));
        ChiTietLoaiThietBiViewModel vm = taoVm(ho);

        vm.load();
        vm.danhDauCanTaiLai();
        vm.load();

        assertEquals(2, ho.goiTheoTen(CHI_TIET).size());
    }

    @Test
    public void FA56_Refresh_GiuDuLieuCuKhiDangTai() {
        FakeCall<TraCuuThietBiChiTiet> lanHai = FakeCall.cho();
        HoTroTraCuuApi ho =
                new HoTroTraCuuApi()
                        .chuanBi(CHI_TIET, FakeCall.traVe(chiTiet(CON_HANG)), lanHai);
        ChiTietLoaiThietBiViewModel vm = taoVm(ho);
        vm.load();

        vm.refresh();

        UiState<TraCuuThietBiChiTiet> trangThai = vm.getChiTiet().getValue();
        assertEquals(UiState.TrangThai.DANG_TAI, trangThai.getTrangThai());
        assertEquals(Integer.valueOf(5), trangThai.getDuLieu().getSoLuongKhaDung());
    }

    @Test
    public void UISV03_HangVuaCoLai_TaiLaiThayHetHangThanhConHangVaNutDoiSangDatTruoc() {
        HoTroTraCuuApi ho =
                new HoTroTraCuuApi()
                        .chuanBi(
                                CHI_TIET,
                                FakeCall.traVe(chiTiet(HET_HANG)),
                                FakeCall.traVe(chiTiet(CON_HANG)));
        ChiTietLoaiThietBiViewModel vm = taoVm(ho);
        vm.load();
        assertEquals(
                HanhDong.DANG_KY_CHO, ChiTietLoaiTrinhBay.hanhDongChoSoLuong(soLuong(vm)));

        vm.refresh();

        assertEquals(Integer.valueOf(5), soLuong(vm));
        assertEquals(HanhDong.DAT_TRUOC, ChiTietLoaiTrinhBay.hanhDongChoSoLuong(soLuong(vm)));
    }

    @Test
    public void FA56_RefreshLoiMang_GiuDuLieuCuVaBaoLoi() {
        HoTroTraCuuApi ho =
                new HoTroTraCuuApi()
                        .chuanBi(
                                CHI_TIET,
                                FakeCall.traVe(chiTiet(CON_HANG)),
                                FakeCall.nem(new IOException("mat ket noi")));
        ChiTietLoaiThietBiViewModel vm = taoVm(ho);
        vm.load();

        vm.refresh();

        UiState<TraCuuThietBiChiTiet> trangThai = vm.getChiTiet().getValue();
        assertEquals(UiState.TrangThai.LOI, trangThai.getTrangThai());
        assertEquals(ApiError.Loai.MANG, trangThai.getLoi().getLoai());
        assertEquals(Integer.valueOf(5), trangThai.getDuLieu().getSoLuongKhaDung());
    }

    @Test
    public void UISV03_MaKhongTonTai404_LoiKhongTimThayVaKhongCoDuLieu() {
        HoTroTraCuuApi ho =
                new HoTroTraCuuApi()
                        .chuanBi(
                                CHI_TIET,
                                FakeCall.traVe(
                                        HoTroXacThuc.<TraCuuThietBiChiTiet>phanHoiLoi(
                                                404, "loi/404-khong-tim-thay.json")));
        ChiTietLoaiThietBiViewModel vm = taoVm(ho);

        vm.load();

        UiState<TraCuuThietBiChiTiet> trangThai = vm.getChiTiet().getValue();
        assertEquals(UiState.TrangThai.LOI, trangThai.getTrangThai());
        assertTrue(ChiTietLoaiTrinhBay.laKhongTimThay(trangThai.getLoi()));
        assertNull(trangThai.getDuLieu());
    }

    @Test
    public void FA56_PhanHoiMuonCuaLanTaiCu_BiBoQua() {
        FakeCall<TraCuuThietBiChiTiet> lanMot = FakeCall.cho();
        FakeCall<TraCuuThietBiChiTiet> lanHai = FakeCall.cho();
        HoTroTraCuuApi ho = new HoTroTraCuuApi().chuanBi(CHI_TIET, lanMot, lanHai);
        ChiTietLoaiThietBiViewModel vm = taoVm(ho);
        vm.load();
        vm.refresh();

        lanHai.hoanTat(chiTiet(CON_HANG));
        lanMot.hoanTat(chiTiet(HET_HANG));

        assertTrue(lanMot.isCanceled());
        assertEquals(Integer.valueOf(5), soLuong(vm));
    }

    @Test
    public void FA51_KhoiTao_ChiLanDauCoTacDung() {
        HoTroTraCuuApi ho =
                new HoTroTraCuuApi().chuanBi(CHI_TIET, FakeCall.traVe(chiTiet(CON_HANG)));
        ChiTietLoaiThietBiViewModel vm = taoVm(ho);

        vm.khoiTao(UUID.randomUUID(), UUID.randomUUID());
        vm.load();

        assertEquals(MA_LOAI, ho.goiTheoTen(CHI_TIET).get(0).thamSo[0]);
        assertEquals(MA_PHONG, ho.goiTheoTen(CHI_TIET).get(0).thamSo[1]);
    }

    @Test
    public void FA51_ChuaKhoiTao_TaiThiNemIllegalState() {
        SinhVienApi api = new HoTroTraCuuApi().api();
        ChiTietLoaiThietBiViewModel vm =
                new ChiTietLoaiThietBiViewModel(
                        new TraCuuRepository(api), new HoSoMuonRepository(api));

        assertThrows(IllegalStateException.class, vm::load);
    }

    // ---------------------------------------------------------------- cổng kiểm tra quyền mượn

    @Test
    public void UISV03_CongBinhThuong_DatTruoc_DuocPhepVaGoiSvToiMoi() {
        HoTroTraCuuApi ho =
                new HoTroTraCuuApi()
                        .chuanBi(CHI_TIET, FakeCall.traVe(chiTiet(CON_HANG)))
                        .chuanBi("svToi", FakeCall.traVe(hoSo(QUYEN_BINH_THUONG)));
        ChiTietLoaiThietBiViewModel vm = taoVm(ho);
        vm.load();

        vm.chonDatTruoc();

        KetQuaCong ketQua = ketQuaCong(vm);
        assertTrue(ketQua.isDuocPhep());
        assertEquals(HanhDong.DAT_TRUOC, ketQua.getHanhDong());
        assertEquals(1, ho.goiTheoTen("svToi").size());
        assertFalse(vm.getDangKiemTraQuyen().getValue());
        assertNull(vm.getLoiKiemTraQuyen().getValue());
    }

    @Test
    public void UISV03_CongBinhThuong_DangKyCho_DuocPhepNhungKhongGuiDangKyCho() {
        HoTroTraCuuApi ho =
                new HoTroTraCuuApi()
                        .chuanBi(CHI_TIET, FakeCall.traVe(chiTiet(HET_HANG)))
                        .chuanBi("svToi", FakeCall.traVe(hoSo(QUYEN_BINH_THUONG)));
        ChiTietLoaiThietBiViewModel vm = taoVm(ho);
        vm.load();

        vm.chonDangKyCho();

        KetQuaCong ketQua = ketQuaCong(vm);
        assertTrue(ketQua.isDuocPhep());
        assertEquals(HanhDong.DANG_KY_CHO, ketQua.getHanhDong());
        assertTrue(ho.goiTheoTen("svDangKyCho").isEmpty());
    }

    @Test
    public void UISV03_CongTamKhoa_DatTruoc_BiChanVaNeuDungHanhDong() {
        HoTroTraCuuApi ho =
                new HoTroTraCuuApi()
                        .chuanBi(CHI_TIET, FakeCall.traVe(chiTiet(CON_HANG)))
                        .chuanBi("svToi", FakeCall.traVe(hoSo(QUYEN_BI_CHAN)));
        ChiTietLoaiThietBiViewModel vm = taoVm(ho);
        vm.load();

        vm.chonDatTruoc();

        KetQuaCong ketQua = ketQuaCong(vm);
        assertFalse(ketQua.isDuocPhep());
        assertEquals("DAT_TRUOC", ketQua.getHanhDong().getMaThamSo());
    }

    @Test
    public void UISV03_CongCamMuonDaiHan_DangKyCho_BiChanVaKhongGuiDangKyCho() {
        HoTroTraCuuApi ho =
                new HoTroTraCuuApi()
                        .chuanBi(CHI_TIET, FakeCall.traVe(chiTiet(HET_HANG)))
                        .chuanBi("svToi", FakeCall.traVe(hoSoQuyen("CAM_MUON_DAI_HAN")));
        ChiTietLoaiThietBiViewModel vm = taoVm(ho);
        vm.load();

        vm.chonDangKyCho();

        KetQuaCong ketQua = ketQuaCong(vm);
        assertFalse(ketQua.isDuocPhep());
        assertEquals("DANG_KY_CHO", ketQua.getHanhDong().getMaThamSo());
        assertTrue(ho.goiTheoTen("svDangKyCho").isEmpty());
    }

    @Test
    public void UISV03_CongMaQuyenChuaBiet_CoiLaBiChan() {
        HoTroTraCuuApi ho =
                new HoTroTraCuuApi()
                        .chuanBi(CHI_TIET, FakeCall.traVe(chiTiet(CON_HANG)))
                        .chuanBi("svToi", FakeCall.traVe(hoSoQuyen("MA_CHUA_BIET")));
        ChiTietLoaiThietBiViewModel vm = taoVm(ho);
        vm.load();

        vm.chonDatTruoc();

        assertFalse(ketQuaCong(vm).isDuocPhep());
    }

    @Test
    public void UISV03_CongLoiMang_KhongDiTiepVaKhongPhatSuKien() {
        HoTroTraCuuApi ho =
                new HoTroTraCuuApi()
                        .chuanBi(CHI_TIET, FakeCall.traVe(chiTiet(HET_HANG)))
                        .chuanBi("svToi", FakeCall.nem(new IOException("mat ket noi")));
        ChiTietLoaiThietBiViewModel vm = taoVm(ho);
        vm.load();

        vm.chonDangKyCho();

        assertNull(vm.getKetQuaCong().getValue());
        assertNotNull(vm.getLoiKiemTraQuyen().getValue());
        assertEquals(ApiError.Loai.MANG, vm.getLoiKiemTraQuyen().getValue().getLoai());
        assertFalse(vm.getDangKiemTraQuyen().getValue());
        assertTrue(ho.goiTheoTen("svDangKyCho").isEmpty());
    }

    @Test
    public void UISV03_CongLoiHeThong500_KhongDiTiepVaGiuMaTheoDoi() {
        HoTroTraCuuApi ho =
                new HoTroTraCuuApi()
                        .chuanBi(CHI_TIET, FakeCall.traVe(chiTiet(CON_HANG)))
                        .chuanBi(
                                "svToi",
                                FakeCall.traVe(
                                        HoTroXacThuc.<SinhVienTheInfo>phanHoiLoi(
                                                500, "loi/500-loi-he-thong.json")));
        ChiTietLoaiThietBiViewModel vm = taoVm(ho);
        vm.load();

        vm.chonDatTruoc();

        assertNull(vm.getKetQuaCong().getValue());
        ApiError loi = vm.getLoiKiemTraQuyen().getValue();
        assertNotNull(loi);
        assertEquals(Integer.valueOf(500), loi.getStatus());
        assertNotNull(ChiTietLoaiTrinhBay.maTheoDoi(loi));
    }

    @Test
    public void UISV03_CongLoiRoiChonLai_GoiSvToiMoiVaXoaLoi() {
        HoTroTraCuuApi ho =
                new HoTroTraCuuApi()
                        .chuanBi(CHI_TIET, FakeCall.traVe(chiTiet(CON_HANG)))
                        .chuanBi(
                                "svToi",
                                FakeCall.nem(new IOException("mat ket noi")),
                                FakeCall.traVe(hoSo(QUYEN_BINH_THUONG)));
        ChiTietLoaiThietBiViewModel vm = taoVm(ho);
        vm.load();
        vm.chonDatTruoc();
        assertNotNull(vm.getLoiKiemTraQuyen().getValue());

        vm.chonDatTruoc();

        assertEquals(2, ho.goiTheoTen("svToi").size());
        assertNull(vm.getLoiKiemTraQuyen().getValue());
        assertTrue(ketQuaCong(vm).isDuocPhep());
    }

    @Test
    public void FA311_CongBamTrung_ChiMotYeuCauSvToi() {
        FakeCall<SinhVienTheInfo> dangCho = FakeCall.cho();
        HoTroTraCuuApi ho =
                new HoTroTraCuuApi()
                        .chuanBi(CHI_TIET, FakeCall.traVe(chiTiet(CON_HANG)))
                        .chuanBi("svToi", dangCho);
        ChiTietLoaiThietBiViewModel vm = taoVm(ho);
        vm.load();

        vm.chonDatTruoc();
        vm.chonDatTruoc();
        vm.chonDangKyCho();

        assertEquals(1, dangCho.soLanEnqueue());
        assertTrue(vm.getDangKiemTraQuyen().getValue());
        dangCho.hoanTat(hoSo(QUYEN_BINH_THUONG));
        assertFalse(vm.getDangKiemTraQuyen().getValue());
        assertEquals(HanhDong.DAT_TRUOC, ketQuaCong(vm).getHanhDong());
    }

    @Test
    public void UISV03_MoiLanChonGoiSvToiMoi_KhongDungBoDem() {
        HoTroTraCuuApi ho =
                new HoTroTraCuuApi()
                        .chuanBi(CHI_TIET, FakeCall.traVe(chiTiet(CON_HANG)))
                        .chuanBi(
                                "svToi",
                                FakeCall.traVe(hoSo(QUYEN_BINH_THUONG)),
                                FakeCall.traVe(hoSo(QUYEN_BI_CHAN)));
        ChiTietLoaiThietBiViewModel vm = taoVm(ho);
        vm.load();

        vm.chonDatTruoc();
        assertTrue(ketQuaCong(vm).isDuocPhep());
        vm.chonDatTruoc();

        assertEquals(2, ho.goiTheoTen("svToi").size());
        assertFalse(ketQuaCong(vm).isDuocPhep());
    }

    @Test
    public void FC63_OnCleared_HuyCaHaiYeuCauDangChay() {
        FakeCall<TraCuuThietBiChiTiet> chiTietDangCho = FakeCall.cho();
        FakeCall<SinhVienTheInfo> quyenDangCho = FakeCall.cho();
        HoTroTraCuuApi ho =
                new HoTroTraCuuApi()
                        .chuanBi(CHI_TIET, chiTietDangCho)
                        .chuanBi("svToi", quyenDangCho);
        ChiTietLoaiThietBiViewModel vm = taoVm(ho);
        vm.load();
        vm.chonDatTruoc();

        vm.onCleared();

        assertTrue(chiTietDangCho.isCanceled());
        assertTrue(quyenDangCho.isCanceled());
    }
}
