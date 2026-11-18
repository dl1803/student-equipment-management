import { describe, expect, it } from "vitest";
import { docSoNguyen } from "./docSo";

describe("docSoNguyen", () => {
  it("FC 5.6: chuỗi chữ số → số nguyên", () => {
    expect(docSoNguyen("0")).toBe(0);
    expect(docSoNguyen("240000")).toBe(240000);
    expect(docSoNguyen(" 15 ")).toBe(15);
  });

  it.each([
    "",
    " ",
    "-1",
    "15.5",
    "15,5",
    "1.000",
    "1e3",
    "12a",
    "+3",
    "9007199254740993",
  ])(
    "FC 5.6: '%s' → null (âm, thập phân, phân cách, ký tự lạ, vượt số nguyên an toàn)",
    (chuoi) => {
      expect(docSoNguyen(chuoi)).toBeNull();
    },
  );
});
