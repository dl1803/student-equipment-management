"use client";

// AuthProvider — giữ TrangThaiPhien cho toàn ứng dụng web (FA 4.4, 4.5; FC 5.10). Sở hữu: Thịnh.
// - Khôi phục phiên sau khi dựng trang (useEffect) để không lệch giao diện máy chủ/trình duyệt.
//   khoiPhucPhien dùng chung Promise làm mới với interceptor nên Strict Mode chạy effect hai lần
//   vẫn chỉ một lần POST /auth/lam-moi; lần chạy bị hủy không gọi GET /auth/toi.
// - Lắng nghe sự kiện kết thúc phiên của module session (làm mới thất bại, tài khoản bị khóa,
//   đăng xuất) → CHUA_DANG_NHAP kèm lý do; bộ nhớ đệm truy vấn đã được Providers xóa.
import { useQueryClient } from "@tanstack/react-query";
import {
  useCallback,
  useEffect,
  useMemo,
  useState,
  type ReactNode,
} from "react";
import { laApiError, type ApiError } from "@/shared/api/apiError";
import { dangXuat, khoiPhucPhien } from "@/shared/api/session/lamMoi";
import { dangKyKetThucPhien } from "@/shared/api/session/phien";
import type { TrangThaiPhien } from "@/shared/api/session/trangThaiPhien";
import type { ThongTinToi } from "@/shared/api/types";
import { AuthContext, type GiaTriAuth } from "@/shared/hooks/useAuth";
import { laVaiTroWeb } from "@/shared/tuyen/menuTheoVaiTro";
import { authToi } from "./api";
import { LOI_SAI_VAI_TRO } from "./hooks";

function thanhApiError(e: unknown): ApiError {
  return laApiError(e) ? e : { loai: "HTTP" };
}

export function AuthProvider({ children }: { children: ReactNode }) {
  const queryClient = useQueryClient();
  const [trangThai, setTrangThai] = useState<TrangThaiPhien>({
    loai: "KHOI_DONG",
  });
  const [loiTaiThongTin, setLoiTaiThongTin] = useState<ApiError | null>(null);
  const [tuyenBaoKhongDuQuyen, setTuyenBaoKhongDuQuyen] = useState<
    string | null
  >(null);

  /** Nhận ThongTinToi: chỉ NV và QT dùng web; vai trò khác bị đăng xuất (NFR-01). */
  const nhanNguoiDung = useCallback(async (nguoiDung: ThongTinToi) => {
    if (!laVaiTroWeb(nguoiDung.vaiTro)) {
      await dangXuat();
      setTrangThai({ loai: "CHUA_DANG_NHAP", loi: LOI_SAI_VAI_TRO });
      return;
    }
    setLoiTaiThongTin(null);
    setTrangThai({ loai: "DA_DANG_NHAP", nguoiDung });
  }, []);

  // Đăng ký trước khi khôi phục để không lỡ sự kiện kết thúc phiên của lần làm mới đầu tiên.
  useEffect(
    () =>
      dangKyKetThucPhien(({ lyDo, loi }) => {
        setLoiTaiThongTin(null);
        setTuyenBaoKhongDuQuyen(null);
        setTrangThai({
          loai: "CHUA_DANG_NHAP",
          ...(lyDo !== undefined ? { lyDo } : {}),
          ...(loi !== undefined ? { loi } : {}),
        });
      }),
    [],
  );

  useEffect(() => {
    let daHuy = false;
    async function khoiPhuc() {
      let coPhien: boolean;
      try {
        coPhien = await khoiPhucPhien();
      } catch {
        return; // Làm mới thất bại: sự kiện kết thúc phiên đã đặt CHUA_DANG_NHAP kèm lý do.
      }
      if (daHuy) return;
      if (!coPhien) {
        setTrangThai({ loai: "CHUA_DANG_NHAP" });
        return;
      }
      try {
        const nguoiDung = await authToi();
        if (!daHuy) await nhanNguoiDung(nguoiDung);
      } catch (e) {
        if (!daHuy) setLoiTaiThongTin(thanhApiError(e));
      }
    }
    void khoiPhuc();
    return () => {
      daHuy = true;
    };
  }, [nhanNguoiDung]);

  const taiLaiThongTinToi = useCallback(async () => {
    setLoiTaiThongTin(null);
    try {
      await nhanNguoiDung(await authToi());
    } catch (e) {
      setLoiTaiThongTin(thanhApiError(e));
    }
  }, [nhanNguoiDung]);

  const hoanTatDangNhap = useCallback(
    (nguoiDung: ThongTinToi) => {
      queryClient.clear(); // Không lộ dữ liệu của phiên trước (FA 4.7).
      void nhanNguoiDung(nguoiDung);
    },
    [queryClient, nhanNguoiDung],
  );

  const giaTri = useMemo<GiaTriAuth>(
    () => ({
      trangThai,
      loiTaiThongTin,
      dangXuat,
      taiLaiThongTinToi,
      hoanTatDangNhap,
      tuyenBaoKhongDuQuyen,
      baoKhongDuQuyen: setTuyenBaoKhongDuQuyen,
    }),
    [
      trangThai,
      loiTaiThongTin,
      taiLaiThongTinToi,
      hoanTatDangNhap,
      tuyenBaoKhongDuQuyen,
    ],
  );

  return <AuthContext.Provider value={giaTri}>{children}</AuthContext.Provider>;
}
