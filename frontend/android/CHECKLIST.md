# Android (sinh viên) — Checklist khởi tạo D01-L

> Tài liệu truy vết của dự án Android, lập ở D01-L ngày làm việc 01. Mục ghi **OPEN** là chưa được nhóm duyệt và không được tự triển khai.
> Nguồn: `Frontend-Architecture.md` (FA), `Frontend-Conventions.md` (FC), `UI-Screens.md`, `User-Flows.md`, `openapi.yaml` v1.1.0, `BANG_THEO_DOI_40_NGAY.md`.

## 1. Quyết định cấu hình dự án

| Hạng mục | Giá trị | Trạng thái | Căn cứ |
|---|---|---|---|
| Vị trí dự án Gradle | `frontend/android/` | Người dùng chốt ngày 13/11/2026, theo cấu trúc repo hiện tại | `README.md` |
| Ngôn ngữ | Java | Đã chốt | FA-02, KT-04 |
| Giao diện | XML Views, ViewBinding | Theo kế hoạch D01-L và ĐX-FA-12 | D01-L, ĐX-FA-12 |
| `minSdk` | 26 | Theo kế hoạch D01-L và ĐX-FA-13 | D01-L, ĐX-FA-13 |
| Gói gốc (`namespace` và `applicationId`) | `com.nhom13.quanlymuontra` | Đề xuất của Architect theo ủy quyền ngày 13/11/2026, chờ nhóm xác nhận | FA 5.2: "do nhóm đặt" |
| `compileSdk`, `targetSdk` | 36, 36 | Như trên | AGP 8.13 hỗ trợ tối đa API 36 trở lên |
| Android Gradle Plugin | 8.13.2 | Như trên | Bản cuối của dòng 8.x |
| Gradle wrapper | 8.13 | Như trên | Gradle tối thiểu và mặc định của AGP 8.13 |
| JDK dùng để build | 17 đến 24 (khuyến nghị 17) | Như trên | AGP cần JDK 17 trở lên; Gradle 8.13 chạy được đến JDK 24 |
| Mức ngôn ngữ Java | 17 | Như trên | Khớp JDK tối thiểu của AGP |
| Cách khai báo Gradle | Kotlin DSL, `gradle/libs.versions.toml` | Như trên | FC 10.2: khai báo tập trung, khóa phiên bản |
| Phiên bản thư viện (D02-L) | Plugin `org.openapi.generator` 7.25.0, Retrofit 3.0.0, OkHttp 4.12.0, Gson 2.13.2, gson-fire 1.9.0, `lifecycle-viewmodel` 2.10.0, `androidx.annotation` 1.9.1, JUnit 4.13.2; khóa trong `gradle/libs.versions.toml` | Đề xuất của Architect, chờ nhóm xác nhận | FC 1.4, FC 10.2; Gson, gson-fire, `androidx.annotation` ngoài danh sách FC 1.4 nên cần nhóm duyệt và cập nhật `Frontend-Architecture.md` |
| Phiên bản thư viện khác | Chưa khóa (Material Components, Navigation, Glide, MockWebServer...) | OPEN | FC 1.4: khóa khi task đầu tiên dùng và phải được nhóm duyệt |

Lý do chọn AGP 8.13.2 thay vì dòng 9.x: AGP 9 đổi mô hình DSL và bỏ API cũ mà một số plugin còn dùng. Plugin sinh client (D02-L) và Safe Args (D06-L) chưa được kiểm chứng với AGP 9 trong tài liệu này. Chuyển lên AGP 9 là một quyết định riêng của nhóm.

Hệ quả cần nhớ cho các ngày sau, không thuộc D01-L: với `targetSdk` 36, Android bắt buộc giao diện tràn viền; bố cục các màn hình phải xử lý inset hệ thống.

## 2. Checklist build (mẫu dùng lại)

Chạy trong thư mục `frontend/android`. Kết quả của từng lần chạy ghi ở mô tả pull request, không ghi vào tệp này.

- [ ] `java -version` cho JDK từ 17 đến 24
- [ ] Biến `ANDROID_HOME` trỏ tới Android SDK có `platforms;android-36` và `build-tools;35.0.0`
- [ ] `.\gradlew.bat --version` báo Gradle 8.13
- [ ] `.\gradlew.bat :app:assembleDebug` kết thúc `BUILD SUCCESSFUL`
- [ ] `.\gradlew.bat :app:testDebugUnitTest` kết thúc `BUILD SUCCESSFUL`
- [ ] `git status` không liệt kê `local.properties`, `build/`, `.gradle/`, `.idea/`
- [ ] Không có mã client sinh từ OpenAPI trong kho mã (FA 5.8)
- [ ] `app/openapi/openapi.yaml` trùng byte với `openapi.yaml` nguồn của backend
- [ ] Không có Activity, Fragment hay layout nào do D01-L tạo

## 3. Hợp đồng API

| Mục | Giá trị |
|---|---|
| Tệp hợp đồng trong dự án | `app/openapi/openapi.yaml` (FC 10.2) |
| Phiên bản | `info.version` 1.1.0, OpenAPI 3.0.3 |
| Tổng số thao tác | 88 |
| Nhóm cần sinh cho Android | Tag `Xác thực` (5 thao tác) và tag `Sinh viên` (16 thao tác), tổng 21 (ĐX-FA-16) |
| Bao đóng schema của 21 thao tác | 49 schema; không có `oneOf` hay `discriminator` |
| Schema có `discriminator` ngoài phạm vi | `KiemTraNguonRequest`, `LapPhieuMuonRequest`, `BaoCaoJson` (thuộc nhóm Quầy và Quản trị, không sinh) |
| Mã lỗi `MaLoi` | 32 giá trị |
| Quy tắc mã sinh | Sinh vào thư mục build, không commit, không sửa tay (FA 3.2, 5.8); kiểu thời gian `java.time`; giữ nguyên mã trạng thái lạ (ĐX-FC-21) |

Danh sách khóa. Cột "Android gọi" ghi màn hình theo Phụ lục B của `UI-Screens.md`.

| # | Phương thức và đường dẫn (sau `/api/v1`) | `operationId` | Cần token | Android gọi |
|---|---|---|---|---|
| 1 | POST `/auth/sinh-vien/dang-nhap` | `authDangNhapSinhVien` | Không | SCR-CM-01 |
| 2 | POST `/auth/nhan-vien/dang-nhap` | `authDangNhapNhanVien` | Không | Không gọi (web, SCR-CM-02) |
| 3 | POST `/auth/lam-moi` | `authLamMoi` | Không | SCR-CM-03 và tầng mạng |
| 4 | POST `/auth/dang-xuat` | `authDangXuat` | Không | SCR-CM-03; nút Đăng xuất ở SCR-SV-14 |
| 5 | GET `/auth/toi` | `authToi` | Có | Không gọi (web) |
| 6 | GET `/sv/tra-cuu/thiet-bi` | `svTraCuuThietBi` | Có | SCR-SV-02, SCR-SV-04 |
| 7 | GET `/sv/tra-cuu/thiet-bi/{maLoai}` | `svTraCuuThietBiChiTiet` | Có | SCR-SV-03 |
| 8 | POST `/sv/phieu-yeu-cau-muon` | `svGuiPhieuYeuCauMuon` | Có | SCR-SV-05 |
| 9 | GET `/sv/phieu-yeu-cau-muon` | `svDsPhieuYeuCauMuon` | Có | SCR-SV-06, SCR-SV-07 |
| 10 | GET `/sv/phieu-yeu-cau-muon/{ma}` | `svChiTietPhieuYeuCauMuon` | Có | SCR-SV-06, SCR-SV-07 |
| 11 | POST `/sv/dang-ky-cho` | `svDangKyCho` | Có | SCR-SV-03, SCR-SV-04, DLG-SV-01 |
| 12 | GET `/sv/dang-ky-cho` | `svDsDangKyCho` | Có | SCR-SV-08 |
| 13 | POST `/sv/dang-ky-cho/{ma}/roi` | `svRoiDangKyCho` | Có | SCR-SV-08 |
| 14 | GET `/sv/phieu-muon` | `svDsPhieuMuon` | Có | SCR-SV-09, SCR-SV-10 |
| 15 | GET `/sv/phieu-muon/{ma}` | `svChiTietPhieuMuon` | Có | SCR-SV-09, SCR-SV-10 |
| 16 | GET `/sv/bien-ban-vi-pham` | `svDsBienBanViPham` | Có | SCR-SV-11 |
| 17 | GET `/sv/thong-bao` | `svDsThongBao` | Có | SCR-SV-13 |
| 18 | GET `/sv/toi` | `svToi` | Có | SCR-SV-01, SCR-SV-03, SCR-SV-04, SCR-SV-14 |
| 19 | GET `/sv/vi-pham` | `svDsViPham` | Có | SCR-SV-12 |
| 20 | GET `/sv/phong-thiet-bi` | `svDsPhongThietBi` | Có | SCR-SV-02, SCR-SV-04 |
| 21 | GET `/sv/loai-thiet-bi` | `svDsLoaiThietBi` | Có | SCR-SV-02, SCR-SV-04 |

Ghi chú cho D02-L:

- Hai thao tác Xác thực (`authDangNhapNhanVien`, `authToi`) Android không gọi nhưng nằm cùng tag `Xác thực`. Cách lọc để sinh hoặc bỏ chúng do D02-L quyết định, không quyết định ở D01-L.
- Tên tag trong `openapi.yaml` có dấu và khoảng trắng (`Xác thực`, `Sinh viên`). D02-L phải kiểm tra bộ sinh lọc đúng theo tag và đặt tên lớp sinh đúng.
- Tên phương thức `Repository` trùng `operationId` (ĐX-FC-02).

## 4. Bảng truy vết màn hình Android

Tổng cộng 16 màn hình và 1 hộp thoại. Tên Fragment và tham số theo FA 5.3; luồng theo `UI-Screens.md` 2.1; use case theo `User-Flows.md` 2.3; API theo Phụ lục B của `UI-Screens.md`.

| Mã | Tên | Fragment | Tham số Safe Args | Vị trí điều hướng | Luồng | Use case | API (`operationId`) |
|---|---|---|---|---|---|---|---|
| SCR-CM-03 | Khởi động và khôi phục phiên | `KhoiDongFragment` | Không | Điểm bắt đầu của graph; không có thanh điều hướng dưới | UF-CM-03 | UC-19 | `authLamMoi`, `authDangXuat` |
| SCR-CM-01 | Đăng nhập sinh viên | `DangNhapSinhVienFragment` | Không | Không có thanh điều hướng dưới | UF-CM-01 | UC-19 | `authDangNhapSinhVien` |
| SCR-SV-01 | Trang chủ sinh viên | `TrangChuFragment` | Không | Tab Trang chủ | ĐX-UF-02, UF-SV-08 | UC-17, UC-02 | `svToi` |
| SCR-SV-02 | Tra cứu thiết bị | `TraCuuThietBiFragment` | Không | Tab Tra cứu | UF-SV-01 | UC-01 | `svTraCuuThietBi`, `svDsLoaiThietBi`, `svDsPhongThietBi` |
| SCR-SV-03 | Chi tiết loại thiết bị tại phòng | `ChiTietLoaiThietBiFragment` | `maLoai`, `maPhong` | Mở chồng lên | UF-SV-01, 02, 03 | UC-01, UC-02, UC-03 | `svTraCuuThietBiChiTiet`, `svToi`, `svDangKyCho` |
| SCR-SV-04 | Soạn phiếu yêu cầu mượn | `SoanPhieuYeuCauFragment` | `maPhong`, `maLoai` (cả hai tùy chọn); số lượng ban đầu 1 | Nav graph con soạn phiếu, dùng chung `SoanPhieuViewModel` với SCR-SV-05 | UF-SV-02, UF-SV-03 | UC-02, UC-03 | `svTraCuuThietBi`, `svDsLoaiThietBi`, `svDsPhongThietBi`, `svToi`, `svDangKyCho` |
| SCR-SV-05 | Xem lại và gửi phiếu yêu cầu | `XemLaiPhieuYeuCauFragment` | Không | Nav graph con soạn phiếu | UF-SV-02 | UC-02 | `svGuiPhieuYeuCauMuon` |
| SCR-SV-06 | Danh sách phiếu yêu cầu của tôi | `DanhSachPhieuYeuCauFragment` | Không | Mở chồng lên | UF-SV-05 | UC-02, UC-06 | `svDsPhieuYeuCauMuon`, `svChiTietPhieuYeuCauMuon` |
| SCR-SV-07 | Chi tiết phiếu yêu cầu mượn | `ChiTietPhieuYeuCauFragment` | `maPhieu`, `cheDoVuaGui` | Mở chồng lên | UF-SV-02, UF-SV-05 | UC-02, UC-06 | `svDsPhieuYeuCauMuon`, `svChiTietPhieuYeuCauMuon` |
| SCR-SV-08 | Danh sách đăng ký chờ của tôi | `DanhSachDangKyChoFragment` | Không | Mở chồng lên | UF-SV-03, UF-SV-04 | UC-03, UC-10 | `svDsDangKyCho`, `svRoiDangKyCho` |
| SCR-SV-09 | Lịch sử mượn/trả | `LichSuMuonTraFragment` | Không | Mở chồng lên | UF-SV-06 | UC-05 | `svDsPhieuMuon`, `svChiTietPhieuMuon` |
| SCR-SV-10 | Chi tiết phiếu mượn | `ChiTietPhieuMuonFragment` | `maPhieu` | Mở chồng lên | UF-SV-06 | UC-05 | `svDsPhieuMuon`, `svChiTietPhieuMuon` |
| SCR-SV-11 | Biên bản và phí phạt | `BienBanPhiPhatFragment` | Không | Mở chồng lên | UF-SV-06 | UC-05 | `svDsBienBanViPham` |
| SCR-SV-12 | Danh sách vi phạm | `DanhSachViPhamFragment` | Không | Mở chồng lên | UF-SV-06 | UC-05 | `svDsViPham` |
| SCR-SV-13 | Thông báo | `ThongBaoFragment` | Không | Tab Thông báo | UF-SV-07 | UC-17, FR-24 | `svDsThongBao` |
| SCR-SV-14 | Hồ sơ mượn | `HoSoMuonFragment` | `cheDoBiChan`, `hanhDongDaChon` | Tab Hồ sơ | UF-SV-08 | UC-17, UC-02 | `svToi`; nút Đăng xuất gọi `authDangXuat` |
| DLG-SV-01 | Hộp thoại Đã đăng ký chờ | `DaDangKyChoDialogFragment` | Không | Hộp thoại, có nút tới SCR-SV-08 | UF-SV-03 | UC-03 | `svDangKyCho` |

Thanh điều hướng dưới có bốn mục: Trang chủ (SCR-SV-01), Tra cứu (SCR-SV-02), Thông báo (SCR-SV-13), Hồ sơ (SCR-SV-14). Mỗi tab giữ ngăn xếp quay lại riêng (FA 5.3).

Mọi lối vào SCR-SV-04 (từ SCR-SV-01, 03, 06, 07) và hành động Đăng ký chờ (SCR-SV-03, 04) phải kiểm tra quyền mượn bằng `svToi` tại thời điểm đó; quyền khác Bình thường thì mở SCR-SV-14 với `cheDoBiChan` và `hanhDongDaChon` (FA 5.3, quy tắc 3).

## 5. Điều hướng dự kiến

Theo sơ đồ `UI-Screens.md` 2.2. Đây là bản ghi truy vết; nav graph thật do D06-L dựng.

| Từ | Đến | Điều kiện |
|---|---|---|
| SCR-CM-03 | SCR-CM-01 | Không có phiên hoặc làm mới thất bại |
| SCR-CM-03 | SCR-SV-01 | Khôi phục được phiên |
| SCR-CM-01 | SCR-SV-01 | Đăng nhập thành công |
| SCR-SV-01 | SCR-SV-02, SCR-SV-06, SCR-SV-08, SCR-SV-09, SCR-SV-11, SCR-SV-12, SCR-SV-14 | Lối vào chức năng |
| SCR-SV-01 | SCR-SV-04 | Đặt trước thiết bị |
| SCR-SV-02 | SCR-SV-03 | Chọn một loại thiết bị |
| SCR-SV-03 | SCR-SV-04 | Đặt trước, loại còn hàng |
| SCR-SV-03 | DLG-SV-01 | Đăng ký chờ, loại hết hàng |
| SCR-SV-03 | SCR-SV-14 | Quyền mượn bị chặn |
| SCR-SV-04 | SCR-SV-05 | Xem lại phiếu |
| SCR-SV-04 | DLG-SV-01 | Đăng ký chờ cho loại hết hàng |
| SCR-SV-05 | SCR-SV-07 | Gửi thành công |
| SCR-SV-05 | SCR-SV-04 | Lỗi cần sửa |
| SCR-SV-05 | SCR-SV-14 | Quyền mượn bị chặn |
| SCR-SV-06 | SCR-SV-07 | Chọn một phiếu |
| SCR-SV-06 | SCR-SV-04 | Tạo phiếu mới |
| SCR-SV-07 | SCR-SV-04 | Tạo phiếu mới |
| SCR-SV-07 | SCR-SV-09 | Phiếu đã nhận |
| SCR-SV-08 | SCR-SV-09 | Đã nhận |
| SCR-SV-08 | SCR-SV-12 | Hết hạn |
| DLG-SV-01 | SCR-SV-08 | Nút trong hộp thoại |
| SCR-SV-09 | SCR-SV-10 | Chọn một phiếu mượn |
| SCR-SV-11 | SCR-SV-10 | Chọn một phiếu mượn |
| SCR-SV-12 | SCR-SV-11 | Xem biên bản |
| SCR-SV-14 | SCR-SV-11 | Có biên bản chưa nộp |
| SCR-SV-13 | SCR-SV-06, SCR-SV-08, SCR-SV-09, SCR-SV-11 | Chọn thông báo theo `loaiThongBao` (FA 5.3, quy tắc 4) |

## 6. Phạm vi Android giai đoạn 1

- 17 use case giai đoạn 1: UC-01, 02, 03, 05, 06, 07, 08, 09, 10, 11, 12, 13, 14, 15, 16, 17, 19 (`User-Flows.md` 2.2).
- Use case có màn hình hoặc hiệu ứng nhìn thấy trên Android qua các luồng ở mục 4: UC-01, 02, 03, 05, 06, 10, 17, 19.
- Use case chỉ làm trên web, Android không có màn hình: UC-07, 08, 09, 11, 12, 13, 14, 15, 16.
- Ngoài giai đoạn 1, không có màn hình hay hành động: UC-04, 18, 20, 21, 22, 23, 24 (FA-09).
- Không có trên Android: tự đăng ký, quên mật khẩu, đổi mật khẩu, thông báo đẩy, đánh dấu đã đọc thông báo, chọn hoặc tải ảnh lên, quét mã bằng camera.

## 7. Quyết định G1–G3

Trạng thái chung: **OPEN**. Định nghĩa gốc của G1–G7 nằm trong `KE_HOACH_CHI_TIET_40_NGAY.md`; tệp này chưa có trong tài liệu dùng để lập D01-L (xác minh ngày 13/11/2026). Vì vậy tệp này không diễn giải nội dung của G1–G3.

| Quyết định | Trạng thái | Task trong `BANG_THEO_DOI_40_NGAY.md` có nhắc |
|---|---|---|
| G1 | OPEN | D01-K, D01-V, D01-L, D02-K, D08-V, D09-V, D13-K, D14-T, D16-T, D19-T, D20-V, D30-T, D31-V, D31-T, D38-V |
| G2 | OPEN | D01-K, D01-V, D01-L, D02-K, D08-V, D09-V, D13-K, D15-T, D16-T, D18-T, D19-T, D20-V, D30-T, D31-V, D31-T, D38-V |
| G3 | OPEN | D01-K, D01-L, D02-K, D12-N, D13-K, D13-N, D14-N, D15-N, D24-N, D24-T, D26-T, D27-N, D30-N, D30-T, D34-K, D34-N, D34-T, D36-N, D38-N |

- D02-K, bước 3 ghi sẽ "Chốt G1/G2/G3/G5/G7 từ phương án đã chuẩn bị".
- Ngoài D01-L, không task Android (cột L) nào trong `BANG_THEO_DOI_40_NGAY.md` nhắc G1–G3. Điều này cần xác nhận lại khi có `KE_HOACH_CHI_TIET_40_NGAY.md`.
- Quy tắc: Android không tự đặt chính sách cho G1–G3. Khi một task Android cần kết quả của chúng, giữ gate OPEN theo `BANG_THEO_DOI_40_NGAY.md` cho đến khi có quyết định được duyệt.

## 8. Quyết định D02-L

| # | Quyết định | Lý do và căn cứ |
|---|---|---|
| 1 | Sinh cả 21 thao tác của hai tag, gồm `authDangNhapNhanVien` và `authToi` dù Android không gọi | ĐX-FA-16 định nghĩa phạm vi theo tag; lọc theo `operationId` phải bảo trì danh sách khi hợp đồng đổi |
| 2 | Lọc bằng quy tắc normalizer `FILTER` giá trị `tag:XacThuc\|SinhVien` | Cơ chế chính thức của openapi-generator: thao tác không khớp bị đánh `x-internal` và không sinh |
| 3 | Bản hợp đồng Android `app/build/openapi/openapi-android.yaml` do task `taoHopDongAndroid` tạo; `app/openapi/openapi.yaml` không bị sửa | Giữ nguyên hợp đồng nguồn |
| 4 | Bản Android bỏ khóa `enum` trong `components.schemas`: mọi mã trạng thái, loại, kết quả, vai trò, `maLoi` là `String` | Enum Java do bộ sinh tạo ném lỗi (hoặc trả `null` nếu trường nullable) khi gặp mã lạ; `enumUnknownDefaultCase` làm mất mã gốc. FA 5.5 và ĐX-FC-21 yêu cầu hiển thị nguyên mã |
| 5 | Đổi tag `Xác thực` thành `XacThuc`, `Sinh viên` thành `SinhVien` trong bản Android | Lớp API sinh ra có tên ASCII: `XacThucApi`, `SinhVienApi` |
| 6 | Chỉ sinh tệp hỗ trợ `JSON.java`; không sinh `ApiClient` và lớp xác thực sinh sẵn | `Retrofit` và `OkHttpClient` do `AppContainer` dựng ở D04-L (FA 5.5) |
| 7 | `schemaMappings` loại 3 schema `discriminator` (`KiemTraNguonRequest`, `LapPhieuMuonRequest`, `BaoCaoJson`) | ĐX-FA-16 |
| 8 | Sinh kiểu thời gian `java.time` (`dateLibrary=java8`), Gson | FC 6.4 |
| 9 | `ApiCallback`, `ApiError`, `UiState`, `Event`, `AppContainer`, `UngDung`, `AppViewModelFactory` đã có; chưa có `Repository` cụ thể | Repository đầu tiên là `XacThucRepository` ở D06-L; cần Retrofit và bộ chuẩn hóa lỗi của D04-L |

Cảnh báo cho D04-L (mâu thuẫn M1, chưa giải quyết): FA 3.2 và FC 6.4 coi `/api/v1` là hằng số của `data/api` còn `API_BASE_URL` không có `/api/v1`; nhưng mỗi `path` trong `openapi.yaml` đã chứa `/api/v1`. Dòng chú thích do bộ sinh tạo, quan sát được ngày D02-L:

- @GET("api/v1/sv/phieu-muon/{ma}")
- @GET("api/v1/sv/phieu-yeu-cau-muon/{ma}")
- @POST("api/v1/sv/dang-ky-cho")

Không thêm hằng số `/api/v1` vào địa chỉ gốc khi dựng Retrofit nếu các dòng trên đã bắt đầu bằng `api/v1/` hoặc `/api/v1/`; cần nhóm xác nhận cách xử lý trước khi cài D04-L.

Chưa có trong D02-L: `NhomLoi` (FA 3.4, FC 7.4) và `LoiTruong` (FC 9.3) không nằm trong task nào của `BANG_THEO_DOI_40_NGAY.md`, cần người dùng giao việc; chuẩn hóa lỗi (`ApiErrorParser`) thuộc D04-L bước 2.
