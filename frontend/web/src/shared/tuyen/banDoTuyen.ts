// Bản đồ tuyến web giai đoạn 1 (FA 4.3, ĐX-FA-19): 28 màn hình, không có tuyến nào ngoài bảng này.
// Sở hữu: Thịnh (shared core). Thêm hoặc đổi tuyến phải cập nhật Frontend-Architecture.md 4.3 trước.
// Chủ sở hữu màn hình: Long giữ SCR-QT-05..09 từ ngày làm việc 20; còn lại là Thịnh.

export type VaiTroTuyen = "CHUNG" | "NHAN_VIEN" | "QUAN_TRI";
export type NguoiSoHuu = "THINH" | "LONG";

export type ManHinhWeb = {
  ten: string;
  tuyen: string;
  vaiTro: VaiTroTuyen;
  soHuu: NguoiSoHuu;
};

export const BAN_DO_TUYEN = {
  "SCR-CM-03": { ten: "Khởi động và khôi phục phiên", tuyen: "/", vaiTro: "CHUNG", soHuu: "THINH" },
  "SCR-CM-02": { ten: "Đăng nhập nhân viên hoặc quản trị viên", tuyen: "/dang-nhap", vaiTro: "CHUNG", soHuu: "THINH" },
  "SCR-NV-01": { ten: "Trang chủ nhân viên", tuyen: "/nv", vaiTro: "NHAN_VIEN", soHuu: "THINH" },
  "SCR-NV-02": { ten: "Danh sách phiếu yêu cầu chờ duyệt", tuyen: "/nv/phieu-yeu-cau", vaiTro: "NHAN_VIEN", soHuu: "THINH" },
  "SCR-NV-03": { ten: "Duyệt hoặc từ chối phiếu yêu cầu", tuyen: "/nv/phieu-yeu-cau/duyet", vaiTro: "NHAN_VIEN", soHuu: "THINH" },
  "SCR-NV-04": { ten: "Giao thiết bị, bước 1: Quét thẻ và hồ sơ sinh viên", tuyen: "/nv/giao-thiet-bi/the", vaiTro: "NHAN_VIEN", soHuu: "THINH" },
  "SCR-NV-05": { ten: "Giao thiết bị, bước 2: Chọn nguồn giao và kiểm tra nguồn", tuyen: "/nv/giao-thiet-bi/nguon", vaiTro: "NHAN_VIEN", soHuu: "THINH" },
  "SCR-NV-06": { ten: "Giao thiết bị, bước 3: Thêm thiết bị và ghi tình trạng ban đầu", tuyen: "/nv/giao-thiet-bi/thiet-bi", vaiTro: "NHAN_VIEN", soHuu: "THINH" },
  "SCR-NV-07": { ten: "Giao thiết bị, bước 4: Phiếu mượn vừa lập", tuyen: "/nv/giao-thiet-bi/phieu", vaiTro: "NHAN_VIEN", soHuu: "THINH" },
  "SCR-NV-08": { ten: "Nhận trả, bước 1: Tìm thiết bị hoặc phiếu mượn", tuyen: "/nv/nhan-tra", vaiTro: "NHAN_VIEN", soHuu: "THINH" },
  "SCR-NV-09": { ten: "Nhận trả, bước 2: Kiểm tra và ghi kết quả", tuyen: "/nv/nhan-tra/kiem-tra", vaiTro: "NHAN_VIEN", soHuu: "THINH" },
  "SCR-NV-10": { ten: "Lập biên bản vi phạm", tuyen: "/nv/bien-ban/lap", vaiTro: "NHAN_VIEN", soHuu: "THINH" },
  "SCR-NV-11": { ten: "Biên bản và thu phạt", tuyen: "/nv/bien-ban", vaiTro: "NHAN_VIEN", soHuu: "THINH" },
  "SCR-NV-12": { ten: "Phiếu chưa trả quá 30 ngày", tuyen: "/nv/qua-han-30-ngay", vaiTro: "NHAN_VIEN", soHuu: "THINH" },
  "SCR-NV-13": { ten: "Danh sách chờ của phòng", tuyen: "/nv/danh-sach-cho", vaiTro: "NHAN_VIEN", soHuu: "THINH" },
  "SCR-NV-14": { ten: "Bảo trì và sửa chữa", tuyen: "/nv/bao-tri", vaiTro: "NHAN_VIEN", soHuu: "THINH" },
  "SCR-QT-01": { ten: "Trang chủ quản trị viên", tuyen: "/qt", vaiTro: "QUAN_TRI", soHuu: "THINH" },
  "SCR-QT-02": { ten: "Phòng thiết bị", tuyen: "/qt/danh-muc/phong", vaiTro: "QUAN_TRI", soHuu: "THINH" },
  "SCR-QT-03": { ten: "Loại thiết bị", tuyen: "/qt/danh-muc/loai-thiet-bi", vaiTro: "QUAN_TRI", soHuu: "THINH" },
  "SCR-QT-04": { ten: "Thiết bị", tuyen: "/qt/danh-muc/thiet-bi", vaiTro: "QUAN_TRI", soHuu: "THINH" },
  "SCR-QT-05": { ten: "Cấu hình: Quy định mượn theo nhóm sinh viên", tuyen: "/qt/cau-hinh/quy-dinh-muon", vaiTro: "QUAN_TRI", soHuu: "LONG" },
  "SCR-QT-06": { ten: "Cấu hình: Phí trả trễ theo loại thiết bị", tuyen: "/qt/cau-hinh/phi-tre", vaiTro: "QUAN_TRI", soHuu: "LONG" },
  "SCR-QT-07": { ten: "Cấu hình: Khung phạt hư hỏng", tuyen: "/qt/cau-hinh/khung-phat", vaiTro: "QUAN_TRI", soHuu: "LONG" },
  "SCR-QT-08": { ten: "Cấu hình: Tham số hệ thống", tuyen: "/qt/cau-hinh/tham-so", vaiTro: "QUAN_TRI", soHuu: "LONG" },
  "SCR-QT-09": { ten: "Tài khoản nhân viên", tuyen: "/qt/tai-khoan-nhan-vien", vaiTro: "QUAN_TRI", soHuu: "LONG" },
  "SCR-QT-10": { ten: "Biên bản vi phạm (hủy hoặc điều chỉnh)", tuyen: "/qt/bien-ban", vaiTro: "QUAN_TRI", soHuu: "THINH" },
  "SCR-QT-11": { ten: "Thanh lý thiết bị", tuyen: "/qt/thanh-ly", vaiTro: "QUAN_TRI", soHuu: "THINH" },
  "SCR-QT-12": { ten: "Báo cáo", tuyen: "/qt/bao-cao", vaiTro: "QUAN_TRI", soHuu: "THINH" },
} as const satisfies Record<string, ManHinhWeb>;

export type MaManHinhWeb = keyof typeof BAN_DO_TUYEN;

export function tuyenCua(ma: MaManHinhWeb): string {
  return BAN_DO_TUYEN[ma].tuyen;
}
