"use client";

// ImageViewer — CMP-23 lớp xem ảnh lớn (DS 8; FC Phụ lục A). Sở hữu: Thịnh (shared core).
// <dialog> gốc với showModal(): khóa tiêu điểm trong lớp xem, Esc đi qua sự kiện `cancel`. Phủ
// color.overlay.scrim, ảnh vừa khung nhìn, shadow.dialog, z.viewer; nút CMP-02 "Đóng" ở góc trên
// phải. Đóng bằng nút, chạm ra ngoài ảnh hoặc Esc. Đóng xong trả tiêu điểm về nơi mở.
import { useEffect, useRef, type MouseEvent } from "react";
import { IconButton } from "./IconButton";
import { IconClose } from "./icons/IconClose";

export type ImageViewerProps = {
  /** URL ảnh đang xem; null là đóng. */
  url: string | null;
  /** Tên truy cập: "Ảnh <số thứ tự> của <tên thiết bị>" (DS CMP-23). */
  nhanTruyCap: string;
  onDong: () => void;
};

export function ImageViewer({ url, nhanTruyCap, onDong }: ImageViewerProps) {
  const dialogRef = useRef<HTMLDialogElement>(null);
  const nutDongRef = useRef<HTMLButtonElement>(null);
  const noiMoRef = useRef<HTMLElement | null>(null);
  const mo = url !== null;

  useEffect(() => {
    const dialog = dialogRef.current;
    if (dialog === null) return;
    if (mo && !dialog.open) {
      const dangCoTieuDiem = document.activeElement;
      noiMoRef.current =
        dangCoTieuDiem instanceof HTMLElement ? dangCoTieuDiem : null;
      dialog.showModal();
      nutDongRef.current?.focus();
    } else if (!mo && dialog.open) {
      dialog.close();
      const noiMo = noiMoRef.current;
      noiMoRef.current = null;
      if (noiMo?.isConnected) noiMo.focus();
    }
  }, [mo]);

  useEffect(() => {
    const dialog = dialogRef.current;
    if (dialog === null) return;
    const khiHuy = (suKien: Event) => {
      suKien.preventDefault();
      onDong();
    };
    dialog.addEventListener("cancel", khiHuy);
    return () => dialog.removeEventListener("cancel", khiHuy);
  }, [onDong]);

  // Bấm vào vùng phủ (ngoài ảnh và nút) thì đóng.
  const bamNen = (suKien: MouseEvent<HTMLElement>) => {
    if (suKien.target === suKien.currentTarget) onDong();
  };

  return (
    <dialog
      ref={dialogRef}
      aria-label={nhanTruyCap}
      onClick={bamNen}
      className="z-[var(--ds-z-viewer)] m-0 size-full max-h-none max-w-none bg-transparent p-0 backdrop:bg-overlay-scrim open:block"
    >
      <div
        onClick={bamNen}
        className="flex size-full items-center justify-center p-12"
      >
        {url !== null ? (
          // eslint-disable-next-line @next/next/no-img-element -- ĐX-FA-11: không dùng next/image.
          <img
            src={url}
            alt={nhanTruyCap}
            className="max-h-full max-w-full rounded-md bg-surface-default object-contain shadow-dialog"
          />
        ) : null}
      </div>
      <span className="absolute top-4 right-4 rounded-md bg-surface-default shadow-dialog">
        <IconButton
          ref={nutDongRef}
          nhanTruyCap="Đóng"
          bieuTuong={<IconClose kichThuoc="lg" />}
          onClick={onDong}
        />
      </span>
    </dialog>
  );
}
