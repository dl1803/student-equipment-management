"use client";

// ConfirmDialog — CMP-26 loại Xác nhận (UI-Screens 3.5, DS 9.2, FC 5.8 [ĐX-FC-18]).
// Sở hữu: Thịnh (shared core). Mọi thao tác thuộc DS 9.2 đi qua hộp này (TC-09).
// - Tiêu điểm ban đầu ở nút Hủy; mọi nút type="button", không có <form>: Enter không xác nhận.
// - Đang xử lý: nút xác nhận vô hiệu, hiện "Đang xử lý"; Hủy và Esc bị khóa.
import { useId, useRef, type ReactNode } from "react";
import { Button } from "./Button";
import { Dialog } from "./Dialog";

export type ConfirmDialogProps = {
  open: boolean;
  /** Tên hành động, ví dụ "Duyệt phiếu yêu cầu". */
  tieuDe: string;
  /** Đối tượng bị tác động. */
  tomTat?: ReactNode;
  /** Điều sẽ xảy ra và không đổi được. */
  heQua: ReactNode;
  /** Nhấn mạnh hệ quả quan trọng, thường là <Banner tong="warning" /> (DS CMP-26). */
  nhanManh?: ReactNode;
  /** Nút xác nhận mang tên hành động (DS 9.2), không dùng "OK" hay "Đồng ý". */
  nhanXacNhan: string;
  bienThe: "primary" | "destructive";
  /** Đang chờ phản hồi: nút xác nhận vô hiệu, hiện "Đang xử lý"; Hủy và Esc bị khóa. */
  dangXuLy: boolean;
  /** Lỗi trả về khi xác nhận, thường là <Banner tong="danger" /> (DS CMP-26, 9.4). */
  loi?: ReactNode;
  onXacNhan: () => void;
  onDong: () => void;
};

export function ConfirmDialog({
  open,
  tieuDe,
  tomTat,
  heQua,
  nhanManh,
  nhanXacNhan,
  bienThe,
  dangXuLy,
  loi,
  onXacNhan,
  onDong,
}: ConfirmDialogProps) {
  const nutHuyRef = useRef<HTMLButtonElement>(null);
  const idHeQua = useId();

  return (
    <Dialog
      open={open}
      tieuDe={tieuDe}
      tieuDiemBanDauRef={nutHuyRef}
      khoaDong={dangXuLy}
      onDong={onDong}
      idMoTa={idHeQua}
      hangNut={
        <>
          <Button ref={nutHuyRef} onClick={onDong} disabled={dangXuLy}>
            Hủy
          </Button>
          <Button bienThe={bienThe} onClick={onXacNhan} dangXuLy={dangXuLy}>
            {nhanXacNhan}
          </Button>
        </>
      }
    >
      {tomTat !== undefined ? <div className="type-body">{tomTat}</div> : null}
      <div id={idHeQua} className="type-body">
        {heQua}
      </div>
      {nhanManh}
      {loi}
    </Dialog>
  );
}
