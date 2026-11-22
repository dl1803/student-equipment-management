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
    render(
      <Sidebar
        tenHeThong="Quản lý mượn trả thiết bị"
        dsNhom={DS_NHOM}
        tuyenHienTai="/qt"
        tuyenTrangChu="/qt"
      />,
    );
    const menu = screen.getByRole("navigation", { name: "Menu chính" });
    expect(within(menu).getByText("Danh mục")).toBeTruthy();
    expect(within(menu).getAllByRole("link")).toHaveLength(3);
  });

  it("CMP-29: mục đang chọn khai báo aria-current=page; trang chủ không sáng ở tuyến con", () => {
    render(
      <Sidebar
        tenHeThong="Quản lý mượn trả thiết bị"
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
    const bienBan = { nhan: "Biên bản", href: "/nv/bien-ban" };
    const trangChu = { nhan: "Trang chủ", href: "/nv" };
    expect(laMucDangChon(bienBan, "/nv/bien-ban/lap", "/nv")).toBe(true);
    expect(laMucDangChon(trangChu, "/nv/bien-ban", "/nv")).toBe(false);
    expect(laMucDangChon(bienBan, "/nv/bien-ban-khac", "/nv")).toBe(false);
  });

  it.each([
    "/nv/giao-thiet-bi/the",
    "/nv/giao-thiet-bi/nguon",
    "/nv/giao-thiet-bi/thiet-bi",
    "/nv/giao-thiet-bi/phieu",
  ])(
    "ĐX-FA-19: luồng nhiều bước — mục Giao thiết bị vẫn đang chọn ở %s",
    (tuyen) => {
      const giaoThietBi = {
        nhan: "Giao thiết bị",
        href: "/nv/giao-thiet-bi/the",
        tienToNhom: "/nv/giao-thiet-bi",
      };
      expect(laMucDangChon(giaoThietBi, tuyen, "/nv")).toBe(true);
      expect(laMucDangChon(giaoThietBi, "/nv/giao-thiet-bi-khac", "/nv")).toBe(
        false,
      );
    },
  );

  it("CMP-29: mục có biểu tượng trước chữ, ẩn với trình đọc màn hình; tên hệ thống dẫn về trang chủ", () => {
    function IconThu() {
      return <svg data-testid="bieu-tuong" aria-hidden="true" />;
    }
    render(
      <Sidebar
        tenHeThong="Quản lý mượn trả thiết bị"
        dsNhom={[
          { dsMuc: [{ nhan: "Trang chủ", href: "/qt", bieuTuong: IconThu }] },
        ]}
        tuyenHienTai="/qt"
        tuyenTrangChu="/qt"
      />,
    );
    const muc = screen.getByRole("link", { name: "Trang chủ" });
    const bieuTuong = within(muc).getByTestId("bieu-tuong");
    expect(muc.firstElementChild).toBe(bieuTuong);
    expect(bieuTuong.getAttribute("aria-hidden")).toBe("true");
    expect(muc.className).toContain("font-bold");
    expect(
      screen
        .getByRole("link", { name: "Quản lý mượn trả thiết bị" })
        .getAttribute("href"),
    ).toBe("/qt");
  });
});
