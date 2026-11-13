// Tiền tố khóa truy vấn TanStack Query dùng chung (FA 4.7, ĐX-FC-10). Sở hữu: Thịnh (shared core).
// Khóa đầy đủ có dạng [vùng, tài nguyên, tham số]; mỗi tính năng dựng khóa của mình trong api.ts
// bằng cách nối tham số vào tiền tố ở đây. Tính năng khác dùng tiền tố này để đánh dấu lỗi thời
// sau thao tác ghi, không import chéo giữa các tính năng.
// Thêm tiền tố mới khi một tính năng cần, kèm cập nhật bảng FA 4.7 nếu thao tác ghi mới ảnh hưởng nó.

export const TIEN_TO_KHOA = {
  // Xác thực
  xacThucToi: ["xac-thuc", "toi"],

  // Quầy (NV) — Thịnh
  quayPhieuYeuCau: ["quay", "phieu-yeu-cau"],
  quayLoaiThietBi: ["quay", "loai-thiet-bi"],
  quayDanhSachCho: ["quay", "danh-sach-cho"],
  quayPhieuMuon: ["quay", "phieu-muon"],
  quayBienBan: ["quay", "bien-ban"],
  quaySinhVien: ["quay", "sinh-vien"],
  quayBaoTri: ["quay", "bao-tri"],

  // Quản trị — danh mục, biên bản, thanh lý, báo cáo: Thịnh
  quanTriPhong: ["quan-tri", "phong"],
  quanTriLoaiThietBi: ["quan-tri", "loai-thiet-bi"],
  quanTriThietBi: ["quan-tri", "thiet-bi"],
  quanTriBienBan: ["quan-tri", "bien-ban"],
  quanTriThanhLy: ["quan-tri", "thanh-ly"],
  quanTriBaoCao: ["quan-tri", "bao-cao"],

  // Quản trị — cấu hình và tài khoản nhân viên (SCR-QT-05..09): Long
  quanTriQuyDinhMuon: ["quan-tri", "quy-dinh-muon"],
  quanTriPhiTre: ["quan-tri", "phi-tre"],
  quanTriKhungPhat: ["quan-tri", "khung-phat"],
  quanTriThamSo: ["quan-tri", "tham-so"],
  quanTriNhanVien: ["quan-tri", "nhan-vien"],
} as const;

export type TenTienToKhoa = keyof typeof TIEN_TO_KHOA;
