import { describe, expect, it, vi } from "vitest";
import {
  KICH_THUOC_TRANG_TOI_DA,
  soTrang,
  taiTatCaTrang,
  trangCanLuiVe,
} from "./phanTrang";

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

describe("taiTatCaTrang (nguồn lựa chọn phân trang đủ)", () => {
  const trangGia = (tongSo: number) =>
    vi.fn(async (trang: number, kichThuoc: number) => {
      const tu = (trang - 1) * kichThuoc;
      const den = Math.min(tu + kichThuoc, tongSo);
      return {
        tongSo,
        danhSach: Array.from(
          { length: Math.max(0, den - tu) },
          (_, i) => tu + i,
        ),
      };
    });

  it("một trang: chỉ gọi trang 1 với kích thước tối đa 100", async () => {
    const goi = trangGia(7);
    expect(await taiTatCaTrang(goi)).toHaveLength(7);
    expect(goi.mock.calls).toEqual([[1, KICH_THUOC_TRANG_TOI_DA]]);
  });

  it("250 mục: gọi lần lượt 3 trang, giữ đúng thứ tự máy chủ", async () => {
    const goi = trangGia(250);
    const kq = await taiTatCaTrang(goi);
    expect(goi.mock.calls.map((c) => c[0])).toEqual([1, 2, 3]);
    expect(kq).toHaveLength(250);
    expect(kq[0]).toBe(0);
    expect(kq[249]).toBe(249);
  });

  it("trang giữa chừng rỗng (dữ liệu bị xóa bớt) thì dừng, không lặp vô hạn", async () => {
    const goi = vi.fn(async (trang: number) =>
      trang === 1
        ? { tongSo: 300, danhSach: [1, 2] }
        : { tongSo: 2, danhSach: [] as number[] },
    );
    expect(await taiTatCaTrang(goi)).toEqual([1, 2]);
    expect(goi).toHaveBeenCalledTimes(2);
  });

  it("lỗi ở một trang thì ném lỗi đó (truy vấn ở trạng thái lỗi, không trả thiếu)", async () => {
    const goi = vi.fn(async (trang: number) => {
      if (trang === 2) throw new Error("mạng");
      return { tongSo: 150, danhSach: [1] };
    });
    await expect(taiTatCaTrang(goi)).rejects.toThrow("mạng");
  });
});
