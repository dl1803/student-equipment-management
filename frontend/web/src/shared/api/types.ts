// Bí danh kiểu từ hợp đồng API (ĐX-FC-02). Nguồn duy nhất là schema.d.ts sinh bởi `npm run gen:api`;
// không tự định nghĩa lại hình dạng yêu cầu hay phản hồi (FA 3.2). Thêm bí danh khi tính năng cần.
import type { components, operations, paths } from "./schema";

export type { components, operations, paths };

type Schemas = components["schemas"];

// Xác thực và phiên
export type VaiTro = Schemas["VaiTro"];
export type ThongTinToi = Schemas["ThongTinToi"];
export type TokenResponse = Schemas["TokenResponse"];
export type DangNhapNhanVienRequest = Schemas["DangNhapNhanVienRequest"];
export type LamMoiRequest = Schemas["LamMoiRequest"];
export type DangXuatRequest = Schemas["DangXuatRequest"];
export type PhongThietBi = Schemas["PhongThietBi"];

// Mã trạng thái (họ trạng thái theo DS 7.2, FC 3.5)
export type TinhTrangThietBi = Schemas["TinhTrangThietBi"];
export type TrangThaiPhieuYeuCauMuon = Schemas["TrangThaiPhieuYeuCauMuon"];
export type TrangThaiPhieuMuon = Schemas["TrangThaiPhieuMuon"];
export type TrangThaiDangKyCho = Schemas["TrangThaiDangKyCho"];
export type KetQuaKiemTra = Schemas["KetQuaKiemTra"];
export type TrangThaiBienBan = Schemas["TrangThaiBienBan"];
export type LoaiViPham = Schemas["LoaiViPham"];
export type TrangThaiHoc = Schemas["TrangThaiHoc"];
export type TrangThaiQuyenMuon = Schemas["TrangThaiQuyenMuon"];
export type TrangThaiKhoa = Schemas["TrangThaiKhoa"];
export type LoaiThucHienBaoTri = Schemas["LoaiThucHienBaoTri"];
export type KetQuaBaoTri = Schemas["KetQuaBaoTri"];
export type NguonGiao = Schemas["LapPhieuMuonRequest"]["nguon"];
export type LoaiThongBao = Schemas["LoaiThongBao"];

// Lỗi
export type MaLoi = Schemas["MaLoi"];
export type ProblemDetails = Schemas["ProblemDetails"];
export type ValidationProblemDetails = Schemas["ValidationProblemDetails"];
