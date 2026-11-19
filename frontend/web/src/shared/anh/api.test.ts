// @vitest-environment jsdom
import { afterEach, beforeEach, describe, expect, it, vi } from "vitest";
import {
  ganBoChuyenDoi,
  traVe,
  type BoChuyenDoi,
} from "@/shared/api/giaLapHttp";
import { laApiError } from "@/shared/api/apiError";
import { anhTai } from "./api";

beforeEach(() => {
  vi.stubEnv("NEXT_PUBLIC_API_BASE_URL", "https://api.test");
});
afterEach(() => {
  vi.unstubAllEnvs();
});

describe("anhTai (POST /anh, FA 4.10 bước 5)", () => {
  it("gửi multipart với file và giaiDoan; trả đúng maAnh, không có URL", async () => {
    const api = vi.fn<BoChuyenDoi>((cfg) => traVe(cfg, 201, { maAnh: "a1b2" }));
    ganBoChuyenDoi({ api });
    const tep = new Blob(["jpg"], { type: "image/jpeg" });

    const kq = await anhTai(tep, "KHI_TRA");

    expect(kq).toEqual({ maAnh: "a1b2" });
    expect(api).toHaveBeenCalledOnce();
    const cfg = api.mock.calls[0]?.[0];
    expect(cfg?.method).toBe("post");
    expect(cfg?.url).toBe("/anh");
    const duLieu = cfg?.data;
    expect(duLieu).toBeInstanceOf(FormData);
    if (!(duLieu instanceof FormData)) return;
    expect(duLieu.get("giaiDoan")).toBe("KHI_TRA");
    const file = duLieu.get("file");
    expect(file).toBeInstanceOf(Blob);
    // Không tự đặt Content-Type để trình duyệt gắn boundary của multipart.
    expect(String(cfg?.headers.getContentType() ?? "")).not.toContain(
      "application/json",
    );
  });

  it("lỗi 400 thành ApiError và chỉ gọi một lần (không tự retry)", async () => {
    const api = vi.fn<BoChuyenDoi>((cfg) =>
      traVe(cfg, 400, {
        type: "about:blank",
        title: "Dữ liệu không hợp lệ",
        status: 400,
        maLoi: "DU_LIEU_KHONG_HOP_LE",
      }),
    );
    ganBoChuyenDoi({ api });
    const loi = await anhTai(new Blob(["x"]), "THIET_BI").catch(
      (e: unknown) => e,
    );
    expect(laApiError(loi)).toBe(true);
    expect(laApiError(loi) && loi.maLoi).toBe("DU_LIEU_KHONG_HOP_LE");
    expect(api).toHaveBeenCalledOnce();
  });
});
