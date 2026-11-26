# D13-V — Bàn giao cấp lượt đồng bộ và chuyển lượt thủ công

Người lập: Nguyễn Đại Vương · Ngày: 26/11/2026 · Giờ thực tế: chưa chốt · Trạng thái: đã nối vào Host / chờ review

## 1. Phạm vi đã làm

- Bộ cấp lượt C9: `IFifoService` → `FifoService` (`Modules/MuonTra/Infrastructure`), đăng ký Scoped trong `AddMuonTraModule`.
- `RoiDangKyChoHandler` dùng C9 thật khi rời từ `DA_THONG_BAO` (bỏ nhánh chặn tạm của D12-V Q1).
- Manifest `DA_CAI` thêm: `GET /api/v1/quay/danh-sach-cho`, `POST /api/v1/quay/danh-sach-cho/{ma}/chuyen-luot`.
- Handler: `DsDanhSachChoQuayHandler`, `ChuyenLuotDanhSachChoHandler`.
- Nhánh: `feature/waiting-list-fifo` (tạo từ `feature/student-waiting-list`); commit:
  - `b1ebda7` — `refactor(backend): đồng bộ kiểu mã đăng ký chờ trong hợp đồng MuonTra`
  - `89addae` — `feat(backend): bổ sung bộ cấp lượt FIFO cho danh sách chờ`
  - `d83c0a4` — `feat(backend): nối bộ cấp lượt thật vào nhánh rời danh sách chờ`
  - `41dfc58` — `feat(backend): bổ sung danh sách chờ và chuyển lượt thủ công tại quầy`

## 2. Hợp đồng C9 để N15 và D15-V sử dụng

- Gọi `IFifoService.CapLuotChoAsync(maPhong, maLoai, ct)` **trong giao dịch của thao tác kích hoạt**, **sau khi** thiết bị đã `SAN_SANG` (hoặc giữ chỗ cũ đã nhả) và đã `SaveChanges`; trước đó đã khóa dòng SV (nếu có) rồi khóa cặp.
- Mỗi lần gọi cấp tối đa một giữ chỗ; trả `null` khi không có khả dụng hoặc không có người chờ. Gọi một lần cho mỗi thiết bị vừa trở lại khả dụng.
- Không có giao dịch → `InvalidOperationException` (chưa ghi gì).

## 3. Quyết định đang áp dụng, chờ nhóm xác nhận

| Mã | Vấn đề | Đề xuất áp dụng | Căn cứ | Trạng thái |
|---|---|---|---|---|
| **D13-Q1** (đóng **O6**) | Thứ tự FIFO khi cùng `thoi_diem_dang_ky`; nhiều lượt cùng lúc; `so_thu_tu_cho` | Đầu hàng = nhỏ nhất theo `(thoi_diem_dang_ky, ma_dang_ky)` (trùng cách đánh số lại của D12-V). Nhiều lượt đồng thời được tuần tự hóa bằng khóa tư vấn (phòng, loại). Số thứ tự đánh lại liên tục chỉ cho các dòng `DANG_CHO` | BR-34; OpenAPI (sắp xếp mặc định); D12-V Q2 | Chờ nhóm xác nhận |
| **D13-Q2** | `CapLuotChoAsync` cấp mấy lượt, khi nào | Mỗi lần gọi cấp **tối đa một** giữ chỗ (mỗi đăng ký giữ một thiết bị, ĐX-KT-05), **chỉ khi `khaDung > 0`** và có người `DANG_CHO`; ngược lại trả `null`, không ghi gì. Người gọi phải gọi **sau khi** thiết bị đã `SAN_SANG` hoặc giữ chỗ cũ đã nhả (đã `SaveChanges`) trong cùng giao dịch | KT 8.2; C2-C5-C9 | Chờ nhóm xác nhận |
| **D13-Q3** | `so_thu_tu_cho` khi sang `DA_THONG_BAO` | Đặt về `null` (nhất quán với sửa `Roi()` ở PR #64; dữ liệu thử D12 để `NULL` khi `DA_THONG_BAO`) | D12-V Q2; DDL `ck_dk_dang_cho_co_so_thu_tu` | Chờ nhóm xác nhận |
| **D13-Q4** | Chuyển lượt thủ công chỉ khi `DA_THONG_BAO` | Thêm phương thức Domain `ChuyenLuot()` (`DA_THONG_BAO → DA_HUY`) thay vì dùng `Roi()` (cho phép cả `DANG_CHO`), để quy tắc nằm ở Domain | API-Notes mục 6; CV 5.1 | Chờ nhóm xác nhận |
| **D13-Q5** | Danh sách chờ quầy hiện dòng nào; kiểm hạn giữ chỗ | Hiện `DANG_CHO` và `DA_THONG_BAO`; không hiện `DA_NHAN`/`DA_HUY`/`HET_HAN`. Rời/chuyển lượt **không so `han_giu_cho` với giờ hiện tại** (như D12-V); quá hạn + vi phạm do J4 (D18-V) xử lý | OpenAPI (không có tham số lọc trạng thái); D12-V | Chờ nhóm xác nhận |
| **D13-Q6** | Nhật ký cho cấp lượt/chuyển lượt | **Không ghi** (không nằm trong danh sách bắt buộc CV 11.1; `DangKyCho` không `IAuditable`) — nhất quán D12-V Q8. Có thể bị NFR-02 rà lại ở D22-K | CV 11.1 | Chờ nhóm xác nhận |
| **D13-Q7** | Thông báo khi đến lượt | Dùng mẫu sẵn có `THIET_BI_CHO_SAN_SANG`, kênh EMAIL + UNG_DUNG (outbox tự tạo), `khoa_trung_lap = GIU_CHO:{maDangKy}`, đối tượng `DANG_KY_CHO` | `MauThongBao`; `ThongBaoOutbox`; CV 3.2 | Chờ nhóm xác nhận |
| **D13-Q8** | `GiuChoHetHanEvent.MaDangKy` còn `Guid` | Chưa sửa ở D13-V, chờ xác nhận K-60; bắt buộc xong trước D17-K | D13-K DF-02; K-60 | Chờ nhóm xác nhận |

## 4. Kiểm thử và kết quả thật

Môi trường: Windows, PowerShell, .NET SDK `9.0.321`, PostgreSQL test (`ConnectionStrings:Test`).

- `dotnet build -c Release` (sau Phần 4): `Build succeeded. 0 Warning(s), 0 Error(s)`
- `dotnet format --verify-no-changes` (sau Phần 4): exit 0
- `dotnet test tests\QuanLyMuonTra.UnitTests -c Release --no-build` (sau Phần 4): `Passed! - Failed: 0, Passed: 560, Skipped: 0, Total: 560` (trong đó `DangKyChoTests` 26/26 Passed)
- `dotnet test tests\QuanLyMuonTra.IntegrationTests -c Release --no-build --filter "FullyQualifiedName~FifoServiceTests|FullyQualifiedName~DiRuntimeDangKyChoTests|FullyQualifiedName~DangKyChoSvTests"` (sau Phần 2): `Passed! - Failed: 0, Passed: 25, Skipped: 0, Total: 25`
- `dotnet test tests\QuanLyMuonTra.IntegrationTests -c Release --no-build --filter "FullyQualifiedName~RoiDangKyChoThatTests|FullyQualifiedName~DangKyChoSvTests|FullyQualifiedName~DangKyChoHttpTests|FullyQualifiedName~DiRuntime"` (sau Phần 3): `Passed! - Failed: 0, Passed: 35, Skipped: 0, Total: 35`
- `dotnet test tests\QuanLyMuonTra.IntegrationTests -c Release --no-build --filter "FullyQualifiedName~DanhSachChoQuayThatTests|FullyQualifiedName~DangKyChoHttpTests|FullyQualifiedName~DiRuntime|FullyQualifiedName~ManifestOperationTests"` (sau Phần 4): `Passed! - Failed: 0, Passed: 23, Skipped: 0, Total: 23`
- `dotnet test tests\QuanLyMuonTra.IntegrationTests -c Release --no-build` (toàn bộ sau Phần 4): `Passed! - Failed: 0, Passed: 641, Skipped: 5, Total: 646` (5 ca Skipped thuộc ngoài D13: `DonAnhMoCoJobTests` 1 ca Supabase Storage, `MigrationBundleTests` 2 ca migration bundle, `SupabaseAnhStorageThatTests` 2 ca Supabase Storage)

## 5. Nội dung chưa kiểm chứng hoặc bị chặn

- C9 chưa được gọi từ nhận trả (D15-V) và hoàn thành bảo trì (N15): chưa kiểm luồng liên module.
- Hết hạn giữ chỗ, J4, `GiuChoHetHanEvent` phát ra: D18-V (chưa làm).
- Kiểm đồng thời có barrier (cấp lượt + giao trực tiếp song song, rời song song): D21-V/D22-V (chưa làm); D13 chỉ kiểm tuần tự.
- HTTP với nhân viên thật cho hai endpoint quầy: chưa kiểm (D21-K).
- `GiuChoHetHanEvent.MaDangKy` (K-60): chưa làm, chờ xác nhận K-60, hạn trước D17-K.
