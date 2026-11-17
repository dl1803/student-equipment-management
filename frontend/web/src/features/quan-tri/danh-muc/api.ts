// Hàm API danh mục phòng và loại thiết bị (SCR-QT-02, QT-03; FA 4.6; FC 5.4). Sở hữu: Thịnh.
// Tên hàm trùng operationId; đường dẫn tương đối, không lặp /api/v1.
import { apiClient } from "@/shared/api/client";
import { TIEN_TO_KHOA } from "@/shared/api/tienToKhoa";
import type {
  LoaiThietBi,
  LoaiThietBiCapNhatRequest,
  LoaiThietBiPage,
  LoaiThietBiTaoMoiRequest,
  operations,
  PhongThietBi,
  PhongThietBiPage,
  PhongThietBiRequest,
} from "@/shared/api/types";

type TuyChon = { signal?: AbortSignal };

export type DsPhongParams = NonNullable<
  operations["qtDsPhongThietBi"]["parameters"]["query"]
>;
export type DsLoaiParams = NonNullable<
  operations["qtDsLoaiThietBi"]["parameters"]["query"]
>;

export const khoaPhong = {
  ds: (tham: DsPhongParams) => [...TIEN_TO_KHOA.quanTriPhong, tham] as const,
};

export const khoaLoaiThietBi = {
  ds: (tham: DsLoaiParams) =>
    [...TIEN_TO_KHOA.quanTriLoaiThietBi, tham] as const,
};

function maHoa(ma: string): string {
  return encodeURIComponent(ma);
}

export async function qtDsPhongThietBi(
  tham: DsPhongParams,
  tuyChon?: TuyChon,
): Promise<PhongThietBiPage> {
  const res = await apiClient.get<PhongThietBiPage>(
    "/quan-tri/phong-thiet-bi",
    { params: tham, signal: tuyChon?.signal },
  );
  return res.data;
}

export async function qtTaoPhongThietBi(
  yeuCau: PhongThietBiRequest,
): Promise<PhongThietBi> {
  const res = await apiClient.post<PhongThietBi>(
    "/quan-tri/phong-thiet-bi",
    yeuCau,
  );
  return res.data;
}

export async function qtSuaPhongThietBi(
  ma: string,
  yeuCau: PhongThietBiRequest,
): Promise<PhongThietBi> {
  const res = await apiClient.put<PhongThietBi>(
    `/quan-tri/phong-thiet-bi/${maHoa(ma)}`,
    yeuCau,
  );
  return res.data;
}

export async function qtDsLoaiThietBi(
  tham: DsLoaiParams,
  tuyChon?: TuyChon,
): Promise<LoaiThietBiPage> {
  const res = await apiClient.get<LoaiThietBiPage>("/quan-tri/loai-thiet-bi", {
    params: tham,
    signal: tuyChon?.signal,
  });
  return res.data;
}

export async function qtTaoLoaiThietBi(
  yeuCau: LoaiThietBiTaoMoiRequest,
): Promise<LoaiThietBi> {
  const res = await apiClient.post<LoaiThietBi>(
    "/quan-tri/loai-thiet-bi",
    yeuCau,
  );
  return res.data;
}

export async function qtSuaLoaiThietBi(
  ma: string,
  yeuCau: LoaiThietBiCapNhatRequest,
): Promise<LoaiThietBi> {
  const res = await apiClient.put<LoaiThietBi>(
    `/quan-tri/loai-thiet-bi/${maHoa(ma)}`,
    yeuCau,
  );
  return res.data;
}

export async function qtXoaLoaiThietBi(ma: string): Promise<void> {
  await apiClient.delete(`/quan-tri/loai-thiet-bi/${maHoa(ma)}`);
}

export async function qtNgungSuDungLoaiThietBi(
  ma: string,
): Promise<LoaiThietBi> {
  const res = await apiClient.post<LoaiThietBi>(
    `/quan-tri/loai-thiet-bi/${maHoa(ma)}/ngung-su-dung`,
  );
  return res.data;
}
