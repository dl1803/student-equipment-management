"use client";

// SCR-CM-03 Khởi động và khôi phục phiên (UI-Screens 4.6; FA 4.5). Sở hữu: Thịnh.
// Chỉ có chỉ báo đang tải ở giữa; không hiển thị tạm màn hình đăng nhập hay màn hình chính.
// Đã có token mà GET /auth/toi lỗi: báo lỗi tải thông tin kèm Thử lại (không đăng xuất).
import { StateView } from "@/shared/components/StateView";
import { useAuth } from "@/shared/hooks/useAuth";

export function KhoiDongScreen() {
  const { loiTaiThongTin, taiLaiThongTinToi } = useAuth();
  const thuLai = () => void taiLaiThongTinToi();

  return (
    <main className="flex min-h-screen items-center justify-center bg-surface-subtle px-gutter">
      {loiTaiThongTin === null ? (
        <StateView loai="DANG_TAI" />
      ) : loiTaiThongTin.loai === "HTTP" ? (
        <StateView
          loai="LOI_HE_THONG"
          {...(loiTaiThongTin.traceId !== undefined
            ? { traceId: loiTaiThongTin.traceId }
            : {})}
          onThuLai={thuLai}
        />
      ) : (
        <StateView loai="LOI_MANG" onThuLai={thuLai} />
      )}
    </main>
  );
}
