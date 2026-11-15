package com.nhom13.quanlymuontra.data.session;

import static org.junit.Assert.assertEquals;
import static org.junit.Assert.assertFalse;
import static org.junit.Assert.assertNotNull;
import static org.junit.Assert.assertNull;
import static org.junit.Assert.assertTrue;

import androidx.arch.core.executor.ArchTaskExecutor;
import androidx.arch.core.executor.TaskExecutor;
import org.junit.After;
import org.junit.Before;
import org.junit.Test;

public class SessionManagerTest {

    private FakeSharedPreferences prefs;
    private FakeTokenCipher cipher;
    private SessionManager phien;

    @Before
    public void chuanBi() {
        // LiveData kiểm tra luồng chính bằng Looper của Android, không có trên JVM: thay bằng bộ thực
        // thi chạy ngay tại luồng gọi (cùng cách InstantTaskExecutorRule của core-testing làm).
        ArchTaskExecutor.getInstance()
                .setDelegate(
                        new TaskExecutor() {
                            @Override
                            public void executeOnDiskIO(Runnable runnable) {
                                runnable.run();
                            }

                            @Override
                            public void postToMainThread(Runnable runnable) {
                                runnable.run();
                            }

                            @Override
                            public boolean isMainThread() {
                                return true;
                            }
                        });
        prefs = new FakeSharedPreferences();
        cipher = new FakeTokenCipher();
        phien = new SessionManager(new TokenStore(prefs, cipher));
    }

    @After
    public void don() {
        ArchTaskExecutor.getInstance().setDelegate(null);
    }

    @Test
    public void FA51_VuaTao_TrangThaiChuaKhoiPhucVaKhongCoToken() {
        assertEquals(TrangThaiPhien.CHUA_KHOI_PHUC, phien.getTrangThaiPhien().getValue());
        assertNull(phien.getTokenPair());
    }

    @Test
    public void UFCM03_KhoiPhucKhiChuaCoPhien_KhongCoPhien() {
        phien.restore();

        assertEquals(TrangThaiPhien.KHONG_CO_PHIEN, phien.getTrangThaiPhien().getValue());
        assertNull(phien.getTokenPair());
    }

    @Test
    public void UFCM03_LuuRoiMoLaiUngDung_KhoiPhucDuocPhien() {
        assertTrue(phien.save(new TokenPair("access-1", "refresh-1")));
        SessionManager lanMoLai = new SessionManager(new TokenStore(prefs, cipher));

        lanMoLai.restore();

        assertEquals(TrangThaiPhien.CO_PHIEN, lanMoLai.getTrangThaiPhien().getValue());
        TokenPair capToken = lanMoLai.getTokenPair();
        assertNotNull(capToken);
        assertEquals("access-1", capToken.getAccessToken());
        assertEquals("refresh-1", capToken.getRefreshToken());
    }

    @Test
    public void FA54_KhoaMatKhiKhoiPhuc_KhongCoPhienVaXoaDuLieuLuu() {
        phien.save(new TokenPair("access-1", "refresh-1"));
        SessionManager lanMoLai = new SessionManager(new TokenStore(prefs, cipher));
        cipher.loiKhiGiaiMa = true;

        lanMoLai.restore();

        assertEquals(TrangThaiPhien.KHONG_CO_PHIEN, lanMoLai.getTrangThaiPhien().getValue());
        assertNull(lanMoLai.getTokenPair());
        assertTrue(prefs.saoChepDuLieu().isEmpty());
    }

    @Test
    public void FA35_Luu_DatTokenMoiVaThanhCoPhien() {
        phien.save(new TokenPair("access-1", "refresh-1"));

        phien.save(new TokenPair("access-2", "refresh-2"));

        assertEquals(TrangThaiPhien.CO_PHIEN, phien.getTrangThaiPhien().getValue());
        TokenPair capToken = phien.getTokenPair();
        assertNotNull(capToken);
        assertEquals("access-2", capToken.getAccessToken());
        assertEquals("refresh-2", capToken.getRefreshToken());
    }

    @Test
    public void FA35_LuuXongMoiTraVe_KhoLuuDaCoCapTokenMoi() {
        phien.save(new TokenPair("access-1", "refresh-1"));
        phien.save(new TokenPair("access-2", "refresh-2"));

        TokenPair daLuu = new TokenStore(prefs, cipher).read();

        assertNotNull(daLuu);
        assertEquals("access-2", daLuu.getAccessToken());
        assertEquals("refresh-2", daLuu.getRefreshToken());
    }

    @Test
    public void FA54_LuuKhoThatBai_TraFalseNhungPhienVanDungDuocTrongBoNho() {
        cipher.loiKhiMaHoa = true;

        boolean daLuu = phien.save(new TokenPair("access-1", "refresh-1"));

        assertFalse(daLuu);
        assertEquals(TrangThaiPhien.CO_PHIEN, phien.getTrangThaiPhien().getValue());
        TokenPair capToken = phien.getTokenPair();
        assertNotNull(capToken);
        assertEquals("access-1", capToken.getAccessToken());
        assertTrue(prefs.saoChepDuLieu().isEmpty());
    }

    @Test
    public void UFCM03_Clear_XoaTokenKhoLuuVaThanhKhongCoPhien() {
        phien.save(new TokenPair("access-1", "refresh-1"));

        phien.clear();

        assertEquals(TrangThaiPhien.KHONG_CO_PHIEN, phien.getTrangThaiPhien().getValue());
        assertNull(phien.getTokenPair());
        assertTrue(prefs.saoChepDuLieu().isEmpty());
    }
}
