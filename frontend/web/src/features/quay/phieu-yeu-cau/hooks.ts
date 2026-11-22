"use client";

// Truy vấn phiếu yêu cầu ở quầy (FA 4.7; FC 5.4). Sở hữu: Thịnh.
// Mặc định QueryClient: staleTime 0, retry 0. Thứ tự dòng do máy chủ quyết định (cũ nhất trước,
// ĐX-API-28); client không tự sắp xếp.
import { keepPreviousData, useQuery } from "@tanstack/react-query";
import {
  khoaPhieuYeuCau,
  quayDsPhieuYeuCauMuon,
  quayKiemTraPhieuYeuCauMuon,
  type DsPhieuYeuCauParams,
} from "./api";

export function usePhieuChoDuyetQuery(
  tham: DsPhieuYeuCauParams,
  tuyChon: { enabled: boolean },
) {
  return useQuery({
    queryKey: khoaPhieuYeuCau.ds(tham),
    queryFn: ({ signal }) => quayDsPhieuYeuCauMuon(tham, { signal }),
    placeholderData: keepPreviousData,
    enabled: tuyChon.enabled,
  });
}

/** Kiểm tra lại điều kiện (không tác dụng phụ); null khi chưa có phiếu đã chọn. */
export function useKiemTraPhieuQuery(maYeuCau: string | null) {
  return useQuery({
    queryKey: khoaPhieuYeuCau.kiemTra(maYeuCau ?? ""),
    queryFn: ({ signal }) =>
      quayKiemTraPhieuYeuCauMuon(maYeuCau ?? "", { signal }),
    enabled: maYeuCau !== null,
  });
}
