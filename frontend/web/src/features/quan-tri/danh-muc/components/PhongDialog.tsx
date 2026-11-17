"use client";

// Biểu mẫu Thêm/Sửa phòng thiết bị — SCR-QT-02 (UI-Screens 7.2; DS CMP-26 loại Biểu mẫu).
// Sở hữu: Thịnh. Dựng khi mở, gỡ khi đóng (giá trị và lỗi tự làm mới mỗi lần mở).
// - Không có nút type="submit": Enter trong ô không tự lưu (ĐX-DS-09); lưu bằng nút Lưu.
// - 400: lỗi cạnh trường; 409 XUNG_DOT_CAP_NHAT: tải lại giá trị mới nhất vào biểu mẫu rồi sửa lại;
//   404: đóng hộp, báo ở màn hình; mất kết nối sau khi gửi: chưa xác định được kết quả.
import { useState } from "react";
import { useForm } from "react-hook-form";
import { laChuaBietKetQua, type ApiError } from "@/shared/api/apiError";
import type { PhongThietBi, PhongThietBiRequest } from "@/shared/api/types";
import { Banner } from "@/shared/components/Banner";
import { Button } from "@/shared/components/Button";
import { Dialog } from "@/shared/components/Dialog";
import { FormField } from "@/shared/components/FormField";
import { TextField } from "@/shared/components/TextField";
import { ganLoiMayChu } from "@/shared/hooks/ganLoiMayChu";
import { useThaoTacGhi } from "@/shared/hooks/useThaoTacGhi";
import { thongBaoTheoLoi, type ThongBao } from "@/shared/labels/thongDiepLoi";
import { useLuuPhongMutation } from "../hooks";
import { THONG_DIEP_DANH_MUC } from "../thongDiep";

export type PhongDialogProps = {
  /** null: thêm phòng mới. */
  phong: PhongThietBi | null;
  onDong: () => void;
  /** Tải lại danh sách và trả bản mới nhất của phòng (null nếu không còn trong trang). */
  layBanMoiNhat: (maPhong: string) => Promise<PhongThietBi | null>;
  taiLaiDanhSach: () => Promise<void>;
  /** Phòng không còn tồn tại (404 hoặc không thấy sau khi tải lại): màn hình báo và đóng hộp. */
  onKhongConTonTai: () => void;
};

function giaTriBanDau(phong: PhongThietBi | null): PhongThietBiRequest {
  return phong === null
    ? { tenPhong: "", coSo: "" }
    : { tenPhong: phong.tenPhong, coSo: phong.coSo };
}

const batBuoc = (thongDiep: string) => (v: string) =>
  v.trim() !== "" || thongDiep;

export function PhongDialog({
  phong,
  onDong,
  layBanMoiNhat,
  taiLaiDanhSach,
  onKhongConTonTai,
}: PhongDialogProps) {
  const form = useForm<PhongThietBiRequest>({
    defaultValues: giaTriBanDau(phong),
  });
  const { register, formState } = form;
  const [thongBao, setThongBao] = useState<ThongBao | null>(null);
  const [loiKhongGan, setLoiKhongGan] = useState<string[]>([]);

  async function xuLyLoi(loi: ApiError) {
    if (laChuaBietKetQua(loi)) return; // useThaoTacGhi giữ khóa và hiện nhánh chưa biết kết quả.
    if (loi.maLoi === "DU_LIEU_KHONG_HOP_LE") {
      setLoiKhongGan(ganLoiMayChu(loi, form));
      return;
    }
    if (loi.maLoi === "XUNG_DOT_CAP_NHAT" && phong !== null) {
      const banMoi = await layBanMoiNhat(phong.maPhong);
      if (banMoi === null) {
        onKhongConTonTai();
        return;
      }
      form.reset(giaTriBanDau(banMoi));
      setThongBao({ tong: "warning", noiDung: THONG_DIEP_DANH_MUC.XUNG_DOT });
      return;
    }
    if (loi.maLoi === "KHONG_TIM_THAY") {
      onKhongConTonTai();
      return;
    }
    setThongBao(thongBaoTheoLoi(loi));
  }

  const mutation = useLuuPhongMutation({
    onSuccess: onDong,
    onError: (loi) => void xuLyLoi(loi),
  });
  const thaoTac = useThaoTacGhi(mutation);

  const luu = () => {
    setThongBao(null);
    setLoiKhongGan([]);
    void form.handleSubmit((giaTri) => {
      thaoTac.gui({
        ma: phong?.maPhong ?? null,
        yeuCau: { tenPhong: giaTri.tenPhong.trim(), coSo: giaTri.coSo.trim() },
      });
    })();
  };

  const kiemTraLai = async () => {
    await taiLaiDanhSach();
    thaoTac.xacNhanDaKiemTra();
  };

  return (
    <Dialog
      open
      tieuDe={phong === null ? "Thêm phòng" : "Sửa phòng thiết bị"}
      kichThuoc="md"
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
      <form
        noValidate
        onSubmit={(suKien) => suKien.preventDefault()}
        className="flex flex-col gap-field"
      >
        <p className="type-caption text-text-secondary">* Bắt buộc</p>
        <FormField
          id="tenPhong"
          nhan="Tên phòng"
          batBuoc
          {...(formState.errors.tenPhong?.message !== undefined
            ? { loi: formState.errors.tenPhong.message }
            : {})}
        >
          {(tt) => (
            <TextField
              {...tt}
              {...register("tenPhong", {
                // PhongThietBiRequest.tenPhong: required, minLength 1
                validate: batBuoc("Nhập tên phòng"),
              })}
            />
          )}
        </FormField>
        <FormField
          id="coSo"
          nhan="Cơ sở"
          batBuoc
          {...(formState.errors.coSo?.message !== undefined
            ? { loi: formState.errors.coSo.message }
            : {})}
        >
          {(tt) => (
            <TextField
              {...tt}
              {...register("coSo", {
                // PhongThietBiRequest.coSo: required, minLength 1
                validate: batBuoc("Nhập cơ sở"),
              })}
            />
          )}
        </FormField>
      </form>
    </Dialog>
  );
}
