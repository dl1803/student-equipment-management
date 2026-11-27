package com.nhom13.quanlymuontra.ui.tracuu;

import android.os.Bundle;
import android.text.SpannableString;
import android.text.Spanned;
import android.text.style.BulletSpan;
import android.view.LayoutInflater;
import android.view.View;
import android.view.ViewGroup;
import androidx.annotation.NonNull;
import androidx.annotation.Nullable;
import androidx.core.content.ContextCompat;
import androidx.fragment.app.Fragment;
import androidx.lifecycle.ViewModelProvider;
import androidx.navigation.NavController;
import androidx.navigation.NavDestination;
import androidx.navigation.fragment.NavHostFragment;
import com.google.android.material.textview.MaterialTextView;
import com.nhom13.quanlymuontra.AppViewModelFactory;
import com.nhom13.quanlymuontra.R;
import com.nhom13.quanlymuontra.UngDung;
import com.nhom13.quanlymuontra.common.HangSo;
import com.nhom13.quanlymuontra.common.nhan.NhanPhatSinh;
import com.nhom13.quanlymuontra.common.nhan.Tong;
import com.nhom13.quanlymuontra.common.widget.ImageViewerDialogFragment;
import com.nhom13.quanlymuontra.common.widget.NutDangXuLy;
import com.nhom13.quanlymuontra.data.model.ApiError;
import com.nhom13.quanlymuontra.data.model.Event;
import com.nhom13.quanlymuontra.data.model.UiState;
import com.nhom13.quanlymuontra.databinding.FragmentChiTietLoaiThietBiBinding;
import com.nhom13.quanlymuontra.generated.model.TraCuuThietBiChiTiet;
import com.nhom13.quanlymuontra.ui.tracuu.ChiTietLoaiThietBiViewModel.HanhDong;
import com.nhom13.quanlymuontra.ui.tracuu.ChiTietLoaiThietBiViewModel.KetQuaCong;
import java.util.List;
import java.util.Objects;
import java.util.UUID;

/**
 * SCR-SV-03 Chi tiết loại thiết bị tại phòng (UI-Screens 5.3, Wireframes SCR-SV-03, demo-ui-android, DS CMP-23, FA
 * 5.3, 5.6). Nhận {@code maLoai} và {@code maPhong} qua Safe Args từ dòng kết quả của SCR-SV-02.
 *
 * <p>Chỉ hiển thị và chuyển sự kiện; mọi quyết định tải và cổng kiểm tra quyền mượn nằm ở {@link
 * ChiTietLoaiThietBiViewModel} (FC 6.2). Nút ghim cuối màn hình là Đặt trước khi máy chủ trả số lượng khả dụng lớn hơn
 * 0, là Đăng ký chờ khi bằng 0. Chọn nút gọi cổng kiểm tra quyền mượn; quyền khác Bình thường thì mở SCR-SV-14 ở chế
 * độ bị chặn nêu hành động vừa chọn (FA 5.3 quy tắc 3).
 *
 * <p>D10-L nối Đặt trước tới SCR-SV-04 khi cổng cho phép; Đăng ký chờ cùng hộp kết quả vẫn chưa gửi gì và nối ở D13-L.
 */
public class ChiTietLoaiThietBiFragment extends Fragment {

    private static final String TAG_XEM_ANH = "xemAnh";

    private FragmentChiTietLoaiThietBiBinding _binding;
    private ChiTietLoaiThietBiViewModel viewModel;
    /** Hành động của nút ghim theo dữ liệu mới nhất; {@code null} khi chưa có dữ liệu. */
    @Nullable private HanhDong hanhDongHienTai;
    /** Phản chiếu cờ đang kiểm tra quyền mượn của ViewModel. */
    private boolean dangKiemTra;
    /** Tên thiết bị của dữ liệu đang hiển thị, dùng cho tên truy cập ảnh xem lớn. */
    @NonNull private String tenThietBi = "";

    @NonNull
    private FragmentChiTietLoaiThietBiBinding binding() {
        return Objects.requireNonNull(_binding);
    }

    @Nullable
    @Override
    public View onCreateView(
            @NonNull LayoutInflater inflater,
            @Nullable ViewGroup container,
            @Nullable Bundle savedInstanceState) {
        _binding = FragmentChiTietLoaiThietBiBinding.inflate(inflater, container, false);
        return _binding.getRoot();
    }

    @Override
    public void onViewCreated(@NonNull View view, @Nullable Bundle savedInstanceState) {
        super.onViewCreated(view, savedInstanceState);
        ChiTietLoaiThietBiFragmentArgs thamSo =
                ChiTietLoaiThietBiFragmentArgs.fromBundle(requireArguments());
        UngDung ungDung = (UngDung) requireActivity().getApplication();
        viewModel =
                new ViewModelProvider(this, new AppViewModelFactory(ungDung.getAppContainer()))
                        .get(ChiTietLoaiThietBiViewModel.class);
        viewModel.khoiTao(UUID.fromString(thamSo.getMaLoai()), UUID.fromString(thamSo.getMaPhong()));

        caiLamMoi();
        caiAnh();
        // Chống bấm trùng: ViewModel bật cờ đồng bộ rồi trạng thái "đang xử lý" của nút do dangKiemTraQuyen quan sát.
        binding().btnGhim.setOnClickListener(nut -> khiBamNutGhim());

        viewModel.getChiTiet().observe(getViewLifecycleOwner(), this::hienThi);
        viewModel.getDangKiemTraQuyen().observe(getViewLifecycleOwner(), this::hienDangKiemTra);
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
        binding().agvAnh.giaiPhong();
        _binding = null;
        super.onDestroyView();
    }

    // ---------------------------------------------------------------- dựng điều khiển

    private void caiLamMoi() {
        binding().srlLamMoi.setColorSchemeColors(
                ContextCompat.getColor(requireContext(), R.color.ds_color_progress_indicator));
        binding().srlLamMoi.setProgressBackgroundColorSchemeColor(
                ContextCompat.getColor(requireContext(), R.color.ds_color_surface_default));
        binding().srlLamMoi.setOnRefreshListener(() -> viewModel.refresh());
    }

    private void caiAnh() {
        binding().agvAnh.datKhiChonAnh(
                (viTri, url) -> {
                    if (getChildFragmentManager().findFragmentByTag(TAG_XEM_ANH) != null) {
                        return;
                    }
                    ImageViewerDialogFragment.tao(url, viTri + 1, tenThietBi)
                            .show(getChildFragmentManager(), TAG_XEM_ANH);
                });
        // "Tải lại" của khung ảnh tải lại chi tiết (DS CMP-23).
        binding().agvAnh.datKhiTaiLai(nut -> viewModel.refresh());
    }

    private void khiBamNutGhim() {
        if (hanhDongHienTai == HanhDong.DAT_TRUOC) {
            viewModel.chonDatTruoc();
        } else if (hanhDongHienTai == HanhDong.DANG_KY_CHO) {
            viewModel.chonDangKyCho();
        }
    }

    // ---------------------------------------------------------------- hiển thị

    private void hienThi(@NonNull UiState<TraCuuThietBiChiTiet> trangThai) {
        FragmentChiTietLoaiThietBiBinding b = binding();
        TraCuuThietBiChiTiet chiTiet = trangThai.getDuLieu();
        boolean dangTai = trangThai.getTrangThai() == UiState.TrangThai.DANG_TAI;
        ApiError loi = trangThai.getLoi();
        boolean coLoi = trangThai.getTrangThai() == UiState.TrangThai.LOI && loi != null;

        b.srlLamMoi.setRefreshing(dangTai && chiTiet != null);
        b.lvTai.setVisibility(dangTai && chiTiet == null ? View.VISIBLE : View.GONE);

        if (coLoi && ChiTietLoaiTrinhBay.laKhongTimThay(loi)) {
            // UI-Screens 3.2: 404 KHONG_TIM_THAY, báo không tìm thấy rồi quay về danh sách (SCR-SV-02 tự tải lại).
            b.evLoi.setVisibility(View.GONE);
            b.bannerKhongTimThay.hienThi(
                    Tong.WARNING, getString(R.string.chi_tiet_khong_tim_thay_tieu_de), null, null);
            b.bannerKhongTimThay.datHanhDong(
                    getString(R.string.chi_tiet_ve_danh_sach),
                    nut -> NavHostFragment.findNavController(this).popBackStack());
        } else {
            b.bannerKhongTimThay.an();
            if (coLoi) {
                b.evLoi.hienThi(loi, nut -> viewModel.refresh());
            } else {
                b.evLoi.setVisibility(View.GONE);
            }
        }

        boolean hienNoiDung = !coLoi && chiTiet != null;
        b.llNoiDung.setVisibility(hienNoiDung ? View.VISIBLE : View.GONE);
        if (hienNoiDung) {
            dungNoiDung(chiTiet);
        }
        capNhatNutGhim(hienNoiDung ? chiTiet : null);
    }

    private void dungNoiDung(@NonNull TraCuuThietBiChiTiet chiTiet) {
        FragmentChiTietLoaiThietBiBinding b = binding();
        tenThietBi = chiTiet.getTenLoai() == null ? "" : chiTiet.getTenLoai();

        b.agvAnh.datAnh(ChiTietLoaiTrinhBay.dsUrlAnh(chiTiet.getDsAnh()), tenThietBi);
        b.tvTenLoai.setText(tenThietBi);
        String hang = chiTiet.getHangSanXuat();
        b.tvHangPhongCoSo.setText(
                hang == null || hang.isEmpty()
                        ? getString(R.string.tra_cuu_ghep_hai, chiTiet.getTenPhong(), chiTiet.getCoSo())
                        : getString(
                                R.string.chi_tiet_ghep_ba, hang, chiTiet.getTenPhong(), chiTiet.getCoSo()));

        boolean coMoTa = ChiTietLoaiTrinhBay.coChu(chiTiet.getMoTa());
        b.llMoTa.setVisibility(coMoTa ? View.VISIBLE : View.GONE);
        if (coMoTa) {
            b.tvMoTa.setText(chiTiet.getMoTa());
        }

        List<String> dsThongSo = ChiTietLoaiTrinhBay.dsThongSo(chiTiet.getDsThongSoKyThuat());
        b.llThongSo.setVisibility(dsThongSo.isEmpty() ? View.GONE : View.VISIBLE);
        dungDsThongSo(dsThongSo);

        // Số lượng khả dụng và ngày dự kiến do máy chủ tính (BR-16, BR-20, ĐX-KT-05); client chỉ hiển thị.
        Integer soLuong = chiTiet.getSoLuongKhaDung();
        b.tvSoLuong.setText(soLuong == null ? HangSo.GIA_TRI_THIEU : String.valueOf(soLuong));
        boolean hetHang = ChiTietLoaiTrinhBay.hetHang(soLuong);
        if (hetHang) {
            b.sbHetHang.setNhanPhatSinh(NhanPhatSinh.HET_HANG);
        }
        b.sbHetHang.setVisibility(hetHang ? View.VISIBLE : View.GONE);
        b.llNgayDuKien.setVisibility(hetHang ? View.VISIBLE : View.GONE);
        if (hetHang) {
            b.tvNgayDuKien.setText(
                    ChiTietLoaiTrinhBay.ngayDuKien(
                            chiTiet.getNgayDuKienCoSan(), getString(R.string.chi_tiet_chua_co_ngay)));
        }
    }

    /** Mỗi thông số một dòng có dấu đầu dòng (cùng cách khối thông báo CMP-24 trình bày danh sách lý do). */
    private void dungDsThongSo(@NonNull List<String> dsThongSo) {
        binding().llDsThongSo.removeAllViews();
        int khoang = getResources().getDimensionPixelSize(R.dimen.ds_space_2);
        for (String thongSo : dsThongSo) {
            SpannableString dong = new SpannableString(thongSo);
            dong.setSpan(new BulletSpan(khoang), 0, dong.length(), Spanned.SPAN_INCLUSIVE_EXCLUSIVE);
            MaterialTextView tv = new MaterialTextView(requireContext());
            tv.setTextAppearance(R.style.TextAppearance_Ds_Body);
            tv.setText(dong);
            binding().llDsThongSo.addView(tv);
        }
    }

    /** Nút ghim: một trong hai nút theo số lượng khả dụng; đang kiểm tra quyền thì "Đang xử lý" và vô hiệu. */
    private void capNhatNutGhim(@Nullable TraCuuThietBiChiTiet chiTiet) {
        FragmentChiTietLoaiThietBiBinding b = binding();
        hanhDongHienTai =
                chiTiet == null
                        ? null
                        : ChiTietLoaiTrinhBay.hanhDongChoSoLuong(chiTiet.getSoLuongKhaDung());
        if (hanhDongHienTai == null) {
            b.llGhim.setVisibility(View.GONE);
            return;
        }
        b.llGhim.setVisibility(View.VISIBLE);
        if (dangKiemTra) {
            NutDangXuLy.batDau(b.btnGhim);
        } else {
            NutDangXuLy.ketThuc(b.btnGhim, ChiTietLoaiTrinhBay.nhanNut(hanhDongHienTai));
            b.btnGhim.setEnabled(true);
        }
    }

    private void hienDangKiemTra(boolean dang) {
        dangKiemTra = dang;
        UiState<TraCuuThietBiChiTiet> trangThai = viewModel.getChiTiet().getValue();
        TraCuuThietBiChiTiet chiTiet = trangThai == null ? null : trangThai.getDuLieu();
        boolean coLoi = trangThai != null && trangThai.getTrangThai() == UiState.TrangThai.LOI;
        capNhatNutGhim(coLoi ? null : chiTiet);
    }

    /** Lỗi kiểm tra quyền mượn: khối thông báo ở đầu màn hình, cuộn lên đầu; không điều hướng (UI-Screens 5.3). */
    private void hienLoiKiemTraQuyen(@Nullable ApiError loi) {
        if (loi == null) {
            binding().bannerLoiQuyen.an();
            return;
        }
        String noiDung = getString(ChiTietLoaiTrinhBay.thongDiepLoiQuyenMuon(loi));
        String maTheoDoi = ChiTietLoaiTrinhBay.maTheoDoi(loi);
        if (maTheoDoi != null) {
            noiDung = noiDung + "\n" + getString(R.string.trang_thai_ma_theo_doi, maTheoDoi);
        }
        binding().bannerLoiQuyen.hienThi(
                Tong.DANGER, getString(R.string.chi_tiet_loi_quyen_muon_tieu_de), noiDung, null);
        binding().svNoiDung.smoothScrollTo(0, 0);
    }

    // ---------------------------------------------------------------- cổng kiểm tra quyền mượn

    private void xuLyKetQuaCong(@NonNull Event<KetQuaCong> suKien) {
        KetQuaCong ketQua = suKien.getContentIfNotHandled();
        if (ketQua == null) {
            return;
        }
        NavController dieuHuong = NavHostFragment.findNavController(this);
        NavDestination diemDen = dieuHuong.getCurrentDestination();
        if (diemDen == null || diemDen.getId() != R.id.chiTietLoaiThietBiFragment) {
            return;
        }
        if (ketQua.isDuocPhep()) {
            if (ketQua.getHanhDong() == HanhDong.DAT_TRUOC) {
                // D10-L: Đặt trước, quyền Bình thường: mở SCR-SV-04 với phòng và loại đang xem (số lượng ban đầu 1).
                ChiTietLoaiThietBiFragmentArgs thamSo =
                        ChiTietLoaiThietBiFragmentArgs.fromBundle(requireArguments());
                dieuHuong.navigate(
                        ChiTietLoaiThietBiFragmentDirections
                                .actionChiTietLoaiThietBiFragmentToSoanPhieuGraph()
                                .setMaPhong(thamSo.getMaPhong())
                                .setMaLoai(thamSo.getMaLoai()));
            }
            // Đăng ký chờ: gửi và hộp kết quả nối ở D13-L.
            return;
        }
        // FA 5.3 quy tắc 3: không gửi; mở SCR-SV-14 ở chế độ bị chặn nêu hành động vừa chọn.
        dieuHuong.navigate(
                ChiTietLoaiThietBiFragmentDirections.actionChiTietLoaiThietBiFragmentToHoSoMuonFragment()
                        .setCheDoBiChan(true)
                        .setHanhDongDaChon(ketQua.getHanhDong().getMaThamSo()));
    }
}
