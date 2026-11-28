"use client";

// SCR-NV-06 Giao thiết bị, bước 3: Thêm thiết bị và ghi tình trạng ban đầu
// Sở hữu: Thịnh. D13-T: khung bốn bước (thanh tiến trình) và điều kiện vào; nội dung bước hiện thực ở
// task của màn hình theo UI-Screens.md 6.6.
import { useBaoVeBuoc } from "./baoVeBuoc";
import { KhungBuocGiao } from "./components/KhungBuocGiao";

export function GiaoThietBiThietBiScreen() {
  const duocHienThi = useBaoVeBuoc(3);
  if (!duocHienThi) return null;
  return (
    <KhungBuocGiao buoc={3}>
      <h2 className="type-title-section text-text-primary">
        Thêm thiết bị và ghi tình trạng ban đầu
      </h2>
    </KhungBuocGiao>
  );
}
