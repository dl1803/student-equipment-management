package com.nhom13.quanlymuontra.common.widget;

import android.app.Dialog;
import android.graphics.Color;
import android.graphics.drawable.ColorDrawable;
import android.os.Bundle;
import android.view.LayoutInflater;
import android.view.View;
import android.view.ViewGroup;
import android.view.Window;
import android.view.WindowManager;
import androidx.activity.ComponentDialog;
import androidx.activity.OnBackPressedCallback;
import androidx.annotation.NonNull;
import androidx.annotation.Nullable;
import androidx.fragment.app.DialogFragment;
import com.google.android.material.button.MaterialButton;
import com.nhom13.quanlymuontra.R;
import com.nhom13.quanlymuontra.common.nhan.Tong;
import com.nhom13.quanlymuontra.databinding.DialogXacNhanBinding;
import java.util.Objects;

/**
 * CMP-26 Hộp xác nhận (DS CMP-26 và 9.2, UI-Screens 3.5): tiêu đề là tên hành động, tóm tắt đối tượng bị
 * tác động, hệ quả, tùy chọn một khối nhấn mạnh (CMP-24 {@code warning}), nút Hủy và nút xác nhận mang tên
 * hành động cụ thể (không dùng "OK", "Đồng ý" hay "Xác nhận" chung chung).
 *
 * <p>Hành vi: không đóng khi chạm ra ngoài; tiêu điểm ban đầu ở nút Hủy; Back có tác dụng như Hủy; nút
 * xác nhận kiểu hủy bỏ ({@code laHuyBo}) khi hành động xóa, ngừng, hủy, mất vị trí hoặc không hoàn tác được
 * (DS 9.2). Khi người dùng bấm nút xác nhận, hộp gửi kết quả rồi <b>ở lại</b> trong trạng thái đang xử lý
 * (nút xác nhận hiển thị "Đang xử lý", nút Hủy và Back bị khóa) cho tới khi bên gọi gọi {@link
 * #datDangXuLy(boolean)} với {@code false} (khi lỗi, để người dùng thử lại hoặc hủy) hoặc {@link
 * #dismiss()} (khi thành công).
 *
 * <p>Cách dùng: hiển thị bằng {@code show(getChildFragmentManager(), TAG)} của màn hình để hộp bị hủy cùng
 * màn hình khi phiên kết thúc; nhận kết quả bằng {@code getChildFragmentManager().setFragmentResultListener(
 * khoaKetQua, getViewLifecycleOwner(), ...)} và kiểm {@code bundle.getBoolean(KEY_XAC_NHAN)}.
 *
 * <p>Xử lý xoay màn hình khi đang xử lý: trạng thái đang xử lý được lưu và phục hồi qua xoay màn hình. Khi
 * ViewModel hoặc thao tác nền hoàn tất sau xoay, màn hình gọi lấy lại hộp bằng {@code (XacNhanDialogFragment)
 * getChildFragmentManager().findFragmentByTag(TAG)} rồi gọi {@code hop.dismiss()} (khi thành công) hoặc
 * {@code hop.datDangXuLy(false)} (khi lỗi, để người dùng thử lại hoặc hủy), tránh kẹt trạng thái đang xử lý.
 * Bên gọi tự bảo đảm không gửi trùng yêu cầu ghi (FA 3.11).
 */
public class XacNhanDialogFragment extends DialogFragment {

    /** Khóa trong {@code Bundle} kết quả: {@code true} khi người dùng bấm nút xác nhận. */
    public static final String KEY_XAC_NHAN = "xacNhan";

    private static final String ARG_KHOA_KET_QUA = "khoaKetQua";
    private static final String ARG_TIEU_DE = "tieuDe";
    private static final String ARG_TOM_TAT = "tomTat";
    private static final String ARG_HE_QUA = "heQua";
    private static final String ARG_NHAN_XAC_NHAN = "nhanXacNhan";
    private static final String ARG_LA_HUY_BO = "laHuyBo";
    private static final String ARG_NHAN_MANH = "nhanManh";
    private static final String STATE_DANG_XU_LY = "dangXuLy";

    private DialogXacNhanBinding _binding;
    private MaterialButton nutXacNhan;
    private boolean dangXuLy;

    /**
     * @param khoaKetQua khóa dùng để nhận kết quả bằng Fragment Result API
     * @param tieuDe tên hành động
     * @param tomTat đối tượng bị tác động
     * @param heQua hệ quả của hành động
     * @param nhanXacNhan tên hành động cụ thể trên nút xác nhận
     * @param laHuyBo {@code true} thì nút xác nhận dùng kiểu hủy bỏ, {@code false} thì kiểu chính
     * @param nhanManh câu nhấn mạnh hệ quả quan trọng hiển thị trong khối {@code warning}; {@code null}
     *     nếu không có
     */
    @NonNull
    public static XacNhanDialogFragment tao(
            @NonNull String khoaKetQua,
            @NonNull String tieuDe,
            @NonNull String tomTat,
            @NonNull String heQua,
            @NonNull String nhanXacNhan,
            boolean laHuyBo,
            @Nullable String nhanManh) {
        Bundle thamSo = new Bundle();
        thamSo.putString(ARG_KHOA_KET_QUA, khoaKetQua);
        thamSo.putString(ARG_TIEU_DE, tieuDe);
        thamSo.putString(ARG_TOM_TAT, tomTat);
        thamSo.putString(ARG_HE_QUA, heQua);
        thamSo.putString(ARG_NHAN_XAC_NHAN, nhanXacNhan);
        thamSo.putBoolean(ARG_LA_HUY_BO, laHuyBo);
        thamSo.putString(ARG_NHAN_MANH, nhanManh);
        XacNhanDialogFragment hop = new XacNhanDialogFragment();
        hop.setArguments(thamSo);
        return hop;
    }

    /** Bật hoặc tắt trạng thái đang xử lý; có thể gọi trước khi hộp hiển thị. */
    public void datDangXuLy(boolean dang) {
        dangXuLy = dang;
        if (_binding != null && nutXacNhan != null) {
            apDungTrangThai();
        }
    }

    @Override
    public void onCreate(@Nullable Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);
        if (savedInstanceState != null) {
            dangXuLy = savedInstanceState.getBoolean(STATE_DANG_XU_LY);
        }
    }

    @Override
    public void onSaveInstanceState(@NonNull Bundle outState) {
        super.onSaveInstanceState(outState);
        outState.putBoolean(STATE_DANG_XU_LY, dangXuLy);
    }

    @NonNull
    @Override
    public Dialog onCreateDialog(@Nullable Bundle savedInstanceState) {
        Dialog hop = super.onCreateDialog(savedInstanceState);
        hop.setCanceledOnTouchOutside(false);
        if (hop instanceof ComponentDialog) {
            // Back (cả cử chỉ quay lại dự đoán) có tác dụng như Hủy, trừ khi đang xử lý thì bị khóa.
            ((ComponentDialog) hop)
                    .getOnBackPressedDispatcher()
                    .addCallback(
                            this,
                            new OnBackPressedCallback(true) {
                                @Override
                                public void handleOnBackPressed() {
                                    if (!dangXuLy) {
                                        dismiss();
                                    }
                                }
                            });
        }
        return hop;
    }

    @NonNull
    private DialogXacNhanBinding binding() {
        return Objects.requireNonNull(_binding);
    }

    @Nullable
    @Override
    public View onCreateView(
            @NonNull LayoutInflater inflater,
            @Nullable ViewGroup container,
            @Nullable Bundle savedInstanceState) {
        _binding = DialogXacNhanBinding.inflate(inflater, container, false);
        return _binding.getRoot();
    }

    @Override
    public void onViewCreated(@NonNull View view, @Nullable Bundle savedInstanceState) {
        super.onViewCreated(view, savedInstanceState);
        Bundle thamSo = requireArguments();

        binding().tvTieuDe.setText(thamSo.getString(ARG_TIEU_DE));
        binding().tvTomTat.setText(thamSo.getString(ARG_TOM_TAT));
        binding().tvHeQua.setText(thamSo.getString(ARG_HE_QUA));
        String nhanManh = thamSo.getString(ARG_NHAN_MANH);
        if (nhanManh != null && !nhanManh.isEmpty()) {
            binding().bannerNhanManh.hienThi(Tong.WARNING, null, nhanManh, null);
            binding().bannerNhanManh.setVisibility(View.VISIBLE);
        }

        // DS 9.2: chọn kiểu nút xác nhận; bản còn lại ẩn.
        boolean laHuyBo = thamSo.getBoolean(ARG_LA_HUY_BO);
        nutXacNhan = laHuyBo ? binding().btnXacNhanHuyBo : binding().btnXacNhan;
        (laHuyBo ? binding().btnXacNhan : binding().btnXacNhanHuyBo).setVisibility(View.GONE);
        nutXacNhan.setVisibility(View.VISIBLE);
        nutXacNhan.setText(thamSo.getString(ARG_NHAN_XAC_NHAN));

        // Rộng bằng màn hình trừ lề ds_space_6 mỗi bên (đã là padding của lớp nền), tối đa ds_size_dialog_max.
        int toiDa = getResources().getDimensionPixelSize(R.dimen.ds_size_dialog_max);
        int le = getResources().getDimensionPixelSize(R.dimen.ds_space_6);
        int rongKhaDung = getResources().getDisplayMetrics().widthPixels - 2 * le;
        ViewGroup.LayoutParams thongSoHop = binding().llHop.getLayoutParams();
        thongSoHop.width = Math.min(rongKhaDung, toiDa);
        binding().llHop.setLayoutParams(thongSoHop);

        binding().btnHuy.setOnClickListener(nut -> dismiss());
        nutXacNhan.setOnClickListener(
                nut -> {
                    if (dangXuLy) {
                        return;
                    }
                    datDangXuLy(true);
                    Bundle ketQua = new Bundle();
                    ketQua.putBoolean(KEY_XAC_NHAN, true);
                    getParentFragmentManager()
                            .setFragmentResult(thamSo.getString(ARG_KHOA_KET_QUA), ketQua);
                });

        apDungTrangThai();
        if (!dangXuLy) {
            // Tiêu điểm ban đầu ở nút Hủy (DS CMP-26).
            binding().btnHuy.requestFocus();
        }
    }

    @Override
    public void onStart() {
        super.onStart();
        Dialog hop = getDialog();
        if (hop == null || hop.getWindow() == null) {
            return;
        }
        // Lớp nền phủ do dialog_xac_nhan.xml vẽ bằng color.overlay.scrim nên bỏ nền và độ mờ mặc định.
        Window cuaSo = hop.getWindow();
        cuaSo.setBackgroundDrawable(new ColorDrawable(Color.TRANSPARENT));
        cuaSo.clearFlags(WindowManager.LayoutParams.FLAG_DIM_BEHIND);
        cuaSo.setLayout(ViewGroup.LayoutParams.MATCH_PARENT, ViewGroup.LayoutParams.MATCH_PARENT);
    }

    @Override
    public void onDestroyView() {
        super.onDestroyView();
        _binding = null;
        nutXacNhan = null;
    }

    private void apDungTrangThai() {
        binding().btnHuy.setEnabled(!dangXuLy);
        if (dangXuLy) {
            NutDangXuLy.batDau(nutXacNhan);
        } else {
            NutDangXuLy.ketThuc(nutXacNhan, requireArguments().getString(ARG_NHAN_XAC_NHAN, ""));
            nutXacNhan.setEnabled(true);
        }
    }
}
