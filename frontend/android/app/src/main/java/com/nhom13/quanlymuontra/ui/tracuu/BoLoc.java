package com.nhom13.quanlymuontra.ui.tracuu;

import androidx.annotation.NonNull;
import androidx.annotation.Nullable;
import java.util.Objects;

/**
 * Trạng thái bộ lọc đang áp dụng của SCR-SV-02: tên tìm kiếm, loại, phòng, tình trạng hàng. Bất biến; mỗi
 * thay đổi tạo bản mới. Tên luôn đã cắt khoảng trắng hai đầu.
 */
public final class BoLoc {

    /** Không có bộ lọc nào (mặc định). */
    public static final BoLoc MAC_DINH = new BoLoc("", null, null, TinhTrangHang.TAT_CA);

    @NonNull private final String ten;
    @Nullable private final LuaChonBoLoc loai;
    @Nullable private final LuaChonBoLoc phong;
    @NonNull private final TinhTrangHang tinhTrang;

    private BoLoc(
            @NonNull String ten,
            @Nullable LuaChonBoLoc loai,
            @Nullable LuaChonBoLoc phong,
            @NonNull TinhTrangHang tinhTrang) {
        this.ten = ten;
        this.loai = loai;
        this.phong = phong;
        this.tinhTrang = tinhTrang;
    }

    @NonNull
    public BoLoc voiTen(@Nullable String tenMoi) {
        return new BoLoc(tenMoi == null ? "" : tenMoi.trim(), loai, phong, tinhTrang);
    }

    @NonNull
    public BoLoc voiLoai(@Nullable LuaChonBoLoc loaiMoi) {
        return new BoLoc(ten, loaiMoi, phong, tinhTrang);
    }

    @NonNull
    public BoLoc voiPhong(@Nullable LuaChonBoLoc phongMoi) {
        return new BoLoc(ten, loai, phongMoi, tinhTrang);
    }

    @NonNull
    public BoLoc voiTinhTrang(@NonNull TinhTrangHang tinhTrangMoi) {
        return new BoLoc(ten, loai, phong, tinhTrangMoi);
    }

    /** Tên đã cắt khoảng trắng; chuỗi rỗng khi không tìm theo tên. */
    @NonNull
    public String getTen() {
        return ten;
    }

    /** Tên gửi lên làm tham số {@code ten}; {@code null} khi rỗng (không gửi). */
    @Nullable
    public String tenTimKiem() {
        return ten.isEmpty() ? null : ten;
    }

    @Nullable
    public LuaChonBoLoc getLoai() {
        return loai;
    }

    @Nullable
    public LuaChonBoLoc getPhong() {
        return phong;
    }

    @NonNull
    public TinhTrangHang getTinhTrang() {
        return tinhTrang;
    }

    /** Có ít nhất một bộ lọc khác mặc định (tên, loại, phòng hoặc tình trạng hàng). */
    public boolean coBoLoc() {
        return !ten.isEmpty() || loai != null || phong != null || tinhTrang != TinhTrangHang.TAT_CA;
    }

    @Override
    public boolean equals(@Nullable Object khac) {
        if (this == khac) {
            return true;
        }
        if (!(khac instanceof BoLoc)) {
            return false;
        }
        BoLoc b = (BoLoc) khac;
        return ten.equals(b.ten)
                && Objects.equals(loai, b.loai)
                && Objects.equals(phong, b.phong)
                && tinhTrang == b.tinhTrang;
    }

    @Override
    public int hashCode() {
        return Objects.hash(ten, loai, phong, tinhTrang);
    }
}
