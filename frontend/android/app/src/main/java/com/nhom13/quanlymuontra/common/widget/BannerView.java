package com.nhom13.quanlymuontra.common.widget;

import android.content.Context;
import android.content.res.ColorStateList;
import android.graphics.drawable.GradientDrawable;
import android.text.SpannableString;
import android.text.Spanned;
import android.text.style.BulletSpan;
import android.util.AttributeSet;
import android.view.LayoutInflater;
import android.view.View;
import android.widget.LinearLayout;
import android.widget.TextView;
import androidx.annotation.ColorRes;
import androidx.annotation.NonNull;
import androidx.annotation.Nullable;
import androidx.core.content.ContextCompat;
import androidx.core.view.ViewCompat;
import androidx.core.widget.ImageViewCompat;
import com.google.android.material.textview.MaterialTextView;
import com.nhom13.quanlymuontra.R;
import com.nhom13.quanlymuontra.common.nhan.Tong;
import com.nhom13.quanlymuontra.databinding.ViewBannerBinding;
import java.util.List;

/**
 * CMP-24 Khối thông báo (DS 8): khối nằm trong luồng trang, không phải thông báo nổi tự biến mất
 * (ĐX-FC-19). Gồm biểu tượng của tông, tiêu đề tùy chọn, nội dung, danh sách lý do tùy chọn và một
 * hành động tùy chọn. Tông trung tính không dùng cho khối thông báo. Khối tông lỗi và cảnh báo
 * được khai báo là thông báo khẩn cho trình đọc màn hình, các tông còn lại là thông báo thường.
 * Không có nút đóng: khối biến mất khi nguyên nhân hết (DS 9.4).
 */
public class BannerView extends LinearLayout {

    private final ViewBannerBinding binding;

    public BannerView(@NonNull Context context) {
        this(context, null);
    }

    public BannerView(@NonNull Context context, @Nullable AttributeSet attrs) {
        this(context, attrs, 0);
    }

    public BannerView(@NonNull Context context, @Nullable AttributeSet attrs, int defStyleAttr) {
        super(context, attrs, defStyleAttr);
        setOrientation(HORIZONTAL);
        int dem = getResources().getDimensionPixelSize(R.dimen.ds_space_card);
        setPadding(dem, dem, dem, dem);
        binding = ViewBannerBinding.inflate(LayoutInflater.from(context), this);
    }

    /**
     * Hiển thị khối thông báo.
     *
     * @param tong một trong INFO, SUCCESS, WARNING, DANGER
     * @param tieuDe tiêu đề; {@code null} hoặc rỗng thì không hiện
     * @param noiDung nội dung; {@code null} hoặc rỗng thì không hiện
     * @param dsLyDo mỗi phần tử một dòng có dấu đầu dòng; {@code null} hoặc rỗng thì không hiện
     * @throws IllegalArgumentException nếu {@code tong} là NEUTRAL
     */
    public void hienThi(
            @NonNull Tong tong,
            @Nullable CharSequence tieuDe,
            @Nullable CharSequence noiDung,
            @Nullable List<String> dsLyDo) {
        if (tong == Tong.NEUTRAL) {
            throw new IllegalArgumentException("Khối thông báo không dùng tông trung tính (DS CMP-24)");
        }

        boolean khan = tong == Tong.DANGER || tong == Tong.WARNING;
        ViewCompat.setAccessibilityLiveRegion(
                this,
                khan
                        ? ViewCompat.ACCESSIBILITY_LIVE_REGION_ASSERTIVE
                        : ViewCompat.ACCESSIBILITY_LIVE_REGION_POLITE);

        GradientDrawable nen = new GradientDrawable();
        nen.setShape(GradientDrawable.RECTANGLE);
        nen.setCornerRadius(getResources().getDimension(R.dimen.ds_radius_lg));
        nen.setColor(mau(GiaoDienTong.nen(tong)));
        nen.setStroke(
                getResources().getDimensionPixelSize(R.dimen.ds_border_width_default),
                mau(GiaoDienTong.vien(tong)));
        setBackground(nen);

        binding.imgBieuTuong.setImageResource(GiaoDienTong.hinhBieuTuong(tong));
        ImageViewCompat.setImageTintList(
                binding.imgBieuTuong, ColorStateList.valueOf(mau(GiaoDienTong.bieuTuong(tong))));

        // setTextAppearance đặt lại màu chữ nên phải gọi trước setTextColor.
        binding.tvTieuDe.setTextAppearance(R.style.TextAppearance_Ds_TitleCard);
        binding.tvTieuDe.setTextColor(mau(GiaoDienTong.chu(tong)));
        datChu(binding.tvTieuDe, tieuDe);
        datChu(binding.tvNoiDung, noiDung);

        binding.llLyDo.removeAllViews();
        int khoang = getResources().getDimensionPixelSize(R.dimen.ds_space_2);
        if (dsLyDo != null) {
            for (String lyDo : dsLyDo) {
                if (lyDo == null || lyDo.isEmpty()) {
                    continue;
                }
                SpannableString dong = new SpannableString(lyDo);
                dong.setSpan(new BulletSpan(khoang), 0, dong.length(), Spanned.SPAN_INCLUSIVE_EXCLUSIVE);
                MaterialTextView tv = new MaterialTextView(getContext());
                tv.setTextAppearance(R.style.TextAppearance_Ds_Body);
                tv.setText(dong);
                binding.llLyDo.addView(tv);
            }
        }
        binding.llLyDo.setVisibility(binding.llLyDo.getChildCount() > 0 ? View.VISIBLE : View.GONE);

        setVisibility(View.VISIBLE);
    }

    /**
     * Đặt hành động tùy chọn ở cuối khối (nút văn bản, CMP-01). {@code nhan == null} thì bỏ hành
     * động.
     */
    public void datHanhDong(@Nullable CharSequence nhan, @Nullable OnClickListener khiNhan) {
        if (nhan == null || nhan.length() == 0) {
            binding.btnHanhDong.setOnClickListener(null);
            binding.btnHanhDong.setVisibility(View.GONE);
            return;
        }
        binding.btnHanhDong.setText(nhan);
        binding.btnHanhDong.setOnClickListener(khiNhan);
        binding.btnHanhDong.setVisibility(View.VISIBLE);
    }

    /** Ẩn khối khi nguyên nhân đã hết. */
    public void an() {
        setVisibility(View.GONE);
    }

    private void datChu(@NonNull TextView tv, @Nullable CharSequence chu) {
        if (chu == null || chu.length() == 0) {
            tv.setVisibility(View.GONE);
        } else {
            tv.setText(chu);
            tv.setVisibility(View.VISIBLE);
        }
    }

    private int mau(@ColorRes int mauResId) {
        return ContextCompat.getColor(getContext(), mauResId);
    }
}
