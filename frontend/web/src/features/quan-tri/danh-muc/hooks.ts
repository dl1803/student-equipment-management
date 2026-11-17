"use client";

// Truy vấn và thao tác ghi danh mục phòng, loại thiết bị (FA 4.7; FC 5.4). Sở hữu: Thịnh.
// Mặc định QueryClient: staleTime 0, retry 0; mutation networkMode "always", không cập nhật lạc
// quan. Ghi thành công thì đánh dấu lỗi thời danh sách của chính tài nguyên (FA 4.7).
import {
  keepPreviousData,
  useMutation,
  useQuery,
  useQueryClient,
} from "@tanstack/react-query";
import type { ApiError } from "@/shared/api/apiError";
import { TIEN_TO_KHOA } from "@/shared/api/tienToKhoa";
import type {
  LoaiThietBi,
  LoaiThietBiCapNhatRequest,
  LoaiThietBiTaoMoiRequest,
  PhongThietBi,
  PhongThietBiRequest,
} from "@/shared/api/types";
import {
  khoaLoaiThietBi,
  khoaPhong,
  qtDsLoaiThietBi,
  qtDsPhongThietBi,
  qtNgungSuDungLoaiThietBi,
  qtSuaLoaiThietBi,
  qtSuaPhongThietBi,
  qtTaoLoaiThietBi,
  qtTaoPhongThietBi,
  qtXoaLoaiThietBi,
  type DsLoaiParams,
  type DsPhongParams,
} from "./api";

/** Tùy chọn gắn vào mutation ở nơi dùng (ví dụ gắn lỗi theo trường vào biểu mẫu). */
type TuyChonGhi<TData> = {
  onSuccess?: (duLieu: TData) => void;
  onError?: (loi: ApiError) => void;
};

export function usePhongThietBiQuery(tham: DsPhongParams) {
  return useQuery({
    queryKey: khoaPhong.ds(tham),
    queryFn: ({ signal }) => qtDsPhongThietBi(tham, { signal }),
    placeholderData: keepPreviousData,
  });
}

export type LuuPhongBien = { ma: string | null; yeuCau: PhongThietBiRequest };

/** Thêm (ma = null) hoặc sửa phòng thiết bị. */
export function useLuuPhongMutation(tuyChon: TuyChonGhi<PhongThietBi> = {}) {
  const queryClient = useQueryClient();
  return useMutation<PhongThietBi, ApiError, LuuPhongBien>({
    mutationFn: ({ ma, yeuCau }) =>
      ma === null ? qtTaoPhongThietBi(yeuCau) : qtSuaPhongThietBi(ma, yeuCau),
    onSuccess: (duLieu) => {
      void queryClient.invalidateQueries({
        queryKey: TIEN_TO_KHOA.quanTriPhong,
      });
      tuyChon.onSuccess?.(duLieu);
    },
    onError: tuyChon.onError,
  });
}

export function useLoaiThietBiQuery(tham: DsLoaiParams) {
  return useQuery({
    queryKey: khoaLoaiThietBi.ds(tham),
    queryFn: ({ signal }) => qtDsLoaiThietBi(tham, { signal }),
    placeholderData: keepPreviousData,
  });
}

export type LuuLoaiBien =
  | { ma: null; yeuCau: LoaiThietBiTaoMoiRequest }
  | { ma: string; yeuCau: LoaiThietBiCapNhatRequest };

/** Thêm (ma = null, có phí trả trễ) hoặc sửa loại thiết bị (không có phí — FA 4.7, SCR-QT-06). */
export function useLuuLoaiMutation(tuyChon: TuyChonGhi<LoaiThietBi> = {}) {
  const queryClient = useQueryClient();
  return useMutation<LoaiThietBi, ApiError, LuuLoaiBien>({
    mutationFn: (bien) =>
      bien.ma === null
        ? qtTaoLoaiThietBi(bien.yeuCau)
        : qtSuaLoaiThietBi(bien.ma, bien.yeuCau),
    onSuccess: (duLieu) => {
      void queryClient.invalidateQueries({
        queryKey: TIEN_TO_KHOA.quanTriLoaiThietBi,
      });
      tuyChon.onSuccess?.(duLieu);
    },
    onError: tuyChon.onError,
  });
}

export function useXoaLoaiMutation(tuyChon: TuyChonGhi<void> = {}) {
  const queryClient = useQueryClient();
  return useMutation<void, ApiError, string>({
    mutationFn: qtXoaLoaiThietBi,
    onSuccess: () => {
      void queryClient.invalidateQueries({
        queryKey: TIEN_TO_KHOA.quanTriLoaiThietBi,
      });
      tuyChon.onSuccess?.();
    },
    onError: tuyChon.onError,
  });
}

export function useNgungSuDungLoaiMutation(
  tuyChon: TuyChonGhi<LoaiThietBi> = {},
) {
  const queryClient = useQueryClient();
  return useMutation<LoaiThietBi, ApiError, string>({
    mutationFn: qtNgungSuDungLoaiThietBi,
    onSuccess: (duLieu) => {
      void queryClient.invalidateQueries({
        queryKey: TIEN_TO_KHOA.quanTriLoaiThietBi,
      });
      tuyChon.onSuccess?.(duLieu);
    },
    onError: tuyChon.onError,
  });
}
