// @vitest-environment jsdom
// SCR-QT-03 bằng fixture: chưa nối API thật, giả lập HTTP qua giaLapHttp (D05-T).
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
import type { LoaiThietBi } from "@/shared/api/types";
import {
  hienThiVoiNguCanh,
  QUAN_TRI_MAU,
} from "@/shared/testing/nguCanhKiemThu";
import { LoaiThietBiScreen } from "./LoaiThietBiScreen";

const LAPTOP: LoaiThietBi = {
  maLoai: "019c0000-0000-7000-8000-00000000c001",
  tenLoai: "Laptop",
  hangSanXuat: "Dell",
  moTa: "Laptop phòng máy",
  chuKyBaoTri: 180,
  phiTreMoiNgay: 20000,
  ngungSuDung: false,
};
const MAY_CHIEU: LoaiThietBi = {
  maLoai: "019c0000-0000-7000-8000-00000000c002",
  tenLoai: "Máy chiếu",
  hangSanXuat: null,
  moTa: null,
  chuKyBaoTri: 90,
  phiTreMoiNgay: 50000,
  ngungSuDung: true,
};

function trangLoai(danhSach: LoaiThietBi[]) {
  return { tongSo: danhSach.length, trang: 1, kichThuoc: 20, danhSach };
}

type Route = (cfg: CauHinhYeuCau) => Promise<PhanHoiHttp> | undefined;
type PhuongThuc = "get" | "post" | "put" | "delete";

function mayChu(
  tuyChon: Partial<Record<PhuongThuc, Route>> = {},
): ReturnType<typeof vi.fn<BoChuyenDoi>> {
  const goi = vi.fn<BoChuyenDoi>((cfg) => {
    const ketQua = tuyChon[cfg.method as PhuongThuc]?.(cfg);
    if (ketQua !== undefined) return ketQua;
    if (cfg.method === "get")
      return traVe(cfg, 200, trangLoai([LAPTOP, MAY_CHIEU]));
    if (cfg.method === "delete") return traVe(cfg, 204);
    return traVe(cfg, 200, LAPTOP);
  });
  ganBoChuyenDoi({ api: goi });
  return goi;
}

function goiTheo(goi: ReturnType<typeof vi.fn<BoChuyenDoi>>, method: string) {
  return goi.mock.calls.map((c) => c[0]).filter((c) => c.method === method);
}

function hopThoai(): HTMLElement {
  const hop = document.querySelector("dialog");
  if (!(hop instanceof HTMLElement)) throw new Error("Không có hộp thoại");
  return hop;
}

function nutCuaDong(tenLoai: string, nhan: string): HTMLElement {
  const dong = screen.getByText(tenLoai).closest("tr");
  if (dong === null) throw new Error(`Không có dòng ${tenLoai}`);
  return within(dong).getByRole("button", { name: nhan });
}

function hienThi() {
  return hienThiVoiNguCanh(<LoaiThietBiScreen />, { nguoiDung: QUAN_TRI_MAU });
}

beforeEach(() => {
  vi.stubEnv("NEXT_PUBLIC_API_BASE_URL", "https://api.test");
  xoaPhien();
});

afterEach(() => {
  vi.unstubAllEnvs();
});

describe("LoaiThietBiScreen", () => {
  it("UI-Screens 7.3: bảng đủ cột, phí định dạng VNĐ, giá trị thiếu là —, nhãn Đã ngưng sử dụng", async () => {
    mayChu();
    hienThi();

    expect(await screen.findByText("Laptop")).toBeTruthy();
    expect(
      screen.getAllByRole("columnheader").map((th) => th.textContent),
    ).toEqual([
      "Tên loại",
      "Hãng sản xuất",
      "Mô tả",
      "Chu kỳ bảo trì (ngày)",
      "Phí trả trễ mỗi ngày (VNĐ)",
      "Trạng thái",
      "Thao tác",
    ]);
    // Trình so khớp chuẩn hóa khoảng trắng không ngắt thành dấu cách thường.
    expect(screen.getByText("20.000 VNĐ")).toBeTruthy();
    const dongMayChieu = screen.getByText("Máy chiếu").closest("tr");
    expect(dongMayChieu?.textContent).toContain("—");
    expect(dongMayChieu?.textContent).toContain("Đã ngưng sử dụng");
  });

  it("FA 3.10: bộ lọc Đã ngưng sử dụng → gửi ngungSuDung=true từ trang 1; có Xóa bộ lọc", async () => {
    const goi = mayChu();
    hienThi();
    await screen.findByText("Laptop");

    fireEvent.change(screen.getByLabelText("Trạng thái sử dụng"), {
      target: { value: "true" },
    });

    await waitFor(() =>
      expect(goiTheo(goi, "get").at(-1)?.params).toEqual({
        trang: 1,
        kichThuoc: 20,
        ngungSuDung: true,
      }),
    );
    fireEvent.click(screen.getByRole("button", { name: "Xóa bộ lọc" }));
    await waitFor(() =>
      expect(goiTheo(goi, "get").at(-1)?.params).toEqual({
        trang: 1,
        kichThuoc: 20,
      }),
    );
  });

  it("DS 9.5: rỗng do bộ lọc → thông điệp riêng kèm Xóa bộ lọc; rỗng không lọc → 'Chưa có loại thiết bị'", async () => {
    mayChu({
      get: (cfg) =>
        traVe(cfg, 200, trangLoai(cfg.params?.ngungSuDung === true ? [] : [])),
    });
    hienThi();
    expect(await screen.findByText("Chưa có loại thiết bị")).toBeTruthy();

    fireEvent.change(screen.getByLabelText("Trạng thái sử dụng"), {
      target: { value: "true" },
    });

    expect(
      await screen.findByText("Không có loại thiết bị phù hợp bộ lọc"),
    ).toBeTruthy();
    expect(
      screen.getAllByRole("button", { name: "Xóa bộ lọc" }).length,
    ).toBeGreaterThan(0);
  });

  it("UI-Screens 7.3: Thêm loại có ô phí; chu kỳ 0 bị chặn ở client; hợp lệ thì gửi số nguyên", async () => {
    const goi = mayChu({ post: (cfg) => traVe(cfg, 201, LAPTOP) });
    hienThi();
    await screen.findByText("Laptop");

    fireEvent.click(screen.getByRole("button", { name: "Thêm loại" }));
    const hop = within(hopThoai());
    fireEvent.change(hop.getByLabelText(/Tên loại/), {
      target: { value: "Bảng vẽ" },
    });
    fireEvent.change(hop.getByLabelText(/Chu kỳ bảo trì/), {
      target: { value: "0" },
    });
    fireEvent.change(hop.getByLabelText(/Phí trả trễ/), {
      target: { value: "15000" },
    });
    fireEvent.click(hop.getByRole("button", { name: "Lưu" }));

    expect(await hop.findByText("Nhập số nguyên từ 1")).toBeTruthy();
    expect(goiTheo(goi, "post")).toHaveLength(0);

    fireEvent.change(hop.getByLabelText(/Chu kỳ bảo trì/), {
      target: { value: "30" },
    });
    fireEvent.click(hop.getByRole("button", { name: "Lưu" }));

    await waitFor(() => expect(goiTheo(goi, "post")).toHaveLength(1));
    expect(JSON.parse(String(goiTheo(goi, "post")[0]?.data))).toEqual({
      tenLoai: "Bảng vẽ",
      hangSanXuat: null,
      moTa: null,
      chuKyBaoTri: 30,
      phiTreMoiNgay: 15000,
    });
  });

  it("UI-Screens 7.3: Sửa → phí chỉ đọc kèm liên kết tới SCR-QT-06; yêu cầu sửa không gửi phí", async () => {
    const goi = mayChu();
    hienThi();
    await screen.findByText("Laptop");

    fireEvent.click(nutCuaDong("Laptop", "Sửa"));
    const hop = within(hopThoai());
    const oPhi = hop.getByLabelText(/Phí trả trễ/) as HTMLInputElement;
    expect(oPhi.readOnly).toBe(true);
    expect(oPhi.value).toBe("20.000 VNĐ");
    expect(
      hop
        .getByRole("link", { name: "Đổi phí trả trễ ở Cấu hình" })
        .getAttribute("href"),
    ).toBe("/qt/cau-hinh/phi-tre");

    fireEvent.change(hop.getByLabelText(/Chu kỳ bảo trì/), {
      target: { value: "200" },
    });
    fireEvent.click(hop.getByRole("button", { name: "Lưu" }));

    await waitFor(() => expect(goiTheo(goi, "put")).toHaveLength(1));
    const put = goiTheo(goi, "put")[0];
    expect(put?.url).toBe(`/quan-tri/loai-thiet-bi/${LAPTOP.maLoai}`);
    expect(JSON.parse(String(put?.data))).toEqual({
      tenLoai: "Laptop",
      hangSanXuat: "Dell",
      moTa: "Laptop phòng máy",
      chuKyBaoTri: 200,
    });
  });

  it("UI-Screens 7.3: 400 lỗi theo trường của chu kỳ bảo trì gắn đúng ô", async () => {
    mayChu({
      post: (cfg) =>
        traVe(cfg, 400, {
          maLoi: "DU_LIEU_KHONG_HOP_LE",
          errors: { chuKyBaoTri: ["Chu kỳ bảo trì tối đa 3650 ngày."] },
        }),
    });
    hienThi();
    await screen.findByText("Laptop");

    fireEvent.click(screen.getByRole("button", { name: "Thêm loại" }));
    const hop = within(hopThoai());
    fireEvent.change(hop.getByLabelText(/Tên loại/), {
      target: { value: "X" },
    });
    fireEvent.change(hop.getByLabelText(/Chu kỳ bảo trì/), {
      target: { value: "9999" },
    });
    fireEvent.change(hop.getByLabelText(/Phí trả trễ/), {
      target: { value: "0" },
    });
    fireEvent.click(hop.getByRole("button", { name: "Lưu" }));

    const loi = await hop.findByText("Chu kỳ bảo trì tối đa 3650 ngày.");
    expect(loi.closest("[role=alert]")?.id).toBe("chuKyBaoTri-loi");
  });

  it("UI-Screens 7.3 bước 2: Xóa (hủy bỏ) thành công → báo đã xóa, tải lại danh sách", async () => {
    const goi = mayChu();
    hienThi();
    await screen.findByText("Laptop");

    fireEvent.click(nutCuaDong("Laptop", "Xóa"));
    const hop = within(hopThoai());
    expect(
      hop.getByRole("heading", { name: "Xóa loại thiết bị" }),
    ).toBeTruthy();
    expect(hop.getByText("Dell")).toBeTruthy();
    fireEvent.click(hop.getByRole("button", { name: "Xóa" }));

    expect(await screen.findByText("Đã xóa loại thiết bị Laptop")).toBeTruthy();
    expect(goiTheo(goi, "delete")[0]?.url).toBe(
      `/quan-tri/loai-thiet-bi/${LAPTOP.maLoai}`,
    );
    await waitFor(() =>
      expect(goiTheo(goi, "get").length).toBeGreaterThanOrEqual(2),
    );
  });

  it("UI-Screens 7.3 bước 3: 409 DA_CO_LICH_SU_CHI_NGUNG_SU_DUNG → đề nghị Ngưng sử dụng, xác nhận thì gọi ngung-su-dung", async () => {
    const goi = mayChu({
      delete: (cfg) =>
        traVe(cfg, 409, {
          status: 409,
          maLoi: "DA_CO_LICH_SU_CHI_NGUNG_SU_DUNG",
        }),
      post: (cfg) => traVe(cfg, 200, { ...LAPTOP, ngungSuDung: true }),
    });
    hienThi();
    await screen.findByText("Laptop");

    fireEvent.click(nutCuaDong("Laptop", "Xóa"));
    fireEvent.click(within(hopThoai()).getByRole("button", { name: "Xóa" }));

    const hop = within(hopThoai());
    expect(
      await hop.findByRole("heading", { name: "Ngưng sử dụng loại thiết bị" }),
    ).toBeTruthy();
    expect(hop.getByText(/chỉ có thể ngưng sử dụng/)).toBeTruthy();
    expect(
      hop.getByText("Loại vẫn giữ lịch sử và không còn dùng cho thiết bị mới."),
    ).toBeTruthy();
    expect(hop.queryByText(/hoàn tác/)).toBeNull();

    fireEvent.click(hop.getByRole("button", { name: "Ngưng sử dụng" }));

    expect(
      await screen.findByText("Đã ngưng sử dụng loại thiết bị Laptop"),
    ).toBeTruthy();
    expect(goiTheo(goi, "post")[0]?.url).toBe(
      `/quan-tri/loai-thiet-bi/${LAPTOP.maLoai}/ngung-su-dung`,
    );
  });

  it("UI-Screens 7.3: loại đã ngưng sử dụng mà Xóa nhận 409 → không đề nghị ngưng lần nữa", async () => {
    const goi = mayChu({
      delete: (cfg) =>
        traVe(cfg, 409, {
          status: 409,
          maLoi: "DA_CO_LICH_SU_CHI_NGUNG_SU_DUNG",
        }),
    });
    hienThi();
    await screen.findByText("Máy chiếu");

    fireEvent.click(nutCuaDong("Máy chiếu", "Xóa"));
    fireEvent.click(within(hopThoai()).getByRole("button", { name: "Xóa" }));

    expect(
      await screen.findByText(
        /không xóa được và đang ở trạng thái ngưng sử dụng/,
      ),
    ).toBeTruthy();
    expect(document.querySelector("dialog")).toBeNull();
    expect(goiTheo(goi, "post")).toHaveLength(0);
  });

  it("FA 3.11: mất kết nối sau khi gửi Xóa → chưa xác định kết quả, không gửi lại tới khi kiểm tra", async () => {
    const goi = mayChu({
      delete: (cfg) =>
        Promise.reject(
          Object.assign(new Error("Network Error"), {
            isAxiosError: true,
            code: "ERR_NETWORK",
            config: cfg,
          }),
        ),
    });
    hienThi();
    await screen.findByText("Laptop");

    fireEvent.click(nutCuaDong("Laptop", "Xóa"));
    const hop = within(hopThoai());
    fireEvent.click(hop.getByRole("button", { name: "Xóa" }));

    expect(await hop.findByText(/Chưa xác định được kết quả/)).toBeTruthy();
    fireEvent.click(hop.getByRole("button", { name: "Xóa" }));
    expect(goiTheo(goi, "delete")).toHaveLength(1);

    fireEvent.click(hop.getByRole("button", { name: "Kiểm tra lại" }));
    await waitFor(() => expect(document.querySelector("dialog")).toBeNull());
  });
});
