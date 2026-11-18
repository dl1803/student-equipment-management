package com.nhom13.quanlymuontra.common.widget;

import android.content.Context;
import android.graphics.Paint;
import android.util.AttributeSet;
import androidx.annotation.NonNull;
import androidx.annotation.Nullable;
import com.google.android.material.textview.MaterialTextView;

/**
 * CMP-03 Liên kết văn bản (DS 8): luôn gạch chân để không chỉ phân biệt bằng màu (DS CMP-03, NT-02). Màu,
 * cỡ chữ và vùng chạm 48 dp nằm ở style {@code Widget.Ds.Link}; lớp này chỉ bổ sung gạch chân vì
 * XML không có thuộc tính gạch chân. Dùng cho liên kết trong dòng chữ hoặc đứng riêng; mục điều
 * hướng và hành động dạng nút không dùng lớp này (xem CMP-01 biến thể văn bản).
 */
public class TextLinkView extends MaterialTextView {

    public TextLinkView(@NonNull Context context) {
        this(context, null);
    }

    public TextLinkView(@NonNull Context context, @Nullable AttributeSet attrs) {
        this(context, attrs, android.R.attr.textViewStyle);
    }

    public TextLinkView(
            @NonNull Context context, @Nullable AttributeSet attrs, int defStyleAttr) {
        super(context, attrs, defStyleAttr);
        setPaintFlags(getPaintFlags() | Paint.UNDERLINE_TEXT_FLAG);
    }
}
