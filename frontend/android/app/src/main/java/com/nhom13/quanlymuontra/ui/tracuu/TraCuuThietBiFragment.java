package com.nhom13.quanlymuontra.ui.tracuu;

import android.os.Bundle;
import android.text.Editable;
import android.text.TextWatcher;
import android.view.LayoutInflater;
import android.view.View;
import android.view.ViewGroup;
import android.view.inputmethod.EditorInfo;
import android.widget.EditText;
import androidx.annotation.NonNull;
import androidx.annotation.Nullable;
import androidx.annotation.StringRes;
import androidx.core.content.ContextCompat;
import androidx.core.view.WindowCompat;
import androidx.core.view.WindowInsetsCompat;
import androidx.core.view.WindowInsetsControllerCompat;
import androidx.fragment.app.Fragment;
import androidx.lifecycle.ViewModelProvider;
import androidx.recyclerview.widget.LinearLayoutManager;
import androidx.recyclerview.widget.RecyclerView;
import com.google.android.material.button.MaterialButton;
import com.google.android.material.chip.Chip;
import com.nhom13.quanlymuontra.AppViewModelFactory;
import com.nhom13.quanlymuontra.R;
import com.nhom13.quanlymuontra.UngDung;
import com.nhom13.quanlymuontra.common.widget.ChonMotDialogFragment;
import com.nhom13.quanlymuontra.common.widget.SelectView;
import com.nhom13.quanlymuontra.data.model.ApiError;
import com.nhom13.quanlymuontra.data.model.UiState;
import com.nhom13.quanlymuontra.databinding.FragmentTraCuuThietBiBinding;
import com.nhom13.quanlymuontra.databinding.ViewTraCuuBoLocBinding;
import com.nhom13.quanlymuontra.generated.model.TraCuuThietBiItem;
import java.util.List;
import java.util.Objects;

/**
 * SCR-SV-02 Tra cứu thiết bị, tab Tra cứu (UI-Screens 5.2, Wireframes SCR-SV-02, ĐX-WF-15, FA 5.3, 5.6).
 *
 * <p>Ô tìm và bộ lọc là đầu của danh sách nên cuộn cùng danh sách. Mọi quyết định tải, debounce, hủy yêu cầu cũ,
 * tải thêm nằm ở {@link TraCuuThietBiViewModel}; lớp này chỉ hiển thị và chuyển sự kiện (FC 6.2). Lỗi tải nguồn
 * bộ lọc chỉ làm ô chọn tương ứng lỗi kèm Thử lại, danh sách kết quả vẫn dùng được.
 *
 * <p>D08-L: dòng kết quả chưa bấm được vì SCR-SV-03 (chi tiết loại tại phòng) thuộc D09-L; ngày đó nối điều
 * hướng với {@code maLoai}, {@code maPhong} của dòng.
 */
public class TraCuuThietBiFragment extends Fragment {

    private static final String TAG_CHON = "chonBoLoc";
    private static final String KHOA_LOAI = "chonLoai";
    private static final String KHOA_PHONG = "chonPhong";

    private FragmentTraCuuThietBiBinding _binding;
    private ViewTraCuuBoLocBinding dau;
    private TraCuuThietBiAdapter adapter;
    private TraCuuThietBiViewModel viewModel;
    /** {@code true} khi đang gán giá trị từ ViewModel vào điều khiển, để không phát lại sự kiện thay đổi. */
    private boolean dangGanBoLoc;

    @NonNull
    private FragmentTraCuuThietBiBinding binding() {
        return Objects.requireNonNull(_binding);
    }

    @Nullable
    @Override
    public View onCreateView(
            @NonNull LayoutInflater inflater,
            @Nullable ViewGroup container,
            @Nullable Bundle savedInstanceState) {
        _binding = FragmentTraCuuThietBiBinding.inflate(inflater, container, false);
        binding().rvKetQua.setLayoutManager(new LinearLayoutManager(requireContext()));
        dau = ViewTraCuuBoLocBinding.inflate(inflater, binding().rvKetQua, false);
        return _binding.getRoot();
    }

    @Override
    public void onViewCreated(@NonNull View view, @Nullable Bundle savedInstanceState) {
        super.onViewCreated(view, savedInstanceState);
        UngDung ungDung = (UngDung) requireActivity().getApplication();
        viewModel =
                new ViewModelProvider(this, new AppViewModelFactory(ungDung.getAppContainer()))
                        .get(TraCuuThietBiViewModel.class);

        caiDanhSach();
        caiLamMoi();
        caiOTimKiem();
        caiBoLoc();

        viewModel.getBoLoc().observe(getViewLifecycleOwner(), this::hienBoLoc);
        viewModel.getDsLoai().observe(getViewLifecycleOwner(), tt -> hienOChonLoai());
        viewModel.getDsPhong().observe(getViewLifecycleOwner(), tt -> hienOChonPhong());
        viewModel.getDanhSach().observe(getViewLifecycleOwner(), this::hienDanhSach);
        viewModel.getTaiThem().observe(getViewLifecycleOwner(), chan -> capNhatAdapter());
        viewModel.load();
    }

    @Override
    public void onDestroyView() {
        // Xoay màn hình giữ dữ liệu; rời màn hình thì lần sau phải tải lại từ máy chủ (FA 5.6, ĐX-FC-22).
        if (!requireActivity().isChangingConfigurations()) {
            viewModel.danhDauCanTaiLai();
        }
        binding().rvKetQua.setAdapter(null);
        adapter = null;
        dau = null;
        _binding = null;
        super.onDestroyView();
    }

    // ---------------------------------------------------------------- dựng điều khiển

    private void caiDanhSach() {
        adapter = new TraCuuThietBiAdapter(dau.getRoot(), viewModel::thuLaiTaiThem);
        binding().rvKetQua.setLayoutManager(new LinearLayoutManager(requireContext()));
        binding().rvKetQua.setAdapter(adapter);
        // Cuộn tới cuối thì tải trang kế cho đến khi đủ tongSo (ĐX-UI-13, FA 5.6).
        binding().rvKetQua.addOnScrollListener(
                new RecyclerView.OnScrollListener() {
                    @Override
                    public void onScrolled(@NonNull RecyclerView rv, int dx, int dy) {
                        if (dy > 0 && !rv.canScrollVertically(1)) {
                            viewModel.taiThem();
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

    private void caiOTimKiem() {
        // Chữ gợi ý đặt bằng mã (sau khi TextInputLayout đã dựng) để không bị TextInputLayout chuyển thành nhãn nổi.
        dau.etTimKiem.setHint(R.string.tra_cuu_tim_kiem_goi_y);
        dau.etTimKiem.addTextChangedListener(
                new TextWatcher() {
                    @Override
                    public void beforeTextChanged(CharSequence s, int start, int count, int after) {}

                    @Override
                    public void onTextChanged(CharSequence s, int start, int before, int count) {}

                    @Override
                    public void afterTextChanged(Editable s) {
                        if (!dangGanBoLoc) {
                            viewModel.datTuKhoa(s.toString(), false);
                        }
                    }
                });
        dau.etTimKiem.setOnEditorActionListener(
                (v, actionId, event) -> {
                    if (actionId != EditorInfo.IME_ACTION_SEARCH) {
                        return false;
                    }
                    viewModel.datTuKhoa(dau.etTimKiem.getText().toString(), true);
                    WindowInsetsControllerCompat dieuKhien =
                            WindowCompat.getInsetsController(requireActivity().getWindow(), v);
                    dieuKhien.hide(WindowInsetsCompat.Type.ime());
                    return true;
                });
    }

    private void caiBoLoc() {
        dau.selLoai.datNhan(getString(R.string.tra_cuu_loc_loai));
        dau.selPhong.datNhan(getString(R.string.tra_cuu_loc_phong));
        dau.selLoai.datKhiMo(v -> moChonLoai());
        dau.selPhong.datKhiMo(v -> moChonPhong());

        dau.tgTinhTrang.addOnButtonCheckedListener(
                (nhom, idNut, daChon) -> {
                    capNhatNetPhanDoan();
                    if (daChon && !dangGanBoLoc) {
                        viewModel.chonTinhTrang(tinhTrangTheoId(idNut));
                    }
                });
        dau.btnXoaBoLoc.setOnClickListener(v -> viewModel.xoaBoLoc());

        getChildFragmentManager()
                .setFragmentResultListener(
                        KHOA_LOAI,
                        getViewLifecycleOwner(),
                        (khoa, ketQua) ->
                                khiChon(
                                        ketQua.getString(ChonMotDialogFragment.KEY_MA, ""),
                                        viewModel.getDsLoai().getValue(),
                                        true));
        getChildFragmentManager()
                .setFragmentResultListener(
                        KHOA_PHONG,
                        getViewLifecycleOwner(),
                        (khoa, ketQua) ->
                                khiChon(
                                        ketQua.getString(ChonMotDialogFragment.KEY_MA, ""),
                                        viewModel.getDsPhong().getValue(),
                                        false));
    }

    // ---------------------------------------------------------------- hộp chọn

    private void moChonLoai() {
        BoLoc loc = viewModel.getBoLoc().getValue();
        moChon(
                KHOA_LOAI,
                R.string.tra_cuu_loc_loai,
                viewModel.getDsLoai().getValue(),
                loc == null || loc.getLoai() == null ? "" : loc.getLoai().getMa().toString());
    }

    private void moChonPhong() {
        BoLoc loc = viewModel.getBoLoc().getValue();
        moChon(
                KHOA_PHONG,
                R.string.tra_cuu_loc_phong,
                viewModel.getDsPhong().getValue(),
                loc == null || loc.getPhong() == null ? "" : loc.getPhong().getMa().toString());
    }

    private void moChon(
            @NonNull String khoa,
            @StringRes int tieuDe,
            @Nullable UiState<List<LuaChonBoLoc>> nguon,
            @NonNull String maDangChon) {
        if (nguon == null
                || nguon.getTrangThai() != UiState.TrangThai.THANH_CONG
                || nguon.getDuLieu() == null
                || getChildFragmentManager().findFragmentByTag(TAG_CHON) != null) {
            return;
        }
        List<LuaChonBoLoc> ds = nguon.getDuLieu();
        String[] ma = new String[ds.size() + 1];
        String[] ten = new String[ds.size() + 1];
        String[] phuDe = new String[ds.size() + 1];
        ma[0] = "";
        ten[0] = getString(R.string.tra_cuu_tat_ca);
        phuDe[0] = "";
        for (int i = 0; i < ds.size(); i++) {
            ma[i + 1] = ds.get(i).getMa().toString();
            ten[i + 1] = ds.get(i).getTen();
            phuDe[i + 1] = ds.get(i).getPhuDe() == null ? "" : ds.get(i).getPhuDe();
        }
        ChonMotDialogFragment.tao(khoa, getString(tieuDe), ma, ten, phuDe, maDangChon)
                .show(getChildFragmentManager(), TAG_CHON);
    }

    /** Kết quả chọn theo mã: rỗng là "Tất cả"; mã không còn trong nguồn thì bỏ qua. */
    private void khiChon(
            @NonNull String ma, @Nullable UiState<List<LuaChonBoLoc>> nguon, boolean laLoai) {
        LuaChonBoLoc chon = null;
        if (!ma.isEmpty()) {
            if (nguon == null || nguon.getDuLieu() == null) {
                return;
            }
            for (LuaChonBoLoc muc : nguon.getDuLieu()) {
                if (muc.getMa().toString().equals(ma)) {
                    chon = muc;
                    break;
                }
            }
            if (chon == null) {
                return;
            }
        }
        if (laLoai) {
            viewModel.chonLoai(chon);
        } else {
            viewModel.chonPhong(chon);
        }
    }

    // ---------------------------------------------------------------- hiển thị

    private void hienBoLoc(@NonNull BoLoc loc) {
        dangGanBoLoc = true;
        try {
            String tuKhoa = viewModel.getTuKhoaHienThi();
            EditText o = dau.etTimKiem;
            if (!tuKhoa.equals(o.getText().toString().trim())) {
                o.setText(tuKhoa);
                o.setSelection(o.getText().length());
            }
            dau.tgTinhTrang.check(idTheoTinhTrang(loc.getTinhTrang()));
        } finally {
            dangGanBoLoc = false;
        }
        capNhatNetPhanDoan();
        hienChip(loc);
        dau.btnXoaBoLoc.setVisibility(loc.coBoLoc() ? View.VISIBLE : View.GONE);
        hienOChonLoai();
        hienOChonPhong();
        UiState<List<TraCuuThietBiItem>> danhSach = viewModel.getDanhSach().getValue();
        if (danhSach != null) {
            hienDanhSach(danhSach);
        }
    }

    private static int idTheoTinhTrang(@NonNull TinhTrangHang tinhTrang) {
        switch (tinhTrang) {
            case CON_HANG:
                return R.id.btn_tinh_trang_con_hang;
            case HET_HANG:
                return R.id.btn_tinh_trang_het_hang;
            case TAT_CA:
            default:
                return R.id.btn_tinh_trang_tat_ca;
        }
    }

    @NonNull
    private static TinhTrangHang tinhTrangTheoId(int idNut) {
        if (idNut == R.id.btn_tinh_trang_con_hang) {
            return TinhTrangHang.CON_HANG;
        }
        if (idNut == R.id.btn_tinh_trang_het_hang) {
            return TinhTrangHang.HET_HANG;
        }
        return TinhTrangHang.TAT_CA;
    }

    /** CMP-08: ô đang chọn có viền dày và dấu check (dấu hiệu ngoài màu); các ô còn lại viền thường, không biểu tượng. */
    private void capNhatNetPhanDoan() {
        MaterialButton[] cacNut = {
            dau.btnTinhTrangTatCa, dau.btnTinhTrangConHang, dau.btnTinhTrangHetHang
        };
        for (MaterialButton nut : cacNut) {
            boolean chon = nut.isChecked();
            nut.setStrokeWidth(
                    getResources()
                            .getDimensionPixelSize(
                                    chon
                                            ? R.dimen.ds_border_width_strong
                                            : R.dimen.ds_border_width_default));
            nut.setIcon(chon ? ContextCompat.getDrawable(requireContext(), R.drawable.ic_check) : null);
        }
    }

    /** CMP-17: mỗi bộ lọc Loại, Phòng, Tình trạng khác mặc định có một chip; chạm chip xóa bộ lọc đó. */
    private void hienChip(@NonNull BoLoc loc) {
        dau.cgBoLoc.removeAllViews();
        if (loc.getLoai() != null) {
            themChip(
                    getString(R.string.tra_cuu_chip_loai, loc.getLoai().getTen()),
                    v -> viewModel.xoaLoai());
        }
        if (loc.getPhong() != null) {
            themChip(
                    getString(R.string.tra_cuu_chip_phong, loc.getPhong().getTen()),
                    v -> viewModel.xoaPhong());
        }
        if (loc.getTinhTrang() == TinhTrangHang.CON_HANG) {
            themChip(getString(R.string.tra_cuu_con_hang), v -> viewModel.xoaTinhTrang());
        } else if (loc.getTinhTrang() == TinhTrangHang.HET_HANG) {
            themChip(getString(R.string.tra_cuu_het_hang), v -> viewModel.xoaTinhTrang());
        }
        dau.cgBoLoc.setVisibility(dau.cgBoLoc.getChildCount() > 0 ? View.VISIBLE : View.GONE);
    }

    private void themChip(@NonNull String nhan, @NonNull View.OnClickListener khiXoa) {
        Chip chip = (Chip) getLayoutInflater().inflate(R.layout.view_chip_bo_loc, dau.cgBoLoc, false);
        chip.setText(nhan);
        chip.setCloseIconContentDescription(getString(R.string.tra_cuu_chip_xoa, nhan));
        chip.setOnClickListener(khiXoa);
        chip.setOnCloseIconClickListener(khiXoa);
        dau.cgBoLoc.addView(chip);
    }

    private void hienOChonLoai() {
        BoLoc loc = viewModel.getBoLoc().getValue();
        LuaChonBoLoc chon = loc == null ? null : loc.getLoai();
        hienOChon(
                dau.selLoai,
                viewModel.getDsLoai().getValue(),
                chon == null ? null : chon.getTen(),
                v -> viewModel.thuLaiNguonLoai());
    }

    private void hienOChonPhong() {
        BoLoc loc = viewModel.getBoLoc().getValue();
        LuaChonBoLoc chon = loc == null ? null : loc.getPhong();
        String giaTri = null;
        if (chon != null) {
            giaTri =
                    chon.getPhuDe() == null
                            ? chon.getTen()
                            : getString(R.string.tra_cuu_ghep_hai, chon.getTen(), chon.getPhuDe());
        }
        hienOChon(
                dau.selPhong,
                viewModel.getDsPhong().getValue(),
                giaTri,
                v -> viewModel.thuLaiNguonPhong());
    }

    private void hienOChon(
            @NonNull SelectView o,
            @Nullable UiState<List<LuaChonBoLoc>> nguon,
            @Nullable String giaTri,
            @NonNull View.OnClickListener thuLai) {
        if (nguon == null || nguon.getTrangThai() == UiState.TrangThai.DANG_TAI) {
            o.hienDangTai();
            return;
        }
        ApiError loi = nguon.getLoi();
        if (nguon.getTrangThai() == UiState.TrangThai.LOI && loi != null) {
            o.hienLoi(loi, thuLai);
            return;
        }
        o.hienGiaTri(giaTri == null ? getString(R.string.tra_cuu_tat_ca) : giaTri);
    }

    private void hienDanhSach(@NonNull UiState<List<TraCuuThietBiItem>> trangThai) {
        List<TraCuuThietBiItem> dong = trangThai.getDuLieu();
        boolean coDong = dong != null && !dong.isEmpty();
        boolean dangTai = trangThai.getTrangThai() == UiState.TrangThai.DANG_TAI;
        ApiError loi = trangThai.getLoi();

        binding().srlLamMoi.setRefreshing(dangTai && coDong);
        dau.lvKetQua.setVisibility(dangTai && !coDong ? View.VISIBLE : View.GONE);

        if (trangThai.getTrangThai() == UiState.TrangThai.LOI && loi != null) {
            dau.evKetQua.hienThi(loi, v -> viewModel.thuLaiDanhSach());
        } else {
            dau.evKetQua.setVisibility(View.GONE);
        }

        if (trangThai.getTrangThai() == UiState.TrangThai.RONG) {
            BoLoc loc = viewModel.getBoLoc().getValue();
            boolean coBoLoc = loc != null && loc.coBoLoc();
            dau.evRong.datBieuTuong(R.drawable.ic_search_off);
            dau.evRong.hienThi(
                    getString(R.string.tra_cuu_rong),
                    coBoLoc ? getString(R.string.tra_cuu_xoa_bo_loc) : null,
                    v -> viewModel.xoaBoLoc());
        } else {
            dau.evRong.setVisibility(View.GONE);
        }
        capNhatAdapter();
    }

    private void capNhatAdapter() {
        if (adapter == null) {
            return;
        }
        UiState<List<TraCuuThietBiItem>> trangThai = viewModel.getDanhSach().getValue();
        TraCuuThietBiViewModel.TrangThaiTaiThem chan = viewModel.getTaiThem().getValue();
        adapter.capNhat(
                trangThai == null ? null : trangThai.getDuLieu(),
                chan == null ? TraCuuThietBiViewModel.TrangThaiTaiThem.KHONG : chan);
    }
}
