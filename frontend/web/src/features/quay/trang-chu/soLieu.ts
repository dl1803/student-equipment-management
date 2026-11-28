"use client";

// Số liệu đầu SCR-NV-01 (UI-Screens 6.1): lấy tongSo do máy chủ đếm từ các API danh sách sẵn có
// (kichThuoc=1 để nhẹ); client không tự đếm hay tính (NT-01). Tên hàm trùng operationId. Sở hữu: Thịnh.
import { useQuery } from "@tanstack/react-query";
import { apiClient } from "@/shared/api/client";
import { TIEN_TO_KHOA } from "@/shared/api/tienToKhoa";

type CoTongSo = { tongSo: number };

async function quayDsPhieuYeuCauMuon(signal?: AbortSignal) {
  const res = await apiClient.get<CoTongSo>("/quay/phieu-yeu-cau-muon", {
    params: { trangThai: "CHO_DUYET", trang: 1, kichThuoc: 1 },
    signal,
  });
  return res.data.tongSo;
}

async function quayDsPhieuQuaHan30Ngay(signal?: AbortSignal) {
  const res = await apiClient.get<CoTongSo>(
    "/quay/phieu-muon/qua-han-30-ngay",
    { params: { trang: 1, kichThuoc: 1 }, signal },
  );
  return res.data.tongSo;
}

async function quayDsDanhSachCho(signal?: AbortSignal) {
  const res = await apiClient.get<CoTongSo>("/quay/danh-sach-cho", {
    params: { trang: 1, kichThuoc: 1 },
    signal,
  });
  return res.data.tongSo;
}

function useTongSo(
  khoa: readonly unknown[],
  ham: (signal?: AbortSignal) => Promise<number>,
): number | null {
  const q = useQuery({
    queryKey: [...khoa, "tong-so-trang-chu"],
    queryFn: ({ signal }) => ham(signal),
  });
  return q.data ?? null;
}

export function useSoLieuTrangChu() {
  return {
    phieuChoDuyet: useTongSo(
      TIEN_TO_KHOA.quayPhieuYeuCau,
      quayDsPhieuYeuCauMuon,
    ),
    quaHan30Ngay: useTongSo(
      TIEN_TO_KHOA.quayPhieuMuon,
      quayDsPhieuQuaHan30Ngay,
    ),
    danhSachCho: useTongSo(TIEN_TO_KHOA.quayDanhSachCho, quayDsDanhSachCho),
  };
}

/** Lời chào theo giờ trên máy người dùng (chỉ trình bày, không phải nghiệp vụ). */
export function loiChao(gio: number): string {
  if (gio < 11) return "Chào buổi sáng";
  if (gio < 14) return "Chào buổi trưa";
  if (gio < 18) return "Chào buổi chiều";
  return "Chào buổi tối";
}
