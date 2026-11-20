// @vitest-environment jsdom
// SCR-QT-04 bằng fixture: chưa nối API thật, giả lập HTTP qua giaLapHttp (D08-T).
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
import type {
  LoaiThietBi,
  NhaCungCap,
  PhongThietBi,
  ThietBi,
} from "@/shared/api/types";
import {
  hienThiVoiNguCanh,
  QUAN_TRI_MAU,
} from "@/shared/testing/nguCanhKiemThu";
import { ThietBiScreen } from "./ThietBiScreen";

const MA_LAPTOP = "019c0000-0000-7000-8000-00000000c001";
const MA_MAY_CHIEU = "019c0000-0000-7000-8000-00000000c002";
const MA_PHONG_A1 = "019c0000-0000-7000-8000-00000000b001";
const MA_PHONG_B2 = "019c0000-0000-7000-8000-00000000b002";

const LOAI: LoaiThietBi[] = [
  {
    maLoai: MA_LAPTOP,
    tenLoai: "Laptop",
    hangSanXuat: "Dell",
    moTa: null,
    chuKyBaoTri: 180,
    phiTreMoiNgay: 20000,
    ngungSuDung: false,
  },
  {
    maLoai: MA_MAY_CHIEU,
    tenLoai: "Máy chiếu",
    hangSanXuat: null,
    moTa: null,
    chuKyBaoTri: 90,
    phiTreMoiNgay: 50000,
    ngungSuDung: true,
  },
];

const PHONG: PhongThietBi[] = [
  { maPhong: MA_PHONG_A1, tenPhong: "Phòng A1", coSo: "Cơ sở Linh Trung" },
  { maPhong: MA_PHONG_B2, tenPhong: "Phòng B2", coSo: "Cơ sở Thủ Đức" },
];

/** 101 nhà cung cấp: mục cuối nằm ở trang 2 (kích thước 100) để kiểm nguồn tải đủ. */
const NCC: NhaCungCap[] = Array.from({ length: 101 }, (_, i) => ({
  maNCC: `019c0000-0000-7000-8000-${String(i).padStart(12, "0")}`,
  tenNCC: i === 100 ? "Công ty Cuối Danh Sách" : `Nhà cung cấp số ${i + 1}`,
  thongTinLienHe: i === 0 ? "028 3725 2002" : null,
}));
const NCC_DAU = NCC[0];
const NCC_CUOI = NCC[100];

const TEN_DAI =
  "Máy chiếu đa phương tiện Epson EB-X51 độ sáng 3.800 lumen kèm điều khiển từ xa, dây HDMI và túi đựng chống sốc";

const LT001: ThietBi = {
  maThietBi: "LT-001",
  maLoai: MA_LAPTOP,
  tenLoai: "Laptop",
  maPhong: MA_PHONG_A1,
  tenPhong: "Phòng A1",
  tenThietBi: "Laptop Dell Latitude 5440",
  moTa: "Máy dùng cho phòng thực hành",
  thongSoKyThuat: "Core i5, RAM 16 GB",
  ngayNhapKho: "2026-09-01",
  thoiHanBaoHanh: "2028-09-01",
  tinhTrang: "SAN_SANG",
  viTriKe: "2",
  viTriTu: "3",
  ngungSuDung: false,
  giaTriHienHanh: 12_000_000,
  dsNhaCungCap: NCC_DAU !== undefined ? [NCC_DAU] : [],
  dsAnh: [{ maAnh: "anh-1", url: "https://cdn.test/lt-001-1.jpg" }],
};

const MC009: ThietBi = {
  maThietBi: "MC-009",
  maLoai: MA_MAY_CHIEU,
  tenLoai: "Máy chiếu",
  maPhong: MA_PHONG_B2,
  tenPhong: "Phòng B2",
  tenThietBi: TEN_DAI,
  moTa: null,
  thongSoKyThuat: null,
  ngayNhapKho: "2025-01-15",
  thoiHanBaoHanh: null,
  tinhTrang: "DANG_DUOC_MUON",
  viTriKe: null,
  viTriTu: null,
  ngungSuDung: true,
  giaTriHienHanh: 5_000_000,
  dsNhaCungCap: [],
  dsAnh: [],
};

function trang<T>(danhSach: T[], tongSo = danhSach.length) {
  return { tongSo, trang: 1, kichThuoc: 20, danhSach };
}

type Route = (cfg: CauHinhYeuCau) => Promise<PhanHoiHttp> | undefined;

/** Bộ chuyển đổi giả theo đường dẫn; `ghiDe` trả undefined thì dùng mặc định. */
function mayChu(
  ghiDe: Route = () => undefined,
  dsThietBi: () => ThietBi[] = () => [LT001, MC009],
) {
  const goi = vi.fn<BoChuyenDoi>((cfg) => {
    const rieng = ghiDe(cfg);
    if (rieng !== undefined) return rieng;
    const url = cfg.url ?? "";
    if (cfg.method === "get") {
      if (url === "/quan-tri/thiet-bi")
        return traVe(cfg, 200, trang(dsThietBi()));
      if (url === "/quan-tri/loai-thiet-bi")
        return traVe(cfg, 200, trang(LOAI));
      if (url === "/quan-tri/phong-thiet-bi")
        return traVe(cfg, 200, trang(PHONG));
      if (url === "/quan-tri/nha-cung-cap") {
        const so = Number(cfg.params?.trang ?? 1);
        const kich = Number(cfg.params?.kichThuoc ?? 20);
        const cat = NCC.slice((so - 1) * kich, so * kich);
        return traVe(cfg, 200, {
          tongSo: NCC.length,
          trang: so,
          kichThuoc: kich,
          danhSach: cat,
        });
      }
    }
    if (cfg.method === "post" && url === "/quan-tri/thiet-bi")
      return traVe(cfg, 201, LT001);
    if (cfg.method === "put") return traVe(cfg, 200, LT001);
    return traVe(cfg, 404, { maLoi: "KHONG_TIM_THAY", status: 404 });
  });
  ganBoChuyenDoi({ api: goi });
  return goi;
}

function goiTheo(
  goi: ReturnType<typeof vi.fn<BoChuyenDoi>>,
  method: string,
  url: string,
) {
  return goi.mock.calls
    .map((c) => c[0])
    .filter((c) => c.method === method && c.url === url);
}

function thanYeuCau(cfg: CauHinhYeuCau | undefined): unknown {
  return typeof cfg?.data === "string" ? JSON.parse(cfg.data) : cfg?.data;
}

function ngan(): HTMLElement {
  const hop = document.querySelector("dialog[open]");
  if (!(hop instanceof HTMLElement)) throw new Error("Ngăn biểu mẫu chưa mở");
  return hop;
}

function hienThi() {
  return hienThiVoiNguCanh(<ThietBiScreen />, { nguoiDung: QUAN_TRI_MAU });
}

async function moNhapKho() {
  fireEvent.click(screen.getByRole("button", { name: "Nhập kho thiết bị" }));
  const hop = ngan();
  // Chờ nguồn lựa chọn tải xong.
  await within(hop).findByRole("option", { name: "Laptop" });
  await within(hop).findByRole("option", { name: /Phòng A1/ });
  return hop;
}

function go(hop: HTMLElement, nhan: string | RegExp, giaTri: string) {
  fireEvent.change(within(hop).getByLabelText(nhan), {
    target: { value: giaTri },
  });
}

function dongCua(tenThietBi: string): HTMLElement {
  const dong = screen.getByText(tenThietBi).closest("tr");
  if (dong === null) throw new Error(`Không có dòng ${tenThietBi}`);
  return dong;
}

beforeEach(() => {
  vi.stubEnv("NEXT_PUBLIC_API_BASE_URL", "https://api.test");
  xoaPhien();
});

afterEach(() => {
  vi.unstubAllEnvs();
});

describe("ThietBiScreen — bảng và bộ lọc (UI-Screens 7.4)", () => {
  it("đủ cột; mã kiểu mã; kệ/tủ; nhãn tình trạng và Đã ngưng sử dụng; giá trị VNĐ; tên dài có dấu hiển thị đủ", async () => {
    mayChu();
    hienThi();
    expect(await screen.findByText("Laptop Dell Latitude 5440")).toBeTruthy();
    expect(
      screen.getAllByRole("columnheader").map((th) => th.textContent),
    ).toEqual([
      "Mã",
      "Tên",
      "Loại",
      "Phòng",
      "Kệ/Tủ",
      "Tình trạng",
      "Giá trị (VNĐ)",
      "Thao tác",
    ]);
    const dong1 = dongCua("Laptop Dell Latitude 5440");
    expect(within(dong1).getByText("LT-001").className).toContain("type-code");
    expect(dong1.textContent).toContain("2/3");
    expect(dong1.textContent).toContain("Sẵn sàng cho mượn");
    expect(dong1.textContent).toContain("12.000.000 VNĐ".replace(" ", " "));
    expect(dong1.textContent).not.toContain("Đã ngưng sử dụng");

    const dong2 = dongCua(TEN_DAI);
    expect(dong2.textContent).toContain("—");
    expect(dong2.textContent).toContain("Đang được mượn");
    expect(dong2.textContent).toContain("Đã ngưng sử dụng");
    // D08-T: dòng chỉ có Sửa (Cập nhật giá trị, Xóa thuộc D09-T).
    expect(within(dong2).getByRole("button", { name: "Sửa" })).toBeTruthy();
  });

  it("FA 3.10: lọc tình trạng, loại, phòng gửi đúng tham số từ trang 1; tên áp dụng khi Enter; Xóa bộ lọc", async () => {
    const goi = mayChu();
    hienThi();
    await screen.findByText("Laptop Dell Latitude 5440");
    const dsThietBi = () => goiTheo(goi, "get", "/quan-tri/thiet-bi");

    fireEvent.change(screen.getByLabelText("Tình trạng"), {
      target: { value: "DANG_BAO_TRI_SUA_CHUA" },
    });
    await waitFor(() =>
      expect(dsThietBi().at(-1)?.params).toEqual({
        trang: 1,
        kichThuoc: 20,
        tinhTrang: "DANG_BAO_TRI_SUA_CHUA",
      }),
    );

    // Loại đã ngưng sử dụng vẫn có trong bộ lọc, kèm chữ nhãn.
    await screen.findByRole("option", { name: "Máy chiếu (Đã ngưng sử dụng)" });
    fireEvent.change(screen.getByLabelText("Loại"), {
      target: { value: MA_MAY_CHIEU },
    });
    fireEvent.change(screen.getByLabelText("Phòng"), {
      target: { value: MA_PHONG_B2 },
    });

    const oTen = screen.getByLabelText("Tên thiết bị");
    fireEvent.change(oTen, { target: { value: "  máy chiếu  " } });
    // Chưa Enter thì chưa gửi tên.
    expect(dsThietBi().at(-1)?.params).not.toHaveProperty("ten");
    fireEvent.submit(oTen);
    await waitFor(() =>
      expect(dsThietBi().at(-1)?.params).toEqual({
        trang: 1,
        kichThuoc: 20,
        ten: "máy chiếu",
        maLoai: MA_MAY_CHIEU,
        maPhong: MA_PHONG_B2,
        tinhTrang: "DANG_BAO_TRI_SUA_CHUA",
      }),
    );

    fireEvent.click(screen.getByRole("button", { name: "Xóa bộ lọc" }));
    await waitFor(() =>
      expect(dsThietBi().at(-1)?.params).toEqual({ trang: 1, kichThuoc: 20 }),
    );
    expect((oTen as HTMLInputElement).value).toBe("");
  });

  it("DS 9.5: rỗng do bộ lọc → 'Không có thiết bị phù hợp bộ lọc' kèm Xóa bộ lọc; rỗng không lọc → 'Chưa có thiết bị'", async () => {
    let ds: ThietBi[] = [];
    mayChu(undefined, () => ds);
    hienThi();
    expect(await screen.findByText("Chưa có thiết bị")).toBeTruthy();

    fireEvent.change(screen.getByLabelText("Tình trạng"), {
      target: { value: "DA_BAO_MAT" },
    });
    expect(
      await screen.findByText("Không có thiết bị phù hợp bộ lọc"),
    ).toBeTruthy();
    ds = [LT001];
    // Nút trong trạng thái rỗng (nút cuối cùng); nút ở thanh bộ lọc làm cùng việc.
    const nutXoa = screen.getAllByRole("button", { name: "Xóa bộ lọc" }).at(-1);
    if (nutXoa === undefined) throw new Error("Thiếu nút Xóa bộ lọc");
    fireEvent.click(nutXoa);
    expect(await screen.findByText("Laptop Dell Latitude 5440")).toBeTruthy();
  });
});

describe("ThietBiScreen — Nhập kho thiết bị (ngăn biểu mẫu)", () => {
  it("nguồn lựa chọn tải đủ mọi trang: nhà cung cấp ở trang 2 có trong danh sách", async () => {
    const goi = mayChu();
    hienThi();
    await screen.findByText("Laptop Dell Latitude 5440");
    const hop = await moNhapKho();

    await waitFor(() =>
      expect(
        goiTheo(goi, "get", "/quan-tri/nha-cung-cap").map((c) => c.params),
      ).toEqual([
        { trang: 1, kichThuoc: 100 },
        { trang: 2, kichThuoc: 100 },
      ]),
    );
    fireEvent.click(
      within(hop).getByRole("combobox", { name: /Nhà cung cấp/ }),
    );
    expect(
      within(hop).getByRole("option", { name: /Công ty Cuối Danh Sách/ }),
    ).toBeTruthy();
    // Dòng phụ: thông tin liên hệ.
    expect(within(hop).getByText("028 3725 2002")).toBeTruthy();
  });

  it("đủ trường theo UI-Screens 7.4, KHÔNG có ô Tình trạng; mã và giá trị nhập được; tiêu điểm ở ô mã", async () => {
    mayChu();
    hienThi();
    await screen.findByText("Laptop Dell Latitude 5440");
    const hop = await moNhapKho();

    expect(within(hop).getByText("Nhập kho thiết bị")).toBeTruthy();
    for (const nhan of [
      /Mã thiết bị/,
      /Tên thiết bị/,
      /^Loại/,
      /^Phòng/,
      "Mô tả",
      "Thông số kỹ thuật",
      /Ngày nhập kho/,
      "Thời hạn bảo hành",
      "Kệ",
      "Tủ",
      /Nhà cung cấp/,
      /Giá trị/,
    ]) {
      expect(within(hop).getByLabelText(nhan)).toBeTruthy();
    }
    expect(within(hop).queryByLabelText(/Tình trạng/)).toBeNull();
    expect(within(hop).getByText("Ảnh (không bắt buộc)")).toBeTruthy();
    expect(
      (within(hop).getByLabelText(/Mã thiết bị/) as HTMLInputElement).readOnly,
    ).toBe(false);
    expect(document.activeElement).toBe(
      within(hop).getByLabelText(/Mã thiết bị/),
    );
    // Loại đã ngưng sử dụng hiển thị kèm nhãn (DS CMP-06).
    expect(
      within(hop).getByRole("option", { name: "Máy chiếu (Đã ngưng sử dụng)" }),
    ).toBeTruthy();
  });

  it("gửi đúng ThietBiTaoMoiRequest: cắt khoảng trắng, trường tùy chọn trống là null, dsMaNCC theo thứ tự nguồn, dsMaAnh rỗng, giaTri số nguyên", async () => {
    const goi = mayChu();
    hienThi();
    await screen.findByText("Laptop Dell Latitude 5440");
    const hop = await moNhapKho();

    go(hop, /Mã thiết bị/, "  LT-100  ");
    go(hop, /Tên thiết bị/, "  Laptop HP ProBook 450 G10 – bản tiếng Việt  ");
    go(hop, /^Loại/, MA_LAPTOP);
    go(hop, /^Phòng/, MA_PHONG_B2);
    go(hop, /Ngày nhập kho/, "2026-11-01");
    go(hop, "Mô tả", "   ");
    go(hop, /Giá trị/, "15000000");
    fireEvent.click(
      within(hop).getByRole("combobox", { name: /Nhà cung cấp/ }),
    );
    fireEvent.click(
      within(hop).getByRole("option", { name: /Công ty Cuối Danh Sách/ }),
    );
    fireEvent.click(
      within(hop).getByRole("option", { name: "Nhà cung cấp số 1" }),
    );
    expect(
      within(hop).getByRole("combobox", { name: /Nhà cung cấp/ }).textContent,
    ).toContain("2 nhà cung cấp đã chọn");

    fireEvent.click(within(hop).getByRole("button", { name: "Lưu" }));

    await waitFor(() =>
      expect(goiTheo(goi, "post", "/quan-tri/thiet-bi")).toHaveLength(1),
    );
    expect(thanYeuCau(goiTheo(goi, "post", "/quan-tri/thiet-bi")[0])).toEqual({
      maThietBi: "LT-100",
      maLoai: MA_LAPTOP,
      maPhong: MA_PHONG_B2,
      tenThietBi: "Laptop HP ProBook 450 G10 – bản tiếng Việt",
      moTa: null,
      thongSoKyThuat: null,
      ngayNhapKho: "2026-11-01",
      thoiHanBaoHanh: null,
      viTriKe: null,
      viTriTu: null,
      dsMaNCC: [NCC_DAU?.maNCC, NCC_CUOI?.maNCC],
      dsMaAnh: [],
      giaTri: 15_000_000,
    });
    // Thành công: đóng ngăn, tải lại danh sách.
    await waitFor(() =>
      expect(document.querySelector("dialog[open]")).toBeNull(),
    );
  });

  it("kiểm tra phía client theo openapi: thiếu trường bắt buộc hoặc giá trị không phải số nguyên không âm thì không gửi", async () => {
    const goi = mayChu();
    hienThi();
    await screen.findByText("Laptop Dell Latitude 5440");
    const hop = await moNhapKho();

    fireEvent.click(within(hop).getByRole("button", { name: "Lưu" }));
    for (const loi of [
      "Nhập mã thiết bị",
      "Nhập tên thiết bị",
      "Chọn loại thiết bị",
      "Chọn phòng thiết bị",
      "Chọn ngày nhập kho",
      "Nhập số nguyên không âm",
    ]) {
      expect(await within(hop).findByText(loi)).toBeTruthy();
    }
    for (const sai of ["-5", "1.5", "1.000.000", "abc"]) {
      go(hop, /Giá trị/, sai);
      fireEvent.click(within(hop).getByRole("button", { name: "Lưu" }));
      await within(hop).findByText("Nhập số nguyên không âm");
    }
    expect(goiTheo(goi, "post", "/quan-tri/thiet-bi")).toHaveLength(0);
  });

  it("UF-QT-01 E1: MA_THIET_BI_TRUNG → lỗi dưới ô mã, giữ dữ liệu đã nhập, ngăn vẫn mở", async () => {
    mayChu((cfg) =>
      cfg.method === "post" && cfg.url === "/quan-tri/thiet-bi"
        ? traVe(cfg, 409, {
            type: "about:blank",
            title: "Xung đột",
            status: 409,
            maLoi: "MA_THIET_BI_TRUNG",
          })
        : undefined,
    );
    hienThi();
    await screen.findByText("Laptop Dell Latitude 5440");
    const hop = await moNhapKho();
    go(hop, /Mã thiết bị/, "LT-001");
    go(hop, /Tên thiết bị/, "Thiết bị trùng mã");
    go(hop, /^Loại/, MA_LAPTOP);
    go(hop, /^Phòng/, MA_PHONG_A1);
    go(hop, /Ngày nhập kho/, "2026-11-01");
    go(hop, /Giá trị/, "0");
    fireEvent.click(within(hop).getByRole("button", { name: "Lưu" }));

    const loi = await within(hop).findByText(
      "Mã thiết bị đã tồn tại. Nhập mã khác.",
    );
    const oMa = within(hop).getByLabelText(/Mã thiết bị/);
    expect(oMa.getAttribute("aria-invalid")).toBe("true");
    expect(oMa.getAttribute("aria-describedby")).toContain(
      loi.closest("p")?.id ?? "?",
    );
    expect(
      (within(hop).getByLabelText(/Tên thiết bị/) as HTMLInputElement).value,
    ).toBe("Thiết bị trùng mã");
    expect(document.querySelector("dialog[open]")).not.toBeNull();
  });

  it("E3: lỗi 400 theo trường của máy chủ (kể cả dsMaAnh) gắn đúng chỗ", async () => {
    mayChu((cfg) =>
      cfg.method === "post" && cfg.url === "/quan-tri/thiet-bi"
        ? traVe(cfg, 400, {
            type: "about:blank",
            title: "Dữ liệu không hợp lệ",
            status: 400,
            maLoi: "DU_LIEU_KHONG_HOP_LE",
            errors: {
              tenThietBi: ["Tên thiết bị quá dài"],
              dsMaAnh: ["Tối đa 5 ảnh"],
            },
          })
        : undefined,
    );
    hienThi();
    await screen.findByText("Laptop Dell Latitude 5440");
    const hop = await moNhapKho();
    go(hop, /Mã thiết bị/, "LT-200");
    go(hop, /Tên thiết bị/, "Tên");
    go(hop, /^Loại/, MA_LAPTOP);
    go(hop, /^Phòng/, MA_PHONG_A1);
    go(hop, /Ngày nhập kho/, "2026-11-01");
    go(hop, /Giá trị/, "1");
    fireEvent.click(within(hop).getByRole("button", { name: "Lưu" }));

    expect(await within(hop).findByText("Tên thiết bị quá dài")).toBeTruthy();
    expect(within(hop).getByText("Tối đa 5 ảnh")).toBeTruthy();
  });
});

describe("ThietBiScreen — Sửa thiết bị", () => {
  async function moSua(tenThietBi: string) {
    await screen.findByText(tenThietBi);
    fireEvent.click(
      within(dongCua(tenThietBi)).getByRole("button", { name: "Sửa" }),
    );
    return ngan();
  }

  it("mã chỉ đọc, giá trị hiện hành chỉ đọc (không có ô nhập giá trị), không có Tình trạng; nạp đúng dữ liệu, NCC và ảnh có sẵn", async () => {
    mayChu();
    hienThi();
    const hop = await moSua("Laptop Dell Latitude 5440");

    expect(within(hop).getByText("Sửa thiết bị")).toBeTruthy();
    const oMa = within(hop).getByLabelText("Mã thiết bị") as HTMLInputElement;
    expect(oMa.readOnly).toBe(true);
    expect(oMa.disabled).toBe(false);
    expect(oMa.value).toBe("LT-001");
    const oGiaTri = within(hop).getByLabelText(
      "Giá trị hiện hành",
    ) as HTMLInputElement;
    expect(oGiaTri.readOnly).toBe(true);
    expect(oGiaTri.value).toBe("12.000.000 VNĐ");
    expect(within(hop).queryByLabelText(/Tình trạng/)).toBeNull();
    await waitFor(() =>
      expect(document.activeElement).toBe(
        within(hop).getByLabelText(/Tên thiết bị/),
      ),
    );

    expect(
      (within(hop).getByLabelText(/Ngày nhập kho/) as HTMLInputElement).value,
    ).toBe("2026-09-01");
    expect(
      within(hop).getByRole("combobox", { name: /Nhà cung cấp/ }).textContent,
    ).toContain("1 nhà cung cấp đã chọn");
    expect(
      within(hop).getByRole("img", { name: "Xem ảnh 1" }).getAttribute("src"),
    ).toBe("https://cdn.test/lt-001-1.jpg");
  });

  it("gửi đúng ThietBiCapNhatRequest: không có mã, giá trị, tình trạng; xóa trường tùy chọn thành null; giữ ảnh và NCC đang gán", async () => {
    const goi = mayChu();
    hienThi();
    const hop = await moSua("Laptop Dell Latitude 5440");
    go(hop, /Tên thiết bị/, "Laptop Dell Latitude 5440 (đã nâng RAM)");
    go(hop, "Mô tả", "");
    go(hop, "Thời hạn bảo hành", "");
    go(hop, "Tủ", "  ");
    fireEvent.click(within(hop).getByRole("button", { name: "Lưu" }));

    await waitFor(() =>
      expect(goiTheo(goi, "put", "/quan-tri/thiet-bi/LT-001")).toHaveLength(1),
    );
    const than = thanYeuCau(
      goiTheo(goi, "put", "/quan-tri/thiet-bi/LT-001")[0],
    );
    expect(than).toEqual({
      maLoai: MA_LAPTOP,
      maPhong: MA_PHONG_A1,
      tenThietBi: "Laptop Dell Latitude 5440 (đã nâng RAM)",
      moTa: null,
      thongSoKyThuat: "Core i5, RAM 16 GB",
      ngayNhapKho: "2026-09-01",
      thoiHanBaoHanh: null,
      viTriKe: "2",
      viTriTu: null,
      dsMaNCC: [NCC_DAU?.maNCC],
      dsMaAnh: ["anh-1"],
    });
    expect(than).not.toHaveProperty("maThietBi");
    expect(than).not.toHaveProperty("giaTri");
    expect(than).not.toHaveProperty("tinhTrang");
  });

  it("mã có ký tự đặc biệt được mã hóa trong đường dẫn", async () => {
    const dacBiet: ThietBi = { ...LT001, maThietBi: "PM/2026 #1" };
    const goi = mayChu(undefined, () => [dacBiet]);
    hienThi();
    const hop = await moSua("Laptop Dell Latitude 5440");
    fireEvent.click(within(hop).getByRole("button", { name: "Lưu" }));
    await waitFor(() =>
      expect(
        goi.mock.calls.some(
          (c) =>
            c[0].method === "put" &&
            c[0].url === "/quan-tri/thiet-bi/PM%2F2026%20%231",
        ),
      ).toBe(true),
    );
  });

  it("nguồn loại chưa có (lỗi tải) vẫn hiện loại đang gán; có Thử lại", async () => {
    let loiLoai = true;
    mayChu((cfg) =>
      cfg.method === "get" && cfg.url === "/quan-tri/loai-thiet-bi" && loiLoai
        ? traVe(cfg, 500, { status: 500, maLoi: "LOI_HE_THONG" })
        : undefined,
    );
    hienThi();
    const hop = await moSua("Laptop Dell Latitude 5440");
    const oLoai = within(hop).getByLabelText(/^Loại/) as HTMLSelectElement;
    await waitFor(() =>
      expect(
        within(hop).getByText("Không tải được danh sách lựa chọn"),
      ).toBeTruthy(),
    );
    expect(oLoai.value).toBe(MA_LAPTOP);
    expect(oLoai.selectedOptions[0]?.textContent).toBe("Laptop");

    loiLoai = false;
    fireEvent.click(within(hop).getByRole("button", { name: "Thử lại" }));
    expect(
      await within(hop).findByRole("option", {
        name: "Máy chiếu (Đã ngưng sử dụng)",
      }),
    ).toBeTruthy();
  });

  it("UF-QT-01 E4: XUNG_DOT_CAP_NHAT → tải lại, nạp bản mới nhất vào biểu mẫu kèm cảnh báo", async () => {
    let lan = 0;
    const banMoi: ThietBi = {
      ...LT001,
      tenThietBi: "Laptop Dell (người khác vừa sửa)",
      giaTriHienHanh: 11_000_000,
    };
    mayChu(
      (cfg) =>
        cfg.method === "put"
          ? traVe(cfg, 409, {
              type: "about:blank",
              title: "Xung đột",
              status: 409,
              maLoi: "XUNG_DOT_CAP_NHAT",
            })
          : undefined,
      () => (lan++ === 0 ? [LT001, MC009] : [banMoi, MC009]),
    );
    hienThi();
    const hop = await moSua("Laptop Dell Latitude 5440");
    go(hop, /Tên thiết bị/, "Tên tôi đang sửa");
    fireEvent.click(within(hop).getByRole("button", { name: "Lưu" }));

    expect(
      await within(hop).findByText(/Biểu mẫu đã nạp giá trị mới nhất/),
    ).toBeTruthy();
    expect(
      (within(hop).getByLabelText(/Tên thiết bị/) as HTMLInputElement).value,
    ).toBe("Laptop Dell (người khác vừa sửa)");
    expect(
      (within(hop).getByLabelText("Giá trị hiện hành") as HTMLInputElement)
        .value,
    ).toBe("11.000.000 VNĐ");
  });

  it("409 XUNG_DOT_CAP_NHAT mà không còn thấy thiết bị trong trang: cảnh báo riêng, GIỮ dữ liệu đang nhập", async () => {
    let lan = 0;
    mayChu(
      (cfg) =>
        cfg.method === "put"
          ? traVe(cfg, 409, {
              type: "about:blank",
              title: "Xung đột",
              status: 409,
              maLoi: "XUNG_DOT_CAP_NHAT",
            })
          : undefined,
      // Lần tải lại sau xung đột: thiết bị không còn trong trang hiện tại.
      () => (lan++ === 0 ? [LT001, MC009] : [MC009]),
    );
    hienThi();
    const hop = await moSua("Laptop Dell Latitude 5440");
    go(hop, /Tên thiết bị/, "Tên tôi đang sửa");
    fireEvent.click(within(hop).getByRole("button", { name: "Lưu" }));

    expect(
      await within(hop).findByText(
        /Danh sách đã được tải lại; kiểm tra giá trị hiện tại trước khi lưu lại/,
      ),
    ).toBeTruthy();
    expect(
      (within(hop).getByLabelText(/Tên thiết bị/) as HTMLInputElement).value,
    ).toBe("Tên tôi đang sửa");
    expect(document.querySelector("dialog[open]")).not.toBeNull();
  });

  it("404 KHONG_TIM_THAY khi lưu: đóng ngăn, báo trên màn hình và tải lại danh sách", async () => {
    const goi = mayChu((cfg) =>
      cfg.method === "put"
        ? traVe(cfg, 404, {
            type: "about:blank",
            title: "Không tìm thấy",
            status: 404,
            maLoi: "KHONG_TIM_THAY",
          })
        : undefined,
    );
    hienThi();
    const hop = await moSua("Laptop Dell Latitude 5440");
    const soLanTaiTruoc = goiTheo(goi, "get", "/quan-tri/thiet-bi").length;
    fireEvent.click(within(hop).getByRole("button", { name: "Lưu" }));

    expect(
      await screen.findByText(
        /Không tìm thấy. Đối tượng có thể đã bị thay đổi/,
      ),
    ).toBeTruthy();
    expect(document.querySelector("dialog[open]")).toBeNull();
    await waitFor(() =>
      expect(goiTheo(goi, "get", "/quan-tri/thiet-bi").length).toBeGreaterThan(
        soLanTaiTruoc,
      ),
    );
  });
});

describe("ThietBiScreen — nguồn lựa chọn của bộ lọc", () => {
  it("đang tải lần đầu: ô Loại và Phòng vô hiệu, aria-busy, hiện 'Đang tải'", async () => {
    mayChu((cfg) =>
      cfg.method === "get" &&
      (cfg.url === "/quan-tri/loai-thiet-bi" ||
        cfg.url === "/quan-tri/phong-thiet-bi")
        ? new Promise<PhanHoiHttp>(() => {})
        : undefined,
    );
    hienThi();
    await screen.findByText("Laptop Dell Latitude 5440");
    for (const nhan of ["Loại", "Phòng"]) {
      const o = screen.getByLabelText(nhan);
      expect(o.hasAttribute("disabled")).toBe(true);
      expect(o.getAttribute("aria-busy")).toBe("true");
      expect(o.textContent).toContain("Đang tải");
    }
    // Ô Tình trạng có danh sách cố định nên vẫn dùng được.
    expect(screen.getByLabelText("Tình trạng").hasAttribute("disabled")).toBe(
      false,
    );
  });
});
