"use client";

// SCR-NV-03 Duyệt hoặc từ chối phiếu yêu cầu (UI-Screens 6.3; UF-NV-01 bước 3; E1, E2). Sở hữu: Thịnh.
// D10-T: xem phiếu và kết quả kiểm tra điều kiện. Nút Duyệt, Từ chối và hộp xác nhận thuộc D11-T.
// - Thông tin phiếu lấy từ dòng đã chọn ở SCR-NV-02 (ngữ cảnh PhieuYeuCauDuocChon); hợp đồng không
//   có GET chi tiết phiếu cho quầy. Thiếu ngữ cảnh (tải lại trang) → quay về danh sách (ĐX-UI-08).
// - GET /quay/phieu-yeu-cau-muon/{ma}/kiem-tra: hiển thị nguyên duocDuyet, dsLyDo[].thongDiep và
//   dsKhaDung do máy chủ trả; client KHÔNG tự tính điều kiện. Dòng có số khả dụng nhỏ hơn số yêu cầu
//   được nhấn mạnh bằng chữ "Không đủ" kèm biểu tượng (dấu hiệu ngoài màu).
// - 403 PHONG_CHUA_CAP_QUYEN (E2) và 404 (phiếu không còn chờ duyệt, E1): báo và quay về danh sách,
//   danh sách được đánh dấu lỗi thời để tải lại.
import { useQueryClient } from "@tanstack/react-query";
import { useRouter } from "next/navigation";
import { useEffect } from "react";
import { laApiError } from "@/shared/api/apiError";
import { TIEN_TO_KHOA } from "@/shared/api/tienToKhoa";
import type { KhaDungTheoLoai } from "@/shared/api/types";
import { Banner } from "@/shared/components/Banner";
import { Button } from "@/shared/components/Button";
import { DataTable } from "@/shared/components/DataTable";
import { IconRefresh } from "@/shared/components/icons/IconRefresh";
import { IconWarning } from "@/shared/components/icons/IconWarning";
import { KeyValueList } from "@/shared/components/KeyValueList";
import { StateView } from "@/shared/components/StateView";
import { formatNgayGio } from "@/shared/format/formatThoiGian";
import { THONG_DIEP_LOI_CHUNG } from "@/shared/labels/thongDiepLoi";
import { BAN_DO_TUYEN } from "@/shared/tuyen/banDoTuyen";
import { useKiemTraPhieuQuery } from "./hooks";
import { usePhieuYeuCauDuocChon } from "./PhieuYeuCauDuocChon";
import { THONG_DIEP_PHIEU_YEU_CAU } from "./thongDiep";

const TUYEN_DANH_SACH = BAN_DO_TUYEN["SCR-NV-02"].tuyen;

export function DuyetPhieuYeuCauScreen() {
  const router = useRouter();
  const queryClient = useQueryClient();
  const { phieu, boChon } = usePhieuYeuCauDuocChon();
  const kiemTra = useKiemTraPhieuQuery(phieu?.maYeuCau ?? null);

  // Tải lại trang làm mất ngữ cảnh: không có dữ liệu phiếu để hiển thị → về danh sách.
  useEffect(() => {
    if (phieu === null) router.replace(TUYEN_DANH_SACH);
  }, [phieu, router]);

  // E1, E2: phiếu không còn chờ duyệt hoặc phòng chưa được cấp quyền → báo ở danh sách và quay về.
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

  if (phieu === null) return null;

  const veDanhSach = () => router.push(TUYEN_DANH_SACH);

  let ketQua;
  if (kiemTra.isPending) {
    ketQua = <StateView loai="DANG_TAI" />;
  } else if (kiemTra.data === undefined) {
    const thuLai = () => void kiemTra.refetch();
    ketQua =
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
    ketQua = (
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

  return (
    <main className="flex flex-col gap-section">
      <div className="flex items-center justify-between gap-4">
        <h1 className="type-title-screen text-text-primary">
          Phiếu yêu cầu <span className="type-code">{phieu.maYeuCau}</span>
        </h1>
        <div className="flex gap-actions">
          <Button onClick={veDanhSach}>Về danh sách</Button>
          <Button
            bieuTuong={<IconRefresh kichThuoc="md" />}
            onClick={() => void kiemTra.refetch()}
            disabled={kiemTra.isFetching}
          >
            Kiểm tra lại
          </Button>
        </div>
      </div>

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
        <div className="flex flex-col gap-section">{ketQua}</div>
      </div>
    </main>
  );
}
