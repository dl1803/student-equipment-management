package com.nhom13.quanlymuontra.ui.phieuyeucau;

import androidx.annotation.NonNull;
import androidx.annotation.Nullable;
import java.util.ArrayList;
import java.util.Collections;
import java.util.List;

/** Tóm tắt phiếu để hiển thị ở SCR-SV-05 (UI-Screens 5.5): tên phòng, cơ sở, từng loại kèm số lượng, số giờ. */
public final class TomTatPhieu {

    /** Một dòng loại: tên hiển thị và số lượng. */
    public static final class Dong {
        @NonNull private final String tenLoai;
        private final int soLuong;

        Dong(@NonNull String tenLoai, int soLuong) {
            this.tenLoai = tenLoai;
            this.soLuong = soLuong;
        }

        @NonNull
        public String getTenLoai() {
            return tenLoai;
        }

        public int getSoLuong() {
            return soLuong;
        }
    }

    @NonNull private final String tenPhong;
    @Nullable private final String coSo;
    private final int thoiGianGio;
    @NonNull private final List<Dong> cacDong;

    TomTatPhieu(
            @NonNull String tenPhong,
            @Nullable String coSo,
            int thoiGianGio,
            @NonNull List<Dong> cacDong) {
        this.tenPhong = tenPhong;
        this.coSo = coSo;
        this.thoiGianGio = thoiGianGio;
        this.cacDong = Collections.unmodifiableList(new ArrayList<>(cacDong));
    }

    @NonNull
    public String getTenPhong() {
        return tenPhong;
    }

    @Nullable
    public String getCoSo() {
        return coSo;
    }

    public int getThoiGianGio() {
        return thoiGianGio;
    }

    @NonNull
    public List<Dong> getCacDong() {
        return cacDong;
    }
}
