package com.nhom13.quanlymuontra.ui.phieuyeucau;

import android.os.Bundle;
import android.text.Editable;
import android.text.TextWatcher;
import android.view.LayoutInflater;
import android.view.View;
import android.view.ViewGroup;
import androidx.annotation.NonNull;
import androidx.annotation.Nullable;
import androidx.core.content.ContextCompat;
import androidx.fragment.app.DialogFragment;
import androidx.fragment.app.Fragment;
import androidx.lifecycle.ViewModelProvider;
import androidx.lifecycle.ViewModelStoreOwner;
import androidx.navigation.NavController;
import androidx.navigation.NavDestination;
import androidx.navigation.fragment.NavHostFragment;
import androidx.recyclerview.widget.LinearLayoutManager;
import androidx.recyclerview.widget.RecyclerView;
import com.nhom13.quanlymuontra.AppViewModelFactory;
import com.nhom13.quanlymuontra.R;
import com.nhom13.quanlymuontra.UngDung;
import com.nhom13.quanlymuontra.common.nhan.Tong;
import com.nhom13.quanlymuontra.common.widget.ChonMotDialogFragment;
import com.nhom13.quanlymuontra.common.widget.XacNhanDialogFragment;
import com.nhom13.quanlymuontra.data.model.ApiError;
import com.nhom13.quanlymuontra.data.model.Event;
import com.nhom13.quanlymuontra.data.model.UiState;
import com.nhom13.quanlymuontra.databinding.FragmentSoanPhieuYeuCauBinding;
import com.nhom13.quanlymuontra.databinding.ViewSoanPhieuChanBinding;
import com.nhom13.quanlymuontra.databinding.ViewSoanPhieuDauBinding;
import com.nhom13.quanlymuontra.generated.model.TraCuuThietBiItem;
import com.nhom13.quanlymuontra.ui.tracuu.ChiTietLoaiTrinhBay;
import com.nhom13.quanlymuontra.ui.tracuu.LuaChonBoLoc;
import com.nhom13.quanlymuontra.ui.tracuu.TraCuuThietBiViewModel.TrangThaiTaiThem;
import java.util.List;
import java.util.Map;
import java.util.Objects;
import java.util.UUID;

/**
 * SCR-SV-04 Soạn phiếu yêu cầu mượn (UI-Screens 5.4, Wireframes SCR-SV-04, DS CMP-04, 06, 10, 16, FA 5.3, 5.6, 5.9).
 * Nhận {@code maPhong} và {@code maLoai} (tùy chọn) qua Safe Args. {@link SoanPhieuViewModel} lấy theo phạm vi nav
 * graph con {@code soanPhieuGraph} (FA 5.3 quy tắc 2, FC 6.3).
 *
 * <p>Chỉ hiển thị và chuyển sự kiện (FC 6.2). Nút Xem lại bật đúng điều kiện nhưng D10-L chưa có đích: SCR-SV-05 nối ở
 * D11-L. Liên kết Đăng ký chờ chỉ chạy cổng quyền mượn; quyền khác Bình thường thì mở SCR-SV-14 ở chế độ bị chặn, các
 * lựa chọn của phiếu vẫn nằm trong {@code ViewModel}. Gửi đăng ký chờ và DLG-SV-01 nối ở D13-L.
 */
public class SoanPhieuYeuCauFragment extends Fragment {

    private static final String KHOA_PHONG = "chonPhongSoanPhieu";
    private static final String KHOA_DOI_PHONG = "xacNhanDoiPhong";
    private static final String TAG_CHON_PHONG = "chonPhong";
    private static final String TAG_DOI_PHONG = "doiPhong";

    private FragmentSoanPhieuYeuCauBinding _binding;
    private ViewSoanPhieuDauBinding dau;
    private ViewSoanPhieuChanBinding chan;
    private SoanPhieuYeuCauAdapter adapter;
    private SoanPhieuViewModel viewModel;

    @NonNull
    private FragmentSoanPhieuYeuCauBinding binding() {
        return Objects.requireNonNull(_binding);
    }

    @Nullable
    @Override
    public View onCreateView(
            @NonNull LayoutInflater inflater,
            @Nullable ViewGroup container,
            @Nullable Bundle savedInstanceState) {
        _binding = FragmentSoanPhieuYeuCauBinding.inflate(inflater, container, false);
        binding().rvLoai.setLayoutManager(new LinearLayoutManager(requireContext()));
        dau = ViewSoanPhieuDauBinding.inflate(inflater, binding().rvLoai, false);
        chan = ViewSoanPhieuChanBinding.inflate(inflater, binding().rvLoai, false);
        return _binding.getRoot();
    }

    @Override
    public void onViewCreated(@NonNull View view, @Nullable Bundle savedInstanceState) {
        super.onViewCreated(view, savedInstanceState);
        UngDung ungDung = (UngDung) requireActivity().getApplication();
        // FC 6.3: ViewModel phạm vi nav graph con, dùng chung với SCR-SV-05 (D11-L).
        ViewModelStoreOwner chu =
                NavHostFragment.findNavController(this).getViewModelStoreOwner(R.id.soanPhieuGraph);
        viewModel =
                new ViewModelProvider(chu, new AppViewModelFactory(ungDung.getAppContainer()))
                        .get(SoanPhieuViewModel.class);
        SoanPhieuYeuCauFragmentArgs thamSo =
                SoanPhieuYeuCauFragmentArgs.fromBundle(requireArguments());
        viewModel.khoiTao(uuidHoacNull(thamSo.getMaPhong()), uuidHoacNull(thamSo.getMaLoai()));

        caiDanhSach();
        caiLamMoi();
        caiOChonPhong();
        caiThoiGian();
        // Nút Xem lại: D10-L chỉ bật/tắt theo điều kiện; đích SCR-SV-05 nối ở D11-L.

        viewModel.getDsPhong().observe(getViewLifecycleOwner(), tt -> hienOChonPhong());
        viewModel.getPhongDaChon().observe(getViewLifecycleOwner(), phong -> {
            hienOChonPhong();
            capNhatDanhSach();
        });
        viewModel.getDsLoai().observe(getViewLifecycleOwner(), tt -> capNhatDanhSach());
        viewModel.getTaiThem().observe(getViewLifecycleOwner(), c -> capNhatDanhSach());
        viewModel.getSoLuongDaChon().observe(getViewLifecycleOwner(), m -> capNhatDanhSach());
        viewModel.getXemLaiBat()
                .observe(getViewLifecycleOwner(), bat -> binding().btnXemLai.setEnabled(bat));
        viewModel.getYeuCauXacNhanDoiPhong()
                .observe(getViewLifecycleOwner(), this::moXacNhanDoiPhong);
        viewModel.getLoiKiemTraQuyen().observe(getViewLifecycleOwner(), this::hienLoiKiemTraQuyen);
        viewModel.getKetQuaCong().observe(getViewLifecycleOwner(), this::xuLyKetQuaCong);
        viewModel.load();
    }

    @Override
    public void onDestroyView() {
        // Xoay màn hình giữ dữ liệu; rời màn hình thì lần sau phải tải lại từ máy chủ (FA 5.6, ĐX-FC-22).
        if (!requireActivity().isChangingConfigurations()) {
            viewModel.danhDauCanTaiLai();
        }
        binding().rvLoai.setAdapter(null);
        adapter = null;
        dau = null;
        chan = null;
        _binding = null;
        super.onDestroyView();
    }

    @Nullable
    private static UUID uuidHoacNull(@Nullable String chuoi) {
        if (chuoi == null) {
            return null;
        }
        try {
            return UUID.fromString(chuoi);
        } catch (IllegalArgumentException e) {
            return null;
        }
    }

    // ---------------------------------------------------------------- dựng điều khiển

    private void caiDanhSach() {
        adapter =
                new SoanPhieuYeuCauAdapter(
                        dau.getRoot(),
                        chan.getRoot(),
                        viewModel::thuLaiTaiThem,
                        viewModel::datSoLuong,
                        maLoai -> viewModel.chonDangKyCho());
        binding().rvLoai.setAdapter(adapter);
        // Cuộn tới cuối thì tải trang kế cho đến khi đủ tongSo (ĐX-UI-13, FA 5.6).
        binding().rvLoai.addOnScrollListener(
                new RecyclerView.OnScrollListener() {
                    @Override
                    public void onScrolled(@NonNull RecyclerView rv, int dx, int dy) {
                        if (dy > 0 && !rv.canScrollVertically(1)) {
                            viewModel.taiThemLoai();
                        }
                    }
                });
    }

    private void caiLamMoi() {
        binding().srlLamMoi.setColorSchemeColors(
                ContextCompat.getColor(requireContext(), R.color.ds_color_progress_indicator));
        binding().srlLamMoi.setProgressBackgroundColorSchemeColor(
                ContextCompat.getColor(requireContext(), R.color.ds_color_surface_default));
        binding().srlLamMoi.setOnRefreshListener(() -> viewModel.refresh());
    }

    private void caiOChonPhong() {
        dau.selPhong.datNhan(getString(R.string.soan_phieu_nhan_phong));
        dau.selPhong.datKhiMo(v -> moChonPhong());
        getChildFragmentManager()
                .setFragmentResultListener(
                        KHOA_PHONG,
                        getViewLifecycleOwner(),
                        (khoa, ketQua) ->
                                khiChonPhong(ketQua.getString(ChonMotDialogFragment.KEY_MA, "")));
        getChildFragmentManager()
                .setFragmentResultListener(
                        KHOA_DOI_PHONG,
                        getViewLifecycleOwner(),
                        (khoa, ketQua) -> {
                            if (ketQua.getBoolean(XacNhanDialogFragment.KEY_XAC_NHAN)) {
                                viewModel.xacNhanDoiPhong();
                                dongHop(TAG_DOI_PHONG);
                            }
                        });
    }

    private void caiThoiGian() {
        // Dựng lại chữ đã nhập (xoay màn hình, quay lại từ SCR-SV-05) trước khi gắn bộ theo dõi.
        chan.etThoiGian.setText(viewModel.getThoiGianNhap());
        capNhatLoiThoiGian(viewModel.getThoiGianNhap());
        chan.etThoiGian.addTextChangedListener(
                new TextWatcher() {
                    @Override
                    public void beforeTextChanged(CharSequence s, int start, int count, int after) {}

                    @Override
                    public void onTextChanged(CharSequence s, int start, int before, int count) {}

                    @Override
                    public void afterTextChanged(Editable s) {
                        String nhap = s.toString();
                        viewModel.datThoiGian(nhap);
                        capNhatLoiThoiGian(nhap);
                    }
                });
    }

    /** Lỗi theo trường ở đúng ô (FC 6.6): chỉ khi đã nhập mà không phải số nguyên từ 1. */
    private void capNhatLoiThoiGian(@NonNull String nhap) {
        boolean sai = !nhap.trim().isEmpty() && SoanPhieuViewModel.phanTichThoiGian(nhap) == null;
        chan.tilThoiGian.setError(sai ? getString(R.string.soan_phieu_thoi_gian_loi) : null);
    }

    // ---------------------------------------------------------------- chọn phòng, đổi phòng

    private void hienOChonPhong() {
        UiState<List<LuaChonBoLoc>> trangThai = viewModel.getDsPhong().getValue();
        LuaChonBoLoc phong = viewModel.getPhongDaChon().getValue();
        if (trangThai == null) {
            return;
        }
        ApiError loi = trangThai.getLoi();
        if (trangThai.getTrangThai() == UiState.TrangThai.LOI && loi != null && phong == null) {
            dau.selPhong.hienLoi(loi, v -> viewModel.thuLaiPhong());
        } else if (trangThai.getTrangThai() == UiState.TrangThai.DANG_TAI && phong == null) {
            dau.selPhong.hienDangTai();
        } else {
            dau.selPhong.hienGiaTri(
                    phong == null
                            ? getString(R.string.soan_phieu_chua_chon_phong)
                            : tenPhong(phong));
        }
    }

    @NonNull
    private String tenPhong(@NonNull LuaChonBoLoc phong) {
        String coSo = phong.getPhuDe();
        return coSo == null || coSo.isEmpty()
                ? phong.getTen()
                : getString(R.string.tra_cuu_ghep_hai, phong.getTen(), coSo);
    }

    private void moChonPhong() {
        UiState<List<LuaChonBoLoc>> nguon = viewModel.getDsPhong().getValue();
        if (nguon == null
                || nguon.getTrangThai() != UiState.TrangThai.THANH_CONG
                || nguon.getDuLieu() == null
                || getChildFragmentManager().findFragmentByTag(TAG_CHON_PHONG) != null) {
            return;
        }
        List<LuaChonBoLoc> ds = nguon.getDuLieu();
        String[] ma = new String[ds.size()];
        String[] ten = new String[ds.size()];
        String[] phuDe = new String[ds.size()];
        for (int i = 0; i < ds.size(); i++) {
            ma[i] = ds.get(i).getMa().toString();
            ten[i] = ds.get(i).getTen();
            phuDe[i] = ds.get(i).getPhuDe() == null ? "" : ds.get(i).getPhuDe();
        }
        LuaChonBoLoc dangChon = viewModel.getPhongDaChon().getValue();
        ChonMotDialogFragment.tao(
                        KHOA_PHONG,
                        getString(R.string.soan_phieu_chon_phong_tieu_de),
                        ma,
                        ten,
                        phuDe,
                        dangChon == null ? "" : dangChon.getMa().toString())
                .show(getChildFragmentManager(), TAG_CHON_PHONG);
    }

    /** Kết quả chọn theo mã; mã không còn trong nguồn thì bỏ qua. */
    private void khiChonPhong(@NonNull String ma) {
        UiState<List<LuaChonBoLoc>> nguon = viewModel.getDsPhong().getValue();
        if (ma.isEmpty() || nguon == null || nguon.getDuLieu() == null) {
            return;
        }
        for (LuaChonBoLoc phong : nguon.getDuLieu()) {
            if (phong.getMa().toString().equals(ma)) {
                viewModel.chonPhong(phong);
                return;
            }
        }
    }

    /** ĐX-UI-06: đổi phòng khi đã chọn loại thì hỏi vì các loại đã chọn sẽ bị xóa. */
    private void moXacNhanDoiPhong(@NonNull Event<LuaChonBoLoc> suKien) {
        LuaChonBoLoc phong = suKien.getContentIfNotHandled();
        if (phong == null || getChildFragmentManager().findFragmentByTag(TAG_DOI_PHONG) != null) {
            return;
        }
        XacNhanDialogFragment.tao(
                        KHOA_DOI_PHONG,
                        getString(R.string.soan_phieu_doi_phong_tieu_de),
                        getString(R.string.soan_phieu_doi_phong_tom_tat, tenPhong(phong)),
                        getString(R.string.soan_phieu_doi_phong_he_qua),
                        getString(R.string.soan_phieu_doi_phong_nhan),
                        true,
                        null)
                .show(getChildFragmentManager(), TAG_DOI_PHONG);
    }

    private void dongHop(@NonNull String tag) {
        Fragment hop = getChildFragmentManager().findFragmentByTag(tag);
        if (hop instanceof DialogFragment) {
            ((DialogFragment) hop).dismiss();
        }
    }

    // ---------------------------------------------------------------- danh sách loại

    private void capNhatDanhSach() {
        if (adapter == null) {
            return;
        }
        UiState<List<TraCuuThietBiItem>> trangThai = viewModel.getDsLoai().getValue();
        LuaChonBoLoc phong = viewModel.getPhongDaChon().getValue();
        TrangThaiTaiThem taiThem = viewModel.getTaiThem().getValue();
        Map<UUID, Integer> soLuong = viewModel.getSoLuongDaChon().getValue();
        if (trangThai == null || taiThem == null || soLuong == null) {
            return;
        }
        List<TraCuuThietBiItem> dong = phong == null ? null : trangThai.getDuLieu();
        boolean dangTai = trangThai.getTrangThai() == UiState.TrangThai.DANG_TAI;
        ApiError loi = trangThai.getLoi();
        boolean coLoi = trangThai.getTrangThai() == UiState.TrangThai.LOI && loi != null;

        binding().srlLamMoi.setRefreshing(phong != null && dangTai && dong != null);
        dau.lvLoai.setVisibility(phong != null && dangTai && dong == null ? View.VISIBLE : View.GONE);
        if (phong != null && coLoi) {
            dau.evLoai.hienThi(loi, nut -> viewModel.thuLaiLoai());
        } else {
            dau.evLoai.setVisibility(View.GONE);
        }
        // Phòng chưa có loại nào: báo phòng chưa có thiết bị để đặt, sinh viên chọn phòng khác (UI-Screens 5.4).
        if (phong != null && trangThai.getTrangThai() == UiState.TrangThai.RONG) {
            dau.evRong.hienThi(getString(R.string.soan_phieu_phong_rong), null, null);
        } else {
            dau.evRong.setVisibility(View.GONE);
        }
        adapter.capNhat(dong, taiThem, soLuong);
    }

    // ---------------------------------------------------------------- cổng quyền mượn (liên kết Đăng ký chờ)

    /** Lỗi kiểm tra quyền mượn: khối thông báo ở đầu màn hình, cuộn lên đầu; không điều hướng. */
    private void hienLoiKiemTraQuyen(@Nullable ApiError loi) {
        if (loi == null) {
            dau.bannerLoiQuyen.an();
            return;
        }
        String noiDung = getString(ChiTietLoaiTrinhBay.thongDiepLoiQuyenMuon(loi));
        String maTheoDoi = ChiTietLoaiTrinhBay.maTheoDoi(loi);
        if (maTheoDoi != null) {
            noiDung = noiDung + "\n" + getString(R.string.trang_thai_ma_theo_doi, maTheoDoi);
        }
        dau.bannerLoiQuyen.hienThi(
                Tong.DANGER, getString(R.string.chi_tiet_loi_quyen_muon_tieu_de), noiDung, null);
        binding().rvLoai.scrollToPosition(0);
    }

    private void xuLyKetQuaCong(@NonNull Event<CongQuyenMuon.KetQua> suKien) {
        CongQuyenMuon.KetQua ketQua = suKien.getContentIfNotHandled();
        if (ketQua == null) {
            return;
        }
        if (ketQua.isDuocPhep()) {
            // Quyền Bình thường: gửi đăng ký chờ và DLG-SV-01 nối ở D13-L. Các lựa chọn của phiếu được giữ nguyên.
            return;
        }
        // FA 5.3 quy tắc 3: không gửi; mở SCR-SV-14 ở chế độ bị chặn nêu hành động vừa chọn.
        NavController dieuHuong = NavHostFragment.findNavController(this);
        NavDestination diemDen = dieuHuong.getCurrentDestination();
        if (diemDen == null || diemDen.getId() != R.id.soanPhieuYeuCauFragment) {
            return;
        }
        dieuHuong.navigate(
                SoanPhieuYeuCauFragmentDirections.actionSoanPhieuYeuCauFragmentToHoSoMuonFragment()
                        .setCheDoBiChan(true)
                        .setHanhDongDaChon(ketQua.getHanhDong().getMaThamSo()));
    }
}
