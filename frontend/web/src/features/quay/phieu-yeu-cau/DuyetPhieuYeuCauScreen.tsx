"use client";

// SCR-NV-03 Duyệt hoặc từ chối phiếu yêu cầu (UI-Screens 6.3; UF-NV-01 bước 3–5; A1, A2, E1–E3).
// Sở hữu: Thịnh. D10-T: xem phiếu và kết quả kiểm tra; D11-T: Duyệt, Từ chối có xác nhận.
// - Thông tin phiếu lấy từ dòng đã chọn ở SCR-NV-02 (ngữ cảnh PhieuYeuCauDuocChon); hợp đồng không
//   có GET chi tiết phiếu cho quầy. Thiếu ngữ cảnh (tải lại trang) → quay về danh sách (ĐX-UI-08).
// - GET .../kiem-tra: hiển thị nguyên duocDuyet, dsLyDo[].thongDiep và dsKhaDung; client KHÔNG tự tính
//   điều kiện. Dòng có số khả dụng nhỏ hơn số yêu cầu được nhấn mạnh bằng chữ "Không đủ" kèm biểu tượng.
// - Nút Duyệt chỉ bật khi máy chủ trả duocDuyet = true (A1) và không đang tải lại; nút Từ chối luôn bật
//   với phiếu Chờ duyệt (A2). Cả hai mở hộp xác nhận (ĐX-UF-05).
// - Thành công: hiển thị kết quả từ phản hồi (hạn giữ chỗ hanGiuCho do máy chủ đặt), không tính ở client.
// - E3, xung đột: tải lại kết quả kiểm tra và hiển thị theo kết quả mới, không tự gửi lại. E1, E2: báo và
//   quay về danh sách. Mất phản hồi: khóa cả hai nút, chỉ cho kiểm tra danh sách trước khi thao tác lại
//   (FA 3.11) — khóa giữ ở màn hình nên đóng hộp không mở đường gửi lại.
import { useQueryClient } from "@tanstack/react-query";
import { useRouter } from "next/navigation";
import { useEffect, useState } from "react";
import { laApiError } from "@/shared/api/apiError";
import { TIEN_TO_KHOA } from "@/shared/api/tienToKhoa";
import type { KhaDungTheoLoai, PhieuYeuCauMuonQuay } from "@/shared/api/types";
import { Banner } from "@/shared/components/Banner";
import { Button } from "@/shared/components/Button";
import { DataTable } from "@/shared/components/DataTable";
import { IconRefresh } from "@/shared/components/icons/IconRefresh";
import { IconWarning } from "@/shared/components/icons/IconWarning";
import { KeyValueList } from "@/shared/components/KeyValueList";
import { StateView } from "@/shared/components/StateView";
import { formatNgayGio } from "@/shared/format/formatThoiGian";
import {
  THONG_DIEP_LOI_CHUNG,
  type ThongBao,
} from "@/shared/labels/thongDiepLoi";
import { BAN_DO_TUYEN } from "@/shared/tuyen/banDoTuyen";
import { DuyetPhieuDialog } from "./components/DuyetPhieuDialog";
import { TuChoiPhieuDialog } from "./components/TuChoiPhieuDialog";
import { useKiemTraPhieuQuery } from "./hooks";
import type { LoiChoManHinh } from "./phanLoaiLoiGhi";
import { usePhieuYeuCauDuocChon } from "./PhieuYeuCauDuocChon";
import {
  chuaBietKetQuaCua,
  daDuyet,
  daTuChoi,
  THONG_DIEP_PHIEU_YEU_CAU,
} from "./thongDiep";

const TUYEN_DANH_SACH = BAN_DO_TUYEN["SCR-NV-02"].tuyen;

type HanhDong = "DUYET" | "TU_CHOI";
type KetQuaXuLy = { hanhDong: HanhDong; phieu: PhieuYeuCauMuonQuay };

export function DuyetPhieuYeuCauScreen() {
  const router = useRouter();
  const { phieu } = usePhieuYeuCauDuocChon();

  // Tải lại trang làm mất ngữ cảnh: không có dữ liệu phiếu để hiển thị → về danh sách.
  useEffect(() => {
    if (phieu === null) router.replace(TUYEN_DANH_SACH);
  }, [phieu, router]);

  if (phieu === null) return null;
  // key theo mã phiếu: Next giữ trạng thái của tuyến đã ghé, nên chọn phiếu khác phải dựng lại phần
  // thân để kết quả xử lý, cảnh báo, khóa "chưa biết kết quả" của phiếu trước không sót sang phiếu mới.
  return <ManHinhPhieu key={phieu.maYeuCau} phieu={phieu} />;
}

function ManHinhPhieu({ phieu }: { phieu: PhieuYeuCauMuonQuay }) {
  const router = useRouter();
  const queryClient = useQueryClient();
  const { boChon } = usePhieuYeuCauDuocChon();
  const [hop, setHop] = useState<HanhDong | null>(null);
  const [ketQuaXuLy, setKetQuaXuLy] = useState<KetQuaXuLy | null>(null);
  const [thongBao, setThongBao] = useState<ThongBao | null>(null);
  const [chuaBietKetQua, setChuaBietKetQua] = useState<HanhDong | null>(null);
  // Phiếu đã xử lý xong thì không kiểm tra nữa (máy chủ sẽ trả 404 vì phiếu không còn Chờ duyệt).
  const kiemTra = useKiemTraPhieuQuery(
    ketQuaXuLy === null ? phieu.maYeuCau : null,
  );

  // E1, E2 khi kiểm tra: phiếu không còn chờ duyệt hoặc phòng chưa được cấp quyền → báo và quay về.
  const loi = kiemTra.error;
  useEffect(() => {
    if (!laApiError(loi)) return;
    const noiDung =
      loi.maLoi === "PHONG_CHUA_CAP_QUYEN"
        ? THONG_DIEP_LOI_CHUNG.PHONG_CHUA_CAP_QUYEN
        : loi.maLoi === "KHONG_TIM_THAY"
          ? THONG_DIEP_PHIEU_YEU_CAU.KHONG_CON_CHO_DUYET
          : null;
    if (noiDung === null) return;
    void queryClient.invalidateQueries({
      queryKey: TIEN_TO_KHOA.quayPhieuYeuCau,
    });
    boChon({ tong: "warning", noiDung });
    router.replace(TUYEN_DANH_SACH);
  }, [loi, boChon, queryClient, router]);

  const veDanhSach = () => router.push(TUYEN_DANH_SACH);

  const roiVeDanhSach = (tb: ThongBao) => {
    void queryClient.invalidateQueries({
      queryKey: TIEN_TO_KHOA.quayPhieuYeuCau,
    });
    boChon(tb);
    router.replace(TUYEN_DANH_SACH);
  };

  const xuLyLoi = (hanhDong: HanhDong) => (kq: LoiChoManHinh) => {
    setHop(null);
    switch (kq.loai) {
      case "ROI_MAN_HINH":
        roiVeDanhSach(kq.thongBao);
        break;
      case "TAI_LAI_KIEM_TRA":
        setThongBao(kq.thongBao);
        void kiemTra.refetch();
        break;
      case "CHUA_BIET_KET_QUA":
        setChuaBietKetQua(hanhDong);
        break;
    }
  };

  const xong = (hanhDong: HanhDong) => (phieuSau: PhieuYeuCauMuonQuay) => {
    setHop(null);
    setThongBao(null);
    setKetQuaXuLy({ hanhDong, phieu: phieuSau });
  };

  const kiemTraDanhSach = () =>
    roiVeDanhSach({
      tong: "warning",
      noiDung: chuaBietKetQuaCua(
        chuaBietKetQua === "TU_CHOI" ? "từ chối" : "duyệt",
        phieu.maYeuCau,
      ),
    });

  const moHop = (hanhDong: HanhDong) => {
    setThongBao(null);
    setHop(hanhDong);
  };

  let noiDungChinh;
  if (ketQuaXuLy !== null) {
    const p = ketQuaXuLy.phieu;
    noiDungChinh =
      ketQuaXuLy.hanhDong === "DUYET" ? (
        <Banner tong="success" tieuDe={daDuyet(phieu.maYeuCau)}>
          <KeyValueList
            dsDong={[
              { ten: "Hạn giữ chỗ", giaTri: formatNgayGio(p.hanGiuCho) },
            ]}
          />
          <p>Sinh viên đã được thông báo.</p>
        </Banner>
      ) : (
        <Banner tong="success" tieuDe={daTuChoi(phieu.maYeuCau)}>
          <KeyValueList
            dsDong={[{ ten: "Lý do từ chối", giaTri: p.lyDoTuChoi }]}
          />
          <p>Sinh viên đã được thông báo.</p>
        </Banner>
      );
  } else if (kiemTra.isPending) {
    noiDungChinh = <StateView loai="DANG_TAI" />;
  } else if (kiemTra.data === undefined) {
    const thuLai = () => void kiemTra.refetch();
    noiDungChinh =
      laApiError(loi) && loi.loai === "HTTP" ? (
        <StateView
          loai="LOI_HE_THONG"
          {...(loi.traceId !== undefined ? { traceId: loi.traceId } : {})}
          onThuLai={thuLai}
        />
      ) : (
        <StateView loai="LOI_MANG" onThuLai={thuLai} />
      );
  } else {
    const kq = kiemTra.data;
    noiDungChinh = (
      <>
        <section
          aria-labelledby="tieu-de-so-luong"
          className="flex flex-col gap-3"
        >
          <h2
            id="tieu-de-so-luong"
            className="type-title-section text-text-primary"
          >
            Số lượng yêu cầu và khả dụng
          </h2>
          <DataTable<KhaDungTheoLoai>
            tieuDe="Số lượng yêu cầu và khả dụng theo loại"
            dsCot={[
              { khoa: "tenLoai", tieuDe: "Loại", hienThi: (d) => d.tenLoai },
              {
                khoa: "soLuongYeuCau",
                tieuDe: "Yêu cầu",
                canLe: "right",
                hienThi: (d) => d.soLuongYeuCau,
              },
              {
                khoa: "soLuongKhaDung",
                tieuDe: "Khả dụng (đã trừ giữ chỗ)",
                canLe: "right",
                khongXuongDong: true,
                hienThi: (d) =>
                  d.soLuongKhaDung < d.soLuongYeuCau ? (
                    <span className="inline-flex items-center gap-1 type-body-strong text-tone-danger-text">
                      <IconWarning
                        kichThuoc="sm"
                        className="text-tone-danger-icon"
                      />
                      {d.soLuongKhaDung} · Không đủ
                    </span>
                  ) : (
                    d.soLuongKhaDung
                  ),
              },
            ]}
            dsDong={kq.dsKhaDung}
            khoaDong={(d) => d.maLoai}
          />
        </section>
        <Banner
          tong={kq.duocDuyet ? "success" : "danger"}
          ketLuan={kq.duocDuyet ? "DUOC_DUYET" : "KHONG_DUOC_DUYET"}
          dsLyDo={kq.dsLyDo.map((lyDo) => lyDo.thongDiep)}
        />
      </>
    );
  }

  const daXuLy = ketQuaXuLy !== null;
  const biKhoa = chuaBietKetQua !== null;
  const choDuyet =
    kiemTra.data?.duocDuyet === true && !kiemTra.isFetching && !biKhoa;

  return (
    <main className="flex flex-col gap-section">
      <div className="flex items-center justify-between gap-4">
        <h1 className="type-title-screen text-text-primary">
          Phiếu yêu cầu <span className="type-code">{phieu.maYeuCau}</span>
        </h1>
        <div className="flex gap-actions">
          <Button
            bienThe={daXuLy ? "primary" : "secondary"}
            onClick={veDanhSach}
          >
            Về danh sách
          </Button>
          {daXuLy ? null : (
            <Button
              bieuTuong={<IconRefresh kichThuoc="md" />}
              onClick={() => void kiemTra.refetch()}
              disabled={kiemTra.isFetching || biKhoa}
            >
              Kiểm tra lại
            </Button>
          )}
        </div>
      </div>

      {biKhoa ? (
        <Banner
          tong="warning"
          hanhDong={{ nhan: "Kiểm tra danh sách", onClick: kiemTraDanhSach }}
        >
          {THONG_DIEP_PHIEU_YEU_CAU.CHUA_BIET_KET_QUA}
        </Banner>
      ) : null}
      {thongBao !== null ? (
        <Banner tong={thongBao.tong}>{thongBao.noiDung}</Banner>
      ) : null}

      <KeyValueList
        dsDong={[
          { ten: "Mã phiếu", giaTri: phieu.maYeuCau, laMa: true },
          {
            ten: "Sinh viên",
            giaTri: `${phieu.sinhVien.hoTen} (${phieu.sinhVien.maSV})`,
          },
          { ten: "Lớp", giaTri: phieu.sinhVien.lop },
          { ten: "Phòng", giaTri: phieu.tenPhong },
          {
            ten: "Thời gian mượn dự kiến",
            giaTri: `${phieu.thoiGianMuonDuKienGio} giờ`,
          },
          { ten: "Gửi lúc", giaTri: formatNgayGio(phieu.thoiGianGui) },
        ]}
      />

      <div aria-live="polite" aria-busy={kiemTra.isFetching || undefined}>
        <div className="flex flex-col gap-section">{noiDungChinh}</div>
      </div>

      {daXuLy ? null : (
        <div className="flex justify-end gap-actions">
          <Button onClick={() => moHop("TU_CHOI")} disabled={biKhoa}>
            Từ chối
          </Button>
          <Button
            bienThe="primary"
            onClick={() => moHop("DUYET")}
            disabled={!choDuyet}
          >
            Duyệt
          </Button>
        </div>
      )}

      {hop === "DUYET" ? (
        <DuyetPhieuDialog
          phieu={phieu}
          onDong={() => setHop(null)}
          onXong={xong("DUYET")}
          onLoi={xuLyLoi("DUYET")}
        />
      ) : null}
      {hop === "TU_CHOI" ? (
        <TuChoiPhieuDialog
          phieu={phieu}
          onDong={() => setHop(null)}
          onXong={xong("TU_CHOI")}
          onLoi={xuLyLoi("TU_CHOI")}
          onTaiLaiKiemTra={() => void kiemTra.refetch()}
        />
      ) : null}
    </main>
  );
}
