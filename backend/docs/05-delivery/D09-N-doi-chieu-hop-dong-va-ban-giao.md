# D09-N — Đối chiếu hợp đồng danh mục/cấu hình và bàn giao ánh xạ dữ liệu

Người lập: Trần Lê Đức Lợi (Backend, ký hiệu N) · Ngày lập: 21/11/2026 · Môi trường: Windows 11, PowerShell, .NET SDK 9.0.321

---

## 1. Phạm vi và trạng thái

- **Căn cứ kế hoạch**: Task D09-N (`KE_HOACH_CHI_TIET_40_NGAY.md`), UC-12, UC-13, ĐX-API-30.
- **Kết quả kiểm tra baseline và mã nguồn**:
  - `dotnet format --verify-no-changes --no-restore`: Đạt (0 error, 0 warning).
  - `dotnet build -c Release`: Đạt (0 error, 0 warning).
  - `dotnet test tests/QuanLyMuonTra.UnitTests -c Release`: Đạt **440/440 Passed** (baseline 438 + 2 ca test L1 bổ sung).
  - `dotnet test tests/QuanLyMuonTra.UnitTests -c Release --filter "FullyQualifiedName~DanhMuc|FullyQualifiedName~QuyDinh"`: Đạt **207/207 Passed**.

---

## 2. Bảng đối chiếu 26 thao tác danh mục và cấu hình với openapi.yaml

Ký hiệu: ✅ Khớp · ⚠️ Lệch đã biết/OPEN · ❌ Lệch mới (đã xử lý) · ⛔ Thiếu (BLOCKED).

| # | operationId | Handler | Kết quả đối chiếu |
|---|---|---|---|
| 1 | qtDsPhongThietBi | `DsPhongThietBiHandler` | ✅ Sắp `tenPhong`, `maPhong` tăng dần |
| 2 | qtTaoPhongThietBi | `TaoPhongThietBiHandler` | ✅ Thiếu/rỗng tên → 400 qua validator |
| 3 | qtSuaPhongThietBi | `SuaPhongThietBiHandler` | ✅ 404; ⚠️ 409 `XUNG_DOT_CAP_NHAT` khai báo nhưng phòng không có `xmin` (O9) |
| 4 | qtDsLoaiThietBi | `DsLoaiThietBiHandler` | ✅ Lọc `ngungSuDung`, sắp `tenLoai`, `maLoai` |
| 5 | qtTaoLoaiThietBi | `TaoLoaiThietBiHandler` | ❌ **L1**: thiếu `phiTreMoiNgay` được nhận là 0 — **ĐÃ SỬA** ở commit `a23da44`; ⚠️ O10 |
| 6 | qtSuaLoaiThietBi | `SuaLoaiThietBiHandler` | ✅ 404, không có phí trong PUT; ⚠️ O9 |
| 7 | qtXoaLoaiThietBi | `XoaLoaiThietBiHandler` | ✅ 409 `DA_CO_LICH_SU_CHI_NGUNG_SU_DUNG` (D08-N) |
| 8 | qtNgungSuDungLoaiThietBi | `NgungSuDungLoaiThietBiHandler` | ✅ Trả `LoaiThietBiQuanTriDto`, idempotent; ⚠️ O9 |
| 9 | qtDsThietBi | `DsThietBiHandler` | ✅ Sắp `maThietBi`, lọc loại/phòng/tình trạng/tên; ⚠️ O7 thiếu `dsAnh` |
| 10 | qtTaoThietBi | `TaoThietBiHandler` | ✅ 409 `MA_THIET_BI_TRUNG`; ⚠️ O7, P2 |
| 11 | qtSuaThietBi | `SuaThietBiHandler` | ✅ 404, 409 (có `xmin`); ⚠️ P1, P2 |
| 12 | qtXoaThietBi | `XoaThietBiHandler` | ✅ (D08-N) |
| 13 | qtNgungSuDungThietBi | `NgungSuDungThietBiHandler` | ✅ Idempotent, 409 có `xmin` |
| 14 | qtCapNhatGiaTriThietBi | `CapNhatGiaTriThietBiHandler` | ✅ Không khai báo 409 trong YAML, code không sinh |
| 15 | qtDsNhaCungCap | `DsNhaCungCapHandler` | ✅ Khớp OpenAPI |
| 16 | qtLayQuyDinhMuon | — | ⛔ **Thiếu** (BLOCKED bởi Q1–Q3) |
| 17 | qtCapNhatQuyDinhMuon | — | ⛔ **Thiếu** (BLOCKED bởi Q1–Q3) |
| 18 | qtLayThamSo | `LayThamSoHandler` | ✅ `gioMoCua`/`gioDongCua` null hợp lệ |
| 19 | qtCapNhatThamSo | `CapNhatThamSoHandler` | ✅ Mọi trường `JsonRequired`, 422; ⚠️ O9 |
| 20 | qtDsKhungPhat | `DsKhungPhatHandler` (`chiDangSuDung=false`) | ✅ Sắp `tyLePhanTram`, `maKhung` |
| 21 | qtTaoKhungPhat | `TaoKhungPhatHandler` | ✅ 400/422 |
| 22 | qtSuaKhungPhat | `SuaKhungPhatHandler` | ✅ 404, 422; ⚠️ O9 |
| 23 | qtXoaKhungPhat | `XoaKhungPhatHandler` | ✅ 409 khi đang dùng |
| 24 | qtTatKhungPhat | `TatKhungPhatHandler` | ✅ Idempotent; ⚠️ O9 |
| 25 | qtCapNhatPhiTre | `CapNhatPhiTreHandler` | ✅ 404, 422; ⚠️ O9 |
| 26 | quayDsKhungPhat | `DsKhungPhatHandler` (`chiDangSuDung=true`) | ✅; ⚠️ O8 phân trang đã ghi nhận |

---

## 3. Danh sách điểm lệch và vấn đề OPEN

1. **L1 (Đã sửa)**: `LoaiThietBiTaoMoiRequest.PhiTreMoiNgay` thiếu `[property: JsonRequired]` khiến thiếu trường trong JSON được nhận là 0 thay vì ném lỗi. Đã bổ sung thuộc tính và test unit `UC12_Json_TaoLoaiThieuPhiTreMoiNgay_NemJsonException` tại commit `a23da44`.
2. **O7 (Chờ D10-N)**: `ThietBiQuanTriDto` hiện chưa có mảng `dsAnh` (required trong OpenAPI). Sẽ hiện thực kèm module ảnh ở D10-N.
3. **O8 (Đã ghi nhận)**: Phân trang `quayDsKhungPhat` đã ghi nhận trong tài liệu C3.
4. **O9 (Chờ nhóm chốt)**: OpenAPI khai báo 409 `XUNG_DOT_CAP_NHAT` cho PUT/POST của phòng, loại, tham số, khung phạt, phí; nhưng theo KT 7.1 chỉ có 5 aggregate dùng cột hệ thống `xmin` (trong đó DanhMuc chỉ có `thiet_bi`). Code hiện tại không phát sinh 409 cho các thực thể không có `xmin`. Chờ nhóm thống nhất sửa YAML hoặc bổ sung `xmin`.
5. **O10 (Chờ nhóm chốt)**: Tạo loại thiết bị chưa giới hạn trần trên của `phiTreMoiNgay` (vượt `numeric(14,0)` có thể gây lỗi tầng CSDL).
6. **P1 (Cần đối chiếu)**: PUT thiết bị khi không gửi `dsMaNCC` → giữ nguyên danh sách hiện tại; khi gửi `[]` → gỡ bỏ toàn bộ liên kết nhà cung cấp. Hành vi này đã có test kiểm chứng.
7. **P2 (Chờ D10-N)**: Khi gửi `dsMaAnh` khác rỗng ở tạo/sửa thiết bị hiện ném `InvalidOperationException` do module ảnh chưa tích hợp.
8. **Q1–Q3 (BLOCKED)**: 2 thao tác cấu hình quy định mượn (`qtLayQuyDinhMuon`, `qtCapNhatQuyDinhMuon`) đang bị chặn chờ nhóm chốt nguồn `tenNhom`, mã lỗi khi loại/nhóm không tồn tại, và phạm vi `dsNhom`.

---

## 4. Bàn giao cấu hình module và migration cho hàng đợi tích hợp của Khôi

### 4.1. Cấu hình nạp vào Host
- Host (`QuanLyMuonTra.Api/Program.cs`, dòng 42–43) đã nạp 2 extension đăng ký mô hình:
  - `DanhMucModelBuilderExtensions` (namespace `QuanLyMuonTra.Modules.DanhMuc.Infrastructure`)
  - `QuyDinhModelBuilderExtensions` (namespace `QuanLyMuonTra.Modules.QuyDinh.Infrastructure`)
- Tổng cộng **10 bảng** đã được ánh xạ qua EF Core:
  - Schema `danh_muc`: `phong_thiet_bi`, `loai_thiet_bi`, `thiet_bi`, `lich_su_gia_tri`, `nha_cung_cap`, `thiet_bi_nha_cung_cap`, `loai_thiet_bi_nha_cung_cap`.
  - Schema `quy_dinh`: `quy_dinh_muon`, `khung_phat`, `tham_so_he_thong`.

### 4.2. Thay đổi ánh xạ kể từ D02-N
- Kiểm tra `git log` đến ngày 21/11/2026: **Không có thay đổi lược đồ bảng, cột hay chỉ mục mới** trong cấu hình EF Core kể từ D02-N.
- Các thay đổi kể từ D02-N tập trung vào logic miền, nghiệp vụ ngưng sử dụng/xóa, kiểm tra tham chiếu qua provider và ghi nhật ký kiểm toán.
- Toàn bộ 32 test kiểm tra cấu hình mô hình (`MoHinh`) đều Passed.

### 4.3. SQL thô cần bổ sung trong migration của Khôi (theo schema.sql)
Ngoài các khai báo ánh xạ thực thể EF Core, migration chung cần bổ sung:
1. **Khóa ngoại liên module**:
   - `fk_qdm_nhom`: `quy_dinh.quy_dinh_muon(ma_nhom)` → `sinh_vien.nhom_sinh_vien(ma_nhom)`.
   - `fk_qdm_loai`: `quy_dinh.quy_dinh_muon(ma_loai)` → `danh_muc.loai_thiet_bi(ma_loai)`.
2. **Trigger ràng buộc hoãn**:
   - `trg_tb_co_gia_tri` (`schema.sql`, dòng 949): Đảm bảo mọi thiết bị phải có ít nhất một bản ghi lịch sử giá trị khi commit giao dịch.
3. **Seed dữ liệu bắt buộc**:
   - Thực thi `db/seed-required/quy-dinh-bat-buoc.sql` (tham số hệ thống mặc định và quy định mượn cơ sở).

### 4.4. Module BaoTri
- Chưa có ánh xạ EF Core trong đợt này (theo kế hoạch từ D13-N trở đi). Chưa có gì để bàn giao cho Host.

### 4.5. Cam kết về Migration và Snapshot
- **Không tự ý tạo hoặc chỉnh sửa** thư mục `Migrations` và snapshot chung của `AppDbContext` trong PR này. Khôi toàn quyền điều phối thứ tự đưa vào hàng đợi tích hợp (G5).

---

## 5. Ca đầu vào sai (kết quả Phần D)

Đã rà soát và kiểm chứng 207 unit tests của DanhMuc và QuyDinh. Tất cả các nhóm ca sai đều có ca kiểm thử tương ứng và đang xanh:

| Nhóm ca sai | Thao tác kiểm tra | Ca kiểm thử tiêu biểu (đều Passed) |
|---|---|---|
| Thiếu khóa JSON bắt buộc | Tạo loại thiết bị (L1), tham số, khung phạt, phí trễ, thiết bị, giá trị | `UC12_Json_TaoLoaiThieuPhiTreMoiNgay_NemJsonException`, `UC13_Json_CapNhatThamSo_ThieuKhoa_NemJsonException`, `UC13_Json_CapNhatPhiTre_ThieuKhoa_NemJsonException` |
| Giá trị miền sai (422) | Tham số (giờ mở $\ge$ giờ đóng, chỉ nhập 1 giờ), Khung phạt (tỷ lệ $\le 0$ hoặc $> 100$, sai số lẻ), Phí trễ âm | `UC13_ThamSo_GioMoSauGioDong_NemLoiNghiepVu`, `UC13_ThamSo_ChiCoMotGio_NemLoiNghiepVu`, `UC13_KhungPhat_TyLeAmHoacLonHon100_NemLoiNghiepVu`, `UC13_CapNhatPhiTre_PhiAm_NemLoiNghiepVu` |
| Không tồn tại (404) | Sửa/ngưng/xóa phòng, loại, thiết bị, khung phạt không tồn tại | `UC12_SuaPhong_KhongTonTai_Nem404`, `UC12_SuaLoai_KhongTonTai_Nem404`, `UC12_SuaThietBi_KhongTonTai_Nem404`, `UC13_SuaKhungPhat_KhongTonTai_Nem404` |
| Xung đột nghiệp vụ (409) | Trùng mã thiết bị; xóa loại/thiết bị/khung đang được sử dụng hoặc có lịch sử | `UC12_TaoThietBi_TrungMa_NemLoiXungDot`, `UC12_XoaLoai_DaCoLichSu_NemLoiXungDot`, `UC12_XoaThietBi_DangMuon_NemLoiXungDot`, `UC13_XoaKhungPhat_DangSuDung_NemLoiXungDot` |

---

## 6. Mốc EXPLAIN (kết quả Phần E)

- **Trạng thái**: **Đã chuẩn bị file kiểm thử — Chưa đo số liệu thật (BLOCKED K-1)**.
- **Lý do**: Đã chuẩn bị file kiểm thử `ExplainTruyVanDocCsdlTests.cs` (giao dịch rollback với 2.000 thiết bị mẫu); tuy nhiên máy phát triển cục bộ hiện chưa cấu hình chuỗi kết nối cơ sở dữ liệu PostgreSQL kiểm thử (`ConnectionStrings:Test` chưa có trong `user-secrets` hoặc biến môi trường). Test được gắn `[CanKetNoiTestFact]` để tự động bỏ qua khi thiếu kết nối và sẵn sàng chạy khi có DB kiểm thử.
- **Kế hoạch tiếp theo**: Mốc so sánh EXPLAIN chi tiết sẽ được thực hiện và ghi nhận vào tài liệu khi Khôi cấp cơ sở dữ liệu kiểm thử dùng chung trên Supabase hoặc ở các mốc Ngày 25/26.

---

## 7. Trạng thái kiểm chứng tổng thể

| Hạng mục kiểm chứng (D09-N) | Trạng thái |
|---|---|
| `dotnet format --verify-no-changes`, `dotnet build -c Release` | Đạt (0 warning, 0 error) — 21/11/2026 — .NET SDK 9.0.321 |
| `UnitTests` (toàn bộ solution) | Đạt 440/440 Passed (438 baseline + 2 test L1) — 21/11/2026 |
| `UnitTests.DanhMuc` và `UnitTests.QuyDinh` (ca đầu vào sai) | Đạt 207/207 Passed — 21/11/2026 |
| Kiểm thử cấu hình mô hình EF Core (`MoHinh`) | Đạt 32/32 Passed — 21/11/2026 |
| `IntegrationTests` CSDL trên PostgreSQL | Chưa kiểm chứng cục bộ — BLOCKED bởi K-1 (chờ kết nối DB kiểm thử) |
| Mốc EXPLAIN trên PostgreSQL kiểm thử | Chưa kiểm chứng — BLOCKED bởi K-1 |
| `qtLayQuyDinhMuon`, `qtCapNhatQuyDinhMuon` | Chưa kiểm chứng — BLOCKED bởi Q1–Q3 |
| Endpoint HTTP và quyền `QuanTri` thật qua HTTP | Chưa kiểm chứng — BLOCKED (chờ Host) |
