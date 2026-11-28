// @vitest-environment jsdom
// SCR-NV-01 bằng fixture (giả lập HTTP qua giaLapHttp). Ví dụ kiểm thử màn hình (bàn giao D04-T).
import { screen, waitFor, within } from "@testing-library/react";
import { afterEach, beforeEach, describe, expect, it, vi } from "vitest";
import {
  ganBoChuyenDoi,
  traVe,
  type BoChuyenDoi,
} from "@/shared/api/giaLapHttp";
import { xoaPhien } from "@/shared/api/session/phien";
import {
  hienThiVoiNguCanh,
  NHAN_VIEN_MAU,
} from "@/shared/testing/nguCanhKiemThu";
import { loiChao } from "./soLieu";
import { TrangChuNhanVienScreen } from "./TrangChuNhanVienScreen";

/** Máy chủ giả: mỗi API danh sách trả tongSo riêng; null = lỗi 500. */
function mayChu(tong: {
  choDuyet: number | null;
  quaHan: number | null;
  danhSachCho: number | null;
}) {
  const goi = vi.fn<BoChuyenDoi>((cfg) => {
    const url = cfg.url ?? "";
    const so =
      url === "/quay/phieu-yeu-cau-muon"
        ? tong.choDuyet
        : url === "/quay/phieu-muon/qua-han-30-ngay"
          ? tong.quaHan
          : url === "/quay/danh-sach-cho"
            ? tong.danhSachCho
            : null;
    return so === null
      ? traVe(cfg, 500, { status: 500 })
      : traVe(cfg, 200, { tongSo: so, trang: 1, kichThuoc: 1, danhSach: [] });
  });
  ganBoChuyenDoi({ api: goi });
  return goi;
}

beforeEach(() => {
  vi.stubEnv("NEXT_PUBLIC_API_BASE_URL", "https://api.test");
  xoaPhien();
});
afterEach(() => {
  vi.unstubAllEnvs();
});

describe("TrangChuNhanVienScreen", () => {
  it("UI-Screens 6.1: lời chào kèm họ tên; phòng được cấp quyền (tên, cơ sở)", async () => {
    mayChu({ choDuyet: 0, quaHan: 0, danhSachCho: 0 });
    hienThiVoiNguCanh(<TrangChuNhanVienScreen />, { nguoiDung: NHAN_VIEN_MAU });

    expect(
      await screen.findByRole("heading", {
        level: 1,
        name: new RegExp(`${NHAN_VIEN_MAU.hoTen}$`),
      }),
    ).toBeTruthy();
    expect(screen.getByText("Phòng thiết bị A1")).toBeTruthy();
    expect(screen.getByText("Cơ sở Linh Trung")).toBeTruthy();
  });

  it("UI-Screens 6.1: ba thẻ số liệu hiện đúng tongSo máy chủ trả, gọi kichThuoc=1, mở đúng màn hình", async () => {
    const goi = mayChu({ choDuyet: 5, quaHan: 2, danhSachCho: 13 });
    hienThiVoiNguCanh(<TrangChuNhanVienScreen />, { nguoiDung: NHAN_VIEN_MAU });
    const khu = screen.getByRole("region", { name: "Số liệu hôm nay" });

    await waitFor(() => expect(within(khu).getByText("13")).toBeTruthy());
    const the = within(khu).getAllByRole("link");
    expect(the.map((a) => [a.getAttribute("href"), a.textContent])).toEqual([
      ["/nv/phieu-yeu-cau", "Phiếu chờ duyệt5"],
      ["/nv/qua-han-30-ngay", "Phiếu chưa trả quá 30 ngày2"],
      ["/nv/danh-sach-cho", "Đăng ký trong danh sách chờ13"],
    ]);
    const cauHinh = goi.mock.calls.map((c) => c[0]);
    expect(
      cauHinh.find((c) => c.url === "/quay/phieu-yeu-cau-muon")?.params,
    ).toEqual({ trangThai: "CHO_DUYET", trang: 1, kichThuoc: 1 });
    expect(cauHinh.every((c) => c.params?.kichThuoc === 1)).toBe(true);
  });

  it("thẻ số liệu lỗi tải → hiện '—', các thẻ khác vẫn đúng (không tự tính)", async () => {
    mayChu({ choDuyet: null, quaHan: 4, danhSachCho: 0 });
    hienThiVoiNguCanh(<TrangChuNhanVienScreen />, { nguoiDung: NHAN_VIEN_MAU });
    const khu = screen.getByRole("region", { name: "Số liệu hôm nay" });

    await waitFor(() => expect(within(khu).getByText("4")).toBeTruthy());
    const [choDuyet] = within(khu).getAllByRole("link");
    expect(choDuyet?.textContent).toBe("Phiếu chờ duyệt—");
  });

  it("UI-Screens 6.1: dsPhong rỗng → thông báo liên hệ quản trị viên, lối vào và số liệu vẫn hiện", () => {
    mayChu({ choDuyet: 0, quaHan: 0, danhSachCho: 0 });
    hienThiVoiNguCanh(<TrangChuNhanVienScreen />, {
      nguoiDung: { ...NHAN_VIEN_MAU, dsPhong: [] },
    });

    expect(
      screen.getByText(
        "Bạn chưa được cấp quyền phòng thiết bị nào; liên hệ quản trị viên",
      ),
    ).toBeTruthy();
    const chucNang = screen.getByRole("region", { name: "Chức năng" });
    expect(within(chucNang).getAllByRole("link")).toHaveLength(7);
    expect(
      screen.getByRole("region", { name: "Số liệu hôm nay" }),
    ).toBeTruthy();
  });

  it("UI-Screens 6.1: bảy lối vào chức năng quầy, đúng tuyến, có biểu tượng", () => {
    mayChu({ choDuyet: 0, quaHan: 0, danhSachCho: 0 });
    hienThiVoiNguCanh(<TrangChuNhanVienScreen />);
    const khu = screen.getByRole("region", { name: "Chức năng" });

    const dsLienKet = within(khu).getAllByRole("link");
    expect(dsLienKet.map((a) => a.getAttribute("href"))).toEqual([
      "/nv/phieu-yeu-cau",
      "/nv/giao-thiet-bi/the",
      "/nv/nhan-tra",
      "/nv/bien-ban",
      "/nv/qua-han-30-ngay",
      "/nv/danh-sach-cho",
      "/nv/bao-tri",
    ]);
    // Biểu tượng chức năng + chevron, đều ẩn với trình đọc màn hình.
    expect(
      dsLienKet.every(
        (a) => a.querySelectorAll('svg[aria-hidden="true"]').length === 2,
      ),
    ).toBe(true);
  });
});

describe("loiChao", () => {
  it("theo giờ: sáng < 11, trưa < 14, chiều < 18, còn lại tối", () => {
    expect([0, 10, 11, 13, 14, 17, 18, 23].map(loiChao)).toEqual([
      "Chào buổi sáng",
      "Chào buổi sáng",
      "Chào buổi trưa",
      "Chào buổi trưa",
      "Chào buổi chiều",
      "Chào buổi chiều",
      "Chào buổi tối",
      "Chào buổi tối",
    ]);
  });
});
