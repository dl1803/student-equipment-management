"use client";

// Dialog — CMP-26 Hộp thoại (DS 8.6, ĐX-DS-09; FC 5.8 [ĐX-FC-18]). Sở hữu: Thịnh (shared core).
// Dùng <dialog> gốc: showModal() có sẵn khóa tiêu điểm trong hộp và phím Esc.
// - Không đóng khi bấm lớp che; Esc đi qua sự kiện `cancel` → onDong.
// - khoaDong = true (đang chờ phản hồi): Esc bị chặn để kết quả thao tác ghi không bị giấu (FA-08).
// - Tiêu điểm ban đầu do loại hộp quyết định (truyền ref); đóng xong trả tiêu điểm về nơi mở.
// Không bọc nội dung trong <form> ở hộp Xác nhận để Enter không tự xác nhận.
// kieu = "NGAN" (Wireframes P-04, ĐX-WF-05): ngăn biểu mẫu trượt từ bên phải, cao hết cửa sổ, rộng
// tối đa layout.web.form-max, giữ danh sách phía sau; đầu ngăn có CMP-02 "Đóng"; thân cuộn dọc,
// chân (hàng nút) cố định. Hành vi khác giữ nguyên như hộp ở giữa.
import {
  useEffect,
  useId,
  useRef,
  type ReactNode,
  type RefObject,
} from "react";
import { classNames } from "./classNames";
import { IconButton } from "./IconButton";
import { IconClose } from "./icons/IconClose";

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
  /** HOP: hộp ở giữa (mặc định); NGAN: ngăn biểu mẫu trượt từ bên phải (chỉ biểu mẫu thiết bị). */
  kieu?: "HOP" | "NGAN";
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
  kieu = "HOP",
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

  const laNgan = kieu === "NGAN";

  return (
    <dialog
      ref={dialogRef}
      aria-labelledby={idTieuDe}
      aria-describedby={idMoTa}
      data-kieu={kieu}
      className={classNames(
        "flex-col bg-surface-default text-text-primary shadow-dialog open:flex",
        "backdrop:bg-overlay-scrim",
        laNgan
          ? "my-0 mr-0 ml-auto h-full max-h-none w-full max-w-web-form-max"
          : classNames(
              "m-auto max-h-[90vh] w-full gap-4 rounded-lg p-dialog",
              kichThuoc === "sm" ? "max-w-dialog-sm" : "max-w-dialog-md",
            ),
      )}
    >
      {laNgan ? (
        <div className="flex items-center justify-between gap-4 border-b border-border-subtle px-dialog py-4">
          <h2 id={idTieuDe} className="type-title-section">
            {tieuDe}
          </h2>
          <IconButton
            nhanTruyCap="Đóng"
            bieuTuong={<IconClose kichThuoc="lg" />}
            onClick={onDong}
            disabled={khoaDong}
          />
        </div>
      ) : (
        <h2 id={idTieuDe} className="type-title-section">
          {tieuDe}
        </h2>
      )}
      <div
        className={classNames(
          "flex min-h-0 flex-1 flex-col gap-3 overflow-y-auto",
          laNgan && "px-dialog py-4",
        )}
      >
        {children}
      </div>
      <div
        className={classNames(
          "flex justify-end gap-actions",
          laNgan && "border-t border-border-subtle px-dialog py-4",
        )}
      >
        {hangNut}
      </div>
    </dialog>
  );
}
