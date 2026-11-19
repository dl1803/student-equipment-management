"use client";

// AnhUploader — CMP-12 Khu đính ảnh (DS 8; FA 4.10; FC 5.9). Sở hữu: Thịnh (shared core).
// Chỉ điều phối và hiển thị; kiểm tra, nén và tải lên tuần tự nằm ở shared/anh (useAnhUploader).
// Lưới ô size.photo.tile cách nhau space.2; ô cuối "Thêm ảnh" (viền nét đứt, add_a_photo, bộ đếm
// n/5) ẩn khi đủ 5 ảnh. Bốn trạng thái ô: Đang tải (ảnh mờ, chỉ báo quay, "Đang tải"), Đã tải và Có
// sẵn (ảnh, "Gỡ ảnh" ở góc), Lỗi (viền lỗi, biểu tượng error, "Thử lại" và "Gỡ ảnh"). Lý do từ chối
// báo ngay dưới khu. Màn hình nhận { dsMaAnh, coTheGui } qua onThayDoi và vô hiệu nút gửi khi
// coTheGui = false (ĐX-FA-09). Ảnh tùy chọn: không có ảnh vẫn gửi được.
import { useEffect, useId, useRef, useState, type ChangeEvent } from "react";
import { SO_ANH_TOI_DA, KIEU_ANH_CHAP_NHAN } from "@/shared/anh/hangSo";
import { THONG_DIEP_TU_CHOI_ANH } from "@/shared/anh/kiemTraAnh";
import { useAnhUploader, type MucAnh } from "@/shared/anh/useAnhUploader";
import type { AnhRef, GiaiDoanAnh } from "@/shared/api/types";
import { Button } from "./Button";
import { ChiBaoQuay } from "./ChiBaoQuay";
import { classNames } from "./classNames";
import { IconButton } from "./IconButton";
import { IconAddAPhoto } from "./icons/IconAddAPhoto";
import { IconClose } from "./icons/IconClose";
import { IconError } from "./icons/IconError";
import { ImageViewer } from "./ImageViewer";

export type GiaTriAnhUploader = {
  /** maAnh của ảnh đã tải và ảnh có sẵn, theo thứ tự hiển thị; đưa vào dsMaAnh của yêu cầu. */
  dsMaAnh: string[];
  /** false khi còn ảnh đang tải hoặc lỗi chưa gỡ: màn hình vô hiệu nút gửi. */
  coTheGui: boolean;
};

export type AnhUploaderProps = {
  /** BAN_DAU ở SCR-NV-06, KHI_TRA ở SCR-NV-09, THIET_BI ở SCR-QT-04 (FA 4.10). */
  giaiDoan: GiaiDoanAnh;
  /** Dùng trong tên truy cập của lớp xem: "Ảnh n của <tên thiết bị>". */
  tenThietBi: string;
  /**
   * Ảnh đã gắn sẵn (SCR-QT-04 khi sửa). CHỈ ĐỌC LÚC KHỞI TẠO: dữ liệu tải bất đồng bộ thì chỉ dựng
   * AnhUploader khi truy vấn chi tiết đã có dữ liệu, kèm `key={maThietBi}` để dựng lại khi đổi
   * thiết bị. Đồng bộ lại sau khởi tạo sẽ ghi đè thao tác thêm/gỡ của người dùng nên không làm.
   */
  dsAnhCoSan?: ReadonlyArray<AnhRef>;
  onThayDoi: (giaTri: GiaTriAnhUploader) => void;
  /** Khu bị vô hiệu kèm chữ giải thích (SCR-NV-09 khi kết quả Mất: "Kết quả Mất không có ảnh"). */
  lyDoVoHieu?: string;
};

const CHU_LOI_TAI_LEN = "Lỗi tải lên";

export function AnhUploader({
  giaiDoan,
  tenThietBi,
  dsAnhCoSan,
  onThayDoi,
  lyDoVoHieu,
}: AnhUploaderProps) {
  const idNhan = useId();
  const idThongBao = useId();
  const oChonRef = useRef<HTMLInputElement>(null);
  const [khoaDangXem, setKhoaDangXem] = useState<string | null>(null);
  const {
    dsAnh,
    soDangNen,
    dsLyDoTuChoi,
    soAnh,
    dsMaAnh,
    coTheGui,
    themTep,
    thuLai,
    go,
  } = useAnhUploader({ giaiDoan, dsAnhCoSan });
  const voHieu = lyDoVoHieu !== undefined;

  const onThayDoiRef = useRef(onThayDoi);
  useEffect(() => {
    onThayDoiRef.current = onThayDoi;
  }, [onThayDoi]);
  const khoaGiaTri = `${coTheGui}|${dsMaAnh.join(",")}`;
  useEffect(() => {
    onThayDoiRef.current({ dsMaAnh: [...dsMaAnh], coTheGui });
    // khoaGiaTri gói đủ dsMaAnh và coTheGui; mảng dsMaAnh đổi tham chiếu ở mỗi lần vẽ.
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [khoaGiaTri]);

  const chonTep = (suKien: ChangeEvent<HTMLInputElement>) => {
    const dsTep = Array.from(suKien.target.files ?? []);
    // Đặt lại để chọn lại đúng tệp vừa gỡ vẫn phát sự kiện change.
    suKien.target.value = "";
    if (dsTep.length > 0) themTep(dsTep);
  };

  const soDangTai =
    dsAnh.filter((m) => m.trangThai === "DANG_TAI").length + soDangNen;
  const soLoi = dsAnh.filter((m) => m.trangThai === "LOI").length;
  const chiSoDangXem = dsAnh.findIndex((m) => m.khoa === khoaDangXem);
  const anhDangXem = chiSoDangXem === -1 ? undefined : dsAnh[chiSoDangXem];

  const oAnh = (muc: MucAnh, i: number) => {
    const ten = `Ảnh ${i + 1}`;
    const nutGo = (
      <span className="flex rounded-md bg-surface-default">
        <IconButton
          kichThuoc="sm"
          nhanTruyCap={`Gỡ ảnh ${i + 1}`}
          bieuTuong={<IconClose kichThuoc="md" />}
          onClick={() => go(muc.khoa)}
          disabled={voHieu}
        />
      </span>
    );

    if (muc.trangThai === "LOI") {
      return (
        <li
          key={muc.khoa}
          role="group"
          aria-label={ten}
          className="flex size-photo-tile flex-col items-center gap-1 rounded-md border border-border-error bg-surface-default p-1 text-center"
        >
          {/* Hàng trên: biểu tượng lỗi và nút Gỡ ảnh, không chồng lên chữ. */}
          <div className="flex w-full items-center justify-between">
            <span className="flex size-control-sm items-center justify-center text-tone-danger-icon">
              <IconError kichThuoc="md" />
            </span>
            {nutGo}
          </div>
          <span className="type-caption text-tone-danger-text">
            {CHU_LOI_TAI_LEN}
          </span>
          <Button
            bienThe="tertiary"
            kichThuoc="sm"
            onClick={() => thuLai(muc.khoa)}
            disabled={voHieu}
          >
            Thử lại
          </Button>
        </li>
      );
    }

    return (
      <li
        key={muc.khoa}
        role="group"
        aria-label={ten}
        aria-busy={muc.trangThai === "DANG_TAI" || undefined}
        className="relative size-photo-tile overflow-hidden rounded-md border border-border-subtle bg-surface-muted"
      >
        <button
          type="button"
          onClick={() => setKhoaDangXem(muc.khoa)}
          disabled={muc.trangThai === "DANG_TAI"}
          className="block size-full cursor-pointer disabled:cursor-wait"
        >
          {/* eslint-disable-next-line @next/next/no-img-element -- ĐX-FA-11: không dùng next/image. */}
          <img
            src={muc.urlXem}
            alt={`Xem ${ten.toLowerCase()}`}
            className="size-full object-cover"
          />
        </button>
        {muc.trangThai === "DANG_TAI" ? (
          <LopDangTai />
        ) : (
          // Ảnh đang tải không có nút Gỡ (DS CMP-12).
          <span className="absolute top-1 right-1">{nutGo}</span>
        )}
      </li>
    );
  };

  return (
    <div
      role="group"
      aria-labelledby={idNhan}
      aria-describedby={idThongBao}
      className="flex flex-col gap-2"
    >
      <span id={idNhan} className="type-label text-text-primary">
        Ảnh (không bắt buộc)
      </span>
      <ul className="flex flex-wrap gap-2">
        {dsAnh.map(oAnh)}
        {Array.from({ length: soDangNen }, (_, i) => (
          <li
            key={`dang-nen-${i}`}
            aria-busy="true"
            className="relative size-photo-tile overflow-hidden rounded-md bg-surface-muted"
          >
            <LopDangTai />
          </li>
        ))}
        {soAnh < SO_ANH_TOI_DA ? (
          <li className="size-photo-tile">
            <button
              type="button"
              disabled={voHieu}
              onClick={() => oChonRef.current?.click()}
              className={classNames(
                "flex size-full cursor-pointer flex-col items-center justify-center gap-1 rounded-md border border-dashed border-border-control bg-surface-default text-text-secondary",
                "hover:bg-surface-subtle disabled:cursor-not-allowed disabled:text-text-disabled disabled:bg-surface-muted",
              )}
            >
              <IconAddAPhoto kichThuoc="lg" />
              <span className="type-label-sm">Thêm ảnh</span>
              <span className="type-caption">
                {soAnh}/{SO_ANH_TOI_DA}
              </span>
            </button>
          </li>
        ) : null}
      </ul>
      <input
        ref={oChonRef}
        type="file"
        accept={KIEU_ANH_CHAP_NHAN.join(",")}
        multiple
        hidden
        tabIndex={-1}
        aria-hidden="true"
        onChange={chonTep}
      />
      <div id={idThongBao} className="flex flex-col gap-1">
        {voHieu ? (
          <p className="type-caption text-text-secondary">{lyDoVoHieu}</p>
        ) : null}
        {dsLyDoTuChoi.length > 0 ? (
          <ul role="alert" className="type-caption text-tone-danger-text">
            {dsLyDoTuChoi.map((lyDo) => (
              <li key={lyDo}>{THONG_DIEP_TU_CHOI_ANH[lyDo]}</li>
            ))}
          </ul>
        ) : null}
        <p role="status" className="sr-only">
          {soDangTai > 0 ? `Đang tải ${soDangTai} ảnh. ` : ""}
          {soLoi > 0 ? `${soLoi} ảnh tải lên lỗi.` : ""}
        </p>
      </div>
      <ImageViewer
        url={anhDangXem?.urlXem ?? null}
        nhanTruyCap={
          chiSoDangXem === -1 ? "" : `Ảnh ${chiSoDangXem + 1} của ${tenThietBi}`
        }
        onDong={() => setKhoaDangXem(null)}
      />
    </div>
  );
}

/** Lớp phủ trạng thái Đang tải: ảnh mờ dưới lớp phủ, chỉ báo quay và chữ "Đang tải". */
function LopDangTai() {
  return (
    <div className="absolute inset-0 flex flex-col items-center justify-center gap-1 bg-overlay-scrim text-text-inverse">
      <ChiBaoQuay kichThuoc="md" />
      <span className="type-caption">Đang tải</span>
    </div>
  );
}
