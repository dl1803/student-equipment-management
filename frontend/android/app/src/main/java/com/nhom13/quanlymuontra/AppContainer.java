package com.nhom13.quanlymuontra;

/**
 * Điểm lắp ráp duy nhất của ứng dụng, tạo một lần trong {@link UngDung} (FA 5.1, ĐX-FA-12).
 *
 * <p>Không dùng framework DI: các thành phần dùng chung ({@code OkHttpClient}, client Retrofit,
 * {@code SessionManager}, các {@code Repository}) được tạo thủ công tại đây và chỉ được thêm vào
 * khi lớp của chúng tồn tại, để không có trường nào không có người dùng. {@code ViewModel} nhận
 * phụ thuộc qua {@link AppViewModelFactory}, không tự truy cập lớp này.
 */
public final class AppContainer {}
