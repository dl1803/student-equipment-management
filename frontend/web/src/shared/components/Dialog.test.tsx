// @vitest-environment jsdom
import { fireEvent, render, screen, within } from "@testing-library/react";
import { describe, expect, it, vi } from "vitest";
import { Dialog } from "./Dialog";

function hop() {
  const d = document.querySelector("dialog");
  if (d === null) throw new Error("không có dialog");
  return d;
}

describe("Dialog kieu NGAN (Wireframes P-04, ĐX-WF-05)", () => {
  it("ngăn bên phải: có tiêu đề làm tên truy cập, nút Đóng ở đầu, hàng nút ở chân", () => {
    const onDong = vi.fn();
    render(
      <Dialog
        open
        kieu="NGAN"
        tieuDe="Nhập kho thiết bị"
        onDong={onDong}
        hangNut={<button type="button">Lưu</button>}
      >
        <p>Nội dung</p>
      </Dialog>,
    );
    const d = hop();
    expect(d.dataset.kieu).toBe("NGAN");
    expect(d.className).toContain("max-w-web-form-max");
    expect(d.className).toContain("ml-auto");
    expect(
      screen.getByRole("dialog", { name: "Nhập kho thiết bị" }),
    ).toBeTruthy();
    fireEvent.click(within(d).getByRole("button", { name: "Đóng" }));
    expect(onDong).toHaveBeenCalledOnce();
  });

  it("khoaDong: nút Đóng vô hiệu và Esc không đóng (đang gửi)", () => {
    const onDong = vi.fn();
    render(
      <Dialog
        open
        kieu="NGAN"
        tieuDe="Sửa thiết bị"
        khoaDong
        onDong={onDong}
        hangNut={null}
      >
        <p>Nội dung</p>
      </Dialog>,
    );
    expect(
      within(hop())
        .getByRole("button", { name: "Đóng" })
        .hasAttribute("disabled"),
    ).toBe(true);
    fireEvent(hop(), new Event("cancel", { cancelable: true }));
    expect(onDong).not.toHaveBeenCalled();
  });

  it("hộp mặc định không có nút Đóng ở đầu (giữ hành vi cũ)", () => {
    render(
      <Dialog open tieuDe="Xóa loại" onDong={vi.fn()} hangNut={null}>
        <p>Nội dung</p>
      </Dialog>,
    );
    expect(hop().dataset.kieu).toBe("HOP");
    expect(within(hop()).queryByRole("button", { name: "Đóng" })).toBeNull();
  });
});
