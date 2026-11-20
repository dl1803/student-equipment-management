// @vitest-environment jsdom
import { fireEvent, render, screen, within } from "@testing-library/react";
import { useState } from "react";
import { describe, expect, it, vi } from "vitest";
import { SelectNhieu, type LuaChonNhieu } from "./SelectNhieu";

const DS: LuaChonNhieu[] = [
  { giaTri: "n1", nhan: "Công ty Phong Vũ", moTa: "028 1234 5678" },
  { giaTri: "n2", nhan: "Công ty An Khang" },
  { giaTri: "n3", nhan: "Nhà cung cấp Thiết bị Giáo dục Miền Nam" },
];

function VeThu({
  ban = [] as string[],
  onChange = vi.fn<(g: string[]) => void>(),
  ...them
}: Partial<Parameters<typeof SelectNhieu>[0]> & {
  ban?: string[];
}) {
  const [giaTri, setGiaTri] = useState<string[]>(ban);
  return (
    <>
      <label htmlFor="ncc">Nhà cung cấp</label>
      <SelectNhieu
        id="ncc"
        dsLuaChon={DS}
        giaTri={giaTri}
        onChange={(g) => {
          onChange(g);
          setGiaTri(g);
        }}
        tomTat={(n) => `${n} nhà cung cấp đã chọn`}
        goiY="Chọn nhà cung cấp"
        {...them}
      />
      <p>Ngoài</p>
    </>
  );
}

const o = () => screen.getByRole("combobox", { name: /Nhà cung cấp/ });

describe("SelectNhieu (CMP-06 chọn nhiều)", () => {
  it("gắn nhãn; chưa chọn hiện gợi ý; đóng thì không có danh sách", () => {
    render(<VeThu />);
    expect(o().textContent).toContain("Chọn nhà cung cấp");
    expect(o().getAttribute("aria-haspopup")).toBe("listbox");
    expect(o().getAttribute("aria-expanded")).toBe("false");
    expect(screen.queryByRole("listbox")).toBeNull();
  });

  it("chọn nhiều mục, danh sách KHÔNG đóng sau mỗi lần chọn; tóm tắt số mục; mục có dòng phụ", () => {
    const onChange = vi.fn<(g: string[]) => void>();
    render(<VeThu onChange={onChange} />);
    fireEvent.click(o());
    const ds = screen.getByRole("listbox");
    expect(ds.getAttribute("aria-multiselectable")).toBe("true");
    expect(within(ds).getByText("028 1234 5678")).toBeTruthy();

    fireEvent.click(screen.getByRole("option", { name: /An Khang/ }));
    fireEvent.click(screen.getByRole("option", { name: /Phong Vũ/ }));
    expect(screen.getByRole("listbox")).toBeTruthy();
    // Giá trị theo thứ tự của danh sách lựa chọn, không theo thứ tự bấm.
    expect(onChange).toHaveBeenLastCalledWith(["n1", "n2"]);
    expect(
      screen
        .getByRole("option", { name: /Phong Vũ/ })
        .getAttribute("aria-selected"),
    ).toBe("true");
    expect(o().textContent).toContain("2 nhà cung cấp đã chọn");

    fireEvent.click(screen.getByRole("option", { name: /Phong Vũ/ }));
    expect(onChange).toHaveBeenLastCalledWith(["n2"]);
  });

  it("bàn phím: ↓ mở, ↓/↑/Home/End di chuyển, Space bật/tắt, Esc đóng và trả tiêu điểm", () => {
    const onChange = vi.fn<(g: string[]) => void>();
    render(<VeThu onChange={onChange} />);
    fireEvent.keyDown(o(), { key: "ArrowDown" });
    const ds = screen.getByRole("listbox");
    expect(document.activeElement).toBe(ds);
    const dangTro = () =>
      document.getElementById(ds.getAttribute("aria-activedescendant") ?? "")
        ?.textContent;
    expect(dangTro()).toContain("Phong Vũ");

    fireEvent.keyDown(ds, { key: "ArrowDown" });
    expect(dangTro()).toContain("An Khang");
    fireEvent.keyDown(ds, { key: " " });
    expect(onChange).toHaveBeenLastCalledWith(["n2"]);
    fireEvent.keyDown(ds, { key: "End" });
    expect(dangTro()).toContain("Miền Nam");
    fireEvent.keyDown(ds, { key: "ArrowDown" });
    expect(dangTro()).toContain("Phong Vũ");
    fireEvent.keyDown(ds, { key: "ArrowUp" });
    expect(dangTro()).toContain("Miền Nam");
    fireEvent.keyDown(ds, { key: "Home" });
    fireEvent.keyDown(ds, { key: "Enter" });
    expect(onChange).toHaveBeenLastCalledWith(["n1", "n2"]);

    fireEvent.keyDown(ds, { key: "Escape" });
    expect(screen.queryByRole("listbox")).toBeNull();
    expect(document.activeElement).toBe(o());
  });

  it("bấm ra ngoài thì đóng", () => {
    render(<VeThu />);
    fireEvent.click(o());
    fireEvent.pointerDown(screen.getByText("Ngoài"));
    expect(screen.queryByRole("listbox")).toBeNull();
  });

  it("giá trị không có trong nguồn lựa chọn được giữ lại khi bật/tắt mục khác", () => {
    const onChange = vi.fn<(g: string[]) => void>();
    render(<VeThu ban={["cu-khong-co"]} onChange={onChange} />);
    expect(o().textContent).toContain("1 nhà cung cấp đã chọn");
    fireEvent.click(o());
    fireEvent.click(screen.getByRole("option", { name: /An Khang/ }));
    expect(onChange).toHaveBeenLastCalledWith(["cu-khong-co", "n2"]);
  });

  it("nguồn đang tải: vô hiệu, hiện 'Đang tải'; lỗi tải: trạng thái lỗi kèm Thử lại", () => {
    const onThuLai = vi.fn();
    const { rerender } = render(<VeThu dangTaiLuaChon />);
    expect(o().hasAttribute("disabled")).toBe(true);
    expect(o().textContent).toContain("Đang tải");

    rerender(<VeThu loiTaiLuaChon={{ onThuLai }} />);
    expect(o().getAttribute("aria-invalid")).toBe("true");
    expect(screen.getByText("Không tải được danh sách lựa chọn")).toBeTruthy();
    fireEvent.click(screen.getByRole("button", { name: "Thử lại" }));
    expect(onThuLai).toHaveBeenCalledOnce();
  });

  it("rê chuột dời mục đang trỏ: Space bật/tắt đúng mục dưới chuột", () => {
    const onChange = vi.fn<(g: string[]) => void>();
    render(<VeThu onChange={onChange} />);
    fireEvent.keyDown(o(), { key: "ArrowDown" });
    const ds = screen.getByRole("listbox");
    fireEvent.mouseEnter(screen.getByRole("option", { name: /Miền Nam/ }));
    fireEvent.keyDown(ds, { key: " " });
    expect(onChange).toHaveBeenLastCalledWith(["n3"]);
  });
});
