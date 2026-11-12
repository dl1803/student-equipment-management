# WIREFRAMES

## Hệ thống quản lý mượn trả thiết bị học tập cho sinh viên

| Mục | Nội dung |
|---|---|
| Dự án | SE100 – Nhóm 13 |
| Tài liệu | `Wireframes.md` |
| Phiên bản | 1.0 |
| Ngày lập | 02/11/2026 |
| Tài liệu căn cứ | `UI-Screens.md` v1.0 (31/10/2026); `User-Flows.md` v1.0 (30/10/2026); `SRS.md` (22/10/2026), chỉ dùng yêu cầu FR-12 |

## Mục lục

1. Giới thiệu
2. Khung bố cục dùng chung
3. Mẫu bố cục dùng chung
4. Wireframe màn hình dùng chung
5. Wireframe màn hình của sinh viên (Android)
6. Wireframe màn hình của nhân viên quản lý thiết bị (web)
7. Wireframe màn hình của quản trị viên (web)
- Phụ lục A. Truy vết màn hình → khung và mẫu bố cục
- Phụ lục B. Truy vết mẫu bố cục → màn hình

---

## 1. Giới thiệu

### 1.1 Mục đích và đối tượng sử dụng

Tài liệu xác định **bố cục** của từng màn hình giai đoạn 1: các vùng trên màn hình, thứ tự và vị trí tương đối của thành phần, và quan hệ giữa các vùng. Wireframe là sơ đồ khối độ trung thực thấp, không phải thiết kế hoàn chỉnh. Tài liệu là đầu vào cho Design-System (kiểu thành phần), Frontend-Architecture (cấu trúc màn hình và điều hướng), Frontend-Conventions và cho kiểm thử chấp nhận giao diện.

| Người đọc | Dùng tài liệu để |
|---|---|
| Thiết kế giao diện | Dựng thiết kế chi tiết trên đúng cấu trúc vùng; không đổi thứ tự hoặc quan hệ giữa các vùng khi chưa thống nhất với tài liệu này. |
| Lập trình frontend (Android, web) | Biết màn hình gồm những vùng nào, vùng nào cuộn, vùng nào cố định, hộp thoại nào chồng lên màn hình nào. |
| Kiểm thử | Kiểm tra sự có mặt và vị trí của từng thành phần, hộp thoại và trạng thái thay đổi bố cục. |

### 1.2 Ranh giới với các tài liệu khác

| Tài liệu | Quan hệ với tài liệu này |
|---|---|
| `UI-Screens.md` | Nguồn trực tiếp về **nội dung**: danh sách màn hình, mã màn hình, thành phần, trường dữ liệu, nhãn, trạng thái, lỗi, điều hướng. Tài liệu này giữ nguyên mã `SCR-…`, `DLG-…` và tên thành phần; chỉ bổ sung **bố cục**. Nội dung đã có ở UI-Screens (trường nguồn dữ liệu, bảng mã lỗi, quy tắc hiển thị) không lặp lại ở đây; mỗi màn hình dẫn về mục tương ứng của UI-Screens. |
| `User-Flows.md` | Nguồn của luồng, thứ tự thao tác và nhãn trạng thái (Phụ lục A của User-Flows). Wireframe không đổi luồng. |
| Design-System | Quyết định màu sắc, kiểu chữ, kích thước, kiểu thành phần, kích thước màn hình web tối thiểu. Tài liệu này không đề cập. |
| Frontend-Architecture, Frontend-Conventions | Quyết định tổ chức mã, đường dẫn, cách tiếp nhận kết quả quét. Tài liệu này chỉ nêu cấu trúc màn hình mà kiến trúc phải thể hiện. |

Các tài liệu Design-System, Frontend-Architecture và Frontend-Conventions chưa được cung cấp khi lập tài liệu này; tài liệu này không dựa vào nội dung của chúng.

**Văn bản hiển thị.** Nhãn nút, tiêu đề và thông điệp trong wireframe là chữ của `UI-Screens.md` (hoặc rút gọn từ đó để vừa khung vẽ); ý nghĩa không đổi. Nhãn trạng thái bắt buộc theo Phụ lục A của `User-Flows.md`. Dữ liệu mẫu trong wireframe (tên, mã, số) chỉ để minh họa vị trí.

### 1.3 Ký hiệu

| Ký hiệu | Ý nghĩa |
|---|---|
| `┌─┐ │ └─┘` | Khung màn hình, vùng hoặc thẻ |
| `[ Nhãn ]` | Nút |
| `[ ________ ]` | Ô nhập |
| `[ Chọn ▾ ]` | Danh sách chọn |
| `[x]` / `[ ]` | Ô tích đã chọn / chưa chọn |
| `(•)` / `( )` | Lựa chọn một đã chọn / chưa chọn |
| `‹Nhãn›` | Nhãn trạng thái (thành phần nhãn dùng chung, `UI-Screens.md` 3.3) |
| `<Liên kết>` | Liên kết hoặc dòng chọn được để mở màn hình khác |
| `▓▓▓` | Ảnh hoặc dải ảnh |
| `◌` | Chỉ báo đang tải |
| `⚠` | Banner hoặc cảnh báo nổi bật |
| `‹` ở thanh ứng dụng | Nút Quay lại (Android) |
| `▣ / ▢` | Mục thanh điều hướng đang chọn / không chọn (Android) |
| `▶` | Mục menu bên đang chọn (web) |
| `…` | Dòng hoặc thẻ cùng kiểu lặp lại |
| `(cuộn)` | Vùng cuộn được |
| `(ghim)` | Vùng cố định, không cuộn theo nội dung |

Mọi wireframe (ĐX-WF-01) vẽ theo màn hình điện thoại dọc (Android) hoặc màn hình máy tính (web) ở trạng thái có dữ liệu; chỉ vẽ thêm biến thể khi bố cục thay đổi. Trạng thái chung (đang tải, rỗng, lỗi mạng, lỗi hệ thống) theo mẫu ở mục 3, màn hình chỉ ghi phần riêng.

### 1.4 Quyết định bổ sung của tài liệu (ĐX-WF)

Những điểm nguồn không nêu mà bố cục cần quyết định. Các quyết định này không thêm chức năng, màn hình hay quy tắc nghiệp vụ.

| Mã | Quyết định | Căn cứ |
|---|---|---|
| ĐX-WF-01 | Tài liệu dùng sơ đồ khối bằng văn bản, không quy định kích thước điểm ảnh, màu, kiểu chữ hay điểm ngắt màn hình. | `UI-Screens.md` 1.2 và 3.9: kích thước và kiểu do Design-System quyết định |
| ĐX-WF-02 | Ứng dụng Android: thanh ứng dụng ở đầu màn hình; thanh điều hướng bốn mục ở **cuối** màn hình, chỉ hiện ở màn hình gốc (Trang chủ, Tra cứu, Thông báo, Hồ sơ). Màn hình chi tiết và biểu mẫu mở chồng lên, có nút Quay lại ở thanh ứng dụng và không có thanh điều hướng (ngoại lệ: SCR-SV-14, xem ĐX-WF-14). | ĐX-UI-02, `UI-Screens.md` 4.1 |
| ĐX-WF-03 | Web NV và QT: thanh đầu trang ở trên; menu bên ở **bên trái**; vùng nội dung ở bên phải. Họ tên, vai trò và nút Đăng xuất ở góc phải thanh đầu trang. | ĐX-UI-03, `UI-Screens.md` 4.2, 4.3 |
| ĐX-WF-04 | Hộp xác nhận và hộp thoại kết quả hiển thị ở giữa màn hình, che nội dung phía sau (hộp thoại chặn). Nút **Hủy** ở bên trái, nút xác nhận mang tên hành động ở bên phải. | `UI-Screens.md` 3.5, ĐX-UF-05 |
| ĐX-WF-05 | Biểu mẫu thêm hoặc sửa ở web quản trị: ít trường dùng hộp thoại biểu mẫu ở giữa màn hình; riêng biểu mẫu thiết bị (nhiều trường, có ảnh) dùng ngăn trượt từ bên phải, giữ danh sách phía sau. | `UI-Screens.md` 7.2 đến 7.9 (số trường từng biểu mẫu) |
| ĐX-WF-06 | Thông báo lỗi nghiệp vụ toàn màn hình (banner) đặt ở **đầu vùng nội dung**, ngay dưới thanh ứng dụng hoặc thanh đầu trang; lỗi theo trường đặt ngay dưới ô nhập tương ứng. | `UI-Screens.md` 3.2 |
| ĐX-WF-07 | Ở màn hình Android có biểu mẫu hoặc hành động chốt (Xem lại, Gửi), nút hành động chính nằm ở cuối màn hình và cố định (ghim) khi nội dung cuộn. | Thực hành thiết kế điện thoại (nút chính ở vùng ngón cái); `UI-Screens.md` 3.8 chỉ quy định kích thước vùng chạm, không quy định vị trí |
| ĐX-WF-08 | Ở web, bộ lọc nằm ngay trên bảng; nút tạo mới (Thêm …) nằm cùng hàng với bộ lọc ở phía phải; phân trang nằm dưới bảng; hành động theo dòng nằm ở cột cuối của bảng. | `UI-Screens.md` 3.7 |
| ĐX-WF-09 | Ở SCR-NV-09, các thẻ kiểm tra của từng thiết bị xếp dọc, mỗi thiết bị một thẻ; trong mỗi thẻ, hai cột cạnh nhau theo yêu cầu của nguồn (trái: tình trạng và ảnh lúc giao; phải: phần nhập lúc trả). | `UI-Screens.md` 6.9 (cạnh nhau, bên trái, bên phải); FR-12 (`SRS.md`): hiển thị song song lúc giao và lúc trả |
| ĐX-WF-10 | Ba nguồn giao ở SCR-NV-05 là ba thẻ chọn nằm ngang ở đầu vùng nội dung; nội dung của nguồn đang chọn hiện ngay bên dưới; chỉ một nguồn đang chọn tại một thời điểm. | `UI-Screens.md` 6.5 (ba thẻ chọn nguồn) |
| ĐX-WF-11 | Hai tab của SCR-NV-14 và ba tab của khối tìm phiếu ở SCR-NV-08 là hàng tab nằm trên bảng, dưới bộ lọc. | `UI-Screens.md` 6.14, 6.8 |
| ĐX-WF-12 | Màn hình không có mục riêng ở menu bên đánh dấu mục gốc của luồng: SCR-NV-03 → Phiếu yêu cầu chờ duyệt; SCR-NV-05, 06, 07 → Giao thiết bị; SCR-NV-09 và SCR-NV-10 → Nhận trả thiết bị. | `UI-Screens.md` 4.2 (menu chỉ có mục gốc); 2.3 (điều hướng) |
| ĐX-WF-13 | Khi một dòng của bảng có từ ba hành động trở lên, các hành động gộp vào menu có nút `[ Thao tác ▾ ]` ở cột cuối; tối đa hai hành động hiển thị trực tiếp. | `Design-System.md` ĐX-DS-14, CMP-18, CMP-20; `UI-Screens.md` 7.9 |
| ĐX-WF-14 | SCR-SV-14 mở từ nút **Xem lý do** (SCR-SV-01) hoặc ở chế độ "bị chặn" (từ SCR-SV-03, 04, 05) có thêm nút Quay lại ở thanh ứng dụng để về màn hình trước; mở từ thanh điều hướng thì không có nút Quay lại. Thanh điều hướng luôn hiện vì SCR-SV-14 là màn hình gốc. | `UI-Screens.md` 4.1, 5.14 |
| ĐX-WF-15 | Ở SCR-SV-02, ô tìm và các bộ lọc nằm trong vùng cuộn, phía trên danh sách kết quả (cuộn cùng danh sách, không ghim). | `UI-Screens.md` 5.2 (không quy định vị trí); điện thoại ưu tiên diện tích danh sách |
| ĐX-WF-16 | Chi tiết biên bản ở SCR-QT-10 là khối nằm ngay dưới bảng, cùng màn hình, hiện khi chọn một dòng. | `UI-Screens.md` 7.10 ("mở khi chọn một dòng") |
| ĐX-WF-17 | Bảng web có nhiều cột (SCR-QT-12) cuộn ngang trong khung bảng; trang không cuộn ngang. | `UI-Screens.md` 7.12 (số cột của báo cáo) |
| ĐX-WF-18 | Ở SCR-NV-05, nguồn Mượn trực tiếp dùng chính bộ lọc Phòng chung ở đầu màn hình để chọn phòng; không vẽ ô chọn phòng thứ hai. | `UI-Screens.md` 6.5 (bộ lọc Phòng áp dụng cho cả ba nguồn) |
| ĐX-WF-19 | Hộp thoại mà nguồn không quy định tên nút (bảng chọn thiết bị và hộp quyết định vừa lập ở SCR-QT-11) có nút điều khiển chuẩn **Đóng**, và bảng chọn có thêm nút xác nhận lựa chọn **Thêm vào danh sách**; đây là điều khiển của hộp thoại, không phải chức năng nghiệp vụ mới. | `UI-Screens.md` 3.8 (hộp thoại đóng được bằng phím Esc); 7.11 (bảng chọn, danh sách đã chọn) |
| ĐX-WF-20 | Nút **Làm mới** nằm ở hàng tiêu đề, bên phải tên màn hình, của mọi màn hình web có danh sách hoặc bảng làm nội dung chính (SCR-NV-02, 11, 12, 13, 14; SCR-QT-02 đến 07, 09, 10, 11). Màn hình quét, biểu mẫu nhập, kết quả thao tác, trang chủ và báo cáo không có nút này. | `UI-Screens.md` 3.1 (nút Làm mới trên web), ĐX-UF-04 |

---

## 2. Khung bố cục dùng chung

### 2.1 Khung ứng dụng Android (sinh viên)

Áp dụng cho mọi màn hình `SCR-SV-xx` (`UI-Screens.md` 4.1; ĐX-WF-02). Màn hình gốc có thanh điều hướng; màn hình chi tiết thì không.


```text
┌────────────────────────────────────────────┐ ┌────────────────────────────────────────────┐
│  Tiêu đề màn hình                          │ │‹ Tiêu đề màn hình                          │
├────────────────────────────────────────────┤ ├────────────────────────────────────────────┤
│ (vùng nội dung, cuộn)                      │ │ (vùng nội dung, cuộn)                      │
│                                            │ │                                            │
│ …                                          │ │ …                                          │
├────────────────────────────────────────────┤ └────────────────────────────────────────────┘
│▣Trang chủ ▢Tra cứu   ▢Thông báo ▢Hồ sơ     │                                               
└────────────────────────────────────────────┘                                               
```

Trái: màn hình gốc (SCR-SV-01, SCR-SV-02, SCR-SV-13, SCR-SV-14). Phải: màn hình chi tiết hoặc biểu mẫu (các màn hình SV còn lại).

| Vùng | Vị trí | Ghi chú |
|---|---|---|
| Thanh ứng dụng | Trên cùng (ghim) | Tiêu đề màn hình; nút Quay lại ở bên trái khi không phải màn hình gốc |
| Vùng nội dung | Giữa, cuộn dọc | Kéo xuống để làm mới (ĐX-UF-04); tải thêm khi cuộn tới cuối danh sách (ĐX-UI-13) |
| Thanh điều hướng | Dưới cùng (ghim) | Bốn mục: Trang chủ, Tra cứu, Thông báo, Hồ sơ; không có huy hiệu số thông báo |
| Hộp thoại | Giữa màn hình, chồng lên | Mẫu P-03 |
| Dữ liệu dài | Trong vùng nội dung | Tên thiết bị, mô tả xuống dòng, không tràn ngang (`UI-Screens.md` 3.9); mọi bố cục dùng được trên các cỡ màn hình điện thoại phổ biến |

Nút Đăng xuất không nằm ở khung mà ở SCR-SV-14 (`UI-Screens.md` 4.1).

### 2.2 Khung web của nhân viên

Áp dụng cho mọi màn hình `SCR-NV-xx` (`UI-Screens.md` 4.2; ĐX-WF-03).


```text
┌──────────────────────────────────────────────────────────────────────────────────────────────────────────┐
│  Quản lý mượn trả thiết bị                              Họ tên · Nhân viên quản lý thiết bị  [Đăng xuất] │
├──────────────────────────────┬───────────────────────────────────────────────────────────────────────────┤
│▶ Trang chủ                   │ (vùng nội dung)                                                           │
│  Phiếu yêu cầu chờ duyệt     │                                                                           │
│  Giao thiết bị               │ …                                                                         │
│  Nhận trả thiết bị           │                                                                           │
│  Biên bản và thu phạt        │                                                                           │
│  Phiếu chưa trả quá 30 ngày  │                                                                           │
│  Danh sách chờ               │                                                                           │
│  Bảo trì và sửa chữa         │                                                                           │
│                              │                                                                           │
└──────────────────────────────┴───────────────────────────────────────────────────────────────────────────┘
```

| Vùng | Vị trí | Ghi chú |
|---|---|---|
| Thanh đầu trang | Trên cùng (ghim) | Tên hệ thống; họ tên và vai trò (Nhân viên quản lý thiết bị); nút **Đăng xuất** |
| Menu bên | Trái (ghim) | Tám mục theo thứ tự ở hình; mục của màn hình đang mở được đánh dấu `▶`; màn hình không có mục riêng đánh dấu mục gốc của luồng (ĐX-WF-12) |
| Vùng nội dung | Phải, cuộn dọc | Tiêu đề màn hình ở đầu vùng, kèm nút **Làm mới** ở màn hình danh sách (ĐX-WF-20); banner lỗi (ĐX-WF-06) ngay dưới tiêu đề |
| Hộp thoại | Giữa màn hình, chồng lên | Mẫu P-03 |

Menu bên không có mục ngoài vai trò NV.

### 2.3 Khung web của quản trị viên

Áp dụng cho mọi màn hình `SCR-QT-xx` (`UI-Screens.md` 4.3; ĐX-WF-03).


```text
┌──────────────────────────────────────────────────────────────────────────────────────────────────────────┐
│  Quản lý mượn trả thiết bị                                           Họ tên · Quản trị viên  [Đăng xuất] │
├──────────────────────────────┬───────────────────────────────────────────────────────────────────────────┤
│▶ Trang chủ                   │ (vùng nội dung)                                                           │
│  Danh mục                    │                                                                           │
│    Phòng thiết bị            │ …                                                                         │
│    Loại thiết bị             │                                                                           │
│    Thiết bị                  │                                                                           │
│  Cấu hình                    │                                                                           │
│    Quy định mượn             │                                                                           │
│    Phí trả trễ               │                                                                           │
│    Khung phạt                │                                                                           │
│    Tham số hệ thống          │                                                                           │
│  Tài khoản nhân viên         │                                                                           │
│  Biên bản vi phạm            │                                                                           │
│  Thanh lý                    │                                                                           │
│  Báo cáo                     │                                                                           │
│                              │                                                                           │
└──────────────────────────────┴───────────────────────────────────────────────────────────────────────────┘
```

| Vùng | Vị trí | Ghi chú |
|---|---|---|
| Thanh đầu trang | Trên cùng (ghim) | Họ tên và vai trò (Quản trị viên); nút **Đăng xuất** |
| Menu bên | Trái (ghim) | Hai nhóm có tiêu đề **Danh mục** và **Cấu hình** (tiêu đề nhóm không chọn được) và các mục đơn lẻ, theo thứ tự ở hình |
| Vùng nội dung | Phải, cuộn dọc | Như khung NV (tiêu đề, nút **Làm mới** ở màn hình danh sách, banner lỗi) |

Menu bên không có mục duyệt, giao, nhận, lập biên bản, bảo trì và không có mục xem nhật ký.

---

## 3. Mẫu bố cục dùng chung

Mỗi mẫu có mã `P-nn` để các màn hình tham chiếu. Màn hình chỉ vẽ lại mẫu khi có thay đổi riêng.

**Trạng thái chung của mọi màn hình** (`UI-Screens.md` 3.1; áp dụng cho cả Android và web, màn hình chỉ ghi phần riêng):

| Trạng thái | Vị trí và cách bố trí |
|---|---|
| Đang tải | Chỉ báo `◌` đặt ở đúng vùng đang chờ, không che các vùng đã có dữ liệu |
| Tải chậm do dịch vụ khởi động | Giữ chỉ báo tải, kèm dòng "Hệ thống đang khởi động, vui lòng chờ" ngay dưới chỉ báo |
| Rỗng | Thông điệp rỗng của màn hình đặt ở vùng của danh sách; không phải lỗi |
| Lỗi mạng hoặc quá thời gian chờ | Thông báo lỗi kèm nút **Thử lại**; dữ liệu đã nhập giữ nguyên |
| Lỗi hệ thống | Thông báo lỗi hệ thống chung kèm `traceId` |

### 3.1 P-01 Danh sách Android

Dùng cho danh sách của SV (SCR-SV-02, 06, 08, 09, 11, 12, 13).


```text
┌────────────────────────────────────────────┐ ┌────────────────────────────────────────────┐
│  Tiêu đề                                   │ │  Tiêu đề                                   │
├────────────────────────────────────────────┤ ├────────────────────────────────────────────┤
│ Danh sách (cuộn, tải thêm)                 │ │                                            │
│ ┌─────────────────────────────┐            │ │       (thông điệp rỗng)                    │
│ │ Dòng 1: tiêu đề      ‹Nhãn› │            │ │       [ Hành động gợi ý ]                  │
│ │ Dòng phụ · thời gian        │            │ ├────────────────────────────────────────────┤
│ └─────────────────────────────┘            │ │▣Trang chủ ▢Tra cứu   ▢Thông báo ▢Hồ sơ     │
│ ┌──────────┐                               │ └────────────────────────────────────────────┘
│ │ Dòng 2 … │                               │                                               
│ └──────────┘                               │                                               
│ ◌ đang tải thêm khi cuộn tới cuối          │                                               
├────────────────────────────────────────────┤                                               
│▣Trang chủ ▢Tra cứu   ▢Thông báo ▢Hồ sơ     │                                               
└────────────────────────────────────────────┘                                               
```

```text
┌────────────────────────────────────────────┐
│  Tiêu đề                                   │
├────────────────────────────────────────────┤
│ ⚠ Lỗi mạng hoặc quá thời gian chờ          │
│   [ Thử lại ]                              │
│ (dữ liệu đã nhập được giữ nguyên)          │
├────────────────────────────────────────────┤
│▣Trang chủ ▢Tra cứu   ▢Thông báo ▢Hồ sơ     │
└────────────────────────────────────────────┘
```

Trái: có dữ liệu. Phải: rỗng (thông điệp và hành động gợi ý riêng của từng danh sách). Dưới: lỗi mạng. Các trạng thái đang tải, tải chậm và lỗi hệ thống theo bảng ở đầu mục 3.

### 3.2 P-02 Bảng web có bộ lọc và phân trang

Dùng cho mọi danh sách dạng bảng của NV và QT; màn hình dùng mẫu này được liệt kê ở Phụ lục B.


```text
┌────────────────────────────────────────────────────────────────────────────────────────────┐
│  Quản lý mượn trả thiết bị                             Họ tên · Quản trị viên  [Đăng xuất] │
├──────────────────────────────┬─────────────────────────────────────────────────────────────┤
│  Trang chủ                   │ Tiêu đề màn hình   [ Làm mới ]                              │
│  Danh mục                    │ ⚠ (banner lỗi, khi có)                                      │
│▶   Phòng thiết bị            │ Bộ lọc: [ Lọc 1 ▾ ] [ Lọc 2 ▾ ] [ Xóa bộ lọc ]   [ + Thêm ] │
│    Loại thiết bị             │ ┌───────┬───────┬───┬───────────┐                           │
│    Thiết bị                  │ │ Cột 1 │ Cột 2 │ … │ Hành động │                           │
│  Cấu hình                    │ ├───────┼───────┼───┼───────────┤                           │
│    Quy định mượn             │ │ …     │ …     │ … │ [ Sửa ]   │                           │
│    Phí trả trễ               │ │ …     │ …     │ … │ [ Sửa ]   │                           │
│    Khung phạt                │ └───────┴───────┴───┴───────────┘                           │
│    Tham số hệ thống          │ Tổng số: …        ‹ 1 2 3 ›                                 │
│  Tài khoản nhân viên         │                                                             │
│  Biên bản vi phạm            │                                                             │
│  Thanh lý                    │                                                             │
│  Báo cáo                     │                                                             │
│                              │                                                             │
└──────────────────────────────┴─────────────────────────────────────────────────────────────┘
```

| Vùng | Quy tắc bố cục |
|---|---|
| Hàng tiêu đề | Tên màn hình và nút **Làm mới** (web, ĐX-UF-04) |
| Hàng bộ lọc | Bộ lọc đang áp dụng luôn nhìn thấy và có cách xóa (nút **Xóa bộ lọc**); nút tạo mới ở phía phải (ĐX-WF-08) |
| Bảng | Cột cuối là hành động theo dòng; dòng chọn được khi màn hình ghi |
| Chân bảng | Tổng số (`tongSo`) và phân trang (ĐX-UI-13); đổi bộ lọc thì về trang 1 |
| Rỗng | Bảng được thay bằng thông điệp rỗng của màn hình, bộ lọc vẫn hiện |
| Lỗi | Banner ở đầu vùng nội dung kèm **Thử lại**; bộ lọc đã chọn giữ nguyên |

Chân bảng (tổng số và phân trang) áp dụng cho mọi bảng danh sách của NV và QT theo ĐX-UI-13, kể cả khi hình của màn hình lược bỏ để gọn. Không áp dụng cho bảng nằm trong khối chi tiết hoặc kết quả (bảng số lượng ở SCR-NV-03, bảng khoản đề xuất ở SCR-NV-10, bảng của SCR-QT-12, nguồn không nêu phân trang cho các bảng này).

### 3.3 P-03 Hộp xác nhận (DLG xác nhận)

Cấu trúc chung của `UI-Screens.md` 3.5; ĐX-WF-04.


```text
┌──────────────────────────────────────────────┐
│ Tiêu đề: tên hành động                       │
│                                              │
│ Tóm tắt: đối tượng bị tác động               │
│ Hệ quả: điều sẽ xảy ra, không thể đổi        │
│ [ ô nhập bổ sung, chỉ khi màn hình yêu cầu ] │
│                                              │
│ [ Hủy ]                    [ Tên hành động ] │
└──────────────────────────────────────────────┘
```

Phím Esc (web) đóng hộp khi chưa gửi yêu cầu. Khi đang chờ phản hồi, nút xác nhận vô hiệu và hiển thị đang xử lý; Hủy, Esc và nút Quay lại hệ thống (Android) bị khóa cho tới khi có kết quả (`Design-System.md` CMP-26). Nội dung riêng từng hộp ghi ở màn hình sở hữu.

### 3.4 P-04 Hộp thoại biểu mẫu và ngăn biểu mẫu (web)

ĐX-WF-05. Hộp thoại biểu mẫu: tiêu đề, các trường theo thứ tự `UI-Screens.md`, lỗi dưới từng trường, nút **Hủy** và nút **Lưu** ở chân. Ngăn biểu mẫu (chỉ thiết bị): cùng cấu trúc, cuộn dọc ở giữa, chân cố định.


```text
┌─────────────────────────┐ ┌──────────────────────────────┐
│ Thêm / Sửa …            │ │ Thêm / Sửa thiết bị      [✕] │
│ Nhãn trường 1 *         │ │ (trường, cuộn)               │
│ [ ______________ ]      │ │ …                            │
│   (lỗi cạnh trường)     │ │ …                            │
│ Nhãn trường 2 *         │ │ ──────────────────           │
│ [ ______________ ]      │ │ [ Hủy ]       [ Lưu ]        │
│                         │ └──────────────────────────────┘
│ [ Hủy ]         [ Lưu ] │                                 
└─────────────────────────┘                                 
```

Dấu `*` chỉ trường bắt buộc. Biểu mẫu phòng, loại, thiết bị, tạo tài khoản và sửa tài khoản lưu trực tiếp; biểu mẫu cấu hình (SCR-QT-05 đến SCR-QT-08) có thêm bước xác nhận khi lưu (mẫu P-03).

### 3.5 P-05 Khu đính ảnh tình trạng

`UI-Screens.md` 3.6. Dùng ở SCR-NV-06, SCR-NV-09, SCR-QT-04.


```text
┌─────────────────────────────────────────────────────────────────────────────────┐
│ Ảnh (không bắt buộc) · tối đa 5 ảnh, mỗi ảnh ≤ 2 MB (jpeg, png, webp)           │
│ [ + Thêm ảnh ]                                                                  │
│ ▓▓▓ ảnh 1 ‹đã tải› [Gỡ]   ▓▓▓ ảnh 2 ◌ đang tải   ▓▓▓ ảnh 3 ‹lỗi› [Thử lại] [Gỡ] │
│ ⚠ (lỗi vượt giới hạn hiện ngay tại khu này)                                     │
└─────────────────────────────────────────────────────────────────────────────────┘
```

Chạm hoặc chọn ảnh để xem lớn: ảnh phóng to hiển thị chồng lên màn hình, đóng được bằng phím Esc (web). Mỗi ảnh có trạng thái đang tải, đã tải, lỗi (kèm Thử lại) và nút Gỡ.

### 3.6 P-06 Ô nhập mã tại quầy

`UI-Screens.md` mục 6 (đoạn "Nhập mã tại quầy") và 3.8. Dùng ở SCR-NV-04, SCR-NV-06, SCR-NV-08, SCR-NV-09, SCR-NV-11.


```text
┌───────────────────────────────────────┐
│ Mã thẻ sinh viên / Mã thiết bị        │
│ [ ▌quét hoặc nhập tay ______ ]        │
│ (lỗi cạnh ô, mã đã nhập được giữ lại) │
└───────────────────────────────────────┘
```

Ô nhập có tiêu điểm khi mở màn hình và sau mỗi lần xử lý xong một mã; phím Enter xác nhận mã (`UI-Screens.md` 3.8).

### 3.7 P-07 Hồ sơ sinh viên (khối)

Dùng ở SCR-NV-04 và SCR-NV-11.


```text
┌───────────────────────────────────────────────────┐
│ Mã SV · Họ tên · Lớp · Khoa                       │
│ Nhóm: …   Hạn thẻ: dd/MM/yyyy   Học: ‹Đang học›   │
│ Quyền mượn: ‹Bình thường›   Đang mượn: n / tối đa │
└───────────────────────────────────────────────────┘
```

Ở SCR-NV-11 khối chỉ có họ tên, lớp, nhóm, trạng thái quyền mượn và số thiết bị đang mượn trên số tối đa (không có mã số, khoa, hạn thẻ, trạng thái học); ở SCR-NV-04 có đủ các trường ở hình (`UI-Screens.md` 6.4, 6.11).

### 3.8 P-08 Thanh tiến trình giao thiết bị

Dùng ở SCR-NV-04 đến SCR-NV-07 (ĐX-UI-01); đặt ở đầu vùng nội dung, dưới tiêu đề.


```text
① Thẻ SV ──── ② Nguồn giao ──── ③ Thiết bị ──── ④ Phiếu mượn
(bước đang mở được nhấn mạnh; chỉ bấm quay lại được các bước đã qua)
```


---

## 4. Wireframe màn hình dùng chung

Ba màn hình không nằm trong khung điều hướng của vai trò nào (`UI-Screens.md` 4.4 đến 4.6).


### SCR-CM-01 Đăng nhập sinh viên

Nội dung: `UI-Screens.md` 4.4. Khung: riêng, không có thanh điều hướng.

```text
┌────────────────────────────────────────────┐
│  Đăng nhập                                 │
├────────────────────────────────────────────┤
│                                            │
│      HỆ THỐNG MƯỢN TRẢ THIẾT BỊ            │
│                                            │
│ ⚠ Phiên đăng nhập đã hết hạn, vui lòng     │
│   đăng nhập lại   (chỉ khi phiên hết hạn)  │
│                                            │
│ Mã số sinh viên *                          │
│ [ ______________________________ ]         │
│ Mật khẩu *                                 │
│ [ ●●●●●●●● ____________________ ]          │
│   (lỗi: Thông tin đăng nhập không đúng)    │
│                                            │
│ [ Đăng nhập ]                    (ghim)    │
└────────────────────────────────────────────┘
```

| Vùng | Quy tắc bố cục |
|---|---|
| Thông báo phiên hết hạn | Đầu vùng nội dung, chỉ hiện khi chuyển về từ phiên hết hạn |
| Biểu mẫu | Hai ô nhập xếp dọc; lỗi đặt dưới ô mật khẩu; nút **Đăng nhập** ở cuối, vô hiệu khi thiếu trường hoặc đang chờ |
| Không có | Thanh điều hướng; liên kết đăng ký, quên mật khẩu, đổi mật khẩu |


### SCR-CM-02 Đăng nhập nhân viên hoặc quản trị viên

Nội dung: `UI-Screens.md` 4.5. Khung: riêng, không có thanh điều hướng.

```text
┌──────────────────────────────────────────────────────────────────────┐
│                                                                      │
│                 HỆ THỐNG MƯỢN TRẢ THIẾT BỊ                           │
│                                                                      │
│   ┌────────────────────────────────────────────────────────────┐     │
│   │ ⚠ Phiên đăng nhập đã hết hạn, vui lòng đăng nhập lại       │     │
│   │    (chỉ khi phiên hết hạn; lỗi đăng nhập hiện cùng vị trí) │     │
│   │                                                            │     │
│   │ Tên đăng nhập *                                            │     │
│   │ [ ____________________________________ ]                   │     │
│   │ Mật khẩu *                                                 │     │
│   │ [ ●●●●●●●● ___________________________ ]                   │     │
│   │                                                            │     │
│   │ [ Đăng nhập ]                                              │     │
│   └────────────────────────────────────────────────────────────┘     │
│                                                                      │
└──────────────────────────────────────────────────────────────────────┘
```

Thẻ đăng nhập đặt giữa màn hình, không có khung điều hướng. Một biểu mẫu chung cho NV và QT; không có liên kết đăng ký, quên hoặc đổi mật khẩu. Lỗi `TAI_KHOAN_BI_KHOA` hiển thị thông báo dùng chung ở cùng vị trí banner.


### SCR-CM-03 Khởi động và khôi phục phiên

Nội dung: `UI-Screens.md` 4.6. Khung: riêng, không có thanh điều hướng.

```text
┌────────────────────────────────────────────┐ ┌──────────────────────────────────────────────┐
│  Khởi động                                 │ │                                              │
├────────────────────────────────────────────┤ │                                              │
│                                            │ │                                              │
│                                            │ │           ◌   Đang kiểm tra phiên…           │
│                                            │ │                                              │
│         ◌                                  │ │                                              │
│    Đang kiểm tra phiên…                    │ └──────────────────────────────────────────────┘
│                                            │                                                 
│                                            │                                                 
└────────────────────────────────────────────┘                                                 
```

Màn hình chỉ có chỉ báo đang tải ở giữa; không hiển thị tạm màn hình đăng nhập hay màn hình chính. Trái: Android; phải: web. Không có thanh điều hướng, thanh đầu trang hay menu bên.

---

## 5. Wireframe màn hình của sinh viên (Android)

Mọi màn hình trong mục này theo khung 2.1. Màn hình vẽ có thanh điều hướng ở cuối là màn hình gốc; các màn hình còn lại có nút Quay lại ở thanh ứng dụng. Mọi danh sách và chi tiết có thao tác kéo để làm mới.


### SCR-SV-01 Trang chủ sinh viên

Nội dung: `UI-Screens.md` 5.1. Khung: mục 2.1, màn hình gốc.

```text
┌────────────────────────────────────────────┐ ┌────────────────────────────────────────────┐
│  Trang chủ                                 │ │  Trang chủ                                 │
├────────────────────────────────────────────┤ ├────────────────────────────────────────────┤
│ Xin chào, Nguyễn Văn A                     │ │ Xin chào, Nguyễn Văn A                     │
│ ┌─ Thẻ quyền mượn ──────────┐              │ │ ┌─ Thẻ quyền mượn ─────────────────┐       │
│ │ Quyền mượn: ‹Bình thường› │              │ │ │ ‹Tạm khóa›  (nổi bật)            │       │
│ │ Đang mượn: 1 / 3 thiết bị │              │ │ │ Bạn chưa thể đặt trước hoặc đăng │       │
│ └───────────────────────────┘              │ │ │ ký chờ                           │       │
│                                            │ │ │ [ Xem lý do ]                    │       │
│ ┌──────────────────┐ ┌──────────────────┐  │ │ └──────────────────────────────────┘       │
│ │ Tra cứu          │ │ Đặt trước        │  │ │                                            │
│ │ thiết bị         │ │ thiết bị         │  │ │ (tám lối vào như bên trái)                 │
│ └──────────────────┘ └──────────────────┘  │ │ (vẫn dùng được khi thẻ lỗi)                │
│ ┌──────────────────┐ ┌──────────────────┐  │ ├────────────────────────────────────────────┤
│ │ Phiếu yêu cầu    │ │ Đăng ký chờ      │  │ │▣Trang chủ ▢Tra cứu   ▢Thông báo ▢Hồ sơ     │
│ │ của tôi          │ │ của tôi          │  │ └────────────────────────────────────────────┘
│ └──────────────────┘ └──────────────────┘  │                                               
│ ┌──────────────────┐ ┌──────────────────┐  │                                               
│ │ Lịch sử          │ │ Biên bản và      │  │                                               
│ │ mượn/trả         │ │ phí phạt         │  │                                               
│ └──────────────────┘ └──────────────────┘  │                                               
│ ┌──────────────────┐ ┌──────────────────┐  │                                               
│ │ Vi phạm          │ │ Hồ sơ mượn       │  │                                               
│ │                  │ │                  │  │                                               
│ └──────────────────┘ └──────────────────┘  │                                               
├────────────────────────────────────────────┤                                               
│▣Trang chủ ▢Tra cứu   ▢Thông báo ▢Hồ sơ     │                                               
└────────────────────────────────────────────┘                                               
```

Trái: quyền mượn Bình thường. Phải: quyền mượn khác Bình thường (thẻ đổi sang dạng cảnh báo, nút **Xem lý do** → SCR-SV-14).

| Vùng | Quy tắc bố cục |
|---|---|
| Lời chào | Đầu vùng nội dung |
| Thẻ quyền mượn | Ngay dưới lời chào; khi lỗi tải hồ sơ, thẻ hiển thị lỗi và **Thử lại**, các lối vào bên dưới vẫn dùng được (ĐX-UI-04) |
| Lối vào chức năng | Lưới hai cột, tám ô theo thứ tự: Tra cứu thiết bị, Đặt trước thiết bị, Phiếu yêu cầu của tôi, Đăng ký chờ của tôi, Lịch sử mượn/trả, Biên bản và phí phạt, Vi phạm, Hồ sơ mượn |
| Không có | Huy hiệu số thông báo chưa đọc |


### SCR-SV-02 Tra cứu thiết bị

Nội dung: `UI-Screens.md` 5.2. Khung: mục 2.1, màn hình gốc. Mẫu bố cục: P-01.

```text
┌────────────────────────────────────────────┐ ┌────────────────────────────────────────────┐
│  Tra cứu thiết bị                          │ │  Tra cứu thiết bị                          │
├────────────────────────────────────────────┤ ├────────────────────────────────────────────┤
│ [ Tìm theo tên loại thiết bị ______ ]      │ │ [ Tìm … ]  [ Loại ▾ ] [ Phòng ▾ ]          │
│ [ Loại ▾ Tất cả ]  [ Phòng ▾ Tất cả ]      │ │ [ Tình trạng hàng ▾ ]  [ Xóa bộ lọc ]      │
│ [ Tình trạng hàng ▾ Tất cả ]               │ │                                            │
│ [ Xóa bộ lọc ]  (chỉ khi có bộ lọc)        │ │                                            │
│                                            │ │    Không tìm thấy thiết bị phù hợp         │
│ ┌─────────────────────────────────┐        │ │    [ Xóa bộ lọc ]                          │
│ │ Laptop Dell 5420 · Dell         │        │ ├────────────────────────────────────────────┤
│ │ Phòng A1 · Cơ sở 1       Còn: 5 │        │ │▢Trang chủ ▣Tra cứu   ▢Thông báo ▢Hồ sơ     │
│ └─────────────────────────────────┘        │ └────────────────────────────────────────────┘
│ ┌─────────────────────────────────┐        │                                               
│ │ Máy chiếu Epson · Epson         │        │                                               
│ │ Phòng B2 · Cơ sở 2   ‹Hết hàng› │        │                                               
│ └─────────────────────────────────┘        │                                               
│ ◌ tải thêm khi cuộn                        │                                               
├────────────────────────────────────────────┤                                               
│▢Trang chủ ▣Tra cứu   ▢Thông báo ▢Hồ sơ     │                                               
└────────────────────────────────────────────┘                                               
```

Trái: có kết quả. Phải: rỗng. Ô tìm và ba bộ lọc nằm trên danh sách và **cuộn cùng danh sách** (ĐX-WF-15); chọn một dòng mở SCR-SV-03. Mỗi dòng gồm tên loại, hãng, tên phòng và cơ sở, số lượng khả dụng; dòng có số lượng 0 mang nhãn **Hết hàng**. Lỗi tải riêng danh sách bộ lọc: bộ lọc đó ở trạng thái lỗi kèm **Thử lại**, danh sách kết quả vẫn dùng được.


### SCR-SV-03 Chi tiết loại thiết bị tại phòng

Nội dung: `UI-Screens.md` 5.3. Khung: mục 2.1.

```text
┌────────────────────────────────────────────┐ ┌────────────────────────────────────────────┐
│‹ Chi tiết thiết bị                         │ │‹ Chi tiết thiết bị                         │
├────────────────────────────────────────────┤ ├────────────────────────────────────────────┤
│ ▓▓▓ ▓▓▓ ▓▓▓  (dải ảnh, vuốt ngang)         │ │ ▓▓▓ ▓▓▓ ▓▓▓  (hoặc hình thay thế)          │
│ Laptop Dell 5420 · Dell                    │ │ Máy chiếu Epson · Epson                    │
│ Phòng A1 · Cơ sở 1                         │ │ Phòng B2 · Cơ sở 2                         │
│ Mô tả: …  (ẩn khi không có)                │ │ Mô tả: …                                   │
│ Thông số kỹ thuật:  (ẩn khi rỗng)          │ │ Thông số kỹ thuật: …                       │
│   · …                                      │ │ Số lượng khả dụng: 0  ‹Hết hàng›           │
│ Số lượng khả dụng: 5                       │ │ Ngày dự kiến có sẵn: dd/MM/yyyy HH:mm      │
│                                            │ │   (hoặc "Chưa có ngày dự kiến")            │
│ ─────────────────────────────────          │ │ ─────────────────────────────────          │
│ [ Đặt trước ]                    (ghim)    │ │ [ Đăng ký chờ ]                  (ghim)    │
└────────────────────────────────────────────┘ └────────────────────────────────────────────┘
```

Trái: còn hàng (nút **Đặt trước**). Phải: hết hàng (nút **Đăng ký chờ**, ngày dự kiến có sẵn). Chỉ một trong hai nút hiện tại một thời điểm, ở cuối màn hình (ĐX-WF-07). Khi SV bị chặn, chọn nút mở SCR-SV-14 ở chế độ "bị chặn"; khi đang chờ kết quả Đăng ký chờ, nút vô hiệu và hiển thị đang xử lý; kết quả thành công hiển thị DLG-SV-01 chồng lên màn hình này.


### SCR-SV-04 Soạn phiếu yêu cầu mượn

Nội dung: `UI-Screens.md` 5.4. Khung: mục 2.1. Mẫu bố cục: P-03.

```text
┌────────────────────────────────────────────┐
│‹ Đặt trước thiết bị                        │
├────────────────────────────────────────────┤
│ Phòng thiết bị *                           │
│ [ Phòng A1 · Cơ sở 1 ▾ ]                   │
│ Muốn mượn ở phòng khác thì gửi phiếu       │
│ khác.                                      │
│                                            │
│ ┌─────────────────────────────────┐        │
│ │ Laptop Dell 5420 · Dell · Còn 5 │        │
│ │ Số lượng:  [ − ]  1  [ + ]      │        │
│ └─────────────────────────────────┘        │
│ ┌──────────────────────────────────────┐   │
│ │ Máy chiếu Epson · Epson · ‹Hết hàng› │   │
│ │ (vô hiệu)       <Đăng ký chờ>        │   │
│ └──────────────────────────────────────┘   │
│ Thời gian mượn dự kiến (giờ) *             │
│ [ 2 ____ ] giờ                             │
│   (lỗi cạnh ô khi có)                      │
│ ─────────────────────────────────          │
│ [ Xem lại ]                      (ghim)    │
└────────────────────────────────────────────┘
```

| Vùng | Quy tắc bố cục |
|---|---|
| Chọn phòng | Đầu màn hình, kèm ghi chú một phiếu thuộc một phòng. Đổi phòng khi đã chọn loại thì hiển thị hộp xác nhận (mẫu P-03, ĐX-UI-06) |
| Danh sách loại của phòng | Xếp dọc dưới ô chọn phòng, cuộn và tải thêm; mỗi loại là một thẻ có bộ chọn số lượng; thẻ hết hàng bị vô hiệu và có liên kết **Đăng ký chờ** |
| Thời gian mượn dự kiến | Dưới danh sách loại |
| Nút **Xem lại** | Ghim ở cuối; chỉ bật khi phòng, ít nhất một loại và thời gian đã hợp lệ |
| Phòng không có loại nào | Thay danh sách loại bằng thông báo "phòng này chưa có thiết bị để đặt" |


### SCR-SV-05 Xem lại và gửi phiếu yêu cầu

Nội dung: `UI-Screens.md` 5.5. Khung: mục 2.1.

```text
┌────────────────────────────────────────────┐
│‹ Xem lại phiếu                             │
├────────────────────────────────────────────┤
│ ⚠ (banner lỗi nghiệp vụ, khi có)           │
│                                            │
│ ┌─ Tóm tắt phiếu ───────────────┐          │
│ │ Phòng: A1 · Cơ sở 1           │          │
│ │ Loại và số lượng:             │          │
│ │   · Laptop Dell 5420  × 1     │          │
│ │ Thời gian mượn dự kiến: 2 giờ │          │
│ └───────────────────────────────┘          │
│                                            │
│ ─────────────────────────────────          │
│ [ Sửa ]                 [ Gửi ]  (ghim)    │
└────────────────────────────────────────────┘
```

Màn hình chỉ có khối tóm tắt và hai nút ở cuối (ĐX-WF-07). Trong lúc chờ kết quả, **Gửi** vô hiệu. Các lỗi `SV_THE_HET_HAN`, `SV_KHONG_CON_THEO_HOC` hiển thị lý do ở banner đầu màn hình và không còn nút Gửi hoạt động; các lỗi cần sửa đưa SV về SCR-SV-04 theo bảng ở `UI-Screens.md` 5.5.


### SCR-SV-06 Danh sách phiếu yêu cầu của tôi

Nội dung: `UI-Screens.md` 5.6. Khung: mục 2.1. Mẫu bố cục: P-01.

```text
┌────────────────────────────────────────────┐ ┌────────────────────────────────────────────┐
│‹ Phiếu yêu cầu của tôi                     │ │‹ Phiếu yêu cầu của tôi                     │
├────────────────────────────────────────────┤ ├────────────────────────────────────────────┤
│ [ Tạo phiếu mới ]                          │ │ [ Tạo phiếu mới ]                          │
│                                            │ │                                            │
│ ┌──────────────────────────────────────┐   │ │                                            │
│ │ YC-000123 · Phòng A1     ‹Chờ duyệt› │   │ │   Bạn chưa gửi phiếu yêu cầu nào           │
│ │ Gửi: dd/MM/yyyy HH:mm                │   │ └────────────────────────────────────────────┘
│ └──────────────────────────────────────┘   │                                               
│ ┌────────────────────────────────────┐     │                                               
│ │ YC-000120 · Phòng B2     ‹Từ chối› │     │                                               
│ │ Gửi: dd/MM/yyyy HH:mm              │     │                                               
│ └────────────────────────────────────┘     │                                               
│ …                                          │                                               
└────────────────────────────────────────────┘                                               
```

Trái: có dữ liệu (mới nhất trước). Phải: rỗng. Nút **Tạo phiếu mới** luôn ở đầu vùng nội dung; khi rỗng, thông điệp rỗng nằm ngay dưới nút và không lặp lại nút. Chọn một dòng mở SCR-SV-07.


### SCR-SV-07 Chi tiết phiếu yêu cầu mượn

Nội dung: `UI-Screens.md` 5.7. Khung: mục 2.1.

```text
┌────────────────────────────────────────────┐ ┌────────────────────────────────────────────┐
│‹ Phiếu yêu cầu                             │ │‹ Phiếu yêu cầu                             │
├────────────────────────────────────────────┤ ├────────────────────────────────────────────┤
│ YC-000123           ‹Chờ duyệt›            │ │ YC-000123             ‹Đã duyệt›           │
│ ┌─ Lưu ý (chỉ khi vừa gửi) ─────────────┐  │ │ Phòng: A1 · Cơ sở 1                        │
│ │ 1. Phiếu chưa giữ chỗ.                │  │ │ Loại: Laptop Dell 5420 × 1                 │
│ │ 2. NV sẽ duyệt trong ngày.            │  │ │ Thời gian dự kiến: 2 giờ                   │
│ │ 3. Được duyệt: có thông báo và thời   │  │ │ Thời gian gửi: dd/MM/yyyy HH:mm            │
│ │    gian giữ chỗ (mặc định 30 phút).   │  │ │ ┌─ Theo trạng thái ───────────────┐        │
│ │ 4. Chưa duyệt trong ngày: tự hết hiệu │  │ │ │ Đã giữ chỗ lúc dd/MM/yyyy HH:mm │        │
│ │    lực, không tính vi phạm.           │  │ │ │ Hạn giữ chỗ: dd/MM/yyyy HH:mm   │        │
│ │ 5. Hạn trả do hệ thống tính khi giao. │  │ │ │ Còn lại: ‹00:24:10›             │        │
│ │ 6. Ứng dụng chỉ để đặt trước; nhận    │  │ │ │ Đến quầy nhận trước hạn.        │        │
│ │    thiết bị tại quầy.                 │  │ │ └─────────────────────────────────┘        │
│ └───────────────────────────────────────┘  │ └────────────────────────────────────────────┘
│ Phòng: A1 · Cơ sở 1                        │                                               
│ Loại: Laptop Dell 5420 × 1                 │                                               
│ Thời gian dự kiến: 2 giờ                   │                                               
│ Thời gian gửi: dd/MM/yyyy HH:mm            │                                               
│ ┌─ Theo trạng thái ────────┐               │                                               
│ │ Chờ NV duyệt trong ngày; │               │                                               
│ │ chưa giữ chỗ.            │               │                                               
│ └──────────────────────────┘               │                                               
└────────────────────────────────────────────┘                                               
```

Trái: chế độ "vừa gửi" (khối lưu ý ở **đầu** màn hình, chỉ khi SV đến từ SCR-SV-05; sáu ý được rút gọn để vừa khung, nội dung đầy đủ theo `UI-Screens.md` 5.7). Phải: mở từ danh sách, trạng thái Đã duyệt (bộ đếm ngược). Khối **Theo trạng thái** nằm cuối vùng nội dung và có nội dung, hành động theo bảng ở `UI-Screens.md` 5.7: Từ chối hiện lý do và nút **Tạo phiếu mới**; Hết hạn hiện nguyên nhân (phân biệt theo thời điểm duyệt) và nút **Tạo phiếu mới**; Đã nhận hiện liên kết xem lịch sử mượn/trả; Đã hủy chỉ có nhãn trạng thái.


### SCR-SV-08 Danh sách đăng ký chờ của tôi

Nội dung: `UI-Screens.md` 5.8. Khung: mục 2.1. Mẫu bố cục: P-01, P-03.

```text
┌────────────────────────────────────────────┐ ┌────────────────────────────────────────────┐
│‹ Đăng ký chờ của tôi                       │ │‹ Đăng ký chờ của tôi                       │
├────────────────────────────────────────────┤ ├────────────────────────────────────────────┤
│ ┌─────────────────────────────────────┐    │ │                                            │
│ │ Laptop Dell · A1       ‹Đang chờ›   │    │ │                                            │
│ │ Đăng ký: dd/MM/yyyy HH:mm           │    │ │   Bạn chưa đăng ký chờ thiết bị nào        │
│ │ Số thứ tự chờ: 3  [ Rời danh sách ] │    │ │   <Tra cứu thiết bị>                       │
│ └─────────────────────────────────────┘    │ └────────────────────────────────────────────┘
│ ┌──────────────────────────────────┐       │                                               
│ │ Máy chiếu · B2    ‹Đã thông báo› │       │                                               
│ │ Thiết bị đã sẵn sàng             │       │                                               
│ │ Hạn giữ chỗ: dd/MM/yyyy HH:mm    │       │                                               
│ │ Còn lại ‹00:20:05›               │       │                                               
│ │ Đến quầy nhận trước hạn.         │       │                                               
│ │               [ Từ chối nhận ]   │       │                                               
│ └──────────────────────────────────┘       │                                               
│ …                                          │                                               
└────────────────────────────────────────────┘                                               
```

Trái: có dữ liệu; nội dung và hành động của mỗi thẻ đổi theo trạng thái (Đang chờ, Đã thông báo, Đã nhận, Đã hủy, Hết hạn) theo bảng ở `UI-Screens.md` 5.8. Phải: rỗng. Hành động **Rời danh sách** hoặc **Từ chối nhận** ở góc dưới phải của thẻ, mở hộp xác nhận (mẫu P-03) nêu rõ không tính vi phạm.


### SCR-SV-09 Lịch sử mượn/trả

Nội dung: `UI-Screens.md` 5.9. Khung: mục 2.1. Mẫu bố cục: P-01.

```text
┌────────────────────────────────────────────┐ ┌────────────────────────────────────────────┐
│‹ Lịch sử mượn/trả                          │ │‹ Lịch sử mượn/trả                          │
├────────────────────────────────────────────┤ ├────────────────────────────────────────────┤
│ ┌────────────────────────────────────────┐ │ │                                            │
│ │ PM-000045            ‹Đang mượn›       │ │ │                                            │
│ │ Mượn: dd/MM/yyyy HH:mm                 │ │ │    Bạn chưa có phiếu mượn nào              │
│ │ Hẹn trả: dd/MM/yyyy HH:mm · 2 thiết bị │ │ └────────────────────────────────────────────┘
│ └────────────────────────────────────────┘ │                                               
│ ┌────────────────────────────────────────┐ │                                               
│ │ PM-000039              ‹Quá hạn›       │ │                                               
│ │ Mượn: dd/MM/yyyy HH:mm                 │ │                                               
│ │ Hẹn trả: dd/MM/yyyy HH:mm · 1 thiết bị │ │                                               
│ └────────────────────────────────────────┘ │                                               
│ …                                          │                                               
└────────────────────────────────────────────┘                                               
```

Trái: có dữ liệu (mới nhất trước; phiếu Quá hạn nhấn mạnh bằng nhãn). Phải: rỗng. Chọn một dòng mở SCR-SV-10.


### SCR-SV-10 Chi tiết phiếu mượn

Nội dung: `UI-Screens.md` 5.10. Khung: mục 2.1.

```text
┌────────────────────────────────────────────┐
│‹ Chi tiết phiếu mượn                       │
├────────────────────────────────────────────┤
│ PM-000039             ‹Quá hạn›            │
│ Mượn: dd/MM/yyyy HH:mm                     │
│ Hẹn trả: dd/MM/yyyy HH:mm                  │
│ Số lần gia hạn: 0                          │
│                                            │
│ ┌─ Thiết bị 1 ───────────────────────┐     │
│ │ Laptop Dell 5420 · LT-001 · Laptop │     │
│ │ Lúc giao: …(mô tả)                 │     │
│ │   ▓▓▓ ▓▓▓                          │     │
│ │ Kết quả kiểm tra: ‹Hư hỏng›        │     │
│ │ Lúc trả: …(mô tả)                  │     │
│ │   ▓▓▓                              │     │
│ │ Trả thực tế: dd/MM/yyyy HH:mm      │     │
│ │ (chưa trả: các mục lúc trả ghi     │     │
│ │ "Chưa trả")                        │     │
│ └────────────────────────────────────┘     │
│                                            │
│ ┌─ Biên bản vi phạm (ẩn khi không có) ─┐   │
│ │ BB-000012 · dd/MM/yyyy               │   │
│ │ Tổng tiền: 240.000 VNĐ ‹Chưa nộp›    │   │
│ │ Nộp: dd/MM/yyyy HH:mm (khi Đã nộp)   │   │
│ │ Nhắc nộp tiền trực tiếp cho NV       │   │
│ │ tại quầy (khi Chưa nộp).             │   │
│ │ · Laptop · Trả trễ · 3 ngày · …      │   │
│ └──────────────────────────────────────┘   │
└────────────────────────────────────────────┘
```

| Vùng | Quy tắc bố cục |
|---|---|
| Đầu phiếu | Mã phiếu, nhãn trạng thái, ngày giờ mượn, hẹn trả, số lần gia hạn (chỉ đọc) |
| Thẻ từng thiết bị | Xếp dọc theo thứ tự `dsChiTiet`; trong mỗi thẻ, **tình trạng và ảnh lúc giao đứng trước tình trạng và ảnh lúc trả** (xếp dọc vì màn hình hẹp) |
| Khối biên bản | Cuối màn hình; ẩn khi phiếu không có biên bản |
| Ảnh | Chạm để xem lớn (`UI-Screens.md` 3.6); ảnh hết hạn URL thì tải lại chi tiết |


### SCR-SV-11 Biên bản và phí phạt

Nội dung: `UI-Screens.md` 5.11. Khung: mục 2.1. Mẫu bố cục: P-01.

```text
┌────────────────────────────────────────────┐ ┌────────────────────────────────────────────┐
│‹ Biên bản và phí phạt                      │ │‹ Biên bản và phí phạt                      │
├────────────────────────────────────────────┤ ├────────────────────────────────────────────┤
│ ┌─────────────────────────────────────────┐│ │                                            │
│ │ BB-000012 · <PM-000039>   ‹Chưa nộp›    ││ │                                            │
│ │ Lập: dd/MM/yyyy · 240.000 VNĐ           ││ │   Bạn chưa có biên bản vi phạm nào         │
│ │ Nhắc nộp tiền trực tiếp cho NV tại quầy ││ └────────────────────────────────────────────┘
│ │ [ Xem các khoản ▾ ]                     ││                                               
│ └─────────────────────────────────────────┘│                                               
│ ┌─────────────────────────────────────┐    │                                               
│ │ BB-000010 · <PM-000031>    ‹Đã nộp› │    │                                               
│ │ Lập: dd/MM/yyyy · 80.000 VNĐ        │    │                                               
│ │ Nộp: dd/MM/yyyy HH:mm               │    │                                               
│ │ [ Xem các khoản ▾ ]                 │    │                                               
│ └─────────────────────────────────────┘    │                                               
│ …                                          │                                               
└────────────────────────────────────────────┘                                               
```

Trái: có dữ liệu; mở rộng một thẻ (**Xem các khoản**) hiện các khoản theo thiết bị ngay dưới thẻ. Mã phiếu mượn là liên kết → SCR-SV-10. Phải: rỗng.


### SCR-SV-12 Danh sách vi phạm

Nội dung: `UI-Screens.md` 5.12. Khung: mục 2.1. Mẫu bố cục: P-01.

```text
┌────────────────────────────────────────────┐ ┌────────────────────────────────────────────┐
│‹ Vi phạm                                   │ │‹ Vi phạm                                   │
├────────────────────────────────────────────┤ ├────────────────────────────────────────────┤
│ ┌───────────────────────────────────┐      │ │                                            │
│ │ ‹Trả trễ›        dd/MM/yyyy HH:mm │      │ │                                            │
│ │ Học kỳ: …          <BB-000012>    │      │ │    Bạn chưa có vi phạm nào                 │
│ └───────────────────────────────────┘      │ └────────────────────────────────────────────┘
│ ┌────────────────────────────────────────┐ │                                               
│ │ ‹Không đến nhận›  dd/MM/yyyy HH:mm     │ │                                               
│ │ Học kỳ: …    Không có biên bản         │ │                                               
│ │ Tham chiếu: mã phiếu yêu cầu / đăng ký │ │                                               
│ └────────────────────────────────────────┘ │                                               
│ …                                          │                                               
└────────────────────────────────────────────┘                                               
```

Trái: có dữ liệu; liên kết mã biên bản → SCR-SV-11 (chỉ khi có). Phải: rỗng.


### SCR-SV-13 Thông báo

Nội dung: `UI-Screens.md` 5.13. Khung: mục 2.1, màn hình gốc. Mẫu bố cục: P-01.

```text
┌────────────────────────────────────────────┐ ┌────────────────────────────────────────────┐
│  Thông báo                                 │ │  Thông báo                                 │
├────────────────────────────────────────────┤ ├────────────────────────────────────────────┤
│ ┌─────────────────────────────────────┐    │ │                                            │
│ │ ‹Nhắc hạn trả›     dd/MM/yyyy HH:mm │    │ │                                            │
│ │ Nội dung thông báo …                │    │ │    Bạn chưa có thông báo nào               │
│ └─────────────────────────────────────┘    │ ├────────────────────────────────────────────┤
│ ┌────────────────────────────────────────┐ │ │▢Trang chủ ▢Tra cứu   ▣Thông báo ▢Hồ sơ     │
│ │ ‹Duyệt hoặc từ chối›  dd/MM/yyyy HH:mm │ │ └────────────────────────────────────────────┘
│ │ Nội dung thông báo …                   │ │                                               
│ └────────────────────────────────────────┘ │                                               
│ …                                          │                                               
├────────────────────────────────────────────┤                                               
│▢Trang chủ ▢Tra cứu   ▣Thông báo ▢Hồ sơ     │                                               
└────────────────────────────────────────────┘                                               
```

Trái: có dữ liệu (mới nhất trước). Phải: rỗng. Mỗi thông báo là một dòng chọn được, mở màn hình theo loại (`UI-Screens.md` 5.13); loại Nhắc nhở của NV không có điều hướng. Không có nút đánh dấu đã đọc, xóa hoặc huy hiệu.


### SCR-SV-14 Hồ sơ mượn

Nội dung: `UI-Screens.md` 5.14. Khung: mục 2.1, màn hình gốc.

```text
┌────────────────────────────────────────────┐
│‹ Hồ sơ mượn                                │
├────────────────────────────────────────────┤
│ ⚠ Đặt trước không thực hiện được: …        │
│   (chỉ ở chế độ "bị chặn"; nêu hành động   │
│   vừa chọn và lý do)                       │
│ ┌─ Thông tin SV ─────────────────┐         │
│ │ SV: 21520001 · Nguyễn Văn A    │         │
│ │ Lớp: … · Khoa: …               │         │
│ │ Nhóm: …                        │         │
│ │ Hạn thẻ: dd/MM/yyyy ‹Đang học› │         │
│ └────────────────────────────────┘         │
│ ┌─ Quyền mượn ───────────┐                 │
│ │ Quyền mượn: ‹Tạm khóa› │                 │
│ │ Đang mượn: 1 / 3       │                 │
│ └────────────────────────┘                 │
│ ┌─ Biên bản chưa nộp ────────────────────┐ │
│ │ · BB-000012 · 240.000 VNĐ · dd/MM/yyyy │ │
│ │   <Xem biên bản>                       │ │
│ └────────────────────────────────────────┘ │
│ ┌─ Lý do không được mượn ─┐                │
│ │ · Thẻ đã hết hạn …      │                │
│ └─────────────────────────┘                │
│ ┌─ Hướng dẫn mở lại ───────────────────┐   │
│ │ Hướng dẫn theo trạng thái quyền mượn │   │
│ └──────────────────────────────────────┘   │
│ [ Đăng xuất ]                              │
├────────────────────────────────────────────┤
│▢Trang chủ ▢Tra cứu   ▢Thông báo ▣Hồ sơ     │
└────────────────────────────────────────────┘
```

| Vùng | Quy tắc bố cục |
|---|---|
| Banner chế độ "bị chặn" | Đầu vùng nội dung, chỉ khi đến từ SCR-SV-03, 04 hoặc 05; không có nút gửi |
| Khối thông tin | Theo thứ tự: Thông tin SV, Quyền mượn, Biên bản chưa nộp (ẩn khi rỗng), Lý do không được mượn (chỉ khi `hopLe` = false), Hướng dẫn mở lại |
| Nút **Đăng xuất** | Cuối màn hình, không có hộp xác nhận |

Nút Quay lại ở thanh ứng dụng chỉ có khi đến từ SCR-SV-01 (Xem lý do) hoặc từ chế độ "bị chặn" (ĐX-WF-14); hình vẽ ở chế độ "bị chặn".


### DLG-SV-01 Hộp thoại Đã đăng ký chờ

Nội dung: `UI-Screens.md` 5.15. Khung: hộp thoại (ĐX-WF-04).

```text
┌───────────────────────────────────────────┐
│ Đã đăng ký chờ                            │
│ ĐK-000007 · Laptop Dell 5420 · Phòng A1   │
│ ‹Đang chờ›    Số thứ tự chờ: 3            │
│ Mỗi đăng ký chờ dành cho một thiết bị.    │
│ Khi có thiết bị cùng loại được trả nguyên │
│ vẹn tại phòng, người đứng đầu danh sách   │
│ được thông báo và giữ chỗ (mặc định       │
│ 30 phút).                                 │
│                                           │
│ [ Đóng ]            [ Xem danh sách chờ ] │
└───────────────────────────────────────────┘
```

Hộp thoại ở giữa màn hình, chồng lên SCR-SV-03 hoặc SCR-SV-04 (ĐX-WF-04). **Đóng** ở trái, **Xem danh sách chờ** (→ SCR-SV-08) ở phải; đóng thì ở lại màn hình trước.



---

## 6. Wireframe màn hình của nhân viên quản lý thiết bị (web)

Mọi màn hình trong mục này theo khung 2.2; hình vẽ có đủ thanh đầu trang và menu bên chỉ để thể hiện vị trí vùng nội dung. Bộ lọc Phòng của NV chỉ liệt kê các phòng được cấp quyền.


### SCR-NV-01 Trang chủ nhân viên

Nội dung: `UI-Screens.md` 6.1. Khung: mục 2.2.

```text
┌──────────────────────────────────────────────────────────────────────────────────────────────────────────┐
│  Quản lý mượn trả thiết bị                              Họ tên · Nhân viên quản lý thiết bị  [Đăng xuất] │
├──────────────────────────────┬───────────────────────────────────────────────────────────────────────────┤
│▶ Trang chủ                   │ Trang chủ                                                                 │
│  Phiếu yêu cầu chờ duyệt     │ Xin chào, Họ tên                                                          │
│  Giao thiết bị               │ ┌────────────────────────────────────────────────────────────────────┐    │
│  Nhận trả thiết bị           │ │ Phòng được cấp quyền: A1 · Cơ sở 1;  B2 · Cơ sở 2                  │    │
│  Biên bản và thu phạt        │ └────────────────────────────────────────────────────────────────────┘    │
│  Phiếu chưa trả quá 30 ngày  │                                                                           │
│  Danh sách chờ               │ ┌────────────────────┐ ┌────────────────────┐ ┌────────────────────┐      │
│  Bảo trì và sửa chữa         │ │ Phiếu yêu cầu      │ │ Giao thiết bị      │ │ Nhận trả           │      │
│                              │ │ chờ duyệt          │ │                    │ │ thiết bị           │      │
│                              │ └────────────────────┘ └────────────────────┘ └────────────────────┘      │
│                              │ ┌────────────────────┐ ┌────────────────────┐ ┌────────────────────┐      │
│                              │ │ Biên bản và        │ │ Phiếu chưa trả     │ │ Danh sách chờ      │      │
│                              │ │ thu phạt           │ │ quá 30 ngày        │ │                    │      │
│                              │ └────────────────────┘ └────────────────────┘ └────────────────────┘      │
│                              │ ┌────────────────────┐                                                    │
│                              │ │ Bảo trì và         │                                                    │
│                              │ │ sửa chữa           │                                                    │
│                              │ └────────────────────┘                                                    │
│                              │                                                                           │
│                              │ ⚠ (khi dsPhong rỗng) Bạn chưa được cấp quyền phòng thiết bị nào;          │
│                              │   liên hệ quản trị viên. Các lối vào vẫn hiển thị, mọi danh sách rỗng.    │
│                              │                                                                           │
└──────────────────────────────┴───────────────────────────────────────────────────────────────────────────┘
```

Bảy lối vào xếp lưới ba cột theo thứ tự ở hình, tương ứng bảy mục chức năng của menu bên. Khối phòng được cấp quyền ở đầu vùng nội dung; cảnh báo chỉ hiện khi `dsPhong` rỗng.


### SCR-NV-02 Danh sách phiếu yêu cầu chờ duyệt

Nội dung: `UI-Screens.md` 6.2. Khung: mục 2.2. Mẫu bố cục: P-02.

```text
┌───────────────────────────────────────────────────────────────────────────────────────────────────────────────┐
│  Quản lý mượn trả thiết bị                                   Họ tên · Nhân viên quản lý thiết bị  [Đăng xuất] │
├──────────────────────────────┬────────────────────────────────────────────────────────────────────────────────┤
│  Trang chủ                   │ Phiếu yêu cầu chờ duyệt                         [ Làm mới ]                    │
│▶ Phiếu yêu cầu chờ duyệt     │ Phòng: [ Tất cả phòng được cấp quyền ▾ ]                                       │
│  Giao thiết bị               │                                                                                │
│  Nhận trả thiết bị           │ ┌────────┬──────────────────────────┬───────┬────────────┬─────┬─────────────┐ │
│  Biên bản và thu phạt        │ │ Mã YC  │ SV (mã · tên · lớp)      │ Phòng │ Loại × SL  │ Giờ │ Gửi lúc     │ │
│  Phiếu chưa trả quá 30 ngày  │ ├────────┼──────────────────────────┼───────┼────────────┼─────┼─────────────┤ │
│  Danh sách chờ               │ │ YC-123 │ 21520001 · Nguyễn A · L1 │ A1    │ Laptop × 1 │ 2   │ dd/MM HH:mm │ │
│  Bảo trì và sửa chữa         │ │ …      │ …                        │ …     │ …          │ …   │ …           │ │
│                              │ └────────┴──────────────────────────┴───────┴────────────┴─────┴─────────────┘ │
│                              │ Tổng số: …            ‹ 1 2 3 ›                                                │
│                              │                                                                                │
│                              │ (rỗng: "Không có phiếu yêu cầu chờ duyệt")                                     │
│                              │                                                                                │
└──────────────────────────────┴────────────────────────────────────────────────────────────────────────────────┘
```

Mẫu P-02. Phiếu gửi trước hiển thị trước. Cả dòng chọn được; chọn một dòng mở SCR-NV-03 mang theo dữ liệu dòng. Không có nút tạo mới.


### SCR-NV-03 Duyệt hoặc từ chối phiếu yêu cầu

Nội dung: `UI-Screens.md` 6.3. Khung: mục 2.2. Mẫu bố cục: P-03.

```text
┌──────────────────────────────────────────────────────────────────────────────────────────────────────────┐
│  Quản lý mượn trả thiết bị                              Họ tên · Nhân viên quản lý thiết bị  [Đăng xuất] │
├──────────────────────────────┬───────────────────────────────────────────────────────────────────────────┤
│  Trang chủ                   │ Duyệt hoặc từ chối phiếu yêu cầu                                          │
│▶ Phiếu yêu cầu chờ duyệt     │ ┌─ Thông tin phiếu ──────────────────────────────────────────────────┐    │
│  Giao thiết bị               │ │ Phiếu YC-123 · SV 21520001 · Nguyễn A · L1                         │    │
│  Nhận trả thiết bị           │ │ Phòng A1 · Dự kiến 2 giờ · Gửi dd/MM/yyyy HH:mm                    │    │
│  Biên bản và thu phạt        │ └────────────────────────────────────────────────────────────────────┘    │
│  Phiếu chưa trả quá 30 ngày  │ Bảng số lượng                                                             │
│  Danh sách chờ               │ ┌─────────────────────────────────────┬─────────┬──────────┐              │
│  Bảo trì và sửa chữa         │ │ Loại                                │ Yêu cầu │ Khả dụng │              │
│                              │ ├─────────────────────────────────────┼─────────┼──────────┤              │
│                              │ │ Laptop Dell                         │ 1       │ 5        │              │
│                              │ │ Máy chiếu (dòng không đủ nhấn mạnh) │ 2       │ 1  ⚠     │              │
│                              │ └─────────────────────────────────────┴─────────┴──────────┘              │
│                              │ ┌─ Kết quả kiểm tra ─────────────────────────────────────────────────┐    │
│                              │ │ Kết luận: được duyệt  (hoặc không được duyệt)                      │    │
│                              │ │ Lý do: · thẻ · nhóm · quyền mượn · giới hạn · số lượng             │    │
│                              │ └────────────────────────────────────────────────────────────────────┘    │
│                              │                                                                           │
│                              │                                   [ Từ chối ]   [ Duyệt ]                 │
│                              │                                                                           │
└──────────────────────────────┴───────────────────────────────────────────────────────────────────────────┘
```

```text
┌──────────────────────────────────────┐ ┌───────────────────────────────┐
│ Duyệt phiếu YC-123                   │ │ Từ chối phiếu YC-123          │
│ SV: …  Phòng: …                      │ │ Lý do từ chối *               │
│ Laptop × 1                           │ │ [ ____________________ ]      │
│ Giữ chỗ từng loại (mặc định 30 phút) │ │ Phiếu chuyển Từ chối, SV được │
│ và thông báo SV.                     │ │ thông báo.                    │
│                                      │ │ [ Hủy ]       [ Từ chối ]     │
│ [ Hủy ]          [ Duyệt ]           │ └───────────────────────────────┘
└──────────────────────────────────────┘                                  
```

| Vùng | Quy tắc bố cục |
|---|---|
| Thông tin phiếu | Đầu màn hình; lấy từ dòng đã chọn ở SCR-NV-02 (ĐX-UI-08) |
| Bảng số lượng | Giữa; dòng không đủ được nhấn mạnh bằng chữ kèm dấu cảnh báo |
| Kết quả kiểm tra | Dưới bảng số lượng; liệt kê từng lý do khi không được duyệt |
| Nút | Cuối vùng nội dung, căn phải: **Từ chối** rồi **Duyệt**. **Duyệt** vô hiệu khi `duocDuyet` = false; **Từ chối** luôn bật |
| Hộp xác nhận | Hai hộp ở dưới hình: Duyệt (trái), Từ chối có ô lý do bắt buộc (phải), theo mẫu P-03 |
| Sau khi xử lý | Khối kết quả thay vùng nút: Duyệt hiển thị hạn giữ chỗ; Từ chối hiển thị trạng thái; kèm nút quay về SCR-NV-02 |


### SCR-NV-04 Giao thiết bị, bước 1: Quét thẻ và hồ sơ sinh viên

Nội dung: `UI-Screens.md` 6.4. Khung: mục 2.2. Mẫu bố cục: P-06, P-07, P-08.

```text
┌──────────────────────────────────────────────────────────────────────────────────────────────────────────┐
│  Quản lý mượn trả thiết bị                              Họ tên · Nhân viên quản lý thiết bị  [Đăng xuất] │
├──────────────────────────────┬───────────────────────────────────────────────────────────────────────────┤
│  Trang chủ                   │ Giao thiết bị                                                             │
│  Phiếu yêu cầu chờ duyệt     │ ▶① Thẻ SV ───  ② Nguồn giao ───  ③ Thiết bị ───  ④ Phiếu mượn             │
│▶ Giao thiết bị               │                                                                           │
│  Nhận trả thiết bị           │ Mã thẻ sinh viên:  [ ▌quét hoặc nhập tay _______ ]                        │
│  Biên bản và thu phạt        │                                                                           │
│  Phiếu chưa trả quá 30 ngày  │ ┌─ Hồ sơ SV ─────────────────────────────────────────────────────────┐    │
│  Danh sách chờ               │ │ Mã SV · Họ tên · Lớp · Khoa                                        │    │
│  Bảo trì và sửa chữa         │ │ Nhóm: …   Hạn thẻ: dd/MM/yyyy   Học: ‹Đang học›                    │    │
│                              │ │ Quyền mượn: ‹Bình thường›   Đang mượn: 1 / 3                       │    │
│                              │ └────────────────────────────────────────────────────────────────────┘    │
│                              │ ┌─ Biên bản chưa nộp ──────────────────────────────────────────────────┐  │
│                              │ │ · BB-000012 · 240.000 VNĐ · dd/MM/yyyy   <Mở ở Biên bản và thu phạt> │  │
│                              │ └──────────────────────────────────────────────────────────────────────┘  │
│                              │ Kết luận: ‹Hợp lệ›                                                        │
│                              │                                                                           │
│                              │                           [ Quét thẻ khác ]   [ Tiếp tục ]                │
│                              │                                                                           │
└──────────────────────────────┴───────────────────────────────────────────────────────────────────────────┘
```

```text
Không hợp lệ (E1):                Mã thẻ không có trong hệ thống:
Kết luận: ‹Không hợp lệ›          ⚠ Không tìm thấy sinh viên
Lý do: · Thẻ đã hết hạn           [ mã đã nhập được giữ lại ]
       · Quyền mượn bị khóa                                      
[ Tiếp tục ] vô hiệu; không có                                   
đường tới SCR-NV-05.                                             
```

Mẫu P-06 (ô nhập mã), P-07 (hồ sơ SV) và P-08 (thanh tiến trình). Thứ tự: tiến trình, ô nhập mã thẻ, hồ sơ SV, biên bản chưa nộp (ẩn khi không có; liên kết sang SCR-NV-11 kèm mã số SV), kết luận và danh sách lý do, nút. **Tiếp tục** chỉ bật khi `hopLe` = true; **Quét thẻ khác** xóa hồ sơ và đưa tiêu điểm về ô nhập mã. Hai biến thể không vẽ lại cả màn hình ở trên: Không hợp lệ và mã thẻ không có trong hệ thống.


### SCR-NV-05 Giao thiết bị, bước 2: Chọn nguồn giao và kiểm tra nguồn

Nội dung: `UI-Screens.md` 6.5. Khung: mục 2.2. Mẫu bố cục: P-02, P-08.

```text
┌──────────────────────────────────────────────────────────────────────────────────────────────────────────────┐
│  Quản lý mượn trả thiết bị                                  Họ tên · Nhân viên quản lý thiết bị  [Đăng xuất] │
├──────────────────────────────┬───────────────────────────────────────────────────────────────────────────────┤
│  Trang chủ                   │ Giao thiết bị                                                                 │
│  Phiếu yêu cầu chờ duyệt     │  ① Thẻ SV ─── ▶② Nguồn giao ───  ③ Thiết bị ───  ④ Phiếu mượn                 │
│▶ Giao thiết bị               │ SV: 21520001 · Nguyễn A     Phòng: [ A1 ▾ ]                                   │
│  Nhận trả thiết bị           │                                                                               │
│  Biên bản và thu phạt        │ ┌────────────────────┐ ┌────────────────────┐ ┌────────────────────┐          │
│  Phiếu chưa trả quá 30 ngày  │ │ ▶ Phiếu yêu cầu    │ │   Danh sách chờ    │ │   Mượn trực tiếp   │          │
│  Danh sách chờ               │ └────────────────────┘ └────────────────────┘ └────────────────────┘          │
│  Bảo trì và sửa chữa         │ ┌────────┬───────────────────────────────┬───────┬────────────┬─────────────┐ │
│                              │ │ Mã YC  │ SV                            │ Phòng │ Loại × SL  │ Hạn giữ chỗ │ │
│                              │ ├────────┼───────────────────────────────┼───────┼────────────┼─────────────┤ │
│                              │ │ YC-123 │ 21520001 · Nguyễn A (nổi bật) │ A1    │ Laptop × 1 │ dd/MM HH:mm │ │
│                              │ │ …      │ …                             │ …     │ …          │ …           │ │
│                              │ └────────┴───────────────────────────────┴───────┴────────────┴─────────────┘ │
│                              │ Tổng số: …     ‹ 1 2 ›                                                        │
│                              │ [ Kiểm tra nguồn ]                                                            │
│                              │                                                                               │
│                              │ ┌─ Kết quả kiểm tra nguồn ───────────────────────────────────────────┐        │
│                              │ │ ‹Hợp lệ›   SV nguồn: 21520001 · Nguyễn A · L1                      │        │
│                              │ │ ⚠ Sinh viên của nguồn khác với thẻ đã quét (chỉ khi khác)          │        │
│                              │ │ Phòng A1 · cần giao: Laptop × 1                                    │        │
│                              │ │ Vị trí lấy hàng: Laptop · A1 · Kệ 2 · Tủ 3 · sẵn sàng 5            │        │
│                              │ └────────────────────────────────────────────────────────────────────┘        │
│                              │                                                   [ Tiếp tục ]                │
│                              │                                                                               │
└──────────────────────────────┴───────────────────────────────────────────────────────────────────────────────┘
```

```text
┌────────────────────────────────────────────┐ ┌─────────────────────────────────────────────┐
│ Nguồn Danh sách chờ                        │ │ Nguồn Mượn trực tiếp                        │
│ Loại: [ Tất cả ▾ ] (lọc)                   │ │ (phòng theo bộ lọc Phòng ở đầu màn hình)    │
│ SV · Loại · Phòng · STT/hạn · ‹Trạng thái› │ │ Loại còn hàng: Laptop (5)  SL [ − ] 1 [ + ] │
│ (chỉ dòng ‹Đã thông báo› chọn được)        │ │ Loại hết hàng: vô hiệu                      │
└────────────────────────────────────────────┘ └─────────────────────────────────────────────┘
```

| Vùng | Quy tắc bố cục |
|---|---|
| Đầu | Tiêu đề, thanh tiến trình, dòng nhắc lại SV, bộ lọc Phòng (áp dụng cho cả ba nguồn) |
| Ba thẻ nguồn | Hàng ngang dưới bộ lọc (ĐX-WF-10); nguồn đang chọn được nhấn mạnh |
| Nội dung nguồn | Ngay dưới thẻ: bảng (Phiếu yêu cầu, Danh sách chờ) hoặc danh sách loại kèm bộ chọn số lượng (Mượn trực tiếp; phòng lấy từ bộ lọc Phòng chung, ĐX-WF-18), như hai hộp ở dưới hình. Dòng của SV đang giao được làm nổi bật khi có trong trang đang xem (ĐX-UI-09) |
| Nút **Kiểm tra nguồn** | Dưới nội dung nguồn |
| Kết quả kiểm tra | Dưới nút: kết luận, lý do khi không hợp lệ, SV của nguồn, cảnh báo khác thẻ, phòng, loại và số lượng cần giao, vị trí lấy hàng (kệ, tủ; "Chưa ghi vị trí" khi không có) |
| Nút **Tiếp tục** | Cuối vùng nội dung; chỉ bật khi `hopLe` = true |
| Rỗng | Thông báo rỗng thay bảng của nguồn; NV chuyển sang nguồn khác |

**Chính sách chưa chốt:** cảnh báo khác thẻ chỉ hỗ trợ NV đối chiếu, không xác nhận SV của nguồn trùng với SV đã quét. Cách xử lý khi hai mã SV khác nhau và quy tắc đối chiếu số lượng giao với nguồn vẫn cần quyết định nghiệp vụ; wireframe không tự thêm điều kiện chặn hoặc suy ra rằng được phép giao thiếu hay thừa số lượng nguồn.


### SCR-NV-06 Giao thiết bị, bước 3: Thêm thiết bị và ghi tình trạng ban đầu

Nội dung: `UI-Screens.md` 6.6. Khung: mục 2.2. Mẫu bố cục: P-03, P-05, P-06, P-08.

```text
┌──────────────────────────────────────────────────────────────────────────────────────────────────────────────┐
│  Quản lý mượn trả thiết bị                                  Họ tên · Nhân viên quản lý thiết bị  [Đăng xuất] │
├──────────────────────────────┬───────────────────────────────────────────────────────────────────────────────┤
│  Trang chủ                   │ Giao thiết bị                                                                 │
│  Phiếu yêu cầu chờ duyệt     │  ① Thẻ SV ───  ② Nguồn giao ─── ▶③ Thiết bị ───  ④ Phiếu mượn                 │
│▶ Giao thiết bị               │                                                                               │
│  Nhận trả thiết bị           │ ┌─ Danh sách cần giao ───────────┐  Mã thiết bị: [ ▌quét/nhập ___ ]           │
│  Biên bản và thu phạt        │ │ Loại · cần · đã thêm · vị trí  │  (thiết bị gắn vào loại ▶ đang chọn)       │
│  Phiếu chưa trả quá 30 ngày  │ │ ▶ Laptop · 1 · 0 · A1 Kệ2 Tủ3  │  ┌─ Thiết bị đã thêm ────────────────────┐ │
│  Danh sách chờ               │ │   Chuột · 1 · 0 · …            │  │ LT-001 · Laptop Dell · Laptop         │ │
│  Bảo trì và sửa chữa         │ └────────────────────────────────┘  │ Phòng A1 · Kệ 2 · Tủ 3        [ Xóa ] │ │
│                              │                                     │ Mô tả tình trạng ban đầu *            │ │
│                              │                                     │ [ ________________________ ]          │ │
│                              │                                     │ ▓▓ ▓▓  [ + Thêm ảnh ]  (P-05)         │ │
│                              │                                     └───────────────────────────────────────┘ │
│                              │                                                                               │
│                              │                                          [ Lập phiếu mượn ]  (ghim)           │
│                              │                                                                               │
└──────────────────────────────┴───────────────────────────────────────────────────────────────────────────────┘
```

```text
┌──────────────────────────────────────────────────────────┐
│ Lập phiếu mượn                                           │
│ SV: … · Nguồn: Phiếu yêu cầu YC-123                      │
│ LT-001 · Laptop Dell · tình trạng: …                     │
│ Hệ quả: thiết bị chuyển Đang được mượn, giải phóng       │
│ giữ chỗ, nguồn chuyển Đã nhận; hạn trả do hệ thống tính. │
│ [ Hủy ]                          [ Lập phiếu mượn ]      │
└──────────────────────────────────────────────────────────┘
```

| Vùng | Quy tắc bố cục |
|---|---|
| Cột trái | **Danh sách cần giao**: từng loại với số lượng cần giao, số đã thêm, vị trí lấy hàng; loại đang là đích nhận thiết bị tiếp theo được đánh dấu `▶` |
| Cột phải | Ô nhập mã thiết bị ở trên; bên dưới là các thẻ thiết bị đã thêm xếp dọc, mỗi thẻ có mô tả bắt buộc, khu đính ảnh (P-05), nút Xóa; thiết bị bị từ chối hiển thị lý do ngay dưới ô nhập và không có thẻ |
| Nút **Lập phiếu mượn** | Cuối vùng nội dung, cố định khi cuộn; chỉ bật khi có ít nhất một thiết bị và mọi thiết bị đã có mô tả |
| Lỗi khi lập | Banner đầu vùng nội dung (ĐX-WF-06); lỗi `DU_LIEU_KHONG_HOP_LE` đặt ở thẻ thiết bị và khu ảnh tương ứng; lỗi `THIET_BI_DA_DUOC_GIAO` đánh dấu thẻ thiết bị liên quan |
| Hộp xác nhận | Dưới hình, mẫu P-03, liệt kê SV, nguồn, từng thiết bị và mô tả |


### SCR-NV-07 Giao thiết bị, bước 4: Phiếu mượn vừa lập

Nội dung: `UI-Screens.md` 6.7. Khung: mục 2.2. Mẫu bố cục: P-08.

```text
┌──────────────────────────────────────────────────────────────────────────────────────────────────────────┐
│  Quản lý mượn trả thiết bị                              Họ tên · Nhân viên quản lý thiết bị  [Đăng xuất] │
├──────────────────────────────┬───────────────────────────────────────────────────────────────────────────┤
│  Trang chủ                   │ Giao thiết bị                                                             │
│  Phiếu yêu cầu chờ duyệt     │  ① Thẻ SV ───  ② Nguồn giao ───  ③ Thiết bị ─── ▶④ Phiếu mượn             │
│▶ Giao thiết bị               │                                                                           │
│  Nhận trả thiết bị           │ ┌─ Phiếu mượn ───────────────────────────────────────────────────────┐    │
│  Biên bản và thu phạt        │ │ PM-000045   SV: 21520001 · Nguyễn A   ‹Đang mượn›                  │    │
│  Phiếu chưa trả quá 30 ngày  │ │ Mượn: dd/MM/yyyy HH:mm      Hẹn trả: dd/MM/yyyy HH:mm              │    │
│  Danh sách chờ               │ └────────────────────────────────────────────────────────────────────┘    │
│  Bảo trì và sửa chữa         │ ┌────────┬─────────────┬────────┬────────────────────┐                    │
│                              │ │ Mã     │ Thiết bị    │ Loại   │ Tình trạng ban đầu │                    │
│                              │ ├────────┼─────────────┼────────┼────────────────────┤                    │
│                              │ │ LT-001 │ Laptop Dell │ Laptop │ …                  │                    │
│                              │ │ …      │ …           │ …      │ …                  │                    │
│                              │ └────────┴─────────────┴────────┴────────────────────┘                    │
│                              │                                                                           │
│                              │              [ Giao cho sinh viên khác ]   [ Về trang chủ ]               │
│                              │                                                                           │
└──────────────────────────────┴───────────────────────────────────────────────────────────────────────────┘
```

Màn hình chỉ xem: không có hành động sửa phiếu. Hai nút ở cuối: **Giao cho sinh viên khác** (→ SCR-NV-04 trống) và **Về trang chủ** (→ SCR-NV-01).

**Thông tin SV trên phiếu:** mã SV lấy từ `PhieuMuon.maSV` trả về. Chỉ hiển thị họ tên của `KetQuaKiemTraNguon.sinhVien` khi `maSV` của thông tin nguồn khớp mã SV trên phiếu; nếu chưa có họ tên khớp thì chỉ hiển thị mã SV, không ghép họ tên cũ từ bước quét thẻ.


### SCR-NV-08 Nhận trả, bước 1: Tìm thiết bị hoặc phiếu mượn

Nội dung: `UI-Screens.md` 6.8. Khung: mục 2.2. Mẫu bố cục: P-02, P-06.

```text
┌──────────────────────────────────────────────────────────────────────────────────────────────────────────┐
│  Quản lý mượn trả thiết bị                              Họ tên · Nhân viên quản lý thiết bị  [Đăng xuất] │
├──────────────────────────────┬───────────────────────────────────────────────────────────────────────────┤
│  Trang chủ                   │ Nhận trả thiết bị                                                         │
│  Phiếu yêu cầu chờ duyệt     │ Mã thiết bị:  [ ▌quét hoặc nhập tay ______ ]                              │
│  Giao thiết bị               │ ┌─ Thông tin bàn giao ───────────────────────────────────────────────┐    │
│▶ Nhận trả thiết bị           │ │ Phiếu PM-045 · SV 21520001 · Nguyễn A · L1                         │    │
│  Biên bản và thu phạt        │ │ Mượn: dd/MM/yyyy HH:mm   Hẹn trả: dd/MM/yyyy HH:mm                 │    │
│  Phiếu chưa trả quá 30 ngày  │ │ Tình trạng ban đầu: …     ▓▓▓ ▓▓▓ ảnh lúc giao                     │    │
│  Danh sách chờ               │ │                               [ Nhận trả thiết bị này ]            │    │
│  Bảo trì và sửa chữa         │ └────────────────────────────────────────────────────────────────────┘    │
│                              │                                                                           │
│                              │ Tìm phiếu theo SV                                                         │
│                              │ Mã số sinh viên: [ ▌quét hoặc nhập tay ______ ]                           │
│                              │ ┌──────────────┐ ┌──────────────┐ ┌──────────────┐                        │
│                              │ │ ▶ Đang mượn  │ │   Quá hạn    │ │   Đã trả     │                        │
│                              │ └──────────────┘ └──────────────┘ └──────────────┘                        │
│                              │ ┌──────────┬──────────────┬───────┬─────────────┬─────────────┬───────┐   │
│                              │ │ Mã phiếu │ SV           │ Mượn  │ Hẹn trả     │ Trạng thái  │ Số TB │   │
│                              │ ├──────────┼──────────────┼───────┼─────────────┼─────────────┼───────┤   │
│                              │ │ PM-045   │ 21520001 · A │ dd/MM │ dd/MM HH:mm │ ‹Đang mượn› │ 2     │   │
│                              │ │ …        │ …            │ …     │ …           │ …           │ …     │   │
│                              │ └──────────┴──────────────┴───────┴─────────────┴─────────────┴───────┘   │
│                              │                                                                           │
└──────────────────────────────┴───────────────────────────────────────────────────────────────────────────┘
```

| Vùng | Quy tắc bố cục |
|---|---|
| Ô nhập mã thiết bị | Đầu màn hình (P-06); có tiêu điểm khi mở |
| Thông tin bàn giao | Hiện dưới ô nhập sau khi tra cứu thành công; nút **Nhận trả thiết bị này** ở cuối khối (→ SCR-NV-09) |
| Khối **Tìm phiếu theo SV** | Dưới khối bàn giao, ngăn cách bằng tiêu đề khối: ô mã số SV (kiểu ô nhập mã P-06, Enter để xác nhận), ba tab Đang mượn, Quá hạn, Đã trả (ĐX-WF-11), rồi bảng phiếu; mỗi lần chỉ một tab |
| Chọn phiếu trong bảng | Phiếu Đang mượn hoặc Quá hạn → SCR-NV-09 (chọn thiết bị từ phiếu); phiếu Đã trả → SCR-NV-10 |
| Lỗi | `THIET_BI_KHONG_THUOC_PHIEU`, `KHONG_TIM_THAY`, `PHONG_CHUA_CAP_QUYEN`: banner đầu vùng nội dung, mã đã nhập được giữ lại |


### SCR-NV-09 Nhận trả, bước 2: Kiểm tra và ghi kết quả

Nội dung: `UI-Screens.md` 6.9. Khung: mục 2.2. Mẫu bố cục: P-03, P-05, P-06.

```text
┌──────────────────────────────────────────────────────────────────────────────────────────────────────────┐
│  Quản lý mượn trả thiết bị                              Họ tên · Nhân viên quản lý thiết bị  [Đăng xuất] │
├──────────────────────────────┬───────────────────────────────────────────────────────────────────────────┤
│  Trang chủ                   │ Kiểm tra và ghi kết quả                                                   │
│  Phiếu yêu cầu chờ duyệt     │ PM-045 · SV 21520001 · Nguyễn A · Hẹn trả … ‹Quá hạn›                     │
│  Giao thiết bị               │ Thiết bị của phiếu                                                        │
│▶ Nhận trả thiết bị           │ ┌────────┬─────────────┬────────┬────────────────────────┐                │
│  Biên bản và thu phạt        │ │ Mã     │ Tên         │ Loại   │ Kết quả đã ghi         │                │
│  Phiếu chưa trả quá 30 ngày  │ ├────────┼─────────────┼────────┼────────────────────────┤                │
│  Danh sách chờ               │ │ LT-001 │ Laptop Dell │ Laptop │ chưa ghi (chọn được)   │                │
│  Bảo trì và sửa chữa         │ │ CH-002 │ Chuột       │ Chuột  │ ‹Nguyên vẹn› (chỉ xem) │                │
│                              │ └────────┴─────────────┴────────┴────────────────────────┘                │
│                              │ Mã thiết bị: [ ▌quét/nhập ______ ]                                        │
│                              │ ┌─ LT-001 · Laptop Dell · Laptop ─────────────────────────────────────┐   │
│                              │ │ ┌────────────────────────────┐  ┌─────────────────────────────────┐ │   │
│                              │ │ │ Tình trạng lúc giao        │  │ Kết quả kiểm tra: [ Hư hỏng ▾ ] │ │   │
│                              │ │ │ …(mô tả)                   │  │ Mô tả tình trạng lúc trả *      │ │   │
│                              │ │ │ ▓▓▓ ▓▓▓                    │  │ [ ____________ ]                │ │   │
│                              │ │ └────────────────────────────┘  │ Ảnh lúc trả: [+] ▓▓ (P-05)      │ │   │
│                              │ │                                 │ Khung phạt *: [ Nhẹ 15% ▾ ]     │ │   │
│                              │ │                                 └─────────────────────────────────┘ │   │
│                              │ └─────────────────────────────────────────────────────────────────────┘   │
│                              │                                               [ Ghi kết quả ]             │
│                              │                                                                           │
└──────────────────────────────┴───────────────────────────────────────────────────────────────────────────┘
```

```text
┌──────────────────────────────────────────────────────────────────────┐
│ Kết quả sau khi ghi                                                  │
│ LT-001 · ‹Hư hỏng› · Trả: dd/MM/yyyy HH:mm · → Đang bảo trì/sửa chữa │
│ Phiếu: ‹Đã trả›                                                      │
│ [ Xem khoản phạt đề xuất ]       [ Nhận trả thiết bị khác ]          │
└──────────────────────────────────────────────────────────────────────┘
```

| Vùng | Quy tắc bố cục |
|---|---|
| Đầu phiếu | Mã phiếu, SV, hẹn trả, nhãn trạng thái |
| Danh sách thiết bị của phiếu | Bảng; thiết bị chưa có kết quả chọn được để thêm vào lần ghi, thiết bị đã có kết quả chỉ xem |
| Thẻ kiểm tra từng thiết bị | **Hai cột (ĐX-WF-09)**: trái là tình trạng và ảnh lúc giao (chỉ đọc); phải là mô tả lúc trả, khu ảnh lúc trả, bộ chọn Kết quả kiểm tra (Nguyên vẹn, Hư hỏng, Thiếu phụ kiện, Mất), bộ chọn Khung phạt. Các thẻ xếp dọc, mỗi thiết bị một thẻ |
| Theo kết quả | Nguyên vẹn: ẩn Khung phạt, mô tả tùy chọn. Hư hỏng, Thiếu phụ kiện: mô tả và Khung phạt bắt buộc. Mất: ẩn Khung phạt, vô hiệu khu ảnh lúc trả |
| Nút **Ghi kết quả** | Cuối vùng nội dung; mở hộp xác nhận (P-03) liệt kê từng thiết bị và kết quả; với Mất, hộp nêu nổi bật thiết bị sẽ chuyển Đã báo mất và kết quả không đổi được |
| Khối sau khi ghi | Xuất hiện dưới thẻ (hình dưới): kết quả, ngày giờ trả thực tế (trừ Mất), tình trạng thiết bị, nhãn phiếu; nút **Xem khoản phạt đề xuất** (nhấn mạnh khi có thiết bị khác Nguyên vẹn hoặc phiếu Quá hạn) và **Nhận trả thiết bị khác** |


### SCR-NV-10 Lập biên bản vi phạm

Nội dung: `UI-Screens.md` 6.10. Khung: mục 2.2. Mẫu bố cục: P-03.

```text
┌────────────────────────────────────────────────────────────────────────────────────────────────────────────┐
│  Quản lý mượn trả thiết bị                                Họ tên · Nhân viên quản lý thiết bị  [Đăng xuất] │
├──────────────────────────────┬─────────────────────────────────────────────────────────────────────────────┤
│  Trang chủ                   │ Lập biên bản vi phạm                                                        │
│  Phiếu yêu cầu chờ duyệt     │ Phiếu PM-045                                                                │
│  Giao thiết bị               │ ┌───────────────┬──────────────┬──────────┬───────────────┬───────────────┐ │
│▶ Nhận trả thiết bị           │ │ Thiết bị      │ Loại vi phạm │ Ngày trễ │ Số tiền (VNĐ) │ Khung phạt    │ │
│  Biên bản và thu phạt        │ ├───────────────┼──────────────┼──────────┼───────────────┼───────────────┤ │
│  Phiếu chưa trả quá 30 ngày  │ │ LT-001 Laptop │ Trả trễ      │ 3        │ 150.000       │ –             │ │
│  Danh sách chờ               │ │ CH-002 Chuột  │ Hư hỏng      │ –        │ 40.000        │ [ Vừa 40% ▾ ] │ │
│  Bảo trì và sửa chữa         │ │ …             │ …            │ …        │ …             │ …             │ │
│                              │ └───────────────┴──────────────┴──────────┴───────────────┴───────────────┘ │
│                              │ Tổng tiền: 190.000 VNĐ  (tạm tính nếu NV đổi khung phạt)                    │
│                              │                                                                             │
│                              │                                               [ Lập biên bản ]              │
│                              │                                                                             │
└──────────────────────────────┴─────────────────────────────────────────────────────────────────────────────┘
```

```text
┌──────────────────────────────────────┐  ┌──────────────────────────────────────┐
│ Biên bản vừa lập                     │  │ Không có khoản vi phạm               │
│ BB-000012 · dd/MM/yyyy · 190.000 VNĐ │  │ "Phiếu này không có khoản vi phạm;   │
│ ‹Chưa nộp›  · các khoản …            │  │ không cần lập biên bản"              │
│ [ Thu phạt và mở khóa ]              │  │ (không có nút Lập biên bản)          │
└──────────────────────────────────────┘  └──────────────────────────────────────┘
```

Bảng khoản đề xuất ở giữa; cột Số tiền chỉ đọc (không có ô sửa); bộ chọn Khung phạt chỉ ở khoản Hư hỏng. **Lập biên bản** ở cuối, mở hộp xác nhận (P-03) nêu số tiền từng khoản, tổng tiền, nhấn mạnh số tiền không đổi sau khi lập. Sau khi lập, khối **Biên bản vừa lập** (hình dưới, trái) thay vùng nút, kèm **Thu phạt và mở khóa** (→ SCR-NV-11 với mã số SV). Biến thể không có khoản (hình dưới, phải) thay bảng.


### SCR-NV-11 Biên bản và thu phạt

Nội dung: `UI-Screens.md` 6.11. Khung: mục 2.2. Mẫu bố cục: P-02, P-03, P-06, P-07.

```text
┌──────────────────────────────────────────────────────────────────────────────────────────────────────────────────────┐
│  Quản lý mượn trả thiết bị                                          Họ tên · Nhân viên quản lý thiết bị  [Đăng xuất] │
├──────────────────────────────┬───────────────────────────────────────────────────────────────────────────────────────┤
│  Trang chủ                   │ Biên bản và thu phạt            [ Làm mới ]                                           │
│  Phiếu yêu cầu chờ duyệt     │ Mã SV: [ ▌quét/nhập ____ ]  Trạng thái: [ Chưa nộp ▾ ]  Phòng: [ Tất cả ▾ ]           │
│  Giao thiết bị               │ <Tìm phiếu của SV>                                                                    │
│  Nhận trả thiết bị           │ ┌─ Hồ sơ SV (khi đã lọc theo mã SV) ─────────────────────────────────┐                │
│▶ Biên bản và thu phạt        │ │ Nguyễn A · L1 · Nhóm … · ‹Tạm khóa› · Đang mượn 1 / 3              │                │
│  Phiếu chưa trả quá 30 ngày  │ │ (Cấm mượn dài hạn: nộp phạt không tự mở khóa; chờ QT gỡ cấm)       │                │
│  Danh sách chờ               │ │                        [ Xác nhận đã nhận đủ thiết bị ]            │                │
│  Bảo trì và sửa chữa         │ └────────────────────────────────────────────────────────────────────┘                │
│                              │ ┌────────┬────────┬───────┬──────────┬───────────┬────────────┬─────────────────────┐ │
│                              │ │ Mã BB  │ Phiếu  │ Mã SV │ Ngày lập │ Tổng tiền │ Trạng thái │ Hành động           │ │
│                              │ ├────────┼────────┼───────┼──────────┼───────────┼────────────┼─────────────────────┤ │
│                              │ │ BB-012 │ PM-045 │ 2152… │ dd/MM    │ 190.000   │ ‹Chưa nộp› │ [ Ghi nhận đã nộp ] │ │
│                              │ │ …      │ …      │ …     │ …        │ …         │ …          │ …                   │ │
│                              │ └────────┴────────┴───────┴──────────┴───────────┴────────────┴─────────────────────┘ │
│                              │ Tổng số: …   ‹ 1 2 ›                                                                  │
│                              │                                                                                       │
└──────────────────────────────┴───────────────────────────────────────────────────────────────────────────────────────┘
```

| Vùng | Quy tắc bố cục |
|---|---|
| Bộ lọc | Đầu màn hình: mã số SV (P-06), trạng thái (mặc định Chưa nộp), phòng; liên kết **Tìm phiếu của SV** (→ SCR-NV-08) |
| Hồ sơ SV | Chỉ hiện khi đã lọc theo mã số SV (P-07, các trường theo ghi chú ở mục 3.7); nút **Xác nhận đã nhận đủ thiết bị** ở cuối khối; sau khi xác nhận, kết quả mở khóa hoặc danh sách lý do còn khóa hiện ngay trong khối |
| Bảng biên bản | Dưới hồ sơ; mỗi dòng mở rộng để xem các khoản theo thiết bị; nút **Ghi nhận đã nộp** chỉ ở biên bản Chưa nộp |
| Hộp xác nhận | Mẫu P-03 cho Ghi nhận đã nộp và Xác nhận đã nhận đủ thiết bị |
| Biên bản Đã hủy | Chỉ xem, không có nút hành động |


### SCR-NV-12 Phiếu chưa trả quá 30 ngày

Nội dung: `UI-Screens.md` 6.12. Khung: mục 2.2. Mẫu bố cục: P-03. Chân phân trang theo `Design-System.md` CMP-19.

```text
┌──────────────────────────────────────────────────────────────────────────────────────────────────────────┐
│  Quản lý mượn trả thiết bị                              Họ tên · Nhân viên quản lý thiết bị  [Đăng xuất] │
├──────────────────────────────┬───────────────────────────────────────────────────────────────────────────┤
│  Trang chủ                   │ Phiếu chưa trả quá 30 ngày         [ Làm mới ]                            │
│  Phiếu yêu cầu chờ duyệt     │ Phòng: [ Tất cả phòng được cấp quyền ▾ ]                                  │
│  Giao thiết bị               │ ┌────────────────────────────────────────────────────────────────────┐    │
│  Nhận trả thiết bị           │ │ PM-031 · SV 21520001 · Nguyễn A · Hẹn trả dd/MM/yyyy · Trễ 45 ngày │    │
│  Biên bản và thu phạt        │ │ [x] LT-004 · Laptop Dell                                           │    │
│▶ Phiếu chưa trả quá 30 ngày  │ │ [ ] CH-009 · Chuột                                                 │    │
│  Danh sách chờ               │ └────────────────────────────────────────────────────────────────────┘    │
│  Bảo trì và sửa chữa         │ ┌────────────────────────────────────────────────────────────────────┐    │
│                              │ │ PM-020 · …                                                         │    │
│                              │ │ [ ] …                                                              │    │
│                              │ └────────────────────────────────────────────────────────────────────┘    │
│                              │ Tổng số: …    Trang 1 / …    [ Trước ] [ Sau ]                            │
│                              │                                           [ Lập biên bản mất ]            │
│                              │                                                                           │
└──────────────────────────────┴───────────────────────────────────────────────────────────────────────────┘
```

Mỗi phiếu là một thẻ, trong thẻ có danh sách thiết bị chưa trả kèm ô chọn. Dưới danh sách thẻ là chân tổng số, trang hiện tại và nút **Trước**, **Sau** theo `Design-System.md` CMP-19; đổi bộ lọc Phòng thì về trang 1. **Lập biên bản mất** nằm sau chân phân trang, bật khi đã chọn ít nhất một thiết bị của **một** phiếu. Hộp xác nhận theo mẫu P-03; kết quả (biên bản vừa lập và nút **Thu phạt và mở khóa** → SCR-NV-11) hiển thị như SCR-NV-10. Rỗng: "Không có phiếu chưa trả quá 30 ngày".


### SCR-NV-13 Danh sách chờ của phòng

Nội dung: `UI-Screens.md` 6.13. Khung: mục 2.2. Mẫu bố cục: P-02, P-03.

```text
┌──────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────┐
│  Quản lý mượn trả thiết bị                                                  Họ tên · Nhân viên quản lý thiết bị  [Đăng xuất] │
├──────────────────────────────┬───────────────────────────────────────────────────────────────────────────────────────────────┤
│  Trang chủ                   │ Danh sách chờ                         [ Làm mới ]                                             │
│  Phiếu yêu cầu chờ duyệt     │ Phòng: [ A1 ▾ ]   Loại: [ Tất cả ▾ ]                                                          │
│  Giao thiết bị               │                                                                                               │
│  Nhận trả thiết bị           │ ┌─────────┬────────┬───────┬─────┬────────────────┬─────────────┬─────────┬─────────────────┐ │
│  Biên bản và thu phạt        │ │ SV      │ Loại   │ Phòng │ STT │ Trạng thái     │ Hạn giữ chỗ │ Đăng ký │ Hành động       │ │
│  Phiếu chưa trả quá 30 ngày  │ ├─────────┼────────┼───────┼─────┼────────────────┼─────────────┼─────────┼─────────────────┤ │
│▶ Danh sách chờ               │ │ 2152…·A │ Laptop │ A1    │ 1   │ ‹Đang chờ›     │ –           │ dd/MM   │ –               │ │
│  Bảo trì và sửa chữa         │ │ 2152…·B │ Laptop │ A1    │ –   │ ‹Đã thông báo› │ dd/MM HH:mm │ dd/MM   │ [ Chuyển lượt ] │ │
│                              │ └─────────┴────────┴───────┴─────┴────────────────┴─────────────┴─────────┴─────────────────┘ │
│                              │ Tổng số: …                                                                                    │
│                              │ Rỗng: "Không có đăng ký chờ"                                                                  │
│                              │                                                                                               │
└──────────────────────────────┴───────────────────────────────────────────────────────────────────────────────────────────────┘
```

Mẫu P-02. Sắp theo thời điểm đăng ký. Nút **Chuyển lượt** chỉ ở dòng Đã thông báo và mở hộp xác nhận (P-03) nêu hệ quả không tính vi phạm; đăng ký Đang chờ không có hành động. Giao cho SV từ danh sách chờ thực hiện ở SCR-NV-05, không ở màn hình này.


### SCR-NV-14 Bảo trì và sửa chữa

Nội dung: `UI-Screens.md` 6.14. Khung: mục 2.2. Mẫu bố cục: P-02, P-03, P-04.

```text
┌─────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────┐
│  Quản lý mượn trả thiết bị                                                     Họ tên · Nhân viên quản lý thiết bị  [Đăng xuất] │
├──────────────────────────────┬──────────────────────────────────────────────────────────────────────────────────────────────────┤
│  Trang chủ                   │ Bảo trì và sửa chữa                 [ Làm mới ]                                                  │
│  Phiếu yêu cầu chờ duyệt     │ Phòng: [ Tất cả phòng được cấp quyền ▾ ]                                                         │
│  Giao thiết bị               │ ┌────────────────────┐ ┌────────────────────────────┐                                            │
│  Nhận trả thiết bị           │ │ ▶ Đến hạn bảo trì  │ │   Chờ tiếp nhận sửa chữa   │                                            │
│  Biên bản và thu phạt        │ └────────────────────┘ └────────────────────────────┘                                            │
│  Phiếu chưa trả quá 30 ngày  │ ┌────────────────────┬────────┬───────┬──────────────────┬─────────┬───────────────────────────┐ │
│  Danh sách chờ               │ │ Thiết bị           │ Loại   │ Phòng │ Tình trạng       │ Đến hạn │ Hành động                 │ │
│▶ Bảo trì và sửa chữa         │ ├────────────────────┼────────┼───────┼──────────────────┼─────────┼───────────────────────────┤ │
│                              │ │ LT-001 Laptop Dell │ Laptop │ A1    │ ‹Sẵn sàng›       │ dd/MM   │ [ Lập lịch bảo trì ]      │ │
│                              │ │ CH-002 Chuột       │ Chuột  │ A1    │ ‹Đang được mượn› │ dd/MM   │ Lập lịch sau khi được trả │ │
│                              │ └────────────────────┴────────┴───────┴──────────────────┴─────────┴───────────────────────────┘ │
│                              │ (tab Chờ tiếp nhận: Thiết bị · Phòng · Mã phiếu mượn · [ Tiếp nhận sửa chữa ])                   │
│                              │ ┌────────────────────────────────────────────────────────────────────┐                           │
│                              │ │ Bản ghi vừa lập: BT-000009 · LT-001 · Bảo trì định kỳ · dd/MM/yyyy │                           │
│                              │ │                                           [ Ghi hoàn thành ]       │                           │
│                              │ └────────────────────────────────────────────────────────────────────┘                           │
│                              │                                                                                                  │
└──────────────────────────────┴──────────────────────────────────────────────────────────────────────────────────────────────────┘
```

```text
┌──────────────────────────────────────────────────────────┐
│ Ghi hoàn thành bảo trì                                   │
│ Nội dung thực hiện *   [ ______________ ]                │
│ Kết quả *   (•) Đưa về Sẵn sàng   ( ) Đề xuất thanh lý   │
│ Ngày hoàn thành *   [ dd/MM/yyyy HH:mm ]                 │
│ [ Hủy ]                        [ Ghi hoàn thành ]        │
└──────────────────────────────────────────────────────────┘
```

| Vùng | Quy tắc bố cục |
|---|---|
| Đầu | Tiêu đề, bộ lọc Phòng (như SCR-NV-02), hàng hai tab (ĐX-WF-11) |
| Tab **Đến hạn bảo trì** | Bảng như hình; nút **Lập lịch bảo trì** chỉ ở dòng Sẵn sàng, dòng Đang được mượn ghi "Lập lịch sau khi thiết bị được trả" |
| Tab **Chờ tiếp nhận sửa chữa** | Bảng: mã và tên thiết bị, phòng, mã phiếu mượn liên quan; nút **Tiếp nhận sửa chữa** ở mọi dòng |
| Hộp xác nhận | Lập lịch hoặc tiếp nhận: mẫu P-03 nêu thiết bị, loại thực hiện, hệ quả |
| Khối bản ghi vừa lập | Hiện dưới bảng sau khi lập thành công, kèm nút **Ghi hoàn thành**; chỉ tồn tại trong phiên làm việc của màn hình (ĐX-UI-11) |
| Hộp thoại **Ghi hoàn thành** | Hình dưới: hộp thoại biểu mẫu (P-04), sau đó hộp xác nhận (P-03) nêu hệ quả của kết quả đã chọn |
| Không có | Thông tin nhà cung cấp hoặc bảo hành |



---

## 7. Wireframe màn hình của quản trị viên (web)

Mọi màn hình trong mục này theo khung 2.3. Quản trị viên không bị giới hạn theo phòng nên không có bộ lọc "phòng được cấp quyền".


### SCR-QT-01 Trang chủ quản trị viên

Nội dung: `UI-Screens.md` 7.1. Khung: mục 2.3.

```text
┌──────────────────────────────────────────────────────────────────────────────────────────────────────────┐
│  Quản lý mượn trả thiết bị                                           Họ tên · Quản trị viên  [Đăng xuất] │
├──────────────────────────────┬───────────────────────────────────────────────────────────────────────────┤
│▶ Trang chủ                   │ Trang chủ                                                                 │
│  Danh mục                    │ Xin chào, Họ tên                                                          │
│    Phòng thiết bị            │                                                                           │
│    Loại thiết bị             │ Danh mục                                                                  │
│    Thiết bị                  │ ┌────────────────────┐ ┌────────────────────┐ ┌────────────────────┐      │
│  Cấu hình                    │ │ Phòng thiết bị     │ │ Loại thiết bị      │ │ Thiết bị           │      │
│    Quy định mượn             │ │                    │ │                    │ │                    │      │
│    Phí trả trễ               │ └────────────────────┘ └────────────────────┘ └────────────────────┘      │
│    Khung phạt                │ Cấu hình                                                                  │
│    Tham số hệ thống          │ ┌────────────────────┐ ┌────────────────────┐ ┌────────────────────┐      │
│  Tài khoản nhân viên         │ │ Quy định mượn      │ │ Phí trả trễ        │ │ Khung phạt         │      │
│  Biên bản vi phạm            │ │                    │ │                    │ │                    │      │
│  Thanh lý                    │ └────────────────────┘ └────────────────────┘ └────────────────────┘      │
│  Báo cáo                     │ ┌────────────────────┐                                                    │
│                              │ │ Tham số hệ thống   │                                                    │
│                              │ │                    │                                                    │
│                              │ └────────────────────┘                                                    │
│                              │ ┌────────────────────┐ ┌────────────────────┐ ┌────────────────────┐      │
│                              │ │ Tài khoản          │ │ Biên bản           │ │ Thanh lý           │      │
│                              │ │ nhân viên          │ │ vi phạm            │ │                    │      │
│                              │ └────────────────────┘ └────────────────────┘ └────────────────────┘      │
│                              │ ┌────────────────────┐                                                    │
│                              │ │ Báo cáo            │                                                    │
│                              │ │                    │                                                    │
│                              │ └────────────────────┘                                                    │
│                              │                                                                           │
└──────────────────────────────┴───────────────────────────────────────────────────────────────────────────┘
```

Lối vào xếp lưới ba cột, chia theo menu bên: nhóm **Danh mục** (3 lối vào), nhóm **Cấu hình** (4 lối vào), rồi các mục đơn lẻ không có tiêu đề nhóm (Tài khoản nhân viên, Biên bản vi phạm, Thanh lý, Báo cáo). Không có lối vào duyệt, giao, nhận, lập biên bản, bảo trì.


### SCR-QT-02 Phòng thiết bị

Nội dung: `UI-Screens.md` 7.2. Khung: mục 2.3. Mẫu bố cục: P-02, P-04.

```text
┌──────────────────────────────────────────────────────────────────────────────────────────────────────────┐
│  Quản lý mượn trả thiết bị                                           Họ tên · Quản trị viên  [Đăng xuất] │
├──────────────────────────────┬───────────────────────────────────────────────────────────────────────────┤
│  Trang chủ                   │ Phòng thiết bị             [ Làm mới ]            [ + Thêm phòng ]        │
│  Danh mục                    │                                                                           │
│▶   Phòng thiết bị            │ ┌───────────┬─────────┬───────────┐                                       │
│    Loại thiết bị             │ │ Tên phòng │ Cơ sở   │ Hành động │                                       │
│    Thiết bị                  │ ├───────────┼─────────┼───────────┤                                       │
│  Cấu hình                    │ │ A1        │ Cơ sở 1 │ [ Sửa ]   │                                       │
│    Quy định mượn             │ │ …         │ …       │ [ Sửa ]   │                                       │
│    Phí trả trễ               │ └───────────┴─────────┴───────────┘                                       │
│    Khung phạt                │ Tổng số: …   ‹ 1 2 ›                                                      │
│    Tham số hệ thống          │ (rỗng: "Chưa có phòng thiết bị")                                          │
│  Tài khoản nhân viên         │                                                                           │
│  Biên bản vi phạm            │                                                                           │
│  Thanh lý                    │                                                                           │
│  Báo cáo                     │                                                                           │
│                              │                                                                           │
└──────────────────────────────┴───────────────────────────────────────────────────────────────────────────┘
```

```text
┌──────────────────────────────────────────┐
│ Thêm phòng / Sửa phòng                   │
│ Tên phòng *  [ ________ ]                │
│ Cơ sở *      [ ________ ]                │
│ [ Hủy ]               [ Lưu ]            │
└──────────────────────────────────────────┘
```

Mẫu P-02 và hộp thoại biểu mẫu P-04 (hình dưới). Mỗi dòng chỉ có hành động **Sửa**; không có Xóa.


### SCR-QT-03 Loại thiết bị

Nội dung: `UI-Screens.md` 7.3. Khung: mục 2.3. Mẫu bố cục: P-02, P-03, P-04.

```text
┌────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────┐
│  Quản lý mượn trả thiết bị                                                             Họ tên · Quản trị viên  [Đăng xuất] │
├──────────────────────────────┬─────────────────────────────────────────────────────────────────────────────────────────────┤
│  Trang chủ                   │ Loại thiết bị         [ Làm mới ]                                                           │
│  Danh mục                    │ Ngưng sử dụng: [ Tất cả ▾ ]                              [ + Thêm loại ]                    │
│    Phòng thiết bị            │ ┌───────────┬───────┬───────┬───────────────┬───────────────────────────┬─────────────────┐ │
│▶   Loại thiết bị             │ │ Tên loại  │ Hãng  │ Mô tả │ Chu kỳ (ngày) │ Phí trễ/ngày              │ Hành động       │ │
│    Thiết bị                  │ ├───────────┼───────┼───────┼───────────────┼───────────────────────────┼─────────────────┤ │
│  Cấu hình                    │ │ Laptop    │ Dell  │ …     │ 180           │ 50.000                    │ [ Sửa ] [ Xóa ] │ │
│    Quy định mượn             │ │ Máy chiếu │ Epson │ …     │ 90            │ 30.000 ‹Đã ngưng sử dụng› │ [ Sửa ] [ Xóa ] │ │
│    Phí trả trễ               │ └───────────┴───────┴───────┴───────────────┴───────────────────────────┴─────────────────┘ │
│    Khung phạt                │ Tổng số: …   ‹ 1 2 ›                                                                        │
│    Tham số hệ thống          │                                                                                             │
│  Tài khoản nhân viên         │                                                                                             │
│  Biên bản vi phạm            │                                                                                             │
│  Thanh lý                    │                                                                                             │
│  Báo cáo                     │                                                                                             │
│                              │                                                                                             │
└──────────────────────────────┴─────────────────────────────────────────────────────────────────────────────────────────────┘
```

```text
┌──────────────────────────────────────────┐  ┌───────────────────────────────────────┐
│ Thêm loại / Sửa loại                     │  │ Không xóa được loại này               │
│ Tên loại *  [ ______ ]                   │  │ Loại này đã có lịch sử, chỉ được      │
│ Hãng  [ ______ ]                         │  │ ngưng sử dụng. Lịch sử giữ lại, không │
│ Mô tả  [ ______ ]                        │  │ còn dùng cho thiết bị mới.            │
│ Chu kỳ bảo trì (ngày) *  [ __ ]          │  │ [ Hủy ]       [ Ngưng sử dụng ]       │
│ Phí trả trễ/ngày (VNĐ) *  [ __ ]         │  └───────────────────────────────────────┘
│   (khi sửa: chỉ đọc + <Đổi phí trả trễ ở │                                           
│    Cấu hình>)                            │                                           
│ [ Hủy ]          [ Lưu ]                 │                                           
└──────────────────────────────────────────┘                                           
```

Mẫu P-02; bộ lọc Ngưng sử dụng (Tất cả, Đang sử dụng, Đã ngưng sử dụng). Hộp thoại biểu mẫu (hình dưới, trái; phí trả trễ chỉ nhập khi thêm). Luồng xóa: **Xóa** → hộp xác nhận (P-03) → nếu `DA_CO_LICH_SU_CHI_NGUNG_SU_DUNG` thì hộp đề nghị **Ngưng sử dụng** (hình dưới, phải) → hộp xác nhận nêu loại vẫn giữ lịch sử. Loại đã ngưng sử dụng không còn đề nghị này.


### SCR-QT-04 Thiết bị

Nội dung: `UI-Screens.md` 7.4. Khung: mục 2.3. Mẫu bố cục: P-02, P-03, P-04, P-05.

```text
┌─────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────┐
│  Quản lý mượn trả thiết bị                                                                                                      Họ tên · Quản trị viên  [Đăng xuất] │
├──────────────────────────────┬──────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────┤
│  Trang chủ                   │ Thiết bị           [ Làm mới ]                                                                                                       │
│  Danh mục                    │ Tên: [ ____ ] Loại: [ ▾ ] Phòng: [ ▾ ] Tình trạng: [ Tất cả ▾ ]   [ + Nhập kho thiết bị ]                                            │
│    Phòng thiết bị            │ ┌────────┬─────────────┬────────┬───────┬───────┬───────────────────────────────┬────────────┬────────────────────────────────────┐  │
│    Loại thiết bị             │ │ Mã     │ Tên         │ Loại   │ Phòng │ Kệ/Tủ │ Tình trạng                    │ Giá trị    │ Hành động                          │  │
│▶   Thiết bị                  │ ├────────┼─────────────┼────────┼───────┼───────┼───────────────────────────────┼────────────┼────────────────────────────────────┤  │
│  Cấu hình                    │ │ LT-001 │ Laptop Dell │ Laptop │ A1    │ 2/3   │ ‹Sẵn sàng›                    │ 12.000.000 │ [ Thao tác ▾ ]                     │  │
│    Quy định mượn             │ │ LT-009 │ Máy cũ      │ Laptop │ A1    │ –     │ ‹Sẵn sàng› ‹Đã ngưng sử dụng› │ 5.000.000  │ [ Thao tác ▾ ]                     │  │
│    Phí trả trễ               │ └────────┴─────────────┴────────┴───────┴───────┴───────────────────────────────┴────────────┴────────────────────────────────────┘  │
│    Khung phạt                │ Tổng số: …   ‹ 1 2 ›                                                                                                                 │
│    Tham số hệ thống          │                                                                                                                                      │
│  Tài khoản nhân viên         │                                                                                                                                      │
│  Biên bản vi phạm            │                                                                                                                                      │
│  Thanh lý                    │                                                                                                                                      │
│  Báo cáo                     │                                                                                                                                      │
│                              │                                                                                                                                      │
└──────────────────────────────┴──────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────┘
```

```text
┌────────────────────────────────────────────────┐  ┌────────────────────────────────┐
│ Nhập kho thiết bị / Sửa thiết bị      [✕]      │  │ Cập nhật giá trị               │
│ Mã thiết bị * (chỉ đọc khi sửa)  [ ____ ]      │  │ Giá trị hiện hành: … (chỉ đọc) │
│ Tên thiết bị *  [ ____ ]                       │  │ Giá trị mới (VNĐ) *  [ ____ ]  │
│ Loại *  [ ▾ ]      Phòng *  [ ▾ ]              │  │ Dùng để tính bồi thường.       │
│ Mô tả  [ ____ ]                                │  │ [ Hủy ]        [ Lưu ]         │
│ Thông số kỹ thuật  [ ____ ]                    │  └────────────────────────────────┘
│ Ngày nhập kho *  [ dd/MM/yyyy ]                │                                    
│ Thời hạn bảo hành  [ dd/MM/yyyy ]              │                                    
│ Kệ  [ __ ]   Tủ  [ __ ]                        │                                    
│ Nhà cung cấp  [x] NCC 1  [ ] NCC 2 …           │                                    
│ Ảnh (P-05; ảnh có sẵn kèm nút gỡ)              │                                    
│ Giá trị (VNĐ) *  [ ____ ] (khi sửa: chỉ đọc    │                                    
│   + [ Cập nhật giá trị ])                      │                                    
│ (không có ô Tình trạng)                        │                                    
│ ──────────────────────────────────────         │                                    
│ [ Hủy ]                        [ Lưu ]         │                                    
└────────────────────────────────────────────────┘                                    
```

Mẫu P-02 với bộ lọc tên, loại, phòng, tình trạng. Biểu mẫu thiết bị là **ngăn trượt từ bên phải** (hình dưới, trái; ĐX-WF-05); **Cập nhật giá trị** là hộp thoại nhỏ (phải). Xóa và ngưng sử dụng như SCR-QT-03. Mã thiết bị chỉ nhập khi thêm; lỗi `MA_THIET_BI_TRUNG` đặt dưới ô mã. Menu **Thao tác** của mỗi dòng giữ đủ ba hành động: **Sửa**, **Cập nhật giá trị**, **Xóa**.


### SCR-QT-05 Cấu hình: Quy định mượn theo nhóm sinh viên

Nội dung: `UI-Screens.md` 7.5. Khung: mục 2.3. Mẫu bố cục: P-03, P-04.

```text
┌──────────────────────────────────────────────────────────────────────────────────────────────────────────┐
│  Quản lý mượn trả thiết bị                                           Họ tên · Quản trị viên  [Đăng xuất] │
├──────────────────────────────┬───────────────────────────────────────────────────────────────────────────┤
│  Trang chủ                   │ Quy định mượn theo nhóm sinh viên      [ Làm mới ]                        │
│  Danh mục                    │ (không có nút thêm hoặc xóa nhóm)                                         │
│    Phòng thiết bị            │ ┌────────────────────────────────────────────────────────────────────┐    │
│    Loại thiết bị             │ │ Nhóm: Sinh viên đại học                                            │    │
│    Thiết bị                  │ │ Số thiết bị tối đa: 3   Thời gian tối đa: 24 giờ                   │    │
│  Cấu hình                    │ │ Ngoại lệ theo loại: Máy chiếu · 48 giờ                             │    │
│▶   Quy định mượn             │ │                                           [ Sửa ]                  │    │
│    Phí trả trễ               │ └────────────────────────────────────────────────────────────────────┘    │
│    Khung phạt                │ ┌────────────────────────────────────────────────────────────────────┐    │
│    Tham số hệ thống          │ │ Nhóm: Sinh viên sau đại học                                        │    │
│  Tài khoản nhân viên         │ │ Số thiết bị tối đa: 3   Thời gian tối đa: 24 giờ                   │    │
│  Biên bản vi phạm            │ │ Ngoại lệ theo loại: Máy chiếu · 48 giờ                             │    │
│  Thanh lý                    │ │                                           [ Sửa ]                  │    │
│  Báo cáo                     │ └────────────────────────────────────────────────────────────────────┘    │
│                              │                                                                           │
└──────────────────────────────┴───────────────────────────────────────────────────────────────────────────┘
```

```text
┌──────────────────────────────────────────────────────┐
│ Sửa quy định nhóm …                                  │
│ Số thiết bị tối đa mượn cùng lúc *  [ __ ]           │
│ Thời gian mượn tối đa (giờ) *      [ __ ]            │
│ Ngoại lệ thời gian theo loại                         │
│   [ Loại ▾ ]  [ giờ __ ]  [ Xóa dòng ]               │
│   [ + Thêm dòng ]                                    │
│ [ Hủy ]                        [ Lưu ]               │
└──────────────────────────────────────────────────────┘
```

Mỗi nhóm một thẻ xếp dọc, nút **Sửa** ở góc dưới phải. Biểu mẫu sửa là hộp thoại (hình dưới); **Lưu** mở hộp xác nhận (P-03) nêu việc thay thế toàn bộ quy định của nhóm và quy tắc không hồi tố.


### SCR-QT-06 Cấu hình: Phí trả trễ theo loại thiết bị

Nội dung: `UI-Screens.md` 7.6. Khung: mục 2.3. Mẫu bố cục: P-02, P-03, P-04.

```text
┌──────────────────────────────────────────────────────────────────────────────────────────────────────────┐
│  Quản lý mượn trả thiết bị                                           Họ tên · Quản trị viên  [Đăng xuất] │
├──────────────────────────────┬───────────────────────────────────────────────────────────────────────────┤
│  Trang chủ                   │ Phí trả trễ theo loại thiết bị      [ Làm mới ]                           │
│  Danh mục                    │                                                                           │
│    Phòng thiết bị            │ ┌───────────┬────────────────────────┬─────────────┐                      │
│    Loại thiết bị             │ │ Tên loại  │ Phí trả trễ/ngày (VNĐ) │ Hành động   │                      │
│    Thiết bị                  │ ├───────────┼────────────────────────┼─────────────┤                      │
│  Cấu hình                    │ │ Laptop    │ 50.000                 │ [ Sửa phí ] │                      │
│    Quy định mượn             │ │ Máy chiếu │ 30.000                 │ [ Sửa phí ] │                      │
│▶   Phí trả trễ               │ └───────────┴────────────────────────┴─────────────┘                      │
│    Khung phạt                │ (rỗng: "Chưa có loại thiết bị")                                           │
│    Tham số hệ thống          │                                                                           │
│  Tài khoản nhân viên         │                                                                           │
│  Biên bản vi phạm            │                                                                           │
│  Thanh lý                    │                                                                           │
│  Báo cáo                     │                                                                           │
│                              │                                                                           │
└──────────────────────────────┴───────────────────────────────────────────────────────────────────────────┘
```

```text
┌──────────────────────────────────────────┐
│ Sửa phí trả trễ                          │
│ Loại: Laptop                             │
│ Phí trả trễ mỗi ngày (VNĐ) *  [ ____ ]   │
│ [ Hủy ]               [ Lưu ]            │
└──────────────────────────────────────────┘
```

Bảng đơn giản không phân trang trong hình; nếu danh sách loại nhiều trang thì dùng phân trang (P-02). **Sửa phí** mở hộp thoại biểu mẫu một trường (hình dưới, P-04), **Lưu** mở hộp xác nhận (P-03) nêu quy tắc không hồi tố.


### SCR-QT-07 Cấu hình: Khung phạt hư hỏng

Nội dung: `UI-Screens.md` 7.7. Khung: mục 2.3. Mẫu bố cục: P-02, P-03, P-04.

```text
┌──────────────────────────────────────────────────────────────────────────────────────────────────────────┐
│  Quản lý mượn trả thiết bị                                           Họ tên · Quản trị viên  [Đăng xuất] │
├──────────────────────────────┬───────────────────────────────────────────────────────────────────────────┤
│  Trang chủ                   │ Khung phạt hư hỏng   [ Làm mới ]             [ + Thêm khung phạt ]        │
│  Danh mục                    │                                                                           │
│    Phòng thiết bị            │ ┌───────────┬───────────┬──────────────┬───────────────────────┐          │
│    Loại thiết bị             │ │ Tên khung │ Tỷ lệ (%) │ Trạng thái   │ Hành động             │          │
│    Thiết bị                  │ ├───────────┼───────────┼──────────────┼───────────────────────┤          │
│  Cấu hình                    │ │ Nhẹ       │ 15        │ Đang sử dụng │ [ Thao tác ▾ ]        │          │
│    Quy định mượn             │ │ Vừa       │ 40        │ Đang sử dụng │ [ Thao tác ▾ ]        │          │
│    Phí trả trễ               │ │ Cũ        │ 10        │ Đã tắt       │ [ Sửa ][ Xóa ]        │          │
│▶   Khung phạt                │ └───────────┴───────────┴──────────────┴───────────────────────┘          │
│    Tham số hệ thống          │ (rỗng: "Chưa có khung phạt")                                              │
│  Tài khoản nhân viên         │                                                                           │
│  Biên bản vi phạm            │                                                                           │
│  Thanh lý                    │                                                                           │
│  Báo cáo                     │                                                                           │
│                              │                                                                           │
└──────────────────────────────┴───────────────────────────────────────────────────────────────────────────┘
```

```text
┌─────────────────────────────┐  ┌─────────────────────────────────────┐
│ Thêm / Sửa khung phạt       │  │ Không xóa được khung phạt này       │
│ Tên khung *  [ ______ ]     │  │ Khung đã được dùng, chỉ được tắt.   │
│ Tỷ lệ phần trăm *  [ __ ] % │  │ [ Hủy ]          [ Tắt khung phạt ] │
│ [ Hủy ]       [ Lưu ]       │  └─────────────────────────────────────┘
└─────────────────────────────┘                                         
```

Khung đang sử dụng có menu **Thao tác** gồm **Sửa**, **Tắt**, **Xóa**; khung đã tắt có hai nút **Sửa**, **Xóa** hiển thị trực tiếp. Nút **Tắt** chỉ ở khung đang sử dụng; khung đã tắt không còn trong danh sách chọn ở màn hình nhận trả. Hộp biểu mẫu (hình dưới, trái) và hộp đề nghị tắt khi `KHUNG_PHAT_DA_DUNG_KHONG_XOA` (phải). Mọi lưu cần hộp xác nhận (P-03) nêu quy tắc không hồi tố.


### SCR-QT-08 Cấu hình: Tham số hệ thống

Nội dung: `UI-Screens.md` 7.8. Khung: mục 2.3. Mẫu bố cục: P-03.

```text
┌──────────────────────────────────────────────────────────────────────────────────────────────────────────┐
│  Quản lý mượn trả thiết bị                                           Họ tên · Quản trị viên  [Đăng xuất] │
├──────────────────────────────┬───────────────────────────────────────────────────────────────────────────┤
│  Trang chủ                   │ Tham số hệ thống                                                          │
│  Danh mục                    │ ⚠ Chưa thiết lập giờ quầy: NV chưa lập được phiếu mượn                    │
│    Phòng thiết bị            │   (chỉ khi giờ mở và giờ đóng đều trống; hiển thị "Chưa thiết lập")       │
│    Loại thiết bị             │ ┌────────────────────────────────────────────────────────────────────┐    │
│    Thiết bị                  │ │ Số ngày quá hạn để khóa quyền mượn (ngày)  [ __ ]                  │    │
│  Cấu hình                    │ │ Thời gian giữ chỗ (phút)                  [ __ ]                   │    │
│    Quy định mượn             │ │ Ân hạn trả trễ (phút)                    [ __ ]                    │    │
│    Phí trả trễ               │ │ Số giờ gia hạn tối đa (giờ)              [ __ ]                    │    │
│    Khung phạt                │ │ Ngưỡng vi phạm mỗi học kỳ (lần)          [ __ ]                    │    │
│▶   Tham số hệ thống          │ │ Số lần nhập sai mật khẩu tối đa (nhân viên, quản trị viên) (lần)   │    │
│  Tài khoản nhân viên         │ │   [ __ ]                                                           │    │
│  Biên bản vi phạm            │ │ Giờ mở cửa quầy (HH:mm)                  [ __:__ ]                 │    │
│  Thanh lý                    │ │ Giờ đóng cửa quầy (HH:mm)                [ __:__ ]                 │    │
│  Báo cáo                     │ └────────────────────────────────────────────────────────────────────┘    │
│                              │ (lỗi cạnh từng trường)                                                    │
│                              │                                                                           │
│                              │                                        [ Lưu ]                            │
│                              │                                                                           │
└──────────────────────────────┴───────────────────────────────────────────────────────────────────────────┘
```

Một biểu mẫu duy nhất, tám trường xếp dọc theo thứ tự ở `UI-Screens.md` 7.8, **một nút Lưu cho cả bộ** ở cuối (không có nút lưu từng trường). Cảnh báo giờ quầy ở đầu vùng nội dung. **Lưu** mở hộp xác nhận (P-03) nêu giá trị mới chỉ áp dụng cho giao dịch phát sinh sau.


### SCR-QT-09 Tài khoản nhân viên

Nội dung: `UI-Screens.md` 7.9. Khung: mục 2.3. Mẫu bố cục: P-02, P-03, P-04.

```text
┌─────────────────────────────────────────────────────────────────────────────────────────────────────────────────────┐
│  Quản lý mượn trả thiết bị                                                 Họ tên · Quản trị viên  [Đăng xuất]      │
├──────────────────────────────┬──────────────────────────────────────────────────────────────────────────────────────┤
│  Trang chủ                   │ Tài khoản nhân viên     [ Làm mới ]          [ + Tạo tài khoản ]                     │
│  Danh mục                    │                                                                                      │
│    Phòng thiết bị            │ ┌───────┬───────────────┬────────┬───────┬─────────────┬────────┬────────────────┐   │
│    Loại thiết bị             │ │ Mã NV │ Tên đăng nhập │ Họ tên │ Email │ Trạng thái  │ Phòng  │ Hành động      │   │
│    Thiết bị                  │ ├───────┼───────────────┼────────┼───────┼─────────────┼────────┼────────────────┤   │
│  Cấu hình                    │ │ NV01  │ nv01          │ Họ tên │ …     │ ‹Hoạt động› │ A1, B2 │ [ Thao tác ▾ ] │   │
│    Quy định mượn             │ │ …     │ …             │ …      │ …     │ ‹Bị khóa›   │ …      │ [ Thao tác ▾ ] │   │
│    Phí trả trễ               │ └───────┴───────────────┴────────┴───────┴─────────────┴────────┴────────────────┘   │
│    Khung phạt                │ Tổng số: …   ‹ 1 2 ›                                                                 │
│    Tham số hệ thống          │ Menu [ Thao tác ▾ ]: Sửa · Cấp quyền phòng · Khóa · Mở khóa · Đặt lại mật khẩu       │
│▶ Tài khoản nhân viên         │                                                                                      │
│  Biên bản vi phạm            │                                                                                      │
│  Thanh lý                    │                                                                                      │
│  Báo cáo                     │                                                                                      │
│                              │                                                                                      │
└──────────────────────────────┴──────────────────────────────────────────────────────────────────────────────────────┘
```

```text
┌──────────────────────────────────┐  ┌─────────────────────────┐
│ Tạo tài khoản                    │  │ Cấp quyền phòng         │
│ Tên đăng nhập *  [ ____ ]        │  │ [x] A1 · Cơ sở 1        │
│ Mật khẩu * (≥ 8 ký tự)  [ ____ ] │  │ [ ] B2 · Cơ sở 2        │
│ Họ tên *  [ ____ ]               │  │ …                       │
│ Mã nhân viên *  [ ____ ]         │  │ [ Hủy ]         [ Lưu ] │
│ Email  [ ____ ]                  │  └─────────────────────────┘
│ [ Hủy ]         [ Tạo ]          │                             
└──────────────────────────────────┘                             
```

Mẫu P-02. Cột Hành động là menu có nút `[ Thao tác ▾ ]` mở danh sách hành động của dòng (ĐX-WF-13); **Khóa** hiện khi trạng thái Hoạt động hoặc Tạm khóa đăng nhập, **Mở khóa** hiện khi Bị khóa hoặc Tạm khóa đăng nhập. Hộp tạo tài khoản (trái) và hộp chọn nhiều phòng (phải) theo mẫu P-04; **Sửa** chỉ có họ tên và email (tên đăng nhập, mã nhân viên chỉ đọc); **Đặt lại mật khẩu** có một ô mật khẩu mới. Khóa, Mở khóa, Đặt lại mật khẩu và lưu quyền phòng cần hộp xác nhận (P-03).


### SCR-QT-10 Biên bản vi phạm (hủy hoặc điều chỉnh)

Nội dung: `UI-Screens.md` 7.10. Khung: mục 2.3. Mẫu bố cục: P-02, P-03.

```text
┌──────────────────────────────────────────────────────────────────────────────────────────────────────────┐
│  Quản lý mượn trả thiết bị                                           Họ tên · Quản trị viên  [Đăng xuất] │
├──────────────────────────────┬───────────────────────────────────────────────────────────────────────────┤
│  Trang chủ                   │ Biên bản vi phạm       [ Làm mới ]                                        │
│  Danh mục                    │ Mã SV: [ ____ ]   Trạng thái: [ Tất cả ▾ ]                                │
│    Phòng thiết bị            │ ┌────────┬────────┬───────┬──────────┬───────────┬────────────┐           │
│    Loại thiết bị             │ │ Mã BB  │ Phiếu  │ Mã SV │ Ngày lập │ Tổng tiền │ Trạng thái │           │
│    Thiết bị                  │ ├────────┼────────┼───────┼──────────┼───────────┼────────────┤           │
│  Cấu hình                    │ │ BB-012 │ PM-045 │ 2152… │ dd/MM    │ 190.000   │ ‹Chưa nộp› │           │
│    Quy định mượn             │ │ …      │ …      │ …     │ …        │ …         │ …          │           │
│    Phí trả trễ               │ └────────┴────────┴───────┴──────────┴───────────┴────────────┘           │
│    Khung phạt                │ Tổng số: …   ‹ 1 2 ›                                                      │
│    Tham số hệ thống          │ ┌─ Chi tiết biên bản ─────────────────────────────────────────────────┐   │
│  Tài khoản nhân viên         │ │ BB-012 · PM-045 · SV 2152… · dd/MM/yyyy · ‹Chưa nộp›                │   │
│▶ Biên bản vi phạm            │ │ Khoản: LT-001 · Trả trễ · 3 ngày · [ 150.000 ] (sửa khi Điều chỉnh) │   │
│  Thanh lý                    │ │ Tổng tiền: 190.000 VNĐ (hệ thống tính lại sau khi lưu)              │   │
│  Báo cáo                     │ │                          [ Hủy biên bản ]   [ Điều chỉnh ]          │   │
│                              │ └─────────────────────────────────────────────────────────────────────┘   │
│                              │                                                                           │
└──────────────────────────────┴───────────────────────────────────────────────────────────────────────────┘
```

Mẫu P-02; chọn một dòng mở **chi tiết biên bản** ngay dưới bảng (khối cùng trang, không chuyển màn hình; ĐX-WF-16). Trong chi tiết: **Hủy biên bản** bật với Chưa nộp hoặc Đã nộp; **Điều chỉnh** chỉ bật với Chưa nộp và chuyển số tiền từng khoản thành ô sửa (không thêm hoặc xóa khoản). Cả hai cần hộp xác nhận (P-03), hộp Hủy nêu hệ quả về quyền mượn; không có ô lý do.


### SCR-QT-11 Thanh lý thiết bị

Nội dung: `UI-Screens.md` 7.11. Khung: mục 2.3. Mẫu bố cục: P-02, P-03.

```text
┌───────────────────────────────────────────────────────────────────────────────────────────────────────────────────┐
│  Quản lý mượn trả thiết bị                                                    Họ tên · Quản trị viên  [Đăng xuất] │
├──────────────────────────────┬────────────────────────────────────────────────────────────────────────────────────┤
│  Trang chủ                   │ Thanh lý thiết bị      [ Làm mới ]                                                 │
│  Danh mục                    │ Danh sách chờ quyết định thanh lý           [ Chọn thêm từ danh mục ]              │
│    Phòng thiết bị            │ ┌─────┬────────────────────┬────────┬───────┬──────────┬────────────┬────────────┐ │
│    Loại thiết bị             │ │     │ Thiết bị           │ Loại   │ Phòng │ Nội dung │ Hoàn thành │ Giá trị    │ │
│    Thiết bị                  │ ├─────┼────────────────────┼────────┼───────┼──────────┼────────────┼────────────┤ │
│  Cấu hình                    │ │ [x] │ LT-001 Laptop Dell │ Laptop │ A1    │ …        │ dd/MM      │ 12.000.000 │ │
│    Quy định mượn             │ │ [ ] │ …                  │ …      │ …     │ …        │ …          │ …          │ │
│    Phí trả trễ               │ └─────┴────────────────────┴────────┴───────┴──────────┴────────────┴────────────┘ │
│    Khung phạt                │ (rỗng: "Không có thiết bị chờ quyết định thanh lý")                                │
│    Tham số hệ thống          │ ┌─ Danh sách đã chọn ────────────────────────────────────────────────┐             │
│  Tài khoản nhân viên         │ │ Đã chọn (2): LT-001 · LT-007            [ Bỏ chọn ]                │             │
│  Biên bản vi phạm            │ └────────────────────────────────────────────────────────────────────┘             │
│▶ Thanh lý                    │ Ngày thanh lý *  [ dd/MM/yyyy ]                                                    │
│  Báo cáo                     │                                                                                    │
│                              │                                 [ Lập quyết định thanh lý ]                        │
│                              │                                                                                    │
└──────────────────────────────┴────────────────────────────────────────────────────────────────────────────────────┘
```

```text
┌───────────────────────────────────────────────┐  ┌──────────────────────────────────┐
│ Chọn thêm từ danh mục                         │  │ Quyết định thanh lý              │
│ Tên [ __ ] Loại [ ▾ ] Phòng [ ▾ ]             │  │ QD-000003 · dd/MM/yyyy           │
│ Tình trạng [ ▾ ]                              │  │ LT-001 · LT-007 · ‹Đã thanh lý›  │
│ [ ] LT-003 · Laptop · ‹Sẵn sàng›              │  │ [ Đóng ]                         │
│ [ ] LT-008 · Chuột · ‹Đang bảo trì/sửa chữa›  │  └──────────────────────────────────┘
│ (chỉ chọn được thiết bị ở hai tình trạng này) │                                      
│ [ Đóng ]       [ Thêm vào danh sách ]         │                                      
└───────────────────────────────────────────────┘                                      
```

| Vùng | Quy tắc bố cục |
|---|---|
| Danh sách chờ quyết định | Bảng có ô chọn ở cột đầu; nút **Chọn thêm từ danh mục** ở hàng tiêu đề, mở bảng chọn (hình dưới, trái) |
| Danh sách đã chọn | Dưới bảng, tổng hợp thiết bị đã chọn từ cả hai nguồn, có thể bỏ chọn |
| Ngày thanh lý và nút | Cuối vùng nội dung; **Lập quyết định thanh lý** bật khi đã chọn ít nhất một thiết bị và có ngày |
| Hộp xác nhận | Mẫu P-03 liệt kê thiết bị và ngày, nêu hệ quả (Đã thanh lý, giữ lịch sử, không còn khả dụng) |
| Kết quả | Hộp quyết định vừa lập (hình dưới, phải); danh sách chờ tải lại. Nút điều khiển chuẩn của hai hộp này theo ĐX-WF-19 |


### SCR-QT-12 Báo cáo

Nội dung: `UI-Screens.md` 7.12. Khung: mục 2.3.

```text
┌──────────────────────────────────────────────────────────────────────────────────────────────────────────────┐
│  Quản lý mượn trả thiết bị                                               Họ tên · Quản trị viên  [Đăng xuất] │
├──────────────────────────────┬───────────────────────────────────────────────────────────────────────────────┤
│  Trang chủ                   │ Báo cáo                                                                       │
│  Danh mục                    │ Báo cáo: [ Sinh viên mượn quá hạn ▾ ]    (6 báo cáo)                          │
│    Phòng thiết bị            │ Từ ngày: [ dd/MM/yyyy ]  Đến ngày: [ dd/MM/yyyy ]                             │
│    Loại thiết bị             │ Mốc lọc: ngày giờ hẹn trả   (ẩn bộ chọn thời gian với báo cáo tình trạng kho) │
│    Thiết bị                  │ [ Xem báo cáo ]   [ Xuất Excel ]   [ Xuất CSV ]                               │
│  Cấu hình                    │                                                                               │
│    Quy định mượn             │ ┌─ Bảng báo cáo (cuộn ngang khi nhiều cột) ──────────────────────────┐        │
│    Phí trả trễ               │ │ Tạo lúc: dd/MM/yyyy HH:mm                                          │        │
│    Khung phạt                │ │ ┌──────────┬───────┬────────┬─────────────┬───┐                    │        │
│    Tham số hệ thống          │ │ │ Mã phiếu │ Mã SV │ Họ tên │ Số ngày trễ │ … │                    │        │
│  Tài khoản nhân viên         │ │ ├──────────┼───────┼────────┼─────────────┼───┤                    │        │
│  Biên bản vi phạm            │ │ │ PM-031   │ 2152… │ A      │ 45          │ … │                    │        │
│  Thanh lý                    │ │ └──────────┴───────┴────────┴─────────────┴───┘                    │        │
│▶ Báo cáo                     │ └────────────────────────────────────────────────────────────────────┘        │
│                              │ (không có dữ liệu: bảng trống + "Không có dữ liệu trong kỳ đã chọn")          │
│                              │                                                                               │
└──────────────────────────────┴───────────────────────────────────────────────────────────────────────────────┘
```

| Vùng | Quy tắc bố cục |
|---|---|
| Hàng điều khiển | Trên cùng: bộ chọn báo cáo, bộ chọn thời gian kèm **mốc lọc** của báo cáo đang chọn (ở cạnh bộ chọn), ba nút **Xem báo cáo**, **Xuất Excel**, **Xuất CSV** |
| Bảng báo cáo | Dưới hàng điều khiển; cột theo báo cáo đang chọn (`UI-Screens.md` 7.12); bảng nhiều cột cuộn ngang trong khung bảng, không làm cuộn ngang cả trang (ĐX-WF-17); hiển thị thời điểm tạo (`taoLuc`) |
| Lỗi thời gian | Đến ngày sớm hơn từ ngày: lỗi đặt ngay dưới bộ chọn thời gian, client kiểm tra trước khi gọi |




---

## Phụ lục A. Truy vết màn hình → khung và mẫu bố cục

| Màn hình | Khung | Mẫu dùng thêm |
|---|---|---|
| SCR-CM-01 | riêng, không có thanh điều hướng | – |
| SCR-CM-02 | riêng, không có thanh điều hướng | – |
| SCR-CM-03 | riêng, không có thanh điều hướng | – |
| SCR-SV-01 | mục 2.1, màn hình gốc | – |
| SCR-SV-02 | mục 2.1, màn hình gốc | P-01 |
| SCR-SV-03 | mục 2.1 | – |
| SCR-SV-04 | mục 2.1 | P-03 |
| SCR-SV-05 | mục 2.1 | – |
| SCR-SV-06 | mục 2.1 | P-01 |
| SCR-SV-07 | mục 2.1 | – |
| SCR-SV-08 | mục 2.1 | P-01, P-03 |
| SCR-SV-09 | mục 2.1 | P-01 |
| SCR-SV-10 | mục 2.1 | – |
| SCR-SV-11 | mục 2.1 | P-01 |
| SCR-SV-12 | mục 2.1 | P-01 |
| SCR-SV-13 | mục 2.1, màn hình gốc | P-01 |
| SCR-SV-14 | mục 2.1, màn hình gốc | – |
| DLG-SV-01 | hộp thoại (ĐX-WF-04) | – |
| SCR-NV-01 | mục 2.2 | – |
| SCR-NV-02 | mục 2.2 | P-02 |
| SCR-NV-03 | mục 2.2 | P-03 |
| SCR-NV-04 | mục 2.2 | P-06, P-07, P-08 |
| SCR-NV-05 | mục 2.2 | P-02, P-08 |
| SCR-NV-06 | mục 2.2 | P-03, P-05, P-06, P-08 |
| SCR-NV-07 | mục 2.2 | P-08 |
| SCR-NV-08 | mục 2.2 | P-02, P-06 |
| SCR-NV-09 | mục 2.2 | P-03, P-05, P-06 |
| SCR-NV-10 | mục 2.2 | P-03 |
| SCR-NV-11 | mục 2.2 | P-02, P-03, P-06, P-07 |
| SCR-NV-12 | mục 2.2 | P-03; chân phân trang CMP-19 |
| SCR-NV-13 | mục 2.2 | P-02, P-03 |
| SCR-NV-14 | mục 2.2 | P-02, P-03, P-04 |
| SCR-QT-01 | mục 2.3 | – |
| SCR-QT-02 | mục 2.3 | P-02, P-04 |
| SCR-QT-03 | mục 2.3 | P-02, P-03, P-04 |
| SCR-QT-04 | mục 2.3 | P-02, P-03, P-04, P-05 |
| SCR-QT-05 | mục 2.3 | P-03, P-04 |
| SCR-QT-06 | mục 2.3 | P-02, P-03, P-04 |
| SCR-QT-07 | mục 2.3 | P-02, P-03, P-04 |
| SCR-QT-08 | mục 2.3 | P-03 |
| SCR-QT-09 | mục 2.3 | P-02, P-03, P-04 |
| SCR-QT-10 | mục 2.3 | P-02, P-03 |
| SCR-QT-11 | mục 2.3 | P-02, P-03 |
| SCR-QT-12 | mục 2.3 | – |

## Phụ lục B. Truy vết mẫu bố cục → màn hình

| Mẫu | Tên | Màn hình dùng |
|---|---|---|
| P-01 | Danh sách Android | SCR-SV-02, SCR-SV-06, SCR-SV-08, SCR-SV-09, SCR-SV-11, SCR-SV-12, SCR-SV-13 |
| P-02 | Bảng web có bộ lọc và phân trang | SCR-NV-02, SCR-NV-05, SCR-NV-08, SCR-NV-11, SCR-NV-13, SCR-NV-14, SCR-QT-02, SCR-QT-03, SCR-QT-04, SCR-QT-06, SCR-QT-07, SCR-QT-09, SCR-QT-10, SCR-QT-11 |
| P-03 | Hộp xác nhận | SCR-SV-04, SCR-SV-08, SCR-NV-03, SCR-NV-06, SCR-NV-09, SCR-NV-10, SCR-NV-11, SCR-NV-12, SCR-NV-13, SCR-NV-14, SCR-QT-03, SCR-QT-04, SCR-QT-05, SCR-QT-06, SCR-QT-07, SCR-QT-08, SCR-QT-09, SCR-QT-10, SCR-QT-11 |
| P-04 | Hộp thoại biểu mẫu và ngăn biểu mẫu | SCR-NV-14, SCR-QT-02, SCR-QT-03, SCR-QT-04, SCR-QT-05, SCR-QT-06, SCR-QT-07, SCR-QT-09 |
| P-05 | Khu đính ảnh tình trạng | SCR-NV-06, SCR-NV-09, SCR-QT-04 |
| P-06 | Ô nhập mã tại quầy | SCR-NV-04, SCR-NV-06, SCR-NV-08, SCR-NV-09, SCR-NV-11 |
| P-07 | Hồ sơ sinh viên (khối) | SCR-NV-04, SCR-NV-11 |
| P-08 | Thanh tiến trình giao thiết bị | SCR-NV-04, SCR-NV-05, SCR-NV-06, SCR-NV-07 |


