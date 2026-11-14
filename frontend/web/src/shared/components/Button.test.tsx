// @vitest-environment jsdom
import { fireEvent, render, screen } from "@testing-library/react";
import { describe, expect, it, vi } from "vitest";
import { Button } from "./Button";

describe("Button", () => {
  it("CMP-01: là nút thật, mặc định type=button để không tự gửi biểu mẫu", () => {
    render(<Button>Lưu</Button>);
    const nut = screen.getByRole("button", { name: "Lưu" });
    expect(nut.getAttribute("type")).toBe("button");
  });

  it.each(["primary", "secondary", "tertiary", "destructive"] as const)(
    "CMP-01: biến thể %s dùng lớp token của biến thể đó",
    (bienThe) => {
      render(<Button bienThe={bienThe}>Lưu</Button>);
      const nut = screen.getByRole("button", { name: "Lưu" });
      expect(nut.dataset.bienThe).toBe(bienThe);
      const tienTo =
        bienThe === "tertiary"
          ? "text-action-tertiary-text"
          : `bg-action-${bienThe}-bg`;
      expect(nut.className).toContain(tienTo);
    },
  );

  it("CMP-01: bấm gọi onClick", () => {
    const onClick = vi.fn();
    render(<Button onClick={onClick}>Duyệt</Button>);
    fireEvent.click(screen.getByRole("button", { name: "Duyệt" }));
    expect(onClick).toHaveBeenCalledTimes(1);
  });

  it("DS mục 6: disabled → không nhận bấm", () => {
    const onClick = vi.fn();
    render(
      <Button disabled onClick={onClick}>
        Duyệt
      </Button>,
    );
    const nut = screen.getByRole("button", { name: "Duyệt" });
    expect(nut).toHaveProperty("disabled", true);
    fireEvent.click(nut);
    expect(onClick).not.toHaveBeenCalled();
  });

  it("ĐX-UF-06: dangXuLy → vô hiệu, chữ 'Đang xử lý', aria-busy, khóa bấm", () => {
    const onClick = vi.fn();
    render(
      <Button bienThe="primary" dangXuLy onClick={onClick}>
        Lập phiếu mượn
      </Button>,
    );
    const nut = screen.getByRole("button", { name: "Đang xử lý" });
    expect(nut).toHaveProperty("disabled", true);
    expect(nut.getAttribute("aria-busy")).toBe("true");
    expect(screen.queryByText("Lập phiếu mượn")).toBeNull();
    fireEvent.click(nut);
    expect(onClick).not.toHaveBeenCalled();
  });

  it("CMP-01: type=submit được giữ khi khai báo", () => {
    render(<Button type="submit">Đăng nhập</Button>);
    expect(
      screen.getByRole("button", { name: "Đăng nhập" }).getAttribute("type"),
    ).toBe("submit");
  });
});
