# D13-N — Đọc thiết bị đến hạn và tạo hồ sơ bảo trì

## 1. Môi trường kiểm chứng
- Ngày chạy: 24/11/2026; .NET SDK: 9.0.321; CSDL: PostgreSQL trên Supabase cá nhân của Lợi; baseline commit: 6203db2

## 2. Đã bàn giao
- Miền + ánh xạ EF `bao_tri.bao_tri_sua_chua` (`ApplyBaoTriConfigurations`), không migration, không đổi snapshot
- `DsThietBiDenHanBaoTriHandler`, `DsChoTiepNhanSuaChuaHandler`, `LapBaoTriHandler` (tầng ứng dụng, chưa có endpoint)
- Hàm DI: `AddBaoTriModule()` đăng ký thêm 3 handler

## 3. Kết quả kiểm chứng
| Hạng mục | Lệnh/Bộ test | Kết quả |
|---|---|---|
| Build, format | `dotnet build -c Release`, `dotnet format --verify-no-changes` | Đạt (0 error, 0 warning) |
| UnitTests BaoTri | `UnitTests` filter `BaoTri` | Đạt 28/28 Passed (Failed: 0, Skipped: 0) |
| Tích hợp danh sách | `BaoTriDsCsdlTests` | Đạt 7/7 Passed (Failed: 0, Skipped: 0) |
| Tích hợp tạo hồ sơ | `LapBaoTriCsdlTests` | Đạt 12/12 Passed (Failed: 0, Skipped: 0) |

## 4. Chưa kiểm chứng / BLOCKED
| Hạng mục | Lý do | Chờ ai |
|---|---|---|
| Đọc lại hồ sơ mở sau tải lại (G3) | G3 OPEN, OpenAPI chưa có thao tác | Lợi, Khôi, Thịnh |
| Quyền `NhanVien` qua HTTP, 403 theo phòng qua HTTP | Chưa có endpoint | Khôi |
| Mô hình EF BaoTri trong Host | `Program.cs` chưa nạp assembly BaoTri | Khôi |
| Mã `SO_LUONG_KHA_DUNG_THAY_DOI` cho `quayLapBaoTri` | OpenAPI chưa khai | Khôi |
| Liên kết phiếu qua `IPhieuMuonReader` | D11-V chưa có bản hiện thực; đang dùng SQL thuần đọc `muon_tra` | Vương |
| Hai ca `MigrationBundleTests` | Chưa có bundle K12 (K-52) | Khôi |
| Tương tranh hai NV đồng thời (NFR-06) | Chưa viết test đa kết nối | D21+ |

## 5. OPEN
- G3, G5; điểm F (thiết bị chờ tiếp nhận sau định kỳ `DE_XUAT_THANH_LY`); `PhanTrang` nhân bản (O11)
