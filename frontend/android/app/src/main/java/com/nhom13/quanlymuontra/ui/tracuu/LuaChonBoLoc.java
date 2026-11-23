package com.nhom13.quanlymuontra.ui.tracuu;

import androidx.annotation.NonNull;
import androidx.annotation.Nullable;
import java.util.UUID;

/**
 * Một lựa chọn của bộ lọc Loại hoặc Phòng (DS CMP-06): mã, tên, và dòng phụ (cơ sở của phòng). Bất biến;
 * hai lựa chọn bằng nhau khi cùng mã. Giữ cả tên để chip và ô chọn hiển thị đúng ngay cả khi nguồn bộ lọc
 * đang lỗi hoặc đang tải lại.
 */
public final class LuaChonBoLoc {

    @NonNull private final UUID ma;
    @NonNull private final String ten;
    @Nullable private final String phuDe;

    private LuaChonBoLoc(@NonNull UUID ma, @NonNull String ten, @Nullable String phuDe) {
        this.ma = ma;
        this.ten = ten;
        this.phuDe = phuDe;
    }

    @NonNull
    public static LuaChonBoLoc tao(@NonNull UUID ma, @NonNull String ten, @Nullable String phuDe) {
        return new LuaChonBoLoc(ma, ten, phuDe);
    }

    @NonNull
    public UUID getMa() {
        return ma;
    }

    @NonNull
    public String getTen() {
        return ten;
    }

    /** Cơ sở của phòng; {@code null} với loại thiết bị. */
    @Nullable
    public String getPhuDe() {
        return phuDe;
    }

    @Override
    public boolean equals(@Nullable Object khac) {
        return khac instanceof LuaChonBoLoc && ma.equals(((LuaChonBoLoc) khac).ma);
    }

    @Override
    public int hashCode() {
        return ma.hashCode();
    }
}
