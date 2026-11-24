package com.nhom13.quanlymuontra.ui.dangnhap;

import android.os.Bundle;
import android.text.Editable;
import android.text.TextWatcher;
import android.view.LayoutInflater;
import android.view.View;
import android.view.ViewGroup;
import androidx.annotation.NonNull;
import androidx.annotation.Nullable;
import androidx.fragment.app.Fragment;
import androidx.lifecycle.ViewModelProvider;
import androidx.navigation.fragment.NavHostFragment;
import com.nhom13.quanlymuontra.AppViewModelFactory;
import com.nhom13.quanlymuontra.R;
import com.nhom13.quanlymuontra.UngDung;
import com.nhom13.quanlymuontra.common.widget.NutDangXuLy;
import com.nhom13.quanlymuontra.data.model.ApiError;
import com.nhom13.quanlymuontra.databinding.FragmentDangNhapSinhVienBinding;
import java.util.Objects;

/**
 * SCR-CM-01 Đăng nhập sinh viên (UI-Screens 4.4). Hai ô bắt buộc; nút Đăng nhập vô hiệu khi thiếu trường
 * hoặc đang chờ phản hồi. Không có liên kết đăng ký, quên hay đổi mật khẩu. Mọi quyết định nằm ở {@link
 * DangNhapSinhVienViewModel}; lớp này chỉ hiển thị và chuyển sự kiện.
 */
public class DangNhapSinhVienFragment extends Fragment {

    private FragmentDangNhapSinhVienBinding _binding;
    private DangNhapSinhVienViewModel viewModel;
    private boolean dangGui;

    @NonNull
    private FragmentDangNhapSinhVienBinding binding() {
        return Objects.requireNonNull(_binding);
    }

    @Nullable
    @Override
    public View onCreateView(
            @NonNull LayoutInflater inflater,
            @Nullable ViewGroup container,
            @Nullable Bundle savedInstanceState) {
        _binding = FragmentDangNhapSinhVienBinding.inflate(inflater, container, false);
        return _binding.getRoot();
    }

    @Override
    public void onViewCreated(@NonNull View view, @Nullable Bundle savedInstanceState) {
        super.onViewCreated(view, savedInstanceState);
        UngDung ungDung = (UngDung) requireActivity().getApplication();
        viewModel =
                new ViewModelProvider(this, new AppViewModelFactory(ungDung.getAppContainer()))
                        .get(DangNhapSinhVienViewModel.class);

        binding().etMssv.addTextChangedListener(
                new TextWatcher() {
                    @Override
                    public void beforeTextChanged(CharSequence s, int start, int count, int after) {}

                    @Override
                    public void onTextChanged(CharSequence s, int start, int before, int count) {}

                    @Override
                    public void afterTextChanged(Editable s) {
                        viewModel.xoaLoiDangNhapSai();
                        binding().tilMssv.setError(null);
                        capNhatNutDangNhap();
                    }
                });
        binding().etMatKhau.addTextChangedListener(
                new TextWatcher() {
                    @Override
                    public void beforeTextChanged(CharSequence s, int start, int count, int after) {}

                    @Override
                    public void onTextChanged(CharSequence s, int start, int before, int count) {}

                    @Override
                    public void afterTextChanged(Editable s) {
                        viewModel.xoaLoiDangNhapSai();
                        binding().tilMatKhau.setError(null);
                        capNhatNutDangNhap();
                    }
                });
        binding().btnDangNhap.setOnClickListener(
                nut -> {
                    // Chống gửi trùng: ViewModel bật cờ dangGui đồng bộ trước khi tạo yêu cầu, kích hoạt trạng thái đang xử lý (FA 3.11, ĐX-FA-22).
                    viewModel.dangNhap(
                            binding().etMssv.getText() != null ? binding().etMssv.getText().toString().trim() : "",
                            binding().etMatKhau.getText() != null ? binding().etMatKhau.getText().toString() : "");
                });

        viewModel.getDangGui().observe(getViewLifecycleOwner(), this::hienTrangThaiGui);
        viewModel.getLoiHienThi().observe(getViewLifecycleOwner(), this::hienLoi);
        viewModel
                .getXoaMatKhau()
                .observe(
                        getViewLifecycleOwner(),
                        suKien -> {
                            if (suKien.getContentIfNotHandled() != null) {
                                binding().etMatKhau.setText("");
                            }
                        });
        viewModel
                .getDaDangNhap()
                .observe(
                        getViewLifecycleOwner(),
                        suKien -> {
                            if (suKien.getContentIfNotHandled() != null) {
                                NavHostFragment.findNavController(this)
                                        .navigate(
                                                DangNhapSinhVienFragmentDirections
                                                        .actionDangNhapSinhVienFragmentToTrangChuFragment());
                            }
                        });
        capNhatNutDangNhap();
    }

    @Override
    public void onDestroyView() {
        super.onDestroyView();
        _binding = null;
    }

    private void hienTrangThaiGui(boolean dangGuiMoi) {
        dangGui = dangGuiMoi;
        binding().tilMssv.setEnabled(!dangGuiMoi);
        binding().tilMatKhau.setEnabled(!dangGuiMoi);
        if (dangGuiMoi) {
            NutDangXuLy.batDau(binding().btnDangNhap);
        } else {
            NutDangXuLy.ketThuc(binding().btnDangNhap, R.string.dang_nhap_hanh_dong);
            capNhatNutDangNhap();
        }
    }

    /** Nút bật khi cả hai ô có nội dung (MSSV sau khi cắt khoảng trắng không rỗng) và không đang chờ phản hồi (UI-Screens 4.4). */
    private void capNhatNutDangNhap() {
        if (dangGui) {
            return;
        }
        boolean duTruong =
                binding().etMssv.getText() != null
                        && binding().etMssv.getText().toString().trim().length() > 0
                        && binding().etMatKhau.length() > 0;
        binding().btnDangNhap.setEnabled(duTruong);
    }

    private void hienLoi(@Nullable ApiError loi) {
        binding().tilMssv.setError(null);
        binding().tilMatKhau.setError(null);
        binding().bannerDangNhap.an();
        if (loi == null) {
            return;
        }
        TrinhBayLoiDangNhap.NoiDung noiDung = TrinhBayLoiDangNhap.cho(loi);
        if (noiDung == null) {
            return;
        }
        String thongBao = noiDung.thongDiepTuyChinh != null
                ? noiDung.thongDiepTuyChinh
                : getString(noiDung.thongDiep);
        if (noiDung.viTri == TrinhBayLoiDangNhap.ViTri.DUOI_O_MSSV) {
            binding().tilMssv.setError(thongBao);
            binding().etMssv.requestFocus();
            return;
        }
        if (noiDung.viTri == TrinhBayLoiDangNhap.ViTri.DUOI_O_MAT_KHAU) {
            binding().tilMatKhau.setError(thongBao);
            binding().etMatKhau.requestFocus();
            return;
        }
        String chu = thongBao;
        if (noiDung.traceId != null && !noiDung.traceId.isEmpty()) {
            chu = chu + "\n" + getString(R.string.trang_thai_ma_theo_doi, noiDung.traceId);
        }
        binding().bannerDangNhap.hienThi(noiDung.tong, null, chu, null);
    }
}
