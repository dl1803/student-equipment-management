# Gói hợp đồng C1, C4, C6, C7, C8 (D01-K)

Cập nhật lần đầu: 13/11/2026 · Chủ sở hữu: Đinh Nhật Khôi · Trạng thái: **chưa được code**

Quy ước: "Đã có trong nguồn" là điều tài liệu dự án nêu. "OPEN" là điều tài liệu chưa nêu, phải chốt trước khi viết code ở ngày ghi trong cột "Chốt". Bản giả chỉ dùng trong `tests/`, không đăng ký trên runtime.

## C1 — Host/DbContext/DI, IClock/VnTime, port transaction/audit (khung D02, audit D03)

| Cổng | Vị trí | Đã có trong nguồn | OPEN | Ranh giới giao dịch | Lỗi | Bản giả kiểm thử | Chốt |
|---|---|---|---|---|---|---|---|
| `IClock` | `QuanLyMuonTra.SharedKernel` (KT 4.4) | Mọi phép tính thời gian dùng `IClock` để kiểm thử với thời gian giả lập (KT 8.5, CV 17.3) | Thành viên của interface | Ngoài giao dịch | Không | Đồng hồ giả trong tests (NFR-09, job) | D02 |
| `VnTime` | `QuanLyMuonTra.SharedKernel` (KT 4.4) | `VnTime.NgayLich(DateTimeOffset)` trả `DateOnly` theo `Asia/Ho_Chi_Minh`; lưu UTC (KT 8.5) | Cách nạp múi giờ trên Windows và Linux | Ngoài giao dịch | Không | Không cần | D02 |
| Port giao dịch | OPEN | Một `AppDbContext`, một giao dịch cho giao/trả/duyệt (KT 4.5, ĐX-KT-08; CV 4.5, 5.2). Nguồn **không đặt tên** port giao dịch | Có cần một cổng riêng hay dùng trực tiếp giao dịch của `AppDbContext` | Mở đầu ở bước 2 và kết thúc ở bước 8 của handler (CV 5.2) | `TRANG_THAI_KHONG_HOP_LE` cho vi phạm máy trạng thái (CV 5.1) | Không dùng giao dịch giả cho kiểm thử tích hợp (CV 17.3) | D02 |
| `ICurrentUser` | OPEN (L7) | Người thực hiện lấy từ claim; không có người dùng (job, bộ hẹn giờ) thì ghi rỗng, tức "Hệ thống" (KT 12.2, CV 11.2) | Vị trí; thành viên | Dùng trong giao dịch ghi nhật ký (bước 7 CV 5.2) | Không | Người dùng giả trong tests | D02 |
| `IAuditLogger` | OPEN (L7) | Sự kiện không đổi thực thể nào (ví dụ nhắc nhở) ghi qua `IAuditLogger` gọi tường minh; không module nào ghi thẳng schema `nhat_ky` (KT 12.2, CV 11.2) | Vị trí; thành viên | Cùng giao dịch nghiệp vụ (bước 7) | Lỗi ghi nhật ký làm hoàn tác giao dịch (kiểm ở D03-K) | Bản ghi nhớ trong tests | D03 |

## C4 — Actor/policy/quyền phòng, hồ sơ SV, reader nợ, IQuyenMuonService (D03 nợ/tham chiếu, D04 guard/auth-toi, D07 hồ sơ/thẻ, D11 cổng ghi)

| Cổng | Vị trí | Đã có trong nguồn | OPEN | Ranh giới giao dịch | Lỗi | Bản giả kiểm thử | Chốt |
|---|---|---|---|---|---|---|---|
| `IQuyenPhongService` | `TaiKhoan.Contracts` (ĐX-CV-01) | Trả tập `maPhong` của NV đọc từ `tai_khoan.nhan_vien_phong`; cache bộ nhớ 30 giây, xóa cache khi QT đổi quyền (UC-14); truy cập từng đối tượng gọi `EnsureAsync(maNV, maPhong)` (KT 7.2, CV 10.2) | Tên phương thức lấy tập `maPhong`; kiểu tham số/trả về | Gọi **trước `BEGIN`** (bước 1 CV 5.2) | `PHONG_CHUA_CAP_QUYEN` | Bản giả chỉ trong tests; runtime dùng bản thật | D04 |
| Chính sách phân quyền | `QuanLyMuonTra.Api` | Bốn chính sách `SinhVien`, `NhanVien`, `QuanTri`, `NhanVienQuanTri`; gắn ở cấp nhóm endpoint (KT 7.2, CV 5.4) | Cách cài đặt guard và kiểm tra trạng thái tài khoản mỗi request (D04-K) | Ngoài giao dịch | 401/403 theo `maLoi` ở bảng lỗi | Không | D04 |
| `ISchoolDirectory` | `SinhVien.Application` (KT 7.1) | Đủ chữ ký: `XacThucAsync(string mssv, string matKhau, CancellationToken ct)`, `LayTheoMssvAsync(string mssv, CancellationToken ct)`, `LayThayDoiTuAsync(DateTimeOffset? moc, CancellationToken ct)` | Thuộc tính của `KetQuaXacThucTruong` và `SinhVienTruongDto` (nguồn chỉ liệt kê dữ liệu SV: MSSV, họ tên, lớp, khoa, email, nhóm, hạn thẻ, trạng thái học, học kỳ) | Cập nhật bản ghi SV ngay khi đăng nhập thành công (KT 7.1) | Theo bảng lỗi xác thực SV | Không có bản giả runtime; bản chạy thật `MockSchoolDirectory` đọc `truong_gia_lap` (KT-05) | Chưa xác định trong D01 (xem kế hoạch ngày của K khi cài đăng nhập SV) |
| Reader hồ sơ SV | `SinhVien.Contracts` (đề xuất) | Quét thẻ trả SV, `hopLe`, `dsLyDoKhongHopLe` (API-Notes) | Tên, kiểu dữ liệu, G1 (SV khác nhau) | Chỉ đọc | Theo ĐX-API-08 | Dữ liệu giả không tự đặt chính sách G1 | D07 |
| Reader nợ | OPEN (L8; đề xuất: giao diện ở `SinhVien.Contracts`, `ViPham` hiện thực) | Tổng biên bản `CHUA_NOP` toàn hệ thống do `ViPham` đọc bảng thật (D03-K) | Vị trí, tên, kiểu trả về; xử lý chu trình | Chỉ đọc, trong giao dịch của người gọi | Nếu thiếu bản triển khai: chặn an toàn, không trả "không có nợ" (D03-K) | Bản giả chỉ trong DI của tests | D03 |
| `IQuyenMuonService` | `SinhVien.Contracts` (đề xuất; `ViPham → SinhVien` đã có) | `ViPham` gọi `IQuyenMuonService.Khoa(...)` (KT 4.5); mở khóa chạy theo sự kiện, không do job (KT 8.6); `trangThaiQuyenMuon` không bị đồng bộ ghi đè (KT 7.1); `ViPham` quyết định điều kiện khóa/mở, `SinhVien` chỉ cập nhật cùng giao dịch (kế hoạch C4) | Tên phương thức mở khóa; kiểu tham số/trả về | **Cùng giao dịch** với biên bản/nộp phạt (KT 8.6) | `TRANG_THAI_KHONG_HOP_LE` khi sai trạng thái | Bản giả chỉ trong tests | D11 |

## C6 — Upload/Storage/attach metadata/signed URL và quyền ảnh (D08)

| Cổng | Vị trí | Đã có trong nguồn | OPEN | Ranh giới giao dịch | Lỗi | Bản giả kiểm thử | Chốt |
|---|---|---|---|---|---|---|---|
| `IAnhStorage` | OPEN (L7); hiện thực `SupabaseAnhStorage` ở `Infrastructure` | Ba thao tác `Upload`, `TaoUrlKy`, `Xoa`; bucket `thiet-bi-anh` (công khai chỉ đọc), `tinh-trang-anh` (riêng tư); khóa lưu `{loai}/{yyyy}/{mm}/{uuid}` do server sinh; URL ký hiệu lực 10 phút (KT 11.1, CV 14.1, 3.2) | Vị trí; tên theo quy ước `Async`; kiểu tham số/trả về | Ghi Storage ngoài giao dịch DB; gắn ảnh vào đối tượng (`doi_tuong_loai`, `doi_tuong_id`, `giai_doan`) cùng giao dịch lập phiếu/ghi kết quả (KT 11.1) | Kiểm tra magic bytes, tối đa 2 MB, 5 ảnh mỗi thiết bị mỗi lần (KT 11.2); mã lỗi cụ thể theo bảng KT 6.6 | Bản giả chỉ trong tests | D08 |
| Quyền đọc ảnh riêng tư | `Infrastructure`/handler (D08) | Chỉ cấp URL ký sau khi kiểm quyền: phòng của NV hoặc SV chủ phiếu (KT 11.1) | Điểm gọi kiểm tra | Chỉ đọc | `PHONG_CHUA_CAP_QUYEN` hoặc mã tương ứng | Bản giả chỉ trong tests | D08 |

## C7 — Outbox/notification/event, template, sender adapter (outbox/sự kiện D09; sender/job D10)

| Cổng | Vị trí | Đã có trong nguồn | OPEN | Ranh giới giao dịch | Lỗi | Bản giả kiểm thử | Chốt |
|---|---|---|---|---|---|---|---|
| `IThongBaoOutbox` | `ThongBao.Contracts` (suy ra từ cạnh `MuonTra/ViPham/BaoTri → ThongBao` của KT 4.2) | Handler chỉ ghi dòng `thong_bao`, không gọi gửi email trực tiếp (CV 5.2, 13); khóa chống trùng `khoa_trung_lap` dạng `{LOAI}:{maDoiTuong}[:{moc}]` (CV 3.2); NV không có email thì không tạo dòng `EMAIL`, vẫn ghi `UNG_DUNG` (KT 10.2) | Thành viên; kiểu tham số | **Cùng giao dịch** nghiệp vụ, bước 6 CV 5.2 | Lỗi gửi không làm hỏng nghiệp vụ (KT 10.2) | Bản giả chỉ trong tests; runtime không dùng bản giả | D09 |
| `IEmailSender` | `ThongBao.Application` (port); hiện thực `HttpEmailSender`, `LogEmailSender` ở `Infrastructure` (KT 10.3) | Gửi qua HTTP API (không SMTP); cấu hình `Email__Provider`, `Email__ApiKey`, `Email__From`; `Log` cho dev, `Http` cho production; tham số mẫu `Gui(nguoiNhan, tieuDe, noiDung)` (KT 10.2) | Tên theo quy ước `Async`; kiểu tham số | Ngoài giao dịch DB (job gửi sau) | Thử lại tối đa 5 lần, giãn cách 1, 5, 15, 60 phút; sau đó `LOI_VINH_VIEN` (KT 10.2) | Bộ gửi giả cho tests; `LogEmailSender` là bản dev, không phải bản giả test | D10 |
| Sự kiện miền | Khai báo trong `Contracts` của module phát (KT 4.5, CV 3.2) | `GiuChoHetHan`, `PhieuYeuCauDaDuyet`, `BienBanDaLap`, `ThietBiDenHanBaoTri`, `ViPhamDaGhiNhan`; gửi **trước khi commit**, cùng giao dịch | Cơ chế phát/nhận trong tiến trình; thuộc tính từng sự kiện | Cùng giao dịch | Không | Không | D09 |
| Mẫu nội dung | Nhúng trong ứng dụng | Tiếng Việt, mỗi loại một mẫu HTML, chỉ chứa thông tin cần thiết (KT 10.2) | Nội dung từng mẫu | Không | Không | Không | D09 |
| Phục hồi `DANG_GUI` | — | Không có quy tắc trong nguồn | **G4** | — | — | — | D03 |

## C8 — Runner/lock từng job/IClock/test harness (D10)

| Thành phần | Vị trí | Đã có trong nguồn | OPEN | Ranh giới giao dịch | Lỗi | Bản giả kiểm thử | Chốt |
|---|---|---|---|---|---|---|---|
| `SchedulerHostedService` | `Infrastructure` (KT 4.1: phần chạy lịch ở hạ tầng; CV 3.2) | `BackgroundService` + `PeriodicTimer`; quét mỗi phút; tick đầu sau khi khởi động chạy mọi job ngay; job viết theo mốc thời gian nên tự bắt kịp; mỗi job chạy lại không tạo kết quả trùng (KT-07, KT 3.4, FR-41) | Interface của job (nguồn không đặt tên) | Mỗi job tự quản giao dịch | Lỗi job không dừng scheduler (cần xác nhận ở D10) | Bộ chạy với đồng hồ giả (CV 17.1) | D10 |
| Khóa từng job | `Infrastructure` | Có "khóa tư vấn" trong danh sách kiểm thử job (CV 17.1) | Cách lấy khóa và tên khóa | Theo job | Không | Tích hợp thật trên CI | D10 |
| Job J1–J9 | Module sở hữu dữ liệu tương ứng (KT 4.1, 9.2) | Tên job: `NhacHanTra`, `DanhDauQuaHan`, `KhoaQuyenMuon`, `HetHanGiuCho`, `HetHieuLucPhieuChoDuyet`, `NhacBaoTri`, `GuiEmail`, `DongBoSinhVien`, `DonAnhMoCo`; `GuiEmail` là J7 (KT 9.2) | Ánh xạ J-số cho các job còn lại: xem KT 9.2 khi viết | Theo job | Không | Không | D10 |
| Test harness | `tests/` | Kiểm thử job bằng đồng hồ giả lập và scheduler thật (CV 17.1; kế hoạch mục 5, NFR-08) | Khung dùng chung | Không | Không | Đồng hồ giả | D10 |

## Ngoài phạm vi D01-K

`IThietBiService`, `IThamChieuDanhMucProvider`, `IThamChieuKhungPhatProvider` thuộc gói C3 (Lợi). `IPhieuMuonReader` thuộc gói C5 (Vương). C2/C9 (khả dụng, khóa, FIFO) thuộc Vương. Khôi chỉ hiện thực các provider tham chiếu phía `ViPham` ở D03.
