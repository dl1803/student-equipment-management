# Bản đồ nghiệp vụ MuonTra, kết quả chuẩn và đầu vào quyết định G1, G2 (D01-V)

Cập nhật lần đầu: 13/11/2026 · Chủ sở hữu: Nguyễn Đại Vương · Trạng thái: **chưa được code**

Quy ước: dữ liệu mẫu do D01-V chọn, kết quả mong đợi suy ra từ công thức trong nguồn (KT 8, BR). Điều nguồn chưa nêu ghi **OPEN**. Thời gian ghi theo giờ Việt Nam (UTC+7), lưu và so sánh bằng UTC.

## 1. Phân rã MuonTra

MuonTra gồm 27 thao tác API trong tổng 88 (đối chiếu `C0-manifest-operation.csv`), sở hữu schema `muon_tra`.

| Nhóm | UC | Thao tác API | Bảng ghi | Ngày (V) | Đọc qua Contracts của |
|---|---|---|---|---|---|
| Tra cứu và dữ liệu chọn | UC-01 | `GET /sv/tra-cuu/thiet-bi`; `GET /sv/tra-cuu/thiet-bi/{maLoai}`; `GET /sv/phong-thiet-bi`; `GET /sv/loai-thiet-bi`; `GET /quay/loai-thiet-bi` | Không ghi | D05 | DanhMuc, TaiKhoan |
| Yêu cầu đặt trước và duyệt | UC-02, UC-06 | `POST /sv/phieu-yeu-cau-muon`; `GET /sv/phieu-yeu-cau-muon`; `GET /sv/phieu-yeu-cau-muon/{ma}`; `GET /quay/phieu-yeu-cau-muon`; `GET /quay/phieu-yeu-cau-muon/{ma}/kiem-tra`; `POST /quay/phieu-yeu-cau-muon/{ma}/duyet`; `POST /quay/phieu-yeu-cau-muon/{ma}/tu-choi` | `phieu_yeu_cau_muon`, `chi_tiet_yeu_cau_muon` | D06, D07, D10 | SinhVien, QuyDinh, DanhMuc, TaiKhoan, ThongBao |
| Danh sách chờ và FIFO | UC-03, UC-10 | `POST /sv/dang-ky-cho`; `GET /sv/dang-ky-cho`; `POST /sv/dang-ky-cho/{ma}/roi`; `GET /quay/danh-sach-cho`; `POST /quay/danh-sach-cho/{ma}/chuyen-luot` | `dang_ky_cho` | D12, D13 | SinhVien, DanhMuc, TaiKhoan, ThongBao |
| Giao thiết bị (ba nguồn) | UC-07 | `POST /quay/phieu-muon/kiem-tra-nguon`; `POST /quay/phieu-muon/kiem-tra-thiet-bi`; `POST /quay/phieu-muon` | `phieu_muon`, `chi_tiet_phieu_muon` | D08, D09, D10 | SinhVien, QuyDinh, DanhMuc, TaiKhoan, ThongBao |
| Lịch sử và đọc phiếu | UC-05 | `GET /sv/phieu-muon`; `GET /sv/phieu-muon/{ma}`; `GET /quay/phieu-muon`; `GET /quay/phieu-muon/{ma}`; `GET /quay/phieu-muon/qua-han-30-ngay` | Không ghi | D11 | TaiKhoan |
| Nhận trả và kiểm tra | UC-08 | `GET /quay/thiet-bi/{maThietBi}/ban-giao`; `POST /quay/phieu-muon/{ma}/ket-qua-kiem-tra` | `chi_tiet_phieu_muon`, `phieu_muon` | D14, D15, D16 | DanhMuc (chuyển tình trạng thiết bị), QuyDinh, TaiKhoan, ThongBao |

Job của MuonTra (KT 9.2): J1 `NhacHanTra` (D17), J2 `DanhDauQuaHan` (D17), J5 `HetHieuLucPhieuChoDuyet` (D17), J4 `HetHanGiuCho` (D18, D19). Tổng thao tác API: 5 + 7 + 5 + 3 + 5 + 2 = 27.

Cùng luồng nhưng **không thuộc** MuonTra (theo kế hoạch và KT):

| Thao tác | Module sở hữu | Căn cứ |
|---|---|---|
| `GET /quay/sinh-vien/the/{maThe}`; `GET /sv/toi` | SinhVien (Khôi, gói C4) | Kế hoạch D07 ghép dữ liệu qua bộ đọc D02-V và D03-K |
| `GET /quay/phieu-muon/{ma}/de-xuat-bien-ban`; các thao tác biên bản; `POST /quay/sinh-vien/{maSV}/xac-nhan-nhan-du-thiet-bi` | ViPham (Khôi) | UC-09; KT 8.6; kế hoạch D14-K |
| `GET /quay/khung-phat` | QuyDinh (Lợi) | Kế hoạch D06-N |
| `GET /quay/bao-tri/cho-tiep-nhan-sua-chua` | BaoTri (Lợi) | KT 8.1 |

MuonTra **dùng** nhưng không sở hữu: dữ liệu SV và nhóm (SinhVien), phòng, loại, thiết bị và tình trạng (DanhMuc), quy định mượn, tham số, khung phạt (QuyDinh), quyền phòng và NV (TaiKhoan), outbox thông báo (ThongBao). Không tham chiếu `ViPham`; giao tiếp qua sự kiện `GiuChoHetHan` và `IPhieuMuonReader`.

## 2. Kết quả chuẩn

### 2.1 Trạng thái (KT 8.1, DDL)

| Mã ca | Trạng thái đầu | Thao tác | Kết quả mong đợi |
|---|---|---|---|
| TT-01 | Yêu cầu `CHO_DUYET` | `Duyet` | `DA_DUYET`; `ma_nv_duyet`, `thoi_diem_duyet`, `han_giu_cho` đều có giá trị (ràng buộc `ck_yc_duyet`) |
| TT-02 | Yêu cầu `DA_DUYET` | `Duyet` lần hai | 409 `TRANG_THAI_KHONG_HOP_LE` |
| TT-03 | Yêu cầu `CHO_DUYET` | `TuChoi` thiếu lý do | Không đổi trạng thái; 400 `DU_LIEU_KHONG_HOP_LE` theo KT 6.6 (chưa đối chiếu schema `TuChoiRequest`) |
| TT-04 | Yêu cầu `CHO_DUYET` | `TuChoi` có lý do | `TU_CHOI`, lưu lý do, không ghi vi phạm (BR-64) |
| TT-05 | Đăng ký `DANG_CHO` | `ThongBaoDenLuot` | `DA_THONG_BAO`; `han_giu_cho` có giá trị (`ck_dk_han_giu_cho`) |
| TT-06 | Đăng ký `DA_THONG_BAO` | `Roi` | `DA_HUY`; không ghi vi phạm |
| TT-07 | Đăng ký `DA_THONG_BAO` quá hạn | `HetHan` | `HET_HAN`; phát `GiuChoHetHan` |
| TT-08 | Phiếu mượn hai chi tiết, một chi tiết đã có kết quả | `XetDaTra` | Phiếu giữ `DANG_MUON` hoặc `QUA_HAN` |
| TT-09 | Phiếu mượn hai chi tiết, cả hai đã có kết quả (một là `MAT`) | `XetDaTra` | `DA_TRA` (BR-53) |
| TT-10 | Thiết bị `SAN_SANG` | `Giao` | `DANG_DUOC_MUON` |
| TT-11 | Thiết bị `DANG_DUOC_MUON` | `Giao` bởi giao dịch thứ hai | Cập nhật có điều kiện trả 0 dòng; 409 `THIET_BI_DA_DUOC_GIAO`; hoàn tác toàn bộ |
| TT-12 | Thiết bị `DANG_DUOC_MUON` | Nhận trả `NGUYEN_VEN`, `HU_HONG` hoặc `THIEU_PHU_KIEN`, `MAT` | `SAN_SANG`; `DANG_BAO_TRI_SUA_CHUA`; `DA_BAO_MAT` (BR-51) |
| TT-13 | Chi tiết kết quả `MAT` | Ghi nhận | `ngay_gio_tra_thuc_te` rỗng, `ma_khung` rỗng, `ma_nv_nhan_tra` có giá trị (`ck_ctpm_ket_qua_dong_bo`) |
| TT-14 | Mọi trạng thái | Quan sát | Không tồn tại tình trạng thiết bị "Chờ xác nhận" (kế hoạch D02) |

### 2.2 Thời gian (KT 8.5, BR-42, BR-57, BR-61)

Quy ước: `anHanPhut` = 15 (mặc định `quy_dinh.tham_so_he_thong`).

| Mã ca | Dữ liệu | Kết quả mong đợi |
|---|---|---|
| TG-01 | Hạn trả 16/11/2026 10:00; trả 10:15 | 0 ngày trễ (đúng mốc ân hạn) |
| TG-02 | Hạn trả 16/11/2026 10:00; trả 10:16 | 1 ngày (`max(1, 0)`) |
| TG-03 | Hạn trả 16/11/2026 23:50; trả 17/11/2026 00:05 | 0 ngày (đúng 15 phút) |
| TG-04 | Hạn trả 16/11/2026 23:50; trả 17/11/2026 00:06 | 1 ngày (qua nửa đêm giờ Việt Nam) |
| TG-05 | Hạn trả `2026-11-16T16:00:00Z`; trả `2026-11-16T17:30:00Z` | 1 ngày: theo giờ Việt Nam là 23:00 ngày 16 và 00:30 ngày 17. Tính theo ngày UTC sẽ ra 0 là **sai** |
| TG-06 | Hạn trả 16/11/2026 10:00; trả 19/11/2026 09:00 | 3 ngày |
| TG-07 | Hạn trả ngày lịch 14/10/2026; hôm nay ngày lịch 13/11/2026 | Chênh 30 ngày, **chưa** `> 30` nên chưa thuộc diện mất |
| TG-08 | Như TG-07, hôm nay 14/11/2026 | Chênh 31 ngày, thuộc diện quá 30 ngày |
| TG-09 | Giờ đóng cửa 21:00; giao 18:00; thời gian mượn 5 giờ | Hạn trả 21:00 (`min(23:00, 21:00)`) |
| TG-10 | Giờ đóng cửa 21:00; giao 15:00; thời gian mượn 5 giờ | Hạn trả 20:00 |
| TG-11 | Giờ đóng cửa 21:00; giao 21:00 | 422 `GIAO_SAU_GIO_DONG_CUA` |
| TG-12 | `gio_mo_cua`, `gio_dong_cua` chưa cấu hình | 409 `CHUA_CAU_HINH_GIO_QUAY` |
| TG-13 | Nhóm tối đa 5 giờ; loại A có ngoại lệ 3 giờ; phiếu gồm loại A và B | Mức áp dụng 3 giờ (nhỏ nhất theo loại, BR-25) |
| TG-14 | Phiếu yêu cầu gửi 16/11/2026 09:00, chưa duyệt | Hết hiệu lực từ 17/11/2026 00:00 giờ Việt Nam (BR-30, J5), không vi phạm |

### 2.3 Giữ chỗ và khả dụng (KT 8.2, 8.3, 9.2, BR-29 đến BR-37)

Dữ liệu nền cho GC-01 đến GC-03: phòng P, loại X có 7 thiết bị: 5 thiết bị `SAN_SANG` và `ngung_su_dung = false`; 1 thiết bị `SAN_SANG` nhưng `ngung_su_dung = true`; 1 thiết bị `DANG_DUOC_MUON`. Yêu cầu A `DA_DUYET` số lượng 2 có `han_giu_cho` 09:30:00. Đăng ký B `DA_THONG_BAO` có `han_giu_cho` 09:30:00. Yêu cầu C `CHO_DUYET` số lượng 3 và yêu cầu D `DA_NHAN` số lượng 1 không tính.

| Mã ca | Thời điểm hiện tại | Kết quả mong đợi |
|---|---|---|
| GC-01 | 09:29:59 | `khaDung = 5 − 2 − 1 = 2` |
| GC-02 | 09:30:00 | `khaDung = 5`: điều kiện `han_giu_cho > now` không còn đúng. Job J4 chọn `han_giu_cho < now` nên chuyển `HET_HAN` ở lần quét sau (O4) |
| GC-03 | 09:30:01, J4 chưa chạy | `khaDung = 5`; giao theo giữ chỗ cũ trả 409 `GIU_CHO_HET_HAN` |
| GC-04 | `khaDung = 1`, hai NV cùng duyệt yêu cầu số lượng 1 | Một thành công, một 409 `SO_LUONG_KHA_DUNG_THAY_DOI`; sau đó `khaDung ≥ 0` |
| GC-05 | Ba đăng ký `DANG_CHO` của (P, X) lúc 08:00, 08:05, 08:10; một thiết bị X về `SAN_SANG` | Đăng ký 08:00 → `DA_THONG_BAO`, `han_giu_cho` = thời điểm cấp + 30 phút; hai đăng ký còn lại giữ `DANG_CHO`; `khaDung` sau cấp = 0 |
| GC-06 | Như GC-05, đăng ký 08:00 để quá hạn | `HET_HAN`, một vi phạm `KHONG_DEN_NHAN`, đăng ký 08:05 → `DA_THONG_BAO` ngay trong cùng giao dịch |
| GC-07 | Như GC-06 nhưng không còn ai `DANG_CHO` | Không cấp lượt; `khaDung` tăng 1 |
| GC-08 | Đăng ký `DA_THONG_BAO` rời | `DA_HUY`, không vi phạm, người kế tiếp được cấp lượt trong cùng giao dịch |
| GC-09 | Chờ tại phòng Q đăng ký sớm hơn chờ tại phòng P; thiết bị về tại P | Không cấp cho đăng ký tại Q (FIFO theo từng loại, từng phòng, BR-34) |

### 2.4 Tiền (KT 8.5, 8.7, BR-43, BR-58 đến BR-60)

| Mã ca | Dữ liệu | Kết quả mong đợi |
|---|---|---|
| TI-01 | Laptop 50.000 đồng mỗi ngày và máy chiếu 30.000 đồng mỗi ngày, cùng phiếu, cả hai trễ 3 ngày, giá trị hiện hành đủ lớn | `(50.000 + 30.000) × 3 = 240.000` đồng (NFR-09) |
| TI-02 | Phí 50.000 mỗi ngày, trễ 3 ngày, giá trị hiện hành 120.000 | `min(150.000, 120.000) = 120.000` |
| TI-03 | Như TI-02, giá trị hiện hành 150.000 | 150.000 (chạm đúng trần) |
| TI-04 | Trễ trong ân hạn (0 ngày) | 0 đồng |
| TI-05 | Hư hỏng, giá trị hiện hành 100 đồng, tỷ lệ 12,5% | 12,5 làm tròn `AwayFromZero` thành 13 đồng (làm tròn về số chẵn sẽ ra 12 là **sai**) |
| TI-06 | Hư hỏng, giá trị 1.000.000 đồng, khung 15%, 40%, 80% | 150.000; 400.000; 800.000 (BR-59) |
| TI-07 | Mất, giá trị hiện hành tại thời điểm mất 8.000.000 | 8.000.000 (100%, BR-60) |
| TI-08 | Giao khi phí loại là 50.000; sau đó quản trị đổi phí loại thành 70.000 | Phiếu đã giao vẫn tính 50.000 (đọc `phi_tre_moi_ngay` của chi tiết); phiếu giao sau tính 70.000 (BR-43, KT 8.7) |
| TI-09 | Trả từng phần: thiết bị 1 trả 18/11, thiết bị 2 trả 20/11, hạn trả 16/11 | Số ngày trễ tính riêng theo thời điểm trả thực tế của từng thiết bị |

## 3. Đầu vào cho quyết định G1 và G2

Cả hai do nhóm quyết định, hạn D02 (register C0). Bảng dưới chỉ liệt kê tình huống; cột kết quả để **OPEN** nếu nguồn chưa nêu.

### 3.1 G1 — SV trên thẻ và nguồn giao

Nguồn đã có: `GET /quay/sinh-vien/the/{maThe}` trả thông tin SV, `hopLe`, `dsLyDoKhongHopLe`; `POST /quay/phieu-muon/kiem-tra-nguon` kiểm còn giữ chỗ (`hanGiuCho > now`) và điều kiện SV; các thao tác kiểm tra trả 200 kèm `hopLe` và `dsLyDo` (ĐX-API-08). Bước kiểm tra không lưu trạng thái phiên (kế hoạch D08).

| Mã ca | Tình huống | Kết quả |
|---|---|---|
| G1-01 | Thẻ quét là SV-A, nguồn là phiếu yêu cầu hoặc đăng ký của SV-A | Hợp lệ (cùng SV) |
| G1-02 | Thẻ quét là SV-A nhưng `hopLe = false` (thẻ hết hạn, khóa quyền mượn, không còn theo học, vượt giới hạn) | Dừng và hiển thị lý do (BR-38) |
| G1-03 | Thẻ quét là SV-B, nguồn thuộc SV-A | **OPEN**: dừng, quét lại, đổi nguồn hay phương án khác |
| G1-04 | Quét thẻ SV-B sau khi đã quét SV-A và nhập thiết bị | **OPEN** |
| G1-05 | Nguồn trực tiếp (không có SV sẵn) | SV lấy từ thẻ; **OPEN**: ghi danh tính cuối thế nào |

Câu hỏi cần nhóm chốt: danh tính cuối; mã lỗi và HTTP khi khác SV (KT 6.6 chưa có mã riêng, thêm mã sẽ ảnh hưởng 32 mã lỗi và YAML); cách xóa ngữ cảnh; dữ liệu client gửi khi lập phiếu.

### 3.2 G2 — Lượng giao và giữ chỗ

Dữ liệu nền: yêu cầu của SV-A `DA_DUYET` còn hạn, loại X số lượng 2 và loại Y số lượng 1.

Nguồn đã có: KT 8.4 ghi nguồn chuyển `DA_NHAN` sau khi giao; BR-40 ghi thiết bị không đạt thì từ chối, không đưa vào phiếu; mỗi đăng ký chờ giữ đúng một thiết bị (ĐX-KT-05). Register C0: chưa chọn exact-match hay giao một phần.

| Mã ca | Tình huống | Kết quả |
|---|---|---|
| G2-01 | Giao X × 2 và Y × 1 | Đủ; phiếu mượn được lập, yêu cầu → `DA_NHAN` (KT 8.4) |
| G2-02 | Giao X × 1 và Y × 1 (thiếu) | **OPEN**: từ chối toàn bộ hay cho giao một phần |
| G2-03 | Giao X × 3 (thừa) | **OPEN** |
| G2-04 | Quét thiết bị loại Z (sai loại) | 422 `THIET_BI_SAI_LOAI`, không đưa vào phiếu (BR-40); phần còn lại **OPEN** theo G2-02 |
| G2-05 | Quét hai lần cùng một mã thiết bị | **OPEN**: mã lỗi (nguồn chưa nêu); DB chặn bằng `uq_ctpm_phieu_thiet_bi` |
| G2-06 | Đã giao một phần, giao tiếp phần còn lại | **OPEN**. Ràng buộc DDL `uq_pm_ma_yeu_cau` và `uq_pm_ma_dang_ky` cho phép mỗi yêu cầu hoặc đăng ký có tối đa **một** phiếu mượn; "giao lại" bằng phiếu thứ hai cần đổi DDL và migration |
| G2-07 | Phần giữ chỗ chưa giao (khi giao ít hơn) | **OPEN**: trả về khả dụng ngay hay giữ đến `han_giu_cho` |
| G2-08 | Trạng thái yêu cầu hoặc đăng ký sau khi giao một phần | **OPEN** |
| G2-09 | Giao khi `han_giu_cho <= now` | 409 `GIU_CHO_HET_HAN` (UC-07 E2) |

Hệ quả cần nhóm cân nhắc (chỉ nêu dữ kiện): phương án giao **đúng và đủ** không cần đổi DDL; phương án **giao một phần** buộc chốt G2-05 đến G2-08 và có thể cần đổi DDL.

### 3.3 Quy tắc cho bản giả (kế hoạch D01-V)

Bản giả trong `tests/` chỉ là công cụ: kết quả mong đợi do từng ca kiểm thử khai báo. Bản giả không được chứa nhánh "khác SV thì dừng" hay "thiếu thì cho giao một phần". Ca còn OPEN không được ghi là đạt; đánh dấu bỏ qua kèm lý do và mã G1/G2.

## 4. Danh sách OPEN

| Mã | Nội dung | Người chốt | Hạn |
|---|---|---|---|
| O1 | Vị trí C2 (khả dụng và khóa); cách đọc nhiều schema trong giao dịch ghi | Vương, Khôi | Trước D03 |
| O2 | Vị trí bộ tính hạn trả (MuonTra dùng, không tham chiếu ViPham) | Vương, Khôi | Trước D04 |
| O3 | Tránh cạnh SinhVien → MuonTra khi hồ sơ SV cần số đang cầm | Khôi, Vương | Trước D02 |
| O4 | Hành vi tại `han_giu_cho = now` (`> now` so với `< now` của J4) | Vương | D03, D18 |
| O5 | Giới hạn tổng có tính số đã giữ chỗ chưa giao | Nhóm | Trước D06 |
| O6 | FIFO: cùng thời điểm, nhiều lượt cùng lúc, `so_thu_tu_cho` | Vương | D13 |
| O7 | Port MAT: ai đổi tình trạng thiết bị | Vương, Lợi, Khôi | D15 |
| O8 | G1 và G2 | Nhóm | D02 |
| O9 | Chú thích XML trên Contracts (CV 19.2) với quy tắc không comment | Nhóm | Trước D02 |
