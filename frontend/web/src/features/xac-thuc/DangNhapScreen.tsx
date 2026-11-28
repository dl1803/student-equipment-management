"use client";

// SCR-CM-02 Đăng nhập nhân viên hoặc quản trị viên (UI-Screens 4.5; FA 4.5, 4.8; FC 5.4, 5.6).
// Sở hữu: Thịnh.
// - Mật khẩu không vào useState, URL hay thông báo lỗi. Sau khi có kết quả (thành công hoặc lỗi):
//   reset() mutation (bỏ variables chứa mật khẩu) và resetField("matKhau"); giữ tên đăng nhập.
// - Không có liên kết đăng ký, quên hoặc đổi mật khẩu (UI-Screens 2.1).
// - Đăng nhập mất phản hồi được thử lại bằng tay, không tự retry, không đếm số lần còn lại (ĐX-FA-22).
// - Thành công: AuthProvider vào phiên; AuthGate chuyển tới trang chủ của vai trò.
// - Bố cục (Wireframes SCR-CM-02, DS 11.2, ĐX-DS-17, ĐX-DS-18): hai cột bằng nhau. Trái: logo và tên
//   hệ thống ở trên, biểu mẫu rộng layout.web.login-width giữa cột, chân trang ở dưới. Ô nhập có biểu
//   tượng person/lock ở đầu (trang trí, ẩn với trình đọc màn hình); ô mật khẩu có nút Hiện/Ẩn mật khẩu
//   (aria-pressed). Trạng thái hiện/ẩn là state của màn hình, không lưu và không chứa mật khẩu. Phải:
//   khối nền color.text.link, chữ color.text.inverse, ba dòng giới thiệu và hình minh họa SVG.
import { useRef, useState, type FormEvent } from "react";
import { useForm, useWatch } from "react-hook-form";
import type { ApiError } from "@/shared/api/apiError";
import type { DangNhapNhanVienRequest } from "@/shared/api/types";
import { Banner } from "@/shared/components/Banner";
import { Button } from "@/shared/components/Button";
import { FormField } from "@/shared/components/FormField";
import { IconButton } from "@/shared/components/IconButton";
import { LogoUngDung } from "@/shared/components/LogoUngDung";
import { IconCheck } from "@/shared/components/icons/IconCheck";
import { IconLock } from "@/shared/components/icons/IconLock";
import { IconPerson } from "@/shared/components/icons/IconPerson";
import { IconVisibility } from "@/shared/components/icons/IconVisibility";
import { IconVisibilityOff } from "@/shared/components/icons/IconVisibilityOff";
import { TextField } from "@/shared/components/TextField";
import { ganLoiMayChu } from "@/shared/hooks/ganLoiMayChu";
import { useAuth } from "@/shared/hooks/useAuth";
import { HinhMinhHoaDangNhap } from "./HinhMinhHoaDangNhap";
import { useDangNhapNhanVienMutation } from "./hooks";
import {
  thongBaoKetThucPhien,
  thongBaoLoiDangNhap,
  type ThongBao,
} from "./thongDiep";

const MAC_DINH: DangNhapNhanVienRequest = { tenDangNhap: "", matKhau: "" };

const TEN_HE_THONG = "Quản lý mượn trả thiết bị";

const DIEM_NOI_BAT = [
  "Duyệt phiếu yêu cầu và giao, nhận thiết bị tại quầy",
  "Theo dõi phiếu quá hạn, biên bản và phí phạt",
  "Quản lý danh mục, cấu hình và báo cáo",
];

function KhoiThongBao({ thongBao }: { thongBao: ThongBao }) {
  return (
    <Banner
      tong={thongBao.tong}
      {...(thongBao.traceId !== undefined ? { traceId: thongBao.traceId } : {})}
    >
      {thongBao.noiDung}
    </Banner>
  );
}

export function DangNhapScreen() {
  const { trangThai, hoanTatDangNhap } = useAuth();
  const form = useForm<DangNhapNhanVienRequest>({ defaultValues: MAC_DINH });
  const { register, handleSubmit, resetField, control, formState } = form;
  const [tenDangNhap, matKhau] = useWatch({
    control,
    name: ["tenDangNhap", "matKhau"],
  });
  const dangNhap = useDangNhapNhanVienMutation();
  const khoaGuiRef = useRef(false);
  const [daThuDangNhap, setDaThuDangNhap] = useState(false);
  const [loi, setLoi] = useState<ApiError | null>(null);
  const [loiKhongGanTruong, setLoiKhongGanTruong] = useState<string[]>([]);
  const [hienMatKhau, setHienMatKhau] = useState(false);

  const thongBaoPhien =
    !daThuDangNhap && trangThai.loai === "CHUA_DANG_NHAP"
      ? thongBaoKetThucPhien(trangThai.lyDo, trangThai.loi)
      : null;

  const guiDangNhap = (giaTri: DangNhapNhanVienRequest) => {
    if (khoaGuiRef.current) return;
    khoaGuiRef.current = true;
    setDaThuDangNhap(true);
    setLoi(null);
    setLoiKhongGanTruong([]);
    dangNhap.mutate(giaTri, {
      onSuccess: (nguoiDung) => hoanTatDangNhap(nguoiDung),
      onError: (e) => {
        if (e.maLoi === "DU_LIEU_KHONG_HOP_LE") {
          setLoiKhongGanTruong(ganLoiMayChu(e, form));
        } else {
          setLoi(e);
        }
      },
      onSettled: () => {
        khoaGuiRef.current = false;
        dangNhap.reset();
        // Xóa giá trị mật khẩu nhưng giữ lỗi máy chủ (nếu có) vừa gắn vào trường này.
        resetField("matKhau", { keepError: true });
      },
    });
  };

  // handleSubmit chỉ được gọi trong sự kiện submit, không trong lúc dựng (react-hooks/refs).
  const handleSubmitForm = (suKien: FormEvent<HTMLFormElement>) => {
    void handleSubmit(guiDangNhap)(suKien);
  };

  const thieuTruong = tenDangNhap.trim() === "" || matKhau === "";

  return (
    <main className="grid min-h-screen grid-cols-2 bg-surface-default">
      <div className="flex flex-col gap-8 p-12">
        {/* Tên hệ thống nằm trong luồng bố cục (không absolute) để biểu mẫu cao lên — nhiều thông báo,
            phóng to trang — không đè lên chữ. */}
        <div className="flex shrink-0 items-center gap-3">
          <LogoUngDung className="size-control-md shrink-0" />
          <p className="type-title-card font-bold text-text-primary">
            {TEN_HE_THONG}
          </p>
        </div>
        <div className="flex flex-1 items-center justify-center">
          <div className="flex w-full max-w-web-login-width flex-col gap-6">
            <div className="flex flex-col gap-2">
              <h1 className="type-title-screen text-text-primary">
                Chào mừng trở lại
              </h1>
              <p className="type-body text-text-secondary">
                Đăng nhập để tiếp tục. Dành cho nhân viên quản lý thiết bị và
                quản trị viên.
              </p>
            </div>
            {thongBaoPhien !== null ? (
              <KhoiThongBao thongBao={thongBaoPhien} />
            ) : null}
            {loi !== null ? (
              <KhoiThongBao thongBao={thongBaoLoiDangNhap(loi)} />
            ) : null}
            {loiKhongGanTruong.length > 0 ? (
              <Banner tong="danger" dsLyDo={loiKhongGanTruong} />
            ) : null}
            <form
              noValidate
              onSubmit={handleSubmitForm}
              className="flex flex-col gap-field"
            >
              <p className="type-caption text-text-secondary">* Bắt buộc</p>
              <FormField
                id="tenDangNhap"
                nhan="Tên đăng nhập"
                batBuoc
                {...(formState.errors.tenDangNhap?.message !== undefined
                  ? { loi: formState.errors.tenDangNhap.message }
                  : {})}
              >
                {(tt) => (
                  <div className="relative">
                    <IconPerson
                      kichThuoc="md"
                      className="pointer-events-none absolute top-1/2 left-4 -translate-y-1/2 text-text-secondary"
                    />
                    <TextField
                      {...tt}
                      {...register("tenDangNhap", { required: true })}
                      autoComplete="username"
                      placeholder="Ví dụ: nv.huong"
                      className="h-control-lg pl-12"
                    />
                  </div>
                )}
              </FormField>
              <FormField
                id="matKhau"
                nhan="Mật khẩu"
                batBuoc
                {...(formState.errors.matKhau?.message !== undefined
                  ? { loi: formState.errors.matKhau.message }
                  : {})}
              >
                {(tt) => (
                  <div className="relative">
                    <IconLock
                      kichThuoc="md"
                      className="pointer-events-none absolute top-1/2 left-4 -translate-y-1/2 text-text-secondary"
                    />
                    <TextField
                      {...tt}
                      {...register("matKhau", { required: true })}
                      type={hienMatKhau ? "text" : "password"}
                      autoComplete="current-password"
                      className="h-control-lg pr-12 pl-12"
                    />
                    <IconButton
                      nhanTruyCap={
                        hienMatKhau ? "Ẩn mật khẩu" : "Hiện mật khẩu"
                      }
                      aria-pressed={hienMatKhau}
                      bieuTuong={
                        hienMatKhau ? (
                          <IconVisibilityOff kichThuoc="md" />
                        ) : (
                          <IconVisibility kichThuoc="md" />
                        )
                      }
                      onClick={() => setHienMatKhau((v) => !v)}
                      className="absolute top-1/2 right-1 -translate-y-1/2"
                    />
                  </div>
                )}
              </FormField>
              <Button
                type="submit"
                bienThe="primary"
                dangXuLy={dangNhap.isPending}
                disabled={thieuTruong}
                className="mt-2 h-control-lg w-full"
              >
                Đăng nhập
              </Button>
            </form>
          </div>
        </div>
        <p className="shrink-0 type-caption text-text-secondary">
          © 2026 Nhóm 13 · Hệ thống quản lý mượn trả thiết bị học tập
        </p>
      </div>
      <div className="m-4 flex flex-col justify-between gap-8 overflow-hidden rounded-lg bg-text-link p-12">
        <div className="flex flex-col gap-4">
          <h2 className="type-title-screen text-text-inverse">
            Mượn trả thiết bị nhanh, rõ ràng
          </h2>
          <ul className="flex flex-col gap-3">
            {DIEM_NOI_BAT.map((d) => (
              <li
                key={d}
                className="flex items-center gap-3 type-body text-text-inverse"
              >
                <span className="grid size-icon-lg shrink-0 place-items-center rounded-full bg-nav-selected-bg text-nav-selected-text">
                  <IconCheck kichThuoc="sm" />
                </span>
                {d}
              </li>
            ))}
          </ul>
        </div>
        <div className="flex justify-center">
          <HinhMinhHoaDangNhap />
        </div>
      </div>
    </main>
  );
}
