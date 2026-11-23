"use client";

// Hộp Từ chối phiếu yêu cầu — SCR-NV-03 (UI-Screens 6.3; UF-NV-01 A1, A2; ĐX-UF-05; DS CMP-26).
// Sở hữu: Thịnh.
// - Ô lý do bắt buộc, không để trống (TuChoiRequest.lyDo: minLength 1, pattern \S); gửi đã cắt khoảng
//   trắng hai đầu. Từ chối được cả phiếu đủ điều kiện (A2).
// - Không có nút type="submit": Enter trong ô không tự gửi (ĐX-DS-09). Tiêu điểm ban đầu ở ô lý do.
// - Chống bấm đúp: useThaoTacGhi (FA 3.11). 400 gắn lỗi vào ô lý do. XUNG_DOT_CAP_NHAT: GIỮ hộp và lý
//   đã nhập, màn hình tải lại kết quả kiểm tra. E1, E2, chưa biết kết quả: chuyển lên màn hình.
import { useEffect, useState } from "react";
import { useForm } from "react-hook-form";
import type { PhieuYeuCauMuonQuay } from "@/shared/api/types";
import { Banner } from "@/shared/components/Banner";
import { Button } from "@/shared/components/Button";
import { Dialog } from "@/shared/components/Dialog";
import { FormField } from "@/shared/components/FormField";
import { TextArea } from "@/shared/components/TextArea";
import { ganLoiMayChu } from "@/shared/hooks/ganLoiMayChu";
import { useThaoTacGhi } from "@/shared/hooks/useThaoTacGhi";
import type { ThongBao } from "@/shared/labels/thongDiepLoi";
import { useTuChoiPhieuMutation } from "../hooks";
import { phanLoaiLoiGhi, type LoiChoManHinh } from "../phanLoaiLoiGhi";
import { THONG_DIEP_PHIEU_YEU_CAU } from "../thongDiep";
import { TomTatPhieu } from "./TomTatPhieu";

type GiaTriBieuMau = { lyDo: string };

export type TuChoiPhieuDialogProps = {
  phieu: PhieuYeuCauMuonQuay;
  onDong: () => void;
  onXong: (phieuSauTuChoi: PhieuYeuCauMuonQuay) => void;
  /** E1, E2, chưa biết kết quả: màn hình đóng hộp và xử lý. */
  onLoi: (loi: LoiChoManHinh) => void;
  /** Xung đột: màn hình tải lại kết quả kiểm tra, hộp vẫn mở. */
  onTaiLaiKiemTra: () => void;
};

export function TuChoiPhieuDialog({
  phieu,
  onDong,
  onXong,
  onLoi,
  onTaiLaiKiemTra,
}: TuChoiPhieuDialogProps) {
  const form = useForm<GiaTriBieuMau>({ defaultValues: { lyDo: "" } });
  const { register, formState } = form;
  const [loi, setLoi] = useState<ThongBao | null>(null);
  const [loiKhongGan, setLoiKhongGan] = useState<string[]>([]);

  // Hộp Biểu mẫu: tiêu điểm ban đầu ở ô nhập (DS CMP-26). Chạy sau showModal của Dialog.
  useEffect(() => {
    form.setFocus("lyDo");
  }, [form]);

  const thaoTac = useThaoTacGhi(
    useTuChoiPhieuMutation({
      onSuccess: onXong,
      onError: (e) => {
        if (e.maLoi === "DU_LIEU_KHONG_HOP_LE") {
          setLoiKhongGan(ganLoiMayChu(e, form));
          return;
        }
        const kq = phanLoaiLoiGhi(e, "TU_CHOI");
        if (kq.loai === "TRONG_HOP") {
          setLoi(kq.thongBao);
        } else if (kq.loai === "TAI_LAI_KIEM_TRA") {
          setLoi(kq.thongBao);
          onTaiLaiKiemTra();
        } else {
          onLoi(kq);
        }
      },
    }),
  );

  const tuChoi = () => {
    setLoi(null);
    setLoiKhongGan([]);
    void form.handleSubmit((gt) => {
      thaoTac.gui({
        maYeuCau: phieu.maYeuCau,
        yeuCau: { lyDo: gt.lyDo.trim() },
      });
    })();
  };

  const loiLyDo = formState.errors.lyDo?.message;

  return (
    <Dialog
      open
      tieuDe="Từ chối phiếu yêu cầu"
      kichThuoc="md"
      khoaDong={thaoTac.dangGui}
      onDong={onDong}
      hangNut={
        <>
          <Button onClick={onDong} disabled={thaoTac.dangGui}>
            Hủy
          </Button>
          <Button
            bienThe="destructive"
            onClick={tuChoi}
            dangXuLy={thaoTac.dangGui}
          >
            Từ chối
          </Button>
        </>
      }
    >
      <TomTatPhieu phieu={phieu} />
      <p className="type-body">{THONG_DIEP_PHIEU_YEU_CAU.HE_QUA_TU_CHOI}</p>
      {loi !== null ? (
        <Banner
          tong={loi.tong}
          {...(loi.traceId !== undefined ? { traceId: loi.traceId } : {})}
        >
          {loi.noiDung}
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
        <FormField
          id="lyDo"
          nhan="Lý do từ chối"
          batBuoc
          {...(loiLyDo !== undefined ? { loi: loiLyDo } : {})}
        >
          {(tt) => (
            <TextArea
              {...tt}
              {...register("lyDo", {
                // TuChoiRequest.lyDo: required, minLength 1, pattern \S
                validate: (v) =>
                  v.trim() !== "" || THONG_DIEP_PHIEU_YEU_CAU.NHAP_LY_DO,
              })}
            />
          )}
        </FormField>
      </form>
    </Dialog>
  );
}
