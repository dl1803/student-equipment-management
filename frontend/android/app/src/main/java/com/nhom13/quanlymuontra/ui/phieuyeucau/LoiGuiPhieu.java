package com.nhom13.quanlymuontra.ui.phieuyeucau;

import androidx.annotation.NonNull;
import androidx.annotation.Nullable;
import com.nhom13.quanlymuontra.data.model.ApiError;

/** Kết quả lỗi của lần gửi phiếu gần nhất, dùng chung cho SCR-SV-04 và SCR-SV-05. Bất biến. */
public final class LoiGuiPhieu {

    @NonNull private final TrinhBayLoiGuiPhieu.PhanLoai phanLoai;
    @Nullable private final ApiError loi;
    @NonNull private final LoiTruongPhieu truong;

    LoiGuiPhieu(
            @NonNull TrinhBayLoiGuiPhieu.PhanLoai phanLoai,
            @Nullable ApiError loi,
            @NonNull LoiTruongPhieu truong) {
        this.phanLoai = phanLoai;
        this.loi = loi;
        this.truong = truong;
    }

    @NonNull
    public TrinhBayLoiGuiPhieu.PhanLoai getPhanLoai() {
        return phanLoai;
    }

    /** Lỗi gốc; {@code null} khi phản hồi 2xx không có thân (chưa xác định kết quả). */
    @Nullable
    public ApiError getLoi() {
        return loi;
    }

    @NonNull
    public LoiTruongPhieu getTruong() {
        return truong;
    }
}
