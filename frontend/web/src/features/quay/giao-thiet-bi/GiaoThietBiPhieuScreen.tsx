"use client";

// SCR-NV-07 Giao thiết bị, bước 4: Phiếu mượn vừa lập
// Sở hữu: Thịnh. D13-T: khung bốn bước (thanh tiến trình) và điều kiện vào; nội dung bước hiện thực ở
// task của màn hình theo UI-Screens.md 6.7.
import { useBaoVeBuoc } from "./baoVeBuoc";
import { KhungBuocGiao } from "./components/KhungBuocGiao";

export function GiaoThietBiPhieuScreen() {
  const duocHienThi = useBaoVeBuoc(4);
  if (!duocHienThi) return null;
  return (
    <KhungBuocGiao buoc={4}>
      <h2 className="type-title-section text-text-primary">
        Phiếu mượn vừa lập
      </h2>
    </KhungBuocGiao>
  );
}
