# D06-V — Bàn giao đặt trước và theo dõi yêu cầu của sinh viên

Người lập: Nguyễn Đại Vương · Ngày: 18/11/2026 · Trạng thái: đã nối vào Host

## 1. Phạm vi đã làm

- Ba thao tác SV: `POST`, `GET` danh sách, `GET` chi tiết `/api/v1/sv/phieu-yeu-cau-muon` (manifest `DA_CAI`).
- Handler: `GuiYeuCauMuonHandler`, `DsPhieuYeuCauMuonSvHandler`, `ChiTietPhieuYeuCauMuonSvHandler` (`Modules/MuonTra/.../Application`).
- Gửi phiếu tạo `CHO_DUYET`, không giữ chỗ, không gán thiết bị; header và chi tiết cùng một giao dịch.

## 2. Thứ tự kiểm tra khi gửi

Phòng và loại tồn tại (400) → thẻ (422) → trạng thái học (422) → quyền mượn (422) → thời gian tối đa (422) → giới hạn tổng (422) → số lượng khả dụng (409).

## 3. Quyết định đang áp dụng, chờ nhóm xác nhận

| Mã | Nội dung |
|---|---|
| O5 | Giới hạn tổng chỉ cộng số đang cầm, không cộng số đã giữ chỗ chưa giao (đọc nguyên văn BR-24). |
| Hạn thẻ | Thẻ có `han_the` đúng ngày hôm nay (ngày lịch VN) vẫn hợp lệ. |
| Đọc hồ sơ SV | `GuiYeuCauMuonHandler` đọc `sinh_vien.sinh_vien` bằng SQL trực tiếp. Thay bằng bộ đọc của D07-K khi có. |
| Tên phòng/loại | Lấy qua `IDanhMucReader`, không `JOIN` xuyên schema. |

## 4. Việc cho ngày sau

- D07-V (duyệt): khóa tư vấn theo cặp (phòng, loại) rồi tính lại khả dụng; phiếu từ D06 là đầu vào.
- D07-K: cung cấp bộ đọc hồ sơ SV để thay SQL đọc trực tiếp ở mục 3.
- `MuonTraModelBuilderExtensions` đã được nạp vào `Program.cs` (trước đó thiếu, `MuonTraReader` sẽ lỗi trong Host).

## 5. Kiểm thử

- `tests/QuanLyMuonTra.UnitTests/MuonTra/KiemTraDieuKienGuiYeuCauTests.cs` (bao gồm kiểm thử phòng vệ tràn số nguyên)
- `tests/QuanLyMuonTra.IntegrationTests/MuonTra/YeuCauMuonSvTests.cs` (rollback, dùng `D06-V-du-lieu-thu.sql`)
- `tests/QuanLyMuonTra.IntegrationTests/MuonTra/YeuCauMuonSvHttpTests.cs` (bao gồm test phần tử null, định dạng mã, không ghi dữ liệu)
- `tests/QuanLyMuonTra.IntegrationTests/MuonTra/HostMoHinhEfMuonTraTests.cs`

## 6. Cổng thật chưa đạt

- HTTP end-to-end gửi phiếu thành công với tài khoản SV thật chưa được kiểm (sẽ ghi dữ liệu vào DB test).
- Chưa có bộ đọc hồ sơ SV từ D07-K.
- O5 và ranh giới `han_the` chưa được nhóm chốt.
