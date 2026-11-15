// @vitest-environment jsdom
import { act, render } from "@testing-library/react";
import { useForm, type UseFormReturn } from "react-hook-form";
import { describe, expect, it } from "vitest";
import type { ApiError } from "@/shared/api/apiError";
import { chuyenKhoaLoiTruong, ganLoiMayChu } from "./ganLoiMayChu";

type GiaTriPhieu = {
  ghiChu: string;
  dsLoaiVaSoLuong: Array<{ maLoai: string; soLuong: number }>;
};

const MAC_DINH: GiaTriPhieu = {
  ghiChu: "giữ nguyên",
  dsLoaiVaSoLuong: [
    { maLoai: "L1", soLuong: 2 },
    { maLoai: "L2", soLuong: 0 },
  ],
};

function hienThiBieuMau(): UseFormReturn<GiaTriPhieu> {
  let form: UseFormReturn<GiaTriPhieu> | undefined;
  function BieuMau() {
    form = useForm<GiaTriPhieu>({ defaultValues: MAC_DINH });
    return (
      <form>
        <input id="ghiChu" {...form.register("ghiChu")} />
        <input id="soLuong0" {...form.register("dsLoaiVaSoLuong.0.soLuong")} />
        <input id="soLuong1" {...form.register("dsLoaiVaSoLuong.1.soLuong")} />
      </form>
    );
  }
  render(<BieuMau />);
  if (form === undefined) throw new Error("Biểu mẫu chưa dựng");
  return form;
}

describe("chuyenKhoaLoiTruong", () => {
  it("FC 5.6: chỉ số mảng dạng [n] → .n", () => {
    expect(chuyenKhoaLoiTruong("dsLoaiVaSoLuong[0].soLuong")).toBe(
      "dsLoaiVaSoLuong.0.soLuong",
    );
    expect(chuyenKhoaLoiTruong("a[1].b[12].c")).toBe("a.1.b.12.c");
    expect(chuyenKhoaLoiTruong("ghiChu")).toBe("ghiChu");
  });
});

describe("ganLoiMayChu", () => {
  const LOI: ApiError = {
    loai: "HTTP",
    status: 400,
    maLoi: "DU_LIEU_KHONG_HOP_LE",
    errors: {
      "dsLoaiVaSoLuong[1].soLuong": ["Số lượng phải lớn hơn 0."],
      ghiChu: ["Ghi chú quá dài.", "Không được chứa ký tự đặc biệt."],
      truongKhongCo: ["Lỗi không gắn được trường."],
    },
  };

  it("FC 5.6: lỗi dạng mảng gắn đúng trường, đưa tiêu điểm tới trường lỗi đầu tiên", () => {
    const form = hienThiBieuMau();

    act(() => {
      ganLoiMayChu(LOI, form);
    });

    expect(form.getFieldState("dsLoaiVaSoLuong.1.soLuong").error?.message).toBe(
      "Số lượng phải lớn hơn 0.",
    );
    expect(
      form.getFieldState("dsLoaiVaSoLuong.0.soLuong").error,
    ).toBeUndefined();
    expect(form.getFieldState("ghiChu").error?.message).toBe(
      "Ghi chú quá dài. Không được chứa ký tự đặc biệt.",
    );
    expect(document.activeElement?.id).toBe("soLuong1");
  });

  it("FC 5.6: khóa không khớp trường nào → trả về để hiển thị ở đầu biểu mẫu", () => {
    const form = hienThiBieuMau();
    let khongKhop: string[] = [];

    act(() => {
      khongKhop = ganLoiMayChu(LOI, form);
    });

    expect(khongKhop).toEqual(["Lỗi không gắn được trường."]);
  });

  it("FA 4.8: giữ nguyên dữ liệu đã nhập, không reset biểu mẫu", () => {
    const form = hienThiBieuMau();

    act(() => {
      ganLoiMayChu(LOI, form);
    });

    expect(form.getValues()).toEqual(MAC_DINH);
  });

  it("FC 5.6: lỗi không có errors → không gắn gì", () => {
    const form = hienThiBieuMau();
    let khongKhop: string[] = ["x"];

    act(() => {
      khongKhop = ganLoiMayChu({ loai: "HTTP", status: 422, maLoi: "X" }, form);
    });

    expect(khongKhop).toEqual([]);
    expect(form.getFieldState("ghiChu").error).toBeUndefined();
  });
});
