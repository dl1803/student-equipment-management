// @vitest-environment jsdom
import { fireEvent, render, screen } from "@testing-library/react";
import { useState } from "react";
import { describe, expect, it, vi } from "vitest";
import { Tabs } from "./Tabs";

type KhoaTab = "DANG_MUON" | "QUA_HAN" | "DA_TRA";
const DS_TAB = [
  { khoa: "DANG_MUON", nhan: "Đang mượn" },
  { khoa: "QUA_HAN", nhan: "Quá hạn" },
  { khoa: "DA_TRA", nhan: "Đã trả" },
] as const;

function VeThu({ onDoiTab = vi.fn() }: { onDoiTab?: (k: KhoaTab) => void }) {
  const [tab, setTab] = useState<KhoaTab>("DANG_MUON");
  return (
    <Tabs<KhoaTab>
      tenNhom="Trạng thái phiếu mượn"
      dsTab={DS_TAB}
      tabDangChon={tab}
      onDoiTab={(k) => {
        onDoiTab(k);
        setTab(k);
      }}
    >
      <p>Nội dung {tab}</p>
    </Tabs>
  );
}

const tab = (ten: string) => screen.getByRole("tab", { name: ten });

describe("Tabs (CMP-09)", () => {
  it("dãy tab có tên; tab đang chọn khai báo aria-selected, chữ đậm và vạch dưới; nội dung gắn với tab", () => {
    render(<VeThu />);
    expect(
      screen.getByRole("tablist", { name: "Trạng thái phiếu mượn" }),
    ).toBeTruthy();
    expect(tab("Đang mượn").getAttribute("aria-selected")).toBe("true");
    expect(tab("Quá hạn").getAttribute("aria-selected")).toBe("false");
    expect(tab("Đang mượn").className).toContain("font-bold");
    expect(tab("Đang mượn").className).toContain("border-border-selected");
    const panel = screen.getByRole("tabpanel", { name: "Đang mượn" });
    expect(panel.textContent).toBe("Nội dung DANG_MUON");
  });

  it("chỉ tab đang chọn nhận Tab (roving tabindex)", () => {
    render(<VeThu />);
    expect(tab("Đang mượn").tabIndex).toBe(0);
    expect(tab("Quá hạn").tabIndex).toBe(-1);
    expect(tab("Đã trả").tabIndex).toBe(-1);
  });

  it("bấm tab: đổi tab và báo onDoiTab để tải lại dữ liệu", () => {
    const onDoiTab = vi.fn();
    render(<VeThu onDoiTab={onDoiTab} />);
    fireEvent.click(tab("Đã trả"));
    expect(onDoiTab).toHaveBeenCalledWith("DA_TRA");
    expect(screen.getByRole("tabpanel", { name: "Đã trả" }).textContent).toBe(
      "Nội dung DA_TRA",
    );
  });

  it("bàn phím: ←/→ di chuyển vòng và chọn luôn, Home/End; tiêu điểm theo tab", () => {
    render(<VeThu />);
    const ds = screen.getByRole("tablist");
    tab("Đang mượn").focus();

    fireEvent.keyDown(ds, { key: "ArrowRight" });
    expect(tab("Quá hạn").getAttribute("aria-selected")).toBe("true");
    expect(document.activeElement).toBe(tab("Quá hạn"));

    fireEvent.keyDown(ds, { key: "End" });
    expect(document.activeElement).toBe(tab("Đã trả"));
    fireEvent.keyDown(ds, { key: "ArrowRight" });
    expect(document.activeElement).toBe(tab("Đang mượn"));
    fireEvent.keyDown(ds, { key: "ArrowLeft" });
    expect(document.activeElement).toBe(tab("Đã trả"));
    fireEvent.keyDown(ds, { key: "Home" });
    expect(tab("Đang mượn").getAttribute("aria-selected")).toBe("true");
  });

  it("phím kèm Alt/Ctrl/Meta/Shift để trình duyệt xử lý: không đổi tab, không chặn mặc định", () => {
    const onDoiTab = vi.fn();
    render(<VeThu onDoiTab={onDoiTab} />);
    const ds = screen.getByRole("tablist");
    tab("Đang mượn").focus();

    for (const [key, boTro] of [
      ["ArrowLeft", { altKey: true }],
      ["ArrowRight", { metaKey: true }],
      ["Home", { ctrlKey: true }],
      ["End", { ctrlKey: true }],
      ["ArrowRight", { shiftKey: true }],
    ] as const) {
      // fireEvent trả false khi trình xử lý gọi preventDefault.
      expect(fireEvent.keyDown(ds, { key, ...boTro })).toBe(true);
    }
    expect(onDoiTab).not.toHaveBeenCalled();
    expect(tab("Đang mượn").getAttribute("aria-selected")).toBe("true");
  });

  it("WCAG 2.4.7: tabpanel nhận tiêu điểm và KHÔNG tắt vòng tiêu điểm chung", () => {
    render(<VeThu />);
    const panel = screen.getByRole("tabpanel");
    expect(panel.tabIndex).toBe(0);
    expect(panel.className).not.toContain("outline-none");
  });
});
