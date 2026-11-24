# D12-N — Harness migration, fixture bảo trì và oracle báo cáo

## 1. Môi trường kiểm chứng

- Ngày chạy: 24/11/2026
- .NET SDK: 9.0.321
- CSDL kiểm thử: PostgreSQL trên Supabase cá nhân của Lợi; CSDL tạm tạo bằng `CREATE DATABASE`, xóa sau mỗi ca
- Baseline commit: 9f0deb1

## 2. Cách chạy harness migration

Khóa cấu hình (user-secrets của `QuanLyMuonTra.IntegrationTests` hoặc biến môi trường):

| Khóa | Biến môi trường | Ý nghĩa |
|---|---|---|
| `ConnectionStrings:Test` | `ConnectionStrings__Test` | Máy chủ PostgreSQL dùng để tạo CSDL tạm (đã có) |
| `ConnectionStrings:TestTam` | `ConnectionStrings__TestTam` | Tùy chọn: máy chủ riêng để tạo CSDL tạm; bỏ trống thì dùng `ConnectionStrings:Test` |
| `MigrationBundle:Path` | `MigrationBundle__Path` | Đường dẫn bundle (K12 bàn giao); chưa đặt thì hai ca bundle bị Skipped |
| `MigrationBundle:PhienBanTruoc` | `MigrationBundle__PhienBanTruoc` | Tên migration của phiên bản trước; chưa đặt thì ca nâng cấp bị Skipped |

Ca 1 (CSDL trống): bundle phiên bản mới nhất, kỳ vọng 11 schema, 35 bảng.
Ca 2 (CSDL phiên bản trước): bundle tới phiên bản trước, rồi tới mới nhất; kỳ vọng 11 schema, 35 bảng và dấu vân tay lược đồ (cột, ràng buộc, chỉ mục) giống CSDL trống.

## 3. Kết quả kiểm chứng

| Hạng mục | Lệnh/Bộ test | Kết quả |
|---|---|---|
| Build, format | `dotnet build -c Release`, `dotnet format --verify-no-changes` | Đạt (0 warning, 0 error, format không đổi) |
| Harness CSDL tạm | `CsdlTamThoiTests` | Đạt (2/2 ca Passed) |
| Fixture bảo trì | `BaoTriDuLieuThuTests` | Đạt (4/4 ca Passed) |
| Oracle sáu báo cáo | `OracleBaoCaoTests` | Đạt (4/4 ca Passed) |
| Ca bundle trên CSDL trống | `MigrationBundleTests` | Skipped (Chưa có bundle K12) |
| Ca bundle trên phiên bản trước | `MigrationBundleTests` | Skipped (Chưa có bundle K12 và phiên bản trước) |

## 4. Chưa kiểm chứng / BLOCKED

| Hạng mục | Lý do | Chờ ai |
|---|---|---|
| Hai ca bundle | Chưa có bundle K12 và chưa chốt G5 | Khôi |
| Kiểm mở lại hồ sơ bằng API client mới | G3 còn OPEN, `openapi.yaml` chưa có thao tác tìm hồ sơ mở | Lợi, Khôi, Thịnh |

## 5. OPEN

- G3, G5 (xem `C0-decision-register.md`).
