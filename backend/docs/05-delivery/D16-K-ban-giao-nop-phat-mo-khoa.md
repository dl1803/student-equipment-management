# D16-K — Ghi nhận nộp phạt, xét mở khóa và quản trị biên bản

Người lập: Đinh Nhật Khôi · Ngày lập: 28/11/2026 · Trạng thái: bàn giao nội bộ nhóm

## 1. Môi trường kiểm chứng
- Ngày chạy: 2026-11-28; .NET SDK: 10.0.401; CSDL: PostgreSQL (Supabase test instance); baseline commit: 052b9f7

## 2. Đã bàn giao
- `XetMoKhoaQuyenMuon` (quy tắc mở khóa dùng chung), `GiaoDichViPham`
- Handler: `XacNhanNhanDuThietBiHandler`, `GhiNhanDaNopHandler`, `HuyBienBanHandler`, `DieuChinhBienBanHandler`
- Endpoint: POST `.../quay/bien-ban-vi-pham/{ma}/da-nop`; POST `.../quay/sinh-vien/{maSV}/xac-nhan-nhan-du-thiet-bi`; POST `.../quan-tri/bien-ban-vi-pham/{ma}/huy`; PUT `.../quan-tri/bien-ban-vi-pham/{ma}/dieu-chinh`
- Manifest: 54 `DA_CAI` / 34 `CHUA_CAI` (đã xác nhận bằng `Select-String`)
- Quyết định K-66…K-71 trong `C0-decision-register.md`

## 3. Kết quả kiểm chứng
| Hạng mục | Lệnh/Bộ test | Kết quả |
|---|---|---|
| Build, format | `dotnet build -c Release`, `dotnet format --verify-no-changes` | 0 Warning(s), 0 Error(s); format đạt |
| Tích hợp nghiệp vụ | `NopPhatMoKhoaTests` | Passed 25/25 (0 Skipped) |
| Tích hợp HTTP | `ViPhamHttpTests` | Passed 17/17 (0 Skipped) |
| Manifest | `ManifestOperationTests` | Passed 2/2 (0 Skipped) |

## 4. Chưa kiểm chứng / BLOCKED
| Hạng mục | Lý do | Chờ ai |
|---|---|---|
| E2E3: trả thiết bị (API) → biên bản → nộp → mở khóa | `IPortGhiNhanMat`/handler nhận trả chưa có (D15-V/D16-V) | Vương; nghiệm thu D19-K |
| Tương tranh nộp + hủy + lập biên bản cùng SV | Thuộc D18-K | Khôi |
| Trạng thái phiếu `QUA_HAN` do J2 | Test đặt bằng SQL; J2 thuộc D17-V | Vương |

## 5. OPEN
- K-64 (khoản trùng ở `POST` đề xuất) vẫn theo ghi chép D15-K; K-66…K-71 chờ nhóm xác nhận

