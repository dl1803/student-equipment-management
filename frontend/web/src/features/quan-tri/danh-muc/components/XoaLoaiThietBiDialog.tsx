"use client";

// Xóa và ngưng sử dụng loại thiết bị — SCR-QT-03 (UI-Screens 7.3; DS 9.2; BR-18). Sở hữu: Thịnh.
// 1. Hộp xác nhận Xóa (hủy bỏ) nêu loại sẽ bị xóa.
// 2. 409 DA_CO_LICH_SU_CHI_NGUNG_SU_DUNG: loại chưa ngưng → chuyển sang hộp Ngưng sử dụng (hủy bỏ),
//    nêu loại vẫn giữ lịch sử, không còn dùng cho thiết bị mới; KHÔNG hứa hẹn hoàn tác (API không có).
//    Loại đã ngưng → đóng hộp, báo ở màn hình (không đề nghị ngưng lần nữa).
import { useState } from "react";
import { laChuaBietKetQua, type ApiError } from "@/shared/api/apiError";
import type { LoaiThietBi } from "@/shared/api/types";
import { Banner } from "@/shared/components/Banner";
import { ConfirmDialog } from "@/shared/components/ConfirmDialog";
import { KeyValueList } from "@/shared/components/KeyValueList";
import { useThaoTacGhi } from "@/shared/hooks/useThaoTacGhi";
import { thongBaoTheoLoi, type ThongBao } from "@/shared/labels/thongDiepLoi";
import { useNgungSuDungLoaiMutation, useXoaLoaiMutation } from "../hooks";
import { THONG_DIEP_DANH_MUC, daNgungLoai, daXoaLoai } from "../thongDiep";

export type XoaLoaiThietBiDialogProps = {
  loai: LoaiThietBi;
  onDong: () => void;
  /** Kết thúc với thông báo hiển thị ở màn hình (thành công hoặc lý do không xóa được). */
  onXong: (thongBao: ThongBao) => void;
  taiLaiDanhSach: () => Promise<void>;
};

export function XoaLoaiThietBiDialog({
  loai,
  onDong,
  onXong,
  taiLaiDanhSach,
}: XoaLoaiThietBiDialogProps) {
  const [buoc, setBuoc] = useState<"XOA" | "NGUNG">("XOA");
  const [loi, setLoi] = useState<ThongBao | null>(null);

  function xuLyLoiChung(e: ApiError) {
    if (laChuaBietKetQua(e)) return;
    if (e.maLoi === "KHONG_TIM_THAY") {
      void taiLaiDanhSach();
      onXong({
        tong: "warning",
        noiDung: THONG_DIEP_DANH_MUC.KHONG_CON_TON_TAI,
      });
      return;
    }
    setLoi(thongBaoTheoLoi(e));
  }

  const xoa = useThaoTacGhi(
    useXoaLoaiMutation({
      onSuccess: () =>
        onXong({ tong: "success", noiDung: daXoaLoai(loai.tenLoai) }),
      onError: (e) => {
        if (e.maLoi === "DA_CO_LICH_SU_CHI_NGUNG_SU_DUNG") {
          if (loai.ngungSuDung) {
            onXong({
              tong: "warning",
              noiDung: THONG_DIEP_DANH_MUC.DA_NGUNG_KHONG_XOA,
            });
          } else {
            setLoi(null);
            setBuoc("NGUNG");
          }
          return;
        }
        xuLyLoiChung(e);
      },
    }),
  );
  const ngung = useThaoTacGhi(
    useNgungSuDungLoaiMutation({
      onSuccess: () =>
        onXong({ tong: "success", noiDung: daNgungLoai(loai.tenLoai) }),
      onError: xuLyLoiChung,
    }),
  );
  const thaoTac = buoc === "XOA" ? xoa : ngung;

  const kiemTraLai = async () => {
    await taiLaiDanhSach();
    thaoTac.xacNhanDaKiemTra();
    onDong(); // Danh sách mới nhất cho biết thao tác đã có hiệu lực hay chưa.
  };

  const tomTat = (
    <KeyValueList
      dsDong={[
        { ten: "Tên loại", giaTri: loai.tenLoai },
        { ten: "Hãng sản xuất", giaTri: loai.hangSanXuat },
      ]}
    />
  );

  const thongBaoTrongHop = (
    <>
      {thaoTac.chuaBietKetQua ? (
        <Banner
          tong="warning"
          hanhDong={{ nhan: "Kiểm tra lại", onClick: () => void kiemTraLai() }}
        >
          {THONG_DIEP_DANH_MUC.CHUA_BIET_KET_QUA}
        </Banner>
      ) : null}
      {loi !== null ? (
        <Banner
          tong={loi.tong}
          {...(loi.traceId !== undefined ? { traceId: loi.traceId } : {})}
        >
          {loi.noiDung}
        </Banner>
      ) : null}
    </>
  );

  return buoc === "XOA" ? (
    <ConfirmDialog
      open
      tieuDe="Xóa loại thiết bị"
      tomTat={tomTat}
      heQua="Loại thiết bị này sẽ bị xóa khỏi danh mục."
      nhanXacNhan="Xóa"
      bienThe="destructive"
      dangXuLy={xoa.dangGui}
      loi={thongBaoTrongHop}
      onXacNhan={() => {
        setLoi(null);
        xoa.gui(loai.maLoai);
      }}
      onDong={onDong}
    />
  ) : (
    <ConfirmDialog
      open
      tieuDe="Ngưng sử dụng loại thiết bị"
      tomTat={tomTat}
      nhanManh={
        <Banner tong="warning">{THONG_DIEP_DANH_MUC.CHI_DUOC_NGUNG}</Banner>
      }
      heQua="Loại vẫn giữ lịch sử và không còn dùng cho thiết bị mới."
      nhanXacNhan="Ngưng sử dụng"
      bienThe="destructive"
      dangXuLy={ngung.dangGui}
      loi={thongBaoTrongHop}
      onXacNhan={() => {
        setLoi(null);
        ngung.gui(loai.maLoai);
      }}
      onDong={onDong}
    />
  );
}
