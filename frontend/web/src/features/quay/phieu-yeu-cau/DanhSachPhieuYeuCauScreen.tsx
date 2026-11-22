"use client";

// SCR-NV-02 Danh sách phiếu yêu cầu chờ duyệt (UI-Screens 6.2; UF-NV-01 bước 1, 2, E1). Sở hữu: Thịnh.
// - GET /quay/phieu-yeu-cau-muon?trangThai=CHO_DUYET, lọc maPhong trong các phòng được cấp (dsPhong của
//   GET /auth/toi qua useAuth) hoặc "Tất cả phòng được cấp quyền"; đổi bộ lọc tải lại từ trang 1.
// - Thứ tự dòng do máy chủ trả (gửi trước hiển thị trước, ĐX-API-28); client không sắp lại.
// - Chọn một dòng: lưu dòng vào ngữ cảnh PhieuYeuCauDuocChon rồi mở SCR-NV-03 (không có GET chi
//   tiết phiếu cho quầy).
// - NV chưa được cấp phòng nào: báo, không gọi danh sách.
import { useRouter } from "next/navigation";
import { useState } from "react";
import {
  KICH_THUOC_TRANG_MAC_DINH,
  trangCanLuiVe,
} from "@/shared/api/phanTrang";
import type { PhieuYeuCauMuonQuay } from "@/shared/api/types";
import { Banner } from "@/shared/components/Banner";
import { Button } from "@/shared/components/Button";
import { DataTable } from "@/shared/components/DataTable";
import { FormField } from "@/shared/components/FormField";
import { IconRefresh } from "@/shared/components/icons/IconRefresh";
import { Pagination } from "@/shared/components/Pagination";
import { Select } from "@/shared/components/Select";
import { trangThaiThanBang } from "@/shared/components/trangThaiThanBang";
import { formatNgayGio } from "@/shared/format/formatThoiGian";
import { useAuth } from "@/shared/hooks/useAuth";
import { BAN_DO_TUYEN } from "@/shared/tuyen/banDoTuyen";
import { usePhieuChoDuyetQuery } from "./hooks";
import { usePhieuYeuCauDuocChon } from "./PhieuYeuCauDuocChon";
import { THONG_DIEP_PHIEU_YEU_CAU } from "./thongDiep";

export function DanhSachPhieuYeuCauScreen() {
  const router = useRouter();
  const { trangThai } = useAuth();
  const dsPhong =
    trangThai.loai === "DA_DANG_NHAP" ? trangThai.nguoiDung.dsPhong : [];
  const { chonPhieu, thongBao, xoaThongBao } = usePhieuYeuCauDuocChon();
  const [trang, setTrang] = useState(1);
  const [maPhong, setMaPhong] = useState<string | null>(null);
  const coPhong = dsPhong.length > 0;

  const truyVan = usePhieuChoDuyetQuery(
    {
      trangThai: "CHO_DUYET",
      trang,
      kichThuoc: KICH_THUOC_TRANG_MAC_DINH,
      ...(maPhong !== null ? { maPhong } : {}),
    },
    { enabled: coPhong },
  );

  // Phiếu hết hạn hoặc được xử lý làm trang vượt quá số trang → lùi về trang cuối.
  if (truyVan.data !== undefined && !truyVan.isPlaceholderData) {
    const luiVe = trangCanLuiVe(
      trang,
      truyVan.data.tongSo,
      KICH_THUOC_TRANG_MAC_DINH,
    );
    if (luiVe !== null) setTrang(luiVe);
  }

  const doiPhong = (ma: string | null) => {
    setMaPhong(ma);
    setTrang(1);
  };

  const moPhieu = (phieu: PhieuYeuCauMuonQuay) => {
    chonPhieu(phieu);
    router.push(BAN_DO_TUYEN["SCR-NV-03"].tuyen);
  };

  return (
    <main className="flex flex-col gap-section">
      <div className="flex items-center justify-between gap-4">
        <h1 className="type-title-screen text-text-primary">
          Phiếu yêu cầu chờ duyệt
        </h1>
        {coPhong ? (
          <Button
            bieuTuong={<IconRefresh kichThuoc="md" />}
            onClick={() => {
              xoaThongBao();
              void truyVan.refetch();
            }}
            disabled={truyVan.isFetching}
          >
            Làm mới
          </Button>
        ) : null}
      </div>

      {thongBao !== null ? (
        <Banner
          tong={thongBao.tong}
          {...(thongBao.traceId !== undefined
            ? { traceId: thongBao.traceId }
            : {})}
        >
          {thongBao.noiDung}
        </Banner>
      ) : null}

      {!coPhong ? (
        <Banner tong="warning">
          {THONG_DIEP_PHIEU_YEU_CAU.CHUA_CAP_PHONG}
        </Banner>
      ) : (
        <>
          <div className="flex flex-wrap items-end gap-3">
            <div className="w-dialog-sm">
              <FormField id="locPhong" nhan="Phòng">
                {(tt) => (
                  <Select
                    {...tt}
                    dsLuaChon={dsPhong.map((p) => ({
                      giaTri: p.maPhong,
                      nhan: p.coSo ? `${p.tenPhong} · ${p.coSo}` : p.tenPhong,
                    }))}
                    giaTri={maPhong}
                    onChange={doiPhong}
                    coMucTatCa
                    nhanTatCa="Tất cả phòng được cấp quyền"
                  />
                )}
              </FormField>
            </div>
          </div>
          <p className="type-caption text-text-secondary">
            {THONG_DIEP_PHIEU_YEU_CAU.GHI_CHU_HET_HAN}
          </p>

          <DataTable<PhieuYeuCauMuonQuay>
            tieuDe="Danh sách phiếu yêu cầu chờ duyệt"
            dsCot={[
              {
                khoa: "maYeuCau",
                tieuDe: "Mã phiếu",
                khongXuongDong: true,
                hienThi: (p) => <span className="type-code">{p.maYeuCau}</span>,
              },
              {
                khoa: "sinhVien",
                tieuDe: "Sinh viên",
                hienThi: (p) => (
                  <div className="flex flex-col">
                    <span className="type-body text-text-primary">
                      {p.sinhVien.hoTen}
                    </span>
                    <span className="type-caption text-text-secondary">
                      <span className="type-code">{p.sinhVien.maSV}</span>
                      {p.sinhVien.lop ? ` · ${p.sinhVien.lop}` : ""}
                    </span>
                  </div>
                ),
              },
              { khoa: "tenPhong", tieuDe: "Phòng", hienThi: (p) => p.tenPhong },
              {
                khoa: "dsLoaiVaSoLuong",
                tieuDe: "Loại và số lượng",
                hienThi: (p) => (
                  <ul className="flex flex-col">
                    {p.dsLoaiVaSoLuong.map((l) => (
                      <li key={l.maLoai} className="whitespace-nowrap">
                        {l.tenLoai}: {l.soLuong}
                      </li>
                    ))}
                  </ul>
                ),
              },
              {
                khoa: "thoiGianMuonDuKienGio",
                tieuDe: "Thời gian mượn",
                canLe: "right",
                khongXuongDong: true,
                hienThi: (p) => `${p.thoiGianMuonDuKienGio} giờ`,
              },
              {
                khoa: "thoiGianGui",
                tieuDe: "Gửi lúc",
                khongXuongDong: true,
                hienThi: (p) => formatNgayGio(p.thoiGianGui),
              },
            ]}
            dsDong={truyVan.data?.danhSach ?? []}
            khoaDong={(p) => p.maYeuCau}
            trangThaiThan={trangThaiThanBang(truyVan, {
              thongDiep: THONG_DIEP_PHIEU_YEU_CAU.RONG,
            })}
            dangTaiLai={truyVan.isFetching && !truyVan.isPending}
            hanhDongDong={(p) => [{ nhan: "Xử lý", onClick: () => moPhieu(p) }]}
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
        </>
      )}
    </main>
  );
}
