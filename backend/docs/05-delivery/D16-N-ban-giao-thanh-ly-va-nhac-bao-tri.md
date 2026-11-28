# D16-N — Lập quyết định thanh lý và chạy nhắc bảo trì J6

## 1. Môi trường kiểm chứng
- Ngày chạy: 28/11/2026; .NET SDK: 9.0.321; CSDL: PostgreSQL test (Supabase); baseline commit: `052b9f7`; giờ thực tế: <bạn điền số giờ thực tế>

## 2. Đã bàn giao
- `DsChoQuyetDinhThanhLyHandler`, `LapQuyetDinhThanhLyHandler`, `LapQuyetDinhThanhLyRequest`, DTO thanh lý (tầng ứng dụng, chưa có endpoint)
- `NhacBaoTriJob` (J6, 1 giờ) đăng ký trong `AddBaoTriModule()`
- Fixture `D16-N-du-lieu-thu.sql`; test danh sách, lập quyết định (Csdl + That), J6 (Csdl + That), unit
- Không đổi OpenAPI, schema, migration

## 3. Kết quả kiểm chứng
| Hạng mục | Lệnh/Bộ test | Kết quả |
|---|---|---|
| Build, format | `dotnet build -c Release`, `dotnet format --verify-no-changes` | `0 Warning(s)`, `0 Error(s)`; format đạt |
| Unit | `ThanhLyUngDungTests` (trong tổng `UnitTests`: `Passed: 607, Failed: 0, Skipped: 0`) | `Passed: 7, Failed: 0, Skipped: 0` |
| Danh sách chờ | `DsChoQuyetDinhThanhLyCsdlTests` | `Passed: 2, Failed: 0, Skipped: 0` |
| Lập quyết định | `LapQuyetDinhThanhLyCsdlTests`, `LapQuyetDinhThanhLyThatTests` | `Passed: 10, Failed: 0, Skipped: 0` |
| J6 | `NhacBaoTriJobTests`, `NhacBaoTriJobCsdlTests`, `NhacBaoTriJobThatTests` | `Passed: 7, Failed: 0, Skipped: 0` |
| Hồi quy | `BaoTri` (`60`), `FifoServiceTests` (`5`), `DanhMuc` (`68`), `Scheduler` (`7`) | Đều `Failed: 0, Skipped: 0` |

## 4. Quyết định áp dụng (chờ nhóm xác nhận)
- Đ1–Đ7 (xem tài liệu hướng dẫn D16-N mục 5)

## 5. Chưa kiểm chứng / BLOCKED
| Hạng mục | Lý do | Chờ ai |
|---|---|---|
| `GET /quan-tri/thanh-ly/cho-quyet-dinh`, `POST /quan-tri/quyet-dinh-thanh-ly` qua HTTP; 400/401/403/409 qua HTTP; policy `QuanTri` | Chưa có endpoint | Khôi |
| Mã `SO_LUONG_KHA_DUNG_THAY_DOI` cho `qtLapQuyetDinhThanhLy` | OpenAPI chưa khai | Khôi |
| Khóa chống trùng J6 gồm `maNv` (K-30b) và tiền tố | Chờ xác nhận | Khôi, Lợi |
| J6 trên host chạy `Scheduler:Bat=true` | Chưa bật/chưa kiểm | Khôi |
| Email thật cho thông báo `NHAC_BAO_TRI` | Gate email thật D20-K | Khôi |
| Mô hình EF BaoTri trong Host | `Program.cs` chưa nạp assembly BaoTri | Khôi |
| Tương tranh nhiều kết nối đúng nghĩa NFR-06 (ma trận đầy đủ) | Chưa viết | D21+ |

## 6. OPEN
- Hồ sơ bảo trì còn mở của thiết bị bị thanh lý; khoảng giá trị `ngayThanhLy`; J6 không giới hạn lô; điểm F; G3; G5
