// @vitest-environment jsdom
import { fireEvent, render, screen } from "@testing-library/react";
import { describe, expect, it, vi } from "vitest";
import { IconButton } from "./IconButton";
import { IconError } from "./icons/IconError";

describe("IconButton", () => {
  it("CMP-02: tên truy cập lấy từ nhanTruyCap; biểu tượng ẩn với trình đọc màn hình", () => {
    render(
      <IconButton
        nhanTruyCap="Đóng"
        bieuTuong={<IconError kichThuoc="lg" />}
      />,
    );
    const nut = screen.getByRole("button", { name: "Đóng" });
    expect(nut.getAttribute("aria-label")).toBe("Đóng");
    expect(nut.querySelector("svg")?.getAttribute("aria-hidden")).toBe("true");
    expect(nut.getAttribute("type")).toBe("button");
  });

  it("CMP-02: bấm gọi onClick", () => {
    const onClick = vi.fn();
    render(
      <IconButton
        nhanTruyCap="Gỡ ảnh"
        bieuTuong={<IconError />}
        onClick={onClick}
      />,
    );
    fireEvent.click(screen.getByRole("button", { name: "Gỡ ảnh" }));
    expect(onClick).toHaveBeenCalledTimes(1);
  });

  it("CMP-02: nhanTruyCap rỗng → báo lỗi lập trình ngay, không render nút vô danh", () => {
    const onError = vi
      .spyOn(console, "error")
      .mockImplementation(() => undefined);
    expect(() =>
      render(<IconButton nhanTruyCap="  " bieuTuong={<IconError />} />),
    ).toThrow(/nhanTruyCap/);
    onError.mockRestore();
  });
});
