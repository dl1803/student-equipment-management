"use client";

// Menu — CMP-20 Menu thao tác (DS 8.4; ĐX-DS-14; FC 5.2). Sở hữu: Thịnh (shared core).
// Gom hành động ở dòng bảng khi có từ 3 hành động. Nút kích hoạt "Thao tác" (Button tertiary sm,
// expand_more); mục hủy bỏ (Xóa, Khóa) tông danger, đặt cuối, cách bằng đường kẻ; mục `an` ẩn.
// Bàn phím (mẫu menu button của WAI-ARIA): Enter/Space/↓ mở và đặt tiêu điểm mục đầu, ↑ mở ở mục
// cuối; ↑/↓ di chuyển vòng, Home/End; Enter chọn; Esc đóng và trả tiêu điểm về nút; Tab đóng.
// Danh sách dùng position: fixed theo vị trí nút để không bị khung cuộn ngang của bảng cắt mất;
// tọa độ là hình học tính lúc chạy (không phải giá trị thiết kế), nên đặt qua style. Cuộn hoặc đổi
// kích thước cửa sổ thì đóng menu thay vì bám theo.
import { useEffect, useId, useRef, useState, type KeyboardEvent } from "react";
import { Button } from "./Button";
import { classNames } from "./classNames";
import type { HanhDongDong } from "./DataTable";
import { IconExpandMore } from "./icons/IconExpandMore";

export type MenuProps = {
  /** Mặc định "Thao tác" (DS CMP-20). */
  nhanNutKichHoat?: string;
  dsMuc: ReadonlyArray<HanhDongDong>;
};

type ViTri = { top: number; right: number };

const CHON_MUC = '[role="menuitem"]';

export function Menu({ nhanNutKichHoat = "Thao tác", dsMuc }: MenuProps) {
  const idNut = useId();
  const idDs = useId();
  const nutRef = useRef<HTMLButtonElement>(null);
  const dsRef = useRef<HTMLDivElement>(null);
  const [viTri, setViTri] = useState<ViTri | null>(null);
  const [tieuDiemDau, setTieuDiemDau] = useState<"DAU" | "CUOI">("DAU");
  const mo = viTri !== null;

  const dsHien = dsMuc.filter((m) => m.an !== true);
  const dsThuong = dsHien.filter((m) => m.huyBo !== true);
  const dsHuyBo = dsHien.filter((m) => m.huyBo === true);

  const moMenu = (viTriTieuDiem: "DAU" | "CUOI") => {
    const khung = nutRef.current?.getBoundingClientRect();
    if (khung === undefined) return;
    setTieuDiemDau(viTriTieuDiem);
    setViTri({ top: khung.bottom, right: window.innerWidth - khung.right });
  };

  const dong = (traTieuDiem: boolean) => {
    setViTri(null);
    if (traTieuDiem) nutRef.current?.focus();
  };

  useEffect(() => {
    if (!mo) return;
    const dsPhanTu = dsRef.current?.querySelectorAll<HTMLElement>(CHON_MUC);
    const phanTu =
      tieuDiemDau === "DAU" ? dsPhanTu?.[0] : dsPhanTu?.[dsPhanTu.length - 1];
    phanTu?.focus({ preventScroll: true });

    const bamNgoai = (suKien: PointerEvent) => {
      const dich = suKien.target;
      if (
        dich instanceof Node &&
        (nutRef.current?.contains(dich) || dsRef.current?.contains(dich))
      ) {
        return;
      }
      setViTri(null);
    };
    const dongKhiDoiKhung = () => setViTri(null);
    document.addEventListener("pointerdown", bamNgoai);
    window.addEventListener("resize", dongKhiDoiKhung);
    window.addEventListener("scroll", dongKhiDoiKhung, true);
    return () => {
      document.removeEventListener("pointerdown", bamNgoai);
      window.removeEventListener("resize", dongKhiDoiKhung);
      window.removeEventListener("scroll", dongKhiDoiKhung, true);
    };
  }, [mo, tieuDiemDau]);

  const phimTrenNut = (suKien: KeyboardEvent<HTMLButtonElement>) => {
    if (suKien.key === "ArrowDown" || suKien.key === "ArrowUp") {
      suKien.preventDefault();
      moMenu(suKien.key === "ArrowDown" ? "DAU" : "CUOI");
    }
  };

  const phimTrenDs = (suKien: KeyboardEvent<HTMLDivElement>) => {
    const dsPhanTu = Array.from(
      dsRef.current?.querySelectorAll<HTMLElement>(CHON_MUC) ?? [],
    );
    const n = dsPhanTu.length;
    const hienTai = dsPhanTu.findIndex((pt) => pt === document.activeElement);
    const datTieuDiem = (i: number) => {
      suKien.preventDefault();
      dsPhanTu[i]?.focus({ preventScroll: true });
    };
    switch (suKien.key) {
      case "ArrowDown":
        datTieuDiem((hienTai + 1) % n);
        break;
      case "ArrowUp":
        datTieuDiem((hienTai - 1 + n) % n);
        break;
      case "Home":
        datTieuDiem(0);
        break;
      case "End":
        datTieuDiem(n - 1);
        break;
      case "Escape":
        suKien.preventDefault();
        dong(true);
        break;
      case "Tab":
        dong(false);
        break;
    }
  };

  const chon = (muc: HanhDongDong) => {
    // Trả tiêu điểm về nút trước khi chạy hành động: hộp thoại mở ra sẽ trả tiêu điểm về đây khi đóng.
    dong(true);
    muc.onClick();
  };

  const mucMenu = (muc: HanhDongDong) => (
    <button
      key={muc.nhan}
      type="button"
      role="menuitem"
      tabIndex={-1}
      onClick={() => chon(muc)}
      className={classNames(
        "flex h-control-md w-full items-center whitespace-nowrap px-3 text-left type-label cursor-pointer",
        "hover:bg-surface-subtle focus:bg-surface-subtle",
        muc.huyBo === true ? "text-tone-danger-text" : "text-text-primary",
      )}
    >
      {muc.nhan}
    </button>
  );

  return (
    <>
      <Button
        ref={nutRef}
        id={idNut}
        bienThe="tertiary"
        kichThuoc="sm"
        bieuTuongSau={<IconExpandMore kichThuoc="md" />}
        aria-haspopup="menu"
        aria-expanded={mo}
        aria-controls={mo ? idDs : undefined}
        onClick={() => (mo ? dong(false) : moMenu("DAU"))}
        onKeyDown={phimTrenNut}
      >
        {nhanNutKichHoat}
      </Button>
      {viTri !== null ? (
        <div
          ref={dsRef}
          id={idDs}
          role="menu"
          aria-labelledby={idNut}
          onKeyDown={phimTrenDs}
          style={{ top: viTri.top, right: viTri.right }}
          className="fixed z-[var(--ds-z-dropdown)] mt-1 flex flex-col rounded-md border border-border-subtle bg-surface-default py-1 shadow-menu"
        >
          {dsThuong.map(mucMenu)}
          {dsThuong.length > 0 && dsHuyBo.length > 0 ? (
            <div
              role="separator"
              className="my-1 border-t border-border-subtle"
            />
          ) : null}
          {dsHuyBo.map(mucMenu)}
        </div>
      ) : null}
    </>
  );
}
