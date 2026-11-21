# Gói hợp đồng C1, C4, C6, C7, C8 (D01-K)

Cập nhật lần đầu: 13/11/2026 · Chủ sở hữu: Đinh Nhật Khôi · Trạng thái: **chưa được code**

Quy ước: "Đã có trong nguồn" là điều tài liệu dự án nêu. "OPEN" là điều tài liệu chưa nêu, phải chốt trước khi viết code ở ngày ghi trong cột "Chốt". Bản giả chỉ dùng trong `tests/`, không đăng ký trên runtime.

## C1 — Host/DbContext/DI, IClock/VnTime, port transaction/audit (khung D02, audit D03)

| Cổng | Vị trí | Đã có trong nguồn | OPEN | Ranh giới giao dịch | Lỗi | Bản giả kiểm thử | Chốt |
|---|---|---|---|---|---|---|---|
| `IClock` | `QuanLyMuonTra.SharedKernel` (KT 4.4) | Mọi phép tính thời gian dùng `IClock` để kiểm thử với thời gian giả lập (KT 8.5, CV 17.3) | Thành viên của interface | Ngoài giao dịch | Không | Đồng hồ giả trong tests (NFR-09, job) | D02 |
| `VnTime` | `QuanLyMuonTra.SharedKernel` (KT 4.4) | `VnTime.NgayLich(DateTimeOffset)` trả `DateOnly` theo `Asia/Ho_Chi_Minh`; lưu UTC (KT 8.5) | Cách nạp múi giờ trên Windows và Linux | Ngoài giao dịch | Không | Không cần | D02 |
| Port giao dịch | OPEN | Một `AppDbContext`, một giao dịch cho giao/trả/duyệt (KT 4.5, ĐX-KT-08; CV 4.5, 5.2). Nguồn **không đặt tên** port giao dịch | Có cần một cổng riêng hay dùng trực tiếp giao dịch của `AppDbContext` | Mở đầu ở bước 2 và kết thúc ở bước 8 của handler (CV 5.2) | `TRANG_THAI_KHONG_HOP_LE` cho vi phạm máy trạng thái (CV 5.1) | Không dùng giao dịch giả cho kiểm thử tích hợp (CV 17.3) | D02 |
| `ICurrentUser` | `QuanLyMuonTra.SharedKernel` (K-2) | Người thực hiện lấy từ claim; không có người dùng (job, bộ hẹn giờ) thì ghi rỗng, tức "Hệ thống" (KT 12.2, CV 11.2) | `SharedKernel` (đề xuất K-2); hiện thực `HttpContextCurrentUser` ở Api; đã code D03, chưa kiểm với JWT thật (D04) | Dùng trong giao dịch ghi nhật ký (bước 7 CV 5.2) | Không | Người dùng giả trong tests | D02 |
| `IAuditLogger` | `QuanLyMuonTra.SharedKernel` (K-2) | Sự kiện không đổi thực thể nào (ví dụ nhắc nhở) ghi qua `IAuditLogger` gọi tường minh; không module nào ghi thẳng schema `nhat_ky` (KT 12.2, CV 11.2) | `SharedKernel` (đề xuất K-2); hiện thực `AuditLogger` ở Infrastructure; yêu cầu giao dịch đang mở; đã kiểm hoàn tác trên PostgreSQL thật (D03) | Cùng giao dịch nghiệp vụ (bước 7) | Lỗi ghi nhật ký làm hoàn tác giao dịch (kiểm ở D03-K) | Bản ghi nhớ trong tests | D03 |

## C4 — Actor/policy/quyền phòng, hồ sơ SV, reader nợ, IQuyenMuonService (D03 nợ/tham chiếu, D04 guard/auth-toi, D07 hồ sơ/thẻ, D11 cổng ghi)

| Cổng | Vị trí | Đã có trong nguồn | OPEN | Ranh giới giao dịch | Lỗi | Bản giả kiểm thử | Chốt |
|---|---|---|---|---|---|---|---|
| `IQuyenPhongService` | `TaiKhoan.Contracts` (ĐX-CV-01) | Trả tập `maPhong` của NV đọc từ `tai_khoan.nhan_vien_phong`; đệm bộ nhớ 30 giây theo maNguoiDung (D06-K); xóa đệm qua IBoNhoDemQuyenPhong.Xoa (TaiKhoan.Application) sau khi giao dịch đổi quyền commit; truy cập từng đối tượng gọi `EnsureAsync(maNguoiDung, maPhong)` (KT 7.2, CV 10.2) | Đã chốt: LayDsMaPhongAsync, EnsureAsync, kiểu Guid (D04-K) | Gọi **trước `BEGIN`** (bước 1 CV 5.2) | `PHONG_CHUA_CAP_QUYEN` | Bản giả chỉ trong tests; runtime dùng bản thật | Đã chốt (D04-K) |
| Chính sách phân quyền | `QuanLyMuonTra.Api` | Bốn chính sách `SinhVien`, `NhanVien`, `QuanTri`, `NhanVienQuanTri`; gắn ở cấp nhóm endpoint (KT 7.2, CV 5.4) | Đã chốt: cài guard qua VaiTroHoatDongRequirement và kiểm tra trạng thái tài khoản mỗi request (D04-K) | Ngoài giao dịch | 401/403 theo `maLoi` ở bảng lỗi | Không | Đã chốt (D04-K) |
| `ISchoolDirectory` | `SinhVien.Application` (KT 7.1) | Đủ chữ ký: `XacThucAsync(string mssv, string matKhau, CancellationToken ct)`, `LayTheoMssvAsync(string mssv, CancellationToken ct)`, `LayThayDoiTuAsync(DateTimeOffset? moc, CancellationToken ct)` | Đã chốt D05-K: KetQuaXacThucTruong(ThanhCong, ThongTin), SinhVienTruongDto(MaSv, HoTen, Lop, Khoa, Email, MaNhom, HanThe, TrangThaiHoc, HocKy); D05 chỉ cài XacThucAsync (K-10) | Cập nhật bản ghi SV ngay khi đăng nhập thành công (KT 7.1) | Theo bảng lỗi xác thực SV | Không có bản giả runtime; bản chạy thật `MockSchoolDirectory` đọc `truong_gia_lap` (KT-05) | Đã chốt (D05-K, K-9, K-10) |
| Reader hồ sơ SV | `SinhVien.Contracts` (đề xuất) | Quét thẻ trả SV, `hopLe`, `dsLyDoKhongHopLe` (API-Notes) | Tên, kiểu dữ liệu, G1 (SV khác nhau) | Chỉ đọc | Theo ĐX-API-08 | Dữ liệu giả không tự đặt chính sách G1 | D07 |
| Reader nợ | `SinhVien.Contracts` (K-3); ViPham hiện thực | Tổng biên bản `CHUA_NOP` toàn hệ thống do `ViPham` đọc bảng thật (D03-K) | `IBienBanChuaNopReader` ở `SinhVien.Contracts` (đề xuất K-3); ViPham hiện thực; đã kiểm trên DB thật; chưa được MuonTra/SinhVien dùng | Chỉ đọc, trong giao dịch của người gọi | Nếu thiếu bản triển khai: chặn an toàn, không trả "không có nợ" (D03-K) | Bản giả chỉ trong DI của tests | D03 |
| `IQuyenMuonService` | `SinhVien.Contracts` (đề xuất; `ViPham → SinhVien` đã có) | `ViPham` gọi `IQuyenMuonService.Khoa(...)` (KT 4.5); mở khóa chạy theo sự kiện, không do job (KT 8.6); `trangThaiQuyenMuon` không bị đồng bộ ghi đè (KT 7.1); `ViPham` quyết định điều kiện khóa/mở, `SinhVien` chỉ cập nhật cùng giao dịch (kế hoạch C4) | Tên phương thức mở khóa; kiểu tham số/trả về | **Cùng giao dịch** với biên bản/nộp phạt (KT 8.6) | `TRANG_THAI_KHONG_HOP_LE` khi sai trạng thái | Bản giả chỉ trong tests | D11 |

> **Ghi chú D03-K:** Giới hạn đã biết: thực thể Added có khóa do DB sinh (mã chứng từ `PM-...`, `BB-...` sinh bởi DEFAULT + sequence) thì interceptor chưa biết khóa → `doi_tuong_id` = NULL (sẽ xử lý ở handler lập chứng từ D09-V, D15-K). Nghĩa vụ: mỗi biên bản vi phạm phải có ít nhất một dòng `vi_pham.vi_pham` để reader nợ xác định được mã sinh viên nợ mà không cần JOIN vượt biên sang schema `muon_tra`.

## C6 — Upload/Storage/attach metadata/signed URL và quyền ảnh (D08)

| Cổng | Vị trí | Đã có trong nguồn | OPEN | Ranh giới giao dịch | Lỗi | Bản giả kiểm thử | Chốt |
|---|---|---|---|---|---|---|---|
| `IAnhStorage` | OPEN (L7); hiện thực `SupabaseAnhStorage` ở `Infrastructure` | Ba thao tác `Upload`, `TaoUrlKy`, `Xoa`; bucket `thiet-bi-anh` (công khai chỉ đọc), `tinh-trang-anh` (riêng tư); khóa lưu `{loai}/{yyyy}/{mm}/{uuid}` do server sinh; URL ký hiệu lực 10 phút (KT 11.1, CV 14.1, 3.2) | Vị trí; tên theo quy ước `Async`; kiểu tham số/trả về | Ghi Storage ngoài giao dịch DB; gắn ảnh vào đối tượng (`doi_tuong_loai`, `doi_tuong_id`, `giai_doan`) cùng giao dịch lập phiếu/ghi kết quả (KT 11.1) | Kiểm tra magic bytes, tối đa 2 MB, 5 ảnh mỗi thiết bị mỗi lần (KT 11.2); mã lỗi cụ thể theo bảng KT 6.6 | Bản giả chỉ trong tests | D08 |
| Quyền đọc ảnh riêng tư | `Infrastructure`/handler (D08) | Chỉ cấp URL ký sau khi kiểm quyền: phòng của NV hoặc SV chủ phiếu (KT 11.1) | Điểm gọi kiểm tra | Chỉ đọc | `PHONG_CHUA_CAP_QUYEN` hoặc mã tương ứng | Bản giả chỉ trong tests | D08 |

> **Ghi chú D08-K:** `IAnhService` (SharedKernel) gồm `TaiLenAsync(giaiDoan, Stream, ct)`, `GanAsync(dsMaAnh, doiTuongLoai, doiTuongId, giaiDoan, ct)`, `GoAnhThietBiAsync(maThietBi, dsMaAnh, ct)`, `LayDsAnhKyAsync(doiTuongLoai, doiTuongId, giaiDoan, ct)`, `LayDsAnhCongKhaiAsync(doiTuongLoai, doiTuongId, ct)`; hằng số `MaGiaiDoanAnh`, `MaDoiTuongAnh`; DTO `AnhRefDto(MaAnh, Url)`. `IAnhStorage` (`UploadAsync`, `TaoUrlKyAsync`, `XoaAsync`, `TaoUrlCongKhai`) và `SupabaseAnhStorage` ở `Infrastructure/LuuTru` (K-22). Đăng ký bằng `AddAnhLuuTru(SupabaseStorageOptions)`.
> Cách dùng: V10 gọi `GanAsync(..., ChiTietPhieuMuon, maChiTiet.ToString(), BanDau)` và V15 với `KhiTra` trong giao dịch của chính mình; N10 gỡ ảnh cũ bằng `GoAnhThietBiAsync` trước rồi `GanAsync(..., ThietBi, maThietBi, ThietBi)` (cho phép quản trị viên gắn/kế thừa ảnh thiết bị do quản trị viên khác tải lên). Giới hạn 5 ảnh tính cả ảnh đã gắn và được chống race condition bằng `pg_advisory_xact_lock`. Mọi vi phạm dữ liệu trả 400 `DU_LIEU_KHONG_HOP_LE`; lỗi dùng sai tổ hợp đối tượng–giai đoạn ném `ArgumentException`.
> Quyền đọc ảnh riêng tư (đóng dòng OPEN "Điểm gọi kiểm tra", K-25): V11 và V14 kiểm quyền phòng hoặc chủ phiếu trước, rồi mới gọi `LayDsAnhKyAsync` (URL hết hạn sau 10 phút, tự bỏ qua ảnh lỗi trong Storage và log warning). `AnhService` không tự kiểm quyền. Ảnh thiết bị gọi trực tiếp `LayDsAnhCongKhaiAsync` lấy URL công khai (đóng O-D08-1, K-28).
> Giới hạn đã biết: ghi Storage nằm ngoài giao dịch DB, ghi dòng thất bại thì xóa bù object; nếu xóa bù cũng lỗi, object mồ côi trong Storage không được J9 dọn (J9 dựa trên dòng siêu dữ liệu).

## C7 — Outbox/notification/event, template, sender adapter (outbox/sự kiện D09; sender/job D10)

| Cổng | Vị trí | Đã có trong nguồn | OPEN | Ranh giới giao dịch | Lỗi | Bản giả kiểm thử | Chốt |
|---|---|---|---|---|---|---|---|
| `IThongBaoOutbox` | `ThongBao.Contracts` (suy ra từ cạnh `MuonTra/ViPham/BaoTri → ThongBao` của KT 4.2) | Handler chỉ ghi dòng `thong_bao`, không gọi gửi email trực tiếp (CV 5.2, 13); khóa chống trùng `khoa_trung_lap` dạng `{LOAI}:{maDoiTuong}[:{moc}]` (CV 3.2); NV không có email thì không tạo dòng `EMAIL`, vẫn ghi `UNG_DUNG` (KT 10.2). Chữ ký `GhiAsync(YeuCauGhiThongBao, CancellationToken)`; người gọi phải đưa người nhận vào `{moc}` của khóa khi gửi nhiều người (K-30b); dùng `INhanVienEmailReader` (K-31) | Đã chốt ở K-30 | **Cùng giao dịch** nghiệp vụ, bước 6 CV 5.2 | Lỗi gửi không làm hỏng nghiệp vụ (KT 10.2) | Bản giả chỉ trong tests; runtime không dùng bản giả | D09 (đã làm) |
| `IEmailSender` | `SharedKernel` (port, K-29); hiện thực `HttpEmailSender`, `LogEmailSender` ở `Infrastructure/Email` (KT 10.3) | Gửi qua HTTP API (không SMTP); cấu hình `Email__Provider`, `Email__ApiKey`, `Email__From`; `Log` cho dev, `Http` cho production; `GuiAsync(nguoiNhan, tieuDe, noiDungHtml, ct)`. Do J7 (D10-K) gọi, handler nghiệp vụ không gọi | Đã chốt ở K-29, K-32 | Ngoài giao dịch DB (job gửi sau) | Thử lại tối đa 5 lần, giãn cách 1, 5, 15, 60 phút; sau đó `LOI_VINH_VIEN` (KT 10.2) | Bộ gửi giả cho tests; `LogEmailSender` là bản dev, không phải bản giả test | D09 |
| Sự kiện miền | Khai báo trong `Contracts` của module phát (KT 4.5, CV 3.2) | `GiuChoHetHan`, `PhieuYeuCauDaDuyet`, `BienBanDaLap`, `ThietBiDenHanBaoTri`, `ViPhamDaGhiNhan`; gửi **trước khi commit**, cùng giao dịch | Cơ chế phát/nhận chưa làm ở D09, hoãn tới D17-K (K-33) | Cùng giao dịch | Không | Không | Hoãn D17 |
| Mẫu nội dung | Nhúng trong ứng dụng | Tiếng Việt, mỗi loại một mẫu HTML, chỉ chứa thông tin cần thiết (KT 10.2). Sáu loại có mẫu trong `MauThongBao` (K-30) | Đã chốt ở K-30 | Không | Không | Không | D09 |
| Phục hồi `DANG_GUI` | — | Không có quy tắc trong nguồn | **G4** | — | — | — | D03 |

> **Ghi chú cho D10-V:** Chuyển `ThongBaoDuyetHoacTuChoi` thành `YeuCauGhiThongBao` loại `DUYET_HOAC_TU_CHOI` với tham số `maYeuCau`, `ketQua` (ví dụ "đã được duyệt"/"đã bị từ chối"), `chiTiet` (hạn giữ chỗ hoặc lý do), khóa `DUYET:{maYeuCau}`; ghi trong cùng giao dịch của handler duyệt/từ chối.

## C8 — Runner/lock từng job/IClock/test harness (D10)

| Thành phần | Vị trí | Đã có trong nguồn | OPEN | Ranh giới giao dịch | Lỗi | Bản giả kiểm thử | Chốt |
|---|---|---|---|---|---|---|---|
| `SchedulerHostedService` | `Infrastructure` (KT 4.1: phần chạy lịch ở hạ tầng; CV 3.2) | `BackgroundService` + `PeriodicTimer`; quét mỗi phút; tick đầu sau khi khởi động chạy mọi job ngay; job viết theo mốc thời gian nên tự bắt kịp; mỗi job chạy lại không tạo kết quả trùng (KT-07, KT 3.4, FR-41) | Interface của job (nguồn không đặt tên) | Mỗi job tự quản giao dịch | Lỗi job không dừng scheduler (cần xác nhận ở D10) | Bộ chạy với đồng hồ giả (CV 17.1) | D10 |
| Khóa từng job | `Infrastructure` | Có "khóa tư vấn" trong danh sách kiểm thử job (CV 17.1) | Cách lấy khóa và tên khóa | Theo job | Không | Tích hợp thật trên CI | D10 |
| Job J1–J9 | Module sở hữu dữ liệu tương ứng (KT 4.1, 9.2) | Tên job: `NhacHanTra`, `DanhDauQuaHan`, `KhoaQuyenMuon`, `HetHanGiuCho`, `HetHieuLucPhieuChoDuyet`, `NhacBaoTri`, `GuiEmail`, `DongBoSinhVien`, `DonAnhMoCo`; `GuiEmail` là J7 (KT 9.2) | Ánh xạ J-số cho các job còn lại: xem KT 9.2 khi viết | Theo job | Không | Không | D10 |
| Test harness | `tests/` | Kiểm thử job bằng đồng hồ giả lập và scheduler thật (CV 17.1; kế hoạch mục 5, NFR-08) | Khung dùng chung | Không | Không | Đồng hồ giả | D10 |

## Ngoài phạm vi D01-K

`IThietBiService`, `IThamChieuDanhMucProvider`, `IThamChieuKhungPhatProvider` thuộc gói C3 (Lợi). `IPhieuMuonReader` thuộc gói C5 (Vương). C2/C9 (khả dụng, khóa, FIFO) thuộc Vương. Khôi chỉ hiện thực các provider tham chiếu phía `ViPham` ở D03.
