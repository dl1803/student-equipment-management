// @vitest-environment jsdom
import { screen } from "@testing-library/react";
import { describe, expect, it } from "vitest";
import {
  hienThiVoiNguCanh,
  QUAN_TRI_MAU,
} from "@/shared/testing/nguCanhKiemThu";
import { TrangChuQuanTriScreen } from "./TrangChuQuanTriScreen";

describe("TrangChuQuanTriScreen", () => {
  it("UI-Screens 7.1: lời chào kèm họ tên, nhóm Danh mục và Cấu hình, đủ 11 lối vào có biểu tượng; không có chức năng quầy", () => {
    hienThiVoiNguCanh(<TrangChuQuanTriScreen />, { nguoiDung: QUAN_TRI_MAU });

    expect(
      screen.getByRole("heading", {
        level: 1,
        name: `Xin chào, ${QUAN_TRI_MAU.hoTen}`,
      }),
    ).toBeTruthy();
    expect(screen.getByRole("heading", { name: "Danh mục" })).toBeTruthy();
    expect(screen.getByRole("heading", { name: "Cấu hình" })).toBeTruthy();
    const dsLienKet = screen.getAllByRole("link");
    const dsHref = dsLienKet.map((a) => a.getAttribute("href"));
    expect(dsHref).toHaveLength(11);
    expect(dsHref.every((h) => h?.startsWith("/qt/"))).toBe(true);
    expect(
      dsLienKet.every(
        (a) => a.querySelectorAll('svg[aria-hidden="true"]').length === 2,
      ),
    ).toBe(true);
    expect(screen.queryByText("Giao thiết bị")).toBeNull();
  });
});
