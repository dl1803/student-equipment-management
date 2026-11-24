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
import androidx.annotation.NonNull;
import androidx.annotation.Nullable;
import androidx.fragment.app.DialogFragment;
import com.nhom13.quanlymuontra.R;
import com.nhom13.quanlymuontra.databinding.DialogChonMotBinding;
import com.nhom13.quanlymuontra.databinding.ItemLuaChonBinding;
import java.util.Objects;

/**
 * CMP-06 danh sách chọn một ở dạng hộp thoại (DS CMP-06). Mỗi mục có tên và dòng phụ tùy chọn (cơ sở của phòng);
 * mục đang chọn có nền {@code surface.selected} và biểu tượng check. Chọn một mục thì gửi mã của mục qua Fragment
 * Result API rồi đóng; mục có mã rỗng là "Tất cả". Kết quả trả theo <b>mã</b>, không theo vị trí, để danh sách
 * đổi trong lúc hộp mở không làm chọn nhầm.
 *
 * <p>Hành vi như hộp của CMP-26: không đóng khi chạm ra ngoài; Back và nút Đóng đóng hộp không đổi lựa chọn.
 * Hiển thị bằng {@code show(getChildFragmentManager(), TAG)} của màn hình; nhận kết quả bằng {@code
 * getChildFragmentManager().setFragmentResultListener(khoaKetQua, getViewLifecycleOwner(), ...)} và đọc {@link
 * #KEY_MA}. Dữ liệu truyền bằng {@code Bundle} arguments vì hộp không phải điểm đến của nav graph (cùng lý do
 * như {@link XacNhanDialogFragment}).
 */
public class ChonMotDialogFragment extends DialogFragment {

    /** Khóa trong {@code Bundle} kết quả: mã của mục được chọn (chuỗi rỗng là "Tất cả"). */
    public static final String KEY_MA = "ma";

    private static final String ARG_KHOA_KET_QUA = "khoaKetQua";
    private static final String ARG_TIEU_DE = "tieuDe";
    private static final String ARG_MA = "ma";
    private static final String ARG_TEN = "ten";
    private static final String ARG_PHU_DE = "phuDe";
    private static final String ARG_MA_DANG_CHON = "maDangChon";

    private DialogChonMotBinding _binding;

    /**
     * @param khoaKetQua khóa nhận kết quả bằng Fragment Result API
     * @param tieuDe tiêu đề hộp (thường là nhãn của ô chọn)
     * @param ma mã từng mục; mục đầu "Tất cả" có mã rỗng
     * @param ten tên từng mục, cùng độ dài với {@code ma}
     * @param phuDe dòng phụ từng mục (chuỗi rỗng nếu không có), cùng độ dài với {@code ma}
     * @param maDangChon mã mục đang chọn (chuỗi rỗng là "Tất cả")
     */
    @NonNull
    public static ChonMotDialogFragment tao(
            @NonNull String khoaKetQua,
            @NonNull String tieuDe,
            @NonNull String[] ma,
            @NonNull String[] ten,
            @NonNull String[] phuDe,
            @NonNull String maDangChon) {
        Bundle thamSo = new Bundle();
        thamSo.putString(ARG_KHOA_KET_QUA, khoaKetQua);
        thamSo.putString(ARG_TIEU_DE, tieuDe);
        thamSo.putStringArray(ARG_MA, ma);
        thamSo.putStringArray(ARG_TEN, ten);
        thamSo.putStringArray(ARG_PHU_DE, phuDe);
        thamSo.putString(ARG_MA_DANG_CHON, maDangChon);
        ChonMotDialogFragment hop = new ChonMotDialogFragment();
        hop.setArguments(thamSo);
        return hop;
    }

    @NonNull
    @Override
    public Dialog onCreateDialog(@Nullable Bundle savedInstanceState) {
        Dialog hop = super.onCreateDialog(savedInstanceState);
        hop.setCanceledOnTouchOutside(false);
        return hop;
    }

    @NonNull
    private DialogChonMotBinding binding() {
        return Objects.requireNonNull(_binding);
    }

    @Nullable
    @Override
    public View onCreateView(
            @NonNull LayoutInflater inflater,
            @Nullable ViewGroup container,
            @Nullable Bundle savedInstanceState) {
        _binding = DialogChonMotBinding.inflate(inflater, container, false);
        return _binding.getRoot();
    }

    @Override
    public void onViewCreated(@NonNull View view, @Nullable Bundle savedInstanceState) {
        super.onViewCreated(view, savedInstanceState);
        Bundle thamSo = requireArguments();
        final String khoa = Objects.requireNonNull(thamSo.getString(ARG_KHOA_KET_QUA));
        String[] ma = Objects.requireNonNull(thamSo.getStringArray(ARG_MA));
        String[] ten = Objects.requireNonNull(thamSo.getStringArray(ARG_TEN));
        String[] phuDe = Objects.requireNonNull(thamSo.getStringArray(ARG_PHU_DE));
        String dangChon = thamSo.getString(ARG_MA_DANG_CHON, "");

        binding().tvTieuDe.setText(thamSo.getString(ARG_TIEU_DE));

        // Rộng bằng màn hình trừ lề ds_space_6 mỗi bên, tối đa ds_size_dialog_max (cùng cách CMP-26).
        int toiDa = getResources().getDimensionPixelSize(R.dimen.ds_size_dialog_max);
        int le = getResources().getDimensionPixelSize(R.dimen.ds_space_6);
        int rongKhaDung = getResources().getDisplayMetrics().widthPixels - 2 * le;
        ViewGroup.LayoutParams thongSoHop = binding().llHop.getLayoutParams();
        thongSoHop.width = Math.min(rongKhaDung, toiDa);
        binding().llHop.setLayoutParams(thongSoHop);

        LayoutInflater inflater = getLayoutInflater();
        for (int i = 0; i < ma.length; i++) {
            final String maMuc = ma[i];
            ItemLuaChonBinding dong = ItemLuaChonBinding.inflate(inflater, binding().llDanhSach, false);
            dong.tvTen.setText(ten[i]);
            boolean coPhuDe = phuDe[i] != null && !phuDe[i].isEmpty();
            dong.tvPhuDe.setText(phuDe[i]);
            dong.tvPhuDe.setVisibility(coPhuDe ? View.VISIBLE : View.GONE);
            boolean dangDuocChon = maMuc.equals(dangChon);
            // selected cho nền và để trình đọc màn hình đọc là đang chọn; check là dấu hiệu ngoài màu.
            dong.getRoot().setSelected(dangDuocChon);
            dong.imgCheck.setVisibility(dangDuocChon ? View.VISIBLE : View.GONE);
            dong.getRoot()
                    .setOnClickListener(
                            nut -> {
                                Bundle ketQua = new Bundle();
                                ketQua.putString(KEY_MA, maMuc);
                                getParentFragmentManager().setFragmentResult(khoa, ketQua);
                                dismiss();
                            });
            binding().llDanhSach.addView(dong.getRoot());
        }
        binding().btnDong.setOnClickListener(nut -> dismiss());
    }

    @Override
    public void onStart() {
        super.onStart();
        Dialog hop = getDialog();
        if (hop == null || hop.getWindow() == null) {
            return;
        }
        // Lớp nền phủ do dialog_chon_mot.xml vẽ bằng color.overlay.scrim nên bỏ nền và độ mờ mặc định.
        Window cuaSo = hop.getWindow();
        cuaSo.setBackgroundDrawable(new ColorDrawable(Color.TRANSPARENT));
        cuaSo.clearFlags(WindowManager.LayoutParams.FLAG_DIM_BEHIND);
        cuaSo.setLayout(ViewGroup.LayoutParams.MATCH_PARENT, ViewGroup.LayoutParams.MATCH_PARENT);
    }

    @Override
    public void onDestroyView() {
        super.onDestroyView();
        _binding = null;
    }
}
