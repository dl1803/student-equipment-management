// Bộ dựng ngữ cảnh kiểm thử cho màn hình (FC 9.1; bàn giao D04-T). Sở hữu: Thịnh (shared core).
// CHỈ DÙNG TRONG TỆP *.test.tsx. Dựng màn hình với QueryClient mặc định của dự án và một
// AuthContext fixture, không cần AuthProvider thật, không gọi mạng.
//
//   const { queryClient } = hienThiVoiNguCanh(<QuyDinhMuonScreen />, {
//     nguoiDung: QUAN_TRI_MAU,
//   });
import type { QueryClient } from "@tanstack/react-query";
import { QueryClientProvider } from "@tanstack/react-query";
import { render, type RenderResult } from "@testing-library/react";
import type { ReactElement } from "react";
import { taoQueryClient } from "@/shared/api/queryClient";
import type { TrangThaiPhien } from "@/shared/api/session/trangThaiPhien";
import type { ThongTinToi } from "@/shared/api/types";
import { AuthContext, type GiaTriAuth } from "@/shared/hooks/useAuth";

export const NHAN_VIEN_MAU: ThongTinToi = {
  maNguoiDung: "019c0000-0000-7000-8000-00000000a001",
  vaiTro: "NHAN_VIEN",
  tenDangNhap: "nv.huong",
  hoTen: "Nguyễn Thị Hương",
  dsPhong: [
    {
      maPhong: "019c0000-0000-7000-8000-00000000b001",
      tenPhong: "Phòng thiết bị A1",
      coSo: "Cơ sở Linh Trung",
    },
    {
      maPhong: "019c0000-0000-7000-8000-00000000b002",
      tenPhong: "Phòng thiết bị B2",
      coSo: "Cơ sở Thủ Đức",
    },
  ],
};

export const QUAN_TRI_MAU: ThongTinToi = {
  maNguoiDung: "019c0000-0000-7000-8000-00000000a002",
  vaiTro: "QUAN_TRI",
  tenDangNhap: "qt.an",
  hoTen: "Trần Văn An",
  dsPhong: [],
};

export type TuyChonNguCanh = {
  /** Người đăng nhập; mặc định NHAN_VIEN_MAU. Bỏ qua khi truyền `trangThai`. */
  nguoiDung?: ThongTinToi;
  /** Trạng thái phiên tùy ý (ví dụ CHUA_DANG_NHAP để kiểm SCR-CM-02). */
  trangThai?: TrangThaiPhien;
  /** Ghi đè từng hàm của GiaTriAuth, ví dụ để theo dõi dangXuat. */
  auth?: Partial<GiaTriAuth>;
  queryClient?: QueryClient;
};

export function taoGiaTriAuth(tuyChon: TuyChonNguCanh = {}): GiaTriAuth {
  const trangThai: TrangThaiPhien = tuyChon.trangThai ?? {
    loai: "DA_DANG_NHAP",
    nguoiDung: tuyChon.nguoiDung ?? NHAN_VIEN_MAU,
  };
  return {
    trangThai,
    loiTaiThongTin: null,
    dangXuat: async () => undefined,
    taiLaiThongTinToi: async () => undefined,
    hoanTatDangNhap: () => undefined,
    tuyenBaoKhongDuQuyen: null,
    baoKhongDuQuyen: () => undefined,
    ...tuyChon.auth,
  };
}

export function hienThiVoiNguCanh(
  ui: ReactElement,
  tuyChon: TuyChonNguCanh = {},
): RenderResult & { queryClient: QueryClient; auth: GiaTriAuth } {
  const queryClient = tuyChon.queryClient ?? taoQueryClient();
  const auth = taoGiaTriAuth(tuyChon);
  const ketQua = render(
    <QueryClientProvider client={queryClient}>
      <AuthContext.Provider value={auth}>{ui}</AuthContext.Provider>
    </QueryClientProvider>,
  );
  return { ...ketQua, queryClient, auth };
}
