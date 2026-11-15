// Chuẩn hóa mọi lỗi thành ApiError trước khi ra khỏi shared/api/ (FA 3.3, 4.6, 4.11; FC 5.5).
// Sở hữu: Thịnh (shared core). Tầng trên không đọc phản hồi Axios thô.
import axios, { AxiosHeaders, type AxiosResponse } from "axios";
import { laApiError, type ApiError } from "./apiError";

const KIEU_PROBLEM_JSON = "application/problem+json";

function laDoiTuong(giaTri: unknown): giaTri is Record<string, unknown> {
  return (
    typeof giaTri === "object" && giaTri !== null && !Array.isArray(giaTri)
  );
}

function chuoiHoacBo(giaTri: unknown): string | undefined {
  return typeof giaTri === "string" ? giaTri : undefined;
}

/** errors của ValidationProblemDetails: khóa trường → mảng thông báo; bỏ giá trị sai kiểu. */
function docErrors(giaTri: unknown): Record<string, string[]> | undefined {
  if (!laDoiTuong(giaTri)) return undefined;
  const ketQua: Record<string, string[]> = {};
  for (const [khoa, dsThongBao] of Object.entries(giaTri)) {
    if (!Array.isArray(dsThongBao)) continue;
    const chuoi = dsThongBao.filter((x): x is string => typeof x === "string");
    if (chuoi.length > 0) ketQua[khoa] = chuoi;
  }
  return Object.keys(ketQua).length > 0 ? ketQua : undefined;
}

/** Tên tiêu đề HTTP không phân biệt hoa thường; headers có thể là AxiosHeaders hoặc object thường. */
function layKieuNoiDung(phanHoi: AxiosResponse): string {
  const headers: unknown = phanHoi.headers;
  if (headers instanceof AxiosHeaders) {
    const kieu = headers.get("content-type");
    return typeof kieu === "string" ? kieu.toLowerCase() : "";
  }
  if (!laDoiTuong(headers)) return "";
  for (const [ten, giaTri] of Object.entries(headers)) {
    if (ten.toLowerCase() === "content-type" && typeof giaTri === "string") {
      return giaTri.toLowerCase();
    }
  }
  return "";
}

/**
 * Đọc thân application/problem+json. Thân có thể đã được Axios phân tích, là chuỗi, hoặc là Blob
 * khi yêu cầu dùng responseType "blob" (FA 4.11 quy tắc 2). Không phải problem+json thì trả null
 * (FA 3.3 quy tắc 1).
 */
async function docProblem(
  phanHoi: AxiosResponse,
): Promise<Record<string, unknown> | null> {
  if (!layKieuNoiDung(phanHoi).includes(KIEU_PROBLEM_JSON)) return null;
  let than: unknown = phanHoi.data;
  if (typeof Blob !== "undefined" && than instanceof Blob) {
    than = await than.text();
  }
  if (typeof than === "string") {
    try {
      than = JSON.parse(than);
    } catch {
      return null; // Khai báo problem+json nhưng thân hỏng: xử lý như lỗi hệ thống chung.
    }
  }
  return laDoiTuong(than) ? than : null;
}

/**
 * Chuyển một lỗi bất kỳ thành ApiError. Lỗi không đến từ Axios (lỗi lập trình) được chuyển thành
 * lỗi hệ thống chung để giao diện không vỡ, và được ghi console.error cho người phát triển.
 */
export async function chuanHoaLoi(loi: unknown): Promise<ApiError> {
  if (laApiError(loi)) return loi;
  if (!axios.isAxiosError(loi)) {
    console.error(loi);
    return { loai: "HTTP" };
  }
  if (axios.isCancel(loi) || loi.code === "ERR_CANCELED")
    return { loai: "HUY" };
  if (loi.code === "ETIMEDOUT" || loi.code === "ECONNABORTED") {
    return { loai: "QUA_THOI_GIAN" };
  }
  const phanHoi = loi.response;
  if (phanHoi === undefined) return { loai: "MANG" };

  const ketQua: ApiError = { loai: "HTTP", status: phanHoi.status };
  const problem = await docProblem(phanHoi);
  if (problem === null) return ketQua;

  const maLoi = chuoiHoacBo(problem.maLoi);
  const detail = chuoiHoacBo(problem.detail);
  const traceId = chuoiHoacBo(problem.traceId);
  const errors = docErrors(problem.errors);
  if (maLoi !== undefined) ketQua.maLoi = maLoi;
  if (detail !== undefined) ketQua.detail = detail;
  if (traceId !== undefined) ketQua.traceId = traceId;
  if (errors !== undefined) ketQua.errors = errors;
  return ketQua;
}
