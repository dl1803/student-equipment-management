// @vitest-environment jsdom
import { fireEvent, render, screen, within } from "@testing-library/react";
import { describe, expect, it, vi } from "vitest";
import { DataTable } from "./DataTable";
import { KeyValueList } from "./KeyValueList";
import { Pagination } from "./Pagination";
import { Select } from "./Select";
import { StateView } from "./StateView";

type Dong = { ma: string; ten: string; tien: string };
const DONG: Dong[] = [
  { ma: "a", ten: "Phòng A1", tien: "1.000 VNĐ" },
  { ma: "b", ten: "Phòng B2", tien: "2.000 VNĐ" },
];
const COT = [
  { khoa: "ten", tieuDe: "Tên", hienThi: (d: Dong) => d.ten },
  {
    khoa: "tien",
    tieuDe: "Tiền",
    canLe: "right" as const,
    hienThi: (d: Dong) => d.tien,
  },
];

describe("DataTable", () => {
  it("CMP-18: là bảng thật có caption, tiêu đề cột scope=col, số căn phải", () => {
    render(
      <DataTable
        tieuDe="Danh sách phòng"
        dsCot={COT}
        dsDong={DONG}
        khoaDong={(d) => d.ma}
      />,
    );
    const bang = screen.getByRole("table", { name: "Danh sách phòng" });
    const dsTieuDe = within(bang).getAllByRole("columnheader");
    expect(dsTieuDe.map((th) => th.textContent)).toEqual(["Tên", "Tiền"]);
    expect(dsTieuDe[0]?.getAttribute("scope")).toBe("col");
    expect(screen.getByText("2.000 VNĐ").className).toContain("text-right");
  });

  it("CMP-18: hành động ở dòng là nút văn bản; mục ẩn không hiện", () => {
    const sua = vi.fn();
    render(
      <DataTable
        tieuDe="Bảng"
        dsCot={COT}
        dsDong={DONG}
        khoaDong={(d) => d.ma}
        hanhDongDong={(d) => [
          { nhan: "Sửa", onClick: () => sua(d.ma) },
          { nhan: "Xóa", onClick: vi.fn(), an: d.ma === "a" },
        ]}
      />,
    );
    expect(screen.getAllByRole("button", { name: "Sửa" })).toHaveLength(2);
    expect(screen.getAllByRole("button", { name: "Xóa" })).toHaveLength(1);
    fireEvent.click(
      screen.getAllByRole("button", { name: "Sửa" })[1] ?? document.body,
    );
    expect(sua).toHaveBeenCalledWith("b");
  });

  it("ĐX-DS-14: từ 3 hành động trở lên phải dùng Menu → báo lỗi lập trình", () => {
    const ghiLoi = vi
      .spyOn(console, "error")
      .mockImplementation(() => undefined);
    const ba = [1, 2, 3].map((i) => ({ nhan: `H${i}`, onClick: vi.fn() }));
    expect(() =>
      render(
        <DataTable
          tieuDe="Bảng"
          dsCot={COT}
          dsDong={DONG}
          khoaDong={(d) => d.ma}
          hanhDongDong={() => ba}
        />,
      ),
    ).toThrow(/Menu/);
    ghiLoi.mockRestore();
  });

  it("CMP-18: trạng thái thân (rỗng) thay phần thân bảng, giữ hàng tiêu đề", () => {
    render(
      <DataTable
        tieuDe="Bảng"
        dsCot={COT}
        dsDong={[]}
        khoaDong={(d) => d.ma}
        trangThaiThan={
          <StateView loai="RONG" thongDiep="Chưa có phòng thiết bị" />
        }
      />,
    );
    expect(screen.getAllByRole("columnheader")).toHaveLength(2);
    expect(screen.getByText("Chưa có phòng thiết bị")).toBeTruthy();
  });
});

describe("Pagination", () => {
  it("CMP-19: hiển thị khoảng, số trang; Trước vô hiệu ở trang 1, Sau đổi trang", () => {
    const onDoiTrang = vi.fn();
    render(
      <Pagination
        trang={1}
        kichThuoc={20}
        tongSo={45}
        onDoiTrang={onDoiTrang}
      />,
    );
    expect(screen.getByText("Hiển thị 1–20 trên tổng 45")).toBeTruthy();
    expect(screen.getByText("Trang 1 / 3")).toBeTruthy();
    expect(screen.getByRole("button", { name: "Trước" })).toHaveProperty(
      "disabled",
      true,
    );
    fireEvent.click(screen.getByRole("button", { name: "Sau" }));
    expect(onDoiTrang).toHaveBeenCalledWith(2);
  });

  it("CMP-19: trang cuối → Sau vô hiệu, khoảng đúng", () => {
    render(
      <Pagination trang={3} kichThuoc={20} tongSo={45} onDoiTrang={vi.fn()} />,
    );
    expect(screen.getByText("Hiển thị 41–45 trên tổng 45")).toBeTruthy();
    expect(screen.getByRole("button", { name: "Sau" })).toHaveProperty(
      "disabled",
      true,
    );
  });

  it("CMP-19: tổng không vượt một trang → ẩn", () => {
    const { container } = render(
      <Pagination trang={1} kichThuoc={20} tongSo={20} onDoiTrang={vi.fn()} />,
    );
    expect(container.innerHTML).toBe("");
  });

  it("CMP-19: tổng giảm còn một trang nhưng đang ở trang 2 → vẫn hiện, Trước dùng được", () => {
    const onDoiTrang = vi.fn();
    render(
      <Pagination
        trang={2}
        kichThuoc={20}
        tongSo={20}
        onDoiTrang={onDoiTrang}
      />,
    );
    expect(screen.getByText("Trang 2 / 1")).toBeTruthy();
    fireEvent.click(screen.getByRole("button", { name: "Trước" }));
    expect(onDoiTrang).toHaveBeenCalledWith(1);
  });
});

describe("Select", () => {
  it("CMP-06: <select> gốc có mục Tất cả; chọn trả giá trị, chọn Tất cả trả null", () => {
    const onChange = vi.fn();
    render(
      <>
        <label htmlFor="loc">Trạng thái</label>
        <Select
          id="loc"
          dsLuaChon={[
            { giaTri: "false", nhan: "Đang sử dụng" },
            { giaTri: "true", nhan: "Đã ngưng sử dụng" },
          ]}
          giaTri={null}
          onChange={onChange}
          coMucTatCa
        />
      </>,
    );
    const o = screen.getByLabelText("Trạng thái");
    expect(o.tagName).toBe("SELECT");
    expect(
      within(o)
        .getAllByRole("option")
        .map((op) => op.textContent),
    ).toEqual(["Tất cả", "Đang sử dụng", "Đã ngưng sử dụng"]);
    fireEvent.change(o, { target: { value: "true" } });
    expect(onChange).toHaveBeenLastCalledWith("true");
    fireEvent.change(o, { target: { value: "" } });
    expect(onChange).toHaveBeenLastCalledWith(null);
  });

  it("CMP-06: không có mục Tất cả và chưa chọn → hiện gợi ý (không chọn lại được), không tự hiện lựa chọn đầu", () => {
    render(
      <>
        <label htmlFor="coSo">Cơ sở</label>
        <Select
          id="coSo"
          dsLuaChon={[{ giaTri: "A", nhan: "Cơ sở A" }]}
          giaTri={null}
          onChange={vi.fn()}
          goiY="Chọn cơ sở"
        />
      </>,
    );
    const o = screen.getByLabelText("Cơ sở") as HTMLSelectElement;
    expect(o.value).toBe("");
    expect(o.selectedOptions[0]?.textContent).toBe("Chọn cơ sở");
    expect(o.options[0]?.disabled).toBe(true);
  });
});

describe("KeyValueList", () => {
  it("CMP-14: cặp tên – giá trị; thiếu giá trị hiển thị —; mã dùng type.code", () => {
    render(
      <KeyValueList
        dsDong={[
          { ten: "Tên loại", giaTri: "Laptop" },
          { ten: "Hãng sản xuất", giaTri: null },
          { ten: "Mã", giaTri: "TB-001", laMa: true },
        ]}
      />,
    );
    expect(screen.getByText("Tên loại").tagName).toBe("DT");
    expect(screen.getByText("—").tagName).toBe("DD");
    expect(screen.getByText("TB-001").className).toContain("type-code");
  });
});
