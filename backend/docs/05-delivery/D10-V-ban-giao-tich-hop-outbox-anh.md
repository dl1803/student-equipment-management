# D10-V — Bàn giao tích hợp ảnh, outbox và hoàn tác khi giao

Người lập: Nguyễn Đại Vương · Ngày: 23/11/2026 · Giờ thực tế: 3 giờ · Trạng thái: đã nối vào Host

## 1. Phạm vi đã làm
- Adapter `ThongBaoDuyetOutbox` (`MuonTra.Infrastructure`) thay `ThongBaoDuyetOutboxChuaNoi`; đã xóa bản chưa nối.
- Cập nhật đăng ký DI runtime trong `DependencyInjection.cs`: `IThongBaoDuyetOutbox` trỏ tới `ThongBaoDuyetOutbox`.
- Bổ sung test đơn vị `ThongBaoDuyetOutboxTests` kiểm tra adapter outbox với giờ Việt Nam và mẫu thông báo.
- Bổ sung test `DiRuntimeKhongBanGiaTests` xác minh toàn bộ các cổng phụ thuộc ở runtime đều dùng bản thật, không còn bản giả hay lớp `*ChuaNoi`.
- Bổ sung môi trường tích hợp `MoiTruongTichHopMuonTra` và bộ test `TichHopThatDuyetGiaoTests` (7 ca test) chạy với PostgreSQL thật, kiểm tra duyệt/từ chối và ba nguồn giao (`PHIEU_YEU_CAU`, `DANG_KY_CHO`, `TRUC_TIEP`) với outbox, ảnh BAN_DAU, phí trễ/hạn lưu lúc giao và nhật ký hệ thống.
- Bổ sung bộ test `HoanTacThatTests` (2 ca test) chạy giao dịch độc lập không bọc bởi giao dịch test, chứng minh hoàn tác toàn bộ khi outbox lỗi và khi giao bị lỗi ở thiết bị thứ hai.
- Bổ sung dữ liệu `lich_su_gia_tri` cho các thiết bị `D07-TB-*` trong `D07-V-du-lieu-thu.sql` để thỏa mãn trigger ràng buộc `trg_tb_co_gia_tri`.

## 2. Quyết định đang áp dụng, chờ nhóm xác nhận
| Mã | Quyết định | Căn cứ |
|---|---|---|
| Q-A | Khóa trùng lặp `DUYET:{maYeuCau}` | C7 của Khôi |
| Q-B | `doi_tuong_loai` = `PHIEU_YEU_CAU` | DatabaseDesign mục 1079; ThongBaoTestSupport |
| Q-C | Giờ trong thông báo: `HH:mm ngày dd/MM/yyyy` giờ Việt Nam | Mẫu thông báo DUYET_HOAC_TU_CHOI |

## 3. Kiểm thử và kết quả thật
- Môi trường: Windows 10, PowerShell, .NET 9.0, PostgreSQL test (`ConnectionStrings:Test`).
- Ngày chạy: 23/11/2026.
- `dotnet format --verify-no-changes`: Passed (0 thay đổi).
- `dotnet build -c Release`: Succeeded (0 Warning, 0 Error).
- Test đơn vị:
  * `QuanLyMuonTra.UnitTests`: Passed 499 / Failed 0 / Skipped 0 / Total 499 (bổ sung test kiểm tra null và ca kiểm thử biên lý do từ chối).
- Test tích hợp:
  * `DiRuntimeKhongBanGiaTests`: Passed 1 / Failed 0 / Skipped 0.
  * `TichHopThatDuyetGiaoTests`: Passed 7 / Failed 0 / Skipped 0.
  * `HoanTacThatTests`: Passed 2 / Failed 0 / Skipped 0.
  * Tổng `QuanLyMuonTra.IntegrationTests` (filter `MuonTra`): Passed 529 / Failed 0 / Skipped 2 / Total 531 (2 test Supabase storage thật bỏ qua do chưa cấu hình bucket).

## 4. Cổng thật chưa đạt
- HTTP 200/201 với NV thật: chưa kiểm (Q-F).
- Tương tranh nhiều connection (cùng thiết bị, duyệt+giao…): chưa kiểm — D21-V/NFR-06 (Q-E).
- Storage thật (Supabase) + URL ký thật: chưa kiểm; mới kiểm `AnhService` thật với `IAnhStorage` giả (`AnhStorageGia`).
- Gửi email thật: thuộc D10-K.
- Còn SQL đọc hồ sơ SV trực tiếp ở gửi yêu cầu/duyệt/kiểm tra (Q-D).
