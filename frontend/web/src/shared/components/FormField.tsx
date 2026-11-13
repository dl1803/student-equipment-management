// FormField — nhãn, ô nhập và lỗi gắn đúng ô (FA 4.2, FC 5.6, DS 9.1). Sở hữu: Thịnh (shared core).
// D01-T: chốt hợp đồng props và ngữ nghĩa tiếp cận; kiểu dáng theo token thuộc task Design-System.
import type { ReactNode } from "react";

/** Thuộc tính FormField truyền xuống điều khiển bên trong để liên kết nhãn và lỗi. */
export type ThuocTinhDieuKhien = {
  id: string;
  required: boolean;
  "aria-invalid": boolean;
  "aria-describedby": string | undefined;
};

export type FormFieldProps = {
  /** id của điều khiển; tên trường trùng thuộc tính JSON của openapi.yaml (FC 5.6). */
  id: string;
  nhan: string;
  batBuoc?: boolean;
  /** Ghi chú nhìn thấy dưới nhãn; không dùng placeholder thay nhãn (DS 10). */
  moTa?: string;
  /** Thông báo lỗi của trường (từ React Hook Form hoặc errors của máy chủ). */
  loi?: string;
  children: (thuocTinh: ThuocTinhDieuKhien) => ReactNode;
};

export function FormField({ id, nhan, batBuoc = false, moTa, loi, children }: FormFieldProps) {
  const idMoTa = moTa ? `${id}-mo-ta` : undefined;
  const idLoi = loi ? `${id}-loi` : undefined;
  const describedBy = [idMoTa, idLoi].filter(Boolean).join(" ") || undefined;

  return (
    <div>
      <label htmlFor={id}>
        {nhan}
        {batBuoc ? <span aria-hidden="true"> *</span> : null}
      </label>
      {moTa ? <p id={idMoTa}>{moTa}</p> : null}
      {children({
        id,
        required: batBuoc,
        "aria-invalid": Boolean(loi),
        "aria-describedby": describedBy,
      })}
      {loi ? (
        <p id={idLoi} role="alert">
          {loi}
        </p>
      ) : null}
    </div>
  );
}
