"use client";

// Hộp Cập nhật giá trị thiết bị — SCR-QT-04 (UI-Screens 7.4; UF-QT-01 bước 4; Wireframes SCR-QT-04
// hộp nhỏ bên phải; DS CMP-26 hộp Biểu mẫu, nút lưu là nút chính). Sở hữu: Thịnh.
// - Gọi PUT /quan-tri/thiet-bi/{ma}/gia-tri (endpoint riêng, không qua biểu mẫu Sửa).
// - Giá trị hiện hành chỉ đọc; ô Giá trị mới bắt buộc, số nguyên không âm (GiaTriThietBiRequest:
//   integer, minimum 0). Nêu rằng giá trị hiện hành dùng để tính bồi thường (BR-17).
// - Giá trị mới sau khi lưu lấy từ phản hồi máy chủ; client không tự tính.
// - Không có nút type="submit": Enter không tự lưu (ĐX-DS-09).
// - Lỗi: 400 cạnh ô; 404 đóng hộp, báo ở nơi mở; mất kết quả → "Kiểm tra lại" (FA 3.11).
import { useEffect, useState } from "react";
import { useForm } from "react-hook-form";
import { laChuaBietKetQua, type ApiError } from "@/shared/api/apiError";
import type { GiaTriThietBi, ThietBi } from "@/shared/api/types";
import { Banner } from "@/shared/components/Banner";
import { Button } from "@/shared/components/Button";
import { Dialog } from "@/shared/components/Dialog";
import { FormField } from "@/shared/components/FormField";
import { KeyValueList } from "@/shared/components/KeyValueList";
import { TextField } from "@/shared/components/TextField";
import { docSoNguyen } from "@/shared/format/docSo";
import { formatTien } from "@/shared/format/formatSo";
import { ganLoiMayChu } from "@/shared/hooks/ganLoiMayChu";
import { useThaoTacGhi } from "@/shared/hooks/useThaoTacGhi";
import { thongBaoTheoLoi, type ThongBao } from "@/shared/labels/thongDiepLoi";
import { useCapNhatGiaTriThietBiMutation } from "../hooks";
import { THONG_DIEP_DANH_MUC } from "../thongDiep";

type GiaTriBieuMau = { giaTri: string };

export type CapNhatGiaTriDialogProps = {
  thietBi: ThietBi;
  onDong: () => void;
  /** Lưu thành công; banGhi là bản ghi giá trị mới do máy chủ trả về. */
  onXong: (banGhi: GiaTriThietBi) => void;
  /** Tải lại danh sách; true nếu tải thành công. */
  taiLaiDanhSach: () => Promise<boolean>;
  /** Máy chủ trả 404: nơi mở báo và đóng hộp. */
  onKhongConTonTai: () => void;
};

export function CapNhatGiaTriDialog({
  thietBi,
  onDong,
  onXong,
  taiLaiDanhSach,
  onKhongConTonTai,
}: CapNhatGiaTriDialogProps) {
  const form = useForm<GiaTriBieuMau>({ defaultValues: { giaTri: "" } });
  const { register, formState } = form;
  const [thongBao, setThongBao] = useState<ThongBao | null>(null);
  const [loiKhongGan, setLoiKhongGan] = useState<string[]>([]);

  // Hộp Biểu mẫu: tiêu điểm ban đầu ở ô nhập (DS CMP-26). Chạy sau showModal của Dialog.
  useEffect(() => {
    form.setFocus("giaTri");
  }, [form]);

  function xuLyLoi(loi: ApiError) {
    if (laChuaBietKetQua(loi)) return;
    if (loi.maLoi === "DU_LIEU_KHONG_HOP_LE") {
      setLoiKhongGan(ganLoiMayChu(loi, form));
      return;
    }
    if (loi.maLoi === "KHONG_TIM_THAY") {
      onKhongConTonTai();
      return;
    }
    setThongBao(thongBaoTheoLoi(loi));
  }

  const thaoTac = useThaoTacGhi(
    useCapNhatGiaTriThietBiMutation({ onSuccess: onXong, onError: xuLyLoi }),
  );

  const luu = () => {
    setThongBao(null);
    setLoiKhongGan([]);
    void form.handleSubmit((gt) => {
      const giaTri = docSoNguyen(gt.giaTri);
      if (giaTri === null) return; // Đã chặn bởi validate; giữ cho chắc kiểu.
      thaoTac.gui({ ma: thietBi.maThietBi, yeuCau: { giaTri } });
    })();
  };

  // Tải lại thất bại thì giữ trạng thái chưa biết kết quả, hộp vẫn mở để kiểm tra lại sau.
  const kiemTraLai = async () => {
    if (!(await taiLaiDanhSach())) return;
    thaoTac.xacNhanDaKiemTra();
    onDong(); // Danh sách mới nhất cho biết giá trị đã đổi hay chưa.
  };

  const loiGiaTri = formState.errors.giaTri?.message;

  return (
    <Dialog
      open
      tieuDe="Cập nhật giá trị"
      khoaDong={thaoTac.dangGui}
      onDong={onDong}
      hangNut={
        <>
          <Button onClick={onDong} disabled={thaoTac.dangGui}>
            Hủy
          </Button>
          <Button
            bienThe="primary"
            onClick={luu}
            dangXuLy={thaoTac.dangGui}
            disabled={thaoTac.chuaBietKetQua}
          >
            Lưu
          </Button>
        </>
      }
    >
      {thaoTac.chuaBietKetQua ? (
        <Banner
          tong="warning"
          hanhDong={{ nhan: "Kiểm tra lại", onClick: () => void kiemTraLai() }}
        >
          {THONG_DIEP_DANH_MUC.CHUA_BIET_KET_QUA}
        </Banner>
      ) : null}
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
      {loiKhongGan.length > 0 ? (
        <Banner tong="danger" dsLyDo={loiKhongGan} />
      ) : null}
      <KeyValueList
        dsDong={[
          { ten: "Thiết bị", giaTri: thietBi.tenThietBi },
          { ten: "Mã thiết bị", giaTri: thietBi.maThietBi },
          {
            ten: "Giá trị hiện hành",
            giaTri: formatTien(thietBi.giaTriHienHanh),
          },
        ]}
      />
      <form
        noValidate
        onSubmit={(suKien) => suKien.preventDefault()}
        className="flex flex-col gap-field"
      >
        <FormField
          id="giaTriMoi"
          nhan="Giá trị mới"
          batBuoc
          moTa={THONG_DIEP_DANH_MUC.GIA_TRI_DUNG_BOI_THUONG}
          {...(loiGiaTri !== undefined ? { loi: loiGiaTri } : {})}
        >
          {(tt) => (
            <TextField
              {...tt}
              {...register("giaTri", {
                // GiaTriThietBiRequest.giaTri: required, integer int64, minimum 0
                validate: (v) =>
                  docSoNguyen(v) !== null || "Nhập số nguyên không âm",
              })}
              inputMode="numeric"
              hauTo="VNĐ"
            />
          )}
        </FormField>
      </form>
    </Dialog>
  );
}
