import { describe, expect, it } from "vitest";
import { formatTien, formatTyLePhanTram } from "./formatSo";

const NBSP = " ";

describe("formatTien", () => {
  it("DS 9.7: số nguyên → dấu chấm hàng nghìn, khoảng trắng không ngắt U+00A0, VNĐ", () => {
    expect(formatTien(240000)).toBe(`240.000${NBSP}VNĐ`);
    expect(formatTien(1234567)).toBe(`1.234.567${NBSP}VNĐ`);
    expect(formatTien(999)).toBe(`999${NBSP}VNĐ`);
  });

  it("DS 9.7: 0 → 0 VNĐ", () => {
    expect(formatTien(0)).toBe(`0${NBSP}VNĐ`);
  });

  it("DS 9.7: khoảng trắng là U+00A0, không phải khoảng trắng thường", () => {
    expect(formatTien(240000)).not.toContain(" ");
  });

  it("ĐX-DS-12: null, undefined, NaN → —", () => {
    expect(formatTien(null)).toBe("—");
    expect(formatTien(undefined)).toBe("—");
    expect(formatTien(Number.NaN)).toBe("—");
  });
});

describe("formatTyLePhanTram", () => {
  it("DS 9.7: giữ phần thập phân, đổi dấu chấm thành dấu phẩy", () => {
    expect(formatTyLePhanTram(15.5)).toBe("15,5%");
    expect(formatTyLePhanTram(12.25)).toBe("12,25%");
  });

  it("DS 9.7: số nguyên không thêm phần thập phân, không nhân 100", () => {
    expect(formatTyLePhanTram(15)).toBe("15%");
    expect(formatTyLePhanTram(100)).toBe("100%");
  });

  it("ĐX-DS-12: null → —", () => {
    expect(formatTyLePhanTram(null)).toBe("—");
  });
});
