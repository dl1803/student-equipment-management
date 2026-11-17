// @vitest-environment jsdom
// SCR-QT-02 bằng fixture: chưa nối API thật, giả lập HTTP qua giaLapHttp (D05-T).
import { fireEvent, screen, waitFor, within } from "@testing-library/react";
import { afterEach, beforeEach, describe, expect, it, vi } from "vitest";
import {
  ganBoChuyenDoi,
  traVe,
  type BoChuyenDoi,
  type CauHinhYeuCau,
  type PhanHoiHttp,
} from "@/shared/api/giaLapHttp";
import { xoaPhien } from "@/shared/api/session/phien";
import type { PhongThietBi } from "@/shared/api/types";
import {
  hienThiVoiNguCanh,
  QUAN_TRI_MAU,
} from "@/shared/testing/nguCanhKiemThu";
import { PhongThietBiScreen } from "./PhongThietBiScreen";

const A1: PhongThietBi = {
  maPhong: "019c0000-0000-7000-8000-00000000b001",
  tenPhong: "Phòng thiết bị A1",
  coSo: "Cơ sở Linh Trung",
};
const B2: PhongThietBi = {
  maPhong: "019c0000-0000-7000-8000-00000000b002",
  tenPhong: "Phòng thiết bị B2",
  coSo: "Cơ sở Thủ Đức",
};

function trangPhong(danhSach: PhongThietBi[], tongSo = danhSach.length) {
  return { tongSo, trang: 1, kichThuoc: 20, danhSach };
}

type Route = (cfg: CauHinhYeuCau) => Promise<PhanHoiHttp> | undefined;

/** Máy chủ giả: định tuyến theo phương thức và đường dẫn; GET danh sách mặc định trả A1, B2. */
function mayChu(
  tuyChon: { get?: Route; post?: Route; put?: Route } = {},
): ReturnType<typeof vi.fn<BoChuyenDoi>> {
  const goi = vi.fn<BoChuyenDoi>((cfg) => {
    const ketQua = tuyChon[cfg.method as "get" | "post" | "put"]?.(cfg);
    if (ketQua !== undefined) return ketQua;
    if (cfg.method === "get") return traVe(cfg, 200, trangPhong([A1, B2]));
    return traVe(cfg, 200, A1);
  });
  ganBoChuyenDoi({ api: goi });
  return goi;
}

function soLanGoi(goi: ReturnType<typeof vi.fn<BoChuyenDoi>>, method: string) {
  return goi.mock.calls.filter((c) => c[0].method === method).length;
}

function hopThoai(): HTMLElement {
  const hop = document.querySelector("dialog");
  if (!(hop instanceof HTMLElement)) throw new Error("Không có hộp thoại");
  return hop;
}

function hienThi() {
  return hienThiVoiNguCanh(<PhongThietBiScreen />, { nguoiDung: QUAN_TRI_MAU });
}

beforeEach(() => {
  vi.stubEnv("NEXT_PUBLIC_API_BASE_URL", "https://api.test");
  xoaPhien();
});

afterEach(() => {
  vi.unstubAllEnvs();
});

describe("PhongThietBiScreen", () => {
  it("UI-Screens 7.2: bảng phân trang cột tên phòng, cơ sở; chỉ có Sửa, không có Xóa", async () => {
    const goi = mayChu();
    hienThi();

    expect(await screen.findByText("Phòng thiết bị A1")).toBeTruthy();
    expect(screen.getByText("Cơ sở Thủ Đức")).toBeTruthy();
    expect(screen.getAllByRole("button", { name: "Sửa" })).toHaveLength(2);
    expect(screen.queryByRole("button", { name: "Xóa" })).toBeNull();
    expect(goi.mock.calls[0]?.[0].params).toEqual({ trang: 1, kichThuoc: 20 });
  });

  it("UI-Screens 7.2: danh sách rỗng → 'Chưa có phòng thiết bị'", async () => {
    mayChu({ get: (cfg) => traVe(cfg, 200, trangPhong([])) });
    hienThi();

    expect(await screen.findByText("Chưa có phòng thiết bị")).toBeTruthy();
  });

  it("FA 3.10: nhiều hơn một trang → Sau tải trang 2", async () => {
    const goi = mayChu({
      get: (cfg) => traVe(cfg, 200, trangPhong([A1, B2], 45)),
    });
    hienThi();
    await screen.findByText("Trang 1 / 3");

    fireEvent.click(screen.getByRole("button", { name: "Sau" }));

    await waitFor(() =>
      expect(goi.mock.calls.at(-1)?.[0].params).toEqual({
        trang: 2,
        kichThuoc: 20,
      }),
    );
  });

  it("UI-Screens 3.1: lỗi tải danh sách 500 → lỗi hệ thống kèm mã theo dõi, Thử lại tải lại", async () => {
    let lan = 0;
    const goi = mayChu({
      get: (cfg) =>
        lan++ === 0
          ? traVe(cfg, 500, { status: 500, traceId: "trace-500" })
          : traVe(cfg, 200, trangPhong([A1])),
    });
    hienThi();

    expect(await screen.findByText("trace-500")).toBeTruthy();
    fireEvent.click(screen.getByRole("button", { name: "Thử lại" }));
    expect(await screen.findByText("Phòng thiết bị A1")).toBeTruthy();
    expect(soLanGoi(goi, "get")).toBe(2);
  });

  it("UF-QT-01: Thêm phòng → gửi giá trị đã cắt khoảng trắng, đóng hộp, tải lại danh sách", async () => {
    const goi = mayChu({ post: (cfg) => traVe(cfg, 201, A1) });
    hienThi();
    await screen.findByText("Phòng thiết bị A1");

    fireEvent.click(screen.getByRole("button", { name: "Thêm phòng" }));
    const hop = within(hopThoai());
    fireEvent.change(hop.getByLabelText(/Tên phòng/), {
      target: { value: "  Phòng C3 " },
    });
    fireEvent.change(hop.getByLabelText(/Cơ sở/), {
      target: { value: "Cơ sở 1" },
    });
    fireEvent.click(hop.getByRole("button", { name: "Lưu" }));

    await waitFor(() => expect(document.querySelector("dialog")).toBeNull());
    const post = goi.mock.calls.find((c) => c[0].method === "post")?.[0];
    expect(post?.url).toBe("/quan-tri/phong-thiet-bi");
    expect(JSON.parse(String(post?.data))).toEqual({
      tenPhong: "Phòng C3",
      coSo: "Cơ sở 1",
    });
    await waitFor(() => expect(soLanGoi(goi, "get")).toBe(2));
  });

  it("UI-Screens 7.2: trường bắt buộc trống → báo cạnh trường, không gửi", async () => {
    const goi = mayChu();
    hienThi();
    await screen.findByText("Phòng thiết bị A1");

    fireEvent.click(screen.getByRole("button", { name: "Thêm phòng" }));
    fireEvent.click(within(hopThoai()).getByRole("button", { name: "Lưu" }));

    expect(await screen.findByText("Nhập tên phòng")).toBeTruthy();
    expect(screen.getByText("Nhập cơ sở")).toBeTruthy();
    expect(soLanGoi(goi, "post")).toBe(0);
  });

  it("UI-Screens 7.2: 400 DU_LIEU_KHONG_HOP_LE → lỗi cạnh trường, tiêu điểm vào trường lỗi, giữ dữ liệu, hộp vẫn mở", async () => {
    mayChu({
      post: (cfg) =>
        traVe(cfg, 400, {
          status: 400,
          maLoi: "DU_LIEU_KHONG_HOP_LE",
          errors: { tenPhong: ["Tên phòng đã tồn tại ở cơ sở này."] },
        }),
    });
    hienThi();
    await screen.findByText("Phòng thiết bị A1");

    fireEvent.click(screen.getByRole("button", { name: "Thêm phòng" }));
    const hop = within(hopThoai());
    fireEvent.change(hop.getByLabelText(/Tên phòng/), {
      target: { value: "Phòng A1" },
    });
    fireEvent.change(hop.getByLabelText(/Cơ sở/), {
      target: { value: "Cơ sở Linh Trung" },
    });
    fireEvent.click(hop.getByRole("button", { name: "Lưu" }));

    const loi = await screen.findByText("Tên phòng đã tồn tại ở cơ sở này.");
    const oTen = hop.getByLabelText(/Tên phòng/);
    expect(loi.closest("[role=alert]")?.id).toBe("tenPhong-loi");
    expect(oTen.getAttribute("aria-invalid")).toBe("true");
    expect(document.activeElement).toBe(oTen);
    expect((oTen as HTMLInputElement).value).toBe("Phòng A1");
    expect(document.querySelector("dialog")).not.toBeNull();
  });

  it("UI-Screens 7.2: Sửa nhận 409 XUNG_DOT_CAP_NHAT → tải lại, nạp giá trị mới nhất vào biểu mẫu, báo sửa lại", async () => {
    let lanGet = 0;
    const A1_MOI = { ...A1, tenPhong: "Phòng A1 (đã đổi)" };
    const goi = mayChu({
      get: (cfg) =>
        traVe(cfg, 200, trangPhong([lanGet++ === 0 ? A1 : A1_MOI, B2])),
      put: (cfg) =>
        traVe(cfg, 409, { status: 409, maLoi: "XUNG_DOT_CAP_NHAT" }),
    });
    hienThi();
    await screen.findByText("Phòng thiết bị A1");

    fireEvent.click(
      screen.getAllByRole("button", { name: "Sửa" })[0] ?? document.body,
    );
    const hop = within(hopThoai());
    expect((hop.getByLabelText(/Tên phòng/) as HTMLInputElement).value).toBe(
      A1.tenPhong,
    );
    fireEvent.change(hop.getByLabelText(/Cơ sở/), {
      target: { value: "Cơ sở mới" },
    });
    fireEvent.click(hop.getByRole("button", { name: "Lưu" }));

    expect(
      await hop.findByText(/Dữ liệu vừa được người khác thay đổi/),
    ).toBeTruthy();
    expect(goi.mock.calls.find((c) => c[0].method === "put")?.[0].url).toBe(
      `/quan-tri/phong-thiet-bi/${A1.maPhong}`,
    );
    await waitFor(() =>
      expect((hop.getByLabelText(/Tên phòng/) as HTMLInputElement).value).toBe(
        A1_MOI.tenPhong,
      ),
    );
    expect((hop.getByLabelText(/Cơ sở/) as HTMLInputElement).value).toBe(
      A1.coSo,
    );
    expect(soLanGoi(goi, "get")).toBe(2);
  });

  it("ĐX-DS-09: biểu mẫu không có nút submit — gửi form (Enter) không lưu", async () => {
    const goi = mayChu();
    hienThi();
    await screen.findByText("Phòng thiết bị A1");

    fireEvent.click(screen.getByRole("button", { name: "Thêm phòng" }));
    const hop = hopThoai();
    const form = hop.querySelector("form");
    fireEvent.change(within(hop).getByLabelText(/Tên phòng/), {
      target: { value: "P" },
    });
    fireEvent.change(within(hop).getByLabelText(/Cơ sở/), {
      target: { value: "C" },
    });
    fireEvent.keyDown(within(hop).getByLabelText(/Cơ sở/), { key: "Enter" });
    if (form !== null) fireEvent.submit(form);

    expect(hop.querySelector("[type=submit]")).toBeNull();
    expect(soLanGoi(goi, "post")).toBe(0);
  });

  it("CMP-26: Esc đóng hộp biểu mẫu khi không đang lưu; nhãn gắn đúng ô", async () => {
    mayChu();
    hienThi();
    await screen.findByText("Phòng thiết bị A1");

    fireEvent.click(screen.getByRole("button", { name: "Thêm phòng" }));
    expect(within(hopThoai()).getByLabelText(/Tên phòng/).tagName).toBe(
      "INPUT",
    );
    fireEvent(hopThoai(), new Event("cancel", { cancelable: true }));

    await waitFor(() => expect(document.querySelector("dialog")).toBeNull());
  });

  it("Review D05-T: 409 mà phòng không còn trong trang hiện tại → giữ hộp và dữ liệu đang nhập, không báo đã xóa", async () => {
    let lanGet = 0;
    mayChu({
      get: (cfg) =>
        traVe(cfg, 200, trangPhong(lanGet++ === 0 ? [A1, B2] : [B2])),
      put: (cfg) =>
        traVe(cfg, 409, { status: 409, maLoi: "XUNG_DOT_CAP_NHAT" }),
    });
    hienThi();
    await screen.findByText("Phòng thiết bị A1");

    fireEvent.click(
      screen.getAllByRole("button", { name: "Sửa" })[0] ?? document.body,
    );
    const hop = within(hopThoai());
    fireEvent.change(hop.getByLabelText(/Cơ sở/), {
      target: { value: "Cơ sở mới" },
    });
    fireEvent.click(hop.getByRole("button", { name: "Lưu" }));

    expect(
      await hop.findByText(/kiểm tra giá trị hiện tại trước khi lưu lại/),
    ).toBeTruthy();
    expect((hop.getByLabelText(/Cơ sở/) as HTMLInputElement).value).toBe(
      "Cơ sở mới",
    );
    expect(screen.queryByText(/Không tìm thấy/)).toBeNull();
  });
});
