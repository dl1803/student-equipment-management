"use client";

// Hộp xác nhận Duyệt phiếu yêu cầu — SCR-NV-03 (UI-Screens 6.3; UF-NV-01 bước 4–5; ĐX-UF-05; DS 9.2).
// Sở hữu: Thịnh.
// - Nêu SV, phòng, các loại và số lượng; hệ quả giữ chỗ và thông báo SV. POST .../duyet không có thân.
// - Chống bấm đúp: useThaoTacGhi khóa đồng bộ ngay lần bấm đầu (FA 3.11).
// - Thành công: trả phiếu máy chủ trả về (có hanGiuCho); client không tự tính hạn giữ chỗ.
// - Lỗi cần màn hình xử lý (E1, E2, E3, xung đột, chưa biết kết quả) chuyển lên qua onLoi; màn hình
//   đóng hộp và giữ khóa ở cấp màn hình, nên đóng hộp không mở đường gửi lại. Lỗi khác hiện trong hộp.
import { useState } from "react";
import type { PhieuYeuCauMuonQuay } from "@/shared/api/types";
import { Banner } from "@/shared/components/Banner";
import { ConfirmDialog } from "@/shared/components/ConfirmDialog";
import { useThaoTacGhi } from "@/shared/hooks/useThaoTacGhi";
import type { ThongBao } from "@/shared/labels/thongDiepLoi";
import { useDuyetPhieuMutation } from "../hooks";
import { phanLoaiLoiGhi, type LoiChoManHinh } from "../phanLoaiLoiGhi";
import { THONG_DIEP_PHIEU_YEU_CAU } from "../thongDiep";
import { TomTatPhieu } from "./TomTatPhieu";

export type DuyetPhieuDialogProps = {
  phieu: PhieuYeuCauMuonQuay;
  onDong: () => void;
  onXong: (phieuSauDuyet: PhieuYeuCauMuonQuay) => void;
  onLoi: (loi: LoiChoManHinh) => void;
};

export function DuyetPhieuDialog({
  phieu,
  onDong,
  onXong,
  onLoi,
}: DuyetPhieuDialogProps) {
  const [loi, setLoi] = useState<ThongBao | null>(null);
  const thaoTac = useThaoTacGhi(
    useDuyetPhieuMutation({
      onSuccess: onXong,
      onError: (e) => {
        const kq = phanLoaiLoiGhi(e, "DUYET");
        if (kq.loai === "TRONG_HOP") setLoi(kq.thongBao);
        else onLoi(kq);
      },
    }),
  );

  return (
    <ConfirmDialog
      open
      tieuDe="Duyệt phiếu yêu cầu"
      tomTat={<TomTatPhieu phieu={phieu} />}
      heQua={THONG_DIEP_PHIEU_YEU_CAU.HE_QUA_DUYET}
      nhanXacNhan="Duyệt"
      bienThe="primary"
      dangXuLy={thaoTac.dangGui}
      loi={
        loi !== null ? (
          <Banner
            tong={loi.tong}
            {...(loi.traceId !== undefined ? { traceId: loi.traceId } : {})}
          >
            {loi.noiDung}
          </Banner>
        ) : undefined
      }
      onXacNhan={() => {
        setLoi(null);
        thaoTac.gui(phieu.maYeuCau);
      }}
      onDong={onDong}
    />
  );
}
