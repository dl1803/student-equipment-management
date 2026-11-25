# D14-N — Hoàn thiện tiếp nhận sửa chữa

## 1. Môi trường kiểm chứng
- Ngày chạy: 25/11/2026; .NET SDK: 9.0.321; CSDL: PostgreSQL test; baseline commit: 74a8556; giờ thực tế: 2 giờ

## 2. Đã bàn giao
- `LapBaoTriHandler` kiểm phiếu của `SUA_CHUA` qua `IPhieuMuonReader` (D11-V), bỏ SQL đọc `muon_tra`
- Fixture `D14-N-du-lieu-thu-sua-chua.sql`; `TiepNhanSuaChuaCsdlTests` (8 ca); unit test DTO quầy không trả bảo hành/NCC
- Không đổi OpenAPI, không đổi schema, không migration

## 3. Kết quả kiểm chứng
| Hạng mục | Lệnh/Bộ test | Kết quả |
|---|---|---|
| Build, format | `dotnet build -c Release`, `dotnet format --verify-no-changes` | 0 Warning(s), 0 Error(s); format đạt |
| Hồi quy tạo hồ sơ | `LapBaoTriCsdlTests` | Passed: 11, Failed: 0, Skipped: 0, Total: 11 |
| Tiếp nhận sửa chữa | `TiepNhanSuaChuaCsdlTests` | Passed: 8, Failed: 0, Skipped: 0, Total: 8 |
| Unit BaoTri | filter `BaoTri` | Passed: 31, Failed: 0, Skipped: 0, Total: 31 |

## 4. Quyết định áp dụng (chờ nhóm xác nhận)
- Đ1: phiếu đúng nguồn = có chi tiết đúng thiết bị với kết quả HU_HONG/THIEU_PHU_KIEN; không ép phiếu hư mới nhất
- Đ2: `SUA_CHUA` không gửi `maPhieuMuon` vẫn hợp lệ (OpenAPI tùy chọn)
- Đ3: `DsChoTiepNhanSuaChuaHandler` vẫn đọc `muon_tra` bằng SQL vì `IPhieuMuonReader` chưa có phương thức lấy phiếu hư mới nhất theo thiết bị

## 5. Chưa kiểm chứng / BLOCKED
| Hạng mục | Lý do | Chờ ai |
|---|---|---|
| Đọc lại hồ sơ mở sau tải lại/đăng nhập lại (G3) | G3 OPEN, OpenAPI chưa có thao tác đọc hồ sơ mở | Lợi, Khôi, Thịnh |
| Quyền `NhanVien`, 403 theo phòng, lỗi 4xx qua HTTP | Chưa có endpoint BaoTri | Khôi |
| Mô hình EF BaoTri trong Host | `Program.cs` chưa nạp assembly BaoTri | Khôi |
| Mã `SO_LUONG_KHA_DUNG_THAY_DOI` cho `quayLapBaoTri` | OpenAPI chưa khai | Khôi |
| Tương tranh hai NV (NFR-06) | Chưa viết test đa kết nối | D21+ |

## 6. OPEN
- G3; G5; điểm F (D13-N); `PhanTrang` nhân bản (O11)
