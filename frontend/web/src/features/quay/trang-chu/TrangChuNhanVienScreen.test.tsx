// @vitest-environment jsdom
// Ví dụ kiểm thử màn hình bằng fixture (bàn giao D04-T, xem BAN_GIAO_CORE.md).
import { screen, within } from "@testing-library/react";
import { describe, expect, it } from "vitest";
import {
  hienThiVoiNguCanh,
  NHAN_VIEN_MAU,
} from "@/shared/testing/nguCanhKiemThu";
import { TrangChuNhanVienScreen } from "./TrangChuNhanVienScreen";

describe("TrangChuNhanVienScreen", () => {
  it("UI-Screens 6.1: hiện họ tên và danh sách phòng được cấp quyền (tên, cơ sở)", () => {
    hienThiVoiNguCanh(<TrangChuNhanVienScreen />, { nguoiDung: NHAN_VIEN_MAU });

    expect(screen.getByText(NHAN_VIEN_MAU.hoTen)).toBeTruthy();
    expect(screen.getByText("Phòng thiết bị A1")).toBeTruthy();
    expect(screen.getByText("Cơ sở Linh Trung")).toBeTruthy();
  });

  it("UI-Screens 6.1: dsPhong rỗng → thông báo liên hệ quản trị viên, lối vào vẫn hiện", () => {
    hienThiVoiNguCanh(<TrangChuNhanVienScreen />, {
      nguoiDung: { ...NHAN_VIEN_MAU, dsPhong: [] },
    });

    expect(
      screen.getByText(
        "Bạn chưa được cấp quyền phòng thiết bị nào; liên hệ quản trị viên",
      ),
    ).toBeTruthy();
    expect(screen.getAllByRole("link")).toHaveLength(7);
  });

  it("UI-Screens 6.1: bảy lối vào chức năng quầy, đúng tuyến", () => {
    hienThiVoiNguCanh(<TrangChuNhanVienScreen />);
    const khu = screen.getByRole("region", { name: "Chức năng" });

    const dsLienKet = within(khu).getAllByRole("link");
    expect(dsLienKet.map((a) => a.getAttribute("href"))).toEqual([
      "/nv/phieu-yeu-cau",
      "/nv/giao-thiet-bi/the",
      "/nv/nhan-tra",
      "/nv/bien-ban",
      "/nv/qua-han-30-ngay",
      "/nv/danh-sach-cho",
      "/nv/bao-tri",
    ]);
  });
});
