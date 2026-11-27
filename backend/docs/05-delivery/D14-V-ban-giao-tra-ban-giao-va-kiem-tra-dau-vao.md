# D14-V — Bàn giao tra bàn giao và kiểm tra đầu vào nhận trả

Người lập: Nguyễn Đại Vương · Ngày: 27/11/2026 · Giờ thực tế: ~1 giờ · Trạng thái: đã nối vào Host, chờ review

## 1. Phạm vi đã làm
- `quayBanGiaoThietBi`: `GET /api/v1/quay/thiet-bi/{maThietBi}/ban-giao` (`NhanVien`) — manifest `DA_CAI` (`POST /api/v1/quay/phieu-muon/{ma}/ket-qua-kiem-tra` giữ nguyên `CHUA_CAI` cho D15-V).
- `QuyTacNhanTra` (`MuonTra.Domain`): kiểm tra đầu vào 4 kết quả (`NGUYEN_VEN`, `HU_HONG`, `THIEU_PHU_KIEN`, `MAT`), ngày giờ trả thực tế của `MAT` là `null`, kiểm tra thiết bị thuộc phiếu đang hiệu lực; bổ sung `MaLoi.ThietBiKhongThuocPhieu` và `MaLoi.ThieuMoTaHoacKhungPhat`.
- `GhiKetQuaKiemTraRequest` + `GhiKetQuaKiemTraValidator` (`Api/MuonTra`): kiểm tra cú pháp/hợp đồng (400), chưa có endpoint và chưa đăng ký `Program.cs` (D15-V làm).
- Dữ liệu thử `D14-V-du-lieu-thu.sql` + `BanGiaoTestSupport` theo lô cho D15-V.
- Nhánh: `feature/return-handover-lookup`; các commit:
  1. `264e426` `feat(backend): bổ sung quy tắc kiểm tra đầu vào kết quả nhận trả`
  2. `17d0b9f` `feat(backend): bổ sung mô hình và bộ kiểm tra yêu cầu ghi kết quả kiểm tra`
  3. `a7e922a` `test(backend): bổ sung dữ liệu thử D14-V theo lô cho nhận trả`
  4. `24602b2` `feat(backend): bổ sung tra bàn giao thiết bị tại quầy khi nhận trả`

## 2. Thứ cho D15-V sử dụng
- `BanGiaoTruyVan.TimChiTietDangHieuLucAsync(db, maThietBi, ct)`: tìm chi tiết đang hiệu lực (`ket_qua_kiem_tra IS NULL`) của thiết bị để lấy `MaChiTiet`, `MaPhieu`, `TinhTrangBanDau`, `MaSv`, `NgayGioMuon`, `NgayGioHenTra`.
- `QuyTacNhanTra.KiemTraDauVao(ketQua, moTa, maKhungPhat)`: ném `NghiepVuException` (`THIEU_MO_TA_HOAC_KHUNG_PHAT`, 422) khi `HU_HONG` hoặc `THIEU_PHU_KIEN` thiếu `moTa` hoặc `maKhungPhat`.
- `QuyTacNhanTra.NgayGioTraThucTe(ketQua, bayGio)`: trả `null` khi `MAT`, trả `bayGio` với 3 kết quả còn lại.
- `QuyTacNhanTra.KiemTraThietBiThuocPhieu(maPhieuDangHieuLuc, maPhieu, maThietBi)`: ném `NghiepVuException` (`THIET_BI_KHONG_THUOC_PHIEU`, 422) khi `maPhieuDangHieuLuc` là `null` hoặc khác `maPhieu`.
- `GhiKetQuaKiemTraRequest` + `GhiKetQuaKiemTraValidator`: sẵn sàng để D15-V đăng ký vào `Program.cs` và gắn `KiemTraDuLieuFilter<GhiKetQuaKiemTraRequest>` trên endpoint `POST /api/v1/quay/phieu-muon/{ma}/ket-qua-kiem-tra`.
- `D14-V-du-lieu-thu.sql` (`BanGiaoTestSupport`): `PM-261126-914001` (4 thiết bị `T1`–`T4` chưa trả cho lô 4 kết quả), `PM-261126-914002` (2 thiết bị ở 2 phòng `P1`/`P2`), `PM-261120-914003` (`DA_TRA`), `D14-TB-T8` (`SAN_SANG`, chưa từng mượn).

## 3. Quyết định đang áp dụng, chờ nhóm xác nhận

| Mã | Vấn đề | Áp dụng mặc định | Trạng thái | Chỗ đổi |
|---|---|---|---|---|
| D14-Q1 | "Phiếu đang hiệu lực" của thiết bị | Chi tiết có `ket_qua_kiem_tra IS NULL` | Chờ nhóm xác nhận | `BanGiaoTruyVan.cs` |
| D14-Q2 | Thứ tự lỗi của GET bàn giao | 400 (mã trống) → 404 (không có thiết bị) → 403 (quyền phòng của thiết bị) → 422 (không thuộc phiếu đang hiệu lực) | Chờ nhóm xác nhận | `BanGiaoThietBiQuayHandler.cs` |
| D14-Q3 | `maKhungPhat` có giá trị với `NGUYEN_VEN`/`MAT` | 400 `DU_LIEU_KHONG_HOP_LE` (validator) | Chờ nhóm xác nhận | `GhiKetQuaKiemTraRequest.cs` |
| D14-Q4 | Thiếu `moTa`/`maKhungPhat` với `HU_HONG`/`THIEU_PHU_KIEN` | 422 `THIEU_MO_TA_HOAC_KHUNG_PHAT` ở Domain (`moTa` khoảng trắng và `Guid.Empty` coi là thiếu) | Chờ nhóm xác nhận | `QuyTacNhanTra.cs` |
| D14-Q5 | `dsKetQua` trùng `maThietBi` | 400 ở validator | Chờ nhóm xác nhận | `GhiKetQuaKiemTraRequest.cs` |
| D14-Q6 | Khung phạt có tồn tại / đang dùng? | Không kiểm ở D14; OPEN cho D15-V | Chờ nhóm xác nhận | D15-V |
| D14-Q7 | "Ảnh tình trạng trước/sau" | GET trả ảnh `BAN_DAU`; gắn ảnh `KHI_TRA` là D15-V; request chỉ kiểm giới hạn `dsMaAnh` ở D14 | Chờ nhóm xác nhận | — |
| D14-Q8 | "Sai chủ sở hữu" | Kiểm bằng hàm thuần `KiemTraThietBiThuocPhieu` (422) cho D15-V và test GET xác nhận mỗi thiết bị trả đúng phiếu/SV | Chờ nhóm xác nhận | `QuyTacNhanTra.cs` |
| D14-Q9 | Mã thiết bị có khoảng trắng đầu/cuối | Không trim (khớp đúng khóa `ma_thiet_bi`); chỉ chặn chuỗi trống | Chờ nhóm xác nhận | `BanGiaoThietBiQuayHandler.cs` |
| D14-Q10 | Đăng ký validator vào `Program.cs` | Để D15-V khi tạo endpoint POST | Chờ nhóm xác nhận | — |

## 4. Kiểm thử và kết quả thật
Môi trường: Windows 10, PowerShell, .NET SDK `9.0.321`, PostgreSQL test (`ConnectionStrings:Test`).

1. **Baseline (Bước 0):**
   - `dotnet build -c Release`: Succeeded (0 Warning, 0 Error).
   - `dotnet test tests\QuanLyMuonTra.UnitTests -c Release --no-build`: Passed 581 / Failed 0 / Skipped 0 / Total 581.
   - `dotnet test tests\QuanLyMuonTra.IntegrationTests -c Release --no-build` (lần đầu): Failed 2 / Passed 681 / Skipped 5 / Total 688 (do 2 bản ghi `TEST-LAP-003` và `TEST-MC-002` trên DB test bị lệch trạng thái sang `SAN_SANG`). Sau khi reset 2 bản ghi về đúng trạng thái trong `C3-du-lieu-thu.sql` (`DANG_DUOC_MUON` và `DA_BAO_MAT`), chạy lại `DanhMucReaderCsdlTests|DsQuanTriCsdlTests`: Passed 14 / Failed 0 / Skipped 0.
2. **Commit 1 (`264e426`):**
   - `dotnet format --verify-no-changes`: Passed.
   - `dotnet build -c Release`: Succeeded (0 Warning, 0 Error).
   - `dotnet test tests\QuanLyMuonTra.UnitTests -c Release --no-build --filter "FullyQualifiedName~QuyTacNhanTraTests|FullyQualifiedName~MaLoiNghiepVuTests"`: Passed 21 / Failed 0 / Skipped 0 / Total 21.
3. **Commit 2 (`17d0b9f`):**
   - `dotnet format --verify-no-changes`: Passed.
   - `dotnet build -c Release`: Succeeded (0 Warning, 0 Error).
   - `dotnet test tests\QuanLyMuonTra.IntegrationTests -c Release --no-build --filter "FullyQualifiedName~GhiKetQuaKiemTraValidatorTests"`: Passed 16 / Failed 0 / Skipped 0 / Total 16.
4. **Commit 3 (`a7e922a`):**
   - `dotnet format --verify-no-changes`: Passed.
   - `dotnet build -c Release`: Succeeded (0 Warning, 0 Error).
   - `dotnet test tests\QuanLyMuonTra.IntegrationTests -c Release --no-build --filter "FullyQualifiedName~DuLieuThuD14Tests"`: Passed 1 / Failed 0 / Skipped 0 / Total 1.
5. **Commit 4 (`24602b2`):**
   - `Select-String` kiểm tra read-only trên `BanGiaoThietBiQuayHandler.cs` và `BanGiaoTruyVan.cs`: không có kết quả khớp.
   - `git diff backend/docs/05-delivery/C0-manifest-operation.csv`: chỉ đổi đúng 1 dòng `GET /api/v1/quay/thiet-bi/{maThietBi}/ban-giao` từ `CHUA_CAI` sang `DA_CAI`.
   - `dotnet format --verify-no-changes`: Passed.
   - `dotnet build -c Release`: Succeeded (0 Warning, 0 Error).
   - `dotnet test tests\QuanLyMuonTra.IntegrationTests -c Release --no-build --filter "FullyQualifiedName~BanGiaoThietBiTests|FullyQualifiedName~BanGiaoThietBiHttpTests|FullyQualifiedName~ManifestOperationTests|FullyQualifiedName~DiRuntime"`: Passed 14 / Failed 0 / Skipped 0 / Total 14.
6. **Nghiệm thu toàn giải pháp:**
   - `dotnet format --verify-no-changes`: Passed.
   - `dotnet build -c Release`: Succeeded (0 Warning, 0 Error).
   - `dotnet test tests\QuanLyMuonTra.UnitTests -c Release --no-build`: Passed 592 / Failed 0 / Skipped 0 / Total 592.
   - `dotnet test tests\QuanLyMuonTra.IntegrationTests -c Release --no-build`: Passed 709 / Failed 0 / Skipped 5 / Total 714 (5 test Skipped giữ nguyên như baseline: 2 `SupabaseAnhStorageThatTests`, 1 `DonAnhMoCoJobTests`, 2 `MigrationBundleTests`).

## 5. Nội dung chưa kiểm chứng hoặc bị chặn
- Ký URL ảnh trên Supabase Storage thật: chưa kiểm (chưa cấu hình bucket), chỉ kiểm bằng fake `AnhServiceGhiNhan`.
- Quyền phòng thật + JWT NV thật cho endpoint `ban-giao`: chưa kiểm (ma trận D21-K).
- Khung phạt không tồn tại / không còn dùng khi ghi kết quả: OPEN cho D15-V (D14-Q6).
- `POST /api/v1/quay/phieu-muon/{ma}/ket-qua-kiem-tra`: chưa làm (D15-V).
- Hiệu năng truy vấn: chưa đo (D25/D26).
