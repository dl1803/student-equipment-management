package com.nhom13.quanlymuontra.ui.phieuyeucau;

import androidx.annotation.NonNull;
import java.util.ArrayList;
import java.util.Collections;
import java.util.List;
import java.util.UUID;

/**
 * Nội dung phiếu yêu cầu mượn mà sinh viên đã soạn ở SCR-SV-04, đúng ba trường của {@code GuiYeuCauMuonRequest}:
 * {@code maPhong}, {@code thoiGianMuonDuKienGio} và {@code dsLoaiVaSoLuong} (mỗi loại một dòng, số lượng từ 1). Một phiếu
 * thuộc đúng một phòng (UF-SV-02 bước 1). Lớp thuần của giao diện, bất biến; D11-L chuyển sang yêu cầu gửi lên máy chủ.
 */
public final class NoiDungPhieu {

    /** Một dòng loại thiết bị và số lượng (từ 1). */
    public static final class DongLoai {
        @NonNull private final UUID maLoai;
        private final int soLuong;

        DongLoai(@NonNull UUID maLoai, int soLuong) {
            this.maLoai = maLoai;
            this.soLuong = soLuong;
        }

        @NonNull
        public UUID getMaLoai() {
            return maLoai;
        }

        public int getSoLuong() {
            return soLuong;
        }
    }

    @NonNull private final UUID maPhong;
    private final int thoiGianMuonDuKienGio;
    @NonNull private final List<DongLoai> dsLoaiVaSoLuong;

    NoiDungPhieu(@NonNull UUID maPhong, int thoiGianMuonDuKienGio, @NonNull List<DongLoai> dsLoaiVaSoLuong) {
        this.maPhong = maPhong;
        this.thoiGianMuonDuKienGio = thoiGianMuonDuKienGio;
        this.dsLoaiVaSoLuong = Collections.unmodifiableList(new ArrayList<>(dsLoaiVaSoLuong));
    }

    @NonNull
    public UUID getMaPhong() {
        return maPhong;
    }

    public int getThoiGianMuonDuKienGio() {
        return thoiGianMuonDuKienGio;
    }

    @NonNull
    public List<DongLoai> getDsLoaiVaSoLuong() {
        return dsLoaiVaSoLuong;
    }
}
