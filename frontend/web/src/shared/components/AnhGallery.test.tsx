// @vitest-environment jsdom
import { fireEvent, render, screen, within } from "@testing-library/react";
import { describe, expect, it, vi } from "vitest";
import type { AnhRef } from "@/shared/api/types";
import { AnhGallery } from "./AnhGallery";

const ds = (phienBan: string): AnhRef[] => [
  { maAnh: "a1", url: `https://ky.test/a1?sig=${phienBan}` },
  { maAnh: "a2", url: `https://ky.test/a2?sig=${phienBan}` },
];

const anh = (ten: string) => screen.getByRole("img", { name: ten });

describe("AnhGallery (CMP-23)", () => {
  it("không có ảnh: hình thay thế", () => {
    render(<AnhGallery dsAnh={[]} tenThietBi="Laptop" />);
    expect(screen.getByRole("img", { name: "Không có ảnh" })).toBeTruthy();
  });

  it("thẻ img thường với loading=lazy và alt 'Ảnh n'", () => {
    render(<AnhGallery dsAnh={ds("v1")} tenThietBi="Laptop" />);
    expect(anh("Ảnh 1").getAttribute("loading")).toBe("lazy");
    expect(anh("Ảnh 2").getAttribute("src")).toBe("https://ky.test/a2?sig=v1");
  });

  it("ảnh ký lỗi: tải lại chi tiết ĐÚNG MỘT LẦN cho cả dải; URL mới vẫn lỗi thì dừng ở 'Không tải được ảnh'", () => {
    const onTaiLai = vi.fn();
    const { rerender } = render(
      <AnhGallery dsAnh={ds("v1")} tenThietBi="Laptop" onTaiLai={onTaiLai} />,
    );
    fireEvent.error(anh("Ảnh 1"));
    fireEvent.error(anh("Ảnh 2"));
    expect(onTaiLai).toHaveBeenCalledOnce();

    // refetch trả URL mới: ảnh được hiển thị lại.
    rerender(
      <AnhGallery dsAnh={ds("v2")} tenThietBi="Laptop" onTaiLai={onTaiLai} />,
    );
    expect(anh("Ảnh 1").getAttribute("src")).toBe("https://ky.test/a1?sig=v2");
    fireEvent.error(anh("Ảnh 1"));
    expect(onTaiLai).toHaveBeenCalledOnce();
    const o = screen.getByRole("group", { name: "Ảnh 1" });
    expect(within(o).getByText("Không tải được ảnh")).toBeTruthy();
  });

  it("'Tải lại' là làm mới thủ công: hiển thị lại và tải lại chi tiết thêm một lần", () => {
    const onTaiLai = vi.fn();
    render(
      <AnhGallery dsAnh={ds("v1")} tenThietBi="Laptop" onTaiLai={onTaiLai} />,
    );
    fireEvent.error(anh("Ảnh 1"));
    fireEvent.click(screen.getByRole("button", { name: "Tải lại" }));
    expect(onTaiLai).toHaveBeenCalledTimes(2);
    expect(anh("Ảnh 1")).toBeTruthy();
    fireEvent.error(anh("Ảnh 1"));
    expect(onTaiLai).toHaveBeenCalledTimes(2);
  });

  it("ảnh danh mục (không có onTaiLai): lỗi chỉ hiện khung lỗi, không có 'Tải lại'", () => {
    render(<AnhGallery dsAnh={ds("v1")} tenThietBi="Laptop" />);
    fireEvent.error(anh("Ảnh 1"));
    expect(screen.getByText("Không tải được ảnh")).toBeTruthy();
    expect(screen.queryByRole("button", { name: "Tải lại" })).toBeNull();
  });

  it("không lưu URL ảnh vào storage", () => {
    const onTaiLai = vi.fn();
    render(
      <AnhGallery dsAnh={ds("v1")} tenThietBi="Laptop" onTaiLai={onTaiLai} />,
    );
    fireEvent.error(anh("Ảnh 1"));
    fireEvent.click(screen.getByRole("button", { name: "Ảnh 2" }));
    // eslint-disable-next-line no-restricted-globals -- kiểm thử khẳng định URL không vào sessionStorage
    expect(sessionStorage.length).toBe(0);
    // eslint-disable-next-line no-restricted-globals -- kiểm thử khẳng định localStorage KHÔNG được dùng
    expect(localStorage.length).toBe(0);
  });

  it("lớp xem: tên 'Ảnh n của <tên thiết bị>', tiêu điểm ở nút Đóng; đóng bằng nút, Esc, bấm nền và trả tiêu điểm", () => {
    render(<AnhGallery dsAnh={ds("v1")} tenThietBi="Laptop" />);
    const nutAnh2 = screen.getByRole("button", { name: "Ảnh 2" });
    nutAnh2.focus();
    fireEvent.click(nutAnh2);

    const lop = screen.getByRole("dialog", { name: "Ảnh 2 của Laptop" });
    expect(lop.hasAttribute("open")).toBe(true);
    expect(document.activeElement).toBe(
      within(lop).getByRole("button", { name: "Đóng" }),
    );
    fireEvent.click(within(lop).getByRole("button", { name: "Đóng" }));
    expect(lop.hasAttribute("open")).toBe(false);
    expect(document.activeElement).toBe(nutAnh2);

    fireEvent.click(nutAnh2);
    fireEvent(lop, new Event("cancel", { cancelable: true }));
    expect(lop.hasAttribute("open")).toBe(false);

    fireEvent.click(nutAnh2);
    fireEvent.click(lop);
    expect(lop.hasAttribute("open")).toBe(false);

    // Bấm vào ảnh không đóng.
    fireEvent.click(nutAnh2);
    fireEvent.click(within(lop).getByRole("img"));
    expect(lop.hasAttribute("open")).toBe(true);
  });
});
