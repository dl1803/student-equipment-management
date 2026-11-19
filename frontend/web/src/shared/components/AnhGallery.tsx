"use client";

// AnhGallery — CMP-23 dải ảnh đã lưu (DS 8; FA 3.9, 4.10; FC 5.9). Sở hữu: Thịnh (shared core).
// Ô ảnh vuông size.photo.tile, radius.md, ảnh cắt vừa khung, cách nhau space.2; chọn ảnh mở
// ImageViewer. Thẻ <img> thường với loading="lazy" (ĐX-FA-11).
// Ảnh tình trạng có URL ký hết hạn sau 10 phút: khi một ảnh lỗi, gọi onTaiLai (refetch truy vấn chi
// tiết) ĐÚNG MỘT LẦN cho cả dải bằng cờ useRef; ảnh vẫn lỗi thì hiện "Không tải được ảnh" kèm nút
// "Tải lại" và dừng. Bấm "Tải lại" là làm mới thủ công: thử hiển thị lại và tải lại chi tiết một lần
// nữa. Không lưu URL vào storage, tuyến hay khóa truy vấn: URL chỉ nằm trong props.
import { useRef, useState } from "react";
import type { AnhRef } from "@/shared/api/types";
import { Button } from "./Button";
import { IconBrokenImage } from "./icons/IconBrokenImage";
import { IconImage } from "./icons/IconImage";
import { ImageViewer } from "./ImageViewer";

export type AnhGalleryProps = {
  dsAnh: ReadonlyArray<AnhRef>;
  /** Dùng trong tên truy cập của lớp xem: "Ảnh n của <tên thiết bị>". */
  tenThietBi: string;
  /**
   * Ảnh tình trạng (URL ký): hàm tải lại chi tiết để lấy URL mới, thường là `truyVan.refetch`.
   * Bỏ trống với ảnh danh mục (URL công khai): ảnh lỗi chỉ hiện khung lỗi.
   */
  onTaiLai?: () => unknown;
};

/** Ảnh lỗi gắn với đúng URL đã lỗi; URL mới sau khi tải lại chi tiết được hiển thị lại. */
function khoaLoi(anh: AnhRef): string {
  return `${anh.maAnh}|${anh.url}`;
}

export function AnhGallery({ dsAnh, tenThietBi, onTaiLai }: AnhGalleryProps) {
  const [dsLoi, setDsLoi] = useState<ReadonlySet<string>>(new Set());
  const [chiSoXem, setChiSoXem] = useState<number | null>(null);
  const daTaiLaiRef = useRef(false);

  if (dsAnh.length === 0) {
    return (
      <div
        role="img"
        aria-label="Không có ảnh"
        className="flex size-photo-tile items-center justify-center rounded-md bg-surface-muted text-text-secondary"
      >
        <IconImage kichThuoc="lg" />
      </div>
    );
  }

  const khiLoi = (anh: AnhRef) => {
    setDsLoi((cu) => new Set(cu).add(khoaLoi(anh)));
    if (onTaiLai !== undefined && !daTaiLaiRef.current) {
      daTaiLaiRef.current = true;
      onTaiLai();
    }
  };

  const taiLaiThuCong = () => {
    daTaiLaiRef.current = true;
    setDsLoi(new Set());
    onTaiLai?.();
  };

  const anhDangXem = chiSoXem === null ? undefined : dsAnh[chiSoXem];

  return (
    <>
      <ul className="flex flex-wrap gap-2">
        {dsAnh.map((anh, i) => {
          const ten = `Ảnh ${i + 1}`;
          const loi = dsLoi.has(khoaLoi(anh));
          return (
            <li key={anh.maAnh} className="size-photo-tile">
              {loi ? (
                <div
                  role="group"
                  aria-label={ten}
                  className="flex size-full flex-col items-center justify-center gap-1 rounded-md border border-border-subtle bg-surface-muted p-1 text-center text-text-secondary"
                >
                  <IconBrokenImage kichThuoc="lg" />
                  <span className="type-caption">Không tải được ảnh</span>
                  {onTaiLai !== undefined ? (
                    <Button
                      bienThe="tertiary"
                      kichThuoc="sm"
                      onClick={taiLaiThuCong}
                    >
                      Tải lại
                    </Button>
                  ) : null}
                </div>
              ) : (
                <button
                  type="button"
                  onClick={() => setChiSoXem(i)}
                  className="block size-full cursor-pointer overflow-hidden rounded-md bg-surface-muted"
                >
                  {/* eslint-disable-next-line @next/next/no-img-element -- ĐX-FA-11: không dùng next/image. */}
                  <img
                    src={anh.url}
                    alt={ten}
                    loading="lazy"
                    onError={() => khiLoi(anh)}
                    className="size-full object-cover"
                  />
                </button>
              )}
            </li>
          );
        })}
      </ul>
      <ImageViewer
        url={
          anhDangXem !== undefined && !dsLoi.has(khoaLoi(anhDangXem))
            ? anhDangXem.url
            : null
        }
        nhanTruyCap={
          chiSoXem === null ? "" : `Ảnh ${chiSoXem + 1} của ${tenThietBi}`
        }
        onDong={() => setChiSoXem(null)}
      />
    </>
  );
}
