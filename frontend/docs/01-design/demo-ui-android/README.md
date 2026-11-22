# Demo UI ứng dụng Android sinh viên

Bản HTML tĩnh để hình dung giao diện app sinh viên trước khi dựng layout XML. **Chỉ để tham khảo**:
nguồn gốc vẫn là `UI-Screens.md` (mục 5), `Wireframes.md` (mục 2.1, 5) và `Design-System.md`. Khi demo khác
tài liệu thì theo tài liệu.

## Cách xem

Mở trực tiếp bằng trình duyệt, không cần mạng hay máy chủ:

| Tệp | Nội dung |
|---|---|
| `index.html` | Mọi màn hình cùng lúc (30 khung 360 × 760 dp), chia nhóm; dưới mỗi khung ghi mã màn hình và trạng thái minh họa |
| `app.html` | Bấm thử chuyển màn như app: thẻ, nút, thanh điều hướng dưới, nút ‹; ô chọn để nhảy tới màn hoặc trạng thái bất kỳ |
| `giao-dien.css` | Token Android (`ds_colors.xml`, `ds_dimens.xml`, `ds_type.xml`) theo ĐX-DS-17, giữ tên token; 1 dp = 1 px |
| `man-hinh.js` | Nội dung từng màn hình |

## Phạm vi

SCR-CM-01, SCR-SV-01..14, DLG-SV-01 và các trạng thái chính: quyền mượn Bình thường/Tạm khóa, có dữ liệu/rỗng,
còn hàng/hết hàng, lỗi nhập, lỗi nghiệp vụ khi gửi phiếu, phiếu Chờ duyệt/Đã duyệt (đếm ngược)/Từ chối, hộp xác
nhận Rời danh sách chờ, Hồ sơ mượn ở chế độ "bị chặn". Nhãn trạng thái dùng cùng tên và tông với web
(`frontend/web/src/shared/labels/trangThai.ts`).

## Lưu ý

- Dữ liệu là ví dụ. Mọi giá trị nghiệp vụ (hạn trả, hạn giữ chỗ, phí, giới hạn số giờ, số lượng khả dụng, quyền
  mượn, lý do) app lấy từ máy chủ, không viết cứng.
- Màu và bo góc theo token ĐX-DS-17 (nút chính xanh chanh chữ tối, liên kết xanh lá đậm, bo 6/10/16 dp).
- Biểu tượng vẽ lại theo Material Symbols (Design-System Phụ lục A); app dùng vector drawable tương ứng.
