# D09-V — Bàn giao lập phiếu giao nguyên tử

Người lập: Nguyễn Đại Vương · Ngày: 21/11/2026 · Giờ thực tế: 4 giờ · Trạng thái: đã nối vào Host

## 1. Phạm vi đã làm
- `POST /api/v1/quay/phieu-muon` (manifest `DA_CAI`); handler `GiaoThietBiHandler` (ba nguồn `PHIEU_YEU_CAU`, `DANG_KY_CHO`, `TRUC_TIEP`).
- Một giao dịch: khóa dòng SV → khóa cặp (phòng, loại) → kiểm tra nguồn/SV/hạn → cập nhật có điều kiện từng thiết bị → phiếu + chi tiết (phí lưu lúc giao) → nguồn `DA_NHAN` → nhật ký.

## 2. Thứ tự lỗi
1. Danh sách thiết bị trùng mã (400) → nguồn không tồn tại (404) → phòng chưa cấp quyền (403)
2. (trong giao dịch, sau khóa) trạng thái nguồn / giữ chỗ hết hạn (409) → thẻ → học → quyền mượn → vượt giới hạn tổng (422)
3. Chưa cấu hình giờ quầy (409) → giao từ giờ đóng cửa (422)
4. Chỉ trực tiếp: số lượng khả dụng (409)
5. Từng thiết bị theo thứ tự mã: không tồn tại (404) → không sẵn sàng/ngưng (422) → sai phòng (422) → sai loại (422)
6. Thiếu/thừa theo loại (400) → cập nhật có điều kiện, 0 dòng (409 `THIET_BI_DA_DUOC_GIAO`)

## 3. Quyết định đang áp dụng, chờ nhóm xác nhận
| Mã | Quyết định | Căn cứ |
|---|---|---|
| G1-A | BE không nhận thông tin thẻ khi lập phiếu. SV lấy từ nguồn (YC/DK) hoặc `maSV` (trực tiếp). FE đối chiếu thẻ | Bạn chốt; D08-V mục 8 |
| G2-A | Giao đủ, exact-match: số thiết bị mỗi loại phải bằng số lượng nguồn. Không giao một phần; mỗi nguồn tối đa một phiếu (giữ nguyên DDL) | Bạn chốt |
| Q1 | Thiếu/thừa so với nguồn, mã thiết bị trùng trong danh sách → 400 `DU_LIEU_KHONG_HOP_LE` (KT 6.6 không có mã riêng; không thêm mã mới) | API-Notes bảng 32 mã lỗi |
| Q2 | Thiết bị thuộc loại không có trong nguồn → 422 `THIET_BI_SAI_LOAI` | BR-40 |
| Q3 | Không tìm thấy nguồn/thiết bị/SV → 404 `KHONG_TIM_THAY`, giống D07/D08. OpenAPI của `quayLapPhieuMuon` khai báo 404 ở Commit 4 | Tiền lệ D07/D08-V |
| Q4 | Handler giao không gọi outbox: KT 10.1 không có loại thông báo nào cho việc giao | KT 10.1 |
| Q5 | `thoiGianMuon`: YC = `min(giờ dự kiến, tối đa nhóm theo loại)` (cắt, không báo lỗi — danh sách 422 của `quayLapPhieuMuon` không có `VUOT_THOI_GIAN_MUON_TOI_DA`); DK/trực tiếp = tối đa nhóm theo loại | BR-42; OpenAPI |
| Q6 | Mã ảnh gắn bằng `IAnhService.GanAsync(..., "CHI_TIET_PHIEU_MUON", maChiTiet.ToString(), "BAN_DAU")`; `maChiTiet` là `Guid.CreateVersion7()`. Dạng chuỗi "D" chữ thường | Comment schema `ma_chi_tiet`; `AnhService.CauHinhGan` |
| Q7 | Mã `PM-…` do DB sinh: dùng `INSERT … RETURNING ma_phieu` (giống `GuiYeuCauMuonHandler`) | D02-V mục 4; mẫu D06-V |
| Q8 | Nhật ký ghi tường minh: `LAP_PHIEU_MUON` (có `doi_tuong_id` = mã phiếu) và `NHAN_YEU_CAU_MUON`/`NHAN_DANG_KY_CHO` cho nguồn. Đổi tình trạng thiết bị đã được `ThietBiService` ghi | KT 12.1 |

## 4. Kiểm thử và kết quả thật
- Đơn vị `KiemTraGiaoThietBiTests`: Passed 14 / Failed 0 / Skipped 0 (tổng UnitTests: 481 Passed / 0 Failed).
- Tích hợp PostgreSQL `GiaoThietBiTests`: Passed 18 / Failed 0 / Skipped 0 (môi trường: Supabase test, ngày 21/11/2026; tổng IntegrationTests: 465 Passed / 0 Failed / 2 Skipped do bucket storage).
- `LapPhieuMuonValidatorTests`: Passed 8. `GiaoThietBiHttpTests` (401, 403): Passed 2 / Skipped 0.
- `ManifestOperationTests`: Passed (xác nhận `POST /api/v1/quay/phieu-muon` khớp manifest `DA_CAI`).

## 5. Cổng thật chưa đạt
- HTTP 201 với nhân viên thật và dữ liệu commit: chưa kiểm.
- Hoàn tác khi lỗi giữa chừng trong giao dịch riêng của handler và tương tranh (cùng thiết bị, duyệt + giao…): chưa kiểm (cần dữ liệu commit) — D10-V/NFR-06.
- Ảnh: mới kiểm bằng `IAnhService` giả; `AnhService` thật + URL ký: D10-V.
- Đối chiếu SV theo G1 do FE thực hiện: chưa kiểm chứng.
- Bản ghi nhật ký thật (`nhat_ky_he_thong`) chưa kiểm vì test dùng `IAuditLogger` giả.
