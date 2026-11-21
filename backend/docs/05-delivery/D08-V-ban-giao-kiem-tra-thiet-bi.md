# D08-V — Bàn giao kiểm tra thiết bị và nguồn giao

Người lập: Nguyễn Đại Vương · Ngày: 21/11/2026 · Trạng thái: phần A và phần B đã nối vào Host; kiểm tra không ghi CSDL

## 1. Phạm vi đã làm
- `POST /api/v1/quay/phieu-muon/kiem-tra-thiet-bi` (manifest `DA_CAI`); handler `KiemTraThietBiGiaoHandler`, không ghi CSDL.
- `POST /api/v1/quay/phieu-muon/kiem-tra-nguon` (manifest `DA_CAI`); handler `KiemTraNguonGiaoHandler` (3 nguồn: `PHIEU_YEU_CAU`, `DANG_KY_CHO`, `TRUC_TIEP`), không ghi CSDL.

## 2. Thứ tự lỗi
- Kiểm tra thiết bị: Quyền phòng (403) → không tồn tại (404) → không sẵn sàng hoặc ngưng sử dụng (422) → sai phòng (422) → sai loại (422).
- Kiểm tra nguồn:
  - `PHIEU_YEU_CAU`: Định dạng (400) → không tồn tại (404) → phòng chưa cấp quyền (403) → đọc hồ sơ SV (404) → tổng hợp lý do giữ chỗ và điều kiện SV (trả 200).
  - `DANG_KY_CHO`: Định dạng (400) → không tồn tại (404) → phòng chưa cấp quyền (403) → đọc hồ sơ SV (404) → tổng hợp lý do giữ chỗ và điều kiện SV (trả 200).
  - `TRUC_TIEP`: Quyền phòng (403) → phòng không tồn tại (400) → loại không tồn tại/ngưng sử dụng (400) → SV không tồn tại (404) → tổng hợp lý do khả dụng và điều kiện SV (trả 200).

## 3. Quyết định đang áp dụng, chờ nhóm xác nhận (Phần A)
- A-1 đến A-5 (xem hướng dẫn D08-V). Đã bổ sung khai báo 404 trong openapi.yaml cho thao tác này.

## 4. Kiểm thử (Phần A)
- Đơn vị: KiemTraThietBiGiaoHandlerTests (11 ca, kết quả: 11 Passed, 0 Failed, 0 Skipped).
- Tích hợp PostgreSQL: KiemTraThietBiGiaoTests (K-1/G7 chưa cấu hình chuỗi kết nối test trên máy dev).
- HTTP: KiemTraThietBiGiaoHttpTests (401, 403 - Skipped do chưa cấu hình ConnectionStrings:Test trên máy dev; ManifestOperationTests: Passed).

## 5. Cổng thật chưa đạt (Phần A)
- HTTP 200/422/400 với nhân viên thật chưa kiểm chứng trên DB staging.
- Quyền phòng thật qua HTTP chưa kiểm chứng trên DB staging.

## 6. Kiểm tra nguồn giao (Phần B)
- Handler `KiemTraNguonGiaoHandler` (3 nguồn), bộ quy tắc `KiemTraDieuKienGiao` (thu thập trọn vẹn lý do theo BR-38 không short-circuit), hợp đồng `IAvailabilityService.DsViTriSanSangAsync`.
- Hồ sơ sinh viên đọc qua `IHoSoSinhVienReader` (QuanLyMuonTra.Modules.SinhVien.Contracts).
- Endpoint `POST /api/v1/quay/phieu-muon/kiem-tra-nguon`: trạng thái manifest `DA_CAI` (G1 đã được xác nhận theo đề xuất Phương án A).
- Test: đơn vị `KiemTraDieuKienGiaoTests` (16 ca đạt; tổng UnitTests 465 Passed), tích hợp PostgreSQL `ViTriSanSangTests` (3 ca) và `KiemTraNguonGiaoTests` (21 ca) — kết quả: Skipped do chưa cấu hình ConnectionStrings:Test trên máy dev.

## 7. Quyết định đang áp dụng, chờ nhóm xác nhận (Phần B)
- B-1 đến B-8 (xem hướng dẫn Phần B). Đã bổ sung khai báo 404 trong openapi.yaml cho kiem-tra-nguon.

## 8. G1/G2
- G1: đề xuất phương án A (dừng khi SV khác thẻ, đối chiếu ở FE, không đổi API): trạng thái duyệt: Đã chọn phương án A.
- G2: đề xuất phương án A (giao đủ, exact-match); rủi ro còn lại: SV đến mà kho thiếu có thể bị ghi "Không đến nhận" (BR-31): trạng thái duyệt: Đã đề xuất phương án A cho D09-V.

## 9. Cổng thật chưa đạt (Phần B)
- HTTP 200/400/404/422 với nhân viên thật chưa kiểm chứng trên DB staging.
- Đối chiếu SV theo G1 do FE thực hiện: chưa kiểm chứng (chờ FE và D07-K).
