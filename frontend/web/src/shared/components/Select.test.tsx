// @vitest-environment jsdom
import { fireEvent, render, screen } from "@testing-library/react";
import { describe, expect, it, vi } from "vitest";
import { Select } from "./Select";
import { TextArea } from "./TextArea";

const DS = [
  { giaTri: "a", nhan: "Laptop" },
  { giaTri: "b", nhan: "Máy chiếu (Đã ngưng sử dụng)" },
];

describe("Select — nguồn lựa chọn tải từ máy chủ (DS CMP-06)", () => {
  it("đang tải: vô hiệu, aria-busy, mục gợi ý là 'Đang tải'", () => {
    render(
      <Select
        id="loai"
        dsLuaChon={[]}
        giaTri={null}
        onChange={vi.fn()}
        dangTaiLuaChon
      />,
    );
    const o = screen.getByRole("combobox");
    expect(o.hasAttribute("disabled")).toBe(true);
    expect(o.getAttribute("aria-busy")).toBe("true");
    expect(o.textContent).toContain("Đang tải");
  });

  it("lỗi tải: ô ở trạng thái lỗi kèm 'Thử lại'", () => {
    const onThuLai = vi.fn();
    render(
      <Select
        id="loai"
        dsLuaChon={DS}
        giaTri={null}
        onChange={vi.fn()}
        loiTaiLuaChon={{ onThuLai }}
      />,
    );
    expect(screen.getByRole("combobox").getAttribute("aria-invalid")).toBe(
      "true",
    );
    fireEvent.click(screen.getByRole("button", { name: "Thử lại" }));
    expect(onThuLai).toHaveBeenCalledOnce();
  });

  it("bình thường: không có dòng lỗi, chọn được", () => {
    const onChange = vi.fn();
    render(
      <Select id="loai" dsLuaChon={DS} giaTri={null} onChange={onChange} />,
    );
    expect(screen.queryByText("Không tải được danh sách lựa chọn")).toBeNull();
    fireEvent.change(screen.getByRole("combobox"), { target: { value: "b" } });
    expect(onChange).toHaveBeenCalledWith("b");
  });
  it("bộ lọc có mục Tất cả đang tải: mục đó hiện 'Đang tải' thay vì 'Tất cả'", () => {
    render(
      <Select
        id="loc"
        dsLuaChon={[]}
        giaTri={null}
        onChange={vi.fn()}
        coMucTatCa
        dangTaiLuaChon
      />,
    );
    const o = screen.getByRole("combobox");
    expect(o.textContent).toBe("Đang tải");
    expect(o.hasAttribute("disabled")).toBe(true);
  });
  it("nhanTatCa đổi chữ của mục Tất cả; chọn lại mục đó trả null", () => {
    const onChange = vi.fn();
    render(
      <Select
        id="loc"
        dsLuaChon={[{ giaTri: "a", nhan: "Phòng A" }]}
        giaTri="a"
        onChange={onChange}
        coMucTatCa
        nhanTatCa="Tất cả phòng được cấp quyền"
      />,
    );
    const o = screen.getByRole("combobox");
    expect(
      screen.getByRole("option", { name: "Tất cả phòng được cấp quyền" }),
    ).toBeTruthy();
    fireEvent.change(o, { target: { value: "" } });
    expect(onChange).toHaveBeenCalledWith(null);
  });
});

describe("TextArea (CMP-04 nhiều dòng)", () => {
  it("tối thiểu 3 dòng; giữ nguyên văn bản dài có dấu", () => {
    const dai =
      "Laptop Dell Latitude 5440, CPU Intel Core i5-1335U, RAM 16 GB, SSD 512 GB; kèm sạc và túi chống sốc.";
    render(<TextArea aria-label="Thông số" rows={1} defaultValue={dai} />);
    const o = screen.getByRole("textbox", { name: "Thông số" });
    expect(o.getAttribute("rows")).toBe("3");
    expect((o as HTMLTextAreaElement).value).toBe(dai);
  });
});
