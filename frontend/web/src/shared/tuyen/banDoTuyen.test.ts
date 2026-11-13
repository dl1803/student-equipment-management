import { existsSync, readdirSync, statSync } from "node:fs";
import { join, relative, sep } from "node:path";
import { describe, expect, it } from "vitest";
import { BAN_DO_TUYEN, type MaManHinhWeb } from "./banDoTuyen";

const THU_MUC_APP = join(process.cwd(), "src", "app");

function tepPageCuaTuyen(tuyen: string): string {
  return join(THU_MUC_APP, ...tuyen.split("/").filter(Boolean), "page.tsx");
}

function tuyenCuaMoiPage(thuMuc: string): string[] {
  return readdirSync(thuMuc).flatMap((ten) => {
    const duongDan = join(thuMuc, ten);
    if (statSync(duongDan).isDirectory()) return tuyenCuaMoiPage(duongDan);
    if (ten !== "page.tsx") return [];
    const tuongDoi = relative(THU_MUC_APP, thuMuc).split(sep).filter(Boolean);
    return [`/${tuongDoi.join("/")}`];
  });
}

const DS_MA = Object.keys(BAN_DO_TUYEN) as MaManHinhWeb[];

const MA_MONG_DOI = [
  "SCR-CM-02",
  "SCR-CM-03",
  ...Array.from({ length: 14 }, (_, i) => `SCR-NV-${String(i + 1).padStart(2, "0")}`),
  ...Array.from({ length: 12 }, (_, i) => `SCR-QT-${String(i + 1).padStart(2, "0")}`),
];

describe("banDoTuyen", () => {
  it("FA 4.3: đủ đúng 28 màn hình web giai đoạn 1", () => {
    expect([...DS_MA].sort()).toEqual([...MA_MONG_DOI].sort());
  });

  it("FA 4.3: mỗi tuyến là duy nhất", () => {
    const dsTuyen = DS_MA.map((ma) => BAN_DO_TUYEN[ma].tuyen);
    expect(new Set(dsTuyen).size).toBe(dsTuyen.length);
  });

  it("FA 4.3: mỗi màn hình có page.tsx tại đúng tuyến", () => {
    for (const ma of DS_MA) {
      expect(existsSync(tepPageCuaTuyen(BAN_DO_TUYEN[ma].tuyen)), ma).toBe(true);
    }
  });

  it("FA 4.3 quy tắc 6: không có page.tsx nào ngoài bản đồ tuyến", () => {
    const dsTuyen = new Set<string>(DS_MA.map((ma) => BAN_DO_TUYEN[ma].tuyen));
    expect(tuyenCuaMoiPage(THU_MUC_APP).filter((t) => !dsTuyen.has(t))).toEqual([]);
  });

  it("ĐX-FA-19: tuyến NV có tiền tố /nv, tuyến QT có tiền tố /qt", () => {
    for (const ma of DS_MA) {
      const { tuyen, vaiTro } = BAN_DO_TUYEN[ma];
      if (vaiTro === "NHAN_VIEN") expect(tuyen === "/nv" || tuyen.startsWith("/nv/"), ma).toBe(true);
      if (vaiTro === "QUAN_TRI") expect(tuyen === "/qt" || tuyen.startsWith("/qt/"), ma).toBe(true);
    }
  });

  it("Phân công D01-T: Long sở hữu đúng SCR-QT-05..09, còn lại là Thịnh", () => {
    const cuaLong = DS_MA.filter((ma) => BAN_DO_TUYEN[ma].soHuu === "LONG").sort();
    expect(cuaLong).toEqual(["SCR-QT-05", "SCR-QT-06", "SCR-QT-07", "SCR-QT-08", "SCR-QT-09"]);
  });
});
