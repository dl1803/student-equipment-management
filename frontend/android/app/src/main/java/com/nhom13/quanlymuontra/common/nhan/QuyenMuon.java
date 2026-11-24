package com.nhom13.quanlymuontra.common.nhan;

import androidx.annotation.Nullable;

/**
 * Mã trạng thái quyền mượn của API (họ {@code QUYEN_MUON}, User-Flows Phụ lục A) mà giao diện phải phân
 * nhánh theo. Chỉ là tên mã, không có quy tắc nghiệp vụ: trạng thái do máy chủ quyết định (FA-07, NT-01).
 */
public final class QuyenMuon {

    public static final String BINH_THUONG = "BINH_THUONG";
    public static final String TAM_KHOA = "TAM_KHOA";
    public static final String CAM_MUON_DAI_HAN = "CAM_MUON_DAI_HAN";

    private QuyenMuon() {}

    /** {@code true} chỉ khi máy chủ trả đúng {@link #BINH_THUONG}; mã khác hoặc {@code null} là khác Bình thường. */
    public static boolean laBinhThuong(@Nullable String ma) {
        return BINH_THUONG.equals(ma);
    }
}
