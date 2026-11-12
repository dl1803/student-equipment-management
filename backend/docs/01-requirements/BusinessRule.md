# QUY TẮC NGHIỆP VỤ (BUSINESS RULES)

## Hệ thống quản lý mượn trả thiết bị học tập cho sinh viên

| Mục | Nội dung |
|---|---|
| Phiên bản | 1.0 |
| Ngày | 06/11/2026 (bổ sung thông tin phiên bản; nội dung quy tắc không đổi) |

---

## 0. Giới thiệu và cách đọc tài liệu

**Mục đích.** Tập hợp các quy tắc nghiệp vụ mà hệ thống phải tuân thủ khi quản lý đăng ký, mượn, trả, bảo trì và thanh lý thiết bị học tập. Tài liệu chỉ tổng hợp lại nội dung đã có trong các tài liệu nguồn, không thêm quy tắc mới.

**Tài liệu nguồn.**

| Ký hiệu | Tài liệu |
|---|---|
| Đề bài | Đề bài gốc (PDF) "Hệ thống quản lý mượn trả thiết bị học tập cho sinh viên" |
| SRS | SRS.md – Tài liệu đặc tả yêu cầu phần mềm |
| UC | Usecase.md – Mô hình Usecase (v1.2): UC-01 … UC-24 |
| PT | Phan-tich_Nhóm_13.docx – Hồ sơ phân tích (mô hình miền, trạng thái, tuần tự, hoạt động) |
| BM | Biểu_mẫu_OOAD_Nhóm_13.xlsx – các sheet P1_01 (Tầm nhìn), P2_02 (Từ điển miền: GL), P2_03 (Quy trình: BP), P2_04 (Thu thập yêu cầu: EL), P2_05 (Đặc tả yêu cầu: FR, NFR), P2_07 (Thay đổi yêu cầu: CR), P3A_04 (Danh mục UML: MD) |

**Nguyên tắc xử lý nguồn.**

1. Khi đề bài gốc khác với yêu cầu đã được chấp thuận sau đó (CR-xx), tài liệu này lấy theo yêu cầu mới nhất; các thay đổi được liệt kê ở mục 19.
2. Mỗi quy tắc có cột **Nguồn** để truy vết về mã yêu cầu (FR, NFR), use case (UC), thuật ngữ (GL), thu thập yêu cầu (EL) hoặc đề nghị thay đổi (CR).
3. Nhãn **[ĐX-xx]** đánh dấu giá trị hoặc cách hiểu **không có trong tài liệu nguồn gốc** mà do SRS quyết định bổ sung (SRS mục 2.8). Các quyết định này được áp dụng làm giá trị mặc định trong tài liệu và được liệt kê đầy đủ ở mục 20; khi nguồn gốc được cập nhật thì chỉnh lại theo nguồn.

---

## 1. Phạm vi và ràng buộc nền

| Mã | Quy tắc | Nguồn |
|---|---|---|
| BR-01 | Ứng dụng chỉ dùng để đặt trước (tra cứu, đặt trước, đăng ký chờ, gia hạn, hủy phiếu yêu cầu, xem lịch sử, phản hồi tình trạng thiết bị). Việc giao và nhận thiết bị luôn thực hiện trực tiếp tại quầy thiết bị. | CR-11, EL-24, FR-03, SRS 2.5 |
| BR-02 | Ngoài phạm vi hệ thống: tích hợp quét mã tự động phần cứng/RFID; tự động thẩm định hoặc phân tích hình ảnh hư hỏng bằng AI (việc đánh giá hư hỏng do nhân viên thực hiện); mượn/trả hoàn toàn trực tuyến; thanh toán trực tuyến; tạo và nhập tài khoản, dữ liệu sinh viên. | BM P1_01, UC-08, UC-09 |
| BR-03 | Sinh viên nhận và trả thiết bị tại phòng thiết bị. Phòng thiết bị thuộc một cơ sở và gồm các kệ, tủ. | GL-35, FR-18 |

---

## 2. Vai trò và phân quyền

| Mã | Quy tắc | Nguồn |
|---|---|---|
| BR-04 | Hệ thống có đúng ba vai trò: **Sinh viên**, **Nhân viên quản lý thiết bị**, **Quản trị viên**. Mỗi vai trò chỉ thao tác trong phạm vi chức năng được cấp; người dùng không truy cập được chức năng ngoài phạm vi quyền. | Đề bài mục 7, NFR-01 |
| BR-05 | Nhân viên chỉ thấy và thao tác với thiết bị, phiếu yêu cầu, phiếu mượn của **phòng thiết bị được Quản trị viên cấp quyền**. Truy cập phòng chưa được cấp quyền bị từ chối. | FR-34, NFR-01, UC-06 E2, UC-18 E1 |
| BR-06 | Sinh viên chỉ xem được dữ liệu của chính mình; truy cập dữ liệu của người khác bị từ chối. | FR-32, UC-05 E1, UC-19 |
| BR-07 | Chỉ Quản trị viên được hủy hoặc điều chỉnh biên bản vi phạm; mọi thay đổi được ghi nhật ký. | GL-34, UC-09 A2 |

**Phạm vi chức năng chính của từng vai trò** (theo đề bài mục 2 và các use case):

| Vai trò | Chức năng chính |
|---|---|
| Sinh viên | Tra cứu thiết bị; đặt trước; đăng ký chờ; gia hạn; hủy phiếu yêu cầu; xem lịch sử mượn/trả, biên bản và phí phạt; nhận thông báo; phản hồi tình trạng thiết bị sau khi trả |
| Nhân viên quản lý thiết bị | Duyệt/từ chối phiếu yêu cầu; giao thiết bị và lập phiếu mượn; nhận trả và kiểm tra; lập biên bản vi phạm, ghi nhận thu phạt; xử lý danh sách chờ; bảo trì, sửa chữa; nhắc nhở sinh viên |
| Quản trị viên | Quản lý danh mục thiết bị, phòng thiết bị; cấu hình quy định mượn, phí, khung phạt, tham số; quản lý tài khoản nhân viên và phân quyền theo phòng; quản lý nhà cung cấp, bảo hành; lập quyết định thanh lý; xem báo cáo, nhật ký; gỡ cấm mượn dài hạn |

---

## 3. Tài khoản và xác thực

| Mã | Quy tắc | Nguồn |
|---|---|---|
| BR-08 | Tài khoản và dữ liệu sinh viên (mã số sinh viên, họ tên, lớp, khoa, nhóm sinh viên, hạn thẻ, trạng thái học, học kỳ) do **nhà trường cung cấp**. Sinh viên không tự đăng ký; hệ thống không tạo hoặc nhập danh sách sinh viên; hệ thống không lưu mật khẩu sinh viên. | CR-13, FR-33, FR-35, NFR-05, UC-19 A1 |
| BR-09 | Tài khoản nhân viên và quản trị viên do hệ thống cấp (nhân viên do Quản trị viên tạo). Mật khẩu được lưu dạng băm, không lưu dạng rõ. | FR-33, FR-34, NFR-05, UC-14, UC-19 |
| BR-10 | Nhập sai mật khẩu quá số lần quy định (`soLanDangNhapSaiToiDa`, Quản trị viên cấu hình, phải lớn hơn 0; mặc định **5 lần** [ĐX-02]) thì tạm khóa đăng nhập. Tài khoản bị khóa không đăng nhập được. | FR-33, FR-34, UC-19 E2, E3, PT 1.5.7 |
| BR-11 | Nhóm sinh viên do nhà trường cung cấp, Quản trị viên không tạo nhóm. Các nhóm nêu trong nguồn: sinh viên đại học; học viên cao học; sinh viên làm đồ án tốt nghiệp. | GL-29, UC-13 |

---

## 4. Thiết bị và danh mục

| Mã | Quy tắc | Nguồn |
|---|---|---|
| BR-12 | Mỗi **thiết bị** là một đơn vị vật lý có mã riêng, duy nhất và thuộc đúng một loại thiết bị; một loại có nhiều thiết bị. Lịch sử mượn, trả, bảo trì được theo dõi độc lập theo từng mã thiết bị. | Đề bài mục 3, GL-04, GL-05, GL-06, UC-12 |
| BR-13 | Mã thiết bị được in chìm trên thiết bị; nhân viên quét mã, hoặc nhập tay khi không quét được. Mã trùng bị từ chối. | GL-06, FR-10, UC-12 E1 |
| BR-14 | Thông tin quản lý của mỗi thiết bị: mã, tên, mô tả, loại, hãng sản xuất, thông số kỹ thuật, hình ảnh, ngày nhập kho, nhà cung cấp, giá trị, thời hạn bảo hành, vị trí lưu trữ (phòng, kệ, tủ) và tình trạng hiện tại. | Đề bài mục 3, FR-18, UC-12 |
| BR-15 | Tình trạng thiết bị chỉ nhận một trong năm giá trị: **Sẵn sàng cho mượn**, **Đang được mượn**, **Đang bảo trì/sửa chữa**, **Đã thanh lý**, **Đã báo mất**. | GL-08, PT 1.5.10 |
| BR-16 | Chỉ thiết bị **Sẵn sàng cho mượn** mới được tính vào số lượng khả dụng và được đưa vào phiếu mượn. Thiết bị hư hỏng tính là hết hàng; thiết bị đang bảo trì/sửa chữa không thể chọn đưa vào phiếu mượn. | GL-08, FR-17, UC-01 A3, UC-07 A3 |
| BR-17 | **Giá trị hiện hành** của thiết bị là giá trị mới nhất do Quản trị viên cập nhật; mỗi lần cập nhật được lưu lịch sử; dùng để tính bồi thường hư hỏng và mất. | GL-33, FR-18 |
| BR-18 | Loại thiết bị hoặc thiết bị đã có lịch sử không bị xóa hẳn, chỉ ngưng sử dụng. Thiết bị đã thanh lý vẫn giữ lịch sử. | FR-18, UC-12 E2, UC-15 |

---

## 5. Tra cứu và số lượng khả dụng

| Mã | Quy tắc | Nguồn |
|---|---|---|
| BR-19 | Sinh viên tìm và lọc thiết bị theo tên, loại thiết bị, phòng thiết bị, tình trạng còn/hết hàng. Chi tiết thiết bị hiển thị thông số kỹ thuật, hình ảnh, phòng quản lý, số lượng khả dụng; nếu hết hàng thì hiển thị ngày dự kiến có sẵn tại phòng đó. | FR-01, FR-02, UC-01 |
| BR-20 | **Số lượng khả dụng** = số thiết bị Sẵn sàng cho mượn của một loại tại một phòng thiết bị, **đã trừ số lượng đang giữ chỗ**. Số lượng khả dụng bằng 0 thì sinh viên đăng ký chờ. | GL-31, FR-02 |

---

## 6. Đặt trước và phiếu yêu cầu mượn

| Mã | Quy tắc | Nguồn |
|---|---|---|
| BR-21 | Sinh viên gửi phiếu yêu cầu mượn (đặt trước) gồm phòng thiết bị, loại thiết bị, số lượng và thời gian mượn dự kiến (tính theo giờ). Một phiếu chọn được nhiều loại thiết bị. Phiếu có mã duy nhất, trạng thái **Chờ duyệt**. | FR-03, UC-02 |
| BR-22 | Phiếu **Chờ duyệt** chưa giữ chỗ và chưa gán thiết bị cụ thể. | UC-02, GL-09, SRS 7.2 |
| BR-23 | Sinh viên không được đặt trước hoặc mượn khi: thẻ hết hạn (`hanThe`); không còn theo học (tốt nghiệp, thôi học, đình chỉ); quyền mượn bị khóa. Hệ thống không cho gửi và hiển thị lý do. | GL-36, FR-04, FR-35, UC-02 E1, PT 1.5.4 |
| BR-24 | **Giới hạn số thiết bị mượn cùng lúc** tính trên **tổng số thiết bị** sinh viên đang cầm, không phân biệt loại thiết bị, cấu hình theo nhóm sinh viên. Khi giao: số đang cầm cộng số giao mới không được vượt giới hạn của nhóm. Vượt giới hạn thì từ chối và nêu rõ điều kiện không đạt. | CR-06, EL-19, FR-04, FR-20, UC-02 A2, UC-07 |
| BR-25 | **Thời gian mượn tính bằng giờ** và không vượt mức tối đa của nhóm sinh viên (5 giờ; nhóm ưu tiên 10 giờ, xem mục 14). Có thể đặt ngoại lệ theo loại thiết bị; phiếu gồm nhiều loại thì lấy mức nhỏ nhất theo các loại trong phiếu. Vượt mức thì từ chối và nêu mức tối đa cho phép. | CR-12, FR-04, FR-20, UC-02 A3, GL-12 |
| BR-26 | Loại thiết bị hết hàng thì đề nghị Đăng ký chờ cho loại đó; các loại còn hàng trong cùng phiếu vẫn đặt được. | UC-02 A1 |
| BR-27 | Sinh viên được hủy phiếu yêu cầu đang **Chờ duyệt** hoặc **Đã duyệt** (chưa nhận thiết bị). Phiếu chuyển **Đã hủy**, giữ chỗ (nếu có) được giải phóng, **không tính vi phạm**. | FR-29, UC-20 |

---

## 7. Duyệt phiếu yêu cầu và giữ chỗ

| Mã | Quy tắc | Nguồn |
|---|---|---|
| BR-28 | Nhân viên duyệt hoặc từ chối phiếu yêu cầu thuộc phòng mình. Trước khi duyệt, hệ thống kiểm tra lại điều kiện sinh viên (thẻ, nhóm, quyền mượn, giới hạn tổng số thiết bị) và số lượng khả dụng (đã trừ giữ chỗ). Không đủ điều kiện hoặc không đủ số lượng thì chặn duyệt; nhân viên từ chối kèm lý do. Sinh viên được thông báo kết quả. | FR-30, FR-04, FR-24, UC-06 |
| BR-29 | Khi duyệt, hệ thống **giữ chỗ 30 phút** (`thoiGianGiuChoPhut`) số lượng từng loại thiết bị trong phiếu. **Giữ chỗ** là dành sẵn số lượng thiết bị cho sinh viên để đến quầy nhận. | GL-30, FR-30, UC-06 |
| BR-30 | Phiếu yêu cầu chưa được duyệt trong ngày thì tự hết hiệu lực (**Hết hạn**), không tính vi phạm. | FR-30, UC-06 E1, UC-17 |
| BR-31 | Đã duyệt nhưng sinh viên không đến nhận trong 30 phút kể từ khi duyệt: hệ thống tự hủy giữ chỗ, phiếu chuyển **Hết hạn**, ghi **1 vi phạm** "Không đến nhận". | FR-28, UC-06 E3, UC-17, CR-11 |
| BR-32 | Khi giữ chỗ hết hạn, hệ thống chuyển lượt cho sinh viên kế tiếp trong danh sách chờ của loại thiết bị tại phòng đó; không còn ai chờ thì số lượng trở lại khả dụng. Việc chuyển lượt tự động, không yêu cầu sinh viên chủ động hủy. | FR-28, UC-10 E1 |

---

## 8. Danh sách chờ

| Mã | Quy tắc | Nguồn |
|---|---|---|
| BR-33 | Chỉ được đăng ký chờ khi loại thiết bị đã hết số lượng khả dụng tại phòng đó. Nếu còn hàng thì từ chối và đề nghị đặt trước. | FR-05, UC-03 E1 |
| BR-34 | Danh sách chờ được quản lý **theo từng loại thiết bị tại từng phòng thiết bị**, xếp theo thứ tự **đăng ký trước, phục vụ trước**. Mỗi đăng ký ghi mã đăng ký, phòng thiết bị, thời điểm đăng ký và số thứ tự chờ. | FR-05, GL-14, UC-03 |
| BR-35 | Khi thiết bị loại X được trả về nguyên vẹn tại một phòng và chuyển sang Sẵn sàng cho mượn, hệ thống giữ chỗ 30 phút cho sinh viên đứng đầu danh sách chờ của loại X tại phòng đó và thông báo ngay; đăng ký chuyển **Đã thông báo**. Thời gian giữ chỗ tính từ lúc thông báo. | FR-06, UC-10, GL-30 |
| BR-36 | Trong thời gian giữ chỗ: (a) sinh viên đến quầy, nhân viên xác nhận và giao thiết bị → đăng ký **Đã nhận**; (b) sinh viên từ chối nhận hoặc rời danh sách → **Đã hủy**, không tính vi phạm, sinh viên kế tiếp được thông báo ngay; (c) sinh viên báo không mượn nữa tại quầy → nhân viên chuyển lượt thủ công, đăng ký chuyển **Đã hủy**, không tính vi phạm; (d) quá 30 phút chưa nhận → **Hết hạn**, ghi 1 vi phạm, chuyển lượt. | FR-28, FR-29, UC-10, SRS 7.4 |
| BR-37 | Sinh viên rời danh sách chờ khi không còn nhu cầu: đăng ký chuyển **Đã hủy**, số thứ tự của các sinh viên phía sau được cập nhật, không tính vi phạm. | FR-29, UC-03 A2 |

---

## 9. Giao thiết bị và phiếu mượn

| Mã | Quy tắc | Nguồn |
|---|---|---|
| BR-38 | Khi giao, nhân viên quét mã thẻ sinh viên; hệ thống hiển thị họ tên, lớp, nhóm sinh viên, hạn thẻ, số thiết bị đang mượn, lịch sử nợ vi phạm và trạng thái hợp lệ/không hợp lệ. Thẻ hết hạn, bị khóa quyền mượn, không còn theo học hoặc vượt giới hạn thì dừng và hiển thị lý do. | FR-08, EL-05, UC-07 E1 |
| BR-39 | Có **ba nguồn giao**: (a) phiếu yêu cầu **Đã duyệt** còn giữ chỗ; (b) sinh viên đến từ danh sách chờ đang được giữ chỗ; (c) **mượn trực tiếp** không đặt trước, nhân viên lập phiếu mượn trực tiếp với đầy đủ kiểm tra như khi giao theo phiếu yêu cầu (điều kiện sinh viên, số lượng khả dụng, giới hạn). Giữ chỗ đã quá 30 phút thì không giao theo phiếu cũ. | FR-31, CR-11, UC-07 A1, A2, E2 |
| BR-40 | Với từng thiết bị giao, nhân viên quét (hoặc nhập tay) mã thiết bị cụ thể; hệ thống kiểm tra thiết bị đang Sẵn sàng, thuộc phòng của nhân viên và đúng loại đã duyệt (nếu không đạt thì từ chối, không đưa vào phiếu). Nhân viên ghi **tình trạng ban đầu** (kèm ảnh nếu cần) để đối chiếu khi trả. | FR-10, EL-06, UC-07 A3 |
| BR-41 | **Phiếu mượn** có mã riêng, gồm ngày giờ mượn (giờ giao thực tế), ngày giờ hẹn trả, sinh viên mượn và danh sách thiết bị. Một phiếu gồm nhiều thiết bị, có thể khác loại. Hạn trả, gia hạn và việc trả tính cho **cả phiếu**; kết quả kiểm tra ghi theo **từng thiết bị**. Phiếu lập xong có trạng thái **Đang mượn**; thiết bị chuyển **Đang được mượn**; giữ chỗ được giải phóng (phiếu yêu cầu chuyển **Đã nhận**; đăng ký chờ chuyển **Đã nhận**). | GL-10, GL-11, FR-11, CR-07, EL-20, UC-07 |
| BR-42 | **Ngày giờ hẹn trả** của cả phiếu = giờ giao + thời gian mượn. Giao theo phiếu yêu cầu: dùng thời gian mượn dự kiến; mượn trực tiếp hoặc từ danh sách chờ: dùng thời gian mượn tối đa của nhóm. Hạn trả không vượt mức tối đa của nhóm (nhỏ nhất theo các loại trong phiếu) và **không muộn hơn giờ đóng cửa quầy** trong ngày. | UC-07, GL-12, FR-11, CR-12 |
| BR-43 | Phí trả trễ mỗi ngày của loại thiết bị được **lưu vào chi tiết phiếu mượn tại thời điểm giao**; thay đổi cấu hình sau đó không ảnh hưởng phiếu đã lập. | UC-07, FR-37 |
| BR-44 | Một thiết bị tại một thời điểm chỉ thuộc tối đa **một** phiếu mượn đang hiệu lực; số lượng giữ chỗ không vượt số lượng khả dụng. Khi hai nhân viên cùng giao một thiết bị, chỉ một giao dịch thành công. | NFR-06, UC-07 E3 |

---

## 10. Gia hạn

| Mã | Quy tắc | Nguồn |
|---|---|---|
| BR-45 | Gia hạn áp dụng cho **cả phiếu mượn**: mỗi phiếu gia hạn **tối đa một lần**; yêu cầu phải gửi **trước** ngày giờ hẹn trả (đã quá hạn thì từ chối); thêm **tối đa 2 giờ** (`soGioGiaHanToiDa`); hạn trả mới không muộn hơn giờ đóng cửa quầy. | FR-07, CR-02, CR-12, GL-15, UC-04 |
| BR-46 | Chỉ gia hạn khi **chưa có sinh viên đăng ký chờ** các loại thiết bị trong phiếu; nếu có người chờ thì từ chối và nêu lý do. | FR-07, EL-04, UC-04 A1 |
| BR-47 | Gia hạn do **hệ thống tự duyệt**, không cần nhân viên. Gia hạn thành công thì cập nhật ngày giờ hẹn trả, tăng số lần gia hạn và ghi nhật ký; phiếu giữ trạng thái Đang mượn. | FR-07, CR-12, UC-04, SRS 7.3 |

---

## 11. Nhận trả và kiểm tra thiết bị

| Mã | Quy tắc | Nguồn |
|---|---|---|
| BR-48 | Sinh viên trả thiết bị tại quầy (thiết bị mất: sinh viên báo mất tại quầy, nhân viên nhập tay mã thiết bị theo phiếu mượn của sinh viên). Thiết bị không thuộc phiếu mượn đang hiệu lực thì báo lỗi, không cho nhận. | UC-08, UC-08 E1 |
| BR-49 | Nhân viên đối chiếu hiện trạng thực tế với tình trạng và ảnh lúc giao (hiển thị song song), kèm phản hồi của sinh viên nếu có; nhập tình trạng lúc trả (kèm ảnh nếu cần). Việc đánh giá hư hỏng do nhân viên thực hiện. | FR-12, EL-07, UC-08 |
| BR-50 | Kết quả kiểm tra **từng thiết bị** thuộc một trong bốn giá trị: **Nguyên vẹn**, **Hư hỏng**, **Thiếu phụ kiện**, **Mất**. Hư hỏng và Thiếu phụ kiện bắt buộc có mô tả và phải chọn **khung phạt**; thiếu phụ kiện xử lý như hư hỏng. | FR-36, UC-08, PT 1.5.17 |
| BR-51 | Tình trạng thiết bị cập nhật theo kết quả kiểm tra: Nguyên vẹn → **Sẵn sàng cho mượn**; Hư hỏng hoặc Thiếu phụ kiện → **Đang bảo trì/sửa chữa**; Mất → **Đã báo mất**. | FR-13, UC-08 A2, A3 |
| BR-52 | Hệ thống ghi nhân viên nhận trả (nhân viên đang đăng nhập) và ngày giờ trả thực tế của từng thiết bị (kết quả Mất không có ngày trả). | UC-08, PT 1.5.17 |
| BR-53 | Phiếu mượn chuyển **Đã trả** khi **mọi thiết bị** trong phiếu đã có kết quả kiểm tra (kể cả kết quả Mất). Sinh viên trả thiếu một số thiết bị thì phiếu giữ Đang mượn hoặc Quá hạn; thiết bị chưa trả tiếp tục tính theo hạn trả của phiếu. | FR-13, UC-08 A1 |
| BR-54 | Thiết bị nguyên vẹn về Sẵn sàng cho mượn mà có sinh viên chờ loại đó tại phòng thì chuyển sang xử lý danh sách chờ (mục 8). | UC-08, UC-10 |
| BR-55 | Sinh viên chỉ gửi **phản hồi tình trạng thiết bị** sau khi đã bàn giao thiết bị tại quầy (nhân viên đã nhận trả). Phản hồi được lưu gắn với mã thiết bị và phiếu mượn để nhân viên đối chiếu. | FR-25, UC-21 |

---

## 12. Vi phạm, phí phạt, bồi thường

| Mã | Quy tắc | Nguồn |
|---|---|---|
| BR-56 | Có bốn loại vi phạm: **Trả trễ**, **Hư hỏng**, **Mất**, **Không đến nhận** (không đến nhận đúng thời hạn giữ chỗ). | GL-16, PT 1.5.18 |
| BR-57 | **Trả trễ**: trả quá ngày giờ hẹn trả sau **15 phút ân hạn** (`anHanPhut`) thì tính trễ. Số ngày trễ tính **theo ngày lịch, tối thiểu 1 ngày**. | FR-43, GL-37, GL-13 |
| BR-58 | **Phí trả trễ** tính riêng từng thiết bị = số ngày trễ × phí/ngày của loại thiết bị, **tối đa bằng giá trị hiện hành** của thiết bị; phí cả phiếu là tổng các thiết bị. Phí trễ tính theo loại thiết bị, **không** theo nhóm sinh viên. | FR-14, FR-37, FR-43, CR-07, CR-09, GL-17, UC-09 E1 |
| BR-59 | **Bồi thường hư hỏng** (kể cả thiếu phụ kiện) = tỷ lệ khung phạt × giá trị hiện hành. Khung phạt do Quản trị viên tạo, nhân viên chọn khung phù hợp; mặc định Nhẹ 15%, Vừa 40%, Nặng 80%; có thể thêm khung; khung đã dùng chỉ được tắt, không xóa. | CR-08, FR-14, FR-36, FR-37, GL-32, UC-13 |
| BR-60 | **Bồi thường mất** = 100% giá trị hiện hành của thiết bị tại thời điểm mất. | GL-18, FR-14, CR-08 |
| BR-61 | Thiết bị chưa trả **quá 30 ngày** thì nhân viên lập biên bản mất; thiết bị chuyển **Đã báo mất**. Mốc 30 ngày tính **theo ngày lịch kể từ ngày giờ hẹn trả** của phiếu mượn, cùng cách tính số ngày trễ [ĐX-04]; thiết bị bị bồi thường 100% giá trị hiện hành (BR-60). | FR-43, CR-09, UC-09 A3, ĐX-04 |
| BR-62 | **Biên bản vi phạm** do nhân viên lập từ các khoản hệ thống tính sẵn **theo từng thiết bị**; lưu mã phiếu, thiết bị, loại vi phạm, số tiền; **số tiền không đổi sau khi lập**. Trạng thái: Chưa nộp / Đã nộp / Đã hủy. | FR-14, GL-34, UC-09, PT 1.5.19 |
| BR-63 | **Cách đếm vi phạm**: mỗi loại vi phạm (Trả trễ, Hư hỏng, Mất) có trong một biên bản tính **một** vi phạm, không tính theo số thiết bị; mỗi lần giữ chỗ hết hạn (phiếu yêu cầu hoặc đăng ký chờ) tính **một** vi phạm Không đến nhận. | UC-09 bước 2, UC-24 bước 1, FR-28, PT 1.5.18 |
| BR-64 | **Không** tính vi phạm khi: hủy phiếu yêu cầu; rời danh sách chờ; từ chối nhận thiết bị khi được giữ chỗ; phiếu yêu cầu chưa duyệt trong ngày tự hết hiệu lực. | FR-29, FR-30, UC-20, UC-10 A1 |
| BR-65 | Tiền phạt do sinh viên nộp **trực tiếp cho nhân viên** (ngoài hệ thống); nhân viên ghi nhận từng biên bản **Đã nộp**; hệ thống thông báo vi phạm và phí phạt cho sinh viên. | UC-09, FR-15, FR-24 |

---

## 13. Khóa, mở khóa quyền mượn và cấm mượn dài hạn

Trạng thái quyền mượn của sinh viên: **Bình thường** (mặc định) / **Tạm khóa** / **Cấm mượn dài hạn** (GL-19, GL-38, PT 1.5.4).

| Mã | Quy tắc | Nguồn |
|---|---|---|
| BR-66 | Hệ thống **tự động khóa** quyền mượn thiết bị mới khi sinh viên có phiếu quá hạn trả quá số ngày quy định (`soNgayKhoa`, mặc định 3 ngày) **hoặc** còn biên bản chưa nộp phạt. Khi khóa: chặn đặt trước và lập phiếu mượn mới, hiển thị lý do. | FR-09, GL-19, EL-08, UC-17 |
| BR-67 | **Mở khóa tự động** ngay khi nhân viên xác nhận đã nhận đủ thiết bị và thu đủ tiền, không cần Quản trị viên. Còn biên bản chưa nộp hoặc còn thiết bị chưa trả thì vẫn khóa. | FR-15, CR-10, EL-23, UC-09 A1 |
| BR-68 | **Cấm mượn dài hạn** (ưu tiên thấp, chưa đưa vào giai đoạn đầu): hệ thống đếm số vi phạm của sinh viên trong học kỳ (học kỳ theo hệ thống của nhà trường); khi vượt ngưỡng (`nguongViPham`, mặc định 3 lần/học kỳ) thì chuyển sang Cấm mượn dài hạn cho đến khi Quản trị viên gỡ cấm kèm lý do. Sinh viên thuộc diện này **không** được mở khóa tự động khi nộp phạt. | FR-42, CR-04, CR-14, GL-38, UC-24, UC-09 E2 |

---

## 14. Quy định mượn và tham số cấu hình

Quản trị viên cấu hình tại UC-13. Giá trị mới chỉ áp dụng cho giao dịch phát sinh sau; phiếu và biên bản đã lập giữ nguyên (không hồi tố). Mỗi thay đổi được ghi nhật ký giá trị trước/sau. (FR-20, FR-37, UC-13)

| Tham số / quy định | Giá trị mặc định hoặc ràng buộc | Nguồn |
|---|---|---|
| Số thiết bị tối đa mượn cùng lúc (tính trên tổng, không phân biệt loại) | Sinh viên đại học: **2**; học viên cao học hoặc sinh viên làm đồ án tốt nghiệp: **3** | FR-20, CR-06, EL-11, EL-19 |
| Thời gian mượn tối đa (giờ) | **5 giờ**; **nhóm ưu tiên: 10 giờ**. Nhóm ưu tiên gồm học viên cao học và sinh viên làm đồ án tốt nghiệp; sinh viên đại học thuộc mức 5 giờ [ĐX-01]. Có thể đặt ngoại lệ theo loại thiết bị | FR-20, GL-12, CR-12, UC-02 A3, ĐX-01 |
| Số ngày quá hạn để khóa quyền mượn mới (`soNgayKhoa`) | **3 ngày** | FR-09, PT 1.5.7 |
| Thời gian giữ chỗ (`thoiGianGiuChoPhut`) | **30 phút** | GL-30, FR-06, FR-30, PT 1.5.7 |
| Ân hạn sau hạn trả (`anHanPhut`) | **15 phút** | GL-37, FR-43, PT 1.5.7 |
| Số giờ gia hạn tối đa (`soGioGiaHanToiDa`) | **2 giờ** | FR-07, CR-12, PT 1.5.7 |
| Ngưỡng vi phạm (`nguongViPham`) | **3 lần/học kỳ** | FR-42, GL-38, PT 1.5.7 |
| Số lần nhập sai mật khẩu tối đa (`soLanDangNhapSaiToiDa`) | Phải lớn hơn 0; mặc định **5 lần** [ĐX-02] | FR-20, FR-33, PT 1.5.7, ĐX-02 |
| Giờ mở cửa / đóng cửa quầy (`gioMoCua`, `gioDongCua`) | `gioMoCua` < `gioDongCua`; hạn trả không muộn hơn giờ đóng cửa. **Không có giá trị mặc định**: Quản trị viên phải thiết lập theo giờ làm việc thực tế của quầy trước khi đưa hệ thống vào sử dụng [ĐX-03] | FR-20, GL-12, PT 1.5.7, ĐX-03 |
| Khung phạt hư hỏng (% giá trị hiện hành) | Nhẹ **15%**, Vừa **40%**, Nặng **80%**; có thể thêm khung; khung đã dùng chỉ tắt, không xóa | FR-37, GL-32, CR-08 |
| Phí trả trễ (VNĐ/ngày) | Cấu hình **theo từng loại thiết bị**, không theo nhóm sinh viên; mỗi loại có một mức (≥ 0). Hệ thống không có danh mục thiết bị hay mức phí mặc định; các ví dụ ở đề bài, GL-04, GL-05, GL-17, EL-22 chỉ để minh họa [ĐX-05] | FR-37, CR-09, PT 1.5.9, ĐX-05 |
| Chu kỳ bảo trì (ngày) | Cấu hình theo từng loại thiết bị, lớn hơn 0 | FR-16, PT 1.5.9 |

Các giá trị nằm ngoài danh sách tham số cấu hình của FR-20 nhưng được yêu cầu trực tiếp: nhắc hạn trả trước **1 giờ** (FR-24, CR-12); chưa trả quá **30 ngày** thì lập biên bản mất (FR-43); mỗi phiếu gia hạn tối đa **một lần** (FR-07, CR-02).

---

## 15. Bảo trì, sửa chữa, thanh lý, nhà cung cấp

| Mã | Quy tắc | Nguồn |
|---|---|---|
| BR-69 | Nhân viên lập **lịch bảo trì định kỳ** theo chu kỳ của từng loại thiết bị và ghi nội dung thực hiện, kết quả, ngày hoàn thành. Trong thời gian bảo trì/sửa chữa thiết bị chuyển **Đang bảo trì/sửa chữa** và không cho mượn. | Đề bài mục 5, FR-16, FR-17, EL-09, UC-11 |
| BR-70 | Thiết bị **đến hạn bảo trì** khi ngày hiện tại ≥ ngày hoàn thành lần bảo trì định kỳ gần nhất (chưa có lần nào thì tính từ ngày nhập kho) + chu kỳ bảo trì của loại; hệ thống nhắc nhân viên. Thiết bị đang được mượn thì nhân viên lập lịch sau khi thiết bị được trả và về Sẵn sàng cho mượn. | UC-11, UC-17 |
| BR-71 | Thiết bị hư hỏng phát sinh từ kiểm tra khi trả được nhân viên tiếp nhận sửa chữa (liên kết với phiếu mượn có thiết bị hư hỏng). Kết quả: đưa về **Sẵn sàng cho mượn** hoặc **đề xuất thanh lý** (hư hỏng nặng không sửa được); thiết bị đề xuất thanh lý hiện trong danh sách chờ quyết định của Quản trị viên. | FR-36, FR-38, UC-11, UC-08 |
| BR-72 | Thiết bị còn trong thời hạn bảo hành thì nhân viên có thể liên hệ nhà cung cấp để sửa chữa hoặc đổi trả. | GL-23, UC-11 A1 |
| BR-73 | **Thanh lý** do Quản trị viên quyết định đối với thiết bị cũ, hư hỏng nặng không sửa được hoặc hết khấu hao. Mỗi đợt thanh lý có **một quyết định thanh lý riêng**, gồm mã số, ngày thanh lý và danh sách thiết bị; thiết bị chuyển **Đã thanh lý** và vẫn giữ lịch sử. | Đề bài mục 5, FR-21, GL-24, GL-25, UC-15 |
| BR-74 | Quản trị viên quản lý thông tin liên hệ nhà cung cấp và thời hạn bảo hành của từng thiết bị, theo dõi ngày hết hạn bảo hành. Một thiết bị hoặc loại thiết bị có thể do nhiều nhà cung cấp. | FR-19, GL-22, GL-23, UC-22 |

---

## 16. Thông báo, tác vụ tự động, nhật ký và báo cáo

| Mã | Quy tắc | Nguồn |
|---|---|---|
| BR-75 | Hệ thống tự động thông báo cho sinh viên qua **ứng dụng và email** (SMS tùy chọn) khi: sắp đến hạn trả (**trước 1 giờ**); quá hạn trả; thiết bị chờ sẵn sàng (gửi **ngay khi bắt đầu giữ chỗ**); phiếu yêu cầu được duyệt hoặc từ chối; phát sinh vi phạm, phí phạt. | FR-24, CR-12, NFR-08, UC-17 |
| BR-76 | Phiếu mượn quá ngày giờ hẹn trả (sau ân hạn) được hệ thống đánh dấu **Quá hạn** và gửi thông báo quá hạn. | UC-17, SRS 7.3 |
| BR-77 | Nhân viên xem danh sách sinh viên sắp đến hạn hoặc quá hạn (lọc theo phòng thiết bị) và gửi thông báo nhắc nhở; mỗi lần nhắc được ghi lại. | FR-39, UC-18 |
| BR-78 | Hệ thống tự chạy theo lịch để: nhắc hạn, đánh dấu quá hạn, khóa quyền mượn, hết hạn giữ chỗ, cho phiếu yêu cầu chưa duyệt trong ngày hết hiệu lực, nhắc bảo trì đến hạn. Chạy lại không tạo kết quả trùng. | FR-41, UC-17 |
| BR-79 | **Nhật ký hệ thống** ghi người thực hiện, thời điểm, thao tác, giá trị trước và sau; chỉ xem, **không ai được sửa hoặc xóa**. Lịch sử mượn/trả, thay đổi tình trạng thiết bị và biên bản xử phạt đều được ghi vết. | FR-40, NFR-02, UC-23 |
| BR-80 | Dữ liệu tình trạng thiết bị (lúc giao, lúc trả) và lịch sử mượn/trả được lưu đầy đủ để đối chiếu khi có tranh chấp giữa sinh viên và nhà trường. Sinh viên xem được các phiếu mượn đang và đã thực hiện (ngày mượn, hẹn trả, trả thực tế, tình trạng khi trả, vi phạm, phí phạt). | Đề bài mục 7, NFR-07, EL-16, FR-32, UC-05 |
| BR-81 | Báo cáo cho Quản trị viên (lọc theo khoảng thời gian, xuất được): (1) tình trạng thiết bị trong kho; (2) sinh viên mượn quá hạn chưa trả; (3) thiết bị hư hỏng/mất kèm sinh viên và phiếu mượn liên quan; (4) tần suất mượn theo loại thiết bị, làm cơ sở hoạch định mua sắm; (5) vi phạm của sinh viên (trả trễ, hư hỏng, mất, không đến nhận) theo từng sinh viên; (6) lịch sử bảo trì, thanh lý theo từng đợt. Số liệu báo cáo tình trạng thiết bị khớp 100% với dữ liệu thực tế. | Đề bài mục 6, FR-22, FR-23, FR-26, FR-27, UC-16 |

---

## 17. Vòng đời trạng thái

**Thiết bị**

| Từ | Sự kiện | Đến |
|---|---|---|
| (khởi tạo) | Thiết bị được nhập kho | Sẵn sàng cho mượn |
| Sẵn sàng cho mượn | Nhân viên lập phiếu mượn (giao thiết bị) | Đang được mượn |
| Đang được mượn | Trả, kiểm tra nguyên vẹn | Sẵn sàng cho mượn |
| Đang được mượn | Trả, kiểm tra hư hỏng hoặc thiếu phụ kiện | Đang bảo trì/sửa chữa |
| Đang được mượn | Kết quả kiểm tra là Mất, hoặc chưa trả quá 30 ngày và nhân viên lập biên bản mất | Đã báo mất |
| Sẵn sàng cho mượn | Nhân viên lập lịch bảo trì | Đang bảo trì/sửa chữa |
| Đang bảo trì/sửa chữa | Hoàn thành, kết quả đạt | Sẵn sàng cho mượn |
| Đang bảo trì/sửa chữa | Quản trị viên lập quyết định thanh lý (hư hỏng nặng không sửa được) | Đã thanh lý |
| Sẵn sàng cho mượn | Quản trị viên lập quyết định thanh lý (thiết bị cũ, hết khấu hao) | Đã thanh lý |

**Phiếu yêu cầu mượn**

| Từ | Sự kiện | Đến |
|---|---|---|
| (khởi tạo) | Sinh viên gửi phiếu | Chờ duyệt |
| Chờ duyệt | Nhân viên duyệt (giữ chỗ 30 phút) | Đã duyệt |
| Chờ duyệt | Nhân viên từ chối kèm lý do | Từ chối |
| Chờ duyệt | Sinh viên hủy | Đã hủy |
| Chờ duyệt | Quá ngày chưa được duyệt (không tính vi phạm) | Hết hạn |
| Đã duyệt | Nhân viên lập phiếu mượn khi sinh viên đến nhận | Đã nhận |
| Đã duyệt | Sinh viên hủy | Đã hủy |
| Đã duyệt | Quá 30 phút chưa nhận (ghi 1 vi phạm) | Hết hạn |

**Phiếu mượn**

| Từ | Sự kiện | Đến |
|---|---|---|
| (khởi tạo) | Nhân viên lập phiếu mượn | Đang mượn |
| Đang mượn | Gia hạn (một lần, tối đa 2 giờ) | Đang mượn |
| Đang mượn / Quá hạn | Sinh viên trả thiếu một số thiết bị | giữ nguyên trạng thái |
| Đang mượn | Quá ngày giờ hẹn trả (sau ân hạn) | Quá hạn |
| Đang mượn / Quá hạn | Mọi thiết bị đã có kết quả kiểm tra | Đã trả |

**Đăng ký chờ**

| Từ | Sự kiện | Đến |
|---|---|---|
| (khởi tạo) | Sinh viên đăng ký chờ | Đang chờ |
| Đang chờ | Có thiết bị cùng loại được trả về tại phòng và sinh viên đứng đầu hàng chờ | Đã thông báo |
| Đang chờ | Sinh viên rời danh sách chờ | Đã hủy |
| Đã thông báo | Sinh viên đến quầy, nhân viên xác nhận | Đã nhận |
| Đã thông báo | Sinh viên từ chối nhận, rời danh sách hoặc báo không mượn nữa tại quầy | Đã hủy |
| Đã thông báo | Quá 30 phút chưa nhận | Hết hạn |

(Nguồn: SRS mục 7; PT mục 2; BM P3A_04 MD-04…MD-07.)

---

## 18. Công thức tính

| Nội dung | Công thức | Nguồn |
|---|---|---|
| Ngày giờ hẹn trả của phiếu | giờ giao + thời gian mượn (xem BR-42) | UC-07, GL-12 |
| Số ngày trễ | Theo ngày lịch, tối thiểu 1 ngày, tính khi trả quá hạn sau ân hạn 15 phút | FR-43 |
| Phí trả trễ của một thiết bị | số ngày trễ × phí trả trễ/ngày của chi tiết phiếu mượn (đã lưu khi giao); tối đa bằng giá trị hiện hành. Phí cả phiếu = tổng các thiết bị | FR-14, FR-43, CR-07, CR-09 |
| Bồi thường hư hỏng | tỷ lệ khung phạt (%) × giá trị hiện hành | FR-14, GL-18 |
| Bồi thường mất | 100% × giá trị hiện hành tại thời điểm mất | FR-14, GL-18 |
| Mốc lập biên bản mất khi chưa trả | 30 ngày lịch kể từ ngày giờ hẹn trả của phiếu; sau mốc này bồi thường 100% giá trị hiện hành | FR-43, UC-09 A3, GL-18, ĐX-04 |
| Đến hạn bảo trì | ngày hiện tại ≥ ngày hoàn thành bảo trì định kỳ gần nhất (hoặc ngày nhập kho nếu chưa có) + chu kỳ bảo trì của loại | UC-11, UC-17 |
| **Ví dụ chuẩn** | Laptop 50.000 và máy chiếu 30.000 VNĐ/ngày, trả trễ 3 ngày: (50.000 + 30.000) × 3 = **240.000 VNĐ** | NFR-09, EL-22 |

---

## 19. Các thay đổi so với đề bài gốc và các phiên bản yêu cầu trước

Các quy tắc ở trên đã áp dụng yêu cầu mới nhất. Bảng này chỉ để đối chiếu với đề bài gốc (PDF) và với các yêu cầu trước đó đã bị điều chỉnh.

| Nội dung | Trước đây | Hiện hành | Nguồn |
|---|---|---|---|
| Đơn vị thời gian mượn | Đề bài: tính theo ngày (ví dụ 7 ngày; 15 ngày) | Tính theo giờ: tối đa 5 giờ, nhóm ưu tiên 10 giờ | CR-12 |
| Số thiết bị tối đa | Đề bài: ví dụ 2 thiết bị (sinh viên đại học), 3 thiết bị (cao học, đồ án); có thể khác nhau tùy loại thiết bị và loại sinh viên | 2 và 3 thiết bị, tính trên tổng, không phân biệt loại thiết bị | CR-06, FR-20 |
| Gia hạn | Đề bài: gia hạn thêm một số ngày theo quy định | Một lần/phiếu, gửi trước hạn trả, thêm tối đa 2 giờ, hệ thống tự duyệt | CR-02, CR-12 |
| Phí trả trễ | Đề bài: tính theo từng ngày trễ trên mỗi thiết bị; mức phí áp dụng theo từng loại thiết bị và từng nhóm đối tượng sinh viên | Theo loại thiết bị, không theo nhóm sinh viên; ân hạn 15 phút; trần bằng giá trị hiện hành | CR-09 |
| Bồi thường hư hỏng | Đề bài: theo mức quy định tùy mức độ hư hỏng; CR-03: mức hư hỏng cố định (Hư hỏng nhẹ, Hư hỏng nặng) | Khung phạt theo % giá trị hiện hành (Nhẹ 15%, Vừa 40%, Nặng 80%) | CR-08 |
| Danh sách chờ | Đề bài: nêu cả việc hệ thống tự thông báo (mục 2.1) lẫn nhân viên thông báo (mục 2.2, 4); chưa nêu giữ chỗ và thời hạn nhận. CR-01: sinh viên phải xác nhận trong 30 phút, có tình trạng thiết bị "Chờ xác nhận" | Hệ thống tự thông báo và giữ chỗ 30 phút; nhân viên xác nhận khi sinh viên đến nhận; hết hạn tự chuyển lượt và ghi 1 vi phạm; bỏ tình trạng "Chờ xác nhận" | CR-01, CR-05, CR-11 |
| Mượn thiết bị | Đề bài: sinh viên gửi phiếu yêu cầu để nhân viên duyệt, đến quầy để nhân viên quét thẻ và lập phiếu | Ứng dụng chỉ để đặt trước; duyệt xong giữ chỗ 30 phút; vẫn cho mượn trực tiếp tại quầy không cần đặt trước | CR-11 |
| Mở khóa quyền mượn | Đề bài: khóa đến khi hoàn tất trả thiết bị và nộp phạt | Tự động mở khi nhân viên xác nhận đã nhận đủ thiết bị và thu đủ tiền | CR-10 |
| Tài khoản sinh viên | EL-25: ban đầu dự kiến có dữ liệu sinh viên riêng, nhập từ bảng tính | Do nhà trường cung cấp; hệ thống không tạo hoặc nhập | CR-13, EL-25 |

---

## 20. Quyết định bổ sung của SRS được áp dụng làm mặc định

Bảy quyết định dưới đây do SRS đề xuất để xử lý các điểm tài liệu nguồn gốc chưa nêu rõ hoặc chưa thống nhất (SRS mục 2.8). Tài liệu này áp dụng chúng làm giá trị mặc định và gắn nhãn [ĐX-xx] tại nơi sử dụng. Đây là nội dung **không có trong tài liệu nguồn gốc**; nên cập nhật ngược vào nguồn gốc ở các điểm ghi ở cột cuối.

| Mã | Vấn đề | Nguồn gốc nêu gì | Quyết định áp dụng | Cập nhật ngược vào nguồn gốc |
|---|---|---|---|---|
| ĐX-01 | Nhóm ưu tiên được mượn tối đa 10 giờ gồm nhóm nào | FR-20 và GL-12 chỉ nêu "nhóm ưu tiên" (ví dụ sinh viên làm đồ án tốt nghiệp); đề bài gốc cho học viên cao học và nhóm làm đồ án mức mượn cao hơn | Nhóm ưu tiên gồm học viên cao học và sinh viên làm đồ án tốt nghiệp; sinh viên đại học tối đa 5 giờ. Giới hạn số thiết bị giữ theo FR-20 (2 và 3). Cả hai giá trị do Quản trị viên cấu hình theo nhóm | Nêu rõ hai nhóm trong GL-12 và FR-20 |
| ĐX-02 | Giá trị mặc định của `soLanDangNhapSaiToiDa` | Chỉ ràng buộc lớn hơn 0 | 5 lần; phải lớn hơn 0; Quản trị viên cấu hình tại UC-13 | Ghi giá trị mặc định trong FR-20 và PT 1.5.7 |
| ĐX-03 | Giờ mở/đóng cửa quầy | Chỉ ràng buộc `gioMoCua` < `gioDongCua` và hạn trả không muộn hơn giờ đóng cửa; không có giá trị | Không đặt mặc định; Quản trị viên thiết lập theo giờ làm việc thực tế trước khi đưa vào sử dụng | Không cần |
| ĐX-04 | Mốc bắt đầu tính "chưa trả quá 30 ngày" | Chỉ nêu "quá 30 ngày" | Tính theo ngày lịch kể từ ngày giờ hẹn trả của phiếu mượn, cùng cách tính số ngày trễ; quá mốc thì lập biên bản mất, thiết bị Đã báo mất, bồi thường 100% giá trị hiện hành | Ghi rõ mốc trong FR-43 và UC-09 A3 |
| ĐX-05 | Ví dụ loại thiết bị và mức phí không đồng nhất giữa các nguồn (GL-04, GL-05, GL-17, EL-22, NFR-09) | Chỉ là ví dụ minh họa | Hệ thống không có danh mục thiết bị hay mức phí mặc định; mức phí do Quản trị viên cấu hình theo loại; NFR-09 (240.000 VNĐ) giữ làm ví dụ chuẩn cho tiêu chí chấp nhận | Không cần |
| ĐX-06 | Mức ưu tiên của use case khác mức ưu tiên của FR tương ứng (ví dụ UC-10, UC-11 là Bắt buộc; FR-05, FR-06, FR-16 là Nên có) | Giữ nguyên theo từng nguồn | Mức của use case quyết định use case có thuộc giai đoạn đầu hay không; mức của FR quyết định thứ tự các yêu cầu trong use case; use case Bắt buộc được hiện thực đủ luồng chính | Không cần |
| ĐX-07 | Nhiều FR/NFR còn trạng thái **Bản nháp** (FR-30, 31, 32, 33, 34, 35, 36, 37, 38, 39, 40, 41, 43; NFR-07, 08, 09); CR-04 ghi "Đang phân tích" | Chưa chốt baseline | Các yêu cầu Bản nháp vẫn được đưa vào vì Usecase v1.2 đã tham chiếu, giữ nhãn Bản nháp và cần nhóm duyệt trước khi chốt baseline; CR-04 hiểu là đã chấp thuận nhưng chưa đưa vào giai đoạn đầu (CR-14), nội dung nằm ở FR-42 và UC-24 | Cập nhật trạng thái FR/NFR ở P2_05 và CR-04 ở P2_07 sau khi duyệt |
