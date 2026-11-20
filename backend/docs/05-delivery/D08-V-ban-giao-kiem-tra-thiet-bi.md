# D08-V — Bàn giao kiểm tra thiết bị trước khi giao

Người lập: Nguyễn Đại Vương · Ngày: 20/11/2026 · Trạng thái: phần A đã nối vào Host; phần B (kiểm nguồn) BLOCKED

## 1. Phạm vi đã làm
- `POST /api/v1/quay/phieu-muon/kiem-tra-thiet-bi` (manifest `DA_CAI`); handler `KiemTraThietBiGiaoHandler`, không ghi CSDL.

## 2. Thứ tự lỗi
Quyền phòng (403) → không tồn tại (404) → không sẵn sàng hoặc ngưng sử dụng (422) → sai phòng (422) → sai loại (422).

## 3. Quyết định đang áp dụng, chờ nhóm xác nhận
- A-1 đến A-5 (xem hướng dẫn D08-V). 404 chưa được khai báo trong openapi.yaml cho thao tác này.

## 4. Kiểm thử
- Đơn vị: KiemTraThietBiGiaoHandlerTests (11 ca, kết quả: 11 Passed, 0 Failed, 0 Skipped; tổng UnitTests 449 Passed).
- Tích hợp PostgreSQL: KiemTraThietBiGiaoTests (BLOCKED - K-1/G7 chưa cấu hình chuỗi kết nối test trên máy dev).
- HTTP: KiemTraThietBiGiaoHttpTests (401, 403 - Skipped do chưa cấu hình ConnectionStrings:Test; ManifestOperationTests: Passed).

## 5. Cổng thật chưa đạt
- HTTP 200/422/400 với nhân viên thật chưa kiểm.
- Quyền phòng thật qua HTTP chưa kiểm.
- kiem-tra-nguon: BLOCKED bởi G1, G2, D07-K (xem hướng dẫn mục 6).
