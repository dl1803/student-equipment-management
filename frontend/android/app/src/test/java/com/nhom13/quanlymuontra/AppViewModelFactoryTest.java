package com.nhom13.quanlymuontra;

import static org.junit.Assert.assertThrows;
import static org.junit.Assert.assertTrue;

import androidx.lifecycle.ViewModel;
import org.junit.Test;

public class AppViewModelFactoryTest {

    @Test
    public void FA51_LopViewModelChuaDangKy_NemIllegalArgumentException() {
        AppViewModelFactory nhaMay = new AppViewModelFactory(new AppContainer());

        IllegalArgumentException loi =
                assertThrows(IllegalArgumentException.class, () -> nhaMay.create(ViewModel.class));

        assertTrue(loi.getMessage().contains(ViewModel.class.getName()));
    }
}
