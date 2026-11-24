package com.nhom13.quanlymuontra.ui.tracuu;

import android.content.Context;
import android.view.LayoutInflater;
import android.view.View;
import android.view.ViewGroup;
import androidx.annotation.NonNull;
import androidx.annotation.Nullable;
import androidx.recyclerview.widget.DiffUtil;
import androidx.recyclerview.widget.ListAdapter;
import androidx.recyclerview.widget.RecyclerView;
import com.nhom13.quanlymuontra.R;
import com.nhom13.quanlymuontra.common.HangSo;
import com.nhom13.quanlymuontra.common.nhan.NhanPhatSinh;
import com.nhom13.quanlymuontra.databinding.ItemTaiThemBinding;
import com.nhom13.quanlymuontra.databinding.ItemTraCuuThietBiBinding;
import com.nhom13.quanlymuontra.generated.model.TraCuuThietBiItem;
import com.nhom13.quanlymuontra.ui.tracuu.TraCuuThietBiViewModel.TrangThaiTaiThem;
import java.util.ArrayList;
import java.util.List;
import java.util.Objects;

/**
 * Danh sách của SCR-SV-02 (FC 6.6: {@code ListAdapter} và {@code DiffUtil}): một mục đầu (view bộ lọc dựng sẵn
 * của Fragment), các dòng kết quả theo đúng thứ tự máy chủ trả, và một dòng cuối khi đang tải thêm hoặc lỗi tải
 * thêm. Không sắp xếp lại (FA 3.10). Dòng nhận diện theo cặp (mã loại, mã phòng).
 */
public final class TraCuuThietBiAdapter
        extends ListAdapter<TraCuuThietBiAdapter.Muc, RecyclerView.ViewHolder> {

    private static final int LOAI_DAU = 0;
    private static final int LOAI_DONG = 1;
    private static final int LOAI_CHAN = 2;

    /** Một mục của danh sách: đầu, dòng kết quả hoặc dòng cuối. */
    static final class Muc {
        final int loai;
        @Nullable final TraCuuThietBiItem dong;
        @Nullable final TrangThaiTaiThem chan;

        private Muc(int loai, @Nullable TraCuuThietBiItem dong, @Nullable TrangThaiTaiThem chan) {
            this.loai = loai;
            this.dong = dong;
            this.chan = chan;
        }

        static Muc dau() {
            return new Muc(LOAI_DAU, null, null);
        }

        static Muc dong(@NonNull TraCuuThietBiItem dong) {
            return new Muc(LOAI_DONG, dong, null);
        }

        static Muc chan(@NonNull TrangThaiTaiThem trangThai) {
            return new Muc(LOAI_CHAN, null, trangThai);
        }
    }

    private static final DiffUtil.ItemCallback<Muc> KHAC_BIET =
            new DiffUtil.ItemCallback<Muc>() {
                @Override
                public boolean areItemsTheSame(@NonNull Muc a, @NonNull Muc b) {
                    if (a.loai != b.loai) {
                        return false;
                    }
                    if (a.loai == LOAI_DONG) {
                        TraCuuThietBiItem x = Objects.requireNonNull(a.dong);
                        TraCuuThietBiItem y = Objects.requireNonNull(b.dong);
                        return Objects.equals(x.getMaLoai(), y.getMaLoai())
                                && Objects.equals(x.getMaPhong(), y.getMaPhong());
                    }
                    return true;
                }

                @Override
                public boolean areContentsTheSame(@NonNull Muc a, @NonNull Muc b) {
                    if (a.loai == LOAI_DONG) {
                        TraCuuThietBiItem x = Objects.requireNonNull(a.dong);
                        TraCuuThietBiItem y = Objects.requireNonNull(b.dong);
                        return Objects.equals(x.getTenLoai(), y.getTenLoai())
                                && Objects.equals(x.getHangSanXuat(), y.getHangSanXuat())
                                && Objects.equals(x.getTenPhong(), y.getTenPhong())
                                && Objects.equals(x.getCoSo(), y.getCoSo())
                                && Objects.equals(x.getSoLuongKhaDung(), y.getSoLuongKhaDung());
                    }
                    if (a.loai == LOAI_CHAN) {
                        return Objects.equals(a.chan, b.chan);
                    }
                    return true;
                }
            };

    private final View dau;
    private final Runnable thuLaiTaiThem;

    /**
     * @param dau view đầu danh sách (bộ lọc), dựng sẵn bởi Fragment
     * @param thuLaiTaiThem xử lý nút Thử lại ở dòng cuối khi lỗi tải thêm
     */
    public TraCuuThietBiAdapter(@NonNull View dau, @NonNull Runnable thuLaiTaiThem) {
        super(KHAC_BIET);
        this.dau = dau;
        this.thuLaiTaiThem = thuLaiTaiThem;
    }

    /**
     * Đặt dữ liệu: đầu, rồi các dòng, rồi dòng cuối nếu {@code chan} khác {@link TrangThaiTaiThem#KHONG}.
     * Luôn tạo danh sách mới để {@code ListAdapter} so sánh.
     */
    public void capNhat(@Nullable List<TraCuuThietBiItem> dong, @NonNull TrangThaiTaiThem chan) {
        List<Muc> cacMuc = new ArrayList<>();
        cacMuc.add(Muc.dau());
        if (dong != null) {
            for (TraCuuThietBiItem d : dong) {
                cacMuc.add(Muc.dong(d));
            }
        }
        if (chan != TrangThaiTaiThem.KHONG) {
            cacMuc.add(Muc.chan(chan));
        }
        submitList(cacMuc);
    }

    @Override
    public int getItemViewType(int viTri) {
        return getItem(viTri).loai;
    }

    @NonNull
    @Override
    public RecyclerView.ViewHolder onCreateViewHolder(@NonNull ViewGroup cha, int loai) {
        LayoutInflater inflater = LayoutInflater.from(cha.getContext());
        if (loai == LOAI_DAU) {
            // View đầu được dùng lại: gỡ khỏi cha cũ (nếu còn) trước khi bọc vào ViewHolder mới.
            ViewGroup chaCu = (ViewGroup) dau.getParent();
            if (chaCu != null) {
                chaCu.removeView(dau);
            }
            return new RecyclerView.ViewHolder(dau) {};
        }
        if (loai == LOAI_DONG) {
            return new DongViewHolder(ItemTraCuuThietBiBinding.inflate(inflater, cha, false));
        }
        return new ChanViewHolder(ItemTaiThemBinding.inflate(inflater, cha, false), thuLaiTaiThem);
    }

    @Override
    public void onBindViewHolder(@NonNull RecyclerView.ViewHolder vh, int viTri) {
        Muc muc = getItem(viTri);
        if (vh instanceof DongViewHolder) {
            ((DongViewHolder) vh).gan(Objects.requireNonNull(muc.dong));
        } else if (vh instanceof ChanViewHolder) {
            ((ChanViewHolder) vh).gan(Objects.requireNonNull(muc.chan));
        }
    }

    /** Dòng kết quả (CMP-16). */
    static final class DongViewHolder extends RecyclerView.ViewHolder {
        private final ItemTraCuuThietBiBinding binding;

        DongViewHolder(@NonNull ItemTraCuuThietBiBinding binding) {
            super(binding.getRoot());
            this.binding = binding;
        }

        void gan(@NonNull TraCuuThietBiItem dong) {
            Context ctx = binding.getRoot().getContext();
            String hang = dong.getHangSanXuat();
            binding.tvTenLoai.setText(
                    hang == null || hang.isEmpty()
                            ? dong.getTenLoai()
                            : ctx.getString(R.string.tra_cuu_ghep_hai, dong.getTenLoai(), hang));
            binding.tvPhong.setText(
                    ctx.getString(R.string.tra_cuu_ghep_hai, dong.getTenPhong(), dong.getCoSo()));
            // Số lượng khả dụng do máy chủ tính (BR-16, BR-20); client chỉ hiển thị, dòng 0 mang nhãn Hết hàng.
            Integer soLuong = dong.getSoLuongKhaDung();
            boolean hetHang = soLuong != null && soLuong == 0;
            if (hetHang) {
                binding.sbHetHang.setNhanPhatSinh(NhanPhatSinh.HET_HANG);
                binding.sbHetHang.setVisibility(View.VISIBLE);
                binding.tvKhaDung.setVisibility(View.GONE);
            } else {
                binding.tvKhaDung.setText(
                        ctx.getString(
                                R.string.tra_cuu_kha_dung,
                                soLuong == null ? HangSo.GIA_TRI_THIEU : String.valueOf(soLuong)));
                binding.tvKhaDung.setVisibility(View.VISIBLE);
                binding.sbHetHang.setVisibility(View.GONE);
            }
        }
    }

    /** Dòng cuối: đang tải thêm hoặc lỗi tải thêm kèm Thử lại. */
    static final class ChanViewHolder extends RecyclerView.ViewHolder {
        private final ItemTaiThemBinding binding;

        ChanViewHolder(@NonNull ItemTaiThemBinding binding, @NonNull Runnable thuLai) {
            super(binding.getRoot());
            this.binding = binding;
            binding.btnThuLaiTaiThem.setOnClickListener(nut -> thuLai.run());
        }

        void gan(@NonNull TrangThaiTaiThem trangThai) {
            binding.piTaiThem.setVisibility(
                    trangThai == TrangThaiTaiThem.DANG_TAI ? View.VISIBLE : View.GONE);
            binding.llLoiTaiThem.setVisibility(
                    trangThai == TrangThaiTaiThem.LOI ? View.VISIBLE : View.GONE);
        }
    }
}
