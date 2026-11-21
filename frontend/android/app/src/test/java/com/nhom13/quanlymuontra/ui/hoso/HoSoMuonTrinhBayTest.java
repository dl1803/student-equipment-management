package com.nhom13.quanlymuontra.ui.hoso;

import static org.junit.Assert.assertEquals;

import com.nhom13.quanlymuontra.R;
import org.junit.Test;

/** Kiểm chọn chữ của SCR-SV-14 (UI-Screens 5.14, UF-SV-08). */
public class HoSoMuonTrinhBayTest {

    @Test
    public void UFSV08_HuongDan_TheoBaTrangThaiQuyenMuon() {
        assertEquals(
                R.string.ho_so_huong_dan_binh_thuong, HoSoMuonTrinhBay.huongDan("BINH_THUONG"));
        assertEquals(R.string.ho_so_huong_dan_tam_khoa, HoSoMuonTrinhBay.huongDan("TAM_KHOA"));
        assertEquals(
                R.string.ho_so_huong_dan_cam_muon_dai_han,
                HoSoMuonTrinhBay.huongDan("CAM_MUON_DAI_HAN"));
    }

    @Test
    public void DX_UI33_HuongDan_MaLaHoacThieuKhongCoHuongDan() {
        assertEquals(0, HoSoMuonTrinhBay.huongDan("MA_CHUA_TUNG_THAY"));
        assertEquals(0, HoSoMuonTrinhBay.huongDan(null));
    }

    @Test
    public void UI514_TieuDeBiChan_TheoHanhDongVuaChon() {
        assertEquals(
                R.string.ho_so_bi_chan_dat_truoc,
                HoSoMuonTrinhBay.tieuDeBiChan(HoSoMuonTrinhBay.HANH_DONG_DAT_TRUOC));
        assertEquals(
                R.string.ho_so_bi_chan_dang_ky_cho,
                HoSoMuonTrinhBay.tieuDeBiChan(HoSoMuonTrinhBay.HANH_DONG_DANG_KY_CHO));
        assertEquals(R.string.ho_so_bi_chan_chung, HoSoMuonTrinhBay.tieuDeBiChan("HANH_DONG_LA"));
        assertEquals(R.string.ho_so_bi_chan_chung, HoSoMuonTrinhBay.tieuDeBiChan(null));
    }
}
