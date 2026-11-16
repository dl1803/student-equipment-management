"use client";

// SCR-CM-02 Đăng nhập nhân viên hoặc quản trị viên (UI-Screens 4.5; FA 4.5, 4.8; FC 5.4, 5.6).
// Sở hữu: Thịnh.
// - Mật khẩu không vào useState, URL hay thông báo lỗi. Sau khi có kết quả (thành công hoặc lỗi):
//   reset() mutation (bỏ variables chứa mật khẩu) và resetField("matKhau"); giữ tên đăng nhập.
// - Không có liên kết đăng ký, quên hoặc đổi mật khẩu (UI-Screens 2.1).
// - Đăng nhập mất phản hồi được thử lại bằng tay, không tự retry, không đếm số lần còn lại (ĐX-FA-22).
// - Thành công: AuthProvider vào phiên; AuthGate chuyển tới trang chủ của vai trò.
import { useRef, useState, type FormEvent } from "react";
import { useForm, useWatch } from "react-hook-form";
import type { ApiError } from "@/shared/api/apiError";
import type { DangNhapNhanVienRequest } from "@/shared/api/types";
import { Banner } from "@/shared/components/Banner";
import { Button } from "@/shared/components/Button";
import { FormField } from "@/shared/components/FormField";
import { TextField } from "@/shared/components/TextField";
import { ganLoiMayChu } from "@/shared/hooks/ganLoiMayChu";
import { useAuth } from "@/shared/hooks/useAuth";
import { useDangNhapNhanVienMutation } from "./hooks";
import {
  thongBaoKetThucPhien,
  thongBaoLoiDangNhap,
  type ThongBao,
} from "./thongDiep";

const MAC_DINH: DangNhapNhanVienRequest = { tenDangNhap: "", matKhau: "" };

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
    <main className="flex min-h-screen items-center justify-center bg-surface-subtle px-gutter">
      <div className="flex w-full max-w-web-login-width flex-col gap-4 rounded-lg border border-border-subtle bg-surface-default p-6">
        <h1 className="type-title-screen text-text-primary">Đăng nhập</h1>
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
              <TextField
                {...tt}
                {...register("tenDangNhap", { required: true })}
                autoComplete="username"
              />
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
              <TextField
                {...tt}
                {...register("matKhau", { required: true })}
                type="password"
                autoComplete="current-password"
              />
            )}
          </FormField>
          <Button
            type="submit"
            bienThe="primary"
            dangXuLy={dangNhap.isPending}
            disabled={thieuTruong}
          >
            Đăng nhập
          </Button>
        </form>
      </div>
    </main>
  );
}
