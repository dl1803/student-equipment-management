# DESIGN SYSTEM

## Hệ thống quản lý mượn trả thiết bị học tập cho sinh viên

| Mục | Nội dung |
|---|---|
| Dự án | SE100 – Nhóm 13 |
| Tài liệu | `Design-System.md` |
| Phiên bản | 1.0 |
| Ngày lập | 01/11/2026 |
| Tài liệu căn cứ | `User-Flows.md` v1.0 (30/10/2026); `UI-Screens.md` v1.0 (31/10/2026); `SRS.md` (22/10/2026: NFR-03, NFR-04, mục 6.3); `backend-architecture.md` (KT-04, KT-12, KT 3.4); `Backend-Conventions.md` (3.1, 3.3, 3.4); đề bài gốc |

## Mục lục

1. Giới thiệu
2. Nguyên tắc thiết kế
3. Kiến trúc token
4. Token nguyên thủy
5. Token ngữ nghĩa
6. Trạng thái tương tác chung
7. Nhãn trạng thái nghiệp vụ
8. Thành phần
9. Mẫu dùng chung
10. Khả năng tiếp cận và khả năng dùng
11. Kích thước màn hình và khung bố cục
12. Tiêu chí chấp nhận giao diện
- Phụ lục A. Bộ biểu tượng
- Phụ lục B. Các điểm được giao cho Design-System và nơi giải quyết
- Phụ lục C. Quy tắc ánh xạ token sang nền tảng

---

## 1. Giới thiệu

### 1.1 Mục đích và đối tượng sử dụng

Tài liệu định nghĩa **các quy tắc và thành phần thiết kế dùng chung** cho giao diện của hệ thống: màu sắc, kiểu chữ, khoảng cách, token, trạng thái tương tác, nhãn trạng thái nghiệp vụ, thành phần, mẫu giao diện lặp lại và yêu cầu tiếp cận. Mọi màn hình trong `UI-Screens.md` dựng từ các thành phần ở đây; màn hình không tự đặt màu, cỡ chữ hay kiểu thành phần riêng.

| Người đọc | Dùng tài liệu để |
|---|---|
| Thiết kế giao diện | Vẽ wireframe và giao diện bằng đúng token, thành phần và trạng thái; không phải tự chọn màu hay kích thước. |
| Lập trình frontend (Android, web) | Cài token và thành phần một lần, dùng lại cho mọi màn hình; biết mã trạng thái nào hiển thị bằng nhãn nào. |
| Kiểm thử | Lấy tiêu chí kiểm tra giao diện (tương phản, vùng chạm, nhãn trạng thái, trạng thái tải/rỗng/lỗi) từ mục 12. |

### 1.2 Ranh giới với các tài liệu khác

| Tài liệu | Quan hệ với tài liệu này |
|---|---|
| `User-Flows.md`, `UI-Screens.md` | Căn cứ trực tiếp. Mã màn hình `SCR-…`/`DLG-…`, nhãn trạng thái (Phụ lục A của `User-Flows.md`), hành vi và thông điệp mặc định được giữ nguyên. Tài liệu này không thêm màn hình, chức năng hay quy tắc nghiệp vụ. |
| `Wireframes.md` | Tài liệu này không quy định bố cục từng màn hình; chỉ quy định thành phần, khung bố cục chung (mục 11) và khoảng cách. Bố cục chi tiết do `Wireframes.md` quyết định, dùng các thành phần ở mục 8. |
| `Frontend-Architecture.md`, `Frontend-Conventions.md` | Các điểm kỹ thuật sau được mô tả trong hai tài liệu đó: khung (framework) web, thư viện thành phần, cách nạp phông, tổ chức tệp token, thời gian chờ tối đa của client, độ trễ gõ tìm kiếm (ĐX-UI-05), cách tiếp nhận kết quả quét mã. Tài liệu này chỉ nêu yêu cầu giao diện mà chúng phải đáp ứng; trạng thái các đề xuất kỹ thuật giữ theo tài liệu sở hữu. |
| `openapi.yaml`, `API-Notes.md` | Hợp đồng dữ liệu; tài liệu này không lặp lại. Chỉ dùng mã lỗi `maLoi` và mã trạng thái đã nêu ở `User-Flows.md` và `UI-Screens.md`. |
| Nguồn nghiệp vụ (`SRS.md`, `Usecase.md`, `BusinessRule.md`, `Authorization.md`) | Là căn cứ gián tiếp qua `User-Flows.md`. Không đặt quy tắc nghiệp vụ mới. |

### 1.3 Nền tảng và phạm vi áp dụng

| Nền tảng | Người dùng | Đơn vị | Ghi chú |
|---|---|---|---|
| Android (Java, ứng dụng native) | Sinh viên (SV) | `dp` cho kích thước, `sp` cho cỡ chữ | KT-04. Token ánh xạ sang tài nguyên Android (màu, kích thước, kiểu chữ); thư viện thành phần Android do Frontend-Architecture chọn. |
| Web (triển khai trên Vercel) | Nhân viên quản lý thiết bị (NV), quản trị viên (QT) | `px` cho kích thước; `rem` cho cỡ chữ (1 rem = 16 px) | KT-04, KT-12. Khung web được mô tả ở `Frontend-Architecture.md` 1.4; token vẫn là biến CSS thuần (CSS custom properties), không phụ thuộc khung. |

Phạm vi hiển thị: **tiếng Việt, chế độ sáng** (ĐX-DS-01). Nguồn không yêu cầu chế độ tối, đa ngôn ngữ hay bộ nhận diện thương hiệu (logo, màu thương hiệu). Không quy định khóa hướng màn hình; bố cục của SV co giãn theo chiều rộng.

### 1.4 Quy ước đọc

| Quy ước | Ý nghĩa |
|---|---|
| `CMP-nn` | Mã thành phần (mục 8). |
| `ĐX-DS-nn` | Quyết định của tài liệu này cho những điểm nguồn không nêu hoặc giao cho Design-System (mục 1.5). |
| `ĐX-UF-nn`, `ĐX-UI-nn` | Quyết định của `User-Flows.md` và `UI-Screens.md`, giữ nguyên nghĩa. |
| Tên token | Dạng chấm, ví dụ `color.blue.600`, `space.4`, `type.body`. Quy tắc ánh xạ sang biến CSS và tài nguyên Android ở Phụ lục C. |
| `[Android]`, `[Web]` | Quy định chỉ áp dụng cho một nền tảng. Không ghi nền tảng nghĩa là áp dụng cho cả hai. |
| "Dùng ở" | Danh sách mã màn hình trong `UI-Screens.md` dùng thành phần đó. |
| Tông | Một trong năm tông phản hồi: `info`, `success`, `warning`, `danger`, `neutral` (mục 5.2). |

### 1.5 Quyết định của tài liệu (ĐX-DS)

| Mã | Quyết định | Căn cứ |
|---|---|---|
| ĐX-DS-01 | Giao diện chỉ có tiếng Việt và chế độ sáng; không có logo hay màu thương hiệu. Khi nhà trường cung cấp nhận diện thương hiệu, chỉ thay lớp token nguyên thủy của màu xanh dương (mục 4.1); các lớp trên giữ nguyên. | `UI-Screens.md` 1.3 (giao diện tiếng Việt); nguồn không có yêu cầu chế độ tối hoặc nhận diện; nguyên tắc YAGNI |
| ĐX-DS-02 | Một bộ giá trị token dùng cho cả hai nền tảng (`dp`/`sp` và `px`/`rem` cùng số). Token chia ba lớp: nguyên thủy → ngữ nghĩa → thành phần. Màu và kiểu chữ trong thành phần và màn hình chỉ dùng token ngữ nghĩa hoặc token thành phần; các thang còn lại (khoảng cách, bo góc, viền, đổ bóng, chuyển động, kích thước, thứ tự lớp) dùng trực tiếp. Không dùng giá trị thô. | KT-04 (hai nền tảng, một API); yêu cầu nhất quán |
| ĐX-DS-03 | Bảng màu gồm xám trung tính, xanh dương (màu chủ đạo, cũng dùng cho tông `info`) và ba màu phản hồi (xanh lá, hổ phách, đỏ). Mọi cặp chữ/nền đạt tỷ lệ tương phản tối thiểu 4,5:1 và thành phần giao diện tối thiểu 3:1; số liệu kiểm chứng ở mục 5.4. | `UI-Screens.md` 3.8 (không chỉ dựa vào màu); không có nguồn màu |
| ĐX-DS-04 | Phông: `[Web]` Roboto (chữ), Roboto Mono (mã), nạp các tập ký tự có tiếng Việt; `[Android]` phông sans-serif mặc định của hệ thống và `monospace` cho mã. Không gói thêm phông vào ứng dụng Android. Mã (phiếu, thiết bị, SV, biên bản) dùng phông đơn cách để NV phân biệt ký tự khi nhập tay. | `UI-Screens.md` 1.3 (tiếng Việt, cần hiển thị đủ dấu); BR-13, BR-40 (nhập tay mã) |
| ĐX-DS-05 | Bộ biểu tượng: Material Symbols (kiểu Outlined), danh sách giới hạn ở Phụ lục A. Biểu tượng chỉ bổ trợ chữ, không thay chữ (trừ nút biểu tượng có tên truy cập). | `UI-Screens.md` 3.8; nguồn không chọn bộ biểu tượng |
| ĐX-DS-06 | Năm tông phản hồi và hai mức nhấn (nhạt, đậm); mã trạng thái nghiệp vụ ánh xạ sang tông ở mục 7. | `UI-Screens.md` 3.3 giao quy tắc phân biệt trạng thái cho Design-System; 3.8 |
| ĐX-DS-07 | Web: chiều rộng cửa sổ tối thiểu được hỗ trợ là 1024 px. Android: chiều rộng tham chiếu để kiểm tra là 320 dp, 360 dp, 412 dp; kiểm tra ở cỡ chữ hệ thống mặc định và mức lớn (hệ số 1,3). | `UI-Screens.md` 3.9 (kích thước web giao cho Design-System, nguồn không nêu); NFR-04 |
| ĐX-DS-08 | Android: vùng chạm tối thiểu 48 × 48 dp. Web: chiều cao điều khiển 32 px (dày đặc), 40 px (mặc định), 48 px (thao tác quầy). | `UI-Screens.md` 3.8 giao kích thước vùng chạm cho Design-System |
| ĐX-DS-09 | Hộp xác nhận: tiêu điểm ban đầu ở nút Hủy; phím Enter không tự xác nhận hành động; nút xác nhận ở bên phải nút Hủy; bấm vào lớp che không đóng hộp. | ĐX-UF-05; `UI-Screens.md` 3.8 (phím Enter dùng để nhập mã ở quầy) nên cần tránh xác nhận nhầm |
| ĐX-DS-10 | Thông điệp mặc định trong `UI-Screens.md` được giữ làm chữ chính thức. Tài liệu này chốt thêm bộ nhãn hành động chuẩn và danh sách chữ do Design-System đặt (mục 9.8) và các nhãn phát sinh ở mục 7.3. | `UI-Screens.md` 1.4 giao Wireframes và Design-System chốt chữ chính thức |
| ĐX-DS-11 | Hành động bị vô hiệu do điều kiện nghiệp vụ phải có lý do hiển thị ngay cạnh hoặc phía trên, không chỉ trong chú giải (tooltip). | `User-Flows.md` UF-NV-01 A1; `UI-Screens.md` SCR-NV-04, SCR-NV-05 (hiển thị lý do); màn hình cảm ứng không có thao tác di chuột |
| ĐX-DS-12 | Giá trị không có dữ liệu và không có chữ riêng ở `UI-Screens.md` hiển thị dấu gạch dài "—". Số lượng, tiền, thời gian dùng chữ số cùng độ rộng (tabular). | ĐX-UI-12 (định dạng); nhu cầu căn cột |
| ĐX-DS-13 | Bộ đếm ngược dạng `mm:ss` (dưới 1 giờ) hoặc `HH:mm:ss`; luôn kèm hạn tuyệt đối; tông `warning` cố định, không có ngưỡng đổi màu. | ĐX-UF-04, ĐX-UI-07; `User-Flows.md` 3.5 |
| ĐX-DS-14 | Hành động ở dòng bảng: tối đa 2 hành động hiển thị trực tiếp; từ 3 hành động trở lên gom vào Menu thao tác. | `UI-Screens.md` SCR-QT-03 (2 hành động), SCR-QT-04, SCR-QT-07 (3), SCR-QT-09 (đến 5) |
| ĐX-DS-15 | Chuyển động tối thiểu, chỉ để báo thay đổi trạng thái; tôn trọng cài đặt giảm chuyển động của hệ điều hành. | Nguyên tắc YAGNI; `User-Flows.md` 3.10 (NFR-03: không chèn bước trung gian không cần thiết) |
| ĐX-DS-16 | Cách trình bày lỗi và thông báo (mục 9.4): lỗi 422 và lỗi từ chối do quyền hoặc hệ thống dùng tông `danger`; lỗi 409 và lỗi dữ liệu đã đổi dùng `warning`; thông tin phiên dùng `info`; không dùng thanh thông báo nổi (toast). | `UI-Screens.md` 3.2 chỉ nêu cách hiển thị chung (cạnh trường, banner, hộp thoại) mà không nêu tông; NT-07 |

---

## 2. Nguyên tắc thiết kế

Các nguyên tắc suy ra từ yêu cầu của dự án; mỗi nguyên tắc có hệ quả cụ thể ở các mục sau.

| Mã | Nguyên tắc | Hệ quả trong Design-System | Căn cứ |
|---|---|---|---|
| NT-01 | **Máy chủ là nguồn sự thật của dữ liệu hiển thị.** Giao diện không tự tính phí, ngày trễ, hạn hay đổi trạng thái theo đồng hồ cục bộ. | Nhãn trạng thái chỉ sinh từ mã API (mục 7); bộ đếm ngược về 0 thì tải lại, không tự đổi nhãn (CMP-21); không có kiểu "dự đoán" hay "tạm tính" cho dữ liệu máy chủ, trừ ĐX-UI-10. | ĐX-UF-04; `UI-Screens.md` 3.4 |
| NT-02 | **Không chỉ dựa vào màu.** Trạng thái, lỗi và nhấn mạnh luôn kèm chữ. | Mỗi tông (trừ `neutral`) có biểu tượng riêng và luôn đi cùng nhãn chữ (CMP-13, CMP-24); nhấn mạnh dùng viền hoặc vạch hình dạng cộng chữ. | `UI-Screens.md` 3.8 |
| NT-03 | **Dùng được liên tục tại quầy.** NV quét hoặc nhập mã nhiều lần liên tiếp, bằng bàn phím và máy quét. | Ô nhập mã quầy có tiêu điểm và trạng thái riêng (CMP-11); điều khiển web đủ lớn (ĐX-DS-08); hộp xác nhận không bị Enter xác nhận nhầm (ĐX-DS-09). | `UI-Screens.md` 3.8, mục 6 |
| NT-04 | **Điện thoại là nền tảng chính của SV.** Không vỡ bố cục trên màn hình điện thoại phổ biến, kể cả khi người dùng tăng cỡ chữ hệ thống. | Chiều rộng tham chiếu và cỡ chữ ở ĐX-DS-07; đơn vị `sp`; chữ dài xuống dòng; vùng chạm 48 dp. | NFR-04; `UI-Screens.md` 3.9 |
| NT-05 | **Một trạng thái, một nhãn.** Mỗi loại trạng thái dùng một thành phần nhãn dùng chung, mã chưa biết hiển thị nguyên mã. | CMP-13 và mục 7. | `UI-Screens.md` 3.3 |
| NT-06 | **Thao tác khó hoàn tác phải nêu rõ hệ quả.** | Mẫu hộp xác nhận thống nhất (CMP-26, mục 9.2) và quy tắc chọn kiểu nút. | ĐX-UF-05; `UI-Screens.md` 3.5 |
| NT-07 | **Tối giản.** Không thêm thành phần, biến thể hay hiệu ứng mà màn hình trong giai đoạn 1 không dùng. | Danh mục thành phần chỉ gồm thứ được `UI-Screens.md` yêu cầu; không có chế độ tối, thanh thông báo nổi (toast), huy hiệu thông báo chưa đọc. | `User-Flows.md` 3.8; ĐX-DS-01, ĐX-DS-15 |

---

## 3. Kiến trúc token

### 3.1 Ba lớp token

| Lớp | Vai trò | Ví dụ | Ai được dùng |
|---|---|---|---|
| Nguyên thủy (primitive) | Giá trị thô của bảng màu, thang cỡ chữ, thang khoảng cách, bo góc, viền, kích thước | `color.blue.600` = `#1F5499`; `space.4` = 16 | **Màu và cỡ, độ đậm chữ**: chỉ lớp ngữ nghĩa và lớp thành phần tham chiếu. **Khoảng cách, bo góc, viền, đổ bóng, chuyển động, kích thước, thứ tự lớp**: thành phần và màn hình dùng trực tiếp vì tên token đã nêu mức độ |
| Ngữ nghĩa (semantic) | Gán mục đích sử dụng cho giá trị nguyên thủy | `color.action.primary.bg` → `color.blue.600`; `type.title-screen` | Thành phần và màn hình |
| Thành phần (component) | Giá trị riêng của một thành phần khi cần khác ngữ nghĩa chung | `size.button.height` | Thành phần tương ứng |

Quy tắc: không dùng giá trị thô (mã màu, số pixel) trong thành phần hay màn hình, và không dùng màu nguyên thủy trực tiếp (ví dụ `color.blue.600`) mà dùng token ngữ nghĩa (ví dụ `color.action.primary.bg`). Muốn đổi diện mạo thì đổi lớp nguyên thủy hoặc lớp ngữ nghĩa, không sửa từng màn hình (ĐX-DS-02).

### 3.2 Quy tắc đặt tên

| Quy tắc | Nội dung |
|---|---|
| Tên token | Tiếng Anh, chữ thường, các phần cách nhau bằng dấu chấm, theo thứ tự `nhóm.đối tượng.biến thể.thuộc tính`; ví dụ `color.tone.danger.bg`. Thành phần kỹ thuật dùng tiếng Anh theo `Backend-Conventions.md` 3.1. |
| Tên thành phần | Tên tiếng Việt hiển thị trong tài liệu kèm tên kỹ thuật tiếng Anh (ví dụ Nút, `Button`). Tên lớp, tệp và cách tổ chức mã do Frontend-Conventions quy định. |
| Thuật ngữ nghiệp vụ | Giữ nguyên tiếng Việt như nguồn (phiếu mượn, biên bản, khung phạt…); mã trạng thái giữ nguyên mã API. |
| Ánh xạ nền tảng | Phụ lục C. |

### 3.3 Danh mục nhóm token

| Nhóm | Nội dung | Mục |
|---|---|---|
| `color.*` | Màu nguyên thủy, màu theo vai trò, tông phản hồi | 4.1, 5.1, 5.2 |
| `font.*`, `type.*` | Phông, cỡ, dòng, độ đậm; vai trò kiểu chữ | 4.2, 5.3 |
| `space.*` | Thang khoảng cách và khoảng cách theo vai trò | 4.3, 5.5 |
| `radius.*`, `border.*`, `shadow.*` | Bo góc, viền, đổ bóng | 4.4 |
| `motion.*`, `z.*` | Thời lượng chuyển động, thứ tự lớp (web) | 4.5, 4.6 |
| `size.*`, `layout.*` | Kích thước điều khiển và khung bố cục | 4.7 |

---

## 4. Token nguyên thủy

### 4.1 Màu

Bảng màu gồm xám trung tính, xanh dương (màu chủ đạo) và ba màu phản hồi (xanh lá, hổ phách, đỏ). Chỉ giữ các bậc được dùng; số bậc không liên tục là có chủ đích (ĐX-DS-03).

**Xám trung tính**

| Token | Giá trị | Dùng cho (qua token ngữ nghĩa ở mục 5) |
|---|---|---|
| `color.gray.0` | `#FFFFFF` | Nền thẻ, ô nhập, hộp thoại, bảng; chữ trên nền đậm |
| `color.gray.50` | `#F6F7F9` | Nền trang; nền khi rê chuột; nền ô chỉ đọc; vùng mở rộng của dòng bảng |
| `color.gray.100` | `#EEF0F3` | Nền tiêu đề cột bảng; nền thành phần bị vô hiệu; nền nhãn trung tính |
| `color.gray.200` | `#E1E4E9` | Đường kẻ phân cách; viền thẻ và bảng |
| `color.gray.300` | `#CBD0D8` | Viền nhãn trung tính |
| `color.gray.400` | `#808998` | Viền điều khiển (ô nhập, ô chọn, nút phụ) |
| `color.gray.500` | `#6B7482` | Chữ gợi ý (placeholder), chữ vô hiệu; viền điều khiển khi rê chuột |
| `color.gray.600` | `#4F5764` | Chữ phụ; tiêu đề cột bảng; biểu tượng phụ; mục điều hướng chưa chọn |
| `color.gray.700` | `#3A414D` | Chữ nhãn trung tính; nền nhãn trung tính đậm |
| `color.gray.900` | `#161A21` | Chữ chính; nền lớp che (kèm độ trong suốt) |

**Xanh dương (chủ đạo; cũng dùng cho tông `info`)**

| Token | Giá trị | Dùng cho |
|---|---|---|
| `color.blue.50` | `#EEF4FB` | Nền mục được chọn; nền tông `info` |
| `color.blue.100` | `#D6E4F5` | Viền tông `info`; nền khi nhấn nút văn bản; nền chỉ báo mục điều hướng đang chọn |
| `color.blue.500` | `#2F69B0` | Vòng tiêu điểm bàn phím |
| `color.blue.600` | `#1F5499` | Nền nút chính; biểu tượng và nền đậm của tông `info`; viền mục được chọn |
| `color.blue.700` | `#184380` | Chữ liên kết và nút văn bản; chữ tông `info`; nền nút chính khi rê chuột |
| `color.blue.800` | `#133466` | Nền nút chính khi nhấn |

**Xanh lá, hổ phách, đỏ**

| Token | Giá trị | Token | Giá trị | Token | Giá trị |
|---|---|---|---|---|---|
| `color.green.50` | `#E7F5EC` | `color.amber.50` | `#FFF4DB` | `color.red.50` | `#FDECEC` |
| `color.green.100` | `#CDEBD8` | `color.amber.100` | `#FFE5A8` | `color.red.100` | `#F9D0D0` |
| `color.green.600` | `#1B7A43` | `color.amber.600` | `#B45F06` | `color.red.600` | `#C62F2F` |
| `color.green.700` | `#146236` | `color.amber.700` | `#8A4A00` | `color.red.700` | `#A12020` |
| – | – | – | – | `color.red.800` | `#7F1818` |

Bậc 50: nền nhạt; bậc 100: viền nhạt; bậc 600: nền đậm và biểu tượng; bậc 700: chữ trên nền nhạt và trạng thái rê chuột; `color.red.800`: nút hủy bỏ khi nhấn.

### 4.2 Kiểu chữ

| Token | Giá trị | Ghi chú |
|---|---|---|
| `font.family.sans` | `[Web]` `"Roboto", system-ui, "Segoe UI", Arial, sans-serif`; `[Android]` phông sans-serif mặc định của hệ thống | ĐX-DS-04. Phông dự phòng bắt buộc có tiếng Việt để chữ có dấu không vỡ khi phông chưa nạp được |
| `font.family.mono` | `[Web]` `"Roboto Mono", ui-monospace, Consolas, monospace`; `[Android]` `monospace` | Chỉ cho mã (phiếu, thiết bị, SV, biên bản) và `traceId` |
| `font.weight.regular` | 400 | |
| `font.weight.medium` | 500 | |
| `font.weight.bold` | 700 | |
| `font.size.12` … `font.size.28` | 12, 14, 16, 20, 24, 28 | `[Web]` px, chia 16 ra rem; `[Android]` sp |
| `font.leading.16` … `font.leading.36` | 16, 20, 24, 28, 32, 36 | Chiều cao dòng, cùng đơn vị với cỡ chữ |

Yêu cầu: `[Web]` cần nạp Roboto độ đậm 400, 500, 700 và Roboto Mono độ đậm 400, gồm tập ký tự có tiếng Việt; cách nạp (tự lưu trữ hay dịch vụ ngoài) do Frontend-Architecture quyết định. Chữ số trong bảng, số lượng, tiền, thời gian dùng chữ số cùng độ rộng: `[Web]` `font-variant-numeric: tabular-nums`; `[Android]` `fontFeatureSettings="tnum"` (ĐX-DS-12).

### 4.3 Khoảng cách

Thang cơ sở 4; chỉ số = số bội của 4.

| Token | Giá trị |
|---|---|
| `space.1` | 4 |
| `space.2` | 8 |
| `space.3` | 12 |
| `space.4` | 16 |
| `space.6` | 24 |
| `space.12` | 48 |

Đơn vị: `[Web]` px, `[Android]` dp. Mọi khoảng cách, đệm (padding) và lề (margin) lấy từ thang này; không dùng giá trị ngoài thang.

### 4.4 Bo góc, viền, đổ bóng

| Token | Giá trị | Dùng cho |
|---|---|---|
| `radius.sm` | 4 | Ô chọn (checkbox), vạch nhấn |
| `radius.md` | 8 | Nút, ô nhập, thẻ, bảng, ô đính ảnh |
| `radius.lg` | 12 | Hộp thoại, khối thông báo, lớp xem ảnh |
| `radius.full` | 9999 | Nhãn trạng thái, chip, nút chọn (radio) |
| `border.width.default` | 1 | Viền thẻ, ô nhập, đường kẻ |
| `border.width.strong` | 2 | Vòng tiêu điểm; viền mục được chọn; viền ô nhập khi tiêu điểm |
| `border.width.accent` | 4 | Vạch nhấn ở đầu dòng được làm nổi bật và mục điều hướng web đang chọn |
| `border.focus-offset` | 2 | Khoảng cách giữa vòng tiêu điểm và mép thành phần `[Web]` |
| `shadow.menu` | `0 4px 12px rgba(22, 26, 33, 0.12)`; `[Android]` độ nổi 4 dp | Menu thả xuống, danh sách chọn |
| `shadow.dialog` | `0 8px 24px rgba(22, 26, 33, 0.16)`; `[Android]` độ nổi 8 dp | Hộp thoại, lớp xem ảnh |

Thẻ, bảng và thanh điều hướng không dùng đổ bóng; chúng phân tách bằng viền `border.width.default` màu `color.border.subtle`.

### 4.5 Chuyển động

| Token | Giá trị | Dùng cho |
|---|---|---|
| `motion.duration.fast` | 100 ms | Đổi màu khi rê chuột hoặc nhấn |
| `motion.duration.base` | 200 ms | Hộp thoại, menu và khối thông báo xuất hiện hoặc biến mất (mờ dần) |
| `motion.easing.standard` | `cubic-bezier(0.2, 0, 0, 1)` | Mọi chuyển động |

Không có chuyển động nào khác (ĐX-DS-15). Khi hệ điều hành bật giảm chuyển động (`[Web]` `prefers-reduced-motion: reduce`; `[Android]` thang thời lượng hoạt họa bằng 0), mọi thời lượng về 0. Chỉ báo đang tải (CMP-25) là chỉ báo chức năng nên vẫn quay.

### 4.6 Thứ tự lớp `[Web]`

| Token | Giá trị | Dùng cho |
|---|---|---|
| `z.sticky` | 100 | Thanh đầu trang, tiêu đề cột bảng cố định |
| `z.dropdown` | 200 | Menu, danh sách chọn |
| `z.overlay` | 300 | Lớp che sau hộp thoại |
| `z.modal` | 310 | Hộp thoại |
| `z.viewer` | 400 | Lớp xem ảnh lớn |

### 4.7 Kích thước điều khiển và khung bố cục

**`[Web]`**

| Token | Giá trị | Dùng cho |
|---|---|---|
| `size.control.sm` | 32 | Nút và ô chọn trong dòng bảng, bộ lọc dày đặc |
| `size.control.md` | 40 | Mặc định cho nút, ô nhập, ô chọn |
| `size.control.lg` | 48 | Ô nhập mã quầy và nút thao tác chính ở quầy (ĐX-DS-08) |
| `size.table.row` | 48 | Chiều cao tối thiểu dòng bảng |
| `size.table.row-sub` | 40 | Chiều cao tối thiểu dòng con khi mở rộng |
| `size.photo.tile` | 96 | Ô ảnh thu nhỏ trong khu đính ảnh và đối chiếu |
| `size.dialog.sm` | 480 | Chiều rộng hộp xác nhận mặc định |
| `size.dialog.md` | 640 | Chiều rộng hộp thoại nhiều nội dung hoặc có biểu mẫu |
| `size.keyvalue.label` | 200 | Chiều rộng cột tên trường của danh sách thông tin (CMP-14) |

**`[Android]`**

| Token | Giá trị | Dùng cho |
|---|---|---|
| `size.touch.min` | 48 | Vùng chạm tối thiểu của mọi điều khiển (ĐX-DS-08), kể cả khi phần nhìn thấy nhỏ hơn |
| `size.button.height` | 48 | Chiều cao nút |
| `size.input.height` | 56 | Chiều cao ô nhập, ô chọn |
| `size.chip.height` | 32 | Chiều cao chip (vùng chạm vẫn 48) |
| `size.list-row.min` | 56 | Chiều cao tối thiểu dòng danh sách |
| `size.appbar.height` | 56 | Thanh tiêu đề |
| `size.bottomnav.height` | 56 | Thanh điều hướng dưới |
| `size.photo.thumb` | 88 | Ô ảnh thu nhỏ trong dải ảnh và chi tiết phiếu |
| `size.dialog.max` | 560 | Chiều rộng tối đa của hộp thoại |

**Chung**

| Token | Giá trị | Dùng cho |
|---|---|---|
| `size.icon.sm` | 16 | Biểu tượng trong nhãn trạng thái, thông báo lỗi dưới ô nhập |
| `size.icon.md` | 20 | Biểu tượng trong nút, ô nhập, khối thông báo |
| `size.icon.lg` | 24 | Biểu tượng điều hướng, nút biểu tượng |
| `size.badge.height` | 24 | Chiều cao nhãn trạng thái |
| `size.state.max` | 480 | Chiều rộng tối đa của khối trạng thái (CMP-25) |

**Khung bố cục:** xem mục 11.

---

## 5. Token ngữ nghĩa

### 5.1 Màu theo vai trò

| Nhóm | Token | Tham chiếu | Dùng cho |
|---|---|---|---|
| Chữ | `color.text.primary` | `color.gray.900` | Nội dung chính |
| | `color.text.secondary` | `color.gray.600` | Chú thích, chữ phụ, nhãn trường chỉ đọc |
| | `color.text.placeholder` | `color.gray.500` | Chữ gợi ý; chỉ dùng trên nền `color.surface.default` |
| | `color.text.disabled` | `color.gray.500` | Chữ của thành phần bị vô hiệu |
| | `color.text.inverse` | `color.gray.0` | Chữ trên nền đậm |
| | `color.text.link` | `color.blue.700` | Liên kết, nút văn bản |
| | `color.text.link-hover` | `color.blue.800` | `[Web]` liên kết khi rê chuột |
| Nền | `color.surface.default` | `color.gray.0` | Thẻ, ô nhập, bảng, hộp thoại, thanh điều hướng |
| | `color.surface.subtle` | `color.gray.50` | Nền trang; rê chuột; ô chỉ đọc; vùng mở rộng; cột "lúc giao" ở khối đối chiếu |
| | `color.surface.muted` | `color.gray.100` | Tiêu đề cột bảng; nền thành phần bị vô hiệu |
| | `color.surface.selected` | `color.blue.50` | Dòng, thẻ, mục được chọn |
| | `color.overlay.scrim` | `color.gray.900` độ mờ 50% | Lớp che sau hộp thoại và lớp xem ảnh |
| | `color.overlay.pressed` | `color.gray.900` độ mờ 12% | `[Android]` hiệu ứng gợn khi chạm |
| Viền | `color.border.subtle` | `color.gray.200` | Đường kẻ, viền thẻ, viền bảng |
| | `color.border.control` | `color.gray.400` | Viền ô nhập, ô chọn, nút phụ |
| | `color.border.control-hover` | `color.gray.500` | `[Web]` viền điều khiển khi rê chuột |
| | `color.border.selected` | `color.blue.600` | Viền thẻ hoặc mục được chọn; vạch nhấn mục điều hướng |
| | `color.border.error` | `color.red.600` | Viền ô nhập có lỗi |
| Tiêu điểm | `color.focus.ring` | `color.blue.500` | Vòng tiêu điểm bàn phím |
| Điều hướng | `color.nav.indicator` | `color.blue.100` | Nền chỉ báo mục đang chọn ở thanh điều hướng dưới `[Android]` |
| Tiến trình | `color.progress.indicator` | `color.blue.600` | Cung quay của chỉ báo đang tải |
| | `color.progress.track` | `color.gray.200` | Vạch nền của chỉ báo đang tải |
| Nút chính | `color.action.primary.bg` / `-hover` / `-pressed` | `color.blue.600` / `700` / `800` | Nền nút chính |
| | `color.action.primary.text` | `color.gray.0` | Chữ nút chính |
| Nút hủy bỏ | `color.action.destructive.bg` / `-hover` / `-pressed` | `color.red.600` / `700` / `800` | Nền nút hủy bỏ |
| | `color.action.destructive.text` | `color.gray.0` | Chữ nút hủy bỏ |
| Nút phụ | `color.action.secondary.bg` / `-hover` / `-pressed` | `color.gray.0` / `50` / `100` | Nền nút phụ |
| | `color.action.secondary.border` | `color.border.control` | Viền nút phụ |
| | `color.action.secondary.text` | `color.text.primary` | Chữ nút phụ |
| Nút văn bản | `color.action.tertiary.text` | `color.text.link` | Chữ nút văn bản |
| | `color.action.tertiary.bg-hover` / `-pressed` | `color.blue.50` / `100` | Nền khi rê chuột / khi nhấn |
| Vô hiệu | `color.action.disabled.bg` | `color.gray.100` | Nền nút, ô nhập bị vô hiệu |
| | `color.action.disabled.text` | `color.gray.500` | Chữ bị vô hiệu |
| | `color.action.disabled.border` | `color.gray.200` | Viền bị vô hiệu |

### 5.2 Tông phản hồi

Năm tông truyền đạt ý nghĩa của trạng thái, thông báo và nhấn mạnh (ĐX-DS-06). Mỗi tông có sáu token `color.tone.<tông>.<thuộc tính>`:

| Tông | Ý nghĩa | `bg` | `border` | `text` | `icon` | `solid` | `on-solid` |
|---|---|---|---|---|---|---|---|
| `info` | Đang diễn ra bình thường; thông tin | `blue.50` | `blue.100` | `blue.700` | `blue.600` | `blue.600` | `gray.0` |
| `success` | Hoàn tất tốt, hợp lệ, sẵn sàng | `green.50` | `green.100` | `green.700` | `green.600` | `green.600` | `gray.0` |
| `warning` | Chờ xử lý hoặc cần chú ý; chưa phải vi phạm nặng | `amber.50` | `amber.100` | `amber.700` | `amber.600` | `amber.600` | `gray.0` |
| `danger` | Vi phạm, bị từ chối, bị chặn, hư hỏng, mất, lỗi | `red.50` | `red.100` | `red.700` | `red.600` | `red.600` | `gray.0` |
| `neutral` | Đã kết thúc, không hoạt động, phân loại trung tính | `gray.100` | `gray.300` | `gray.700` | `gray.600` | `gray.700` | `gray.0` |

(Bảng ghi tên bậc rút gọn; ví dụ `blue.50` là `color.blue.50`.) Mức nhấn **nhạt** dùng `bg`, `text`, `icon`; mức **đậm** dùng `solid` làm nền và `on-solid` làm chữ.

### 5.3 Vai trò kiểu chữ

| Token | Cỡ / dòng | Độ đậm | Dùng cho |
|---|---|---|---|
| `type.title-screen` | 24 / 32 | bold | Tiêu đề trang web; lời chào ở Trang chủ SV; tiêu đề màn hình đăng nhập |
| `type.title-section` | 20 / 28 | medium | Tiêu đề khu vực và hộp thoại; tiêu đề thanh tiêu đề Android |
| `type.title-card` | 16 / 24 | medium | Tiêu đề thẻ, khối cấu hình, nhóm thông tin |
| `type.body` | `[Web]` 14 / 20; `[Android]` 16 / 24 | regular | Nội dung, giá trị ô nhập, ô bảng, dòng danh sách |
| `type.body-strong` | như `type.body` | medium | Nhấn mạnh trong nội dung: họ tên SV, tổng tiền, tên loại ở dòng danh sách |
| `type.label` | 14 / 20 | medium | Nhãn ô nhập, chữ trên nút, tiêu đề cột, tab, chip, mục menu và điều hướng web |
| `type.label-sm` | 12 / 16 | medium | Nhãn trạng thái, nhãn thanh điều hướng dưới `[Android]` |
| `type.caption` | 12 / 16 | regular | Chú thích, thời điểm, chữ trợ giúp, thông báo lỗi dưới ô nhập |
| `type.code` | 14 / 20 | regular, `font.family.mono` | Mã phiếu, mã thiết bị, mã SV, mã biên bản, `traceId` |
| `type.numeric-lg` | 28 / 36 | bold | Giá trị bộ đếm ngược (CMP-21) |

Mọi vai trò dùng `font.family.sans`, trừ `type.code`. Màu chữ mặc định `color.text.primary`.

### 5.4 Tương phản đã kiểm chứng

Tỷ lệ tính theo công thức độ chói tương đối của WCAG 2.x từ giá trị ở mục 4.1. Yêu cầu: chữ tối thiểu 4,5:1; viền điều khiển, vòng tiêu điểm và biểu tượng tối thiểu 3:1 (ĐX-DS-03).

| Cặp màu (chữ hoặc thành phần / nền) | Dùng ở | Tỷ lệ | Yêu cầu | Kết quả |
|---|---|---|---|---|
| Chữ chính / nền mặc định (`default`) | Nội dung chính | 17,44:1 | 4,5:1 | Đạt |
| Chữ chính / nền phụ (`subtle`) | Nội dung trên nền trang, hàng rê chuột | 16,27:1 | 4,5:1 | Đạt |
| Chữ chính / nền được chọn (`selected`) | Dòng, thẻ được chọn | 15,76:1 | 4,5:1 | Đạt |
| Chữ phụ / nền mặc định (`default`) | Chú thích, chữ phụ | 7,29:1 | 4,5:1 | Đạt |
| Chữ phụ / nền phụ (`subtle`) | Chú thích trên nền trang | 6,80:1 | 4,5:1 | Đạt |
| Chữ phụ / nền xám nhạt (`muted`) | Chữ phụ trong tiêu đề bảng | 6,39:1 | 4,5:1 | Đạt |
| Chữ phụ / nền được chọn (`selected`) | Chú thích trong dòng được chọn | 6,59:1 | 4,5:1 | Đạt |
| Chữ gợi ý / nền mặc định (`default`) | Placeholder (chỉ trên ô nhập nền trắng) | 4,72:1 | 4,5:1 | Đạt |
| Chữ liên kết / nền mặc định (`default`) | Liên kết, nút văn bản | 9,75:1 | 4,5:1 | Đạt |
| Chữ liên kết / nền phụ (`subtle`) | Liên kết trên nền trang | 9,10:1 | 4,5:1 | Đạt |
| Chữ nút văn bản / nền rê chuột | Nút văn bản khi rê chuột | 8,81:1 | 4,5:1 | Đạt |
| Chữ nút văn bản / nền khi nhấn | Nút văn bản khi nhấn | 7,56:1 | 4,5:1 | Đạt |
| Chữ trắng / nút chính | Nút chính | 7,54:1 | 4,5:1 | Đạt |
| Chữ trắng / nút chính rê chuột | – | 9,75:1 | 4,5:1 | Đạt |
| Chữ trắng / nút chính khi nhấn | – | 12,28:1 | 4,5:1 | Đạt |
| Chữ trắng / nút hủy bỏ | Nút hủy bỏ | 5,46:1 | 4,5:1 | Đạt |
| Chữ trắng / nút hủy bỏ rê chuột | – | 7,66:1 | 4,5:1 | Đạt |
| Chữ trắng / nút hủy bỏ khi nhấn | – | 10,26:1 | 4,5:1 | Đạt |
| Chữ tông `info` / nền tông `info` | Nhãn nhạt, khối thông báo | 8,81:1 | 4,5:1 | Đạt |
| Chữ tông `success` / nền tông `success` | Nhãn nhạt, khối thông báo | 6,60:1 | 4,5:1 | Đạt |
| Chữ tông `warning` / nền tông `warning` | Nhãn nhạt, khối thông báo | 6,28:1 | 4,5:1 | Đạt |
| Chữ tông `danger` / nền tông `danger` | Nhãn nhạt, khối thông báo | 6,70:1 | 4,5:1 | Đạt |
| Chữ tông `neutral` / nền tông `neutral` | Nhãn nhạt, khối thông báo | 9,00:1 | 4,5:1 | Đạt |
| Chữ trắng / nền đậm tông `info` | Nhãn đậm | 7,54:1 | 4,5:1 | Đạt |
| Chữ trắng / nền đậm tông `success` | Nhãn đậm | 5,37:1 | 4,5:1 | Đạt |
| Chữ trắng / nền đậm tông `warning` | Nhãn đậm | 4,58:1 | 4,5:1 | Đạt |
| Chữ trắng / nền đậm tông `danger` | Nhãn đậm | 5,46:1 | 4,5:1 | Đạt |
| Chữ trắng / nền đậm tông `neutral` | Nhãn đậm | 10,28:1 | 4,5:1 | Đạt |
| Viền điều khiển / nền mặc định (`default`) | Ô nhập, ô chọn, nút phụ | 3,53:1 | 3:1 | Đạt |
| Viền điều khiển / nền phụ (`subtle`) | Ô chọn trên nền trang | 3,29:1 | 3:1 | Đạt |
| Viền điều khiển / nền xám nhạt (`muted`) | Viền khi điều khiển nằm trên nền tắt tiếng | 3,09:1 | 3:1 | Đạt |
| Vòng tiêu điểm / nền mặc định (`default`) | Tiêu điểm bàn phím | 5,57:1 | 3:1 | Đạt |
| Vòng tiêu điểm / nền phụ (`subtle`) | Tiêu điểm bàn phím trên nền trang | 5,20:1 | 3:1 | Đạt |
| Viền lỗi / nền mặc định (`default`) | Ô nhập lỗi | 5,46:1 | 3:1 | Đạt |
| Viền mục được chọn / nền mặc định (`default`) | Thẻ, mục được chọn | 7,54:1 | 3:1 | Đạt |
| Biểu tượng tông `info` / nền tông `info` | Biểu tượng trong nhãn, khối thông báo | 6,81:1 | 3:1 | Đạt |
| Biểu tượng tông `success` / nền tông `success` | Biểu tượng trong nhãn, khối thông báo | 4,77:1 | 3:1 | Đạt |
| Biểu tượng tông `warning` / nền tông `warning` | Biểu tượng trong nhãn, khối thông báo | 4,20:1 | 3:1 | Đạt |
| Biểu tượng tông `danger` / nền tông `danger` | Biểu tượng trong nhãn, khối thông báo | 4,78:1 | 3:1 | Đạt |
| Biểu tượng tông `neutral` / nền tông `neutral` | Biểu tượng trong nhãn, khối thông báo | 6,39:1 | 3:1 | Đạt |

Ngoại lệ không bắt buộc đạt 4,5:1: chữ bị vô hiệu (`color.text.disabled` trên `color.action.disabled.bg` đạt 4,14:1); đường kẻ và viền trang trí (`color.border.subtle`, viền tông). Các thành phần này không là cách duy nhất truyền thông tin: nhãn, chữ và vị trí vẫn nhận biết được.

### 5.5 Khoảng cách theo vai trò

| Token | Giá trị | Dùng cho |
|---|---|---|
| `space.gutter` | `[Android]` `space.4`; `[Web]` `space.6` | Lề trái, phải của nội dung màn hình |
| `space.section` | `space.6` | Khoảng giữa các khu vực lớn trong một màn hình |
| `space.card` | `space.4` | Đệm bên trong thẻ, khối thông báo, ô đính ảnh |
| `space.dialog` | `space.6` | Đệm bên trong hộp thoại |
| `space.field-label` | `space.1` | Khoảng giữa nhãn và ô nhập; giữa ô nhập và dòng lỗi |
| `space.field` | `space.4` | Khoảng giữa hai trường liền nhau trong biểu mẫu |
| `space.actions` | `space.3` | Khoảng giữa các nút cạnh nhau |
| `space.list-gap` | `space.2` | Khoảng giữa các thẻ dòng trong danh sách `[Android]` |
| `space.state` | `space.12` | Đệm dọc của khối trạng thái rỗng, lỗi, đang tải (CMP-25) |

---

## 6. Trạng thái tương tác chung

Mọi thành phần tương tác hỗ trợ các trạng thái dưới đây trừ khi mục 8 ghi khác. Mục 8 chỉ nêu giá trị riêng của từng thành phần.

| Trạng thái | Nền tảng | Quy tắc chung |
|---|---|---|
| Mặc định | Cả hai | Theo token của thành phần. |
| Rê chuột (hover) | Web | Đổi nền hoặc viền sang token `-hover` trong `motion.duration.fast`. Rê chuột không là cách duy nhất báo thành phần bấm được (chữ, viền và con trỏ cũng báo). |
| Tiêu điểm (focus) | Web | Chỉ hiển thị khi dùng bàn phím (`:focus-visible`): vòng `border.width.strong` màu `color.focus.ring`, cách mép `border.focus-offset`. Không bao giờ ẩn vòng tiêu điểm mà không có thay thế. |
| | Android | Dùng chỉ báo tiêu điểm của hệ thống cho bàn phím ngoài; không tự vẽ thêm. |
| Nhấn (pressed) | Cả hai | Web: token `-pressed`. Android: hiệu ứng gợn `color.overlay.pressed` trong phạm vi thành phần. |
| Vô hiệu (disabled) | Cả hai | Nền `color.action.disabled.bg`, chữ `color.action.disabled.text`, viền `color.action.disabled.border`; không nhận tiêu điểm, không phản hồi chạm. **Khi vô hiệu do điều kiện nghiệp vụ** (ví dụ Duyệt bị chặn, Tiếp tục khi SV không hợp lệ, Lập phiếu mượn khi thiếu mô tả) **lý do phải hiển thị ngay cạnh hoặc phía trên hành động**, không chỉ trong chú giải (ĐX-DS-11). Khi vô hiệu chỉ vì đang chờ phản hồi thì dùng trạng thái đang xử lý bên dưới. |
| Đang xử lý (loading) | Cả hai | Hành động gửi dữ liệu vô hiệu trong lúc chờ (ĐX-UF-06). Nút: chữ đổi thành "Đang xử lý" kèm chỉ báo quay, giữ nguyên kích thước để bố cục không dịch chuyển. Vùng dữ liệu: chỉ báo ở đúng vùng đang chờ, không che vùng đã có dữ liệu (`UI-Screens.md` 3.1). |
| Được chọn (selected) | Cả hai | Nền `color.surface.selected`, viền `color.border.selected` độ dày `border.width.strong` hoặc dấu chọn; luôn có dấu hiệu ngoài màu (dấu tích, viền dày hoặc vạch nhấn). |
| Lỗi (error) | Cả hai | Viền `color.border.error`, biểu tượng `error`, chữ lỗi `color.tone.danger.text` dưới trường, gắn với đúng trường; không dùng màu đơn thuần. |
| Chỉ đọc (read-only) | Cả hai | Nền `color.surface.subtle`, viền `color.border.subtle`, chữ vẫn `color.text.primary` (không làm mờ để không bị nhầm với vô hiệu); không nhận nhập, vẫn đọc được bằng trình đọc màn hình. |
| Được làm nổi bật (highlighted) | Cả hai | Chỉ dùng cho dòng cần nhận ra. **Kiểu thông tin** (dòng của SV đang giao ở ĐX-UI-09; loại đang nhận thiết bị ở SCR-NV-06): nền `color.surface.selected`, vạch nhấn `border.width.accent` màu `color.border.selected` ở đầu dòng. **Kiểu cảnh báo** (khoản không đủ số lượng ở SCR-NV-03): nền `color.tone.danger.bg`, vạch nhấn màu `color.tone.danger.solid`. Cả hai có chữ thay thế cho trình đọc màn hình; kiểu cảnh báo luôn kèm nhãn chữ (xem CMP-18). |

**Thứ tự ưu tiên khi nhiều trạng thái cùng xảy ra:** vô hiệu → đang xử lý → lỗi → được chọn → tiêu điểm → rê chuột.

---

## 7. Nhãn trạng thái nghiệp vụ

Mục này thực hiện yêu cầu của `UI-Screens.md` 3.3: mỗi loại trạng thái dùng **một thành phần nhãn dùng chung** (CMP-13) với nhãn và quy tắc phân biệt do Design-System quy định. Nhãn chữ lấy từ Phụ lục A của `User-Flows.md`; mã lấy từ API. Màn hình không tự đặt tên nhãn hay chọn màu khác.

### 7.1 Quy tắc

| Quy tắc | Nội dung |
|---|---|
| Khóa ánh xạ | Cặp (đối tượng, mã). Một số mã trùng giữa các đối tượng (`DA_HUY`, `HET_HAN`, `DA_NHAN`, `QUA_HAN`, `HU_HONG`, `MAT`) có thể có tông khác nhau; luôn tra theo đối tượng. |
| Tông | Theo mục 5.2. Tông chỉ truyền đạt ý nghĩa tổng quát; ý nghĩa chính xác nằm ở chữ của nhãn. |
| Mức nhấn | **Nhạt** là mặc định. **Đậm** chỉ dùng cho trạng thái mà nguồn yêu cầu nhấn mạnh: phiếu mượn Quá hạn (SCR-SV-09), quyền mượn khác Bình thường (SCR-SV-01), và kết quả kiểm tra Mất (SCR-NV-09 nêu nổi bật hệ quả). Đã đậm thì đậm ở mọi nơi hiển thị mã đó. |
| Nguyên nhân kèm theo | Nhãn không thể hiện nguyên nhân. Với `HET_HAN` (phiếu yêu cầu, đăng ký chờ), nguyên nhân hiển thị bằng dòng chữ cạnh nhãn; khi đã ghi vi phạm "Không đến nhận" thì dòng chữ dùng tông `danger` kèm biểu tượng `error`, còn "chưa được duyệt trong ngày" (không vi phạm) dùng `color.text.secondary`. Tông của nhãn `HET_HAN` không đổi (`User-Flows.md` UF-SV-05). |
| Biểu tượng | Theo tông (Phụ lục A); `neutral` không có biểu tượng. Biểu tượng bổ trợ, chữ là chính. |
| Chỉ dựa trên mã | Nhãn sinh từ mã API, không từ phép tính ở client (NT-01). |
| Mã chưa biết | Hiển thị nguyên mã bằng `type.code`, tông `neutral`, không biểu tượng; không báo lỗi (`UI-Screens.md` 3.3). |
| Vai trò | Ba vai trò hiển thị bằng chữ thường, không dùng nhãn trạng thái: Sinh viên (`SINH_VIEN`), Nhân viên quản lý thiết bị (`NHAN_VIEN`), Quản trị viên (`QUAN_TRI`). |

### 7.2 Bảng ánh xạ mã API

| Đối tượng | Mã (API) | Nhãn hiển thị | Tông | Mức nhấn |
|---|---|---|---|---|
| Tình trạng thiết bị | `SAN_SANG` | Sẵn sàng cho mượn | `success` | Nhạt |
|  | `DANG_DUOC_MUON` | Đang được mượn | `info` | Nhạt |
|  | `DANG_BAO_TRI_SUA_CHUA` | Đang bảo trì/sửa chữa | `warning` | Nhạt |
|  | `DA_THANH_LY` | Đã thanh lý | `neutral` | Nhạt |
|  | `DA_BAO_MAT` | Đã báo mất | `danger` | Nhạt |
| Phiếu yêu cầu mượn | `CHO_DUYET` | Chờ duyệt | `warning` | Nhạt |
|  | `DA_DUYET` | Đã duyệt | `success` | Nhạt |
|  | `TU_CHOI` | Từ chối | `danger` | Nhạt |
|  | `DA_HUY` | Đã hủy | `neutral` | Nhạt |
|  | `HET_HAN` | Hết hạn | `neutral` | Nhạt |
|  | `DA_NHAN` | Đã nhận | `success` | Nhạt |
| Phiếu mượn | `DANG_MUON` | Đang mượn | `info` | Nhạt |
|  | `QUA_HAN` | Quá hạn | `danger` | Đậm |
|  | `DA_TRA` | Đã trả | `success` | Nhạt |
| Đăng ký chờ | `DANG_CHO` | Đang chờ | `info` | Nhạt |
|  | `DA_THONG_BAO` | Đã thông báo | `success` | Nhạt |
|  | `DA_NHAN` | Đã nhận | `success` | Nhạt |
|  | `DA_HUY` | Đã hủy | `neutral` | Nhạt |
|  | `HET_HAN` | Hết hạn | `neutral` | Nhạt |
| Kết quả kiểm tra | `NGUYEN_VEN` | Nguyên vẹn | `success` | Nhạt |
|  | `HU_HONG` | Hư hỏng | `danger` | Nhạt |
|  | `THIEU_PHU_KIEN` | Thiếu phụ kiện | `warning` | Nhạt |
|  | `MAT` | Mất | `danger` | Đậm |
| Biên bản vi phạm | `CHUA_NOP` | Chưa nộp | `warning` | Nhạt |
|  | `DA_NOP` | Đã nộp | `success` | Nhạt |
|  | `DA_HUY` | Đã hủy | `neutral` | Nhạt |
| Loại vi phạm | `TRA_TRE` | Trả trễ | `warning` | Nhạt |
|  | `HU_HONG` | Hư hỏng | `danger` | Nhạt |
|  | `MAT` | Mất | `danger` | Nhạt |
|  | `KHONG_DEN_NHAN` | Không đến nhận | `warning` | Nhạt |
| Trạng thái học | `DANG_HOC` | Đang học | `success` | Nhạt |
|  | `TOT_NGHIEP` | Tốt nghiệp | `neutral` | Nhạt |
|  | `THOI_HOC` | Thôi học | `danger` | Nhạt |
|  | `DINH_CHI` | Đình chỉ | `danger` | Nhạt |
| Quyền mượn của SV | `BINH_THUONG` | Bình thường | `success` | Nhạt |
|  | `TAM_KHOA` | Tạm khóa | `warning` | Đậm |
|  | `CAM_MUON_DAI_HAN` | Cấm mượn dài hạn | `danger` | Đậm |
| Tài khoản NV, QT | `HOAT_DONG` | Hoạt động | `success` | Nhạt |
|  | `TAM_KHOA_DANG_NHAP` | Tạm khóa đăng nhập | `warning` | Nhạt |
|  | `BI_KHOA` | Bị khóa | `danger` | Nhạt |
| Loại thực hiện bảo trì | `BAO_TRI_DINH_KY` | Bảo trì định kỳ | `neutral` | Nhạt |
|  | `SUA_CHUA` | Sửa chữa | `neutral` | Nhạt |
| Kết quả bảo trì | `DUA_VE_SAN_SANG` | Đưa về Sẵn sàng | `success` | Nhạt |
|  | `DE_XUAT_THANH_LY` | Đề xuất thanh lý | `warning` | Nhạt |
| Nguồn giao thiết bị | `PHIEU_YEU_CAU` | Phiếu yêu cầu | `neutral` | Nhạt |
|  | `DANG_KY_CHO` | Danh sách chờ | `neutral` | Nhạt |
|  | `TRUC_TIEP` | Mượn trực tiếp | `neutral` | Nhạt |
| Loại thông báo | `NHAC_HAN_TRA` | Nhắc hạn trả | `warning` | Nhạt |
|  | `QUA_HAN` | Quá hạn | `danger` | Nhạt |
|  | `THIET_BI_CHO_SAN_SANG` | Thiết bị chờ sẵn sàng | `success` | Nhạt |
|  | `DUYET_HOAC_TU_CHOI` | Duyệt hoặc từ chối | `info` | Nhạt |
|  | `VI_PHAM_PHI_PHAT` | Vi phạm, phí phạt | `danger` | Nhạt |
|  | `NHAC_NHO_NV` | Nhắc nhở của NV | `info` | Nhạt |
|  | `NHAC_BAO_TRI` | Nhắc bảo trì | `neutral` | Nhạt |

Ghi chú: `NHAC_NHO_NV` thuộc chức năng ngoài giai đoạn 1 (`User-Flows.md` 3.8) và `NHAC_BAO_TRI` chỉ gửi cho NV qua email, không hiển thị trong ứng dụng; hai mã vẫn có dòng ánh xạ để mọi mã API đều có nhãn, không thêm màn hình hay chức năng.

### 7.3 Nhãn phát sinh từ dữ liệu không phải mã trạng thái

`UI-Screens.md` dùng một số nhãn sinh từ giá trị logic hoặc điều kiện so sánh, không có mã trạng thái trong API. Chúng dùng cùng thành phần CMP-13 (ĐX-DS-10).

| Nhãn | Điều kiện (trường dữ liệu) | Tông | Nhấn | Dùng ở |
|---|---|---|---|---|
| Hết hàng | `soLuongKhaDung` = 0 | `danger` | Nhạt | SCR-SV-02, SCR-SV-03, SCR-SV-04 |
| Không đủ | `soLuongYeuCau` lớn hơn `soLuongKhaDung` ở một loại | `danger` | Nhạt | SCR-NV-03 |
| Hợp lệ / Không hợp lệ | `hopLe` = true / false | `success` / `danger` | Đậm (dùng làm tiêu đề khối thông báo, CMP-24) | SCR-NV-04, SCR-NV-05 |
| Được duyệt / Không được duyệt | `duocDuyet` = true / false | `success` / `danger` | Đậm (dùng làm tiêu đề khối thông báo, CMP-24) | SCR-NV-03 |
| Đã ngưng sử dụng | `ngungSuDung` = true | `neutral` | Nhạt | SCR-QT-03, SCR-QT-04 |
| Đang sử dụng / Đã tắt | `dangSuDung` = true / false | `success` / `neutral` | Nhạt | SCR-QT-07 |
| Chưa trả | Thiết bị của phiếu chưa có kết quả kiểm tra | `neutral` | Nhạt | SCR-SV-10 |
| Bảo trì / Thanh lý | `loaiBanGhi` = `BAO_TRI` / `THANH_LY` | `neutral` | Nhạt | SCR-QT-12 |
| Tạm tính | NV đổi khung phạt khác với khung máy chủ đề xuất, số tiền hiển thị chưa phải số chính thức (ĐX-UI-10) | `warning` | Nhạt | SCR-NV-10 |

Khi mã số SV của nguồn khác SV đã quét (ĐX-UI-14), cảnh báo "Sinh viên của nguồn khác với thẻ đã quét" dùng CMP-24 tông `warning`, không phải nhãn.

---

## 8. Thành phần

### 8.1 Danh mục

| Mã | Thành phần | Tên kỹ thuật | Nền tảng | Nhóm |
|---|---|---|---|---|
| CMP-01 | Nút | `Button` | Cả hai | Hành động |
| CMP-02 | Nút biểu tượng | `IconButton` | Cả hai | Hành động |
| CMP-03 | Liên kết | `Link` | Cả hai | Hành động |
| CMP-04 | Ô nhập | `TextField` | Cả hai | Nhập liệu |
| CMP-05 | Ô tìm kiếm | `SearchField` | Android | Nhập liệu |
| CMP-06 | Ô chọn | `Select` | Cả hai | Nhập liệu |
| CMP-07 | Ô tích và nút chọn | `Checkbox`, `Radio` | Cả hai | Nhập liệu |
| CMP-08 | Điều khiển phân đoạn | `SegmentedControl` | Cả hai | Nhập liệu |
| CMP-09 | Tab | `Tabs` | Web | Điều hướng nội dung |
| CMP-10 | Bộ chọn số lượng | `QuantityStepper` | Cả hai | Nhập liệu |
| CMP-11 | Ô nhập mã quầy | `ScanInput` | Web | Nhập liệu |
| CMP-12 | Khu đính ảnh | `PhotoAttach` | Web | Nhập liệu |
| CMP-13 | Nhãn trạng thái | `StatusBadge` | Cả hai | Hiển thị |
| CMP-14 | Danh sách thông tin | `KeyValueList` | Cả hai | Hiển thị |
| CMP-15 | Thẻ | `Card` | Cả hai | Hiển thị |
| CMP-16 | Dòng danh sách | `ListRow` | Android | Hiển thị |
| CMP-17 | Chip bộ lọc | `FilterChip` | Android | Hiển thị |
| CMP-18 | Bảng dữ liệu | `DataTable` | Web | Hiển thị |
| CMP-19 | Phân trang | `Pagination` | Web | Hiển thị |
| CMP-20 | Menu thao tác | `Menu` | Web | Hiển thị |
| CMP-21 | Bộ đếm ngược | `Countdown` | Cả hai | Hiển thị |
| CMP-22 | Khối đối chiếu | `ComparePanel` | Web | Hiển thị |
| CMP-23 | Ảnh và xem ảnh lớn | `ImageGallery`, `ImageViewer` | Cả hai | Hiển thị |
| CMP-24 | Khối thông báo | `Banner` | Cả hai | Phản hồi |
| CMP-25 | Khối trạng thái | `StateView` | Cả hai | Phản hồi |
| CMP-26 | Hộp thoại | `Dialog` | Cả hai | Lớp phủ |
| CMP-27 | Thanh điều hướng dưới | `BottomNav` | Android | Khung điều hướng |
| CMP-28 | Thanh tiêu đề | `TopBar` | Android | Khung điều hướng |
| CMP-29 | Thanh menu bên | `Sidebar` | Web | Khung điều hướng |
| CMP-30 | Thanh đầu trang | `Header` | Web | Khung điều hướng |
| CMP-31 | Thanh tiến trình các bước | `Stepper` | Web | Điều hướng nội dung |

Mỗi thành phần dưới đây ghi: mục đích, cấu tạo, biến thể và kích thước, trạng thái và quy tắc riêng, khả năng tiếp cận, nơi dùng. Trạng thái chung ở mục 6 không lặp lại.

### 8.2 Hành động

#### CMP-01 Nút (`Button`)

| Mục | Quy định |
|---|---|
| Mục đích | Kích hoạt một hành động. |
| Cấu tạo | Chữ (`type.label`); biểu tượng `size.icon.md` bên trái, tùy chọn. Nhãn chữ là động từ theo mục 9.8. Biểu tượng dùng khi giúp nhận ra hành động, ví dụ `refresh` ở nút Làm mới, `download` ở nút Xuất Excel và Xuất CSV (SCR-QT-12). |
| Biến thể | **Chính** (`primary`): hành động quan trọng nhất của vùng, tối đa một nút chính trong mỗi vùng hành động (toàn màn hình hoặc một hộp thoại). **Phụ** (`secondary`): hành động thay thế. **Văn bản** (`tertiary`): hành động nhẹ, hành động ở dòng bảng, liên kết dạng nút. **Hủy bỏ** (`destructive`): hành động xóa, ngừng, khóa, hủy, từ chối (danh sách ở mục 9.2). |
| Kích thước | `[Web]` `size.control.sm`/`md`/`lg`, mặc định `md`; `lg` cho thao tác chính ở quầy. `[Android]` `size.button.height`; nút chính cuối biểu mẫu rộng hết chiều ngang. Bo góc `radius.md`; đệm ngang `space.4`. |
| Màu theo biến thể | Chính: `color.action.primary.*`. Hủy bỏ: `color.action.destructive.*`. Phụ: `color.action.secondary.*`. Văn bản: chữ `color.action.tertiary.text`, nền trong suốt, `bg-hover`/`bg-pressed`. Vô hiệu: `color.action.disabled.*` (mọi biến thể). |
| Quy tắc | Nút gửi dữ liệu vô hiệu trong lúc chờ phản hồi và hiển thị "Đang xử lý" (mục 6). Hành động bị vô hiệu do nghiệp vụ phải có lý do ở cạnh (ĐX-DS-11). Hai nút cạnh nhau cách `space.actions`; nút chính ở bên phải trong hộp thoại và hàng nút cuối biểu mẫu `[Web]`. |
| Tiếp cận | Là phần tử nút thật; nhãn chữ đủ nghĩa khi đọc riêng; `[Android]` vùng chạm `size.touch.min`. |
| Dùng ở | Mọi màn hình có hành động. |

#### CMP-02 Nút biểu tượng (`IconButton`)

| Mục | Quy định |
|---|---|
| Mục đích | Hành động quen thuộc không cần chữ: Quay lại, Đóng, mở rộng dòng, gỡ ảnh, xóa nội dung ô tìm kiếm. |
| Cấu tạo | Biểu tượng `size.icon.lg` (hoặc `md` trong ô nhập). Không có chữ. |
| Kích thước | `[Web]` `size.control.md` × `size.control.md` (vùng bấm). `[Android]` `size.touch.min` × `size.touch.min`. |
| Trạng thái | Nền trong suốt; rê chuột `color.action.secondary.bg-hover`; nhấn `-pressed`; biểu tượng `color.text.secondary`, khi vô hiệu `color.text.disabled`. |
| Quy tắc | Bắt buộc có tên truy cập bằng tiếng Việt (ví dụ "Quay lại", "Đóng", "Gỡ ảnh"). Chỉ dùng cho biểu tượng trong Phụ lục A có nghĩa phổ biến; hành động khác dùng CMP-01. |
| Dùng ở | SCR-SV-03 đến SCR-SV-12 (Quay lại, CMP-28); hộp thoại (Đóng); SCR-NV-06, SCR-NV-09, SCR-QT-04 (Gỡ ảnh); SCR-SV-02 (xóa nội dung ô tìm kiếm); dòng mở rộng của CMP-18. |

#### CMP-03 Liên kết (`Link`)

| Mục | Quy định |
|---|---|
| Mục đích | Chuyển tới màn hình hoặc đối tượng liên quan: mã phiếu mượn, "Tìm phiếu của SV", "Đổi phí trả trễ ở Cấu hình", "Xem lịch sử mượn/trả", liên kết "Đăng ký chờ" ở dòng hết hàng. |
| Cấu tạo | Chữ `color.text.link`, `type.body` hoặc theo ngữ cảnh. Luôn gạch chân, trừ mục điều hướng và nút văn bản. |
| Trạng thái | Rê chuột: chữ `color.text.link-hover`; tiêu điểm: vòng tiêu điểm; đã thăm: không đổi màu. |
| Quy tắc | Mã (phiếu, biên bản) làm liên kết dùng `type.code`. Hành động thay đổi dữ liệu không dùng liên kết mà dùng CMP-01. |
| Dùng ở | SCR-SV-04, SCR-SV-07, SCR-SV-08, SCR-SV-11, SCR-SV-12, SCR-SV-14, SCR-NV-04, SCR-NV-11, SCR-QT-03. |

### 8.3 Nhập liệu

#### CMP-04 Ô nhập (`TextField`)

| Mục | Quy định |
|---|---|
| Mục đích | Nhập một giá trị: văn bản, số nguyên, số thập phân cho tỷ lệ khung phạt, mật khẩu, nhiều dòng, ngày, ngày giờ, giờ. |
| Cấu tạo | Từ trên xuống: nhãn (`type.label`, dấu `*` ở cuối nhãn nếu bắt buộc) → ô nhập → chú thích (`type.caption`, `color.text.secondary`) hoặc dòng lỗi (`type.caption`, `color.tone.danger.text`, biểu tượng `error` `size.icon.sm`). Nhãn luôn hiển thị, không thay bằng placeholder (`UI-Screens.md` 3.8). Khoảng cách `space.field-label`. |
| Kích thước | `[Web]` chiều cao `size.control.md` (ô nhiều dòng tối thiểu 3 dòng). `[Android]` `size.input.height`. Bo góc `radius.md`; đệm ngang `space.3`. Chữ `type.body`. |
| Biến thể | **Văn bản**; **Số nguyên** (chỉ nhận chữ số; `[Android]` bàn phím số; có thể có đơn vị hậu tố bên phải: giờ, phút, ngày, VNĐ bằng `type.body` `color.text.secondary`); **Số thập phân** (`[Web]` tỷ lệ khung phạt `tyLePhanTram`, `inputMode="decimal"`, hậu tố `%`): nhận toàn bộ chuỗi số hữu hạn, giữ phần thập phân khi gửi kiểu `number`; không dùng bộ đọc số nguyên, không cắt hoặc tự làm tròn. Miền theo `openapi.yaml` `KhungPhatRequest` là lớn hơn 0 đến 100%, BE kiểm tra cuối; không áp giới hạn độ chính xác khác contract; **Mật khẩu** (che ký tự, không có nút hiện mật khẩu); **Nhiều dòng** (lý do từ chối, mô tả tình trạng); **Ngày**, **Ngày giờ**, **Giờ** (xem dưới). |
| Màu | Nền `color.surface.default`; viền `color.border.control` (rê chuột `color.border.control-hover`); tiêu điểm: viền `color.focus.ring` dày `border.width.strong`; lỗi: viền `color.border.error`; vô hiệu và chỉ đọc theo mục 6; placeholder `color.text.placeholder`. |
| Ngày, giờ | Hiển thị và nhận nhập theo `dd/MM/yyyy`, `dd/MM/yyyy HH:mm`, `HH:mm` (24 giờ), giờ Việt Nam, không phụ thuộc vùng ngôn ngữ của thiết bị (ĐX-UI-12). Có cách chọn bằng bộ chọn ngày của nền tảng. |
| Quy tắc | Khi có lỗi: giữ nguyên dữ liệu đã nhập, hiển thị lỗi cạnh đúng trường, đưa tiêu điểm tới trường lỗi đầu tiên (`UI-Screens.md` 3.2). Trường chỉ đọc (ví dụ mã thiết bị khi sửa) dùng trạng thái chỉ đọc, không dùng vô hiệu. Biểu mẫu có trường bắt buộc ghi chú "* Bắt buộc" ở đầu biểu mẫu. Mật khẩu: sau `DANG_NHAP_SAI` xóa giá trị mật khẩu, giữ tên đăng nhập hoặc mã số sinh viên. |
| Tiếp cận | Nhãn gắn với ô; lỗi gắn với ô (mô tả của ô); bắt buộc được khai báo cho trình đọc màn hình. |
| Dùng ở | SCR-CM-01, SCR-CM-02, SCR-SV-04 (thời gian mượn), SCR-NV-03 (lý do từ chối), SCR-NV-06 (mô tả tình trạng ban đầu), SCR-NV-09 (mô tả tình trạng lúc trả), SCR-NV-14 (hộp Ghi hoàn thành), SCR-QT-02 đến SCR-QT-09, SCR-QT-10 (điều chỉnh số tiền), SCR-QT-11 (ngày thanh lý), SCR-QT-12 (từ ngày, đến ngày); bộ lọc tên và mã số SV ở SCR-NV-08, SCR-NV-11, SCR-QT-04, SCR-QT-10. |

#### CMP-05 Ô tìm kiếm (`SearchField`) `[Android]`

| Mục | Quy định |
|---|---|
| Mục đích | Tìm theo tên loại thiết bị (SCR-SV-02). |
| Cấu tạo | CMP-04 một dòng, biểu tượng `search` ở đầu; khi có chữ hiện CMP-02 xóa nội dung ở cuối. Nhãn nhìn thấy "Tìm theo tên thiết bị" đặt trên ô, không dùng placeholder thay nhãn (`UI-Screens.md` 3.8). |
| Quy tắc | Gửi tìm kiếm khi SV xác nhận nhập (phím tìm kiếm) hoặc ngừng gõ (ĐX-UI-05; độ trễ do Frontend-Conventions quy định). Đổi nội dung thì tải lại từ trang 1. |
| Dùng ở | SCR-SV-02. Bộ lọc tên ở web dùng CMP-04. |

#### CMP-06 Ô chọn (`Select`)

| Mục | Quy định |
|---|---|
| Mục đích | Chọn một (hoặc nhiều) giá trị từ danh sách cố định: phòng, loại thiết bị, tình trạng, trạng thái biên bản, báo cáo. |
| Cấu tạo | Nhãn và ô giống CMP-04, giá trị đang chọn hoặc gợi ý, biểu tượng `expand_more` bên phải. Mở ra danh sách: `[Web]` danh sách thả xuống (`shadow.menu`, `z.dropdown`); `[Android]` danh sách chọn của nền tảng ở dạng hộp thoại hoặc trang dưới. |
| Mục trong danh sách | Cao tối thiểu `size.control.md` `[Web]` hoặc `size.touch.min` `[Android]`; chữ `type.body`; mục có hai thông tin (tên phòng và cơ sở) hiển thị tên ở dòng trên, cơ sở ở dòng dưới bằng `type.caption`. Mục đang chọn: nền `color.surface.selected` và biểu tượng `check`. |
| Biến thể | **Chọn một**: có mục "Tất cả" đầu danh sách khi dùng làm bộ lọc. **Chọn nhiều** (SCR-QT-09 quyền phòng, SCR-QT-04 nhà cung cấp): mỗi mục có CMP-07 ô tích; ô hiển thị tóm tắt số mục đã chọn, ví dụ "2 phòng đã chọn"; danh sách không đóng sau mỗi lần chọn; có nút Xong `[Android]`. |
| Quy tắc | Loại hoặc phòng đã ngưng sử dụng hiển thị kèm nhãn "Đã ngưng sử dụng" (CMP-13) trong danh sách (SCR-QT-04). Bộ lọc phòng của NV chỉ liệt kê phòng được cấp quyền (`UI-Screens.md` 3.7). |
| Lỗi tải lựa chọn | Khi danh sách lựa chọn không tải được (ví dụ bộ lọc ở SCR-SV-02), ô ở trạng thái lỗi kèm nút "Thử lại"; các điều khiển và kết quả khác vẫn dùng được. |
| Bàn phím `[Web]` | Enter hoặc Space mở; mũi tên di chuyển; Enter chọn; Esc đóng. |
| Dùng ở | SCR-SV-02, SCR-SV-04; SCR-NV-02, SCR-NV-05, SCR-NV-09 và SCR-NV-10 (khung phạt), SCR-NV-11, SCR-NV-12, SCR-NV-13, SCR-NV-14; SCR-QT-03 đến SCR-QT-05, SCR-QT-09, SCR-QT-10, SCR-QT-11, SCR-QT-12. |

#### CMP-07 Ô tích và nút chọn (`Checkbox`, `Radio`)

| Mục | Quy định |
|---|---|
| Mục đích | Ô tích: chọn nhiều mục độc lập (thiết bị cần lập biên bản mất, thiết bị cần thanh lý, mục của chọn nhiều). Nút chọn: chọn một trong ít lựa chọn trong biểu mẫu (Kết quả bảo trì: Đưa về Sẵn sàng hoặc Đề xuất thanh lý). |
| Cấu tạo | Hình điều khiển cùng nhãn chữ bên phải (`type.body`); cả hàng nhãn bấm được. Hình ô tích: vuông `size.icon.md`, `radius.sm`; nút chọn: tròn `size.icon.md`, `radius.full`. |
| Màu | Chưa chọn: viền `color.border.control`, nền `color.surface.default`. Đã chọn: nền `color.action.primary.bg`, dấu tích `color.text.inverse`. Vô hiệu theo mục 6. Lỗi: viền `color.border.error`. |
| Kích thước | Vùng bấm gồm cả nhãn; `[Android]` tối thiểu `size.touch.min`; `[Web]` tối thiểu `size.control.sm`. |
| Tiếp cận | Nhóm nút chọn có tên nhóm; ô tích và nút chọn có thể chọn bằng Space. |
| Dùng ở | SCR-NV-12, SCR-QT-11 (ô tích); SCR-NV-14 (nút chọn kết quả); chọn nhiều của CMP-06. |

#### CMP-08 Điều khiển phân đoạn (`SegmentedControl`)

| Mục | Quy định |
|---|---|
| Mục đích | Chọn một trong 3 đến 4 lựa chọn ngắn mà lựa chọn là hành động chính: Tình trạng hàng (Tất cả, Còn hàng, Hết hàng); Kết quả kiểm tra (Nguyên vẹn, Hư hỏng, Thiếu phụ kiện, Mất). |
| Cấu tạo | Dãy ô liền nhau, chiều rộng bằng nhau, chữ `type.label`, viền `color.border.control`, `radius.md` ở hai đầu. Ô đang chọn: nền `color.surface.selected`, viền `color.border.selected` dày `border.width.strong`, kèm biểu tượng `check` (dấu hiệu ngoài màu). |
| Kích thước | `[Web]` `size.control.md`; `[Android]` `size.input.height`. Chữ không xuống dòng; nếu không đủ chỗ ở cỡ chữ lớn thì xếp thành hai hàng. |
| Quy tắc | Luôn có một lựa chọn mặc định ở bộ lọc; ở Kết quả kiểm tra chưa chọn gì cho đến khi NV chọn, và thiếu lựa chọn thì đánh dấu lỗi tại thẻ thiết bị theo mục 9.1. |
| Tiếp cận | Là nhóm nút chọn có tên nhóm; mũi tên di chuyển giữa các lựa chọn `[Web]`. |
| Dùng ở | SCR-SV-02; SCR-NV-09. |

#### CMP-09 Tab (`Tabs`) `[Web]`

| Mục | Quy định |
|---|---|
| Mục đích | Chuyển giữa các khung nhìn của cùng một màn hình: Đang mượn, Quá hạn, Đã trả (SCR-NV-08); Đến hạn bảo trì, Chờ tiếp nhận sửa chữa (SCR-NV-14). |
| Cấu tạo | Dãy tab ngang; chữ `type.label`; viền dưới `color.border.subtle`. Tab đang chọn: chữ `color.text.link` và vạch dưới `border.width.strong` màu `color.border.selected`; tab khác: chữ `color.text.secondary`. Chiều cao `size.control.lg`. |
| Quy tắc | Đổi tab thì tải lại dữ liệu của tab đó; nội dung tab không dùng nhãn trạng thái thay chữ tab. |
| Tiếp cận | Mũi tên trái phải di chuyển giữa tab; tab đang chọn được khai báo cho trình đọc màn hình. |
| Dùng ở | SCR-NV-08, SCR-NV-14. |

#### CMP-10 Bộ chọn số lượng (`QuantityStepper`)

| Mục | Quy định |
|---|---|
| Mục đích | Chọn số lượng nguyên trong khoảng giới hạn. |
| Cấu tạo | Nút giảm, giá trị, nút tăng (CMP-02, biểu tượng `remove` và `add`). Giá trị `type.body-strong`, chữ số cùng độ rộng. `[Web]` giá trị là ô nhập số; `[Android]` giá trị là chữ, thay đổi bằng hai nút. |
| Biến thể | **Tối thiểu 0** (SCR-SV-04: 0 nghĩa là không đưa loại vào phiếu; giá trị 0 hiển thị `color.text.secondary`). **Tối thiểu 1** (SCR-NV-05, nguồn Mượn trực tiếp). Tối đa là `soLuongKhaDung`. |
| Quy tắc | Nút giảm vô hiệu ở mức tối thiểu, nút tăng vô hiệu ở mức tối đa; lý do là số lượng khả dụng đang hiển thị trong cùng dòng (ĐX-DS-11). Loại hết hàng: bộ chọn bị vô hiệu và thay bằng nhãn Hết hàng. |
| Kích thước | Mỗi nút `size.touch.min` `[Android]` hoặc `size.control.md` `[Web]`. |
| Dùng ở | SCR-SV-04, SCR-NV-05. |

#### CMP-11 Ô nhập mã quầy (`ScanInput`) `[Web]`

| Mục | Quy định |
|---|---|
| Mục đích | Nhận mã thẻ sinh viên hoặc mã thiết bị bằng máy quét hoặc nhập tay, nhiều lần liên tiếp (BR-13, BR-40). |
| Cấu tạo | CMP-04 một dòng, cao `size.control.lg`, chữ `type.code`, biểu tượng `qr_code_scanner` ở đầu. Nhãn nhìn thấy nêu loại mã ("Mã thẻ sinh viên", "Mã thiết bị"). Chú thích: "Quét hoặc nhập mã, rồi nhấn Enter". |
| Trạng thái | **Chờ nhập** (mặc định). **Đang tra cứu**: ô tạm không nhận mã mới, chỉ báo quay ở cuối ô, dữ liệu liền kề hiển thị trạng thái đang tải (không chèn bước trung gian, NFR-03). **Lỗi**: viền `color.border.error`, thông báo lỗi dưới ô; giữ mã đã nhập và chọn sẵn toàn bộ chữ để quét hoặc gõ đè. **Đã xử lý**: ô xóa mã (trừ khi màn hình yêu cầu giữ mã) và nhận tiêu điểm lại. |
| Quy tắc | Có tiêu điểm khi mở màn hình và sau mỗi lần xử lý xong một mã. Enter xác nhận mã; nhập tay và quét có kết quả như nhau. |
| Tiếp cận | Tên truy cập là nhãn; kết quả tra cứu và lỗi được thông báo cho trình đọc màn hình ở vùng thông báo (mục 10). |
| Dùng ở | SCR-NV-04 (mã thẻ), SCR-NV-06 (mã thiết bị), SCR-NV-08 (mã thiết bị), SCR-NV-09 (mã thiết bị), SCR-NV-11 (mã số SV, quét thẻ hoặc nhập). |

#### CMP-12 Khu đính ảnh (`PhotoAttach`) `[Web]`

| Mục | Quy định |
|---|---|
| Mục đích | Đính tối đa 5 ảnh tình trạng hoặc ảnh danh mục thiết bị (`UI-Screens.md` 3.6). |
| Cấu tạo | Lưới ô ảnh `size.photo.tile` cách nhau `space.2`. Ô cuối "Thêm ảnh": viền nét đứt `color.border.control`, biểu tượng `add_a_photo`, chữ "Thêm ảnh" và bộ đếm "n/5". Ẩn ô này khi đủ 5 ảnh. |
| Trạng thái ô ảnh | **Đang tải**: ảnh mờ kèm chỉ báo quay và chữ "Đang tải". **Đã tải**: ảnh thu nhỏ và CMP-02 "Gỡ ảnh" ở góc. **Lỗi**: viền `color.border.error`, biểu tượng `error`, chữ lỗi ngắn, nút "Thử lại" và "Gỡ ảnh". **Ảnh có sẵn** (SCR-QT-04 khi sửa): như Đã tải. |
| Kiểm tra tại chỗ | Báo ngay dưới khu đính ảnh, không chờ máy chủ: "Chỉ nhận ảnh jpeg, png hoặc webp"; "Mỗi ảnh tối đa 2 MB"; "Tối đa 5 ảnh". Ảnh vượt giới hạn không được thêm vào lưới. |
| Quy tắc | Ảnh tùy chọn; nhãn khu ghi "Ảnh (không bắt buộc)", riêng ở SCR-NV-09 khi kết quả Mất khu bị vô hiệu kèm chữ "Kết quả Mất không có ảnh". Chạm hoặc nhấn vào ảnh mở CMP-23. Nén, tải lên và gắn `maAnh` theo `UI-Screens.md` 3.6. |
| Tiếp cận | Nút thêm ảnh là nút thật, mở bộ chọn tệp; mỗi ảnh có tên truy cập "Ảnh n"; trạng thái tải và lỗi được thông báo. |
| Dùng ở | SCR-NV-06, SCR-NV-09, SCR-QT-04. |

### 8.4 Hiển thị

#### CMP-13 Nhãn trạng thái (`StatusBadge`)

| Mục | Quy định |
|---|---|
| Mục đích | Hiển thị mọi trạng thái nghiệp vụ và nhãn phát sinh (mục 7). Đây là thành phần duy nhất được dùng cho mục đích này. |
| Cấu tạo | Biểu tượng theo tông (`size.icon.sm`, Phụ lục A; `neutral` không có) → chữ `type.label-sm`. Chiều cao tối thiểu `size.badge.height`; đệm ngang `space.2`; `radius.full`; khoảng giữa biểu tượng và chữ `space.1`. |
| Mức nhấn | **Nhạt**: nền `color.tone.<tông>.bg`, viền `color.tone.<tông>.border` dày `border.width.default`, chữ `.text`, biểu tượng `.icon`. **Đậm**: nền `.solid`, chữ `.on-solid`, không viền. |
| Quy tắc | Không tương tác (không bấm, không nhận tiêu điểm). Chiều rộng theo nội dung; chữ dài (ví dụ "Đang bảo trì/sửa chữa") không bị cắt, xuống dòng khi hết chỗ và nhãn cao thêm. Chỉ nhận mã từ API và điều kiện ở mục 7.3; không nhận màu hay chữ tùy ý. |
| Tiếp cận | Chữ là thông tin; biểu tượng ẩn với trình đọc màn hình. |
| Dùng ở | SCR-SV-01, SCR-SV-02, SCR-SV-03, SCR-SV-04, SCR-SV-06 đến SCR-SV-14, DLG-SV-01; SCR-NV-03, SCR-NV-04, SCR-NV-05, SCR-NV-07 đến SCR-NV-11, SCR-NV-13, SCR-NV-14; SCR-QT-03, SCR-QT-04, SCR-QT-07, SCR-QT-09, SCR-QT-10, SCR-QT-12. |

#### CMP-14 Danh sách thông tin (`KeyValueList`)

| Mục | Quy định |
|---|---|
| Mục đích | Trình bày các cặp tên – giá trị của một đối tượng: hồ sơ SV, đầu phiếu, chi tiết thiết bị. |
| Cấu tạo | Tên trường `type.caption` `color.text.secondary`; giá trị `type.body` `color.text.primary`. `[Web]` hai cột (cột tên rộng `size.keyvalue.label`), cách dòng `space.2`. `[Android]` xếp dọc: tên trên, giá trị dưới, cách cặp `space.3`. |
| Quy tắc | Mã dùng `type.code`; tiền, ngày giờ theo mục 9.7; giá trị là trạng thái dùng CMP-13; giá trị thiếu hiển thị "—" (ĐX-DS-12) hoặc chữ riêng của màn hình (ví dụ "Chưa có ngày dự kiến"). Giá trị dài xuống dòng, không cắt. |
| Dùng ở | SCR-SV-03, SCR-SV-05, SCR-SV-07, SCR-SV-10, SCR-SV-14; SCR-NV-03, SCR-NV-04, SCR-NV-07, SCR-NV-08, SCR-NV-09, SCR-NV-10; SCR-QT-10. |

#### CMP-15 Thẻ (`Card`)

| Mục | Quy định |
|---|---|
| Mục đích | Nhóm thông tin hoặc hành động liên quan trong một khung. |
| Cấu tạo | Nền `color.surface.default`, viền `color.border.subtle`, `radius.md`, đệm `space.card`, không đổ bóng. Tiêu đề (nếu có) `type.title-card`. |
| Biến thể | **Tĩnh**: chỉ chứa nội dung. **Tương tác**: cả thẻ bấm được, có biểu tượng `chevron_right` ở cuối khi chuyển màn hình; rê chuột `[Web]` viền `color.border.control-hover`; có trạng thái nhấn và tiêu điểm theo mục 6. Dùng làm "lối vào chức năng" ở các trang chủ: tiêu đề `type.title-card`, không biểu tượng riêng cho từng chức năng. **Chọn được**: như tương tác nhưng chọn một trong nhiều; được chọn: nền `color.surface.selected`, viền `color.border.selected` dày `border.width.strong`, biểu tượng `check_circle` ở góc. **Tông**: nền `color.tone.<tông>.bg`, viền `.border`, kèm biểu tượng và chữ của tông (thẻ quyền mượn khi khác Bình thường). |
| Quy tắc | Thẻ tông không thay Khối thông báo (CMP-24) khi nội dung là thông báo lỗi hoặc cảnh báo. Thẻ chọn được là nhóm nút chọn: chọn bằng Space hoặc Enter, mũi tên di chuyển `[Web]`. |
| Dùng ở | SCR-SV-01 (thẻ quyền mượn, lối vào); SCR-SV-05 (tóm tắt phiếu); SCR-NV-01, SCR-QT-01 (lối vào); SCR-NV-05 (ba thẻ nguồn giao, chọn được); SCR-NV-06 (thẻ thiết bị đã thêm); SCR-NV-09 (thẻ kiểm tra từng thiết bị); SCR-NV-07, SCR-NV-10 (kết quả vừa lập); SCR-QT-05 (khối từng nhóm), SCR-QT-08 (biểu mẫu). |

#### CMP-16 Dòng danh sách (`ListRow`) `[Android]`

| Mục | Quy định |
|---|---|
| Mục đích | Một mục trong danh sách của SV (thay cho bảng). |
| Cấu tạo | Thẻ tương tác (CMP-15) cao tối thiểu `size.list-row.min`. Dòng đầu: tiêu đề `type.body-strong` (tên loại, mã phiếu, mã biên bản) và CMP-13 bên phải; các dòng sau: thông tin phụ `type.body` `color.text.secondary` (phòng, ngày giờ, số lượng); thời điểm dùng `type.caption`. Cách các thẻ `space.list-gap`. |
| Biến thể | **Điều hướng**: có `chevron_right`, chạm cả thẻ mở chi tiết; dòng không có đích điều hướng (thông báo `NHAC_NHO_NV`) không có `chevron_right` và không bấm được. **Mở rộng**: có CMP-02 `expand_more`/`expand_less` ở góc để xem các khoản theo thiết bị trong chính dòng (SCR-SV-11); vùng mở rộng cách dòng chính bằng đường kẻ `color.border.subtle`. **Có điều khiển**: chứa CMP-10 (SCR-SV-04); không có `chevron_right`; chữ "Hết hàng" thay bộ chọn khi hết hàng. |
| Quy tắc | Chữ dài xuống dòng, không cắt (`UI-Screens.md` 3.9). Tải thêm khi cuộn: chỉ báo ở cuối danh sách khi đang tải thêm; lỗi tải thêm hiển thị dòng "Không tải thêm được" kèm nút "Thử lại" ở cuối danh sách. Phiếu Quá hạn (SCR-SV-09) nhấn mạnh bằng CMP-13 mức đậm; dòng không đổi nền. Danh sách thông báo (SCR-SV-13) chỉ đọc: không có kiểu đã đọc hay chưa đọc, không có nút xóa (`User-Flows.md` 3.8). |
| Dùng ở | SCR-SV-02, SCR-SV-04, SCR-SV-06, SCR-SV-08, SCR-SV-09, SCR-SV-11, SCR-SV-12, SCR-SV-13. |

#### CMP-17 Chip bộ lọc (`FilterChip`) `[Android]`

| Mục | Quy định |
|---|---|
| Mục đích | Hiển thị bộ lọc đang áp dụng và cho xóa từng bộ lọc (`UI-Screens.md` 3.7: bộ lọc đang áp dụng luôn nhìn thấy và có cách xóa). |
| Cấu tạo | Nền `color.surface.selected`, viền `color.border.selected` dày `border.width.default`, chữ `type.label` `color.text.link`, `radius.full`, cao `size.chip.height` (vùng chạm `size.touch.min`), biểu tượng `close` `size.icon.sm` ở cuối. Chữ dạng "Loại: Laptop", "Phòng: tên phòng", "Còn hàng". |
| Quy tắc | Mỗi bộ lọc khác giá trị mặc định có một chip; chạm chip xóa bộ lọc đó và tải lại từ trang 1. Nút "Xóa bộ lọc" (CMP-01 văn bản) chỉ hiện khi có ít nhất một chip. Chip xếp nhiều hàng khi không đủ chỗ. |
| Tiếp cận | Tên truy cập "Xóa bộ lọc <tên bộ lọc>". |
| Dùng ở | SCR-SV-02. |

#### CMP-18 Bảng dữ liệu (`DataTable`) `[Web]`

| Mục | Quy định |
|---|---|
| Mục đích | Hiển thị danh sách có nhiều cột cho NV và QT. |
| Cấu tạo | Khung `color.surface.default`, viền `color.border.subtle`, `radius.md`. Hàng tiêu đề: nền `color.surface.muted`, chữ `type.label` `color.text.secondary`, cố định khi cuộn dọc (`z.sticky`). Dòng: cao tối thiểu `size.table.row`, chữ `type.body`, đường kẻ dưới `color.border.subtle`, đệm ngang `space.3`. |
| Căn lề và định dạng | Chữ căn trái. Số lượng, số tiền căn phải, chữ số cùng độ rộng. Mã (`type.code`), ngày giờ, tiền không xuống dòng. Văn bản dài (mô tả, nội dung thực hiện, lý do) xuống dòng, **không cắt bằng dấu ba chấm** vì thông tin dùng để đối chiếu (BR-80, NFR-07). Cột nhãn trạng thái căn trái. Cột hành động ở cuối, căn phải. |
| Trạng thái dòng | Rê chuột: nền `color.surface.subtle`. Được chọn: `color.surface.selected`. Được làm nổi bật: theo mục 6 (kiểu thông tin cho ĐX-UI-09, kiểu cảnh báo cho khoản không đủ ở SCR-NV-03); kiểu cảnh báo **kèm** chữ nhãn: khoản không đủ có CMP-13 "Không đủ" ở ô số lượng khả dụng. Dòng bấm được (chọn dòng để mở màn hình khác): con trỏ bàn tay, nhận tiêu điểm, Enter mở. |
| Biến thể | **Dòng mở rộng**: ô đầu có CMP-02 `expand_more`/`expand_less` (`aria-expanded`); vùng mở rộng nền `color.surface.subtle`, dòng con cao `size.table.row-sub`. **Cột chọn**: ô tích (CMP-07) ở ô đầu (SCR-QT-11); ô tích trong ô của dòng (SCR-NV-12). **Hành động ở dòng**: tối đa 2 nút văn bản `sm`; từ 3 hành động dùng CMP-20 (ĐX-DS-14). |
| Cuộn | Khi bảng rộng hơn khung, cuộn ngang bên trong khung bảng; trang không cuộn ngang. |
| Trạng thái bảng | Đang tải, rỗng, lỗi: dùng CMP-25 thay phần thân bảng, giữ hàng tiêu đề; rỗng hiển thị thông điệp của màn hình (ví dụ "Không có phiếu yêu cầu chờ duyệt"). |
| Tiếp cận | Là bảng thật (tiêu đề cột gắn với ô); cột chọn và mở rộng có tên truy cập. |
| Dùng ở | SCR-NV-02, SCR-NV-03 (bảng số lượng), SCR-NV-05 (danh sách nguồn), SCR-NV-07, SCR-NV-08, SCR-NV-09 (danh sách thiết bị của phiếu), SCR-NV-10, SCR-NV-11, SCR-NV-12, SCR-NV-13, SCR-NV-14; SCR-QT-02 đến SCR-QT-04, SCR-QT-06, SCR-QT-07, SCR-QT-09 đến SCR-QT-12. |

#### CMP-19 Phân trang (`Pagination`) `[Web]`

| Mục | Quy định |
|---|---|
| Mục đích | Chuyển trang của bảng (`trang`, `kichThuoc`, `tongSo`; ĐX-UI-13). |
| Cấu tạo | Dưới bảng, căn phải: chữ "Hiển thị a–b trên tổng n" (`type.caption`), "Trang x / y", hai nút CMP-01 phụ `sm` "Trước" và "Sau" (biểu tượng `chevron_left`, `chevron_right`). Kích thước trang mặc định 20; không có bộ chọn kích thước trang. |
| Quy tắc | "Trước" vô hiệu ở trang 1, "Sau" vô hiệu ở trang cuối (lý do hiển nhiên từ "Trang x / y"). Đổi bộ lọc thì về trang 1. Ẩn khi `tongSo` không vượt một trang. |
| Dùng ở | Mọi CMP-18 hiển thị danh sách có phân trang từ API (ĐX-UI-13). |

#### CMP-20 Menu thao tác (`Menu`) `[Web]`

| Mục | Quy định |
|---|---|
| Mục đích | Gom hành động ở dòng bảng khi có từ 3 hành động trở lên (ĐX-DS-14). |
| Cấu tạo | Nút kích hoạt là CMP-01 văn bản `sm` với chữ "Thao tác" và biểu tượng `expand_more`; danh sách thả xuống: `color.surface.default`, `radius.md`, `shadow.menu`, `z.dropdown`; mỗi mục cao `size.control.md`, chữ `type.label`. |
| Mục | Hành động hủy bỏ (Xóa, Khóa) dùng chữ `color.tone.danger.text` và đặt cuối danh sách, cách phần còn lại bằng đường kẻ `color.border.subtle`. Mục không áp dụng cho trạng thái hiện tại thì ẩn, không để vô hiệu (ví dụ Khóa/Mở khóa theo trạng thái tài khoản, SCR-QT-09). |
| Bàn phím | Enter hoặc Space mở; phím mũi tên di chuyển; Enter chọn; Esc đóng và trả tiêu điểm về nút kích hoạt. |
| Dùng ở | SCR-QT-04, SCR-QT-07, SCR-QT-09. SCR-QT-02, SCR-QT-03, SCR-QT-06 và các màn hình có tối đa 2 hành động dùng nút văn bản trực tiếp. |

#### CMP-21 Bộ đếm ngược (`Countdown`)

| Mục | Quy định |
|---|---|
| Mục đích | Cho SV biết còn bao lâu để đến quầy trong thời gian giữ chỗ (`hanGiuCho`). |
| Cấu tạo | Khối tông `warning` (nền `color.tone.warning.bg`, viền `.border`, `radius.md`, đệm `space.3`): biểu tượng `schedule`; dòng "Còn lại" `type.caption`; giá trị `type.numeric-lg` màu `color.tone.warning.text`; dòng hạn tuyệt đối "Hạn giữ chỗ: dd/MM/yyyy HH:mm" `type.body`. |
| Định dạng | `mm:ss` khi còn dưới 1 giờ, `HH:mm:ss` khi còn từ 1 giờ trở lên (ĐX-DS-13). Hạn tuyệt đối luôn hiển thị để người dùng không phụ thuộc đồng hồ đếm (`User-Flows.md` UF-HT-01). |
| Khi về 0 | Giá trị thay bằng "Đang cập nhật" kèm chỉ báo quay trong lúc tải lại dữ liệu từ máy chủ; sau đó hiển thị trạng thái do máy chủ trả (Hết hạn hoặc trạng thái khác). Không tự đổi nhãn trạng thái (ĐX-UF-04). |
| Tiếp cận | Không dùng vùng thông báo trực tiếp cho từng giây; hạn tuyệt đối là thông tin chính cho trình đọc màn hình. |
| Dùng ở | SCR-SV-07 (phiếu Đã duyệt), SCR-SV-08 (đăng ký Đã thông báo). Bảng của NV hiển thị hạn tuyệt đối, không có bộ đếm. |

#### CMP-22 Khối đối chiếu (`ComparePanel`) `[Web]`

| Mục | Quy định |
|---|---|
| Mục đích | Đặt tình trạng lúc giao cạnh tình trạng lúc trả của một thiết bị để NV đối chiếu (`UI-Screens.md` SCR-NV-09; UF-NV-03 bước 3). |
| Cấu tạo | Hai cột bằng nhau trong thẻ kiểm tra (CMP-15). **Bên trái, "Lúc giao"**: nền `color.surface.subtle`, chỉ đọc: mô tả tình trạng ban đầu và ảnh (CMP-23). **Bên phải, "Lúc trả"**: nền `color.surface.default`, nhập được: ô mô tả (CMP-04 nhiều dòng), khu đính ảnh (CMP-12), bộ chọn Kết quả kiểm tra (CMP-08), ô chọn Khung phạt (CMP-06). Tiêu đề cột `type.title-card`. |
| Quy tắc | Luôn đặt cạnh nhau ở chiều rộng từ 1024 px (ĐX-DS-07). Khung phạt hiển thị khi kết quả là Hư hỏng hoặc Thiếu phụ kiện, ẩn ở Nguyên vẹn và Mất; ảnh bị vô hiệu ở Mất (`UI-Screens.md` SCR-NV-09). |
| Dùng ở | SCR-NV-09. |

#### CMP-23 Ảnh và xem ảnh lớn (`ImageGallery`, `ImageViewer`)

| Mục | Quy định |
|---|---|
| Mục đích | Hiển thị ảnh thiết bị và ảnh tình trạng; xem ảnh lớn. |
| Dải ảnh | Ô ảnh vuông `size.photo.tile` `[Web]` hoặc `size.photo.thumb` `[Android]`, `radius.md`, ảnh cắt vừa khung; cách nhau `space.2`; `[Android]` SCR-SV-03 là dải cuộn ngang trong khung riêng. Chạm hoặc chọn ảnh mở lớp xem. |
| Không có ảnh | Hình thay thế: nền `color.surface.muted`, biểu tượng `image` màu `color.text.secondary`. |
| Ảnh không tải được | Ô hiển thị biểu tượng `broken_image`, chữ "Không tải được ảnh" và nút văn bản "Tải lại" thực hiện tải lại chi tiết để lấy URL mới (ảnh tình trạng có URL hết hạn sau 10 phút, mục 9.6); không lưu URL. |
| Lớp xem ảnh | Phủ `color.overlay.scrim`, ảnh vừa khung nhìn, `shadow.dialog`, `z.viewer`; nút CMP-02 "Đóng" ở góc trên phải; đóng bằng nút, chạm ra ngoài, phím Esc `[Web]` hoặc nút Quay lại hệ thống `[Android]`. Tên truy cập ảnh: "Ảnh <số thứ tự> của <tên thiết bị>". |
| Dùng ở | SCR-SV-03, SCR-SV-10, SCR-NV-08, SCR-NV-09, SCR-QT-04 (qua CMP-12). |

### 8.5 Phản hồi

#### CMP-24 Khối thông báo (`Banner`)

| Mục | Quy định |
|---|---|
| Mục đích | Thông báo kết quả, cảnh báo, lỗi hoặc thông tin ở đầu màn hình hoặc đầu một khu vực (không phải hộp thoại). |
| Cấu tạo | Nền `color.tone.<tông>.bg`, viền `.border` dày `border.width.default`, `radius.lg`, đệm `space.card`. Biểu tượng `.icon` `size.icon.md` ở đầu; tiêu đề tùy chọn `type.title-card` màu `.text`; nội dung `type.body` `color.text.primary`; danh sách lý do tùy chọn (mỗi lý do một dòng có dấu đầu dòng); hành động tùy chọn (CMP-01 văn bản) ở cuối. |
| Tông (ĐX-DS-16) | `danger`: thao tác bị từ chối do quy tắc, quyền hoặc lỗi hệ thống. `warning`: dữ liệu vừa thay đổi cần tải lại, cảnh báo cần chú ý (kể cả cảnh báo ở ĐX-UI-14 và cảnh báo giờ quầy chưa thiết lập ở SCR-QT-08), hạn chờ rồi thử lại. `info`: thông tin, phiên hết hạn, khối sáu ý "vừa gửi" ở SCR-SV-07. `success`: kết quả thành công hoặc kết luận hợp lệ. `neutral`: không dùng cho khối thông báo. |
| Quy tắc | Không tự đóng và không có nút đóng; biến mất khi nguyên nhân đã giải quyết hoặc khi rời màn hình. Một khu vực tối đa một khối thông báo cùng tông; nhiều khối thông báo xếp dọc cách `space.2`. Khi khối thông báo xuất hiện do hành động của người dùng, cuộn tới khối thông báo và thông báo cho trình đọc màn hình. Không dùng thanh thông báo nổi (toast) vì nguồn không yêu cầu (NT-07). |
| Biến thể | **Kết luận**: tiêu đề là nhãn ở mục 7.3 (Hợp lệ, Không hợp lệ, Được duyệt, Không được duyệt) và danh sách lý do (`thongDiep`). |
| Tiếp cận | Khối thông báo `danger` và `warning` là thông báo khẩn (đọc ngay); `info` và `success` là thông báo thường. |
| Dùng ở | Mọi màn hình có lỗi hoặc cảnh báo; tiêu biểu SCR-CM-01, SCR-CM-02, SCR-SV-05, SCR-SV-07, SCR-SV-14, SCR-NV-03 đến SCR-NV-06, SCR-NV-11, SCR-QT-08. |

#### CMP-25 Khối trạng thái (`StateView`)

| Mục | Quy định |
|---|---|
| Mục đích | Hiển thị trạng thái đang tải, rỗng, lỗi của một vùng dữ liệu (`UI-Screens.md` 3.1). |
| Cấu tạo | Canh giữa vùng, đệm dọc `space.state`, rộng tối đa `size.state.max`. Biểu tượng `size.icon.lg` (hoặc chỉ báo quay) → thông điệp `type.body` → hành động tùy chọn (CMP-01). Màu biểu tượng theo bảng dưới; chữ `color.text.primary` hoặc `color.text.secondary`. |
| Chỉ báo quay | Vòng tròn, cung quay `color.progress.indicator` trên vạch nền `color.progress.track`; `size.icon.lg` (nội tuyến) hoặc `size.control.md` (cả vùng); `[Android]` dùng chỉ báo tiến trình của nền tảng. |

| Loại | Khi nào | Nội dung mặc định | Hành động |
|---|---|---|---|
| Đang tải | Chờ phản hồi | Chỉ báo quay ở đúng vùng; không che vùng đã có dữ liệu | – |
| Tải chậm | Dịch vụ "ngủ dậy" (KT 3.4) | Chỉ báo quay và dòng "Hệ thống đang khởi động, vui lòng chờ"; không báo lỗi sớm | – |
| Rỗng | Danh sách không có dòng | Thông điệp riêng của từng danh sách (ví dụ "Bạn chưa có thông báo nào"); biểu tượng `inbox`, hoặc `search_off` khi rỗng do bộ lọc; tông trung tính, **không** dùng màu lỗi | Theo màn hình (ví dụ "Tạo phiếu mới", "Xóa bộ lọc") |
| Lỗi mạng hoặc quá thời gian chờ | Mất kết nối, hết thời gian chờ | "Không tải được dữ liệu. Kiểm tra kết nối rồi thử lại."; biểu tượng `cloud_off`; dữ liệu đã nhập giữ nguyên | **Thử lại** (nút phụ) |
| Lỗi hệ thống | Lỗi 500 | "Đã xảy ra lỗi hệ thống."; biểu tượng `error` màu `color.tone.danger.icon`; dòng "Mã theo dõi:" kèm `traceId` bằng `type.code`, chọn được để sao chép; không hiển thị chi tiết nội bộ | **Thử lại** (nút phụ) |

| Làm mới dữ liệu | Quy định |
|---|---|
| `[Android]` | Kéo xuống để làm mới ở mọi danh sách và chi tiết (chỉ báo của nền tảng). |
| `[Web]` | Nút "Làm mới" (CMP-01 phụ, biểu tượng `refresh`) ở đầu màn hình có danh sách. |
| Dùng ở | SCR-CM-03 (chỉ báo đang tải toàn màn hình khi khởi động) và mọi màn hình có dữ liệu tải từ máy chủ. |

### 8.6 Lớp phủ

#### CMP-26 Hộp thoại (`Dialog`)

| Mục | Quy định |
|---|---|
| Mục đích | Xác nhận thao tác khó hoàn tác, nhập một vài trường, hoặc báo một kết quả nghiệp vụ. |
| Cấu tạo | Lớp che `color.overlay.scrim` (`z.overlay`); hộp `color.surface.default`, `radius.lg`, `shadow.dialog` (`z.modal`), đệm `space.dialog`. Tiêu đề `type.title-section`; nội dung; hàng nút ở cuối căn phải, cách nhau `space.actions`. Nội dung dài cuộn bên trong, tiêu đề và hàng nút luôn nhìn thấy. |
| Kích thước | `[Web]` `size.dialog.sm` (xác nhận đơn giản) hoặc `size.dialog.md` (liệt kê nhiều dòng hoặc có biểu mẫu); tối đa 90% chiều cao cửa sổ. `[Android]` rộng bằng màn hình trừ lề `space.6` mỗi bên, tối đa `size.dialog.max`. |
| Loại **Xác nhận** | Theo mẫu `UI-Screens.md` 3.5: **Tiêu đề** là tên hành động; **Tóm tắt** đối tượng bị tác động (CMP-14 hoặc đoạn ngắn); **Hệ quả** điều sẽ xảy ra và không đổi được; nhấn mạnh hệ quả quan trọng bằng CMP-24 `warning` ngay trong hộp ("số tiền không đổi sau khi lập", "thiết bị sẽ chuyển Đã báo mất"). **Nút**: Hủy (nút phụ) và nút xác nhận mang tên hành động (nút chính hoặc hủy bỏ theo mục 9.2). |
| Loại **Biểu mẫu** | Tiêu đề, các trường (CMP-04, CMP-06, CMP-07), nút Hủy và nút lưu. Lỗi trường hiển thị cạnh trường, giữ hộp mở. |
| Loại **Kết quả** | Thông tin kết quả (CMP-14 và CMP-13) và hai nút (hành động đi tiếp là nút chính, "Đóng" là nút phụ); dùng cho DLG-SV-01. |
| Hành vi | Loại Xác nhận và Kết quả: tiêu điểm ban đầu ở nút Hủy (hoặc "Đóng"); loại Biểu mẫu: ở trường đầu tiên. Phím Enter không tự xác nhận hành động (ĐX-DS-09). Tiêu điểm bị giữ trong hộp; đóng xong trả tiêu điểm về thành phần đã mở hộp. Esc đóng hộp `[Web]`; nút Quay lại hệ thống `[Android]` tương đương Hủy. **Không** đóng khi bấm vào lớp che, để tránh đóng nhầm giữa lúc đang thao tác. Hộp và lớp che xuất hiện, biến mất bằng chuyển mờ `motion.duration.base` với `motion.easing.standard`. Khi đang chờ phản hồi: nút xác nhận vô hiệu và hiển thị "Đang xử lý"; Hủy, Esc và Quay lại bị khóa cho tới khi có kết quả. |
| Kết quả lỗi | Lỗi nghiệp vụ trả về khi xác nhận hiển thị bằng CMP-24 trong hộp (hoặc ở màn hình sau khi đóng nếu cần tải lại dữ liệu), theo mục 9.4. |
| Tiếp cận | Là hộp thoại phương thức có tiêu đề làm tên truy cập; nội dung phía sau không nhận tiêu điểm. |
| Dùng ở | Hộp xác nhận ở mục 9.2 (SCR-SV-04, SCR-SV-08, SCR-NV-03, SCR-NV-06, SCR-NV-09, SCR-NV-10, SCR-NV-11, SCR-NV-12, SCR-NV-13, SCR-NV-14, SCR-QT-03 đến SCR-QT-11); hộp biểu mẫu (SCR-NV-14 Ghi hoàn thành, SCR-QT-04 Cập nhật giá trị, SCR-QT-11 bảng "Chọn thêm từ danh mục" dùng kích thước `size.dialog.md` chứa bộ lọc, CMP-18 có cột chọn và hai nút Hủy và Thêm vào danh sách); DLG-SV-01. |

### 8.7 Khung điều hướng và tiến trình

#### CMP-27 Thanh điều hướng dưới (`BottomNav`) `[Android]`

| Mục | Quy định |
|---|---|
| Mục đích | Điều hướng giữa bốn màn hình gốc của SV (`UI-Screens.md` 4.1): Trang chủ, Tra cứu, Thông báo, Hồ sơ. |
| Cấu tạo | Cao `size.bottomnav.height`, nền `color.surface.default`, đường kẻ trên `color.border.subtle`. Bốn mục chia đều chiều rộng, mỗi mục: biểu tượng `size.icon.lg` và chữ `type.label-sm` bên dưới (biểu tượng `home`, `search`, `notifications`, `person`). |
| Trạng thái | Mục đang chọn: biểu tượng dạng đặc, chữ và biểu tượng `color.text.link`, nền chỉ báo `color.nav.indicator` dạng `radius.full` sau biểu tượng. Mục khác: `color.text.secondary`. |
| Quy tắc | Chỉ hiện ở màn hình gốc; màn hình chi tiết và biểu mẫu mở chồng lên và không có thanh này. **Không** có huy hiệu số thông báo chưa đọc (`User-Flows.md` 3.8). Vùng chạm mỗi mục tối thiểu `size.touch.min`. |
| Dùng ở | SCR-SV-01, SCR-SV-02, SCR-SV-13, SCR-SV-14. |

#### CMP-28 Thanh tiêu đề (`TopBar`) `[Android]`

| Mục | Quy định |
|---|---|
| Mục đích | Đặt tên màn hình và cho quay lại. |
| Cấu tạo | Cao `size.appbar.height`, nền `color.surface.default`, đường kẻ dưới `color.border.subtle`. Tiêu đề `type.title-section` một dòng. Màn hình chi tiết và biểu mẫu có CMP-02 `arrow_back` ("Quay lại") ở đầu; màn hình gốc không có. |
| Quy tắc | Tiêu đề là tên màn hình trong `UI-Screens.md` rút gọn nếu cần; không chứa thông tin động dài. |
| Dùng ở | Mọi màn hình SV trừ SCR-CM-01 và SCR-CM-03; DLG-SV-01 là hộp thoại nên không có thanh này. |

#### CMP-29 Thanh menu bên (`Sidebar`) `[Web]`

| Mục | Quy định |
|---|---|
| Mục đích | Điều hướng giữa các chức năng của NV hoặc QT (`UI-Screens.md` 4.2, 4.3). |
| Cấu tạo | Rộng `layout.web.sidebar-width`, nền `color.surface.default`, đường kẻ phải `color.border.subtle`, cố định, tự cuộn khi dài. Mục: cao `size.control.md`, chữ `type.label`, đệm ngang `space.4`; không có biểu tượng. Nhóm (QT: Danh mục, Cấu hình) có tiêu đề nhóm `type.caption` `color.text.secondary`. Danh sách mục theo vai trò lấy nguyên từ `UI-Screens.md` 4.2 và 4.3. |
| Trạng thái | Mục đang chọn: nền `color.surface.selected`, chữ `color.text.link`, vạch nhấn `border.width.accent` màu `color.border.selected` ở mép trái. Rê chuột: nền `color.surface.subtle`. |
| Quy tắc | Chỉ liệt kê mục thuộc vai trò (NFR-01; `User-Flows.md` 3.2); không có mục ngoài phạm vi giai đoạn 1. |
| Tiếp cận | Là điều hướng có tên "Menu chính"; mục đang chọn được khai báo là trang hiện tại. |
| Dùng ở | SCR-NV-01 đến SCR-NV-14; SCR-QT-01 đến SCR-QT-12. |

#### CMP-30 Thanh đầu trang (`Header`) `[Web]`

| Mục | Quy định |
|---|---|
| Mục đích | Hiển thị người dùng đang đăng nhập và cho đăng xuất. |
| Cấu tạo | Cao `layout.web.header-height`, nền `color.surface.default`, đường kẻ dưới `color.border.subtle`, `z.sticky`. Bên phải: họ tên (`type.body-strong`), vai trò (`type.caption` `color.text.secondary`, ví dụ "Nhân viên quản lý thiết bị", "Quản trị viên"), nút "Đăng xuất" (CMP-01 văn bản, biểu tượng `logout`). Nội dung bên trái do Wireframes quyết định. |
| Quy tắc | Đăng xuất không có hộp xác nhận (UF-CM-03). |
| Dùng ở | SCR-NV-01 đến SCR-NV-14; SCR-QT-01 đến SCR-QT-12. |

#### CMP-31 Thanh tiến trình các bước (`Stepper`) `[Web]`

| Mục | Quy định |
|---|---|
| Mục đích | Hiển thị vị trí trong luồng Giao thiết bị bốn bước SCR-NV-04 đến SCR-NV-07 (ĐX-UI-01). Nhãn bước do Design-System chốt, rút gọn từ tên màn hình: 1 Thẻ SV, 2 Nguồn giao, 3 Thiết bị, 4 Phiếu mượn. |
| Cấu tạo | Bốn bước nối nhau bằng đường kẻ `color.border.subtle`. Mỗi bước: hình tròn `size.icon.lg` (`radius.full`) và chữ `type.label`. **Đã qua**: hình tròn nền `color.action.primary.bg` chứa dấu `check`, chữ `color.text.link`, bấm được để quay lại. **Hiện tại**: hình tròn nền `color.action.primary.bg` chứa số bước, chữ `color.text.primary` độ đậm `font.weight.bold`. **Chưa tới**: hình tròn viền `color.border.control` chứa số, chữ `color.text.secondary`, không bấm được và không nhận tiêu điểm. |
| Quy tắc | Chỉ cho quay lại các bước đã qua (`UI-Screens.md` SCR-NV-04). Dấu `check` và số phân biệt bước đã qua và hiện tại ngoài màu. |
| Tiếp cận | Bước hiện tại được khai báo là bước hiện tại. |
| Dùng ở | SCR-NV-04, SCR-NV-05, SCR-NV-06, SCR-NV-07. |

---

## 9. Mẫu dùng chung

### 9.1 Biểu mẫu và lỗi trường

| Quy tắc | Nội dung |
|---|---|
| Bố cục | Một cột. Nhãn nằm trên ô; khoảng giữa hai trường `space.field`. Trường bắt buộc có dấu `*`; biểu mẫu có trường bắt buộc ghi chú "* Bắt buộc" ở đầu. `[Web]` hàng nút ở cuối biểu mẫu căn phải, nút chính ở ngoài cùng bên phải. `[Android]` nút chính rộng hết chiều ngang ở cuối. |
| Kiểm tra tại chỗ | Chỉ kiểm tra các giới hạn nguồn đã nêu rõ (ví dụ số nguyên từ 1, ảnh tối đa 5 và 2 MB, đến ngày không sớm hơn từ ngày). Quy tắc nghiệp vụ do máy chủ kiểm tra; client không tự suy diễn (NT-01). |
| Hiển thị lỗi | Lỗi theo `errors` (khóa trường → thông báo) đặt dưới đúng trường (CMP-04 hoặc điều khiển tương ứng); giữ nguyên dữ liệu đã nhập; đưa tiêu điểm tới trường lỗi đầu tiên; lỗi không gắn được với trường nào hiển thị bằng CMP-24 `danger` ở đầu biểu mẫu. Lỗi trùng mã hoặc tên (`MA_THIET_BI_TRUNG`, `TEN_DANG_NHAP_TRUNG`, `MA_NV_TRUNG`) đặt dưới trường tương ứng. |
| Nút gửi | Vô hiệu trong lúc chờ phản hồi và khi thiếu trường bắt buộc mà `UI-Screens.md` nêu (đăng nhập, Xem lại, Lập phiếu mượn). Nếu lý do là trường bắt buộc còn trống và trường đó đang nhìn thấy thì dấu `*` là đủ; các lý do khác phải hiển thị cạnh nút (ĐX-DS-11). |
| Dòng nhập lặp | Danh sách dòng thêm hoặc xóa được (ngoại lệ thời gian theo loại ở SCR-QT-05): mỗi dòng là một hàng các trường (CMP-06, CMP-04) kèm CMP-02 "Xóa dòng" (biểu tượng `close`); nút văn bản "Thêm dòng" ở cuối danh sách; mỗi loại tối đa một dòng nên loại đã chọn ở dòng khác bị ẩn khỏi danh sách chọn. |
| Lưu cả bộ | Biểu mẫu "Tham số hệ thống" tải bộ giá trị hiện tại, sửa, lưu toàn bộ một lần; không có nút lưu từng trường (SCR-QT-08). |
| Ghi chú không hồi tố | Mọi hộp xác nhận lưu cấu hình nêu: giá trị mới chỉ áp dụng cho giao dịch phát sinh sau, phiếu và biên bản đã lập giữ nguyên (SCR-QT-05 đến SCR-QT-08). Riêng SCR-QT-05 nêu thêm việc lưu thay thế toàn bộ quy định của nhóm. |

### 9.2 Hộp xác nhận và kiểu nút xác nhận

Nút xác nhận dùng kiểu **hủy bỏ** khi hành động xóa dữ liệu, ngừng hoặc tắt đối tượng, khóa, hủy hoặc từ chối chứng từ, mất vị trí trong hàng chờ, xóa lựa chọn đã nhập, chuyển thiết bị sang trạng thái không quay lại; các hành động còn lại dùng kiểu **chính**. Danh sách thao tác cần xác nhận theo `User-Flows.md` 3.9 và `UI-Screens.md`:

| Thao tác | Màn hình | Nút xác nhận | Nhấn mạnh hệ quả (ngoài mẫu chung của CMP-26) |
|---|---|---|---|
| Gửi phiếu yêu cầu | SCR-SV-05 | Chính | Màn hình xem lại thay cho hộp xác nhận (ĐX-UF-05); phiếu chuyển Chờ duyệt |
| Rời danh sách; Từ chối nhận | SCR-SV-08 | Hủy bỏ | Không tính vi phạm; lượt chuyển cho người kế tiếp (với Từ chối nhận) |
| Đổi phòng khi đã chọn loại | SCR-SV-04 | Hủy bỏ | Các loại đã chọn sẽ bị xóa (ĐX-UI-06) |
| Duyệt | SCR-NV-03 | Chính | Giữ chỗ (mặc định 30 phút) và thông báo SV |
| Từ chối | SCR-NV-03 | Hủy bỏ | Ô lý do bắt buộc; phiếu chuyển Từ chối, SV được thông báo |
| Lập phiếu mượn | SCR-NV-06 | Chính | Thiết bị chuyển Đang được mượn, giải phóng giữ chỗ, nguồn chuyển Đã nhận; hạn trả do hệ thống tính |
| Ghi kết quả kiểm tra | SCR-NV-09 | Chính; **Hủy bỏ** khi có thiết bị kết quả Mất | Kết quả Mất: thiết bị chuyển Đã báo mất, không đổi được (khối thông báo `danger`) |
| Lập biên bản | SCR-NV-10 | Chính | **Số tiền không đổi sau khi lập** (khối thông báo `warning`); SV chuyển Tạm khóa |
| Lập biên bản mất | SCR-NV-12 | Hủy bỏ | Khoản Mất 100% giá trị hiện hành; thiết bị Đã báo mất; SV Tạm khóa |
| Ghi nhận đã nộp | SCR-NV-11 | Chính | Hệ thống chỉ ghi nhận, không thanh toán trực tuyến |
| Xác nhận đã nhận đủ thiết bị | SCR-NV-11 | Chính | Hệ thống xét mở khóa quyền mượn |
| Chuyển lượt | SCR-NV-13 | Hủy bỏ | Đăng ký chuyển Đã hủy, không tính vi phạm |
| Lập lịch bảo trì; Tiếp nhận sửa chữa | SCR-NV-14 | Chính | Thiết bị chuyển Đang bảo trì/sửa chữa, không còn cho mượn |
| Ghi hoàn thành bảo trì | SCR-NV-14 | Chính | Theo kết quả: về Sẵn sàng hoặc vào danh sách chờ thanh lý |
| Xóa; Ngưng sử dụng; Tắt khung phạt | SCR-QT-03, SCR-QT-04, SCR-QT-07 | Hủy bỏ | Nêu đối tượng bị xóa; với Ngưng sử dụng: vẫn giữ lịch sử, không còn dùng cho thiết bị mới, và hộp không hứa hẹn khả năng hoàn tác (API không có thao tác này); với Tắt khung phạt: khung không còn trong danh sách chọn của NV |
| Lưu cấu hình | SCR-QT-05 đến SCR-QT-08 | Chính | Không hồi tố (mục 9.1); với SCR-QT-05 nêu thêm: lưu thay thế toàn bộ quy định của nhóm (loại không còn trong danh sách ngoại lệ thì mất ngoại lệ) |
| Khóa tài khoản | SCR-QT-09 | Hủy bỏ | Token còn hạn bị từ chối ngay |
| Mở khóa | SCR-QT-09 | Chính | Tài khoản trở lại Hoạt động |
| Đặt lại mật khẩu | SCR-QT-09 | Chính | NV dùng mật khẩu mới (tối thiểu 8 ký tự) |
| Lưu quyền phòng | SCR-QT-09 | Chính | Thay thế toàn bộ danh sách phòng; NV chỉ thấy phòng trong danh sách mới |
| Hủy biên bản | SCR-QT-10 | Hủy bỏ | Hệ thống xét lại mở khóa quyền mượn của SV |
| Lưu điều chỉnh biên bản | SCR-QT-10 | Chính | Tổng tiền do hệ thống tính lại |
| Lập quyết định thanh lý | SCR-QT-11 | Hủy bỏ | Thiết bị chuyển Đã thanh lý, vẫn giữ lịch sử |

Hộp Biểu mẫu của CMP-26 dùng để nhập dữ liệu: "Ghi hoàn thành" nhập ở hộp Biểu mẫu rồi xác nhận ở hộp Xác nhận như bảng trên; "Cập nhật giá trị" (SCR-QT-04) chỉ dùng hộp Biểu mẫu, nút lưu là nút chính.

### 9.3 Trạng thái màn hình

| Quy tắc | Nội dung |
|---|---|
| Đủ trạng thái | Mỗi vùng dữ liệu tải từ máy chủ được thiết kế đủ các trạng thái: đang tải, tải chậm, rỗng, lỗi mạng, lỗi hệ thống (CMP-25), cộng với trạng thái nghiệp vụ riêng của màn hình. |
| Cục bộ | Trạng thái chỉ ảnh hưởng vùng gây ra nó: lỗi tải một vùng phụ không làm mất các vùng và lối vào khác (ĐX-UI-04). Dữ liệu đã hiển thị vẫn giữ nguyên khi làm mới. |
| Rỗng không phải lỗi | Dùng tông trung tính, thông điệp riêng của từng danh sách và hành động gợi ý do màn hình nêu. |
| Chuyển trạng thái do dữ liệu máy chủ | Khi máy chủ báo trạng thái đối tượng đã đổi (`TRANG_THAI_KHONG_HOP_LE`, `XUNG_DOT_CAP_NHAT`), hiển thị khối thông báo `warning`, tải lại và hiển thị giá trị mới; không tự thử lại (mục 9.4). |

### 9.4 Trình bày lỗi theo `maLoi`

Mục này chỉ quy định **thành phần và tông** dùng để trình bày; thông điệp, điều hướng và xử lý sau lỗi theo `UI-Screens.md` 3.2 và bảng lỗi của từng màn hình. Quy tắc chọn tông: lỗi 422 và lỗi từ chối do quyền hoặc hệ thống dùng `danger`; lỗi 409 và lỗi dữ liệu đã đổi dùng `warning`; thông tin phiên dùng `info` (ĐX-DS-16).

| Mã lỗi | Trình bày |
|---|---|
| `CHUA_XAC_THUC` (401) khi làm mới phiên thất bại | Về màn hình đăng nhập; CMP-24 `info` "Phiên đăng nhập đã hết hạn, vui lòng đăng nhập lại" |
| `TAI_KHOAN_BI_KHOA` (403) | CMP-24 `danger` ở màn hình đăng nhập (khi đang dùng thì đăng xuất rồi hiển thị ở màn hình đăng nhập) |
| `KHONG_DU_QUYEN` (403) | CMP-24 `danger` "Bạn không có quyền thực hiện thao tác này"; quay về màn hình chính của vai trò |
| `PHONG_CHUA_CAP_QUYEN` (403) | CMP-24 `danger` "Phòng này chưa được cấp quyền cho bạn"; quay về danh sách đang xem |
| `DU_LIEU_KHONG_HOP_LE` (400) | Lỗi dưới từng trường (mục 9.1); lỗi không gắn trường dùng CMP-24 `danger` |
| `KHONG_TIM_THAY` (404) | CMP-24 `warning` "Không tìm thấy"; quay về danh sách và tải lại |
| `XUNG_DOT_CAP_NHAT`, `TRANG_THAI_KHONG_HOP_LE` (409) | CMP-24 `warning` nêu dữ liệu hoặc trạng thái vừa thay đổi; hiển thị giá trị mới |
| `QUA_NHIEU_YEU_CAU` (429) | CMP-24 `warning` yêu cầu chờ rồi thử lại; không tự lặp |
| Lỗi nghiệp vụ 422 (ví dụ `SV_BI_KHOA_QUYEN_MUON`, `THIET_BI_KHONG_SAN_SANG`, `GIAO_SAU_GIO_DONG_CUA`, `GIA_TRI_CAU_HINH_KHONG_HOP_LE`) | CMP-24 `danger` ở đầu màn hình hoặc khu vực; nếu lỗi gắn với một ô cụ thể thì đặt dưới ô đó (`VUOT_GIOI_HAN_THIET_BI` cạnh các ô số lượng; `VUOT_THOI_GIAN_MUON_TOI_DA` cạnh ô thời gian) |
| Lỗi nghiệp vụ 409 (ví dụ `SO_LUONG_KHA_DUNG_THAY_DOI`, `THIET_BI_DA_DUOC_GIAO`, `GIU_CHO_HET_HAN`, `CHUA_CAU_HINH_GIO_QUAY`) | CMP-24 `warning`; thiết bị hoặc dòng liên quan được đánh dấu bằng trạng thái lỗi của thành phần (không chỉ bằng màu) |
| `DA_CO_LICH_SU_CHI_NGUNG_SU_DUNG`, `KHUNG_PHAT_DA_DUNG_KHONG_XOA` (409) | Hộp xác nhận (CMP-26) đề nghị Ngưng sử dụng hoặc Tắt khung phạt, nêu hệ quả |
| Lỗi 500 | CMP-25 "Lỗi hệ thống" khi tải dữ liệu; CMP-24 `danger` kèm `traceId` (`type.code`) khi lỗi xảy ra lúc thực hiện thao tác |
| Mất kết nối, quá thời gian chờ | CMP-25 "Lỗi mạng" khi tải dữ liệu; CMP-24 `danger` khi thực hiện thao tác ghi, giữ dữ liệu đã nhập và nêu lối kiểm tra trạng thái trước khi gửi lại (ĐX-UF-06) |

### 9.5 Danh sách, bộ lọc, phân trang

| Quy tắc | `[Android]` | `[Web]` |
|---|---|---|
| Cách hiển thị | Dải CMP-16, mỗi dòng một thẻ; không cuộn ngang | CMP-18 trong khung có cuộn ngang nội bộ |
| Tải thêm | Tải trang kế tiếp khi cuộn tới cuối cho đến khi đủ `tongSo`; chỉ báo tải thêm và lỗi tải thêm ở cuối danh sách (CMP-16) | CMP-19 |
| Thứ tự | Do máy chủ quyết định; client không sắp xếp lại; **không có tiêu đề cột bấm để sắp xếp** | Như bên trái |
| Bộ lọc | Ô tìm kiếm (CMP-05), ô chọn (CMP-06), điều khiển phân đoạn (CMP-08); bộ lọc đang áp dụng hiển thị bằng CMP-17 | Thanh bộ lọc nằm trên bảng: các điều khiển ngang hàng (CMP-04, CMP-06, CMP-08), nhãn trên ô, cách nhau `space.3`, xuống hàng khi hết chỗ; giá trị đang áp dụng chính là giá trị hiển thị trong điều khiển |
| Xóa bộ lọc | Nút "Xóa bộ lọc" (CMP-17) | Nút "Xóa bộ lọc" (CMP-01 văn bản) cạnh thanh bộ lọc, chỉ hiện khi có bộ lọc khác mặc định |
| Đổi bộ lọc | Tải lại từ trang 1 | Tải lại từ trang 1 |
| Rỗng | CMP-25 "Rỗng"; rỗng do bộ lọc kèm nút "Xóa bộ lọc" | CMP-25 "Rỗng" trong thân bảng |

### 9.6 Ảnh tình trạng và ảnh danh mục

Quy tắc nghiệp vụ và kỹ thuật (giới hạn, nén, thứ tự tải, URL có chữ ký) giữ nguyên như `UI-Screens.md` 3.6; bảng dưới chỉ nêu phần giao diện.

| Nội dung | Quy định giao diện |
|---|---|
| Giới hạn | Tối đa 5 ảnh mỗi thiết bị mỗi lần, mỗi ảnh tối đa 2 MB, jpeg, png hoặc webp; vi phạm thì báo ngay tại khu đính ảnh (CMP-12). |
| Trạng thái từng ảnh | Đang tải, đã tải, lỗi (kèm Thử lại), có nút Gỡ (CMP-12). |
| Xem ảnh | Ảnh thu nhỏ dùng CMP-23; chạm hoặc chọn để xem lớn. |
| URL hết hạn | Ảnh tình trạng có URL hết hạn sau 10 phút: khi ảnh không tải được, ô ảnh hiển thị lỗi và nút "Tải lại" tải lại chi tiết để lấy URL mới (CMP-23). Ảnh danh mục thiết bị dùng URL công khai. |
| Ảnh tùy chọn, mô tả bắt buộc | Khu đính ảnh ghi "không bắt buộc"; ô mô tả tình trạng có dấu `*` khi bắt buộc (SCR-NV-06; SCR-NV-09 khi kết quả Hư hỏng hoặc Thiếu phụ kiện). |

### 9.7 Định dạng dữ liệu hiển thị

Thời gian, tiền, số ngày trễ, hạn trả và hạn giữ chỗ do máy chủ tính; client chỉ định dạng (NT-01).

| Loại | Định dạng | Ví dụ |
|---|---|---|
| Ngày giờ | `dd/MM/yyyy HH:mm`, 24 giờ, giờ Việt Nam (`Asia/Ho_Chi_Minh`); API trả giờ UTC | 31/10/2026 14:30 |
| Ngày | `dd/MM/yyyy` | 31/10/2026 |
| Giờ (giờ mở, đóng cửa quầy) | `HH:mm` | 07:30 |
| Tiền | Số nguyên, dấu chấm phân cách hàng nghìn, một khoảng trắng không ngắt rồi `VNĐ`; không có phần thập phân; căn phải trong bảng | 240.000 VNĐ |
| Số lượng, số lượt | Số nguyên; chữ số cùng độ rộng | 12 |
| Số tiền tạm tính (ĐX-UI-10) | Như tiền, kèm nhãn "Tạm tính" (mục 7.3); số tiền chính thức sau khi lập biên bản không kèm nhãn | 80.000 VNĐ kèm nhãn Tạm tính |
| Số thiết bị đang mượn so với tối đa | `n/m` kèm nhãn nêu nghĩa | 2/5 |
| Tỷ lệ khung phạt | Số có thể có phần thập phân, kèm `%`, không khoảng trắng; giữ giá trị do API trả, không ép về số nguyên | 15%; 15,5% cho giá trị API `15.5` |
| Thời gian mượn | Số kèm đơn vị | 4 giờ |
| Số ngày trễ | Số kèm "ngày" | 3 ngày |
| Mã (phiếu, thiết bị, SV, biên bản) | `type.code`, không xuống dòng; hiển thị nguyên văn như API trả (định dạng mã chứng từ theo `Backend-Conventions.md` 3.4) | `PM-261024-000123` |
| Người (SV) | Mã số rồi họ tên; lớp ở cột hoặc dòng riêng | `20520001` Nguyễn Văn A (ví dụ minh họa) |
| Giá trị thiếu | "—" hoặc chữ riêng của màn hình | — |
| Thời gian tương đối | **Không dùng** ("5 phút trước"); luôn hiển thị thời điểm cụ thể. Ngoại lệ duy nhất: bộ đếm ngược (CMP-21) | – |

### 9.8 Văn phong và nhãn hành động chuẩn

**Văn phong** (ĐX-DS-10): tiếng Việt có dấu đầy đủ; câu ngắn, chủ động, nêu việc cần làm hoặc kết quả; dùng "Bạn" khi cần xưng hô với người dùng, như các thông điệp mặc định trong `UI-Screens.md`; thông điệp rỗng và thông điệp ở màn hình web không cần xưng hô thì bỏ đại từ; nhãn nút, tiêu đề và thông điệp một câu không có dấu chấm cuối; không dùng chữ in hoa toàn bộ; không hiển thị mã lỗi `maLoi` cho người dùng (chỉ `traceId` ở lỗi hệ thống); dùng đúng thuật ngữ nghiệp vụ như `User-Flows.md` Phụ lục A (ví dụ "Tạm khóa", "Biên bản", "Khung phạt"). Thông điệp mặc định trong `UI-Screens.md` được dùng nguyên văn.

**Nhãn hành động chuẩn.** Nút xác nhận mang tên hành động cụ thể (ví dụ "Duyệt", "Lập phiếu mượn"), không dùng "OK", "Đồng ý" hay "Xác nhận" chung chung. Các nhãn dùng chung:

| Nhãn | Dùng khi |
|---|---|
| Hủy | Đóng hộp xác nhận hoặc biểu mẫu mà không thay đổi dữ liệu |
| Đóng | Đóng hộp chỉ để xem thông tin, hoặc lớp xem ảnh |
| Quay lại | Về màn hình trước (nút biểu tượng `[Android]`; nút chữ khi cần `[Web]`) |
| Thử lại | Sau lỗi mạng, lỗi hệ thống, lỗi tải một vùng |
| Làm mới | Tải lại dữ liệu `[Web]` |
| Lưu | Lưu biểu mẫu thêm, sửa hoặc cấu hình |
| Sửa; Xóa; Ngưng sử dụng; Tắt | Hành động ở dòng danh mục |
| Thêm `<đối tượng>` | Mở biểu mẫu thêm (ví dụ "Thêm phòng", "Thêm loại", "Thêm khung phạt") |
| Gửi; Xem lại; Tiếp tục | Gửi phiếu; sang bước xem lại; sang bước kế tiếp của luồng |
| Xóa bộ lọc | Bỏ mọi bộ lọc |
| Đang xử lý | Chữ trên nút khi đang chờ phản hồi |
| Tải lại | Lấy lại ảnh không tải được |

Các nhãn nghiệp vụ còn lại (Duyệt, Từ chối, Lập phiếu mượn, Ghi kết quả, Lập biên bản, Ghi nhận đã nộp, Chuyển lượt, …) lấy nguyên văn từ `UI-Screens.md`.

**Chữ do Design-System đặt.** Các chuỗi chữ sau không có nguyên văn trong `UI-Screens.md`; chúng được chốt ở đây để mọi màn hình dùng giống nhau (ĐX-DS-10). Nhãn trạng thái phát sinh ở mục 7.3; thông điệp riêng của từng màn hình giữ nguyên văn theo `UI-Screens.md`.

| Chữ | Dùng ở |
|---|---|
| "Đang xử lý" | Chữ trên nút khi chờ phản hồi (CMP-01, CMP-26) |
| "* Bắt buộc"; "Thêm dòng"; "Xóa dòng" | Biểu mẫu (mục 9.1) |
| "Tìm theo tên thiết bị" | Nhãn ô tìm kiếm (CMP-05) |
| "Mã thẻ sinh viên"; "Mã thiết bị"; "Quét hoặc nhập mã, rồi nhấn Enter" | Nhãn và chú thích ô nhập mã quầy (CMP-11) |
| "Thêm ảnh"; "n/5"; "Đang tải"; "Gỡ ảnh"; "Ảnh (không bắt buộc)"; "Kết quả Mất không có ảnh"; "Chỉ nhận ảnh jpeg, png hoặc webp"; "Mỗi ảnh tối đa 2 MB"; "Tối đa 5 ảnh" | Khu đính ảnh (CMP-12) |
| "Không tải được ảnh"; "Tải lại" | Ô ảnh không tải được (CMP-23) |
| "Còn lại"; "Hạn giữ chỗ: dd/MM/yyyy HH:mm"; "Đang cập nhật" | Bộ đếm ngược (CMP-21) |
| "Lúc giao"; "Lúc trả" | Tiêu đề hai cột của khối đối chiếu (CMP-22) |
| "Thao tác" | Nút kích hoạt menu thao tác (CMP-20) |
| "Trước"; "Sau"; "Hiển thị a–b trên tổng n"; "Trang x / y" | Phân trang (CMP-19) |
| "Không tải được dữ liệu. Kiểm tra kết nối rồi thử lại."; "Đã xảy ra lỗi hệ thống."; "Mã theo dõi:" | Khối trạng thái lỗi mạng và lỗi hệ thống (CMP-25) |
| "Không tải thêm được" | Lỗi tải thêm ở cuối danh sách (CMP-16) |
| "Thẻ SV"; "Nguồn giao"; "Thiết bị"; "Phiếu mượn" | Nhãn bước của thanh tiến trình (CMP-31) |
| "Menu chính"; "Quay lại"; "Đóng" | Tên truy cập của thanh menu bên, nút quay lại và nút đóng (CMP-29, CMP-02) |

---

## 10. Khả năng tiếp cận và khả năng dùng

Nguồn nghiệp vụ không có yêu cầu tiếp cận riêng; `UI-Screens.md` 3.8 đặt các yêu cầu tối thiểu. Mục này cụ thể hóa chúng thành yêu cầu kiểm tra được cho thiết kế và mã.

| Yêu cầu | Quy định | Nền tảng |
|---|---|---|
| Tương phản | Chữ tối thiểu 4,5:1; viền điều khiển, vòng tiêu điểm, biểu tượng tối thiểu 3:1 (mục 5.4). Cặp màu mới thêm phải được kiểm tra trước khi dùng. | Cả hai |
| Không chỉ dựa vào màu | Trạng thái, lỗi, nhấn mạnh luôn kèm chữ; mục được chọn kèm dấu tích, viền dày hoặc vạch nhấn; bước đã qua của luồng kèm dấu tích (CMP-13, CMP-24, CMP-31). | Cả hai |
| Nhãn cho ô nhập | Mọi ô nhập có nhãn nhìn thấy, không thay bằng placeholder; lỗi gắn với đúng ô. | Cả hai |
| Phím tắt bàn phím | Mọi điều khiển và hành động dùng được bằng Tab, Shift+Tab, Enter, Space, mũi tên (menu, tab, nhóm chọn); thứ tự Tab theo thứ tự đọc. | Web |
| Ô nhập mã quầy | Có tiêu điểm khi vào màn hình và sau mỗi lần xử lý; Enter xác nhận mã (CMP-11). | Web |
| Hộp thoại | Esc đóng; tiêu điểm vào trong hộp, bị giữ trong hộp, trả về nơi mở khi đóng; Enter không tự xác nhận (CMP-26, ĐX-DS-09). | Web |
| Vòng tiêu điểm | Luôn nhìn thấy khi dùng bàn phím; không bị cắt bởi khung chứa (mục 6). | Web |
| Vùng chạm | Điều khiển tối thiểu 48 × 48 dp, kể cả khi phần nhìn thấy nhỏ hơn (chip, nút biểu tượng, ô tích); khoảng giữa hai vùng chạm liền nhau không bị chồng lấn. | Android |
| Cỡ chữ hệ thống | Mọi chữ dùng `sp`; không cố định chiều cao khung chứa chữ (dùng chiều cao tối thiểu); chữ dài xuống dòng; không vỡ bố cục ở hệ số 1,3 (ĐX-DS-07). | Android |
| Tên truy cập | Nút biểu tượng, ảnh, ô tích, bộ chọn số lượng có tên truy cập tiếng Việt; biểu tượng trang trí ẩn với trình đọc màn hình. | Cả hai |
| Thông báo cho trình đọc màn hình | Khối thông báo `danger` và `warning` xuất hiện do hành động của người dùng, lỗi trường và kết quả tra cứu ở ô nhập mã được thông báo ngay; trạng thái "đang tải" được khai báo là đang bận; bộ đếm ngược không thông báo từng giây (CMP-21). | Web; tương đương trên Android |
| Ngôn ngữ | Trang web khai báo ngôn ngữ tiếng Việt để trình đọc màn hình phát âm đúng. | Web |
| Không phụ thuộc rê chuột | Mọi thông tin và hành động có thể truy cập không cần rê chuột (không có nội dung chỉ hiện khi rê). | Web |
| Bảng | Tiêu đề cột gắn với ô; mở rộng và chọn dòng có tên truy cập (CMP-18). | Web |
| Giảm chuyển động | Tôn trọng cài đặt hệ điều hành (mục 4.5). | Cả hai |

---

## 11. Kích thước màn hình và khung bố cục

Mục này chỉ quy định khung chung và kích thước; bố cục chi tiết từng màn hình do Wireframes quyết định (mục 1.2).

### 11.1 Android (SV)

| Nội dung | Quy định |
|---|---|
| Chiều rộng tham chiếu để thiết kế và kiểm tra | 320 dp, 360 dp, 412 dp (ĐX-DS-07). Bố cục co giãn, không có chiều rộng cố định cho khối nội dung. |
| Cỡ chữ hệ thống | Kiểm tra ở mặc định và hệ số 1,3. |
| Lề và cột | Lề trái, phải `space.gutter` (16 dp); một cột; các khu vực cách nhau `space.section`. |
| Khung | Thanh tiêu đề `size.appbar.height` ở trên (CMP-28); thanh điều hướng dưới `size.bottomnav.height` ở dưới, chỉ ở màn hình gốc (CMP-27); nội dung cuộn dọc ở giữa. Khi bàn phím ảo mở, vùng nhập liệu cuộn để trường đang nhập không bị che. |
| Cuộn ngang | Không dùng, trừ dải ảnh (CMP-23). |
| Màn hình đăng nhập | Không có thanh điều hướng dưới; nội dung canh giữa theo chiều dọc, lề `space.6`. |

### 11.2 Web (NV, QT)

| Token | Giá trị | Dùng cho |
|---|---|---|
| `layout.web.min-width` | 1024 | Chiều rộng cửa sổ tối thiểu được hỗ trợ (ĐX-DS-07). Dưới mức này trang cuộn ngang và không được hỗ trợ chính thức |
| `layout.web.header-height` | 56 | Chiều cao thanh đầu trang (CMP-30) |
| `layout.web.sidebar-width` | 240 | Chiều rộng thanh menu bên (CMP-29) |
| `layout.web.content-max` | 1440 | Chiều rộng tối đa vùng nội dung; rộng hơn thì canh giữa |
| `layout.web.form-max` | 640 | Chiều rộng tối đa của biểu mẫu một cột |
| `layout.web.login-width` | 400 | Chiều rộng khung đăng nhập |

| Nội dung | Quy định |
|---|---|
| Khung chung | Thanh đầu trang chạy suốt chiều ngang ở trên; thanh menu bên ở bên trái, dưới thanh đầu trang; vùng nội dung ở bên phải, cuộn dọc độc lập, đệm `space.gutter`. Màn hình đăng nhập (SCR-CM-02) và khởi động (SCR-CM-03) không có hai thanh này. |
| Khung trang | Từ trên xuống: tiêu đề trang (`type.title-screen`) cùng hành động chính và nút Làm mới ở bên phải; khối thông báo (nếu có); thanh bộ lọc (nếu có); nội dung; phân trang. Các khu vực cách nhau `space.section`. |
| Màn hình nhiều bước | Thanh tiến trình (CMP-31) nằm dưới tiêu đề trang. |
| Cuộn | Trang không cuộn ngang; bảng và khối rộng cuộn ngang trong khung riêng (CMP-18). Hàng tiêu đề bảng và thanh đầu trang cố định khi cuộn dọc. |
| Màn hình đăng nhập | Khung đăng nhập canh giữa cả hai chiều, rộng `layout.web.login-width`, nền `color.surface.default` với viền `color.border.subtle` và `radius.lg`; nền trang `color.surface.subtle`. |

---

## 12. Tiêu chí chấp nhận giao diện

Danh mục dùng để kiểm thử chấp nhận phần giao diện theo Design-System. Mỗi tiêu chí kiểm tra được bằng quan sát, công cụ đo hoặc rà soát mã.

| Mã | Tiêu chí | Cách kiểm tra |
|---|---|---|
| TC-01 | Màu, cỡ chữ, khoảng cách, bo góc, viền, đổ bóng trong mã đều lấy từ token; không có giá trị thô ngoài lớp nguyên thủy | Rà soát mã |
| TC-02 | Các cặp màu ở mục 5.4 đạt tỷ lệ yêu cầu khi đo trên giao diện thực | Công cụ đo tương phản |
| TC-03 | Mỗi mã ở mục 7.2 hiển thị đúng nhãn, tông và mức nhấn; mã chưa biết hiển thị nguyên mã, không gây lỗi màn hình | Kiểm thử với dữ liệu mẫu |
| TC-04 | Nhãn trạng thái, lỗi và nhấn mạnh vẫn hiểu được khi hiển thị thang xám | Chụp màn hình thang xám |
| TC-05 | Android: không vỡ bố cục, không cắt chữ ở chiều rộng 320, 360, 412 dp và hệ số cỡ chữ 1,0 và 1,3 | Kiểm tra trên máy ảo hoặc thiết bị |
| TC-06 | Android: mọi điều khiển có vùng chạm tối thiểu 48 × 48 dp | Công cụ kiểm tra tiếp cận của Android |
| TC-07 | Web ở 1024 px trở lên: trang không cuộn ngang; bảng rộng cuộn trong khung | Kiểm tra trình duyệt |
| TC-08 | Web: thao tác hoàn toàn bằng bàn phím; vòng tiêu điểm luôn thấy; Esc đóng hộp thoại; Enter ở ô mã quầy xử lý mã; Enter không xác nhận hộp thoại | Kiểm tra thủ công |
| TC-09 | Mọi thao tác ở mục 9.2 có hộp xác nhận đủ tiêu đề, tóm tắt, hệ quả và dùng đúng kiểu nút xác nhận | Kiểm thử chấp nhận |
| TC-10 | Mỗi vùng dữ liệu có đủ trạng thái đang tải, tải chậm, rỗng, lỗi mạng, lỗi hệ thống | Kiểm thử với mạng chậm, mất mạng, dữ liệu rỗng |
| TC-11 | Ngày giờ, ngày, tiền và giá trị thiếu hiển thị đúng mục 9.7 ở mọi màn hình | Rà soát giao diện |
| TC-12 | Hành động bị vô hiệu do nghiệp vụ luôn có lý do hiển thị cạnh đó | Kiểm thử chấp nhận |
| TC-13 | Khu đính ảnh báo ngay khi vượt giới hạn, có đủ bốn trạng thái ảnh; ảnh không tải được có đường tải lại | Kiểm thử với ảnh vượt giới hạn và URL hết hạn |
| TC-14 | Chữ tiếng Việt có dấu hiển thị đúng ở mọi nền tảng (ví dụ "Nguyễn Thị Hương", "Thiếu phụ kiện", "Đã báo mất") | Kiểm tra bằng dữ liệu có dấu thanh và chữ ơ, ư, ă, â, ê, ô, đ |
| TC-15 | Khi hệ điều hành bật giảm chuyển động, không còn chuyển động trang trí | Kiểm tra thủ công |

---

## Phụ lục A. Bộ biểu tượng

Material Symbols, kiểu Outlined, độ đậm nét 400 (ĐX-DS-05). `[Web]` nhúng từng biểu tượng dạng SVG; `[Android]` dùng vector drawable. Cách nạp do Frontend-Architecture quyết định. Biểu tượng bổ trợ chữ; biểu tượng của nút biểu tượng (CMP-02) có tên truy cập.

| Nhóm | Biểu tượng | Dùng ở |
|---|---|---|
| Tông | `check_circle` | Tông `success` (CMP-13, CMP-24) |
| | `info` | Tông `info` |
| | `warning` | Tông `warning` |
| | `error` | Tông `danger`; dòng lỗi dưới ô nhập; dòng vi phạm kèm nhãn `HET_HAN`; lỗi hệ thống |
| Điều hướng | `home`, `search`, `notifications`, `person` | CMP-27 (dạng đặc khi đang chọn); `search` còn dùng ở CMP-05 |
| | `arrow_back` | CMP-28 |
| | `chevron_right` | CMP-15, CMP-16 (chuyển màn hình); CMP-19 nút "Sau" |
| | `chevron_left` | CMP-19 nút "Trước" |
| | `expand_more`, `expand_less` | CMP-06, CMP-16 và CMP-18 (mở rộng), CMP-20 |
| | `logout` | CMP-30 |
| Hành động | `close` | CMP-02 (Đóng, xóa nội dung), CMP-17, CMP-23 |
| | `refresh` | Nút Làm mới `[Web]` |
| | `add`, `remove` | CMP-10 |
| | `check` | Mục đang chọn (CMP-06, CMP-08), bước đã qua (CMP-31) |
| | `add_a_photo` | CMP-12 |
| | `qr_code_scanner` | CMP-11 |
| | `download` | Nút Xuất Excel, Xuất CSV (SCR-QT-12) |
| Trạng thái | `schedule` | CMP-21 |
| | `inbox`, `search_off` | CMP-25 rỗng |
| | `cloud_off` | CMP-25 lỗi mạng |
| | `image`, `broken_image` | CMP-23 |

---

## Phụ lục B. Các điểm được giao cho Design-System và nơi giải quyết

| Điểm được giao | Nguồn | Giải quyết ở |
|---|---|---|
| Màu sắc, kiểu chữ, kích thước, kiểu thành phần | `User-Flows.md` 1.2; `UI-Screens.md` 1.2 | Mục 4, 5, 8 |
| Một thành phần nhãn dùng chung; quy tắc phân biệt trạng thái | `UI-Screens.md` 3.3 | Mục 7, CMP-13 |
| Kích thước vùng chạm (Android) | `UI-Screens.md` 3.8 | ĐX-DS-08, mục 4.7, mục 10 |
| Kích thước màn hình web tối thiểu | `UI-Screens.md` 3.9 | ĐX-DS-07, mục 11.2 |
| Chữ chính thức của thông điệp | `UI-Screens.md` 1.4 | ĐX-DS-10, mục 7.3, mục 9.8 |
| Cách trình bày lỗi (cạnh trường, banner, hộp thoại) | `UI-Screens.md` 3.2 | Mục 9.4 |
| Mẫu hộp xác nhận | `UI-Screens.md` 3.5 | CMP-26, mục 9.2 |
| Thanh tiến trình bốn bước | `UI-Screens.md` ĐX-UI-01 | CMP-31 |
| Bộ lọc đang áp dụng luôn nhìn thấy và có cách xóa | `UI-Screens.md` 3.7 | CMP-17, mục 9.5 |

**Các điểm thuộc Frontend-Architecture và Frontend-Conventions** (không giải quyết ở tài liệu này): khung web và thư viện thành phần; cách nạp phông và biểu tượng; tổ chức tệp token; độ trễ gõ tìm kiếm (ĐX-UI-05); thời gian chờ tối đa của client (`UI-Screens.md` 3.1); cách tiếp nhận kết quả quét mã; cách web khôi phục phiên sau khi tải lại trang.

---

## Phụ lục C. Quy tắc ánh xạ token sang nền tảng

| Nền tảng | Quy tắc | Ví dụ |
|---|---|---|
| Web (biến CSS) | Tiền tố `--ds-`; dấu chấm đổi thành gạch ngang; tên giữ nguyên chữ thường và gạch ngang sẵn có | `color.blue.600` → `--ds-color-blue-600`; `space.4` → `--ds-space-4`; `color.action.primary.bg-hover` → `--ds-color-action-primary-bg-hover` |
| Android (tài nguyên) | Tiền tố `ds_`; dấu chấm và gạch ngang đổi thành gạch dưới; chữ thường. Màu ở tệp màu, kích thước và cỡ chữ ở tệp kích thước (`dp` và `sp`) | `color.blue.600` → `ds_color_blue_600`; `space.4` → `ds_space_4` (dp); `font.size.14` → `ds_font_size_14` (sp) |
| Vai trò kiểu chữ | Một nhóm gồm cỡ, chiều cao dòng, độ đậm, họ phông: `[Web]` ba biến `-size`, `-line`, `-weight` kèm họ phông; `[Android]` một kiểu chữ (text appearance) | `type.body` → `--ds-type-body-size`, `--ds-type-body-line`, `--ds-type-body-weight` |
| Độ mờ | `color.overlay.scrim`: CSS `rgba(22, 26, 33, 0.5)`; Android `#80161A21` (ARGB). `color.overlay.pressed`: CSS `rgba(22, 26, 33, 0.12)`; Android `#1F161A21` | – |
| Tham chiếu | Token ngữ nghĩa tham chiếu token nguyên thủy, không lặp giá trị: `[Web]` `var(--ds-color-blue-600)`; `[Android]` `@color/ds_color_blue_600` | `--ds-color-action-primary-bg: var(--ds-color-blue-600);` |
| Token riêng nền tảng | Chỉ khai báo ở nền tảng tương ứng (`[Web]` `z.*`, `layout.web.*`, `size.control.*`; `[Android]` `size.touch.*`, `size.input.*`) | – |
