"use client";

// SCR-NV-05 Giao thiết bị, bước 2: Chọn nguồn giao và kiểm tra nguồn
// Sở hữu: Thịnh. D13-T: khung bốn bước (thanh tiến trình) và điều kiện vào; nội dung bước hiện thực ở
// task của màn hình theo UI-Screens.md 6.5.
import { useBaoVeBuoc } from "./baoVeBuoc";
import { KhungBuocGiao } from "./components/KhungBuocGiao";

export function GiaoThietBiNguonScreen() {
  const duocHienThi = useBaoVeBuoc(2);
  if (!duocHienThi) return null;
  return (
    <KhungBuocGiao buoc={2}>
      <h2 className="type-title-section text-text-primary">
        Chọn nguồn giao và kiểm tra nguồn
      </h2>
    </KhungBuocGiao>
  );
}
