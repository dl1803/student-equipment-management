package com.nhom13.quanlymuontra.ui.phieuyeucau;

import android.os.Bundle;
import android.text.TextUtils;
import android.view.LayoutInflater;
import android.view.View;
import android.view.ViewGroup;
import androidx.annotation.NonNull;
import androidx.annotation.Nullable;
import androidx.fragment.app.Fragment;
import androidx.lifecycle.ViewModelProvider;
import androidx.lifecycle.ViewModelStoreOwner;
import androidx.navigation.NavController;
import androidx.navigation.NavDestination;
import androidx.navigation.fragment.NavHostFragment;
import com.nhom13.quanlymuontra.AppViewModelFactory;
import com.nhom13.quanlymuontra.R;
import com.nhom13.quanlymuontra.UngDung;
import com.nhom13.quanlymuontra.common.nhan.Tong;
import com.nhom13.quanlymuontra.common.widget.NutDangXuLy;
import com.nhom13.quanlymuontra.data.model.ApiError;
import com.nhom13.quanlymuontra.data.model.Event;
import com.nhom13.quanlymuontra.databinding.FragmentXemLaiPhieuYeuCauBinding;
import com.nhom13.quanlymuontra.ui.phieuyeucau.SoanPhieuViewModel.DieuHuongGui;
import com.nhom13.quanlymuontra.ui.phieuyeucau.SoanPhieuViewModel.TrangThaiGui;
import com.nhom13.quanlymuontra.ui.tracuu.ChiTietLoaiThietBiViewModel.HanhDong;
import com.nhom13.quanlymuontra.ui.tracuu.ChiTietLoaiTrinhBay;
import java.util.ArrayList;
import java.util.List;
import java.util.Objects;

/**
 * SCR-SV-05 Xem lại và gửi phiếu yêu cầu (UI-Screens 5.5, Wireframes SCR-SV-05, FA 5.3 quy tắc 2, FA 3.11). {@link
 * SoanPhieuViewModel} lấy theo phạm vi nav graph con {@code soanPhieuGraph}, dùng chung với SCR-SV-04.
 *
 * <p>Chỉ hiển thị và chuyển sự kiện (FC 6.2). Nút Gửi bị vô hiệu ngay trong hàm xử lý bấm, trước khi {@code ViewModel}
 * phát yêu cầu. Gửi thành công chỉ hiện khối thông báo tạm kèm mã phiếu: SCR-SV-07 nối ở D12-L. Chưa xác định kết quả
 * chỉ hiện khối cảnh báo: sự kiện kiểm tra danh sách phiếu (SCR-SV-06) nối ở D12-L.
 */
public class XemLaiPhieuYeuCauFragment extends Fragment {

    private FragmentXemLaiPhieuYeuCauBinding _binding;
    private SoanPhieuViewModel viewModel;

    @NonNull
    private FragmentXemLaiPhieuYeuCauBinding binding() {
        return Objects.requireNonNull(_binding);
    }

    @Nullable
    @Override
    public View onCreateView(
            @NonNull LayoutInflater inflater,
            @Nullable ViewGroup container,
            @Nullable Bundle savedInstanceState) {
        _binding = FragmentXemLaiPhieuYeuCauBinding.inflate(inflater, container, false);
        return _binding.getRoot();
    }

    @Override
    public void onViewCreated(@NonNull View view, @Nullable Bundle savedInstanceState) {
        super.onViewCreated(view, savedInstanceState);
        UngDung ungDung = (UngDung) requireActivity().getApplication();
        final NavController dieuHuong = NavHostFragment.findNavController(this);
        // FC 6.3: ViewModel phạm vi nav graph con, dùng chung với SCR-SV-04.
        ViewModelStoreOwner chu = dieuHuong.getViewModelStoreOwner(R.id.soanPhieuGraph);
        viewModel =
                new ViewModelProvider(chu, new AppViewModelFactory(ungDung.getAppContainer()))
                        .get(SoanPhieuViewModel.class);

        // Khôi phục sau khi tiến trình bị hủy: không có nội dung phiếu thì quay về SCR-SV-04.
        if (viewModel.layTomTat() == null
                && viewModel.getTrangThaiGui().getValue() == TrangThaiGui.SAN_SANG) {
            view.post(
                    () -> {
                        if (isAdded()) {
                            dieuHuong.popBackStack();
                        }
                    });
            return;
        }

        binding().btnSua.setOnClickListener(v -> dieuHuong.popBackStack());
        binding().btnGui.setOnClickListener(v -> khiBamGui());

        viewModel.getTrangThaiGui().observe(getViewLifecycleOwner(), t -> hienTrangThai());
        viewModel.getLoiGui().observe(getViewLifecycleOwner(), l -> hienTrangThai());
        viewModel.getMaYeuCauVuaGui().observe(getViewLifecycleOwner(), m -> hienTrangThai());
        viewModel.getDieuHuongGui().observe(getViewLifecycleOwner(), this::xuLyDieuHuong);
    }

    @Override
    public void onDestroyView() {
        _binding = null;
        super.onDestroyView();
    }

    /** FC 6.2: vô hiệu nút ngay trong hàm xử lý bấm, trước khi ViewModel tạo lời gọi. */
    private void khiBamGui() {
        if (!binding().btnGui.isEnabled()) {
            return;
        }
        binding().btnGui.setEnabled(false);
        if (!viewModel.gui()) {
            binding().btnGui.setEnabled(true);
        }
    }

    private void hienTrangThai() {
        if (_binding == null) {
            return;
        }
        TrangThaiGui trangThai = viewModel.getTrangThaiGui().getValue();
        TomTatPhieu tomTat = viewModel.layTomTat();
        binding().cardTomTat.setVisibility(tomTat == null ? View.GONE : View.VISIBLE);
        if (tomTat != null) {
            dungTomTat(tomTat);
        }
        hienKhoiThongBao(trangThai, viewModel.getLoiGui().getValue());
        hienNut(trangThai);
    }

    private void dungTomTat(@NonNull TomTatPhieu tomTat) {
        String coSo = tomTat.getCoSo();
        String phong =
                coSo == null || coSo.isEmpty()
                        ? tomTat.getTenPhong()
                        : getString(R.string.tra_cuu_ghep_hai, tomTat.getTenPhong(), coSo);
        List<String> dong = new ArrayList<>();
        for (TomTatPhieu.Dong d : tomTat.getCacDong()) {
            dong.add(getString(R.string.xem_lai_dong_loai, d.getTenLoai(), d.getSoLuong()));
        }
        binding().kvlTomTat.xoa();
        binding().kvlTomTat.themDong(getString(R.string.xem_lai_nhan_phong), phong);
        binding().kvlTomTat.themDong(getString(R.string.xem_lai_nhan_loai), TextUtils.join("\n", dong));
        binding().kvlTomTat.themDong(
                getString(R.string.xem_lai_nhan_thoi_gian),
                getString(R.string.xem_lai_thoi_gian_gio, tomTat.getThoiGianGio()));
    }

    /** Khối thông báo ở đầu màn hình: thành công tạm, chưa xác định, lý do không cho gửi, lỗi tạm thời. */
    private void hienKhoiThongBao(@Nullable TrangThaiGui trangThai, @Nullable LoiGuiPhieu loi) {
        if (trangThai == TrangThaiGui.DA_GUI) {
            binding().bannerKetQua.hienThi(
                    Tong.SUCCESS,
                    getString(R.string.gui_thanh_cong_tieu_de),
                    getString(R.string.gui_thanh_cong_noi_dung, viewModel.getMaYeuCauVuaGui().getValue()),
                    null);
            return;
        }
        // Nhóm lỗi đưa về SCR-SV-04 hiển thị ở đó, không hiển thị ở đây.
        if (loi == null || TrinhBayLoiGuiPhieu.quayVeSua(loi.getPhanLoai())) {
            binding().bannerKetQua.an();
            return;
        }
        TrinhBayLoiGuiPhieu.PhanLoai phanLoai = loi.getPhanLoai();
        String noiDung = getString(TrinhBayLoiGuiPhieu.thongDiep(phanLoai));
        ApiError goc = loi.getLoi();
        String maTheoDoi = goc == null ? null : ChiTietLoaiTrinhBay.maTheoDoi(goc);
        if (maTheoDoi != null) {
            noiDung = noiDung + "\n" + getString(R.string.trang_thai_ma_theo_doi, maTheoDoi);
        }
        boolean canhBao =
                phanLoai == TrinhBayLoiGuiPhieu.PhanLoai.CHUA_XAC_DINH
                        || phanLoai == TrinhBayLoiGuiPhieu.PhanLoai.QUA_NHIEU_YEU_CAU;
        String tieuDe =
                phanLoai == TrinhBayLoiGuiPhieu.PhanLoai.CHUA_XAC_DINH
                        ? getString(R.string.gui_chua_xac_dinh_tieu_de)
                        : getString(R.string.gui_khong_the_gui_tieu_de);
        binding().bannerKetQua.hienThi(canhBao ? Tong.WARNING : Tong.DANGER, tieuDe, noiDung, null);
    }

    private void hienNut(@Nullable TrangThaiGui trangThai) {
        if (trangThai == TrangThaiGui.DA_GUI) {
            binding().llGhim.setVisibility(View.GONE);
            return;
        }
        binding().llGhim.setVisibility(View.VISIBLE);
        if (trangThai == TrangThaiGui.DANG_GUI) {
            NutDangXuLy.batDau(binding().btnGui);
            return;
        }
        NutDangXuLy.ketThuc(binding().btnGui, R.string.xem_lai_nut_gui);
        binding().btnGui.setEnabled(trangThai == TrangThaiGui.SAN_SANG);
    }

    private void xuLyDieuHuong(@NonNull Event<DieuHuongGui> suKien) {
        DieuHuongGui huong = suKien.getContentIfNotHandled();
        if (huong == null) {
            return;
        }
        NavController dieuHuong = NavHostFragment.findNavController(this);
        NavDestination diemDen = dieuHuong.getCurrentDestination();
        if (diemDen == null || diemDen.getId() != R.id.xemLaiPhieuYeuCauFragment) {
            return;
        }
        switch (huong) {
            case QUAY_VE_SUA:
                dieuHuong.popBackStack();
                break;
            case MO_HO_SO_BI_CHAN:
                dieuHuong.navigate(
                        XemLaiPhieuYeuCauFragmentDirections
                                .actionXemLaiPhieuYeuCauFragmentToHoSoMuonFragment()
                                .setCheDoBiChan(true)
                                .setHanhDongDaChon(HanhDong.DAT_TRUOC.getMaThamSo()));
                break;
            default:
                break;
        }
    }
}
