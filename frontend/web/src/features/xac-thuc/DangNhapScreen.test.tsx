// @vitest-environment jsdom
import { fireEvent, screen, waitFor } from "@testing-library/react";
import { afterEach, beforeEach, describe, expect, it, vi } from "vitest";
import type { ApiError } from "@/shared/api/apiError";
import { apiClient } from "@/shared/api/client";
import {
  traVe,
  type BoChuyenDoi,
  type CauHinhYeuCau,
  type PhanHoiHttp,
} from "@/shared/api/giaLapHttp";
import { khachPhien } from "@/shared/api/session/lamMoi";
import { layRefreshToken, xoaPhien } from "@/shared/api/session/phien";
import type { TrangThaiPhien } from "@/shared/api/session/trangThaiPhien";
import {
  hienThiVoiNguCanh,
  NHAN_VIEN_MAU,
} from "@/shared/testing/nguCanhKiemThu";
import { DangNhapScreen } from "./DangNhapScreen";

const MAT_KHAU = "mk-bi-mat-123";
const CHUA_DANG_NHAP: TrangThaiPhien = { loai: "CHUA_DANG_NHAP" };

const TOKEN = { accessToken: "at", refreshToken: "rt", vaiTro: "NHAN_VIEN" };

/** Đăng nhập trả `ketQuaDangNhap`; GET /auth/toi trả NHAN_VIEN_MAU. */
function adapter(
  ketQuaDangNhap: (config: CauHinhYeuCau) => Promise<PhanHoiHttp>,
): ReturnType<typeof vi.fn<BoChuyenDoi>> {
  return vi.fn<BoChuyenDoi>((config) =>
    config.url === "/auth/toi"
      ? traVe(config, 200, NHAN_VIEN_MAU)
      : ketQuaDangNhap(config),
  );
}

function nhapVaGui(tenDangNhap: string, matKhau: string) {
  fireEvent.change(screen.getByLabelText(/Tên đăng nhập/), {
    target: { value: tenDangNhap },
  });
  fireEvent.change(screen.getByLabelText(/Mật khẩu/), {
    target: { value: matKhau },
  });
  fireEvent.click(screen.getByRole("button", { name: "Đăng nhập" }));
}

function oMatKhau(): HTMLInputElement {
  const o = screen.getByLabelText(/Mật khẩu/);
  if (!(o instanceof HTMLInputElement)) throw new Error("Không phải input");
  return o;
}

function oTenDangNhap(): HTMLInputElement {
  const o = screen.getByLabelText(/Tên đăng nhập/);
  if (!(o instanceof HTMLInputElement)) throw new Error("Không phải input");
  return o;
}

beforeEach(() => {
  vi.stubEnv("NEXT_PUBLIC_API_BASE_URL", "https://api.test");
  xoaPhien();
});

afterEach(() => {
  vi.unstubAllEnvs();
});

describe("DangNhapScreen", () => {
  it("UI-Screens 4.5: nút Đăng nhập vô hiệu khi thiếu trường; không có liên kết đăng ký/quên mật khẩu", () => {
    hienThiVoiNguCanh(<DangNhapScreen />, { trangThai: CHUA_DANG_NHAP });
    const nut = screen.getByRole("button", { name: "Đăng nhập" });

    expect(nut).toHaveProperty("disabled", true);
    fireEvent.change(oTenDangNhap(), { target: { value: "nv.huong" } });
    expect(nut).toHaveProperty("disabled", true);
    fireEvent.change(oMatKhau(), { target: { value: MAT_KHAU } });
    expect(nut).toHaveProperty("disabled", false);
    expect(screen.queryAllByRole("link")).toHaveLength(0);
    expect(oMatKhau().type).toBe("password");
  });

  it("UF-CM-02: thành công → đăng nhập rồi GET /auth/toi, hoàn tất phiên; mật khẩu bị dọn khỏi ô và mutation", async () => {
    const goi = adapter((config) => traVe(config, 200, TOKEN));
    apiClient.defaults.adapter = goi;
    const hoanTatDangNhap = vi.fn();
    const { queryClient } = hienThiVoiNguCanh(<DangNhapScreen />, {
      trangThai: CHUA_DANG_NHAP,
      auth: { hoanTatDangNhap },
    });

    nhapVaGui("nv.huong", MAT_KHAU);

    await waitFor(() =>
      expect(hoanTatDangNhap).toHaveBeenCalledWith(NHAN_VIEN_MAU),
    );
    expect(goi.mock.calls.map((c) => c[0].url)).toEqual([
      "/auth/nhan-vien/dang-nhap",
      "/auth/toi",
    ]);
    expect(JSON.parse(String(goi.mock.calls[0]?.[0].data))).toEqual({
      tenDangNhap: "nv.huong",
      matKhau: MAT_KHAU,
    });
    await waitFor(() => expect(oMatKhau().value).toBe(""));
    await waitFor(() =>
      expect(queryClient.getMutationCache().getAll()).toHaveLength(0),
    );
  });

  it("UI-Screens 4.5 E1: DANG_NHAP_SAI → thông báo, giữ tên đăng nhập, xóa mật khẩu; mutation không còn giữ mật khẩu", async () => {
    apiClient.defaults.adapter = adapter((config) =>
      traVe(config, 401, { status: 401, maLoi: "DANG_NHAP_SAI" }),
    );
    const { queryClient } = hienThiVoiNguCanh(<DangNhapScreen />, {
      trangThai: CHUA_DANG_NHAP,
    });

    nhapVaGui("nv.huong", MAT_KHAU);

    const thongBao = await screen.findByText("Thông tin đăng nhập không đúng");
    expect(thongBao.closest("[role=alert]")).not.toBeNull();
    expect(oTenDangNhap().value).toBe("nv.huong");
    await waitFor(() => expect(oMatKhau().value).toBe(""));
    expect(document.body.textContent).not.toContain(MAT_KHAU);
    await waitFor(() =>
      expect(
        queryClient
          .getMutationCache()
          .getAll()
          .some((m) =>
            JSON.stringify(m.state.variables ?? null).includes(MAT_KHAU),
          ),
      ).toBe(false),
    );
  });

  it.each([
    [
      "TAI_KHOAN_BI_KHOA",
      403,
      "Tài khoản không đăng nhập được. Nhân viên liên hệ quản trị viên để mở lại; quản trị viên liên hệ bên vận hành hệ thống.",
      "danger",
    ],
    [
      "QUA_NHIEU_YEU_CAU",
      429,
      "Bạn đã thử quá nhiều lần. Vui lòng chờ một lát rồi thử lại.",
      "warning",
    ],
  ])(
    "UI-Screens 4.5: %s → thông báo đúng tông",
    async (maLoi, status, chu, tong) => {
      apiClient.defaults.adapter = adapter((config) =>
        traVe(config, status, { status, maLoi }),
      );
      hienThiVoiNguCanh(<DangNhapScreen />, { trangThai: CHUA_DANG_NHAP });

      nhapVaGui("nv.huong", MAT_KHAU);

      const thongBao = await screen.findByText(chu);
      expect(thongBao.closest("[data-tong]")?.getAttribute("data-tong")).toBe(
        tong,
      );
      await waitFor(() => expect(oMatKhau().value).toBe(""));
    },
  );

  it("FA 3.11: bấm Đăng nhập hai lần liên tiếp → chỉ một yêu cầu đăng nhập", async () => {
    const goi = adapter(
      (config) =>
        new Promise((r) => setTimeout(() => r(traVe(config, 200, TOKEN)), 10)),
    );
    apiClient.defaults.adapter = goi;
    hienThiVoiNguCanh(<DangNhapScreen />, { trangThai: CHUA_DANG_NHAP });

    nhapVaGui("nv.huong", MAT_KHAU);
    fireEvent.click(
      screen.getByRole("button", { name: /Đăng nhập|Đang xử lý/ }),
    );

    await waitFor(() =>
      expect(
        goi.mock.calls.filter((c) => c[0].url !== "/auth/toi"),
      ).toHaveLength(1),
    );
  });

  const LOI_429: ApiError = {
    loai: "HTTP",
    status: 429,
    maLoi: "QUA_NHIEU_YEU_CAU",
    traceId: "trace-429",
  };

  it.each<[string, TrangThaiPhien, string, string]>([
    [
      "hết phiên (401)",
      {
        loai: "CHUA_DANG_NHAP",
        lyDo: "HET_PHIEN",
        loi: { loai: "HTTP", status: 401, maLoi: "CHUA_XAC_THUC" },
      },
      "Phiên đăng nhập đã hết hạn, vui lòng đăng nhập lại",
      "info",
    ],
    [
      "429",
      { loai: "CHUA_DANG_NHAP", lyDo: "HET_PHIEN", loi: LOI_429 },
      "Hệ thống đang nhận quá nhiều yêu cầu nên phiên đã kết thúc. Vui lòng chờ một lát rồi đăng nhập lại.",
      "warning",
    ],
    [
      "mất kết nối",
      { loai: "CHUA_DANG_NHAP", lyDo: "HET_PHIEN", loi: { loai: "MANG" } },
      "Không kết nối được tới hệ thống nên phiên đã kết thúc. Kiểm tra kết nối rồi đăng nhập lại.",
      "danger",
    ],
    [
      "tài khoản bị khóa",
      { loai: "CHUA_DANG_NHAP", lyDo: "BI_KHOA" },
      "Tài khoản không đăng nhập được. Nhân viên liên hệ quản trị viên để mở lại; quản trị viên liên hệ bên vận hành hệ thống.",
      "danger",
    ],
  ])(
    "FA 3.5: về đăng nhập do %s → báo đúng nguyên nhân",
    (_ten, trangThai, chu, tong) => {
      hienThiVoiNguCanh(<DangNhapScreen />, { trangThai });

      const thongBao = screen.getByText(chu);
      expect(thongBao.closest("[data-tong]")?.getAttribute("data-tong")).toBe(
        tong,
      );
    },
  );

  it("FA 3.5: 429 khi làm mới giữ traceId để báo cho nhóm", () => {
    hienThiVoiNguCanh(<DangNhapScreen />, {
      trangThai: { loai: "CHUA_DANG_NHAP", lyDo: "HET_PHIEN", loi: LOI_429 },
    });
    expect(screen.getByText("trace-429")).toBeTruthy();
  });

  it("NFR-01: tài khoản không thuộc vai trò web (SINH_VIEN) → báo không đủ quyền, thu hồi phiên vừa tạo", async () => {
    apiClient.defaults.adapter = vi.fn<BoChuyenDoi>((config) =>
      config.url === "/auth/toi"
        ? traVe(config, 200, { ...NHAN_VIEN_MAU, vaiTro: "SINH_VIEN" })
        : traVe(config, 200, TOKEN),
    );
    const dangXuatApi = vi.fn<BoChuyenDoi>((config) => traVe(config, 204));
    khachPhien.defaults.adapter = dangXuatApi;
    const hoanTatDangNhap = vi.fn();
    hienThiVoiNguCanh(<DangNhapScreen />, {
      trangThai: CHUA_DANG_NHAP,
      auth: { hoanTatDangNhap },
    });

    nhapVaGui("sv.23520001", MAT_KHAU);

    const thongBao = await screen.findByText(
      "Bạn không có quyền thực hiện thao tác này",
    );
    expect(thongBao.closest("[data-tong]")?.getAttribute("data-tong")).toBe(
      "danger",
    );
    expect(hoanTatDangNhap).not.toHaveBeenCalled();
    expect(dangXuatApi.mock.calls[0]?.[0].url).toBe("/auth/dang-xuat");
    expect(layRefreshToken()).toBeNull();
    await waitFor(() => expect(oMatKhau().value).toBe(""));
  });

  it("UF-CM-03: đăng xuất chủ động → không hiện thông báo nào", () => {
    hienThiVoiNguCanh(<DangNhapScreen />, { trangThai: CHUA_DANG_NHAP });
    expect(screen.queryByRole("alert")).toBeNull();
    expect(screen.queryByRole("status")).toBeNull();
  });
});
