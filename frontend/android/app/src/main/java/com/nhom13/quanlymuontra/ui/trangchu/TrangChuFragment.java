package com.nhom13.quanlymuontra.ui.trangchu;

import android.os.Bundle;
import android.view.LayoutInflater;
import android.view.View;
import android.view.ViewGroup;
import android.widget.LinearLayout;
import androidx.annotation.NonNull;
import androidx.annotation.Nullable;
import androidx.core.content.ContextCompat;
import androidx.fragment.app.Fragment;
import androidx.lifecycle.ViewModelProvider;
import androidx.navigation.NavController;
import androidx.navigation.fragment.NavHostFragment;
import com.nhom13.quanlymuontra.AppViewModelFactory;
import com.nhom13.quanlymuontra.R;
import com.nhom13.quanlymuontra.UngDung;
import com.nhom13.quanlymuontra.common.format.FormatSoLuong;
import com.nhom13.quanlymuontra.common.nhan.HoTrangThai;
import com.nhom13.quanlymuontra.common.nhan.NhanTrangThai;
import com.nhom13.quanlymuontra.common.nhan.QuyenMuon;
import com.nhom13.quanlymuontra.common.widget.TheTong;
import com.nhom13.quanlymuontra.data.model.ApiError;
import com.nhom13.quanlymuontra.data.model.UiState;
import com.nhom13.quanlymuontra.databinding.FragmentTrangChuBinding;
import com.nhom13.quanlymuontra.databinding.ItemLoiVaoBinding;
import com.nhom13.quanlymuontra.generated.model.SinhVienTheInfo;
import java.util.Objects;

/**
 * SCR-SV-01 Trang chủ sinh viên, tab Trang chủ (UI-Screens 5.1, Wireframes SCR-SV-01, FA 5.3).
 *
 * <p>Lỗi hoặc đang tải hồ sơ chỉ ảnh hưởng thẻ quyền mượn; lời chào và tám lối vào luôn dùng được
 * (ĐX-UI-04). Quyền mượn lấy nguyên từ máy chủ qua {@code svToi}, không suy ra ở client. Mọi quyết định tải
 * nằm ở {@link TrangChuViewModel}; lớp này chỉ hiển thị và chuyển sự kiện (FC 6.2).
 *
 * <p>Chỉ hai lối vào có đích trong nav graph hiện tại (Tra cứu thiết bị, Hồ sơ mượn). Các lối vào còn lại hiển
 * thị đủ nhưng chưa nối; mỗi ô được nối ở ngày của màn hình đích (CHECKLIST mục 12).
 */
public class TrangChuFragment extends Fragment {

    private FragmentTrangChuBinding _binding;
    private TrangChuViewModel viewModel;

    @NonNull
    private FragmentTrangChuBinding binding() {
        return Objects.requireNonNull(_binding);
    }

    @Nullable
    @Override
    public View onCreateView(
            @NonNull LayoutInflater inflater,
            @Nullable ViewGroup container,
            @Nullable Bundle savedInstanceState) {
        _binding = FragmentTrangChuBinding.inflate(inflater, container, false);
        return _binding.getRoot();
    }

    @Override
    public void onViewCreated(@NonNull View view, @Nullable Bundle savedInstanceState) {
        super.onViewCreated(view, savedInstanceState);
        UngDung ungDung = (UngDung) requireActivity().getApplication();
        viewModel =
                new ViewModelProvider(this, new AppViewModelFactory(ungDung.getAppContainer()))
                        .get(TrangChuViewModel.class);

        caiLamMoi();
        dungLoiVao();
        binding().btnXemLyDo.setOnClickListener(
                nut ->
                        NavHostFragment.findNavController(this)
                                .navigate(
                                        TrangChuFragmentDirections
                                                .actionTrangChuFragmentToHoSoMuonFragment()
                                                .setCoNutQuayLai(true)));

        viewModel.getUiState().observe(getViewLifecycleOwner(), this::hienThi);
        viewModel.load();
    }

    @Override
    public void onDestroyView() {
        // Xoay màn hình giữ dữ liệu; rời màn hình thì lần sau phải tải lại từ máy chủ (FA 5.6, ĐX-FC-22).
        if (!requireActivity().isChangingConfigurations()) {
            viewModel.danhDauCanTaiLai();
        }
        _binding = null;
        super.onDestroyView();
    }

    private void caiLamMoi() {
        binding().srlLamMoi.setColorSchemeColors(
                ContextCompat.getColor(requireContext(), R.color.ds_color_progress_indicator));
        binding().srlLamMoi.setProgressBackgroundColorSchemeColor(
                ContextCompat.getColor(requireContext(), R.color.ds_color_surface_default));
        binding().srlLamMoi.setOnRefreshListener(() -> viewModel.refresh());
    }

    /** Dựng lưới hai cột gồm tám ô theo thứ tự của {@link LoiVaoTrangChu}. */
    private void dungLoiVao() {
        LayoutInflater inflater = getLayoutInflater();
        LinearLayout khung = binding().llLoiVao;
        int nuaKhoang = getResources().getDimensionPixelSize(R.dimen.ds_space_1);
        LoiVaoTrangChu[] dsLoiVao = LoiVaoTrangChu.values();
        for (int dau = 0; dau < dsLoiVao.length; dau += 2) {
            LinearLayout hang = new LinearLayout(requireContext());
            hang.setOrientation(LinearLayout.HORIZONTAL);
            khung.addView(
                    hang,
                    new LinearLayout.LayoutParams(
                            ViewGroup.LayoutParams.MATCH_PARENT,
                            ViewGroup.LayoutParams.WRAP_CONTENT));
            for (int i = dau; i < dau + 2 && i < dsLoiVao.length; i++) {
                LoiVaoTrangChu loiVao = dsLoiVao[i];
                ItemLoiVaoBinding o = ItemLoiVaoBinding.inflate(inflater, hang, false);
                o.tvNhan.setText(loiVao.getNhanResId());
                o.getRoot().setOnClickListener(nut -> moLoiVao(loiVao));
                LinearLayout.LayoutParams tham = (LinearLayout.LayoutParams) o.getRoot().getLayoutParams();
                if (i % 2 == 0) {
                    tham.setMarginEnd(nuaKhoang);
                } else {
                    tham.setMarginStart(nuaKhoang);
                }
                hang.addView(o.getRoot(), tham);
            }
        }
    }

    private void moLoiVao(@NonNull LoiVaoTrangChu loiVao) {
        NavController dieuHuong = NavHostFragment.findNavController(this);
        switch (loiVao) {
            case TRA_CUU:
                dieuHuong.navigate(
                        TrangChuFragmentDirections.actionTrangChuFragmentToTraCuuThietBiFragment());
                break;
            case HO_SO:
                dieuHuong.navigate(
                        TrangChuFragmentDirections.actionTrangChuFragmentToHoSoMuonFragment());
                break;
            default:
                // Đích của các lối vào còn lại chưa có trong nav graph; nối ở ngày của màn hình đích
                // (Đặt trước: D10-L kèm kiểm quyền mượn, FA 5.3 quy tắc 3). Chưa làm gì.
                break;
        }
    }

    private void hienThi(@NonNull UiState<SinhVienTheInfo> trangThai) {
        SinhVienTheInfo hoSo = trangThai.getDuLieu();
        boolean dangTai = trangThai.getTrangThai() == UiState.TrangThai.DANG_TAI;
        ApiError loi = trangThai.getLoi();
        boolean coLoi = trangThai.getTrangThai() == UiState.TrangThai.LOI && loi != null;

        binding().srlLamMoi.setRefreshing(dangTai && hoSo != null);

        if (hoSo == null) {
            binding().tvLoiChao.setVisibility(View.GONE);
        } else {
            binding().tvLoiChao.setText(getString(R.string.trang_chu_loi_chao, hoSo.getHoTen()));
            binding().tvLoiChao.setVisibility(View.VISIBLE);
        }

        binding().lvTheQuyenMuon.setVisibility(dangTai && hoSo == null ? View.VISIBLE : View.GONE);
        if (coLoi) {
            binding().evTheQuyenMuon.hienThi(loi, nut -> viewModel.refresh());
        } else {
            binding().evTheQuyenMuon.setVisibility(View.GONE);
        }
        if (!coLoi && hoSo != null) {
            hienThe(hoSo);
        } else {
            binding().cardQuyenMuon.setVisibility(View.GONE);
        }
    }

    private void hienThe(@NonNull SinhVienTheInfo hoSo) {
        FragmentTrangChuBinding b = binding();
        String quyenMuon = hoSo.getTrangThaiQuyenMuon();
        boolean binhThuong = QuyenMuon.laBinhThuong(quyenMuon);

        b.sbQuyenMuon.setTrangThai(HoTrangThai.QUYEN_MUON, quyenMuon);
        if (binhThuong) {
            TheTong.datMacDinh(b.cardQuyenMuon);
        } else {
            // Tông của thẻ là tông của nhãn quyền mượn (DS 7.2), không tự suy ở client.
            TheTong.apDung(
                    b.cardQuyenMuon,
                    NhanTrangThai.thongTin(HoTrangThai.QUYEN_MUON, quyenMuon).getTong());
        }
        b.tvQuyenMuonNhan.setVisibility(binhThuong ? View.VISIBLE : View.GONE);
        b.tvDangMuon.setVisibility(binhThuong ? View.VISIBLE : View.GONE);
        b.tvDangMuon.setText(
                getString(
                        R.string.trang_chu_dang_muon,
                        FormatSoLuong.tren(hoSo.getSoThietBiDangMuon(), hoSo.getSoThietBiToiDa())));
        b.tvKhongTheDatTruoc.setVisibility(binhThuong ? View.GONE : View.VISIBLE);
        b.btnXemLyDo.setVisibility(binhThuong ? View.GONE : View.VISIBLE);
        b.cardQuyenMuon.setVisibility(View.VISIBLE);
    }
}
