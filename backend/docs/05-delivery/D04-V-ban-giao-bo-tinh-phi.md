# D04-V — Bàn giao bộ tính phí và bộ tính hạn trả

Người lập: Nguyễn Đại Vương · Ngày: 15/11/2026 · Trạng thái: bàn giao, chưa nối vào Host

## 1. Bộ tính phí

- Vị trí: `QuanLyMuonTra.Modules.ViPham.Domain.TinhPhi` (`src/Modules/ViPham/QuanLyMuonTra.Modules.ViPham/Domain/TinhPhi.cs`). Lớp tĩnh, hàm thuần: không đọc đồng hồ, không truy cập CSDL, không ném lỗi.
- Bên dùng: ViPham (Khôi, từ D14). Nguồn dữ liệu đầu vào: `phi_tre_moi_ngay` và `ngay_gio_hen_tra` từ phiếu mượn (`IPhieuMuonReader`), giá trị hiện hành từ DanhMuc, tỷ lệ từ khung phạt.

| Hàm | Đầu vào | Kết quả | Quy tắc |
|---|---|---|---|
| `SoNgayTre` | `hanTra`, `thoiDiem`, `anHanPhut` | `int` | 0 nếu `thoiDiem <= hanTra + anHanPhut`; ngược lại `max(1, ngày lịch VN(thoiDiem) − ngày lịch VN(hanTra))` |
| `PhiTre` | `soNgayTre`, `phiMoiNgay` (đã lưu lúc giao), `giaTriHienHanh` | `decimal` | `min(soNgayTre × phiMoiNgay, giaTriHienHanh)`; phí cả phiếu do bên gọi cộng |
| `BoiThuongHuHong` | `giaTriHienHanh`, `tyLePhanTram` (ví dụ 15, 12,5) | `decimal` | `giaTriHienHanh × tyLePhanTram / 100`, làm tròn đến đồng, `AwayFromZero` |
| `BoiThuongMat` | `giaTriHienHanh` | `decimal` | 100% giá trị hiện hành |

- Chưa có hàm mốc "chưa trả quá 30 ngày" (BR-61); thuộc D15-K.

## 2. Bộ tính hạn trả

- Vị trí: `QuanLyMuonTra.Modules.MuonTra.Domain.BoTinhHanTra` (`src/Modules/MuonTra/QuanLyMuonTra.Modules.MuonTra/Domain/BoTinhHanTra.cs`), hiện thực `IBoTinhHanTra`; đăng ký Scoped trong `AddMuonTraModule`.
- Quy tắc: `hanTra = min(gioGiao + thoiGianMuonGio, giờ đóng cửa cùng ngày lịch VN)`; kết quả ở UTC.
- Lỗi: `gioDongCua` null → 409 `CHUA_CAU_HINH_GIO_QUAY`; `thoiGianMuonGio <= 0` → 400 `DU_LIEU_KHONG_HOP_LE`; giờ giao từ giờ đóng cửa trở đi → 422 `GIAO_SAU_GIO_DONG_CUA`. Cả ba ném `LoiNghiepVuException`.
- Việc chọn `thoiGianMuonGio` (giờ dự kiến của yêu cầu; tối đa nhóm cho mượn trực tiếp/danh sách chờ; mức nhỏ nhất theo loại) thuộc handler D09-V.

## 3. Thay đổi kèm theo

- `VnTime`: thêm `GioLich(DateTimeOffset)` và `ThoiDiem(DateOnly, TimeOnly)` (SharedKernel).
- `MuonTra/Domain/MaLoi`: thêm `GiaoSauGioDongCua`, `ChuaCauHinhGioQuay`.
- `QuanLyMuonTra.UnitTests` tham chiếu thêm module ViPham.

## 4. Kiểm thử

- `tests/QuanLyMuonTra.UnitTests/ViPham/TinhPhiTests.cs` (22 ca) và `tests/QuanLyMuonTra.UnitTests/MuonTra/BoTinhHanTraTests.cs` (16 ca). Chỉ kiểm đơn vị, không cần CSDL.
- Lệnh: `dotnet test tests\QuanLyMuonTra.UnitTests -c Release --filter "FullyQualifiedName~TinhPhiTests|FullyQualifiedName~BoTinhHanTraTests"` (chạy từ thư mục `backend`).

## 5. Việc còn lại cho Host (Khôi)

- `Program.cs` chưa gọi `AddMuonTraModule()`, nên `IBoTinhHanTra` chưa giải mã được ở runtime Host.
- `XuLyLoiToanCuc` chỉ ánh xạ `LoiNghiepVuException`; các ngoại lệ `NghiepVuException` trong Domain MuonTra/QuyDinh sẽ thành HTTP 500 nếu không được ánh xạ.
- O2 (vị trí bộ tính hạn trả): đề xuất đặt ở `MuonTra/Domain`, chờ Khôi xác nhận.
