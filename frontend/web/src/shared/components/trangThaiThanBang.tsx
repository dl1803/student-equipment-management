// Trạng thái thân bảng (FC 5.2; DS 9.3, 9.5): đang tải → lỗi → rỗng → dữ liệu. Sở hữu: Thịnh (shared
// core). Dùng chung cho bảng danh mục (SCR-QT-02..04) và bảng quầy (SCR-NV-02).
// Khi tải lại mà đã có dữ liệu thì giữ dữ liệu cũ (placeholderData), không quay về đang tải.
import type { ReactNode } from "react";
import { laApiError } from "@/shared/api/apiError";
import { StateView } from "@/shared/components/StateView";

type TruyVanBang = {
  isPending: boolean;
  error: unknown;
  data: { danhSach: ReadonlyArray<unknown> } | undefined;
  refetch: () => unknown;
};

export function trangThaiThanBang(
  truyVan: TruyVanBang,
  rong: { thongDiep: string; xoaBoLoc?: () => void },
): ReactNode | undefined {
  if (truyVan.isPending) return <StateView loai="DANG_TAI" />;
  const thuLai = () => void truyVan.refetch();
  if (truyVan.data === undefined) {
    const loi = truyVan.error;
    if (laApiError(loi) && loi.loai === "HTTP") {
      return (
        <StateView
          loai="LOI_HE_THONG"
          {...(loi.traceId !== undefined ? { traceId: loi.traceId } : {})}
          onThuLai={thuLai}
        />
      );
    }
    return <StateView loai="LOI_MANG" onThuLai={thuLai} />;
  }
  if (truyVan.data.danhSach.length === 0) {
    return rong.xoaBoLoc !== undefined ? (
      <StateView
        loai="RONG"
        thongDiep={rong.thongDiep}
        doBoLoc
        hanhDong={{ nhan: "Xóa bộ lọc", onClick: rong.xoaBoLoc }}
      />
    ) : (
      <StateView loai="RONG" thongDiep={rong.thongDiep} />
    );
  }
  return undefined;
}
