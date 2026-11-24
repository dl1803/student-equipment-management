import {
  AxiosError,
  AxiosHeaders,
  CanceledError,
  type AxiosResponse,
  type InternalAxiosRequestConfig,
} from "axios";
import { describe, expect, it, vi } from "vitest";
import { chuanHoaLoi } from "./chuanHoaLoi";

const CONFIG = { headers: new AxiosHeaders() } as InternalAxiosRequestConfig;
const TRACE_ID = "00-4bf92f3577b34da6a3ce929d0e0e4736-00f067aa0ba902b7-00";

function loiHttp(
  status: number,
  data: unknown,
  kieuNoiDung = "application/problem+json; charset=utf-8",
): AxiosError {
  const phanHoi: AxiosResponse = {
    data,
    status,
    statusText: "",
    headers: { "content-type": kieuNoiDung },
    config: CONFIG,
  };
  return new AxiosError(
    "loi",
    AxiosError.ERR_BAD_REQUEST,
    CONFIG,
    null,
    phanHoi,
  );
}

describe("chuanHoaLoi", () => {
  it("FA 3.3: problem+json có maLoi → HTTP kèm status, maLoi, detail, traceId", async () => {
    const loi = await chuanHoaLoi(
      loiHttp(422, {
        status: 422,
        maLoi: "SV_BI_KHOA_QUYEN_MUON",
        detail: "Sinh viên đang bị khóa quyền mượn",
        traceId: TRACE_ID,
      }),
    );
    expect(loi).toEqual({
      loai: "HTTP",
      status: 422,
      maLoi: "SV_BI_KHOA_QUYEN_MUON",
      detail: "Sinh viên đang bị khóa quyền mượn",
      traceId: TRACE_ID,
    });
  });

  it("FA 3.3: ValidationProblemDetails → errors theo khóa trường (dạng mảng)", async () => {
    const loi = await chuanHoaLoi(
      loiHttp(400, {
        maLoi: "DU_LIEU_KHONG_HOP_LE",
        errors: {
          "dsLoaiVaSoLuong[0].soLuong": ["Số lượng phải lớn hơn 0."],
          hong: "không phải mảng",
        },
      }),
    );
    expect(loi.errors).toEqual({
      "dsLoaiVaSoLuong[0].soLuong": ["Số lượng phải lớn hơn 0."],
    });
  });

  it("FA 3.3: lỗi 500 chỉ có traceId → không có maLoi", async () => {
    const loi = await chuanHoaLoi(
      loiHttp(500, { status: 500, traceId: TRACE_ID }),
    );
    expect(loi).toEqual({ loai: "HTTP", status: 500, traceId: TRACE_ID });
  });

  it("FA 3.3 quy tắc 1: thân không phải problem+json (trang lỗi hạ tầng) → HTTP không có maLoi", async () => {
    const loi = await chuanHoaLoi(
      loiHttp(502, "<html><body>Bad Gateway</body></html>", "text/html"),
    );
    expect(loi).toEqual({ loai: "HTTP", status: 502 });
  });

  it("FA 3.3: problem+json nhưng thân là chuỗi JSON → vẫn đọc được", async () => {
    const loi = await chuanHoaLoi(
      loiHttp(409, JSON.stringify({ maLoi: "XUNG_DOT_CAP_NHAT" })),
    );
    expect(loi.maLoi).toBe("XUNG_DOT_CAP_NHAT");
  });

  it("FA 4.11 quy tắc 2: thân lỗi dạng Blob (responseType blob) → đọc thành problem+json", async () => {
    const blob = new Blob(
      [JSON.stringify({ maLoi: "KHONG_DU_QUYEN", traceId: TRACE_ID })],
      { type: "application/problem+json" },
    );
    const loi = await chuanHoaLoi(loiHttp(403, blob));
    expect(loi).toEqual({
      loai: "HTTP",
      status: 403,
      maLoi: "KHONG_DU_QUYEN",
      traceId: TRACE_ID,
    });
  });

  it("FA 4.11: đọc thân Blob thất bại → HTTP không có maLoi, không ném", async () => {
    const blob = new Blob(["x"]);
    blob.text = () => Promise.reject(new Error("luồng bị hủy"));

    const loi = await chuanHoaLoi(loiHttp(500, blob));

    expect(loi).toEqual({ loai: "HTTP", status: 500 });
  });

  it("FA 3.3: problem+json hỏng → HTTP không có maLoi, không ném", async () => {
    const loi = await chuanHoaLoi(loiHttp(500, "{khong-phai-json"));
    expect(loi).toEqual({ loai: "HTTP", status: 500 });
  });

  it("FA 3.3: mất kết nối (không có phản hồi) → MANG", async () => {
    const loi = await chuanHoaLoi(
      new AxiosError("Network Error", AxiosError.ERR_NETWORK, CONFIG),
    );
    expect(loi).toEqual({ loai: "MANG" });
  });

  it("ĐX-FA-04: quá thời gian chờ → QUA_THOI_GIAN", async () => {
    expect(
      await chuanHoaLoi(
        new AxiosError("timeout", AxiosError.ETIMEDOUT, CONFIG),
      ),
    ).toEqual({ loai: "QUA_THOI_GIAN" });
    expect(
      await chuanHoaLoi(
        new AxiosError("timeout", AxiosError.ECONNABORTED, CONFIG),
      ),
    ).toEqual({ loai: "QUA_THOI_GIAN" });
  });

  it("FA 3.3: yêu cầu bị hủy → HUY", async () => {
    expect(await chuanHoaLoi(new CanceledError())).toEqual({ loai: "HUY" });
  });

  it("FA 3.3: ApiError đã chuẩn hóa → trả nguyên", async () => {
    const goc = {
      loai: "HTTP" as const,
      status: 429,
      maLoi: "QUA_NHIEU_YEU_CAU",
    };
    expect(await chuanHoaLoi(goc)).toBe(goc);
  });

  it("FA 3.3: lỗi không đến từ Axios → lỗi hệ thống chung, không làm vỡ giao diện", async () => {
    const ghiLoi = vi
      .spyOn(console, "error")
      .mockImplementation(() => undefined);
    expect(await chuanHoaLoi(new TypeError("x"))).toEqual({ loai: "HTTP" });
    ghiLoi.mockRestore();
  });
});
