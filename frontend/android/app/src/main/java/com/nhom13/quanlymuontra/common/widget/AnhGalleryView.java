package com.nhom13.quanlymuontra.common.widget;

import android.content.Context;
import android.graphics.drawable.Drawable;
import android.util.AttributeSet;
import android.view.LayoutInflater;
import android.view.View;
import android.widget.ImageView;
import android.widget.LinearLayout;
import androidx.annotation.NonNull;
import androidx.annotation.Nullable;
import com.bumptech.glide.Glide;
import com.bumptech.glide.load.DataSource;
import com.bumptech.glide.load.engine.GlideException;
import com.bumptech.glide.request.RequestListener;
import com.bumptech.glide.request.target.Target;
import com.nhom13.quanlymuontra.R;
import com.nhom13.quanlymuontra.databinding.ViewAnhGalleryBinding;
import java.util.ArrayList;
import java.util.List;

/**
 * CMP-23 dải ảnh (DS CMP-23, ĐX-FA-15): các ô ảnh vuông {@code size.photo.thumb} trong dải cuộn ngang, ảnh cắt vừa
 * khung. Không có ảnh thì hiện một ô thay thế (nền {@code surface.muted}, biểu tượng {@code image}). Ảnh danh mục là
 * URL công khai nên dùng bộ nhớ đệm mặc định của Glide; lớp này không ghi URL vào nhật ký.
 *
 * <p>Có ít nhất một ảnh không tải được thì thay cả dải bằng một khối lỗi kèm nút "Tải lại"; nút này chỉ gọi {@link
 * #datKhiTaiLai}, việc tải lại dữ liệu do màn hình quyết định. Không tự thử lại vì URL công khai không hết hạn.
 * Chạm một ô gọi {@link KhiChonAnh}; lớp xem lớn do màn hình mở ({@link ImageViewerDialogFragment}).
 */
public class AnhGalleryView extends LinearLayout {

    /** Người dùng chạm vào một ảnh. */
    public interface KhiChonAnh {
        /**
         * @param viTri vị trí ảnh trong dải, tính từ 0
         * @param url URL của ảnh được chạm
         */
        void chon(int viTri, @NonNull String url);
    }

    private final ViewAnhGalleryBinding binding;
    @Nullable private KhiChonAnh khiChonAnh;
    @Nullable private OnClickListener khiTaiLai;
    @Nullable private List<String> dangHienThi;
    @Nullable private String tenDangHienThi;
    private boolean dangLoi;
    /** Tăng mỗi lần dựng lại dải để bỏ qua kết quả muộn của các ô đã bị thay. */
    private int lanDung;

    public AnhGalleryView(@NonNull Context context) {
        this(context, null);
    }

    public AnhGalleryView(@NonNull Context context, @Nullable AttributeSet attrs) {
        this(context, attrs, 0);
    }

    public AnhGalleryView(@NonNull Context context, @Nullable AttributeSet attrs, int defStyleAttr) {
        super(context, attrs, defStyleAttr);
        setOrientation(VERTICAL);
        binding = ViewAnhGalleryBinding.inflate(LayoutInflater.from(context), this);
        binding.btnTaiLaiAnh.setOnClickListener(
                nut -> {
                    if (khiTaiLai != null) {
                        khiTaiLai.onClick(nut);
                    }
                });
    }

    /** Đặt hàm xử lý khi chạm một ảnh; {@code null} thì bỏ. */
    public void datKhiChonAnh(@Nullable KhiChonAnh khiChon) {
        this.khiChonAnh = khiChon;
    }

    /** Đặt hàm xử lý nút "Tải lại" của khối lỗi; {@code null} thì bỏ. */
    public void datKhiTaiLai(@Nullable OnClickListener khiTai) {
        this.khiTaiLai = khiTai;
    }

    /**
     * Hiển thị dải ảnh. Danh sách rỗng là không có ảnh (hiện ô thay thế). Gọi lại với cùng danh sách khi không đang
     * lỗi thì không dựng lại.
     *
     * @param dsUrl URL các ảnh theo đúng thứ tự máy chủ trả
     * @param tenThietBi dùng cho tên truy cập "Ảnh n của <tên>"
     */
    public void datAnh(@NonNull List<String> dsUrl, @NonNull String tenThietBi) {
        if (!dangLoi && dsUrl.equals(dangHienThi) && tenThietBi.equals(tenDangHienThi)) {
            return;
        }
        giaiPhong();
        dangHienThi = new ArrayList<>(dsUrl);
        tenDangHienThi = tenThietBi;
        dangLoi = false;
        final int phienBan = ++lanDung;

        binding.llLoiAnh.setVisibility(View.GONE);
        binding.hsvDaiAnh.setVisibility(View.VISIBLE);
        binding.llDaiAnh.removeAllViews();
        if (dsUrl.isEmpty()) {
            binding.llDaiAnh.addView(taoOThayThe());
            return;
        }
        for (int i = 0; i < dsUrl.size(); i++) {
            binding.llDaiAnh.addView(taoOAnh(dsUrl.get(i), i, tenThietBi, phienBan));
        }
    }

    /** Hủy các yêu cầu tải ảnh của dải; Fragment gọi ở {@code onDestroyView}. */
    public void giaiPhong() {
        for (int i = 0; i < binding.llDaiAnh.getChildCount(); i++) {
            View con = binding.llDaiAnh.getChildAt(i);
            if (con instanceof ImageView) {
                Glide.with(con).clear(con);
            }
        }
    }

    @NonNull
    private ImageView taoOAnh(
            @NonNull String url, int viTri, @NonNull String tenThietBi, int phienBan) {
        ImageView anh = new ImageView(getContext());
        anh.setLayoutParams(thongSoO(viTri));
        anh.setBackgroundResource(R.drawable.ds_anh_o_nen);
        anh.setClipToOutline(true);
        anh.setScaleType(ImageView.ScaleType.CENTER_CROP);
        anh.setFocusable(true);
        anh.setContentDescription(
                getContext().getString(R.string.anh_mo_ta_o_anh, viTri + 1, tenThietBi));
        anh.setOnClickListener(
                nut -> {
                    if (khiChonAnh != null) {
                        khiChonAnh.chon(viTri, url);
                    }
                });
        Glide.with(anh)
                .load(url)
                .centerCrop()
                .listener(
                        new RequestListener<Drawable>() {
                            @Override
                            public boolean onLoadFailed(
                                    @Nullable GlideException loi,
                                    Object mau,
                                    Target<Drawable> dich,
                                    boolean laYeuCauDau) {
                                baoLoi(phienBan);
                                return false;
                            }

                            @Override
                            public boolean onResourceReady(
                                    Drawable taiNguyen,
                                    Object mau,
                                    Target<Drawable> dich,
                                    DataSource nguon,
                                    boolean laYeuCauDau) {
                                return false;
                            }
                        })
                .into(anh);
        return anh;
    }

    @NonNull
    private ImageView taoOThayThe() {
        ImageView o = new ImageView(getContext());
        o.setLayoutParams(thongSoO(0));
        o.setBackgroundResource(R.drawable.ds_anh_o_nen);
        o.setScaleType(ImageView.ScaleType.CENTER);
        o.setImageResource(R.drawable.ic_image);
        o.setContentDescription(getContext().getString(R.string.anh_thay_the_mo_ta));
        return o;
    }

    /** Ô vuông {@code size.photo.thumb}; các ô sau cách ô trước {@code space.2}. */
    @NonNull
    private LinearLayout.LayoutParams thongSoO(int viTri) {
        int canh = getResources().getDimensionPixelSize(R.dimen.ds_size_photo_thumb);
        LinearLayout.LayoutParams thongSo = new LinearLayout.LayoutParams(canh, canh);
        if (viTri > 0) {
            thongSo.setMarginStart(getResources().getDimensionPixelSize(R.dimen.ds_space_2));
        }
        return thongSo;
    }

    /** Một ô không tải được: thay cả dải bằng khối lỗi (DS CMP-23). Bỏ qua kết quả muộn của dải đã bị thay. */
    private void baoLoi(int phienBan) {
        if (phienBan != lanDung || dangLoi) {
            return;
        }
        dangLoi = true;
        binding.hsvDaiAnh.setVisibility(View.GONE);
        binding.llLoiAnh.setVisibility(View.VISIBLE);
    }
}
