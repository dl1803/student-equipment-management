"use client";

// Dialog — CMP-26 Hộp thoại (DS 8.6, ĐX-DS-09; FC 5.8 [ĐX-FC-18]). Sở hữu: Thịnh (shared core).
// Dùng <dialog> gốc: showModal() có sẵn khóa tiêu điểm trong hộp và phím Esc.
// - Không đóng khi bấm lớp che; Esc đi qua sự kiện `cancel` → onDong.
// - khoaDong = true (đang chờ phản hồi): Esc bị chặn để kết quả thao tác ghi không bị giấu (FA-08).
// - Tiêu điểm ban đầu do loại hộp quyết định (truyền ref); đóng xong trả tiêu điểm về nơi mở.
// Không bọc nội dung trong <form> ở hộp Xác nhận để Enter không tự xác nhận.
import {
  useEffect,
  useId,
  useRef,
  type ReactNode,
  type RefObject,
} from "react";
import { classNames } from "./classNames";

export type DialogProps = {
  open: boolean;
  tieuDe: string;
  /** Nội dung giữa tiêu đề và hàng nút; tự cuộn khi dài. */
  children: ReactNode;
  /** Hàng nút ở cuối, căn phải; nút chính ở ngoài cùng bên phải (DS CMP-26). */
  hangNut: ReactNode;
  /** sm 480px cho xác nhận đơn giản; md 640px cho nhiều dòng hoặc biểu mẫu. */
  kichThuoc?: "sm" | "md";
  /** Phần tử nhận tiêu điểm khi mở (nút Hủy/Đóng, hoặc trường đầu tiên của hộp Biểu mẫu). */
  tieuDiemBanDauRef?: RefObject<HTMLElement | null>;
  /** Đang chờ phản hồi: không cho đóng bằng Esc. */
  khoaDong?: boolean;
  /** Người dùng yêu cầu đóng (Esc). Component cha quyết định đổi open. */
  onDong: () => void;
  /** id của phần tử mô tả chính (ví dụ đoạn hệ quả), gắn vào aria-describedby. */
  idMoTa?: string;
};

function traTieuDiem(noiMoRef: { current: HTMLElement | null }) {
  const noiMo = noiMoRef.current;
  noiMoRef.current = null;
  if (noiMo !== null && noiMo.isConnected) noiMo.focus();
}

export function Dialog({
  open,
  tieuDe,
  children,
  hangNut,
  kichThuoc = "sm",
  tieuDiemBanDauRef,
  khoaDong = false,
  onDong,
  idMoTa,
}: DialogProps) {
  const dialogRef = useRef<HTMLDialogElement>(null);
  const noiMoRef = useRef<HTMLElement | null>(null);
  const idTieuDe = useId();

  useEffect(() => {
    const dialog = dialogRef.current;
    if (dialog === null) return;
    if (open && !dialog.open) {
      const dangCoTieuDiem = document.activeElement;
      noiMoRef.current =
        dangCoTieuDiem instanceof HTMLElement ? dangCoTieuDiem : null;
      dialog.showModal();
      tieuDiemBanDauRef?.current?.focus();
    } else if (!open && dialog.open) {
      dialog.close();
      traTieuDiem(noiMoRef);
    }
  }, [open, tieuDiemBanDauRef]);

  // Hộp bị gỡ khi đang mở (ví dụ rời màn hình) cũng trả tiêu điểm về nơi mở.
  useEffect(() => () => traTieuDiem(noiMoRef), []);

  useEffect(() => {
    const dialog = dialogRef.current;
    if (dialog === null) return;
    const handleCancel = (event: Event) => {
      // Esc: luôn tự xử lý để trạng thái open do component cha quyết định.
      event.preventDefault();
      if (!khoaDong) onDong();
    };
    dialog.addEventListener("cancel", handleCancel);
    return () => dialog.removeEventListener("cancel", handleCancel);
  }, [khoaDong, onDong]);

  return (
    <dialog
      ref={dialogRef}
      aria-labelledby={idTieuDe}
      aria-describedby={idMoTa}
      className={classNames(
        "m-auto max-h-[90vh] w-full flex-col gap-4 rounded-lg bg-surface-default p-dialog text-text-primary shadow-dialog open:flex",
        "backdrop:bg-overlay-scrim",
        kichThuoc === "sm" ? "max-w-dialog-sm" : "max-w-dialog-md",
      )}
    >
      <h2 id={idTieuDe} className="type-title-section">
        {tieuDe}
      </h2>
      <div className="flex min-h-0 flex-1 flex-col gap-3 overflow-y-auto">
        {children}
      </div>
      <div className="flex justify-end gap-actions">{hangNut}</div>
    </dialog>
  );
}
