"use client";

// SCR-QT-03 Loại thiết bị (UI-Screens 7.3; UF-QT-01; BR-18; Wireframes 7). Sở hữu: Thịnh.
// Xem, thêm, sửa, xóa hoặc ngưng sử dụng loại. Bộ lọc Ngưng sử dụng (Tất cả / Đang sử dụng /
// Đã ngưng sử dụng); đổi bộ lọc tải lại từ trang 1 (FA 3.10).
import { useState } from "react";
import {
  KICH_THUOC_TRANG_MAC_DINH,
  trangCanLuiVe,
} from "@/shared/api/phanTrang";
import type { LoaiThietBi } from "@/shared/api/types";
import { Banner } from "@/shared/components/Banner";
import { Button } from "@/shared/components/Button";
import { DataTable } from "@/shared/components/DataTable";
import { FormField } from "@/shared/components/FormField";
import { IconRefresh } from "@/shared/components/icons/IconRefresh";
import { Pagination } from "@/shared/components/Pagination";
import { Select } from "@/shared/components/Select";
import { StatusBadge } from "@/shared/components/StatusBadge";
import { formatTien } from "@/shared/format/formatSo";
import { GIA_TRI_THIEU } from "@/shared/format/formatThoiGian";
import { type ThongBao } from "@/shared/labels/thongDiepLoi";
import { LoaiThietBiDialog } from "./components/LoaiThietBiDialog";
import { trangThaiThanBang } from "@/shared/components/trangThaiThanBang";
import { XoaLoaiThietBiDialog } from "./components/XoaLoaiThietBiDialog";
import { useLoaiThietBiQuery } from "./hooks";
import { THONG_DIEP_DANH_MUC } from "./thongDiep";

/** Giá trị bộ lọc trong ô chọn; null là Tất cả (không gửi tham số ngungSuDung). */
type LocNgungSuDung = "false" | "true";

const LUA_CHON_LOC = [
  { giaTri: "false", nhan: "Đang sử dụng" },
  { giaTri: "true", nhan: "Đã ngưng sử dụng" },
] as const satisfies ReadonlyArray<{ giaTri: LocNgungSuDung; nhan: string }>;

type HopDangMo =
  | { loai: "BIEU_MAU"; loaiThietBi: LoaiThietBi | null }
  | { loai: "XOA"; loaiThietBi: LoaiThietBi };

export function LoaiThietBiScreen() {
  const [trang, setTrang] = useState(1);
  const [loc, setLoc] = useState<LocNgungSuDung | null>(null);
  const truyVan = useLoaiThietBiQuery({
    trang,
    kichThuoc: KICH_THUOC_TRANG_MAC_DINH,
    ...(loc !== null ? { ngungSuDung: loc === "true" } : {}),
  });
  const [hop, setHop] = useState<HopDangMo | null>(null);
  const [thongBao, setThongBao] = useState<ThongBao | null>(null);

  // Xóa dòng cuối của trang cuối làm trang vượt quá số trang → lùi về trang cuối (điều chỉnh khi render).
  if (truyVan.data !== undefined && !truyVan.isPlaceholderData) {
    const luiVe = trangCanLuiVe(
      trang,
      truyVan.data.tongSo,
      KICH_THUOC_TRANG_MAC_DINH,
    );
    if (luiVe !== null) setTrang(luiVe);
  }

  const doiLoc = (giaTri: LocNgungSuDung | null) => {
    setLoc(giaTri);
    setTrang(1);
  };

  const taiLaiDanhSach = async () => (await truyVan.refetch()).isSuccess;

  // API không có GET theo mã nên chỉ tìm được trong trang hiện tại; null không có nghĩa là đã xóa.
  const layBanMoiNhat = async (maLoai: string) => {
    const ketQua = await truyVan.refetch();
    return ketQua.data?.danhSach.find((l) => l.maLoai === maLoai) ?? null;
  };

  const moHop = (moi: HopDangMo) => {
    setThongBao(null);
    setHop(moi);
  };

  const ketThucHop = (tb: ThongBao) => {
    setHop(null);
    setThongBao(tb);
  };

  const khongConTonTai = () => {
    ketThucHop({
      tong: "warning",
      noiDung: THONG_DIEP_DANH_MUC.KHONG_CON_TON_TAI,
    });
    void truyVan.refetch();
  };

  return (
    <main className="flex flex-col gap-section">
      <div className="flex items-center justify-between gap-4">
        <h1 className="type-title-screen text-text-primary">Loại thiết bị</h1>
        <div className="flex gap-actions">
          <Button
            bieuTuong={<IconRefresh kichThuoc="md" />}
            onClick={() => void truyVan.refetch()}
            disabled={truyVan.isFetching}
          >
            Làm mới
          </Button>
          <Button
            bienThe="primary"
            onClick={() => moHop({ loai: "BIEU_MAU", loaiThietBi: null })}
          >
            Thêm loại
          </Button>
        </div>
      </div>

      {thongBao !== null ? (
        <Banner tong={thongBao.tong}>{thongBao.noiDung}</Banner>
      ) : null}

      <div className="flex items-end gap-3">
        <div className="w-dialog-sm">
          <FormField id="locNgungSuDung" nhan="Trạng thái sử dụng">
            {(tt) => (
              <Select<LocNgungSuDung>
                {...tt}
                dsLuaChon={LUA_CHON_LOC}
                giaTri={loc}
                onChange={doiLoc}
                coMucTatCa
              />
            )}
          </FormField>
        </div>
        {loc !== null ? (
          <Button bienThe="tertiary" onClick={() => doiLoc(null)}>
            Xóa bộ lọc
          </Button>
        ) : null}
      </div>

      <DataTable<LoaiThietBi>
        tieuDe="Danh sách loại thiết bị"
        dsCot={[
          { khoa: "tenLoai", tieuDe: "Tên loại", hienThi: (l) => l.tenLoai },
          {
            khoa: "hangSanXuat",
            tieuDe: "Hãng sản xuất",
            hienThi: (l) => l.hangSanXuat?.trim() || GIA_TRI_THIEU,
          },
          {
            khoa: "moTa",
            tieuDe: "Mô tả",
            hienThi: (l) => l.moTa?.trim() || GIA_TRI_THIEU,
          },
          {
            khoa: "chuKyBaoTri",
            tieuDe: "Chu kỳ bảo trì (ngày)",
            canLe: "right",
            hienThi: (l) => l.chuKyBaoTri,
          },
          {
            khoa: "phiTreMoiNgay",
            tieuDe: "Phí trả trễ mỗi ngày (VNĐ)",
            canLe: "right",
            khongXuongDong: true,
            hienThi: (l) => formatTien(l.phiTreMoiNgay),
          },
          {
            khoa: "ngungSuDung",
            tieuDe: "Trạng thái",
            hienThi: (l) =>
              l.ngungSuDung ? (
                <StatusBadge nhanPhatSinh="DA_NGUNG_SU_DUNG" />
              ) : null,
          },
        ]}
        dsDong={truyVan.data?.danhSach ?? []}
        khoaDong={(l) => l.maLoai}
        trangThaiThan={trangThaiThanBang(
          truyVan,
          loc !== null
            ? {
                thongDiep: THONG_DIEP_DANH_MUC.LOAI_RONG_BO_LOC,
                xoaBoLoc: () => doiLoc(null),
              }
            : { thongDiep: THONG_DIEP_DANH_MUC.LOAI_RONG },
        )}
        dangTaiLai={truyVan.isFetching && !truyVan.isPending}
        hanhDongDong={(l) => [
          {
            nhan: "Sửa",
            onClick: () => moHop({ loai: "BIEU_MAU", loaiThietBi: l }),
          },
          {
            nhan: "Xóa",
            huyBo: true,
            onClick: () => moHop({ loai: "XOA", loaiThietBi: l }),
          },
        ]}
      />

      {truyVan.data !== undefined ? (
        <Pagination
          trang={trang}
          kichThuoc={KICH_THUOC_TRANG_MAC_DINH}
          tongSo={truyVan.data.tongSo}
          onDoiTrang={setTrang}
          dangTai={truyVan.isFetching}
        />
      ) : null}

      {hop?.loai === "BIEU_MAU" ? (
        <LoaiThietBiDialog
          loai={hop.loaiThietBi}
          onDong={() => setHop(null)}
          layBanMoiNhat={layBanMoiNhat}
          taiLaiDanhSach={taiLaiDanhSach}
          onKhongConTonTai={khongConTonTai}
        />
      ) : null}
      {hop?.loai === "XOA" ? (
        <XoaLoaiThietBiDialog
          loai={hop.loaiThietBi}
          onDong={() => setHop(null)}
          onXong={ketThucHop}
          taiLaiDanhSach={taiLaiDanhSach}
        />
      ) : null}
    </main>
  );
}
