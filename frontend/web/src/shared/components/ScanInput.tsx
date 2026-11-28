"use client";

// ScanInput — CMP-11 Ô nhập mã quầy (DS 8.3; FA 4.9; UI-Screens 3.8). Sở hữu: Thịnh (shared core).
// Một dòng, cao size.control.lg, chữ type.code, biểu tượng qr_code_scanner ở đầu; nhãn nhìn thấy và
// chú thích "Quét hoặc nhập mã, rồi nhấn Enter". Máy quét gõ như bàn phím nên nhập tay và quét như nhau.
// - Enter xác nhận: cắt khoảng trắng hai đầu; chuỗi rỗng không gửi. Không dùng camera.
// - Trạng thái do màn hình truyền vào:
//   CHO_NHAP     mặc định.
//   DANG_TRA_CUU ô vô hiệu (không nhận mã mới), chỉ báo quay ở cuối ô.
//   LOI          viền lỗi, thông báo dưới ô (nếu có), giữ mã và chọn sẵn toàn bộ để gõ đè.
//   Từ DANG_TRA_CUU về CHO_NHAP là "đã xử lý": ô xóa mã (trừ khi giuMaSauXuLy) và nhận tiêu điểm lại.
// - Có tiêu điểm khi gắn và sau mỗi lần xử lý; màn hình gọi xoaVaTapTrung() khi cần làm trống ô
//   (ví dụ "Quét thẻ khác").
import {
  forwardRef,
  useEffect,
  useImperativeHandle,
  useRef,
  useState,
  type KeyboardEvent,
} from "react";
import { ChiBaoQuay } from "./ChiBaoQuay";
import { classNames } from "./classNames";
import { IconError } from "./icons/IconError";
import { IconQrCodeScanner } from "./icons/IconQrCodeScanner";

export type TrangThaiScanInput = "CHO_NHAP" | "DANG_TRA_CUU" | "LOI";

export type ScanInputHandle = {
  /** Xóa mã trong ô và đưa tiêu điểm về ô. */
  xoaVaTapTrung: () => void;
};

export type ScanInputProps = {
  id: string;
  /** Nêu loại mã: "Mã thẻ sinh viên", "Mã thiết bị" (DS 11.1). */
  nhan: string;
  trangThai: TrangThaiScanInput;
  /** Thông báo lỗi dưới ô khi trangThai = LOI. */
  loi?: string;
  /** Mã đã cắt khoảng trắng, khác rỗng. */
  onXacNhan: (ma: string) => void;
  /** Giữ mã sau khi xử lý xong thay vì xóa (DS CMP-11 "trừ khi màn hình yêu cầu giữ mã"). */
  giuMaSauXuLy?: boolean;
};

export const CHU_THICH_SCAN_INPUT = "Quét hoặc nhập mã, rồi nhấn Enter";

export const ScanInput = forwardRef<ScanInputHandle, ScanInputProps>(
  function ScanInput(
    { id, nhan, trangThai, loi, onXacNhan, giuMaSauXuLy = false },
    ref,
  ) {
    const oRef = useRef<HTMLInputElement>(null);
    const [ma, setMa] = useState("");
    const truocDo = useRef<TrangThaiScanInput>(trangThai);

    useImperativeHandle(ref, () => ({
      xoaVaTapTrung: () => {
        setMa("");
        oRef.current?.focus();
      },
    }));

    useEffect(() => {
      oRef.current?.focus();
    }, []);

    useEffect(() => {
      const cu = truocDo.current;
      truocDo.current = trangThai;
      const o = oRef.current;
      if (o === null || cu === trangThai) return;
      if (trangThai === "LOI") {
        o.focus();
        o.select();
      } else if (trangThai === "CHO_NHAP" && cu === "DANG_TRA_CUU") {
        // "Đã xử lý": làm trống ô theo trạng thái màn hình truyền vào.
        if (!giuMaSauXuLy) setMa("");
        o.focus();
      }
    }, [trangThai, giuMaSauXuLy]);

    const dangTraCuu = trangThai === "DANG_TRA_CUU";
    const coLoi = trangThai === "LOI" && loi !== undefined && loi !== "";
    const idChuThich = `${id}-mo-ta`;
    const idLoi = `${id}-loi`;

    const khiNhanPhim = (e: KeyboardEvent<HTMLInputElement>) => {
      if (e.key !== "Enter") return;
      e.preventDefault();
      const daCat = ma.trim();
      if (daCat === "" || dangTraCuu) return;
      onXacNhan(daCat);
    };

    return (
      <div className="flex flex-col gap-field-label">
        <label htmlFor={id} className="type-label text-text-primary">
          {nhan}
        </label>
        <div className="relative">
          <IconQrCodeScanner
            kichThuoc="lg"
            className="pointer-events-none absolute top-1/2 left-3 -translate-y-1/2 text-text-secondary"
          />
          <input
            ref={oRef}
            id={id}
            type="text"
            autoComplete="off"
            spellCheck={false}
            value={ma}
            onChange={(e) => setMa(e.target.value)}
            onKeyDown={khiNhanPhim}
            disabled={dangTraCuu}
            aria-busy={dangTraCuu || undefined}
            aria-invalid={coLoi}
            aria-describedby={coLoi ? `${idLoi} ${idChuThich}` : idChuThich}
            className={classNames(
              "h-control-lg w-full rounded-md border border-border-control bg-surface-default pr-12 pl-12 type-code text-text-primary transition-colors",
              "hover:border-border-control-hover",
              "focus-visible:outline-none focus:border-focus-ring focus:border-[length:var(--ds-border-width-strong)]",
              "aria-invalid:border-border-error",
              "disabled:cursor-not-allowed disabled:bg-action-disabled-bg disabled:text-action-disabled-text disabled:border-action-disabled-border",
            )}
          />
          {dangTraCuu ? (
            <ChiBaoQuay
              kichThuoc="md"
              className="absolute top-1/2 right-4 -translate-y-1/2"
            />
          ) : null}
        </div>
        {coLoi ? (
          <p
            id={idLoi}
            role="alert"
            className="flex items-start gap-1 type-caption text-tone-danger-text"
          >
            <IconError kichThuoc="sm" />
            <span>{loi}</span>
          </p>
        ) : null}
        <p id={idChuThich} className="type-caption text-text-secondary">
          {CHU_THICH_SCAN_INPUT}
        </p>
      </div>
    );
  },
);
