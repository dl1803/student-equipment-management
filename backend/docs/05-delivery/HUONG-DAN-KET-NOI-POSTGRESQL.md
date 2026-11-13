# Kết nối PostgreSQL dùng chung cho máy dev

Cập nhật lần đầu: 13/11/2026 · Người lập: Đinh Nhật Khôi

## Phạm vi và giả định

- Nhóm quyết định không dùng PostgreSQL cục bộ và không dùng Docker Desktop. Tài liệu này mô tả cách cấu hình để API trên máy dev trỏ vào PostgreSQL dùng chung.
- PostgreSQL ở đây là PostgreSQL trên Supabase (KT-03; nhóm đã xác nhận).
- Chuỗi kết nối **không** nằm trong Git, trong tài liệu hay trong tin nhắn nhóm công khai.

## Các bước (mỗi thành viên backend)

1. Xin chuỗi kết nối từ người quản môi trường của nhóm (G7). Chuỗi dùng connection pooler, SSL bắt buộc và `Maximum Pool Size` khoảng 10 (KT 15, CV 16).
2. Mở PowerShell tại thư mục gốc repo và chạy lệnh sau, thay phần trong dấu `<>` bằng chuỗi nhận được:

   `dotnet user-secrets set "ConnectionStrings:Default" "<CHUOI_KET_NOI>" --project backend/src/QuanLyMuonTra.Api`

3. Kiểm tra khóa đã có (lệnh in giá trị ra màn hình, chỉ chạy khi không chia sẻ màn hình):

   `dotnet user-secrets list --project backend/src/QuanLyMuonTra.Api`

4. Khóa cấu hình `ConnectionStrings:Default` tương ứng biến môi trường `ConnectionStrings__Default` dùng trên Render (KT 15).

## Điều cấm

- Không đặt chuỗi kết nối vào `appsettings.json`, mã nguồn, `launchSettings.json` hay tài liệu.
- Không dùng cơ sở dữ liệu `dev` dùng chung để chạy thử DDL phá hủy dữ liệu. Môi trường `test` (biến `ConnectionStrings__Test`) có chuỗi kết nối riêng; cách bố trí dự án Supabase và cách cô lập dữ liệu kiểm thử đang OPEN (K-1 trong `C0-decision-register.md`).

## Trạng thái kiểm chứng

- Ngày 1: chưa có EF Core/Npgsql, chưa có `/health/ready` nên **chưa kiểm chứng được kết nối thật**; việc này làm ở D02-K.
- Kiểm thử tích hợp và đồng thời chạy trên PostgreSQL của môi trường `test` (Supabase), không dùng Docker, nên chạy được cả trên máy dev (chuỗi kết nối đặt bằng `dotnet user-secrets` cho project kiểm thử tích hợp, thực hiện ở D02-K) lẫn trên CI (GitHub secret `ConnectionStrings__Test`).

## Lưu ý vận hành

- Supabase gói miễn phí tạm dừng dự án sau 7 ngày không hoạt động (KT 3.4); kiểm tra trước buổi làm việc/bảo vệ.
