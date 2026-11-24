// KeyValueList — CMP-14 Danh sách thông tin (DS 8.4). Sở hữu: Thịnh (shared core).
// Web: hai cột (cột tên rộng size.keyvalue.label), cách dòng space.2. Giá trị thiếu hiển thị "—"
// (ĐX-DS-12) hoặc chữ riêng của màn hình; giá trị dài xuống dòng, không cắt.
import type { ReactNode } from "react";
import { GIA_TRI_THIEU } from "@/shared/format/formatThoiGian";
import { classNames } from "./classNames";

export type DongKeyValue = {
  ten: string;
  giaTri: ReactNode | null | undefined;
  /** Mã phiếu, mã thiết bị, mã SV dùng type.code. */
  laMa?: boolean;
  /** Chữ riêng khi thiếu giá trị, ví dụ "Chưa có ngày dự kiến". */
  giaTriThieu?: string;
};

export type KeyValueListProps = {
  dsDong: ReadonlyArray<DongKeyValue>;
};

function laThieu(giaTri: ReactNode | null | undefined): boolean {
  return giaTri === null || giaTri === undefined || giaTri === "";
}

export function KeyValueList({ dsDong }: KeyValueListProps) {
  return (
    <dl className="grid grid-cols-[var(--ds-size-keyvalue-label)_1fr] gap-x-4 gap-y-2">
      {dsDong.map((dong) => (
        <div key={dong.ten} className="contents">
          <dt className="type-caption text-text-secondary">{dong.ten}</dt>
          <dd
            className={classNames(
              "break-words text-text-primary",
              dong.laMa ? "type-code" : "type-body",
            )}
          >
            {laThieu(dong.giaTri)
              ? (dong.giaTriThieu ?? GIA_TRI_THIEU)
              : dong.giaTri}
          </dd>
        </div>
      ))}
    </dl>
  );
}
