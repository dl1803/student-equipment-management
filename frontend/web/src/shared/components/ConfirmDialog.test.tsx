// @vitest-environment jsdom
import { fireEvent, render, screen } from "@testing-library/react";
import { useState } from "react";
import { describe, expect, it, vi } from "vitest";
import { ConfirmDialog } from "./ConfirmDialog";

type Props = {
  dangXuLy?: boolean;
  onXacNhan?: () => void;
  onDong?: () => void;
};

function HopMau({ dangXuLy = false, onXacNhan = vi.fn(), onDong }: Props) {
  const [open, setOpen] = useState(false);
  return (
    <>
      <button type="button" onClick={() => setOpen(true)}>
        Mở hộp
      </button>
      <ConfirmDialog
        open={open}
        tieuDe="Duyệt phiếu yêu cầu"
        tomTat="Phiếu PYC-0001 của 23520001"
        heQua="Giữ chỗ 30 phút và thông báo cho sinh viên."
        nhanXacNhan="Duyệt"
        bienThe="primary"
        dangXuLy={dangXuLy}
        onXacNhan={onXacNhan}
        onDong={() => {
          onDong?.();
          setOpen(false);
        }}
      />
    </>
  );
}

function moHop() {
  const nutMo = screen.getByRole("button", { name: "Mở hộp" });
  nutMo.focus();
  fireEvent.click(nutMo);
  const hop = document.querySelector("dialog");
  if (hop === null) throw new Error("Không có <dialog>");
  return { nutMo, hop };
}

function nhanEsc(hop: HTMLDialogElement) {
  const suKien = new Event("cancel", { cancelable: true });
  fireEvent(hop, suKien);
  return suKien;
}

describe("ConfirmDialog", () => {
  it("CMP-26: mở → tiêu đề là tên hành động, mô tả trỏ tới hệ quả, tiêu điểm ở nút Hủy", () => {
    render(<HopMau />);
    const { hop } = moHop();

    expect(hop.open).toBe(true);
    expect(
      screen.getByRole("heading", { name: "Duyệt phiếu yêu cầu" }),
    ).toBeTruthy();
    const idMoTa = hop.getAttribute("aria-describedby");
    expect(idMoTa).not.toBeNull();
    expect(document.getElementById(idMoTa ?? "")?.textContent).toBe(
      "Giữ chỗ 30 phút và thông báo cho sinh viên.",
    );
    expect(document.activeElement).toBe(
      screen.getByRole("button", { name: "Hủy" }),
    );
  });

  it("ĐX-DS-09: phím Enter không xác nhận hành động (không có form, mọi nút type=button)", () => {
    const onXacNhan = vi.fn();
    render(<HopMau onXacNhan={onXacNhan} />);
    const { hop } = moHop();

    fireEvent.keyDown(hop, { key: "Enter" });

    expect(onXacNhan).not.toHaveBeenCalled();
    expect(hop.querySelector("form")).toBeNull();
    for (const nut of hop.querySelectorAll("button")) {
      expect(nut.getAttribute("type")).toBe("button");
    }
  });

  it("CMP-26: Esc đóng hộp và trả tiêu điểm về nơi mở", () => {
    const onDong = vi.fn();
    render(<HopMau onDong={onDong} />);
    const { nutMo, hop } = moHop();

    const suKien = nhanEsc(hop);

    expect(suKien.defaultPrevented).toBe(true);
    expect(onDong).toHaveBeenCalledTimes(1);
    expect(hop.open).toBe(false);
    expect(document.activeElement).toBe(nutMo);
  });

  it("FA-08: đang xử lý → Esc và Hủy bị khóa, nút xác nhận vô hiệu hiện 'Đang xử lý'", () => {
    const onDong = vi.fn();
    render(<HopMau dangXuLy onDong={onDong} />);
    const { hop } = moHop();

    nhanEsc(hop);

    expect(onDong).not.toHaveBeenCalled();
    expect(hop.open).toBe(true);
    expect(screen.getByRole("button", { name: "Hủy" })).toHaveProperty(
      "disabled",
      true,
    );
    expect(screen.getByRole("button", { name: "Đang xử lý" })).toHaveProperty(
      "disabled",
      true,
    );
  });

  it("UI-Screens 3.5: nút xác nhận mang tên hành động và gọi onXacNhan", () => {
    const onXacNhan = vi.fn();
    render(<HopMau onXacNhan={onXacNhan} />);
    moHop();

    fireEvent.click(screen.getByRole("button", { name: "Duyệt" }));

    expect(onXacNhan).toHaveBeenCalledTimes(1);
  });
});
