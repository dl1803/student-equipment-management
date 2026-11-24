package com.nhom13.quanlymuontra.ui.tracuu;

import androidx.annotation.NonNull;
import androidx.annotation.Nullable;
import androidx.lifecycle.LiveData;
import androidx.lifecycle.MutableLiveData;
import androidx.lifecycle.ViewModel;
import com.nhom13.quanlymuontra.common.HangSo;
import com.nhom13.quanlymuontra.data.model.ApiError;
import com.nhom13.quanlymuontra.data.model.UiState;
import com.nhom13.quanlymuontra.data.repository.ApiCallback;
import com.nhom13.quanlymuontra.data.repository.TraCuuRepository;
import com.nhom13.quanlymuontra.generated.model.LoaiThietBiTomTat;
import com.nhom13.quanlymuontra.generated.model.LoaiThietBiTomTatPage;
import com.nhom13.quanlymuontra.generated.model.PhongThietBi;
import com.nhom13.quanlymuontra.generated.model.PhongThietBiPage;
import com.nhom13.quanlymuontra.generated.model.TraCuuThietBiItem;
import com.nhom13.quanlymuontra.generated.model.TraCuuThietBiItemPage;
import java.util.ArrayList;
import java.util.Collections;
import java.util.List;
import java.util.UUID;
import retrofit2.Call;

/**
 * SCR-SV-02 Tra cứu thiết bị (UI-Screens 5.2, FA 3.10, 5.6; FC 6.2, 6.3).
 *
 * <ul>
 *   <li>{@link #load()} chỉ tải khi {@code canTaiLai} là {@code true}; {@link #refresh()} tải vô điều kiện
 *       (ĐX-FC-22). Mỗi lần tải gồm danh sách trang 1 và hai nguồn bộ lọc, mỗi phần có trạng thái riêng nên
 *       lỗi nguồn bộ lọc không làm mất danh sách (UI-Screens 5.2).
 *   <li>Đổi tìm kiếm hoặc bộ lọc: xóa dòng cũ và tải từ trang 1; yêu cầu cũ bị hủy và phản hồi muộn của nó bị
 *       bỏ qua (số lần tải {@code lanTai}) nên kết quả cũ không ghi đè kết quả mới. Kéo làm mới giữ dòng cũ
 *       cho tới khi có kết quả mới.
 *   <li>Gõ tên: chờ {@link HangSo#DO_TRE_GO_TIM_KIEM_MS} (ĐX-FA-05); phím tìm gửi ngay. Đổi bộ lọc khác khi
 *       còn tên đang chờ thì chốt luôn tên đó, chỉ gửi một yêu cầu.
 *   <li>Tải thêm theo {@code tongSo}; lỗi tải thêm giữ nguyên các dòng đã có.
 *   <li>Mọi truy vấn là đọc nên hủy im lặng được; {@code HUY} không hiển thị gì (FA 3.3 quy tắc 4). Thứ tự
 *       dòng do máy chủ quyết định, không sắp xếp lại (FA 3.10).
 * </ul>
 */
public final class TraCuuThietBiViewModel extends ViewModel {

    /** Trạng thái dòng cuối danh sách (UI-Screens 5.2, DS CMP-16). */
    public enum TrangThaiTaiThem {
        KHONG,
        DANG_TAI,
        LOI
    }

    private final TraCuuRepository traCuuRepository;
    private final BoHenGio boHenGio;

    private final MutableLiveData<UiState<List<TraCuuThietBiItem>>> danhSach =
            new MutableLiveData<>(UiState.<List<TraCuuThietBiItem>>dangTai(null));
    private final MutableLiveData<TrangThaiTaiThem> taiThem =
            new MutableLiveData<>(TrangThaiTaiThem.KHONG);
    private final MutableLiveData<BoLoc> boLoc = new MutableLiveData<>(BoLoc.MAC_DINH);
    private final MutableLiveData<UiState<List<LuaChonBoLoc>>> dsLoai =
            new MutableLiveData<>(UiState.<List<LuaChonBoLoc>>dangTai(null));
    private final MutableLiveData<UiState<List<LuaChonBoLoc>>> dsPhong =
            new MutableLiveData<>(UiState.<List<LuaChonBoLoc>>dangTai(null));

    private final List<TraCuuThietBiItem> daTai = new ArrayList<>();
    private boolean canTaiLai = true;
    private int lanTai = 0;
    private int trangDaTai = 0;
    private int tongSo = 0;
    private boolean hetTrang = false;
    @Nullable private Call<TraCuuThietBiItemPage> callDanhSach;

    private int lanLoai = 0;
    private int lanPhong = 0;
    @Nullable private Call<LoaiThietBiTomTatPage> callLoai;
    @Nullable private Call<PhongThietBiPage> callPhong;

    @Nullable private Runnable huyHenGio;
    @Nullable private String tuKhoaDangCho;

    public TraCuuThietBiViewModel(
            @NonNull TraCuuRepository traCuuRepository, @NonNull BoHenGio boHenGio) {
        this.traCuuRepository = traCuuRepository;
        this.boHenGio = boHenGio;
    }

    // ---------------------------------------------------------------- trạng thái phơi bày

    /** Danh sách kết quả; {@code getDuLieu()} là các dòng đã tải (mọi trang). */
    @NonNull
    public LiveData<UiState<List<TraCuuThietBiItem>>> getDanhSach() {
        return danhSach;
    }

    @NonNull
    public LiveData<TrangThaiTaiThem> getTaiThem() {
        return taiThem;
    }

    /** Bộ lọc đang áp dụng (tên chỉ cập nhật sau độ trễ gõ). */
    @NonNull
    public LiveData<BoLoc> getBoLoc() {
        return boLoc;
    }

    @NonNull
    public LiveData<UiState<List<LuaChonBoLoc>>> getDsLoai() {
        return dsLoai;
    }

    @NonNull
    public LiveData<UiState<List<LuaChonBoLoc>>> getDsPhong() {
        return dsPhong;
    }

    /** Chữ cần có trong ô tìm kiếm: tên đang chờ độ trễ gõ nếu có, không thì tên đang áp dụng. */
    @NonNull
    public String getTuKhoaHienThi() {
        return tuKhoaDangCho != null ? tuKhoaDangCho : giaTriBoLoc().getTen();
    }

    // ---------------------------------------------------------------- tải

    /** Tải nếu đang cần tải lại (mở màn hình hoặc quay lại từ ngăn xếp). */
    public void load() {
        if (!canTaiLai) {
            return;
        }
        canTaiLai = false;
        taiTatCa();
    }

    /** Tải vô điều kiện cả danh sách và hai nguồn bộ lọc: kéo để làm mới. */
    public void refresh() {
        canTaiLai = false;
        taiTatCa();
    }

    /** Fragment gọi ở {@code onDestroyView} khi không phải xoay màn hình: lần mở sau phải tải lại. */
    public void danhDauCanTaiLai() {
        canTaiLai = true;
    }

    /** Thử lại riêng danh sách kết quả (giữ nguyên bộ lọc), sau lỗi tải trang đầu. */
    public void thuLaiDanhSach() {
        taiTrangDau(!daTai.isEmpty());
    }

    /** Thử lại riêng nguồn bộ lọc Loại. */
    public void thuLaiNguonLoai() {
        taiNguonLoai();
    }

    /** Thử lại riêng nguồn bộ lọc Phòng. */
    public void thuLaiNguonPhong() {
        taiNguonPhong();
    }

    private void taiTatCa() {
        taiNguonLoai();
        taiNguonPhong();
        taiTrangDau(true);
    }

    // ---------------------------------------------------------------- bộ lọc

    /**
     * Đổi tên tìm kiếm. {@code ngay} là {@code false} khi đang gõ (chờ độ trễ), {@code true} khi người dùng
     * bấm phím tìm của bàn phím.
     */
    public void datTuKhoa(@Nullable String tuKhoa, boolean ngay) {
        String moi = tuKhoa == null ? "" : tuKhoa.trim();
        huyHenGioDangCho();
        if (ngay) {
            tuKhoaDangCho = null;
            apDung(giaTriBoLoc().voiTen(moi));
            return;
        }
        if (moi.equals(giaTriBoLoc().getTen())) {
            tuKhoaDangCho = null;
            return;
        }
        tuKhoaDangCho = moi;
        huyHenGio =
                boHenGio.datLich(
                        () -> {
                            huyHenGio = null;
                            apDung(chotTuKhoaDangCho());
                        },
                        HangSo.DO_TRE_GO_TIM_KIEM_MS);
    }

    public void chonLoai(@Nullable LuaChonBoLoc loai) {
        apDung(chotTuKhoaDangCho().voiLoai(loai));
    }

    public void chonPhong(@Nullable LuaChonBoLoc phong) {
        apDung(chotTuKhoaDangCho().voiPhong(phong));
    }

    public void chonTinhTrang(@NonNull TinhTrangHang tinhTrang) {
        apDung(chotTuKhoaDangCho().voiTinhTrang(tinhTrang));
    }

    public void xoaLoai() {
        chonLoai(null);
    }

    public void xoaPhong() {
        chonPhong(null);
    }

    public void xoaTinhTrang() {
        chonTinhTrang(TinhTrangHang.TAT_CA);
    }

    /** Bỏ mọi bộ lọc, kể cả tên tìm kiếm (nút Xóa bộ lọc). */
    public void xoaBoLoc() {
        huyHenGioDangCho();
        tuKhoaDangCho = null;
        apDung(BoLoc.MAC_DINH);
    }

    @NonNull
    private BoLoc giaTriBoLoc() {
        BoLoc hienTai = boLoc.getValue();
        return hienTai == null ? BoLoc.MAC_DINH : hienTai;
    }

    /** Nhập tên đang chờ vào bộ lọc và hủy hẹn giờ, để một thay đổi bộ lọc chỉ gửi một yêu cầu. */
    @NonNull
    private BoLoc chotTuKhoaDangCho() {
        BoLoc hienTai = giaTriBoLoc();
        huyHenGioDangCho();
        String cho = tuKhoaDangCho;
        tuKhoaDangCho = null;
        return cho == null ? hienTai : hienTai.voiTen(cho);
    }

    private void huyHenGioDangCho() {
        if (huyHenGio != null) {
            huyHenGio.run();
            huyHenGio = null;
        }
    }

    private void apDung(@NonNull BoLoc moi) {
        if (moi.equals(giaTriBoLoc())) {
            return;
        }
        boLoc.setValue(moi);
        taiTrangDau(false);
    }

    // ---------------------------------------------------------------- danh sách kết quả

    private void taiTrangDau(boolean giuDongCu) {
        huy(callDanhSach);
        callDanhSach = null;
        final int lan = ++lanTai;
        if (!giuDongCu) {
            daTai.clear();
            trangDaTai = 0;
            tongSo = 0;
            hetTrang = false;
        }
        taiThem.setValue(TrangThaiTaiThem.KHONG);
        danhSach.setValue(
                UiState.<List<TraCuuThietBiItem>>dangTai(daTai.isEmpty() ? null : saoChepDong()));
        BoLoc loc = giaTriBoLoc();
        callDanhSach =
                traCuuRepository.svTraCuuThietBi(
                        loc.tenTimKiem(),
                        maCua(loc.getLoai()),
                        maCua(loc.getPhong()),
                        loc.getTinhTrang().getGiaTriConHang(),
                        1,
                        HangSo.KICH_THUOC_TRANG,
                        new ApiCallback<TraCuuThietBiItemPage>() {
                            @Override
                            public void onSuccess(@Nullable TraCuuThietBiItemPage trang) {
                                if (lan != lanTai) {
                                    return;
                                }
                                daTai.clear();
                                int soDong = themDong(trang);
                                trangDaTai = 1;
                                capNhatTong(trang, soDong);
                                if (daTai.isEmpty()) {
                                    danhSach.setValue(UiState.<List<TraCuuThietBiItem>>rong());
                                } else {
                                    danhSach.setValue(UiState.thanhCong(saoChepDong()));
                                }
                            }

                            @Override
                            public void onError(@NonNull ApiError loi) {
                                if (lan != lanTai || loi.getLoai() == ApiError.Loai.HUY) {
                                    return;
                                }
                                if (giuDongCu && !daTai.isEmpty()) {
                                    // Kéo làm mới thất bại nhưng bộ lọc không đổi: giữ dòng cũ và báo lỗi (FA 5.1).
                                    danhSach.setValue(
                                            UiState.<List<TraCuuThietBiItem>>loi(loi, saoChepDong()));
                                } else {
                                    // Đổi bộ lọc hoặc tải lần đầu bị lỗi: không còn dòng khớp bộ lọc mới.
                                    daTai.clear();
                                    trangDaTai = 0;
                                    tongSo = 0;
                                    hetTrang = false;
                                    danhSach.setValue(UiState.<List<TraCuuThietBiItem>>loi(loi, null));
                                }
                            }
                        });
    }

    /** Tải trang kế khi cuộn tới cuối, nếu đang có kết quả, chưa đủ {@code tongSo} và chưa đang tải thêm. */
    public void taiThem() {
        UiState<List<TraCuuThietBiItem>> hienTai = danhSach.getValue();
        if (hienTai == null || hienTai.getTrangThai() != UiState.TrangThai.THANH_CONG) {
            return;
        }
        if (hetTrang || taiThem.getValue() != TrangThaiTaiThem.KHONG) {
            return;
        }
        taiTrangKe();
    }

    /** Thử lại trang kế sau lỗi tải thêm; các dòng đã có được giữ nguyên. */
    public void thuLaiTaiThem() {
        if (taiThem.getValue() == TrangThaiTaiThem.LOI) {
            taiTrangKe();
        }
    }

    private void taiTrangKe() {
        final int lan = lanTai;
        final int trang = trangDaTai + 1;
        BoLoc loc = giaTriBoLoc();
        taiThem.setValue(TrangThaiTaiThem.DANG_TAI);
        callDanhSach =
                traCuuRepository.svTraCuuThietBi(
                        loc.tenTimKiem(),
                        maCua(loc.getLoai()),
                        maCua(loc.getPhong()),
                        loc.getTinhTrang().getGiaTriConHang(),
                        trang,
                        HangSo.KICH_THUOC_TRANG,
                        new ApiCallback<TraCuuThietBiItemPage>() {
                            @Override
                            public void onSuccess(@Nullable TraCuuThietBiItemPage ketQua) {
                                if (lan != lanTai) {
                                    return;
                                }
                                int soDong = themDong(ketQua);
                                trangDaTai = trang;
                                capNhatTong(ketQua, soDong);
                                danhSach.setValue(UiState.thanhCong(saoChepDong()));
                                taiThem.setValue(TrangThaiTaiThem.KHONG);
                            }

                            @Override
                            public void onError(@NonNull ApiError loi) {
                                if (lan != lanTai || loi.getLoai() == ApiError.Loai.HUY) {
                                    return;
                                }
                                taiThem.setValue(TrangThaiTaiThem.LOI);
                            }
                        });
    }

    /** Thêm các dòng của trang vào {@code daTai}; trả số dòng vừa thêm. */
    private int themDong(@Nullable TraCuuThietBiItemPage trang) {
        if (trang == null || trang.getDanhSach() == null) {
            return 0;
        }
        daTai.addAll(trang.getDanhSach());
        return trang.getDanhSach().size();
    }

    /** Dừng tải thêm khi đủ {@code tongSo} hoặc máy chủ trả trang rỗng (tránh lặp vô hạn). */
    private void capNhatTong(@Nullable TraCuuThietBiItemPage trang, int soDongVuaNhan) {
        Integer tong = trang == null ? null : trang.getTongSo();
        tongSo = tong == null ? daTai.size() : tong;
        hetTrang = soDongVuaNhan == 0 || daTai.size() >= tongSo;
    }

    @NonNull
    private List<TraCuuThietBiItem> saoChepDong() {
        return Collections.unmodifiableList(new ArrayList<>(daTai));
    }

    @Nullable
    private static UUID maCua(@Nullable LuaChonBoLoc luaChon) {
        return luaChon == null ? null : luaChon.getMa();
    }

    // ---------------------------------------------------------------- nguồn của bộ lọc (tải trọn mọi trang)

    private void taiNguonLoai() {
        huy(callLoai);
        callLoai = null;
        final int lan = ++lanLoai;
        dsLoai.setValue(UiState.<List<LuaChonBoLoc>>dangTai(null));
        taiTrangLoai(1, new ArrayList<LuaChonBoLoc>(), lan);
    }

    private void taiTrangLoai(
            final int trang, @NonNull final List<LuaChonBoLoc> tichLuy, final int lan) {
        callLoai =
                traCuuRepository.svDsLoaiThietBi(
                        trang,
                        HangSo.KICH_THUOC_TRANG_TOI_DA,
                        new ApiCallback<LoaiThietBiTomTatPage>() {
                            @Override
                            public void onSuccess(@Nullable LoaiThietBiTomTatPage ketQua) {
                                if (lan != lanLoai) {
                                    return;
                                }
                                List<LoaiThietBiTomTat> dong =
                                        ketQua == null || ketQua.getDanhSach() == null
                                                ? Collections.<LoaiThietBiTomTat>emptyList()
                                                : ketQua.getDanhSach();
                                for (LoaiThietBiTomTat loai : dong) {
                                    if (loai.getMaLoai() != null && loai.getTenLoai() != null) {
                                        tichLuy.add(
                                                LuaChonBoLoc.tao(
                                                        loai.getMaLoai(), loai.getTenLoai(), null));
                                    }
                                }
                                Integer tong = ketQua == null ? null : ketQua.getTongSo();
                                if (!dong.isEmpty() && tong != null && tichLuy.size() < tong) {
                                    taiTrangLoai(trang + 1, tichLuy, lan);
                                    return;
                                }
                                dsLoai.setValue(
                                        UiState.thanhCong(
                                                Collections.unmodifiableList(
                                                        new ArrayList<>(tichLuy))));
                            }

                            @Override
                            public void onError(@NonNull ApiError loi) {
                                if (lan != lanLoai || loi.getLoai() == ApiError.Loai.HUY) {
                                    return;
                                }
                                dsLoai.setValue(UiState.<List<LuaChonBoLoc>>loi(loi, null));
                            }
                        });
    }

    private void taiNguonPhong() {
        huy(callPhong);
        callPhong = null;
        final int lan = ++lanPhong;
        dsPhong.setValue(UiState.<List<LuaChonBoLoc>>dangTai(null));
        taiTrangPhong(1, new ArrayList<LuaChonBoLoc>(), lan);
    }

    private void taiTrangPhong(
            final int trang, @NonNull final List<LuaChonBoLoc> tichLuy, final int lan) {
        callPhong =
                traCuuRepository.svDsPhongThietBi(
                        trang,
                        HangSo.KICH_THUOC_TRANG_TOI_DA,
                        new ApiCallback<PhongThietBiPage>() {
                            @Override
                            public void onSuccess(@Nullable PhongThietBiPage ketQua) {
                                if (lan != lanPhong) {
                                    return;
                                }
                                List<PhongThietBi> dong =
                                        ketQua == null || ketQua.getDanhSach() == null
                                                ? Collections.<PhongThietBi>emptyList()
                                                : ketQua.getDanhSach();
                                for (PhongThietBi phong : dong) {
                                    if (phong.getMaPhong() != null && phong.getTenPhong() != null) {
                                        tichLuy.add(
                                                LuaChonBoLoc.tao(
                                                        phong.getMaPhong(),
                                                        phong.getTenPhong(),
                                                        phong.getCoSo()));
                                    }
                                }
                                Integer tong = ketQua == null ? null : ketQua.getTongSo();
                                if (!dong.isEmpty() && tong != null && tichLuy.size() < tong) {
                                    taiTrangPhong(trang + 1, tichLuy, lan);
                                    return;
                                }
                                dsPhong.setValue(
                                        UiState.thanhCong(
                                                Collections.unmodifiableList(
                                                        new ArrayList<>(tichLuy))));
                            }

                            @Override
                            public void onError(@NonNull ApiError loi) {
                                if (lan != lanPhong || loi.getLoai() == ApiError.Loai.HUY) {
                                    return;
                                }
                                dsPhong.setValue(UiState.<List<LuaChonBoLoc>>loi(loi, null));
                            }
                        });
    }

    // ---------------------------------------------------------------- vòng đời

    private static void huy(@Nullable Call<?> call) {
        if (call != null) {
            call.cancel();
        }
    }

    @Override
    protected void onCleared() {
        huy(callDanhSach);
        huy(callLoai);
        huy(callPhong);
        huyHenGioDangCho();
        super.onCleared();
    }
}
