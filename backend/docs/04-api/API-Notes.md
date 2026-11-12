# API-Notes: Hệ thống quản lý mượn trả thiết bị học tập cho sinh viên

> **Dự án:** SE100 Nhóm 13. **Phạm vi:** Giai đoạn 1 (UC-01, 02, 03, 05, 06, 07, 08, 09, 10, 11, 12, 13, 14, 15, 16, 19).
> **Phiên bản:** 1.1.1. **Đi kèm:** `openapi.yaml` (OpenAPI 3.0.3, 88 thao tác). Tài liệu này giải thích những gì `openapi.yaml` không thể hiện gọn: luồng gọi, máy trạng thái, công thức, quy tắc lỗi, các quyết định thiết kế API và các điểm nguồn chưa nêu.
> **Nguồn:** `backend-architecture.md` (KT), `Backend-Conventions.md` (CV), `Authorization.md`, `Usecase.md`, `BusinessRule.md` (BR), `SRS.md`, `DatabaseDesign.md`, `schema.sql`.
> **Nguyên tắc:** mọi endpoint, mã lỗi và quy tắc trong tài liệu này đều lấy từ nguồn. Chỗ nguồn không nêu được ghi thành quyết định **ĐX-API** (mục 11), **đã được nhóm duyệt**; các tài liệu nguồn cần cập nhật theo nằm ở mục 12.

## 1. Mục đích và cách dùng

| Đối tượng | Dùng tài liệu này để |
|---|---|
| Nhóm backend (ASP.NET Core) | Đối chiếu từng endpoint với nghiệp vụ; viết handler đúng mã trạng thái và `maLoi`; kiểm tra `openapi.json` sinh từ mã khớp hợp đồng. |
| Nhóm Android | Sinh client từ `openapi.yaml`; biết endpoint nào gọi theo thứ tự nào; xử lý 401, 409, 422 đúng cách (mục 5, 10, 13). |
| Nhóm kiểm thử | Lấy ca kiểm thử từ bảng mã lỗi, máy trạng thái và luồng nghiệp vụ. |

`openapi.yaml` là nguồn chuẩn về **hình dạng** yêu cầu và phản hồi (kiểu, trường bắt buộc, enum, ràng buộc). Tài liệu này là nguồn chuẩn về **ý nghĩa nghiệp vụ**. Hai tài liệu không được mâu thuẫn; nếu mâu thuẫn thì xử lý theo `backend-architecture.md` mục 1.4: hợp đồng API thuộc `openapi.yaml`, còn KT 6 và `Backend-Conventions.md` 6 chỉ tóm lược và phải cập nhật theo hợp đồng.

## 2. Quy ước chung

| Nội dung | Quy ước | Nguồn |
|---|---|---|
| Kiểu | REST over HTTPS, JSON (`application/json`), lỗi `application/problem+json` | KT 6.1 |
| Tiền tố | `/api/v1`; riêng `/health`, `/health/ready` nằm ngoài tiền tố | KT 6.1, 14 |
| Khu vực | `/auth` (công khai và phiên), `/sv` (sinh viên), `/quay` (nhân viên), `/quan-tri` (quản trị viên), `/anh` (tải ảnh, NV và QT) | KT 6.1 |
| Đặt tên đường dẫn | Tên miền không dấu, kebab-case; hành động là động từ kebab-case sau mã: `POST /{tai-nguyen}/{ma}/{hanh-dong}` | CV 6.3 |
| Tên thuộc tính JSON | camelCase theo hồ sơ phân tích (`maThietBi`, `ngayGioHenTra`); viết tắt `SV`, `NV`, `QT`, `NCC` giữ chữ hoa (`maSV`, `maNVThucHien`, `maQTLap`, `maNCC`) | KT 6.1, ĐX-API-04 |
| Giá trị enum | Chuỗi `SCREAMING_SNAKE_CASE` không dấu, trùng giá trị CHECK trong `schema.sql` | CV 3.3 |
| Thời gian | ISO 8601 có múi giờ, UTC (`2026-10-27T03:00:00Z`); client tự hiển thị giờ Việt Nam. "Ngày lịch" tính theo `Asia/Ho_Chi_Minh` | KT 6.1, 8.5 |
| Giờ quầy | `gioMoCua`, `gioDongCua` dạng `HH:mm[:ss]`, giờ Việt Nam | KT 8.5, DM-07 |
| Tiền | Số nguyên VNĐ (`int64`) | KT 6.1 |
| Phân trang | `?trang=1&kichThuoc=20`; `kichThuoc` 1 đến 100, mặc định 20; phản hồi `{ "tongSo", "trang", "kichThuoc", "danhSach" }`; sai khoảng trả 400 | KT 6.1 |
| Phạm vi dữ liệu của SV | Mã SV lấy từ claim `sub`, **không bao giờ** từ đường dẫn hay body; dữ liệu người khác trả 403 | BR-06 |
| Phạm vi phòng của NV | Danh sách tự lọc theo phòng được cấp quyền; từng đối tượng thuộc phòng chưa cấp trả 403 `PHONG_CHUA_CAP_QUYEN` | KT 6.1 |
| Mã thành công | Tạo chứng từ: 201; hành động trên đối tượng có sẵn: 200; xóa, đăng xuất, đặt lại mật khẩu: 204 | CV 6.5, ĐX-API-03 |
| Lỗi | RFC 9457, thêm `maLoi` và `traceId`; 400 có `errors` (từ khóa trường → thông báo); 500 chỉ có `traceId` | KT 6.1, 6.6 |
| Tương tranh | Concurrency token `xmin` trên các aggregate; va chạm trả 409 `XUNG_DOT_CAP_NHAT` | KT 8 |
| Giới hạn tần suất | Chỉ `/auth/*`, vượt trả 429 `QUA_NHIEU_YEU_CAU` | KT 7.4 |
| Idempotency-Key | Không dùng; chống trùng dựa vào máy trạng thái và ràng buộc CSDL | KT 8 |
| Phương thức | Chỉ `GET`, `POST`, `PUT`, và ba `DELETE` (ĐX-CV-09); không `PATCH` | CV 6.3 |
| Sắp xếp danh sách | Mỗi danh sách ghi thứ tự mặc định trong mô tả endpoint; khóa phụ luôn là mã tăng dần để phân trang ổn định | ĐX-API-28 |
| Mật khẩu nhân viên, quản trị viên | Tối thiểu 8 ký tự khi tạo hoặc đặt lại (400 nếu ngắn hơn); đăng nhập không kiểm tra độ dài | ĐX-API-27 |

## 3. Xác thực, phiên và phân quyền

### 3.1 Token

| Thành phần | Quy tắc | Nguồn |
|---|---|---|
| Access token | JWT HS256, hạn 15 phút, claim `sub` (mã người dùng hoặc `maSV`), `role`, `jti`; lệch đồng hồ cho phép 30 giây | KT 7.3 |
| Refresh token | Ngẫu nhiên, hạn 7 ngày, chỉ lưu giá trị băm; **xoay vòng** mỗi lần `lam-moi`; dùng lại token đã xoay vòng thì thu hồi cả chuỗi | KT 7.3 |
| Gửi token | Header `Authorization: Bearer <accessToken>`; refresh token gửi trong body (không dùng cookie) | KT 7.3 |
| Đăng xuất | `POST /auth/dang-xuat` với refresh token trong body, không cần access token còn hạn | KT 6.2, ĐX-CV-08 |
| Tài khoản NV/QT bị khóa | Trạng thái được đọc ở **mỗi request**: token còn hạn vẫn bị từ chối ngay (403 `TAI_KHOAN_BI_KHOA`) | KT 7.2 |
| Sinh viên | Xác thực bằng MSSV và mật khẩu của trường qua `ISchoolDirectory`; hệ thống không lưu mật khẩu; không có trạng thái khóa tài khoản (ĐX-PQ-05) | KT 7.1 |
| Nhân viên, quản trị viên | Một endpoint đăng nhập chung; nhập sai đạt `soLanDangNhapSaiToiDa` thì `TAM_KHOA_DANG_NHAP`; đăng nhập đúng đặt lại bộ đếm | KT 7.4, BR-10 |

### 3.2 Chính sách theo nhóm endpoint

| Khu vực | Chính sách | Ghi chú |
|---|---|---|
| `/auth/*/dang-nhap`, `/auth/lam-moi`, `/auth/dang-xuat` | Không (ngoại lệ NFR-01) | Có giới hạn tần suất |
| `/auth/toi` | `NhanVienQuanTri` | Sinh viên không dùng |
| `/sv/*` | `SinhVien` | Mã SV từ token |
| `/quay/*` | `NhanVien` | Quản trị viên **không có** endpoint nghiệp vụ quầy (ĐX-PQ-02) |
| `/anh` | `NhanVienQuanTri` | |
| `/quan-tri/*` | `QuanTri` | |
| `/health`, `/health/ready` | Không (ngoại lệ NFR-01) | `/health/ready` kiểm tra kết nối CSDL |

### 3.3 Quyền mượn bị khóa (ĐX-PQ-08)

Khi `trangThaiQuyenMuon` là `TAM_KHOA` hoặc `CAM_MUON_DAI_HAN`:

| Bị chặn (422 `SV_BI_KHOA_QUYEN_MUON`) | Không bị chặn |
|---|---|
| `POST /sv/phieu-yeu-cau-muon` | Tra cứu, xem lịch sử, xem biên bản, xem thông báo |
| `POST /sv/dang-ky-cho` | `POST /sv/dang-ky-cho/{ma}/roi` |
| `POST /quay/phieu-muon` (cả ba nguồn: phiếu yêu cầu, danh sách chờ, mượn trực tiếp) | `POST /quay/phieu-muon/{ma}/ket-qua-kiem-tra` (nhận trả tại quầy) |
| Duyệt phiếu yêu cầu (kiểm tra lại điều kiện quyền mượn, SSD-02): `POST /quay/phieu-yeu-cau-muon/{ma}/duyet` | |

## 4. Danh mục endpoint (88 thao tác)

Bảng sinh trực tiếp từ `openapi.yaml`; số trong ngoặc là mã HTTP thành công.

### 4.1 Xác thực

| STT | Method | Đường dẫn | Chính sách | UC / SSD | operationId | Mô tả |
|---|---|---|---|---|---|---|
| 1 | POST | `/api/v1/auth/sinh-vien/dang-nhap` | `Công khai` | UC-19 | `authDangNhapSinhVien` | Đăng nhập sinh viên (200) |
| 2 | POST | `/api/v1/auth/nhan-vien/dang-nhap` | `Công khai` | UC-19 | `authDangNhapNhanVien` | Đăng nhập nhân viên hoặc quản trị viên (200) |
| 3 | POST | `/api/v1/auth/lam-moi` | `Công khai` | UC-19 | `authLamMoi` | Làm mới token (200) |
| 4 | POST | `/api/v1/auth/dang-xuat` | `Công khai` | UC-19 | `authDangXuat` | Đăng xuất (204) |
| 5 | GET | `/api/v1/auth/toi` | `NhanVienQuanTri` | UC-19 | `authToi` | Thông tin người đăng nhập (200) |

### 4.2 Ảnh

| STT | Method | Đường dẫn | Chính sách | UC / SSD | operationId | Mô tả |
|---|---|---|---|---|---|---|
| 6 | POST | `/api/v1/anh` | `NhanVienQuanTri` | UC-07, UC-08, UC-12 | `anhTai` | Tải ảnh lên (201) |

### 4.3 Sinh viên

| STT | Method | Đường dẫn | Chính sách | UC / SSD | operationId | Mô tả |
|---|---|---|---|---|---|---|
| 7 | GET | `/api/v1/sv/tra-cuu/thiet-bi` | `SinhVien` | UC-01 | `svTraCuuThietBi` | Tra cứu thiết bị (200) |
| 8 | GET | `/api/v1/sv/tra-cuu/thiet-bi/{maLoai}` | `SinhVien` | UC-01 | `svTraCuuThietBiChiTiet` | Chi tiết loại thiết bị tại một phòng (200) |
| 9 | GET | `/api/v1/sv/phieu-yeu-cau-muon` | `SinhVien` | UC-02 | `svDsPhieuYeuCauMuon` | Danh sách phiếu yêu cầu của tôi (200) |
| 10 | POST | `/api/v1/sv/phieu-yeu-cau-muon` | `SinhVien` | UC-02, SSD-01 | `svGuiPhieuYeuCauMuon` | Gửi phiếu yêu cầu mượn (đặt trước) (201) |
| 11 | GET | `/api/v1/sv/phieu-yeu-cau-muon/{ma}` | `SinhVien` | UC-02 | `svChiTietPhieuYeuCauMuon` | Chi tiết phiếu yêu cầu của tôi (200) |
| 12 | GET | `/api/v1/sv/dang-ky-cho` | `SinhVien` | UC-03 | `svDsDangKyCho` | Xem số thứ tự chờ của tôi (200) |
| 13 | POST | `/api/v1/sv/dang-ky-cho` | `SinhVien` | UC-03 | `svDangKyCho` | Đăng ký chờ (201) |
| 14 | POST | `/api/v1/sv/dang-ky-cho/{ma}/roi` | `SinhVien` | UC-03 | `svRoiDangKyCho` | Rời danh sách chờ (200) |
| 15 | GET | `/api/v1/sv/phieu-muon` | `SinhVien` | UC-05 | `svDsPhieuMuon` | Lịch sử mượn/trả của tôi (200) |
| 16 | GET | `/api/v1/sv/phieu-muon/{ma}` | `SinhVien` | UC-05 | `svChiTietPhieuMuon` | Chi tiết phiếu mượn của tôi (200) |
| 17 | GET | `/api/v1/sv/bien-ban-vi-pham` | `SinhVien` | UC-05 | `svDsBienBanViPham` | Biên bản vi phạm của tôi (200) |
| 18 | GET | `/api/v1/sv/thong-bao` | `SinhVien` | FR-24 | `svDsThongBao` | Thông báo của tôi (200) |
| 19 | GET | `/api/v1/sv/toi` | `SinhVien` | UC-17, UC-01 | `svToi` | Hồ sơ mượn của tôi (200) |
| 20 | GET | `/api/v1/sv/vi-pham` | `SinhVien` | UC-05 | `svDsViPham` | Vi phạm của tôi (200) |
| 21 | GET | `/api/v1/sv/phong-thiet-bi` | `SinhVien` | UC-01, UC-02 | `svDsPhongThietBi` | Danh sách phòng thiết bị (200) |
| 22 | GET | `/api/v1/sv/loai-thiet-bi` | `SinhVien` | UC-01 | `svDsLoaiThietBi` | Danh sách loại thiết bị (200) |

### 4.4 Nhân viên (quầy)

| STT | Method | Đường dẫn | Chính sách | UC / SSD | operationId | Mô tả |
|---|---|---|---|---|---|---|
| 23 | GET | `/api/v1/quay/phieu-yeu-cau-muon` | `NhanVien` | UC-06, SSD-02 layPhieuChoDuyet | `quayDsPhieuYeuCauMuon` | Danh sách phiếu yêu cầu của phòng mình (200) |
| 24 | GET | `/api/v1/quay/phieu-yeu-cau-muon/{ma}/kiem-tra` | `NhanVien` | UC-06, SSD-02 kiemTraPhieu | `quayKiemTraPhieuYeuCauMuon` | Kiểm tra lại điều kiện trước khi duyệt (200) |
| 25 | POST | `/api/v1/quay/phieu-yeu-cau-muon/{ma}/duyet` | `NhanVien` | UC-06, SSD-02 duyet | `quayDuyetPhieuYeuCauMuon` | Duyệt phiếu yêu cầu và giữ chỗ (200) |
| 26 | POST | `/api/v1/quay/phieu-yeu-cau-muon/{ma}/tu-choi` | `NhanVien` | UC-06, SSD-03 tuChoi | `quayTuChoiPhieuYeuCauMuon` | Từ chối phiếu yêu cầu (200) |
| 27 | GET | `/api/v1/quay/khung-phat` | `NhanVien` | UC-08, UC-09 | `quayDsKhungPhat` | Danh sách khung phạt đang dùng (200) |
| 28 | GET | `/api/v1/quay/loai-thiet-bi` | `NhanVien` | UC-07 | `quayDsLoaiThietBi` | Loại thiết bị và số lượng khả dụng tại phòng mình (200) |
| 29 | GET | `/api/v1/quay/phieu-muon` | `NhanVien` | UC-08, UC-09 | `quayDsPhieuMuon` | Tìm phiếu mượn của phòng mình (200) |
| 30 | POST | `/api/v1/quay/phieu-muon` | `NhanVien` | UC-07, SSD ghiTinhTrangBanDau + lapPhieuMuon | `quayLapPhieuMuon` | Giao thiết bị và lập phiếu mượn (nguyên tử) (201) |
| 31 | GET | `/api/v1/quay/phieu-muon/{ma}` | `NhanVien` | UC-08, UC-09 | `quayChiTietPhieuMuon` | Chi tiết phiếu mượn (200) |
| 32 | GET | `/api/v1/quay/bien-ban-vi-pham` | `NhanVien` | UC-09 | `quayDsBienBanViPham` | Tìm biên bản vi phạm của phòng mình (200) |
| 33 | POST | `/api/v1/quay/bien-ban-vi-pham` | `NhanVien` | UC-09, UC-09 A3 | `quayLapBienBanViPham` | Lập biên bản vi phạm (201) |
| 34 | GET | `/api/v1/quay/sinh-vien/the/{maThe}` | `NhanVien` | UC-07, SSD-04 layThongTinSV | `quayLayThongTinSinhVien` | Quét thẻ sinh viên (200) |
| 35 | POST | `/api/v1/quay/phieu-muon/kiem-tra-nguon` | `NhanVien` | UC-07, SSD-04, SSD-05, SSD-06 | `quayKiemTraNguonGiao` | Kiểm tra nguồn giao thiết bị (200) |
| 36 | POST | `/api/v1/quay/phieu-muon/kiem-tra-thiet-bi` | `NhanVien` | UC-07, SSD themThietBi | `quayKiemTraThietBiGiao` | Kiểm tra một thiết bị trước khi giao (200) |
| 37 | GET | `/api/v1/quay/thiet-bi/{maThietBi}/ban-giao` | `NhanVien` | UC-08, SSD-07 nhanThietBi | `quayBanGiaoThietBi` | Xem tình trạng và ảnh lúc giao (200) |
| 38 | POST | `/api/v1/quay/phieu-muon/{ma}/ket-qua-kiem-tra` | `NhanVien` | UC-08, SSD-07 ghiKetQuaKiemTra | `quayGhiKetQuaKiemTra` | Ghi kết quả kiểm tra khi nhận trả (200) |
| 39 | GET | `/api/v1/quay/phieu-muon/{ma}/de-xuat-bien-ban` | `NhanVien` | UC-09 | `quayDeXuatBienBan` | Đề xuất các khoản của biên bản vi phạm (200) |
| 40 | POST | `/api/v1/quay/bien-ban-vi-pham/{ma}/da-nop` | `NhanVien` | UC-09 | `quayGhiNhanDaNop` | Ghi nhận biên bản đã nộp (200) |
| 41 | POST | `/api/v1/quay/sinh-vien/{maSV}/xac-nhan-nhan-du-thiet-bi` | `NhanVien` | UC-09 | `quayXacNhanNhanDuThietBi` | Xác nhận đã nhận đủ thiết bị và xét mở khóa (200) |
| 42 | GET | `/api/v1/quay/phieu-muon/qua-han-30-ngay` | `NhanVien` | UC-09 A3 | `quayDsPhieuQuaHan30Ngay` | Phiếu chưa trả quá 30 ngày (200) |
| 43 | GET | `/api/v1/quay/danh-sach-cho` | `NhanVien` | UC-10 | `quayDsDanhSachCho` | Xem danh sách chờ (200) |
| 44 | POST | `/api/v1/quay/danh-sach-cho/{ma}/chuyen-luot` | `NhanVien` | UC-10 A2 | `quayChuyenLuotDanhSachCho` | Chuyển lượt thủ công (200) |
| 45 | GET | `/api/v1/quay/bao-tri/den-han` | `NhanVien` | UC-11 | `quayDsThietBiDenHanBaoTri` | Thiết bị đến hạn bảo trì (200) |
| 46 | GET | `/api/v1/quay/bao-tri/cho-tiep-nhan-sua-chua` | `NhanVien` | UC-11 | `quayDsChoTiepNhanSuaChua` | Thiết bị chờ tiếp nhận sửa chữa (200) |
| 47 | POST | `/api/v1/quay/bao-tri` | `NhanVien` | UC-11 | `quayLapBaoTri` | Lập lịch bảo trì hoặc tiếp nhận sửa chữa (201) |
| 48 | POST | `/api/v1/quay/bao-tri/{ma}/hoan-thanh` | `NhanVien` | UC-11 | `quayHoanThanhBaoTri` | Hoàn thành bảo trì/sửa chữa (200) |

### 4.5 Quản trị viên

| STT | Method | Đường dẫn | Chính sách | UC / SSD | operationId | Mô tả |
|---|---|---|---|---|---|---|
| 49 | GET | `/api/v1/quan-tri/phong-thiet-bi` | `QuanTri` | UC-12 | `qtDsPhongThietBi` | Danh sách phòng thiết bị (200) |
| 50 | POST | `/api/v1/quan-tri/phong-thiet-bi` | `QuanTri` | UC-12 | `qtTaoPhongThietBi` | Thêm phòng thiết bị (201) |
| 51 | PUT | `/api/v1/quan-tri/phong-thiet-bi/{ma}` | `QuanTri` | UC-12 | `qtSuaPhongThietBi` | Sửa phòng thiết bị (200) |
| 52 | GET | `/api/v1/quan-tri/loai-thiet-bi` | `QuanTri` | UC-12 | `qtDsLoaiThietBi` | Danh sách loại thiết bị (200) |
| 53 | POST | `/api/v1/quan-tri/loai-thiet-bi` | `QuanTri` | UC-12 | `qtTaoLoaiThietBi` | Thêm loại thiết bị (201) |
| 54 | PUT | `/api/v1/quan-tri/loai-thiet-bi/{ma}` | `QuanTri` | UC-12 | `qtSuaLoaiThietBi` | Sửa loại thiết bị (200) |
| 55 | DELETE | `/api/v1/quan-tri/loai-thiet-bi/{ma}` | `QuanTri` | UC-12 E2 | `qtXoaLoaiThietBi` | Xóa cứng loại thiết bị (204) |
| 56 | POST | `/api/v1/quan-tri/loai-thiet-bi/{ma}/ngung-su-dung` | `QuanTri` | UC-12 E2 | `qtNgungSuDungLoaiThietBi` | Ngưng sử dụng loại thiết bị (200) |
| 57 | GET | `/api/v1/quan-tri/thiet-bi` | `QuanTri` | UC-12 | `qtDsThietBi` | Danh sách thiết bị (200) |
| 58 | POST | `/api/v1/quan-tri/thiet-bi` | `QuanTri` | UC-12 | `qtTaoThietBi` | Thêm thiết bị (nhập kho) (201) |
| 59 | PUT | `/api/v1/quan-tri/thiet-bi/{ma}` | `QuanTri` | UC-12 | `qtSuaThietBi` | Sửa thiết bị (200) |
| 60 | DELETE | `/api/v1/quan-tri/thiet-bi/{ma}` | `QuanTri` | UC-12 E2 | `qtXoaThietBi` | Xóa cứng thiết bị (204) |
| 61 | POST | `/api/v1/quan-tri/thiet-bi/{ma}/ngung-su-dung` | `QuanTri` | UC-12 E2 | `qtNgungSuDungThietBi` | Ngưng sử dụng thiết bị (200) |
| 62 | PUT | `/api/v1/quan-tri/thiet-bi/{ma}/gia-tri` | `QuanTri` | UC-12 bước 5 | `qtCapNhatGiaTriThietBi` | Cập nhật giá trị thiết bị (200) |
| 63 | GET | `/api/v1/quan-tri/nha-cung-cap` | `QuanTri` | UC-12 | `qtDsNhaCungCap` | Danh sách nhà cung cấp (200) |
| 64 | GET | `/api/v1/quan-tri/cau-hinh/quy-dinh-muon` | `QuanTri` | UC-13 | `qtLayQuyDinhMuon` | Xem quy định mượn theo nhóm (200) |
| 65 | PUT | `/api/v1/quan-tri/cau-hinh/quy-dinh-muon` | `QuanTri` | UC-13 | `qtCapNhatQuyDinhMuon` | Cập nhật quy định mượn của một nhóm (200) |
| 66 | GET | `/api/v1/quan-tri/cau-hinh/tham-so` | `QuanTri` | UC-13 | `qtLayThamSo` | Xem tham số hệ thống (200) |
| 67 | PUT | `/api/v1/quan-tri/cau-hinh/tham-so` | `QuanTri` | UC-13 | `qtCapNhatThamSo` | Cập nhật tham số hệ thống (200) |
| 68 | GET | `/api/v1/quan-tri/cau-hinh/khung-phat` | `QuanTri` | UC-13 | `qtDsKhungPhat` | Danh sách khung phạt (200) |
| 69 | POST | `/api/v1/quan-tri/cau-hinh/khung-phat` | `QuanTri` | UC-13 A1 | `qtTaoKhungPhat` | Thêm khung phạt (201) |
| 70 | PUT | `/api/v1/quan-tri/cau-hinh/khung-phat/{ma}` | `QuanTri` | UC-13 | `qtSuaKhungPhat` | Sửa khung phạt (200) |
| 71 | DELETE | `/api/v1/quan-tri/cau-hinh/khung-phat/{ma}` | `QuanTri` | UC-13 E2 | `qtXoaKhungPhat` | Xóa cứng khung phạt (204) |
| 72 | POST | `/api/v1/quan-tri/cau-hinh/khung-phat/{ma}/tat` | `QuanTri` | UC-13 E2 | `qtTatKhungPhat` | Tắt khung phạt (200) |
| 73 | PUT | `/api/v1/quan-tri/cau-hinh/phi-tre/{maLoai}` | `QuanTri` | UC-13 | `qtCapNhatPhiTre` | Cập nhật phí trả trễ theo loại (200) |
| 74 | GET | `/api/v1/quan-tri/nhan-vien` | `QuanTri` | UC-14 | `qtDsNhanVien` | Danh sách nhân viên (200) |
| 75 | POST | `/api/v1/quan-tri/nhan-vien` | `QuanTri` | UC-14 | `qtTaoNhanVien` | Tạo tài khoản nhân viên (201) |
| 76 | PUT | `/api/v1/quan-tri/nhan-vien/{ma}` | `QuanTri` | UC-14 | `qtSuaNhanVien` | Sửa tài khoản nhân viên (200) |
| 77 | POST | `/api/v1/quan-tri/nhan-vien/{ma}/khoa` | `QuanTri` | UC-14 | `qtKhoaNhanVien` | Khóa tài khoản nhân viên (200) |
| 78 | POST | `/api/v1/quan-tri/nhan-vien/{ma}/mo-khoa` | `QuanTri` | UC-14 | `qtMoKhoaNhanVien` | Mở khóa tài khoản nhân viên (200) |
| 79 | PUT | `/api/v1/quan-tri/nhan-vien/{ma}/quyen-phong` | `QuanTri` | UC-14 | `qtCapNhatQuyenPhong` | Cấp hoặc thu hồi quyền theo phòng (200) |
| 80 | POST | `/api/v1/quan-tri/nhan-vien/{ma}/dat-lai-mat-khau` | `QuanTri` | UC-14 | `qtDatLaiMatKhauNhanVien` | Đặt lại mật khẩu nhân viên (204) |
| 81 | GET | `/api/v1/quan-tri/bien-ban-vi-pham` | `QuanTri` | UC-09 A2 | `qtDsBienBanViPham` | Danh sách biên bản vi phạm (200) |
| 82 | POST | `/api/v1/quan-tri/bien-ban-vi-pham/{ma}/huy` | `QuanTri` | UC-09 A2 | `qtHuyBienBanViPham` | Hủy biên bản vi phạm (200) |
| 83 | PUT | `/api/v1/quan-tri/bien-ban-vi-pham/{ma}/dieu-chinh` | `QuanTri` | UC-09 A2 | `qtDieuChinhBienBanViPham` | Điều chỉnh biên bản vi phạm (200) |
| 84 | GET | `/api/v1/quan-tri/thanh-ly/cho-quyet-dinh` | `QuanTri` | UC-15 | `qtDsChoQuyetDinhThanhLy` | Thiết bị chờ quyết định thanh lý (200) |
| 85 | POST | `/api/v1/quan-tri/quyet-dinh-thanh-ly` | `QuanTri` | UC-15 | `qtLapQuyetDinhThanhLy` | Lập quyết định thanh lý (201) |
| 86 | GET | `/api/v1/quan-tri/bao-cao/{loai}` | `QuanTri` | UC-16 | `qtBaoCao` | Xem và xuất báo cáo (200) |

### 4.6 Hạ tầng

| STT | Method | Đường dẫn | Chính sách | UC / SSD | operationId | Mô tả |
|---|---|---|---|---|---|---|
| 87 | GET | `/health` | `Công khai` | - | `healthSong` | Kiểm tra dịch vụ còn sống (200) |
| 88 | GET | `/health/ready` | `Công khai` | - | `healthSanSang` | Kiểm tra kết nối CSDL (200) |

Ghi chú diễn giải danh sách nguồn:

- KT 6.5 ghi gộp "GET, POST, PUT `/phong-thiet-bi`, `/loai-thiet-bi`, `/thiet-bi`, `/nhan-vien`". Tài liệu này hiểu là `GET` danh sách, `POST` vào tập, `PUT /{ma}`; danh mục **không** có `GET /{ma}` (ĐX-API-20); danh sách có bộ lọc đủ để tra cứu một đối tượng.
- Mười endpoint `GET` đọc bổ sung (ĐX-API-25) không có trong KT 6.2 đến 6.5 gốc nhưng cần để luồng nghiệp vụ chạy trọn: `/sv/toi`, `/sv/vi-pham`, `/sv/phong-thiet-bi`, `/sv/loai-thiet-bi`, `/quay/khung-phat`, `/quay/loai-thiet-bi`, `/quay/phieu-muon`, `/quay/phieu-muon/{ma}`, `/quay/bien-ban-vi-pham`, `/quan-tri/bien-ban-vi-pham`. Cột `x-tham-chieu` trong `openapi.yaml` ghi `ĐX-API-25`.
- `POST /anh` trước đây nằm trong bảng `/quay` của KT 6.4 (nay đã chuyển thành ghi chú dưới bảng, T-10); đường dẫn thực là `/api/v1/anh` (KT 6.1, CV 6.2) và dùng chính sách `NhanVienQuanTri`.
- KT 6.5 ghi `GET, POST, PUT /cau-hinh/khung-phat`: `PUT` nằm ở `/cau-hinh/khung-phat/{ma}`.

## 5. Luồng nghiệp vụ và thứ tự gọi API

Thao tác nhiều bước ở quầy **không giữ phiên phía server**: SSD tuần tự được ánh xạ thành các endpoint kiểm tra không có tác dụng phụ và **một** endpoint lập phiếu nguyên tử (KT 6.1).

### 5.1 Sinh viên đặt trước, nhân viên duyệt, giao, nhận trả

| Bước | Tác nhân | Endpoint | Kết quả |
|---|---|---|---|
| 1 | SV | `GET /sv/tra-cuu/thiet-bi`, `GET /sv/tra-cuu/thiet-bi/{maLoai}?maPhong=`; danh sách lọc: `GET /sv/phong-thiet-bi`, `GET /sv/loai-thiet-bi`; hồ sơ và lý do bị khóa: `GET /sv/toi` | Xem loại thiết bị, số lượng khả dụng |
| 2 | SV | `POST /sv/phieu-yeu-cau-muon` | Phiếu `CHO_DUYET` (chưa giữ chỗ). 409 `SO_LUONG_KHA_DUNG_THAY_DOI` thì chọn lại |
| 3 | NV | `GET /quay/phieu-yeu-cau-muon?trangThai=CHO_DUYET` | Danh sách chờ duyệt của phòng mình |
| 4 | NV | `GET /quay/phieu-yeu-cau-muon/{ma}/kiem-tra` | `duocDuyet` và `dsLyDo` |
| 5a | NV | `POST /quay/phieu-yeu-cau-muon/{ma}/duyet` | `DA_DUYET`, giữ chỗ `thoiGianGiuChoPhut` (mặc định 30 phút), thông báo SV |
| 5b | NV | `POST /quay/phieu-yeu-cau-muon/{ma}/tu-choi` (body `lyDo`) | `TU_CHOI`, thông báo SV |
| 6 | NV | `GET /quay/sinh-vien/the/{maThe}` | Quét thẻ: thông tin SV, `hopLe`, `dsLyDoKhongHopLe` |
| 7 | NV | `POST /quay/phieu-muon/kiem-tra-nguon` | Kiểm tra giữ chỗ còn hạn, điều kiện SV, giới hạn tổng; trả vị trí lấy hàng |
| 7b | NV | Mượn trực tiếp: `GET /quay/loai-thiet-bi?maPhong=` | Chọn loại và số lượng theo số khả dụng |
| 8 | NV | Lặp: `POST /quay/phieu-muon/kiem-tra-thiet-bi` | Từng thiết bị `SAN_SANG`, đúng phòng, đúng loại |
| 9 | NV | Lặp (nếu có ảnh): `POST /anh` với `giaiDoan=BAN_DAU` | `maAnh` |
| 10 | NV | `POST /quay/phieu-muon` | **Nguyên tử.** Phiếu `DANG_MUON` (201). Lỗi ở bước nào thì hoàn tác toàn bộ |
| 10b | NV | `GET /quay/phieu-muon?maSV=&trangThai=DANG_MUON`, `GET /quay/phieu-muon/{ma}` | Khi trả: tìm và xem phiếu của SV |
| 11 | NV | `GET /quay/thiet-bi/{maThietBi}/ban-giao` | Khi trả: xem tình trạng và ảnh lúc giao |
| 12 | NV | Lặp (nếu có ảnh): `POST /anh` với `giaiDoan=KHI_TRA` | `maAnh` |
| 12b | NV | `GET /quay/khung-phat` | Chỉ khi hư hỏng hoặc thiếu phụ kiện: chọn `maKhungPhat` |
| 13 | NV | `POST /quay/phieu-muon/{ma}/ket-qua-kiem-tra` | Cập nhật thiết bị và phiếu; `DA_TRA` khi mọi thiết bị đã có kết quả |

Mượn trực tiếp (không đặt trước) và giao từ danh sách chờ dùng cùng các bước 6 đến 10 với `nguon = TRUC_TIEP` hoặc `DANG_KY_CHO` (SSD-05, SSD-06).

### 5.2 Đăng ký chờ

| Bước | Tác nhân | Endpoint | Ghi chú |
|---|---|---|---|
| 1 | SV | `POST /sv/dang-ky-cho` | Chỉ khi khả dụng bằng 0; còn hàng trả 422 `CON_HANG_KHONG_DANG_KY_CHO` |
| 2 | SV | `GET /sv/dang-ky-cho` | Xem `soThuTuCho` |
| 3 | Hệ thống | (job, không có endpoint) | Khi có thiết bị cùng loại được trả (`NGUYEN_VEN`), chuyển giữ chỗ cho người đứng đầu, thông báo `THIET_BI_CHO_SAN_SANG` |
| 4 | SV | `POST /sv/dang-ky-cho/{ma}/roi` | Rời chờ hoặc từ chối nhận: `DA_HUY`, không tính vi phạm |
| 5 | NV | `GET /quay/danh-sach-cho`, `POST /quay/danh-sach-cho/{ma}/chuyen-luot` | Chuyển lượt thủ công khi SV báo không mượn nữa |
| 6 | NV | Giao như 5.1 bước 6 đến 10 với `nguon = DANG_KY_CHO` | Đăng ký chuyển `DA_NHAN` |

### 5.3 Vi phạm, phí phạt, mở khóa

| Bước | Tác nhân | Endpoint | Ghi chú |
|---|---|---|---|
| 1 | NV | `GET /quay/phieu-muon/{ma}/de-xuat-bien-ban`, `GET /quay/khung-phat` | Hệ thống tính sẵn các khoản theo từng thiết bị; khung phạt để chọn |
| 2 | NV | `POST /quay/bien-ban-vi-pham` | Số tiền do **server tính lại**; SV chuyển `TAM_KHOA`; thông báo `VI_PHAM_PHI_PHAT` |
| 3 | NV | `GET /quay/bien-ban-vi-pham?maSV=&trangThai=CHUA_NOP`, rồi `POST /quay/bien-ban-vi-pham/{ma}/da-nop` | Ghi nhận từng biên bản `DA_NOP` (tiền nộp trực tiếp, ngoài hệ thống) |
| 4 | NV | `POST /quay/sinh-vien/{maSV}/xac-nhan-nhan-du-thiet-bi` | Xét mở khóa |
| 5 | QT | `GET /quan-tri/bien-ban-vi-pham`, rồi `POST /quan-tri/bien-ban-vi-pham/{ma}/huy` hoặc `PUT .../dieu-chinh` | Chỉ QT; hủy thì xét mở khóa lại (ĐX-PQ-09) |
| A3 | NV | `GET /quay/phieu-muon/qua-han-30-ngay` rồi `POST /quay/bien-ban-vi-pham` (`loaiViPham = MAT`) | Thiết bị chưa trả quá 30 ngày lịch: `DA_BAO_MAT`, bồi thường 100% |

### 5.4 Bảo trì, sửa chữa, thanh lý

| Bước | Tác nhân | Endpoint | Ghi chú |
|---|---|---|---|
| 1 | NV | `GET /quay/bao-tri/den-han`, `GET /quay/bao-tri/cho-tiep-nhan-sua-chua` | Thiết bị đến hạn; thiết bị hư hỏng chờ tiếp nhận |
| 2 | NV | `POST /quay/bao-tri` | `BAO_TRI_DINH_KY` hoặc `SUA_CHUA` |
| 3 | NV | `POST /quay/bao-tri/{ma}/hoan-thanh` | `DUA_VE_SAN_SANG` hoặc `DE_XUAT_THANH_LY` |
| 4 | QT | `GET /quan-tri/thanh-ly/cho-quyet-dinh` | Thiết bị NV đề xuất thanh lý |
| 5 | QT | `POST /quan-tri/quyet-dinh-thanh-ly` | Thiết bị chuyển `DA_THANH_LY` |

### 5.5 Tải ảnh

1. `POST /anh` (multipart: `file`, `giaiDoan`) trả `maAnh` chưa gắn đối tượng.
2. Đưa `maAnh` vào `dsMaAnh` của `POST /quay/phieu-muon` (`BAN_DAU`), `POST /quay/phieu-muon/{ma}/ket-qua-kiem-tra` (`KHI_TRA`), hoặc `POST/PUT /quan-tri/thiet-bi` (`THIET_BI`).
3. Tối đa **5 ảnh** mỗi thiết bị mỗi lần giao hoặc trả; mỗi ảnh tối đa **2 MB** (sau nén ở client: cạnh dài tối đa 1280 px, JPEG khoảng 80%); định dạng `jpeg`, `png`, `webp` kiểm tra bằng magic bytes. Vi phạm trả 400 `DU_LIEU_KHONG_HOP_LE`.
4. Ảnh chưa gắn quá 24 giờ bị job `DonAnhMoCo` xóa; ảnh của phiếu đã lập không bao giờ bị xóa (NFR-07).
5. Ảnh tình trạng (bucket `tinh-trang-anh`) trả **URL có chữ ký, hết hạn sau 10 phút**, chỉ cấp sau khi kiểm tra quyền (NV của phòng hoặc SV chủ phiếu). Hết hạn thì gọi lại endpoint để lấy URL mới. Ảnh danh mục (`thiet-bi-anh`) là URL công khai chỉ đọc.

## 6. Máy trạng thái liên quan đến API

| Đối tượng | Chuyển trạng thái | Endpoint hoặc tác nhân | Lỗi nếu trái |
|---|---|---|---|
| Phiếu yêu cầu | `CHO_DUYET` → `DA_DUYET` | `POST /quay/phieu-yeu-cau-muon/{ma}/duyet` | 409 `TRANG_THAI_KHONG_HOP_LE` |
| | `CHO_DUYET` → `TU_CHOI` | `.../tu-choi` | 409 `TRANG_THAI_KHONG_HOP_LE` |
| | `DA_DUYET` → `DA_NHAN` | `POST /quay/phieu-muon` (nguồn `PHIEU_YEU_CAU`) | 409 `GIU_CHO_HET_HAN` nếu quá hạn giữ chỗ |
| | `CHO_DUYET` → `HET_HAN`, `DA_DUYET` → `HET_HAN`, → `DA_HUY` | Job hoặc UC-20 (UC-20 ngoài giai đoạn 1) | |
| Đăng ký chờ | `DANG_CHO` → `DA_THONG_BAO` | Đồng bộ trong `POST /quay/phieu-muon/{ma}/ket-qua-kiem-tra` (thiết bị trả nguyên vẹn) hoặc `POST /quay/bao-tri/{ma}/hoan-thanh` (thiết bị về `SAN_SANG`), không do job (ĐX-KT-13) | |
| | `DANG_CHO` hoặc `DA_THONG_BAO` → `DA_HUY` | `POST /sv/dang-ky-cho/{ma}/roi` | 409 `TRANG_THAI_KHONG_HOP_LE` |
| | `DA_THONG_BAO` → `DA_HUY` | `POST /quay/danh-sach-cho/{ma}/chuyen-luot` (NV chuyển lượt thủ công) | 409 `TRANG_THAI_KHONG_HOP_LE` |
| | `DA_THONG_BAO` → `DA_NHAN` | `POST /quay/phieu-muon` (nguồn `DANG_KY_CHO`) | 409 `GIU_CHO_HET_HAN` |
| | `DA_THONG_BAO` → `HET_HAN` | Job (quá hạn giữ chỗ, ghi 1 vi phạm) | |
| Phiếu mượn | → `DANG_MUON` | `POST /quay/phieu-muon` | |
| | `DANG_MUON` → `QUA_HAN` | Job | |
| | `DANG_MUON` hoặc `QUA_HAN` → `DA_TRA` | Khi mọi thiết bị có kết quả: `.../ket-qua-kiem-tra` | |
| Thiết bị | `SAN_SANG` → `DANG_DUOC_MUON` | `POST /quay/phieu-muon` | 409 `THIET_BI_DA_DUOC_GIAO` |
| | `DANG_DUOC_MUON` → `SAN_SANG` | `.../ket-qua-kiem-tra` (`NGUYEN_VEN`) | |
| | `DANG_DUOC_MUON` → `DANG_BAO_TRI_SUA_CHUA` | `.../ket-qua-kiem-tra` (`HU_HONG`, `THIEU_PHU_KIEN`) | |
| | `DANG_DUOC_MUON` → `DA_BAO_MAT` | `.../ket-qua-kiem-tra` (`MAT`), hoặc biên bản mất | |
| | `SAN_SANG` → `DANG_BAO_TRI_SUA_CHUA` | `POST /quay/bao-tri` (`BAO_TRI_DINH_KY`) | 409 `TRANG_THAI_KHONG_HOP_LE` |
| | `DANG_BAO_TRI_SUA_CHUA` → `SAN_SANG` | `POST /quay/bao-tri/{ma}/hoan-thanh` (`DUA_VE_SAN_SANG`) | |
| | `SAN_SANG` hoặc `DANG_BAO_TRI_SUA_CHUA` → `DA_THANH_LY` | `POST /quan-tri/quyet-dinh-thanh-ly` | 409 `TRANG_THAI_KHONG_HOP_LE` |
| Biên bản | `CHUA_NOP` → `DA_NOP` | `POST /quay/bien-ban-vi-pham/{ma}/da-nop` | 409 `TRANG_THAI_KHONG_HOP_LE` |
| | `CHUA_NOP` hoặc `DA_NOP` → `DA_HUY` | `POST /quan-tri/bien-ban-vi-pham/{ma}/huy` | 409 `TRANG_THAI_KHONG_HOP_LE` |
| Tài khoản NV | → `BI_KHOA` | `POST /quan-tri/nhan-vien/{ma}/khoa` | 409 `TRANG_THAI_KHONG_HOP_LE` |
| | `BI_KHOA` hoặc `TAM_KHOA_DANG_NHAP` → `HOAT_DONG` | `POST /quan-tri/nhan-vien/{ma}/mo-khoa` (ĐX-PQ-04) | 409 `TRANG_THAI_KHONG_HOP_LE` |

Tình trạng thiết bị **chỉ đổi qua máy trạng thái**; không có endpoint ghi trực tiếp `tinhTrang`.

## 7. Công thức và quy tắc ảnh hưởng đến phản hồi

| Quy tắc | Công thức hoặc nội dung | Nguồn |
|---|---|---|
| Số lượng khả dụng | Thiết bị `SAN_SANG`, không ngưng sử dụng, **trừ** số lượng đang giữ chỗ còn hạn | BR-20 |
| Hạn trả | `hanTra = min(gioGiao + thoiGianMuon, gioDongCuaCungNgay)`; `thoiGianMuon` là thời gian dự kiến của phiếu yêu cầu, hoặc thời gian mượn tối đa của nhóm khi mượn trực tiếp hoặc giao từ danh sách chờ; luôn không vượt mức tối đa của nhóm (nhỏ nhất theo các loại trong phiếu) | BR-42, KT 8.5 |
| Giao sau giờ đóng cửa | Từ chối 422 `GIAO_SAU_GIO_DONG_CUA`; chưa cấu hình giờ quầy thì 409 `CHUA_CAU_HINH_GIO_QUAY` | KT 6.6 |
| Giới hạn số thiết bị | Tính trên **tổng** số thiết bị đang cầm cộng số giao mới, không phân biệt loại | BR-24 |
| Thời gian mượn tối đa | Theo nhóm SV; phiếu nhiều loại lấy mức nhỏ nhất, áp dụng ngoại lệ theo loại nếu có | BR-25 |
| Trễ hạn | Trả quá `ngayGioHenTra` sau `anHanPhut` (mặc định 15 phút) thì tính trễ; số ngày trễ theo **ngày lịch**, tối thiểu 1 | BR-57 |
| Phí trả trễ | Từng thiết bị: `min(soNgayTre × phiTreMoiNgay (đã lưu khi giao), giaTriHienHanh)`; phí phiếu là tổng | BR-58, KT 8.5 |
| Bồi thường hư hỏng (kể cả thiếu phụ kiện) | `làmTròn(giaTriHienHanh × tyLe / 100)` đến đồng, `MidpointRounding.AwayFromZero` | BR-59, KT 8.5 |
| Bồi thường mất | 100% `giaTriHienHanh` tại thời điểm mất | BR-60 |
| Mốc biên bản mất | Chưa trả quá 30 ngày lịch kể từ `ngayGioHenTra` | BR-61, ĐX-04 |
| Đếm vi phạm | Mỗi loại (`TRA_TRE`, `HU_HONG`, `MAT`) có trong một biên bản tính một vi phạm; thiếu phụ kiện tính là `HU_HONG` | BR-63 |
| Khóa quyền mượn | Job `KhoaQuyenMuon`, hoặc khi lập biên bản (`TAM_KHOA`) | BR-66 |
| Mở khóa | Chỉ theo sự kiện (NV ghi nhận đã nộp, xác nhận nhận đủ, hoặc QT hủy biên bản): không còn biên bản `CHUA_NOP` **và** không còn thiết bị chưa trả của phiếu quá hạn; **không mở** nếu `CAM_MUON_DAI_HAN` | BR-67, KT 8.6, ĐX-KT-14 |
| Đến hạn bảo trì | Ngày hiện tại ≥ ngày hoàn thành bảo trì định kỳ gần nhất (chưa có thì ngày nhập kho) + `chuKyBaoTri` | BR-70 |

Cấu hình đổi (`phiTreMoiNgay`, khung phạt, quy định mượn, tham số) **không hồi tố**: phiếu và biên bản đã lập giữ giá trị lúc lập.

## 8. Quy tắc kiểm tra đầu vào và lỗi

| Loại lỗi | HTTP | `maLoi` | Khi nào |
|---|---|---|---|
| Sai cú pháp, thiếu trường, sai kiểu, sai định dạng, vi phạm `minLength`, `minItems`, `maxItems`, `format` trong schema | 400 | `DU_LIEU_KHONG_HOP_LE` | Trước khi vào nghiệp vụ |
| Vi phạm ràng buộc **cấu hình** của UC-13 (quy định mượn, tham số, khung phạt, phí trả trễ) | 422 | `GIA_TRI_CAU_HINH_KHONG_HOP_LE` | ĐX-API-15 |
| Vi phạm quy tắc nghiệp vụ | 422 hoặc 409 | Theo bảng mã lỗi | Mục 10 |
| Đối tượng không tồn tại | 404 | `KHONG_TIM_THAY` | |
| Đối tượng của người khác, hoặc phòng chưa cấp quyền | 403 | `KHONG_DU_QUYEN`, `PHONG_CHUA_CAP_QUYEN` | Tồn tại nhưng không được xem |
| Mật khẩu nhân viên ngắn hơn 8 ký tự | 400 | `DU_LIEU_KHONG_HOP_LE` | ĐX-API-27 |

Lỗi 500 không lộ chi tiết nội bộ; chỉ trả `traceId`. Ghi log (Serilog) không chứa mật khẩu, token hay nội dung ảnh.

## 9. Báo cáo (UC-16)

`GET /quan-tri/bao-cao/{loai}?tuNgay=&denNgay=&dinhDang=` với `dinhDang` là `json` (mặc định), `xlsx` hoặc `csv`. Mọi định dạng có **đúng các cột dưới đây theo thứ tự**; xlsx và csv dùng tiêu đề tiếng Việt ở cột "Tiêu đề cột xlsx/csv"; csv mã hóa UTF-8. `tuNgay`, `denNgay` là ngày lịch Asia/Ho_Chi_Minh, gồm cả hai đầu; bỏ trống thì không giới hạn đầu đó. Không có dữ liệu trong kỳ thì báo cáo trống. Cột và mốc lọc đã được duyệt (ĐX-API-29).

### 9.1 `tinh-trang-kho`: Tình trạng kho

Mỗi dòng là một loại thiết bị tại một phòng; số thiết bị theo từng tình trạng. Là ảnh chụp tại thời điểm xuất, **không** lọc theo `tuNgay`, `denNgay`. `tong` bằng tổng năm cột đếm nên khớp 100% dữ liệu thực tế. Mốc lọc thời gian: Ảnh chụp hiện tại. Căn cứ: BR-81 (1), FR-22.

| Khóa JSON | Kiểu | Rỗng được | Tiêu đề cột xlsx/csv | Ghi chú |
|---|---|---|---|---|
| `maLoai` | uuid | không | Mã loại | Mã loại thiết bị |
| `tenLoai` | chuỗi | không | Tên loại |  |
| `maPhong` | uuid | không | Mã phòng |  |
| `tenPhong` | chuỗi | không | Phòng |  |
| `soSanSang` | số nguyên | không | Sẵn sàng | Số thiết bị `SAN_SANG` |
| `soDangDuocMuon` | số nguyên | không | Đang mượn | Số thiết bị `DANG_DUOC_MUON` |
| `soDangBaoTriSuaChua` | số nguyên | không | Bảo trì/sửa chữa | Số thiết bị `DANG_BAO_TRI_SUA_CHUA` |
| `soDaThanhLy` | số nguyên | không | Đã thanh lý | Số thiết bị `DA_THANH_LY` |
| `soDaBaoMat` | số nguyên | không | Đã báo mất | Số thiết bị `DA_BAO_MAT` |
| `tong` | số nguyên | không | Tổng | Tổng số thiết bị |

### 9.2 `qua-han`: Sinh viên mượn quá hạn

Mỗi dòng là một thiết bị chưa trả của phiếu đang `QUA_HAN`. Lọc theo `ngayGioHenTra` nằm trong kỳ. Mốc lọc thời gian: `ngayGioHenTra`. Căn cứ: BR-81 (2).

| Khóa JSON | Kiểu | Rỗng được | Tiêu đề cột xlsx/csv | Ghi chú |
|---|---|---|---|---|
| `maPhieu` | chuỗi | không | Mã phiếu mượn |  |
| `maSV` | chuỗi | không | MSSV |  |
| `hoTen` | chuỗi | không | Họ tên |  |
| `lop` | chuỗi | có | Lớp |  |
| `ngayGioMuon` | date-time | không | Ngày giờ mượn |  |
| `ngayGioHenTra` | date-time | không | Hẹn trả |  |
| `soNgayTre` | số nguyên | không | Số ngày trễ | Theo ngày lịch Asia/Ho_Chi_Minh, tính đến thời điểm xuất |
| `maThietBi` | chuỗi | không | Mã thiết bị | Thiết bị chưa trả |
| `tenThietBi` | chuỗi | không | Tên thiết bị |  |
| `tenPhong` | chuỗi | không | Phòng |  |

### 9.3 `hu-hong-mat`: Thiết bị hư hỏng hoặc mất

Mỗi dòng là một thiết bị có kết quả kiểm tra `HU_HONG`, `THIEU_PHU_KIEN` hoặc `MAT`, kèm sinh viên và phiếu mượn. Lọc theo `ngayGioMuon` của phiếu nằm trong kỳ. Mốc lọc thời gian: `ngayGioMuon` của phiếu. Căn cứ: BR-81 (3).

| Khóa JSON | Kiểu | Rỗng được | Tiêu đề cột xlsx/csv | Ghi chú |
|---|---|---|---|---|
| `maThietBi` | chuỗi | không | Mã thiết bị |  |
| `tenThietBi` | chuỗi | không | Tên thiết bị |  |
| `tenLoai` | chuỗi | không | Loại |  |
| `tenPhong` | chuỗi | không | Phòng |  |
| `ketQuaKiemTra` | enum HU_HONG, THIEU_PHU_KIEN, MAT | không | Kết quả kiểm tra |  |
| `tinhTrangKhiTra` | chuỗi | có | Tình trạng khi trả | Rỗng nếu không ghi mô tả |
| `maPhieu` | chuỗi | không | Mã phiếu mượn |  |
| `maSV` | chuỗi | không | MSSV |  |
| `hoTen` | chuỗi | không | Họ tên |  |
| `ngayGioMuon` | date-time | không | Ngày giờ mượn |  |
| `ngayGioTraThucTe` | date-time | có | Ngày giờ trả | Rỗng với `MAT` |
| `maBienBan` | chuỗi | có | Mã biên bản | Rỗng nếu chưa lập biên bản |

### 9.4 `tan-suat-muon`: Tần suất mượn theo loại

Mỗi dòng là một loại thiết bị; đếm trong các phiếu mượn có `ngayGioMuon` nằm trong kỳ. Loại không có lượt mượn trong kỳ không xuất hiện. Mốc lọc thời gian: `ngayGioMuon` của phiếu. Căn cứ: BR-81 (4).

| Khóa JSON | Kiểu | Rỗng được | Tiêu đề cột xlsx/csv | Ghi chú |
|---|---|---|---|---|
| `maLoai` | uuid | không | Mã loại |  |
| `tenLoai` | chuỗi | không | Tên loại |  |
| `soLuotMuon` | số nguyên | không | Số lượt mượn | Số thiết bị được giao thuộc loại này (mỗi thiết bị trong phiếu tính một lượt) |
| `soPhieuMuon` | số nguyên | không | Số phiếu mượn | Số phiếu mượn có thiết bị thuộc loại này |

### 9.5 `vi-pham-sinh-vien`: Vi phạm theo sinh viên

Mỗi dòng là một sinh viên có ít nhất một vi phạm trong kỳ; đếm theo bản ghi vi phạm (BR-63). Lọc theo `thoiDiemGhiNhan` nằm trong kỳ. Mốc lọc thời gian: `thoiDiemGhiNhan` của vi phạm. Căn cứ: BR-81 (5), FR-26.

| Khóa JSON | Kiểu | Rỗng được | Tiêu đề cột xlsx/csv | Ghi chú |
|---|---|---|---|---|
| `maSV` | chuỗi | không | MSSV |  |
| `hoTen` | chuỗi | không | Họ tên |  |
| `lop` | chuỗi | có | Lớp |  |
| `soTraTre` | số nguyên | không | Trả trễ | Số vi phạm `TRA_TRE` |
| `soHuHong` | số nguyên | không | Hư hỏng | Số vi phạm `HU_HONG` |
| `soMat` | số nguyên | không | Mất | Số vi phạm `MAT` |
| `soKhongDenNhan` | số nguyên | không | Không đến nhận | Số vi phạm `KHONG_DEN_NHAN` |
| `tongSoViPham` | số nguyên | không | Tổng số vi phạm | Tổng bốn cột trên |

### 9.6 `bao-tri-thanh-ly`: Lịch sử bảo trì và thanh lý

Bảng phẳng gồm hai loại bản ghi phân biệt bằng `loaiBanGhi`: `BAO_TRI` (mỗi bản ghi bảo trì/sửa chữa, lọc theo `ngayLap`) và `THANH_LY` (mỗi thiết bị trong một đợt thanh lý, lọc theo `ngayThanhLy`). Các đợt thanh lý phân biệt bằng `maQuyetDinh`. Mốc lọc thời gian: `ngayLap` (bảo trì) hoặc `ngayThanhLy` (thanh lý). Căn cứ: BR-81 (6), FR-27.

| Khóa JSON | Kiểu | Rỗng được | Tiêu đề cột xlsx/csv | Ghi chú |
|---|---|---|---|---|
| `loaiBanGhi` | enum BAO_TRI, THANH_LY | không | Loại bản ghi |  |
| `maThietBi` | chuỗi | không | Mã thiết bị |  |
| `tenThietBi` | chuỗi | không | Tên thiết bị |  |
| `tenPhong` | chuỗi | không | Phòng |  |
| `loaiThucHien` | enum BAO_TRI_DINH_KY, SUA_CHUA | có | Loại thực hiện | Chỉ `BAO_TRI` |
| `ngayLap` | date-time | có | Ngày lập | Chỉ `BAO_TRI` |
| `ngayHoanThanh` | date-time | có | Ngày hoàn thành | Chỉ `BAO_TRI`, rỗng nếu chưa hoàn thành |
| `ketQuaBaoTri` | enum DUA_VE_SAN_SANG, DE_XUAT_THANH_LY | có | Kết quả | Chỉ `BAO_TRI` |
| `noiDungThucHien` | chuỗi | có | Nội dung thực hiện | Chỉ `BAO_TRI` |
| `maQuyetDinh` | chuỗi | có | Mã quyết định thanh lý | Chỉ `THANH_LY` |
| `ngayThanhLy` | ngày | có | Ngày thanh lý | Chỉ `THANH_LY` |

## 10. Mã lỗi nghiệp vụ

Bảng sinh từ `openapi.yaml`, trùng bảng KT 6.6 và Conv 6.7, gồm `MA_NV_TRUNG` (ĐX-API-26). Client phân nhánh theo `maLoi`, **không** theo `detail`.

| `maLoi` | HTTP | Tình huống | Số endpoint có thể trả | Endpoint cụ thể (khi ít) |
|---|---|---|---|---|
| `DANG_NHAP_SAI` | 401 | Thông tin đăng nhập không đúng | 2 | `authDangNhapNhanVien`, `authDangNhapSinhVien` |
| `CHUA_XAC_THUC` | 401 | Token thiếu, không hợp lệ hoặc hết hạn; client dùng auth/lam-moi | 84 | xem `responses` trong `openapi.yaml` |
| `TAI_KHOAN_BI_KHOA` | 403 | Tài khoản bị khóa hoặc tạm khóa đăng nhập | 67 | xem `responses` trong `openapi.yaml` |
| `KHONG_DU_QUYEN` | 403 | Truy cập chức năng ngoài vai trò, hoặc dữ liệu của người khác | 82 | xem `responses` trong `openapi.yaml` |
| `PHONG_CHUA_CAP_QUYEN` | 403 | Nhân viên truy cập đối tượng thuộc phòng chưa được cấp quyền | 15 | xem `responses` trong `openapi.yaml` |
| `SV_THE_HET_HAN` | 422 | Thẻ sinh viên đã hết hạn | 3 | `quayDuyetPhieuYeuCauMuon`, `quayLapPhieuMuon`, `svGuiPhieuYeuCauMuon` |
| `SV_KHONG_CON_THEO_HOC` | 422 | Sinh viên không còn theo học | 3 | `quayDuyetPhieuYeuCauMuon`, `quayLapPhieuMuon`, `svGuiPhieuYeuCauMuon` |
| `SV_BI_KHOA_QUYEN_MUON` | 422 | Quyền mượn của sinh viên đang bị khóa | 4 | `quayDuyetPhieuYeuCauMuon`, `quayLapPhieuMuon`, `svDangKyCho`, `svGuiPhieuYeuCauMuon` |
| `VUOT_GIOI_HAN_THIET_BI` | 422 | Vượt giới hạn số thiết bị của nhóm sinh viên | 3 | `quayDuyetPhieuYeuCauMuon`, `quayLapPhieuMuon`, `svGuiPhieuYeuCauMuon` |
| `VUOT_THOI_GIAN_MUON_TOI_DA` | 422 | Vượt thời gian mượn tối đa của nhóm sinh viên | 2 | `quayDuyetPhieuYeuCauMuon`, `svGuiPhieuYeuCauMuon` |
| `CON_HANG_KHONG_DANG_KY_CHO` | 422 | Còn số lượng khả dụng nên không đăng ký chờ | 1 | `svDangKyCho` |
| `THIET_BI_KHONG_SAN_SANG` | 422 | Thiết bị không ở tình trạng Sẵn sàng cho mượn | 2 | `quayKiemTraThietBiGiao`, `quayLapPhieuMuon` |
| `THIET_BI_SAI_PHONG` | 422 | Thiết bị không thuộc phòng của phiếu hoặc phòng được cấp quyền | 2 | `quayKiemTraThietBiGiao`, `quayLapPhieuMuon` |
| `THIET_BI_SAI_LOAI` | 422 | Thiết bị không đúng loại đã duyệt | 2 | `quayKiemTraThietBiGiao`, `quayLapPhieuMuon` |
| `THIET_BI_KHONG_THUOC_PHIEU` | 422 | Thiết bị không thuộc phiếu mượn đang hiệu lực | 3 | `quayBanGiaoThietBi`, `quayGhiKetQuaKiemTra`, `quayLapBienBanViPham` |
| `THIEU_MO_TA_HOAC_KHUNG_PHAT` | 422 | Hư hỏng hoặc thiếu phụ kiện thiếu mô tả hoặc khung phạt | 2 | `quayGhiKetQuaKiemTra`, `quayLapBienBanViPham` |
| `GIA_TRI_CAU_HINH_KHONG_HOP_LE` | 422 | Giá trị cấu hình vi phạm ràng buộc | 5 | `qtCapNhatPhiTre`, `qtCapNhatQuyDinhMuon`, `qtCapNhatThamSo`, `qtSuaKhungPhat`, `qtTaoKhungPhat` |
| `GIAO_SAU_GIO_DONG_CUA` | 422 | Thời điểm giao đã từ giờ đóng cửa trở đi | 1 | `quayLapPhieuMuon` |
| `SO_LUONG_KHA_DUNG_THAY_DOI` | 409 | Số lượng khả dụng đã thay đổi, không còn đủ | 3 | `quayDuyetPhieuYeuCauMuon`, `quayLapPhieuMuon`, `svGuiPhieuYeuCauMuon` |
| `GIU_CHO_HET_HAN` | 409 | Giữ chỗ đã quá hạn, không giao theo phiếu cũ | 1 | `quayLapPhieuMuon` |
| `THIET_BI_DA_DUOC_GIAO` | 409 | Thiết bị vừa được giao trong một giao dịch khác | 1 | `quayLapPhieuMuon` |
| `TRANG_THAI_KHONG_HOP_LE` | 409 | Chuyển trạng thái trái máy trạng thái | 15 | xem `responses` trong `openapi.yaml` |
| `MA_THIET_BI_TRUNG` | 409 | Mã thiết bị đã tồn tại | 1 | `qtTaoThietBi` |
| `TEN_DANG_NHAP_TRUNG` | 409 | Tên đăng nhập đã tồn tại | 1 | `qtTaoNhanVien` |
| `MA_NV_TRUNG` | 409 | Mã nhân viên đã tồn tại (ĐX-API-26) | 1 | `qtTaoNhanVien` |
| `KHUNG_PHAT_DA_DUNG_KHONG_XOA` | 409 | Khung phạt đã dùng, chỉ được tắt | 1 | `qtXoaKhungPhat` |
| `CHUA_CAU_HINH_GIO_QUAY` | 409 | Chưa cấu hình giờ mở cửa, đóng cửa quầy | 1 | `quayLapPhieuMuon` |
| `XUNG_DOT_CAP_NHAT` | 409 | Dữ liệu vừa bị thay đổi bởi giao dịch khác (concurrency token xmin) | 28 | xem `responses` trong `openapi.yaml` |
| `DA_CO_LICH_SU_CHI_NGUNG_SU_DUNG` | 409 | Đã có lịch sử hoặc còn đối tượng phụ thuộc, chỉ được ngưng sử dụng | 2 | `qtXoaLoaiThietBi`, `qtXoaThietBi` |
| `DU_LIEU_KHONG_HOP_LE` | 400 | Dữ liệu vào sai cú pháp hoặc thiếu trường | 82 | xem `responses` trong `openapi.yaml` |
| `KHONG_TIM_THAY` | 404 | Mã tài nguyên không tồn tại | 36 | xem `responses` trong `openapi.yaml` |
| `QUA_NHIEU_YEU_CAU` | 429 | Vượt giới hạn tần suất trên /auth/* | 4 | `authDangNhapNhanVien`, `authDangNhapSinhVien`, `authDangXuat`, `authLamMoi` |

Hướng xử lý của client:

| Tình huống | Hành động |
|---|---|
| 401 `CHUA_XAC_THUC` | Gọi `POST /auth/lam-moi` một lần; thành công thì gọi lại yêu cầu ban đầu; thất bại thì về màn hình đăng nhập |
| 403 `TAI_KHOAN_BI_KHOA` | Hiển thị tài khoản bị khóa, đăng xuất |
| 409 `SO_LUONG_KHA_DUNG_THAY_DOI` | Tải lại số lượng, để SV chọn lại |
| 409 `XUNG_DOT_CAP_NHAT` | Tải lại dữ liệu rồi thử lại |
| 409 `THIET_BI_DA_DUOC_GIAO` | Thiết bị đã được NV khác giao; chọn thiết bị khác |
| 409 `GIU_CHO_HET_HAN` | Không giao theo phiếu cũ; xem lại trạng thái phiếu hoặc đăng ký |
| 422 `SV_*`, `VUOT_*` | Hiển thị đúng điều kiện không đạt (BR-23, BR-24, BR-25) |
| 429 `QUA_NHIEU_YEU_CAU` | Chờ rồi thử lại |

## 11. Quyết định thiết kế API (ĐX-API)

Những điểm nguồn **không nêu**, tài liệu này quyết định để `openapi.yaml` dùng được ngay. **Toàn bộ mục dưới đây đã được nhóm duyệt.** Việc cập nhật tài liệu nguồn theo các quyết định này nằm ở mục 12.

| Mã | Quyết định | Căn cứ | Trạng thái |
|---|---|---|---|
| ĐX-API-01 | Dùng OpenAPI 3.0.3 (`nullable: true` cho trường rỗng được) để tương thích công cụ sinh client | Nguồn không nêu phiên bản | Đã duyệt |
| ĐX-API-02 | `paths` ghi đầy đủ `/api/v1/...`; `/health*` ngoài tiền tố; `servers.url` không chứa `/api/v1`; `host` là biến, đặt theo môi trường (CI, ứng dụng Android), không cố định tên miền trong hợp đồng | KT 6.1, 14 | Đã duyệt |
| ĐX-API-03 | Mã thành công: `POST` tạo mới 201; hành động và cập nhật 200 kèm đối tượng sau thay đổi; `DELETE`, đăng xuất, đặt lại mật khẩu 204 | CV 6.5 nêu 201 cho tạo chứng từ và 200 cho hành động; 204 là bổ sung | Đã duyệt |
| ĐX-API-04 | Tên trường: viết tắt `SV`, `NV`, `QT`, `NCC` giữ chữ hoa trong camelCase | KT đã dùng `maSV`, `maNV`, `maThe` | Đã duyệt |
| ĐX-API-05 | Đăng nhập và `lam-moi` trả `{accessToken, refreshToken, vaiTro}`; `vaiTro` để client chuyển giao diện theo vai trò | UC-19 | Đã duyệt |
| ĐX-API-06 | `lam-moi` và `dang-xuat` với refresh token không hợp lệ, hết hạn, đã thu hồi hoặc của tài khoản bị khóa: 401 `CHUA_XAC_THUC` | KT 7.3 | Đã duyệt |
| ĐX-API-07 | `GET /auth/toi` trả `maNguoiDung`, `vaiTro`, `tenDangNhap`, `hoTen`, `dsPhong`; quản trị viên có `dsPhong` rỗng | KT 6.2 | Đã duyệt |
| ĐX-API-08 | Các kiểm tra hiển thị lý do (quét thẻ, kiểm tra phiếu yêu cầu, kiểm tra nguồn giao) trả 200 kèm `hopLe` hoặc `duocDuyet` và `dsLyDo`; `maLoi` trong `dsLyDo` lấy từ bảng KT 6.6. Riêng kiểm tra thiết bị trả 422 `THIET_BI_*` | SSD-02, SSD-04; KT 6.6 | Đã duyệt |
| ĐX-API-09 | Body lập phiếu mượn phân biệt theo `nguon`: `PHIEU_YEU_CAU` cần `maYeuCau`; `DANG_KY_CHO` cần `maDangKy`; `TRUC_TIEP` cần `maSV`, `maPhong`, `dsLoaiVaSoLuong`; cả ba kèm `dsThietBi[{maThietBi, moTa, dsMaAnh}]`. `kiem-tra-nguon` cùng ba dạng, không có `dsThietBi` | KT 6.4; SSD-04, 05, 06 | Đã duyệt |
| ĐX-API-10 | Body `kiem-tra-thiet-bi` là `{maThietBi, maPhong, maLoai}` (phòng và loại lấy từ kết quả `kiem-tra-nguon`) | BR-40 | Đã duyệt |
| ĐX-API-11 | `ket-qua-kiem-tra` nhận mảng `dsKetQua`, xử lý trong một giao dịch; `maKhungPhat` bắt buộc với `HU_HONG`, `THIEU_PHU_KIEN`, bỏ trống với `NGUYEN_VEN`, `MAT`; `moTa` bắt buộc với `HU_HONG`, `THIEU_PHU_KIEN` | KT 6.4, BR-50; `ck_ctpm_ket_qua_dong_bo` | Đã duyệt |
| ĐX-API-12 | Body lập biên bản chỉ gồm khoản `{maThietBi, loaiViPham, maKhungPhat}`; số ngày trễ và số tiền do server tính; `maKhungPhat` bắt buộc với `HU_HONG` | KT 6.4; `ck_ctbb_khung_phat` | Đã duyệt |
| ĐX-API-13 | Điều chỉnh biên bản: `PUT .../dieu-chinh` nhận `dsKhoan[{maThietBi, loaiViPham, soTien}]`; chỉ biên bản `CHUA_NOP`; chỉ điều chỉnh `soTien` của khoản đã có; `tongTien` tính lại; không bắt buộc lý do vì đã có nhật ký giá trị trước và sau | BR-07, BR-62 | Đã duyệt |
| ĐX-API-14 | `PUT /cau-hinh/quy-dinh-muon` thay thế quy định của **một** nhóm (dòng chung và ngoại lệ theo loại); `GET` trả mọi nhóm. `PUT /cau-hinh/tham-so` thay thế toàn bộ bộ tham số | KT 6.5, UC-13 | Đã duyệt |
| ĐX-API-15 | Ràng buộc miền giá trị của cấu hình UC-13 trả 422 `GIA_TRI_CAU_HINH_KHONG_HOP_LE`, không đặt `minimum` trong schema các body này; ràng buộc cấu trúc còn lại trả 400 | KT 6.6 | Đã duyệt |
| ĐX-API-16 | `POST /anh` nhận thêm trường `giaiDoan` (`BAN_DAU`, `KHI_TRA`, `THIET_BI`) để chọn bucket (`THIET_BI` → `thiet-bi-anh`; hai giá trị còn lại → `tinh-trang-anh`). Khi gắn ảnh, `giaiDoan` phải khớp đối tượng | `ck_anh_gan_doi_tuong`, KT 11 | Đã duyệt |
| ĐX-API-17 | Chi tiết tra cứu theo (loại, phòng): `dsThongSoKyThuat` là các thông số không trùng và `dsAnh` là ảnh danh mục của các thiết bị thuộc loại tại phòng đó | UC-01 bước 4 | Đã duyệt |
| ĐX-API-18 | Vị trí lấy hàng nhóm theo (loại, kệ, tủ) của thiết bị `SAN_SANG`, kèm `soThietBiSanSang` | SSD-04 | Đã duyệt |
| ĐX-API-19 | Báo cáo `json` phân biệt theo `loai` (`oneOf` với `discriminator`), mỗi loại có schema riêng (xem ĐX-API-29) | BR-81 | Đã duyệt |
| ĐX-API-20 | Danh mục (`phong-thiet-bi`, `loai-thiet-bi`, `thiet-bi`, `nhan-vien`) không có `GET /{ma}`; các `GET` đọc khác chỉ thêm theo ĐX-API-25 | CV 6.3 | Đã duyệt |
| ĐX-API-21 | Tham số lọc: `loai-thiet-bi` (QT) lọc `ngungSuDung`; `thiet-bi` (QT) lọc `ten`, `maLoai`, `maPhong`, `tinhTrang`; danh sách quầy có `maPhong`, `maLoai`, `maSV`, `trangThai` theo từng endpoint; danh sách của SV chỉ phân trang, riêng tra cứu có bộ lọc theo FR-01 | FR-01, UC-12 | Đã duyệt |
| ĐX-API-22 | `maPhong` ngoài quyền trong danh sách quầy cho kết quả rỗng (giao với phòng được cấp quyền), không trả 403 | KT 6.1 | Đã duyệt |
| ĐX-API-23 | Các `PUT` dùng ngữ nghĩa thay thế toàn bộ (kể cả `PUT /nhan-vien/{ma}/quyen-phong`); `{ma}` của nhân viên là `maNguoiDung` (UUID) | KT 6.5 | Đã duyệt |
| ĐX-API-24 | Quét thẻ: `dsBienBanChuaNop` (biên bản `CHUA_NOP`) là "nợ vi phạm"; `soThietBiToiDa` là giới hạn của nhóm | FR-08, SSD-04 | Đã duyệt |
| ĐX-API-25 | Bổ sung mười endpoint `GET` đọc, cùng chính sách khu vực: `/sv/toi`, `/sv/vi-pham`, `/sv/phong-thiet-bi`, `/sv/loai-thiet-bi`, `/quay/khung-phat`, `/quay/loai-thiet-bi`, `/quay/phieu-muon`, `/quay/phieu-muon/{ma}`, `/quay/bien-ban-vi-pham`, `/quan-tri/bien-ban-vi-pham`. Lý do: (a) NV chọn khung phạt (UC-08, UC-09); (b) NV tìm phiếu mượn và biên bản của SV (UC-08, UC-09); (c) NV chọn loại và số lượng khi mượn trực tiếp (UC-07), SV có danh sách lọc (UC-01); (d) QT chọn biên bản để hủy hoặc điều chỉnh (UC-09 A2); (e) SV xem lý do bị khóa (UC-17 bước 4); (f) SV xem vi phạm `KHONG_DEN_NHAN` không có biên bản (UC-05). Phạm vi: NV chỉ thấy phiếu mượn và biên bản có thiết bị thuộc phòng được cấp quyền | UC-05, 07, 08, 09, 17; Authorization (phạm vi phòng) | Đã duyệt |
| ĐX-API-26 | Thêm mã lỗi `MA_NV_TRUNG` (409) cho trùng mã nhân viên (`uq_nvql_ma_nv`) khi tạo tài khoản nhân viên | KT 6.6 chỉ có `TEN_DANG_NHAP_TRUNG` | Đã duyệt |
| ĐX-API-27 | Mật khẩu nhân viên, quản trị viên khi tạo hoặc đặt lại tối thiểu 8 ký tự; vi phạm trả 400 `DU_LIEU_KHONG_HOP_LE`; đăng nhập không kiểm tra độ dài | Nguồn không nêu chính sách mật khẩu | Đã duyệt |
| ĐX-API-28 | Thứ tự mặc định của danh sách: của SV và thông báo mới nhất trước; phiếu yêu cầu chờ duyệt cũ nhất trước; danh sách chờ theo `thoiDiemDangKy` tăng dần (BR-34); danh mục theo tên hoặc mã tăng dần; khóa phụ là mã tăng dần | BR-34 (danh sách chờ); còn lại là bổ sung | Đã duyệt |
| ĐX-API-29 | Cột và mốc lọc của sáu báo cáo như mục 9; `tinh-trang-kho` là ảnh chụp hiện tại, bỏ qua `tuNgay`, `denNgay`; xlsx là một trang tính; csv UTF-8 | BR-81, FR-22 đến FR-27 | Đã duyệt |
| ĐX-API-30 | `openapi.yaml` là hợp đồng thiết kế (contract-first) dùng để review và sinh client; CI so sánh với `openapi.json` sinh từ mã và báo lỗi nếu lệch | KT 6.1, CV 6 nêu `openapi.json` sinh từ mã | Đã duyệt |

## 12. Việc cần cập nhật ở tài liệu nguồn

Các quyết định ở mục 11 làm lệch một số câu chữ trong tài liệu gốc. Cần sửa để các tài liệu thống nhất. Cột Trạng thái ghi kết quả khắc phục theo `CROSS_DOCUMENT_REMEDIATION_PLAN.md` (T-xx là mã task của kế hoạch đó).

| Tài liệu | Mục | Nội dung cần cập nhật | Theo | Trạng thái |
|---|---|---|---|---|
| `backend-architecture.md` | 6.1 | Hàng "Tài liệu": ghi `openapi.yaml` là hợp đồng thiết kế; `openapi.json` sinh từ mã phải khớp, CI so sánh. Câu cấm tài liệu API soạn riêng không nằm ở KT mà ở `Backend-Conventions.md` 6.1 (dòng kế tiếp) | ĐX-API-30 | Đã áp dụng (T-09) |
| `Backend-Conventions.md` | 6.1 | Bỏ câu cấm tài liệu API soạn riêng khỏi hàng "Tài liệu"; thay bằng: `openapi.yaml` là hợp đồng thiết kế; `openapi.json` sinh từ mã phải khớp, CI so sánh | ĐX-API-30 | Đã áp dụng (T-09) |
| `Backend-Conventions.md` | 18.1 | Bước CI "2. OpenAPI": xuất `openapi.json` làm artifact và so sánh với `openapi.yaml`, báo lỗi nếu lệch | ĐX-API-30 | Đã áp dụng (T-09) |
| `backend-architecture.md` | 2.2 (KT-18) | Hàng "OpenAPI": ghi `openapi.yaml` là hợp đồng thiết kế | ĐX-API-30 | Đã áp dụng (T-09) |
| `backend-architecture.md` | 16.4 | Bước CI "2. OpenAPI": như `Backend-Conventions.md` 18.1 | ĐX-API-30 | Đã áp dụng (T-09) |
| `backend-architecture.md` | 6.3 | Thêm `GET /sv/toi`, `/sv/vi-pham`, `/sv/phong-thiet-bi`, `/sv/loai-thiet-bi` | ĐX-API-25 | Đã áp dụng (T-10) |
| `backend-architecture.md` | 6.4 | Thêm `GET /quay/khung-phat`, `/quay/loai-thiet-bi`, `/quay/phieu-muon`, `/quay/phieu-muon/{ma}`, `/quay/bien-ban-vi-pham`; sửa `/anh` thành `/api/v1/anh` ngoài bảng `/quay` | ĐX-API-25 | Đã áp dụng (T-10) |
| `backend-architecture.md` | 6.5 | Thêm `GET /quan-tri/bien-ban-vi-pham`; ghi cột sáu báo cáo (hoặc dẫn tới API-Notes mục 9) | ĐX-API-25, 29 | Phần `GET /quan-tri/bien-ban-vi-pham`: Đã áp dụng (T-10). Phần ghi cột sáu báo cáo: Chưa áp dụng, ngoài phạm vi kế hoạch này |
| `backend-architecture.md` | 6.6 | Thêm mã lỗi `MA_NV_TRUNG` (409) | ĐX-API-26 | Đã áp dụng (T-11) |
| `Backend-Conventions.md` | 6.7 | Thêm mã lỗi `MA_NV_TRUNG` (409) cho khớp KT 6.6 | ĐX-API-26 | Đã áp dụng (T-11) |
| `Authorization.md` | Ma trận quyền | NV xem phiếu mượn, biên bản của phòng được cấp quyền, khung phạt, loại thiết bị theo phòng; SV xem hồ sơ mượn, vi phạm, danh sách phòng và loại; QT xem danh sách biên bản | ĐX-API-25 | Không cần: quyền theo use case (ĐX-PQ-03); phạm vi phòng của biên bản đã ghi ở ĐX-PQ-11 |
| `backend-architecture.md` hoặc `Authorization.md` | 7.4 | Ghi chính sách mật khẩu tối thiểu 8 ký tự | ĐX-API-27 | Đã áp dụng (T-08) ở `backend-architecture.md` 7.4 và `Backend-Conventions.md` 10.4; `Authorization.md` không sửa |

## 13. Hướng dẫn tích hợp

- **Sinh client Android:** dùng `openapi.yaml` (hoặc `openapi.json` do CI lưu thành artifact, KT 6.1). Giữ đúng `operationId`; tên đã ổn định theo mẫu `svXxx`, `quayXxx`, `qtXxx`, `authXxx`.
- **Địa chỉ máy chủ:** `servers` dùng biến `scheme` và `host`; đặt giá trị theo môi trường khi cấu hình client (ĐX-API-02).
- **Trường rỗng được:** các trường `nullable: true` luôn có mặt trong phản hồi (giá trị `null`), không bị bỏ khỏi JSON.
- **Phân biệt dạng nguồn giao và báo cáo:** `LapPhieuMuonRequest`, `KiemTraNguonRequest` (theo `nguon`) và `BaoCaoJson` (theo `loai`) là `oneOf` có `discriminator`; bộ sinh client cần hỗ trợ `oneOf` có `discriminator` (nếu không, dùng trực tiếp các schema thành phần như `GiaoTheoPhieuYeuCau`, `GiaoTheoDangKyCho`, `GiaoTrucTiep`, `BaoCaoTinhTrangKho`).
- **Báo cáo:** `dinhDang=xlsx` hoặc `csv` trả tệp nhị phân; `json` trả `BaoCaoJson`. Kiểu nội dung do `dinhDang` quyết định, không phải header `Accept`.
- **Ảnh:** URL ảnh tình trạng hết hạn sau 10 phút; không lưu URL lâu dài, lưu `maAnh`.
- **Múi giờ:** mọi `date-time` là UTC; hiển thị theo `Asia/Ho_Chi_Minh`.
- **Đường dẫn trùng dạng:** `GET /quay/phieu-muon/qua-han-30-ngay` là đường dẫn cố định, được ưu tiên hơn `GET /quay/phieu-muon/{ma}`; hãy đăng ký route cố định trước hoặc dùng ràng buộc mẫu `PM-...` cho `{ma}`.

## 14. Kiểm tra chất lượng đã thực hiện

| Hạng mục | Cách kiểm tra | Kết quả |
|---|---|---|
| Cấu trúc OpenAPI 3.0 | Bộ kiểm tra viết riêng: YAML không trùng khóa; mọi `$ref` phân giải; `operationId` duy nhất; tham số `path` khớp mẫu đường dẫn; mỗi thao tác có đúng một mã 2xx; `required` nằm trong `properties`; enum nullable có `null`; `discriminator` khớp `oneOf` và giá trị enum của từng schema | Không lỗi |
| Đủ endpoint | Đối chiếu danh sách KT 6.2 đến 6.5 cộng mười endpoint ĐX-API-25 với `paths` | Đủ, không thừa |
| Mã lỗi | Mỗi `maLoi` ghi ở phản hồi có trong bảng mã lỗi và đúng mã HTTP; mọi mã đều xuất hiện ở ít nhất một endpoint | Khớp |
| Enum | So sánh enum với `CHECK ... IN (...)` trong `schema.sql` | Khớp |
| Trường dữ liệu | Đối chiếu cột `schema.sql` với thuộc tính API; cột không xuất ra chỉ là nội bộ (mật khẩu băm, bộ đếm, khóa lưu trữ, khóa trùng lặp thông báo, tác nhân ghi nhật ký) | Không thiếu cột nghiệp vụ |
| Trích dẫn BR | Mọi `BR-nn` trích trong `openapi.yaml` có trong `BusinessRule.md` và đúng chủ đề | Khớp |
| Danh sách | Mỗi danh sách phân trang trả schema `*Page` và ghi thứ tự mặc định; mật khẩu `minLength` 8; cột báo cáo trong `openapi.yaml` khớp mục 9 | Khớp |
| Tài liệu Markdown | Bảng đủ cột, không còn ký tự thừa, mọi `operationId` và đường dẫn nêu trong tài liệu tồn tại trong `openapi.yaml` | Khớp |

Việc kiểm tra bằng công cụ OpenAPI bên ngoài (ví dụ `openapi-spec-validator`, `redocly lint`) chưa chạy được trong môi trường tạo tài liệu (không có mạng); nên chạy một lần trong CI trước khi dùng sinh client.

## 15. Lịch sử thay đổi

| Phiên bản | Nội dung |
|---|---|
| 1.0.0 | Bản đầu cho giai đoạn 1: 78 thao tác, bám KT 6.2 đến 6.6 |
| 1.1.0 | Nhóm duyệt toàn bộ đề xuất: thêm mười endpoint đọc (ĐX-API-25), mã lỗi `MA_NV_TRUNG`, mật khẩu tối thiểu 8 ký tự, thứ tự mặc định, schema riêng sáu báo cáo; tổng 88 thao tác |
| 1.1.1 | 07/11/2026, bổ sung sau audit cuối (không đổi số phiên bản, không đổi endpoint, mã lỗi, ĐX-API): mục 5 dòng "Đăng ký chờ" ghi bước giữ chỗ chạy đồng bộ trong thao tác trả hoặc hoàn thành bảo trì, không do job (ĐX-KT-13); câu mở đầu mục 1 chỉ về quy tắc thẩm quyền ở KT 1.4. Trước đó, 06/11/2026, theo kế hoạch khắc phục xuyên tài liệu: thêm nguồn ĐX-KT-14 ở hàng "Mở khóa" của mục 7; sửa hai câu mô tả KT đã lỗi thời (mục 4.6, mục 10); mục 12 thêm cột Trạng thái, sửa dòng gán nhầm câu cấm tài liệu API soạn riêng cho `backend-architecture.md`, bổ sung bốn dòng cho ĐX-API-30. Không đổi endpoint, mã lỗi, ĐX-API; hợp đồng `openapi.yaml` giữ phiên bản 1.1.0 |
