# Gói hợp đồng C2, C5, C9

Cập nhật lần đầu: 13/11/2026 · Chủ sở hữu: Nguyễn Đại Vương · Trạng thái: **hợp đồng đã khai báo trong `*.Contracts` và `SharedKernel`, chưa có bản hiện thực**

Quy ước: "Đã có trong nguồn" là điều tài liệu dự án nêu. Mọi bản giả chỉ dùng trong `tests/`, không đăng ký trên runtime. Mã C2, C5, C9 là mã artifact của kế hoạch. Mọi phương thức bất đồng bộ có hậu tố `Async` và tham số cuối `CancellationToken ct` (CV 19.1). Toàn bộ port chạy trong giao dịch và `AppDbContext` của bên gọi.

## C2 — Availability, advisory lock và thứ tự khóa chung

Để loại bỏ nguy cơ phụ thuộc vòng giữa `DanhMuc` và `MuonTra` (vì `MuonTra` đã phụ thuộc `DanhMuc`, nếu `DanhMuc` gọi C2 qua `MuonTra.Contracts` sẽ gây chu trình `DanhMuc ↔ MuonTra`), toàn bộ hợp đồng C2 được đặt tại `QuanLyMuonTra.SharedKernel.Contracts`.

| Cổng | Vị trí | Đầu vào → Đầu ra | Ranh giới giao dịch | Lỗi | Bên dùng |
|---|---|---|---|---|---|
| `IAvailabilityService.TinhKhaDungAsync` | `SharedKernel.Contracts` | `(Guid maPhong, Guid maLoai)` → `Task<int>`: `khaDung(phong, loai)` = số `thiet_bi` `SAN_SANG` và `ngung_su_dung = false`, trừ tổng `so_luong` của phiếu yêu cầu `DA_DUYET` có `han_giu_cho > now`, trừ số `dang_ky_cho` `DA_THONG_BAO` có `han_giu_cho > now`. Bất biến `khaDung ≥ 0` | Trong giao dịch, sau khi đã giữ khóa tư vấn, tính lại ngay trước khi ghi | `SO_LUONG_KHA_DUNG_THAY_DOI` (409) khi duyệt/gửi; `THIET_BI_DA_DUOC_GIAO` (409) | MuonTra, DanhMuc, BaoTri |
| `IAdvisoryLockService.LockPhongLoaiAsync` | `SharedKernel.Contracts` | `(Guid maPhong, Guid maLoai)` → `Task`: Lấy `pg_advisory_xact_lock` theo cặp (phòng, loại) | Sau `BEGIN` và sau khóa dòng `sinh_vien` (nếu có), trước bước kiểm tra điều kiện | Lỗi giao dịch làm hoàn tác | MuonTra, DanhMuc, BaoTri |
| `IAdvisoryLockService.LockDanhSachPhongLoaiAsync` | `SharedKernel.Contracts` | `IEnumerable<(Guid, Guid)>` → `Task`: Sắp xếp tăng dần theo cặp `(MaPhong, MaLoai)` trước khi lấy khóa để chống deadlock | Cùng giao dịch | Lỗi giao dịch làm hoàn tác | MuonTra, DanhMuc |
| `IBoTinhHanTra.TinhHanTra` | `SharedKernel.Contracts` | `(DateTimeOffset gioGiao, int thoiGianMuonGio, TimeOnly? gioDongCua)` → `DateTimeOffset`: `min(gioGiao + thoiGianMuon, gioDongCuaCungNgay)`. Tách độc lập khỏi Fee Engine của ViPham vì MuonTra không tham chiếu ViPham | Hàm thuần, không giao dịch | `GIAO_SAU_GIO_DONG_CUA` (422), `CHUA_CAU_HINH_GIO_QUAY` (409) | MuonTra |

Thao tác bắt buộc đi qua C2 (danh sách của ĐX-KT-13, KT 8.2):

| Thao tác làm giảm khả dụng | UC | Module gọi | Người cài |
|---|---|---|---|
| Duyệt phiếu yêu cầu | UC-06 | MuonTra | Vương (D07) |
| Giao trực tiếp; giao từ giữ chỗ của phiếu; giao từ giữ chỗ của đăng ký chờ | UC-07, UC-10 | MuonTra | Vương (D09) |
| Giữ chỗ cho SV đứng đầu danh sách chờ khi thiết bị về `SAN_SANG` và chuyển lượt kế tiếp | UC-10 | MuonTra | Vương (D13, D18) |
| Lập lịch bảo trì | UC-11 | BaoTri | Lợi (D13-N) |
| Ngưng sử dụng, thanh lý, xóa cứng thiết bị đang `SAN_SANG` | UC-12, UC-15 | DanhMuc, BaoTri | Lợi (D08-N, D16-N) |
| Sửa thiết bị làm đổi phòng hoặc loại | UC-12 | DanhMuc | Lợi |

## C5 — MuonTra reader tối thiểu, reader phiếu đầy đủ, provider tham chiếu và port MAT

| Cổng | Vị trí | Đầu vào → Đầu ra | Ranh giới giao dịch | Lỗi | Bên dùng |
|---|---|---|---|---|---|
| `IMuonTraReader.DemThietBiDangCamAsync` | `MuonTra.Contracts` | `string maSv` → `Task<int>`: Đếm số thiết bị đang cầm (`ket_qua_kiem_tra IS NULL`). Module `SinhVien` không tham chiếu trực tiếp `MuonTra.Contracts` để tránh chu trình; việc ghép thông tin thẻ SV do tầng Host/Api thực hiện | Chỉ đọc | Không ném; trả 0 nếu không có | Host, MuonTra |
| `IThamChieuMuonTraProvider` | `MuonTra.Contracts` | `CoThamChieuPhongAsync`, `CoThamChieuLoaiAsync`, `CoThamChieuThietBiAsync`, `CoThamChieuKhungPhatAsync` → `Task<bool>`: Hiện thực cổng kiểm tra tham chiếu cho `DanhMuc` và `QuyDinh` | Chỉ đọc | `DA_CO_LICH_SU_CHI_NGUNG_SU_DUNG` (409), `KHUNG_PHAT_DA_DUNG_KHONG_XOA` (409) | DanhMuc, QuyDinh |
| `IPhieuMuonReader.LayPhieuMuonAsync`, `LayChiTietAsync`, `LayChiTietTheoPhieuAsync`, `LayDsQuaHan30NgayAsync` | `MuonTra.Contracts` | Đọc phiếu mượn, chi tiết phiếu mượn và danh sách phiếu quá hạn 30 ngày (UC-09 bước 1) | Chỉ đọc | `KHONG_TIM_THAY` (404), `KHONG_DU_QUYEN` (403) | ViPham, MuonTra |
| `IPortGhiNhanMat.GhiNhanMatAsync` | `MuonTra.Contracts` | `(Guid maChiTietPhieu, Guid maNvNhanTra)` → `Task<bool>`: Đánh dấu `ket_qua_kiem_tra = 'MAT'`, chuyển thiết bị sang `DA_BAO_MAT` | Cùng giao dịch lập biên bản mất | `TRANG_THAI_KHONG_HOP_LE` (409) | ViPham |
| Bộ tính phí (Fee Engine) | `ViPham/Domain` | `SoNgayTre(hanTra, thoiDiem, anHanPhut)` → `int`; `PhiTre(soNgayTre, phiMoiNgay, giaTriHienHanh)` → `decimal`. Công thức bồi thường hư hỏng và mất 100% | Hàm thuần | Không ném lỗi | ViPham (Vương bàn giao Khôi D14) |

## C9 — Cấp lượt FIFO cùng giao dịch

| Cổng | Vị trí | Đầu vào → Đầu ra | Ranh giới giao dịch | Lỗi | Bên dùng |
|---|---|---|---|---|---|
| `IFifoService.CapLuotChoAsync` | `MuonTra.Contracts` | `(Guid maPhong, Guid maLoai)` → `Task<DangKyChoDto?>`: Khi thiết bị về `SAN_SANG`, cấp giữ chỗ 30 phút cho SV đứng đầu hàng chờ `DANG_CHO`, chuyển sang `DA_THONG_BAO` | Cùng giao dịch của thao tác kích hoạt, sau khóa tư vấn C2 | Lỗi làm hoàn tác toàn bộ giao dịch kích hoạt | MuonTra, BaoTri |
| Ranh giới J4 | `MuonTra` | Job J4 quét định kỳ các đăng ký `DA_THONG_BAO` có `han_giu_cho < now`, chuyển `HET_HAN`, phát sự kiện `GiuChoHetHanEvent` và gọi `IFifoService` chuyển lượt | Giao dịch riêng của job | Không ném | Job nền J4 |
| `GiuChoHetHanEvent` | `MuonTra.Contracts` | Record: `(Guid MaDangKy, string MaSv, Guid MaPhong, Guid MaLoai, DateTimeOffset ThoiDiemHetHan)`. Phát trước khi commit cùng giao dịch để `ViPham` ghi vi phạm `KHONG_DEN_NHAN` | Cùng giao dịch | Không ném | ViPham (gói C10) |

## Ngoài phạm vi D01-V

`IThietBiService`, `IThamChieuDanhMucProvider`, `IThamChieuKhungPhatProvider` thuộc gói C3 (Lợi). Reader và quyền phòng, reader nợ, `IQuyenMuonService` thuộc gói C4 (Khôi). Outbox, runner job, bộ nhận `GiuChoHetHan` thuộc C7, C8, C10 (Khôi).
