"use client";

// ConfirmDialog — CMP-26 loại Xác nhận (UI-Screens 3.5, DS 9.2, FC 5.8 [ĐX-FC-18]).
// Sở hữu: Thịnh (shared core). D01-T: chốt hợp đồng props và hành vi <dialog> gốc;
// kiểu dáng token, ErrorBanner trong hộp và kiểm thử component thuộc task sau.
import { useEffect, useId, useRef, type ReactNode } from "react";

export type ConfirmDialogProps = {
  open: boolean;
  /** Tên hành động, ví dụ "Duyệt phiếu yêu cầu". */
  tieuDe: string;
  /** Đối tượng bị tác động. */
  tomTat?: ReactNode;
  /** Điều sẽ xảy ra và không đổi được. */
  heQua: ReactNode;
  /** Nút xác nhận mang tên hành động (DS 9.2), không dùng "OK" hay "Đồng ý". */
  nhanXacNhan: string;
  bienThe: "primary" | "destructive";
  /** Đang chờ phản hồi: nút xác nhận vô hiệu, hiện "Đang xử lý"; Hủy và Esc bị khóa. */
  dangXuLy: boolean;
  /** Lỗi trả về khi xác nhận, hiển thị trong hộp (ErrorBanner ở task sau). */
  loi?: ReactNode;
  onXacNhan: () => void;
  onDong: () => void;
};

function traTieuDiem(noiMoRef: { current: HTMLElement | null }) {
  const noiMo = noiMoRef.current;
  noiMoRef.current = null;
  if (noiMo !== null && noiMo.isConnected) noiMo.focus();
}

export function ConfirmDialog({
  open,
  tieuDe,
  tomTat,
  heQua,
  nhanXacNhan,
  bienThe,
  dangXuLy,
  loi,
  onXacNhan,
  onDong,
}: ConfirmDialogProps) {
  const dialogRef = useRef<HTMLDialogElement>(null);
  const nutHuyRef = useRef<HTMLButtonElement>(null);
  /** Phần tử có tiêu điểm trước khi mở hộp, để trả tiêu điểm khi đóng (DS CMP-26, WCAG 2.4.3). */
  const noiMoRef = useRef<HTMLElement | null>(null);
  const idTieuDe = useId();
  const idHeQua = useId();

  useEffect(() => {
    const dialog = dialogRef.current;
    if (dialog === null) return;
    if (open && !dialog.open) {
      const dangCoTieuDiem = document.activeElement;
      noiMoRef.current =
        dangCoTieuDiem instanceof HTMLElement ? dangCoTieuDiem : null;
      dialog.showModal();
      nutHuyRef.current?.focus();
    } else if (!open && dialog.open) {
      dialog.close();
      traTieuDiem(noiMoRef);
    }
  }, [open]);

  // Hộp bị gỡ khi đang mở (ví dụ rời màn hình) cũng trả tiêu điểm về nơi mở.
  useEffect(() => () => traTieuDiem(noiMoRef), []);

  useEffect(() => {
    const dialog = dialogRef.current;
    if (dialog === null) return;
    const handleCancel = (event: Event) => {
      // Esc: luôn tự xử lý để trạng thái open do component cha quyết định.
      event.preventDefault();
      if (!dangXuLy) onDong();
    };
    dialog.addEventListener("cancel", handleCancel);
    return () => dialog.removeEventListener("cancel", handleCancel);
  }, [dangXuLy, onDong]);

  return (
    <dialog
      ref={dialogRef}
      aria-labelledby={idTieuDe}
      aria-describedby={idHeQua}
      data-bien-the={bienThe}
    >
      <h2 id={idTieuDe}>{tieuDe}</h2>
      {tomTat ? <div>{tomTat}</div> : null}
      <div id={idHeQua}>{heQua}</div>
      {loi ? <div>{loi}</div> : null}
      <div>
        <button
          ref={nutHuyRef}
          type="button"
          onClick={onDong}
          disabled={dangXuLy}
        >
          Hủy
        </button>
        <button type="button" onClick={onXacNhan} disabled={dangXuLy}>
          {dangXuLy ? "Đang xử lý" : nhanXacNhan}
        </button>
      </div>
    </dialog>
  );
}
