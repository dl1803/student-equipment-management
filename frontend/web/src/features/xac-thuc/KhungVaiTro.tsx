"use client";

// Khung ứng dụng theo vai trò cho /nv và /qt (FA 4.4; UI-Screens 4.2–4.3; DS 11.2). Sở hữu: Thịnh.
// Bố cục (DS 11.2, Wireframes 2.2/2.3, ĐX-DS-17): thanh menu bên (CMP-29) cao hết cửa sổ ở trái, có
// tên hệ thống ở đầu; cột nội dung ở phải gồm thanh đầu trang (CMP-30) và nội dung, cuộn dọc độc lập.
// Sai vai trò: chuyển tới trang chủ của vai trò hiện tại kèm banner "Bạn không có quyền thực hiện
// thao tác này" (UI-Screens 3.2); trong lúc chuyển không dựng nội dung trang.
import { usePathname, useRouter } from "next/navigation";
import { useEffect, useState, type ReactNode } from "react";
import { Banner } from "@/shared/components/Banner";
import { Header } from "@/shared/components/Header";
import { Sidebar } from "@/shared/components/Sidebar";
import { useAuth } from "@/shared/hooks/useAuth";
import { THONG_DIEP_LOI_CHUNG } from "@/shared/labels/thongDiepLoi";
import { nhanVaiTro } from "@/shared/labels/vaiTro";
import {
  laVaiTroWeb,
  menuCua,
  tuyenTrangChu,
  type VaiTroWeb,
} from "@/shared/tuyen/menuTheoVaiTro";

/** Tên hệ thống ở đầu thanh menu bên, chung cho NV và QT (Wireframes 2.2, 2.3). */
const TEN_HE_THONG = "Quản lý mượn trả thiết bị";

export type KhungVaiTroProps = {
  vaiTro: VaiTroWeb;
  children: ReactNode;
};

export function KhungVaiTro({ vaiTro, children }: KhungVaiTroProps) {
  const { trangThai, dangXuat, tuyenBaoKhongDuQuyen, baoKhongDuQuyen } =
    useAuth();
  const tuyen = usePathname();
  const router = useRouter();
  const [dangDangXuat, setDangDangXuat] = useState(false);

  const nguoiDung =
    trangThai.loai === "DA_DANG_NHAP" ? trangThai.nguoiDung : null;
  const vaiTroHienTai =
    nguoiDung !== null && laVaiTroWeb(nguoiDung.vaiTro)
      ? nguoiDung.vaiTro
      : null;
  const tuyenChuyenVe =
    vaiTroHienTai !== null && vaiTroHienTai !== vaiTro
      ? tuyenTrangChu(vaiTroHienTai)
      : null;

  useEffect(() => {
    if (tuyenChuyenVe === null) return;
    baoKhongDuQuyen(tuyenChuyenVe);
    router.replace(tuyenChuyenVe);
  }, [tuyenChuyenVe, baoKhongDuQuyen, router]);

  // Banner chỉ dành cho lần đến tuyến đích ngay sau khi bị chuyển về; rời tuyến đó thì xóa dấu để
  // lần sau quay lại trang chủ không hiện lại. Khung đang chuyển hướng không xóa (chưa tới đích).
  useEffect(() => {
    if (
      tuyenChuyenVe === null &&
      tuyenBaoKhongDuQuyen !== null &&
      tuyen !== tuyenBaoKhongDuQuyen
    ) {
      baoKhongDuQuyen(null);
    }
  }, [tuyenChuyenVe, tuyenBaoKhongDuQuyen, tuyen, baoKhongDuQuyen]);

  // AuthGate đã chặn trạng thái khác; ở đây chỉ còn chờ chuyển tuyến khi sai vai trò.
  if (nguoiDung === null || tuyenChuyenVe !== null) return null;

  const handleDangXuat = () => {
    setDangDangXuat(true);
    void dangXuat(); // Luôn xóa phiên cục bộ; AuthGate chuyển về /dang-nhap.
  };

  return (
    <div className="flex h-screen bg-surface-default">
      <Sidebar
        tenHeThong={TEN_HE_THONG}
        dsNhom={menuCua(vaiTro)}
        tuyenHienTai={tuyen}
        tuyenTrangChu={tuyenTrangChu(vaiTro)}
      />
      <div className="flex min-w-0 flex-1 flex-col overflow-y-auto">
        <Header
          hoTen={nguoiDung.hoTen}
          nhanVaiTro={nhanVaiTro(nguoiDung.vaiTro)}
          onDangXuat={handleDangXuat}
          dangDangXuat={dangDangXuat}
        />
        <div className="mx-auto flex w-full max-w-web-content-max flex-col gap-section px-gutter pb-gutter">
          {tuyenBaoKhongDuQuyen === tuyen ? (
            <Banner tong="danger">{THONG_DIEP_LOI_CHUNG.KHONG_DU_QUYEN}</Banner>
          ) : null}
          {children}
        </div>
      </div>
    </div>
  );
}
