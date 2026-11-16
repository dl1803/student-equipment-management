// @vitest-environment jsdom
import { fireEvent, render, screen } from "@testing-library/react";
import { describe, expect, it, vi } from "vitest";
import { Card } from "./Card";

describe("Card", () => {
  it("CMP-15 Tương tác: cả thẻ là liên kết tới màn hình, có chevron_right", () => {
    render(
      <Card
        bienThe="TUONG_TAC"
        tieuDe="Giao thiết bị"
        href="/nv/giao-thiet-bi/the"
      />,
    );
    const lienKet = screen.getByRole("link", { name: "Giao thiết bị" });
    expect(lienKet.getAttribute("href")).toBe("/nv/giao-thiet-bi/the");
    expect(lienKet.querySelector("svg")?.getAttribute("aria-hidden")).toBe(
      "true",
    );
  });

  it("CMP-15 Chọn được: là nút chọn, khai báo aria-checked và có dấu ngoài màu khi chọn", () => {
    const onChon = vi.fn();
    const { rerender } = render(
      <Card
        bienThe="CHON_DUOC"
        tieuDe="Phiếu yêu cầu"
        duocChon={false}
        onChon={onChon}
      />,
    );
    const nut = screen.getByRole("radio", { name: "Phiếu yêu cầu" });
    expect(nut.getAttribute("aria-checked")).toBe("false");
    fireEvent.click(nut);
    expect(onChon).toHaveBeenCalledTimes(1);

    rerender(
      <Card
        bienThe="CHON_DUOC"
        tieuDe="Phiếu yêu cầu"
        duocChon
        onChon={onChon}
      />,
    );
    expect(nut.getAttribute("aria-checked")).toBe("true");
    expect(nut.querySelector("svg")).not.toBeNull();
  });

  it("CMP-15 Tông: nền theo tông và biểu tượng của tông", () => {
    render(
      <Card bienThe="TONG" tong="warning" tieuDe="Quyền mượn">
        Tạm khóa
      </Card>,
    );
    const the = screen.getByText("Quyền mượn").closest("section");
    expect(the?.className).toContain("bg-tone-warning-bg");
  });
});
