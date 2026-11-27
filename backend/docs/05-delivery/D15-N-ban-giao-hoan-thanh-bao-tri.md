# D15-N — Hoàn thành bảo trì và cấp lượt chờ trong cùng giao dịch

## 1. Môi trường kiểm chứng
- Ngày chạy: 27/11/2026; .NET SDK: 9.0.321; CSDL: PostgreSQL test (Supabase); baseline commit: `9d3257b`

## 2. Đã bàn giao
- `HoanThanhBaoTriHandler`, `HoanThanhBaoTriRequest` (tầng ứng dụng, chưa có endpoint)
- `BaoTriSuaChua.HoanThanh`, `KiemTraKetQua`; `KetQuaBaoTriMa`
- Fixture `D15-N-du-lieu-thu-hoan-thanh.sql`; `HoanThanhBaoTriCsdlTests` (8 ca); `HoanThanhBaoTriThatTests` (3 ca); unit test miền (12 ca mới)
- Không đổi OpenAPI, schema, migration

## 3. Kết quả kiểm chứng
| Hạng mục | Lệnh/Bộ test | Kết quả |
|---|---|---|
| Build, format | `dotnet build -c Release`, `dotnet format --verify-no-changes` | `0 Warning(s)`, `0 Error(s)`; format đạt |
| Unit BaoTri | `dotnet test tests/QuanLyMuonTra.UnitTests --filter "FullyQualifiedName~BaoTri"` | `Passed: 43, Failed: 0, Skipped: 0` |
| Tích hợp hoàn thành | `HoanThanhBaoTriCsdlTests` | `Passed: 8, Failed: 0, Skipped: 0` |
| Rollback/song song | `HoanThanhBaoTriThatTests` | `Passed: 3, Failed: 0, Skipped: 0` |
| Hồi quy D12-N–D15-N | `dotnet test tests/QuanLyMuonTra.IntegrationTests --filter "FullyQualifiedName~QuanLyMuonTra.IntegrationTests.BaoTri"` | `Passed: 41, Failed: 0, Skipped: 0` |
| Hồi quy C9 | `dotnet test tests/QuanLyMuonTra.IntegrationTests --filter "FullyQualifiedName~FifoServiceTests"` | `Passed: 5, Failed: 0, Skipped: 0` |

## 4. Quyết định áp dụng (chờ nhóm xác nhận)
- Đ1–Đ8 (xem tài liệu hướng dẫn D15-N mục 5.1)

## 5. Chưa kiểm chứng / BLOCKED
| Hạng mục | Lý do | Chờ ai |
|---|---|---|
| Đọc lại hồ sơ mở sau tải lại bằng API thật (G3) | G3 OPEN, OpenAPI chưa có thao tác đọc hồ sơ mở | Lợi, Khôi, Thịnh |
| `POST /quay/bao-tri/{ma}/hoan-thanh` qua HTTP; 400/401/403/404/409 qua HTTP | Chưa có endpoint BaoTri | Khôi |
| Mô hình EF BaoTri trong Host | `Program.cs` chưa nạp assembly BaoTri | Khôi |
| Gửi email thật cho SV được cấp lượt | Chưa kiểm gửi thật (D10-K/D20-K) | Khôi |
| Test barrier đa kết nối đúng nghĩa NFR-06 | Chưa viết | D21+ |

## 6. OPEN
- G3, G5; điểm F (thiết bị chờ tiếp nhận sau định kỳ `DE_XUAT_THANH_LY`)
