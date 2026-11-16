"use client";

// Khung ứng dụng theo vai trò cho /nv và /qt (FA 4.4; UI-Screens 4.2–4.3; DS 11.2). Sở hữu: Thịnh.
// Thanh đầu trang (CMP-30) chạy suốt chiều ngang; menu bên (CMP-29) ở trái; nội dung cuộn ở phải.
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

/** Tên hệ thống ở thanh đầu trang NV (Wireframes 2.2); khung QT (2.3) không ghi. */
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

  // AuthGate đã chặn trạng thái khác; ở đây chỉ còn chờ chuyển tuyến khi sai vai trò.
  if (nguoiDung === null || tuyenChuyenVe !== null) return null;

  const handleDangXuat = () => {
    setDangDangXuat(true);
    void dangXuat(); // Luôn xóa phiên cục bộ; AuthGate chuyển về /dang-nhap.
  };

  return (
    <div className="flex h-screen flex-col bg-surface-subtle">
      <Header
        {...(vaiTro === "NHAN_VIEN" ? { tenHeThong: TEN_HE_THONG } : {})}
        hoTen={nguoiDung.hoTen}
        nhanVaiTro={nhanVaiTro(nguoiDung.vaiTro)}
        onDangXuat={handleDangXuat}
        dangDangXuat={dangDangXuat}
      />
      <div className="flex min-h-0 flex-1">
        <Sidebar
          dsNhom={menuCua(vaiTro)}
          tuyenHienTai={tuyen}
          tuyenTrangChu={tuyenTrangChu(vaiTro)}
        />
        <div className="min-w-0 flex-1 overflow-y-auto">
          <div className="mx-auto flex w-full max-w-web-content-max flex-col gap-section p-gutter">
            {tuyenBaoKhongDuQuyen === tuyen ? (
              <Banner tong="danger">
                {THONG_DIEP_LOI_CHUNG.KHONG_DU_QUYEN}
              </Banner>
            ) : null}
            {children}
          </div>
        </div>
      </div>
    </div>
  );
}
