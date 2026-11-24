// Layout /nv/phieu-yeu-cau. Đặt ngữ cảnh PhieuYeuCauDuocChon (FA 4.7) cho SCR-NV-02 và SCR-NV-03.
// Sở hữu: Thịnh.
import type { ReactNode } from "react";
import { PhieuYeuCauDuocChonProvider } from "@/features/quay/phieu-yeu-cau/PhieuYeuCauDuocChon";

export default function Layout({ children }: { children: ReactNode }) {
  return <PhieuYeuCauDuocChonProvider>{children}</PhieuYeuCauDuocChonProvider>;
}
