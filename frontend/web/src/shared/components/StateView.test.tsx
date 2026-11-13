// @vitest-environment jsdom
import { act, fireEvent, render, screen } from "@testing-library/react";
import { afterEach, describe, expect, it, vi } from "vitest";
import { StateView } from "./StateView";

afterEach(() => {
  vi.useRealTimers();
});

describe("StateView", () => {
  it("CMP-25: đang tải → vùng aria-busy, chữ 'Đang tải'", () => {
    render(<StateView loai="DANG_TAI" />);
    const vung = screen.getByRole("status");
    expect(vung.getAttribute("aria-busy")).toBe("true");
    expect(vung.textContent).toContain("Đang tải");
  });

  it("ĐX-FA-04: chờ quá 8 giây → 'Hệ thống đang khởi động, vui lòng chờ'", () => {
    vi.useFakeTimers();
    render(<StateView loai="DANG_TAI" />);
    act(() => {
      vi.advanceTimersByTime(7_999);
    });
    expect(
      screen.queryByText("Hệ thống đang khởi động, vui lòng chờ"),
    ).toBeNull();
    act(() => {
      vi.advanceTimersByTime(1);
    });
    expect(
      screen.getByText("Hệ thống đang khởi động, vui lòng chờ"),
    ).toBeTruthy();
  });

  it("DS 9.3: rỗng → thông điệp của màn hình, tông trung tính, không phải cảnh báo lỗi", () => {
    render(
      <StateView loai="RONG" thongDiep="Không có phiếu yêu cầu chờ duyệt" />,
    );
    expect(screen.getByText("Không có phiếu yêu cầu chờ duyệt")).toBeTruthy();
    expect(screen.queryByRole("alert")).toBeNull();
    const vung = screen.getByRole("status");
    expect(vung.innerHTML).not.toContain("danger");
    expect(vung.querySelector("svg")?.getAttribute("class")).toContain(
      "text-tone-neutral-icon",
    );
  });

  it("DS 9.5: rỗng do bộ lọc kèm hành động 'Xóa bộ lọc'", () => {
    const xoa = vi.fn();
    render(
      <StateView
        loai="RONG"
        thongDiep="Không có kết quả"
        doBoLoc
        hanhDong={{ nhan: "Xóa bộ lọc", onClick: xoa }}
      />,
    );
    fireEvent.click(screen.getByRole("button", { name: "Xóa bộ lọc" }));
    expect(xoa).toHaveBeenCalledTimes(1);
  });

  it("CMP-25: lỗi mạng → thông điệp chuẩn và nút Thử lại", () => {
    const thuLai = vi.fn();
    render(<StateView loai="LOI_MANG" onThuLai={thuLai} />);
    expect(screen.getByRole("alert").textContent).toContain(
      "Không tải được dữ liệu. Kiểm tra kết nối rồi thử lại.",
    );
    fireEvent.click(screen.getByRole("button", { name: "Thử lại" }));
    expect(thuLai).toHaveBeenCalledTimes(1);
  });

  it("CMP-25: lỗi hệ thống → 'Đã xảy ra lỗi hệ thống.', mã theo dõi bằng type.code, nút Thử lại", () => {
    const thuLai = vi.fn();
    render(
      <StateView
        loai="LOI_HE_THONG"
        traceId="00-abc123-01"
        onThuLai={thuLai}
      />,
    );
    const vung = screen.getByRole("alert");
    expect(vung.textContent).toContain("Đã xảy ra lỗi hệ thống.");
    expect(vung.textContent).toContain("Mã theo dõi:");
    expect(screen.getByText("00-abc123-01").className).toContain("type-code");
    fireEvent.click(screen.getByRole("button", { name: "Thử lại" }));
    expect(thuLai).toHaveBeenCalledTimes(1);
  });
});
