package com.nhom13.quanlymuontra.ui.phieuyeucau;

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
import com.nhom13.quanlymuontra.databinding.ItemSoanPhieuLoaiBinding;
import com.nhom13.quanlymuontra.databinding.ItemTaiThemBinding;
import com.nhom13.quanlymuontra.generated.model.TraCuuThietBiItem;
import com.nhom13.quanlymuontra.ui.tracuu.TraCuuThietBiViewModel.TrangThaiTaiThem;
import java.util.ArrayList;
import java.util.List;
import java.util.Map;
import java.util.Objects;
import java.util.UUID;
import java.util.function.BiConsumer;
import java.util.function.Consumer;

/**
 * Danh sách của SCR-SV-04 (FC 6.6: {@code ListAdapter} và {@code DiffUtil}): mục đầu (chọn phòng), các dòng loại theo
 * đúng thứ tự máy chủ trả, một dòng tải thêm khi đang tải hoặc lỗi, và mục cuối (thời gian mượn). Đầu và cuối là view
 * dựng sẵn của Fragment (ô nhập không bị bind lại). Không sắp xếp lại (FA 3.10). Dòng nhận diện theo mã loại.
 */
public final class SoanPhieuYeuCauAdapter
        extends ListAdapter<SoanPhieuYeuCauAdapter.Muc, RecyclerView.ViewHolder> {

    private static final int LOAI_DAU = 0;
    private static final int LOAI_DONG = 1;
    private static final int LOAI_TAI_THEM = 2;
    private static final int LOAI_CUOI = 3;

    /** Một mục của danh sách. */
    static final class Muc {
        final int loai;
        @Nullable final TraCuuThietBiItem dong;
        final int soLuongDaChon;
        @Nullable final TrangThaiTaiThem taiThem;

        private Muc(
                int loai,
                @Nullable TraCuuThietBiItem dong,
                int soLuongDaChon,
                @Nullable TrangThaiTaiThem taiThem) {
            this.loai = loai;
            this.dong = dong;
            this.soLuongDaChon = soLuongDaChon;
            this.taiThem = taiThem;
        }

        static Muc dau() {
            return new Muc(LOAI_DAU, null, 0, null);
        }

        static Muc dong(@NonNull TraCuuThietBiItem dong, int soLuongDaChon) {
            return new Muc(LOAI_DONG, dong, soLuongDaChon, null);
        }

        static Muc taiThem(@NonNull TrangThaiTaiThem trangThai) {
            return new Muc(LOAI_TAI_THEM, null, 0, trangThai);
        }

        static Muc cuoi() {
            return new Muc(LOAI_CUOI, null, 0, null);
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
                        return Objects.equals(
                                Objects.requireNonNull(a.dong).getMaLoai(),
                                Objects.requireNonNull(b.dong).getMaLoai());
                    }
                    return true;
                }

                @Override
                public boolean areContentsTheSame(@NonNull Muc a, @NonNull Muc b) {
                    if (a.loai == LOAI_DONG) {
                        TraCuuThietBiItem x = Objects.requireNonNull(a.dong);
                        TraCuuThietBiItem y = Objects.requireNonNull(b.dong);
                        return a.soLuongDaChon == b.soLuongDaChon
                                && Objects.equals(x.getTenLoai(), y.getTenLoai())
                                && Objects.equals(x.getHangSanXuat(), y.getHangSanXuat())
                                && Objects.equals(x.getSoLuongKhaDung(), y.getSoLuongKhaDung());
                    }
                    if (a.loai == LOAI_TAI_THEM) {
                        return Objects.equals(a.taiThem, b.taiThem);
                    }
                    return true;
                }
            };

    private final View dau;
    private final View cuoi;
    private final Runnable thuLaiTaiThem;
    private final BiConsumer<UUID, Integer> khiDoiSoLuong;
    private final Consumer<UUID> khiChonDangKyCho;

    /**
     * @param dau view đầu danh sách (chọn phòng), dựng sẵn bởi Fragment
     * @param cuoi view cuối danh sách (thời gian mượn), dựng sẵn bởi Fragment
     * @param thuLaiTaiThem xử lý nút Thử lại ở dòng tải thêm
     * @param khiDoiSoLuong xử lý khi sinh viên đổi số lượng của một loại (mã loại, số lượng mới)
     * @param khiChonDangKyCho xử lý khi chọn liên kết Đăng ký chờ của một loại hết hàng
     */
    public SoanPhieuYeuCauAdapter(
            @NonNull View dau,
            @NonNull View cuoi,
            @NonNull Runnable thuLaiTaiThem,
            @NonNull BiConsumer<UUID, Integer> khiDoiSoLuong,
            @NonNull Consumer<UUID> khiChonDangKyCho) {
        super(KHAC_BIET);
        this.dau = dau;
        this.cuoi = cuoi;
        this.thuLaiTaiThem = thuLaiTaiThem;
        this.khiDoiSoLuong = khiDoiSoLuong;
        this.khiChonDangKyCho = khiChonDangKyCho;
    }

    /** Đặt dữ liệu: đầu, các dòng loại, dòng tải thêm nếu cần, cuối. Luôn tạo danh sách mới để so sánh. */
    public void capNhat(
            @Nullable List<TraCuuThietBiItem> dong,
            @NonNull TrangThaiTaiThem taiThem,
            @NonNull Map<UUID, Integer> soLuongDaChon) {
        List<Muc> cacMuc = new ArrayList<>();
        cacMuc.add(Muc.dau());
        if (dong != null) {
            for (TraCuuThietBiItem d : dong) {
                Integer chon = d.getMaLoai() == null ? null : soLuongDaChon.get(d.getMaLoai());
                cacMuc.add(Muc.dong(d, chon == null ? 0 : chon));
            }
        }
        if (taiThem != TrangThaiTaiThem.KHONG) {
            cacMuc.add(Muc.taiThem(taiThem));
        }
        cacMuc.add(Muc.cuoi());
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
            return boc(dau);
        }
        if (loai == LOAI_CUOI) {
            return boc(cuoi);
        }
        if (loai == LOAI_DONG) {
            return new DongViewHolder(
                    ItemSoanPhieuLoaiBinding.inflate(inflater, cha, false),
                    khiDoiSoLuong,
                    khiChonDangKyCho);
        }
        return new TaiThemViewHolder(ItemTaiThemBinding.inflate(inflater, cha, false), thuLaiTaiThem);
    }

    /** View dựng sẵn được dùng lại: gỡ khỏi cha cũ (nếu còn) trước khi bọc vào ViewHolder mới. */
    @NonNull
    private static RecyclerView.ViewHolder boc(@NonNull View view) {
        ViewGroup chaCu = (ViewGroup) view.getParent();
        if (chaCu != null) {
            chaCu.removeView(view);
        }
        return new RecyclerView.ViewHolder(view) {};
    }

    @Override
    public void onBindViewHolder(@NonNull RecyclerView.ViewHolder vh, int viTri) {
        Muc muc = getItem(viTri);
        if (vh instanceof DongViewHolder) {
            ((DongViewHolder) vh).gan(Objects.requireNonNull(muc.dong), muc.soLuongDaChon);
        } else if (vh instanceof TaiThemViewHolder) {
            ((TaiThemViewHolder) vh).gan(Objects.requireNonNull(muc.taiThem));
        }
    }

    /** Dòng loại (CMP-16 biến thể Có điều khiển). */
    static final class DongViewHolder extends RecyclerView.ViewHolder {
        private final ItemSoanPhieuLoaiBinding binding;
        private final BiConsumer<UUID, Integer> khiDoiSoLuong;
        private final Consumer<UUID> khiChonDangKyCho;

        DongViewHolder(
                @NonNull ItemSoanPhieuLoaiBinding binding,
                @NonNull BiConsumer<UUID, Integer> khiDoiSoLuong,
                @NonNull Consumer<UUID> khiChonDangKyCho) {
            super(binding.getRoot());
            this.binding = binding;
            this.khiDoiSoLuong = khiDoiSoLuong;
            this.khiChonDangKyCho = khiChonDangKyCho;
        }

        void gan(@NonNull TraCuuThietBiItem dong, int soLuongDaChon) {
            Context ctx = binding.getRoot().getContext();
            final UUID maLoai = dong.getMaLoai();
            String hang = dong.getHangSanXuat();
            binding.tvTenLoai.setText(
                    hang == null || hang.isEmpty()
                            ? dong.getTenLoai()
                            : ctx.getString(R.string.tra_cuu_ghep_hai, dong.getTenLoai(), hang));

            // Số lượng khả dụng do máy chủ tính (BR-16, BR-20); client chỉ hiển thị. Thiếu số liệu coi như hết hàng.
            Integer khaDung = dong.getSoLuongKhaDung();
            int toiDa = khaDung == null ? 0 : khaDung;
            boolean hetHang = toiDa <= 0;

            binding.qsSoLuong.datKhiDoi(null);
            if (hetHang) {
                binding.sbHetHang.setNhanPhatSinh(NhanPhatSinh.HET_HANG);
                binding.sbHetHang.setVisibility(View.VISIBLE);
                binding.tvKhaDung.setVisibility(View.GONE);
                binding.llChonSoLuong.setVisibility(View.GONE);
                binding.btnDangKyCho.setVisibility(View.VISIBLE);
                binding.btnDangKyCho.setOnClickListener(
                        nut -> {
                            if (maLoai != null) {
                                khiChonDangKyCho.accept(maLoai);
                            }
                        });
            } else {
                binding.tvKhaDung.setText(
                        ctx.getString(
                                R.string.tra_cuu_kha_dung,
                                khaDung == null ? HangSo.GIA_TRI_THIEU : String.valueOf(khaDung)));
                binding.tvKhaDung.setVisibility(View.VISIBLE);
                binding.sbHetHang.setVisibility(View.GONE);
                binding.btnDangKyCho.setVisibility(View.GONE);
                binding.btnDangKyCho.setOnClickListener(null);
                binding.llChonSoLuong.setVisibility(View.VISIBLE);
                binding.qsSoLuong.datGioiHan(0, toiDa);
                binding.qsSoLuong.datGiaTri(soLuongDaChon);
                binding.qsSoLuong.datKhiDoi(
                        giaTri -> {
                            if (maLoai != null) {
                                khiDoiSoLuong.accept(maLoai, giaTri);
                            }
                        });
            }
        }
    }

    /** Dòng tải thêm: đang tải hoặc lỗi kèm Thử lại. */
    static final class TaiThemViewHolder extends RecyclerView.ViewHolder {
        private final ItemTaiThemBinding binding;

        TaiThemViewHolder(@NonNull ItemTaiThemBinding binding, @NonNull Runnable thuLai) {
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
