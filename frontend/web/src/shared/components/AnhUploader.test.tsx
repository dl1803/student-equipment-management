// @vitest-environment jsdom
import { act, fireEvent, render, screen, within } from "@testing-library/react";
import { afterEach, beforeEach, describe, expect, it, vi } from "vitest";
import { anhTai } from "@/shared/anh/api";
import type * as MoDunNenAnh from "@/shared/anh/nenAnh";
import { LoiNenAnh, nenAnh } from "@/shared/anh/nenAnh";
import type { TaiAnhResponse } from "@/shared/api/types";
import { AnhUploader, type GiaTriAnhUploader } from "./AnhUploader";

vi.mock("@/shared/anh/api", () => ({ anhTai: vi.fn() }));
vi.mock("@/shared/anh/nenAnh", async (goc) => ({
  ...(await goc<typeof MoDunNenAnh>()),
  nenAnh: vi.fn(),
}));

const anhTaiGia = vi.mocked(anhTai);
const nenAnhGia = vi.mocked(nenAnh);

type Hua<T> = {
  promise: Promise<T>;
  xong: (v: T) => void;
  loi: (e: unknown) => void;
};
function taoHua<T>(): Hua<T> {
  let xong!: (v: T) => void;
  let loi!: (e: unknown) => void;
  const promise = new Promise<T>((a, b) => {
    xong = a;
    loi = b;
  });
  return { promise, xong, loi };
}

let demUrl = 0;
const taoUrl = vi.fn(() => `blob:tam-${++demUrl}`);
const thuHoiUrl = vi.fn();

beforeEach(() => {
  demUrl = 0;
  taoUrl.mockClear();
  thuHoiUrl.mockClear();
  anhTaiGia.mockReset();
  nenAnhGia.mockReset();
  nenAnhGia.mockImplementation(
    async (tep) => new Blob([tep], { type: "image/jpeg" }),
  );
  vi.stubGlobal("URL", {
    ...URL,
    createObjectURL: taoUrl,
    revokeObjectURL: thuHoiUrl,
  });
});
afterEach(() => {
  vi.unstubAllGlobals();
});

const tep = (ten: string, type = "image/jpeg") =>
  new File(["x"], ten, { type });

/** Chờ chuỗi Promise của hàng đợi chạy tiếp. */
async function choXuLy() {
  await act(async () => {
    for (let i = 0; i < 10; i++) await Promise.resolve();
  });
}

function veKhu(props: Partial<Parameters<typeof AnhUploader>[0]> = {}) {
  const onThayDoi = vi.fn<(g: GiaTriAnhUploader) => void>();
  const kq = render(
    <AnhUploader
      giaiDoan="BAN_DAU"
      tenThietBi="Laptop Dell"
      onThayDoi={onThayDoi}
      {...props}
    />,
  );
  const oChon =
    kq.container.querySelector<HTMLInputElement>('input[type="file"]');
  if (oChon === null) throw new Error("thiếu ô chọn tệp");
  const chon = async (...dsTep: File[]) => {
    fireEvent.change(oChon, { target: { files: dsTep } });
    await choXuLy();
  };
  const giaTriCuoi = () => onThayDoi.mock.calls.at(-1)?.[0];
  return { ...kq, oChon, chon, onThayDoi, giaTriCuoi };
}

describe("AnhUploader (CMP-12)", () => {
  it("ô chọn tệp: accept đúng 3 định dạng, multiple, không có capture; nhãn 'Ảnh (không bắt buộc)'", () => {
    const { oChon, giaTriCuoi } = veKhu();
    expect(oChon.accept).toBe("image/jpeg,image/png,image/webp");
    expect(oChon.multiple).toBe(true);
    expect(oChon.hasAttribute("capture")).toBe(false);
    expect(
      screen.getByRole("group", { name: "Ảnh (không bắt buộc)" }),
    ).toBeTruthy();
    // Ảnh tùy chọn: chưa có ảnh vẫn gửi được.
    expect(giaTriCuoi()).toEqual({ dsMaAnh: [], coTheGui: true });
    expect(
      screen.getByRole("button", { name: /Thêm ảnh/ }).textContent,
    ).toContain("0/5");
  });

  it("tải lên tuần tự từng ảnh theo thứ tự chọn, đúng giaiDoan; chặn gửi tới khi xong", async () => {
    const dsHua = [taoHua<TaiAnhResponse>(), taoHua<TaiAnhResponse>()];
    let lan = 0;
    anhTaiGia.mockImplementation(() => {
      const hua = dsHua[lan++];
      if (hua === undefined) throw new Error("gọi thừa");
      return hua.promise;
    });
    const { chon, giaTriCuoi } = veKhu({ giaiDoan: "KHI_TRA" });

    await chon(tep("1.jpg"), tep("2.png", "image/png"));
    // Chỉ một yêu cầu đang chạy.
    expect(anhTaiGia).toHaveBeenCalledTimes(1);
    expect(anhTaiGia.mock.calls[0]?.[1]).toBe("KHI_TRA");
    expect(giaTriCuoi()?.coTheGui).toBe(false);
    expect(screen.getAllByText("Đang tải").length).toBeGreaterThan(0);

    await act(async () => dsHua[0]?.xong({ maAnh: "m1" }));
    await choXuLy();
    expect(anhTaiGia).toHaveBeenCalledTimes(2);
    expect(giaTriCuoi()).toEqual({ dsMaAnh: ["m1"], coTheGui: false });

    await act(async () => dsHua[1]?.xong({ maAnh: "m2" }));
    await choXuLy();
    expect(giaTriCuoi()).toEqual({ dsMaAnh: ["m1", "m2"], coTheGui: true });
    expect(
      screen.getByRole("button", { name: /Thêm ảnh/ }).textContent,
    ).toContain("2/5");
  });

  it("phản hồi chỉ có maAnh: ảnh xem trước là URL tạm của tệp đã nén, không bịa URL", async () => {
    anhTaiGia.mockResolvedValue({ maAnh: "m1" });
    const { chon } = veKhu();
    await chon(tep("1.jpg"));
    const anh = within(screen.getByRole("group", { name: "Ảnh 1" })).getByRole(
      "img",
    );
    expect(anh.getAttribute("src")).toBe("blob:tam-1");
    expect(taoUrl).toHaveBeenCalledOnce();
  });

  it("lỗi tải lên: ô Lỗi có Thử lại và Gỡ ảnh, chặn gửi, KHÔNG tự thử lại; Thử lại gọi lại đúng một lần", async () => {
    anhTaiGia.mockRejectedValueOnce(new Error("mạng"));
    const { chon, giaTriCuoi } = veKhu();
    await chon(tep("1.jpg"));

    const o = screen.getByRole("group", { name: "Ảnh 1" });
    expect(within(o).getByText("Lỗi tải lên")).toBeTruthy();
    expect(o.className).toContain("border-border-error");
    expect(giaTriCuoi()?.coTheGui).toBe(false);
    await choXuLy();
    expect(anhTaiGia).toHaveBeenCalledTimes(1);

    anhTaiGia.mockResolvedValueOnce({ maAnh: "m1" });
    fireEvent.click(within(o).getByRole("button", { name: "Thử lại" }));
    await choXuLy();
    expect(anhTaiGia).toHaveBeenCalledTimes(2);
    expect(giaTriCuoi()).toEqual({ dsMaAnh: ["m1"], coTheGui: true });
  });

  it("gỡ ảnh lỗi: bỏ khỏi lưới, thu hồi đúng URL tạm, mở lại nút gửi", async () => {
    anhTaiGia.mockRejectedValueOnce(new Error("mạng"));
    const { chon, giaTriCuoi } = veKhu();
    await chon(tep("1.jpg"));
    fireEvent.click(screen.getByRole("button", { name: "Gỡ ảnh 1" }));
    expect(screen.queryByRole("group", { name: "Ảnh 1" })).toBeNull();
    expect(thuHoiUrl).toHaveBeenCalledExactlyOnceWith("blob:tam-1");
    expect(giaTriCuoi()).toEqual({ dsMaAnh: [], coTheGui: true });
  });

  it("đang tải thì không có nút Gỡ ảnh", async () => {
    anhTaiGia.mockReturnValue(new Promise(() => {}));
    const { chon } = veKhu();
    await chon(tep("1.jpg"));
    const o = screen.getByRole("group", { name: "Ảnh 1" });
    expect(o.getAttribute("aria-busy")).toBe("true");
    expect(within(o).queryByRole("button", { name: /Gỡ ảnh/ })).toBeNull();
  });

  it("từ chối sai định dạng và vượt 5 ảnh ngay tại khu; đủ 5 thì ẩn 'Thêm ảnh'", async () => {
    anhTaiGia.mockImplementation(async () => ({ maAnh: `m${Math.random()}` }));
    const { chon } = veKhu();
    await chon(
      tep("a.gif", "image/gif"),
      ...["1", "2", "3", "4", "5", "6"].map((t) => tep(`${t}.jpg`)),
    );
    const canhBao = screen.getByRole("alert");
    expect(canhBao.textContent).toContain("Chỉ nhận ảnh jpeg, png hoặc webp");
    expect(canhBao.textContent).toContain("Tối đa 5 ảnh");
    expect(nenAnhGia).toHaveBeenCalledTimes(5);
    expect(anhTaiGia).toHaveBeenCalledTimes(5);
    expect(screen.queryByRole("button", { name: /Thêm ảnh/ })).toBeNull();
  });

  it("nén vẫn quá 2 MB: không thêm vào lưới, báo 'Mỗi ảnh tối đa 2 MB', không tải lên", async () => {
    nenAnhGia.mockRejectedValueOnce(new LoiNenAnh("QUA_LON"));
    const { chon, giaTriCuoi } = veKhu();
    await chon(tep("to.jpg"));
    expect(screen.getByRole("alert").textContent).toContain(
      "Mỗi ảnh tối đa 2 MB",
    );
    expect(screen.queryByRole("group", { name: "Ảnh 1" })).toBeNull();
    expect(anhTaiGia).not.toHaveBeenCalled();
    expect(giaTriCuoi()).toEqual({ dsMaAnh: [], coTheGui: true });
  });

  it("ảnh có sẵn: hiện bằng URL máy chủ, có trong dsMaAnh, gỡ được mà không thu hồi URL máy chủ", async () => {
    const { giaTriCuoi } = veKhu({
      giaiDoan: "THIET_BI",
      dsAnhCoSan: [{ maAnh: "s1", url: "https://cdn.test/s1.jpg" }],
    });
    const o = screen.getByRole("group", { name: "Ảnh 1" });
    expect(within(o).getByRole("img").getAttribute("src")).toBe(
      "https://cdn.test/s1.jpg",
    );
    expect(giaTriCuoi()).toEqual({ dsMaAnh: ["s1"], coTheGui: true });
    fireEvent.click(within(o).getByRole("button", { name: "Gỡ ảnh 1" }));
    expect(thuHoiUrl).not.toHaveBeenCalled();
    expect(giaTriCuoi()).toEqual({ dsMaAnh: [], coTheGui: true });
  });

  it("gỡ component: thu hồi mọi URL tạm còn lại", async () => {
    anhTaiGia.mockResolvedValue({ maAnh: "m" });
    const { chon, unmount } = veKhu();
    await chon(tep("1.jpg"), tep("2.jpg"));
    unmount();
    expect(thuHoiUrl.mock.calls.map((c) => c[0]).sort()).toEqual([
      "blob:tam-1",
      "blob:tam-2",
    ]);
  });

  it("bấm ảnh mở lớp xem với tên 'Ảnh n của <tên thiết bị>'", async () => {
    anhTaiGia.mockResolvedValue({ maAnh: "m1" });
    const { chon } = veKhu();
    await chon(tep("1.jpg"));
    fireEvent.click(screen.getByRole("button", { name: "Xem ảnh 1" }));
    expect(
      screen.getByRole("dialog", { name: "Ảnh 1 của Laptop Dell" }),
    ).toBeTruthy();
  });

  it("vô hiệu: hiện lý do, không thêm được ảnh", () => {
    veKhu({ lyDoVoHieu: "Kết quả Mất không có ảnh" });
    expect(screen.getByText("Kết quả Mất không có ảnh")).toBeTruthy();
    expect(
      screen.getByRole("button", { name: /Thêm ảnh/ }).hasAttribute("disabled"),
    ).toBe(true);
  });
});
