// @vitest-environment jsdom
import { fireEvent, screen } from "@testing-library/react";
import { beforeEach, describe, expect, it, vi } from "vitest";
import {
  hienThiVoiNguCanh,
  NHAN_VIEN_MAU,
  QUAN_TRI_MAU,
} from "@/shared/testing/nguCanhKiemThu";
import { KhungVaiTro } from "./KhungVaiTro";

const dieuHuong = vi.hoisted(() => {
  const replace = vi.fn();
  return { tuyen: "/nv", router: { replace, push: vi.fn() }, replace };
});

vi.mock("next/navigation", () => ({
  usePathname: () => dieuHuong.tuyen,
  useRouter: () => dieuHuong.router,
}));

beforeEach(() => {
  dieuHuong.tuyen = "/nv";
  dieuHuong.replace.mockClear();
});

describe("KhungVaiTro", () => {
  it("UI-Screens 4.2: khung NV có thanh đầu trang (họ tên, vai trò) và menu bên của NV", () => {
    hienThiVoiNguCanh(
      <KhungVaiTro vaiTro="NHAN_VIEN">
        <p>Nội dung</p>
      </KhungVaiTro>,
      { nguoiDung: NHAN_VIEN_MAU },
    );

    expect(screen.getByRole("banner").textContent).toContain(
      NHAN_VIEN_MAU.hoTen,
    );
    expect(screen.getByText("Nhân viên quản lý thiết bị")).toBeTruthy();
    const menu = screen.getByRole("navigation", { name: "Menu chính" });
    expect(menu.textContent).toContain("Giao thiết bị");
    expect(menu.textContent).not.toContain("Báo cáo");
    expect(screen.getByText("Nội dung")).toBeTruthy();
  });

  it("FA 4.4: NV mở /qt → chuyển về /nv kèm banner không đủ quyền, không dựng nội dung QT", () => {
    dieuHuong.tuyen = "/qt/bao-cao";
    const baoKhongDuQuyen = vi.fn();
    hienThiVoiNguCanh(
      <KhungVaiTro vaiTro="QUAN_TRI">
        <p>Báo cáo QT</p>
      </KhungVaiTro>,
      { nguoiDung: NHAN_VIEN_MAU, auth: { baoKhongDuQuyen } },
    );

    expect(screen.queryByText("Báo cáo QT")).toBeNull();
    expect(baoKhongDuQuyen).toHaveBeenCalledWith("/nv");
    expect(dieuHuong.replace).toHaveBeenCalledWith("/nv");
  });

  it("UI-Screens 3.2: trang chủ đích hiện banner 'Bạn không có quyền thực hiện thao tác này'", () => {
    hienThiVoiNguCanh(
      <KhungVaiTro vaiTro="NHAN_VIEN">
        <p>Trang chủ NV</p>
      </KhungVaiTro>,
      { nguoiDung: NHAN_VIEN_MAU, auth: { tuyenBaoKhongDuQuyen: "/nv" } },
    );

    expect(screen.getByRole("alert").textContent).toContain(
      "Bạn không có quyền thực hiện thao tác này",
    );
  });

  it("UF-CM-03: bấm Đăng xuất gọi dangXuat, không có hộp xác nhận", () => {
    dieuHuong.tuyen = "/qt";
    const dangXuat = vi.fn(async () => undefined);
    hienThiVoiNguCanh(
      <KhungVaiTro vaiTro="QUAN_TRI">
        <p>Trang chủ QT</p>
      </KhungVaiTro>,
      { nguoiDung: QUAN_TRI_MAU, auth: { dangXuat } },
    );

    fireEvent.click(screen.getByRole("button", { name: "Đăng xuất" }));

    expect(dangXuat).toHaveBeenCalledTimes(1);
    expect(document.querySelector("dialog")).toBeNull();
  });
});
