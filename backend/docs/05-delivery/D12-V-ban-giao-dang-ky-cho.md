# D12-V — Bàn giao đăng ký, xem và rời danh sách chờ

Người lập: Nguyễn Đại Vương · Ngày: 25/11/2026 · Giờ thực tế: chưa chốt (người thực hiện cập nhật) · Trạng thái: đã nối vào Host

## 1. Phạm vi đã làm

- Ba thao tác SV (manifest `DA_CAI`): `POST`/`GET` `/api/v1/sv/dang-ky-cho`, `POST /api/v1/sv/dang-ky-cho/{ma}/roi`.
- Handler: `DangKyChoHandler`, `DsDangKyChoSvHandler`, `RoiDangKyChoHandler` (`Modules/MuonTra/.../Application`).
- Nhánh thực hiện: `feature/student-waiting-list`.
- Danh sách commit:
  1. `adff081` — `feat(backend): bổ sung xem danh sách đăng ký chờ của sinh viên`
  2. `432f377` — `feat(backend): bổ sung đăng ký chờ khi loại thiết bị hết khả dụng`
  3. `892a5e7` — `feat(backend): bổ sung rời danh sách chờ và chặn an toàn nhánh giữ chỗ`
  4. `e80f013` — `docs(backend): bàn giao D12-V đăng ký chờ`
  5. Commit sau review PR #64 — `fix(backend): xóa số thứ tự chờ về null trong db khi hủy đăng ký`

## 2. Thứ tự kiểm tra

Đăng ký: phòng/loại tồn tại (400) → quyền mượn bình thường (422 `SV_BI_KHOA_QUYEN_MUON`) → khả dụng bằng 0 (422 `CON_HANG_KHONG_DANG_KY_CHO`).
Rời: định dạng mã (400) → không tìm thấy (404) → chủ sở hữu (403) → trạng thái (409).

## 3. Quyết định đang áp dụng, chờ nhóm xác nhận

| Mã | Vấn đề | Đề xuất áp dụng | Căn cứ |
|---|---|---|---|
| **Q1** | Rời khi `DA_THONG_BAO` nhưng chưa có C9 | `RoiDangKyChoHandler` nhận `IFifoService? capLuot = null` (tham số mặc định, MS.DI dùng `null` khi chưa đăng ký). Khi đăng ký đang `DA_THONG_BAO` mà `capLuot` là `null` → ném `InvalidOperationException` **trước khi ghi bất cứ gì** (HTTP 500 kèm traceId, DB không đổi). Không tạo lớp "…ChuaNoi" vì `DiRuntimeKhongBanGiaTests` cấm kiểu có tên kết thúc `ChuaNoi` trong assembly MuonTra | Kế hoạch: "runtime dùng bộ cấp lượt thật hoặc chặn an toàn, không trả thành công giả" |
| **Q2** | Ý nghĩa `soThuTuCho` | Chỉ đếm các đăng ký `DANG_CHO` của cùng (phòng, loại): đăng ký mới = số `DANG_CHO` hiện có + 1; khi có người rời, đánh số lại liên tục theo `(thoi_diem_dang_ky, ma_dang_ky)`. Phản hồi chỉ trả `soThuTuCho` khi `trangThai = DANG_CHO`, các trạng thái khác trả `null` | OpenAPI: "Số thứ tự chờ; có khi `DANG_CHO`"; DDL `ck_dk_dang_cho_co_so_thu_tu`; dữ liệu thử D03/D05 đều để `NULL` khi `DA_THONG_BAO`. **Liên quan O6 (D13): tie-break khi cùng thời điểm** |
| **Q3** | Cùng SV đăng ký trùng (phòng, loại) 2 lần | **Không chặn** (OpenAPI không có mã lỗi cho việc này; kế hoạch cấm tự thêm UNIQUE) | OpenAPI; kế hoạch D12-V |
| **Q4** | Khóa khi đăng ký | Dù đăng ký không làm giảm khả dụng, vẫn khóa dòng SV rồi khóa tư vấn cặp (phòng, loại) để (a) số thứ tự không trùng khi hai SV đăng ký cùng lúc, (b) đọc khả dụng nhất quán. Dùng đúng thứ tự khóa chung nên không gây deadlock với duyệt/giao | KT 8.2 (thứ tự khóa); BR-34 |
| **Q5** | Điều kiện SV khi đăng ký | **Chỉ** kiểm `trang_thai_quyen_muon = BINH_THUONG`. **Không** dùng lại `KiemTraDieuKienGuiYeuCau.KiemTraSinhVien` (hàm đó còn kiểm hạn thẻ + trạng thái học — ngoài hợp đồng đăng ký chờ) | Kế hoạch D12-V "Kết quả cuối ngày"; OpenAPI chỉ liệt kê 2 mã 422 |
| **Q6** | Kiểm tra phòng/loại tồn tại khi đăng ký | Giống D06-V: phòng hoặc loại không tồn tại, hoặc loại đã ngừng sử dụng → 400 `DU_LIEU_KHONG_HOP_LE` (tránh vi phạm khóa ngoại → 500) | `GuiYeuCauMuonHandler` |
| **Q7** | `GET /sv/dang-ky-cho` trả những trạng thái nào | **Tất cả** đăng ký của SV (API không có tham số lọc trạng thái) | OpenAPI |
| **Q8** | Nhật ký (audit) | Không ghi: đăng ký/rời chờ **không nằm** trong danh sách sự kiện bắt buộc ở CV mục 11.1 và `DangKyCho` không gắn `IAuditable` | CV 11.1 |
| **Q9** | Thứ tự lỗi khi rời | 400 (sai định dạng mã) → 404 → 403 `KHONG_DU_QUYEN` → 409 `TRANG_THAI_KHONG_HOP_LE`/`XUNG_DOT_CAP_NHAT` | Tiền lệ Q-5 của D11-V |
| **Q10** | `Location` của 201 | `/api/v1/sv/dang-ky-cho` (API không có GET theo mã) | OpenAPI |

## 4. Kiểm thử và kết quả thật

Môi trường: Windows, PowerShell, .NET SDK `9.0.321`, PostgreSQL test (`ConnectionStrings:Test`).

1. **Commit 1:**
   - `dotnet format --verify-no-changes`: Passed (exit code 0).
   - `dotnet build -c Release`: Succeeded (0 Warning, 0 Error).
   - `dotnet test tests\QuanLyMuonTra.IntegrationTests -c Release --no-build --filter "FullyQualifiedName~DangKyChoSvTests|FullyQualifiedName~DangKyChoHttpTests|FullyQualifiedName~ManifestOperationTests"`: Passed 10 / Failed 0 / Skipped 0 / Total 10.

2. **Sau hoàn thiện mã nguồn Commit 3 (trước khi commit Commit 3):**
   - `dotnet format --verify-no-changes`: Passed (exit code 0).
   - `dotnet build -c Release`: Succeeded (0 Warning, 0 Error).
   - `dotnet test tests\QuanLyMuonTra.UnitTests -c Release --no-build`: Passed 529 / Failed 0 / Skipped 0 / Total 529 (gồm 14 ca unit mới trong `KiemTraDieuKienDangKyChoTests` và 11 ca sẵn có trong `DangKyChoTests`).
   - `dotnet test tests\QuanLyMuonTra.IntegrationTests -c Release --no-build`: Passed 607 / Failed 0 / Skipped 5 / Total 612 (5 ca bỏ qua đều thuộc phần ngoài D12-V đã có từ baseline: 2 ca `SupabaseAnhStorageThatTests`, 2 ca `MigrationBundleTests`, 1 ca `DonAnhMoCoJobTests.UC12_J9_XoaObjectThatTrenSupabaseStorage`; toàn bộ test của D12-V gồm `DangKyChoSvTests` 19 ca, `DangKyChoHttpTests` 10 ca, `DiRuntimeDangKyChoTests` 1 ca, `ManifestOperationTests` và `DiRuntimeKhongBanGiaTests` đều Passed).

3. **Sau sửa theo review PR #64 (`DangKyCho.Roi()` đặt `SoThuTuCho = null`):**
   - `dotnet format --verify-no-changes`: Passed (exit code 0).
   - `dotnet build -c Release`: Succeeded (0 Warning, 0 Error).
   - `dotnet test tests\QuanLyMuonTra.UnitTests -c Release --no-build`: Passed 529 / Failed 0 / Skipped 0 / Total 529.
   - `dotnet test tests\QuanLyMuonTra.IntegrationTests -c Release --no-build --filter "FullyQualifiedName~DangKyChoSvTests|FullyQualifiedName~DangKyChoHttpTests|FullyQualifiedName~DiRuntimeDangKyChoTests|FullyQualifiedName~ManifestOperationTests|FullyQualifiedName~DiRuntimeKhongBanGiaTests"`: Passed 33 / Failed 0 / Skipped 0 / Total 33.

## 5. Nội dung chưa kiểm chứng hoặc bị chặn

- **Bị chặn đến D13-V:** rời khi `DA_THONG_BAO` chưa có bộ cấp lượt C9 thật; runtime chặn an toàn (HTTP 500, DB không đổi); chỉ kiểm bằng bản giả cô lập.
- **Chưa kiểm:** HTTP end-to-end ghi dữ liệu thật (đăng ký/rời thành công qua token SV thật) — các test HTTP chỉ phủ 401/403/400/404/200 rỗng, không ghi dữ liệu.
- **Chưa kiểm:** đồng thời (hai SV đăng ký cùng lúc; rời song song với giao trực tiếp/giữ chỗ) — thuộc kiểm NFR-06 ở V13 và D30.

## 6. Bàn giao cho D13-V

- Đầu D13: đồng bộ hợp đồng `IFifoService`/`DangKyChoDto` (`MaDangKy` hiện là `Guid`, DDL là `text`).
- Khi đăng ký `IFifoService` thật ở runtime: đổi `Assert.Null` trong `DiRuntimeDangKyChoTests` và kiểm lại nhánh rời `DA_THONG_BAO` bằng PostgreSQL thật + outbox.
- O6 (tie-break cùng `thoi_diem_dang_ky`, nhiều lượt cùng lúc): hiện đánh số lại theo `(thoi_diem_dang_ky, ma_dang_ky)`.
