package com.nhom13.quanlymuontra.ui.phieuyeucau;

import static org.junit.Assert.assertEquals;
import static org.junit.Assert.assertFalse;
import static org.junit.Assert.assertNotNull;
import static org.junit.Assert.assertNull;
import static org.junit.Assert.assertTrue;

import com.nhom13.quanlymuontra.FakeCall;
import com.nhom13.quanlymuontra.Fixtures;
import com.nhom13.quanlymuontra.HoTroLiveData;
import com.nhom13.quanlymuontra.HoTroSinhVienApi;
import com.nhom13.quanlymuontra.HoTroTraCuuApi;
import com.nhom13.quanlymuontra.data.model.ApiError;
import com.nhom13.quanlymuontra.data.model.Event;
import com.nhom13.quanlymuontra.data.repository.HoSoMuonRepository;
import com.nhom13.quanlymuontra.generated.JSON;
import com.nhom13.quanlymuontra.generated.model.SinhVienTheInfo;
import com.nhom13.quanlymuontra.ui.tracuu.ChiTietLoaiThietBiViewModel.HanhDong;
import java.io.IOException;
import org.junit.After;
import org.junit.Before;
import org.junit.Test;
import retrofit2.Response;

/** Cổng quyền mượn dùng chung (FA 5.3 quy tắc 3, ĐX-UF-03): mỗi lần chọn gọi {@code svToi} mới. */
public class CongQuyenMuonTest {

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

    private static Response<SinhVienTheInfo> hoSoQuyen(String maQuyen) {
        String json = Fixtures.doc(BI_CHAN).replace("TAM_KHOA", maQuyen);
        return Response.success(new JSON().getGson().fromJson(json, SinhVienTheInfo.class));
    }

    private static CongQuyenMuon tao(HoTroTraCuuApi ho) {
        return new CongQuyenMuon(new HoSoMuonRepository(ho.api()));
    }

    private static CongQuyenMuon.KetQua ketQua(CongQuyenMuon cong) {
        Event<CongQuyenMuon.KetQua> suKien = cong.getKetQua().getValue();
        assertNotNull("Chưa có kết quả cổng", suKien);
        return suKien.getContentIfNotHandled();
    }

    @Test
    public void UFSV02_QuyenBinhThuong_DuocPhepVaGoiSvToiMoi() {
        HoTroTraCuuApi ho =
                new HoTroTraCuuApi()
                        .chuanBi(SV_TOI, FakeCall.traVe(HoTroSinhVienApi.hoSoThanhCong(BINH_THUONG)));
        CongQuyenMuon cong = tao(ho);

        cong.kiemTra(HanhDong.DAT_TRUOC);

        CongQuyenMuon.KetQua kq = ketQua(cong);
        assertTrue(kq.isDuocPhep());
        assertEquals(HanhDong.DAT_TRUOC, kq.getHanhDong());
        assertEquals(1, ho.goiTheoTen(SV_TOI).size());
        assertEquals(Boolean.FALSE, cong.getDangKiemTra().getValue());
    }

    @Test
    public void UFSV02_MoiLanChon_GoiSvToiMoiKhongDungBoDem() {
        HoTroTraCuuApi ho =
                new HoTroTraCuuApi()
                        .chuanBi(
                                SV_TOI,
                                FakeCall.traVe(HoTroSinhVienApi.hoSoThanhCong(BINH_THUONG)),
                                FakeCall.traVe(HoTroSinhVienApi.hoSoThanhCong(BI_CHAN)));
        CongQuyenMuon cong = tao(ho);

        cong.kiemTra(HanhDong.DAT_TRUOC);
        assertTrue(ketQua(cong).isDuocPhep());
        cong.kiemTra(HanhDong.DANG_KY_CHO);

        CongQuyenMuon.KetQua lan2 = ketQua(cong);
        assertFalse(lan2.isDuocPhep());
        assertEquals(HanhDong.DANG_KY_CHO, lan2.getHanhDong());
        assertEquals(2, ho.goiTheoTen(SV_TOI).size());
    }

    @Test
    public void DXUF03_QuyenKhacBinhThuongKeCaMaChuaBiet_BiChan() {
        for (String ma : new String[] {"TAM_KHOA", "CAM_MUON_DAI_HAN", "MA_LA_CHUA_BIET"}) {
            HoTroTraCuuApi ho = new HoTroTraCuuApi().chuanBi(SV_TOI, FakeCall.traVe(hoSoQuyen(ma)));
            CongQuyenMuon cong = tao(ho);

            cong.kiemTra(HanhDong.DAT_TRUOC);

            assertFalse(ma, ketQua(cong).isDuocPhep());
        }
    }

    @Test
    public void FA311_BamDupKhiDangKiemTra_ChiGoiMotLan() {
        FakeCall<SinhVienTheInfo> cho = FakeCall.cho();
        HoTroTraCuuApi ho = new HoTroTraCuuApi().chuanBi(SV_TOI, cho);
        CongQuyenMuon cong = tao(ho);

        cong.kiemTra(HanhDong.DAT_TRUOC);
        cong.kiemTra(HanhDong.DAT_TRUOC);

        assertEquals(Boolean.TRUE, cong.getDangKiemTra().getValue());
        assertEquals(1, ho.goiTheoTen(SV_TOI).size());
        cho.hoanTat(HoTroSinhVienApi.hoSoThanhCong(BINH_THUONG));
        assertEquals(Boolean.FALSE, cong.getDangKiemTra().getValue());
    }

    @Test
    public void UFSV02_LoiMang_HienLoiVaKhongDiTiep() {
        HoTroTraCuuApi ho =
                new HoTroTraCuuApi().chuanBi(SV_TOI, FakeCall.nem(new IOException("mất mạng")));
        CongQuyenMuon cong = tao(ho);

        cong.kiemTra(HanhDong.DAT_TRUOC);

        assertNotNull(cong.getLoi().getValue());
        assertEquals(ApiError.Loai.MANG, cong.getLoi().getValue().getLoai());
        assertNull(cong.getKetQua().getValue());
        assertEquals(Boolean.FALSE, cong.getDangKiemTra().getValue());
    }

    @Test
    public void FC63_Huy_HuyCallDangChayVaKhongHienLoi() {
        FakeCall<SinhVienTheInfo> cho = FakeCall.cho();
        HoTroTraCuuApi ho = new HoTroTraCuuApi().chuanBi(SV_TOI, cho);
        CongQuyenMuon cong = tao(ho);
        cong.kiemTra(HanhDong.DAT_TRUOC);

        cong.huy();

        assertTrue(cho.isCanceled());
    }
}
