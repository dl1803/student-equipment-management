// @vitest-environment jsdom
import { fireEvent, render, screen } from "@testing-library/react";
import { describe, expect, it, vi } from "vitest";
import { ScanInput, type TrangThaiScanInput } from "./ScanInput";

function hienThi(trangThai: TrangThaiScanInput, loi?: string) {
  const onXacNhan = vi.fn<(ma: string) => void>();
  const ketQua = render(
    <ScanInput
      id="ma"
      nhan="Mã thiết bị"
      trangThai={trangThai}
      {...(loi !== undefined ? { loi } : {})}
      onXacNhan={onXacNhan}
    />,
  );
  const o = screen.getByLabelText("Mã thiết bị") as HTMLInputElement;
  return { ...ketQua, o, onXacNhan };
}

describe("ScanInput (CMP-11)", () => {
  it("tiêu điểm khi gắn; Enter gửi mã đã cắt khoảng trắng, rỗng không gửi; chú thích gắn với ô", () => {
    const { o, onXacNhan } = hienThi("CHO_NHAP");
    expect(document.activeElement).toBe(o);
    expect(o.getAttribute("aria-describedby")).toBe("ma-mo-ta");

    fireEvent.change(o, { target: { value: "  " } });
    fireEvent.keyDown(o, { key: "Enter" });
    expect(onXacNhan).not.toHaveBeenCalled();

    fireEvent.change(o, { target: { value: " TB-01 " } });
    fireEvent.keyDown(o, { key: "a" });
    expect(onXacNhan).not.toHaveBeenCalled();
    fireEvent.keyDown(o, { key: "Enter" });
    expect(onXacNhan).toHaveBeenCalledWith("TB-01");
  });

  it("đang tra cứu → vô hiệu, aria-busy; xong (về CHO_NHAP) → xóa mã, lấy lại tiêu điểm", () => {
    const { o, rerender, onXacNhan } = hienThi("CHO_NHAP");
    fireEvent.change(o, { target: { value: "TB-01" } });
    rerender(
      <ScanInput
        id="ma"
        nhan="Mã thiết bị"
        trangThai="DANG_TRA_CUU"
        onXacNhan={onXacNhan}
      />,
    );
    expect(o.disabled).toBe(true);
    expect(o.getAttribute("aria-busy")).toBe("true");

    rerender(
      <ScanInput
        id="ma"
        nhan="Mã thiết bị"
        trangThai="CHO_NHAP"
        onXacNhan={onXacNhan}
      />,
    );
    expect(o.value).toBe("");
    expect(document.activeElement).toBe(o);
  });

  it("lỗi → viền lỗi, thông báo role=alert dưới ô, giữ mã và chọn toàn bộ", () => {
    const { o, rerender, onXacNhan } = hienThi("CHO_NHAP");
    fireEvent.change(o, { target: { value: "TB-99" } });
    rerender(
      <ScanInput
        id="ma"
        nhan="Mã thiết bị"
        trangThai="DANG_TRA_CUU"
        onXacNhan={onXacNhan}
      />,
    );
    rerender(
      <ScanInput
        id="ma"
        nhan="Mã thiết bị"
        trangThai="LOI"
        loi="Không tìm thấy thiết bị"
        onXacNhan={onXacNhan}
      />,
    );
    expect(screen.getByRole("alert").textContent).toContain(
      "Không tìm thấy thiết bị",
    );
    expect(o.getAttribute("aria-invalid")).toBe("true");
    expect(o.value).toBe("TB-99");
    expect(document.activeElement).toBe(o);
    expect([o.selectionStart, o.selectionEnd]).toEqual([0, 5]);
  });
});
