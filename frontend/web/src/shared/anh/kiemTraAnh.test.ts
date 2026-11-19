import { describe, expect, it } from "vitest";
import {
  CANH_DAI_TOI_DA_PX,
  CHAT_LUONG_NEN,
  DUNG_LUONG_ANH_TOI_DA_BYTE,
  KIEU_ANH_CHAP_NHAN,
  SO_ANH_TOI_DA,
} from "./hangSo";
import {
  laTepAnhChapNhan,
  locTepDaChon,
  THONG_DIEP_TU_CHOI_ANH,
} from "./kiemTraAnh";

const tep = (name: string, type: string) => ({ name, type });

describe("hằng số ảnh (FC 7.1)", () => {
  it("đúng giá trị tài liệu nguồn", () => {
    expect(SO_ANH_TOI_DA).toBe(5);
    expect(DUNG_LUONG_ANH_TOI_DA_BYTE).toBe(2_097_152);
    expect(CANH_DAI_TOI_DA_PX).toBe(1280);
    expect(CHAT_LUONG_NEN).toEqual([0.8, 0.7, 0.6]);
    expect(KIEU_ANH_CHAP_NHAN).toEqual([
      "image/jpeg",
      "image/png",
      "image/webp",
    ]);
  });
});

describe("locTepDaChon (FA 4.10 bước 2, DS CMP-12)", () => {
  it("nhận jpeg, png, webp; từ chối loại khác, giữ thứ tự đã chọn", () => {
    const kq = locTepDaChon(
      [
        tep("a.jpg", "image/jpeg"),
        tep("b.gif", "image/gif"),
        tep("c.png", "image/png"),
        tep("d.heic", "image/heic"),
        tep("e.webp", "image/webp"),
      ],
      0,
    );
    expect(kq.dsNhan.map((t) => t.name)).toEqual(["a.jpg", "c.png", "e.webp"]);
    expect(kq.dsLyDo).toEqual(["SAI_KIEU"]);
  });

  it("chỉ nhận tới đủ 5 ảnh tính cả ảnh đã có; phần dư báo 'Tối đa 5 ảnh'", () => {
    const kq = locTepDaChon(
      [
        tep("1.jpg", "image/jpeg"),
        tep("2.jpg", "image/jpeg"),
        tep("3.jpg", "image/jpeg"),
      ],
      3,
    );
    expect(kq.dsNhan.map((t) => t.name)).toEqual(["1.jpg", "2.jpg"]);
    expect(kq.dsLyDo).toEqual(["QUA_SO_LUONG"]);
  });

  it("đã đủ 5 ảnh: không nhận thêm", () => {
    const kq = locTepDaChon([tep("1.jpg", "image/jpeg")], 5);
    expect(kq.dsNhan).toEqual([]);
    expect(kq.dsLyDo).toEqual(["QUA_SO_LUONG"]);
  });

  it("gộp lý do không trùng, theo thứ tự xuất hiện", () => {
    const kq = locTepDaChon(
      [
        tep("x.gif", "image/gif"),
        tep("1.jpg", "image/jpeg"),
        tep("2.jpg", "image/jpeg"),
        tep("y.bmp", "image/bmp"),
      ],
      4,
    );
    expect(kq.dsNhan.map((t) => t.name)).toEqual(["1.jpg"]);
    expect(kq.dsLyDo).toEqual(["SAI_KIEU", "QUA_SO_LUONG"]);
  });

  it("thông điệp nguyên văn DS CMP-12", () => {
    expect(THONG_DIEP_TU_CHOI_ANH).toEqual({
      SAI_KIEU: "Chỉ nhận ảnh jpeg, png hoặc webp",
      QUA_DUNG_LUONG: "Mỗi ảnh tối đa 2 MB",
      QUA_SO_LUONG: "Tối đa 5 ảnh",
    });
  });
});

describe("laTepAnhChapNhan (kiểu viết hoa, kiểu rỗng)", () => {
  it.each([
    ["anh.jpg", "IMAGE/JPEG", true],
    ["anh.webp", "", true],
    ["ANH.JPEG", "", true],
    ["anh.png", " ", true],
    ["anh.gif", "", false],
    ["anh", "", false],
    // Kiểu đã biết thì không xét đuôi.
    ["anh.jpg", "image/gif", false],
  ])("%s, type=%j → %s", (name, type, ketQua) => {
    expect(laTepAnhChapNhan({ name, type })).toBe(ketQua);
  });
});
