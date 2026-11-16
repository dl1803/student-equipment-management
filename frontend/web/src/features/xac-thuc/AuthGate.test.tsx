// @vitest-environment jsdom
import { QueryClientProvider } from "@tanstack/react-query";
import {
  act,
  fireEvent,
  render,
  screen,
  waitFor,
} from "@testing-library/react";
import { StrictMode } from "react";
import { afterEach, beforeEach, describe, expect, it, vi } from "vitest";
import { apiClient } from "@/shared/api/client";
import { traVe, type BoChuyenDoi } from "@/shared/api/giaLapHttp";
import { taoQueryClient } from "@/shared/api/queryClient";
import { khachPhien } from "@/shared/api/session/lamMoi";
import { ghiCapToken, xoaPhien } from "@/shared/api/session/phien";
import type { ThongTinToi } from "@/shared/api/types";
import { useAuth } from "@/shared/hooks/useAuth";
import { NHAN_VIEN_MAU, QUAN_TRI_MAU } from "@/shared/testing/nguCanhKiemThu";
import { AuthGate } from "./AuthGate";
import { AuthProvider } from "./AuthProvider";

const dieuHuong = vi.hoisted(() => {
  const replace = vi.fn();
  return { tuyen: "/", router: { replace, push: vi.fn() }, replace };
});

vi.mock("next/navigation", () => ({
  usePathname: () => dieuHuong.tuyen,
  useRouter: () => dieuHuong.router,
}));

const TOKEN_MOI = {
  accessToken: "at-moi",
  refreshToken: "rt-moi",
  vaiTro: "NHAN_VIEN",
};

function adapterLamMoi(): ReturnType<typeof vi.fn<BoChuyenDoi>> {
  return vi.fn<BoChuyenDoi>((config) => traVe(config, 200, TOKEN_MOI));
}

function adapterToi(
  nguoiDung: ThongTinToi,
): ReturnType<typeof vi.fn<BoChuyenDoi>> {
  return vi.fn<BoChuyenDoi>((config) => traVe(config, 200, nguoiDung));
}

/** Hiển thị trạng thái phiên để kiểm tra ở /dang-nhap (nơi AuthGate cho dựng khi chưa đăng nhập). */
function HienTrangThai() {
  const { trangThai } = useAuth();
  return <pre data-testid="trang-thai">{JSON.stringify(trangThai)}</pre>;
}

function dungUngDung(noiDung = <p>Nội dung được bảo vệ</p>) {
  const queryClient = taoQueryClient();
  render(
    <StrictMode>
      <QueryClientProvider client={queryClient}>
        <AuthProvider>
          <AuthGate>{noiDung}</AuthGate>
        </AuthProvider>
      </QueryClientProvider>
    </StrictMode>,
  );
  return { queryClient };
}

beforeEach(() => {
  vi.stubEnv("NEXT_PUBLIC_API_BASE_URL", "https://api.test");
  xoaPhien();
  dieuHuong.tuyen = "/";
  dieuHuong.replace.mockClear();
});

afterEach(() => {
  vi.unstubAllEnvs();
});

describe("AuthProvider + AuthGate", () => {
  it("UF-CM-03, FA 4.5: Strict Mode chạy effect hai lần vẫn chỉ một lần làm mới và một lần GET /auth/toi", async () => {
    ghiCapToken({ accessToken: "at-cu", refreshToken: "rt-cu" });
    const lamMoi = adapterLamMoi();
    const toi = adapterToi(NHAN_VIEN_MAU);
    khachPhien.defaults.adapter = lamMoi;
    apiClient.defaults.adapter = toi;

    dungUngDung();

    await waitFor(() => expect(dieuHuong.replace).toHaveBeenCalledWith("/nv"));
    expect(lamMoi).toHaveBeenCalledTimes(1);
    expect(toi).toHaveBeenCalledTimes(1);
    expect(toi.mock.calls[0]?.[0].url).toBe("/auth/toi");
  });

  it("UI-Screens 4.6: trong lúc kiểm tra phiên chỉ có chỉ báo đang tải, không dựng nội dung", async () => {
    ghiCapToken({ accessToken: "at-cu", refreshToken: "rt-cu" });
    khachPhien.defaults.adapter = adapterLamMoi();
    apiClient.defaults.adapter = adapterToi(NHAN_VIEN_MAU);
    dieuHuong.tuyen = "/nv";

    dungUngDung();

    expect(screen.getByRole("status").getAttribute("aria-busy")).toBe("true");
    expect(screen.queryByText("Nội dung được bảo vệ")).toBeNull();
    expect(await screen.findByText("Nội dung được bảo vệ")).toBeTruthy();
    expect(dieuHuong.replace).not.toHaveBeenCalled(); // Ở lại tuyến hợp lệ hiện tại
  });

  it("UF-CM-03: tab chưa có phiên → chuyển /dang-nhap, không gọi làm mới, không dựng nội dung", async () => {
    const lamMoi = adapterLamMoi();
    khachPhien.defaults.adapter = lamMoi;
    dieuHuong.tuyen = "/nv/phieu-yeu-cau";

    dungUngDung();

    await waitFor(() =>
      expect(dieuHuong.replace).toHaveBeenCalledWith("/dang-nhap"),
    );
    expect(lamMoi).not.toHaveBeenCalled();
    expect(screen.queryByText("Nội dung được bảo vệ")).toBeNull();
  });

  it("ĐX-FA-23: làm mới thất bại do 429 → chưa đăng nhập, giữ nguyên lỗi 429 và traceId", async () => {
    ghiCapToken({ accessToken: "at-cu", refreshToken: "rt-cu" });
    khachPhien.defaults.adapter = vi.fn<BoChuyenDoi>((config) =>
      traVe(config, 429, {
        status: 429,
        maLoi: "QUA_NHIEU_YEU_CAU",
        traceId: "trace-429",
      }),
    );
    dieuHuong.tuyen = "/dang-nhap";

    dungUngDung(<HienTrangThai />);

    const trangThai = await screen.findByTestId("trang-thai");
    expect(JSON.parse(trangThai.textContent ?? "{}")).toEqual({
      loai: "CHUA_DANG_NHAP",
      lyDo: "HET_PHIEN",
      loi: {
        loai: "HTTP",
        status: 429,
        maLoi: "QUA_NHIEU_YEU_CAU",
        traceId: "trace-429",
      },
    });
  });

  it("FA 4.4: đã có phiên mà mở /dang-nhap → chuyển tới trang chủ của vai trò", async () => {
    ghiCapToken({ accessToken: "at-cu", refreshToken: "rt-cu" });
    khachPhien.defaults.adapter = adapterLamMoi();
    apiClient.defaults.adapter = adapterToi(QUAN_TRI_MAU);
    dieuHuong.tuyen = "/dang-nhap";

    dungUngDung();

    await waitFor(() => expect(dieuHuong.replace).toHaveBeenCalledWith("/qt"));
  });

  it("FA 3.5: phiên kết thúc khi đang dùng → nội dung bị gỡ ngay, cache bị xóa, chuyển /dang-nhap", async () => {
    ghiCapToken({ accessToken: "at-cu", refreshToken: "rt-cu" });
    khachPhien.defaults.adapter = adapterLamMoi();
    apiClient.defaults.adapter = adapterToi(NHAN_VIEN_MAU);
    dieuHuong.tuyen = "/nv";
    const { queryClient } = dungUngDung();
    await screen.findByText("Nội dung được bảo vệ");
    queryClient.setQueryData(["quay", "phieu-yeu-cau"], ["du lieu phien cu"]);

    act(() => {
      xoaPhien("HET_PHIEN");
    });

    expect(screen.queryByText("Nội dung được bảo vệ")).toBeNull();
    await waitFor(() =>
      expect(dieuHuong.replace).toHaveBeenCalledWith("/dang-nhap"),
    );
  });

  it("FA 4.5: GET /auth/toi lỗi sau khi có token → báo lỗi kèm Thử lại, không đăng xuất; thử lại được thì vào phiên", async () => {
    ghiCapToken({ accessToken: "at-cu", refreshToken: "rt-cu" });
    khachPhien.defaults.adapter = adapterLamMoi();
    const toi = vi
      .fn<BoChuyenDoi>()
      .mockImplementationOnce((config) =>
        traVe(config, 500, { status: 500, traceId: "trace-500" }),
      )
      .mockImplementation((config) => traVe(config, 200, NHAN_VIEN_MAU));
    apiClient.defaults.adapter = toi;
    dieuHuong.tuyen = "/nv";

    dungUngDung();

    expect(await screen.findByText("trace-500")).toBeTruthy();
    expect(dieuHuong.replace).not.toHaveBeenCalledWith("/dang-nhap");
    fireEvent.click(screen.getByRole("button", { name: "Thử lại" }));
    expect(await screen.findByText("Nội dung được bảo vệ")).toBeTruthy();
  });

  it("NFR-01: vai trò không dùng web (SINH_VIEN) → đăng xuất, không vào nội dung", async () => {
    ghiCapToken({ accessToken: "at-cu", refreshToken: "rt-cu" });
    khachPhien.defaults.adapter = vi.fn<BoChuyenDoi>((config) =>
      traVe(config, 200, config.url === "/auth/dang-xuat" ? {} : TOKEN_MOI),
    );
    apiClient.defaults.adapter = adapterToi({
      ...NHAN_VIEN_MAU,
      vaiTro: "SINH_VIEN",
    });
    dieuHuong.tuyen = "/nv";

    dungUngDung();

    await waitFor(() =>
      expect(dieuHuong.replace).toHaveBeenCalledWith("/dang-nhap"),
    );
    expect(screen.queryByText("Nội dung được bảo vệ")).toBeNull();
  });
});
