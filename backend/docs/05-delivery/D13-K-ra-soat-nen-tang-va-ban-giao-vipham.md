# D13-K — Rà nền tảng và bàn giao quyền sở hữu module ViPham

Người lập: Đinh Nhật Khôi · Ngày lập: 25/11/2026 · Trạng thái: bàn giao nội bộ nhóm

## 1. Phạm vi và cách rà

- Task D13-K (kế hoạch 40 ngày, Ngày 13). Chỉ rà soát và lập hồ sơ, không sửa mã nguồn.
- Nhánh thực hiện: `docs/ra-soat-nen-tang-vipham` (tách từ `develop` tại commit `058c96b`, trùng `origin/develop`).
- Commit gốc đã rà (`git rev-parse --short HEAD`): `058c96b` (`Merge pull request #57 from dl1803/feature/uf-sv-01-tra-cuu-thiet-bi`).
- Kết quả D11-V và D12-V trên `develop` tại thời điểm rà:
  - **D11-V đã gộp** qua PR #59 (`ff4b768`, merged 24/11/2026): đã có `D11-V-ban-giao-doc-phieu-muon.md`, 5 endpoint đọc phiếu mượn, `PhieuMuonReader` hiện thực `IPhieuMuonReader` và đăng ký DI trong `MuonTra/DependencyInjection.cs`, kèm `PhieuMuonReaderTests` (4 ca).
  - **D12-V chưa thấy** trên `develop`: chưa có handler đăng ký chờ (chỉ có entity `DangKyCho`) và chưa có tài liệu bàn giao D12-V.
- Mọi phát hiện ở mục 3 đến 5 xuất phát từ đọc mã và chạy lệnh trực tiếp trên commit `058c96b`; các mục chưa triển khai được ghi rõ là chưa kiểm.

## 2. Kết quả chạy thực tế

| Lệnh | Kết quả |
|---|---|
| `dotnet build -c Release` | Build succeeded (0 Warning(s), 0 Error(s), 13.61 s) |
| `dotnet test` UnitTests (`QuanLyMuonTra.UnitTests -c Release --no-build`) | Passed 515, Failed 0, Skipped 0 (Total 515) |
| `ManifestOperationTests` (`IntegrationTests --no-build`) | Passed 2, Failed 0, Skipped 0 (Total 2) |
| `TinhPhiTests` (`UnitTests --no-build`) | Passed 30, Failed 0, Skipped 0 (Total 30) |
| `ViPhamThamChieuTests` (cần DB, `ConnectionStrings:Test`) | Passed 6, Failed 0, Skipped 0 (Total 6) |
| `CapNhatPhiTre` (`CapNhatPhiTreCsdlTests`, cần DB, `ConnectionStrings:Test`) | Passed 4, Failed 0, Skipped 0 (Total 4) |
| `PhieuMuonReaderTests` (cần DB, `ConnectionStrings:Test`) | Passed 4, Failed 0, Skipped 0 (Total 4) |
| Manifest `DA_CAI` / `CHUA_CAI` | 38 / 50 (gồm 2 health + 36 API) |
| Mã băm `openapi.yaml` BE và FE | Khác SHA-256 byte-level do xuống dòng (`BE`: `1504739AA52F11B62C7CB2C79E1E20BEDFA3D707AB73C156B4A018DEAEF0FF8F` CRLF; `FE`: `D7C5F1250C6CD395A3A0233B3A2D741C8B5930860F04EFE5343444D6D669300F` LF); **giống nhau hoàn toàn** về nội dung khi chuẩn hóa xuống dòng (`\r\n` → `\n`) |
| `schema.sql` so với baseline | Khớp (`5A334746DD053A1E2F7E4EC184D7ADC14FC1BA591FE950CF033F8780235208A1`) |
| `BusinessRule.md` so với baseline | Khớp (`217A2950930DC2383BEC9B8DB74F7236E417FD6BCCCDE9767D1FE7B5658FFF5C`) |

## 3. Bộ tính phí và hợp đồng đọc phiếu

- `TinhPhi` (`Domain/TinhPhi.cs`) khớp BR-57 đến BR-60 qua đọc mã:
  - `SoNgayTre`: trả 0 khi `thoiDiem <= hanTra.AddMinutes(anHanPhut)`; ngược lại `Math.Max(1, ngayTra.DayNumber - ngayHan.DayNumber)` theo `VnTime.NgayLich` (giờ Việt Nam).
  - `PhiTre`: `Math.Min(soNgayTre * phiMoiNgay, giaTriHienHanh)` (trả 0 nếu tham số không dương).
  - `BoiThuongHuHong`: `decimal.Round(giaTriHienHanh * tyLePhanTram / 100m, 0, MidpointRounding.AwayFromZero)`.
  - `BoiThuongMat`: `Math.Max(0m, giaTriHienHanh)`.
  - Kết quả test `TinhPhiTests`: Passed 30 / Failed 0 / Skipped 0.
- Phí lưu lúc giao: `GiaoThietBiHandler` ghi `loai.PhiTreMoiNgay` vào cột `chi_tiet_phieu_muon.phi_tre_moi_ngay` tại thời điểm giao (`INSERT INTO muon_tra.chi_tiet_phieu_muon ...`); cột DDL `NOT NULL` (`numeric(14,0)`). Kết quả test DB `CapNhatPhiTre`: Passed 4 / Failed 0 / Skipped 0.
- Điều kiện đầu vào cho D14-K:
  - `anHanPhut`: từ `IQuyDinhReader.LayThamSoAsync().AnHanPhut` (không null, mặc định DDL 15).
  - `phiMoiNgay`: từ `ChiTietPhieuMuonDto.PhiTreMoiNgay` (`chi_tiet_phieu_muon.phi_tre_moi_ngay`, lưu lúc giao, không null).
  - `giaTriHienHanh`: từ `ThietBiDto.GiaTriHienHanh` (không null).
  - `hanTra`: từ `PhieuMuonDto.NgayGioHenTra` (không null).
  - `NgayGioTraThucTe` có thể null (đang mượn hoặc `MAT`): không gọi `TinhPhi.SoNgayTre` với giá trị null; thiết bị `MAT` dùng nhánh bồi thường mất; thiết bị chưa trả nhưng đang tính đề xuất biên bản phải dùng giờ hiện tại từ `IClock` (không dùng `DateTime.Now`), ngày lịch dùng `VnTime`.
  - Hàm cho mốc 30 ngày (BR-61) chưa có trong `TinhPhi.cs`; làm ở D15-K (riêng `IPhieuMuonReader.LayDsQuaHan30NgayAsync` đã có ở D11-V).
- `IPhieuMuonReader` và `MuonTraDtos.cs`:
  - Trạng thái hiện thực: đã có lớp `PhieuMuonReader` trong `MuonTra.Infrastructure`, đã đăng ký DI `AddScoped<IPhieuMuonReader, PhieuMuonReader>()` trong `MuonTra/DependencyInjection.cs`, đã qua `PhieuMuonReaderTests` (4/4 Passed). Chưa kiểm chứng tích hợp từ `ViPham` gọi `IPhieuMuonReader` (thực hiện ở D14-K/D19-K).
  - Đối chiếu kiểu hợp đồng với DDL (`schema.sql`):
    - `PhieuMuonDto.MaPhieu`, `ChiTietPhieuMuonDto.MaPhieu`, `IPhieuMuonReader.LayPhieuMuonAsync(string)`, `LayChiTietTheoPhieuAsync(string)`: đã đồng bộ `string` trong D11-V (khớp `text` trong DDL).
    - `PhieuMuonDto.MaNvLap`: đã đổi tên khớp `ma_nv_lap`; đã bỏ `MaPhong` khỏi `PhieuMuonDto` (khớp `phieu_muon` không có cột phòng; khi cần phòng của thiết bị phải đọc từ `DanhMuc` qua `MaThietBi`).
    - `DangKyChoDto.MaDangKy` (`MuonTraDtos.cs:34`) và `GiuChoHetHanEvent.MaDangKy` (`GiuChoHetHanEvent.cs:4`): **vẫn là `Guid`**, lệch với `dang_ky_cho.ma_dang_ky` và `vi_pham.vi_pham.ma_dang_ky` là **`text`** trong DDL. Không chặn D14-K–D16-K, nhưng chặn D17-K nếu chưa sửa.

## 4. Danh sách lỗi nền

| Mã | Mô tả | Chủ | Mức | Ảnh hưởng | Trạng thái |
|---|---|---|---|---|---|
| DF-01 | `IPhieuMuonReader` đã được Vương hiện thực (`PhieuMuonReader`) và đăng ký DI trong D11-V (PR #59), đã qua `PhieuMuonReaderTests` (4/4 Passed); tích hợp thực tế từ module `ViPham` gọi `IPhieuMuonReader` chưa kiểm chứng | Vương / Khôi | Theo dõi | D14-K, D15-K, D16-K, D19-K | Đã có hiện thực (D11-V); chờ kiểm tích hợp từ ViPham ở D14-K/D19-K |
| DF-02 | `DangKyChoDto.MaDangKy` và `GiuChoHetHanEvent.MaDangKy` vẫn là `Guid` trong `MuonTra.Contracts`, trong khi DDL (`dang_ky_cho.ma_dang_ky`, `vi_pham.vi_pham.ma_dang_ky`) là `text` (phần `PhieuMuonDto`/`ChiTietPhieuMuonDto`/`IPhieuMuonReader` và `MaPhong`/`MaNvLap` đã được sửa ở D11-V PR #59) | Vương | Chặn D17-K | D17-K (`GiuChoHetHanEvent`) | Mở (đã giảm phạm vi từ D11-V; báo Vương sửa trước D17-K) |
| DF-03 | `XuLyLoiToanCuc` (`Api/Loi/XuLyLoiToanCuc.cs`) không ánh xạ `NghiepVuException` (trả 500 không `maLoi`); D02-V mục 3 nêu ngược lại; chưa có test | Khôi | Cao | ViPham, nhận trả, FIFO | Mở; sửa đầu D14-K (đề xuất K-59) |
| DF-04 | Sổ C0 ghi G1/G2 OPEN, handover D08-V/D09-V ghi phương án đã chọn | Khôi | Trung bình | D14-T, D15-T | Đã cập nhật ghi chú đề xuất vào sổ theo K-58 (giữ OPEN chờ nhóm xác nhận) |
| DF-05 | G3 OPEN quá hạn D02 | Lợi, Khôi, Thịnh | Cao | D13-N, D14-N, D24-T | Chờ họp nhóm |
| DF-06 | UC-12/13 có handler nhưng chưa nối endpoint (`Program.cs`), manifest vẫn `CHUA_CAI`, chưa có người nhận việc trong kế hoạch | Chờ nhóm xác nhận | Trung bình | D21-K, D28-N | Mở |
| DF-07 | `QuanLyMuonTra.ArchitectureTests` chưa có file `.cs` | Khôi | Thấp | Cổng kiến trúc | Ghi nhận |
| DF-08 | `GiuChoHetHanEvent`, `IPortGhiNhanMat`, `IFifoService` chỉ có khai báo | Vương | Theo lịch | D15-K, D17-K | Theo dõi |
| DF-09 | `frontend/web/openapi/openapi.yaml` dùng LF trong khi `backend/docs/04-api/openapi.yaml` dùng CRLF nên khác mã băm SHA-256 byte-level (nội dung giống nhau 100% khi bỏ qua CRLF/LF) | Khôi / Thịnh | Thấp | Kiểm tra đồng bộ OpenAPI theo byte | Ghi nhận |

## 5. Trạng thái G1 đến G7 sau rà soát

| Mã | Trạng thái | Ghi chú |
|---|---|---|
| G1 | OPEN; đề xuất phương án A (D08-V mục 8), chờ nhóm xác nhận | Đã ghi nhận vào sổ C0 theo K-58; giữ OPEN |
| G2 | OPEN; đề xuất phương án A (D09-V mục 3), chờ nhóm xác nhận | Đã ghi nhận vào sổ C0 theo K-58; giữ OPEN |
| G3 | OPEN, quá hạn D02 | Cần họp Lợi, Khôi, Thịnh |
| G4 | Đã chốt (K-39) | — |
| G5 | OPEN; nền là `schema.sql`, không có migration EF | `schema.sql` không đổi so với baseline (`5A334746DD053A1E2F7E4EC184D7ADC14FC1BA591FE950CF033F8780235208A1`) |
| G6 | Thuộc FE | — |
| G7 | OPEN | Giờ quầy, CORS, giới hạn tần suất auth chưa chốt |

## 6. Hợp đồng và chính sách quyền của API đã cài

- Hợp đồng được kiểm bằng `ManifestOperationTests` (mã ↔ manifest ↔ `openapi.yaml`); repo không sinh OpenAPI từ mã. Kết quả: Passed 2 / Failed 0 / Skipped 0; manifest có `38 DA_CAI` (2 health + 36 API) và `50 CHUA_CAI`.
- 36 API đã cài: chính sách ở lớp endpoint (`TaiKhoanEndpoints.cs`, `AnhEndpoints.cs`, `SinhVienEndpoints.cs`, `TraCuuEndpoints.cs`, `ChinhSachPhanQuyen.cs`) khớp bảng chính sách trong `API-Notes.md` (đọc mã):
  - 4 endpoint `POST /api/v1/auth/nhan-vien/dang-nhap`, `sinh-vien/dang-nhap`, `lam-moi`, `dang-xuat`: không yêu cầu xác thực (khớp `Công khai`).
  - `GET /api/v1/auth/toi`: `NhanVienQuanTri` (khớp).
  - 7 endpoint `/api/v1/quan-tri/nhan-vien*`: `QuanTri` (khớp).
  - `POST /api/v1/anh`: `NhanVienQuanTri` (khớp).
  - 11 endpoint `/api/v1/sv/*` (`tra-cuu/thiet-bi`, `tra-cuu/thiet-bi/{maLoai}`, `phong-thiet-bi`, `loai-thiet-bi`, `POST/GET phieu-yeu-cau-muon`, `GET phieu-yeu-cau-muon/{ma}`, `GET phieu-muon`, `GET phieu-muon/{ma}`, `GET toi`, `GET thong-bao`): `SinhVien` (khớp).
  - 12 endpoint `/api/v1/quay/*` (`GET loai-thiet-bi`, `GET phieu-yeu-cau-muon`, `GET phieu-yeu-cau-muon/{ma}/kiem-tra`, `POST phieu-yeu-cau-muon/{ma}/duyet`, `POST phieu-yeu-cau-muon/{ma}/tu-choi`, `POST phieu-muon/kiem-tra-thiet-bi`, `POST phieu-muon/kiem-tra-nguon`, `POST phieu-muon`, `GET phieu-muon`, `GET phieu-muon/qua-han-30-ngay`, `GET phieu-muon/{ma}`, `GET sinh-vien/the/{maThe}`): `NhanVien` (khớp).
- Phạm vi phòng của NV và kiểm HTTP ba vai trò cho các luồng chưa kiểm trên HTTP thật sẽ kiểm ở D21-K, không làm hôm nay.
- Các API còn `CHUA_CAI` (50 thao tác, trong đó có 10 thao tác ViPham và 24 thao tác UC-12/13) **chưa được kiểm**.

## 7. Review PR hôm trước (Khôi review Vương)

- PR đã review: Không có PR đang mở của Vương cần review tại thời điểm rà soát (PR D11-V #59 `feat(muontra): doc phieu muon, lich su va bo doc phieu muon` của `vuongtapcode` đã được `dl1803` approve và merge ngày 24/11/2026; PR D12-V chưa thấy mở; PR #62 đang mở là của `dl1803` thuộc D13-N).
- Kết luận: Không có PR cần review (bỏ qua bước 3.2 theo hướng dẫn).
- Mục không đạt theo CV mục 20: Không áp dụng (không có PR mở của Vương).

## 8. Hồ sơ tiếp nhận quyền sở hữu module ViPham

### 8.1 Hiện có

- `Domain/TinhPhi.cs` (Vương, D04-V): hàm thuần tính trễ, bồi thường hư hỏng, bồi thường mất. Từ D14 thư mục `ViPham` thuộc Khôi; Vương không sửa file trong thư mục này.
- `Infrastructure/BienBanChuaNopReader.cs` và hai provider tham chiếu (`ThamChieuDanhMucViPhamProvider.cs`, `ThamChieuKhungPhatViPhamProvider.cs`, Khôi, D03-K); `ViPhamModule.cs` đăng ký ba thành phần này.
- `QuanLyMuonTra.Modules.ViPham.csproj` đã khai báo đủ các tham chiếu Contracts (`MuonTra`, `SinhVien`, `QuyDinh`, `DanhMuc`, `ThongBao`, `TaiKhoan`, `ViPham.Contracts`, `SharedKernel`).
- `ViPham.Contracts`: hiện chỉ có `.csproj`, chưa có file mã `.cs`.
- Ánh xạ EF, handler, endpoint ViPham: chưa có; `Program.cs` chưa nạp assembly `ViPham` vào cấu hình EF.

### 8.2 Việc ViPham phải làm (theo kế hoạch)

| Ngày | Thao tác / nội dung |
|---|---|
| D14-K | Ánh xạ EF `ViPham`, `BienBanViPham`, `ChiTietBienBan` và nạp vào cấu hình EF ở `Program.cs`; `GET quay/phieu-muon/{ma}/de-xuat-bien-ban`; `GET sv/bien-ban-vi-pham`, `GET quay/bien-ban-vi-pham`, `GET quan-tri/bien-ban-vi-pham`; `GET sv/vi-pham`; thay `BienBanPhieuTruyVan.cs` trong `MuonTra` bằng reader `ViPham` thật (hoặc phối hợp qua `ViPham.Contracts`) |
| D15-K | `POST quay/bien-ban-vi-pham`; nhánh chưa trả quá 30 ngày (ghi `MAT` qua `IPortGhiNhanMat`) |
| D16-K | `POST quay/bien-ban-vi-pham/{ma}/da-nop`; `POST quay/sinh-vien/{maSV}/xac-nhan-nhan-du-thiet-bi`; `POST quan-tri/bien-ban-vi-pham/{ma}/huy`; `PUT quan-tri/bien-ban-vi-pham/{ma}/dieu-chinh` |
| D17-K | Bộ xử lý `GiuChoHetHan` (vi phạm không đến nhận), job J3 |
| D18-K, D19-K | Kiểm tương tranh, phạm vi quyền; tích hợp với phiếu thật |

### 8.3 Điều kiện vào D14-K (cổng)

- [x] DF-01: `IPhieuMuonReader` đã có hiện thực (`PhieuMuonReader`) và đăng ký DI từ D11-V (PR #59); tiếp tục kiểm tích hợp với `ViPham` ở D14-K/D19-K.
- [ ] DF-02: Phần `PhieuMuonDto`/`ChiTietPhieuMuonDto`/`IPhieuMuonReader` đã đồng bộ kiểu `string`, còn `DangKyChoDto.MaDangKy` và `GiuChoHetHanEvent.MaDangKy` là `Guid` (chưa chặn D14-K nhưng cần Vương sửa trước D17-K).
- [ ] Nhóm xác nhận K-59 (sửa DF-03 `XuLyLoiToanCuc` ánh xạ `NghiepVuException` ở đầu D14-K).
- [x] Build/test nền ở mục 2 không có lỗi mới (`dotnet build -c Release` 0 lỗi, UnitTests 515/515, ManifestOperationTests 2/2, TinhPhiTests 30/30, ViPhamThamChieuTests 6/6, CapNhatPhiTre 4/4, PhieuMuonReaderTests 4/4).

### 8.4 Quyết định đã đề xuất

K-56, K-57, K-58, K-59, K-60 (xem `C0-decision-register.md`, trạng thái Khôi đề xuất, chờ nhóm xác nhận).

