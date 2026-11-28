"use client";

// Ngữ cảnh LuongGiaoThietBi (FA 4.3, 4.7; FC dòng "Ngữ cảnh luồng"). Sở hữu: Thịnh.
// Đặt tại app/nv/giao-thiet-bi/layout.tsx cho bốn bước SCR-NV-04..07; mất khi rời nhóm tuyến
// giao-thiet-bi hoặc tải lại trang (ĐX-FA-19).
// - sinhVien: hồ sơ SV vừa quét ở bước 1 (nguyên phản hồi máy chủ, kể cả hopLe và lý do).
// - nguon: kết quả kiểm tra nguồn giao ở bước 2 (D14-T); dsThietBi của bước 3 thêm ở task bước 3.
// - phieuDaLap: phiếu mượn vừa lập ở bước 3, hiển thị ở bước 4.
// Đặt lại: "Quét thẻ khác" (bước 1), "Giao cho sinh viên khác" (bước 4) và vào bước 1 KHÔNG qua thanh
// tiến trình (mục menu Giao thiết bị, lối vào ở trang chủ). Quay lại bằng thanh tiến trình gọi
// danhDauQuayLai() trước khi chuyển tuyến nên bước 1 giữ hồ sơ đã quét.
import {
  createContext,
  useCallback,
  useContext,
  useMemo,
  useRef,
  useState,
  type ReactNode,
} from "react";
import type {
  KetQuaKiemTraNguon,
  PhieuMuon,
  SinhVienTheInfo,
} from "@/shared/api/types";

type TrangThaiLuong = {
  sinhVien: SinhVienTheInfo | null;
  nguon: KetQuaKiemTraNguon | null;
  phieuDaLap: PhieuMuon | null;
};

const TRONG: TrangThaiLuong = {
  sinhVien: null,
  nguon: null,
  phieuDaLap: null,
};

export type GiaTriLuongGiaoThietBi = TrangThaiLuong & {
  /** Ghi hồ sơ SV vừa quét; bỏ mọi dữ liệu của các bước sau (SV khác thì nguồn cũ không còn đúng). */
  ghiNhanSinhVien: (sinhVien: SinhVienTheInfo) => void;
  /** Bỏ toàn bộ dữ liệu luồng, về trạng thái bắt đầu. */
  datLai: () => void;
  /** Đánh dấu lần chuyển tuyến kế tiếp là quay lại bằng thanh tiến trình. */
  danhDauQuayLai: () => void;
  /** Bỏ dấu quay lại; bước 2 trở đi gọi khi mở. */
  boDauQuayLai: () => void;
  /** Lần vào bước 1 này có phải do quay lại bằng thanh tiến trình. */
  laQuayLai: () => boolean;
};

export const LuongGiaoThietBiContext =
  createContext<GiaTriLuongGiaoThietBi | null>(null);

export function LuongGiaoThietBiProvider({
  children,
}: {
  children: ReactNode;
}) {
  const [trangThai, setTrangThai] = useState<TrangThaiLuong>(TRONG);
  // Ref, không phải state: chỉ đọc khi bước 1 mở, không cần dựng lại giao diện.
  const quayLai = useRef(false);

  const ghiNhanSinhVien = useCallback((sinhVien: SinhVienTheInfo) => {
    setTrangThai({ ...TRONG, sinhVien });
  }, []);
  const datLai = useCallback(() => {
    setTrangThai((cu) =>
      cu.sinhVien === null && cu.nguon === null && cu.phieuDaLap === null
        ? cu
        : TRONG,
    );
  }, []);
  const danhDauQuayLai = useCallback(() => {
    quayLai.current = true;
  }, []);
  const boDauQuayLai = useCallback(() => {
    quayLai.current = false;
  }, []);
  const laQuayLai = useCallback(() => quayLai.current, []);

  const giaTri = useMemo<GiaTriLuongGiaoThietBi>(
    () => ({
      ...trangThai,
      ghiNhanSinhVien,
      datLai,
      danhDauQuayLai,
      boDauQuayLai,
      laQuayLai,
    }),
    [
      trangThai,
      ghiNhanSinhVien,
      datLai,
      danhDauQuayLai,
      boDauQuayLai,
      laQuayLai,
    ],
  );
  return (
    <LuongGiaoThietBiContext.Provider value={giaTri}>
      {children}
    </LuongGiaoThietBiContext.Provider>
  );
}

export function useLuongGiaoThietBi(): GiaTriLuongGiaoThietBi {
  const giaTri = useContext(LuongGiaoThietBiContext);
  if (giaTri === null) {
    throw new Error(
      "useLuongGiaoThietBi phải dùng bên trong LuongGiaoThietBiProvider (app/nv/giao-thiet-bi/layout.tsx)",
    );
  }
  return giaTri;
}
