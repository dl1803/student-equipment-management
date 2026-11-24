// @vitest-environment jsdom
import { render, screen } from "@testing-library/react";
import { describe, expect, it } from "vitest";
import { StatusBadge } from "./StatusBadge";

function nhanTheoChu(chu: string): HTMLElement {
  const phanTu = screen.getByText(chu).closest("[data-tong]");
  if (!(phanTu instanceof HTMLElement))
    throw new Error(`Không thấy nhãn ${chu}`);
  return phanTu;
}

describe("StatusBadge", () => {
  it("CMP-13: mã đã biết → nhãn tiếng Việt, tông và biểu tượng theo tông", () => {
    render(<StatusBadge ho="PHIEU_YEU_CAU" ma="CHO_DUYET" />);
    const nhan = nhanTheoChu("Chờ duyệt");
    expect(nhan.dataset.tong).toBe("warning");
    expect(nhan.dataset.nhanDam).toBe("false");
    expect(nhan.className).toContain("bg-tone-warning-bg");
    expect(nhan.querySelector("svg")?.getAttribute("aria-hidden")).toBe("true");
  });

  it("DS 7.1: mức đậm dùng nền solid và chữ on-solid", () => {
    render(<StatusBadge ho="PHIEU_MUON" ma="QUA_HAN" />);
    const nhan = nhanTheoChu("Quá hạn");
    expect(nhan.dataset.nhanDam).toBe("true");
    expect(nhan.className).toContain("bg-tone-danger-solid");
    expect(nhan.className).toContain("text-tone-danger-on-solid");
  });

  it("DS 7.1: tông neutral không có biểu tượng", () => {
    render(<StatusBadge ho="BIEN_BAN" ma="DA_HUY" />);
    const nhan = nhanTheoChu("Đã hủy");
    expect(nhan.dataset.tong).toBe("neutral");
    expect(nhan.querySelector("svg")).toBeNull();
  });

  it("TC-03: mã lạ từ backend → hiển thị nguyên mã bằng type.code, tông neutral, không lỗi", () => {
    render(<StatusBadge ho="TINH_TRANG_THIET_BI" ma="MA_MOI_CHUA_CO" />);
    const chu = screen.getByText("MA_MOI_CHUA_CO");
    expect(chu.className).toContain("type-code");
    expect(nhanTheoChu("MA_MOI_CHUA_CO").dataset.tong).toBe("neutral");
  });

  it("DS 7.3: nhãn phát sinh dùng cùng thành phần", () => {
    render(<StatusBadge nhanPhatSinh="HET_HANG" />);
    expect(nhanTheoChu("Hết hàng").dataset.tong).toBe("danger");
  });

  it("CMP-13: không tương tác — không phải nút, không nhận tiêu điểm", () => {
    render(<StatusBadge ho="PHIEU_MUON" ma="DANG_MUON" />);
    expect(screen.queryByRole("button")).toBeNull();
    expect(nhanTheoChu("Đang mượn").getAttribute("tabindex")).toBeNull();
  });
});
