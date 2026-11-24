// @vitest-environment jsdom
import { render, screen } from "@testing-library/react";
import { describe, expect, it } from "vitest";
import { FormField } from "./FormField";
import { TextField } from "./TextField";

describe("FormField + TextField", () => {
  it("CMP-04: nhãn nhìn thấy gắn với ô bằng htmlFor/id", () => {
    render(
      <FormField id="tenDangNhap" nhan="Tên đăng nhập">
        {(tt) => <TextField {...tt} />}
      </FormField>,
    );
    const o = screen.getByLabelText("Tên đăng nhập");
    expect(o.tagName).toBe("INPUT");
    expect(o.id).toBe("tenDangNhap");
  });

  it("DS 9.1: trường bắt buộc có dấu * (ẩn với trình đọc màn hình) và required trên ô", () => {
    render(
      <FormField id="matKhau" nhan="Mật khẩu" batBuoc>
        {(tt) => <TextField {...tt} type="password" />}
      </FormField>,
    );
    const sao = screen.getByText("*");
    expect(sao.getAttribute("aria-hidden")).toBe("true");
    expect(screen.getByLabelText(/Mật khẩu/)).toHaveProperty("required", true);
  });

  it("FC 5.6: gợi ý liên kết bằng aria-describedby, ô chưa lỗi thì aria-invalid=false", () => {
    render(
      <FormField id="soLuong" nhan="Số lượng" moTa="Tối đa 5">
        {(tt) => <TextField {...tt} inputMode="numeric" />}
      </FormField>,
    );
    const o = screen.getByLabelText("Số lượng");
    expect(o.getAttribute("aria-describedby")).toBe("soLuong-mo-ta");
    expect(o.getAttribute("aria-invalid")).toBe("false");
    expect(screen.getByText("Tối đa 5").id).toBe("soLuong-mo-ta");
  });

  it("FC 5.6: lỗi hiển thị dưới ô, role=alert, aria-invalid=true, liên kết bằng aria-describedby", () => {
    render(
      <FormField
        id="soLuong"
        nhan="Số lượng"
        moTa="Tối đa 5"
        loi="Phải là số nguyên"
      >
        {(tt) => <TextField {...tt} />}
      </FormField>,
    );
    const o = screen.getByLabelText("Số lượng");
    expect(o.getAttribute("aria-invalid")).toBe("true");
    expect(o.getAttribute("aria-describedby")).toBe(
      "soLuong-loi soLuong-mo-ta",
    );
    const loi = screen.getByRole("alert");
    expect(loi.id).toBe("soLuong-loi");
    expect(loi.textContent).toBe("Phải là số nguyên");
    expect(o.className).toContain("aria-invalid:border-border-error");
  });

  it("CMP-04: hậu tố đơn vị hiển thị bên phải ô", () => {
    render(
      <FormField id="thoiGian" nhan="Thời gian mượn">
        {(tt) => <TextField {...tt} hauTo="giờ" />}
      </FormField>,
    );
    expect(screen.getByText("giờ")).toBeTruthy();
  });
});
