// @vitest-environment jsdom
import { render, screen, within } from "@testing-library/react";
import { describe, expect, it } from "vitest";
import { laMucDangChon, Sidebar } from "./Sidebar";

const DS_NHOM = [
  { dsMuc: [{ nhan: "Trang chủ", href: "/qt" }] },
  {
    tieuDe: "Danh mục",
    dsMuc: [
      { nhan: "Phòng thiết bị", href: "/qt/danh-muc/phong" },
      { nhan: "Loại thiết bị", href: "/qt/danh-muc/loai-thiet-bi" },
    ],
  },
];

describe("Sidebar", () => {
  it("CMP-29: là điều hướng tên 'Menu chính', có tiêu đề nhóm", () => {
    render(<Sidebar dsNhom={DS_NHOM} tuyenHienTai="/qt" tuyenTrangChu="/qt" />);
    const menu = screen.getByRole("navigation", { name: "Menu chính" });
    expect(within(menu).getByText("Danh mục")).toBeTruthy();
    expect(within(menu).getAllByRole("link")).toHaveLength(3);
  });

  it("CMP-29: mục đang chọn khai báo aria-current=page; trang chủ không sáng ở tuyến con", () => {
    render(
      <Sidebar
        dsNhom={DS_NHOM}
        tuyenHienTai="/qt/danh-muc/phong"
        tuyenTrangChu="/qt"
      />,
    );
    expect(
      screen
        .getByRole("link", { name: "Phòng thiết bị" })
        .getAttribute("aria-current"),
    ).toBe("page");
    expect(
      screen
        .getByRole("link", { name: "Trang chủ" })
        .getAttribute("aria-current"),
    ).toBeNull();
  });

  it("CMP-29: tuyến con của một mục vẫn đánh dấu mục đó", () => {
    expect(laMucDangChon("/nv/bien-ban", "/nv/bien-ban/lap", "/nv")).toBe(true);
    expect(laMucDangChon("/nv", "/nv/bien-ban", "/nv")).toBe(false);
    expect(laMucDangChon("/nv/bien-ban", "/nv/bien-ban-khac", "/nv")).toBe(
      false,
    );
  });
});
