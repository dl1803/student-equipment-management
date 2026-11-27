# D12-T — Tích hợp web danh mục và duyệt với API thật

Cập nhật: 27/11/2026 · Người lập: Thịnh (FE web)

Nguồn: `BANG_THEO_DOI_40_NGAY.md` D12-T; UF-E2E-01; UI-Screens 6.2–6.3, 7.2–7.4; NFR-01.

## 1. Môi trường

| Thành phần | Giá trị |
|---|---|
| Web | `frontend/web` chạy `next dev` tại `http://localhost:3000` (CORS của API chỉ cho phép địa chỉ này) |
| API | `backend/src/QuanLyMuonTra.Api` chạy cục bộ `http://localhost:5283`, cùng mã `develop` @ `9d3257b` |
| CSDL | Supabase **môi trường test** (project `mpmmhxefphacqccgdgip`). Chuỗi kết nối đặt bằng `dotnet user-secrets`, không nằm trong Git hay tài liệu |
| Tài khoản thử | NV `nvthu01` (cấp Phòng Thiết bị A); SV `24520001` (quyền mượn Bình thường, nhóm DAI_HOC, tối đa 2 thiết bị) |
| Dữ liệu thử | Phòng Thiết bị A (`019c0000-0000-7000-8000-000000000001`): Laptop 2, Máy chiếu 2 khả dụng |

Không dùng mock thay kết quả: mọi dòng "Đạt" dưới đây là phản hồi HTTP thật của API trên CSDL test.

## 2. Kết quả HTTP thật

### 2.1 Phiên và quyền (NFR-01)

| Ca kiểm | Yêu cầu | Kết quả | Đánh giá |
|---|---|---|---|
| API sẵn sàng | `GET /health/ready` | 200 `Healthy` | Đạt |
| Chưa đăng nhập | `GET /quay/phieu-yeu-cau-muon`, `GET /auth/toi` | 401 `CHUA_XAC_THUC` | Đạt |
| CORS | `OPTIONS` từ `http://localhost:3000` | 204, `Access-Control-Allow-Origin: http://localhost:3000`; địa chỉ khác không được cho phép | Đạt |
| Đăng nhập NV trên web | `POST /auth/nhan-vien/dang-nhap` → `GET /auth/toi` | 200; web hiện họ tên và "Phòng Thiết bị A" | Đạt |
| SV gọi API quầy | `GET /quay/phieu-yeu-cau-muon` với token SV | 403 `KHONG_DU_QUYEN` | Đạt |
| NV gọi API sinh viên | `GET /sv/toi` với token NV | 403 `KHONG_DU_QUYEN` | Đạt |
| NV sai phòng (E2) | Phiếu thuộc phòng NV không được cấp | Chưa kiểm: CSDL test chỉ có một phòng | **OPEN** |
| QT không dùng quầy | Token QT gọi `/quay/...` | Chưa kiểm: chưa có tài khoản QT thử trong CSDL test | **OPEN** |

### 2.2 SCR-NV-02, SCR-NV-03 — duyệt và từ chối (UF-NV-01)

Hai phiếu do SV `24520001` gửi qua `POST /sv/phieu-yeu-cau-muon` (201): `YC-261127-000259` (Laptop ×1, 2 giờ) và `YC-261127-000260` (Máy chiếu ×1, 2 giờ).

| Ca kiểm | Yêu cầu | Kết quả | Đánh giá |
|---|---|---|---|
| Danh sách chờ duyệt | `GET /quay/phieu-yeu-cau-muon?trangThai=CHO_DUYET` (có và không có `maPhong`) | 200; web hiện 2 phiếu, cũ nhất trước, đủ cột | Đạt |
| Danh sách rỗng | Cùng yêu cầu khi chưa có phiếu | 200 `tongSo = 0`; web hiện "Không có phiếu yêu cầu chờ duyệt" | Đạt |
| Kiểm tra điều kiện | `GET .../YC-261127-000259/kiem-tra` | 200 `duocDuyet = true`, Laptop yêu cầu 1 / khả dụng 2; web: "Được duyệt", Duyệt và Từ chối bật | Đạt |
| Duyệt | `POST .../YC-261127-000259/duyet` | 200; hạn giữ chỗ do máy chủ trả 27/11/2026 10:02 (duyệt lúc khoảng 09:32); web hiện kết quả, không bị đẩy về danh sách | Đạt |
| Danh sách sau duyệt | Tải lại danh sách | Chỉ còn `YC-261127-000260` | Đạt |
| Từ chối phiếu đủ điều kiện (A2) | `POST .../YC-261127-000260/tu-choi` `{ lyDo }` | 200; lý do lưu đúng; web hiện "Đã từ chối" kèm lý do | Đạt |
| Phiếu đã xử lý (E1) | `POST .../YC-261127-000259/duyet` lần 2; `POST .../YC-261127-000260/tu-choi` lần 2 | 409 `TRANG_THAI_KHONG_HOP_LE` | Đạt |
| Phía SV | `GET /sv/phieu-yeu-cau-muon` | `000259` `DA_DUYET`, `hanGiuCho` khớp; `000260` `TU_CHOI`, `lyDoTuChoi` khớp | Đạt |
| Thông báo SV | `GET /sv/thong-bao` | Có 2 thông báo `DUYET_HOAC_TU_CHOI` đúng nội dung (duyệt kèm giờ giữ chỗ; từ chối kèm lý do) | Đạt |
| 409 `SO_LUONG_KHA_DUNG_THAY_DOI`, 422 điều kiện SV khi Duyệt (E3) | Cần hai NV duyệt tranh nhau hoặc đổi dữ liệu SV giữa chừng | Giữ kiểm bằng fixture (D11-T); chưa dựng được trên dữ liệu thật | **OPEN** |

### 2.3 SCR-QT-02..04 — danh mục

| Ca kiểm | Yêu cầu | Kết quả | Đánh giá |
|---|---|---|---|
| Phòng, loại, thiết bị, nhà cung cấp | `GET /quan-tri/phong-thiet-bi`, `/loai-thiet-bi`, `/thiet-bi` | **404**: API chưa map các endpoint danh mục quản trị (biên bản D11-N ghi `CHUA_CAI`, người phụ trách Khôi) | **Chưa đạt — chờ backend** |
| Web khi API danh mục 404 | Mở SCR-QT-03, QT-04 | Không vỡ trang; nguồn lựa chọn báo "Không tải được danh sách lựa chọn" kèm Thử lại | Đạt (xử lý lỗi) |

## 3. Sai lệch phát hiện và đã sửa

| Phát hiện | Trước | Sau |
|---|---|---|
| `GET .../kiem-tra` cho phiếu **đã xử lý hoặc hết hạn** không trả 404 mà trả 200 `duocDuyet = false`, `dsLyDo = [TRANG_THAI_KHONG_HOP_LE]` (404 chỉ khi mã phiếu không tồn tại) | Web D10-T chỉ coi 404 là "phiếu không còn chờ duyệt"; NV mở phiếu cũ sẽ thấy "Không được duyệt" và vẫn bấm được Từ chối (rồi nhận 409) | `DuyetPhieuYeuCauScreen`: lý do `TRANG_THAI_KHONG_HOP_LE` trong kết quả kiểm tra được xử lý như E1 — báo "Phiếu yêu cầu không còn chờ duyệt…" và quay về danh sách đã tải lại. Có test theo đúng phản hồi thật |

Đề nghị backend (Vương) ghi rõ hành vi này trong mô tả `quayKiemTraPhieuYeuCauMuon` của `openapi.yaml`.

## 4. Việc còn OPEN

| Việc | Cần ai |
|---|---|
| Endpoint danh mục quản trị `/quan-tri/phong-thiet-bi`, `/loai-thiet-bi`, `/thiet-bi`, `/nha-cung-cap` (UC-12/13) | Khôi (D11-N) |
| Phòng thứ hai trong CSDL test để kiểm NV sai phòng (403 `PHONG_CHUA_CAP_QUYEN`) | Lợi / Khôi |
| Tài khoản QT thử trong CSDL test để kiểm QT không dùng được quầy | Lợi / Khôi |
| Chạy lại QT-02..04 với API thật khi có endpoint | Thịnh |
