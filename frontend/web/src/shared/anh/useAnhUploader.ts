"use client";

// useAnhUploader — điều phối khu đính ảnh (FA 3.9, 4.10; FC 5.9; ĐX-FA-09). Sở hữu: Thịnh.
// Chọn tệp → lọc (kiemTraAnh) → nén (nenAnh) → tải lên TUẦN TỰ từng ảnh (vòng for…of có await, mọi
// lô và mọi lần thử lại nối vào một chuỗi Promise) → giữ maAnh. Không tự thử lại (ĐX-FA-22).
// Ảnh vượt dung lượng sau nén hoặc không đọc được không vào lưới, chỉ báo dưới khu (DS CMP-12).
// Mỗi URL tạm (createObjectURL) có đúng một revokeObjectURL: khi gỡ ảnh hoặc khi component bị gỡ.
// Phản hồi tải lên chỉ có maAnh: ảnh xem trước luôn là URL tạm của tệp đã nén, không bịa URL máy chủ.
import { useCallback, useEffect, useRef, useState } from "react";
import type { AnhRef, GiaiDoanAnh } from "@/shared/api/types";
import { anhTai } from "./api";
import { locTepDaChon, type LyDoTuChoiAnh } from "./kiemTraAnh";
import { LoiNenAnh, nenAnh } from "./nenAnh";

/** Bốn trạng thái ô ảnh của CMP-12 (FC 5.9). */
export type TrangThaiAnh = "DANG_TAI" | "DA_TAI" | "LOI" | "CO_SAN";

export type MucAnh = {
  khoa: string;
  trangThai: TrangThaiAnh;
  /** URL để hiển thị: URL tạm của tệp đã nén, hoặc URL máy chủ trả cho ảnh có sẵn. */
  urlXem: string;
  /** Có khi DA_TAI hoặc CO_SAN. */
  maAnh: string | null;
};

export type UseAnhUploaderTuyChon = {
  giaiDoan: GiaiDoanAnh;
  /** Ảnh đã gắn sẵn (SCR-QT-04 khi sửa). Chỉ đọc lúc khởi tạo (xem AnhUploaderProps.dsAnhCoSan). */
  dsAnhCoSan?: ReadonlyArray<AnhRef>;
};

export type UseAnhUploaderKetQua = {
  dsAnh: ReadonlyArray<MucAnh>;
  /** Số tệp đã nhận nhưng còn đang nén, chưa có ô trong lưới. */
  soDangNen: number;
  /** Lý do từ chối của lần chọn tệp gần nhất. */
  dsLyDoTuChoi: ReadonlyArray<LyDoTuChoiAnh>;
  /** Tổng số ảnh tính vào giới hạn 5 (ô trong lưới và tệp đang nén). */
  soAnh: number;
  /** maAnh theo thứ tự hiển thị, của ảnh DA_TAI và CO_SAN. */
  dsMaAnh: ReadonlyArray<string>;
  /** false khi còn ảnh đang nén, đang tải hoặc lỗi chưa gỡ (ĐX-FA-09). */
  coTheGui: boolean;
  themTep: (dsTep: ReadonlyArray<File>) => void;
  thuLai: (khoa: string) => void;
  go: (khoa: string) => void;
};

export function useAnhUploader({
  giaiDoan,
  dsAnhCoSan,
}: UseAnhUploaderTuyChon): UseAnhUploaderKetQua {
  const [dsAnh, datDsAnhState] = useState<ReadonlyArray<MucAnh>>(() =>
    (dsAnhCoSan ?? []).map((anh) => ({
      khoa: `co-san-${anh.maAnh}`,
      trangThai: "CO_SAN",
      urlXem: anh.url,
      maAnh: anh.maAnh,
    })),
  );
  const [soDangNen, datSoDangNenState] = useState(0);
  const [dsLyDoTuChoi, setDsLyDoTuChoi] = useState<
    ReadonlyArray<LyDoTuChoiAnh>
  >([]);

  // Bản sao đồng bộ của state để kiểm tra giới hạn ngay trong sự kiện chọn tệp.
  const dsAnhRef = useRef(dsAnh);
  const soDangNenRef = useRef(0);
  const tepNenRef = useRef(new Map<string, Blob>());
  const urlTamRef = useRef(new Set<string>());
  const chuoiRef = useRef<Promise<void>>(Promise.resolve());
  const demKhoaRef = useRef(0);
  const daGoRef = useRef(false);
  const huyRef = useRef(new AbortController());

  const datDsAnh = useCallback(
    (bien: (cu: ReadonlyArray<MucAnh>) => ReadonlyArray<MucAnh>) => {
      dsAnhRef.current = bien(dsAnhRef.current);
      datDsAnhState(dsAnhRef.current);
    },
    [],
  );
  const doiSoDangNen = useCallback((chenh: number) => {
    soDangNenRef.current += chenh;
    datSoDangNenState(soDangNenRef.current);
  }, []);
  const datTrangThai = useCallback(
    (khoa: string, trangThai: TrangThaiAnh, maAnh: string | null = null) => {
      datDsAnh((cu) =>
        cu.map((m) => (m.khoa === khoa ? { ...m, trangThai, maAnh } : m)),
      );
    },
    [datDsAnh],
  );

  useEffect(() => {
    daGoRef.current = false;
    huyRef.current = new AbortController();
    const urlTam = urlTamRef.current;
    return () => {
      daGoRef.current = true;
      huyRef.current.abort();
      for (const url of urlTam) URL.revokeObjectURL(url);
      urlTam.clear();
    };
  }, []);

  const taiMot = useCallback(
    async (khoa: string) => {
      const tep = tepNenRef.current.get(khoa);
      if (tep === undefined || daGoRef.current) return;
      try {
        const { maAnh } = await anhTai(tep, giaiDoan, {
          signal: huyRef.current.signal,
        });
        if (daGoRef.current || !tepNenRef.current.has(khoa)) return;
        datTrangThai(khoa, "DA_TAI", maAnh);
      } catch {
        if (daGoRef.current || !tepNenRef.current.has(khoa)) return;
        datTrangThai(khoa, "LOI");
      }
    },
    [giaiDoan, datTrangThai],
  );

  const xuLyLo = useCallback(
    async (dsTep: ReadonlyArray<File>) => {
      // Số tệp của lô còn tính vào soDangNen. Lô bị ngắt giữa chừng vì lỗi ngoài dự kiến thì trả
      // lại phần còn lại ở finally để coTheGui không bị kẹt ở false.
      let conTrongLo = dsTep.length;
      try {
        for (const tep of dsTep) {
          if (daGoRef.current) return;
          let daTra = false;
          const traCho = () => {
            if (daTra) return;
            daTra = true;
            conTrongLo -= 1;
            doiSoDangNen(-1);
          };
          let daNen: Blob;
          try {
            daNen = await nenAnh(tep);
          } catch (loi) {
            traCho();
            const lyDo: LyDoTuChoiAnh =
              loi instanceof LoiNenAnh && loi.lyDo === "QUA_LON"
                ? "QUA_DUNG_LUONG"
                : "SAI_KIEU";
            setDsLyDoTuChoi((cu) => (cu.includes(lyDo) ? cu : [...cu, lyDo]));
            continue;
          }
          if (daGoRef.current) return;
          demKhoaRef.current += 1;
          const khoa = `anh-${demKhoaRef.current}`;
          const urlXem = URL.createObjectURL(daNen);
          urlTamRef.current.add(urlXem);
          tepNenRef.current.set(khoa, daNen);
          traCho();
          datDsAnh((cu) => [
            ...cu,
            { khoa, trangThai: "DANG_TAI", urlXem, maAnh: null },
          ]);
          await taiMot(khoa);
        }
      } finally {
        if (conTrongLo > 0 && !daGoRef.current) doiSoDangNen(-conTrongLo);
      }
    },
    [datDsAnh, doiSoDangNen, taiMot],
  );

  const noiChuoi = useCallback((viec: () => Promise<void>) => {
    // Promise bị từ chối sẽ bỏ qua mọi .then nối sau nó: nuốt lỗi ở cuối mỗi mắt xích để một lỗi
    // ngoài dự kiến không làm tê liệt cả hàng đợi (lần chọn tệp hoặc "Thử lại" sau vẫn chạy).
    chuoiRef.current = chuoiRef.current.then(viec).catch(() => undefined);
  }, []);

  const themTep = useCallback(
    (dsTep: ReadonlyArray<File>) => {
      const { dsNhan, dsLyDo } = locTepDaChon(
        dsTep,
        dsAnhRef.current.length + soDangNenRef.current,
      );
      setDsLyDoTuChoi(dsLyDo);
      if (dsNhan.length === 0) return;
      doiSoDangNen(dsNhan.length);
      noiChuoi(() => xuLyLo(dsNhan));
    },
    [doiSoDangNen, noiChuoi, xuLyLo],
  );

  const thuLai = useCallback(
    (khoa: string) => {
      const muc = dsAnhRef.current.find((m) => m.khoa === khoa);
      if (muc?.trangThai !== "LOI") return;
      datTrangThai(khoa, "DANG_TAI");
      noiChuoi(() => taiMot(khoa));
    },
    [datTrangThai, noiChuoi, taiMot],
  );

  const go = useCallback(
    (khoa: string) => {
      const muc = dsAnhRef.current.find((m) => m.khoa === khoa);
      // Ảnh đang tải không có nút Gỡ (DS CMP-12); bỏ qua để không mất dấu yêu cầu đang chạy.
      if (muc === undefined || muc.trangThai === "DANG_TAI") return;
      if (urlTamRef.current.delete(muc.urlXem)) URL.revokeObjectURL(muc.urlXem);
      tepNenRef.current.delete(khoa);
      datDsAnh((cu) => cu.filter((m) => m.khoa !== khoa));
      setDsLyDoTuChoi([]);
    },
    [datDsAnh],
  );

  const dsMaAnh = dsAnh.flatMap((m) =>
    m.maAnh !== null && (m.trangThai === "DA_TAI" || m.trangThai === "CO_SAN")
      ? [m.maAnh]
      : [],
  );
  const coTheGui =
    soDangNen === 0 &&
    dsAnh.every((m) => m.trangThai === "DA_TAI" || m.trangThai === "CO_SAN");

  return {
    dsAnh,
    soDangNen,
    dsLyDoTuChoi,
    soAnh: dsAnh.length + soDangNen,
    dsMaAnh,
    coTheGui,
    themTep,
    thuLai,
    go,
  };
}
