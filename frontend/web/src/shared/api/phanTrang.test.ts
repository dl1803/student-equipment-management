import { describe, expect, it } from "vitest";
import { soTrang, trangCanLuiVe } from "./phanTrang";

describe("phanTrang", () => {
  it("soTrang: làm tròn lên, ít nhất 1", () => {
    expect(soTrang(0, 20)).toBe(1);
    expect(soTrang(20, 20)).toBe(1);
    expect(soTrang(21, 20)).toBe(2);
  });

  it("trangCanLuiVe: trang vượt số trang → trang cuối; còn hợp lệ → null", () => {
    expect(trangCanLuiVe(2, 20, 20)).toBe(1);
    expect(trangCanLuiVe(3, 40, 20)).toBe(2);
    expect(trangCanLuiVe(2, 0, 20)).toBe(1);
    expect(trangCanLuiVe(2, 21, 20)).toBeNull();
    expect(trangCanLuiVe(1, 0, 20)).toBeNull();
  });
});
