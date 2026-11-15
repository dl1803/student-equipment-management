// Gắn lỗi theo trường của máy chủ vào React Hook Form (FA 3.11, 4.8; FC 5.6 [ĐX-FC-03]).
// Sở hữu: Thịnh (shared core).
import type { FieldValues, Path, UseFormReturn } from "react-hook-form";
import type { ApiError } from "@/shared/api/apiError";

/**
 * `dsLoaiVaSoLuong[0].soLuong` (khóa errors của máy chủ) → `dsLoaiVaSoLuong.0.soLuong` (RHF).
 * Bỏ tiền tố JSON path `$.` (ASP.NET Core dùng khi lỗi giải mã thân yêu cầu) và dấu chấm thừa ở đầu.
 */
export function chuyenKhoaLoiTruong(khoa: string): string {
  return khoa
    .replace(/^\$\.?/, "")
    .replace(/\[(\d+)\]/g, ".$1")
    .replace(/^\./, "");
}

/** Đường dẫn có tồn tại trong giá trị hiện tại của biểu mẫu không (theo defaultValues). */
function laDuongDanCua<T extends FieldValues>(
  giaTri: T,
  duongDan: string,
): duongDan is Path<T> {
  let hienTai: unknown = giaTri;
  for (const phan of duongDan.split(".")) {
    if (typeof hienTai !== "object" || hienTai === null || !(phan in hienTai)) {
      return false;
    }
    hienTai = Reflect.get(hienTai, phan);
  }
  return true;
}

/**
 * Gắn từng lỗi của `loi.errors` vào đúng trường và đưa tiêu điểm tới trường lỗi đầu tiên. Giữ
 * nguyên dữ liệu đã nhập (không reset). Trường phải có trong defaultValues của biểu mẫu.
 *
 * @returns thông báo của các khóa không khớp trường nào, để hiển thị ở ErrorBanner đầu biểu mẫu
 */
export function ganLoiMayChu<T extends FieldValues>(
  loi: ApiError,
  form: Pick<UseFormReturn<T>, "setError" | "getValues">,
): string[] {
  const khongKhop: string[] = [];
  const giaTri = form.getValues();
  let daDuaTieuDiem = false;

  for (const [khoa, dsThongBao] of Object.entries(loi.errors ?? {})) {
    const thongBao = dsThongBao.join(" ");
    const duongDan = chuyenKhoaLoiTruong(khoa);
    if (duongDan !== "" && laDuongDanCua(giaTri, duongDan)) {
      form.setError(
        duongDan,
        { type: "server", message: thongBao },
        { shouldFocus: !daDuaTieuDiem },
      );
      daDuaTieuDiem = true;
    } else {
      khongKhop.push(thongBao);
    }
  }
  return khongKhop;
}
