# D02-V — Bàn giao ánh xạ EF và provider MuonTra cho Host

Người lập: Nguyễn Đại Vương · Trạng thái: bàn giao, chưa tích hợp vào Host

## 1. Điểm gọi từ AppDbContext

- Gọi `modelBuilder.ApplyMuonTraConfiguration()` (namespace `QuanLyMuonTra.Modules.MuonTra.Infrastructure`) trong `OnModelCreating` của `AppDbContext`.
- Hàm này đăng ký 5 bảng schema `muon_tra`: `phieu_yeu_cau_muon`, `chi_tiet_yeu_cau_muon`, `dang_ky_cho`, `phieu_muon`, `chi_tiet_phieu_muon`.
- `muon_tra.phan_hoi_tinh_trang` (UC-21, giai đoạn sau) chưa được ánh xạ.

## 2. Lớp cần đăng ký DI (Host)

| Lớp | Interface | Ghi chú |
|---|---|---|
| `MuonTraReader` | `IMuonTraReader` | Đếm thiết bị đang cầm (`ket_qua_kiem_tra IS NULL`) |
| `ThamChieuMuonTraProvider` | `IThamChieuMuonTraProvider`, `IThamChieuDanhMucProvider`, `IThamChieuKhungPhatProvider` | Hai lớp trên nhận `DbContext`; cách cấp `DbContext` cho module (K-4) do Khôi quyết |

## 3. Việc cần Khôi xử lý

- Concurrency token `xmin` (cột hệ thống) của `phieu_yeu_cau_muon`, `dang_ky_cho`, `phieu_muon` được khai báo trong ánh xạ; model snapshot và migration chung (G5) do Khôi quản lý.
- `NghiepVuException` (maLoi + thông điệp) hiện nằm trong module MuonTra; middleware cần ánh xạ `TRANG_THAI_KHONG_HOP_LE` sang 409 và `DU_LIEU_KHONG_HOP_LE` sang 400 (CV 6.7).
- Khóa `ma_yeu_cau`, `ma_dang_ky`, `ma_phieu` do sequence DB sinh; cách EF nhận giá trị khi thêm mới chưa được kiểm (D06/D08 sẽ kiểm).
- Test tích hợp đọc biến môi trường `ConnectionStrings__Test`; workflow CI chưa truyền biến này nên các test này bị bỏ qua trên CI cho tới khi cấu hình secret.
- `MuonTraTestContext` trong `QuanLyMuonTra.IntegrationTests` là vỏ tạm; đổi sang `AppDbContext` khi có.

## 4. Chưa làm trong D02-V

- Cổng nợ dùng trong kiểm thử: chưa có interface reader nợ (K-3 OPEN).
- Tiêu chí `CoThamChieuPhongAsync` (phòng có yêu cầu hoặc đăng ký) chưa có nguồn xác nhận.
- Kiểm tra `xmin` xung đột đồng thời trên hai giao dịch thật chưa thực hiện.
- DTO `PhieuMuonDto`, `DangKyChoDto`, `GiuChoHetHanEvent` và `IPhieuMuonReader` lệch kiểu so với DDL (xem mục 5 của hướng dẫn D02-V).
