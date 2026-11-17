"use client";

// Biểu mẫu Thêm/Sửa loại thiết bị — SCR-QT-03 (UI-Screens 7.3; DS CMP-26 loại Biểu mẫu).
// Sở hữu: Thịnh.
// - Phí trả trễ mỗi ngày chỉ nhập khi thêm; khi sửa hiển thị chỉ đọc kèm liên kết tới SCR-QT-06
//   (Long phụ trách) và không gửi trong yêu cầu sửa (LoaiThietBiCapNhatRequest không có phí).
// - Kiểm tra phía client chỉ theo openapi.yaml: tên bắt buộc, chu kỳ số nguyên ≥ 1, phí số nguyên
//   ≥ 0 (FC 5.6). Lỗi khác như PhongDialog: 400 cạnh trường, 409 tải lại, 404 đóng hộp.
import { useState } from "react";
import { useForm } from "react-hook-form";
import { laChuaBietKetQua, type ApiError } from "@/shared/api/apiError";
import type { LoaiThietBi } from "@/shared/api/types";
import { Banner } from "@/shared/components/Banner";
import { Button } from "@/shared/components/Button";
import { Dialog } from "@/shared/components/Dialog";
import { FormField } from "@/shared/components/FormField";
import { TextField } from "@/shared/components/TextField";
import { TextLink } from "@/shared/components/TextLink";
import { docSoNguyen } from "@/shared/format/docSo";
import { formatTien } from "@/shared/format/formatSo";
import { ganLoiMayChu } from "@/shared/hooks/ganLoiMayChu";
import { useThaoTacGhi } from "@/shared/hooks/useThaoTacGhi";
import { thongBaoTheoLoi, type ThongBao } from "@/shared/labels/thongDiepLoi";
import { tuyenCua } from "@/shared/tuyen/banDoTuyen";
import { useLuuLoaiMutation } from "../hooks";
import { THONG_DIEP_DANH_MUC } from "../thongDiep";

/** Giá trị biểu mẫu: số nhập dạng chuỗi, đổi bằng docSoNguyen trước khi gửi (FC 5.6). */
type GiaTriLoai = {
  tenLoai: string;
  hangSanXuat: string;
  moTa: string;
  chuKyBaoTri: string;
  phiTreMoiNgay: string;
};

export type LoaiThietBiDialogProps = {
  /** null: thêm loại mới. */
  loai: LoaiThietBi | null;
  onDong: () => void;
  layBanMoiNhat: (maLoai: string) => Promise<LoaiThietBi | null>;
  /** Tải lại danh sách; true nếu tải thành công. */
  taiLaiDanhSach: () => Promise<boolean>;
  onKhongConTonTai: () => void;
};

function giaTriBanDau(loai: LoaiThietBi | null): GiaTriLoai {
  return {
    tenLoai: loai?.tenLoai ?? "",
    hangSanXuat: loai?.hangSanXuat ?? "",
    moTa: loai?.moTa ?? "",
    chuKyBaoTri: loai === null ? "" : String(loai.chuKyBaoTri),
    phiTreMoiNgay: loai === null ? "" : String(loai.phiTreMoiNgay),
  };
}

/** Chuỗi tùy chọn: rỗng sau khi cắt khoảng trắng thì gửi null. */
function hoacNull(chuoi: string): string | null {
  const giaTri = chuoi.trim();
  return giaTri === "" ? null : giaTri;
}

const TUYEN_PHI_TRE = tuyenCua("SCR-QT-06");

export function LoaiThietBiDialog({
  loai,
  onDong,
  layBanMoiNhat,
  taiLaiDanhSach,
  onKhongConTonTai,
}: LoaiThietBiDialogProps) {
  const laThem = loai === null;
  const form = useForm<GiaTriLoai>({ defaultValues: giaTriBanDau(loai) });
  const { register, formState } = form;
  const [thongBao, setThongBao] = useState<ThongBao | null>(null);
  const [loiKhongGan, setLoiKhongGan] = useState<string[]>([]);
  // Phí chỉ đọc khi sửa: lấy theo bản mới nhất sau xung đột 409, không theo prop ban đầu.
  const [phiHienTai, setPhiHienTai] = useState(loai?.phiTreMoiNgay ?? null);

  async function xuLyLoi(loi: ApiError) {
    if (laChuaBietKetQua(loi)) return;
    if (loi.maLoi === "DU_LIEU_KHONG_HOP_LE") {
      setLoiKhongGan(ganLoiMayChu(loi, form));
      return;
    }
    if (loi.maLoi === "XUNG_DOT_CAP_NHAT" && loai !== null) {
      const banMoi = await layBanMoiNhat(loai.maLoai);
      if (banMoi === null) {
        // Không thấy trong trang hiện tại chưa chắc đã bị xóa: giữ dữ liệu đang nhập (xem PhongDialog).
        setThongBao({
          tong: "warning",
          noiDung: THONG_DIEP_DANH_MUC.XUNG_DOT_KHONG_NAP,
        });
        return;
      }
      form.reset(giaTriBanDau(banMoi));
      setPhiHienTai(banMoi.phiTreMoiNgay);
      setThongBao({ tong: "warning", noiDung: THONG_DIEP_DANH_MUC.XUNG_DOT });
      return;
    }
    if (loi.maLoi === "KHONG_TIM_THAY") {
      onKhongConTonTai();
      return;
    }
    setThongBao(thongBaoTheoLoi(loi));
  }

  const mutation = useLuuLoaiMutation({
    onSuccess: onDong,
    onError: (loi) => void xuLyLoi(loi),
  });
  const thaoTac = useThaoTacGhi(mutation);

  const luu = () => {
    setThongBao(null);
    setLoiKhongGan([]);
    void form.handleSubmit((giaTri) => {
      const chuKyBaoTri = docSoNguyen(giaTri.chuKyBaoTri);
      if (chuKyBaoTri === null) return; // Đã chặn bởi validate; giữ cho chắc kiểu.
      const chung = {
        tenLoai: giaTri.tenLoai.trim(),
        hangSanXuat: hoacNull(giaTri.hangSanXuat),
        moTa: hoacNull(giaTri.moTa),
        chuKyBaoTri,
      };
      if (laThem) {
        const phiTreMoiNgay = docSoNguyen(giaTri.phiTreMoiNgay);
        if (phiTreMoiNgay === null) return;
        thaoTac.gui({ ma: null, yeuCau: { ...chung, phiTreMoiNgay } });
      } else {
        thaoTac.gui({ ma: loai.maLoai, yeuCau: chung });
      }
    })();
  };

  // Tải lại thất bại thì giữ trạng thái chưa biết kết quả để người dùng kiểm tra lại sau.
  const kiemTraLai = async () => {
    if (await taiLaiDanhSach()) thaoTac.xacNhanDaKiemTra();
  };

  const loiCua = (truong: keyof GiaTriLoai) => {
    const thongDiep = formState.errors[truong]?.message;
    return thongDiep !== undefined ? { loi: thongDiep } : {};
  };

  return (
    <Dialog
      open
      tieuDe={laThem ? "Thêm loại" : "Sửa loại thiết bị"}
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
        <FormField id="tenLoai" nhan="Tên loại" batBuoc {...loiCua("tenLoai")}>
          {(tt) => (
            <TextField
              {...tt}
              {...register("tenLoai", {
                // LoaiThietBi*Request.tenLoai: required, minLength 1
                validate: (v) => v.trim() !== "" || "Nhập tên loại",
              })}
            />
          )}
        </FormField>
        <FormField
          id="hangSanXuat"
          nhan="Hãng sản xuất"
          {...loiCua("hangSanXuat")}
        >
          {(tt) => <TextField {...tt} {...register("hangSanXuat")} />}
        </FormField>
        <FormField id="moTa" nhan="Mô tả" {...loiCua("moTa")}>
          {(tt) => <TextField {...tt} {...register("moTa")} />}
        </FormField>
        <FormField
          id="chuKyBaoTri"
          nhan="Chu kỳ bảo trì"
          batBuoc
          {...loiCua("chuKyBaoTri")}
        >
          {(tt) => (
            <TextField
              {...tt}
              {...register("chuKyBaoTri", {
                // LoaiThietBi*Request.chuKyBaoTri: required, integer, minimum 1
                validate: (v) => {
                  const so = docSoNguyen(v);
                  return (so !== null && so >= 1) || "Nhập số nguyên từ 1";
                },
              })}
              inputMode="numeric"
              hauTo="ngày"
            />
          )}
        </FormField>
        {laThem ? (
          <FormField
            id="phiTreMoiNgay"
            nhan="Phí trả trễ mỗi ngày"
            batBuoc
            {...loiCua("phiTreMoiNgay")}
          >
            {(tt) => (
              <TextField
                {...tt}
                {...register("phiTreMoiNgay", {
                  // LoaiThietBiTaoMoiRequest.phiTreMoiNgay: required, integer, minimum 0
                  validate: (v) =>
                    docSoNguyen(v) !== null || "Nhập số nguyên không âm",
                })}
                inputMode="numeric"
                hauTo="VNĐ"
              />
            )}
          </FormField>
        ) : (
          <div className="flex flex-col gap-field-label">
            <FormField id="phiTreMoiNgay" nhan="Phí trả trễ mỗi ngày">
              {(tt) => (
                <TextField {...tt} readOnly value={formatTien(phiHienTai)} />
              )}
            </FormField>
            <div>
              <TextLink href={TUYEN_PHI_TRE}>
                Đổi phí trả trễ ở Cấu hình
              </TextLink>
            </div>
          </div>
        )}
      </form>
    </Dialog>
  );
}
