package com.nhom13.quanlymuontra;

import okhttp3.Request;
import okio.Timeout;
import retrofit2.Call;
import retrofit2.Callback;
import retrofit2.Response;

/**
 * Bản giả của {@link Call} cho test ViewModel và Repository mà không cần mạng. Hoàn tất ngay khi {@link
 * #enqueue} (đồng bộ, trên luồng test) hoặc chờ đến khi test gọi {@link #hoanTat} / {@link #that}.
 */
public final class FakeCall<T> implements Call<T> {

    private final Request yeuCau = new Request.Builder().url("http://localhost/").build();
    private final Response<T> phanHoiSan;
    private final Throwable loiSan;
    private Callback<T> dangCho;
    private boolean daGoi;
    private int soLanEnqueue;
    private boolean daHuy;

    private FakeCall(Response<T> phanHoiSan, Throwable loiSan) {
        this.phanHoiSan = phanHoiSan;
        this.loiSan = loiSan;
    }

    /** Trả ngay phản hồi này khi được {@code enqueue}. */
    public static <T> FakeCall<T> traVe(Response<T> phanHoi) {
        return new FakeCall<>(phanHoi, null);
    }

    /** Báo ngay lỗi này ({@code onFailure}) khi được {@code enqueue}. */
    public static <T> FakeCall<T> nem(Throwable loi) {
        return new FakeCall<>(null, loi);
    }

    /** Không hoàn tất cho tới khi test gọi {@link #hoanTat} hoặc {@link #that}. */
    public static <T> FakeCall<T> cho() {
        return new FakeCall<>(null, null);
    }

    public void hoanTat(Response<T> phanHoi) {
        dangCho.onResponse(this, phanHoi);
    }

    public void that(Throwable loi) {
        dangCho.onFailure(this, loi);
    }

    public boolean daGoi() {
        return daGoi;
    }

    /** Số lần {@link #enqueue} được gọi; dùng để chứng minh không có yêu cầu lặp. */
    public int soLanEnqueue() {
        return soLanEnqueue;
    }

    @Override
    public Response<T> execute() {
        throw new UnsupportedOperationException("Test chỉ dùng enqueue");
    }

    @Override
    public void enqueue(Callback<T> callback) {
        daGoi = true;
        soLanEnqueue++;
        if (phanHoiSan != null) {
            callback.onResponse(this, phanHoiSan);
        } else if (loiSan != null) {
            callback.onFailure(this, loiSan);
        } else {
            dangCho = callback;
        }
    }

    @Override
    public boolean isExecuted() {
        return daGoi;
    }

    @Override
    public void cancel() {
        daHuy = true;
    }

    @Override
    public boolean isCanceled() {
        return daHuy;
    }

    @Override
    public Call<T> clone() {
        throw new UnsupportedOperationException("Test không nhân bản Call");
    }

    @Override
    public Request request() {
        return yeuCau;
    }

    @Override
    public Timeout timeout() {
        return Timeout.NONE;
    }
}
