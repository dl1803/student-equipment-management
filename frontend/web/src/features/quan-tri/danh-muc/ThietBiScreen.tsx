"use client";

// SCR-QT-04 Thiết bị (UI-Screens 7.4; UF-QT-01; Wireframes SCR-QT-04, P-02, P-04, P-05). Sở hữu: Thịnh.
// Bảng thiết bị phân trang với bộ lọc Tên, Loại, Phòng, Tình trạng (một giá trị hoặc Tất cả); đổi
// bộ lọc tải lại từ trang 1 và bộ lọc đang áp dụng luôn có cách xóa (FA 3.10). Nút "Nhập kho thiết
// bị" và hành động "Sửa" mở ngăn biểu mẫu bên phải (ThietBiNgan).
// Menu Thao tác của dòng giữ đủ ba hành động (Wireframes SCR-QT-04): Sửa; Cập nhật giá trị (hộp
// nhỏ, PUT .../gia-tri); Xóa (409 có lịch sử → đề nghị Ngưng sử dụng, như SCR-QT-03).
// Giá trị hiện hành và tình trạng là số liệu máy chủ; client chỉ định dạng (NT-01).
import { useState, type FormEvent } from "react";
import {
  KICH_THUOC_TRANG_MAC_DINH,
  trangCanLuiVe,
} from "@/shared/api/phanTrang";
import type { ThietBi, TinhTrangThietBi } from "@/shared/api/types";
import { Banner } from "@/shared/components/Banner";
import { Button } from "@/shared/components/Button";
import { DataTable } from "@/shared/components/DataTable";
import { FormField } from "@/shared/components/FormField";
import { IconRefresh } from "@/shared/components/icons/IconRefresh";
import { Pagination } from "@/shared/components/Pagination";
import { Select, type LuaChon } from "@/shared/components/Select";
import { StatusBadge } from "@/shared/components/StatusBadge";
import { TextField } from "@/shared/components/TextField";
import { formatTien } from "@/shared/format/formatSo";
import { GIA_TRI_THIEU } from "@/shared/format/formatThoiGian";
import { type ThongBao } from "@/shared/labels/thongDiepLoi";
import { nhanTrangThai, thongTinNhanPhatSinh } from "@/shared/labels/trangThai";
import { ThietBiNgan } from "./components/ThietBiNgan";
import { trangThaiThanBang } from "./components/trangThaiThanBang";
import {
  useTatCaLoaiQuery,
  useTatCaPhongQuery,
  useThietBiQuery,
} from "./hooks";
import { CapNhatGiaTriDialog } from "./components/CapNhatGiaTriDialog";
import { XoaThietBiDialog } from "./components/XoaThietBiDialog";
import { THONG_DIEP_DANH_MUC, daCapNhatGiaTri } from "./thongDiep";

/** Thứ tự lựa chọn Tình trạng theo UI-Screens 7.4 (DS 7.2). */
const DS_TINH_TRANG = [
  "SAN_SANG",
  "DANG_DUOC_MUON",
  "DANG_BAO_TRI_SUA_CHUA",
  "DA_THANH_LY",
  "DA_BAO_MAT",
] as const satisfies ReadonlyArray<TinhTrangThietBi>;

const LUA_CHON_TINH_TRANG: ReadonlyArray<LuaChon<TinhTrangThietBi>> =
  DS_TINH_TRANG.map((ma) => ({
    giaTri: ma,
    nhan: nhanTrangThai("TINH_TRANG_THIET_BI", ma),
  }));

type BoLoc = {
  ten: string;
  maLoai: string | null;
  maPhong: string | null;
  tinhTrang: TinhTrangThietBi | null;
};

const BO_LOC_TRONG: BoLoc = {
  ten: "",
  maLoai: null,
  maPhong: null,
  tinhTrang: null,
};

/** Kệ và tủ: "2/3"; thiếu một phần thì phần đó là "—"; thiếu cả hai là "—". */
function hienThiKeTu(
  viTriKe: string | null | undefined,
  viTriTu: string | null | undefined,
): string {
  const ke = viTriKe?.trim() || null;
  const tu = viTriTu?.trim() || null;
  if (ke === null && tu === null) return GIA_TRI_THIEU;
  return `${ke ?? GIA_TRI_THIEU}/${tu ?? GIA_TRI_THIEU}`;
}

type HopDangMo =
  | { loai: "NGAN"; thietBi: ThietBi | null }
  | { loai: "GIA_TRI"; thietBi: ThietBi }
  | { loai: "XOA"; thietBi: ThietBi };

export function ThietBiScreen() {
  const [trang, setTrang] = useState(1);
  const [boLoc, setBoLoc] = useState<BoLoc>(BO_LOC_TRONG);
  // Ô tên chỉ áp dụng khi nhấn Enter hoặc rời ô, không gửi yêu cầu theo từng phím.
  const [tenDangNhap, setTenDangNhap] = useState("");
  const truyVan = useThietBiQuery({
    trang,
    kichThuoc: KICH_THUOC_TRANG_MAC_DINH,
    ...(boLoc.ten !== "" ? { ten: boLoc.ten } : {}),
    ...(boLoc.maLoai !== null ? { maLoai: boLoc.maLoai } : {}),
    ...(boLoc.maPhong !== null ? { maPhong: boLoc.maPhong } : {}),
    ...(boLoc.tinhTrang !== null ? { tinhTrang: boLoc.tinhTrang } : {}),
  });
  const truyVanLoai = useTatCaLoaiQuery();
  const truyVanPhong = useTatCaPhongQuery();
  const [hop, setHop] = useState<HopDangMo | null>(null);
  const [thongBao, setThongBao] = useState<ThongBao | null>(null);

  // Trang vượt quá số trang (dữ liệu bị xóa bớt) → lùi về trang cuối (điều chỉnh khi render).
  if (truyVan.data !== undefined && !truyVan.isPlaceholderData) {
    const luiVe = trangCanLuiVe(
      trang,
      truyVan.data.tongSo,
      KICH_THUOC_TRANG_MAC_DINH,
    );
    if (luiVe !== null) setTrang(luiVe);
  }

  const coBoLoc =
    boLoc.ten !== "" ||
    boLoc.maLoai !== null ||
    boLoc.maPhong !== null ||
    boLoc.tinhTrang !== null;

  const doiBoLoc = (thayDoi: Partial<BoLoc>) => {
    setBoLoc((cu) => ({ ...cu, ...thayDoi }));
    setTrang(1);
  };

  const apDungTen = () => {
    const ten = tenDangNhap.trim();
    if (ten !== boLoc.ten) doiBoLoc({ ten });
  };

  const xoaBoLoc = () => {
    setTenDangNhap("");
    doiBoLoc(BO_LOC_TRONG);
  };

  const guiTimTen = (suKien: FormEvent<HTMLFormElement>) => {
    suKien.preventDefault();
    apDungTen();
  };

  const taiLaiDanhSach = async () => (await truyVan.refetch()).isSuccess;

  // API không có GET theo mã nên chỉ tìm được trong trang hiện tại; null không có nghĩa là đã xóa.
  const layBanMoiNhat = async (maThietBi: string) => {
    const ketQua = await truyVan.refetch();
    return (
      ketQua.data?.danhSach.find((tb) => tb.maThietBi === maThietBi) ?? null
    );
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
    setHop(null);
    setThongBao({
      tong: "warning",
      noiDung: THONG_DIEP_DANH_MUC.KHONG_CON_TON_TAI,
    });
    void truyVan.refetch();
  };

  const luaChonLoc = {
    loai: (truyVanLoai.data ?? []).map((l) => ({
      giaTri: l.maLoai,
      nhan: l.ngungSuDung
        ? `${l.tenLoai} (${thongTinNhanPhatSinh("DA_NGUNG_SU_DUNG").nhan})`
        : l.tenLoai,
    })),
    phong: (truyVanPhong.data ?? []).map((p) => ({
      giaTri: p.maPhong,
      nhan: p.coSo ? `${p.tenPhong} · ${p.coSo}` : p.tenPhong,
    })),
  };

  return (
    <main className="flex flex-col gap-section">
      <div className="flex items-center justify-between gap-4">
        <h1 className="type-title-screen text-text-primary">Thiết bị</h1>
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
            onClick={() => moHop({ loai: "NGAN", thietBi: null })}
          >
            Nhập kho thiết bị
          </Button>
        </div>
      </div>

      {thongBao !== null ? (
        <Banner tong={thongBao.tong}>{thongBao.noiDung}</Banner>
      ) : null}

      <div className="flex flex-wrap items-end gap-3">
        <form
          role="search"
          aria-label="Tìm theo tên thiết bị"
          onSubmit={guiTimTen}
          className="w-dialog-sm"
        >
          <FormField id="locTen" nhan="Tên thiết bị">
            {(tt) => (
              <TextField
                {...tt}
                type="search"
                value={tenDangNhap}
                onChange={(e) => setTenDangNhap(e.target.value)}
                onBlur={apDungTen}
              />
            )}
          </FormField>
        </form>
        <div className="w-keyvalue-label">
          <FormField id="locLoai" nhan="Loại">
            {(tt) => (
              <Select
                {...tt}
                dsLuaChon={luaChonLoc.loai}
                giaTri={boLoc.maLoai}
                onChange={(maLoai) => doiBoLoc({ maLoai })}
                coMucTatCa
                dangTaiLuaChon={
                  truyVanLoai.isPending && luaChonLoc.loai.length === 0
                }
                {...(truyVanLoai.isError
                  ? {
                      loiTaiLuaChon: {
                        onThuLai: () => void truyVanLoai.refetch(),
                      },
                    }
                  : {})}
              />
            )}
          </FormField>
        </div>
        <div className="w-keyvalue-label">
          <FormField id="locPhong" nhan="Phòng">
            {(tt) => (
              <Select
                {...tt}
                dsLuaChon={luaChonLoc.phong}
                giaTri={boLoc.maPhong}
                onChange={(maPhong) => doiBoLoc({ maPhong })}
                coMucTatCa
                dangTaiLuaChon={
                  truyVanPhong.isPending && luaChonLoc.phong.length === 0
                }
                {...(truyVanPhong.isError
                  ? {
                      loiTaiLuaChon: {
                        onThuLai: () => void truyVanPhong.refetch(),
                      },
                    }
                  : {})}
              />
            )}
          </FormField>
        </div>
        <div className="w-keyvalue-label">
          <FormField id="locTinhTrang" nhan="Tình trạng">
            {(tt) => (
              <Select<TinhTrangThietBi>
                {...tt}
                dsLuaChon={LUA_CHON_TINH_TRANG}
                giaTri={boLoc.tinhTrang}
                onChange={(tinhTrang) => doiBoLoc({ tinhTrang })}
                coMucTatCa
              />
            )}
          </FormField>
        </div>
        {coBoLoc ? (
          <Button bienThe="tertiary" onClick={xoaBoLoc}>
            Xóa bộ lọc
          </Button>
        ) : null}
      </div>

      <DataTable<ThietBi>
        tieuDe="Danh sách thiết bị"
        dsCot={[
          {
            khoa: "maThietBi",
            tieuDe: "Mã",
            khongXuongDong: true,
            hienThi: (tb) => <span className="type-code">{tb.maThietBi}</span>,
          },
          { khoa: "tenThietBi", tieuDe: "Tên", hienThi: (tb) => tb.tenThietBi },
          { khoa: "tenLoai", tieuDe: "Loại", hienThi: (tb) => tb.tenLoai },
          { khoa: "tenPhong", tieuDe: "Phòng", hienThi: (tb) => tb.tenPhong },
          {
            khoa: "keTu",
            tieuDe: "Kệ/Tủ",
            khongXuongDong: true,
            hienThi: (tb) => hienThiKeTu(tb.viTriKe, tb.viTriTu),
          },
          {
            khoa: "tinhTrang",
            tieuDe: "Tình trạng",
            hienThi: (tb) => (
              <div className="flex flex-wrap gap-1">
                <StatusBadge ho="TINH_TRANG_THIET_BI" ma={tb.tinhTrang} />
                {tb.ngungSuDung ? (
                  <StatusBadge nhanPhatSinh="DA_NGUNG_SU_DUNG" />
                ) : null}
              </div>
            ),
          },
          {
            khoa: "giaTriHienHanh",
            tieuDe: "Giá trị (VNĐ)",
            canLe: "right",
            khongXuongDong: true,
            hienThi: (tb) => formatTien(tb.giaTriHienHanh),
          },
        ]}
        dsDong={truyVan.data?.danhSach ?? []}
        khoaDong={(tb) => tb.maThietBi}
        trangThaiThan={trangThaiThanBang(
          truyVan,
          coBoLoc
            ? {
                thongDiep: THONG_DIEP_DANH_MUC.THIET_BI_RONG_BO_LOC,
                xoaBoLoc,
              }
            : { thongDiep: THONG_DIEP_DANH_MUC.THIET_BI_RONG },
        )}
        dangTaiLai={truyVan.isFetching && !truyVan.isPending}
        hanhDongDong={(tb) => [
          { nhan: "Sửa", onClick: () => moHop({ loai: "NGAN", thietBi: tb }) },
          {
            nhan: "Cập nhật giá trị",
            onClick: () => moHop({ loai: "GIA_TRI", thietBi: tb }),
          },
          {
            nhan: "Xóa",
            onClick: () => moHop({ loai: "XOA", thietBi: tb }),
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

      {hop?.loai === "NGAN" ? (
        <ThietBiNgan
          // Dựng lại khi đổi thiết bị: biểu mẫu và khu ảnh chỉ đọc dữ liệu lúc khởi tạo.
          key={hop.thietBi?.maThietBi ?? "them-moi"}
          thietBi={hop.thietBi}
          onDong={() => setHop(null)}
          layBanMoiNhat={layBanMoiNhat}
          taiLaiDanhSach={taiLaiDanhSach}
          onKhongConTonTai={khongConTonTai}
        />
      ) : null}
      {hop?.loai === "GIA_TRI" ? (
        <CapNhatGiaTriDialog
          thietBi={hop.thietBi}
          onDong={() => setHop(null)}
          onXong={(banGhi) =>
            ketThucHop({
              tong: "success",
              noiDung: daCapNhatGiaTri(
                hop.thietBi.tenThietBi,
                formatTien(banGhi.giaTri),
              ),
            })
          }
          taiLaiDanhSach={taiLaiDanhSach}
          onKhongConTonTai={khongConTonTai}
        />
      ) : null}
      {hop?.loai === "XOA" ? (
        <XoaThietBiDialog
          thietBi={hop.thietBi}
          onDong={() => setHop(null)}
          onXong={ketThucHop}
          taiLaiDanhSach={taiLaiDanhSach}
        />
      ) : null}
    </main>
  );
}
