// Nén ảnh trước khi tải lên (FA 4.10 bước 3–4; FC 5.9). Sở hữu: Thịnh (shared core).
// createImageBitmap → canvas nền trắng (ảnh PNG/WebP trong suốt không thành nền đen) →
// toBlob("image/jpeg", q) với q lần lượt 0,8 rồi 0,7 rồi 0,6; vẫn lớn hơn 2 MB thì báo "ảnh quá lớn".
// Cạnh dài hơn 1280 px thì thu nhỏ giữ tỉ lệ; ảnh nhỏ hơn không phóng to.
import {
  CANH_DAI_TOI_DA_PX,
  CHAT_LUONG_NEN,
  DUNG_LUONG_ANH_TOI_DA_BYTE,
  KIEU_ANH_SAU_NEN,
} from "./hangSo";

/** Lỗi nén: QUA_LON khi mọi mức chất lượng vẫn vượt 2 MB; KHONG_DOC_DUOC khi trình duyệt không giải mã được tệp. */
export class LoiNenAnh extends Error {
  readonly lyDo: "QUA_LON" | "KHONG_DOC_DUOC";

  constructor(lyDo: "QUA_LON" | "KHONG_DOC_DUOC") {
    super(lyDo === "QUA_LON" ? "Ảnh quá lớn" : "Không đọc được ảnh");
    this.name = "LoiNenAnh";
    this.lyDo = lyDo;
  }
}

/** Kích thước sau nén: cạnh dài tối đa CANH_DAI_TOI_DA_PX, giữ tỉ lệ, làm tròn, không phóng to. */
export function kichThuocSauNen(
  rong: number,
  cao: number,
): { rong: number; cao: number } {
  const canhDai = Math.max(rong, cao);
  if (canhDai <= CANH_DAI_TOI_DA_PX) return { rong, cao };
  const tiLe = CANH_DAI_TOI_DA_PX / canhDai;
  return {
    rong: Math.max(1, Math.round(rong * tiLe)),
    cao: Math.max(1, Math.round(cao * tiLe)),
  };
}

function xuatBlob(canvas: HTMLCanvasElement, chatLuong: number) {
  return new Promise<Blob | null>((resolve) => {
    canvas.toBlob(resolve, KIEU_ANH_SAU_NEN, chatLuong);
  });
}

/**
 * Nén một ảnh thành JPEG không quá DUNG_LUONG_ANH_TOI_DA_BYTE.
 *
 * @throws LoiNenAnh
 */
export async function nenAnh(tep: Blob): Promise<Blob> {
  let anh: ImageBitmap;
  try {
    anh = await createImageBitmap(tep);
  } catch {
    throw new LoiNenAnh("KHONG_DOC_DUOC");
  }
  try {
    const { rong, cao } = kichThuocSauNen(anh.width, anh.height);
    const canvas = document.createElement("canvas");
    canvas.width = rong;
    canvas.height = cao;
    const ve = canvas.getContext("2d");
    if (ve === null) throw new LoiNenAnh("KHONG_DOC_DUOC");
    // Màu nền là màu kỹ thuật của tệp JPEG (không có kênh trong suốt), không phải màu giao diện.
    ve.fillStyle = "#ffffff";
    ve.fillRect(0, 0, rong, cao);
    ve.drawImage(anh, 0, 0, rong, cao);

    for (const chatLuong of CHAT_LUONG_NEN) {
      const ketQua = await xuatBlob(canvas, chatLuong);
      if (ketQua === null) throw new LoiNenAnh("KHONG_DOC_DUOC");
      if (ketQua.size <= DUNG_LUONG_ANH_TOI_DA_BYTE) return ketQua;
    }
    throw new LoiNenAnh("QUA_LON");
  } finally {
    anh.close();
  }
}
