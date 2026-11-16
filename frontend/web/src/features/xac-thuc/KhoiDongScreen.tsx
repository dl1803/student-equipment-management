"use client";

// SCR-CM-03 Khởi động và khôi phục phiên (UI-Screens 4.6; FA 4.5). Sở hữu: Thịnh.
// Chỉ có chỉ báo đang tải ở giữa; không hiển thị tạm màn hình đăng nhập hay màn hình chính.
// Đã có token mà GET /auth/toi lỗi: báo lỗi tải thông tin kèm Thử lại; hệ thống không tự đăng
// xuất (FA 4.5), nhưng người dùng được chủ động Đăng xuất để không bị kẹt khi lỗi kéo dài.
import { useState } from "react";
import { Button } from "@/shared/components/Button";
import { StateView } from "@/shared/components/StateView";
import { useAuth } from "@/shared/hooks/useAuth";

export function KhoiDongScreen() {
  const { loiTaiThongTin, taiLaiThongTinToi, dangXuat } = useAuth();
  const [dangDangXuat, setDangDangXuat] = useState(false);
  const thuLai = () => void taiLaiThongTinToi();
  const handleDangXuat = () => {
    setDangDangXuat(true);
    void dangXuat(); // Luôn xóa phiên cục bộ; AuthGate chuyển về /dang-nhap.
  };

  return (
    <main className="flex min-h-screen items-center justify-center bg-surface-subtle px-gutter">
      {loiTaiThongTin === null ? (
        <StateView loai="DANG_TAI" />
      ) : (
        <div className="flex flex-col items-center gap-2">
          {loiTaiThongTin.loai === "HTTP" ? (
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
          <Button
            bienThe="tertiary"
            onClick={handleDangXuat}
            dangXuLy={dangDangXuat}
          >
            Đăng xuất
          </Button>
        </div>
      )}
    </main>
  );
}
