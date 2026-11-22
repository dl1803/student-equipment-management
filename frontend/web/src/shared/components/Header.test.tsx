// @vitest-environment jsdom
import { fireEvent, render, screen } from "@testing-library/react";
import { describe, expect, it, vi } from "vitest";
import { Header } from "./Header";

describe("Header", () => {
  it("CMP-30: hiện họ tên, vai trò bằng chữ và nút Đăng xuất (không có hộp xác nhận)", () => {
    const onDangXuat = vi.fn();
    render(
      <Header
        hoTen="Nguyễn Thị Hương"
        nhanVaiTro="Nhân viên quản lý thiết bị"
        onDangXuat={onDangXuat}
      />,
    );

    expect(screen.getByRole("banner").textContent).toContain(
      "Nguyễn Thị Hương",
    );
    expect(screen.getByText("Nhân viên quản lý thiết bị")).toBeTruthy();
    fireEvent.click(screen.getByRole("button", { name: "Đăng xuất" }));
    expect(onDangXuat).toHaveBeenCalledTimes(1);
    expect(document.querySelector("dialog")).toBeNull();
  });

  it("Wireframes 2.2/2.3: tên hệ thống không nằm ở thanh đầu trang (đã ở đầu thanh menu bên)", () => {
    render(
      <Header
        hoTen="Trần Văn An"
        nhanVaiTro="Quản trị viên"
        onDangXuat={vi.fn()}
      />,
    );
    const thanh = screen.getByRole("banner");
    expect(thanh.textContent).not.toContain("Quản lý mượn trả thiết bị");
    expect(thanh.textContent).toContain("Quản trị viên");
  });

  it("CMP-30: đang đăng xuất → nút vô hiệu, hiện 'Đang xử lý'", () => {
    render(
      <Header
        hoTen="Trần Văn An"
        nhanVaiTro="Quản trị viên"
        onDangXuat={vi.fn()}
        dangDangXuat
      />,
    );
    expect(screen.getByRole("button", { name: "Đang xử lý" })).toHaveProperty(
      "disabled",
      true,
    );
  });
});
