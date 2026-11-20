"use client";

// Truy vấn và thao tác ghi danh mục phòng, loại thiết bị, thiết bị (FA 4.7; FC 5.4). Sở hữu: Thịnh.
// Mặc định QueryClient: staleTime 0, retry 0; mutation networkMode "always", không cập nhật lạc
// quan. Ghi thành công thì đánh dấu lỗi thời danh sách của chính tài nguyên (FA 4.7).
import {
  keepPreviousData,
  useMutation,
  useQuery,
  useQueryClient,
} from "@tanstack/react-query";
import type { ApiError } from "@/shared/api/apiError";
import { taiTatCaTrang } from "@/shared/api/phanTrang";
import { TIEN_TO_KHOA } from "@/shared/api/tienToKhoa";
import type {
  LoaiThietBi,
  LoaiThietBiCapNhatRequest,
  LoaiThietBiTaoMoiRequest,
  PhongThietBi,
  PhongThietBiRequest,
  ThietBi,
  ThietBiCapNhatRequest,
  ThietBiTaoMoiRequest,
} from "@/shared/api/types";
import {
  khoaLoaiThietBi,
  khoaNhaCungCap,
  khoaPhong,
  khoaThietBi,
  qtDsLoaiThietBi,
  qtDsNhaCungCap,
  qtDsPhongThietBi,
  qtDsThietBi,
  qtNgungSuDungLoaiThietBi,
  qtSuaLoaiThietBi,
  qtSuaPhongThietBi,
  qtSuaThietBi,
  qtTaoLoaiThietBi,
  qtTaoPhongThietBi,
  qtTaoThietBi,
  qtXoaLoaiThietBi,
  type DsLoaiParams,
  type DsPhongParams,
  type DsThietBiParams,
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

// ---------- Nguồn lựa chọn (SCR-QT-04): tải đủ mọi trang, không chỉ trang 1 ----------

/** Mọi phòng thiết bị, để chọn phòng của thiết bị và lọc theo phòng. */
export function useTatCaPhongQuery() {
  return useQuery({
    queryKey: khoaPhong.tatCa(),
    queryFn: ({ signal }) =>
      taiTatCaTrang((trang, kichThuoc) =>
        qtDsPhongThietBi({ trang, kichThuoc }, { signal }),
      ),
  });
}

/** Mọi loại thiết bị, gồm cả loại đã ngưng sử dụng (hiển thị kèm nhãn, DS CMP-06). */
export function useTatCaLoaiQuery() {
  return useQuery({
    queryKey: khoaLoaiThietBi.tatCa(),
    queryFn: ({ signal }) =>
      taiTatCaTrang((trang, kichThuoc) =>
        qtDsLoaiThietBi({ trang, kichThuoc }, { signal }),
      ),
  });
}

/** Mọi nhà cung cấp (chỉ đọc ở giai đoạn 1, ĐX-KT-11), để gán cho thiết bị. */
export function useTatCaNhaCungCapQuery() {
  return useQuery({
    queryKey: khoaNhaCungCap.tatCa(),
    queryFn: ({ signal }) =>
      taiTatCaTrang((trang, kichThuoc) =>
        qtDsNhaCungCap({ trang, kichThuoc }, { signal }),
      ),
  });
}

// ---------- Thiết bị (SCR-QT-04) ----------

export function useThietBiQuery(tham: DsThietBiParams) {
  return useQuery({
    queryKey: khoaThietBi.ds(tham),
    queryFn: ({ signal }) => qtDsThietBi(tham, { signal }),
    placeholderData: keepPreviousData,
  });
}

export type LuuThietBiBien =
  | { ma: null; yeuCau: ThietBiTaoMoiRequest }
  | { ma: string; yeuCau: ThietBiCapNhatRequest };

/**
 * Nhập kho (ma = null, có mã và giá trị ban đầu) hoặc sửa thiết bị (không đổi mã, tình trạng, giá
 * trị — ThietBiCapNhatRequest không có các trường này).
 */
export function useLuuThietBiMutation(tuyChon: TuyChonGhi<ThietBi> = {}) {
  const queryClient = useQueryClient();
  return useMutation<ThietBi, ApiError, LuuThietBiBien>({
    mutationFn: (bien) =>
      bien.ma === null
        ? qtTaoThietBi(bien.yeuCau)
        : qtSuaThietBi(bien.ma, bien.yeuCau),
    onSuccess: (duLieu) => {
      void queryClient.invalidateQueries({
        queryKey: TIEN_TO_KHOA.quanTriThietBi,
      });
      tuyChon.onSuccess?.(duLieu);
    },
    onError: tuyChon.onError,
  });
}
