// Cấu hình menu theo vai trò (UI-Screens 4.2, 4.3; FA 4.4). Sở hữu: Thịnh (shared core).
// FA 4.4 đặt cấu hình này ở features/xac-thuc; được chuyển về shared/tuyen vì trang chủ NV-01,
// QT-01 (feature khác) cũng dựng lối vào từ đây, mà FC 4.1 cấm import chéo giữa các tính năng.
// Mọi href lấy từ BAN_DO_TUYEN nên không thể trỏ tới tuyến ngoài giai đoạn 1.
import type { NhomMenu } from "@/shared/components/Sidebar";
import { BAN_DO_TUYEN, type MaManHinhWeb } from "./banDoTuyen";

export type VaiTroWeb = "NHAN_VIEN" | "QUAN_TRI";

type MucCauHinh = {
  nhan: string;
  ma: MaManHinhWeb;
  /** Mục mở đầu một luồng nhiều bước: đánh dấu đang chọn ở mọi bước cùng thư mục cha. */
  laLuongNhieuBuoc?: true;
};
type NhomCauHinh = { tieuDe?: string; dsMuc: ReadonlyArray<MucCauHinh> };

const MENU: Record<VaiTroWeb, ReadonlyArray<NhomCauHinh>> = {
  // UI-Screens 4.2
  NHAN_VIEN: [
    {
      dsMuc: [
        { nhan: "Trang chủ", ma: "SCR-NV-01" },
        { nhan: "Phiếu yêu cầu chờ duyệt", ma: "SCR-NV-02" },
        { nhan: "Giao thiết bị", ma: "SCR-NV-04", laLuongNhieuBuoc: true },
        { nhan: "Nhận trả thiết bị", ma: "SCR-NV-08" },
        { nhan: "Biên bản và thu phạt", ma: "SCR-NV-11" },
        { nhan: "Phiếu chưa trả quá 30 ngày", ma: "SCR-NV-12" },
        { nhan: "Danh sách chờ", ma: "SCR-NV-13" },
        { nhan: "Bảo trì và sửa chữa", ma: "SCR-NV-14" },
      ],
    },
  ],
  // UI-Screens 4.3
  QUAN_TRI: [
    { dsMuc: [{ nhan: "Trang chủ", ma: "SCR-QT-01" }] },
    {
      tieuDe: "Danh mục",
      dsMuc: [
        { nhan: "Phòng thiết bị", ma: "SCR-QT-02" },
        { nhan: "Loại thiết bị", ma: "SCR-QT-03" },
        { nhan: "Thiết bị", ma: "SCR-QT-04" },
      ],
    },
    {
      tieuDe: "Cấu hình",
      dsMuc: [
        { nhan: "Quy định mượn", ma: "SCR-QT-05" },
        { nhan: "Phí trả trễ", ma: "SCR-QT-06" },
        { nhan: "Khung phạt", ma: "SCR-QT-07" },
        { nhan: "Tham số hệ thống", ma: "SCR-QT-08" },
      ],
    },
    {
      dsMuc: [
        { nhan: "Tài khoản nhân viên", ma: "SCR-QT-09" },
        { nhan: "Biên bản vi phạm", ma: "SCR-QT-10" },
        { nhan: "Thanh lý", ma: "SCR-QT-11" },
        { nhan: "Báo cáo", ma: "SCR-QT-12" },
      ],
    },
  ],
};

const TRANG_CHU: Record<VaiTroWeb, MaManHinhWeb> = {
  NHAN_VIEN: "SCR-NV-01",
  QUAN_TRI: "SCR-QT-01",
};

/** "/nv/giao-thiet-bi/the" → "/nv/giao-thiet-bi" (tiền tố chung của bốn bước SCR-NV-04..07). */
function thuMucCha(tuyen: string): string {
  return tuyen.slice(0, tuyen.lastIndexOf("/"));
}

export function laVaiTroWeb(vaiTro: string): vaiTro is VaiTroWeb {
  return vaiTro === "NHAN_VIEN" || vaiTro === "QUAN_TRI";
}

export function tuyenTrangChu(vaiTro: VaiTroWeb): string {
  return BAN_DO_TUYEN[TRANG_CHU[vaiTro]].tuyen;
}

/** Nhóm menu của vai trò cho Sidebar (CMP-29). */
export function menuCua(vaiTro: VaiTroWeb): NhomMenu[] {
  return MENU[vaiTro].map((nhom) => ({
    ...(nhom.tieuDe !== undefined ? { tieuDe: nhom.tieuDe } : {}),
    dsMuc: nhom.dsMuc.map((muc) => {
      const href = BAN_DO_TUYEN[muc.ma].tuyen;
      return muc.laLuongNhieuBuoc === true
        ? { nhan: muc.nhan, href, tienToNhom: thuMucCha(href) }
        : { nhan: muc.nhan, href };
    }),
  }));
}

/** Lối vào chức năng ở trang chủ của vai trò: menu trừ chính trang chủ (UI-Screens 6.1, 7.1). */
export function loiVaoCua(vaiTro: VaiTroWeb): NhomMenu[] {
  const trangChu = tuyenTrangChu(vaiTro);
  return menuCua(vaiTro)
    .map((nhom) => ({
      ...nhom,
      dsMuc: nhom.dsMuc.filter((muc) => muc.href !== trangChu),
    }))
    .filter((nhom) => nhom.dsMuc.length > 0);
}

/** Mã màn hình có trong menu của vai trò (để kiểm thử đối chiếu). */
export function maManHinhTrongMenu(vaiTro: VaiTroWeb): MaManHinhWeb[] {
  return MENU[vaiTro].flatMap((nhom) => nhom.dsMuc.map((muc) => muc.ma));
}
