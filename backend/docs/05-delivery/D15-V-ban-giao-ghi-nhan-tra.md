# D15-V — Bàn giao ghi nhận trả và kết quả từng thiết bị

Người lập: Nguyễn Đại Vương · Ngày: 28/11/2026 · Giờ thực tế: ~1 giờ · Trạng thái: đã nối vào Host, chờ review

## 1. Phạm vi đã làm
- `quayGhiKetQuaKiemTra`: `POST /api/v1/quay/phieu-muon/{ma}/ket-qua-kiem-tra` (`NhanVien`) — manifest `DA_CAI`.
- Domain: `PhieuMuon.GhiKetQuaKiemTra`, `ChiTietPhieuMuon.GhiKetQuaKiemTra`, `QuyTacNhanTra.KhungPhatLuuTru`, `QuyTacNhanTra.MaKetQua`.
- Application: `GhiKetQuaKiemTraHandler`, `GhiDongKetQuaNhanTra`, `KetQuaKiemTraYeuCau`.
- Port MAT: `IPortGhiNhanMat` → `PortGhiNhanMat` (đăng ký Scoped).
- Nhánh: `feature/record-return-inspection-results`; các commit:
  1. `e7afbbc` `feat(muontra): bổ sung ghi kết quả kiểm tra vào miền phiếu mượn`
  2. `23c155e` `feat(muontra): bổ sung bộ xử lý ghi kết quả kiểm tra khi nhận trả`
  3. `b78ce5c` `test(muontra): kiểm chứng cấp lượt, hoàn tác cả lô và từ chối ghi lại kết quả nhận trả`
  4. `9e8928c` `feat(muontra): hiện thực port ghi nhận mất cho nhận trả`
  5. `5c7a1e8` `feat(api): bổ sung endpoint ghi kết quả kiểm tra khi nhận trả`

## 2. Thứ cho K19, D16-V sử dụng
- **Port `IPortGhiNhanMat.GhiNhanMatAsync(Guid maChiTietPhieu, Guid maNvNhanTra, CancellationToken ct)`** (`PortGhiNhanMat`, đăng ký Scoped trong `AddMuonTraModule`):
  - Chạy trong giao dịch của bên gọi (`InvalidOperationException` nếu không có giao dịch).
  - Không tự khóa dòng `sinh_vien`: bên gọi (ViPham) khóa dòng SV trước theo CV 8.2.
  - Trả `true` khi đã ghi `MAT` (`ngay_gio_tra_thuc_te = NULL`, thiết bị `DA_BAO_MAT`, nhật ký `GHI_KET_QUA_KIEM_TRA`, xét đóng phiếu `DA_TRA` nếu mọi chi tiết đã có kết quả); trả `false` khi không có chi tiết mã đó; ném `409 TRANG_THAI_KHONG_HOP_LE` khi chi tiết đã có kết quả hoặc thiết bị không còn `DANG_DUOC_MUON`.
- **Handler `GhiKetQuaKiemTraHandler.XuLyAsync(Guid maNv, string maPhieu, IReadOnlyList<KetQuaKiemTraYeuCau> dsKetQua, CancellationToken ct)`**:
  - Trả `PhieuMuonDayDuDto` dựng trước `COMMIT` qua `PhieuMuonDoc`.
  - Mã lỗi: `400 DU_LIEU_KHONG_HOP_LE`, `403 PHONG_CHUA_CAP_QUYEN`, `404 KHONG_TIM_THAY`, `409 TRANG_THAI_KHONG_HOP_LE` / `XUNG_DOT_CAP_NHAT`, `422 THIET_BI_KHONG_THUOC_PHIEU` / `THIEU_MO_TA_HOAC_KHUNG_PHAT`.
- **Dữ liệu thử D14-V dùng lại** (`D14-V-du-lieu-thu.sql`, `GhiKetQuaKiemTraTestSupport`): `PM-261126-914001` (`T1`–`T4`), `PM-261126-914002` (`T5`, `T6`), `PM-261120-914003` (`T7`, `DA_TRA`), khung phạt `019c0000-0000-7000-8000-0000000d14d1`, NV1 `019c0000-0000-7000-8000-0000000d14a1`.

## 3. Quyết định đang áp dụng, chờ nhóm xác nhận

| Mã | Vấn đề | Mặc định áp dụng | Trạng thái | Chỗ đổi nếu nhóm quyết khác |
|---|---|---|---|---|
| D15-Q1 (đóng D14-Q6) | Khung phạt không tồn tại / đã tắt | `400 DU_LIEU_KHONG_HOP_LE`; khung phải tồn tại và `DangSuDung = true` (`Guid.Empty` vẫn coi là thiếu → 422 theo D14-Q4) | Chờ nhóm xác nhận | `GhiKetQuaKiemTraHandler.KiemTraKhungPhatAsync` |
| D15-Q2 | Thứ tự lỗi của POST | 400 (mã phiếu/validator) → 404 (phiếu) → 403 (NV không có quyền phòng nào của phiếu) → từng thiết bị theo mã tăng dần: 422 không thuộc phiếu → 403 phòng của thiết bị → trong giao dịch: 400 khung phạt → 422/400 từ Domain | Chờ nhóm xác nhận | `GhiKetQuaKiemTraHandler.XuLyAsync` |
| D15-Q3 | Từ chối ghi lại kết quả đã xác nhận | `422 THIET_BI_KHONG_THUOC_PHIEU` (chỉ tìm chi tiết `ket_qua_kiem_tra IS NULL`) | Chờ nhóm xác nhận | `PhieuMuon.GhiKetQuaKiemTra` |
| D15-Q4 | `moTa` với `NGUYEN_VEN`/`MAT` | Lưu `moTa` (đã trim) vào `tinh_trang_khi_tra` nếu có, ngược lại `NULL` | Chờ nhóm xác nhận | `ChiTietPhieuMuon.GhiKetQuaKiemTra` |
| D15-Q5 | Nhật ký | Mỗi dòng kết quả ghi 1 bản ghi `GHI_KET_QUA_KIEM_TRA` (`muon_tra.chi_tiet_phieu_muon`); đổi tình trạng thiết bị do `ThietBiService` ghi `SUA` | Chờ nhóm xác nhận | `GhiDongKetQuaNhanTra` |
| D15-Q6 | Hợp đồng `IPortGhiNhanMat` | `true` = đã ghi MAT; `false` = không có chi tiết; `409 TRANG_THAI_KHONG_HOP_LE` nếu đã có kết quả hoặc thiết bị không còn `DANG_DUOC_MUON`; `InvalidOperationException` nếu không có giao dịch; không tự khóa dòng SV | Chờ nhóm xác nhận | `PortGhiNhanMat` |
| D15-Q7 | Khóa và C9 | `sinh_vien FOR UPDATE` rồi `LockDanhSachPhongLoaiAsync` cho các cặp `(phòng, loại)` của dòng `NGUYEN_VEN`; sau `SaveChanges` gọi `CapLuotChoAsync` một lần cho mỗi thiết bị `NGUYEN_VEN` | Chờ nhóm xác nhận | `GhiKetQuaKiemTraHandler.GhiAsync` |
| D15-Q8 | `QUA_HAN` | D15-V không đổi `DANG_MUON → QUA_HAN` (thuộc D17-V) | Chờ nhóm xác nhận | — |
| D15-Q9 | SV bị khóa | Không kiểm tra quyền mượn của SV (ĐX-PQ-08) | Chờ nhóm xác nhận | — |
| D15-Q10 | Dữ liệu trả về | `PhieuMuonDayDuDto` dựng trước `COMMIT` bằng `PhieuMuonDoc` | Chờ nhóm xác nhận | `GhiKetQuaKiemTraHandler.GhiAsync` |

## 4. Kiểm thử và kết quả thật
Môi trường: Windows 10, PowerShell, .NET SDK `9.0.321`, PostgreSQL test (`ConnectionStrings:Test`).

1. **Baseline (Bước 0):**
   - `dotnet build -c Release`: Succeeded (0 Warning, 0 Error).
   - `dotnet test tests\QuanLyMuonTra.UnitTests -c Release --no-build`: Passed 607 / Failed 0 / Skipped 0 / Total 607.
   - `dotnet test tests\QuanLyMuonTra.IntegrationTests -c Release --no-build`: Passed 779 / Failed 0 / Skipped 5 / Total 784.
2. **Commit 1 (`e7afbbc`):**
   - `dotnet format --verify-no-changes`: Passed.
   - `dotnet build -c Release`: Succeeded (0 Warning, 0 Error).
   - `dotnet test tests\QuanLyMuonTra.UnitTests -c Release --no-build --filter "FullyQualifiedName~GhiKetQuaKiemTraMienTests|FullyQualifiedName~QuyTacNhanTraTests|FullyQualifiedName~PhieuMuonTests"`: Passed 49 / Failed 0 / Skipped 0 / Total 49.
3. **Commit 2 (`23c155e`):**
   - `dotnet format --verify-no-changes`: Passed.
   - `dotnet build -c Release`: Succeeded (0 Warning, 0 Error).
   - `dotnet test tests\QuanLyMuonTra.IntegrationTests -c Release --no-build --filter "FullyQualifiedName~GhiKetQuaKiemTraTests|FullyQualifiedName~DiRuntime|FullyQualifiedName~ManifestOperationTests"`: Passed 15 / Failed 0 / Skipped 0 / Total 15.
4. **Commit 3 (`b78ce5c`):**
   - `dotnet format --verify-no-changes`: Passed.
   - `dotnet build -c Release`: Succeeded (0 Warning, 0 Error).
   - `dotnet test tests\QuanLyMuonTra.IntegrationTests -c Release --no-build --filter "FullyQualifiedName~GhiKetQuaKiemTraGiaoDichTests|FullyQualifiedName~FifoServiceTests|FullyQualifiedName~DanhSachChoQuayThatTests"`: Passed 15 / Failed 0 / Skipped 0 / Total 15.
5. **Commit 4 (`9e8928c`):**
   - `dotnet format --verify-no-changes`: Passed.
   - `dotnet build -c Release`: Succeeded (0 Warning, 0 Error).
   - `dotnet test tests\QuanLyMuonTra.IntegrationTests -c Release --no-build --filter "FullyQualifiedName~PortGhiNhanMatTests|FullyQualifiedName~DiRuntime"`: Passed 9 / Failed 0 / Skipped 0 / Total 9.
   - `dotnet test tests\QuanLyMuonTra.UnitTests -c Release --no-build`: Passed 619 / Failed 0 / Skipped 0 / Total 619.
6. **Commit 5 (`5c7a1e8`):**
   - `git diff backend/docs/05-delivery/C0-manifest-operation.csv`: chỉ đổi đúng 1 dòng `POST /api/v1/quay/phieu-muon/{ma}/ket-qua-kiem-tra` từ `CHUA_CAI` sang `DA_CAI`.
   - `dotnet format --verify-no-changes`: Passed.
   - `dotnet build -c Release`: Succeeded (0 Warning, 0 Error).
   - `dotnet test tests\QuanLyMuonTra.IntegrationTests -c Release --no-build --filter "FullyQualifiedName~GhiKetQuaKiemTraHttpTests|FullyQualifiedName~ManifestOperationTests|FullyQualifiedName~DiRuntime|FullyQualifiedName~GhiKetQuaKiemTraValidatorTests"`: Passed 25 / Failed 0 / Skipped 0 / Total 25.
   - `dotnet test tests\QuanLyMuonTra.UnitTests -c Release --no-build`: Passed 619 / Failed 0 / Skipped 0 / Total 619.
   - `dotnet test tests\QuanLyMuonTra.IntegrationTests -c Release --no-build`: Passed 803 / Failed 0 / Skipped 5 / Total 808 (5 test Skipped giữ nguyên như baseline: 2 `SupabaseAnhStorageThatTests`, 1 `DonAnhMoCoJobTests`, 2 `MigrationBundleTests`).

## 5. Nội dung chưa kiểm chứng hoặc bị chặn
- Ký URL ảnh trên Supabase Storage thật; gắn ảnh `KHI_TRA` với bucket thật: chưa cấu hình/kiểm bucket (chỉ kiểm qua `AnhService` thật với `AnhStorageGia`).
- Email thật cho SV được cấp lượt: chỉ ghi outbox; gửi thật chưa kiểm (D10-K / D20-K).
- `POST` thành công `200` với JWT NV thật + quyền phòng thật: chỉ kiểm 401/403/400/404 qua Host (ma trận quyền D21-K).
- Gọi thật từ ViPham (báo mất quá 30 ngày, K15) qua `IPortGhiNhanMat`: K15/K19 chưa làm (Khôi D15-K / K19).
- Trả từng phần bốn kết quả end-to-end, SV bị khóa vẫn trả, POST lặp qua HTTP, phí theo thời điểm trả: thuộc D16-V.
- `QUA_HAN` (J2) và xử lý thời hạn khi có truy cập: thuộc D17-V (D15-Q8).
- Kiểm đồng thời có barrier (trả + giao trực tiếp, hai POST cùng lúc, đổi phòng/loại lúc trả): chỉ có kiểm tuần tự (D21-V / D22-V).
- Quyết định D15-Q1…Q10 và D14-Q1…Q10: mặc định chưa được nhóm duyệt.
- G2 (lượng giao/giữ chỗ): vẫn OPEN theo kế hoạch.
