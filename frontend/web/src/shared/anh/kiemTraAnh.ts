// Kiểm tra tệp ảnh vừa chọn trước khi nén (FA 4.10 bước 2; FC 5.9; DS CMP-12 "Kiểm tra tại chỗ").
// Sở hữu: Thịnh (shared core). Chỉ kiểm tra định dạng và số lượng đã khai báo trong tài liệu; dung
// lượng chỉ biết sau khi nén nên kiểm ở nenAnh. Hàm thuần, không đụng DOM.
import {
  DUOI_ANH_CHAP_NHAN,
  KIEU_ANH_CHAP_NHAN,
  SO_ANH_TOI_DA,
} from "./hangSo";

/** Lý do một tệp không được thêm vào khu đính ảnh. */
export type LyDoTuChoiAnh = "SAI_KIEU" | "QUA_SO_LUONG" | "QUA_DUNG_LUONG";

/** Chữ báo dưới khu đính ảnh, nguyên văn DS CMP-12. */
export const THONG_DIEP_TU_CHOI_ANH: Readonly<Record<LyDoTuChoiAnh, string>> = {
  SAI_KIEU: "Chỉ nhận ảnh jpeg, png hoặc webp",
  QUA_DUNG_LUONG: "Mỗi ảnh tối đa 2 MB",
  QUA_SO_LUONG: "Tối đa 5 ảnh",
};

/**
 * Kiểu tệp có được nhận không. Kiểu so không phân biệt hoa thường; kiểu rỗng (trình duyệt không
 * nhận ra) thì xét đuôi tệp.
 */
export function laTepAnhChapNhan(tep: { type: string; name: string }): boolean {
  const kieu = tep.type.trim().toLowerCase();
  if (kieu !== "") return KIEU_ANH_CHAP_NHAN.includes(kieu);
  const ten = tep.name.toLowerCase();
  return DUOI_ANH_CHAP_NHAN.some((duoi) => ten.endsWith(duoi));
}

export type KetQuaChonTep<T> = {
  /** Tệp hợp lệ theo đúng thứ tự đã chọn, đã cắt bớt để tổng số không vượt SO_ANH_TOI_DA. */
  dsNhan: T[];
  /** Các lý do từ chối (không trùng), theo thứ tự xuất hiện. */
  dsLyDo: LyDoTuChoiAnh[];
};

/**
 * Lọc các tệp vừa chọn: bỏ tệp sai định dạng, rồi chỉ nhận tới khi tổng số ảnh của thiết bị đạt
 * SO_ANH_TOI_DA; phần dư bị từ chối với lý do "Tối đa 5 ảnh".
 *
 * @param soAnhDaCo số ảnh đã có hoặc đang xử lý trong khu đính ảnh
 */
export function locTepDaChon<T extends { type: string; name: string }>(
  dsTep: ReadonlyArray<T>,
  soAnhDaCo: number,
): KetQuaChonTep<T> {
  const dsLyDo: LyDoTuChoiAnh[] = [];
  const themLyDo = (lyDo: LyDoTuChoiAnh) => {
    if (!dsLyDo.includes(lyDo)) dsLyDo.push(lyDo);
  };
  const conCho = Math.max(0, SO_ANH_TOI_DA - soAnhDaCo);
  const dsNhan: T[] = [];
  for (const tep of dsTep) {
    if (!laTepAnhChapNhan(tep)) {
      themLyDo("SAI_KIEU");
    } else if (dsNhan.length >= conCho) {
      themLyDo("QUA_SO_LUONG");
    } else {
      dsNhan.push(tep);
    }
  }
  return { dsNhan, dsLyDo };
}
