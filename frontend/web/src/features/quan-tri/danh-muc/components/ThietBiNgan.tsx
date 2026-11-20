"use client";

// Ngăn biểu mẫu Nhập kho/Sửa thiết bị — SCR-QT-04 (UI-Screens 7.4; UF-QT-01; Wireframes SCR-QT-04,
// P-04 ngăn trượt từ bên phải, P-05 khu đính ảnh). Sở hữu: Thịnh.
// - Mã thiết bị và Giá trị chỉ nhập khi thêm; khi sửa mã là ô chỉ đọc (không vô hiệu, DS CMP-04),
//   giá trị hiện hành hiển thị chỉ đọc. ThietBiCapNhatRequest không có mã, giá trị, tình trạng.
// - KHÔNG có ô Tình trạng: thiết bị mới là Sẵn sàng, tình trạng chỉ đổi qua nghiệp vụ (UF-QT-01 E2).
// - Nhà cung cấp chọn nhiều từ danh sách có sẵn (dsMaNCC); không thêm nhà cung cấp mới ở đây.
// - Ảnh qua AnhUploader (giaiDoan THIET_BI) → dsMaAnh; nút Lưu vô hiệu khi còn ảnh đang tải/lỗi.
// - Nguồn lựa chọn (loại, phòng, nhà cung cấp) tải đủ mọi trang; khi sửa luôn có mục đang gán dù
//   nguồn chưa tải xong hoặc thiếu mục đó, để không âm thầm đổi giá trị.
// - Trường tùy chọn để trống gửi null (kể cả khi sửa, để xóa giá trị cũ). Kiểm tra phía client chỉ
//   theo openapi.yaml: bắt buộc, minLength 1, số nguyên ≥ 0 (FC 5.6, ĐX-FA-08).
// - Lỗi: 400 cạnh trường (ganLoiMayChu); MA_THIET_BI_TRUNG dưới ô mã; XUNG_DOT_CAP_NHAT nạp bản
//   mới nhất rồi sửa lại; KHONG_TIM_THAY đóng ngăn; mất kết quả → "Kiểm tra lại" (FA 3.11).
import { useEffect, useState } from "react";
import { Controller, useForm } from "react-hook-form";
import { laChuaBietKetQua, type ApiError } from "@/shared/api/apiError";
import type {
  LoaiThietBi,
  NhaCungCap,
  PhongThietBi,
  ThietBi,
} from "@/shared/api/types";
import { AnhUploader } from "@/shared/components/AnhUploader";
import { Banner } from "@/shared/components/Banner";
import { Button } from "@/shared/components/Button";
import { Dialog } from "@/shared/components/Dialog";
import { FormField } from "@/shared/components/FormField";
import { Select, type LuaChon } from "@/shared/components/Select";
import {
  SelectNhieu,
  type LuaChonNhieu,
} from "@/shared/components/SelectNhieu";
import { TextArea } from "@/shared/components/TextArea";
import { TextField } from "@/shared/components/TextField";
import { docSoNguyen } from "@/shared/format/docSo";
import { formatTien } from "@/shared/format/formatSo";
import { ganLoiMayChu } from "@/shared/hooks/ganLoiMayChu";
import { useThaoTacGhi } from "@/shared/hooks/useThaoTacGhi";
import { thongBaoTheoLoi, type ThongBao } from "@/shared/labels/thongDiepLoi";
import { thongTinNhanPhatSinh } from "@/shared/labels/trangThai";
import {
  useLuuThietBiMutation,
  useTatCaLoaiQuery,
  useTatCaNhaCungCapQuery,
  useTatCaPhongQuery,
} from "../hooks";
import { THONG_DIEP_DANH_MUC } from "../thongDiep";

/** Giá trị biểu mẫu: ngày là chuỗi `yyyy-MM-dd` của ô date; số nhập dạng chuỗi (FC 5.6). */
type GiaTriThietBi = {
  maThietBi: string;
  tenThietBi: string;
  /** "" là chưa chọn. */
  maLoai: string;
  maPhong: string;
  moTa: string;
  thongSoKyThuat: string;
  ngayNhapKho: string;
  thoiHanBaoHanh: string;
  viTriKe: string;
  viTriTu: string;
  dsMaNCC: string[];
  /** Do AnhUploader cập nhật; có trong biểu mẫu để lỗi `dsMaAnh` của máy chủ gắn đúng chỗ. */
  dsMaAnh: string[];
  giaTri: string;
};

export type ThietBiNganProps = {
  /** null: nhập kho thiết bị mới. */
  thietBi: ThietBi | null;
  onDong: () => void;
  /** Tìm bản mới nhất sau xung đột 409; null nếu không còn thấy trong trang hiện tại. */
  layBanMoiNhat: (maThietBi: string) => Promise<ThietBi | null>;
  /** Tải lại danh sách; true nếu tải thành công. */
  taiLaiDanhSach: () => Promise<boolean>;
  onKhongConTonTai: () => void;
};

function giaTriBanDau(thietBi: ThietBi | null): GiaTriThietBi {
  return {
    maThietBi: thietBi?.maThietBi ?? "",
    tenThietBi: thietBi?.tenThietBi ?? "",
    maLoai: thietBi?.maLoai ?? "",
    maPhong: thietBi?.maPhong ?? "",
    moTa: thietBi?.moTa ?? "",
    thongSoKyThuat: thietBi?.thongSoKyThuat ?? "",
    ngayNhapKho: thietBi?.ngayNhapKho ?? "",
    thoiHanBaoHanh: thietBi?.thoiHanBaoHanh ?? "",
    viTriKe: thietBi?.viTriKe ?? "",
    viTriTu: thietBi?.viTriTu ?? "",
    dsMaNCC: thietBi?.dsNhaCungCap.map((ncc) => ncc.maNCC) ?? [],
    dsMaAnh: thietBi?.dsAnh.map((anh) => anh.maAnh) ?? [],
    giaTri: "",
  };
}

/** Chuỗi tùy chọn: rỗng sau khi cắt khoảng trắng thì gửi null. */
function hoacNull(chuoi: string): string | null {
  const giaTri = chuoi.trim();
  return giaTri === "" ? null : giaTri;
}

/** Thêm mục đang gán vào cuối nguồn lựa chọn nếu nguồn chưa có (đang tải, lỗi, hoặc thiếu mục). */
function gopMucHienTai<T extends { giaTri: string }>(
  dsLuaChon: ReadonlyArray<T>,
  dsHienTai: ReadonlyArray<T>,
): T[] {
  const daCo = new Set(dsLuaChon.map((lc) => lc.giaTri));
  return [...dsLuaChon, ...dsHienTai.filter((lc) => !daCo.has(lc.giaTri))];
}

const luaChonLoai = (
  l: Pick<LoaiThietBi, "maLoai" | "tenLoai"> & { ngungSuDung?: boolean },
): LuaChon => ({
  giaTri: l.maLoai,
  // <option> gốc không chứa được CMP-13: nhãn "Đã ngưng sử dụng" ghi kèm bằng chữ (DS CMP-06).
  nhan:
    l.ngungSuDung === true
      ? `${l.tenLoai} (${thongTinNhanPhatSinh("DA_NGUNG_SU_DUNG").nhan})`
      : l.tenLoai,
});

const luaChonPhong = (
  p: Pick<PhongThietBi, "maPhong" | "tenPhong"> & { coSo?: string },
): LuaChon => ({
  giaTri: p.maPhong,
  // Mục có hai thông tin (tên phòng, cơ sở): <option> chỉ một dòng nên nối bằng dấu "·".
  nhan: p.coSo ? `${p.tenPhong} · ${p.coSo}` : p.tenPhong,
});

const luaChonNcc = (ncc: NhaCungCap): LuaChonNhieu => ({
  giaTri: ncc.maNCC,
  nhan: ncc.tenNCC,
  ...(ncc.thongTinLienHe?.trim() ? { moTa: ncc.thongTinLienHe.trim() } : {}),
});

/** Gom thông điệp lỗi của trường mảng (dsMaAnh, dsMaNCC) ở mọi mức lồng nhau. */
function thongDiepLoiMang(loi: unknown): string | undefined {
  const dsThongDiep: string[] = [];
  const duyet = (nut: unknown) => {
    if (typeof nut !== "object" || nut === null) return;
    const thongDiep: unknown = Reflect.get(nut, "message");
    if (typeof thongDiep === "string" && thongDiep !== "") {
      dsThongDiep.push(thongDiep);
    }
    for (const [khoa, con] of Object.entries(nut)) {
      if (khoa !== "ref" && khoa !== "message") duyet(con);
    }
  };
  duyet(loi);
  return dsThongDiep.length > 0
    ? [...new Set(dsThongDiep)].join(" ")
    : undefined;
}

export function ThietBiNgan({
  thietBi,
  onDong,
  layBanMoiNhat,
  taiLaiDanhSach,
  onKhongConTonTai,
}: ThietBiNganProps) {
  const laThem = thietBi === null;
  // Bản đang sửa: đổi sang bản mới nhất sau xung đột 409 (giá trị chỉ đọc, ảnh có sẵn, mục đang gán).
  const [banDangSua, setBanDangSua] = useState(thietBi);
  // Dựng lại khu ảnh khi nạp bản mới nhất (AnhUploader chỉ đọc dsAnhCoSan lúc khởi tạo).
  const [phienBanAnh, setPhienBanAnh] = useState(0);
  const [anhCoTheGui, setAnhCoTheGui] = useState(true);
  const form = useForm<GiaTriThietBi>({ defaultValues: giaTriBanDau(thietBi) });
  const { register, control, formState } = form;
  const [thongBao, setThongBao] = useState<ThongBao | null>(null);
  const [loiKhongGan, setLoiKhongGan] = useState<string[]>([]);

  const truyVanLoai = useTatCaLoaiQuery();
  const truyVanPhong = useTatCaPhongQuery();
  const truyVanNcc = useTatCaNhaCungCapQuery();

  // Biểu mẫu: tiêu điểm ban đầu ở trường đầu tiên nhập được (DS CMP-26). Chạy sau showModal của
  // Dialog vì hiệu ứng của component con chạy trước.
  useEffect(() => {
    form.setFocus(laThem ? "maThietBi" : "tenThietBi");
  }, [form, laThem]);

  const dsLuaChonLoai = gopMucHienTai(
    (truyVanLoai.data ?? []).map(luaChonLoai),
    // Chỉ lấy mã và tên loại: ThietBi.ngungSuDung là của thiết bị, không phải của loại.
    banDangSua !== null
      ? [
          luaChonLoai({
            maLoai: banDangSua.maLoai,
            tenLoai: banDangSua.tenLoai,
          }),
        ]
      : [],
  );
  const dsLuaChonPhong = gopMucHienTai(
    (truyVanPhong.data ?? []).map(luaChonPhong),
    banDangSua !== null
      ? [
          luaChonPhong({
            maPhong: banDangSua.maPhong,
            tenPhong: banDangSua.tenPhong,
          }),
        ]
      : [],
  );
  const dsLuaChonNcc = gopMucHienTai(
    (truyVanNcc.data ?? []).map(luaChonNcc),
    (banDangSua?.dsNhaCungCap ?? []).map(luaChonNcc),
  );

  async function xuLyLoi(loi: ApiError) {
    if (laChuaBietKetQua(loi)) return;
    if (loi.maLoi === "DU_LIEU_KHONG_HOP_LE") {
      setLoiKhongGan(ganLoiMayChu(loi, form));
      return;
    }
    if (loi.maLoi === "MA_THIET_BI_TRUNG") {
      form.setError(
        "maThietBi",
        { type: "server", message: THONG_DIEP_DANH_MUC.MA_THIET_BI_TRUNG },
        { shouldFocus: true },
      );
      return;
    }
    if (loi.maLoi === "XUNG_DOT_CAP_NHAT" && banDangSua !== null) {
      const banMoi = await layBanMoiNhat(banDangSua.maThietBi);
      if (banMoi === null) {
        // Không thấy trong trang hiện tại chưa chắc đã bị xóa: giữ dữ liệu đang nhập.
        setThongBao({
          tong: "warning",
          noiDung: THONG_DIEP_DANH_MUC.XUNG_DOT_KHONG_NAP,
        });
        return;
      }
      form.reset(giaTriBanDau(banMoi));
      setBanDangSua(banMoi);
      setPhienBanAnh((v) => v + 1);
      setThongBao({ tong: "warning", noiDung: THONG_DIEP_DANH_MUC.XUNG_DOT });
      return;
    }
    if (loi.maLoi === "KHONG_TIM_THAY") {
      onKhongConTonTai();
      return;
    }
    setThongBao(thongBaoTheoLoi(loi));
  }

  const mutation = useLuuThietBiMutation({
    onSuccess: onDong,
    onError: (loi) => void xuLyLoi(loi),
  });
  const thaoTac = useThaoTacGhi(mutation);

  const luu = () => {
    setThongBao(null);
    setLoiKhongGan([]);
    void form.handleSubmit((gt) => {
      const chung = {
        maLoai: gt.maLoai,
        maPhong: gt.maPhong,
        tenThietBi: gt.tenThietBi.trim(),
        moTa: hoacNull(gt.moTa),
        thongSoKyThuat: hoacNull(gt.thongSoKyThuat),
        ngayNhapKho: gt.ngayNhapKho,
        thoiHanBaoHanh: gt.thoiHanBaoHanh === "" ? null : gt.thoiHanBaoHanh,
        viTriKe: hoacNull(gt.viTriKe),
        viTriTu: hoacNull(gt.viTriTu),
        dsMaNCC: [...gt.dsMaNCC],
        dsMaAnh: [...gt.dsMaAnh],
      };
      if (banDangSua === null) {
        const giaTri = docSoNguyen(gt.giaTri);
        if (giaTri === null) return; // Đã chặn bởi validate; giữ cho chắc kiểu.
        thaoTac.gui({
          ma: null,
          yeuCau: { ...chung, maThietBi: gt.maThietBi.trim(), giaTri },
        });
      } else {
        thaoTac.gui({ ma: banDangSua.maThietBi, yeuCau: chung });
      }
    })();
  };

  // Tải lại thất bại thì giữ trạng thái chưa biết kết quả để người dùng kiểm tra lại sau.
  const kiemTraLai = async () => {
    if (await taiLaiDanhSach()) thaoTac.xacNhanDaKiemTra();
  };

  const loiCua = (
    truong: Exclude<keyof GiaTriThietBi, "dsMaNCC" | "dsMaAnh">,
  ) => {
    const thongDiep = formState.errors[truong]?.message;
    return thongDiep !== undefined ? { loi: thongDiep } : {};
  };
  const loiMang = (truong: "dsMaNCC" | "dsMaAnh") => {
    const thongDiep = thongDiepLoiMang(formState.errors[truong]);
    return thongDiep !== undefined ? { loi: thongDiep } : {};
  };
  const loiAnh = thongDiepLoiMang(formState.errors.dsMaAnh);

  return (
    <Dialog
      open
      kieu="NGAN"
      tieuDe={laThem ? "Nhập kho thiết bị" : "Sửa thiết bị"}
      khoaDong={thaoTac.dangGui}
      onDong={onDong}
      hangNut={
        <>
          <Button onClick={onDong} disabled={thaoTac.dangGui}>
            Hủy
          </Button>
          <Button
            bienThe="primary"
            onClick={luu}
            dangXuLy={thaoTac.dangGui}
            disabled={thaoTac.chuaBietKetQua || !anhCoTheGui}
          >
            Lưu
          </Button>
        </>
      }
    >
      {thaoTac.chuaBietKetQua ? (
        <Banner
          tong="warning"
          hanhDong={{ nhan: "Kiểm tra lại", onClick: () => void kiemTraLai() }}
        >
          {THONG_DIEP_DANH_MUC.CHUA_BIET_KET_QUA}
        </Banner>
      ) : null}
      {thongBao !== null ? (
        <Banner
          tong={thongBao.tong}
          {...(thongBao.traceId !== undefined
            ? { traceId: thongBao.traceId }
            : {})}
        >
          {thongBao.noiDung}
        </Banner>
      ) : null}
      {loiKhongGan.length > 0 ? (
        <Banner tong="danger" dsLyDo={loiKhongGan} />
      ) : null}
      <form
        noValidate
        onSubmit={(suKien) => suKien.preventDefault()}
        className="flex flex-col gap-field"
      >
        <p className="type-caption text-text-secondary">* Bắt buộc</p>

        {laThem ? (
          <FormField
            id="maThietBi"
            nhan="Mã thiết bị"
            batBuoc
            {...loiCua("maThietBi")}
          >
            {(tt) => (
              <TextField
                {...tt}
                {...register("maThietBi", {
                  // ThietBiTaoMoiRequest.maThietBi: required, minLength 1
                  validate: (v) => v.trim() !== "" || "Nhập mã thiết bị",
                })}
                className="type-code"
                autoComplete="off"
              />
            )}
          </FormField>
        ) : (
          <FormField id="maThietBi" nhan="Mã thiết bị">
            {(tt) => (
              <TextField
                {...tt}
                readOnly
                value={banDangSua?.maThietBi ?? ""}
                className="type-code"
              />
            )}
          </FormField>
        )}

        <FormField
          id="tenThietBi"
          nhan="Tên thiết bị"
          batBuoc
          {...loiCua("tenThietBi")}
        >
          {(tt) => (
            <TextField
              {...tt}
              {...register("tenThietBi", {
                // ThietBi*Request.tenThietBi: required, minLength 1
                validate: (v) => v.trim() !== "" || "Nhập tên thiết bị",
              })}
            />
          )}
        </FormField>

        <div className="grid grid-cols-2 gap-field">
          <FormField id="maLoai" nhan="Loại" batBuoc {...loiCua("maLoai")}>
            {(tt) => (
              <Controller
                control={control}
                name="maLoai"
                rules={{ validate: (v) => v !== "" || "Chọn loại thiết bị" }}
                render={({ field }) => (
                  <Select
                    {...tt}
                    dsLuaChon={dsLuaChonLoai}
                    giaTri={field.value === "" ? null : field.value}
                    onChange={(v) => field.onChange(v ?? "")}
                    goiY="Chọn loại"
                    dangTaiLuaChon={
                      truyVanLoai.isPending && dsLuaChonLoai.length === 0
                    }
                    {...(truyVanLoai.isError
                      ? {
                          loiTaiLuaChon: {
                            onThuLai: () => void truyVanLoai.refetch(),
                          },
                        }
                      : {})}
                  />
                )}
              />
            )}
          </FormField>
          <FormField id="maPhong" nhan="Phòng" batBuoc {...loiCua("maPhong")}>
            {(tt) => (
              <Controller
                control={control}
                name="maPhong"
                rules={{ validate: (v) => v !== "" || "Chọn phòng thiết bị" }}
                render={({ field }) => (
                  <Select
                    {...tt}
                    dsLuaChon={dsLuaChonPhong}
                    giaTri={field.value === "" ? null : field.value}
                    onChange={(v) => field.onChange(v ?? "")}
                    goiY="Chọn phòng"
                    dangTaiLuaChon={
                      truyVanPhong.isPending && dsLuaChonPhong.length === 0
                    }
                    {...(truyVanPhong.isError
                      ? {
                          loiTaiLuaChon: {
                            onThuLai: () => void truyVanPhong.refetch(),
                          },
                        }
                      : {})}
                  />
                )}
              />
            )}
          </FormField>
        </div>

        <FormField id="moTa" nhan="Mô tả" {...loiCua("moTa")}>
          {(tt) => <TextArea {...tt} {...register("moTa")} />}
        </FormField>
        <FormField
          id="thongSoKyThuat"
          nhan="Thông số kỹ thuật"
          {...loiCua("thongSoKyThuat")}
        >
          {(tt) => <TextArea {...tt} {...register("thongSoKyThuat")} />}
        </FormField>

        <div className="grid grid-cols-2 gap-field">
          <FormField
            id="ngayNhapKho"
            nhan="Ngày nhập kho"
            batBuoc
            {...loiCua("ngayNhapKho")}
          >
            {(tt) => (
              <TextField
                {...tt}
                type="date"
                {...register("ngayNhapKho", {
                  // ThietBi*Request.ngayNhapKho: required, format date
                  validate: (v) => v !== "" || "Chọn ngày nhập kho",
                })}
              />
            )}
          </FormField>
          <FormField
            id="thoiHanBaoHanh"
            nhan="Thời hạn bảo hành"
            {...loiCua("thoiHanBaoHanh")}
          >
            {(tt) => (
              <TextField {...tt} type="date" {...register("thoiHanBaoHanh")} />
            )}
          </FormField>
        </div>

        <div className="grid grid-cols-2 gap-field">
          <FormField id="viTriKe" nhan="Kệ" {...loiCua("viTriKe")}>
            {(tt) => <TextField {...tt} {...register("viTriKe")} />}
          </FormField>
          <FormField id="viTriTu" nhan="Tủ" {...loiCua("viTriTu")}>
            {(tt) => <TextField {...tt} {...register("viTriTu")} />}
          </FormField>
        </div>

        <FormField id="dsMaNCC" nhan="Nhà cung cấp" {...loiMang("dsMaNCC")}>
          {(tt) => (
            <Controller
              control={control}
              name="dsMaNCC"
              render={({ field }) => (
                <SelectNhieu
                  {...tt}
                  dsLuaChon={dsLuaChonNcc}
                  giaTri={field.value}
                  onChange={field.onChange}
                  tomTat={(n) => `${n} nhà cung cấp đã chọn`}
                  goiY="Chọn nhà cung cấp"
                  dangTaiLuaChon={
                    truyVanNcc.isPending && dsLuaChonNcc.length === 0
                  }
                  {...(truyVanNcc.isError
                    ? {
                        loiTaiLuaChon: {
                          onThuLai: () => void truyVanNcc.refetch(),
                        },
                      }
                    : {})}
                />
              )}
            />
          )}
        </FormField>

        <div className="flex flex-col gap-field-label">
          <AnhUploader
            key={phienBanAnh}
            giaiDoan="THIET_BI"
            tenThietBi={form.getValues("tenThietBi") || "thiết bị"}
            dsAnhCoSan={banDangSua?.dsAnh ?? []}
            onThayDoi={({ dsMaAnh, coTheGui }) => {
              form.setValue("dsMaAnh", dsMaAnh);
              form.clearErrors("dsMaAnh");
              setAnhCoTheGui(coTheGui);
            }}
          />
          {loiAnh !== undefined ? (
            <p role="alert" className="type-caption text-tone-danger-text">
              {loiAnh}
            </p>
          ) : null}
        </div>

        {laThem ? (
          <FormField id="giaTri" nhan="Giá trị" batBuoc {...loiCua("giaTri")}>
            {(tt) => (
              <TextField
                {...tt}
                {...register("giaTri", {
                  // ThietBiTaoMoiRequest.giaTri: required, integer int64, minimum 0
                  validate: (v) =>
                    docSoNguyen(v) !== null || "Nhập số nguyên không âm",
                })}
                inputMode="numeric"
                hauTo="VNĐ"
              />
            )}
          </FormField>
        ) : (
          <FormField
            id="giaTri"
            nhan="Giá trị hiện hành"
            moTa="Đổi giá trị bằng Cập nhật giá trị; giá trị hiện hành dùng để tính bồi thường."
          >
            {(tt) => (
              <TextField
                {...tt}
                readOnly
                value={formatTien(banDangSua?.giaTriHienHanh)}
              />
            )}
          </FormField>
        )}
      </form>
    </Dialog>
  );
}
