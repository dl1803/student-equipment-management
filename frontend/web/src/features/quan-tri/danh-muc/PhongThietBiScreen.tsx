"use client";

// SCR-QT-02 Phòng thiết bị (UI-Screens 7.2; UF-QT-01; Wireframes 7). Sở hữu: Thịnh.
// Xem, thêm, sửa phòng; KHÔNG có Xóa. Bảng phân trang: tên phòng, cơ sở.
import { useState } from "react";
import {
  KICH_THUOC_TRANG_MAC_DINH,
  trangCanLuiVe,
} from "@/shared/api/phanTrang";
import type { PhongThietBi } from "@/shared/api/types";
import { Banner } from "@/shared/components/Banner";
import { Button } from "@/shared/components/Button";
import { DataTable } from "@/shared/components/DataTable";
import { IconRefresh } from "@/shared/components/icons/IconRefresh";
import { Pagination } from "@/shared/components/Pagination";
import { type ThongBao } from "@/shared/labels/thongDiepLoi";
import { PhongDialog } from "./components/PhongDialog";
import { trangThaiThanBang } from "./components/trangThaiThanBang";
import { usePhongThietBiQuery } from "./hooks";
import { THONG_DIEP_DANH_MUC } from "./thongDiep";

/** Hộp biểu mẫu đang mở: phong = null là thêm mới. */
type HopBieuMau = { phong: PhongThietBi | null };

export function PhongThietBiScreen() {
  const [trang, setTrang] = useState(1);
  const truyVan = usePhongThietBiQuery({
    trang,
    kichThuoc: KICH_THUOC_TRANG_MAC_DINH,
  });
  const [hop, setHop] = useState<HopBieuMau | null>(null);
  const [thongBao, setThongBao] = useState<ThongBao | null>(null);

  // Tổng số giảm làm trang hiện tại vượt quá số trang → lùi về trang cuối (điều chỉnh khi render).
  if (truyVan.data !== undefined && !truyVan.isPlaceholderData) {
    const luiVe = trangCanLuiVe(
      trang,
      truyVan.data.tongSo,
      KICH_THUOC_TRANG_MAC_DINH,
    );
    if (luiVe !== null) setTrang(luiVe);
  }

  const moHop = (phong: PhongThietBi | null) => {
    setThongBao(null);
    setHop({ phong });
  };

  const taiLaiDanhSach = async () => (await truyVan.refetch()).isSuccess;

  // API không có GET theo mã nên chỉ tìm được trong trang hiện tại; null không có nghĩa là đã xóa.
  const layBanMoiNhat = async (maPhong: string) => {
    const ketQua = await truyVan.refetch();
    return ketQua.data?.danhSach.find((p) => p.maPhong === maPhong) ?? null;
  };

  const khongConTonTai = () => {
    setHop(null);
    setThongBao({
      tong: "warning",
      noiDung: THONG_DIEP_DANH_MUC.KHONG_CON_TON_TAI,
    });
    void truyVan.refetch();
  };

  return (
    <main className="flex flex-col gap-section">
      <div className="flex items-center justify-between gap-4">
        <h1 className="type-title-screen text-text-primary">Phòng thiết bị</h1>
        <div className="flex gap-actions">
          <Button
            bieuTuong={<IconRefresh kichThuoc="md" />}
            onClick={() => void truyVan.refetch()}
            disabled={truyVan.isFetching}
          >
            Làm mới
          </Button>
          <Button bienThe="primary" onClick={() => moHop(null)}>
            Thêm phòng
          </Button>
        </div>
      </div>

      {thongBao !== null ? (
        <Banner tong={thongBao.tong}>{thongBao.noiDung}</Banner>
      ) : null}

      <DataTable<PhongThietBi>
        tieuDe="Danh sách phòng thiết bị"
        dsCot={[
          { khoa: "tenPhong", tieuDe: "Tên phòng", hienThi: (p) => p.tenPhong },
          { khoa: "coSo", tieuDe: "Cơ sở", hienThi: (p) => p.coSo },
        ]}
        dsDong={truyVan.data?.danhSach ?? []}
        khoaDong={(p) => p.maPhong}
        trangThaiThan={trangThaiThanBang(truyVan, {
          thongDiep: THONG_DIEP_DANH_MUC.PHONG_RONG,
        })}
        dangTaiLai={truyVan.isFetching && !truyVan.isPending}
        hanhDongDong={(p) => [{ nhan: "Sửa", onClick: () => moHop(p) }]}
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

      {hop !== null ? (
        <PhongDialog
          phong={hop.phong}
          onDong={() => setHop(null)}
          layBanMoiNhat={layBanMoiNhat}
          taiLaiDanhSach={taiLaiDanhSach}
          onKhongConTonTai={khongConTonTai}
        />
      ) : null}
    </main>
  );
}
