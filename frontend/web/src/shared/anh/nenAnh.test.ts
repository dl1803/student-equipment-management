// @vitest-environment jsdom
import { afterEach, beforeEach, describe, expect, it, vi } from "vitest";
import { DUNG_LUONG_ANH_TOI_DA_BYTE } from "./hangSo";
import { kichThuocSauNen, LoiNenAnh, nenAnh } from "./nenAnh";

describe("kichThuocSauNen (cạnh dài 1280 px, không phóng to)", () => {
  it.each([
    [2560, 1440, 1280, 720],
    [1440, 2560, 720, 1280],
    [4000, 3000, 1280, 960],
    [1280, 800, 1280, 800],
    [800, 600, 800, 600],
    [3001, 1, 1280, 1],
  ])("%i×%i → %i×%i", (rong, cao, rongMoi, caoMoi) => {
    expect(kichThuocSauNen(rong, cao)).toEqual({
      rong: rongMoi,
      cao: caoMoi,
    });
  });
});

describe("nenAnh (FA 4.10 bước 3–4)", () => {
  const dongAnh = vi.fn();
  let dsKichThuocBlob: number[];
  let dsChatLuong: number[];
  let ve: {
    fillStyle: string;
    fillRect: ReturnType<typeof vi.fn>;
    drawImage: ReturnType<typeof vi.fn>;
  };

  beforeEach(() => {
    dongAnh.mockClear();
    dsChatLuong = [];
    dsKichThuocBlob = [];
    vi.stubGlobal(
      "createImageBitmap",
      vi.fn(async () => ({ width: 2560, height: 1440, close: dongAnh })),
    );
    ve = { fillStyle: "", fillRect: vi.fn(), drawImage: vi.fn() };
    vi.spyOn(HTMLCanvasElement.prototype, "getContext").mockImplementation(
      function (this: HTMLCanvasElement) {
        // Gắn ve vào canvas để kiểm kích thước canvas khi vẽ.
        ve.fillRect.mockImplementation(() => {
          expect([this.width, this.height]).toEqual([1280, 720]);
        });
        return ve as unknown as RenderingContext;
      },
    );
    vi.spyOn(HTMLCanvasElement.prototype, "toBlob").mockImplementation(
      (goiLai, kieu, chatLuong) => {
        expect(kieu).toBe("image/jpeg");
        dsChatLuong.push(chatLuong ?? -1);
        const kichThuoc = dsKichThuocBlob.shift() ?? 1;
        goiLai(new Blob([new Uint8Array(kichThuoc)], { type: "image/jpeg" }));
      },
    );
  });

  afterEach(() => {
    vi.restoreAllMocks();
    vi.unstubAllGlobals();
  });

  const tepGoc = () => new Blob(["x"], { type: "image/png" });

  it("vẽ trên nền trắng rồi xuất JPEG 0,8; đủ nhỏ thì dừng", async () => {
    dsKichThuocBlob = [1000];
    const kq = await nenAnh(tepGoc());
    expect(kq.size).toBe(1000);
    expect(dsChatLuong).toEqual([0.8]);
    expect(ve.fillStyle).toBe("#ffffff");
    expect(ve.fillRect).toHaveBeenCalledWith(0, 0, 1280, 720);
    // Nền trắng vẽ trước ảnh.
    expect(ve.fillRect.mock.invocationCallOrder[0]).toBeLessThan(
      ve.drawImage.mock.invocationCallOrder[0] ?? 0,
    );
    expect(ve.drawImage).toHaveBeenCalledWith(
      expect.anything(),
      0,
      0,
      1280,
      720,
    );
    expect(dongAnh).toHaveBeenCalledOnce();
  });

  it("lớn hơn 2 MB thì thử 0,7 rồi 0,6; đúng 2.097.152 byte vẫn nhận", async () => {
    dsKichThuocBlob = [
      DUNG_LUONG_ANH_TOI_DA_BYTE + 1,
      DUNG_LUONG_ANH_TOI_DA_BYTE + 1,
      DUNG_LUONG_ANH_TOI_DA_BYTE,
    ];
    const kq = await nenAnh(tepGoc());
    expect(dsChatLuong).toEqual([0.8, 0.7, 0.6]);
    expect(kq.size).toBe(DUNG_LUONG_ANH_TOI_DA_BYTE);
  });

  it("vẫn lớn hơn 2 MB ở 0,6 thì báo QUA_LON và giải phóng ảnh", async () => {
    dsKichThuocBlob = [3e6, 3e6, 3e6];
    const loi = await nenAnh(tepGoc()).catch((e: unknown) => e);
    expect(loi).toBeInstanceOf(LoiNenAnh);
    expect(loi instanceof LoiNenAnh && loi.lyDo).toBe("QUA_LON");
    expect(dsChatLuong).toEqual([0.8, 0.7, 0.6]);
    expect(dongAnh).toHaveBeenCalledOnce();
  });

  it("tệp không giải mã được thì báo KHONG_DOC_DUOC", async () => {
    vi.stubGlobal(
      "createImageBitmap",
      vi.fn(async () => {
        throw new DOMException("bad", "InvalidStateError");
      }),
    );
    const loi = await nenAnh(tepGoc()).catch((e: unknown) => e);
    expect(loi instanceof LoiNenAnh && loi.lyDo).toBe("KHONG_DOC_DUOC");
  });
});
