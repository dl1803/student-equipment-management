// FormField — nhãn, ô nhập và lỗi gắn đúng ô; bao quanh CMP-04/06/07 (FA 4.2, FC 5.6, DS CMP-04, 9.1).
// Sở hữu: Thịnh (shared core).
import type { ReactNode } from "react";
import { IconError } from "./icons/IconError";

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
  /** Chú thích nhìn thấy dưới ô (gợi ý); không dùng placeholder thay nhãn (DS 10). */
  moTa?: string;
  /** Thông báo lỗi của trường (từ React Hook Form hoặc errors của máy chủ). */
  loi?: string;
  children: (thuocTinh: ThuocTinhDieuKhien) => ReactNode;
};

export function FormField({
  id,
  nhan,
  batBuoc = false,
  moTa,
  loi,
  children,
}: FormFieldProps) {
  const idMoTa = moTa ? `${id}-mo-ta` : undefined;
  const idLoi = loi ? `${id}-loi` : undefined;
  const describedBy = [idLoi, idMoTa].filter(Boolean).join(" ") || undefined;

  return (
    <div className="flex flex-col gap-field-label">
      <label htmlFor={id} className="type-label text-text-primary">
        {nhan}
        {batBuoc ? <span aria-hidden="true"> *</span> : null}
      </label>
      {children({
        id,
        required: batBuoc,
        "aria-invalid": Boolean(loi),
        "aria-describedby": describedBy,
      })}
      {loi ? (
        <p
          id={idLoi}
          role="alert"
          className="flex items-start gap-1 type-caption text-tone-danger-text"
        >
          <IconError kichThuoc="sm" />
          <span>{loi}</span>
        </p>
      ) : null}
      {moTa ? (
        <p id={idMoTa} className="type-caption text-text-secondary">
          {moTa}
        </p>
      ) : null}
    </div>
  );
}
