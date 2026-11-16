// @vitest-environment jsdom
import { fireEvent, render, screen } from "@testing-library/react";
import { describe, expect, it, vi } from "vitest";
import { Banner } from "./Banner";

describe("Banner", () => {
  it.each([
    ["danger", "alert"],
    ["warning", "alert"],
    ["info", "status"],
    ["success", "status"],
  ] as const)(
    "CMP-24: tông %s là thông báo %s cho trình đọc màn hình",
    (tong, role) => {
      render(<Banner tong={tong}>Nội dung</Banner>);
      const khoi = screen.getByRole(role);
      expect(khoi.dataset.tong).toBe(tong);
      expect(khoi.className).toContain(`bg-tone-${tong}-bg`);
      expect(khoi.querySelector("svg")?.getAttribute("aria-hidden")).toBe(
        "true",
      );
    },
  );

  it("DS 9.4: lỗi hệ thống kèm mã theo dõi bằng type.code", () => {
    render(
      <Banner tong="danger" traceId="00-abc-01">
        Đã xảy ra lỗi hệ thống.
      </Banner>,
    );
    expect(screen.getByRole("alert").textContent).toContain("Mã theo dõi:");
    expect(screen.getByText("00-abc-01").className).toContain("type-code");
  });

  it("CMP-24 biến thể Kết luận: tiêu đề là nhãn phát sinh, kèm danh sách lý do", () => {
    render(
      <Banner
        tong="danger"
        ketLuan="KHONG_HOP_LE"
        dsLyDo={["Thẻ sinh viên đã hết hạn", "Còn biên bản chưa nộp"]}
      />,
    );
    expect(screen.getByText("Không hợp lệ")).toBeTruthy();
    expect(screen.getAllByRole("listitem")).toHaveLength(2);
  });

  it("CMP-24: không có nút đóng; hành động tùy chọn là nút văn bản", () => {
    const onClick = vi.fn();
    render(
      <Banner tong="warning" hanhDong={{ nhan: "Tải lại", onClick }}>
        Dữ liệu vừa thay đổi
      </Banner>,
    );
    expect(screen.queryByRole("button", { name: /đóng/i })).toBeNull();
    fireEvent.click(screen.getByRole("button", { name: "Tải lại" }));
    expect(onClick).toHaveBeenCalledTimes(1);
  });
});
