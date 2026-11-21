package com.nhom13.quanlymuontra.ui.hoso;

import android.os.Bundle;
import android.view.LayoutInflater;
import android.view.View;
import android.view.ViewGroup;
import android.widget.TextView;
import androidx.annotation.NonNull;
import androidx.annotation.Nullable;
import androidx.core.content.ContextCompat;
import androidx.fragment.app.Fragment;
import androidx.lifecycle.ViewModelProvider;
import com.nhom13.quanlymuontra.AppViewModelFactory;
import com.nhom13.quanlymuontra.R;
import com.nhom13.quanlymuontra.UngDung;
import com.nhom13.quanlymuontra.common.format.FormatSoLuong;
import com.nhom13.quanlymuontra.common.format.FormatThoiGian;
import com.nhom13.quanlymuontra.common.format.FormatTien;
import com.nhom13.quanlymuontra.common.nhan.HoTrangThai;
import com.nhom13.quanlymuontra.common.nhan.Tong;
import com.nhom13.quanlymuontra.common.widget.NutDangXuLy;
import com.nhom13.quanlymuontra.common.widget.StatusBadgeView;
import com.nhom13.quanlymuontra.data.model.ApiError;
import com.nhom13.quanlymuontra.data.model.UiState;
import com.nhom13.quanlymuontra.databinding.FragmentHoSoMuonBinding;
import com.nhom13.quanlymuontra.databinding.ItemBienBanChuaNopBinding;
import com.nhom13.quanlymuontra.generated.model.LyDo;
import com.nhom13.quanlymuontra.generated.model.SinhVienTheInfo;
import java.util.ArrayList;
import java.util.List;
import java.util.Objects;

/**
 * SCR-SV-14 Hồ sơ mượn, tab Hồ sơ (UI-Screens 5.14, Wireframes SCR-SV-14, UF-SV-08, FA 5.3).
 *
 * <p>Hiển thị đúng những gì máy chủ trả về qua {@code svToi}: không suy quyền mượn, không đọc {@code detail};
 * lý do không được mượn hiển thị nguyên {@code thongDiep}. Chế độ bị chặn ({@code cheDoBiChan}) thêm khối thông
 * báo nêu hành động vừa chọn không thực hiện được; màn hình không có nút gửi hành động đó. Không có nút mở khóa
 * hay gỡ cấm. Nút Đăng xuất không có hộp xác nhận (UI-Screens 4.1). Nút Quay lại ở thanh tiêu đề do {@code
 * MainActivity} quyết định theo tham số {@code coNutQuayLai} và {@code cheDoBiChan} (ĐX-WF-14).
 */
public class HoSoMuonFragment extends Fragment {

    private FragmentHoSoMuonBinding _binding;
    private HoSoMuonViewModel viewModel;
    private HoSoMuonFragmentArgs thamSo;

    @NonNull
    private FragmentHoSoMuonBinding binding() {
        return Objects.requireNonNull(_binding);
    }

    @Nullable
    @Override
    public View onCreateView(
            @NonNull LayoutInflater inflater,
            @Nullable ViewGroup container,
            @Nullable Bundle savedInstanceState) {
        _binding = FragmentHoSoMuonBinding.inflate(inflater, container, false);
        return _binding.getRoot();
    }

    @Override
    public void onViewCreated(@NonNull View view, @Nullable Bundle savedInstanceState) {
        super.onViewCreated(view, savedInstanceState);
        Bundle goiVao = getArguments() != null ? getArguments() : new Bundle();
        thamSo = HoSoMuonFragmentArgs.fromBundle(goiVao);
        UngDung ungDung = (UngDung) requireActivity().getApplication();
        viewModel =
                new ViewModelProvider(this, new AppViewModelFactory(ungDung.getAppContainer()))
                        .get(HoSoMuonViewModel.class);

        binding().srlLamMoi.setColorSchemeColors(
                ContextCompat.getColor(requireContext(), R.color.ds_color_progress_indicator));
        binding().srlLamMoi.setProgressBackgroundColorSchemeColor(
                ContextCompat.getColor(requireContext(), R.color.ds_color_surface_default));
        binding().srlLamMoi.setOnRefreshListener(() -> viewModel.refresh());
        // Chống bấm trùng: ViewModel bật cờ đồng bộ rồi trạng thái "đang xử lý" của nút do dangXuat quan sát (FA 3.11).
        binding().btnDangXuat.setOnClickListener(nut -> viewModel.dangXuat());

        viewModel.getUiState().observe(getViewLifecycleOwner(), this::hienThi);
        viewModel.getDangXuat().observe(getViewLifecycleOwner(), this::hienTrangThaiDangXuat);
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

    private void hienTrangThaiDangXuat(boolean dangXuat) {
        if (dangXuat) {
            NutDangXuLy.batDau(binding().btnDangXuat);
        } else {
            NutDangXuLy.ketThuc(binding().btnDangXuat, R.string.ho_so_dang_xuat);
            binding().btnDangXuat.setEnabled(true);
        }
    }

    private void hienThi(@NonNull UiState<SinhVienTheInfo> trangThai) {
        SinhVienTheInfo hoSo = trangThai.getDuLieu();
        boolean dangTai = trangThai.getTrangThai() == UiState.TrangThai.DANG_TAI;
        ApiError loi = trangThai.getLoi();
        boolean coLoi = trangThai.getTrangThai() == UiState.TrangThai.LOI && loi != null;

        binding().srlLamMoi.setRefreshing(dangTai && hoSo != null);
        hienKhoiBiChan(hoSo);

        binding().lvTai.setVisibility(dangTai && hoSo == null ? View.VISIBLE : View.GONE);
        if (coLoi) {
            binding().evLoi.hienThi(loi, nut -> viewModel.refresh());
        } else {
            binding().evLoi.setVisibility(View.GONE);
        }
        if (!coLoi && hoSo != null) {
            dungNoiDung(hoSo);
            binding().llNoiDung.setVisibility(View.VISIBLE);
        } else {
            binding().llNoiDung.setVisibility(View.GONE);
        }
    }

    /** Chế độ bị chặn (UI-Screens 5.14): nêu hành động vừa chọn không thực hiện được kèm các lý do máy chủ trả. */
    private void hienKhoiBiChan(@Nullable SinhVienTheInfo hoSo) {
        if (!thamSo.getCheDoBiChan()) {
            binding().bannerBiChan.an();
            return;
        }
        List<String> dsLyDo = new ArrayList<>();
        if (hoSo != null && hoSo.getDsLyDoKhongHopLe() != null) {
            for (LyDo lyDo : hoSo.getDsLyDoKhongHopLe()) {
                String thongDiep = lyDo.getThongDiep();
                if (thongDiep != null && !thongDiep.isEmpty()) {
                    dsLyDo.add(thongDiep);
                }
            }
        }
        binding().bannerBiChan.hienThi(
                Tong.DANGER,
                getString(HoSoMuonTrinhBay.tieuDeBiChan(thamSo.getHanhDongDaChon())),
                null,
                dsLyDo);
    }

    private void dungNoiDung(@NonNull SinhVienTheInfo hoSo) {
        FragmentHoSoMuonBinding b = binding();

        // Thông tin sinh viên (CMP-14): mã dùng kiểu chữ mã, ngày theo DS 9.7, trạng thái qua StatusBadgeView.
        b.kvlThongTin.xoa();
        b.kvlThongTin.themDongMa(getString(R.string.ho_so_ma_sv), hoSo.getMaSV());
        b.kvlThongTin.themDong(getString(R.string.ho_so_ho_ten), hoSo.getHoTen());
        b.kvlThongTin.themDong(getString(R.string.ho_so_lop), hoSo.getLop());
        b.kvlThongTin.themDong(getString(R.string.ho_so_khoa), hoSo.getKhoa());
        b.kvlThongTin.themDong(getString(R.string.ho_so_nhom), hoSo.getTenNhom());
        b.kvlThongTin.themDong(getString(R.string.ho_so_han_the), FormatThoiGian.ngay(hoSo.getHanThe()));
        StatusBadgeView trangThaiHoc = new StatusBadgeView(requireContext());
        trangThaiHoc.setTrangThai(HoTrangThai.TRANG_THAI_HOC, hoSo.getTrangThaiHoc());
        b.kvlThongTin.themDongView(getString(R.string.ho_so_trang_thai_hoc), trangThaiHoc);

        // Quyền mượn: nhãn và số thiết bị lấy nguyên từ máy chủ.
        b.kvlQuyenMuon.xoa();
        StatusBadgeView quyenMuon = new StatusBadgeView(requireContext());
        quyenMuon.setTrangThai(HoTrangThai.QUYEN_MUON, hoSo.getTrangThaiQuyenMuon());
        b.kvlQuyenMuon.themDongView(getString(R.string.ho_so_quyen_muon_trang_thai), quyenMuon);
        b.kvlQuyenMuon.themDong(
                getString(R.string.ho_so_so_thiet_bi),
                FormatSoLuong.tren(hoSo.getSoThietBiDangMuon(), hoSo.getSoThietBiToiDa()));

        dungBienBan(hoSo);
        dungLyDo(hoSo);

        int huongDan = HoSoMuonTrinhBay.huongDan(hoSo.getTrangThaiQuyenMuon());
        if (huongDan == 0) {
            b.cardHuongDan.setVisibility(View.GONE);
        } else {
            b.tvHuongDan.setText(huongDan);
            b.cardHuongDan.setVisibility(View.VISIBLE);
        }
    }

    /** Biên bản chưa nộp: ẩn cả khối khi rỗng. Liên kết tới SCR-SV-11 chưa có đích nên chưa thêm. */
    private void dungBienBan(@NonNull SinhVienTheInfo hoSo) {
        FragmentHoSoMuonBinding b = binding();
        b.llBienBan.removeAllViews();
        boolean coBienBan = false;
        if (hoSo.getDsBienBanChuaNop() != null) {
            for (var bienBan : hoSo.getDsBienBanChuaNop()) {
                ItemBienBanChuaNopBinding dong =
                        ItemBienBanChuaNopBinding.inflate(getLayoutInflater(), b.llBienBan, true);
                dong.tvMa.setText(bienBan.getMaBienBan());
                dong.tvChiTiet.setText(
                        getString(
                                R.string.ho_so_bien_ban_chi_tiet,
                                FormatTien.tien(bienBan.getTongTien()),
                                FormatThoiGian.ngayGio(bienBan.getNgayLap())));
                coBienBan = true;
            }
        }
        b.cardBienBan.setVisibility(coBienBan ? View.VISIBLE : View.GONE);
    }

    /** Lý do không được mượn: chỉ khi {@code hopLe} là false; nguyên văn {@code thongDiep}, không đọc {@code detail}. */
    private void dungLyDo(@NonNull SinhVienTheInfo hoSo) {
        FragmentHoSoMuonBinding b = binding();
        b.llLyDo.removeAllViews();
        int soLyDo = 0;
        if (Boolean.FALSE.equals(hoSo.getHopLe()) && hoSo.getDsLyDoKhongHopLe() != null) {
            for (LyDo lyDo : hoSo.getDsLyDoKhongHopLe()) {
                String thongDiep = lyDo.getThongDiep();
                if (thongDiep == null || thongDiep.isEmpty()) {
                    continue;
                }
                TextView dong = new TextView(requireContext());
                dong.setTextAppearance(R.style.TextAppearance_Ds_Body);
                dong.setText(getString(R.string.ho_so_ly_do_dong, thongDiep));
                b.llLyDo.addView(dong);
                soLyDo++;
            }
        }
        b.cardLyDo.setVisibility(soLyDo > 0 ? View.VISIBLE : View.GONE);
    }
}
