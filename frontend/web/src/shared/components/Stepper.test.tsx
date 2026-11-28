// @vitest-environment jsdom
import { fireEvent, render, screen } from "@testing-library/react";
import { describe, expect, it, vi } from "vitest";
import { Stepper } from "./Stepper";

const DS_BUOC = [
  { nhan: "Thẻ SV", href: "/b1" },
  { nhan: "Nguồn giao", href: "/b2" },
  { nhan: "Thiết bị", href: "/b3" },
  { nhan: "Phiếu mượn", href: "/b4" },
];

describe("Stepper (CMP-31)", () => {
  it("bước đã qua là liên kết có dấu check; bước hiện tại aria-current; bước chưa tới không bấm, không nhận tiêu điểm", () => {
    const onQuayLai = vi.fn();
    render(
      <Stepper
        tenThanh="Các bước"
        dsBuoc={DS_BUOC}
        buocHienTai={3}
        onQuayLai={onQuayLai}
      />,
    );
    const thanh = screen.getByRole("navigation", { name: "Các bước" });
    const dsLienKet = screen.getAllByRole("link");
    expect(dsLienKet.map((a) => a.getAttribute("href"))).toEqual([
      "/b1",
      "/b2",
    ]);
    // Bước đã qua hiện dấu check thay số.
    expect(dsLienKet[0]?.textContent).not.toContain("1");
    expect(dsLienKet[0]?.querySelector("svg")).not.toBeNull();

    const hienTai = thanh.querySelector("[aria-current=step]");
    expect(hienTai?.textContent).toBe("3Thiết bị");
    expect(thanh.querySelectorAll("[aria-current]")).toHaveLength(1);
    const chuaToi = screen.getByText("Phiếu mượn").parentElement;
    expect(chuaToi?.tagName).toBe("SPAN");
    expect(chuaToi?.getAttribute("tabindex")).toBeNull();

    fireEvent.click(dsLienKet[1] as HTMLElement);
    expect(onQuayLai).toHaveBeenCalledWith(2);
  });
});
