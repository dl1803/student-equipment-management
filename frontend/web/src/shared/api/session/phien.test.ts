// @vitest-environment jsdom
import { afterEach, beforeEach, describe, expect, it, vi } from "vitest";
import {
  dangKyKetThucPhien,
  ghiCapToken,
  layAccessToken,
  layRefreshToken,
  xoaPhien,
} from "./phien";

beforeEach(() => {
  xoaPhien();
});

afterEach(() => {
  vi.restoreAllMocks();
});

describe("phien", () => {
  it("ĐX-FA-02: ghi cặp token → access token trong bộ nhớ, refresh token trong sessionStorage", () => {
    ghiCapToken({ accessToken: "at", refreshToken: "rt" });

    expect(layAccessToken()).toBe("at");
    expect(sessionStorage.getItem("refreshToken")).toBe("rt");
  });

  it("FA 3.5: sessionStorage không ghi được → phiên vẫn dùng được trong bộ nhớ, refresh token cũ bị xóa", () => {
    ghiCapToken({ accessToken: "at-cu", refreshToken: "rt-cu" });
    vi.spyOn(Storage.prototype, "setItem").mockImplementation(() => {
      throw new DOMException("Hết hạn ngạch", "QuotaExceededError");
    });

    expect(() =>
      ghiCapToken({ accessToken: "at-moi", refreshToken: "rt-moi" }),
    ).not.toThrow();
    expect(layAccessToken()).toBe("at-moi");
    expect(layRefreshToken()).toBeNull(); // Không giữ refresh token đã bị máy chủ xoay vòng
  });

  it("FA 4.5: một người lắng nghe ném lỗi không chặn những người lắng nghe sau", () => {
    const ghiLoi = vi
      .spyOn(console, "error")
      .mockImplementation(() => undefined);
    const nguoiSau = vi.fn();
    const huy1 = dangKyKetThucPhien(() => {
      throw new Error("lỗi người lắng nghe");
    });
    const huy2 = dangKyKetThucPhien(nguoiSau);

    xoaPhien("HET_PHIEN");

    expect(nguoiSau).toHaveBeenCalledWith({ lyDo: "HET_PHIEN" });
    expect(ghiLoi).toHaveBeenCalledTimes(1);
    huy1();
    huy2();
  });

  it("FC 5.3: hủy đăng ký thì không còn nhận sự kiện", () => {
    const langNghe = vi.fn();
    const huy = dangKyKetThucPhien(langNghe);
    huy();

    xoaPhien();

    expect(langNghe).not.toHaveBeenCalled();
  });
});
