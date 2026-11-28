// @vitest-environment jsdom
// SCR-NV-04 và khung bốn bước Giao thiết bị bằng fixture HTTP (D13-T).
import { act, fireEvent, screen, waitFor } from "@testing-library/react";
import type { ReactElement } from "react";
import { afterEach, beforeEach, describe, expect, it, vi } from "vitest";
import {
  ganBoChuyenDoi,
  matKetNoi,
  traVe,
  type BoChuyenDoi,
  type CauHinhYeuCau,
  type PhanHoiHttp,
} from "@/shared/api/giaLapHttp";
import { xoaPhien } from "@/shared/api/session/phien";
import type { PhieuMuon, SinhVienTheInfo } from "@/shared/api/types";
import { hienThiVoiNguCanh } from "@/shared/testing/nguCanhKiemThu";
import { GiaoThietBiNguonScreen } from "./GiaoThietBiNguonScreen";
import { GiaoThietBiPhieuScreen } from "./GiaoThietBiPhieuScreen";
import { GiaoThietBiTheScreen } from "./GiaoThietBiTheScreen";
import { GiaoThietBiThietBiScreen } from "./GiaoThietBiThietBiScreen";
import {
  LuongGiaoThietBiContext,
  LuongGiaoThietBiProvider,
  useLuongGiaoThietBi,
  type GiaTriLuongGiaoThietBi,
} from "./LuongGiaoThietBi";

const dieuHuong = vi.hoisted(() => ({
  push: vi.fn<(tuyen: string) => void>(),
  replace: vi.fn<(tuyen: string) => void>(),
}));
vi.mock("next/navigation", () => ({
  useRouter: () => dieuHuong,
  usePathname: () => "/nv/giao-thiet-bi/the",
}));

const SV_HOP_LE: SinhVienTheInfo = {
  maSV: "24520001",
  hoTen: "Nguyễn Văn An",
  lop: "KTPM2024.1",
  khoa: "Công nghệ phần mềm",
  maNhom: "SV_THUONG",
  tenNhom: "Sinh viên thường",
  hanThe: "2028-08-31",
  trangThaiHoc: "DANG_HOC",
  trangThaiQuyenMuon: "BINH_THUONG",
  soThietBiDangMuon: 1,
  soThietBiToiDa: 3,
  dsBienBanChuaNop: [],
  hopLe: true,
  dsLyDoKhongHopLe: [],
};

const SV_KHONG_HOP_LE: SinhVienTheInfo = {
  ...SV_HOP_LE,
  maSV: "24520002",
  hoTen: "Trần Thị Bình",
  hanThe: "2025-08-31",
  trangThaiQuyenMuon: "TAM_KHOA",
  dsBienBanChuaNop: [
    {
      maBienBan: "BB-000012",
      tongTien: 240000,
      ngayLap: "2026-11-20T03:00:00Z",
    },
  ],
  hopLe: false,
  dsLyDoKhongHopLe: [
    { maLoi: "SV_THE_HET_HAN", thongDiep: "Thẻ sinh viên đã hết hạn" },
    {
      maLoi: "SV_BI_KHOA_QUYEN_MUON",
      thongDiep: "Quyền mượn đang bị khóa",
    },
  ],
};

type Route = (cfg: CauHinhYeuCau) => Promise<PhanHoiHttp>;

function mayChu(traLoi: Route) {
  const goi = vi.fn<BoChuyenDoi>((cfg) => traLoi(cfg));
  ganBoChuyenDoi({ api: goi });
  return goi;
}

/** Trả SV theo mã trong đường dẫn; mã lạ → 404 KHONG_TIM_THAY. */
function theoMa(cfg: CauHinhYeuCau): Promise<PhanHoiHttp> {
  const ma = decodeURIComponent((cfg.url ?? "").split("/").pop() ?? "");
  const sv = [SV_HOP_LE, SV_KHONG_HOP_LE].find((s) => s.maSV === ma);
  return sv !== undefined
    ? traVe(cfg, 200, sv)
    : traVe(cfg, 404, { status: 404, maLoi: "KHONG_TIM_THAY" });
}

function hienThiBuoc1() {
  return hienThiVoiNguCanh(
    <LuongGiaoThietBiProvider>
      <GiaoThietBiTheScreen />
    </LuongGiaoThietBiProvider>,
  );
}

function oMaThe(): HTMLInputElement {
  const o = screen.getByLabelText("Mã thẻ sinh viên");
  if (!(o instanceof HTMLInputElement)) throw new Error("Không phải input");
  return o;
}

function quet(ma: string) {
  fireEvent.change(oMaThe(), { target: { value: ma } });
  fireEvent.keyDown(oMaThe(), { key: "Enter" });
}

function nut(ten: string): HTMLButtonElement {
  const n = screen.getByRole("button", { name: ten });
  if (!(n instanceof HTMLButtonElement)) throw new Error("Không phải nút");
  return n;
}

beforeEach(() => {
  vi.stubEnv("NEXT_PUBLIC_API_BASE_URL", "https://api.test");
  xoaPhien();
  dieuHuong.push.mockClear();
  dieuHuong.replace.mockClear();
});
afterEach(() => {
  vi.unstubAllEnvs();
});

describe("SCR-NV-04 Quét thẻ và hồ sơ sinh viên", () => {
  it("CMP-11: ô mã thẻ có tiêu điểm khi mở, nhãn và chú thích; thanh tiến trình ở bước 1", () => {
    hienThiBuoc1();

    expect(document.activeElement).toBe(oMaThe());
    expect(screen.getByText("Quét hoặc nhập mã, rồi nhấn Enter")).toBeTruthy();
    const thanh = screen.getByRole("navigation", {
      name: "Các bước giao thiết bị",
    });
    expect(thanh.querySelector("[aria-current=step]")?.textContent).toContain(
      "Thẻ SV",
    );
    // Chưa qua bước nào: không có liên kết quay lại.
    expect(thanh.querySelectorAll("a")).toHaveLength(0);
  });

  it("FA 4.9: Enter gửi mã đã cắt khoảng trắng; mã rỗng không gửi", async () => {
    const goi = mayChu(theoMa);
    hienThiBuoc1();

    quet("   ");
    expect(goi).not.toHaveBeenCalled();

    quet("  24520001 ");
    await screen.findByText("Nguyễn Văn An");
    expect(goi.mock.calls[0]?.[0].url).toBe("/quay/sinh-vien/the/24520001");
    expect(goi.mock.calls[0]?.[0].method).toBe("get");
  });

  it("UF-NV-02 bước 2: SV hợp lệ → hồ sơ, kết luận Hợp lệ, Tiếp tục sang bước 2; ô xóa mã và nhận lại tiêu điểm", async () => {
    mayChu(theoMa);
    hienThiBuoc1();

    quet("24520001");

    await screen.findByText("Nguyễn Văn An");
    expect(screen.getByText("KTPM2024.1")).toBeTruthy();
    expect(screen.getByText("Công nghệ phần mềm")).toBeTruthy();
    expect(screen.getByText("Sinh viên thường")).toBeTruthy();
    expect(screen.getByText("31/08/2028")).toBeTruthy();
    expect(screen.getByText("Đang học")).toBeTruthy();
    expect(screen.getByText("Bình thường")).toBeTruthy();
    expect(screen.getByText("1 / 3 thiết bị")).toBeTruthy();
    expect(screen.getByText("Hợp lệ")).toBeTruthy();
    expect(screen.queryByText("Biên bản chưa nộp")).toBeNull();
    await waitFor(() => expect(oMaThe().value).toBe(""));
    expect(document.activeElement).toBe(oMaThe());

    expect(nut("Tiếp tục").disabled).toBe(false);
    fireEvent.click(nut("Tiếp tục"));
    expect(dieuHuong.push).toHaveBeenCalledWith("/nv/giao-thiet-bi/nguon");
  });

  it("E1: SV không hợp lệ → từng lý do của máy chủ, Tiếp tục vô hiệu; biên bản chưa nộp liên kết SCR-NV-11 kèm mã số SV", async () => {
    mayChu(theoMa);
    hienThiBuoc1();

    quet("24520002");

    await screen.findByText("Không hợp lệ");
    expect(screen.getByText("Thẻ sinh viên đã hết hạn")).toBeTruthy();
    expect(screen.getByText("Quyền mượn đang bị khóa")).toBeTruthy();
    expect(screen.getByText("Tạm khóa")).toBeTruthy();
    expect(nut("Tiếp tục").disabled).toBe(true);

    expect(screen.getByText("BB-000012")).toBeTruthy();
    expect(screen.getByText("240.000 VNĐ")).toBeTruthy();
    expect(
      screen
        .getByRole("link", { name: "Mở ở Biên bản và thu phạt" })
        .getAttribute("href"),
    ).toBe("/nv/bien-ban?maSV=24520002");
  });

  it("UI-Screens 6.4: KHONG_TIM_THAY → báo không tìm thấy sinh viên, giữ mã và chọn sẵn toàn bộ; không có hồ sơ", async () => {
    mayChu(theoMa);
    hienThiBuoc1();

    quet("99999999");

    const loi = await screen.findByText("Không tìm thấy sinh viên");
    expect(loi.closest("[role=alert]")).not.toBeNull();
    const o = oMaThe();
    expect(o.value).toBe("99999999");
    expect(o.getAttribute("aria-invalid")).toBe("true");
    expect(document.activeElement).toBe(o);
    expect(o.selectionStart).toBe(0);
    expect(o.selectionEnd).toBe("99999999".length);
    expect(screen.queryByRole("button", { name: "Tiếp tục" })).toBeNull();
  });

  it("403 KHONG_DU_QUYEN → khối báo không đủ quyền; giữ mã", async () => {
    mayChu((cfg) => traVe(cfg, 403, { status: 403, maLoi: "KHONG_DU_QUYEN" }));
    hienThiBuoc1();

    quet("24520001");

    const thongBao = await screen.findByText(
      "Bạn không có quyền thực hiện thao tác này",
    );
    expect(thongBao.closest("[data-tong]")?.getAttribute("data-tong")).toBe(
      "danger",
    );
    expect(oMaThe().value).toBe("24520001");
  });

  it("Mất kết nối → báo kiểm tra kết nối, có thể quét lại cùng mã (tra cứu mới)", async () => {
    let lan = 0;
    const goi = mayChu((cfg) => (++lan === 1 ? matKetNoi(cfg) : theoMa(cfg)));
    hienThiBuoc1();

    quet("24520001");
    await screen.findByText(
      "Không kết nối được tới hệ thống. Kiểm tra kết nối rồi thử lại",
    );

    fireEvent.keyDown(oMaThe(), { key: "Enter" });
    await screen.findByText("Nguyễn Văn An");
    expect(goi).toHaveBeenCalledTimes(2);
  });

  it("NFR-03: đang tra cứu → ô vô hiệu, chỉ báo đang tải; Enter lần nữa không gửi thêm", async () => {
    let traLoi: (() => void) | undefined;
    const goi = mayChu(
      (cfg) =>
        new Promise((r) => {
          traLoi = () => r(theoMa(cfg));
        }),
    );
    hienThiBuoc1();

    quet("24520001");

    expect(oMaThe().disabled).toBe(true);
    expect(screen.getByText("Đang tải")).toBeTruthy();
    await waitFor(() => expect(goi).toHaveBeenCalledTimes(1));
    fireEvent.keyDown(oMaThe(), { key: "Enter" });
    await new Promise((r) => setTimeout(r, 0));
    expect(goi).toHaveBeenCalledTimes(1);

    await act(async () => traLoi?.());
    await screen.findByText("Nguyễn Văn An");
    expect(oMaThe().disabled).toBe(false);
  });

  it("Quét thẻ khác → xóa hồ sơ, ô trống và nhận tiêu điểm", async () => {
    mayChu(theoMa);
    hienThiBuoc1();
    quet("24520001");
    await screen.findByText("Nguyễn Văn An");
    fireEvent.change(oMaThe(), { target: { value: "2452" } });
    nut("Tiếp tục").focus();

    fireEvent.click(nut("Quét thẻ khác"));

    expect(screen.queryByText("Nguyễn Văn An")).toBeNull();
    expect(screen.queryByRole("button", { name: "Tiếp tục" })).toBeNull();
    expect(oMaThe().value).toBe("");
    expect(document.activeElement).toBe(oMaThe());
  });
});

// ---------------- Khung bốn bước và ngữ cảnh ----------------

function nguCanh(
  thayDoi: Partial<GiaTriLuongGiaoThietBi> = {},
): GiaTriLuongGiaoThietBi {
  return {
    sinhVien: null,
    nguon: null,
    phieuDaLap: null,
    ghiNhanSinhVien: vi.fn(),
    datLai: vi.fn(),
    danhDauQuayLai: vi.fn(),
    boDauQuayLai: vi.fn(),
    laQuayLai: () => false,
    ...thayDoi,
  };
}

function hienThiVoi(ui: ReactElement, giaTri: GiaTriLuongGiaoThietBi) {
  return hienThiVoiNguCanh(
    <LuongGiaoThietBiContext.Provider value={giaTri}>
      {ui}
    </LuongGiaoThietBiContext.Provider>,
  );
}

const PHIEU = { maPhieuMuon: "PM-0001" } as unknown as PhieuMuon;

describe("LuongGiaoThietBi: điều kiện vào các bước (FA 4.7)", () => {
  it.each([
    ["bước 2", <GiaoThietBiNguonScreen key="2" />],
    ["bước 3", <GiaoThietBiThietBiScreen key="3" />],
    ["bước 4", <GiaoThietBiPhieuScreen key="4" />],
  ])(
    "%s thiếu ngữ cảnh (tải lại trang) → không vẽ gì, router.replace về bước 1",
    (_ten, ui) => {
      const { container } = hienThiVoi(ui, nguCanh());
      expect(container.textContent).toBe("");
      expect(dieuHuong.replace).toHaveBeenCalledWith("/nv/giao-thiet-bi/the");
    },
  );

  it("bước 2 với SV không hợp lệ (mở thẳng đường dẫn) → về bước 1", () => {
    hienThiVoi(
      <GiaoThietBiNguonScreen />,
      nguCanh({ sinhVien: SV_KHONG_HOP_LE }),
    );
    expect(dieuHuong.replace).toHaveBeenCalledWith("/nv/giao-thiet-bi/the");
  });

  it("bước 2 có SV hợp lệ → hiển thị; bước 1 là liên kết quay lại (đánh dấu quay lại), bước 3, 4 không bấm được", () => {
    const giaTri = nguCanh({ sinhVien: SV_HOP_LE });
    hienThiVoi(<GiaoThietBiNguonScreen />, giaTri);

    expect(dieuHuong.replace).not.toHaveBeenCalled();
    expect(giaTri.boDauQuayLai).toHaveBeenCalled();
    const thanh = screen.getByRole("navigation", {
      name: "Các bước giao thiết bị",
    });
    const dsLienKet = thanh.querySelectorAll("a");
    expect(dsLienKet).toHaveLength(1);
    expect(dsLienKet[0]?.getAttribute("href")).toBe("/nv/giao-thiet-bi/the");
    expect(thanh.querySelector("[aria-current=step]")?.textContent).toContain(
      "Nguồn giao",
    );
    fireEvent.click(screen.getByRole("link", { name: /Thẻ SV/ }));
    expect(giaTri.danhDauQuayLai).toHaveBeenCalled();
  });

  it("đã lập phiếu → mở lại bước 2, 3 chuyển về bước 4", () => {
    hienThiVoi(
      <GiaoThietBiNguonScreen />,
      nguCanh({ sinhVien: SV_HOP_LE, phieuDaLap: PHIEU }),
    );
    expect(dieuHuong.replace).toHaveBeenCalledWith("/nv/giao-thiet-bi/phieu");
  });

  it("vào bước 1 không qua thanh tiến trình → đặt lại luồng; quay lại bằng thanh tiến trình → giữ hồ sơ", () => {
    const moi = nguCanh({ sinhVien: SV_HOP_LE });
    const { unmount } = hienThiVoi(<GiaoThietBiTheScreen />, moi);
    expect(moi.datLai).toHaveBeenCalled();
    unmount();

    const quayLai = nguCanh({ sinhVien: SV_HOP_LE, laQuayLai: () => true });
    hienThiVoi(<GiaoThietBiTheScreen />, quayLai);
    expect(quayLai.datLai).not.toHaveBeenCalled();
    expect(screen.getByText("Nguyễn Văn An")).toBeTruthy();
  });

  it("FC: useLuongGiaoThietBi ngoài provider ném lỗi", () => {
    function DungNgoai() {
      useLuongGiaoThietBi();
      return null;
    }
    vi.spyOn(console, "error").mockImplementation(() => undefined);
    expect(() => hienThiVoiNguCanh(<DungNgoai />)).toThrow(
      /LuongGiaoThietBiProvider/,
    );
    vi.restoreAllMocks();
  });
});
