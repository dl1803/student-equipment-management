// @vitest-environment jsdom
import { fireEvent, render, screen } from "@testing-library/react";
import { describe, expect, it, vi } from "vitest";
import { Header } from "./Header";

describe("Header", () => {
  it("CMP-30: hiện họ tên, vai trò bằng chữ và nút Đăng xuất (không có hộp xác nhận)", () => {
    const onDangXuat = vi.fn();
    render(
      <Header
        tenHeThong="Quản lý mượn trả thiết bị"
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

  it("Wireframes 2.3: khung QT không có tên hệ thống → không sinh thẻ văn bản rỗng", () => {
    render(
      <Header
        hoTen="Trần Văn An"
        nhanVaiTro="Quản trị viên"
        onDangXuat={vi.fn()}
      />,
    );
    const thanh = screen.getByRole("banner");
    expect(
      [...thanh.querySelectorAll("p")].some((p) => p.textContent === ""),
    ).toBe(false);
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
