package com.nhom13.quanlymuontra.ui.phieuyeucau;

import androidx.annotation.NonNull;
import com.nhom13.quanlymuontra.generated.model.GuiYeuCauMuonRequest;
import com.nhom13.quanlymuontra.generated.model.LoaiVaSoLuongRequest;
import java.util.ArrayList;
import java.util.Collections;
import java.util.List;
import java.util.UUID;

/**
 * Nội dung phiếu yêu cầu mượn mà sinh viên đã soạn ở SCR-SV-04, đúng ba trường của {@code GuiYeuCauMuonRequest}:
 * {@code maPhong}, {@code thoiGianMuonDuKienGio} và {@code dsLoaiVaSoLuong} (mỗi loại một dòng, số lượng từ 1). Một phiếu
 * thuộc đúng một phòng (UF-SV-02 bước 1). Lớp thuần của giao diện, bất biến; {@link #taoYeuCau()} chuyển sang yêu cầu gửi lên máy chủ (D11-L).
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

    /** Yêu cầu gửi lên máy chủ; thứ tự dòng loại giữ đúng thứ tự trong phiếu. */
    @NonNull
    public GuiYeuCauMuonRequest taoYeuCau() {
        List<LoaiVaSoLuongRequest> ds = new ArrayList<>();
        for (DongLoai dong : dsLoaiVaSoLuong) {
            LoaiVaSoLuongRequest muc = new LoaiVaSoLuongRequest();
            muc.setMaLoai(dong.getMaLoai());
            muc.setSoLuong(dong.getSoLuong());
            ds.add(muc);
        }
        GuiYeuCauMuonRequest yeuCau = new GuiYeuCauMuonRequest();
        yeuCau.setMaPhong(maPhong);
        yeuCau.setThoiGianMuonDuKienGio(thoiGianMuonDuKienGio);
        yeuCau.setDsLoaiVaSoLuong(ds);
        return yeuCau;
    }
}
