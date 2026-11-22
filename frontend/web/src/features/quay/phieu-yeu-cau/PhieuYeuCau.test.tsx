// @vitest-environment jsdom
// SCR-NV-02, SCR-NV-03 bằng fixture: chưa nối API thật, giả lập HTTP qua giaLapHttp (D10-T).
import { fireEvent, screen, waitFor, within } from "@testing-library/react";
import { useMemo, useState, type ReactNode } from "react";
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
  KetQuaKiemTraPhieu,
  PhieuYeuCauMuonQuay,
} from "@/shared/api/types";
import type { ThongBao } from "@/shared/labels/thongDiepLoi";
import {
  hienThiVoiNguCanh,
  NHAN_VIEN_MAU,
} from "@/shared/testing/nguCanhKiemThu";
import { DanhSachPhieuYeuCauScreen } from "./DanhSachPhieuYeuCauScreen";
import { DuyetPhieuYeuCauScreen } from "./DuyetPhieuYeuCauScreen";
import {
  PhieuYeuCauDuocChonContext,
  PhieuYeuCauDuocChonProvider,
  usePhieuYeuCauDuocChon as useNguCanh,
  type GiaTriPhieuYeuCauDuocChon,
} from "./PhieuYeuCauDuocChon";

const dieuHuong = vi.hoisted(() => ({
  push: vi.fn<(tuyen: string) => void>(),
  replace: vi.fn<(tuyen: string) => void>(),
}));
vi.mock("next/navigation", () => ({
  useRouter: () => dieuHuong,
  usePathname: () => "/nv/phieu-yeu-cau",
}));

const MA_PHONG_A1 = NHAN_VIEN_MAU.dsPhong[0]?.maPhong ?? "";
const MA_PHONG_B2 = NHAN_VIEN_MAU.dsPhong[1]?.maPhong ?? "";

function phieu(
  ma: string,
  thayDoi: Partial<PhieuYeuCauMuonQuay> = {},
): PhieuYeuCauMuonQuay {
  return {
    maYeuCau: ma,
    maSV: "22520001",
    maPhong: MA_PHONG_A1,
    tenPhong: "Phòng thiết bị A1",
    thoiGianGui: "2026-11-22T01:15:00Z",
    thoiGianMuonDuKienGio: 4,
    trangThai: "CHO_DUYET",
    lyDoTuChoi: null,
    thoiDiemDuyet: null,
    hanGiuCho: null,
    dsLoaiVaSoLuong: [
      {
        maLoai: "019c0000-0000-7000-8000-00000000c001",
        tenLoai: "Laptop",
        soLuong: 2,
      },
      {
        maLoai: "019c0000-0000-7000-8000-00000000c002",
        tenLoai: "Máy chiếu",
        soLuong: 1,
      },
    ],
    sinhVien: {
      maSV: "22520001",
      hoTen: "Lê Thị Ánh Dương",
      lop: "KTPM2022.1",
    },
    ...thayDoi,
  };
}

// Máy chủ trả theo thứ tự gửi (cũ nhất trước); YC-002 cố ý có mã "lớn" hơn nhưng gửi trước.
const YC_CU = phieu("YC-0002", { thoiGianGui: "2026-11-22T01:00:00Z" });
const YC_MOI = phieu("YC-0001", {
  thoiGianGui: "2026-11-22T02:30:00Z",
  maPhong: MA_PHONG_B2,
  tenPhong: "Phòng thiết bị B2",
  sinhVien: { maSV: "22520999", hoTen: "Trần Minh Khôi", lop: null },
});

const KQ_DAT: KetQuaKiemTraPhieu = {
  duocDuyet: true,
  dsLyDo: [],
  dsKhaDung: [
    {
      maLoai: "019c0000-0000-7000-8000-00000000c001",
      tenLoai: "Laptop",
      soLuongYeuCau: 2,
      soLuongKhaDung: 5,
    },
    {
      maLoai: "019c0000-0000-7000-8000-00000000c002",
      tenLoai: "Máy chiếu",
      soLuongYeuCau: 1,
      soLuongKhaDung: 1,
    },
  ],
};

const KQ_KHONG_DAT: KetQuaKiemTraPhieu = {
  duocDuyet: false,
  dsLyDo: [
    { maLoi: "SV_THE_HET_HAN", thongDiep: "Thẻ sinh viên đã hết hạn" },
    {
      maLoi: "SO_LUONG_KHA_DUNG_THAY_DOI",
      thongDiep: "Máy chiếu chỉ còn 0 thiết bị khả dụng",
    },
  ],
  dsKhaDung: [
    {
      maLoai: "019c0000-0000-7000-8000-00000000c002",
      tenLoai: "Máy chiếu",
      soLuongYeuCau: 1,
      soLuongKhaDung: 0,
    },
  ],
};

type Route = (cfg: CauHinhYeuCau) => Promise<PhanHoiHttp> | undefined;

function mayChu(ghiDe: Route = () => undefined) {
  const goi = vi.fn<BoChuyenDoi>((cfg) => {
    const rieng = ghiDe(cfg);
    if (rieng !== undefined) return rieng;
    const url = cfg.url ?? "";
    if (url === "/quay/phieu-yeu-cau-muon")
      return traVe(cfg, 200, {
        tongSo: 2,
        trang: 1,
        kichThuoc: 20,
        danhSach: [YC_CU, YC_MOI],
      });
    if (url.endsWith("/kiem-tra")) return traVe(cfg, 200, KQ_DAT);
    return traVe(cfg, 404, { status: 404, maLoi: "KHONG_TIM_THAY" });
  });
  ganBoChuyenDoi({ api: goi });
  return goi;
}

function goiTheo(goi: ReturnType<typeof mayChu>, url: string) {
  return goi.mock.calls.map((c) => c[0]).filter((c) => c.url === url);
}

const URL_DS = "/quay/phieu-yeu-cau-muon";

beforeEach(() => {
  vi.stubEnv("NEXT_PUBLIC_API_BASE_URL", "https://api.test");
  xoaPhien();
  dieuHuong.push.mockClear();
  dieuHuong.replace.mockClear();
});
afterEach(() => {
  vi.unstubAllEnvs();
});

// ---------------- SCR-NV-02 ----------------

describe("SCR-NV-02 Danh sách phiếu yêu cầu chờ duyệt (UI-Screens 6.2)", () => {
  function hienThiDs(nguoiDung = NHAN_VIEN_MAU) {
    return hienThiVoiNguCanh(
      <PhieuYeuCauDuocChonProvider>
        <DanhSachPhieuYeuCauScreen />
      </PhieuYeuCauDuocChonProvider>,
      { nguoiDung },
    );
  }

  it("gọi CHO_DUYET từ trang 1; đủ cột; giữ nguyên thứ tự máy chủ trả (cũ nhất trước)", async () => {
    const goi = mayChu();
    hienThiDs();
    await screen.findByText("Lê Thị Ánh Dương");

    expect(goiTheo(goi, URL_DS)[0]?.params).toEqual({
      trangThai: "CHO_DUYET",
      trang: 1,
      kichThuoc: 20,
    });
    expect(
      screen.getAllByRole("columnheader").map((th) => th.textContent),
    ).toEqual([
      "Mã phiếu",
      "Sinh viên",
      "Phòng",
      "Loại và số lượng",
      "Thời gian mượn",
      "Gửi lúc",
      "Thao tác",
    ]);
    const dong = screen.getAllByRole("row").slice(1);
    expect(
      dong.map((d) => within(d).getAllByRole("cell")[0]?.textContent),
    ).toEqual(["YC-0002", "YC-0001"]);
    const dauTien = dong[0];
    if (dauTien === undefined) throw new Error("thiếu dòng");
    expect(dauTien.textContent).toContain("22520001");
    expect(dauTien.textContent).toContain("KTPM2022.1");
    expect(dauTien.textContent).toContain("Laptop: 2");
    expect(dauTien.textContent).toContain("Máy chiếu: 1");
    expect(dauTien.textContent).toContain("4 giờ");
    // 01:00Z = 08:00 giờ Việt Nam.
    expect(dauTien.textContent).toContain("22/11/2026 08:00");
    // SV không có lớp: không hiện dấu phân cách thừa.
    expect(dong[1]?.textContent).not.toContain("·");
  });

  it("bộ lọc Phòng chỉ gồm phòng được cấp và 'Tất cả phòng được cấp quyền'; đổi phòng gửi maPhong từ trang 1", async () => {
    const goi = mayChu();
    hienThiDs();
    await screen.findByText("Lê Thị Ánh Dương");
    const o = screen.getByLabelText("Phòng");
    expect(
      within(o)
        .getAllByRole("option")
        .map((op) => op.textContent),
    ).toEqual([
      "Tất cả phòng được cấp quyền",
      "Phòng thiết bị A1 · Cơ sở Linh Trung",
      "Phòng thiết bị B2 · Cơ sở Thủ Đức",
    ]);

    fireEvent.change(o, { target: { value: MA_PHONG_B2 } });
    await waitFor(() =>
      expect(goiTheo(goi, URL_DS).at(-1)?.params).toEqual({
        trangThai: "CHO_DUYET",
        trang: 1,
        kichThuoc: 20,
        maPhong: MA_PHONG_B2,
      }),
    );
  });

  it("danh sách rỗng → 'Không có phiếu yêu cầu chờ duyệt'", async () => {
    mayChu((cfg) =>
      cfg.url === URL_DS
        ? traVe(cfg, 200, { tongSo: 0, trang: 1, kichThuoc: 20, danhSach: [] })
        : undefined,
    );
    hienThiDs();
    expect(
      await screen.findByText("Không có phiếu yêu cầu chờ duyệt"),
    ).toBeTruthy();
  });

  it("NV chưa được cấp phòng (dsPhong rỗng) → báo, KHÔNG gọi danh sách", async () => {
    const goi = mayChu();
    hienThiDs({ ...NHAN_VIEN_MAU, dsPhong: [] });
    expect(
      await screen.findByText(/chưa được cấp quyền phòng thiết bị nào/),
    ).toBeTruthy();
    expect(goiTheo(goi, URL_DS)).toHaveLength(0);
    expect(screen.queryByRole("table")).toBeNull();
  });

  it("lỗi tải danh sách → khối lỗi có Thử lại", async () => {
    mayChu((cfg) =>
      cfg.url === URL_DS
        ? traVe(cfg, 500, { status: 500, maLoi: "LOI_HE_THONG" })
        : undefined,
    );
    hienThiDs();
    expect(await screen.findByRole("button", { name: "Thử lại" })).toBeTruthy();
  });

  it("chọn dòng → lưu dòng vào ngữ cảnh rồi mở /nv/phieu-yeu-cau/duyet (không gọi GET chi tiết)", async () => {
    const goi = mayChu();
    const ghiPhieu = vi.fn<(ma: string | undefined) => void>();
    function Doc() {
      // Đọc ngữ cảnh để kiểm dòng đã lưu.
      ghiPhieu(useNguCanh().phieu?.maYeuCau);
      return null;
    }
    hienThiVoiNguCanh(
      <PhieuYeuCauDuocChonProvider>
        <DanhSachPhieuYeuCauScreen />
        <Doc />
      </PhieuYeuCauDuocChonProvider>,
    );
    await screen.findByText("Trần Minh Khôi");
    const dong = screen.getByText("Trần Minh Khôi").closest("tr");
    if (dong === null) throw new Error("thiếu dòng");
    fireEvent.click(within(dong).getByRole("button", { name: "Xử lý" }));

    expect(dieuHuong.push).toHaveBeenCalledWith("/nv/phieu-yeu-cau/duyet");
    await waitFor(() => expect(ghiPhieu).toHaveBeenLastCalledWith("YC-0001"));
    expect(goi.mock.calls.every((c) => c[0].url === URL_DS)).toBe(true);
  });

  it("hiển thị lời báo SCR-NV-03 để lại khi đưa NV về danh sách", async () => {
    mayChu();
    function DatBao() {
      const nc = useNguCanh();
      return (
        <button
          type="button"
          onClick={() =>
            nc.boChon({ tong: "warning", noiDung: "Lời báo từ màn duyệt" })
          }
        >
          đặt báo
        </button>
      );
    }
    hienThiVoiNguCanh(
      <PhieuYeuCauDuocChonProvider>
        <DatBao />
        <DanhSachPhieuYeuCauScreen />
      </PhieuYeuCauDuocChonProvider>,
    );
    fireEvent.click(screen.getByRole("button", { name: "đặt báo" }));
    expect(await screen.findByText("Lời báo từ màn duyệt")).toBeTruthy();
    // Nút Làm mới vô hiệu trong lúc tải lần đầu; chờ danh sách tải xong.
    await screen.findByText("Lê Thị Ánh Dương");
    fireEvent.click(screen.getByRole("button", { name: "Làm mới" }));
    expect(screen.queryByText("Lời báo từ màn duyệt")).toBeNull();
  });
});

// ---------------- SCR-NV-03 ----------------

/** Ngữ cảnh có sẵn dòng đã chọn; ghi lại lời báo khi màn hình đưa NV về danh sách. */
function NguCanhCoPhieu({
  phieuBanDau,
  onBoChon,
  children,
}: {
  phieuBanDau: PhieuYeuCauMuonQuay | null;
  onBoChon: (tb: ThongBao | null) => void;
  children: ReactNode;
}) {
  const [p, setP] = useState(phieuBanDau);
  const giaTri = useMemo<GiaTriPhieuYeuCauDuocChon>(
    () => ({
      phieu: p,
      chonPhieu: setP,
      boChon: (tb) => {
        onBoChon(tb);
        setP(null);
      },
      thongBao: null,
      xoaThongBao: () => undefined,
    }),
    [p, onBoChon],
  );
  return (
    <PhieuYeuCauDuocChonContext.Provider value={giaTri}>
      {children}
    </PhieuYeuCauDuocChonContext.Provider>
  );
}

describe("SCR-NV-03 Xem phiếu và kết quả kiểm tra điều kiện (UI-Screens 6.3)", () => {
  function hienThiDuyet(phieuBanDau: PhieuYeuCauMuonQuay | null = YC_CU) {
    const onBoChon = vi.fn<(tb: ThongBao | null) => void>();
    const kq = hienThiVoiNguCanh(
      <NguCanhCoPhieu phieuBanDau={phieuBanDau} onBoChon={onBoChon}>
        <DuyetPhieuYeuCauScreen />
      </NguCanhCoPhieu>,
    );
    return { ...kq, onBoChon };
  }

  it("tải lại trang (không có dòng trong ngữ cảnh) → quay về danh sách, không gọi API", () => {
    const goi = mayChu();
    hienThiDuyet(null);
    expect(dieuHuong.replace).toHaveBeenCalledWith("/nv/phieu-yeu-cau");
    expect(goi).not.toHaveBeenCalled();
  });

  it("thông tin phiếu lấy từ dòng đã chọn; gọi kiem-tra đúng mã; được duyệt → kết luận và bảng số lượng", async () => {
    const goi = mayChu();
    hienThiDuyet();
    expect(screen.getByText("Lê Thị Ánh Dương (22520001)")).toBeTruthy();
    expect(screen.getByText("KTPM2022.1")).toBeTruthy();
    expect(screen.getByText("4 giờ")).toBeTruthy();

    expect(await screen.findByText("Được duyệt")).toBeTruthy();
    expect(
      goiTheo(goi, "/quay/phieu-yeu-cau-muon/YC-0002/kiem-tra"),
    ).toHaveLength(1);
    expect(goi).toHaveBeenCalledTimes(1);
    const bang = screen.getByRole("table", {
      name: "Số lượng yêu cầu và khả dụng theo loại",
    });
    expect(bang.textContent).toContain("Laptop");
    expect(bang.textContent).not.toContain("Không đủ");
  });

  it("không được duyệt → kết luận, liệt kê đúng thongDiep của máy chủ, nhấn mạnh dòng không đủ", async () => {
    mayChu((cfg) =>
      cfg.url?.endsWith("/kiem-tra")
        ? traVe(cfg, 200, KQ_KHONG_DAT)
        : undefined,
    );
    hienThiDuyet();
    expect(await screen.findByText("Không được duyệt")).toBeTruthy();
    expect(screen.getByText("Thẻ sinh viên đã hết hạn")).toBeTruthy();
    expect(
      screen.getByText("Máy chiếu chỉ còn 0 thiết bị khả dụng"),
    ).toBeTruthy();
    const dong = screen.getByText("Máy chiếu").closest("tr");
    expect(dong?.textContent).toContain("Không đủ");
  });

  it("duocDuyet do máy chủ quyết định: số lượng đủ nhưng duocDuyet=false vẫn là Không được duyệt", async () => {
    mayChu((cfg) =>
      cfg.url?.endsWith("/kiem-tra")
        ? traVe(cfg, 200, {
            ...KQ_DAT,
            duocDuyet: false,
            dsLyDo: [
              {
                maLoi: "SV_BI_KHOA_QUYEN_MUON",
                thongDiep: "Sinh viên đang bị khóa quyền mượn",
              },
            ],
          })
        : undefined,
    );
    hienThiDuyet();
    expect(await screen.findByText("Không được duyệt")).toBeTruthy();
    expect(screen.getByText("Sinh viên đang bị khóa quyền mượn")).toBeTruthy();
  });

  it("E2: 403 PHONG_CHUA_CAP_QUYEN (phiếu ngoài phòng) → báo và quay về danh sách", async () => {
    mayChu((cfg) =>
      cfg.url?.endsWith("/kiem-tra")
        ? traVe(cfg, 403, { status: 403, maLoi: "PHONG_CHUA_CAP_QUYEN" })
        : undefined,
    );
    const { onBoChon } = hienThiDuyet();
    await waitFor(() =>
      expect(dieuHuong.replace).toHaveBeenCalledWith("/nv/phieu-yeu-cau"),
    );
    expect(onBoChon).toHaveBeenCalledWith({
      tong: "warning",
      noiDung: "Phòng này chưa được cấp quyền cho bạn",
    });
  });

  it("E1: 404 (phiếu đã hết hạn hoặc đã xử lý) → báo không còn chờ duyệt và quay về danh sách", async () => {
    mayChu((cfg) =>
      cfg.url?.endsWith("/kiem-tra")
        ? traVe(cfg, 404, { status: 404, maLoi: "KHONG_TIM_THAY" })
        : undefined,
    );
    const { onBoChon } = hienThiDuyet();
    await waitFor(() =>
      expect(dieuHuong.replace).toHaveBeenCalledWith("/nv/phieu-yeu-cau"),
    );
    expect(onBoChon.mock.calls[0]?.[0]?.noiDung).toMatch(/không còn chờ duyệt/);
  });

  it("lỗi hệ thống khi kiểm tra → ở lại màn hình, khối lỗi có Thử lại", async () => {
    const goi = mayChu((cfg) =>
      cfg.url?.endsWith("/kiem-tra")
        ? traVe(cfg, 500, { status: 500, maLoi: "LOI_HE_THONG" })
        : undefined,
    );
    hienThiDuyet();
    fireEvent.click(await screen.findByRole("button", { name: "Thử lại" }));
    await waitFor(() => expect(goi).toHaveBeenCalledTimes(2));
    expect(dieuHuong.replace).not.toHaveBeenCalled();
  });

  it("Kiểm tra lại gọi lại kiem-tra; Về danh sách mở /nv/phieu-yeu-cau", async () => {
    const goi = mayChu();
    hienThiDuyet();
    await screen.findByText("Được duyệt");
    fireEvent.click(screen.getByRole("button", { name: "Kiểm tra lại" }));
    await waitFor(() => expect(goi).toHaveBeenCalledTimes(2));
    fireEvent.click(screen.getByRole("button", { name: "Về danh sách" }));
    expect(dieuHuong.push).toHaveBeenCalledWith("/nv/phieu-yeu-cau");
  });

  it("D10-T chưa có thao tác chốt: không có nút Duyệt hay Từ chối", async () => {
    mayChu();
    hienThiDuyet();
    await screen.findByText("Được duyệt");
    expect(screen.queryByRole("button", { name: "Duyệt" })).toBeNull();
    expect(screen.queryByRole("button", { name: "Từ chối" })).toBeNull();
  });
});
