"use client";

// AuthGate — bao quanh mọi tuyến (FA 4.4; UI-Screens 4.6). Sở hữu: Thịnh.
// - KHOI_DONG: chỉ SCR-CM-03, không hiển thị tạm trang đăng nhập hay màn hình chính.
// - CHUA_DANG_NHAP: mọi tuyến trừ /dang-nhap chuyển về /dang-nhap; KHÔNG dựng nội dung trang
//   trong lúc chuyển, để không lóe dữ liệu của phiên trước.
// - DA_DANG_NHAP: "/" và "/dang-nhap" chuyển tới trang chủ của vai trò; tuyến khác ở lại.
// Chặn tuyến ở client chỉ để điều hướng; máy chủ vẫn là nơi từ chối thật (FA 2.2).
import { usePathname, useRouter } from "next/navigation";
import { useEffect, type ReactNode } from "react";
import { useAuth } from "@/shared/hooks/useAuth";
import { BAN_DO_TUYEN } from "@/shared/tuyen/banDoTuyen";
import { laVaiTroWeb, tuyenTrangChu } from "@/shared/tuyen/menuTheoVaiTro";
import { KhoiDongScreen } from "./KhoiDongScreen";

const TUYEN_DANG_NHAP = BAN_DO_TUYEN["SCR-CM-02"].tuyen;
const TUYEN_KHOI_DONG = BAN_DO_TUYEN["SCR-CM-03"].tuyen;

export function AuthGate({ children }: { children: ReactNode }) {
  const { trangThai } = useAuth();
  const tuyen = usePathname();
  const router = useRouter();

  let dich: string | null = null;
  if (trangThai.loai === "CHUA_DANG_NHAP" && tuyen !== TUYEN_DANG_NHAP) {
    dich = TUYEN_DANG_NHAP;
  } else if (
    trangThai.loai === "DA_DANG_NHAP" &&
    (tuyen === TUYEN_DANG_NHAP || tuyen === TUYEN_KHOI_DONG) &&
    laVaiTroWeb(trangThai.nguoiDung.vaiTro)
  ) {
    dich = tuyenTrangChu(trangThai.nguoiDung.vaiTro);
  }

  useEffect(() => {
    if (dich !== null) router.replace(dich);
  }, [dich, router]);

  if (trangThai.loai === "KHOI_DONG" || dich !== null) {
    return <KhoiDongScreen />;
  }
  return children;
}
