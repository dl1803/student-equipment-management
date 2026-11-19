# D07-V — Bàn giao duyệt và từ chối yêu cầu tại quầy

Người lập: Nguyễn Đại Vương · Ngày: 19/11/2026 · Trạng thái: đã nối vào Host; duyệt/từ chối bị chặn ở runtime chờ C7

## 1. Phạm vi đã làm

- Bốn thao tác Quầy: `GET /api/v1/quay/phieu-yeu-cau-muon`, `GET .../{ma}/kiem-tra`, `POST .../{ma}/duyet`, `POST .../{ma}/tu-choi` (manifest `DA_CAI`).
- Handler: `DsPhieuYeuCauMuonQuayHandler`, `KiemTraPhieuYeuCauMuonHandler`, `DuyetPhieuYeuCauMuonHandler`, `TuChoiPhieuYeuCauMuonHandler` (`Modules/MuonTra/.../Application`).
- Duyệt trong một giao dịch: kiểm tra quyền phòng → khóa dòng SV (`FOR UPDATE`) → khóa tư vấn (phòng, loại) theo thứ tự cố định → đọc lại phiếu → kiểm tra điều kiện và khả dụng → `DA_DUYET` + `han_giu_cho = thời điểm duyệt + ThoiGianGiuChoPhut` → outbox → audit → commit.
- Từ chối: bắt buộc lý do, không kiểm tra điều kiện SV, không ghi vi phạm, không giữ chỗ.

## 2. Thứ tự lỗi khi duyệt

Sai định dạng mã (400) → không tồn tại (404) → phòng chưa cấp quyền (403) → trạng thái không còn chờ duyệt (409) → thẻ → trạng thái học → quyền mượn (422) → thời gian tối đa (422) → giới hạn tổng (422) → số lượng khả dụng (409).

## 3. Quyết định đang áp dụng, chờ nhóm xác nhận

| Mã | Nội dung |
|---|---|
| Audit | Ghi tường minh qua `IAuditLogger`: thao tác `DUYET_YEU_CAU_MUON`, `TU_CHOI_YEU_CAU_MUON`; đối tượng `muon_tra.phieu_yeu_cau_muon`. `PhieuYeuCauMuon` không gắn `IAuditable`. |
| Kiểm tra phiếu không còn chờ duyệt | `kiem-tra` trả 200, `duocDuyet=false`, lý do `TRANG_THAI_KHONG_HOP_LE` (OpenAPI không có 409 cho thao tác này). |
| Dịch lỗi domain | Handler tự dịch `NghiepVuException` thành `LoiNghiepVuException`; Host chưa ánh xạ `NghiepVuException` (D02-V ghi ngược lại). Cần Khôi xác nhận. |
| O5, hạn thẻ, đọc hồ sơ SV | Giữ nguyên quyết định D06-V (xem D06-V mục 3). Dùng `HoSoSinhVienTruyVan`, thay bằng bộ đọc D07-K khi có. |
| Giữ chỗ | Thời lượng đọc từ `ThamSoHeThong.ThoiGianGiuChoPhut`, không gán cứng. |

## 4. Mục chặn (BLOCKED)

- Cổng thông báo C7 (`IThongBaoOutbox`, D09) chưa có. D07-V dùng cổng cục bộ `IThongBaoDuyetOutbox` (`MuonTra.Application`). Runtime đăng ký `ThongBaoDuyetOutboxChuaNoi` (ném `InvalidOperationException`), nên `POST .../duyet` và `POST .../tu-choi` trả 500 và hoàn tác toàn bộ cho đến khi nối.
- D10-V bắt buộc: thay `ThongBaoDuyetOutboxChuaNoi` bằng bộ chuyển đổi trong `MuonTra.Infrastructure` gọi C7 thật; ghi dòng `thong_bao.thong_bao` loại `DUYET_HOAC_TU_CHOI`, kênh `EMAIL` và `UNG_DUNG`, người nhận `ma_sv_nhan`, trong giao dịch hiện tại, khóa trùng lặp đề xuất `DUYET_HOAC_TU_CHOI:{maYeuCau}`; rồi kiểm lại duyệt/từ chối HTTP đầu cuối.

## 5. Kiểm thử

- `tests/QuanLyMuonTra.UnitTests/MuonTra/KiemTraDieuKienDuyetTests.cs`
- `tests/QuanLyMuonTra.IntegrationTests/MuonTra/DuyetTuChoiYeuCauTests.cs` (rollback, `D07-V-du-lieu-thu.sql`; quyền phòng, audit, outbox là bản giả)
- `tests/QuanLyMuonTra.IntegrationTests/MuonTra/DuyetTuChoiYeuCauHttpTests.cs` (401, 403)
- `tests/QuanLyMuonTra.IntegrationTests/MuonTra/TuChoiValidatorTests.cs`

## 6. Cổng thật chưa đạt

- HTTP duyệt/từ chối trả 200 với tài khoản nhân viên thật chưa được kiểm (cần dữ liệu commit và C7).
- Chưa chạy song song thật hai giao dịch commit đồng thời; đã kiểm tuần tự (người thứ hai nhận 409) và kiểm handler lấy khóa tư vấn bằng `lock_timeout`.
- Hoàn tác khi outbox/audit lỗi giữa chừng chưa kiểm được trong khung rollback chung; dựa vào cấu trúc `try/finally` của handler.
- O5 và ranh giới `han_the` chưa được nhóm chốt.
