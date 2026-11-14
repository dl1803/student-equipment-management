# C0 — Sổ quyết định và xác nhận phạm vi (D01-K)

Cập nhật lần đầu: 13/11/2026 · Cập nhật ngày 02: 14/11/2026 (D02-K) · Người lập: Đinh Nhật Khôi · Trạng thái chung: **OPEN**

## 1. Xác nhận phạm vi (D01-K, việc 1)

| Hạng mục | Kết quả xác nhận | Nguồn |
|---|---|---|
| Use case giai đoạn 1 | 17 UC: UC-01, 02, 03, 05, 06, 07, 08, 09, 10, 11, 12, 13, 14, 15, 16, 17, 19 (16 UC Bắt buộc + UC-03 do phụ thuộc) | KT 1.2, KT-10, ĐX-KT-01 |
| Thao tác API | 88 operation trong `openapi.yaml`; manifest ở `C0-manifest-operation.csv`, tất cả `CHUA_CAI` | `openapi.yaml` |
| Phân công | Khôi: Host, `AppDbContext`, model snapshot và thứ tự migration, CI chung, xác thực/hạ tầng, TaiKhoan, SinhVien, ThongBao, NhatKy, ViPham. Vương: MuonTra, khả dụng/FIFO. Lợi: DanhMuc, QuyDinh, BaoTri, BaoCao. Long: Android. Thịnh: web | Kế hoạch mục 4 và các hàng D01 |
| Baseline tài liệu | 16 tài liệu kỹ thuật trong repo, SHA-256 ở `C0-baseline-sha256.txt`. 5 tài liệu nguồn ngoài repo chưa có SHA | Kế hoạch mục 9 |

## 2. Quyết định nhóm đã cung cấp

| # | Quyết định | Ngày ghi | Ghi chú |
|---|---|---|---|
| Q1 | Solution backend đặt trong `backend/` | 13/11/2026 | Thay `apps/api/` của kế hoạch mục 4 |
| Q2 | Nhóm chốt .NET 9 | 13/11/2026 | Đã đồng bộ KT-13, KT 3.3, CV 2.1 và hai README (commit đồng bộ 13/11/2026). Còn lệch: Dockerfile `10.0` ở KT 16.1 và CV 18.2, chưa sửa vì nhóm yêu cầu không đụng Docker |
| Q3 | Nhánh chính `main`, nhánh nền `develop` | 13/11/2026 | Đã đồng bộ CV 18.3. Cách đưa `develop` vào `main` nhóm chưa quy định |
| Q4 | Không dùng PostgreSQL cục bộ hay Docker Desktop; dùng PostgreSQL trên Supabase | 13/11/2026 | Đã xác nhận Supabase. Đã đồng bộ KT 15, 16.4, 17 và CV 16, 17.1, 17.3, 18.1 |
| Q5 | Kiểm thử tích hợp và đồng thời chạy trên PostgreSQL Supabase (môi trường `test`), không dùng Docker | 13/11/2026 | Do Khôi giao mình quyết; tên `ConnectionStrings__Test` do mình đề xuất |

## 3. Danh sách quyết định cần chốt

Mọi dòng dưới đây đang **OPEN**. Một quyết định OPEN chặn đúng nhánh ghi ở cột "Chặn" và không chặn phần còn lại.

| Mã | Nội dung cần chốt | Người quyết định | Hạn | Dữ kiện nguồn đã xác minh | Chặn |
|---|---|---|---|---|---|
| G1 | SV trên thẻ và nguồn: xử lý khi hai SV khác nhau (dừng, quét lại, đổi nguồn hoặc phương án được nhóm duyệt). Ghi danh tính cuối, lỗi, cách xóa ngữ cảnh, dữ liệu gửi khi lập phiếu | Khôi cùng Vương, Thịnh; nhóm quyết định nghiệp vụ | D02 | `GET /quay/sinh-vien/the/{maThe}` trả thông tin SV, `hopLe`, `dsLyDoKhongHopLe` (API-Notes); ĐX-API-08: các kiểm tra hiển thị lý do trả 200 kèm `hopLe`/`duocDuyet` và `dsLyDo`. `hopLe=true` chưa đủ khi chưa chốt nhánh khác SV | Kiểm nguồn và lập phiếu giao thật, UAT tương ứng. Không chặn danh mục, phần đọc Android |
| G2 | Lượng giao và giữ chỗ: mỗi loại phải giao đủ hay cho phép một phần; thiếu/thừa/sai loại/trùng mã; tiêu thụ giữ chỗ, lượng dư, trạng thái YC/DK, giao lại | Vương chủ trì nghiệp vụ, Khôi ghi quyết định; Thịnh dùng hợp đồng | D02 | Chưa chọn exact-match hay partial (kế hoạch yêu cầu không tự chọn) | Giao dịch giao thật. Vẫn làm nhập mã, ảnh, nhánh độc lập |
| G3 | Cách tìm lại hồ sơ bảo trì mở/`maBaoTri` sau khi tải lại. Nếu cần GET mới: đường dẫn, DTO, quyền, lỗi; cập nhật YAML, client sinh tự động, dữ liệu thử, kiểm thử cùng lúc | Lợi, Khôi, Thịnh | D02 chốt; API xong trước ngày web tích hợp | `openapi.yaml` hiện có `POST /quay/bao-tri`, `POST /quay/bao-tri/{ma}/hoan-thanh`, `GET /quay/bao-tri/den-han`, `GET /quay/bao-tri/cho-tiep-nhan-sua-chua`; không có GET nào tìm hồ sơ mở theo thiết bị | UC-11 chưa trọn vẹn nếu chưa kiểm giải pháp |
| G4 | Outbox khi tiến trình dừng: phục hồi `DANG_GUI` bị bỏ dở, thời hạn giữ quyền xử lý/claim/retry, khả năng gửi lặp; thay đổi schema nếu cần | Khôi; Vương review giao dịch | D03 | Trạng thái `CHO_GUI`, `DANG_GUI`, `DA_GUI`, `LOI`, `LOI_VINH_VIEN`; job `GuiEmail` mỗi phút, tối đa 50 dòng/tick, đặt `DANG_GUI` có điều kiện; thử lại tối đa 5 lần cách 1, 5, 15, 60 phút (KT 10.2, CV 13.2). Trong 10 tài liệu backend không có quy tắc phục hồi `DANG_GUI` bị bỏ dở | Nghiệm thu dừng/khởi động lại và email. Không chặn mẫu email, bộ gửi giả lập |
| G5 | Migration chung: một model/snapshot và một thứ tự migration cho `AppDbContext`; module gửi cấu hình/phần thay đổi riêng; chứng minh tạo mới và nâng cấp | Khôi tích hợp; Vương, Lợi gửi phần module | D02 | KT 16.4 và CV 18.1: không chạy migration lúc khởi động, dùng migration bundle; CV 18.3: mỗi thay đổi lược đồ kèm migration trong cùng PR, không sửa migration đã vào `main`; ĐX-KT-08: một `AppDbContext` | Nghiệm thu DB/migration. Vẫn làm kiểm đơn vị, kiểm qua cổng giả |
| G6 | Lựa chọn FE: rà ĐX-FA/FC/DS/WF thực sự áp dụng (App Router, phiên phía client, bộ sinh mã, SDK/package, 90 giây/8 giây/400 ms, lưu token, công cụ kiểm thử, phiên bản) | Long, Thịnh; Khôi phối hợp API | D02 | Thuộc FE, Khôi chỉ phối hợp phần API | Nghiệm thu nền phiên/CI. Không chặn phác màn bằng dữ liệu thử |
| G7 | Baseline và môi trường: duyệt 13 FR nháp và NFR-07/08/09 theo ĐX-07; giờ quầy thực tế; thông tin truy cập/hạn mức; CORS; giới hạn tần suất auth; phân biệt seed demo | Khôi và toàn nhóm; người quản môi trường cung cấp giá trị | Baseline D02; dev/test D05; giờ quầy trước vận hành | 13 FR nháp: FR-30, 31, 32, 33, 34, 35, 36, 37, 38, 39, 40, 41, 43; NFR-07, 08, 09 cũng "Bản nháp" (SRS). Không đặt giờ mặc định; không coi 10 yêu cầu/phút là đã chốt | Baseline D02; môi trường dev/test D05 |

## 4. Quyết định kỹ thuật do D01-K đặt ra (OPEN)

| Mã | Nội dung | Người quyết định | Hạn đề xuất |
|---|---|---|---|
| K-1 | Bố trí dự án Supabase cho `dev`, `test`, `prod` (gói miễn phí tối đa hai dự án hoạt động) và cách cô lập dữ liệu kiểm thử khi dev và test dùng chung PostgreSQL | Khôi, nhóm | D02 |
| K-2 | Vị trí các cổng dùng chung `ICurrentUser`, `IAuditLogger`, `IAnhStorage` (L7): SharedKernel hay nơi khác; cần nhóm duyệt vì KT 4.4 chưa liệt kê | Khôi, nhóm | D02 (ICurrentUser), D03 (IAuditLogger), D08 (IAnhStorage) |
| K-3 | Tránh chu trình khi MuonTra/SinhVien cần reader nợ của ViPham (L8) | Khôi, Vương | Trước D03 |
| K-4 | Hướng tham chiếu `Infrastructure` ↔ module (nạp cấu hình EF, runner job) | Khôi | D02 |
| K-5 | Dòng Dockerfile `10.0` (KT 16.1, CV 18.2) và cách triển khai Render khi nhóm không muốn đụng Docker | Nhóm | Khi nhóm quyết định |

## 5. Mâu thuẫn giữa các nguồn đã phát hiện

| # | Nội dung | Nguồn A | Nguồn B | Cách xử lý |
|---|---|---|---|---|
| M1 | Phiên bản .NET | KT-13, CV 2.1: .NET 10 (đã sửa); Dockerfile 16.1 và CV 18.2: 10.0 (chưa sửa) | Hai README: .NET 8 (đã sửa); nhóm: .NET 9 | Theo quyết định nhóm (Q2); Dockerfile còn lệch (K-5) |
| M2 | Số UC | README gốc và `backend/docs/README.md`: 19 UC | SRS và `Usecase.md`: 24 UC (17 UC thuộc giai đoạn 1 theo KT 1.2) | Sửa hai README thành 24 là tùy chọn (nhóm E ở file đồng bộ); phạm vi giai đoạn 1 vẫn là 17 |
| M3 | Môi trường dev và kiểm thử | KT 15, 17, CV 16, 17: Docker Compose, Testcontainers (đã sửa) | Nhóm: Supabase, không Docker | Theo Q4, Q5; bố trí dự án còn OPEN (K-1) |
| M4 | Nhánh nền | CV 18.3: tách từ `main` (đã sửa) | Nhóm: nhánh nền `develop` | Theo quyết định nhóm (Q3) |
