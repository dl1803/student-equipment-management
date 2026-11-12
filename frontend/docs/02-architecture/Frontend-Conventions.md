# FRONTEND CONVENTIONS

## Hệ thống quản lý mượn trả thiết bị học tập cho sinh viên

| Mục | Nội dung |
|---|---|
| Dự án | SE100 – Nhóm 13 |
| Tài liệu | `Frontend-Conventions.md` |
| Phiên bản | 1.0 |
| Ngày lập | 05/11/2026 |
| Tài liệu căn cứ | `Frontend-Architecture.md` v1.0 (01/11/2026); `Design-System.md` v1.0 (01/11/2026); `UI-Screens.md` v1.0 (31/10/2026); `User-Flows.md` v1.0 (30/10/2026); `Wireframes.md` v1.0 (02/11/2026, chỉ để đối chiếu); `Backend-Conventions.md` (mục 3.1, 17.3, 18.3, 19); `API-Notes.md` v1.1.0 và `openapi.yaml` v1.1.0; `DatabaseDesign.md` (chỉ để xác nhận 2 MB là 2.097.152 byte) |

> Tài liệu quy định **cách viết mã và cách phối hợp** khi hiện thực hai ứng dụng frontend (web Next.js cho NV và QT, Android Java cho SV) để mọi thành viên làm theo cùng một chuẩn. Tài liệu **không thêm yêu cầu nghiệp vụ, màn hình hay công nghệ mới**, không lặp lại kiến trúc (xem `Frontend-Architecture.md`, viết tắt **FA** khi dẫn mục) và không lặp lại giá trị thiết kế (xem `Design-System.md`, viết tắt **DS**). Nội dung nào không có trong các tài liệu căn cứ được gắn nhãn **[ĐX-FC-xx]** và tổng hợp ở mục 12.

## Mục lục

1. Giới thiệu và phạm vi
2. Nguyên tắc chung
3. Quy ước đặt tên
4. Tổ chức mã nguồn và phụ thuộc
5. Quy ước mã web (TypeScript, React, Next.js)
6. Quy ước mã Android (Java)
7. Quy ước dùng chung cho hai nền tảng
8. Hiện thực Design-System trong mã
9. Kiểm thử
10. Git, cấu hình và CI
11. Danh sách kiểm tra khi review
12. Danh mục ĐX-FC
- Phụ lục A. Ánh xạ thành phần Design-System sang tên trong mã

---

## 1. Giới thiệu và phạm vi

### 1.1 Mục đích và cách dùng

| Nội dung | Quy định |
|---|---|
| Phạm vi áp dụng | Toàn bộ mã của app Android (SV) và web (NV, QT), gồm mã sản phẩm, mã kiểm thử và tệp cấu hình |
| Người dùng tài liệu | Lập trình viên frontend, người review, người kiểm thử |
| Mức bắt buộc | Mọi quy tắc là **bắt buộc**. Quy tắc nào được bảo vệ tự động (ESLint, TypeScript, kiểm tra định dạng, kiểm thử, CI) được nêu ở mục tương ứng; các quy tắc còn lại kiểm bằng review theo danh sách ở mục 11 |
| Truy vết | Trong mã, kiểm thử và mô tả pull request dùng mã `UF-…`, `SCR-…`, `DLG-…`, `CMP-…`, `BR-…`, `NFR-…` như các tài liệu nguồn |

### 1.2 Ranh giới với các tài liệu khác

| Nội dung | Tài liệu có thẩm quyền | Quan hệ với tài liệu này |
|---|---|---|
| Kiến trúc, công nghệ, cấu trúc thư mục, hướng phụ thuộc, hành vi dùng chung (phiên, lỗi, thời gian chờ, thao tác ghi, ảnh, đếm ngược) | `Frontend-Architecture.md` | Tài liệu này chỉ quy định **cách viết mã** để hiện thực đúng; không đổi quyết định kiến trúc. Khi hai tài liệu mâu thuẫn về kiến trúc hoặc hành vi, `Frontend-Architecture.md` thắng và tài liệu này phải sửa |
| Giá trị token, kiểu thành phần, nhãn trạng thái, thông điệp mặc định, tiếp cận | `Design-System.md` | Tài liệu này quy định cách đưa token và thành phần vào mã (mục 8); khi mâu thuẫn về **hình thức hiển thị**, `Design-System.md` thắng |
| Màn hình, thành phần, trạng thái, bảng lỗi từng màn hình | `UI-Screens.md` | Mã màn hình là gốc để đặt tên (mục 3) |
| Luồng, nhãn trạng thái (Phụ lục A), quy tắc dùng chung | `User-Flows.md` | Giữ nguyên |
| Hợp đồng API, `operationId`, tên trường, mã lỗi | `openapi.yaml`, `API-Notes.md` | Frontend là bên tiêu thụ; tên hàm API bám `operationId` (mục 3.2) |
| Bố cục màn hình | `Wireframes.md` | Không thuộc tài liệu này |
| Nguyên tắc ngôn ngữ đặt tên, quy ước Git, ngôn ngữ chú thích | `Backend-Conventions.md` 3.1, 18.3, 19.2 | Áp dụng lại cho frontend (mục 3.1, 10.1, 2) |

### 1.3 Ký hiệu

| Ký hiệu | Ý nghĩa |
|---|---|
| `FA-nn`, `ĐX-FA-nn` | Quyết định trong `Frontend-Architecture.md` (mục 1.4 và 1.5 của tài liệu đó) |
| `ĐX-DS-nn`, `CMP-nn`, `TC-nn` | Quyết định, thành phần, tiêu chí chấp nhận trong `Design-System.md` |
| `ĐX-UF-nn`, `ĐX-UI-nn`, `ĐX-WF-nn`, `ĐX-CV-nn`, `ĐX-API-nn` | Quyết định trong `User-Flows.md`, `UI-Screens.md`, `Wireframes.md`, `Backend-Conventions.md`, `API-Notes.md` |
| **[ĐX-FC-nn]** | Đề xuất của tài liệu này cho điểm các nguồn không nêu hoặc giao cho Frontend-Conventions; chờ nhóm duyệt (mục 12) |
| `[Web]`, `[Android]` | Quy định chỉ áp dụng cho một nền tảng. Không ghi nền tảng nghĩa là áp dụng cho cả hai |
| Tên theo nghiệp vụ | Thuật ngữ nghiệp vụ giữ tiếng Việt không dấu trong tên định danh (mục 3.1) |

### 1.4 Công nghệ áp dụng (theo FA 2.3, 2.4, 2.5)

Tài liệu này không chọn công nghệ. Quy ước dưới đây chỉ dùng các công nghệ đã chốt:

| Nền tảng | Công nghệ |
|---|---|
| Web | Next.js (App Router), React, TypeScript, TailwindCSS, React Hook Form, TanStack Query, Axios; công cụ phát triển: openapi-typescript, Vitest, React Testing Library, ESLint |
| Android | Java, `minSdk` 26, XML Views với ViewBinding và Material Components, Navigation Component (Safe Args), `ViewModel` và `LiveData`, Retrofit và OkHttp (client sinh bởi openapi-generator), Glide; công cụ phát triển: JUnit, MockWebServer |

Phiên bản chính (major) của từng công nghệ do nhóm chốt khi khởi tạo dự án và được khóa bằng tệp khóa của trình quản lý gói (web) hoặc tệp phiên bản của Gradle (Android) (FA 4.13). Các quy ước dưới đây không phụ thuộc vào phiên bản chính cụ thể. Thêm thư viện mới ngoài danh sách trên phải được nhóm duyệt và cập nhật `Frontend-Architecture.md` trước khi dùng (mục 2). Các công cụ định dạng và kiểm tra mã (Prettier, EditorConfig, Spotless) không nằm trong sản phẩm chạy; chúng được đề xuất ở ĐX-FC-08.

---

## 2. Nguyên tắc chung

| Nguyên tắc | Hệ quả trong mã | Căn cứ |
|---|---|---|
| Tên theo nghiệp vụ | Thuật ngữ nghiệp vụ (phiếu mượn, biên bản, khung phạt…) giữ tiếng Việt không dấu; không dịch sang tiếng Anh; thành phần kỹ thuật dùng hậu tố hoặc tiền tố tiếng Anh | KT-11; Backend-Conventions 3.1; FA 1.3 |
| Frontend không chứa nghiệp vụ | Không có hàm tính phí, số ngày trễ, hạn trả, hạn giữ chỗ, trạng thái hay điều kiện mượn. Hàm trong `shared/format` (web) và `common/format` (Android) chỉ định dạng giá trị đã có | FA-07 |
| Một nơi cho mỗi mối quan tâm | Gọi API, phiên, chuẩn hóa lỗi, nhãn trạng thái, định dạng, ảnh mỗi thứ đúng một module; màn hình không tự viết lại | FA 2.2 |
| Mỗi lớp chỉ gọi lớp ngay dưới | Giao diện không gọi HTTP; lớp trên không đọc phản hồi HTTP thô, chỉ nhận dữ liệu hoặc `ApiError` | FA 3.1 |
| Không giá trị thô trong giao diện | Màu, cỡ chữ, khoảng cách, bo góc, viền, đổ bóng, chuyển động lấy từ token | ĐX-DS-02, TC-01 |
| Mã lạ không làm vỡ | Mã trạng thái chưa biết hiển thị nguyên mã; `maLoi` chưa biết xử lý theo nhóm "Nghiệp vụ theo màn hình"; trường lạ trong phản hồi bị bỏ qua | FA 2.2, 3.2 |
| Không thư viện mới khi chưa cần | Không thêm thư viện, tầng trừu tượng hay bản sao chức năng đã có (ví dụ thư viện ngày giờ, thư viện schema, thư viện nối class) | FA 2.5, ĐX-FA-08, ĐX-FA-10 |
| Không lộ bí mật | Không ghi mật khẩu, token (access, refresh) vào nhật ký, URL, thông điệp lỗi hay kho lưu trữ ngoài quy định của FA 4.5 và 5.4; không commit bí mật | FA-06, NFR-05 |
| Chú thích tiếng Việt có dấu | Giải thích **vì sao**, không lặp lại mã; ghi mã truy vết khi cài đặt một quy tắc cụ thể (ví dụ `// UF-NV-02 bước 3: không tự gửi lại thao tác ghi`) | Backend-Conventions 19.2 (ĐX-CV-05) |
| Chữ hiển thị tiếng Việt | Mọi tệp mã và tài nguyên là UTF-8; chữ có dấu dựng sẵn (NFC); tên định danh không dấu | DS ĐX-DS-01, TC-14 |

---

## 3. Quy ước đặt tên

### 3.1 Nguyên tắc ngôn ngữ

| Loại | Quy tắc | Ví dụ |
|---|---|---|
| Thuật ngữ nghiệp vụ | Tiếng Việt không dấu, theo `openapi.yaml` và `User-Flows.md` | `PhieuMuon`, `maThietBi`, `hanGiuCho`, `KhungPhat` |
| Thành phần kỹ thuật | Hậu tố hoặc tiền tố tiếng Anh | `Fragment`, `ViewModel`, `Repository`, `Provider`, `Screen`, `Query`, `Mutation`, `Dialog` |
| Động từ kỹ thuật | Tiếng Anh (`load`, `refresh`, `retry`, `parse`, `format`) | `refreshSession()`, `parseApiError()` |
| Hành động nghiệp vụ | Tiếng Việt không dấu, theo tên thao tác trong `operationId` | `duyetPhieuYeuCauMuon`, `lapPhieuMuon` |
| Không dịch | Không đặt `LoanSlip` cho `PhieuMuon`, `Device` cho `ThietBi` | – |

### 3.2 Bảng quy ước cho web

| Đối tượng | Quy ước | Ví dụ |
|---|---|---|
| Thư mục, đoạn tuyến | kebab-case không dấu, tên theo nghiệp vụ (FA 4.2, 4.3) | `giao-thiet-bi`, `qua-han-30-ngay` |
| Tệp component, màn hình | PascalCase `.tsx`, tên tệp trùng tên export | `PhieuYeuCauTable.tsx`, `DuyetPhieuYeuCauScreen.tsx` |
| Màn hình | `{TenManHinh}Screen`; mỗi `page.tsx` chỉ import đúng một `Screen` từ `features/` (FA 4.2) | `GiaoThietBiTheScreen` cho `SCR-NV-04` |
| Tệp không phải component | camelCase `.ts` | `chuanHoaLoi.ts`, `khoiPhucPhien.ts` |
| Tệp cố định của tính năng | `api.ts`, `hooks.ts`, thư mục `components/` (FA 4.2) | `features/quay/phieu-yeu-cau/api.ts` |
| Tệp đặc biệt của Next.js | Đúng tên do Next.js quy định | `page.tsx`, `layout.tsx`, `error.tsx`, `global-error.tsx` |
| Hook truy vấn | `use{DoiTuong}{BienThe}Query` | `usePhieuYeuCauChoDuyetQuery` |
| Hook thao tác ghi | `use{HanhDong}{DoiTuong}Mutation` | `useDuyetPhieuYeuCauMutation` |
| Hook dùng chung | `use{MoTa}` | `useDemNguoc`, `useThongBaoKhoiDong` |
| Hàm gọi API | **Trùng `operationId`** của `openapi.yaml` (camelCase) **[ĐX-FC-02]** | `quayDsPhieuYeuCauMuon`, `quayDuyetPhieuYeuCauMuon`, `authLamMoi` |
| Thể hiện Axios chung | `apiClient` ở `shared/api/client.ts` **[ĐX-FC-02]** | – |
| Tiền tố khóa truy vấn | Hằng `TIEN_TO_KHOA` ở `shared/api/tienToKhoa.ts`, mỗi tiền tố là `[vùng, tài nguyên]` theo FA 4.7 **[ĐX-FC-10]** | `TIEN_TO_KHOA.quayPhieuYeuCau` |
| Khóa truy vấn | Đối tượng `khoa{DoiTuong}` trong `api.ts` của tính năng sở hữu tài nguyên, dựng từ `TIEN_TO_KHOA` rồi thêm tham số (FA 4.7) | `khoaPhieuYeuCau.ds({ trangThai, maPhong, trang })` |
| Kiểu dữ liệu API | Tên schema của `openapi.yaml`, đặt bí danh trong `shared/api/types.ts` **[ĐX-FC-02]** | `export type PhieuMuon = components["schemas"]["PhieuMuon"]` |
| Kiểu, giao diện (`type`, `interface`) | PascalCase, không tiền tố `I` | `TrangThaiPhien`, `ApiError` |
| Props | `{TenComponent}Props` | `ConfirmDialogProps` |
| Context và provider | `{Ten}Context`, `{Ten}Provider` (FA 4.2) | `AuthProvider`, `LuongGiaoThietBiContext` |
| Hằng số | UPPER_SNAKE_CASE, hậu tố đơn vị khi là số đo | `THOI_GIAN_CHO_HTTP_MS`, `SO_ANH_TOI_DA` |
| Biến, hàm, tham số | camelCase | `maPhieu`, `chuanHoaLoi` |
| Giá trị trạng thái, vai trò, mã lỗi | Nguyên mã API (CHỮ HOA, gạch dưới), không đổi tên, không đổi kiểu chữ | `DANG_MUON`, `TAI_KHOAN_BI_KHOA` |
| Tệp kiểm thử | `{TenTep}.test.ts` hoặc `.test.tsx`, đặt cạnh tệp được kiểm thử | `formatThoiGian.test.ts` |
| Biến môi trường | `NEXT_PUBLIC_` kèm tên UPPER_SNAKE_CASE (FA 4.14) | `NEXT_PUBLIC_API_BASE_URL` |
| Nhánh, commit | Mục 10.1 | – |

### 3.3 Bảng quy ước cho Android

| Đối tượng | Quy ước | Ví dụ |
|---|---|---|
| Gói | Chữ thường không dấu, theo FA 5.2 | `ui.phieuyeucau`, `data.repository` |
| Lớp | PascalCase; hậu tố kỹ thuật tiếng Anh | `TraCuuThietBiFragment`, `PhieuYeuCauRepository` |
| Fragment | `{TenManHinh}Fragment`, tên theo bảng ánh xạ FA 5.3 | `DanhSachPhieuYeuCauFragment` |
| ViewModel | `{TenManHinh}ViewModel`; ViewModel dùng chung theo nav graph con đặt tên theo luồng | `TraCuuThietBiViewModel`, `SoanPhieuViewModel` |
| Adapter, ViewHolder | `{DoiTuong}Adapter`, `{DoiTuong}ViewHolder` | `PhieuMuonAdapter` |
| Repository | `{NhomApi}Repository` (FA 5.2) | `HoSoMuonRepository` |
| Hộp thoại | `{Ten}DialogFragment` (FA 5.2, 5.3) | `XacNhanDialogFragment`, `DaDangKyChoDialogFragment` |
| Phương thức | camelCase; động từ kỹ thuật tiếng Anh, đối tượng theo nghiệp vụ | `loadPhieuYeuCau()`, `submitPhieu()` |
| Hằng số | UPPER_SNAKE_CASE, `static final`, hậu tố đơn vị | `THOI_GIAN_CHO_TOI_DA_GIAY` |
| Trường | camelCase, không tiền tố `m` | `uiState`, `maPhieu` |
| Giá trị `UiState`, mã trạng thái | Nguyên mã (`DANG_TAI`, `THANH_CONG`, `RONG`, `LOI`) | – |
| Tệp layout | `{loai}_{ten_man_hinh}.xml`; `loai` là `fragment`, `item`, `dialog`, `view` | `fragment_tra_cuu_thiet_bi.xml`, `item_phieu_muon.xml`, `dialog_xac_nhan.xml` |
| Mã định danh view (`android:id`) | snake_case, tiền tố loại view: `btn_`, `tv_`, `et_`, `til_`, `rv_`, `img_`, `chip_`, `srl_`; ViewBinding chuyển thành camelCase | `btn_gui` → `binding.btnGui` **[ĐX-FC-15]** |
| Chuỗi chữ | `strings.xml`, snake_case không dấu, tiền tố theo nhóm: `nhan_{ho}_{ma}` cho nhãn trạng thái, `loi_{nhom}` cho thông điệp lỗi, `{ten_man_hinh}_{ten}` cho chữ riêng của màn hình | `nhan_phieu_muon_dang_muon`, `loi_khong_du_quyen`, `soan_phieu_nut_xem_lai` |
| Token thiết kế | Theo DS Phụ lục C: tiền tố `ds_`, chữ thường, gạch dưới | `ds_color_blue_600`, `ds_space_4`, `ds_font_size_14` |
| Kiểu (style) | `Widget.Ds.{ThanhPhan}.{BienThe}`; kiểu chữ `TextAppearance.Ds.{VaiTro}` **[ĐX-FC-15]** | `Widget.Ds.Button.Primary`, `TextAppearance.Ds.Body` |
| Biểu tượng | Vector drawable `ic_{ten}` theo tên biểu tượng ở DS Phụ lục A | `ic_check_circle`, `ic_cloud_off` |
| Nav graph | Điểm đến: id camelCase trùng tên lớp Fragment (chữ đầu thường); hành động: `action_{nguon}_to_{dich}` | `khoiDongFragment`; `action_khoiDongFragment_to_trangChuFragment` |
| Tham số Safe Args | camelCase, tên theo FA 5.3 | `maPhieu`, `cheDoVuaGui` |
| Biến cấu hình build | `buildConfigField` `API_BASE_URL` (FA 5.8) | – |
| Tệp kiểm thử | `{TenLop}Test.java` đặt cùng gói trong `src/test` | `ApiErrorParserTest.java` |

### 3.4 Tên thành phần giao diện

`Design-System.md` đặt tên kỹ thuật cho 31 thành phần (CMP-01 đến CMP-31) và giao cho tài liệu này quy định tên lớp và tệp (DS 3.2). `Frontend-Architecture.md` 4.2 và 5.2 đã đặt sẵn một số tên trong mã. Quy tắc thống nhất **[ĐX-FC-01]**:

| Quy tắc | Nội dung |
|---|---|
| Tên đã có ở `Frontend-Architecture.md` | Giữ nguyên làm tên trong mã (`ConfirmDialog`, `CodeInput`, `AnhUploader`, `AnhGallery`, `ErrorBanner`, `LoadingBlock`, `EmptyState`, `FormField`; Android `ErrorView`, `LoadingView`, `EmptyView`, `XacNhanDialogFragment`) |
| Thành phần còn lại | Dùng tên kỹ thuật của DS 8.1 |
| Va chạm tên | CMP-03 `Link` đặt tên `TextLink` ở web để không trùng `next/link` |
| Quan hệ | Phụ lục A ghi từng `CMP-nn` tương ứng tên nào trong mã; thành phần mới ngoài danh mục CMP không được thêm (DS NT-07). Nhu cầu mới phải được thêm vào `Design-System.md` trước |
| Vị trí | Web: `shared/components/{Ten}.tsx`; Android: `common/widget/` (FA 4.2, 5.2). FA 2.5 giới hạn thành phần dùng chung ở danh sách của FA 4.2 vì khi đó chưa có `Design-System.md`; nay danh sách này gồm toàn bộ CMP của Phụ lục A. Thành phần dùng ở từ hai tính năng trở lên phải là một CMP; nhu cầu chưa có trong `Design-System.md` phải được bổ sung ở đó trước, không tạo bản sao trong từng tính năng |
| Lỗi tải một vùng (web) | FA 4.12 nêu `ErrorBanner` kèm **Thử lại**, DS 9.4 và CMP-25 dùng `StateView`. Hình thức hiển thị do DS quyết định (mục 1.2), nên lỗi tải một vùng dùng `StateView` kèm **Thử lại**; `ErrorBanner` dùng cho lỗi ghi và các lỗi không gắn trường (mục 5.5) |

### 3.5 Mã màn hình, luồng và tên trong mã

| Quy tắc | Nội dung |
|---|---|
| Mã màn hình trong mã | Chú thích đầu tệp `Screen` hoặc `Fragment` ghi mã `SCR-…` hoặc `DLG-…` (ví dụ `// SCR-NV-04 Giao thiết bị, bước 1`); đây là chỗ truy vết chính giữa tài liệu và mã (kiểm thử truy vết theo mục 9.2) |
| Không đặt mã màn hình vào tên định danh | Tên lớp và tệp theo nghiệp vụ (mục 3.2, 3.3), không phải `ScrNv04` |
| Họ trạng thái | Tên họ lấy từ Phụ lục A của `User-Flows.md`, viết UPPER_SNAKE_CASE ở web (`PHIEU_YEU_CAU`, `PHIEU_MUON`, `DANG_KY_CHO`, `TINH_TRANG_THIET_BI`, `KET_QUA_KIEM_TRA`, `BIEN_BAN`, `LOAI_VI_PHAM`, `TRANG_THAI_HOC`, `QUYEN_MUON`, `TAI_KHOAN`, `LOAI_BAO_TRI`, `KET_QUA_BAO_TRI`, `NGUON_GIAO`, `LOAI_THONG_BAO`) và `snake_case` trong khóa `strings.xml` (`phieu_yeu_cau`, `phieu_muon`, …). `VAI_TRO` không phải họ trạng thái (DS 7.1) |

---

## 4. Tổ chức mã nguồn và phụ thuộc

Cây thư mục và vai trò từng thư mục theo `Frontend-Architecture.md` 4.2 (web) và 5.2 (Android); mục này chỉ bổ sung quy tắc đặt tệp và biên giới import để kiểm tra được.

### 4.1 Web

**Đặt tệp**

| Nội dung | Vị trí |
|---|---|
| `page.tsx`, `layout.tsx` | `src/app/`; mỗi `page.tsx` chỉ import một `Screen` (FA 4.2) |
| Màn hình `{Ten}Screen.tsx` | `features/<nhóm>/<tính năng>/` |
| Component riêng của một tính năng | `features/<nhóm>/<tính năng>/components/` |
| Thành phần Design-System (Phụ lục A) và `icons/` | `shared/components/` |
| Hàm API và khóa truy vấn của tính năng | `api.ts` |
| Hook truy vấn và thao tác ghi của tính năng | `hooks.ts` |
| CSS toàn cục: `tokens.css`, `globals.css` | Trực tiếp trong `src/app/`; không phải màn hình nên không vi phạm quy tắc "`app/` chỉ ghép màn hình" **[ĐX-FC-06]** |
| Hằng số | Trong module sở hữu mối quan tâm (mục 7.1) |
| Kiểm thử | Cạnh tệp được kiểm thử |

Không có thư mục `utils/`, `helpers/`, `common/`, `lib/` chứa tệp lẫn lộn: mỗi hàm thuộc đúng một module theo mối quan tâm (`shared/api`, `shared/format`, `shared/labels`, `shared/anh`, `shared/hooks`).

**Biên giới import** (FA 4.2, cưỡng chế bằng `no-restricted-imports` của ESLint). FA 4.2 cấm các nhóm trong `features/` import lẫn nhau; từ "nhóm" được dùng cho cả `xac-thuc`, `quay`, `quan-tri` lẫn các thư mục con của chúng, nên tài liệu này áp dụng cách hiểu chặt nhất **[ĐX-FC-10]**:

| Từ | Được import | Không được import |
|---|---|---|
| `app/**` | `features/**`, `shared/**`, `providers/**` | – |
| Một thư mục tính năng (ví dụ `features/quay/giao-thiet-bi/`, `features/xac-thuc/`) | `shared/**` | Mọi thư mục tính năng khác dưới `features/`; `app/**` |
| `shared/**` | `shared/**` | `features/**`, `app/**`, `providers/**` |
| Ngoài `shared/api/**` | – | `axios`; `fetch` gọi API |
| Ngoài `shared/api/session/**` | – | `sessionStorage` |
| Toàn bộ mã | – | `localStorage`, `next/image` (FA 4.1, 4.10, 4.13) |

Những thứ nhiều tính năng cùng cần đặt ở `shared/`, không import chéo giữa các tính năng:

| Nhu cầu chung | Nơi đặt |
|---|---|
| Tiền tố khóa truy vấn để đánh dấu lỗi thời sau thao tác ghi (FA 4.7 liệt kê khóa của tính năng khác) | `shared/api/tienToKhoa.ts` |
| Đọc phiên và `ThongTinToi` (ví dụ `dsPhong` của bộ lọc phòng, FA 4.4) | `AuthContext` và hook `useAuth` ở `shared/hooks/useAuth.ts`; `AuthProvider` ở `features/xac-thuc/` cung cấp giá trị cho `AuthContext` |
| Kiểu `TrangThaiPhien` | `shared/api/session/` |
| Thành phần giao diện dùng ở từ hai tính năng | `shared/components/`, và phải là một CMP (mục 3.4) |

Dùng bí danh `@/` trỏ tới `src/` cho import xuyên thư mục; import tương đối chỉ trong cùng một thư mục tính năng.

**Server và client (ĐX-FA-01)**

| Quy tắc | Nội dung |
|---|---|
| `"use client"` | Dòng đầu của mọi tệp dùng hook, trạng thái hoặc API trình duyệt, và của mọi `page.tsx`, `layout.tsx` có dữ liệu hoặc trạng thái người dùng. `src/app/layout.tsx` (gốc) giữ là Server Component để khai báo `metadata` và phông, chỉ dựng `<html lang="vi">` và `Providers` (Client Component) |
| Không dùng | Route Handler (`route.ts`), Server Action (`"use server"`), `middleware`, proxy API, `next/image`; kiểm tra bằng `no-restricted-syntax` và bước kiểm tra tên tệp ở CI (mục 10.3) |
| Chống lệch dựng trang | Không đọc `window`, storage hay API browser-only trong SSR hoặc lúc render. Khởi tạo/khôi phục phiên sau mount trong `useEffect`; trước đó hiển thị `SCR-CM-03` (FA 4.1). Sau khởi tạo, module `session`, interceptor và callback/handler chạy ở client được dùng API trình duyệt cho xoay token, canvas, URL ảnh theo mục 5.9–5.10; quyền đọc/ghi token vẫn chỉ qua module `session` |

### 4.2 Android

| Nội dung | Quy tắc |
|---|---|
| Gói | Đúng cây gói của FA 5.2; tên gói gốc do nhóm đặt khi tạo dự án |
| Biên giới | `ui` → `data.repository`, `data.model`, `common`; `data` không phụ thuộc `ui`; `common` chỉ phụ thuộc `data.model` **[ĐX-FC-10]**. Kiểm tra bằng bước `grep` ở CI (mục 10.3) và review |
| Tệp trực tiếp trong `common/` | Chỉ lớp hằng số dùng chung `HangSo` (mục 7.1) **[ĐX-FC-03]** |
| Mỗi tính năng `ui/<tính năng>` | Chứa `Fragment`, `ViewModel`, `Adapter`, `ViewHolder` của tính năng; không chứa `Repository` |
| `ApiCallback<T>` | Giao diện callback của `Repository`, đặt ở `data/repository` (mục 6.4) |
| Tài nguyên | `res/layout`, `res/navigation`, `res/values` theo FA 5.2; tệp token `ds_colors.xml`, `ds_dimens.xml`, `ds_type.xml` trong `res/values/` **[ĐX-FC-06]** |
| Mã sinh | Client sinh từ `openapi.yaml` nằm trong thư mục build, không đưa vào kho mã, không sửa tay (FA 3.2, 5.8) |

---

## 5. Quy ước mã web (TypeScript, React, Next.js)

### 5.1 TypeScript

| Quy tắc | Nội dung | Kiểm tra |
|---|---|---|
| Chế độ chặt | `strict: true`, `noFallthroughCasesInSwitch: true` **[ĐX-FC-09]** | `tsc --noEmit` ở CI |
| Không `any` | Dùng `unknown` rồi thu hẹp bằng hàm kiểm tra kiểu; không dùng `!` (non-null) và `as` để lách kiểu, trừ `as const` và trừ kiểm thử | `@typescript-eslint/no-explicit-any`, `@typescript-eslint/no-non-null-assertion`, `@typescript-eslint/consistent-type-assertions` với `assertionStyle: "never"` (tắt cho tệp kiểm thử) |
| Kiểu từ hợp đồng | Chỉ dùng kiểu sinh từ `openapi.yaml` qua `shared/api/types.ts`; không tự định nghĩa lại hình dạng phản hồi hay yêu cầu (FA 3.2) | Review |
| Giá trị enum của API | Là chuỗi; hàm nhận mã từ API khai báo tham số kiểu `string`, không kiểu hợp (union), để mã lạ không làm vỡ (FA 3.2) | Review |
| Không dùng `enum` của TypeScript | Dùng kiểu hợp chuỗi hoặc `as const` | Review |
| Xuất | Xuất có tên (`export function`); xuất mặc định chỉ cho tệp Next.js bắt buộc (`page`, `layout`, `error`, `global-error`, `not-found`) | `no-restricted-exports` với `restrictDefaultExports`, kèm ngoại lệ cho các tệp đó |
| Import kiểu | `import type` cho import chỉ dùng làm kiểu | `@typescript-eslint/consistent-type-imports` |
| Số và tiền | Số nguyên VNĐ là `number`; không dùng `parseFloat`; chuyển chuỗi nhập thành số bằng `docSoNguyen` ở `shared/format` (mục 5.6) | Review |
| Trường rỗng được | Trường `nullable` của API luôn có mặt với giá trị `null` (API-Notes mục 13); mã kiểm tra `=== null` hoặc `== null`, không giả định trường vắng mặt | Review |
| Kiểu hợp có `discriminator` | `LapPhieuMuonRequest` và `KiemTraNguonRequest` (theo `nguon`), `BaoCaoJson` (theo loại báo cáo): phân nhánh bằng `switch` trên trường phân biệt, có nhánh `default` | Review |
| Định dạng mã | Prettier với cấu hình mặc định (thụt 2 khoảng trắng, dấu chấm phẩy, nháy kép) và `.editorconfig` **[ĐX-FC-08]** | `format:check` ở CI |

### 5.2 Component và màn hình

| Quy tắc | Nội dung |
|---|---|
| Dạng | Function component; props khai báo bằng `type {Ten}Props`; không dùng class component |
| `Screen` | Là nơi gọi hook (truy vấn, thao tác ghi, ngữ cảnh) và quyết định hiển thị; component trong `components/` chỉ nhận props, không gọi API |
| Đủ trạng thái | Mỗi vùng dữ liệu hiển thị theo thứ tự: đang tải (`LoadingBlock`, có thông điệp khởi động sau 8 giây) → lỗi (mục 5.5) → rỗng (`EmptyState`) → dữ liệu. Khi đang tải lại mà đã có dữ liệu thì giữ dữ liệu cũ (`placeholderData`, FA 4.7), không quay về `LoadingBlock` (TC-10) |
| Trạng thái ở đâu | Theo bảng FA 4.7: dữ liệu máy chủ ở TanStack Query; phiên ở `AuthProvider`; luồng nhiều bước ở React Context tại layout của luồng; dữ liệu nhập ở React Hook Form; bộ lọc, trang, hộp thoại mở ở `useState` |
| Không sao chép dữ liệu máy chủ vào `useState` | Dữ liệu hiển thị lấy thẳng từ kết quả truy vấn (FA 2.2) |
| Tên sự kiện | Prop dạng `onXxx`; hàm xử lý nội bộ dạng `handleXxx` |
| `key` | Là mã ổn định của đối tượng (`maPhieu`, `maThietBi`); chỉ dùng chỉ số cho danh sách tĩnh không đổi thứ tự. Với `useFieldArray` dùng `id` do React Hook Form cấp |
| Văn bản hiển thị | Chữ chính thức theo `UI-Screens.md` và DS 9.8; nhãn hành động chuẩn dùng đúng bộ chữ của DS 9.8; chữ riêng của màn hình đặt trong `Screen` hoặc `components/` của tính năng. Không dùng chữ làm `placeholder` thay nhãn (DS 10) |
| Ngữ cảnh luồng | Hai ngữ cảnh của FA 4.7, `LuongGiaoThietBi` và `PhieuYeuCauDuocChon`, hiện thực bằng `{Ten}Context`, `{Ten}Provider` và hook `use{Ten}` ném lỗi khi dùng ngoài provider; thiếu dữ liệu thì chuyển về bước đầu bằng `router.replace` trong `useEffect` (FA 4.3, 4.7) |
| Thứ tự và bộ lọc phòng | Thứ tự danh sách do máy chủ quyết định: không gọi `sort` lên dữ liệu trả về (FA 3.10); bộ lọc phòng của NV chỉ liệt kê `dsPhong` lấy từ `useAuth` |

### 5.3 Hook và hiệu ứng

| Quy tắc | Nội dung | Kiểm tra |
|---|---|---|
| Không lấy dữ liệu trong `useEffect` | Lấy dữ liệu chỉ bằng TanStack Query (FA 4.1) | Review |
| Dọn dẹp | Mọi `setInterval`, `setTimeout`, trình nghe sự kiện, `URL.createObjectURL`, đăng ký lắng nghe của module `session` phải được hủy trong hàm dọn của `useEffect` | Review; kiểm thử ở mục 9 |
| Idempotent | Hiệu ứng chạy hai lần (Strict Mode) không gây hai lần gọi ghi hoặc hai lần làm mới phiên; việc khôi phục phiên dùng chung một `Promise` (FA 4.5) | Kiểm thử phiên |
| Mảng phụ thuộc đầy đủ | Không tắt quy tắc `exhaustive-deps` | `react-hooks/exhaustive-deps` ở mức `error` |
| Hook dùng chung | `useThongBaoKhoiDong` (8 giây) và `useAuth` nằm ở `shared/hooks/` (FA 4.2); `useDemNguoc` chỉ được viết khi có màn hình web dùng bộ đếm (mục 7.5) | – |

### 5.4 Tầng gọi API và TanStack Query

**Hàm API (`api.ts`)** (FA 4.6)

| Quy tắc | Nội dung |
|---|---|
| Tên | Trùng `operationId` (mục 3.2) |
| Chữ ký | `(thamSo, tuyChon?: { signal?: AbortSignal })`; trả dữ liệu đã bóc phản hồi (phản hồi 204 trả `void`) |
| Gọi | Chỉ qua `apiClient` của `shared/api/client.ts`; đường dẫn tương đối so với `baseURL` (đã gồm `/api/v1`), không lặp lại `/api/v1` ở nơi khác; không đặt tiêu đề `Accept` (FA 4.6) |
| Tham số | Tham số truy vấn truyền qua `params` của Axios; mã nằm trong đường dẫn qua `encodeURIComponent` |
| Không làm | Không hiển thị, không điều hướng, không bắt lỗi để nuốt, không retry |
| Báo cáo dạng tệp | `responseType: "blob"` theo FA 4.11; tên tệp do client đặt theo FA 4.11 |

**Truy vấn**

| Quy tắc | Nội dung |
|---|---|
| Khóa | `[vùng, tài nguyên, tham số]`, lấy từ `khoa{DoiTuong}` trong `api.ts` (dựng từ `TIEN_TO_KHOA`); không viết mảng khóa rải rác ở nơi gọi |
| `queryFn` | Nhận `signal` của TanStack Query và chuyển cho hàm API để hủy khi màn hình đóng (lỗi `HUY` không hiển thị, FA 3.3) |
| Tùy chọn mặc định | Dùng mặc định của `QueryClient` ở FA 4.7 (`staleTime: 0`, `refetchOnWindowFocus: false`, `retry: 0`). Ghi đè ở từng truy vấn chỉ khi có lý do ghi bằng chú thích |
| Truy vấn phụ thuộc | Dùng `enabled`; truy vấn độc lập của một màn hình chạy song song (FA 4.13) |
| Phân trang | `placeholderData: keepPreviousData`; `trang` và bộ lọc là `useState` của component, không đưa lên URL (FA 4.3) |
| Đổi bộ lọc | Đặt `trang` về 1 trong cùng hàm xử lý đổi bộ lọc |

**Thao tác ghi**

| Quy tắc | Nội dung |
|---|---|
| Hook | `useMutation` kế thừa `QueryClient` với `retry: 0` và `networkMode: "always"` cho mutation (FA 3.6, 4.7); không ghi đè để pause/chờ reconnect, không persistence hoặc `resumePausedMutations`. Offline trước khi bấm phải đi qua lần gọi HTTP và lỗi mạng hiện có, không giữ write để tự phát khi có mạng; không dùng `onMutate` cập nhật lạc quan (NFR-06) |
| Đánh dấu lỗi thời | Trong `onSuccess`, gọi `invalidateQueries` đúng các khóa ở bảng FA 4.7; tiền tố khóa của tính năng khác lấy từ `TIEN_TO_KHOA` (mục 4.1) |
| Chống gửi trùng | Cờ `useRef` đặt `true` **đồng bộ** trong hàm xử lý bấm, trước mọi `await`, và đặt lại khi có phản hồi; nút bị vô hiệu theo cờ này kết hợp `isPending` (FA 3.11, 4.8) |
| Mật khẩu | Thao tác đăng nhập, tạo và đặt lại mật khẩu nhân viên dùng `gcTime: 0`; sau khi lấy kết quả/lỗi cần hiển thị, gọi mutation `reset()`, xóa tham chiếu request nhạy cảm và `resetField` ô mật khẩu kể cả form còn mounted (FA 3.5, 4.8). Không reset khi đang chờ; GC chỉ áp dụng khi mutation không còn được dùng, không thay cho việc tách observer khỏi `variables` |
| Chưa biết kết quả | Khi lỗi là `MANG`, `QUA_THOI_GIAN` hoặc `HUY` sau khi thao tác ghi nghiệp vụ đã phát, `laChuaBietKetQua(error)` ở `shared/api` được gọi trong ngữ cảnh thao tác ghi đó; `Screen` hiển thị "Chưa xác định được kết quả" và hành động kiểm tra qua API danh sách/chi tiết hiện hữu. Chỉ bật gửi lại sau khi đã kiểm tra, kể cả hủy/mất theo dõi khi điều hướng; không coi hủy là rollback (FA 3.11) |
| Thử lại bằng tay (ĐX-FA-22) | Kiểm tra thẻ/nguồn/thiết bị/điều kiện không đổi nghiệp vụ được **Thử lại** trực tiếp. Đăng nhập và từng ảnh có cách thử lại riêng, có thể đã tạo phiên/cập nhật bộ đếm hoặc tạo ảnh chưa gắn; không gọi chúng là chỉ đọc, không tự retry, không đếm số lần đăng nhập còn lại ở client; giữ xử lý 429/khóa của BE |
| Không tự lặp | Lỗi `TRANG_THAI_KHONG_HOP_LE`, `XUNG_DOT_CAP_NHAT`, `QUA_NHIEU_YEU_CAU` không tự gọi lại (FA 3.4) |

**Ví dụ tối thiểu** (tên hàm trùng `operationId`; `shared/api/types.ts` xuất lại `operations` và các schema):

```ts
// features/quay/phieu-yeu-cau/api.ts
import { apiClient } from "@/shared/api/client";
import { TIEN_TO_KHOA } from "@/shared/api/tienToKhoa";
import type { operations, PhieuYeuCauMuonQuay, PhieuYeuCauMuonQuayPage } from "@/shared/api/types";

export type DsPhieuYeuCauParams = NonNullable<
  operations["quayDsPhieuYeuCauMuon"]["parameters"]["query"]
>;

export const khoaPhieuYeuCau = {
  ds: (tham: DsPhieuYeuCauParams) => [...TIEN_TO_KHOA.quayPhieuYeuCau, tham] as const,
};

export async function quayDsPhieuYeuCauMuon(
  tham: DsPhieuYeuCauParams,
  tuyChon?: { signal?: AbortSignal },
) {
  const res = await apiClient.get<PhieuYeuCauMuonQuayPage>("/quay/phieu-yeu-cau-muon", {
    params: tham,
    signal: tuyChon?.signal,
  });
  return res.data;
}

export async function quayDuyetPhieuYeuCauMuon(ma: string) {
  const res = await apiClient.post<PhieuYeuCauMuonQuay>(
    `/quay/phieu-yeu-cau-muon/${encodeURIComponent(ma)}/duyet`,
  );
  return res.data;
}
```

```ts
// features/quay/phieu-yeu-cau/hooks.ts
import { keepPreviousData, useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { TIEN_TO_KHOA } from "@/shared/api/tienToKhoa";
import {
  khoaPhieuYeuCau,
  quayDsPhieuYeuCauMuon,
  quayDuyetPhieuYeuCauMuon,
  type DsPhieuYeuCauParams,
} from "./api";

export function usePhieuYeuCauQuery(tham: DsPhieuYeuCauParams) {
  return useQuery({
    queryKey: khoaPhieuYeuCau.ds(tham),
    queryFn: ({ signal }) => quayDsPhieuYeuCauMuon(tham, { signal }),
    placeholderData: keepPreviousData,
  });
}

export function useDuyetPhieuYeuCauMutation() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: quayDuyetPhieuYeuCauMuon,
    onSuccess: () => {
      // FA 4.7: duyệt đánh dấu lỗi thời phiếu yêu cầu, loại thiết bị, danh sách chờ
      for (const queryKey of [
        TIEN_TO_KHOA.quayPhieuYeuCau,
        TIEN_TO_KHOA.quayLoaiThietBi,
        TIEN_TO_KHOA.quayDanhSachCho,
      ]) {
        void queryClient.invalidateQueries({ queryKey });
      }
    },
  });
}
```

### 5.5 Xử lý lỗi trong mã

| Quy tắc | Nội dung |
|---|---|
| Kiểu lỗi | Mọi lỗi ra khỏi `shared/api/` là `ApiError` (FA 3.3, định nghĩa ở `shared/api/apiError.ts`); mã phía trên dùng hàm kiểm tra kiểu `laApiError(e)`, không đọc phản hồi Axios thô |
| Phân nhánh | Theo `maLoi`, `status`, `loai`; **không** đọc `detail` để phân nhánh (ĐX-CV-02, FA 3.3). `switch` trên `maLoi` luôn có nhánh `default` xử lý như nhóm "Nghiệp vụ theo màn hình" |
| Nhóm xử lý | Bảng nhóm ở FA 3.4 được cài ở một nơi duy nhất `shared/api/nhomLoi.ts`: hàm `nhomCuaLoi(error)` trả một trong `PHIEN`, `DANG_NHAP_SAI`, `KHOA_TAI_KHOAN`, `QUYEN`, `DU_LIEU_NHAP`, `KHONG_TON_TAI`, `CAN_TAI_LAI`, `TAN_SUAT`, `NGHIEP_VU_THEO_MAN_HINH`, `HE_THONG`, `MANG`. `HUY` của truy vấn/kiểm tra không đổi nghiệp vụ được bỏ qua; `HUY` sau write đã phát đi nhánh chưa biết kết quả ở mục 5.4 trước khi phân nhóm (FA 3.3). Màn hình không tự lặp lại bảng này |
| Thông điệp | Thông điệp dùng chung (nhóm Phiên, Quyền, Không tồn tại, Cần tải lại, Tần suất, Hệ thống, Mạng) nằm ở `shared/labels/thongDiepLoi.ts`; thông điệp riêng của màn hình (bảng lỗi trong `UI-Screens.md`) nằm ở `features/.../thongDiep.ts` của tính năng, kiểu `Record<string, string>` khóa theo `maLoi` **[ĐX-FC-11]**. Chữ theo `UI-Screens.md` 3.2 và DS 9.4 |
| Hiển thị | Theo bảng FA 4.12 và DS 9.4: lỗi tải một vùng dùng khối trạng thái lỗi (`StateView`, CMP-25) kèm **Thử lại**; lỗi ghi và lỗi không gắn trường dùng `ErrorBanner` (CMP-24); lỗi theo trường dùng `FormField`; các nhóm Quyền, Không tồn tại, Cần tải lại dùng `ErrorBanner` ở đầu màn hình kèm điều hướng hoặc tải lại theo `UI-Screens.md` 3.2 (FA 4.12) |
| Lỗi 500 | Hiển thị `traceId` bằng kiểu chữ mã, chọn được để sao chép; không hiển thị `detail` hay chi tiết nội bộ |
| Không nuốt lỗi | Không dùng `catch {}` rỗng; không dùng `console.log` hay `console.error` để thay xử lý lỗi (mục 5.11). Chỉ bỏ qua kết quả đăng xuất theo FA 3.5 và `HUY` của truy vấn/kiểm tra không đổi nghiệp vụ; không nuốt `HUY` sau write đã phát |
| Lỗi dựng giao diện | `error.tsx` của từng nhóm tuyến và `global-error.tsx` ở gốc hiển thị thông báo chung kèm nút tải lại (FA 4.12) |

### 5.6 Biểu mẫu

| Quy tắc | Nội dung |
|---|---|
| Thư viện | React Hook Form; kiểm tra bằng `required`, `min`, `max`, `maxLength`, `pattern` đúng với giới hạn khai báo trong `openapi.yaml`, mỗi quy tắc kèm chú thích tên trường schema nguồn (ĐX-FA-08, FA 3.11). Không kiểm tra nghiệp vụ ở client |
| Thẻ `form` | `<form noValidate>` để thông báo lỗi do DS quy định thay cho bong bóng của trình duyệt |
| Tên trường | Trùng tên thuộc tính JSON của `openapi.yaml`, kể cả đường dẫn (`dsLoaiVaSoLuong.0.soLuong`), để lỗi `errors` của máy chủ gắn thẳng vào đúng trường |
| Lỗi máy chủ | Hàm dùng chung `ganLoiMayChu` (đặt ở `shared/hooks/ganLoiMayChu.ts`) đổi khóa `dsLoaiVaSoLuong[0].soLuong` thành `dsLoaiVaSoLuong.0.soLuong`, gọi `setError`, khóa không khớp trường nào thì trả về để hiển thị ở `ErrorBanner` đầu biểu mẫu, rồi đưa tiêu điểm tới trường lỗi đầu tiên (FA 4.8) **[ĐX-FC-03]** |
| Giữ dữ liệu | Không `reset` toàn bộ khi lỗi; chỉ `reset` sau thao tác thành công mà biểu mẫu cần trống (FA 4.8). Ngoại lệ mật khẩu: sau kết quả thành công/lỗi của đăng nhập, tạo/đặt lại mật khẩu nhân viên, `resetField` ô mật khẩu và dọn mutation/request theo mục 5.4; khi lỗi giữ tên đăng nhập/mã SV và dữ liệu khác được phép (UI-Screens 4.4, 4.5) |
| Số nguyên | Ô `inputMode="numeric"`; `docSoNguyen(chuoi)` chỉ chấp nhận chuỗi khớp `^\d+$` và trả `number`, ngược lại trả `null` (từ chối âm, thập phân); không dùng `parseFloat` (FA 4.8) |
| Phần trăm thập phân | Riêng `KhungPhatRequest.tyLePhanTram` ở `SCR-QT-07` dùng biến thể thập phân CMP-04, `inputMode="decimal"`. `docTyLePhanTram(chuoi)` chỉ nhận toàn chuỗi khớp `^-?\d+(?:[.,]\d+)?$`, thay một dấu phẩy thập phân bằng dấu chấm rồi dùng `Number`; chỉ trả số hữu hạn, sai cú pháp trả `null`. Không nhận dấu phân nhóm, khoảng trắng, dấu trộn/lặp, đuôi ký tự hay giá trị không hữu hạn; dấu chấm/phẩy duy nhất luôn là dấu thập phân, không là hàng nghìn. Giữ phần thập phân, không `parseInt`, đọc một phần chuỗi hoặc tự làm tròn; miền `0 < tyLePhanTram ≤ 100` theo BE, không thêm giới hạn số chữ số thập phân chặt hơn API. Tiền/số lượng/giờ nguyên vẫn dùng quy tắc số nguyên |
| Ngày giờ nhập tay | `datetime-local` chuyển bằng `thanhChuoiGioVn(giaTri)` thành chuỗi ISO có `+07:00`; trường `date` gửi nguyên giá trị `yyyy-MM-dd` (FA 4.8, ĐX-FA-10) |
| Mật khẩu | `type="password"`; `autoComplete` đúng ngữ cảnh (`current-password` ở đăng nhập, `new-password` ở tạo hoặc đặt lại); mật khẩu không được gán vào `useState`, ngữ cảnh, URL hay nhật ký |
| Nhãn và lỗi | Mỗi ô nhập dùng `FormField` bảo đảm: `<label htmlFor>` nhìn thấy; thông báo lỗi liên kết bằng `aria-describedby`; `aria-invalid` khi lỗi; dấu `*` cho trường bắt buộc và ghi chú "* Bắt buộc" ở đầu biểu mẫu có trường bắt buộc (DS 9.1) |
| Nút gửi | Vô hiệu khi đang chờ và khi thiếu trường bắt buộc mà `UI-Screens.md` nêu; lý do vô hiệu khác phải hiển thị cạnh nút (ĐX-DS-11) |

### 5.7 Kiểu dáng với TailwindCSS

| Quy tắc | Nội dung | Kiểm tra |
|---|---|---|
| Chỉ dùng token | Lớp tiện ích Tailwind chỉ lấy từ thang đã ánh xạ từ token (mục 8.1); không dùng màu, cỡ chữ, khoảng cách, bo góc, bóng của bảng mặc định Tailwind | Cấu hình theme (mục 8.1) |
| Không giá trị tùy ý | Cấm lớp có giá trị thô dạng `[#1f5499]`, `[13px]`, `[1.3rem]`; chỉ cho phép `[var(--ds-…)]` khi thang tiện ích chưa có token tương ứng | Bước `grep` ở CI (mục 10.3) |
| Không `style` thô | Không `style={{ … }}` với giá trị thô; được phép truyền biến CSS động | Review |
| Không `@apply` | Chỉ dùng trong `globals.css` để định nghĩa lớp vai trò kiểu chữ (mục 8.1) | Review |
| Nối lớp có điều kiện | Hàm `classNames(...)` nội bộ ở `shared/components/classNames.ts`; không thêm thư viện nối lớp | `no-restricted-imports` cho `clsx`, `classnames` |
| Không bố cục điện thoại | Không dùng biến thể đáp ứng (`sm:`, `md:`…) vì web chỉ hỗ trợ từ 1024 px (ĐX-FA-06, ĐX-DS-07); bảng rộng cuộn ngang trong `overflow-x-auto` của khung bảng, trang không cuộn ngang (TC-07) | Review |
| Màu và chữ | Chỉ token ngữ nghĩa hoặc token thành phần; không dùng màu nguyên thủy như `color.blue.600` trực tiếp (DS 3.1) | Review |
| Chữ số căn cột | Số lượng, tiền, thời gian trong bảng dùng lớp `tabular-nums` (ĐX-DS-12) | Review |

### 5.8 Khả năng tiếp cận trong mã web

Cụ thể hóa DS mục 10 thành quy tắc mã:

| Quy tắc | Nội dung |
|---|---|
| Phần tử gốc | Dùng `button`, `a`, `table` (kèm `th scope`), `dialog`, `label`, `input`; không dùng `div` hay `span` có `onClick` thay nút |
| Ngôn ngữ | `<html lang="vi">` ở `app/layout.tsx` |
| Nút biểu tượng | `IconButton` bắt buộc có `aria-label` tiếng Việt; biểu tượng trang trí có `aria-hidden="true"` |
| Vòng tiêu điểm | Không dùng `outline: none` nếu không thay bằng vòng tiêu điểm có token (`:focus-visible`); không bị cắt bởi `overflow` của khung chứa (DS 10) |
| Thông báo | Khối thông báo `danger` và `warning` do hành động của người dùng, lỗi trường và kết quả tra cứu ở ô nhập mã dùng `role="alert"`; khối `info` và `success` dùng `role="status"` (DS CMP-24); vùng đang tải đặt `aria-busy="true"`; bộ đếm ngược không có vùng thông báo trực tiếp theo từng giây (DS 10, CMP-21) |
| Không phụ thuộc rê chuột | Không có thông tin hay hành động chỉ hiện khi rê (DS 10); `title` không là nơi chứa lý do vô hiệu (ĐX-DS-11) |
| Thứ tự Tab | Theo thứ tự đọc; không dùng `tabIndex` dương |
| Màu | Trạng thái, lỗi, nhấn mạnh luôn kèm chữ (NT-02) |

**`Dialog` và `ConfirmDialog` (CMP-26, ĐX-DS-09)** **[ĐX-FC-18]**

| Quy tắc | Nội dung |
|---|---|
| Phần tử | `<dialog>` gốc, mở bằng `showModal()` trong `useEffect` khi `open` đổi sang `true` và đóng bằng `close()` khi đổi sang `false` (khóa tiêu điểm, đóng bằng Esc, trả tiêu điểm về nơi mở, FA 4.2) |
| Không dùng `<form>` | Không bọc nội dung hộp xác nhận trong `<form>`; mọi nút khai báo `type="button"` để phím Enter không kích hoạt nút xác nhận |
| Tiêu điểm ban đầu | Hộp loại Xác nhận và Kết quả: ngay sau `showModal()` gọi `focus()` lên nút **Hủy** (hoặc **Đóng**) qua `ref`; không dựa vào prop `autoFocus` của React vì phần tử được gắn khi hộp chưa mở. Hộp loại Biểu mẫu: trường đầu tiên (DS CMP-26) |
| Lớp che | Không gắn xử lý đóng khi bấm vào lớp che hay `::backdrop` |
| Esc | Xử lý sự kiện `cancel` của `<dialog>` để gọi `onDong`; khi nút xác nhận đang xử lý, sự kiện `cancel` bị `preventDefault()` và nút **Hủy** bị vô hiệu, để kết quả của thao tác ghi đang chờ không bị giấu (FA-08) |
| Thao tác phải xác nhận | Mọi thao tác thuộc danh sách DS 9.2 (TC-09) đi qua `ConfirmDialog`; thao tác không thuộc danh sách thì không thêm hộp xác nhận |
| Nút xác nhận | Mang tên hành động (DS 9.2); `bienThe` là `primary` hoặc `destructive` theo bảng DS 9.2; vô hiệu và hiển thị đang xử lý trong lúc chờ (UI-Screens 3.5); lỗi trả về hiển thị bằng `ErrorBanner` trong hộp |

**Ô nhập mã `CodeInput` (CMP-11, FA 4.9)**

| Quy tắc | Nội dung |
|---|---|
| Cấu trúc | Một `<form>` chứa đúng một ô văn bản; `onSubmit` gọi `preventDefault()` rồi xử lý, nên Enter và nút xác nhận có cùng đường xử lý |
| Giá trị | Cắt khoảng trắng hai đầu; chuỗi rỗng không gửi |
| Tiêu điểm | `ref.current.focus()` khi vào màn hình (`useEffect`) và sau mỗi lần xử lý xong (khi ô được bật lại); khi lỗi, `select()` toàn bộ chữ |
| Thành công | Xóa ô và trả tiêu điểm, trừ khi màn hình yêu cầu giữ mã (FA 4.9, DS CMP-11) |
| Trong lúc chờ | Ô và nút vô hiệu; vì ô vô hiệu mất tiêu điểm nên phải gọi lại `focus()` sau khi bật lại |
| Không làm | Không đo tốc độ gõ, không có logic riêng cho máy quét, không dùng camera (ĐX-FA-03) |

### 5.9 Ảnh trong mã web

| Quy tắc | Nội dung |
|---|---|
| Module | Chỉ `shared/anh/` kiểm tra và nén ảnh; `AnhUploader` chỉ điều phối và hiển thị |
| Hằng số | `SO_ANH_TOI_DA = 5`, `DUNG_LUONG_ANH_TOI_DA_BYTE = 2 * 1024 * 1024`, `CANH_DAI_TOI_DA_PX = 1280`, `CHAT_LUONG_NEN = [0.8, 0.7, 0.6]` (mục 7.1) |
| Nén | `createImageBitmap` → canvas nền trắng → `toBlob("image/jpeg", chấtLượng)` theo FA 4.10; hàm trả `Blob` hoặc lỗi "ảnh quá lớn" |
| Trạng thái ảnh | Kiểu hợp `"DANG_TAI" \| "DA_TAI" \| "LOI" \| "CO_SAN"` (bốn trạng thái của CMP-12) |
| Tải lên | Tuần tự từng ảnh bằng vòng `for … of` có `await`; không dùng `Promise.all` (ĐX-FA-09) |
| URL tạm | Mỗi `URL.createObjectURL` có đúng một `URL.revokeObjectURL` khi gỡ ảnh, khi gắn xong hoặc khi component bị gỡ |
| Chọn tệp | `<input type="file" accept="image/jpeg,image/png,image/webp" multiple>`; không dùng thuộc tính `capture` (FA 4.10) |
| Ảnh tùy chọn | Không có ảnh không chặn gửi; chỉ chặn khi còn ảnh đang tải hoặc ảnh lỗi chưa gỡ (ĐX-FA-09) |
| Hiển thị | Thẻ `<img>` thường với `loading="lazy"` và `alt` có nghĩa (ví dụ "Ảnh 1"); không `next/image` (ĐX-FA-11) |
| Ảnh hết hạn | `onError` của ảnh tình trạng gọi `refetch` của truy vấn chi tiết **đúng một lần** bằng cờ `useRef` của `AnhGallery` (đặt lại khi người dùng làm mới thủ công), rồi hiển thị "Không tải được ảnh" và dừng (FA 4.10) |
| Không lưu URL ký | Không đưa URL ảnh tình trạng vào `localStorage`, `sessionStorage`, tham số tuyến hay khóa truy vấn |

### 5.10 Phiên trong mã web

| Quy tắc | Nội dung |
|---|---|
| Module | `shared/api/session/` là nơi duy nhất đọc, ghi, xóa token; có hàm đăng ký lắng nghe kèm `lyDo` (FA 4.5) |
| Access token | Biến trong module, không đưa vào `useState`, ngữ cảnh, URL, `sessionStorage` |
| Refresh token | `sessionStorage` với khóa `KHOA_REFRESH_TOKEN` (hằng số trong module) **[ĐX-FC-03]** |
| Làm mới | Một `Promise` dùng chung cho interceptor và khôi phục phiên; Axios gọi làm mới dùng thể hiện riêng không có interceptor (FA 4.6) |
| Xóa phiên | Một hàm `xoaPhien(lyDo, loi?)` xóa token, bộ nhớ đệm truy vấn và phát sự kiện kèm `ApiError` nếu có; `AuthProvider` đặt `TrangThaiPhien` và chuyển tới `/dang-nhap` (FA 4.5). Mọi lỗi làm mới (401/429/mạng/timeout/500) đều kết thúc phiên và tất cả waiter, không replay; giữ nguyên nguyên nhân để 429 báo chờ và lỗi hệ thống có `traceId` ở đăng nhập, không tự làm mới/đăng nhập lại |
| Đọc thông tin | Qua `useAuth` (`shared/hooks/useAuth.ts`, mục 4.1) |

### 5.11 Điều cấm và cấu hình ESLint

| Điều cấm | Cơ chế kiểm tra |
|---|---|
| `dangerouslySetInnerHTML`; nhúng script bên thứ ba (FA 4.13) | `react/no-danger`; review |
| `console.log`, `console.info`, `console.debug` trong mã được commit | `no-console` ở mức `error` |
| `localStorage`; `sessionStorage` ngoài `shared/api/session/**` | `no-restricted-globals` kèm ghi đè cấu hình cho thư mục `session` |
| `axios` ngoài `shared/api/**`; `next/image`; `clsx`, `classnames` | `no-restricted-imports` |
| Vi phạm biên giới import ở mục 4.1 | `no-restricted-imports` với `patterns` theo thư mục (FA 4.2) |
| `any`, `!` | `@typescript-eslint` (mục 5.1) |
| `"use server"`, `route.ts`, `middleware.ts` | `no-restricted-syntax`; bước kiểm tra tên tệp ở CI |
| Ghi token, mật khẩu vào nhật ký, URL, thông điệp lỗi | Review (mục 11) |

Cấu hình ESLint dùng bộ quy tắc đi kèm Next.js (gồm React, React Hooks, tiếp cận JSX) cộng các quy tắc ở bảng trên; chỉ được tắt một quy tắc cho một dòng bằng chú thích nêu lý do.

### 5.12 Cấu hình Next.js

| Mục | Quy tắc |
|---|---|
| Tiêu đề bảo mật | `next.config` đặt `X-Content-Type-Options: nosniff`, `Referrer-Policy: strict-origin-when-cross-origin`, `X-Frame-Options: DENY`; không đặt CSP chặt (ĐX-FA-21) |
| Không chuyển hướng API | Không cấu hình `rewrites` hay `redirects` tới API (ĐX-FA-01) |
| Biến môi trường | Chỉ `NEXT_PUBLIC_API_BASE_URL` (không có `/api/v1`, không chứa bí mật); đọc ở một nơi duy nhất trong `shared/api/` và ném lỗi khi thiếu lúc khởi động; có tệp `.env.example` ghi tên biến và ví dụ giá trị giả (FA 4.14) |
| Phông | Mục 8.2 |

---

## 6. Quy ước mã Android (Java)

### 6.1 Ngôn ngữ và phong cách

| Quy tắc | Nội dung | Kiểm tra |
|---|---|---|
| Định dạng | `.editorconfig` ở thư mục gốc; mã định dạng bằng google-java-format kiểu AOSP (thụt 4 khoảng trắng) qua Spotless **[ĐX-FC-08]** | `spotlessCheck` ở CI |
| Nullability | Tham số, giá trị trả về và trường công khai khai báo `@NonNull` hoặc `@Nullable` (`androidx.annotation`). Trường của mô hình sinh từ API có thể `null`; mã dùng chúng phải kiểm tra | Android Lint; review |
| Không rò bộ nhớ | Không giữ `Context`, `View`, `Fragment` trong `ViewModel`, `Repository` hay trường `static` | Review |
| Không `System.out` | Không in ra nhật ký bằng `System.out`; xem mục 6.8 cho `Log` | Review |
| Kiểu | Không dùng kiểu thô (raw type); không bắt `Exception` hay `Throwable` chung chung ngoài `ApiErrorParser` | Review |
| Lint | Các kiểm tra `HardcodedText`, `ContentDescription`, `LabelFor`, `SetTextI18n` đặt mức `error` **[ĐX-FC-08]** | `lint` ở CI |
| Chú thích | Tiếng Việt có dấu, giải thích vì sao, kèm mã truy vết khi cài đặt quy tắc cụ thể; lớp công khai ở `data/` có Javadoc ngắn nêu mục đích và lỗi có thể trả | Review |

### 6.2 Fragment và ViewBinding

| Quy tắc | Nội dung |
|---|---|
| Vai trò | `Fragment` chỉ hiển thị theo `UiState` và chuyển sự kiện của người dùng cho `ViewModel`; không gọi API, không đọc phản hồi HTTP (FA 5.1) |
| ViewBinding | Trường `_binding` gán trong `onCreateView`, đặt về `null` trong `onDestroyView`; truy cập qua phương thức `binding()` không trả `null`; không giữ tham chiếu tới view sau `onDestroyView`; `RecyclerView` đặt `setAdapter(null)` trong `onDestroyView` |
| Quan sát | `LiveData` được quan sát bằng `getViewLifecycleOwner()`, không dùng `this` |
| Nạp dữ liệu | Mở màn hình và quay lại từ ngăn xếp luôn tải từ máy chủ; xoay màn hình không tải lại (FA 5.6). Cài bằng cờ `canTaiLai` của `ViewModel` (ban đầu `true`): `load()` chỉ tải khi cờ là `true` rồi đặt `false`; `Fragment` gọi `load()` trong `onViewCreated` (không nạp ở `onCreate`) và gọi `danhDauCanTaiLai()` trong `onDestroyView` trừ khi `requireActivity().isChangingConfigurations()` là `true`. Kéo để làm mới và **Thử lại** gọi `refresh()` tải vô điều kiện **[ĐX-FC-22]** |
| Tham số | Chỉ nhận qua Safe Args (`{Fragment}Args`), chỉ chứa mã và cờ chế độ (FA 5.3 quy tắc 1); không dùng `Bundle` thủ công |
| Điều hướng | Qua `{Fragment}Directions` do Safe Args sinh; không `navigate(R.id.…)` với id thô |
| Nút gửi | Vô hiệu ngay trong hàm xử lý bấm (trước khi `ViewModel` tạo `Call`) để chống gửi trùng (FA 5.6) |

```java
// Fragment (SCR-SV-06), rút gọn
@Override
public void onViewCreated(@NonNull View view, @Nullable Bundle savedInstanceState) {
    super.onViewCreated(view, savedInstanceState);
    viewModel.getUiState().observe(getViewLifecycleOwner(), this::hienThi);
    viewModel.load();
}

@Override
public void onDestroyView() {
    // Xoay màn hình giữ dữ liệu; rời màn hình thì lần sau phải tải lại (FA 5.6)
    if (!requireActivity().isChangingConfigurations()) {
        viewModel.danhDauCanTaiLai();
    }
    _binding = null;
    super.onDestroyView();
}
```

### 6.3 ViewModel, LiveData, `UiState` và `Event`

| Quy tắc | Nội dung |
|---|---|
| Phơi bày | `ViewModel` giữ `MutableLiveData` là trường `private`, chỉ phơi bày `LiveData` chỉ đọc |
| `UiState<T>` | Lớp bất biến ở `data/model` (FA 5.1): trạng thái nhận `DANG_TAI`, `THANH_CONG`, `RONG`, `LOI`, kèm dữ liệu và `ApiError`. Tạo bằng phương thức tĩnh `dangTai(duLieuCu)`, `thanhCong(duLieu)`, `rong()`, `loi(apiError, duLieuCu)`; không có hàm tạo công khai. Khi tải lại, `dangTai(duLieuCu)` giữ dữ liệu cũ để không nhấp nháy |
| `Event<T>` | Sự kiện một lần (điều hướng, thông báo lỗi ghi) đi qua `LiveData<Event<T>>`; đọc bằng `getContentIfNotHandled()` để không phát lại khi xoay màn hình (FA 5.1) |
| Gọi `Repository` | Callback của Retrofit chạy trên luồng chính, nên `ViewModel` cập nhật bằng `setValue`, không dùng `postValue` |
| Hủy | `ViewModel` giữ `Call` đang chạy; chỉ hủy im lặng truy vấn/kiểm tra không đổi nghiệp vụ khi bị thay thế (tìm kiếm) hoặc trong `onCleared()`. Write đã phát phải giữ theo dõi đến phản hồi hoặc chuyển sang nhánh chưa biết kết quả khi hủy/mất theo dõi do điều hướng; không hủy tất cả và bỏ qua `HUY` của write (FA 3.3, 3.11) |
| Chống gửi trùng | Cờ `boolean` đặt `true` đồng bộ trước khi tạo `Call`, đặt lại khi có phản hồi (FA 3.11) |
| Chưa biết kết quả | Với thao tác ghi nghiệp vụ đã phát bị `MANG`, `QUA_THOI_GIAN` hoặc `HUY`, giữ nhánh chưa biết kết quả khi quay lại; `ViewModel` phát `Event` yêu cầu kiểm tra qua API đọc hiện hữu (tải `SCR-SV-06` hoặc `SCR-SV-08` theo FA 5.6), không phát thành công/thất bại hoặc cho POST lại ngay. Chỉ cho gửi lại sau khi đã kiểm tra. Kiểm tra không đổi nghiệp vụ được **Thử lại** trực tiếp; đăng nhập được thử lại bằng tay ở màn hình đăng nhập, vẫn có thể đã tạo phiên/cập nhật bộ đếm và phải tôn trọng 429/khóa (ĐX-FA-22) |
| Phạm vi nav graph | `SoanPhieuViewModel` lấy bằng `new ViewModelProvider(navController.getViewModelStoreOwner(R.id.…), factory)` với id của nav graph con (FA 5.3 quy tắc 2) |
| Khởi tạo | Qua `ViewModelProvider.Factory` nhận `Repository` từ `AppContainer`; không dùng framework DI (ĐX-FA-12) |
| Đếm ngược | Mục 7.5; lặp tải lại theo ĐX-FA-18 nằm trong `ViewModel` (FA 5.6) |

### 6.4 `Repository` và tầng mạng

| Quy tắc | Nội dung |
|---|---|
| Tên phương thức | Trùng `operationId` (ví dụ `svDsPhieuYeuCauMuon`, `authDangNhapSinhVien`), như hàm API ở web **[ĐX-FC-02]** |
| Chữ ký | `Call<T> {operationId}(thamSo…, ApiCallback<T> callback)`; `ApiCallback<T>` có `onSuccess(T)` và `onError(ApiError)`; `Repository` dùng `Call.enqueue`, chuyển lỗi bằng `ApiErrorParser` và trả `Call` để `ViewModel` hủy được (FA 5.1) |
| Không rò kiểu | `Response`, `ResponseBody` và ngoại lệ mạng không ra khỏi `data/`; chỉ có mô hình dữ liệu và `ApiError` |
| `ApiErrorParser` | Là nơi duy nhất chuẩn hóa lỗi theo FA 5.5: trước hết lấy nguyên `ApiError` làm mới từ kết quả/wrapper nội bộ của tầng mạng, giữ status/maLoi/traceId (đặc biệt 429), không đổi thành `MANG` hoặc lỗi 401 gốc. Các lỗi còn lại: ưu tiên `Call.isCanceled()` → `HUY`, rồi `SocketTimeoutException`, `InterruptedIOException` → `QUA_THOI_GIAN`; `IOException` khác → `MANG`; thân không phải `application/problem+json` → `HTTP` không có `maLoi`. `HUY` của write đã phát phải qua mục 6.3, không bị nuốt |
| `OkHttpClient` | Tổng thời gian chờ tối đa 90 giây: `callTimeout`, `connectTimeout`, `readTimeout`, `writeTimeout` lấy từ `HangSo`; `retryOnConnectionFailure(false)` (FA 5.5, FA-08) |
| Địa chỉ | `BuildConfig.API_BASE_URL` (không có `/api/v1`); `/api/v1` là hằng số của `data/api` (FA 3.2) |
| Mã sinh | Cấu hình `apiPackage` và `modelPackage` của bộ sinh vào gói con `generated` của gói gốc; chỉ nhóm Xác thực và Sinh viên (ĐX-FA-16); sinh kiểu thời gian `java.time` (tùy chọn `dateLibrary` của bộ sinh); không sửa tay mã sinh |
| Mã trạng thái lạ | Trường mã trạng thái, loại, kết quả phải đọc được khi giá trị không có trong enum đã sinh và vẫn hiển thị nguyên mã (FA 5.5); cách cấu hình bộ sinh hoặc bộ chuyển đổi JSON chọn khi thiết lập, tiêu chí đạt là kiểm thử "Mã lạ" ở FA 5.10 **[ĐX-FC-21]** |
| `Authenticator`, `AuthInterceptor` | Theo FA 5.4 và 5.5; `AuthInterceptor` không gắn `Authorization` cho đăng nhập SV, làm mới, đăng xuất |

### 6.5 Phiên trên Android

| Quy tắc | Nội dung |
|---|---|
| Lớp duy nhất | `TokenStore` là lớp duy nhất đọc, ghi `SharedPreferences` của token; `KeystoreCipher` là lớp duy nhất dùng `AndroidKeyStore`; `SessionManager` là lớp duy nhất giữ token trong bộ nhớ và phát `LiveData` trạng thái phiên; mã khác lấy token qua `SessionManager` (FA 5.1, 5.4) |
| Mã hóa | `AES/GCM/NoPadding`, IV ngẫu nhiên mỗi lần ghi, lưu IV cùng bản mã dạng Base64; bí danh khóa là hằng số trong `KeystoreCipher` (FA 5.4) |
| Không sao lưu | `android:allowBackup="false"` và khai báo loại tệp `SharedPreferences` của token khỏi sao lưu đám mây và chuyển thiết bị trong `backup_rules.xml` và `data_extraction_rules.xml` (FA 5.4, ĐX-FA-14) |
| Lỗi giải mã | Khóa mất hoặc bản mã hỏng được coi như không có phiên: xóa dữ liệu, về đăng nhập (FA 5.4) |
| Phiên kết thúc | `MainActivity` quan sát trạng thái phiên và dùng action toàn cục về `DangNhapSinhVienFragment`, xóa ngăn xếp quay lại (FA 5.3 quy tắc 5); khi đăng xuất, hết phiên, bị khóa hoặc bất kỳ lần làm mới thất bại (401/429/mạng/timeout/500), xóa token và mọi dữ liệu đã tải của người dùng trước (FA 3.5). Dừng yêu cầu gốc/waiter, không replay; chuyển `ApiError` đã bảo toàn tới đăng nhập để báo đúng lý do, 429 hướng dẫn chờ |
| Đăng xuất | Gọi `authDangXuat`, bỏ qua kết quả, xóa phiên cục bộ, không hộp xác nhận (UI-Screens 4.1, FA 5.4) |

### 6.6 Tài nguyên và giao diện

| Quy tắc | Nội dung | Kiểm tra |
|---|---|---|
| Không chữ cứng | Mọi chữ hiển thị nằm trong `strings.xml` (tiếng Việt, có dấu); không dựng chữ bằng nối chuỗi; dùng placeholder `%1$s`, `%1$d` hoặc `plurals` | Lint `HardcodedText`, `SetTextI18n` |
| Không màu và kích thước thô | Layout, style, mã Java không có mã màu (`#…`), `px`; màu, khoảng cách, bo góc, cỡ chữ dùng tài nguyên `ds_…` (mục 8.4) | Bước `grep` ở CI (mục 10.3) |
| Chỉ token ngữ nghĩa cho màu và chữ | Không dùng tài nguyên màu nguyên thủy (ví dụ `@color/ds_color_blue_600`) ngoài `ds_colors.xml` (DS 3.1, ĐX-DS-02) | Bước `grep` ở CI |
| Đơn vị | Kích thước `dp`; chữ `sp`; chiều cao khung chứa chữ dùng `minHeight` hoặc `wrap_content`, không cố định; nội dung không có chiều rộng cố định; dùng `ConstraintLayout`, `0dp`, `wrap_content` (FA 5.6, NFR-04) | Review; kiểm tra TC-05 |
| Vùng chạm | Mọi điều khiển tối thiểu 48 × 48 dp (`ds_size_touch_min`), kể cả khi phần nhìn thấy nhỏ hơn (TC-06) | Công cụ kiểm tra tiếp cận của Android (TC-06) |
| Tên truy cập | `ImageButton`, ảnh, ô tích, bộ chọn số lượng có `contentDescription` tiếng Việt; biểu tượng trang trí dùng `importantForAccessibility="no"` (DS 10) | Lint `ContentDescription` |
| Nhãn ô nhập | Mọi ô nhập có nhãn nhìn thấy; lỗi hiển thị bằng `TextInputLayout.setError` ở đúng ô | Review |
| Lỗi theo trường | Hàm thuần `LoiTruong` đặt trong `ui/<tính năng>` tương ứng, đổi khóa `errors` (ví dụ `dsLoaiVaSoLuong[2].soLuong`) thành đích (ô số lượng của dòng 2); không giữ `View`/`Context`, không được gọi từ `data`. Khóa không khớp ô nào hiển thị ở `BannerView` đầu màn hình; tiêu điểm tới ô lỗi đầu tiên (FA 5.9) | Kiểm thử đơn vị |
| Không Snackbar, Toast | Lỗi ghi và thông báo hiển thị bằng `BannerView` (CMP-24) ở đầu màn hình hoặc hộp thoại đang mở; không dùng `Snackbar` hay `Toast` vì DS ĐX-DS-16 không dùng thanh thông báo nổi **[ĐX-FC-19]** | Review |
| Hộp thoại | `DialogFragment`; nút Quay lại hệ thống tương đương **Hủy**; `setCanceledOnTouchOutside(false)`; tiêu điểm ban đầu ở **Hủy** (hoặc **Đóng**) với hộp Xác nhận và Kết quả (DS CMP-26, ĐX-DS-09) | Review |
| Chế độ sáng | Theme là biến thể sáng của Material Components, không dùng `DayNight`; đặt `android:forceDarkAllowed` là `false` (ĐX-DS-01) | Review |
| Chuyển động | Chỉ dùng thời lượng của token `motion.*` và API hoạt họa của hệ thống để tôn trọng thang thời lượng hoạt họa bằng 0 (ĐX-DS-15) | Review |
| Ảnh | Glide; ảnh tình trạng `DiskCacheStrategy.NONE`; ảnh danh mục dùng bộ nhớ đệm mặc định; có hình thay thế khi thiếu ảnh; không lưu URL ký, không ghi vào nhật ký (FA 5.7, ĐX-FA-15). Khi tải ảnh tình trạng thất bại, `ViewModel` tải lại chi tiết phiếu mượn đúng một lần để lấy URL mới; vẫn lỗi thì hiển thị khung "Không tải được ảnh" và dừng | Review |
| Danh sách | `RecyclerView` với `ListAdapter` và `DiffUtil` theo mã đối tượng; thứ tự do máy chủ quyết định, không sắp xếp lại (FA 3.10); phân trang bằng `OnScrollListener`; lỗi tải thêm hiển thị dòng cuối kèm **Thử lại** (FA 5.6) | Review |
| Kéo để làm mới | `SwipeRefreshLayout` ở mọi danh sách và chi tiết có dữ liệu máy chủ (FA 5.6) | Review |

### 6.7 Điều hướng

| Quy tắc | Nội dung |
|---|---|
| Nav graph | Một nav graph chính, `MainActivity` chứa `NavHostFragment`, điểm bắt đầu `KhoiDongFragment` (FA 5.3); nav graph con cho soạn phiếu |
| Đặt tên | Theo mục 3.3 |
| Chọn thông báo | Một hàm ánh xạ `loaiThongBao` sang đích điều hướng theo bảng của `SCR-SV-13`; loại không có điều hướng hoặc loại lạ thì không làm gì (FA 5.3 quy tắc 4) |
| Kiểm tra quyền mượn | `HoSoMuonRepository` gọi `svToi` tại thời điểm cần, không dùng giá trị đã đệm (FA 5.3 quy tắc 3) |

### 6.8 Nhật ký

| Quy tắc | Nội dung |
|---|---|
| `Log` | Chỉ trong khối `if (BuildConfig.DEBUG)`; không ghi token, mật khẩu, thân yêu cầu, tiêu đề `Authorization`, URL ảnh có chữ ký |
| `HttpLoggingInterceptor` | Nếu dùng thì chỉ ở build debug, mức `BASIC` (FA 5.4) |
| HTTPS | Bản release `usesCleartextTraffic="false"` (FA 5.5) |

---

## 7. Quy ước dùng chung cho hai nền tảng

### 7.1 Hằng số và nơi đặt

Giá trị lấy từ tài liệu nguồn; không đặt số hoặc chuỗi cố định ở nơi khác. Muốn đổi giá trị phải đổi tài liệu nguồn trước. Mỗi hằng số có đúng một nơi định nghĩa **[ĐX-FC-03]**:

| Hằng số | Giá trị | Nguồn | Web | Android |
|---|---|---|---|---|
| Thời gian chờ HTTP | 90 giây | ĐX-FA-04 | `THOI_GIAN_CHO_HTTP_MS = 90_000` ở `shared/api/` | `HangSo.THOI_GIAN_CHO_TOI_DA_GIAY = 90` |
| Ngưỡng hiện thông điệp "Hệ thống đang khởi động, vui lòng chờ" | 8 giây | ĐX-FA-04 | `NGUONG_THONG_BAO_KHOI_DONG_MS = 8_000` ở `shared/hooks/` | `HangSo.NGUONG_THONG_BAO_KHOI_DONG_GIAY = 8` |
| Độ trễ gõ tìm kiếm (chỉ ô tìm theo tên của SV) | 400 ms | ĐX-FA-05 | Không áp dụng | `HangSo.DO_TRE_GO_TIM_KIEM_MS = 400` |
| Số lần tải lại tối đa khi đếm ngược về 0 mà máy chủ còn trả trạng thái cũ | 6 | ĐX-FA-18 | `SO_LAN_TAI_LAI_TOI_DA = 6` ở `shared/hooks/` (nếu có bộ đếm, mục 7.5) | `HangSo.SO_LAN_TAI_LAI_TOI_DA = 6` |
| Khoảng cách giữa các lần tải lại đó | 10 giây | ĐX-FA-18 | `KHOANG_CACH_TAI_LAI_MS = 10_000` | `HangSo.KHOANG_CACH_TAI_LAI_GIAY = 10` |
| Số ảnh tối đa mỗi thiết bị mỗi lần | 5 | UI-Screens 3.6 | `SO_ANH_TOI_DA` ở `shared/anh/` | Không áp dụng (SV không tải ảnh) |
| Dung lượng ảnh tối đa sau nén | 2 MB (2 × 1024 × 1024 = 2.097.152 byte) | UI-Screens 3.6; `DatabaseDesign.md` (`ck_anh_kich_thuoc`) | `DUNG_LUONG_ANH_TOI_DA_BYTE` | Không áp dụng |
| Cạnh dài tối đa của ảnh | 1280 px | UI-Screens 3.6 | `CANH_DAI_TOI_DA_PX` | Không áp dụng |
| Chất lượng nén JPEG, theo thứ tự thử | 0,8 rồi 0,7 rồi 0,6 | FA 4.10 | `CHAT_LUONG_NEN = [0.8, 0.7, 0.6]` | Không áp dụng |
| Định dạng ảnh chấp nhận | `image/jpeg`, `image/png`, `image/webp` | UI-Screens 3.6 | `KIEU_ANH_CHAP_NHAN` | Không áp dụng |
| Kích thước trang | Mặc định 20; tối đa 100 | UI-Screens 3.7 | `KICH_THUOC_TRANG_MAC_DINH = 20` | `HangSo.KICH_THUOC_TRANG = 20` |
| Múi giờ hiển thị | `Asia/Ho_Chi_Minh` | UI-Screens 3.4 | `MUI_GIO_VN` ở `shared/format/` | `HangSo.MUI_GIO_VN` |
| Độ lệch gửi cho giờ nhập tay | `+07:00` | ĐX-FA-10 | `DO_LECH_GIO_VN` ở `shared/format/` | Không áp dụng (SV không nhập giờ tuyệt đối) |
| Khóa lưu refresh token | `refreshToken` | ĐX-FC-03 | `KHOA_REFRESH_TOKEN` ở `shared/api/session/` | Không áp dụng |

`minSdk` 26 và chiều rộng web tối thiểu 1024 px không phải hằng số trong mã: khai báo ở cấu hình build (Android) và ở thiết kế (web, `layout.web.min-width` của DS 11.2).

### 7.2 Thời gian và tiền

Hàm định dạng chỉ định dạng giá trị máy chủ trả về, không tính toán (FA 3.7, FA-07). Tên hàm, cách làm và ví dụ kiểm thử **[ĐX-FC-12]**:

| Nội dung | Web (`shared/format/`) | Android (`common/format/`) |
|---|---|---|
| Thời điểm (`date-time`, UTC) → `dd/MM/yyyy HH:mm` giờ Việt Nam | `formatNgayGio(iso: string \| null \| undefined): string`. Dùng `Intl.DateTimeFormat("vi-VN", { timeZone: MUI_GIO_VN, hourCycle: "h23", … })` với `formatToParts` rồi ghép `dd/MM/yyyy HH:mm` thủ công, không dùng chuỗi mặc định của locale (thứ tự phần tử khác nhau giữa các môi trường). Bộ định dạng tạo một lần ở mức module | `FormatThoiGian.ngayGio(OffsetDateTime)` dùng `DateTimeFormatter.ofPattern("dd/MM/yyyy HH:mm")` với `ZoneId.of("Asia/Ho_Chi_Minh")` |
| Ngày (`date`, `yyyy-MM-dd`) → `dd/MM/yyyy` | `formatNgay(chuoi)`: tách chuỗi, **không** tạo `Date` (tránh lệch múi giờ) | `FormatThoiGian.ngay(LocalDate)`: không đổi múi giờ |
| Giờ nhập tay → gửi | `thanhChuoiGioVn("2026-11-05T14:30")` trả `"2026-11-05T14:30:00+07:00"` | Không áp dụng |
| Giờ (giờ mở, đóng cửa quầy, `HH:mm[:ss]`) → `HH:mm` | `formatGio(chuoi)`: lấy 5 ký tự đầu, không đổi múi giờ (giá trị đã là giờ Việt Nam) | `FormatThoiGian.gio(String)`: như web |
| Tiền | `formatTien(soNguyen)`: nhóm hàng nghìn bằng dấu chấm bằng biểu thức chính quy (`String(n).replace(/\B(?=(\d{3})+(?!\d))/g, ".")`) để kết quả không phụ thuộc dữ liệu locale của môi trường chạy, rồi nối khoảng trắng không ngắt `\u00A0` và `VNĐ` (DS 9.7) | `FormatTien.tien(long)`: `DecimalFormat` với `DecimalFormatSymbols` đặt dấu nhóm `'.'` tường minh, không phụ thuộc locale máy, rồi nối `\u00A0` và `VNĐ` |
| Giá trị thiếu | `null`, `undefined` → "—" (ĐX-DS-12) | `null` → "—" |
| Chuỗi nhập → số nguyên | `docSoNguyen(chuoi)` (mục 5.6) | Không áp dụng: SV không nhập số bằng bàn phím, số lượng chọn bằng `QuantityStepperView` |
| Tỷ lệ phần trăm → hiển thị/nhập | `formatTyLePhanTram(tyLe)`: giữ nguyên giá trị số API, đổi dấu thập phân trong chuỗi số thành dấu phẩy rồi nối `%`; không nhân 100 hoặc tự làm tròn. API `15.5` hiển thị `15,5%`; nhập `15,5` hoặc `15.5` qua `docTyLePhanTram` (mục 5.6) đều gửi JSON number `15.5` | Không áp dụng: SV không quản lý khung phạt |

Ví dụ kiểm thử bắt buộc (cả hai nền tảng): `2026-11-05T07:30:00Z` → `05/11/2026 14:30`; `2026-11-05T17:00:00Z` → `06/11/2026 00:00` (qua nửa đêm, không ra `24:00`); ngày `2026-11-05` → `05/11/2026`; `240000` → `240.000 VNĐ`; `0` → `0 VNĐ`, trong đó khoảng trắng giữa số và `VNĐ` là khoảng trắng không ngắt U+00A0 (DS 9.7), kiểm thử so sánh đúng ký tự này; giờ `07:30:00` → `07:30`; `null` → `—`.

Riêng web `SCR-QT-07`: `15,5` và `15.5` đều thành `15.5`, hiển thị `15,5%`; `15.5abc`, `1.234,5`, `1,2,3`, `NaN`, `Infinity` bị từ chối cú pháp. `0`, số âm và `101` không bị cắt/đổi thành giá trị hợp lệ; miền phần trăm vẫn do BE kiểm tra. `docSoNguyen("15.5")` và `docSoNguyen("15,5")` vẫn trả `null` cho các ô số nguyên.

### 7.3 Nhãn trạng thái

| Quy tắc | Web | Android |
|---|---|---|
| Nơi đặt | `shared/labels/` | `common/nhan/` (FA 5.2) |
| Khóa tra | Cặp (họ, mã) theo DS 7.1: một mã có thể trùng giữa các họ nên luôn tra theo họ | Như web |
| Nhãn chữ | Bảng tĩnh trong `shared/labels/trangThai.ts` | `strings.xml` với khóa `nhan_{ho}_{ma}` (mục 3.3) |
| Tông và mức nhấn | Cùng bảng, theo DS 7.2 | Bảng trong `common/nhan/` theo DS 7.2 |
| Hàm | `nhanTrangThai(ho, ma: string): string` và `thongTinTrangThai(ho, ma: string)` trả `{ nhan, tong, nhanDam }` | `NhanTrangThai.layNhan(Context, ho, ma)` và `thongTin(ho, ma)` |
| Mã chưa biết | Trả nguyên mã, tông `neutral`, không biểu tượng (DS 7.1) | Như web |
| Nhãn phát sinh | `StatusBadge` nhận khóa `nhanPhatSinh` thay cho (họ, mã): `HET_HANG`, `KHONG_DU`, `HOP_LE`, `KHONG_HOP_LE`, `DUOC_DUYET`, `KHONG_DUOC_DUYET`, `DA_NGUNG_SU_DUNG`, `DANG_SU_DUNG`, `DA_TAT`, `CHUA_TRA`, `BAO_TRI`, `THANH_LY`, `TAM_TINH`, tương ứng DS 7.3 | `StatusBadgeView` có phương thức tương ứng |
| Màn hình | Không tự đặt nhãn, màu hay chữ trạng thái; chỉ dùng `StatusBadge` (CMP-13) | Chỉ dùng `StatusBadgeView` |
| Vai trò | Hiển thị bằng chữ thường, không dùng `StatusBadge` (DS 7.1) | Như web |
| Kiểm thử | Mọi mã ở Phụ lục A của `User-Flows.md` có nhãn; mã lạ trả nguyên mã (FA 4.15, 5.10) | Như web |

### 7.4 Lỗi dùng chung

| Quy tắc | Web | Android |
|---|---|---|
| Kiểu `ApiError` | `shared/api/apiError.ts` theo FA 3.3 | `data/model/ApiError.java` với các trường tương ứng (`loai`, `status`, `maLoi`, `detail`, `traceId`, `errors`) |
| Nhóm xử lý | `shared/api/nhomLoi.ts`: bảng `Record<MaLoi, NhomLoi>` khai báo kiểu đầy đủ theo kiểu sinh từ OpenAPI (thiếu mã thì `tsc` báo lỗi khi CI sinh lại kiểu), kèm tra cứu lúc chạy `?? "NGHIEP_VU_THEO_MAN_HINH"` cho mã lạ | `data/model/NhomLoi.java` (enum) và `ApiError.nhom()` cài bảng FA 3.4 một lần |
| Tên nhóm | `PHIEN`, `DANG_NHAP_SAI`, `KHOA_TAI_KHOAN`, `QUYEN`, `DU_LIEU_NHAP`, `KHONG_TON_TAI`, `CAN_TAI_LAI`, `TAN_SUAT`, `NGHIEP_VU_THEO_MAN_HINH`, `HE_THONG`, `MANG` | Giống web |
| Đối chiếu bảng | Mọi `maLoi` của `MaLoi` (32 mã ở `openapi.yaml` v1.1.0) thuộc đúng một nhóm theo FA 3.4 | Như web, kiểm thử đơn vị liệt kê 32 mã |
| Thông điệp | Mục 5.5 | `strings.xml` khóa `loi_{nhom}` cho thông điệp dùng chung và `{ten_man_hinh}_loi_{ma_loi}` cho thông điệp riêng |

### 7.5 Đếm ngược giữ chỗ

Chỉ Android có màn hình dùng bộ đếm (`SCR-SV-07` phiếu Đã duyệt, `SCR-SV-08` đăng ký Đã thông báo; DS CMP-21 "Dùng ở"); bảng của NV hiển thị hạn tuyệt đối, không có bộ đếm. Hook `useDemNguoc` ở web chỉ được viết khi có màn hình web dùng bộ đếm; khi viết phải theo cùng quy tắc dưới đây **[ĐX-FC-20]**.

| Quy tắc | Nội dung |
|---|---|
| Nguồn | Còn lại = `hanGiuCho` (thời điểm tuyệt đối do máy chủ trả) trừ thời điểm hiện tại của thiết bị, không nhỏ hơn 0; không lưu "số giây còn lại" làm trạng thái gốc (FA 3.8, ĐX-UI-07) |
| Nhịp | Một nhịp 1 giây cho cả màn hình (dù có nhiều dòng đếm ngược, FA 3.8); chỉ chạy khi màn hình đang hiển thị: Android khởi động ở `onStart` và dừng ở `onStop` (FA 5.6); web dừng khi component bị gỡ hoặc tab ẩn (`visibilitychange`) và tính lại từ thời điểm tuyệt đối khi hiện lại |
| Định dạng | `mm:ss` dưới 1 giờ, `HH:mm:ss` từ 1 giờ; luôn kèm hạn tuyệt đối `dd/MM/yyyy HH:mm` (ĐX-DS-13, CMP-21) |
| Khi về 0 | Hiển thị "Đang cập nhật" kèm chỉ báo quay (CMP-21), tải lại dữ liệu từ máy chủ **một lần cho cả màn hình**, không tự đổi trạng thái (FA 3.8, ĐX-UF-04) |
| Máy chủ còn trả trạng thái cũ | Trạng thái cũ nghĩa là đối tượng vẫn ở trạng thái có đếm ngược và `hanGiuCho` đã qua. Khi đó tải lại tiếp, cách nhau 10 giây; **tổng cộng tối đa 6 lần tải lại tính từ lúc về 0** (lần 1 ngay khi về 0, các lần 2 đến 6 cách nhau 10 giây); sau lần thứ 6 dừng và để người dùng làm mới thủ công (ĐX-FA-18) |
| Hủy | Hủy chuỗi tải lại khi rời màn hình, khi dữ liệu trả về đã đổi trạng thái, hoặc khi người dùng làm mới thủ công |
| Tiếp cận | Không dùng vùng thông báo trực tiếp cho từng giây; hạn tuyệt đối là thông tin chính cho trình đọc màn hình (DS CMP-21) |
| Kiểm thử | Logic đếm tách thành lớp hoặc hàm thuần nhận hàm cấp thời điểm hiện tại để kiểm thử không phụ thuộc đồng hồ thật (mục 9) |

---

## 8. Hiện thực Design-System trong mã

### 8.1 Token trên web **[ĐX-FC-06, ĐX-FC-07]**

| Quy tắc | Nội dung |
|---|---|
| Tệp | `src/app/tokens.css` khai báo mọi token nguyên thủy và ngữ nghĩa thành biến CSS trong `:root`, tên theo DS Phụ lục C (tiền tố `--ds-`, dấu chấm đổi thành gạch ngang). Giá trị chép nguyên từ `Design-System.md`, không đổi, không thêm giá trị không có ở DS |
| Tham chiếu | Token ngữ nghĩa tham chiếu token nguyên thủy bằng `var(--ds-…)`, không lặp giá trị (DS Phụ lục C) |
| Vai trò kiểu chữ | Mỗi vai trò ở DS 5.3 là ba biến `-size`, `-line`, `-weight` kèm họ phông và một lớp tiện ích `type-{vai-tro}` (ví dụ `type-body`) định nghĩa trong `globals.css` |
| Giảm chuyển động | `globals.css` đặt các biến `--ds-motion-duration-*` về `0ms` trong `@media (prefers-reduced-motion: reduce)`; chỉ báo đang tải vẫn quay (DS 4.5, TC-15) |
| Theme Tailwind | Cấu hình theme **thay thế** (không mở rộng) các nhóm màu, khoảng cách, bo góc, đổ bóng, kích thước, thứ tự lớp bằng giá trị `var(--ds-…)`, để lớp mặc định của Tailwind (ví dụ `bg-blue-500`) không còn dùng được. Tên lớp suy ra máy móc từ tên token: bỏ nhóm đầu, đổi dấu chấm thành gạch ngang (`color.action.primary.bg` → `bg-action-primary-bg`; `space.4` → `p-4`; `radius.md` → `rounded-md`; `shadow.dialog` → `shadow-dialog`; `size.control.md` → `h-control-md`). Cú pháp và vị trí cấu hình theo phiên bản Tailwind nhóm dùng |
| Chỉ một nơi | Mọi giá trị thô (mã màu, px, rem) chỉ xuất hiện ở `tokens.css` (TC-01) |
| Đổi diện mạo | Đổi lớp token nguyên thủy hoặc ngữ nghĩa ở `tokens.css`; không sửa từng màn hình (ĐX-DS-01, ĐX-DS-02) |

### 8.2 Phông web **[ĐX-FC-04]**

`Design-System.md` ĐX-DS-04 yêu cầu Roboto (400, 500, 700) và Roboto Mono (400) có tập ký tự tiếng Việt và giao cách nạp cho kiến trúc, nhưng `Frontend-Architecture.md` chưa chọn. Quy ước:

| Quy tắc | Nội dung |
|---|---|
| Cách nạp | `next/font/google` trong `app/layout.tsx`, tự lưu trữ lúc build nên trình duyệt không gọi dịch vụ phông bên ngoài lúc chạy; đây là tính năng của Next.js, không thêm thư viện và không nhúng script bên thứ ba (ĐX-FA-21) |
| Tham số | Roboto: `subsets` gồm `vietnamese` và `latin`, `weight` `400`, `500`, `700`, `display: "swap"`, `variable: "--font-roboto"`. Roboto Mono: cùng tập ký tự, `weight` `400`, `variable: "--font-roboto-mono"` |
| Gắn token | `--ds-font-family-sans: var(--font-roboto), system-ui, "Segoe UI", Arial, sans-serif` và `--ds-font-family-mono: var(--font-roboto-mono), ui-monospace, Consolas, monospace` (DS 4.2); phông dự phòng có tiếng Việt |
| Chữ số | Số lượng, tiền, thời gian dùng `font-variant-numeric: tabular-nums` (ĐX-DS-12) |
| Mã | Mã phiếu, thiết bị, SV, biên bản và `traceId` dùng lớp `type-code` (phông đơn cách, ĐX-DS-04) |
| Android | Phông sans-serif mặc định của hệ thống; `monospace` cho mã; không gói phông vào ứng dụng (ĐX-DS-04) |

### 8.3 Biểu tượng **[ĐX-FC-05]**

| Quy tắc | Web | Android |
|---|---|---|
| Nguồn | Material Symbols Outlined, độ đậm nét 400, đúng danh sách ở DS Phụ lục A (ĐX-DS-05) | Như web |
| Dạng | Mỗi biểu tượng là một component SVG nhúng trong `shared/components/icons/`, tên `Icon{TenPascal}` (`check_circle` → `IconCheckCircle`), `fill="currentColor"`, kích thước theo `size.icon.sm`/`md`/`lg`, mặc định `aria-hidden="true"` | Vector drawable `ic_{ten}` (mục 3.3), màu qua `app:tint` bằng token ngữ nghĩa |
| Thêm mới | Biểu tượng ngoài DS Phụ lục A không được thêm; phải bổ sung vào `Design-System.md` trước | Như web |
| Không dùng | Thư viện biểu tượng, phông biểu tượng | Như web |

### 8.4 Token trên Android **[ĐX-FC-06]**

| Quy tắc | Nội dung |
|---|---|
| Tệp | `res/values/ds_colors.xml` (màu nguyên thủy và ngữ nghĩa), `ds_dimens.xml` (khoảng cách, bo góc, viền, kích thước, đơn vị `dp`; cỡ chữ `sp`), `ds_type.xml` (kiểu chữ `TextAppearance.Ds.*`); tên tài nguyên theo DS Phụ lục C (`ds_color_blue_600`, `ds_space_4`, `ds_font_size_14`) |
| Tham chiếu | Token ngữ nghĩa tham chiếu nguyên thủy bằng `@color/ds_…`, không lặp giá trị |
| Độ mờ | `color.overlay.scrim` là `#80161A21` và `color.overlay.pressed` là `#1F161A21` (ARGB) theo DS Phụ lục C |
| Kiểu chữ | Một vai trò là một `TextAppearance.Ds.{VaiTro}`; mã dùng `fontFamily="monospace"`; số dùng `fontFeatureSettings="tnum"` (DS 4.2) |
| Kiểu thành phần | `Widget.Ds.{ThanhPhan}.{BienThe}` kế thừa kiểu Material Components, chỉ đổi thuộc tính bằng token `ds_…` |
| Chỉ một nơi | Mã màu và kích thước thô chỉ xuất hiện ở ba tệp trên (TC-01) |

### 8.5 Thành phần theo Design-System

| Quy tắc | Nội dung |
|---|---|
| Danh mục | Đúng 31 thành phần của DS 8.1 (Phụ lục A); không thêm biến thể, thành phần hay hiệu ứng mà `UI-Screens.md` không dùng (NT-07) |
| Một thành phần cho một mục đích | Trạng thái nghiệp vụ chỉ qua `StatusBadge`; xác nhận chỉ qua `ConfirmDialog` hoặc `XacNhanDialogFragment`; lỗi hệ thống, rỗng, đang tải của vùng dữ liệu chỉ qua `StateView` (CMP-25) |
| Biến thể | Khai báo bằng prop kiểu hợp chuỗi ở web (ví dụ `bienThe: "primary" \| "secondary" \| "tertiary" \| "destructive"` của `Button`, theo DS CMP-01; `tong: "info" \| "success" \| "warning" \| "danger"` của `Banner`, theo DS CMP-24, không có `neutral`) và bằng style ở Android; không nhận màu hay chữ tùy ý (NT-05, DS CMP-13) |
| `Banner` | Không tự đóng và không có nút đóng; biến mất khi nguyên nhân đã giải quyết hoặc khi rời màn hình; một khu vực tối đa một khối cùng tông (DS CMP-24) |
| Hành động ở dòng bảng | `DataTable` nhận danh sách hành động và gom vào `Menu` theo ngưỡng của DS ĐX-DS-14: tối đa 2 hành động hiển thị trực tiếp, từ 3 trở lên vào `Menu`; mục không áp dụng cho trạng thái hiện tại thì ẩn, không vô hiệu (DS CMP-20). Ngưỡng là một hằng số trong `DataTable` |
| Lý do vô hiệu | Hành động bị vô hiệu do nghiệp vụ phải kèm lý do hiển thị cạnh đó, không chỉ ở `title` (ĐX-DS-11, TC-12) |
| Trạng thái đủ | Mỗi vùng dữ liệu cài đủ: đang tải, tải chậm, rỗng, lỗi mạng, lỗi hệ thống (TC-10) |

---

## 9. Kiểm thử

Phạm vi bắt buộc theo ĐX-FA-17 (`Frontend-Architecture.md` 4.15 và 5.10); mục này quy định cách viết, đặt tên và bổ sung các kiểm thử phát sinh từ quy ước này.

### 9.1 Công cụ và vị trí

| Nền tảng | Công cụ | Vị trí |
|---|---|---|
| Web | Vitest, React Testing Library; môi trường mô phỏng trình duyệt cho kiểm thử component | Cạnh tệp được kiểm thử, `{Ten}.test.ts` hoặc `.test.tsx` |
| Android — JVM | JUnit; `MockWebServer` cho `TokenAuthenticator` và `ApiErrorParser`; boundary mã hóa giả cho logic lưu phiên, không có Android Keystore thật | `src/test/java/…` cùng gói với lớp được kiểm thử |
| Android — thiết bị/emulator | Kiểm tra thủ công Android Keystore và lưu phiên thật theo mục 9.4, dùng app hiện có, không thêm thư viện kiểm thử | Ghi kết quả riêng trước mốc bàn giao; không thuộc `testDebugUnitTest` |

Không thêm thư viện giả lập HTTP hay thư viện kiểm thử khác ngoài danh sách (mục 1.4). Ở web, kiểm thử lớp gọi API thay bộ chuyển đổi (`adapter`) của Axios bằng hàm giả hoặc thay hàm của `api.ts` bằng `vi.mock`.

### 9.2 Đặt tên và cấu trúc

| Quy tắc | Web | Android |
|---|---|---|
| Tên kiểm thử | `describe("{ThanhPhanHoacModule}")` rồi `it("{MaTruyVet}: {tình huống} → {kết quả mong đợi}")` bằng tiếng Việt có dấu **[ĐX-FC-13]** | Tên phương thức `{MaTruyVet}_{TinhHuong}_{KetQuaMongDoi}`, tiếng Việt không dấu, cùng dạng với Backend-Conventions 17.3 **[ĐX-FC-13]** |
| `MaTruyVet` | Mã luồng (`UF-CM-03`), mã quyết định (`ĐX-FA-18`), mã thành phần (`CMP-11`) hoặc tên quy tắc; mỗi kiểm thử ít nhất một mã | Như web; viết liền không gạch ngang (`UFCM03`) |
| Mẫu | Chuẩn bị – thực hiện – kiểm tra; một ý chính cho mỗi kiểm thử | Như web |
| Thời gian | Kiểm thử đếm ngược và tải lại không dùng đồng hồ thật: `vi.useFakeTimers()` hoặc hàm cấp thời điểm hiện tại được truyền vào (mục 7.5) | Lớp đếm nhận hàm cấp thời điểm hiện tại; không dùng `Thread.sleep` |
| Độc lập | Mỗi kiểm thử tự dựng dữ liệu; không phụ thuộc thứ tự chạy; không gọi mạng thật | Như web |
| Ngưỡng độ phủ | Không đặt ngưỡng phần trăm; tiêu chí đạt là có đủ kiểm thử của mục 9.3 và pass ở CI | Như web |

Ví dụ: web `it("UF-CM-03: ba yêu cầu cùng 401 → chỉ một lần gọi làm mới")`; Android `UFCM03_BaYeuCauCung401_ChiMotLanLamMoi`.

### 9.3 Danh mục kiểm thử bắt buộc

| Nhóm | Nội dung | Web | Android |
|---|---|---|---|
| Chuẩn hóa lỗi | `problem+json` có `maLoi`; `ValidationProblemDetails` có `errors`; lỗi 500 chỉ có `traceId`; thân không phải JSON; hết thời gian chờ; mất kết nối; hủy. Web thêm thân lỗi dạng `Blob` | `chuanHoaLoi` | `ApiErrorParser` |
| Nhóm lỗi | Mọi `maLoi` của `openapi.yaml` thuộc đúng một nhóm; mã lạ rơi vào "Nghiệp vụ theo màn hình" (mục 7.4) | `nhomLoi` | `NhomLoi` |
| Mã lạ | Mã trạng thái không có trong enum đã sinh không gây lỗi và hiển thị nguyên mã; `maLoi` lạ xử lý theo FA 3.4 | `shared/labels` | Giải mã JSON với mã lạ (ĐX-FC-21) và nhãn |
| Phiên | Nhiều yêu cầu cùng 401 chỉ một lần làm mới; token đã được làm mới thành công thì không làm mới lại; 200 lưu cặp token rồi replay một lần. Mọi refresh failure (401/429/mạng/timeout/500) xóa phiên/cache, về đăng nhập, kết thúc mọi waiter, không replay; 429 vẫn báo chờ với status/maLoi/traceId được bảo toàn | Mô-đun `session` và interceptor, gồm khôi phục sau mount | `TokenAuthenticator` dùng `MockWebServer`, gồm nhánh khởi động |
| Lưu phiên — JVM | Logic đóng gói IV/bản mã, serialization, lỗi đọc/giải mã dẫn tới không có phiên; boundary mã hóa giả, không chứng minh khóa hoặc mã hóa của OS | Không áp dụng | Logic `TokenStore` với boundary giả; Keystore thật kiểm tra riêng ở mục 9.4 |
| Định dạng | Các ví dụ ở mục 7.2, gồm qua nửa đêm, giờ quầy và tiền có khoảng trắng không ngắt | `shared/format` | `common/format` |
| Nhãn trạng thái | Mọi mã ở Phụ lục A của `User-Flows.md` có nhãn; mã lạ trả nguyên mã | `shared/labels` | `common/nhan` |
| Lỗi theo trường | Chuyển khóa `errors` (`dsLoaiVaSoLuong[0].soLuong`) thành đích đúng; khóa lạ trả về để hiển thị ở đầu biểu mẫu | `ganLoiMayChu` | `LoiTruong` |
| Ảnh | Từ chối sai định dạng; từ chối quá 2 MB sau nén; chặn quá 5 ảnh; nén cạnh dài 1280 px; tải tuần tự | `shared/anh` | Không áp dụng |
| Thành phần | `CodeInput`: Enter xử lý, tiêu điểm vào màn hình và sau mỗi lần xử lý, giữ giá trị và chọn toàn bộ chữ khi lỗi. `ConfirmDialog`: Esc đóng, Enter không xác nhận, tiêu điểm ban đầu ở **Hủy**, vô hiệu và không đóng bằng Esc khi đang xử lý. `AnhUploader`: trạng thái từng ảnh, chặn gửi khi ảnh đang tải hoặc lỗi | Có | Không áp dụng |
| Chống gửi trùng | Bấm hai lần liên tiếp chỉ gửi một yêu cầu | Có | Có (`ViewModel`) |
| Hủy sau khi phát write | Giả lập server commit rồi client hủy/mất theo dõi khi rời màn hình: không báo rollback/thành công/thất bại, không POST lại ngay; yêu cầu kiểm tra bằng API đọc hiện hữu. Hủy truy vấn tìm kiếm vẫn im lặng | Có | Có (`ViewModel`/`Repository`) |
| Mutation offline | Offline trước khi bấm không giữ write chờ reconnect; nối mạng lại không tự phát write, chỉ chủ động gửi lại | `QueryClient`/mutation | Không áp dụng |
| Dọn mật khẩu | Form còn mounted sau thành công/lỗi không còn password trong control/current mutation variables; giữ username và thông báo lỗi cần thiết, không giữ payload nhạy cảm trong lỗi | Hook và biểu mẫu có mật khẩu | Dọn control/tham chiếu request theo FA 3.5 |
| Nạp lại khi mở màn hình | Quay lại từ ngăn xếp tải lại từ máy chủ; xoay màn hình không tải lại (mục 6.2) | Không áp dụng | `ViewModel` với cờ `canTaiLai` |
| Bảo vệ tuyến | Sai vai trò về trang chủ đúng vai trò; thiếu ngữ cảnh luồng về bước đầu; sau khi lập phiếu mượn thì bước 1 đến 3 chuyển về bước 4 (FA 4.15) | Có | Không áp dụng |
| Đếm ngược | Về 0 thì tải lại; tổng tối đa 6 lần cách 10 giây rồi dừng (mục 7.5) | Chỉ khi có hook web (mục 7.5) | Có |

### 9.4 Kiểm tra thủ công trước mỗi mốc bàn giao

Các tiêu chí không tự động hóa được của DS mục 12 và kiểm tra Android Keystore thật, thực hiện trước mỗi mốc bàn giao và ghi kết quả vào mô tả pull request hoặc biên bản kiểm thử:

| Tiêu chí | Nội dung kiểm tra |
|---|---|
| TC-02, TC-04 | Đo tương phản trên giao diện thực; xem ở thang xám |
| TC-05, TC-06 | Android ở 320, 360, 412 dp và cỡ chữ 1,0, 1,3; vùng chạm tối thiểu 48 dp |
| TC-07, TC-08 | Web ở 1024 px và rộng hơn: trang không cuộn ngang; thao tác hoàn toàn bằng bàn phím |
| TC-10 | Từng vùng dữ liệu ở mạng chậm, mất mạng, lỗi 500; dịch vụ "ngủ dậy" |
| TC-14, TC-15 | Chữ có dấu hiển thị đúng; bật giảm chuyển động của hệ điều hành |
| Lưu phiên Android — Keystore thật | Trên thiết bị/emulator: tạo khóa `AndroidKeyStore`, mã hóa/giải mã khớp, đóng/mở lại app đọc được phiên; thử khóa mất/bản mã hỏng phải dọn phiên/về đăng nhập; xác minh cấu hình loại tệp phiên khỏi sao lưu đám mây/chuyển thiết bị. Ghi tên thiết bị/emulator, OS/API, build, từng bước, kết quả và ngày kiểm tra; chưa thực hiện thì ghi chưa chạy, không suy đạt từ test JVM |

---

## 10. Git, cấu hình và CI

### 10.1 Git **[ĐX-FC-14]**

Nhóm đã duyệt quy ước Git ở Backend-Conventions 18.3 (ĐX-CV-03); frontend dùng lại cùng nguyên tắc, đổi phạm vi theo frontend.

| Nội dung | Quy tắc |
|---|---|
| Nhánh chính | `main`; không commit trực tiếp |
| Tên nhánh | Tách từ `main`, đặt `{loai}/{ma-luong}-{mo-ta-ngan}`; `loai` thuộc `feature`, `fix`, `refactor`, `test`, `docs`, `chore`; mã luồng viết thường (`uf-nv-02`) hoặc `chung` cho việc không thuộc luồng nào. Ví dụ `feature/uf-nv-02-giao-thiet-bi` |
| Commit | Theo Conventional Commits: `{loai}({pham-vi}): {mô tả}`; `loai` gồm `feat`, `fix`, `refactor`, `test`, `docs`, `chore`; `pham-vi` là `web`, `android` hoặc tên tính năng viết thường; mô tả bằng tiếng Việt. Ví dụ `feat(giao-thiet-bi): hiển thị lỗi theo trường khi lập phiếu mượn` |
| Pull request | Một PR một mục tiêu; tiêu đề kèm mã `UF-` hoặc `SCR-`; mô tả nêu quy tắc đã cài và kết quả mục 9.4 nếu có; CI xanh; ít nhất một người review; hoàn tất bằng squash merge |
| Bí mật | Không commit `.env*` thật, `local.properties`, keystore, token |

### 10.2 Cấu hình và tệp phiên bản

| Nội dung | Web | Android |
|---|---|---|
| Biến cấu hình | `NEXT_PUBLIC_API_BASE_URL` theo môi trường Vercel (FA 4.14); `.env.example` được commit, `.env*.local` không | `API_BASE_URL` qua `buildConfigField` theo `buildType` (FA 5.8) |
| Tệp khóa | Tệp khóa của trình quản lý gói được commit | Phiên bản Gradle và thư viện khai báo tập trung, khóa phiên bản |
| Mã sinh từ API | `src/shared/api/schema.d.ts` do openapi-typescript sinh và **được commit** (FA 3.2) | Mã client sinh lúc build, **không commit** (FA 5.8) |
| Bản `openapi.yaml` | `openapi/openapi.yaml` ở thư mục gốc dự án web; lệnh `gen:api` sinh `schema.d.ts` **[ĐX-FC-17]** | `app/openapi/openapi.yaml` **[ĐX-FC-17]** |
| Cập nhật hợp đồng | Chép bản `openapi.yaml` mới từ backend, sinh lại, sửa mã cho đến khi biên dịch và kiểm thử đạt, rồi commit chung một pull request | Như web |

### 10.3 Lệnh và CI **[ĐX-FC-16]**

Tên script (web, `package.json`) độc lập với trình quản lý gói: `gen:api`, `lint`, `format:check`, `typecheck`, `test`, `build`.

| Bước | Web (thứ tự theo FA 4.15) | Android |
|---|---|---|
| 1 | Sinh lại `schema.d.ts` từ `openapi.yaml` và so sánh với bản đã commit, lệch thì thất bại | `spotlessCheck` |
| 2 | `lint` và `format:check` | `lint` (Android Lint) |
| 3 | `typecheck` | `testDebugUnitTest` |
| 4 | `test` | `assembleDebug` (client sinh chạy trước biên dịch) |
| 5 | `build` | Các kiểm tra `grep` bên dưới |
| Kiểm tra bổ sung | Tên tệp bị cấm dưới `src/`: `route.ts`, `middleware.ts`, `proxy.ts`; lớp có giá trị tùy ý dạng `[#…]`, `[…px]`, `[…rem]` trong `*.tsx` | Mã màu `#…` và `px` trong `res/layout`, `res/navigation`, mã Java; tham chiếu `@color/ds_color_{gray,blue,green,amber,red}_…` ngoài `ds_colors.xml`; `import` từ gói `ui` trong gói `data` hoặc `common` |

`testDebugUnitTest` chỉ kiểm chứng logic trên JVM, không kiểm chứng Android Keystore thật. Kết quả thiết bị/emulator ở mục 9.4 là bằng chứng riêng cần có trước bàn giao; không thêm instrumentation library để thay đổi phạm vi công cụ đã chọn.

Kiểm tra `grep` thất bại khi có kết quả khớp; kết quả khớp hợp lệ phải được sửa hoặc đưa vào danh sách ngoại lệ có chú thích trong chính tập lệnh. CI Android dùng GitHub Actions như web và backend (FA 4.15).

---

## 11. Danh sách kiểm tra khi review

Người review đối chiếu từng mục trước khi duyệt; mục kiểm tự động chỉ cần xác nhận CI xanh.

| # | Nội dung kiểm tra | Mục |
|---|---|---|
| 1 | Tên định danh, tệp, thư mục đúng quy ước; thuật ngữ nghiệp vụ không bị dịch | 3 |
| 2 | Thành phần giao diện dùng đúng tên ở Phụ lục A; không thành phần ngoài danh mục CMP | 3.4, 8.5 |
| 3 | Đúng vị trí tệp; không vi phạm biên giới import; không thư mục `utils` | 4 |
| 4 | Không có logic nghiệp vụ hay phép tính phí, hạn, trạng thái ở client | 2 |
| 5 | Gọi API chỉ qua `api.ts` hoặc `Repository`; tên hàm trùng `operationId` | 5.4, 6.4 |
| 6 | Lỗi xử lý theo `maLoi` và nhóm; không đọc `detail` để phân nhánh; không nuốt lỗi | 5.5, 7.4 |
| 7 | Thao tác ghi: chống gửi trùng đồng bộ, không tự gửi lại, xử lý "chưa biết kết quả", không cập nhật lạc quan | 5.4, 6.3 |
| 8 | Mỗi vùng dữ liệu đủ trạng thái đang tải, tải chậm, rỗng, lỗi; giữ dữ liệu cũ khi tải lại | 5.2, 6.3 |
| 9 | Biểu mẫu: tên trường trùng JSON, lỗi máy chủ gắn đúng trường, giữ dữ liệu khi lỗi | 5.6 |
| 10 | Không giá trị thô (màu, px, chữ cứng); chỉ token và `strings.xml` | 5.7, 6.6, 8 |
| 11 | Nhãn trạng thái chỉ qua `StatusBadge`; mã lạ hiển thị nguyên mã | 7.3 |
| 12 | Ngày giờ, tiền định dạng bằng hàm chung; không tạo `Date` từ chuỗi ngày thuần | 7.2 |
| 13 | Hằng số lấy từ nơi định nghĩa duy nhất; không số cố định rải rác | 7.1 |
| 14 | Tiếp cận: nhãn ô nhập, tên truy cập, vòng tiêu điểm, vùng chạm 48 dp, `lang="vi"`, `role="alert"` | 5.8, 6.6 |
| 15 | Hộp xác nhận: không `<form>`, `type="button"`, tiêu điểm ban đầu ở **Hủy**, không đóng khi bấm lớp che | 5.8, 6.6 |
| 16 | Hiệu ứng có dọn dẹp; `ViewBinding` đặt `null` ở `onDestroyView`; chỉ hủy im lặng Call đọc/kiểm tra ở `onCleared`, write đã phát giữ theo dõi hoặc đi nhánh chưa biết kết quả | 5.3, 6.2, 6.3 |
| 17 | Không ghi token, mật khẩu, URL ảnh ký vào nhật ký, URL, thông điệp lỗi; không `console.log`, `Log` ngoài debug | 2, 5.11, 6.8 |
| 18 | Ảnh: kiểm tra, nén, tải tuần tự, thu hồi URL tạm, tải lại URL hết hạn đúng một lần | 5.9 |
| 19 | Kiểm thử đủ theo mục 9.3 cho phần thay đổi; tên kiểm thử có mã truy vết | 9 |
| 20 | Chú thích tiếng Việt có dấu, nêu vì sao và mã truy vết; không mã chết, không `TODO` bỏ lại | 2 |
| 21 | Không thêm thư viện, thành phần, màn hình hay chức năng ngoài tài liệu nguồn | 1.4, 2 |
| 22 | Nhánh, commit, pull request đúng quy ước; CI xanh | 10 |

---

## 12. Danh mục ĐX-FC

Các điểm mà tài liệu nguồn không nêu hoặc giao cho Frontend-Conventions. Không có điểm nào thêm chức năng, màn hình hay quy tắc nghiệp vụ. Trạng thái của mọi mục: **đề xuất, chờ nhóm duyệt**.

| Mã | Quyết định | Căn cứ |
|---|---|---|
| ĐX-FC-01 | Tên trong mã của thành phần giao diện: giữ tên đã có ở `Frontend-Architecture.md` (`ConfirmDialog`, `CodeInput`, `AnhUploader`, `AnhGallery`, `ErrorBanner`, `LoadingBlock`, `EmptyState`, `FormField`, `ErrorView`, `LoadingView`, `EmptyView`, `XacNhanDialogFragment`), dùng tên kỹ thuật của DS 8.1 cho phần còn lại, đổi `Link` thành `TextLink` ở web; danh sách thành phần dùng chung mở rộng thành toàn bộ CMP (Phụ lục A); lỗi tải một vùng ở web dùng `StateView` thay cho `ErrorBanner` ở FA 4.12 | DS 3.2 giao tên lớp, tệp cho Frontend-Conventions; FA 4.2, 5.2 đã đặt tên; FA 2.5 giới hạn danh sách vì chưa có Design-System; DS 9.4 và CMP-25 quy định `StateView` cho lỗi tải |
| ĐX-FC-02 | Hàm API web và phương thức `Repository` Android trùng `operationId`; thể hiện Axios chung tên `apiClient`; `shared/api/schema.d.ts` là tệp kiểu sinh; `shared/api/types.ts` đặt bí danh kiểu và xuất lại `operations` | API-Notes 13 (giữ đúng `operationId`); FA 3.2, 4.6 |
| ĐX-FC-03 | Nơi đặt hằng số và hàm dùng chung: mỗi hằng số một nơi (mục 7.1), lớp `HangSo` ở `common/` Android, `ganLoiMayChu` ở `shared/hooks/`, khóa `refreshToken` | FA 3.1, 4.2, 5.2 không quy định chi tiết |
| ĐX-FC-04 | Phông web nạp bằng `next/font/google` tự lưu trữ lúc build, tập ký tự `vietnamese` và `latin`, Roboto 400, 500, 700 và Roboto Mono 400 | DS ĐX-DS-04 giao cách nạp cho kiến trúc; FA chưa quyết định; ĐX-FA-21 |
| ĐX-FC-05 | Biểu tượng web là component SVG nhúng ở `shared/components/icons/`; Android là vector drawable `ic_{ten}`; không thư viện biểu tượng | DS ĐX-DS-05 giao cách nạp cho kiến trúc; FA 2.5 |
| ĐX-FC-06 | Tệp token: web `src/app/tokens.css` và `globals.css`; Android `ds_colors.xml`, `ds_dimens.xml`, `ds_type.xml` | DS 3.1, 3.2, Phụ lục C |
| ĐX-FC-07 | Tailwind: theme thay thế bằng token, cấm giá trị tùy ý ngoài `var(--ds-…)`, hàm `classNames` nội bộ thay thư viện nối lớp | DS ĐX-DS-02, TC-01; FA 2.5 (không thư viện ngoài danh sách) |
| ĐX-FC-08 | Công cụ định dạng và lint: Prettier với cấu hình mặc định và EditorConfig (web), Spotless với google-java-format AOSP (Android), Android Lint đặt các kiểm tra tiếp cận và chữ cứng ở mức `error` | Backend-Conventions 19.1 (định dạng kiểm tra ở CI); DS 10 |
| ĐX-FC-09 | `tsconfig` `strict` và `noFallthroughCasesInSwitch`; bộ quy tắc ESLint ở mục 5.1 và 5.11 | FA 4.2 (ESLint đã chọn), FA 4.13 |
| ĐX-FC-10 | Biên giới import theo cách hiểu chặt FA 4.2: không import giữa hai thư mục tính năng bất kỳ dưới `features/`; phần dùng chung đặt ở `shared/` (`tienToKhoa.ts`, `useAuth`, kiểu `TrangThaiPhien`); Android `common` chỉ phụ thuộc `data.model` | FA 4.2 cấm import lẫn nhau giữa "nhóm" nhưng dùng từ này cho nhiều cấp; FA 4.4 cần `dsPhong` và FA 4.7 cần khóa của tính năng khác |
| ĐX-FC-11 | Thông điệp lỗi dùng chung ở `shared/labels/thongDiepLoi.ts` (web) và `strings.xml` (Android); thông điệp riêng màn hình ở `thongDiep.ts` của tính năng | `UI-Screens.md` 3.2; DS 9.4 |
| ĐX-FC-12 | Cách định dạng thời gian và tiền (`formatToParts`, `hourCycle: "h23"`, không tạo `Date` từ `yyyy-MM-dd`, nhóm hàng nghìn không phụ thuộc locale, khoảng trắng không ngắt trước `VNĐ`, giờ quầy `HH:mm`) và ví dụ kiểm thử | DS 9.7; FA 3.7, ĐX-FA-10 |
| ĐX-FC-13 | Quy ước đặt tên kiểm thử và vị trí tệp kiểm thử | Backend-Conventions 17.3; FA 4.15, 5.10 |
| ĐX-FC-14 | Quy ước Git cho frontend dùng lại Backend-Conventions 18.3, đổi phạm vi theo frontend | ĐX-CV-03 |
| ĐX-FC-15 | Quy ước tên tài nguyên Android: tiền tố layout, `android:id`, khóa chuỗi, tên style và `TextAppearance` | DS Phụ lục C (tiền tố `ds_`); FA 5.2 |
| ĐX-FC-16 | Bước và thứ tự CI, tên script, các kiểm tra `grep` | FA 4.15 (CI web); KT 16.4 (GitHub Actions) |
| ĐX-FC-17 | Vị trí bản `openapi.yaml` trong mỗi dự án, lệnh sinh kiểu và tệp `.env.example` | FA 3.2, 4.14, 5.8 ("bản `openapi.yaml` hiện hành nằm trong dự án") |
| ĐX-FC-18 | Hộp thoại: `<dialog>` gốc, không `<form>`, nút `type="button"`, tiêu điểm ban đầu đặt bằng `ref` ở **Hủy**; khi đang xử lý thì Esc và **Hủy** không đóng hộp | DS CMP-26, ĐX-DS-09; FA 3.11, FA-08 (không giấu kết quả của thao tác ghi chưa biết kết quả) |
| ĐX-FC-19 | Android không dùng `Snackbar` hay `Toast`; lỗi ghi hiển thị bằng `BannerView` (CMP-24) | DS ĐX-DS-16 (không dùng thanh thông báo nổi); FA 5.9 nêu `Snackbar` hoặc vùng cố định "theo Design-System" |
| ĐX-FC-20 | Đếm ngược: tổng tối đa 6 lần tải lại tính từ lúc về 0 (lần 1 ngay khi về 0, lần 2 đến 6 cách 10 giây); khi về 0 hiển thị "Đang cập nhật" theo DS CMP-21 (FA 3.8 ghi "00:00 kèm đang cập nhật"); chỉ viết hook web khi có màn hình web dùng bộ đếm | ĐX-FA-18 (không nói lần đầu có tính vào 6 không); DS CMP-21 và UI-Screens 5.7, 5.8 (bộ đếm chỉ ở màn hình SV) |
| ĐX-FC-21 | Bộ sinh client Android hoặc bộ chuyển đổi JSON phải giữ nguyên mã trạng thái lạ; cách cấu hình chọn khi thiết lập, tiêu chí đạt là kiểm thử "Mã lạ" | FA 5.5, 5.10 |
| ĐX-FC-22 | Android nạp lại khi mở màn hình hoặc quay lại từ ngăn xếp nhưng không nạp lại khi xoay màn hình, cài bằng cờ `canTaiLai` và `isChangingConfigurations()` | FA 5.6 nêu cả hai yêu cầu nhưng cùng một `onViewCreated` chạy trong cả hai trường hợp nên cần một quy tắc xác định |

---

## Phụ lục A. Ánh xạ thành phần Design-System sang tên trong mã

Tên theo mục 3.4 **[ĐX-FC-01]**. `–` là thành phần chỉ có ở nền tảng kia (`[Web]` hoặc `[Android]` theo DS 8.1). Android dùng thành phần Material Components kèm style `Widget.Ds.*` khi không có lớp riêng.

| CMP | Tên DS | Tên trong mã web (`shared/components/`) | Tên trong mã Android |
|---|---|---|---|
| CMP-01 | `Button` | `Button` | `MaterialButton` với `Widget.Ds.Button.{Primary,Secondary,Tertiary,Destructive}` |
| CMP-02 | `IconButton` | `IconButton` | `Widget.Ds.IconButton` |
| CMP-03 | `Link` | `TextLink` | `Widget.Ds.Link` |
| CMP-04 | `TextField` | `TextField` (dùng trong `FormField`) | `TextInputLayout` với `Widget.Ds.TextField` |
| CMP-05 | `SearchField` | – | `Widget.Ds.SearchField` |
| CMP-06 | `Select` | `Select` (dùng trong `FormField`) | `Widget.Ds.Select` |
| CMP-07 | `Checkbox`, `Radio` | `Checkbox`, `Radio` | `Widget.Ds.Checkbox`, `Widget.Ds.Radio` |
| CMP-08 | `SegmentedControl` | `SegmentedControl` | `Widget.Ds.SegmentedControl` |
| CMP-09 | `Tabs` | `Tabs` | – |
| CMP-10 | `QuantityStepper` | `QuantityStepper` | `QuantityStepperView` |
| CMP-11 | `ScanInput` | `CodeInput` | – |
| CMP-12 | `PhotoAttach` | `AnhUploader` | – |
| CMP-13 | `StatusBadge` | `StatusBadge` | `StatusBadgeView` |
| CMP-14 | `KeyValueList` | `KeyValueList` | `KeyValueListView` |
| CMP-15 | `Card` | `Card` | `MaterialCardView` với `Widget.Ds.Card` |
| CMP-16 | `ListRow` | – | `Widget.Ds.ListRow` áp dụng cho gốc của `item_{ten}.xml` |
| CMP-17 | `FilterChip` | – | `Chip` với `Widget.Ds.FilterChip` |
| CMP-18 | `DataTable` | `DataTable` | – |
| CMP-19 | `Pagination` | `Pagination` | – |
| CMP-20 | `Menu` | `Menu` | – |
| CMP-21 | `Countdown` | `Countdown` (chỉ khi có màn hình web dùng, mục 7.5) | `CountdownView` |
| CMP-22 | `ComparePanel` | `ComparePanel` | – |
| CMP-23 | `ImageGallery`, `ImageViewer` | `AnhGallery`, `ImageViewer` | `AnhGalleryView`, `ImageViewerDialogFragment` |
| CMP-24 | `Banner` | `Banner`; `ErrorBanner` là `Banner` nhận `ApiError` | `BannerView` |
| CMP-25 | `StateView` | `StateView`; `LoadingBlock` và `EmptyState` là dạng dựng sẵn của `StateView` | `LoadingView`, `EmptyView`, `ErrorView` |
| CMP-26 | `Dialog` | `Dialog`; `ConfirmDialog` là `Dialog` loại Xác nhận | `XacNhanDialogFragment` (loại Xác nhận), `DaDangKyChoDialogFragment` (loại Kết quả, `DLG-SV-01`) |
| CMP-27 | `BottomNav` | – | `BottomNavigationView` với `Widget.Ds.BottomNav` |
| CMP-28 | `TopBar` | – | `MaterialToolbar` với `Widget.Ds.TopBar` |
| CMP-29 | `Sidebar` | `Sidebar` | – |
| CMP-30 | `Header` | `Header` | – |
| CMP-31 | `Stepper` | `Stepper` | – |
