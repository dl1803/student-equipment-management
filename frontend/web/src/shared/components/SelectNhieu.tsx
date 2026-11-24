"use client";

// SelectNhieu — CMP-06 Ô chọn, biến thể Chọn nhiều (DS 8.3; SCR-QT-04 nhà cung cấp, SCR-QT-09 quyền
// phòng). Sở hữu: Thịnh (shared core). Hiện thực hợp đồng SelectNhieuProps đã công bố ở D04-T.
// - Ô giống CMP-04: hiện tóm tắt số mục đã chọn (tomTat), hoặc gợi ý khi chưa chọn; expand_more bên phải.
// - Danh sách thả xuống (shadow.menu, z.dropdown) KHÔNG đóng sau mỗi lần chọn; mỗi mục có ô tích
//   CMP-07; mục có hai thông tin hiện dòng phụ bằng type.caption; mục đang chọn nền surface.selected.
// - Bàn phím (mẫu listbox chọn nhiều của WAI-ARIA): Enter/Space/↓/↑ trên ô mở danh sách; ↑/↓
//   di chuyển, Home/End; Space hoặc Enter bật/tắt mục; Esc đóng và trả tiêu điểm về ô; Tab đóng.
//   Tiêu điểm nằm trên danh sách, mục đang trỏ báo bằng aria-activedescendant.
// - Bấm ra ngoài thì đóng. Nguồn lựa chọn đang tải/lỗi như Select (DS CMP-06 "Lỗi tải lựa chọn").
// Giá trị trả về theo thứ tự của dsLuaChon (thứ tự máy chủ); giá trị đã chọn nhưng không có trong
// dsLuaChon được giữ nguyên ở đầu, để không âm thầm mất dữ liệu khi nguồn lựa chọn thiếu mục.
import { useEffect, useId, useRef, useState, type KeyboardEvent } from "react";
import { classNames } from "./classNames";
import { IconCheck } from "./icons/IconCheck";
import { IconExpandMore } from "./icons/IconExpandMore";
import { CHU_NGUON_LUA_CHON, LoiNguonLuaChon, type LuaChon } from "./Select";

export type LuaChonNhieu<T extends string = string> = LuaChon<T> & {
  /** Dòng phụ (ví dụ thông tin liên hệ của nhà cung cấp, cơ sở của phòng). */
  moTa?: string;
};

export type SelectNhieuProps<T extends string = string> = {
  id: string;
  dsLuaChon: ReadonlyArray<LuaChonNhieu<T>>;
  giaTri: ReadonlyArray<T>;
  onChange: (giaTri: T[]) => void;
  /** Tóm tắt hiển thị trong ô, ví dụ (n) => `${n} phòng đã chọn`. */
  tomTat: (soDaChon: number) => string;
  /** Chữ khi chưa chọn mục nào (mặc định "Chọn"). */
  goiY?: string;
  disabled?: boolean;
  required?: boolean;
  "aria-invalid"?: boolean;
  "aria-describedby"?: string;
  /** Nguồn lựa chọn đang tải lần đầu. */
  dangTaiLuaChon?: boolean;
  /** Danh sách lựa chọn không tải được: ô ở trạng thái lỗi kèm "Thử lại" (DS CMP-06). */
  loiTaiLuaChon?: { onThuLai: () => void };
};

export function SelectNhieu<T extends string = string>({
  id,
  dsLuaChon,
  giaTri,
  onChange,
  tomTat,
  goiY = "Chọn",
  disabled = false,
  required,
  dangTaiLuaChon = false,
  loiTaiLuaChon,
  ...aria
}: SelectNhieuProps<T>) {
  const idDs = useId();
  const khungRef = useRef<HTMLDivElement>(null);
  const oRef = useRef<HTMLButtonElement>(null);
  const dsRef = useRef<HTMLUListElement>(null);
  const [mo, setMo] = useState(false);
  const [viTri, setViTri] = useState(0);

  const voHieu = disabled || dangTaiLuaChon || loiTaiLuaChon !== undefined;
  const daChon = new Set<string>(giaTri);
  const idMuc = (i: number) => `${idDs}-muc-${i}`;

  const moDs = (viTriDau: "DAU" | "CUOI") => {
    if (voHieu) return;
    const chiSoDaChon = dsLuaChon.findIndex((lc) => daChon.has(lc.giaTri));
    setViTri(
      viTriDau === "CUOI"
        ? Math.max(0, dsLuaChon.length - 1)
        : Math.max(0, chiSoDaChon),
    );
    setMo(true);
  };

  const dong = (traTieuDiem: boolean) => {
    setMo(false);
    if (traTieuDiem) oRef.current?.focus();
  };

  // Mở: chuyển tiêu điểm vào danh sách; bấm ra ngoài khung thì đóng.
  useEffect(() => {
    if (!mo) return;
    dsRef.current?.focus({ preventScroll: true });
    const bamNgoai = (suKien: PointerEvent) => {
      const dich = suKien.target;
      if (dich instanceof Node && khungRef.current?.contains(dich)) return;
      setMo(false);
    };
    document.addEventListener("pointerdown", bamNgoai);
    return () => document.removeEventListener("pointerdown", bamNgoai);
  }, [mo]);

  // Giữ mục đang trỏ trong vùng nhìn thấy của danh sách khi di chuyển bằng phím.
  useEffect(() => {
    if (!mo) return;
    document
      .getElementById(idMuc(viTri))
      ?.scrollIntoView?.({ block: "nearest" });
    // idMuc chỉ phụ thuộc idDs cố định.
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [mo, viTri]);

  const doiMuc = (giaTriMuc: T) => {
    const conLai = new Set(daChon);
    if (conLai.has(giaTriMuc)) conLai.delete(giaTriMuc);
    else conLai.add(giaTriMuc);
    const coTrongDs = new Set<string>(dsLuaChon.map((lc) => lc.giaTri));
    const ngoaiDs = giaTri.filter((g) => !coTrongDs.has(g) && conLai.has(g));
    const trongDs = dsLuaChon
      .filter((lc) => conLai.has(lc.giaTri))
      .map((lc) => lc.giaTri);
    onChange([...ngoaiDs, ...trongDs]);
  };

  const phimTrenO = (suKien: KeyboardEvent<HTMLButtonElement>) => {
    if (suKien.key === "ArrowDown" || suKien.key === "ArrowUp") {
      suKien.preventDefault();
      moDs(suKien.key === "ArrowDown" ? "DAU" : "CUOI");
    }
  };

  const phimTrenDs = (suKien: KeyboardEvent<HTMLUListElement>) => {
    const n = dsLuaChon.length;
    switch (suKien.key) {
      case "ArrowDown":
        suKien.preventDefault();
        if (n > 0) setViTri((v) => (v + 1) % n);
        break;
      case "ArrowUp":
        suKien.preventDefault();
        if (n > 0) setViTri((v) => (v <= 0 ? n - 1 : v - 1));
        break;
      case "Home":
        suKien.preventDefault();
        setViTri(0);
        break;
      case "End":
        suKien.preventDefault();
        setViTri(Math.max(0, n - 1));
        break;
      case " ":
      case "Enter": {
        suKien.preventDefault();
        const muc = dsLuaChon[viTri];
        if (muc !== undefined) doiMuc(muc.giaTri);
        break;
      }
      case "Escape":
        suKien.preventDefault();
        dong(true);
        break;
      case "Tab":
        dong(true);
        break;
    }
  };

  const chu = dangTaiLuaChon
    ? CHU_NGUON_LUA_CHON.DANG_TAI
    : giaTri.length === 0
      ? goiY
      : tomTat(giaTri.length);

  return (
    <div ref={khungRef} className="relative flex flex-col gap-1">
      <button
        ref={oRef}
        id={id}
        type="button"
        // Mẫu ô chọn chỉ-chọn của WAI-ARIA: combobox hỗ trợ aria-invalid, aria-required.
        role="combobox"
        disabled={voHieu}
        aria-haspopup="listbox"
        aria-expanded={mo}
        aria-controls={mo ? idDs : undefined}
        aria-required={required || undefined}
        aria-busy={dangTaiLuaChon || undefined}
        aria-invalid={aria["aria-invalid"] || loiTaiLuaChon !== undefined}
        aria-describedby={aria["aria-describedby"]}
        onClick={() => (mo ? dong(false) : moDs("DAU"))}
        onKeyDown={phimTrenO}
        className={classNames(
          "flex h-control-md w-full items-center justify-between gap-2 rounded-md border border-border-control bg-surface-default px-3 text-left type-body transition-colors cursor-pointer",
          "hover:border-border-control-hover aria-invalid:border-border-error",
          "disabled:cursor-not-allowed disabled:bg-action-disabled-bg disabled:text-action-disabled-text disabled:border-action-disabled-border",
          giaTri.length === 0 ? "text-text-placeholder" : "text-text-primary",
        )}
      >
        <span className="truncate">{chu}</span>
        <IconExpandMore kichThuoc="md" className="text-text-secondary" />
      </button>
      {mo ? (
        <ul
          ref={dsRef}
          id={idDs}
          role="listbox"
          aria-multiselectable="true"
          aria-labelledby={id}
          aria-activedescendant={
            dsLuaChon.length > 0 ? idMuc(viTri) : undefined
          }
          tabIndex={-1}
          onKeyDown={phimTrenDs}
          // Không có token chiều cao riêng cho danh sách thả xuống: dùng size.dialog.sm làm giới hạn
          // để danh sách dài cuộn bên trong.
          className="absolute top-full right-0 left-0 z-[var(--ds-z-dropdown)] mt-1 max-h-dialog-sm overflow-y-auto rounded-md border border-border-subtle bg-surface-default py-1 shadow-menu focus-visible:outline-none"
        >
          {dsLuaChon.length === 0 ? (
            <li
              role="presentation"
              className="px-3 py-2 type-body text-text-secondary"
            >
              Không có lựa chọn
            </li>
          ) : null}
          {dsLuaChon.map((lc, i) => {
            const chon = daChon.has(lc.giaTri);
            return (
              <li
                key={lc.giaTri}
                id={idMuc(i)}
                role="option"
                aria-selected={chon}
                // Tên mục là dòng chính; dòng phụ là mô tả, không dính vào tên.
                aria-labelledby={`${idMuc(i)}-nhan`}
                aria-describedby={lc.moTa ? `${idMuc(i)}-mo-ta` : undefined}
                // Rê chuột cũng dời mục đang trỏ, để Space/Enter tác động đúng mục dưới chuột.
                onMouseEnter={() => setViTri(i)}
                onClick={() => {
                  setViTri(i);
                  doiMuc(lc.giaTri);
                }}
                className={classNames(
                  "flex min-h-control-md cursor-pointer items-start gap-2 px-3 py-2",
                  chon ? "bg-surface-selected" : "hover:bg-surface-subtle",
                  i === viTri &&
                    "outline-[length:var(--ds-border-width-strong)] -outline-offset-2 outline-focus-ring",
                )}
              >
                <span
                  aria-hidden="true"
                  className={classNames(
                    "flex size-icon-md shrink-0 items-center justify-center rounded-sm border",
                    chon
                      ? "border-action-primary-bg bg-action-primary-bg text-text-inverse"
                      : "border-border-control bg-surface-default",
                  )}
                >
                  {chon ? <IconCheck kichThuoc="sm" /> : null}
                </span>
                <span className="flex min-w-0 flex-col">
                  <span
                    id={`${idMuc(i)}-nhan`}
                    className="type-body text-text-primary"
                  >
                    {lc.nhan}
                  </span>
                  {lc.moTa !== undefined && lc.moTa !== "" ? (
                    <span
                      id={`${idMuc(i)}-mo-ta`}
                      className="type-caption text-text-secondary"
                    >
                      {lc.moTa}
                    </span>
                  ) : null}
                </span>
              </li>
            );
          })}
        </ul>
      ) : null}
      {loiTaiLuaChon !== undefined ? (
        <LoiNguonLuaChon onThuLai={loiTaiLuaChon.onThuLai} />
      ) : null}
    </div>
  );
}
