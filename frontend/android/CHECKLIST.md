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
| Phiên bản thư viện (D03-L) | `androidx.lifecycle:lifecycle-livedata` 2.10.0, dùng chung khóa `lifecycle` trong `gradle/libs.versions.toml` | Đề xuất của Architect, chờ nhóm xác nhận | FC 1.4, FC 10.2; LiveData thuộc ĐX-FA-12 nhưng artifact chưa nằm trong danh sách đã khóa |
| Phiên bản thư viện (D05-L) | `com.google.android.material:material` 1.14.0, khóa `material` trong `gradle/libs.versions.toml` | Đề xuất của Architect, chờ nhóm xác nhận | FC 1.4, FC 10.2; xem mục 10 |
| Phiên bản thư viện (D06-L) | `androidx.navigation:navigation-fragment` 2.9.8 và plugin Safe Args `androidx.navigation.safeargs` 2.9.8 (khóa `navigation`); `com.squareup.retrofit2:converter-gson` 3.0.0 (dùng chung khóa `retrofit`) | Đề xuất của Architect, chờ nhóm xác nhận | FC 1.4, FC 10.2; xem mục 11 |
| Phiên bản thư viện khác | Chưa khóa (Glide...); MockWebServer đã khóa ở D04-L, Navigation ở D06-L | OPEN | FC 1.4: khóa khi task đầu tiên dùng và phải được nhóm duyệt |

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

Theo sơ đồ `UI-Screens.md` 2.2. Đây là bản ghi truy vết; nav graph thật nằm ở `app/src/main/res/navigation/nav_graph.xml` (D06-L), hiện có sáu điểm đến: `khoiDongFragment` (điểm bắt đầu), `dangNhapSinhVienFragment` và bốn tab gốc; các màn hình mở chồng thêm ở các ngày sau.

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

## 9. Quyết định D03-L

Nguồn: FA 5.4, 5.10; FC 6.5, 6.8, 9.1–9.4; ĐX-FA-13, ĐX-FA-14. Mọi quyết định dưới đây là đề xuất của Architect, chờ nhóm xác nhận.

| # | Quyết định | Lý do và căn cứ |
|---|---|---|
| 1 | Mỗi token mã hóa riêng; chuỗi lưu là `Base64(IV)` + `:` + `Base64(bản mã)` bằng `java.util.Base64`; hai giá trị ghi trong một `commit()` | FA 5.4; `java.util.Base64` có từ API 26 (ĐX-FA-13); `commit()` để cặp token mới xuống đĩa trước khi replay (FA 3.5) |
| 2 | Ranh giới mã hóa `TokenCipher`, lỗi là `GeneralSecurityException`; bản thật `KeystoreCipher` | Kế hoạch D03-L; FC 9.1 (boundary mã hóa giả khi kiểm thử JVM) |
| 3 | Tệp `SharedPreferences` tên `phien_dang_nhap`; `allowBackup="false"` cùng `backup_rules.xml` và `data_extraction_rules.xml` loại tệp này; `CauHinhSaoLuuPhienTest` kiểm sự trùng khớp | FA 5.4, FC 6.5, ĐX-FA-14 |
| 4 | `SessionManager` phát `TrangThaiPhien` (`CHUA_KHOI_PHUC`, `CO_PHIEN`, `KHONG_CO_PHIEN`) bằng `postValue`; token lấy qua `getTokenPair()` | FA 5.1, 5.3 quy tắc 5; D04-L gọi từ luồng mạng |
| 5 | Chưa nối `SessionManager` vào `AppContainer`; `TokenStore.create(Context)` sẵn để nối ở D04-L | `AppContainer` cần `Context`; người dùng đầu tiên là `AuthInterceptor` (D04-L) |
| 6 | Test `SessionManager` thay bộ thực thi của `LiveData` bằng `ArchTaskExecutor.setDelegate`, không thêm `core-testing` | FC 9.1 cấm thêm thư viện kiểm thử |

Phạm vi kiểm chứng của D03-L: test trong `src/test` chạy trên JVM với boundary mã hóa giả; **không** chứng minh Android Keystore thật và **không** chứng minh hệ điều hành loại tệp khỏi sao lưu. Kiểm tra trên thiết bị/emulator theo FC 9.4 thuộc D18-L: **chưa chạy**.

Chưa có trong D03-L (thuộc ngày sau): kiểm hạn `exp` của JWT khi khởi động và điều hướng theo `TrangThaiPhien` (D06-L), `AuthInterceptor`/`TokenAuthenticator` và lý do `ApiError` khi kết thúc phiên (D04-L).

## 10. Quyết định D05-L

Nguồn: DS 3 đến 9, Phụ lục A đến C; FC 3.3, 3.5, 6.6, 7.1 đến 7.3, 8.4, 9.3, 10.3, Phụ lục A; FA 5.2; UI-Screens 3.1 đến 3.4; `BANG_THEO_DOI_40_NGAY.md` D05-L. Mọi quyết định dưới đây là đề xuất của Architect, chờ nhóm xác nhận.

| # | Quyết định | Lý do và căn cứ |
|---|---|---|
| 1 | Khóa Material Components `1.14.0` trong `libs.versions.toml` (khóa `material`) | FC 1.4: khóa khi task đầu tiên dùng và phải được nhóm duyệt. Bản ổn định 13/05/2026 yêu cầu compileSdk 35 trở lên, minSdk 23 trở lên, dựng bằng AGP 8.11.1; tương thích compileSdk 36, minSdk 26, AGP 8.13.2 của dự án |
| 2 | Theme `Theme.QuanLyMuonTra` kế thừa `Theme.MaterialComponents.Light.NoActionBar` (dòng M2), áp dụng ở `<application>`, `android:forceDarkAllowed=false` | FC 6.6 yêu cầu biến thể sáng của Material Components, không DayNight. Dòng M2 gần Design-System hơn M3 (bo góc 8 dp, thẻ không đổ bóng, nút cao 48 dp) |
| 3 | Token Android chỉ gồm token áp dụng cho Android: bỏ `hover`, `-pressed` của nút, `text.link-hover`, `border.control-hover`, `size.control.*`, `size.table.*`, `z.*`, `layout.web.*`, `motion.*` | DS mục 6 (nhấn trên Android là gợn sóng `ds_color_overlay_pressed`), DS Phụ lục C (token riêng nền tảng). Token `motion.*` thêm khi có thành phần dùng chuyển động |
| 4 | Độ đậm 400 và 700 dùng `sans-serif`, độ đậm 500 dùng `sans-serif-medium`, mã dùng `monospace`; không gói phông | ĐX-DS-04; `android:textFontWeight` cần API 28 trong khi minSdk là 26 |
| 5 | Nhãn trạng thái tra theo cặp (`HoTrangThai`, mã) trong `NhanTrangThai`; mã lạ hiển thị nguyên mã, tông trung tính, không biểu tượng; tra phân biệt chữ hoa chữ thường; `null` hiển thị dấu gạch dài | DS 7.1, FC 7.3, UI-Screens 3.3; mã API là chữ hoa cố định (mọi giá trị enum trong `openapi.yaml`) |
| 6 | Thêm `HangSo.MUI_GIO_VN`, `HangSo.NGUONG_THONG_BAO_KHOI_DONG_GIAY` (8 giây) và `HangSo.GIA_TRI_THIEU` (U+2014) | FC 7.1, ĐX-FA-04, ĐX-DS-12; mỗi hằng số một nơi định nghĩa |
| 7 | Thêm lớp `TextLinkView` (kế thừa `MaterialTextView`) cho CMP-03 | XML không có thuộc tính gạch chân mà DS CMP-03 (kèm NT-02) yêu cầu luôn gạch chân. Phụ lục A của FC chỉ ghi `Widget.Ds.Link`; cần bổ sung tên lớp này vào Phụ lục A khi nhóm duyệt |
| 8 | `ErrorView` chọn nội dung bằng `TrinhBayLoi.choVung`: `MANG` và `QUA_THOI_GIAN` là lỗi mạng; mọi lỗi HTTP là lỗi hệ thống kèm `traceId` nếu có; `HUY` ẩn view | DS 9.4, 9.8; FA 3.3 quy tắc 4; FA 3.4 đoạn cuối (mã không có xử lý riêng và phản hồi không phải problem+json) |
| 9 | Biểu tượng lỗi mạng và rỗng dùng tông trung tính; biểu tượng lỗi hệ thống dùng `ds_color_tone_danger_icon` | DS 8 CMP-25 chỉ chỉ định màu cho lỗi hệ thống; rỗng không phải lỗi (DS 9.3) |
| 10 | Ô nhập: nhãn là `TextView` riêng phía trên ô (có `android:labelFor`), tắt nhãn nổi `hintEnabled`; biểu tượng lỗi là biểu tượng tích hợp của `TextInputLayout` ở cuối ô | DS CMP-04 đặt nhãn luôn hiện phía trên; `TextInputLayout` không có biểu tượng đầu dòng lỗi |
| 11 | Thẻ chỉ có hình thức tĩnh `Widget.Ds.Card`; biến thể chạm được, chọn được, theo tông thêm khi màn hình dùng đến | Kế hoạch D05-L chỉ yêu cầu hình thức nền |
| 12 | Tiền tố id `ll_`, `fl_`, `pi_` cho `LinearLayout`, `FrameLayout`, `CircularProgressIndicator` | FC 3.3 chỉ liệt kê các tiền tố `btn_`, `tv_`, `et_`, `til_`, `rv_`, `img_`, `chip_`, `srl_`; cần bổ sung khi nhóm duyệt |
| 13 | `TokenThietKeTest` thực hiện cục bộ phép kiểm TC-01 (không mã màu hay kích thước thô ngoài tệp token) bằng đọc tệp trong `src/main` | FC 10.3 mô tả bước grep trên CI; CI Android chưa tồn tại nên kiểm bằng test để chạy được ở mọi máy |
| 14 | Điều chỉnh sau review PR #31: đặt `colorPrimaryVariant`, `colorSecondaryVariant`, `android:statusBarColor` và `windowLightStatusBar` trong theme; thêm `android:minWidth` cho `Widget.Ds.Link`; đặt `setAccessibilityLiveRegion` trước khi đổi hiển thị trong `BannerView`; coi chuỗi rỗng là giá trị thiếu trong `ThongTinTrangThai.maLa`; bọc `Collections.unmodifiableSet` trong `NhanTrangThai.maDaBiet` | Ngăn rò rỉ màu mặc định tím trên API 26–34, bảo đảm vùng chạm 48×48 dp (TC-06), TalkBack đọc banner ngay lần đầu (kiểm tra D29-L), chuẩn hóa giá trị thiếu (DS 9.7) và bảo vệ bảng ánh xạ |

Biểu tượng (`ic_check_circle`, `ic_info`, `ic_warning`, `ic_error`, `ic_inbox`, `ic_cloud_off`) lấy từ Material Icons kiểu Outlined (Apache License 2.0); Design-System ghi Material Symbols Outlined độ đậm 400. Hình hai bộ gần như trùng nhau nhưng chưa được so bằng mắt: **chưa chạy** (mở từng tệp trong Android Studio để xem trước).

Phạm vi kiểm chứng của D05-L: test trong `src/test` chạy trên JVM và kiểm token, bảng nhãn, định dạng và quy tắc chọn nội dung lỗi. **Không** chứng minh giao diện thật: bố cục, màu, bo góc, chiều cao 48 dp, hành vi `LoadingView` sau 8 giây, giảm chuyển động, chữ cỡ 1,3 lần, TalkBack. Các kiểm tra này cần thiết bị hoặc emulator (FC 9.4, TC-02, TC-05, TC-06): **chưa chạy**, thuộc D19-L (rà giao diện Android: TC-01 đến 06, 10, 11, 14, 15) và D29-L (rà tiếp cận, bằng chứng thiết bị).

Chưa có trong D05-L (thuộc ngày sau): `BottomNav`, `TopBar`, hộp thoại xác nhận, `MainActivity`, điều hướng (D06-L); ô tìm kiếm, ô chọn, nút phân đoạn, chip lọc, dòng danh sách (D08-L); khung ảnh (D09-L); chữ "Đang xử lý" và trạng thái đang xử lý của nút (thêm khi màn hình đầu tiên cần, dự kiến SCR-CM-01 ở D06-L); cấu hình lint và spotless cho Android (D19-L chạy chúng); `ic_search_off` (D08-L); Navigation và Glide chưa được khóa phiên bản.

Điểm cần người dùng xác nhận (chưa có ngày nào sở hữu): ghi chú nguồn của D05-L trích FC 7.4 và bước 4 nói "lỗi từng vùng", nhưng không bước nào của D05-L yêu cầu `NhomLoi` và `ApiError.nhom()` (FC 7.4, kèm kiểm thử 32 mã ở FC 9.3) hay `LoiTruong` (lỗi theo trường, FC 9.3); mục 8 đã ghi `NhomLoi` không nằm trong task nào. D05-L không làm các phần này vì không cần chúng để đạt kết quả của ngày; "lỗi từng vùng" được hiểu là lỗi theo vùng tải dữ liệu, mỗi vùng một `ErrorView` (ĐX-UI-04). Nếu ý định là giao `NhomLoi` hoặc `LoiTruong` cho D05-L hay ngày khác thì cần ghi vào kế hoạch.

## 11. Quyết định D06-L

Nguồn: FA 3.3 đến 3.5, 3.11, 5.1, 5.3 đến 5.5, 5.8; FC 3.3, 6.2 đến 6.5, 6.8, 9.1, Phụ lục A; DS CMP-01, CMP-24 đến CMP-28, 9.2, 9.4, 9.8; UI-Screens 3.5, 4.1, 4.4, 4.6; User-Flows 3.1; Wireframes SCR-CM-01, SCR-CM-03; `BANG_THEO_DOI_40_NGAY.md` D06-L. Mọi quyết định dưới đây là đề xuất của Architect, chờ nhóm xác nhận.

| # | Quyết định | Lý do và căn cứ |
|---|---|---|
| 1 | Khóa `androidx.navigation:navigation-fragment` và plugin Safe Args `androidx.navigation.safeargs` cùng phiên bản `2.9.8` (khóa `navigation`); không thêm `navigation-ui` | FC 1.4: khóa khi task đầu tiên dùng và phải được nhóm duyệt. 2.9.8 là bản ổn định cuối của dòng 2.9 (22/04/2026); Safe Args từ 2.9.6 tương thích mọi AGP từ 8.4.2. Không chọn 2.10.2 (23/09/2026, mới nhất) vì chưa kiểm chứng yêu cầu compileSdk của các thư viện AndroidX đi kèm: `androidx.core` 1.19.0 trở lên yêu cầu compileSdk 37 trong khi dự án dùng compileSdk 36. Chuyển tab không dùng `NavigationUI` (quyết định 5) nên không cần `navigation-ui` |
| 2 | Khóa `com.squareup.retrofit2:converter-gson` `3.0.0`, dùng chung khóa `retrofit` | Bộ sinh chỉ sinh `JSON.java` (D02-L), không sinh bộ chuyển đổi Retrofit; cần một `Converter.Factory` cho Gson để Retrofit đọc ghi thân JSON bằng `JSON.getGson()` (FA 5.5: bộ chuyển đổi do bộ sinh cấu hình, bỏ qua trường lạ). Cùng phiên bản Retrofit 3.0.0 đã khóa |
| 3 | `BuildConfig.API_BASE_URL` theo buildType: debug `http://10.0.2.2:5283/`, release `https://chua-cau-hinh.invalid/` | FA 5.5, 5.8; FC 10.2. 5283 là cổng http trong `launchSettings.json` của backend, 10.0.2.2 là địa chỉ máy tính nhìn từ emulator (thiết bị thật không dùng được). Máy chủ production chưa có (triển khai lần đầu ở D12-K): `.invalid` do RFC 2606 dành riêng nên bản release không thể gọi nhầm máy chủ nào. **OPEN**: nhóm điền địa chỉ production khi có |
| 4 | Chỉ bản debug được gọi http, và chỉ tới `10.0.2.2` (`src/debug/AndroidManifest.xml` và `network_security_config_debug.xml`); `main` giữ `usesCleartextTraffic=false` | FC 6.8, FA 5.5; thực hiện ghi chú D04-L trong manifest. Bản release không có cấu hình mạng riêng |
| 5 | Chuyển tab bằng `NavOptions` (một bản duy nhất, lưu và khôi phục trạng thái, `popUpTo` graph gốc có `saveState`), không dùng `NavigationUI.setupWithNavController`; chạm lại tab đang chọn thì về màn hình gốc của tab | FA 5.3 ghi "NavigationUI với lưu trạng thái". Theo mã nguồn của `NavigationUI`, hàm chọn mục `popUpTo` điểm bắt đầu của graph; ở đây là `KhoiDongFragment`, đã bị gỡ khỏi ngăn xếp sau khi khởi động, nên các tab chồng lên nhau và không lưu được trạng thái. `popUpTo` graph gốc làm mỗi tab có khóa lưu riêng là mã của chính tab (tài liệu `NavOptions.Builder.setPopUpTo`: khi `inclusive=false` khóa là mã destination bị gỡ cuối cùng). Chuyển tab gọi `navigate` bằng mã mục menu (trùng mã điểm đến, như `NavigationUI` làm), là ngoại lệ có chủ đích của FC 6.2 "không `navigate(R.id.…)` với id thô" vì tab không có `Directions`. **Chưa chạy trên thiết bị**; cần nhóm xác nhận hướng này thay cho chữ "NavigationUI" của FA 5.3 |
| 6 | Phiên kết thúc: `MainActivity` quan sát `TrangThaiPhien`; khi `KHONG_CO_PHIEN` và điểm đến hiện tại không phải `KhoiDongFragment` hay `DangNhapSinhVienFragment` thì dùng action toàn cục `action_global_to_dangNhapSinhVienFragment` (`popUpTo` graph gốc, `inclusive`) rồi `clearBackStack` cho cả bốn tab | FA 5.3 quy tắc 5, FA 3.5 (xóa dữ liệu phiên trước); trạng thái các tab đã lưu nằm trong `NavController` nên phải xóa riêng. Ở hai màn hình chung không làm gì vì `KhoiDongFragment` tự điều hướng và đang ở đăng nhập thì không cần |
| 7 | Tiến trình bị hệ điều hành hủy rồi dựng lại (`savedInstanceState` khác `null` và trạng thái còn `CHUA_KHOI_PHUC`): `MainActivity` gọi `SessionManager.restore()` | Ngăn xếp điều hướng được khôi phục nhưng `SessionManager` mới còn trống; nếu không còn phiên thì quyết định 6 đưa người dùng về đăng nhập |
| 8 | `MainActivity` xử lý giao diện tràn viền: `WindowCompat.setDecorFitsSystemWindows(false)` và đặt padding khối gốc theo `systemBars`, `displayCutout`, `ime` | Hệ quả `targetSdk` 36 đã ghi ở mục 1; nếu không, thanh tiêu đề nằm dưới thanh trạng thái |
| 9 | `XacThucRepository` không trả token ra ngoài tầng dữ liệu (`ApiCallback<Void>`); đăng nhập và làm mới ghi token qua `SessionManager`; làm mới lỗi (mọi loại) gọi `ketThucPhien(ApiError)`; đăng xuất xóa phiên cục bộ **trước** khi gửi và báo kết quả máy chủ qua callback | FC 6.5 (token chỉ qua `SessionManager`), FA 5.4 (mọi lỗi làm mới kết thúc phiên kèm `ApiError`), UI-Screens 4.6 và User-Flows 3.1 (xóa phiên cục bộ kể cả khi đăng xuất lỗi; chưa biết máy chủ đã thu hồi thì không tự gửi lại) |
| 10 | `HanAccessToken.conHan`: so `exp` (giây) với thời điểm hiện tại, không có dung sai; token không đọc được hoặc thiếu `exp` coi là hết hạn | FA 5.4: chỉ để tránh một lần gọi chắc chắn bị 401; máy chủ vẫn kiểm lại. Hết hạn mà thực ra còn hạn thì chỉ tốn một lần làm mới; còn hạn mà thực ra hết thì `TokenAuthenticator` làm mới khi gặp 401 |
| 11 | Bảng chọn nội dung lỗi của SCR-CM-01 (`TrinhBayLoiDangNhap`): `DANG_NHAP_SAI` đặt dưới ô mật khẩu; `CHUA_XAC_THUC` là thông báo `info` phiên hết hạn; 429 là `warning` chờ rồi thử lại; mạng và quá thời gian là `danger` lỗi kết nối; còn lại là lỗi hệ thống kèm `traceId`; yêu cầu bị hủy không hiển thị gì. Dùng chung cho lỗi của lần đăng nhập và lý do kết thúc phiên | UI-Screens 4.4, DS 9.4, FA 3.3 quy tắc 1 và 4, FA 3.4. `TAI_KHOAN_BI_KHOA` không có xử lý riêng ở D06-L vì hợp đồng của `authDangNhapSinhVien` và `authLamMoi` không có 403 |
| 12 | Hai câu do Architect viết vì tài liệu chỉ nêu ý: `qua_nhieu_yeu_cau` ("Bạn thao tác quá nhiều lần, vui lòng chờ một lúc rồi thử lại") và `loi_ket_noi_may_chu` ("Không kết nối được máy chủ. Kiểm tra kết nối rồi thử lại.") | DS 9.4 và UI-Screens 4.4 chỉ nêu "chờ rồi thử lại" và "lỗi mạng"; DS 9.8 quy định xưng "Bạn", câu đơn không dấu chấm cuối. Các chữ còn lại lấy nguyên văn tài liệu. Chờ nhóm duyệt chữ |
| 13 | SCR-CM-01 và SCR-CM-03 không có thanh tiêu đề; SCR-CM-03 không có dòng chữ "Đang kiểm tra phiên" | DS CMP-28 ("mọi màn hình sinh viên trừ SCR-CM-01 và SCR-CM-03"), UI-Screens 4.1, 4.6 chỉ quy định chỉ báo tải. Wireframe vẽ dòng tiêu đề và dòng chữ đó, mâu thuẫn với DS và UI-Screens; Design-System quyết định hình thức nên theo DS. **Cần nhóm xác nhận** |
| 14 | Nền chỉ báo mục đang chọn của `BottomNavigationView` rộng `ds_size_touch_min` và cao `ds_size_chip_height` (48 × 32 dp); bo tròn bằng `50%`; biểu tượng Tra cứu dùng chung một hình cho hai trạng thái | DS CMP-27 chỉ nêu màu `color.nav.indicator` và `radius.full`, không nêu kích thước; hai token sẵn có được dùng lại. Material Icons không có `search` dạng đặc riêng. Cần nhóm xác nhận hoặc bổ sung kích thước vào DS |
| 15 | Bốn Fragment khung tab rỗng (`TrangChuFragment`, `TraCuuThietBiFragment`, `ThongBaoFragment`, `HoSoMuonFragment`) cùng layout rỗng, tạo ở D06-L | Kế hoạch yêu cầu "các tuyến gốc hoạt động" và "bốn tab giữ ngăn xếp"; không có điểm đến thì không dựng được. Các ngày sau **sửa** chính các tệp này, không tạo mới: SCR-SV-01 và SCR-SV-14 (D07-L), SCR-SV-02 (D08-L), SCR-SV-13 theo ngày của nó |
| 16 | Tiền tố id `tb_` (thanh tiêu đề), `bnv_` (thanh điều hướng dưới), `fcv_` (vùng chứa Fragment), `v_` (đường kẻ), `lv_` (`LoadingView`) | FC 3.3 chưa có các tiền tố này; cần bổ sung khi nhóm duyệt |
| 17 | `NutDangXuLy.batDau/ketThuc` cho trạng thái "Đang xử lý" của nút (chữ đổi, vô hiệu, chỉ báo quay `IndeterminateDrawable`, giữ chiều rộng), dùng ở nút Đăng nhập và nút xác nhận của hộp thoại | DS CMP-01, CMP-26, 9.8; CHECKLIST mục 10 đã hẹn thêm khi SCR-CM-01 cần |
| 18 | `XacNhanDialogFragment` gửi kết quả bằng Fragment Result API và ở lại trạng thái đang xử lý tới khi bên gọi gọi `datDangXuLy(false)` hoặc `dismiss()`; hiển thị bằng `childFragmentManager` của màn hình; trạng thái đang xử lý giữ qua xoay màn hình | DS CMP-26, 9.2; hộp thuộc `childFragmentManager` nên bị hủy cùng màn hình khi phiên kết thúc. Hộp không phải điểm đến của nav graph nên nội dung hiển thị truyền bằng `Bundle` arguments của `DialogFragment` (ngoại lệ của FC 6.2 "không dùng `Bundle` thủ công"; Safe Args chỉ áp dụng cho điểm đến). Chưa có màn hình nào gọi hộp ở D06-L |

