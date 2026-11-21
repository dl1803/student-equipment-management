"use client";

// Xóa và ngưng sử dụng thiết bị — SCR-QT-04 (UI-Screens 7.4 "như SCR-QT-03"; UF-QT-01 E2; DS 9.2;
// BR-18). Sở hữu: Thịnh.
// 1. Hộp xác nhận Xóa (hủy bỏ) nêu thiết bị sẽ bị xóa.
// 2. 409 DA_CO_LICH_SU_CHI_NGUNG_SU_DUNG: thiết bị chưa ngưng → chuyển sang hộp Ngưng sử dụng (hủy
//    bỏ), nêu thiết bị vẫn giữ lịch sử và không còn tính vào số lượng khả dụng; KHÔNG hứa hoàn tác
//    (API không có). Thiết bị đã ngưng → đóng hộp, báo ở màn hình (không đề nghị ngưng lần nữa).
// 3. Ngưng gặp 409 XUNG_DOT_CAP_NHAT: tải lại danh sách và báo trong hộp để kiểm tra rồi thử lại.
// Không tự đổi tình trạng thiết bị: tình trạng chỉ đổi qua nghiệp vụ (UF-QT-01 E2).
import { useState } from "react";
import { laChuaBietKetQua, type ApiError } from "@/shared/api/apiError";
import type { ThietBi } from "@/shared/api/types";
import { Banner } from "@/shared/components/Banner";
import { ConfirmDialog } from "@/shared/components/ConfirmDialog";
import { KeyValueList } from "@/shared/components/KeyValueList";
import { useThaoTacGhi } from "@/shared/hooks/useThaoTacGhi";
import { thongBaoTheoLoi, type ThongBao } from "@/shared/labels/thongDiepLoi";
import { useNgungSuDungThietBiMutation, useXoaThietBiMutation } from "../hooks";
import {
  THONG_DIEP_DANH_MUC,
  daNgungThietBi,
  daXoaThietBi,
} from "../thongDiep";

export type XoaThietBiDialogProps = {
  thietBi: ThietBi;
  onDong: () => void;
  /** Kết thúc với thông báo hiển thị ở màn hình (thành công hoặc lý do không xóa được). */
  onXong: (thongBao: ThongBao) => void;
  /** Tải lại danh sách; true nếu tải thành công. */
  taiLaiDanhSach: () => Promise<boolean>;
};

export function XoaThietBiDialog({
  thietBi,
  onDong,
  onXong,
  taiLaiDanhSach,
}: XoaThietBiDialogProps) {
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
    if (e.maLoi === "XUNG_DOT_CAP_NHAT") {
      void taiLaiDanhSach();
      setLoi({ tong: "warning", noiDung: THONG_DIEP_DANH_MUC.XUNG_DOT_NGUNG });
      return;
    }
    setLoi(thongBaoTheoLoi(e));
  }

  const xoa = useThaoTacGhi(
    useXoaThietBiMutation({
      onSuccess: () =>
        onXong({ tong: "success", noiDung: daXoaThietBi(thietBi.tenThietBi) }),
      onError: (e) => {
        if (e.maLoi === "DA_CO_LICH_SU_CHI_NGUNG_SU_DUNG") {
          // Danh sách có thể đã cũ (ví dụ thiết bị vừa được giao): tải lại để bảng khớp máy chủ.
          void taiLaiDanhSach();
          if (thietBi.ngungSuDung) {
            onXong({
              tong: "warning",
              noiDung: THONG_DIEP_DANH_MUC.THIET_BI_DA_NGUNG_KHONG_XOA,
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
    useNgungSuDungThietBiMutation({
      onSuccess: () =>
        onXong({
          tong: "success",
          noiDung: daNgungThietBi(thietBi.tenThietBi),
        }),
      onError: xuLyLoiChung,
    }),
  );
  const thaoTac = buoc === "XOA" ? xoa : ngung;

  // Tải lại thất bại thì giữ trạng thái chưa biết kết quả, hộp vẫn mở để kiểm tra lại sau.
  const kiemTraLai = async () => {
    if (!(await taiLaiDanhSach())) return;
    thaoTac.xacNhanDaKiemTra();
    onDong(); // Danh sách mới nhất cho biết thao tác đã có hiệu lực hay chưa.
  };

  const tomTat = (
    <KeyValueList
      dsDong={[
        { ten: "Mã thiết bị", giaTri: thietBi.maThietBi },
        { ten: "Tên thiết bị", giaTri: thietBi.tenThietBi },
        { ten: "Phòng", giaTri: thietBi.tenPhong },
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
      tieuDe="Xóa thiết bị"
      tomTat={tomTat}
      heQua="Thiết bị này sẽ bị xóa khỏi danh mục cùng ảnh và lịch sử giá trị."
      nhanXacNhan="Xóa"
      bienThe="destructive"
      dangXuLy={xoa.dangGui}
      loi={thongBaoTrongHop}
      onXacNhan={() => {
        setLoi(null);
        xoa.gui(thietBi.maThietBi);
      }}
      onDong={onDong}
    />
  ) : (
    <ConfirmDialog
      open
      tieuDe="Ngưng sử dụng thiết bị"
      tomTat={tomTat}
      nhanManh={
        <Banner tong="warning">
          {THONG_DIEP_DANH_MUC.THIET_BI_CHI_DUOC_NGUNG}
        </Banner>
      }
      heQua="Thiết bị vẫn giữ lịch sử và không còn tính vào số lượng khả dụng."
      nhanXacNhan="Ngưng sử dụng"
      bienThe="destructive"
      dangXuLy={ngung.dangGui}
      loi={thongBaoTrongHop}
      onXacNhan={() => {
        setLoi(null);
        ngung.gui(thietBi.maThietBi);
      }}
      onDong={onDong}
    />
  );
}
