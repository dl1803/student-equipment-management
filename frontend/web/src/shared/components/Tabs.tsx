"use client";

// Tabs — CMP-09 Tab (DS 8.3; dùng ở SCR-NV-08, SCR-NV-14). Sở hữu: Thịnh (shared core).
// Dãy tab ngang, viền dưới color.border.subtle. Tab đang chọn: chữ color.text.primary, chữ đậm và vạch
// dưới border.width.strong màu color.border.selected (dấu hiệu ngoài màu); tab khác: chữ
// color.text.secondary. Cao size.control.lg (ĐX-DS-17: kiểu tab gạch chân).
// Mẫu tab của WAI-ARIA, kích hoạt tự động: ←/→ di chuyển vòng và chọn luôn, Home/End về đầu/cuối;
// chỉ tab đang chọn nhận Tab (roving tabindex). Nội dung tab đang chọn nằm trong một tabpanel gắn
// với tab đó. Đổi tab thì màn hình tải lại dữ liệu của tab (DS CMP-09) qua onDoiTab.
import { useId, useRef, type KeyboardEvent, type ReactNode } from "react";
import { classNames } from "./classNames";

export type MucTab<T extends string = string> = {
  khoa: T;
  nhan: string;
};

export type TabsProps<T extends string = string> = {
  /** Tên truy cập của dãy tab, ví dụ "Trạng thái phiếu mượn". */
  tenNhom: string;
  dsTab: ReadonlyArray<MucTab<T>>;
  tabDangChon: T;
  onDoiTab: (khoa: T) => void;
  /** Nội dung của tab đang chọn. */
  children: ReactNode;
};

export function Tabs<T extends string = string>({
  tenNhom,
  dsTab,
  tabDangChon,
  onDoiTab,
  children,
}: TabsProps<T>) {
  const goc = useId();
  const dsRef = useRef<HTMLDivElement>(null);
  const idTab = (khoa: string) => `${goc}-tab-${khoa}`;
  const idPanel = `${goc}-panel`;

  const chonVaDatTieuDiem = (chiSo: number) => {
    const muc = dsTab[chiSo];
    if (muc === undefined) return;
    onDoiTab(muc.khoa);
    document.getElementById(idTab(muc.khoa))?.focus();
  };

  const phim = (suKien: KeyboardEvent<HTMLDivElement>) => {
    const n = dsTab.length;
    if (n === 0) return;
    const hienTai = dsTab.findIndex((t) => t.khoa === tabDangChon);
    switch (suKien.key) {
      case "ArrowRight":
        suKien.preventDefault();
        chonVaDatTieuDiem((hienTai + 1) % n);
        break;
      case "ArrowLeft":
        suKien.preventDefault();
        chonVaDatTieuDiem(hienTai <= 0 ? n - 1 : hienTai - 1);
        break;
      case "Home":
        suKien.preventDefault();
        chonVaDatTieuDiem(0);
        break;
      case "End":
        suKien.preventDefault();
        chonVaDatTieuDiem(n - 1);
        break;
    }
  };

  return (
    <div className="flex flex-col gap-section">
      <div
        ref={dsRef}
        role="tablist"
        aria-label={tenNhom}
        onKeyDown={phim}
        className="flex gap-6 border-b border-border-subtle"
      >
        {dsTab.map((tab) => {
          const dangChon = tab.khoa === tabDangChon;
          return (
            <button
              key={tab.khoa}
              id={idTab(tab.khoa)}
              type="button"
              role="tab"
              aria-selected={dangChon}
              aria-controls={idPanel}
              tabIndex={dangChon ? 0 : -1}
              onClick={() => onDoiTab(tab.khoa)}
              className={classNames(
                "-mb-px flex h-control-lg cursor-pointer items-center border-b-[length:var(--ds-border-width-strong)] px-1 type-label transition-colors",
                dangChon
                  ? "border-border-selected font-bold text-text-primary"
                  : "border-transparent text-text-secondary hover:text-text-primary",
              )}
            >
              {tab.nhan}
            </button>
          );
        })}
      </div>
      <div
        id={idPanel}
        role="tabpanel"
        aria-labelledby={idTab(tabDangChon)}
        tabIndex={0}
        className="focus-visible:outline-none"
      >
        {children}
      </div>
    </div>
  );
}
