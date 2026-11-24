# D11-V — Bàn giao đọc phiếu, lịch sử và bộ đọc phiếu mượn

Người lập: Nguyễn Đại Vương · Ngày: 24/11/2026 · Giờ thực tế: 3 giờ · Trạng thái: đã nối vào Host

## 1. Phạm vi đã làm

Hoàn thành 5 thao tác OpenAPI và bộ đọc liên module:
- `svDsPhieuMuon`: `GET /api/v1/sv/phieu-muon` (SinhVien) — đọc danh sách phiếu mượn của sinh viên, sắp xếp mới nhất trước, phân trang ổn định.
- `svChiTietPhieuMuon`: `GET /api/v1/sv/phieu-muon/{ma}` (SinhVien) — chi tiết phiếu mượn, kiểm quyền chủ sở hữu (người khác trả 403), ký URL ảnh sau khi kiểm quyền, kèm danh sách biên bản.
- `quayDsPhieuMuon`: `GET /api/v1/quay/phieu-muon` (NhanVien) — đọc danh sách phiếu mượn tại quầy theo phạm vi phòng được cấp quyền, lọc theo mã SV, trạng thái, mã phòng.
- `quayChiTietPhieuMuon`: `GET /api/v1/quay/phieu-muon/{ma}` (NhanVien) — chi tiết phiếu mượn tại quầy, kiểm tra quyền phòng (`PHONG_CHUA_CAP_QUYEN`), ký URL ảnh sau khi kiểm quyền.
- `quayDsPhieuQuaHan30Ngay`: `GET /api/v1/quay/phieu-muon/qua-han-30-ngay` (NhanVien) — danh sách phiếu chưa trả quá 30 ngày lịch VN, đặt trước route `{ma}`.
- Hiện thực `IPhieuMuonReader` (`PhieuMuonReader`) trong `MuonTra.Infrastructure` và đăng ký DI runtime cho module ViPham dùng.

Nhánh thực hiện: `feature/read-borrow-records`
Danh sách commit:
1. `fix(backend): đồng bộ hợp đồng IPhieuMuonReader với DDL phiếu mượn`
2. `feat(backend): bổ sung đọc lịch sử và chi tiết phiếu mượn của sinh viên`
3. `feat(backend): bổ sung đọc phiếu mượn tại quầy theo phạm vi phòng`
4. `feat(backend): bổ sung danh sách phiếu chưa trả quá 30 ngày`
5. `feat(backend): hiện thực IPhieuMuonReader cho module ViPham`
6. `docs(backend): bàn giao D11-V đọc phiếu mượn và IPhieuMuonReader`

## 2. Hợp đồng IPhieuMuonReader

Đã đồng bộ hợp đồng `IPhieuMuonReader` với DDL nguồn:
- `PhieuMuonDto`: `MaPhieu` kiểu `string`, bỏ `MaPhong` (phòng gắn với thiết bị), đổi `MaNvGiao` thành `MaNvLap`.
- `ChiTietPhieuMuonDto`: `MaPhieu` kiểu `string`, `MaChiTiet` giữ `Guid`.
- Các phương thức:
  - `LayPhieuMuonAsync(string maPhieu, CancellationToken ct)`
  - `LayChiTietAsync(Guid maChiTiet, CancellationToken ct)`
  - `LayChiTietTheoPhieuAsync(string maPhieu, CancellationToken ct)`
  - `LayDsQuaHan30NgayAsync(CancellationToken ct)`
- Nhắc **Khôi**: module ViPham dùng `IPhieuMuonReader` từ D14-K và D15-K.

## 3. Bảng quyết định đang áp dụng (chờ nhóm xác nhận)

| Mã | Vấn đề | Đề xuất áp dụng | Lý do / nguồn | Vị trí code |
|---|---|---|---|---|
| **Q-1** | `dsBienBan` của `GET /sv/phieu-muon/{ma}` | SQL chỉ-đọc `vi_pham.bien_ban_vi_pham` + `vi_pham.chi_tiet_bien_ban` trong lớp riêng | Ngoại lệ tạm của CV 4.4; trả dữ liệu thật cho SV đối chiếu (BR-80); chờ Khôi thay bằng reader ViPham ở D14-K | `BienBanPhieuTruyVan.cs` |
| **Q-2** | Lọc danh sách quầy theo phòng thiết bị | SQL chỉ-đọc `EXISTS` trên `danh_muc.thiet_bi` | Ngoại lệ tạm của CV 4.4; tránh nạp toàn bộ vào RAM; chờ Lợi cung cấp reader lọc phòng nếu cần | `PhieuMuonTruyVan.cs` |
| Q-3 | Phiếu có thiết bị nhiều phòng | NV thấy phiếu nếu có ít nhất 1 thiết bị thuộc phòng được cấp; chi tiết trả đủ thiết bị của phiếu | API-Notes ĐX-API-25 | `PhieuMuonTruyVan.cs`, `ChiTietPhieuMuonQuayHandler.cs` |
| Q-4 | Biên bản trạng thái `DA_HUY` | Hiện mọi trạng thái kèm `trangThai` để đối chiếu | BR-80, OpenAPI | `BienBanPhieuTruyVan.cs` |
| Q-5 | Thứ tự lỗi chi tiết | Sai định dạng (400) -> Không tìm thấy (404) -> Quyền (403: `KHONG_DU_QUYEN` hoặc `PHONG_CHUA_CAP_QUYEN`) | Tiền lệ D07/D08 | `ChiTietPhieuMuonSvHandler.cs`, `ChiTietPhieuMuonQuayHandler.cs` |
| Q-6 | Ký ảnh trong phiếu mượn | Danh sách không ký ảnh; chi tiết chỉ ký URL ảnh sau khi kiểm chủ sở hữu / quyền phòng | Kế hoạch D11-V, OpenAPI | `PhieuMuonDoc.cs` |
| Q-7 | Thứ tự danh sách chi tiết | Sắp xếp theo `maThietBi` tăng dần | Thứ tự ổn định | `PhieuMuonDoc.cs`, `PhieuMuonReader.cs` |
| Q-8 | Lọc `maSV` ở danh sách quầy | So khớp chính xác sau `Trim()` | OpenAPI | `DsPhieuMuonQuayHandler.cs` |
| Q-9 | Điều kiện quá hạn 30 ngày | Ngày lịch VN `(hôm nay - hanTra) > 30` và còn thiết bị `ket_qua_kiem_tra IS NULL`; không lọc theo `trang_thai` | BR-61, OpenAPI | `PhieuMuonTruyVan.cs`, `DsPhieuQuaHan30NgayHandler.cs` |
| Q-10 | Chữ ký `IPhieuMuonReader` | Theo DDL: `maPhieu` dạng `string`, bỏ `MaPhong`, `MaNvLap` | Khớp DDL DM-16/17 | `MuonTraDtos.cs`, `IPhieuMuonReader.cs` |

## 4. Kiểm thử và kết quả thật

Môi trường: Windows 10, PowerShell, .NET 9.0, PostgreSQL test (`ConnectionStrings:Test`).

1. **Commit 1:**
   - `dotnet build -c Release`: Succeeded (0 Warning, 0 Error).

2. **Commit 2:**
   - `dotnet format --verify-no-changes`: Passed (0 lỗi).
   - `dotnet build -c Release`: Succeeded (0 Warning, 0 Error).
   - `KiemTraDocPhieuMuonTests`: Passed 16 / Failed 0 / Skipped 0.
   - `DocPhieuMuonSvTests` + `ManifestOperationTests`: Passed 7 / Failed 0 / Skipped 0.
   - Kiểm tra Read-only (không `SaveChanges`/`Execute*`): Passed.

3. **Commit 3:**
   - `dotnet format --verify-no-changes`: Passed (0 lỗi).
   - `dotnet build -c Release`: Succeeded (0 Warning, 0 Error).
   - `DocPhieuMuonQuayTests` (6 ca) + `DocPhieuMuonSvTests` (6 ca) + `ManifestOperationTests` (1 ca): Passed 13 / Failed 0 / Skipped 0.

4. **Commit 4:**
   - `dotnet format --verify-no-changes`: Passed (0 lỗi).
   - `dotnet build -c Release`: Succeeded (0 Warning, 0 Error).
   - `PhieuQuaHan30NgayTests` (4 ca) + `DocPhieuMuonHttpTests` (3 ca) + quầy/SV/manifest: Passed 20 / Failed 0 / Skipped 0.

5. **Commit 5:**
   - `dotnet format --verify-no-changes`: Passed (0 lỗi).
   - `dotnet build -c Release`: Succeeded (0 Warning, 0 Error).
   - `PhieuMuonReaderTests` (4 ca) + `DiRuntimeDocPhieuMuonTests` (1 ca) + `DiRuntimeKhongBanGiaTests` (1 ca): Passed 6 / Failed 0 / Skipped 0.

6. **Nghiệm thu tổng thể toàn giải pháp:**
   - `dotnet format --verify-no-changes`: Passed.
   - `dotnet build -c Release`: Succeeded (0 Warning, 0 Error).
   - `dotnet test tests\QuanLyMuonTra.UnitTests -c Release --no-build`: Passed 515 / Failed 0 / Skipped 0 / Total 515.
   - `dotnet test tests\QuanLyMuonTra.IntegrationTests -c Release --no-build`: Passed 556 / Failed 0 / Skipped 2 / Total 558 (2 test bỏ qua là Supabase Storage thật chưa cấu hình bucket từ baseline D10-V; toàn bộ test mới của D11-V đều Passed).

## 5. Nội dung chưa kiểm chứng hoặc bị chặn

- **Bị chặn:** URL ảnh ký bằng Supabase Storage thật (chưa có cấu hình bucket thật; test dùng `AnhServiceGhiNhan` kiểm chứng đúng thứ tự kiểm quyền trước khi ký URL).
- **Bị chặn / tạm:** `dsBienBan` lấy qua SQL trực tiếp bảng `vi_pham.*` với fixture chèn tay, chưa phải tích hợp module ViPham thật (Khôi làm D14-K).
- **Chưa kiểm chứng:** HTTP 200/201 với tài khoản SV/NV thật và route `/phieu-muon/qua-han-30-ngay` trước `{ma}` ở mức gọi HTTP 200 (mới kiểm chứng HTTP 401/403 qua pipeline).
- **Chưa đo:** Hiệu năng truy vấn danh sách quầy / quá hạn (NFR-03 không yêu cầu đo ở D11-V).

## 6. Bàn giao và việc cho người khác

- **Đinh Nhật Khôi (ViPham):**
  - Đã có `IPhieuMuonReader` thật đăng ký trong DI để dùng ở D14-K và D15-K.
  - Thay thế `BienBanPhieuTruyVan.cs` bằng reader ViPham thật khi hoàn thành D14-K.
  - Mốc kiểm lại tích hợp ViPham: D19-K/D20-V.
- **Lợi (DanhMuc):**
  - Có thể cung cấp thêm reader lọc thiết bị theo phòng nếu muốn xóa bỏ câu lệnh SQL `EXISTS` trong `PhieuMuonTruyVan.cs` (quyết định Q-2).
