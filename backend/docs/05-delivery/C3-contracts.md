# Gói hợp đồng C3 (D01-N)

Cập nhật lần đầu: 13/11/2026 · Chủ sở hữu: Trần Lê Đức Lợi · Trạng thái: **hợp đồng đã khai báo trong `*.Contracts`, chưa có bản hiện thực**

Quy ước: "Nguồn" là điều tài liệu dự án nêu. "Đề xuất D01-N" là phần do task D01-N đặt ra, **chờ Vương/Khôi xác nhận** (OPEN). Mọi bản giả chỉ nằm trong `tests/`, không đăng ký trên runtime. Mọi phương thức I/O có hậu tố `Async`, tham số cuối `CancellationToken ct` (CV 19.1). Toàn bộ port chạy trong **giao dịch và `AppDbContext` của bên gọi**; port không tự `BEGIN`/`COMMIT`.

## 1. Module thuộc Lợi

DanhMuc (`danh_muc`), QuyDinh (`quy_dinh`), BaoTri (`bao_tri`), BaoCao (không sở hữu bảng, chỉ đọc nhiều schema bằng SQL). Không có CRUD nhà cung cấp, sinh viên, nhóm sinh viên; không đặt giờ quầy mặc định.

## 2. Port đọc

| Cổng | Vị trí | Đầu vào → Đầu ra | Lỗi | Bên dùng | Chốt |
|---|---|---|---|---|---|
| `IDanhMucReader.LayThietBiAsync` | `DanhMuc.Contracts` | `maThietBi` → `ThietBiDto?` (`null` = không tồn tại; kèm `NgayNhapKho`, `GiaTriHienHanh` = bản ghi `lich_su_gia_tri` mới nhất, BR-17, BR-70) | Không ném; `null` do bên gọi ánh xạ `KHONG_TIM_THAY` | MuonTra (kiểm thiết bị, vị trí kệ/tủ), ViPham (giá trị bồi thường), BaoTri (tính hạn bảo trì) | D04 cài |
| `IDanhMucReader.LayLoaiAsync` | như trên | `maLoai` → `LoaiThietBiDto?` (có `PhiTreMoiNgay`, `ChuKyBaoTri`, `NgungSuDung`) | như trên | MuonTra (lưu phí lúc giao), BaoTri | D04 |
| `IDanhMucReader.LayPhongAsync` | như trên | `maPhong` → `PhongThietBiDto?` | như trên | MuonTra, TaiKhoan | D04 |
| `IDanhMucReader.DsPhongAsync`, `DsLoaiDangSuDungAsync` | như trên | → danh sách phòng; danh sách loại **chưa ngưng sử dụng** (sắp theo tên, khóa phụ mã tăng dần, ĐX-API-28) | Không ném; rỗng hợp lệ | MuonTra (dữ liệu chọn phòng/loại) | D04 |
| `IQuyDinhReader.LayQuyDinhNhomAsync` | `QuyDinh.Contracts` | `maNhom` → `QuyDinhNhomDto?`: dòng chung (`SoThietBiToiDa`, `ThoiGianMuonToiDaGio`) + `DsNgoaiLeTheoLoai` (chỉ thời gian) | `null` = nhóm chưa có quy định chung: **OPEN**, bên gọi chặn an toàn, không dùng mặc định | MuonTra (BR-24/25, hạn trả) | D04 |
| `IQuyDinhReader.LayThamSoAsync` | như trên | → `ThamSoHeThongDto` (dòng `id = 1`); `GioMoCua`/`GioDongCua` = `null` khi chưa cấu hình | Không ném; bên gọi trả `CHUA_CAU_HINH_GIO_QUAY` khi cần giờ mà `null` (ĐX-03) | MuonTra, ViPham | D04 |
| `IQuyDinhReader.LayKhungPhatAsync`, `DsKhungPhatDangSuDungAsync` | như trên | `maKhung` → `KhungPhatDto?`; danh sách chỉ `DangSuDung = true` | `null`/rỗng hợp lệ | MuonTra (`GET /khung-phat`), ViPham | D04 |

Mặc định (đọc từ DB, **không** hard-code ở bên gọi): `SoNgayKhoa` 3, `ThoiGianGiuChoPhut` 30, `AnHanPhut` 15, `SoGioGiaHanToiDa` 2, `NguongViPham` 3, `SoLanDangNhapSaiToiDa` 5; quy định chung `DAI_HOC` 2 thiết bị/5 giờ, `CAO_HOC` và `DO_AN_TOT_NGHIEP` 3 thiết bị/10 giờ; khung phạt Nhẹ 15, Vừa 40, Nặng 80 (BusinessRule mục 14, `schema.sql` mục 10). Không hồi tố: reader luôn đọc giá trị hiện hành; giá trị cần giữ đã lưu vào chứng từ (KT 8.7).

## 3. Port cập nhật có điều kiện — `IThietBiService` (`DanhMuc.Contracts`)

Giao dịch: chạy **trong giao dịch của bên gọi**, bên gọi đã giữ khóa tư vấn theo cặp (phòng, loại) (C2, KT 8.2). Hiện thực bằng `UPDATE … WHERE ma_thiet_bi = ? AND tinh_trang = <nguồn> AND ngung_su_dung = false`; trả `true` khi **đúng 1 dòng** bị ảnh hưởng, `false` khi 0 dòng. Port không ném lỗi nghiệp vụ; bên gọi hoàn tác và ánh xạ lỗi.

| Phương thức | Chuyển trạng thái | Lỗi bên gọi ánh xạ khi `false` | Bên gọi |
|---|---|---|---|
| `DatDangDuocMuonAsync(maThietBi, maPhong, maLoai)` | `SAN_SANG` → `DANG_DUOC_MUON`, thêm điều kiện đúng phòng, loại và chưa ngưng sử dụng (`ngung_su_dung = false`) (KT 8.4, BR-18) | `THIET_BI_DA_DUOC_GIAO` (409) | MuonTra |
| `NhanTraNguyenVenAsync` | `DANG_DUOC_MUON` → `SAN_SANG` | `TRANG_THAI_KHONG_HOP_LE` (409) | MuonTra |
| `NhanTraHuHongAsync` | `DANG_DUOC_MUON` → `DANG_BAO_TRI_SUA_CHUA` (BR-51) | như trên | MuonTra |
| `DanhDauMatAsync` | `DANG_DUOC_MUON` → `DA_BAO_MAT` | như trên | MuonTra |
| `ChuyenBaoTriAsync` | `SAN_SANG` → `DANG_BAO_TRI_SUA_CHUA` | như trên | BaoTri |
| `HoanThanhBaoTriAsync` | `DANG_BAO_TRI_SUA_CHUA` → `SAN_SANG` | như trên | BaoTri |
| `ThanhLyAsync` | `SAN_SANG` hoặc `DANG_BAO_TRI_SUA_CHUA` → `DA_THANH_LY` | như trên | BaoTri |

Nhật ký: người gọi/DanhMuc dùng thành phần ghi nhật ký của Khôi (K-2 còn OPEN); C3 không định nghĩa cổng nhật ký. Bản giả: chỉ trong `tests/`.

## 4. Port tham chiếu (đảo phụ thuộc, KT 4.2, 6.5)

| Cổng | Khai báo | Phương thức | Bên hiện thực (bảng tham chiếu) | Lỗi/An toàn |
|---|---|---|---|---|
| `IThamChieuDanhMucProvider` | `DanhMuc.Contracts` | `CoThamChieuThietBiAsync(maThietBi)`, `CoThamChieuLoaiAsync(maLoai)`; `true` = có tham chiếu → chặn xóa cứng (`DA_CO_LICH_SU_CHI_NGUNG_SU_DUNG`) | MuonTra (`chi_tiet_phieu_muon`; `chi_tiet_yeu_cau_muon`, `dang_ky_cho`) — V02; ViPham (`chi_tiet_bien_ban`) — K03; BaoTri (`bao_tri_sua_chua`, `chi_tiet_thanh_ly`) — N03; QuyDinh (`quy_dinh_muon` ngoại lệ theo loại) — N03 | Phải đọc DB thật; thiếu bản hiện thực thì handler xóa **chặn an toàn**, không coi là "không có tham chiếu" (D08-N) |
| `IThamChieuKhungPhatProvider` | `QuyDinh.Contracts` | `CoThamChieuKhungAsync(maKhung)`; `true` → `KHUNG_PHAT_DA_DUNG_KHONG_XOA` (409) | MuonTra (`chi_tiet_phieu_muon`), ViPham (`chi_tiet_bien_ban`) | như trên |

Điều kiện `tinh_trang ≠ SAN_SANG` và "còn thiết bị thuộc loại" do DanhMuc tự kiểm (bảng của chính nó).

## 5. Mã dữ liệu thử đã khóa

Tệp: `backend/db/seed-demo/C3-du-lieu-thu.sql`, sau đó `backend/db/seed-demo/C3-du-lieu-thu-ncc.sql` (idempotent). Chạy sau `schema.sql`. Không dùng trên production.

| Đối tượng | Mã |
|---|---|
| Nhóm (đã có sẵn trong DDL, không tạo thêm) | `DAI_HOC`, `CAO_HOC`, `DO_AN_TOT_NGHIEP` |
| Phòng A / B | `019c0000-0000-7000-8000-000000000001` / `…0002` |
| Loại Laptop (phí 50.000) / Máy chiếu (phí 30.000) | `019c0000-0000-7000-8000-000000000011` / `…0012` |
| Thiết bị | Phòng A: `TEST-LAP-001` (SAN_SANG), `-002` (SAN_SANG), `-003` (DANG_DUOC_MUON), `-004` (DANG_BAO_TRI_SUA_CHUA), `TEST-MC-001` (SAN_SANG), `-002` (DA_BAO_MAT). Phòng B: `TEST-LAP-101` (SAN_SANG), `-102` (DA_THANH_LY), `TEST-MC-101` (SAN_SANG), `-102` (DANG_DUOC_MUON) |
| Giá trị hiện hành | Laptop 15.000.000; Máy chiếu 8.000.000 |
| Ngoại lệ thời gian | `DAI_HOC` × Máy chiếu = 3 giờ (`…0301`) |
| Khung phạt thử | `…0401` 12,50% đang dùng; `…0402` 5,00% đã tắt |
| Giờ quầy | `NULL` (chưa cấu hình, ĐX-03) |
| Nhà cung cấp thử | `019c0000-0000-7000-8000-000000000101` / `…0102` / `…0103` (tệp `C3-du-lieu-thu-ncc.sql`, chạy sau `C3-du-lieu-thu.sql`) |
| Gán NCC cho thiết bị | `TEST-LAP-001` ↔ 101, 102; `TEST-LAP-002` ↔ 101; `TEST-MC-001` ↔ 102, 103; thiết bị còn lại không có NCC |
| Ảnh danh mục | Không có dòng nào trong `luu_tru.anh_tai_len`; mọi thiết bị thử có `dsAnh` rỗng |

Lưu ý: tình trạng `DANG_DUOC_MUON` trong dữ liệu thử đặt trực tiếp, chưa gắn phiếu mượn (phiếu thuộc dữ liệu thử của MuonTra).

## 6. Oracle báo cáo

`backend/db/seed-demo/C3-oracle-tinh-trang-kho.csv`: oracle `tinh-trang-kho` (API-Notes 9.1), 10 cột đúng thứ tự, 4 dòng, tổng 10 thiết bị. So sánh theo tập dòng. Oracle chi tiết 5 báo cáo còn lại: N12, N17–19.

## 7. OPEN

| # | Nội dung | Người chốt | Trước |
|---|---|---|---|
| O1 | Tên phương thức, DTO và 2 interface reader là đề xuất D01-N | Vương, Khôi | D02 |
| O2 | Hậu tố `Async` (nguồn ví dụ `DatDangDuocMuon`) | Vương | D02 |
| O3 | Mã lỗi khi nhóm chưa có quy định chung | Nhóm | D04 |
| O4 | Báo cáo kho có tính thiết bị `ngung_su_dung = true` vào `soSanSang`/`tong` hay không | Nhóm | D17 |
| O5 | TaiKhoan cần `SoLanDangNhapSaiToiDa` nhưng chưa có cạnh phụ thuộc tới QuyDinh.Contracts (KT 4.2) | Khôi | D05 |
| O6 | Dữ liệu thử chưa kiểm chứng trên PostgreSQL thật (phụ thuộc K-1) | Khôi, Lợi | D02 |

## 8. Trạng thái kiểm chứng

| Hạng mục | Trạng thái |
|---|---|
| `dotnet build -c Release`, `dotnet format --verify-no-changes` | Đạt (0 warning, 0 error), 13/11/2026, .NET SDK 9.0.305 |
| Dữ liệu thử + oracle trên PostgreSQL | Chưa kiểm chứng trên PostgreSQL thật — BLOCKED bởi môi trường test (K-1 còn OPEN) |
| Test mô hình EF `MoHinhEf` (không kết nối DB) | Đạt 29/29 (22 DanhMuc + 7 QuyDinh) — 14/11/2026 — .NET SDK 9.0.305 |
| Seed bắt buộc tách riêng, seed NCC trên PostgreSQL | Chưa kiểm chứng — BLOCKED bởi K-1 |

## 9. Bàn giao ánh xạ EF cho tích hợp (D02-N)

| Mục | Nội dung |
|---|---|
| Hàm đăng ký (đề xuất, chờ Khôi) | `ApplyDanhMucConfigurations(this ModelBuilder)` trong `QuanLyMuonTra.Modules.DanhMuc.Infrastructure`; `ApplyQuyDinhConfigurations(this ModelBuilder)` trong `QuanLyMuonTra.Modules.QuyDinh.Infrastructure` |
| Bảng đã ánh xạ | `danh_muc`: `phong_thiet_bi`, `loai_thiet_bi`, `nha_cung_cap`, `thiet_bi`, `lich_su_gia_tri`, `thiet_bi_nha_cung_cap`, `loai_thiet_bi_nha_cung_cap`; `quy_dinh`: `tham_so_he_thong`, `khung_phat`, `quy_dinh_muon` |
| Chưa ánh xạ | `bao_tri.*` (ngày BaoTri của N) |
| Gói | Hai module thêm `Microsoft.EntityFrameworkCore`; phiên bản phải trùng `Infrastructure` |
| Có trong ánh xạ | Tên schema/bảng/cột, khóa chính, FK trong cùng module (tên và `ON DELETE` như DDL), CHECK, chỉ mục (kể cả có điều kiện), mặc định, `xmin` của `thiet_bi`, chuyển đổi chuỗi của `tinh_trang` |
| **Không** có trong ánh xạ | FK liên module `fk_qdm_nhom`, `fk_qdm_loai`; trigger hoãn `trg_tb_co_gia_tri`; `COMMENT`; `GRANT`/`REVOKE`/RLS; seed |
| Seed | Bắt buộc: `backend/db/seed-required/quy-dinh-bat-buoc.sql` (phụ thuộc nhóm SV đã có). Demo: `backend/db/seed-demo/` |
| OPEN | Q-A (phiên bản gói), Q-B (tên hàm, K-4), Q-C (FK liên module vào migration bằng SQL thô), Q-D (nguồn seed bắt buộc trong đường migration) |
| Chưa kiểm chứng | Migration sinh từ ánh xạ so với `schema.sql` (`pg_dump --schema-only`); mọi thứ trên PostgreSQL/Supabase thật (K-1) |

