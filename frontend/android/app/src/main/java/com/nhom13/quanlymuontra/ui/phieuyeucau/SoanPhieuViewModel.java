package com.nhom13.quanlymuontra.ui.phieuyeucau;

import androidx.annotation.NonNull;
import androidx.annotation.Nullable;
import androidx.lifecycle.LiveData;
import androidx.lifecycle.MutableLiveData;
import androidx.lifecycle.ViewModel;
import com.nhom13.quanlymuontra.common.HangSo;
import com.nhom13.quanlymuontra.data.model.ApiError;
import com.nhom13.quanlymuontra.data.model.Event;
import com.nhom13.quanlymuontra.data.model.UiState;
import com.nhom13.quanlymuontra.data.repository.ApiCallback;
import com.nhom13.quanlymuontra.data.repository.HoSoMuonRepository;
import com.nhom13.quanlymuontra.data.repository.PhieuYeuCauRepository;
import com.nhom13.quanlymuontra.data.repository.TraCuuRepository;
import com.nhom13.quanlymuontra.generated.model.PhieuYeuCauMuon;
import com.nhom13.quanlymuontra.generated.model.PhongThietBi;
import com.nhom13.quanlymuontra.generated.model.PhongThietBiPage;
import com.nhom13.quanlymuontra.generated.model.TraCuuThietBiItem;
import com.nhom13.quanlymuontra.generated.model.TraCuuThietBiItemPage;
import com.nhom13.quanlymuontra.ui.tracuu.ChiTietLoaiThietBiViewModel.HanhDong;
import com.nhom13.quanlymuontra.ui.tracuu.LuaChonBoLoc;
import com.nhom13.quanlymuontra.ui.tracuu.TraCuuThietBiViewModel.TrangThaiTaiThem;
import java.util.ArrayList;
import java.util.Collections;
import java.util.Iterator;
import java.util.LinkedHashMap;
import java.util.List;
import java.util.Map;
import java.util.UUID;
import retrofit2.Call;

/**
 * SCR-SV-04 Soạn phiếu yêu cầu mượn (UI-Screens 5.4, UF-SV-02 bước 1 đến 3, FA 5.3 quy tắc 2, FA 5.6; FC 6.2, 6.3).
 *
 * <p>{@code ViewModel} phạm vi nav graph con {@code soanPhieuGraph}: SCR-SV-05 (D11-L) dùng chung nên phòng, loại, số
 * lượng và thời gian nhập được giữ khi sang màn xem lại rồi quay về, và bị hủy khi rời nav graph con.
 *
 * <ul>
 *   <li>Một phiếu thuộc đúng một phòng. Đổi phòng khi đã chọn loại thì phát yêu cầu xác nhận (ĐX-UI-06) và chỉ đổi sau
 *       {@link #xacNhanDoiPhong()}; đổi xong xóa toàn bộ lựa chọn.
 *   <li>Số lượng của một loại nằm trong [0, {@code soLuongKhaDung} của dòng]; 0 là không đưa loại vào phiếu. Số khả
 *       dụng do máy chủ trả; lớp này chỉ kẹp theo số đang hiển thị, không tính giới hạn nghiệp vụ (giới hạn số thiết
 *       bị, thời gian tối đa do máy chủ kiểm khi gửi).
 *   <li>Mở lại màn hình tải lại phòng và các trang loại đã có để lấy số khả dụng mới; lựa chọn vượt số mới bị đưa về
 *       mức tối đa còn lại, loại đã hết hàng bị bỏ khỏi phiếu (UI-Screens 5.4 "Trạng thái đặc thù").
 *   <li>Danh sách loại tải thêm theo {@code tongSo}; lỗi tải thêm giữ các dòng cũ. Thứ tự dòng do máy chủ quyết định.
 *   <li>Nút Xem lại chỉ bật khi {@link #layNoiDungPhieu()} khác {@code null}: đã chọn phòng, có ít nhất một loại số
 *       lượng từ 1 và thời gian là số nguyên từ 1.
 *   <li>Liên kết Đăng ký chờ chỉ chạy cổng quyền mượn ({@link CongQuyenMuon}); gửi đăng ký chờ thuộc D13-L.
 *   <li>D11-L: {@link #gui()} gửi phiếu một lần (khóa đồng bộ, không gửi đôi). {@link #onCleared()} không hủy lời gọi
 *       gửi (FA 3.11). Mất kết nối, quá thời gian chờ, hủy hoặc phản hồi không đọc được là "chưa xác định kết quả":
 *       không tự gửi lại và khóa gửi tới hết vòng đời {@code ViewModel}; SCR-SV-06 (D12-L) kiểm tra và mở khóa.
 * </ul>
 */
public final class SoanPhieuViewModel extends ViewModel {

    /** Số chữ số tối đa của ô thời gian, chỉ để tránh tràn {@code int}; không phải giới hạn nghiệp vụ. */
    private static final int SO_CHU_SO_TOI_DA = 9;

    /** Trạng thái lần gửi phiếu. */
    public enum TrangThaiGui {
        SAN_SANG,
        DANG_GUI,
        /** Máy chủ đã tạo phiếu; nháp đã xóa. */
        DA_GUI,
        /** Không cho gửi (thẻ hết hạn, không còn theo học, quyền mượn bị khóa). */
        CHAN_GUI,
        /** Không biết máy chủ đã tạo phiếu hay chưa; không cho gửi lại. */
        CHUA_XAC_DINH
    }

    /** Hướng điều hướng sau lỗi gửi, do SCR-SV-05 xử lý. */
    public enum DieuHuongGui {
        QUAY_VE_SUA,
        MO_HO_SO_BI_CHAN
    }

    private final TraCuuRepository traCuuRepository;
    private final CongQuyenMuon cong;
    private final PhieuYeuCauRepository phieuYeuCauRepository;

    private final MutableLiveData<UiState<List<LuaChonBoLoc>>> dsPhong =
            new MutableLiveData<>(UiState.<List<LuaChonBoLoc>>dangTai(null));
    private final MutableLiveData<LuaChonBoLoc> phongDaChon = new MutableLiveData<>();
    private final MutableLiveData<UiState<List<TraCuuThietBiItem>>> dsLoai =
            new MutableLiveData<>(UiState.<List<TraCuuThietBiItem>>rong());
    private final MutableLiveData<TrangThaiTaiThem> taiThem =
            new MutableLiveData<>(TrangThaiTaiThem.KHONG);
    private final MutableLiveData<Map<UUID, Integer>> soLuongDaChon =
            new MutableLiveData<>(Collections.<UUID, Integer>emptyMap());
    private final MutableLiveData<Boolean> xemLaiBat = new MutableLiveData<>(false);
    private final MutableLiveData<Event<LuaChonBoLoc>> yeuCauXacNhanDoiPhong = new MutableLiveData<>();
    private final MutableLiveData<TrangThaiGui> trangThaiGui =
            new MutableLiveData<>(TrangThaiGui.SAN_SANG);
    private final MutableLiveData<LoiGuiPhieu> loiGui = new MutableLiveData<>();
    private final MutableLiveData<String> maYeuCauVuaGui = new MutableLiveData<>();
    private final MutableLiveData<Event<DieuHuongGui>> dieuHuongGui = new MutableLiveData<>();
    private final MutableLiveData<Event<Boolean>> yeuCauKiemTraDanhSach = new MutableLiveData<>();

    private final List<TraCuuThietBiItem> daTai = new ArrayList<>();
    private final Map<UUID, Integer> daChon = new LinkedHashMap<>();
    private String thoiGianNhap = "";
    private List<UUID> thuTuLoaiDaGui = Collections.emptyList();

    @Nullable private UUID maPhongKhoiTao;
    @Nullable private UUID maLoaiKhoiTao;
    private boolean daKhoiTao = false;
    @Nullable private LuaChonBoLoc phongCho;

    private boolean canTaiLai = true;
    private int lanPhong = 0;
    @Nullable private Call<PhongThietBiPage> callPhong;
    private int lanLoai = 0;
    private int trangDaTai = 0;
    private int tongSo = 0;
    private boolean hetTrang = false;
    @Nullable private Call<TraCuuThietBiItemPage> callLoai;

    public SoanPhieuViewModel(
            @NonNull TraCuuRepository traCuuRepository,
            @NonNull HoSoMuonRepository hoSoMuonRepository,
            @NonNull PhieuYeuCauRepository phieuYeuCauRepository) {
        this.traCuuRepository = traCuuRepository;
        this.cong = new CongQuyenMuon(hoSoMuonRepository);
        this.phieuYeuCauRepository = phieuYeuCauRepository;
    }

    // ---------------------------------------------------------------- trạng thái phơi bày

    /** Nguồn chọn phòng (tải trọn mọi trang). */
    @NonNull
    public LiveData<UiState<List<LuaChonBoLoc>>> getDsPhong() {
        return dsPhong;
    }

    /** Phòng đang chọn; {@code null} khi chưa chọn. */
    @NonNull
    public LiveData<LuaChonBoLoc> getPhongDaChon() {
        return phongDaChon;
    }

    /**
     * Các loại thiết bị của phòng đang chọn (mọi trang đã tải). {@code RONG} là phòng chưa có thiết bị để đặt; khi chưa
     * chọn phòng giá trị cũng là {@code RONG} nên màn hình phải kiểm {@link #getPhongDaChon()} trước.
     */
    @NonNull
    public LiveData<UiState<List<TraCuuThietBiItem>>> getDsLoai() {
        return dsLoai;
    }

    @NonNull
    public LiveData<TrangThaiTaiThem> getTaiThem() {
        return taiThem;
    }

    /** Số lượng đã chọn theo mã loại (chỉ gồm loại có số lượng từ 1). */
    @NonNull
    public LiveData<Map<UUID, Integer>> getSoLuongDaChon() {
        return soLuongDaChon;
    }

    /** {@code true} khi nút Xem lại được bật. */
    @NonNull
    public LiveData<Boolean> getXemLaiBat() {
        return xemLaiBat;
    }

    /** Yêu cầu hiển thị hộp xác nhận đổi phòng; nội dung là phòng muốn đổi sang. */
    @NonNull
    public LiveData<Event<LuaChonBoLoc>> getYeuCauXacNhanDoiPhong() {
        return yeuCauXacNhanDoiPhong;
    }

    @NonNull
    public LiveData<Boolean> getDangKiemTraQuyen() {
        return cong.getDangKiemTra();
    }

    @NonNull
    public LiveData<ApiError> getLoiKiemTraQuyen() {
        return cong.getLoi();
    }

    @NonNull
    public LiveData<Event<CongQuyenMuon.KetQua>> getKetQuaCong() {
        return cong.getKetQua();
    }

    @NonNull
    public LiveData<TrangThaiGui> getTrangThaiGui() {
        return trangThaiGui;
    }

    /** Lỗi của lần gửi gần nhất; {@code null} khi không có. SCR-SV-04 hiển thị nhóm "quay về sửa", SCR-SV-05 hiển thị nhóm còn lại. */
    @NonNull
    public LiveData<LoiGuiPhieu> getLoiGui() {
        return loiGui;
    }

    /** Mã phiếu vừa tạo khi {@link TrangThaiGui#DA_GUI}. */
    @NonNull
    public LiveData<String> getMaYeuCauVuaGui() {
        return maYeuCauVuaGui;
    }

    @NonNull
    public LiveData<Event<DieuHuongGui>> getDieuHuongGui() {
        return dieuHuongGui;
    }

    /** Yêu cầu kiểm tra danh sách phiếu (SCR-SV-06, D12-L nối đích) khi chưa xác định kết quả. */
    @NonNull
    public LiveData<Event<Boolean>> getYeuCauKiemTraDanhSach() {
        return yeuCauKiemTraDanhSach;
    }

    /** Chữ đang nhập ở ô thời gian mượn (để dựng lại ô sau xoay màn hình hoặc khi quay lại từ SCR-SV-05). */
    @NonNull
    public String getThoiGianNhap() {
        return thoiGianNhap;
    }

    // ---------------------------------------------------------------- khởi tạo và tải

    /**
     * Nhận tham số Safe Args tùy chọn. Chỉ lần gọi đầu có tác dụng (graph giữ {@code ViewModel} qua nhiều lần mở
     * Fragment). {@code maLoai} không có {@code maPhong} bị bỏ qua (CHECKLIST mục 15, X4).
     */
    public void khoiTao(@Nullable UUID maPhong, @Nullable UUID maLoai) {
        if (daKhoiTao) {
            return;
        }
        daKhoiTao = true;
        maPhongKhoiTao = maPhong;
        maLoaiKhoiTao = maPhong == null ? null : maLoai;
    }

    /** Tải nếu đang cần tải lại (mở màn hình hoặc quay lại từ ngăn xếp); xoay màn hình không tải lại (FA 5.6). */
    public void load() {
        if (!canTaiLai) {
            return;
        }
        canTaiLai = false;
        taiTatCa();
    }

    /** Tải vô điều kiện: kéo để làm mới. */
    public void refresh() {
        canTaiLai = false;
        taiTatCa();
    }

    /** Fragment gọi ở {@code onDestroyView} khi không phải xoay màn hình. */
    public void danhDauCanTaiLai() {
        canTaiLai = true;
    }

    /** Thử lại riêng nguồn chọn phòng. */
    public void thuLaiPhong() {
        taiPhong();
    }

    /** Thử lại riêng danh sách loại của phòng đang chọn. */
    public void thuLaiLoai() {
        if (phongDaChon.getValue() == null) {
            return;
        }
        if (daTai.isEmpty()) {
            taiTrangDauLoai();
        } else {
            taiLaiLoai();
        }
    }

    private void taiTatCa() {
        // Chốt "đã có phòng" TRƯỚC khi tải phòng: lần mở đầu tiên phòng được chọn sẵn sau khi nguồn phòng về và tự tải loại.
        boolean daCoPhong = phongDaChon.getValue() != null;
        taiPhong();
        if (daCoPhong) {
            thuLaiLoai();
        }
    }

    // ---------------------------------------------------------------- chọn phòng

    /**
     * Chọn phòng. Đã chọn loại thì không đổi ngay mà phát yêu cầu xác nhận (ĐX-UI-06); chưa chọn loại nào thì đổi và
     * tải loại của phòng mới.
     */
    public void chonPhong(@NonNull LuaChonBoLoc phong) {
        if (phong.equals(phongDaChon.getValue())) {
            return;
        }
        if (!daChon.isEmpty()) {
            phongCho = phong;
            yeuCauXacNhanDoiPhong.setValue(new Event<>(phong));
            return;
        }
        apDungPhong(phong);
    }

    /** Sinh viên xác nhận đổi phòng: xóa mọi lựa chọn và tải loại của phòng mới. */
    public void xacNhanDoiPhong() {
        LuaChonBoLoc phong = phongCho;
        phongCho = null;
        if (phong != null) {
            apDungPhong(phong);
        }
    }

    /** Sinh viên hủy đổi phòng: giữ nguyên phòng và lựa chọn. */
    public void huyDoiPhong() {
        phongCho = null;
    }

    private void apDungPhong(@NonNull LuaChonBoLoc phong) {
        daChon.clear();
        xoaLoiGuiKhiSuaNhap();
        phongDaChon.setValue(phong);
        capNhatSoLuong();
        capNhatXemLai();
        taiTrangDauLoai();
    }

    // ---------------------------------------------------------------- số lượng và thời gian

    /** Đặt số lượng của một loại, kẹp vào [0, khả dụng]; 0 gỡ loại khỏi phiếu. Loại không có trong danh sách bị bỏ qua. */
    public void datSoLuong(@NonNull UUID maLoai, int soLuong) {
        TraCuuThietBiItem dong = timDong(maLoai);
        if (dong == null) {
            return;
        }
        int khaDung = khaDungCua(dong);
        int moi = Math.max(0, Math.min(khaDung, soLuong));
        if (moi == 0) {
            daChon.remove(maLoai);
        } else {
            daChon.put(maLoai, moi);
        }
        xoaLoiGuiKhiSuaNhap();
        capNhatSoLuong();
        capNhatXemLai();
    }

    /** Ghi chữ đang nhập ở ô thời gian mượn. */
    public void datThoiGian(@Nullable String nhap) {
        thoiGianNhap = nhap == null ? "" : nhap;
        xoaLoiGuiKhiSuaNhap();
        capNhatXemLai();
    }

    /**
     * Số giờ nguyên từ 1 (UI-Screens 5.4: {@code thoiGianMuonDuKienGio} min 1), hoặc {@code null} khi chưa nhập hoặc
     * không hợp lệ. Chỉ chữ số ASCII, tối đa {@value #SO_CHU_SO_TOI_DA} chữ số. Không kiểm mức tối đa của nhóm: do máy
     * chủ kiểm khi gửi.
     */
    @Nullable
    public static Integer phanTichThoiGian(@Nullable String nhap) {
        if (nhap == null) {
            return null;
        }
        String chuoi = nhap.trim();
        if (chuoi.isEmpty() || chuoi.length() > SO_CHU_SO_TOI_DA) {
            return null;
        }
        for (int i = 0; i < chuoi.length(); i++) {
            char c = chuoi.charAt(i);
            if (c < '0' || c > '9') {
                return null;
            }
        }
        int gio = Integer.parseInt(chuoi);
        return gio >= 1 ? gio : null;
    }

    /**
     * Nội dung phiếu đã soạn, hoặc {@code null} khi chưa đủ điều kiện (chưa chọn phòng, chưa có loại nào số lượng từ
     * 1, hoặc thời gian không hợp lệ).
     */
    @Nullable
    public NoiDungPhieu layNoiDungPhieu() {
        LuaChonBoLoc phong = phongDaChon.getValue();
        Integer gio = phanTichThoiGian(thoiGianNhap);
        if (phong == null || gio == null || daChon.isEmpty()) {
            return null;
        }
        List<NoiDungPhieu.DongLoai> ds = new ArrayList<>();
        for (Map.Entry<UUID, Integer> muc : daChon.entrySet()) {
            ds.add(new NoiDungPhieu.DongLoai(muc.getKey(), muc.getValue()));
        }
        return new NoiDungPhieu(phong.getMa(), gio, ds);
    }

    private void capNhatSoLuong() {
        soLuongDaChon.setValue(Collections.unmodifiableMap(new LinkedHashMap<>(daChon)));
    }

    private void capNhatXemLai() {
        xemLaiBat.setValue(
                layNoiDungPhieu() != null && trangThaiGui.getValue() != TrangThaiGui.DANG_GUI);
    }

    // ---------------------------------------------------------------- gửi phiếu (D11-L)

    /**
     * Tóm tắt để SCR-SV-05 hiển thị, hoặc {@code null} khi chưa đủ điều kiện (xem {@link #layNoiDungPhieu()}) hoặc
     * nháp đã xóa sau khi gửi thành công.
     */
    @Nullable
    public TomTatPhieu layTomTat() {
        NoiDungPhieu noiDung = layNoiDungPhieu();
        LuaChonBoLoc phong = phongDaChon.getValue();
        if (noiDung == null || phong == null) {
            return null;
        }
        List<TomTatPhieu.Dong> cacDong = new ArrayList<>();
        for (NoiDungPhieu.DongLoai dong : noiDung.getDsLoaiVaSoLuong()) {
            TraCuuThietBiItem loai = timDong(dong.getMaLoai());
            String ten =
                    loai == null || loai.getTenLoai() == null
                            ? HangSo.GIA_TRI_THIEU
                            : loai.getTenLoai();
            cacDong.add(new TomTatPhieu.Dong(ten, dong.getSoLuong()));
        }
        return new TomTatPhieu(
                phong.getTen(), phong.getPhuDe(), noiDung.getThoiGianMuonDuKienGio(), cacDong);
    }

    /** SCR-SV-04 chuyển sang SCR-SV-05: bỏ lỗi cũ (trừ khi đang chặn gửi hoặc chưa xác định) và sự kiện điều hướng tồn đọng. */
    public void chuanBiXemLai() {
        Event<DieuHuongGui> tonDong = dieuHuongGui.getValue();
        if (tonDong != null) {
            tonDong.getContentIfNotHandled();
        }
        TrangThaiGui hienTai = trangThaiGui.getValue();
        if (hienTai == TrangThaiGui.SAN_SANG || hienTai == TrangThaiGui.DA_GUI) {
            loiGui.setValue(null);
            maYeuCauVuaGui.setValue(null);
            datTrangThaiGui(TrangThaiGui.SAN_SANG);
        }
    }

    /**
     * Gửi phiếu. Khóa ngay (đồng bộ) trước khi tạo lời gọi nên bấm kép chỉ phát một yêu cầu. Trả {@code true} khi đã
     * phát yêu cầu, {@code false} khi bị từ chối (đang gửi, đã gửi, bị chặn, chưa xác định kết quả, hoặc phiếu chưa
     * hợp lệ).
     */
    public boolean gui() {
        if (trangThaiGui.getValue() != TrangThaiGui.SAN_SANG) {
            return false;
        }
        NoiDungPhieu noiDung = layNoiDungPhieu();
        if (noiDung == null) {
            return false;
        }
        List<UUID> thuTu = new ArrayList<>();
        for (NoiDungPhieu.DongLoai dong : noiDung.getDsLoaiVaSoLuong()) {
            thuTu.add(dong.getMaLoai());
        }
        thuTuLoaiDaGui = Collections.unmodifiableList(thuTu);
        loiGui.setValue(null);
        datTrangThaiGui(TrangThaiGui.DANG_GUI);
        phieuYeuCauRepository.svGuiPhieuYeuCauMuon(
                noiDung.taoYeuCau(),
                new ApiCallback<PhieuYeuCauMuon>() {
                    @Override
                    public void onSuccess(@Nullable PhieuYeuCauMuon phieu) {
                        khiGuiThanhCong(phieu);
                    }

                    @Override
                    public void onError(@NonNull ApiError loi) {
                        khiGuiLoi(loi);
                    }
                });
        return true;
    }

    private void khiGuiThanhCong(@Nullable PhieuYeuCauMuon phieu) {
        String ma = phieu == null ? null : phieu.getMaYeuCau();
        if (ma == null || ma.isEmpty()) {
            // 2xx nhưng không có mã phiếu: máy chủ đã xử lý nhưng không biết phiếu nào.
            datChuaXacDinh(null);
            return;
        }
        daChon.clear();
        thoiGianNhap = "";
        capNhatSoLuong();
        maYeuCauVuaGui.setValue(ma);
        datTrangThaiGui(TrangThaiGui.DA_GUI);
    }

    private void khiGuiLoi(@NonNull ApiError loi) {
        TrinhBayLoiGuiPhieu.PhanLoai phanLoai = TrinhBayLoiGuiPhieu.phanLoai(loi);
        if (phanLoai == TrinhBayLoiGuiPhieu.PhanLoai.CHUA_XAC_DINH) {
            datChuaXacDinh(loi);
            return;
        }
        LoiTruongPhieu truong =
                phanLoai == TrinhBayLoiGuiPhieu.PhanLoai.DU_LIEU_KHONG_HOP_LE
                        ? LoiTruongPhieu.phanTich(loi.getErrors(), thuTuLoaiDaGui)
                        : LoiTruongPhieu.rong();
        loiGui.setValue(new LoiGuiPhieu(phanLoai, loi, truong));
        switch (phanLoai) {
            case THE_HET_HAN:
            case KHONG_CON_THEO_HOC:
                datTrangThaiGui(TrangThaiGui.CHAN_GUI);
                break;
            case BI_KHOA_QUYEN_MUON:
                datTrangThaiGui(TrangThaiGui.CHAN_GUI);
                dieuHuongGui.setValue(new Event<>(DieuHuongGui.MO_HO_SO_BI_CHAN));
                break;
            case DU_LIEU_KHONG_HOP_LE:
            case VUOT_GIOI_HAN_THIET_BI:
            case VUOT_THOI_GIAN_MUON_TOI_DA:
            case SO_LUONG_THAY_DOI:
                datTrangThaiGui(TrangThaiGui.SAN_SANG);
                dieuHuongGui.setValue(new Event<>(DieuHuongGui.QUAY_VE_SUA));
                break;
            default:
                datTrangThaiGui(TrangThaiGui.SAN_SANG);
                break;
        }
    }

    private void datChuaXacDinh(@Nullable ApiError loi) {
        loiGui.setValue(
                new LoiGuiPhieu(
                        TrinhBayLoiGuiPhieu.PhanLoai.CHUA_XAC_DINH, loi, LoiTruongPhieu.rong()));
        datTrangThaiGui(TrangThaiGui.CHUA_XAC_DINH);
        yeuCauKiemTraDanhSach.setValue(new Event<>(Boolean.TRUE));
    }

    private void datTrangThaiGui(@NonNull TrangThaiGui moi) {
        trangThaiGui.setValue(moi);
        capNhatXemLai();
    }

    /** Sinh viên sửa nhập thì bỏ lỗi cũ, trừ khi đang gửi, đang chặn hoặc chưa xác định kết quả. */
    private void xoaLoiGuiKhiSuaNhap() {
        TrangThaiGui hienTai = trangThaiGui.getValue();
        boolean choPhep = hienTai == TrangThaiGui.SAN_SANG || hienTai == TrangThaiGui.DA_GUI;
        if (choPhep && loiGui.getValue() != null) {
            loiGui.setValue(null);
        }
    }

    // ---------------------------------------------------------------- Đăng ký chờ (chỉ cổng)

    /**
     * Sinh viên chọn liên kết Đăng ký chờ ở một dòng hết hàng: chạy cổng quyền mượn. Khi được phép chưa làm gì thêm
     * (gửi đăng ký chờ và DLG-SV-01 thuộc D13-L); các lựa chọn của phiếu không bị đụng tới.
     */
    public void chonDangKyCho() {
        cong.kiemTra(HanhDong.DANG_KY_CHO);
    }

    // ---------------------------------------------------------------- nguồn chọn phòng (tải trọn mọi trang)

    private void taiPhong() {
        huy(callPhong);
        callPhong = null;
        final int lan = ++lanPhong;
        UiState<List<LuaChonBoLoc>> hienTai = dsPhong.getValue();
        dsPhong.setValue(
                UiState.<List<LuaChonBoLoc>>dangTai(hienTai == null ? null : hienTai.getDuLieu()));
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
                                List<LuaChonBoLoc> ds =
                                        Collections.unmodifiableList(new ArrayList<>(tichLuy));
                                dsPhong.setValue(UiState.thanhCong(ds));
                                apDungPhongKhoiTao(ds);
                            }

                            @Override
                            public void onError(@NonNull ApiError loi) {
                                if (lan != lanPhong || loi.getLoai() == ApiError.Loai.HUY) {
                                    return;
                                }
                                UiState<List<LuaChonBoLoc>> hienTai = dsPhong.getValue();
                                dsPhong.setValue(
                                        UiState.<List<LuaChonBoLoc>>loi(
                                                loi, hienTai == null ? null : hienTai.getDuLieu()));
                            }
                        });
    }

    /** Chọn sẵn phòng truyền qua Safe Args (một lần); không có trong nguồn thì bỏ qua và để sinh viên chọn. */
    private void apDungPhongKhoiTao(@NonNull List<LuaChonBoLoc> ds) {
        UUID ma = maPhongKhoiTao;
        maPhongKhoiTao = null;
        if (ma == null || phongDaChon.getValue() != null) {
            return;
        }
        for (LuaChonBoLoc phong : ds) {
            if (ma.equals(phong.getMa())) {
                apDungPhong(phong);
                return;
            }
        }
        maLoaiKhoiTao = null;
    }

    // ---------------------------------------------------------------- danh sách loại của phòng

    private void taiTrangDauLoai() {
        huy(callLoai);
        callLoai = null;
        final int lan = ++lanLoai;
        daTai.clear();
        trangDaTai = 0;
        tongSo = 0;
        hetTrang = false;
        taiThem.setValue(TrangThaiTaiThem.KHONG);
        LuaChonBoLoc phong = phongDaChon.getValue();
        if (phong == null) {
            dsLoai.setValue(UiState.<List<TraCuuThietBiItem>>rong());
            return;
        }
        dsLoai.setValue(UiState.<List<TraCuuThietBiItem>>dangTai(null));
        callLoai =
                traCuuRepository.svTraCuuThietBi(
                        null,
                        null,
                        phong.getMa(),
                        null,
                        1,
                        HangSo.KICH_THUOC_TRANG,
                        new ApiCallback<TraCuuThietBiItemPage>() {
                            @Override
                            public void onSuccess(@Nullable TraCuuThietBiItemPage trang) {
                                if (lan != lanLoai) {
                                    return;
                                }
                                int soDong = themDong(trang);
                                trangDaTai = 1;
                                capNhatTong(trang, soDong);
                                phatDanhSach();
                                apDungLoaiKhoiTao();
                                dongBoSoLuong();
                            }

                            @Override
                            public void onError(@NonNull ApiError loi) {
                                if (lan != lanLoai || loi.getLoai() == ApiError.Loai.HUY) {
                                    return;
                                }
                                dsLoai.setValue(UiState.<List<TraCuuThietBiItem>>loi(loi, null));
                            }
                        });
    }

    /** Tải lại các trang đã có (giữ dòng cũ cho tới khi có kết quả mới) để cập nhật số khả dụng. */
    private void taiLaiLoai() {
        LuaChonBoLoc phong = phongDaChon.getValue();
        if (phong == null) {
            return;
        }
        huy(callLoai);
        callLoai = null;
        final int lan = ++lanLoai;
        taiThem.setValue(TrangThaiTaiThem.KHONG);
        dsLoai.setValue(
                UiState.<List<TraCuuThietBiItem>>dangTai(daTai.isEmpty() ? null : saoChepDong()));
        taiLaiTrang(phong, 1, Math.max(1, trangDaTai), new ArrayList<TraCuuThietBiItem>(), lan);
    }

    private void taiLaiTrang(
            @NonNull final LuaChonBoLoc phong,
            final int trang,
            final int dich,
            @NonNull final List<TraCuuThietBiItem> tichLuy,
            final int lan) {
        callLoai =
                traCuuRepository.svTraCuuThietBi(
                        null,
                        null,
                        phong.getMa(),
                        null,
                        trang,
                        HangSo.KICH_THUOC_TRANG,
                        new ApiCallback<TraCuuThietBiItemPage>() {
                            @Override
                            public void onSuccess(@Nullable TraCuuThietBiItemPage ketQua) {
                                if (lan != lanLoai) {
                                    return;
                                }
                                int soDong =
                                        ketQua == null || ketQua.getDanhSach() == null
                                                ? 0
                                                : ketQua.getDanhSach().size();
                                if (soDong > 0) {
                                    tichLuy.addAll(ketQua.getDanhSach());
                                }
                                Integer tong = ketQua == null ? null : ketQua.getTongSo();
                                int tongMoi = tong == null ? tichLuy.size() : tong;
                                boolean het = soDong == 0 || tichLuy.size() >= tongMoi;
                                if (trang < dich && !het) {
                                    taiLaiTrang(phong, trang + 1, dich, tichLuy, lan);
                                    return;
                                }
                                daTai.clear();
                                daTai.addAll(tichLuy);
                                trangDaTai = trang;
                                tongSo = tongMoi;
                                hetTrang = het;
                                phatDanhSach();
                                apDungLoaiKhoiTao();
                                dongBoSoLuong();
                            }

                            @Override
                            public void onError(@NonNull ApiError loi) {
                                if (lan != lanLoai || loi.getLoai() == ApiError.Loai.HUY) {
                                    return;
                                }
                                dsLoai.setValue(
                                        UiState.<List<TraCuuThietBiItem>>loi(
                                                loi, daTai.isEmpty() ? null : saoChepDong()));
                            }
                        });
    }

    /** Tải trang kế khi cuộn tới cuối, nếu đang có kết quả, chưa đủ {@code tongSo} và chưa đang tải thêm. */
    public void taiThemLoai() {
        UiState<List<TraCuuThietBiItem>> hienTai = dsLoai.getValue();
        if (hienTai == null
                || hienTai.getTrangThai() != UiState.TrangThai.THANH_CONG
                || phongDaChon.getValue() == null) {
            return;
        }
        if (hetTrang || taiThem.getValue() != TrangThaiTaiThem.KHONG) {
            return;
        }
        taiTrangKe();
    }

    /** Thử lại trang kế sau lỗi tải thêm; các dòng đã có được giữ nguyên. */
    public void thuLaiTaiThem() {
        if (taiThem.getValue() == TrangThaiTaiThem.LOI && phongDaChon.getValue() != null) {
            taiTrangKe();
        }
    }

    private void taiTrangKe() {
        LuaChonBoLoc phong = phongDaChon.getValue();
        if (phong == null) {
            return;
        }
        final int lan = lanLoai;
        final int trang = trangDaTai + 1;
        taiThem.setValue(TrangThaiTaiThem.DANG_TAI);
        callLoai =
                traCuuRepository.svTraCuuThietBi(
                        null,
                        null,
                        phong.getMa(),
                        null,
                        trang,
                        HangSo.KICH_THUOC_TRANG,
                        new ApiCallback<TraCuuThietBiItemPage>() {
                            @Override
                            public void onSuccess(@Nullable TraCuuThietBiItemPage ketQua) {
                                if (lan != lanLoai) {
                                    return;
                                }
                                int soDong = themDong(ketQua);
                                trangDaTai = trang;
                                capNhatTong(ketQua, soDong);
                                phatDanhSach();
                                taiThem.setValue(TrangThaiTaiThem.KHONG);
                                apDungLoaiKhoiTao();
                                dongBoSoLuong();
                            }

                            @Override
                            public void onError(@NonNull ApiError loi) {
                                if (lan != lanLoai || loi.getLoai() == ApiError.Loai.HUY) {
                                    return;
                                }
                                taiThem.setValue(TrangThaiTaiThem.LOI);
                            }
                        });
    }

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

    private void phatDanhSach() {
        if (daTai.isEmpty()) {
            dsLoai.setValue(UiState.<List<TraCuuThietBiItem>>rong());
        } else {
            dsLoai.setValue(UiState.thanhCong(saoChepDong()));
        }
    }

    @NonNull
    private List<TraCuuThietBiItem> saoChepDong() {
        return Collections.unmodifiableList(new ArrayList<>(daTai));
    }

    // ---------------------------------------------------------------- đồng bộ lựa chọn với số khả dụng mới

    /**
     * Chọn sẵn loại truyền qua Safe Args với số lượng 1 (FA 5.3): khi dòng của loại đã tải và còn hàng. Giữ chờ tới khi
     * tìm thấy hoặc hết trang (loại ngoài các trang đã tải được chọn sẵn khi người dùng cuộn tải thêm tới nó).
     */
    private void apDungLoaiKhoiTao() {
        UUID ma = maLoaiKhoiTao;
        if (ma == null) {
            return;
        }
        TraCuuThietBiItem dong = timDong(ma);
        if (dong != null) {
            if (khaDungCua(dong) >= 1 && !daChon.containsKey(ma)) {
                daChon.put(ma, 1);
            }
            maLoaiKhoiTao = null;
        } else if (hetTrang) {
            maLoaiKhoiTao = null;
        }
    }

    /**
     * Đưa lựa chọn về khớp số khả dụng vừa tải (UI-Screens 5.4): vượt số mới thì về mức tối đa còn lại, loại đã hết
     * hàng thì bị bỏ khỏi phiếu. Loại chưa nằm trong các dòng đã tải thì giữ nguyên.
     */
    private void dongBoSoLuong() {
        Iterator<Map.Entry<UUID, Integer>> it = daChon.entrySet().iterator();
        while (it.hasNext()) {
            Map.Entry<UUID, Integer> muc = it.next();
            TraCuuThietBiItem dong = timDong(muc.getKey());
            if (dong == null) {
                continue;
            }
            int khaDung = khaDungCua(dong);
            if (khaDung <= 0) {
                it.remove();
            } else if (muc.getValue() > khaDung) {
                muc.setValue(khaDung);
            }
        }
        capNhatSoLuong();
        capNhatXemLai();
    }

    @Nullable
    private TraCuuThietBiItem timDong(@NonNull UUID maLoai) {
        for (TraCuuThietBiItem dong : daTai) {
            if (maLoai.equals(dong.getMaLoai())) {
                return dong;
            }
        }
        return null;
    }

    private static int khaDungCua(@NonNull TraCuuThietBiItem dong) {
        Integer khaDung = dong.getSoLuongKhaDung();
        return khaDung == null ? 0 : khaDung;
    }

    // ---------------------------------------------------------------- vòng đời

    private static void huy(@Nullable Call<?> call) {
        if (call != null) {
            call.cancel();
        }
    }

    @Override
    protected void onCleared() {
        huy(callPhong);
        huy(callLoai);
        cong.huy();
        // Cố ý không hủy lời gọi gửi phiếu: thao tác ghi đã phát không bị hủy im lặng khi rời màn hình (FA 3.11).
        super.onCleared();
    }
}
