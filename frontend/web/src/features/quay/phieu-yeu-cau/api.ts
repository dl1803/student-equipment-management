// Hàm API phiếu yêu cầu ở quầy (SCR-NV-02, SCR-NV-03; FA 4.6; FC 5.4). Sở hữu: Thịnh.
// Tên hàm trùng operationId; đường dẫn tương đối, không lặp /api/v1. Hợp đồng không có GET chi tiết
// một phiếu yêu cầu cho quầy: SCR-NV-03 dùng dòng đã chọn ở SCR-NV-02 (UI-Screens 6.3 ghi chú).
import { apiClient } from "@/shared/api/client";
import { TIEN_TO_KHOA } from "@/shared/api/tienToKhoa";
import type {
  KetQuaKiemTraPhieu,
  operations,
  PhieuYeuCauMuonQuay,
  PhieuYeuCauMuonQuayPage,
  TuChoiRequest,
} from "@/shared/api/types";

type TuChon = { signal?: AbortSignal };

export type DsPhieuYeuCauParams = NonNullable<
  operations["quayDsPhieuYeuCauMuon"]["parameters"]["query"]
>;

export const khoaPhieuYeuCau = {
  ds: (tham: DsPhieuYeuCauParams) =>
    [...TIEN_TO_KHOA.quayPhieuYeuCau, "ds", tham] as const,
  kiemTra: (maYeuCau: string) =>
    [...TIEN_TO_KHOA.quayPhieuYeuCau, "kiem-tra", maYeuCau] as const,
};

export async function quayDsPhieuYeuCauMuon(
  tham: DsPhieuYeuCauParams,
  tuyChon?: TuChon,
): Promise<PhieuYeuCauMuonQuayPage> {
  const res = await apiClient.get<PhieuYeuCauMuonQuayPage>(
    "/quay/phieu-yeu-cau-muon",
    { params: tham, signal: tuyChon?.signal },
  );
  return res.data;
}

export async function quayKiemTraPhieuYeuCauMuon(
  maYeuCau: string,
  tuyChon?: TuChon,
): Promise<KetQuaKiemTraPhieu> {
  const res = await apiClient.get<KetQuaKiemTraPhieu>(
    `/quay/phieu-yeu-cau-muon/${encodeURIComponent(maYeuCau)}/kiem-tra`,
    { signal: tuyChon?.signal },
  );
  return res.data;
}

/** Duyệt và giữ chỗ (không có thân); hạn giữ chỗ do máy chủ đặt, trả trong phiếu sau khi duyệt. */
export async function quayDuyetPhieuYeuCauMuon(
  maYeuCau: string,
): Promise<PhieuYeuCauMuonQuay> {
  const res = await apiClient.post<PhieuYeuCauMuonQuay>(
    `/quay/phieu-yeu-cau-muon/${encodeURIComponent(maYeuCau)}/duyet`,
  );
  return res.data;
}

export async function quayTuChoiPhieuYeuCauMuon(
  maYeuCau: string,
  yeuCau: TuChoiRequest,
): Promise<PhieuYeuCauMuonQuay> {
  const res = await apiClient.post<PhieuYeuCauMuonQuay>(
    `/quay/phieu-yeu-cau-muon/${encodeURIComponent(maYeuCau)}/tu-choi`,
    yeuCau,
  );
  return res.data;
}
