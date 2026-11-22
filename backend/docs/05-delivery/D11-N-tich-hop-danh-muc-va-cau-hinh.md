# D11-N — Tích hợp danh mục và cấu hình với backend thật

## 1. Môi trường kiểm chứng

- Ngày chạy: 23/11/2026; .NET SDK: 9.0.321
- Cơ sở dữ liệu: PostgreSQL trên Supabase **cá nhân của Lợi** (không phải môi trường test chung K-1); nạp `schema.sql`, không nạp dữ liệu demo; test chạy trong giao dịch rollback
- Storage: Supabase Storage dự án cá nhân; bucket `thiet-bi-anh` (công khai), `tinh-trang-anh` (riêng tư)
- Commit: 17d7775

## 2. Kết quả kiểm chứng

| Hạng mục | Lệnh/Bộ test | Kết quả |
|---|---|---|
| Build, format | `dotnet build -c Release`, `dotnet format --verify-no-changes` | Đạt (0 error, 0 warning), format chuẩn không thay đổi |
| UnitTests | `UnitTests` | Đạt 479/479 Passed (Failed: 0, Skipped: 0) |
| Cấu trúc CSDL | `Database.CauTrucCsdlTests` | Đạt 25/25 Passed (Failed: 0, Skipped: 0) |
| UC-12 trên PostgreSQL | `IntegrationTests.DanhMuc.` | Đạt 65/65 Passed (Failed: 0, Skipped: 0, log: `d11n-danhmuc.trx`) |
| UC-13 trên PostgreSQL | `IntegrationTests.QuyDinh.` | Đạt 20/20 Passed (Failed: 0, Skipped: 0, log: `d11n-quydinh.trx`) |
| Chặn giao khi chưa cấu hình giờ | `GiaoThietBiTests.UC07_ChuaCauHinhGioQuay_Tra409` | Đạt (trả 409 `CHUA_CAU_HINH_GIO_QUAY`, thiết bị giữ nguyên `SAN_SANG`) |
| Khả dụng trên DB thật | `KhaDungVaKhoaTests` | Đạt (9/9 ca Passed, tổng 10/10 ca MuonTra Passed, log: `d11n-muontra.trx`) |
| Storage thật (adapter) | `SupabaseAnhStorageThatTests` | 2 ca Skipped (chưa cấu hình trong user-secrets) |
| Seed bắt buộc tách demo | ca `UC13_SeedBatBuoc…`, `Select-String` | Đạt (seed bắt buộc chạy 2 lần không trùng; `Select-String` không có dòng kết quả) |

## 3. Chưa kiểm chứng / BLOCKED

| Hạng mục | Lý do | Chờ ai |
|---|---|---|
| Xác thực, quyền `QuanTri`/`NhanVien` thật qua HTTP cho 24 thao tác UC-12/13 | Chưa có endpoint (manifest `CHUA_CAI`) | Khôi |
| Storage thật qua luồng tạo/sửa thiết bị kèm `dsMaAnh` | Cùng lý do | Khôi |
| Ngưng/xóa thiết bị sẵn sàng không phá giữ chỗ | Q1 OPEN, handler chưa tính lại `khaDung` (ĐX-KT-13) | Nhóm, Vương |
| `qtLayQuyDinhMuon`, `qtCapNhatQuyDinhMuon` | Q1–Q3 | Nhóm |
| Nghiệm thu tích hợp thật với duyệt/từ chối có outbox và job | D10-V, D10-K chưa có | Vương, Khôi |
| Môi trường test chung | K-1 OPEN | Khôi, nhóm |

## 4. Lỗi cần sửa

| Ca / hạng mục | Thông báo lỗi | Thuộc module | Người xử lý | Trạng thái |
|---|---|---|---|---|
| Không có | Không phát sinh lỗi loại A khi chạy trên PostgreSQL thật | Không có | Không có | Đạt |

## 5. Provider tham chiếu còn chờ

- `IThamChieuDanhMucProvider`: runtime có đủ 4 provider (MuonTra, ViPham, BaoTri, QuyDinh), `ProviderThamChieuHostTests` Đạt 1/1 Passed. Mới kiểm bằng dữ liệu thử SQL, chưa bằng luồng nghiệp vụ thật (phát sinh từ phiếu mượn, biên bản, bảo trì thật) — kiểm sâu ở D21-N/D22-N.
- `IThamChieuKhungPhatProvider`: MuonTra, ViPham — như trên.
- Chưa có: Không có (runtime DI đã đăng ký đủ 4 provider DanhMuc và 2 provider KhungPhat).

## 6. Dữ liệu và hợp đồng bàn giao cho MuonTra/BaoTri

- Dữ liệu thử: `backend/db/seed-demo/C3-du-lieu-thu.sql`, `C3-du-lieu-thu-ncc.sql`; oracle: `C3-oracle-tinh-trang-kho.csv`; mã cố định ở `C3-contracts.md` mục 5.
- Seed bắt buộc: `backend/db/seed-required/quy-dinh-bat-buoc.sql` (idempotent, không chứa danh mục/giờ quầy); giờ quầy mặc định `NULL`.
- Cổng đọc/cập nhật/tham chiếu: `C3-contracts.md` mục 2–4.
- Mặc định đọc từ DB, không hard-code: xem `C3-contracts.md` mục 2.
