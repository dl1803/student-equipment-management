"use client";

// Truy vấn và thao tác ghi phiếu yêu cầu ở quầy (FA 4.7; FC 5.4). Sở hữu: Thịnh.
// Mặc định QueryClient: staleTime 0, retry 0; mutation networkMode "always", không cập nhật lạc
// quan, không tự gửi lại. Thứ tự dòng do máy chủ quyết định (cũ nhất trước, ĐX-API-28); client
// không tự sắp xếp.
import {
  keepPreviousData,
  useMutation,
  useQuery,
  useQueryClient,
  type QueryClient,
} from "@tanstack/react-query";
import type { ApiError } from "@/shared/api/apiError";
import { TIEN_TO_KHOA } from "@/shared/api/tienToKhoa";
import type { PhieuYeuCauMuonQuay, TuChoiRequest } from "@/shared/api/types";
import {
  khoaPhieuYeuCau,
  quayDsPhieuYeuCauMuon,
  quayDuyetPhieuYeuCauMuon,
  quayKiemTraPhieuYeuCauMuon,
  quayTuChoiPhieuYeuCauMuon,
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

/** Kiểm tra lại điều kiện (không tác dụng phụ); null khi chưa có phiếu hoặc phiếu đã xử lý xong. */
export function useKiemTraPhieuQuery(maYeuCau: string | null) {
  return useQuery({
    queryKey: khoaPhieuYeuCau.kiemTra(maYeuCau ?? ""),
    queryFn: ({ signal }) =>
      quayKiemTraPhieuYeuCauMuon(maYeuCau ?? "", { signal }),
    enabled: maYeuCau !== null,
  });
}

/**
 * Duyệt, từ chối thành công → đánh dấu lỗi thời quay/phieu-yeu-cau, quay/loai-thiet-bi,
 * quay/danh-sach-cho (FA 4.7). Trừ kết quả kiểm tra của CHÍNH phiếu vừa xử lý: phiếu không còn Chờ
 * duyệt nên kiểm tra lại sẽ nhận 404 và đưa NV rời màn hình trước khi kịp thấy kết quả.
 */
function danhDauLoiThoiSauXuLy(queryClient: QueryClient, maYeuCau: string) {
  const khoaKiemTraCuaPhieu = JSON.stringify(khoaPhieuYeuCau.kiemTra(maYeuCau));
  void queryClient.invalidateQueries({
    queryKey: TIEN_TO_KHOA.quayPhieuYeuCau,
    predicate: (q) => JSON.stringify(q.queryKey) !== khoaKiemTraCuaPhieu,
  });
  void queryClient.invalidateQueries({
    queryKey: TIEN_TO_KHOA.quayLoaiThietBi,
  });
  void queryClient.invalidateQueries({
    queryKey: TIEN_TO_KHOA.quayDanhSachCho,
  });
}

/** Tùy chọn gắn vào mutation ở nơi dùng. */
type TuChonGhi<TData> = {
  onSuccess?: (duLieu: TData) => void;
  onError?: (loi: ApiError) => void;
};

/** Biến của Duyệt: mã phiếu (không có thân). */
export function useDuyetPhieuMutation(
  tuyChon: TuChonGhi<PhieuYeuCauMuonQuay> = {},
) {
  const queryClient = useQueryClient();
  return useMutation<PhieuYeuCauMuonQuay, ApiError, string>({
    mutationFn: quayDuyetPhieuYeuCauMuon,
    onSuccess: (duLieu, maYeuCau) => {
      danhDauLoiThoiSauXuLy(queryClient, maYeuCau);
      tuyChon.onSuccess?.(duLieu);
    },
    onError: tuyChon.onError,
  });
}

export type TuChoiPhieuBien = { maYeuCau: string; yeuCau: TuChoiRequest };

export function useTuChoiPhieuMutation(
  tuyChon: TuChonGhi<PhieuYeuCauMuonQuay> = {},
) {
  const queryClient = useQueryClient();
  return useMutation<PhieuYeuCauMuonQuay, ApiError, TuChoiPhieuBien>({
    mutationFn: ({ maYeuCau, yeuCau }) =>
      quayTuChoiPhieuYeuCauMuon(maYeuCau, yeuCau),
    onSuccess: (duLieu, { maYeuCau }) => {
      danhDauLoiThoiSauXuLy(queryClient, maYeuCau);
      tuyChon.onSuccess?.(duLieu);
    },
    onError: tuyChon.onError,
  });
}
