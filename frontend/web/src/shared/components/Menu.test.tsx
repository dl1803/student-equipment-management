// @vitest-environment jsdom
import { fireEvent, render, screen } from "@testing-library/react";
import { afterEach, describe, expect, it, vi } from "vitest";
import { Menu } from "./Menu";

function dsMucMau(ham: Record<string, () => void> = {}) {
  return [
    { nhan: "Xóa", onClick: ham.xoa ?? vi.fn(), huyBo: true },
    { nhan: "Sửa", onClick: ham.sua ?? vi.fn() },
    { nhan: "Mở khóa", onClick: vi.fn(), an: true },
    { nhan: "Cấp quyền phòng", onClick: vi.fn() },
  ];
}

function moBangChuot() {
  const nut = screen.getByRole("button", { name: "Thao tác" });
  fireEvent.click(nut);
  return nut;
}

function boTieuDiem() {
  if (document.activeElement instanceof HTMLElement) {
    document.activeElement.blur();
  }
}

const tenMuc = () => screen.getAllByRole("menuitem").map((m) => m.textContent);

describe("Menu (CMP-20)", () => {
  it("nút kích hoạt 'Thao tác' khai báo menu; đóng thì không có danh sách", () => {
    render(<Menu dsMuc={dsMucMau()} />);
    const nut = screen.getByRole("button", { name: "Thao tác" });
    expect(nut.getAttribute("aria-haspopup")).toBe("menu");
    expect(nut.getAttribute("aria-expanded")).toBe("false");
    expect(screen.queryByRole("menu")).toBeNull();
  });

  it("mở: mục ẩn không hiện; mục hủy bỏ đặt cuối, tông danger, cách bằng đường kẻ; tiêu điểm ở mục đầu", () => {
    render(<Menu dsMuc={dsMucMau()} />);
    const nut = moBangChuot();

    const menu = screen.getByRole("menu", { name: "Thao tác" });
    expect(nut.getAttribute("aria-expanded")).toBe("true");
    expect(nut.getAttribute("aria-controls")).toBe(menu.id);
    expect(tenMuc()).toEqual(["Sửa", "Cấp quyền phòng", "Xóa"]);
    expect(screen.getByRole("separator")).toBeTruthy();
    expect(screen.getByRole("menuitem", { name: "Xóa" }).className).toContain(
      "text-tone-danger-text",
    );
    expect(document.activeElement?.textContent).toBe("Sửa");
  });

  it("bàn phím: ↓/↑ di chuyển vòng, Home/End, Enter chọn và trả tiêu điểm về nút", () => {
    const sua = vi.fn();
    render(<Menu dsMuc={dsMucMau({ sua })} />);
    const nut = moBangChuot();
    const menu = screen.getByRole("menu");

    fireEvent.keyDown(menu, { key: "ArrowDown" });
    expect(document.activeElement?.textContent).toBe("Cấp quyền phòng");
    fireEvent.keyDown(menu, { key: "ArrowDown" });
    fireEvent.keyDown(menu, { key: "ArrowDown" });
    expect(document.activeElement?.textContent).toBe("Sửa");
    fireEvent.keyDown(menu, { key: "ArrowUp" });
    expect(document.activeElement?.textContent).toBe("Xóa");
    fireEvent.keyDown(menu, { key: "Home" });
    expect(document.activeElement?.textContent).toBe("Sửa");
    fireEvent.keyDown(menu, { key: "End" });
    expect(document.activeElement?.textContent).toBe("Xóa");
    fireEvent.keyDown(menu, { key: "Home" });

    // <button> gốc: Enter/Space kích hoạt click.
    fireEvent.click(document.activeElement ?? document.body);
    expect(sua).toHaveBeenCalledOnce();
    expect(screen.queryByRole("menu")).toBeNull();
    expect(document.activeElement).toBe(nut);
  });

  it("bàn phím: ↓ trên nút mở ở mục đầu, ↑ mở ở mục cuối; Esc đóng và trả tiêu điểm về nút", () => {
    render(<Menu dsMuc={dsMucMau()} />);
    const nut = screen.getByRole("button", { name: "Thao tác" });

    fireEvent.keyDown(nut, { key: "ArrowUp" });
    expect(document.activeElement?.textContent).toBe("Xóa");
    fireEvent.keyDown(screen.getByRole("menu"), { key: "Escape" });
    expect(screen.queryByRole("menu")).toBeNull();
    expect(document.activeElement).toBe(nut);

    fireEvent.keyDown(nut, { key: "ArrowDown" });
    expect(document.activeElement?.textContent).toBe("Sửa");
  });

  it("bấm ra ngoài hoặc bấm lại nút thì đóng, không chạy hành động", () => {
    const xoa = vi.fn();
    render(
      <>
        <p>Ngoài</p>
        <Menu dsMuc={dsMucMau({ xoa })} />
      </>,
    );
    moBangChuot();
    fireEvent.pointerDown(screen.getByText("Ngoài"));
    expect(screen.queryByRole("menu")).toBeNull();

    const nut = moBangChuot();
    fireEvent.click(nut);
    expect(screen.queryByRole("menu")).toBeNull();
    expect(xoa).not.toHaveBeenCalled();
  });

  it("Tab trong menu đóng menu và trả tiêu điểm về nút (không rơi về body)", () => {
    render(<Menu dsMuc={dsMucMau()} />);
    const nut = moBangChuot();
    fireEvent.keyDown(screen.getByRole("menu"), { key: "Tab" });
    expect(screen.queryByRole("menu")).toBeNull();
    expect(document.activeElement).toBe(nut);
  });

  it("bàn phím: tiêu điểm chưa ở mục nào thì ↑ tới mục cuối, ↓ tới mục đầu", () => {
    render(<Menu dsMuc={dsMucMau()} />);
    moBangChuot();
    const menu = screen.getByRole("menu");

    boTieuDiem();
    fireEvent.keyDown(menu, { key: "ArrowUp" });
    expect(document.activeElement?.textContent).toBe("Xóa");

    boTieuDiem();
    fireEvent.keyDown(menu, { key: "ArrowDown" });
    expect(document.activeElement?.textContent).toBe("Sửa");
  });

  it("menu không có mục hiện: phím mũi tên không lỗi", () => {
    render(<Menu dsMuc={[{ nhan: "Mở khóa", onClick: vi.fn(), an: true }]} />);
    moBangChuot();
    const menu = screen.getByRole("menu");
    expect(() => {
      fireEvent.keyDown(menu, { key: "ArrowDown" });
      fireEvent.keyDown(menu, { key: "ArrowUp" });
    }).not.toThrow();
  });
});

describe("Menu (CMP-20) — vị trí danh sách", () => {
  const CAO_KHUNG = 800;
  const RONG_KHUNG = 1000;
  const CAO_DS = 150;

  function giaLapHinhHoc(nutTren: number) {
    Object.defineProperty(document.documentElement, "clientHeight", {
      configurable: true,
      value: CAO_KHUNG,
    });
    Object.defineProperty(document.documentElement, "clientWidth", {
      configurable: true,
      value: RONG_KHUNG,
    });
    vi.spyOn(HTMLElement.prototype, "offsetHeight", "get").mockReturnValue(
      CAO_DS,
    );
    vi.spyOn(HTMLElement.prototype, "getBoundingClientRect").mockReturnValue(
      DOMRect.fromRect({ x: 880, y: nutTren, width: 100, height: 32 }),
    );
  }

  afterEach(() => {
    vi.restoreAllMocks();
    Reflect.deleteProperty(document.documentElement, "clientHeight");
    Reflect.deleteProperty(document.documentElement, "clientWidth");
  });

  it("đủ chỗ bên dưới: mở xuống; mép phải căn theo clientWidth (không gồm thanh cuộn)", () => {
    giaLapHinhHoc(100);
    render(<Menu dsMuc={dsMucMau()} />);
    moBangChuot();
    const menu = screen.getByRole("menu");
    expect(menu.dataset.huong).toBe("XUONG");
    expect(menu.style.top).toBe("132px");
    expect(menu.style.right).toBe(`${RONG_KHUNG - 980}px`);
    expect(menu.className).toContain("mt-1");
  });

  it("dòng cuối khung nhìn: lật mở lên để mục hủy bỏ không tràn khỏi màn hình", () => {
    giaLapHinhHoc(740);
    render(<Menu dsMuc={dsMucMau()} />);
    moBangChuot();
    const menu = screen.getByRole("menu");
    expect(menu.dataset.huong).toBe("LEN");
    expect(menu.style.bottom).toBe(`${CAO_KHUNG - 740}px`);
    expect(menu.style.top).toBe("");
    expect(menu.className).toContain("mb-1");
  });
});
