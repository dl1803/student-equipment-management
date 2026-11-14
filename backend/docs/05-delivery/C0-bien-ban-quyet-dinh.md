# C0 — Biên bản quyết định (D02-K)

Ngày họp: ____ · Người ghi: Đinh Nhật Khôi · Người tham dự: ____

## G1 — SV trên thẻ và nguồn

| Mục | Nội dung |
|---|---|
| Câu hỏi | Khi SV trên thẻ khác SV của nguồn giao thì xử lý thế nào? Ghi danh tính cuối, lỗi, cách xóa ngữ cảnh, dữ liệu gửi khi lập phiếu |
| Nguồn/mục | Kế hoạch G1; API-Notes (`GET /quay/sinh-vien/the/{maThe}`); ĐX-API-08; sổ C0 dòng G1 |
| Phương án | Dừng · Quét lại · Đổi nguồn · Phương án khác do nhóm duyệt |
| Người quyết định | Khôi cùng Vương, Thịnh; nhóm quyết định nghiệp vụ |
| Ngày/trạng thái | OPEN |
| Quyết định | (điền sau họp) |
| Thay đổi API/DB/UI | (điền sau họp) |
| Dữ liệu thử và kết quả mong đợi | (điền sau họp) |
| Công việc bị ảnh hưởng | Kiểm nguồn/lập phiếu giao thật và UAT tương ứng (D08-V, D09-V, D14-T, D16-T). Không chặn danh mục hoặc phần đọc Android |
| Kiểm thử | (điền sau họp) |

## G2 — Lượng giao và giữ chỗ

| Mục | Nội dung |
|---|---|
| Câu hỏi | Mỗi loại phải giao đủ hay cho phép giao một phần? Xử lý thiếu/thừa/sai loại/trùng mã; tiêu thụ giữ chỗ, lượng dư, trạng thái YC/DK, giao lại |
| Nguồn/mục | Kế hoạch G2; sổ C0 dòng G2 |
| Phương án | Giao đủ (exact-match) · Cho phép một phần (partial) · Khác — kèm bảng đầu vào → kết quả |
| Người quyết định | Vương chủ trì nghiệp vụ, Khôi ghi quyết định |
| Ngày/trạng thái | OPEN |
| Quyết định / Thay đổi / Dữ liệu thử / Kiểm thử | (điền sau họp) |
| Công việc bị ảnh hưởng | Giao dịch giao thật (D09-V, D15-T). Vẫn làm nhập mã, ảnh và nhánh độc lập |

## G3 — Tìm lại hồ sơ bảo trì mở sau khi tải lại

| Mục | Nội dung |
|---|---|
| Câu hỏi | Cách tìm lại hồ sơ mở/`maBaoTri` sau tải lại. Nếu cần GET mới: đường dẫn, DTO, quyền, lỗi |
| Dữ kiện đã xác minh | `openapi.yaml` hiện có `POST /quay/bao-tri`, `POST /quay/bao-tri/{ma}/hoan-thanh`, `GET /quay/bao-tri/den-han`, `GET /quay/bao-tri/cho-tiep-nhan-sua-chua`. `maBaoTri` chỉ xuất hiện trong phản hồi `POST /quay/bao-tri` (schema `BaoTriSuaChua`) và trong `ThietBiChoQuyetDinhThanhLy` (sau hoàn thành). Danh sách `cho-tiep-nhan-sua-chua` là thiết bị **chưa có** bản ghi mở. Chưa có GET nào trả bản ghi mở của một thiết bị |
| Phương án | Thêm GET mới (nhóm chốt đường dẫn/DTO/quyền/lỗi) · Giải pháp khác không cần API mới và không lưu bền dữ liệu nghiệp vụ ở client (trái FA) |
| Người quyết định | Lợi, Khôi, Thịnh |
| Ngày/trạng thái | OPEN |
| Quyết định / Thay đổi / Dữ liệu thử / Kiểm thử | (điền sau họp) |
| Công việc bị ảnh hưởng | D13-N, D14-N, D15-N, D24-N, D24-T. UC-11 chưa trọn vẹn khi chưa có giải pháp được kiểm |

## G5 — Migration chung

| Mục | Nội dung |
|---|---|
| Câu hỏi | Một model/snapshot và một thứ tự migration cho `AppDbContext`; module gửi cấu hình/phần thay đổi riêng; chứng minh tạo mới và nâng cấp |
| Nguồn/mục | Kế hoạch G5; KT 16.4, CV 18.1 (không chạy migration lúc khởi động, dùng bundle); CV 18.3 (mỗi thay đổi lược đồ kèm migration cùng PR, không sửa migration đã vào `develop`/`main`); ĐX-KT-08 (một `AppDbContext`) |
| Điểm tài liệu chưa nhất quán | KT 5.7 và CV 7.4: "mỗi module có thư mục migration riêng, cùng áp dụng vào một CSDL" ≠ yêu cầu một model/snapshot/thứ tự của G5 |
| Phương án | Nhóm chọn cách nhất quán hai điều trên, rồi sửa đồng bộ KT 5.7/CV 7.4 hoặc G5 |
| Người quyết định | Khôi tích hợp; Vương, Lợi gửi phần module |
| Ngày/trạng thái | OPEN |
| Quyết định / Thay đổi / Dữ liệu thử / Kiểm thử | (điền sau họp) |
| Công việc bị ảnh hưởng | Nghiệm thu DB/migration (D12-N, D27-K). Vẫn làm kiểm đơn vị hoặc qua cổng giả |

## G7 — Baseline và môi trường

| Mục | Nội dung |
|---|---|
| Câu hỏi | Duyệt 13 FR nháp (FR-30…FR-41, FR-43 theo sổ C0) và NFR-07/08/09 theo ĐX-07 (SRS); giờ quầy thực tế; thông tin truy cập/hạn mức; CORS; giới hạn tần suất auth; phân biệt seed demo |
| Nguồn/mục | Kế hoạch G7; SRS ĐX-07; sổ C0 dòng G7 |
| Người quyết định | Khôi và toàn nhóm; người quản môi trường cung cấp giá trị |
| Hạn | Baseline D02; dev/test D05; giờ quầy trước vận hành |
| Ngày/trạng thái | OPEN (từng mục con ghi trạng thái riêng) |
| Giá trị cung cấp (chỉ điền khi **có người thật sự cung cấp**) | Giờ quầy: ____ · Truy cập/hạn mức: ____ · CORS: ____ · Giới hạn tần suất auth: ____ · Môi trường test (K-1): ____ |
| Lưu ý | Không đặt giờ quầy mặc định; không coi "10 yêu cầu/phút" là đã chốt |
| Công việc bị ảnh hưởng | Nghiệm thu NFR; giờ quầy chặn lập phiếu giao khi chưa cấu hình |

