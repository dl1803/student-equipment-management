import { describe, expect, it } from "vitest";
import {
  formatGio,
  formatNgay,
  formatNgayGio,
  thanhChuoiGioVn,
} from "./formatThoiGian";

describe("formatNgayGio", () => {
  it("ĐX-FC-12: thời điểm UTC → dd/MM/yyyy HH:mm giờ Việt Nam", () => {
    expect(formatNgayGio("2026-11-05T07:30:00Z")).toBe("05/11/2026 14:30");
  });

  it("ĐX-FC-12: qua nửa đêm giờ Việt Nam → sang ngày mới, không ra 24:00", () => {
    expect(formatNgayGio("2026-11-05T17:00:00Z")).toBe("06/11/2026 00:00");
  });

  it("ĐX-FC-12: chuỗi có độ lệch khác vẫn quy về giờ Việt Nam", () => {
    expect(formatNgayGio("2026-11-05T14:30:00+07:00")).toBe("05/11/2026 14:30");
  });

  it("ĐX-DS-12: null, undefined, rỗng → —", () => {
    expect(formatNgayGio(null)).toBe("—");
    expect(formatNgayGio(undefined)).toBe("—");
    expect(formatNgayGio("")).toBe("—");
  });

  it("Mã lạ không làm vỡ: chuỗi không đọc được → trả nguyên văn", () => {
    expect(formatNgayGio("khong-phai-ngay")).toBe("khong-phai-ngay");
  });
});

describe("formatNgay", () => {
  it("ĐX-FC-12: yyyy-MM-dd → dd/MM/yyyy, không đổi múi giờ", () => {
    expect(formatNgay("2026-11-05")).toBe("05/11/2026");
    expect(formatNgay("2026-01-01")).toBe("01/01/2026");
  });

  it("ĐX-DS-12: null → —; sai dạng → nguyên văn", () => {
    expect(formatNgay(null)).toBe("—");
    expect(formatNgay("05/11/2026")).toBe("05/11/2026");
  });
});

describe("formatGio", () => {
  it("ĐX-FC-12: giờ quầy HH:mm:ss → HH:mm", () => {
    expect(formatGio("07:30:00")).toBe("07:30");
    expect(formatGio("17:00")).toBe("17:00");
  });

  it("ĐX-DS-12: null → —", () => {
    expect(formatGio(null)).toBe("—");
  });
});

describe("thanhChuoiGioVn", () => {
  it("ĐX-FA-10: giờ nhập tay → ISO kèm +07:00", () => {
    expect(thanhChuoiGioVn("2026-11-05T14:30")).toBe(
      "2026-11-05T14:30:00+07:00",
    );
    expect(thanhChuoiGioVn("2026-11-05T14:30:15")).toBe(
      "2026-11-05T14:30:15+07:00",
    );
  });

  it("ĐX-FA-10: sai dạng → null", () => {
    expect(thanhChuoiGioVn("")).toBeNull();
    expect(thanhChuoiGioVn("05/11/2026 14:30")).toBeNull();
  });
});
