"use client";

// Ngữ cảnh PhieuYeuCauDuocChon (FA 4.3, 4.7; FC dòng "Ngữ cảnh luồng"). Sở hữu: Thịnh.
// Đặt tại app/nv/phieu-yeu-cau/layout.tsx; sống tới khi rời nhóm tuyến phiếu yêu cầu hoặc tải lại.
// - phieu: dòng đã chọn ở SCR-NV-02. Hợp đồng không có GET chi tiết phiếu cho quầy nên SCR-NV-03 chỉ
//   có dữ liệu này; thiếu (tải lại trang) thì SCR-NV-03 quay về danh sách (ĐX-UI-08).
// - thongBao: lời báo SCR-NV-03 để lại khi đưa NV về danh sách (403 phòng, phiếu không còn), để
//   SCR-NV-02 hiển thị một lần.
import {
  createContext,
  useContext,
  useMemo,
  useState,
  type ReactNode,
} from "react";
import type { PhieuYeuCauMuonQuay } from "@/shared/api/types";
import type { ThongBao } from "@/shared/labels/thongDiepLoi";

export type GiaTriPhieuYeuCauDuocChon = {
  phieu: PhieuYeuCauMuonQuay | null;
  chonPhieu: (phieu: PhieuYeuCauMuonQuay) => void;
  /** Bỏ phiếu đã chọn và để lại lời báo cho danh sách (null nếu không có gì để báo). */
  boChon: (thongBao: ThongBao | null) => void;
  thongBao: ThongBao | null;
  xoaThongBao: () => void;
};

export const PhieuYeuCauDuocChonContext =
  createContext<GiaTriPhieuYeuCauDuocChon | null>(null);

export function PhieuYeuCauDuocChonProvider({
  children,
}: {
  children: ReactNode;
}) {
  const [phieu, setPhieu] = useState<PhieuYeuCauMuonQuay | null>(null);
  const [thongBao, setThongBao] = useState<ThongBao | null>(null);
  const giaTri = useMemo<GiaTriPhieuYeuCauDuocChon>(
    () => ({
      phieu,
      chonPhieu: (moi) => {
        setThongBao(null);
        setPhieu(moi);
      },
      boChon: (tb) => {
        setPhieu(null);
        setThongBao(tb);
      },
      thongBao,
      xoaThongBao: () => setThongBao(null),
    }),
    [phieu, thongBao],
  );
  return (
    <PhieuYeuCauDuocChonContext.Provider value={giaTri}>
      {children}
    </PhieuYeuCauDuocChonContext.Provider>
  );
}

export function usePhieuYeuCauDuocChon(): GiaTriPhieuYeuCauDuocChon {
  const giaTri = useContext(PhieuYeuCauDuocChonContext);
  if (giaTri === null) {
    throw new Error(
      "usePhieuYeuCauDuocChon phải dùng bên trong PhieuYeuCauDuocChonProvider (app/nv/phieu-yeu-cau/layout.tsx)",
    );
  }
  return giaTri;
}
