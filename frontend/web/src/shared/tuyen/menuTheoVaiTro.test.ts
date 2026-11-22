import { describe, expect, it } from "vitest";
import { BAN_DO_TUYEN } from "./banDoTuyen";
import {
  loiVaoCua,
  maManHinhTrongMenu,
  menuCua,
  tuyenTrangChu,
} from "./menuTheoVaiTro";

describe("menuTheoVaiTro", () => {
  it("UI-Screens 4.2: menu NV đúng thứ tự, chỉ gồm màn hình của NV", () => {
    expect(maManHinhTrongMenu("NHAN_VIEN")).toEqual([
      "SCR-NV-01",
      "SCR-NV-02",
      "SCR-NV-04",
      "SCR-NV-08",
      "SCR-NV-11",
      "SCR-NV-12",
      "SCR-NV-13",
      "SCR-NV-14",
    ]);
    for (const ma of maManHinhTrongMenu("NHAN_VIEN")) {
      expect(BAN_DO_TUYEN[ma].vaiTro, ma).toBe("NHAN_VIEN");
    }
  });

  it("UI-Screens 4.3: menu QT đủ 12 màn hình, có nhóm Danh mục và Cấu hình, không có chức năng quầy", () => {
    const dsMa = maManHinhTrongMenu("QUAN_TRI");
    expect(dsMa).toHaveLength(12);
    for (const ma of dsMa) expect(BAN_DO_TUYEN[ma].vaiTro, ma).toBe("QUAN_TRI");
    expect(menuCua("QUAN_TRI").map((nhom) => nhom.tieuDe)).toEqual([
      undefined,
      "Danh mục",
      "Cấu hình",
      undefined,
    ]);
  });

  it("FA 4.4: menu không chứa mục ngoài vai trò; href lấy từ bản đồ tuyến", () => {
    const hrefNv = menuCua("NHAN_VIEN").flatMap((n) =>
      n.dsMuc.map((m) => m.href),
    );
    expect(hrefNv.every((h) => h === "/nv" || h.startsWith("/nv/"))).toBe(true);
    const hrefQt = menuCua("QUAN_TRI").flatMap((n) =>
      n.dsMuc.map((m) => m.href),
    );
    expect(hrefQt.every((h) => h === "/qt" || h.startsWith("/qt/"))).toBe(true);
  });

  it("ĐX-FA-19: mục Giao thiết bị mang tiền tố nhóm của bốn bước, mục khác thì không", () => {
    const dsMuc = menuCua("NHAN_VIEN").flatMap((n) => n.dsMuc);
    const giaoThietBi = dsMuc.find((m) => m.nhan === "Giao thiết bị");
    expect(giaoThietBi?.tienToNhom).toBe("/nv/giao-thiet-bi");
    for (const ma of ["SCR-NV-05", "SCR-NV-06", "SCR-NV-07"] as const) {
      expect(BAN_DO_TUYEN[ma].tuyen.startsWith("/nv/giao-thiet-bi/")).toBe(
        true,
      );
    }
    expect(dsMuc.filter((m) => m.tienToNhom !== undefined)).toHaveLength(1);
  });

  it("UI-Screens 6.1, 7.1: lối vào ở trang chủ là menu trừ chính trang chủ", () => {
    expect(tuyenTrangChu("NHAN_VIEN")).toBe("/nv");
    expect(tuyenTrangChu("QUAN_TRI")).toBe("/qt");
    const loiVaoNv = loiVaoCua("NHAN_VIEN").flatMap((n) => n.dsMuc);
    expect(loiVaoNv).toHaveLength(7);
    expect(loiVaoNv.some((m) => m.href === "/nv")).toBe(false);
    expect(loiVaoCua("QUAN_TRI").flatMap((n) => n.dsMuc)).toHaveLength(11);
  });

  it("DS CMP-29: mọi mục menu của hai vai trò đều có biểu tượng", () => {
    for (const vaiTro of ["NHAN_VIEN", "QUAN_TRI"] as const) {
      for (const muc of menuCua(vaiTro).flatMap((n) => n.dsMuc)) {
        expect(muc.bieuTuong, muc.nhan).toBeTypeOf("function");
      }
    }
  });
});
