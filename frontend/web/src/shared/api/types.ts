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

// Lỗi
export type MaLoi = Schemas["MaLoi"];
export type ProblemDetails = Schemas["ProblemDetails"];
export type ValidationProblemDetails = Schemas["ValidationProblemDetails"];
