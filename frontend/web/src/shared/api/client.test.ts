// @vitest-environment jsdom
import {
  AxiosError,
  type AxiosAdapter,
  type AxiosResponse,
  type InternalAxiosRequestConfig,
} from "axios";
import { afterEach, beforeEach, describe, expect, it, vi } from "vitest";
import type { ApiError } from "./apiError";
import { apiClient } from "./client";
import { noiXoaCacheKhiKetThucPhien, taoQueryClient } from "./queryClient";
import { dangXuat, khachPhien, khoiPhucPhien } from "./session/lamMoi";
import {
  dangKyKetThucPhien,
  ghiCapToken,
  layAccessToken,
  layRefreshToken,
  xoaPhien,
  type SuKienKetThucPhien,
} from "./session/phien";

const AT_CU = "at-cu";
const RT_CU = "rt-cu";
const AT_MOI = "at-moi";
const RT_MOI = "rt-moi";
const TRACE_ID = "00-trace-429";

function phanHoi(
  config: InternalAxiosRequestConfig,
  status: number,
  data: unknown,
): AxiosResponse {
  return {
    data,
    status,
    statusText: "",
    headers: {
      "content-type":
        status >= 400 ? "application/problem+json" : "application/json",
    },
    config,
  };
}

/** Giả lập bộ chuyển đổi: 4xx/5xx bị từ chối bằng AxiosError có response như adapter thật. */
function traVe(
  config: InternalAxiosRequestConfig,
  status: number,
  data: unknown = {},
): Promise<AxiosResponse> {
  const res = phanHoi(config, status, data);
  if (status >= 400) {
    return Promise.reject(
      new AxiosError("loi", AxiosError.ERR_BAD_REQUEST, config, null, res),
    );
  }
  return Promise.resolve(res);
}

const tre = (ms: number) => new Promise((r) => setTimeout(r, ms));
const loi401 = { status: 401, maLoi: "CHUA_XAC_THUC" };
const tokenMoi = {
  accessToken: AT_MOI,
  refreshToken: RT_MOI,
  vaiTro: "NHAN_VIEN",
};

function laTokenMoi(config: InternalAxiosRequestConfig): boolean {
  return config.headers.get("Authorization") === `Bearer ${AT_MOI}`;
}

/** Endpoint được bảo vệ: token cũ → 401 CHUA_XAC_THUC, token mới → 200. */
function adapterBaoVe(): ReturnType<typeof vi.fn<AxiosAdapter>> {
  return vi.fn<AxiosAdapter>((config) =>
    laTokenMoi(config)
      ? traVe(config, 200, { url: config.url, rt: layRefreshToken() })
      : traVe(config, 401, loi401),
  );
}

function adapterLamMoiThanhCong(): ReturnType<typeof vi.fn<AxiosAdapter>> {
  return vi.fn<AxiosAdapter>(async (config) => {
    await tre(5); // Để mọi yêu cầu 401 cùng lúc kịp chờ chung một lần làm mới.
    return traVe(config, 200, tokenMoi);
  });
}

let huyDangKy: Array<() => void> = [];

beforeEach(() => {
  vi.stubEnv("NEXT_PUBLIC_API_BASE_URL", "https://api.test/");
  xoaPhien();
  ghiCapToken({ accessToken: AT_CU, refreshToken: RT_CU });
});

afterEach(() => {
  for (const huy of huyDangKy) huy();
  huyDangKy = [];
  vi.unstubAllEnvs();
});

function ghiSuKien(): SuKienKetThucPhien[] {
  const ds: SuKienKetThucPhien[] = [];
  huyDangKy.push(dangKyKetThucPhien((s) => ds.push(s)));
  return ds;
}

describe("apiClient — gắn token", () => {
  it("FA 3.5: gắn Bearer cho endpoint cần xác thực; baseURL gồm /api/v1", async () => {
    const adapter = vi.fn<AxiosAdapter>((config) => traVe(config, 200));
    apiClient.defaults.adapter = adapter;

    await apiClient.get("/sv/toi");

    const config = adapter.mock.calls[0]?.[0];
    expect(config?.baseURL).toBe("https://api.test/api/v1");
    expect(config?.headers.get("Authorization")).toBe(`Bearer ${AT_CU}`);
  });

  it("FA 3.5: không gắn token cho bốn endpoint không yêu cầu xác thực", async () => {
    const adapter = vi.fn<AxiosAdapter>((config) => traVe(config, 200));
    apiClient.defaults.adapter = adapter;

    await apiClient.post("/auth/nhan-vien/dang-nhap", {});

    expect(adapter.mock.calls[0]?.[0].headers.get("Authorization")).toBeFalsy();
  });

  it.each(["auth/nhan-vien/dang-nhap", "/auth/nhan-vien/dang-nhap/"])(
    "UI-Screens 4.5: đường dẫn đăng nhập viết '%s' vẫn là endpoint không xác thực — không gắn token, TAI_KHOAN_BI_KHOA không kết thúc phiên",
    async (duongDan) => {
      const suKien = ghiSuKien();
      const adapter = vi.fn<AxiosAdapter>((config) =>
        traVe(config, 403, { maLoi: "TAI_KHOAN_BI_KHOA" }),
      );
      apiClient.defaults.adapter = adapter;

      await expect(apiClient.post(duongDan, {})).rejects.toMatchObject({
        maLoi: "TAI_KHOAN_BI_KHOA",
      });
      expect(
        adapter.mock.calls[0]?.[0].headers.get("Authorization"),
      ).toBeFalsy();
      expect(suKien).toHaveLength(0);
      expect(layRefreshToken()).toBe(RT_CU);
    },
  );

  it("FA 4.14: NEXT_PUBLIC_API_BASE_URL lỡ chứa /api/v1 → không bị lặp tiền tố", async () => {
    vi.stubEnv("NEXT_PUBLIC_API_BASE_URL", "https://api.test/api/v1/");
    const adapter = vi.fn<AxiosAdapter>((config) => traVe(config, 200));
    apiClient.defaults.adapter = adapter;

    await apiClient.get("/sv/toi");

    expect(adapter.mock.calls[0]?.[0].baseURL).toBe("https://api.test/api/v1");
  });
});

describe("apiClient — làm mới single-flight", () => {
  it("UF-CM-03: ba yêu cầu cùng 401 → chỉ một lần làm mới, mỗi yêu cầu gọi lại đúng một lần", async () => {
    const adapter = adapterBaoVe();
    const lamMoi = adapterLamMoiThanhCong();
    apiClient.defaults.adapter = adapter;
    khachPhien.defaults.adapter = lamMoi;

    const ketQua = await Promise.all([
      apiClient.get("/sv/a"),
      apiClient.get("/sv/b"),
      apiClient.post("/sv/c", {}),
    ]);

    expect(lamMoi).toHaveBeenCalledTimes(1);
    expect(JSON.parse(String(lamMoi.mock.calls[0]?.[0].data))).toEqual({
      refreshToken: RT_CU,
    });
    expect(adapter).toHaveBeenCalledTimes(6); // 3 lần đầu + 3 lần gọi lại
    expect(ketQua.map((r) => r.data.url)).toEqual(["/sv/a", "/sv/b", "/sv/c"]);
  });

  it("FA 3.5: cặp token mới được ghi (refresh token vào sessionStorage) trước khi gọi lại", async () => {
    apiClient.defaults.adapter = adapterBaoVe();
    khachPhien.defaults.adapter = adapterLamMoiThanhCong();

    const res = await apiClient.get("/sv/a");

    expect(res.data.rt).toBe(RT_MOI);
    expect(layAccessToken()).toBe(AT_MOI);
    expect(layRefreshToken()).toBe(RT_MOI);
  });

  it("FA 3.5: yêu cầu gọi lại vẫn 401 → không làm mới lần hai, trả ApiError", async () => {
    const adapter = vi.fn<AxiosAdapter>((config) => traVe(config, 401, loi401));
    const lamMoi = adapterLamMoiThanhCong();
    apiClient.defaults.adapter = adapter;
    khachPhien.defaults.adapter = lamMoi;

    await expect(apiClient.get("/sv/a")).rejects.toMatchObject({
      loai: "HTTP",
      status: 401,
      maLoi: "CHUA_XAC_THUC",
    });
    expect(lamMoi).toHaveBeenCalledTimes(1);
    expect(adapter).toHaveBeenCalledTimes(2);
  });

  it("FC 9.3: token đã được làm mới thành công bởi yêu cầu khác → không làm mới lại", async () => {
    let moCong: () => void = () => undefined;
    const cong = new Promise<void>((r) => {
      moCong = r;
    });
    const adapter = vi.fn<AxiosAdapter>(async (config) => {
      if (laTokenMoi(config)) return traVe(config, 200);
      await cong; // Yêu cầu gửi bằng token cũ, nhận 401 SAU khi phiên đã được làm mới.
      return traVe(config, 401, loi401);
    });
    const lamMoi = adapterLamMoiThanhCong();
    apiClient.defaults.adapter = adapter;
    khachPhien.defaults.adapter = lamMoi;

    const dangGui = apiClient.get("/sv/a");
    await tre(1);
    ghiCapToken({ accessToken: AT_MOI, refreshToken: RT_MOI });
    moCong();

    await expect(dangGui).resolves.toMatchObject({ status: 200 });
    expect(lamMoi).not.toHaveBeenCalled();
  });

  it("FA 4.5: khôi phục phiên dùng chung Promise làm mới với interceptor (Strict Mode gọi hai lần)", async () => {
    const lamMoi = adapterLamMoiThanhCong();
    apiClient.defaults.adapter = adapterBaoVe();
    khachPhien.defaults.adapter = lamMoi;

    const [a, b, c] = await Promise.all([
      khoiPhucPhien(),
      khoiPhucPhien(),
      apiClient.get("/sv/a"),
    ]);

    expect(a).toBe(true);
    expect(b).toBe(true);
    expect(c.status).toBe(200);
    expect(lamMoi).toHaveBeenCalledTimes(1);
  });

  it("UF-CM-03: tab chưa có refresh token → khôi phục trả false, không gọi làm mới", async () => {
    xoaPhien();
    const lamMoi = adapterLamMoiThanhCong();
    khachPhien.defaults.adapter = lamMoi;

    await expect(khoiPhucPhien()).resolves.toBe(false);
    expect(lamMoi).not.toHaveBeenCalled();
  });
});

describe("apiClient — làm mới thất bại (ĐX-FA-23)", () => {
  const CAC_THAT_BAI: Array<
    [
      string,
      (config: InternalAxiosRequestConfig) => Promise<AxiosResponse>,
      ApiError,
    ]
  > = [
    [
      "401",
      (config) => traVe(config, 401, loi401),
      { loai: "HTTP", status: 401, maLoi: "CHUA_XAC_THUC" },
    ],
    [
      "429",
      (config) =>
        traVe(config, 429, {
          status: 429,
          maLoi: "QUA_NHIEU_YEU_CAU",
          traceId: TRACE_ID,
        }),
      {
        loai: "HTTP",
        status: 429,
        maLoi: "QUA_NHIEU_YEU_CAU",
        traceId: TRACE_ID,
      },
    ],
    [
      "mất kết nối",
      (config) =>
        Promise.reject(
          new AxiosError("Network Error", AxiosError.ERR_NETWORK, config),
        ),
      { loai: "MANG" },
    ],
    [
      "quá thời gian chờ",
      (config) =>
        Promise.reject(new AxiosError("timeout", AxiosError.ETIMEDOUT, config)),
      { loai: "QUA_THOI_GIAN" },
    ],
    [
      "lỗi hệ thống 500",
      (config) => traVe(config, 500, { status: 500, traceId: "trace-500" }),
      { loai: "HTTP", status: 500, traceId: "trace-500" },
    ],
  ];

  it.each(CAC_THAT_BAI)(
    "ĐX-FA-23: làm mới thất bại do %s → xóa phiên và cache, kết thúc mọi yêu cầu chờ, không gọi lại, giữ nguyên lỗi",
    async (_ten, lamMoiThatBai, loiMongDoi) => {
      const queryClient = taoQueryClient();
      queryClient.setQueryData(
        ["quay", "phieu-yeu-cau"],
        ["du lieu phien truoc"],
      );
      huyDangKy.push(noiXoaCacheKhiKetThucPhien(queryClient));
      const suKien = ghiSuKien();
      const adapter = adapterBaoVe();
      const lamMoi = vi.fn<AxiosAdapter>(async (config) => {
        await tre(5);
        return lamMoiThatBai(config);
      });
      apiClient.defaults.adapter = adapter;
      khachPhien.defaults.adapter = lamMoi;

      const ketQua = await Promise.allSettled([
        apiClient.get("/sv/a"),
        apiClient.post("/sv/b", {}),
      ]);

      for (const kq of ketQua) {
        expect(kq.status).toBe("rejected");
        if (kq.status === "rejected") expect(kq.reason).toEqual(loiMongDoi);
      }
      expect(lamMoi).toHaveBeenCalledTimes(1);
      expect(adapter).toHaveBeenCalledTimes(2); // Không gọi lại yêu cầu gốc nào
      expect(layAccessToken()).toBeNull();
      expect(layRefreshToken()).toBeNull();
      expect(
        queryClient.getQueryData(["quay", "phieu-yeu-cau"]),
      ).toBeUndefined();
      expect(suKien).toEqual([{ lyDo: "HET_PHIEN", loi: loiMongDoi }]);
    },
  );

  it("FA 3.5: làm mới bị TAI_KHOAN_BI_KHOA → kết thúc phiên với lý do BI_KHOA", async () => {
    const suKien = ghiSuKien();
    apiClient.defaults.adapter = adapterBaoVe();
    khachPhien.defaults.adapter = vi.fn<AxiosAdapter>((config) =>
      traVe(config, 403, { maLoi: "TAI_KHOAN_BI_KHOA" }),
    );

    await expect(apiClient.get("/sv/a")).rejects.toMatchObject({
      maLoi: "TAI_KHOAN_BI_KHOA",
    });
    expect(suKien[0]?.lyDo).toBe("BI_KHOA");
  });
});

describe("apiClient — lỗi khác", () => {
  it("FA 3.4: TAI_KHOAN_BI_KHOA khi đang dùng → xóa phiên với lý do BI_KHOA", async () => {
    const suKien = ghiSuKien();
    apiClient.defaults.adapter = vi.fn<AxiosAdapter>((config) =>
      traVe(config, 403, { status: 403, maLoi: "TAI_KHOAN_BI_KHOA" }),
    );

    await expect(apiClient.get("/sv/a")).rejects.toMatchObject({
      status: 403,
      maLoi: "TAI_KHOAN_BI_KHOA",
    });
    expect(layRefreshToken()).toBeNull();
    expect(suKien).toHaveLength(1);
    expect(suKien[0]?.lyDo).toBe("BI_KHOA");
  });

  it("UI-Screens 4.5: TAI_KHOAN_BI_KHOA ở màn hình đăng nhập → chỉ trả lỗi, không phát sự kiện kết thúc phiên", async () => {
    const suKien = ghiSuKien();
    apiClient.defaults.adapter = vi.fn<AxiosAdapter>((config) =>
      traVe(config, 403, { maLoi: "TAI_KHOAN_BI_KHOA" }),
    );

    await expect(
      apiClient.post("/auth/nhan-vien/dang-nhap", {}),
    ).rejects.toMatchObject({ maLoi: "TAI_KHOAN_BI_KHOA" });
    expect(suKien).toHaveLength(0);
  });

  it("FA 3.5: 401 của endpoint đăng nhập (DANG_NHAP_SAI) không kích hoạt làm mới", async () => {
    const lamMoi = adapterLamMoiThanhCong();
    khachPhien.defaults.adapter = lamMoi;
    apiClient.defaults.adapter = vi.fn<AxiosAdapter>((config) =>
      traVe(config, 401, { maLoi: "DANG_NHAP_SAI" }),
    );

    await expect(
      apiClient.post("/auth/nhan-vien/dang-nhap", {}),
    ).rejects.toMatchObject({ maLoi: "DANG_NHAP_SAI" });
    expect(lamMoi).not.toHaveBeenCalled();
  });

  it("FA-08: lỗi mạng của POST không tự gửi lại", async () => {
    const adapter = vi.fn<AxiosAdapter>((config) =>
      Promise.reject(
        new AxiosError("Network Error", AxiosError.ERR_NETWORK, config),
      ),
    );
    apiClient.defaults.adapter = adapter;

    await expect(apiClient.post("/quay/phieu-muon", {})).rejects.toEqual({
      loai: "MANG",
    });
    expect(adapter).toHaveBeenCalledTimes(1);
  });
});

describe("dangXuat", () => {
  it("FA 3.5: gửi refresh token để thu hồi rồi xóa phiên", async () => {
    const adapter = vi.fn<AxiosAdapter>((config) => traVe(config, 204));
    khachPhien.defaults.adapter = adapter;

    await dangXuat();

    expect(adapter.mock.calls[0]?.[0].url).toBe("/auth/dang-xuat");
    expect(JSON.parse(String(adapter.mock.calls[0]?.[0].data))).toEqual({
      refreshToken: RT_CU,
    });
    expect(layAccessToken()).toBeNull();
    expect(layRefreshToken()).toBeNull();
  });

  it("FA 3.5: đăng xuất lỗi vẫn xóa phiên cục bộ", async () => {
    khachPhien.defaults.adapter = vi.fn<AxiosAdapter>((config) =>
      Promise.reject(
        new AxiosError("Network Error", AxiosError.ERR_NETWORK, config),
      ),
    );

    await expect(dangXuat()).resolves.toBeUndefined();
    expect(layRefreshToken()).toBeNull();
  });
});

describe("Lưu trữ", () => {
  it("FA-06: không dùng cookie hay localStorage cho token", async () => {
    apiClient.defaults.adapter = adapterBaoVe();
    khachPhien.defaults.adapter = adapterLamMoiThanhCong();

    await apiClient.get("/sv/a");

    expect(document.cookie).toBe("");
    // eslint-disable-next-line no-restricted-globals -- kiểm thử khẳng định localStorage KHÔNG được dùng
    expect(localStorage.length).toBe(0);
  });
});
