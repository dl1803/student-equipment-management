# D02-V — Bàn giao ánh xạ EF và provider MuonTra cho Host

Người lập: Nguyễn Đại Vương · Trạng thái: bàn giao, chưa tích hợp vào Host

## 1. Điểm gọi từ AppDbContext

- Gọi `modelBuilder.ApplyMuonTraConfiguration()` (namespace `QuanLyMuonTra.Modules.MuonTra.Infrastructure`) trong `OnModelCreating` của `AppDbContext` (hoặc nạp qua assembly của module).
- Hàm này đăng ký 5 bảng schema `muon_tra`: `phieu_yeu_cau_muon`, `chi_tiet_yeu_cau_muon`, `dang_ky_cho`, `phieu_muon`, `chi_tiet_phieu_muon`.
- `muon_tra.phan_hoi_tinh_trang` (UC-21, giai đoạn sau) chưa được ánh xạ.

## 2. Đăng ký DI (Host)

Có thể gọi trực tiếp extension:
```csharp
services.AddMuonTraModule();
```
(Namespace `QuanLyMuonTra.Modules.MuonTra`). Hàm này đã cấu hình đầy đủ:
- `IMuonTraReader` -> `MuonTraReader` (đếm thiết bị đang cầm `ket_qua_kiem_tra IS NULL` và `TrangThai != DaTra`).
- `ThamChieuMuonTraProvider` cho cả 3 interface: `IThamChieuMuonTraProvider`, `IThamChieuDanhMucProvider`, `IThamChieuKhungPhatProvider`.
- Đã đăng ký alias `services.AddScoped<DbContext>(sp => sp.GetRequiredService<AppDbContext>())` trong `QuanLyMuonTra.Infrastructure.DependencyInjection` để các provider inject `DbContext` chạy mượt mà không lỗi runtime.

## 3. Ngoại lệ nghiệp vụ dùng chung

- `NghiepVuException` đã được chuyển về `QuanLyMuonTra.SharedKernel`.
- Middleware Host bắt `NghiepVuException` và ánh xạ mã lỗi theo CV 6.7:
  - `TRANG_THAI_KHONG_HOP_LE` -> 409 Conflict.
  - `DU_LIEU_KHONG_HOP_LE` -> 400 Bad Request.

## 4. Việc cần Khôi xử lý tiếp

- Concurrency token `xmin` (cột hệ thống) của `phieu_yeu_cau_muon`, `dang_ky_cho`, `phieu_muon` được khai báo trong ánh xạ; model snapshot và migration chung (G5) do Khôi quản lý.
- Khóa `ma_yeu_cau`, `ma_dang_ky`, `ma_phieu` do sequence DB sinh; cách EF nhận giá trị khi thêm mới chưa được kiểm (D06/D08 sẽ kiểm).
- Cấu hình repository secret `ConnectionStrings__Test` trên GitHub Actions để test DB thật chạy tự động trên CI.

## 5. Chưa làm trong D02-V

- Cổng nợ dùng trong kiểm thử: chưa có interface reader nợ (K-3 OPEN).
- Tiêu chí `CoThamChieuPhongAsync` (phòng có yêu cầu hoặc đăng ký) chưa có nguồn xác nhận chính thức từ tài liệu.
- Kiểm tra `xmin` xung đột đồng thời trên hai giao dịch thật chưa thực hiện.
- DTO `PhieuMuonDto`, `DangKyChoDto`, `GiuChoHetHanEvent` và `IPhieuMuonReader` lệch kiểu so với DDL (xem mục 5 của hướng dẫn D02-V).
