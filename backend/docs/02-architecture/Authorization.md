# PHÂN QUYỀN (AUTHORIZATION)

## Hệ thống quản lý mượn trả thiết bị học tập cho sinh viên

| Mục | Nội dung |
|---|---|
| Phiên bản | 1.1 (thêm ĐX-PQ-11, mục 4.1 và mục 12) |
| Ngày | 06/11/2026 |

---

## 0. Giới thiệu và cách đọc tài liệu

**Mục đích.** Tổng hợp các nội dung về xác thực và phân quyền của hệ thống: vai trò, tài khoản, phạm vi dữ liệu được truy cập, quyền thực hiện từng use case, các thao tác có quy định riêng về người thực hiện, các trường hợp bị từ chối truy cập và việc ghi vết. Tài liệu chỉ tổng hợp lại nội dung đã có trong các tài liệu nguồn, không thêm quy tắc mới. Những điểm nguồn không nêu hoặc nêu chưa rõ được liệt kê ở mục 12 kèm quyết định xử lý bổ sung, đánh dấu [ĐX-PQ-xx].

**Tài liệu nguồn.**

| Ký hiệu | Tài liệu |
|---|---|
| BR | BusinessRule.md – Quy tắc nghiệp vụ (BR-01 … BR-81) |
| SRS | SRS.md – Tài liệu đặc tả yêu cầu phần mềm (ngày lập 22/10/2026) |
| UC | Usecase.md – Mô hình Usecase, version 1.2 (UC-01 … UC-24) |

**Cách ghi nguồn.** Cột **Nguồn** truy vết về mã quy tắc (BR), yêu cầu chức năng (FR), yêu cầu phi chức năng (NFR), use case (UC; A = luồng thay thế, E = ngoại lệ), thuật ngữ (GL), đề nghị thay đổi (CR), câu chuyện người dùng (US), lớp thực thể (DM), quyết định xử lý bổ sung (ĐX) và mục của SRS.

**Nhãn [ĐX-xx].** Đánh dấu giá trị hoặc cách hiểu **không có trong tài liệu nguồn gốc** mà do SRS quyết định bổ sung (SRS mục 2.8); được giữ nguyên nhãn như trong BR và SRS.

**Nhãn [ĐX-PQ-xx].** Đánh dấu quyết định xử lý bổ sung của **tài liệu này** cho các điểm nguồn chưa nêu hoặc nêu chưa rõ (mục 12), theo cách SRS xử lý ĐX-01 … ĐX-07. Đây là nội dung **không có trong tài liệu nguồn**, được suy ra từ các quy định có sẵn của nguồn (cột *Cơ sở* ở mục 12), cần nhóm duyệt và nên cập nhật ngược vào nguồn ở các điểm ghi ở cột cuối của mục 12.

**Mục lục.**

0. Giới thiệu và cách đọc tài liệu
1. Nguyên tắc phân quyền chung
2. Vai trò
3. Tài khoản và xác thực
4. Phạm vi dữ liệu được truy cập
5. Ma trận phân quyền theo use case
6. Quyền chi tiết theo vai trò
7. Thao tác có quy định về người thực hiện
8. Quyền mượn của sinh viên
9. Các trường hợp từ chối truy cập
10. Ghi vết liên quan đến phân quyền
11. Yêu cầu, câu chuyện người dùng và đề nghị thay đổi liên quan
12. Các điểm nguồn chưa nêu hoặc nêu chưa rõ và quyết định xử lý bổ sung

---

## 1. Nguyên tắc phân quyền chung

| STT | Nguyên tắc | Nguồn |
|---|---|---|
| 1 | Hệ thống phân quyền theo **đúng ba vai trò**: Sinh viên (SV), Nhân viên quản lý thiết bị (NV), Quản trị viên (QT); đối với NV còn phân quyền **theo phòng thiết bị**. | BR-04, NFR-01, FR-34, SRS 2.4 |
| 2 | Mỗi vai trò chỉ thao tác trong phạm vi chức năng được cấp; người dùng không truy cập được chức năng ngoài phạm vi quyền. Tiêu chí chấp nhận: 100% người dùng không thể truy cập các đường dẫn/chức năng ngoài phạm vi quyền được cấp. | BR-04, NFR-01, UC-19 (Yêu cầu đặc biệt) |
| 3 | Sau khi đăng nhập, người dùng vào giao diện tương ứng với vai trò (Sinh viên, Nhân viên quản lý thiết bị, Quản trị viên). | UC-19 (luồng chính bước 3, hậu điều kiện), FR-33 |
| 4 | Đăng nhập (UC-19) là tiền điều kiện của các use case còn lại. | UC-19 (Ghi chú), SRS 4.1 |
| 5 | SV chỉ xem được dữ liệu của chính mình; truy cập dữ liệu của người khác bị từ chối. | BR-06, FR-32, UC-05 E1, UC-19 |
| 6 | NV chỉ thấy và thao tác với thiết bị, phiếu yêu cầu, phiếu mượn của phòng thiết bị được QT cấp quyền; truy cập phòng chưa được cấp quyền bị từ chối. | BR-05, FR-34, NFR-01, UC-06 E2, UC-18 E1 |
| 7 | Ngoài ba vai trò người dùng, hệ thống có tác nhân nội bộ **Hệ thống (bộ hẹn giờ)** thực hiện tự động theo thời gian/sự kiện. | SRS 2.4, UC mục 2 |

---

## 2. Vai trò

### 2.1 Các tác nhân

| STT | Tác nhân | Định nghĩa | Tài khoản | Phạm vi dữ liệu |
|---|---|---|---|---|
| 1 | Sinh viên (SV) | Người dùng chính có nhu cầu mượn thiết bị để học trên lớp, làm bài tập nhóm, đồ án, thực tập, thuyết trình hoặc hoạt động câu lạc bộ học thuật (GL-01); sinh viên do nhà trường quản lý, dùng ứng dụng trên điện thoại (SRS 2.4) | Do nhà trường cung cấp; SV không tự đăng ký (BR-08) | Chỉ dữ liệu của chính mình (BR-06) |
| 2 | Nhân viên quản lý thiết bị (NV) | Người duyệt yêu cầu mượn, giao và nhận thiết bị, kiểm tra tình trạng, ghi nhận vi phạm và quản lý lịch bảo trì; làm việc tại quầy (GL-02; SRS 2.4) | Do hệ thống cấp, do QT tạo (BR-09) | Phòng thiết bị được QT cấp quyền (BR-05) |
| 3 | Quản trị viên (QT) | Người hoạch định quy định mượn, quản lý danh mục thiết bị, phân quyền, nhà cung cấp/bảo hành và quyết định thanh lý (GL-03) | Do hệ thống cấp (BR-09) | Không giới hạn theo phòng thiết bị [ĐX-PQ-02] |
| 4 | Hệ thống (bộ hẹn giờ) | Tác nhân nội bộ tự động theo thời gian/sự kiện: nhắc hạn, đánh dấu quá hạn, khóa quyền mượn, hết hạn giữ chỗ và chuyển lượt danh sách chờ, tính phí, tự mở khóa quyền mượn (SRS 2.4; UC mục 2) | – | – |

### 2.2 Chức năng chính của từng vai trò

(Theo đề bài mục 2 và các use case, như BR mục 2.)

| Vai trò | Chức năng chính |
|---|---|
| Sinh viên | Tra cứu thiết bị; đặt trước; đăng ký chờ; gia hạn; hủy phiếu yêu cầu; xem lịch sử mượn/trả, biên bản và phí phạt; nhận thông báo; phản hồi tình trạng thiết bị sau khi trả |
| Nhân viên quản lý thiết bị | Duyệt/từ chối phiếu yêu cầu; giao thiết bị và lập phiếu mượn; nhận trả và kiểm tra; lập biên bản vi phạm, ghi nhận thu phạt; xử lý danh sách chờ; bảo trì, sửa chữa; nhắc nhở sinh viên |
| Quản trị viên | Quản lý danh mục thiết bị, phòng thiết bị; cấu hình quy định mượn, phí, khung phạt, tham số; quản lý tài khoản nhân viên và phân quyền theo phòng; quản lý nhà cung cấp, bảo hành; lập quyết định thanh lý; xem báo cáo, nhật ký; gỡ cấm mượn dài hạn |

---

## 3. Tài khoản và xác thực

### 3.1 Nguồn cấp tài khoản

| Đối tượng | Quy tắc | Nguồn |
|---|---|---|
| Sinh viên | Tài khoản và dữ liệu sinh viên (mã số sinh viên, họ tên, lớp, khoa, nhóm sinh viên, hạn thẻ, trạng thái học, học kỳ) do **nhà trường cung cấp**. SV không tự đăng ký; hệ thống không tạo hoặc nhập danh sách sinh viên; hệ thống không lưu mật khẩu sinh viên. | BR-08, FR-33, FR-35, NFR-05, UC-19 A1, UC-14 (Ghi chú), CR-13 |
| Nhân viên | Tài khoản do hệ thống cấp; **QT tạo, sửa, khóa/mở** tại UC-14. Tên đăng nhập trùng bị từ chối. | BR-09, FR-34, UC-14 E1 |
| Quản trị viên | Tài khoản do hệ thống cấp. Tài khoản QT không quản lý tại UC-14 (UC-14 chỉ quản lý tài khoản nhân viên) [ĐX-PQ-01]. | BR-09, FR-33, UC-19, UC-14, ĐX-PQ-01 |
| Mật khẩu NV, QT | Lưu dạng băm, không lưu dạng rõ. | BR-09, NFR-05, UC-14, UC-19 |
| Nhóm sinh viên | Do nhà trường cung cấp, QT không tạo nhóm. Các nhóm nêu trong nguồn: sinh viên đại học; học viên cao học; sinh viên làm đồ án tốt nghiệp. | BR-11, GL-29, UC-13 |
| Xác thực và dữ liệu SV | Hệ thống xác thực SV bằng tài khoản của nhà trường và nhận dữ liệu SV từ nhà trường; dữ liệu cập nhật từ nhà trường áp dụng cho giao dịch phát sinh sau. Cách kết nối kỹ thuật (giao thức, tần suất đồng bộ) thuộc giai đoạn thiết kế, không quy định ở tài liệu này [ĐX-PQ-06]. | FR-35, BR-08, CR-13, ĐX-PQ-06 |

### 3.2 Đăng nhập (UC-19)

| Mục | Nội dung |
|---|---|
| Actor | Sinh viên; Nhân viên quản lý thiết bị; Quản trị viên |
| Độ ưu tiên | Bắt buộc |
| Tiền điều kiện | Tài khoản đã được cấp (SV: do nhà trường; NV: do QT tạo tại UC-14) và chưa bị khóa. |
| Luồng chính | 1. Người dùng nhập thông tin đăng nhập. 2. Hệ thống xác thực tài khoản. 3. Hệ thống chuyển người dùng đến giao diện theo vai trò (Sinh viên, Nhân viên quản lý thiết bị, Quản trị viên). |
| Hậu điều kiện | Người dùng vào giao diện tương ứng với vai trò; SV chỉ xem dữ liệu của chính mình. |
| Luồng thay thế | A1. SV không có chức năng tự đăng ký: tài khoản và dữ liệu sinh viên do nhà trường cung cấp (FR-33, FR-35). |
| Ngoại lệ | E1. Thông tin đăng nhập không đúng: từ chối. E2. Nhập sai mật khẩu quá số lần quy định (`soLanDangNhapSaiToiDa`, do QT cấu hình tại UC-13): tạm khóa đăng nhập (FR-33). E3. Tài khoản bị khóa: không đăng nhập được (FR-34). |
| Yêu cầu đặc biệt | Hệ thống không lưu mật khẩu sinh viên; mật khẩu NV và QT được băm an toàn (NFR-05). Phân quyền theo 3 vai trò và theo phòng thiết bị; người dùng không truy cập được chức năng ngoài phạm vi quyền (NFR-01). |

### 3.3 Khóa đăng nhập và trạng thái tài khoản

| Nội dung | Quy tắc | Nguồn |
|---|---|---|
| Số lần nhập sai mật khẩu tối đa (`soLanDangNhapSaiToiDa`) | QT cấu hình tại UC-13; phải lớn hơn 0; mặc định **5 lần** [ĐX-02]. | BR-10, FR-20, UC-13, SRS 3.3, ĐX-02 |
| Hậu quả khi nhập sai quá số lần quy định | Tạm khóa đăng nhập. Tài khoản bị khóa không đăng nhập được. | BR-10, FR-33, UC-19 E2, E3 |
| Trạng thái tài khoản NV, QT (`NguoiDungHeThong.trangThaiKhoa`) | **Hoạt động** (mặc định) / **Tạm khóa đăng nhập** / **Bị khóa**. Hai trạng thái sau đều không đăng nhập được. | SRS 6.3, FR-33, FR-34 |
| Khóa/mở tài khoản nhân viên | QT thực hiện tại UC-14; tài khoản bị khóa không đăng nhập được; mọi thay đổi được ghi lại. | UC-14, FR-34, US-25 |
| Phân biệt hai trạng thái không đăng nhập được | **Tạm khóa đăng nhập**: do hệ thống tự đặt khi nhập sai mật khẩu quá `soLanDangNhapSaiToiDa`. **Bị khóa**: do QT chủ động khóa tại UC-14. [ĐX-PQ-04] | SRS 6.3, UC-19 E2, E3, UC-14, ĐX-PQ-04 |
| Mở lại tài khoản bị khóa | Tài khoản NV (cả hai trạng thái): QT mở lại tại UC-14. Không có thời hạn tự động mở lại (nguồn không có tham số này). Tài khoản QT: do bên vận hành hệ thống mở lại ngoài các use case, vì tài khoản QT do hệ thống cấp. [ĐX-PQ-04] | UC-14, BR-09, ĐX-PQ-04 |
| Phạm vi áp dụng khóa đăng nhập do nhập sai mật khẩu | Áp dụng cho tài khoản do hệ thống quản lý (NV, QT). Với SV, xác thực và việc khóa do hệ thống tài khoản của nhà trường quản lý; hệ thống này không lưu mật khẩu SV và không đếm số lần nhập sai của SV. [ĐX-PQ-05] | NFR-05, SRS 6.1 (DM-01, DM-04), BR-08, ĐX-PQ-05 |
| Đổi mật khẩu, quên mật khẩu | Không có chức năng riêng. SV: mật khẩu thuộc hệ thống tài khoản của nhà trường. NV: khi cần cấp lại mật khẩu, QT thực hiện qua thao tác sửa tài khoản tại UC-14. Tài khoản QT: do bên vận hành hệ thống xử lý ngoài các use case. [ĐX-PQ-07] | UC-14 (luồng chính bước 2, 4), BR-08, SRS Phụ lục C (STT 1), ĐX-PQ-07 |

### 3.4 Lớp thực thể liên quan đến tài khoản và phân quyền

| Mã | Lớp | Mô tả | Thuộc tính chính | Nguồn |
|---|---|---|---|---|
| DM-01 | NguoiDungHeThong | Lớp cha của NV và QT: định danh chung `maNguoiDung`; tài khoản do hệ thống cấp, mật khẩu băm, có thể bị khóa. Lớp trừu tượng. | maNguoiDung, tenDangNhap, matKhauBam, hoTen, trangThaiKhoa, soLanDangNhapSai | SRS 6.1 |
| DM-02 | NhanVienQuanLy | Kế thừa NguoiDungHeThong; có mã nhân viên; được cấp quyền theo từng phòng thiết bị. | maNV | SRS 6.1 |
| DM-03 | QuanTriVien | Kế thừa NguoiDungHeThong; có mã quản trị viên. | maQT | SRS 6.1 |
| DM-04 | SinhVien | Dữ liệu và tài khoản do nhà trường cung cấp; không tự đăng ký. Không kế thừa NguoiDungHeThong; hệ thống không lưu mật khẩu SV. | maSV, hoTen, lop, khoa, email, hanThe, trangThaiHoc, hocKy, trangThaiQuyenMuon | SRS 6.1 |
| DM-05 | NhomSinhVien | Đại học; cao học; làm đồ án tốt nghiệp (nhà trường cung cấp). | maNhom, tenNhom | SRS 6.1 |
| DM-08 | PhongThietBi | Thuộc một cơ sở, gồm các kệ và tủ. | maPhong, tenPhong, coSo | SRS 6.1 |
| DM-26 | NhatKyHeThong | Người thực hiện (NV, QT, SV hoặc Hệ thống), thời điểm, thao tác, giá trị trước/sau; chỉ xem, không sửa/xóa. | maNhatKy, thoiDiem, thaoTac, doiTuong, giaTriTruoc, giaTriSau | SRS 6.1 |

**Quan hệ liên quan** (SRS Phụ lục B):

| Lớp A | Quan hệ | Lớp B | Bản số (A – B) |
|---|---|---|---|
| NguoiDungHeThong | Kế thừa | NhanVienQuanLy | – |
| NguoiDungHeThong | Kế thừa | QuanTriVien | – |
| NhanVienQuanLy | Kết hợp (được cấp quyền) | PhongThietBi | 0..* – 0..* |
| NhomSinhVien | Kết hợp (thuộc) | SinhVien | 1 – 0..* |
| NguoiDungHeThong | Kết hợp (thực hiện) | NhatKyHeThong | 0..1 – 0..* |
| SinhVien | Kết hợp (thực hiện) | NhatKyHeThong | 0..1 – 0..* |

---

## 4. Phạm vi dữ liệu được truy cập

| Vai trò | Phạm vi | Khi truy cập ngoài phạm vi | Nguồn |
|---|---|---|---|
| Sinh viên | Chỉ dữ liệu của chính SV đăng nhập (phiếu mượn, vi phạm, phí phạt của mình; phiếu yêu cầu của mình). | Bị từ chối | BR-06, FR-32, UC-05 E1, UC-19, UC-20 |
| Nhân viên quản lý thiết bị | Thiết bị, phiếu yêu cầu, phiếu mượn của phòng thiết bị được QT cấp quyền. | Bị từ chối | BR-05, FR-34, UC-06 E2, UC-18 E1 |
| Quản trị viên | Không giới hạn theo phòng thiết bị: phân quyền theo phòng chỉ áp dụng cho NV [ĐX-PQ-02]. | – | SRS 2.4, ĐX-PQ-02 |

### 4.1 Cách áp dụng phạm vi phòng thiết bị của NV trong các chức năng

| Use case | Cách áp dụng | Nguồn |
|---|---|---|
| UC-06 | NV mở danh sách phiếu yêu cầu **của phòng mình**; phiếu thuộc phòng chưa được cấp quyền: từ chối truy cập (E2). Sự kiện `layPhieuChoDuyet(maNV)` trả danh sách phiếu Chờ duyệt của **các phòng thiết bị** NV được cấp quyền. | UC-06, BR-28, SRS SSD-02 |
| UC-07 | Với từng thiết bị giao, hệ thống kiểm tra thiết bị Sẵn sàng, **thuộc phòng của NV** và đúng loại đã duyệt; không đạt thì từ chối, không đưa vào phiếu. | UC-07 (bước 4, A3), BR-40 |
| UC-18 | NV xem danh sách SV sắp đến hạn hoặc quá hạn, lọc theo phòng thiết bị; tiền điều kiện là phiếu mượn thuộc phòng NV được cấp quyền; phiếu thuộc phòng chưa được cấp quyền: từ chối truy cập (E1). | UC-18, BR-77, FR-39 |
| UC-14 | QT cấp hoặc thu hồi quyền của NV theo từng phòng thiết bị. | UC-14, FR-34, US-17 |
| UC-07, UC-09 | Tình trạng cấp sinh viên (thẻ, nhóm, số thiết bị đang mượn, tổng nợ biên bản Chưa nộp) và việc xét mở khóa quyền mượn **không giới hạn theo phòng**, vì quyền mượn của SV áp dụng toàn hệ thống (BR-23, BR-66). Danh sách phiếu mượn và biên bản vẫn giới hạn theo phòng | BR-23, BR-66, ĐX-PQ-11 |

---

## 5. Ma trận phân quyền theo use case

**Chú giải:** ✔ = tác nhân thực hiện use case (theo mục *Actor* của đặc tả); ○ = có tham gia nhưng không thao tác nghiệp vụ (có mặt tại quầy; nhận thông báo); — = không tham gia.

| UC | Tên use case | SV | NV | QT | Hệ thống (bộ hẹn giờ) | Ghi chú về quyền |
|---|---|---|---|---|---|---|
| UC-01 | Tra cứu thiết bị | ✔ | — | — | — | SV đã đăng nhập |
| UC-02 | Đặt trước thiết bị | ✔ | — | — | — | SV đã đăng nhập; thẻ còn hạn; quyền mượn bình thường |
| UC-03 | Đăng ký chờ | ✔ | — | — | — | SV đã đăng nhập; loại thiết bị có số lượng khả dụng bằng 0 tại phòng đã chọn |
| UC-04 | Gia hạn thời gian mượn | ✔ | — | — | — | Hệ thống tự kiểm tra và duyệt, không cần NV |
| UC-05 | Xem lịch sử mượn/trả, biên bản và phí phạt | ✔ | — | — | — | Chỉ dữ liệu của chính SV |
| UC-06 | Duyệt hoặc từ chối phiếu yêu cầu mượn | — | ✔ | — | — | Phiếu thuộc phòng NV được cấp quyền |
| UC-07 | Giao thiết bị và lập phiếu mượn | ○ (có mặt tại quầy) | ✔ | — | — | Thiết bị thuộc phòng của NV |
| UC-08 | Nhận trả và kiểm tra thiết bị | ○ (có mặt tại quầy) | ✔ | — | — | SV mang thiết bị đến trả tại quầy |
| UC-09 | Lập biên bản vi phạm, thu phạt và mở khóa | — | ✔ | ✔ (chỉ hủy hoặc điều chỉnh biên bản) | ✔ | Hệ thống tính tiền và tự mở khóa khi đủ điều kiện |
| UC-10 | Xử lý danh sách chờ | — | ✔ | — | ✔ | Hệ thống giữ chỗ, thông báo, chuyển lượt; NV xác nhận khi SV đến nhận |
| UC-11 | Bảo trì và sửa chữa thiết bị | — | ✔ | — | — | |
| UC-12 | Quản lý loại thiết bị và thiết bị | — | — | ✔ | — | |
| UC-13 | Cấu hình quy định mượn, mức phí và khung phạt | — | — | ✔ | — | |
| UC-14 | Quản lý tài khoản nhân viên và phân quyền theo phòng | — | — | ✔ | — | |
| UC-15 | Lập quyết định thanh lý | — | — | ✔ | — | |
| UC-16 | Xem báo cáo, thống kê | — | — | ✔ | — | |
| UC-17 | Theo dõi hạn trả, giữ chỗ và khóa quyền mượn tự động | ○ (nhận thông báo) | ○ (nhận thông báo nhắc bảo trì) | — | ✔ | |
| UC-18 | Nhắc nhở sinh viên sắp đến hạn hoặc quá hạn | — | ✔ | — | — | Phiếu thuộc phòng NV được cấp quyền |
| UC-19 | Đăng nhập | ✔ | ✔ | ✔ | — | |
| UC-20 | Hủy phiếu yêu cầu mượn | ✔ | — | — | — | Phiếu yêu cầu của chính SV |
| UC-21 | Phản hồi tình trạng thiết bị sau khi trả | ✔ | — | — | — | Sau khi đã bàn giao thiết bị tại quầy |
| UC-22 | Quản lý nhà cung cấp và thời hạn bảo hành | — | — | ✔ | — | |
| UC-23 | Xem nhật ký hệ thống | — | — | ✔ | — | Chỉ xem |
| UC-24 | Quản lý cấm mượn dài hạn | — | — | ✔ (gỡ cấm) | ✔ (đếm vi phạm, chuyển sang Cấm mượn dài hạn) | Ưu tiên thấp, chưa đưa vào giai đoạn đầu |

**Nguồn:** mục *Actor* của từng đặc tả use case (UC mục 4; SRS 4.3), danh sách use case (UC mục 3; SRS 4.1) và sơ đồ Use-case (UC mục 1). Ghi chú về quyền lấy từ tiền điều kiện, luồng và ngoại lệ của từng use case.

---

## 6. Quyền chi tiết theo vai trò

Quyền của mỗi vai trò là tập các use case của vai trò đó (mục 5), với các hành động được mô tả trong từng use case, trong phạm vi dữ liệu ở mục 4. Không có ma trận quyền xem/thêm/sửa/xóa tách riêng; hành động xóa chỉ có ở UC-12 và theo quy tắc ngưng sử dụng khi đã có lịch sử [ĐX-PQ-03].

### 6.1 Sinh viên

| UC | Quyền | Điều kiện / ràng buộc | Nguồn |
|---|---|---|---|
| UC-01 | Tìm, lọc (tên, loại thiết bị, phòng thiết bị, còn/hết hàng) và xem chi tiết thiết bị | Đã đăng nhập | BR-19, UC-01, FR-01, FR-02 |
| UC-02 | Gửi phiếu yêu cầu mượn (đặt trước) nhiều loại thiết bị | Đã đăng nhập; thẻ còn hạn; quyền mượn bình thường. Bị khóa, thẻ hết hạn hoặc không còn theo học: không cho gửi, hiển thị lý do. Không vượt giới hạn số thiết bị và thời gian mượn tối đa của nhóm (mục 8.4) | BR-21, BR-23, BR-24, BR-25, UC-02, FR-04 |
| UC-03 | Đăng ký vào danh sách chờ; rời danh sách chờ | Đã đăng nhập; chỉ khi loại thiết bị đã hết số lượng khả dụng tại phòng đó | BR-33, BR-37, UC-03 |
| UC-04 | Gia hạn phiếu mượn | Đang giữ thiết bị theo phiếu mượn, chưa quá ngày giờ hẹn trả; mỗi phiếu gia hạn tối đa một lần, thêm tối đa 2 giờ; chỉ khi chưa có SV đăng ký chờ các loại thiết bị trong phiếu; hệ thống tự duyệt | BR-45, BR-46, BR-47, UC-04 |
| UC-05 | Xem lịch sử mượn/trả, biên bản vi phạm và phí phạt | Chỉ dữ liệu của chính SV đăng nhập | BR-06, BR-80, UC-05, FR-32 |
| UC-20 | Hủy phiếu yêu cầu mượn | Phiếu yêu cầu của mình đang Chờ duyệt hoặc Đã duyệt và chưa nhận thiết bị; không tính vi phạm | BR-27, UC-20 |
| UC-21 | Gửi phản hồi tình trạng thiết bị | Đã bàn giao thiết bị tại quầy (NV đã nhận trả); phản hồi gắn với mã thiết bị và phiếu mượn | BR-55, UC-21 |
| UC-19 | Đăng nhập bằng tài khoản sinh viên của trường | Tài khoản do nhà trường cấp, chưa bị khóa | UC-19, US-24 |
| UC-07, UC-08 | Có mặt tại quầy khi nhận và khi trả thiết bị; việc giao, nhận, kiểm tra do NV thực hiện | Giao và nhận thiết bị luôn thực hiện trực tiếp tại quầy | UC-07, UC-08, BR-01 |
| UC-17 | Nhận thông báo qua ứng dụng và email (SMS tùy chọn) khi: sắp đến hạn trả (trước 1 giờ); quá hạn trả; thiết bị chờ sẵn sàng; phiếu yêu cầu được duyệt hoặc từ chối; phát sinh vi phạm, phí phạt | – | BR-75, FR-24, UC-17 |

**Giới hạn của ứng dụng sinh viên:** ứng dụng chỉ dùng để đặt trước (tra cứu, đặt trước, đăng ký chờ, gia hạn, hủy phiếu yêu cầu, xem lịch sử, phản hồi tình trạng thiết bị); việc giao và nhận thiết bị luôn thực hiện trực tiếp tại quầy thiết bị (BR-01). Mượn/trả hoàn toàn trực tuyến và thanh toán trực tuyến nằm ngoài phạm vi hệ thống (BR-02).

### 6.2 Nhân viên quản lý thiết bị

| UC | Quyền | Điều kiện / ràng buộc | Nguồn |
|---|---|---|---|
| UC-06 | Duyệt hoặc từ chối phiếu yêu cầu mượn; từ chối kèm lý do | Phiếu thuộc phòng được cấp quyền. Trước khi duyệt, hệ thống kiểm tra lại điều kiện SV và số lượng khả dụng; không đủ thì chặn duyệt. Duyệt thì giữ chỗ 30 phút | BR-28, BR-29, UC-06, FR-30 |
| UC-07 | Quét mã thẻ SV; giao thiết bị; ghi tình trạng ban đầu; lập phiếu mượn (theo phiếu yêu cầu Đã duyệt, cho SV đến từ danh sách chờ, hoặc mượn trực tiếp không đặt trước) | SV có mặt tại quầy; thiết bị Sẵn sàng, thuộc phòng của NV, đúng loại đã duyệt. SV bị khóa, thẻ hết hạn, không còn theo học hoặc vượt giới hạn: dừng, hiển thị lý do | BR-38, BR-39, BR-40, BR-41, UC-07, FR-31 |
| UC-08 | Nhận trả từng thiết bị; đối chiếu tình trạng lúc giao và lúc trả; ghi kết quả kiểm tra (Nguyên vẹn, Hư hỏng, Thiếu phụ kiện, Mất); mô tả và chọn khung phạt khi Hư hỏng/Thiếu phụ kiện | Thiết bị thuộc phiếu mượn đang hiệu lực, nếu không: báo lỗi, không cho nhận. Việc đánh giá hư hỏng do NV thực hiện. Hệ thống ghi NV nhận trả (NV đang đăng nhập) | BR-48, BR-49, BR-50, BR-52, UC-08 |
| UC-09 | Lập biên bản vi phạm; ghi nhận từng biên bản Đã nộp; xác nhận đã nhận đủ thiết bị; lập biên bản mất khi thiết bị chưa trả quá 30 ngày | Số tiền không đổi sau khi lập; chỉ QT hủy hoặc điều chỉnh biên bản. Tiền phạt do SV nộp trực tiếp cho NV (ngoài hệ thống) | BR-61, BR-62, BR-65, UC-09 |
| UC-10 | Xem danh sách chờ; chuyển lượt thủ công khi SV báo không mượn nữa tại quầy; xác nhận khi SV đến nhận | Chuyển lượt tự động do hệ thống khi hết thời gian giữ chỗ | BR-36, UC-10 (bước 4, A2) |
| UC-11 | Lập lịch bảo trì định kỳ; tiếp nhận sửa chữa thiết bị hư hỏng; ghi nội dung thực hiện, kết quả, ngày hoàn thành; chọn kết quả đưa về Sẵn sàng cho mượn hoặc đề xuất thanh lý; liên hệ nhà cung cấp khi còn trong thời hạn bảo hành | Thiết bị đang bảo trì/sửa chữa không cho mượn | BR-69, BR-70, BR-71, BR-72, UC-11 |
| UC-18 | Xem danh sách SV sắp đến hạn hoặc quá hạn (lọc theo phòng thiết bị) và gửi thông báo nhắc nhở; mỗi lần nhắc được ghi lại | Phiếu thuộc phòng được cấp quyền | BR-77, UC-18, FR-39 |
| UC-17 | Nhận thông báo nhắc bảo trì khi thiết bị đến hạn bảo trì | – | UC-17, BR-70 |
| UC-19 | Đăng nhập bằng tài khoản do hệ thống cấp | Tài khoản do QT tạo, chưa bị khóa | UC-19, UC-14 |

### 6.3 Quản trị viên

| UC | Quyền | Điều kiện / ràng buộc | Nguồn |
|---|---|---|---|
| UC-12 | Thêm/sửa danh mục phòng thiết bị; thêm/sửa/xóa loại thiết bị và từng thiết bị; cập nhật giá trị hiện hành của thiết bị | Mã thiết bị trùng: từ chối. Loại/thiết bị đã có lịch sử chỉ ngưng sử dụng, không xóa hẳn. Mỗi lần cập nhật giá trị được lưu lịch sử | BR-12 – BR-18, UC-12, FR-18 |
| UC-13 | Cấu hình số thiết bị và thời gian mượn tối đa theo nhóm SV (ngoại lệ theo loại thiết bị), phí trả trễ theo loại thiết bị, khung phạt hư hỏng và tham số hệ thống (số ngày khóa, thời gian giữ chỗ, ân hạn, số giờ gia hạn, ngưỡng vi phạm, số lần nhập sai mật khẩu tối đa, giờ mở/đóng cửa quầy) | Giá trị mới chỉ áp dụng cho giao dịch phát sinh sau, không hồi tố; ghi nhật ký giá trị trước/sau. Khung phạt đã dùng chỉ được tắt, không xóa. QT không tạo nhóm sinh viên | BR mục 14, BR-59, UC-13, FR-20, FR-37 |
| UC-14 | Tạo, sửa, khóa/mở tài khoản nhân viên; cấp hoặc thu hồi quyền theo từng phòng thiết bị | Tên đăng nhập trùng: từ chối. Mật khẩu được băm; ghi nhật ký. Tài khoản sinh viên không quản lý tại đây | BR-09, UC-14, FR-34 |
| UC-15 | Lập quyết định thanh lý (mã số riêng, ngày thanh lý, danh sách thiết bị); thiết bị chuyển Đã thanh lý | Căn cứ kết quả bảo trì, báo cáo hư hỏng hoặc đề xuất thanh lý của NV. Thiết bị đã thanh lý vẫn giữ lịch sử | BR-73, UC-15, FR-21 |
| UC-16 | Xem và xuất báo cáo, thống kê (lọc theo khoảng thời gian): tình trạng thiết bị trong kho; SV mượn quá hạn chưa trả; thiết bị hư hỏng/mất; tần suất mượn theo loại thiết bị; vi phạm của SV; lịch sử bảo trì, thanh lý | – | BR-81, UC-16 |
| UC-22 | Quản lý thông tin liên hệ nhà cung cấp; gán nhà cung cấp cho loại thiết bị hoặc thiết bị; cập nhật thời hạn bảo hành; theo dõi ngày hết hạn bảo hành | Một thiết bị hoặc loại thiết bị có thể do nhiều nhà cung cấp | BR-74, UC-22, FR-19 |
| UC-23 | Xem và lọc nhật ký hệ thống (người dùng, đối tượng, thời gian) | Chỉ xem; không sửa hoặc xóa được | BR-79, UC-23, FR-40 |
| UC-24 | Gỡ cấm mượn dài hạn kèm lý do | Ưu tiên thấp, chưa đưa vào giai đoạn đầu. Thao tác được ghi lại | BR-68, UC-24, FR-42 |
| UC-09 | Hủy hoặc điều chỉnh biên bản vi phạm | Chỉ QT được thực hiện; thay đổi được ghi nhật ký | BR-07, GL-34, UC-09 A2 |
| UC-19 | Đăng nhập bằng tài khoản do hệ thống cấp | Chưa bị khóa | UC-19 |

### 6.4 Hệ thống (bộ hẹn giờ)

| UC | Hành động tự động | Nguồn |
|---|---|---|
| UC-17 | Chạy theo lịch: nhắc hạn trả trước 1 giờ (ứng dụng và email); đánh dấu phiếu Quá hạn và gửi thông báo quá hạn; khóa quyền mượn mới; hết hạn giữ chỗ quá 30 phút; cho phiếu yêu cầu chưa duyệt trong ngày hết hiệu lực; nhắc bảo trì đến hạn. Chạy lại không tạo kết quả trùng. | BR-75 – BR-78, UC-17, FR-41 |
| UC-09 | Tính tiền theo từng thiết bị; tự động mở lại quyền mượn khi NV xác nhận đã nhận đủ thiết bị và thu đủ tiền, không cần QT | BR-62, BR-67, UC-09 |
| UC-10 | Giữ chỗ 30 phút, chuyển đăng ký chờ sang Đã thông báo và thông báo cho SV đứng đầu danh sách chờ; chuyển lượt tự động khi hết thời gian giữ chỗ | BR-32, BR-35, UC-10 |
| UC-24 | Đếm số lần vi phạm của SV trong học kỳ; vượt ngưỡng thì chuyển SV sang Cấm mượn dài hạn | BR-68, UC-24, FR-42 |

---

## 7. Thao tác có quy định về người thực hiện

| STT | Thao tác | Quy định về quyền | Nguồn |
|---|---|---|---|
| 1 | Hủy hoặc điều chỉnh biên bản vi phạm | **Chỉ QT** được thực hiện; mọi thay đổi được ghi nhật ký giá trị trước/sau. NV lập biên bản; số tiền không đổi sau khi lập (NV và hệ thống không sửa). Biên bản Đã hủy không còn được tính là biên bản chưa nộp khi xét khóa/mở khóa [ĐX-PQ-09] | BR-07, BR-62, GL-34, UC-09 A2 |
| 2 | Quyết định thanh lý | **QT** quyết định; NV chỉ đề xuất, thiết bị đề xuất thanh lý hiện trong danh sách chờ quyết định của QT | BR-71, BR-73, FR-21, FR-38, UC-11, UC-15 |
| 3 | Tạo, sửa, khóa/mở tài khoản NV; cấp/thu hồi quyền theo phòng | QT thực hiện tại UC-14 | BR-09, FR-34, UC-14 |
| 4 | Cấu hình quy định mượn, phí, khung phạt, tham số hệ thống | QT thực hiện tại UC-13 | BR mục 14, FR-20, FR-37, UC-13 |
| 5 | Quản lý danh mục phòng thiết bị, loại thiết bị, thiết bị; cập nhật giá trị hiện hành | QT thực hiện | BR-14, BR-17, FR-18, UC-12 |
| 6 | Duyệt hoặc từ chối phiếu yêu cầu mượn | NV, trong phạm vi phòng được cấp quyền | BR-28, FR-30, UC-06 |
| 7 | Duyệt gia hạn | Hệ thống tự duyệt, không cần NV | BR-47, FR-07, UC-04 |
| 8 | Giao thiết bị và nhận trả thiết bị | Thực hiện trực tiếp tại quầy bởi NV; ứng dụng chỉ dùng để đặt trước | BR-01, UC-07, UC-08 |
| 9 | Đánh giá hư hỏng và chọn khung phạt | NV thực hiện; không tự động phân tích hình ảnh hư hỏng bằng AI | BR-02, BR-49, UC-08 |
| 10 | Ghi nhận SV đã nộp phạt theo từng biên bản | NV ghi nhận; tiền phạt nộp trực tiếp cho NV ngoài hệ thống | BR-65, UC-09 |
| 11 | Mở khóa quyền mượn (do quá hạn hoặc chưa nộp phạt) | Hệ thống tự động mở khi NV xác nhận đã nhận đủ thiết bị và thu đủ tiền; không cần QT | BR-67, FR-15, CR-10, UC-09 |
| 12 | Gỡ Cấm mượn dài hạn | SV thuộc diện này không được mở khóa tự động khi nộp phạt; QT gỡ cấm kèm lý do | BR-68, FR-42, UC-24, UC-09 E2 |
| 13 | Chuyển lượt trong danh sách chờ | Hệ thống tự động khi giữ chỗ hết hạn; NV chuyển thủ công khi SV báo không mượn nữa tại quầy | BR-32, BR-36, FR-28, UC-10 |
| 14 | Nhắc nhở SV sắp đến hạn hoặc quá hạn | Hệ thống tự động nhắc (UC-17); NV gửi nhắc thủ công theo phòng được cấp quyền, mỗi lần nhắc được ghi lại | BR-75, BR-77, FR-39, UC-17, UC-18 |
| 15 | Gửi phản hồi tình trạng thiết bị | SV, chỉ sau khi đã bàn giao thiết bị tại quầy (NV đã nhận trả) | BR-55, FR-25, UC-21 |
| 16 | Xem nhật ký hệ thống | QT xem và lọc; **không ai** được sửa hoặc xóa | BR-79, FR-40, NFR-02, UC-23 |

---

## 8. Quyền mượn của sinh viên

### 8.1 Trạng thái quyền mượn

Trạng thái quyền mượn của sinh viên (`SinhVien.trangThaiQuyenMuon`): **Bình thường** (mặc định) / **Tạm khóa** / **Cấm mượn dài hạn** (BR mục 13; SRS 6.3).

| Trạng thái | Ý nghĩa | Nguồn |
|---|---|---|
| Bình thường | Mặc định | SRS 6.3 |
| Tạm khóa | Tạm ngừng cho SV mượn thiết bị mới, áp dụng khi quá hạn trả vượt số ngày quy định hoặc còn biên bản chưa nộp | GL-19, BR-66 |
| Cấm mượn dài hạn | Không cho SV mượn thiết bị cho đến khi QT gỡ, áp dụng khi số lần vi phạm trong học kỳ vượt ngưỡng; ưu tiên thấp | GL-38, BR-68 |

### 8.2 Điều kiện không được đặt trước hoặc mượn

| Điều kiện | Kết quả | Nguồn |
|---|---|---|
| Thẻ hết hạn (`hanThe`) | Không được đặt trước hoặc mượn | BR-23, GL-36, FR-04 |
| Không còn theo học (tốt nghiệp, thôi học, đình chỉ) | Không được đặt trước hoặc mượn | BR-23, FR-35 |
| Quyền mượn bị khóa | Không được đặt trước hoặc mượn | BR-23, FR-04 |

Khi gửi phiếu yêu cầu: hệ thống không cho gửi và hiển thị lý do (BR-23, UC-02 E1). Khi giao thiết bị: dừng và hiển thị lý do (BR-38, UC-07 E1). Trạng thái học (`SinhVien.trangThaiHoc`): Đang học / Tốt nghiệp / Thôi học / Đình chỉ (SRS 6.3). Nhóm sinh viên và hạn thẻ quyết định quyền mượn và giới hạn mượn; dữ liệu cập nhật từ nhà trường áp dụng cho giao dịch phát sinh sau (FR-35).

### 8.3 Khóa và mở khóa quyền mượn

| Nội dung | Quy tắc | Nguồn |
|---|---|---|
| Tự động khóa | Khi SV có phiếu quá hạn trả quá số ngày quy định (`soNgayKhoa`, mặc định **3 ngày**) **hoặc** còn biên bản chưa nộp phạt. Khi khóa: chặn đặt trước và lập phiếu mượn mới, hiển thị lý do. | BR-66, FR-09, UC-17 |
| Mở khóa tự động | Ngay khi NV xác nhận đã nhận đủ thiết bị và thu đủ tiền, không cần QT. Còn biên bản chưa nộp hoặc còn thiết bị chưa trả thì vẫn khóa. | BR-67, FR-15, CR-10, UC-09 |
| Cấm mượn dài hạn | Hệ thống đếm số vi phạm của SV trong học kỳ; vượt ngưỡng (`nguongViPham`, mặc định **3 lần/học kỳ**) thì chuyển sang Cấm mượn dài hạn cho đến khi QT gỡ cấm kèm lý do. Không được mở khóa tự động khi nộp phạt. Ưu tiên thấp, chưa đưa vào giai đoạn đầu. | BR-68, FR-42, UC-24, CR-14 |

### 8.4 Giới hạn mượn theo nhóm sinh viên

Giá trị mặc định do QT cấu hình tại UC-13 theo nhóm sinh viên; giá trị mới không áp dụng hồi tố (BR mục 14; SRS 3.3).

| Nhóm sinh viên | Số thiết bị tối đa mượn cùng lúc (tính trên tổng, không phân biệt loại) | Thời gian mượn tối đa |
|---|---|---|
| Sinh viên đại học | 2 | 5 giờ |
| Học viên cao học | 3 | 10 giờ (nhóm ưu tiên) [ĐX-01] |
| Sinh viên làm đồ án tốt nghiệp | 3 | 10 giờ (nhóm ưu tiên) [ĐX-01] |

Có thể đặt ngoại lệ thời gian mượn theo loại thiết bị; phiếu gồm nhiều loại thì lấy mức nhỏ nhất theo các loại trong phiếu (BR-24, BR-25, FR-20).

### 8.5 Chức năng của sinh viên khi quyền mượn là Tạm khóa hoặc Cấm mượn dài hạn [ĐX-PQ-08]

| Nhóm | Chức năng | Cơ sở |
|---|---|---|
| Bị chặn | Đặt trước (UC-02); đăng ký chờ (UC-03); lập phiếu mượn mới (UC-07), kể cả mượn trực tiếp và giao cho SV đến từ danh sách chờ | BR-66, BR-23, GL-19, GL-38, UC-07 E1; UC-03 là extend của UC-02 |
| Không bị chặn | Tra cứu (UC-01); xem lịch sử, biên bản, phí phạt (UC-05); hủy phiếu yêu cầu (UC-20) và rời danh sách chờ (UC-03 A2); gia hạn phiếu đang mượn theo điều kiện riêng của UC-04 (UC-04); phản hồi tình trạng thiết bị (UC-21); nhận thông báo (UC-17); trả thiết bị tại quầy (UC-08) | BR-66 và GL-19 chỉ chặn việc mượn thiết bị mới; BR-27, BR-37 |

---

## 9. Các trường hợp từ chối truy cập

| STT | Tình huống | Kết quả | Nguồn |
|---|---|---|---|
| 1 | Thông tin đăng nhập không đúng | Từ chối | UC-19 E1 |
| 2 | Nhập sai mật khẩu quá số lần quy định (`soLanDangNhapSaiToiDa`) | Tạm khóa đăng nhập | UC-19 E2, BR-10, FR-33 |
| 3 | Đăng nhập bằng tài khoản bị khóa | Không đăng nhập được | UC-19 E3, UC-14 E2, FR-34, BR-10 |
| 4 | SV muốn tự đăng ký tài khoản | Không có chức năng tự đăng ký | UC-19 A1, FR-33, BR-08 |
| 5 | Tạo tài khoản NV với tên đăng nhập trùng | Từ chối | UC-14 E1 |
| 6 | SV truy cập dữ liệu của người khác | Từ chối | UC-05 E1, BR-06 |
| 7 | NV truy cập phiếu yêu cầu thuộc phòng chưa được cấp quyền | Từ chối truy cập | UC-06 E2, BR-05 |
| 8 | NV truy cập phiếu mượn thuộc phòng chưa được cấp quyền (nhắc nhở SV) | Từ chối truy cập | UC-18 E1, BR-05 |
| 9 | Người dùng truy cập đường dẫn hoặc chức năng ngoài phạm vi quyền được cấp | Không thể truy cập (tiêu chí chấp nhận: 100%) | NFR-01, BR-04 |
| 10 | NV đưa vào phiếu mượn thiết bị không Sẵn sàng, không thuộc phòng của mình hoặc không đúng loại đã duyệt | Từ chối, không đưa vào phiếu | BR-40, UC-07 A3 |
| 11 | SV bị khóa, thẻ hết hạn hoặc không còn theo học gửi phiếu yêu cầu | Không cho gửi, hiển thị lý do | UC-02 E1, BR-23 |
| 12 | SV bị khóa, thẻ hết hạn, không còn theo học hoặc vượt giới hạn khi NV giao thiết bị | Dừng, hiển thị lý do | UC-07 E1, BR-38 |
| 13 | SV thuộc diện Cấm mượn dài hạn nộp phạt | Không được mở khóa tự động | UC-09 E2, UC-24 E1, BR-68 |
| 14 | Sửa hoặc xóa nhật ký hệ thống | Không ai được thực hiện | FR-40, NFR-02, BR-79 |

---

## 10. Ghi vết liên quan đến phân quyền

| Nội dung | Quy định | Nguồn |
|---|---|---|
| Nhật ký hệ thống | Ghi người thực hiện, thời điểm, thao tác, giá trị trước và sau. Người thực hiện có thể là NV, QT, SV hoặc Hệ thống; không có liên kết người thực hiện nghĩa là Hệ thống tự động thực hiện. Chỉ xem, không ai được sửa hoặc xóa. | BR-79, FR-40, NFR-02, SRS 6.1 (DM-26), SRS 6.2 |
| Phạm vi ghi vết | Lịch sử mượn/trả, thay đổi tình trạng thiết bị và biên bản xử phạt được ghi vết nhật ký kiểm toán; dữ liệu nhật ký không thể chỉnh sửa hoặc xóa bởi bất kỳ người dùng nào. | NFR-02, BR-79 |
| Xem nhật ký | QT xem và lọc theo người dùng, đối tượng hoặc thời gian. | UC-23, FR-40 |
| Tài khoản NV và quyền theo phòng | Mọi thay đổi được ghi lại, ghi nhật ký. | FR-34, UC-14 |
| Cấu hình quy định mượn, phí, khung phạt, tham số | Mỗi thay đổi được ghi nhật ký giá trị trước/sau. | BR mục 14, UC-13, FR-40 |
| Hủy hoặc điều chỉnh biên bản vi phạm | Thay đổi được ghi nhật ký. | BR-07, UC-09 A2 |
| Duyệt hoặc từ chối phiếu yêu cầu | Ghi nhật ký. | UC-06 (hậu điều kiện), SRS SSD-02, SSD-03 |
| Gia hạn thành công | Ghi nhật ký. | BR-47, UC-04 |
| Lập quyết định thanh lý | Ghi nhật ký. | UC-15 |
| Cập nhật danh mục thiết bị | Ghi nhật ký. | UC-12 (hậu điều kiện) |
| Gỡ cấm mượn dài hạn | Ghi lại thao tác. | UC-24 |
| Nhận trả thiết bị | Hệ thống ghi NV nhận trả (NV đang đăng nhập). | BR-52, UC-08 |
| Nhắc nhở của NV | Mỗi lần nhắc được ghi lại. | BR-77, UC-18, FR-39 |

---

## 11. Yêu cầu, câu chuyện người dùng và đề nghị thay đổi liên quan

### 11.1 Yêu cầu chức năng và phi chức năng

| Mã | Nội dung liên quan đến phân quyền | Ưu tiên · Trạng thái | Use case |
|---|---|---|---|
| FR-33 | Đăng nhập theo vai trò: SV dùng tài khoản của trường; NV, QT dùng tài khoản do hệ thống cấp; SV không tự đăng ký; nhập sai mật khẩu quá số lần quy định thì tạm khóa đăng nhập | Bắt buộc · Bản nháp | UC-19, UC-13 |
| FR-34 | QT quản lý tài khoản NV (tạo, sửa, khóa/mở), cấp hoặc thu hồi quyền theo từng phòng thiết bị; NV chỉ thấy và thao tác với thiết bị, yêu cầu, phiếu của phòng được cấp quyền; tài khoản bị khóa không đăng nhập được | Bắt buộc · Bản nháp | UC-14, UC-18, UC-19 |
| FR-35 | Dùng tài khoản và dữ liệu SV do nhà trường cung cấp; không có chức năng tạo hoặc nhập danh sách SV; SV đã tốt nghiệp, thôi học hoặc bị đình chỉ không đặt trước được | Bắt buộc · Bản nháp | UC-02, UC-14, UC-19 |
| FR-32 | SV xem lịch sử mượn/trả của mình; chỉ hiển thị dữ liệu của chính SV đăng nhập | Bắt buộc · Bản nháp | UC-05 |
| FR-30 | NV xem danh sách phiếu yêu cầu của phòng mình, duyệt hoặc từ chối | Bắt buộc · Bản nháp | UC-06, UC-17 |
| FR-39 | NV xem danh sách SV sắp đến hạn hoặc quá hạn, lọc theo phòng thiết bị, gửi nhắc nhở; mỗi lần nhắc được ghi lại | Nên có · Bản nháp | UC-18 |
| FR-40 | QT xem nhật ký hệ thống; chỉ xem, không sửa hoặc xóa | Nên có · Bản nháp | UC-23, UC-13 |
| FR-09 | Tự động khóa quyền mượn thiết bị mới | Bắt buộc · Đã thay đổi | UC-17, UC-09 |
| FR-15 | Mở khóa tự động, không cần QT; không áp dụng với cấm mượn dài hạn | Bắt buộc · Đã thay đổi | UC-24, UC-09 |
| FR-42 | Cấm mượn dài hạn; QT gỡ cấm kèm lý do | Có thể có · Đã xem xét | UC-24, UC-09 |
| FR-20 | QT cấu hình quy định mượn theo nhóm SV và các tham số, gồm `soLanDangNhapSaiToiDa` | Bắt buộc · Đã thay đổi | UC-07, UC-17, UC-13, UC-19 |
| NFR-01 | Phân quyền nghiêm ngặt theo 3 vai trò và theo phòng thiết bị; 100% người dùng không thể truy cập ngoài phạm vi quyền được cấp | Bắt buộc · Đã thay đổi | UC-19, UC-14, UC-05 |
| NFR-02 | Ghi vết nhật ký kiểm toán; nhật ký không thể chỉnh sửa hoặc xóa bởi bất kỳ người dùng nào | Bắt buộc · Đã duyệt | UC-23 |
| NFR-05 | Mật khẩu NV, QT được mã hóa băm an toàn; không lưu mật khẩu SV; không lưu mật khẩu dạng rõ | Bắt buộc · Đã thay đổi | UC-19, UC-14 |

Các yêu cầu ở trạng thái **Bản nháp** vẫn được đưa vào SRS vì đã được Usecase v1.2 tham chiếu; giữ nhãn Bản nháp và cần nhóm duyệt trước khi chốt baseline (SRS mục 2.8, ĐX-07).

### 11.2 Độ ưu tiên của các use case liên quan trực tiếp

| Use case | Độ ưu tiên |
|---|---|
| UC-14 – Quản lý tài khoản nhân viên và phân quyền theo phòng | Bắt buộc |
| UC-19 – Đăng nhập | Bắt buộc |
| UC-18 – Nhắc nhở sinh viên sắp đến hạn hoặc quá hạn | Nên có |
| UC-23 – Xem nhật ký hệ thống | Nên có |
| UC-24 – Quản lý cấm mượn dài hạn | Có thể có (chưa đưa vào giai đoạn đầu) |

### 11.3 Câu chuyện người dùng

| Mã | Vai trò | Tôi muốn | Để | Cho trước | Khi | Thì |
|---|---|---|---|---|---|---|
| US-17 | Quản trị viên | phân quyền cho nhân viên quản lý thiết bị theo từng phòng thiết bị | mỗi người chỉ thao tác trong phạm vi chức năng và phòng thiết bị được cấp phép | đã có tài khoản nhân viên quản lý thiết bị và danh sách phòng thiết bị | tôi cấp hoặc thu hồi quyền của một nhân viên theo phòng thiết bị | hệ thống cập nhật quyền và nhân viên đó chỉ thao tác được với các phòng thiết bị đang được cấp quyền |
| US-24 | Sinh viên | đăng nhập bằng tài khoản sinh viên của trường | tôi sử dụng hệ thống mà không phải đăng ký tài khoản riêng | nhà trường đã cung cấp tài khoản của tôi | tôi đăng nhập bằng tài khoản của trường | hệ thống đưa tôi vào giao diện sinh viên và chỉ cho tôi xem dữ liệu của chính mình |
| US-25 | Quản trị viên | quản lý tài khoản nhân viên quản lý thiết bị | chỉ người được cấp phép mới thao tác tại quầy | tôi đã đăng nhập với vai trò quản trị viên | tôi tạo, sửa, khóa hoặc mở tài khoản của một nhân viên | hệ thống lưu thay đổi; tài khoản bị khóa không đăng nhập được |
| US-28 | Quản trị viên | hệ thống dùng dữ liệu sinh viên do nhà trường cung cấp | quy định mượn được áp dụng đúng cho từng sinh viên mà không phải nhập tay | nhà trường đã cung cấp và cập nhật dữ liệu sinh viên | sinh viên đặt trước hoặc được nhân viên quét mã thẻ | hệ thống áp giới hạn theo nhóm sinh viên và chặn thẻ hết hạn hoặc sinh viên đã thôi học, bị đình chỉ |
| US-31 | Quản trị viên | gỡ cấm mượn dài hạn của sinh viên | sinh viên chỉ được mượn lại khi tôi đồng ý | sinh viên đang bị cấm mượn dài hạn do vượt ngưỡng vi phạm trong học kỳ | tôi gỡ cấm và nhập lý do | hệ thống mở lại quyền mượn và ghi lại thao tác |

### 11.4 Đề nghị thay đổi và quy trình nghiệp vụ liên quan

| Mã | Nội dung liên quan đến phân quyền | Quyết định · Trạng thái | Nguồn |
|---|---|---|---|
| CR-13 | Tài khoản và dữ liệu sinh viên do nhà trường cung cấp (đồng bộ DDA); hệ thống không tạo hoặc nhập danh sách sinh viên; thay nội dung cũ của BP-12 | Chấp thuận · Đã áp dụng | SRS mục 10 |
| CR-15 | Bổ sung yêu cầu còn thiếu, gồm: duyệt phiếu yêu cầu, lịch sử mượn/trả, phân quyền theo phòng thiết bị, tiếp nhận sửa chữa, nhắc nhở thủ công, nhật ký hệ thống | Chấp thuận · Đã áp dụng | SRS mục 10 |
| CR-10 | Mở khóa quyền mượn tự động khi nhân viên xác nhận đã nhận đủ thiết bị và thu đủ tiền phạt | Chấp thuận · Đã áp dụng | SRS mục 10 |
| CR-04 | Cấm mượn dài hạn khi vượt 3 lần vi phạm trong học kỳ đến khi QT gỡ cấm | Chấp thuận · Đang phân tích | SRS mục 10 |
| CR-14 | Giữ CR-04 ở mức ưu tiên Thấp, chưa đưa vào giai đoạn đầu; số lần vi phạm tính cả không đến nhận; học kỳ theo hệ thống của nhà trường | Chấp thuận · Đã áp dụng | SRS mục 10 |
| BP-12 | Quy trình Quản lý tài khoản, xác thực và phân quyền người dùng (UC-14, UC-19); nội dung cũ đã được thay theo CR-13, luồng hiện hành xem UC-19, UC-14 [ĐX-PQ-10] | – | SRS 3.2, Phụ lục C (STT 1), ĐX-PQ-10 |

CR-04 được hiểu là đã chấp thuận nhưng chưa đưa vào giai đoạn đầu theo CR-14; nội dung nằm ở FR-42 và UC-24 (ĐX-07).

---

## 12. Các điểm nguồn chưa nêu hoặc nêu chưa rõ và quyết định xử lý bổ sung

Mười một điểm dưới đây **không có hoặc chưa rõ** trong BusinessRule.md, SRS.md, Usecase.md. Mỗi điểm có một quyết định xử lý bổ sung **[ĐX-PQ-xx]** do tài liệu này đề xuất, được suy ra từ các quy định có sẵn của nguồn (cột *Cơ sở*) sao cho đúng nghiệp vụ và nhất quán giữa các tài liệu. Các quyết định này **không phải nội dung nguồn**, được áp dụng làm mặc định trong tài liệu và cần nhóm duyệt; cột cuối nêu việc nên cập nhật ngược vào nguồn.

| Mã | Điểm chưa nêu hoặc chưa rõ | Nguồn nêu gì | Quyết định áp dụng | Cơ sở | Cập nhật ngược vào nguồn |
|---|---|---|---|---|---|
| ĐX-PQ-01 | Cách cấp tài khoản Quản trị viên | Tài khoản NV và QT "do hệ thống cấp", trong đó NV do QT tạo tại UC-14; không nêu cách cấp tài khoản QT | Tài khoản QT do hệ thống cấp sẵn khi triển khai; không có use case tạo tài khoản QT. UC-14 chỉ quản lý tài khoản nhân viên | BR-09, FR-33, UC-19; UC-14 (mô tả, Ghi chú) chỉ nêu tài khoản nhân viên | Nêu rõ trong UC-19 hoặc FR-33 |
| ĐX-PQ-02 | Phạm vi theo phòng thiết bị của QT | Phân quyền theo phòng được nêu cho NV; không nêu cho QT | QT không bị giới hạn theo phòng thiết bị; phân quyền theo phòng chỉ áp dụng cho NV. QT không thực hiện nghiệp vụ quầy (duyệt, giao, nhận), chỉ có các use case ở mục 5 | SRS 2.4 ("theo phòng thiết bị đối với NV"); FR-34, UC-14 (cấp quyền phòng cho NV); UC-12 (QT quản lý danh mục phòng); UC-16 (báo cáo toàn kho); BP-10 (quy định áp dụng thống nhất cho toàn hệ thống) | Nêu rõ trong NFR-01 hoặc FR-34 |
| ĐX-PQ-03 | Mức quyền chi tiết (xem/thêm/sửa/xóa) | Phân quyền ở mức use case và actor; không có ma trận xem/thêm/sửa/xóa | Quyền của mỗi vai trò là tập use case của vai trò đó với các hành động mô tả trong use case, trong phạm vi dữ liệu ở mục 4. Không lập ma trận xem/thêm/sửa/xóa riêng; hành động xóa chỉ có ở UC-12, theo quy tắc ngưng sử dụng khi đã có lịch sử | NFR-01, BR-04, UC-12 E2, FR-18, BR-18 | Không cần |
| ĐX-PQ-04 | Trạng thái "Tạm khóa đăng nhập" và cách mở lại | Hai trạng thái "Tạm khóa đăng nhập", "Bị khóa" đều không đăng nhập được; QT khóa/mở tài khoản NV; không nêu thời hạn, người mở lại | "Tạm khóa đăng nhập" do hệ thống tự đặt khi nhập sai quá `soLanDangNhapSaiToiDa`; "Bị khóa" do QT chủ động khóa. Tài khoản NV ở cả hai trạng thái do QT mở lại tại UC-14. Không có thời hạn tự động mở lại (nguồn không có tham số này). Tài khoản QT do bên vận hành hệ thống mở lại ngoài các use case (nhất quán ĐX-PQ-01) | SRS 6.3, UC-19 E2, E3, UC-14, FR-34, BR-10 | Ghi cách mở lại trong UC-14 và hồ sơ phân tích 1.5.1 |
| ĐX-PQ-05 | Khóa đăng nhập do nhập sai mật khẩu có áp dụng cho SV không | FR-33 và UC-19 (actor gồm SV) nêu chung; `trangThaiKhoa`, `soLanDangNhapSai` chỉ có ở NguoiDungHeThong; hệ thống không lưu mật khẩu SV | Áp dụng cho tài khoản do hệ thống quản lý (NV, QT). Với SV, xác thực và việc khóa do hệ thống tài khoản của nhà trường quản lý; hệ thống này không đếm số lần nhập sai của SV | NFR-05, BR-08, SRS 6.1 (DM-01, DM-04), UC-19 (Yêu cầu đặc biệt) | Nêu rõ phạm vi áp dụng trong FR-33 và UC-19 E2 |
| ĐX-PQ-06 | Cơ chế xác thực và đồng bộ tài khoản SV với nhà trường | Tài khoản và dữ liệu SV do nhà trường cung cấp; CR-13 ghi "đồng bộ DDA", không giải thích thêm | Ở mức yêu cầu: hệ thống xác thực SV bằng tài khoản của nhà trường và nhận dữ liệu SV (mã số sinh viên, họ tên, lớp, khoa, nhóm sinh viên, hạn thẻ, trạng thái học, học kỳ); dữ liệu cập nhật áp dụng cho giao dịch phát sinh sau. Giao thức và tần suất đồng bộ thuộc giai đoạn thiết kế. Giữ nguyên chữ "DDA" theo CR-13, không diễn giải | FR-35, BR-08, CR-13, US-28 | Làm rõ "DDA" trong CR-13 |
| ĐX-PQ-07 | Đổi mật khẩu, quên mật khẩu | Không có trong yêu cầu hiện hành; chi tiết cũ của BP-12 đã bị thay theo CR-13 | Không có chức năng đổi/quên mật khẩu riêng. SV: thuộc hệ thống tài khoản của nhà trường. NV: QT cấp lại mật khẩu qua thao tác sửa tài khoản tại UC-14 (UC-14 nêu mật khẩu được băm khi lưu). QT: bên vận hành hệ thống xử lý ngoài các use case | UC-14 (bước 2, 4), BR-08, BR-09, SRS Phụ lục C (STT 1) | Ghi rõ trong UC-14 |
| ĐX-PQ-08 | Chức năng của SV khi quyền mượn là Tạm khóa hoặc Cấm mượn dài hạn | BR-66, FR-09 chặn "đặt trước và lập phiếu mượn mới"; BR-23 "không được đặt trước hoặc mượn"; GL-19, GL-38 nói việc mượn thiết bị mới | Bị chặn: đặt trước, đăng ký chờ, lập phiếu mượn mới (kể cả mượn trực tiếp và giao từ danh sách chờ). Không bị chặn: tra cứu, xem lịch sử, hủy phiếu yêu cầu, rời danh sách chờ, gia hạn phiếu đang mượn theo điều kiện riêng của UC-04, phản hồi tình trạng thiết bị, nhận thông báo, trả thiết bị tại quầy (mục 8.5) | UC-03 là extend của UC-02 nên đăng ký chờ chịu cùng điều kiện đặt trước; UC-07 E1 chặn khi giao; BR-66 và GL-19 chỉ chặn việc mượn mới; BR-27, BR-37 | Thêm tiền điều kiện "quyền mượn bình thường" cho UC-03 |
| ĐX-PQ-09 | Phạm vi việc QT "điều chỉnh" biên bản vi phạm | GL-34: số tiền không đổi sau khi lập, chỉ QT được hủy hoặc điều chỉnh; UC-09 A2: thay đổi ghi nhật ký | "Điều chỉnh" là QT sửa nội dung biên bản khi có sai sót hoặc tranh chấp; NV và hệ thống không sửa số tiền sau khi lập. Mọi hủy/điều chỉnh ghi nhật ký giá trị trước/sau. Biên bản Đã hủy không còn được tính là biên bản chưa nộp khi xét khóa/mở khóa. Tác động của việc hủy biên bản lên số lần vi phạm (UC-24) không được quy định thêm | GL-34, BR-07, BR-62, BR-79, UC-09 A2, trạng thái biên bản Chưa nộp / Đã nộp / Đã hủy (SRS 6.3) | Mô tả "điều chỉnh" trong GL-34 và UC-09 A2 |
| ĐX-PQ-10 | Mô tả BP-12 trong SRS 3.2 còn nội dung cũ | Cột Mục tiêu còn nêu tài khoản định danh do QT cấp theo MSSV; cột Ghi chú nêu đã thay theo CR-13 | Giữ cách xử lý của SRS: theo CR-13, FR-33, FR-35, UC-19; không dùng nội dung cũ của BP-12 | SRS Phụ lục C (STT 1), CR-13 | Viết lại BP-12 trong P2_03 và cột Mục tiêu ở SRS 3.2 |
| ĐX-PQ-11 | Phạm vi phòng thiết bị với dữ liệu cấp sinh viên khi quét thẻ và xác nhận nhận đủ thiết bị | NV chỉ thấy và thao tác với thiết bị, phiếu yêu cầu, phiếu mượn của phòng được cấp quyền (BR-05, FR-34); quét thẻ SV hiển thị nhóm, hạn thẻ, số thiết bị đang mượn, lịch sử nợ vi phạm (SSD-04 bước 1); không nêu các dữ liệu cấp sinh viên này có bị giới hạn theo phòng hay không | Tình trạng cấp sinh viên (thẻ, nhóm, số thiết bị đang mượn, tổng nợ biên bản Chưa nộp) và việc xét mở khóa quyền mượn không giới hạn theo phòng, vì quyền mượn của SV áp dụng toàn hệ thống. Danh sách phiếu mượn và biên bản vẫn giới hạn theo phòng được cấp quyền | SSD-04, BR-23, BR-66, ĐX-API-24 và ĐX-API-25 (API-Notes.md) | Không cần |
