// Hàm API của luồng Giao thiết bị (SCR-NV-04..07; FA 4.6; FC 5.4). Sở hữu: Thịnh.
// Tên hàm trùng operationId; đường dẫn tương đối, không lặp /api/v1.
import { apiClient } from "@/shared/api/client";
import { TIEN_TO_KHOA } from "@/shared/api/tienToKhoa";
import type { SinhVienTheInfo } from "@/shared/api/types";

type TuChon = { signal?: AbortSignal };

export const khoaGiaoThietBi = {
  /** lan: số lần quét, để mỗi lần nhấn Enter là một lần tra cứu mới (kể cả quét lại cùng mã). */
  sinhVienTheoThe: (maThe: string, lan: number) =>
    [...TIEN_TO_KHOA.quaySinhVien, "the", maThe, lan] as const,
};

/** Quét thẻ: maThe là MSSV (ĐX-KT-05). SV không hợp lệ vẫn trả 200 kèm hopLe = false và lý do. */
export async function quayLayThongTinSinhVien(
  maThe: string,
  tuyChon?: TuChon,
): Promise<SinhVienTheInfo> {
  const res = await apiClient.get<SinhVienTheInfo>(
    `/quay/sinh-vien/the/${encodeURIComponent(maThe)}`,
    { signal: tuyChon?.signal },
  );
  return res.data;
}
