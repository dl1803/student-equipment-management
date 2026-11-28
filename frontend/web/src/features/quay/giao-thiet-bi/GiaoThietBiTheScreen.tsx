"use client";

// SCR-NV-04 Giao thiết bị, bước 1: Quét thẻ và hồ sơ sinh viên (UI-Screens 6.4; UF-NV-02 bước 1, 2; E1).
// Sở hữu: Thịnh. D13-T.
// - Bố cục (Wireframes SCR-NV-04, P-06..P-08): tiến trình, ô nhập mã thẻ (CMP-11), hồ sơ SV, biên bản
//   chưa nộp (ẩn khi không có), kết luận và lý do, hàng nút.
// - GET /quay/sinh-vien/the/{maThe}: hiển thị nguyên hồ sơ, hopLe và dsLyDoKhongHopLe[].thongDiep do
//   máy chủ trả; client KHÔNG tự tính điều kiện mượn hay số thiết bị (NT-01).
// - Tiếp tục chỉ bật khi hopLe = true. Quét thẻ khác xóa hồ sơ, đưa tiêu điểm về ô nhập.
// - KHONG_TIM_THAY: báo không tìm thấy sinh viên, giữ mã để sửa. Lỗi khác: khối thông báo theo nhóm lỗi.
// - Hồ sơ nằm trong ngữ cảnh LuongGiaoThietBi để quay lại từ bước 2 vẫn thấy; vào bước 1 không qua
//   thanh tiến trình (menu, trang chủ) thì đặt lại luồng (FA 4.7).
import { useRouter } from "next/navigation";
import { useEffect, useRef, useState } from "react";
import { laApiError } from "@/shared/api/apiError";
import { nhomCuaLoi } from "@/shared/api/nhomLoi";
import type { SinhVienTheInfo } from "@/shared/api/types";
import { Banner } from "@/shared/components/Banner";
import { Button } from "@/shared/components/Button";
import { Card } from "@/shared/components/Card";
import { KeyValueList } from "@/shared/components/KeyValueList";
import {
  ScanInput,
  type ScanInputHandle,
  type TrangThaiScanInput,
} from "@/shared/components/ScanInput";
import { StateView } from "@/shared/components/StateView";
import { StatusBadge } from "@/shared/components/StatusBadge";
import { TextLink } from "@/shared/components/TextLink";
import { formatTien } from "@/shared/format/formatSo";
import { formatNgay, formatNgayGio } from "@/shared/format/formatThoiGian";
import {
  THONG_DIEP_LOI_CHUNG,
  thongBaoTheoLoi,
  type ThongBao,
} from "@/shared/labels/thongDiepLoi";
import { BAN_DO_TUYEN } from "@/shared/tuyen/banDoTuyen";
import { KhungBuocGiao, TUYEN_BUOC } from "./components/KhungBuocGiao";
import { useTraCuuSinhVien } from "./hooks";
import { useLuongGiaoThietBi } from "./LuongGiaoThietBi";
import { THONG_DIEP_GIAO_THIET_BI } from "./thongDiep";

export function GiaoThietBiTheScreen() {
  const router = useRouter();
  const { sinhVien, ghiNhanSinhVien, datLai, laQuayLai } =
    useLuongGiaoThietBi();
  const traCuu = useTraCuuSinhVien();
  const oNhap = useRef<ScanInputHandle>(null);
  const lanMoiNhat = useRef(0);
  const [trangThaiO, setTrangThaiO] = useState<TrangThaiScanInput>("CHO_NHAP");
  const [loiO, setLoiO] = useState<string | undefined>(undefined);
  const [thongBao, setThongBao] = useState<ThongBao | null>(null);

  // Vào bước 1 không qua thanh tiến trình = bắt đầu luồng mới.
  useEffect(() => {
    if (!laQuayLai()) datLai();
  }, [laQuayLai, datLai]);

  const xacNhan = (maThe: string) => {
    const lan = ++lanMoiNhat.current;
    setTrangThaiO("DANG_TRA_CUU");
    setLoiO(undefined);
    setThongBao(null);
    datLai();
    traCuu(maThe).then(
      (sv) => {
        if (lan !== lanMoiNhat.current) return;
        ghiNhanSinhVien(sv);
        setTrangThaiO("CHO_NHAP");
      },
      (loi: unknown) => {
        if (lan !== lanMoiNhat.current) return;
        setTrangThaiO("LOI");
        if (!laApiError(loi)) {
          setThongBao({
            tong: "danger",
            noiDung: THONG_DIEP_LOI_CHUNG.HE_THONG,
          });
          return;
        }
        const nhom = nhomCuaLoi(loi);
        if (nhom === "KHONG_TON_TAI") {
          setLoiO(THONG_DIEP_GIAO_THIET_BI.KHONG_TIM_THAY_SV);
        } else if (nhom === "DU_LIEU_NHAP") {
          setLoiO(THONG_DIEP_GIAO_THIET_BI.MA_THE_KHONG_HOP_LE);
        } else {
          setThongBao(thongBaoTheoLoi(loi));
        }
      },
    );
  };

  const quetTheKhac = () => {
    lanMoiNhat.current += 1;
    datLai();
    setThongBao(null);
    setLoiO(undefined);
    setTrangThaiO("CHO_NHAP");
    oNhap.current?.xoaVaTapTrung();
  };

  const dangTraCuu = trangThaiO === "DANG_TRA_CUU";

  return (
    <KhungBuocGiao buoc={1}>
      <div className="max-w-web-form-max">
        <ScanInput
          ref={oNhap}
          id="maThe"
          nhan="Mã thẻ sinh viên"
          trangThai={trangThaiO}
          {...(loiO !== undefined ? { loi: loiO } : {})}
          onXacNhan={xacNhan}
        />
      </div>

      <div
        aria-live="polite"
        aria-busy={dangTraCuu || undefined}
        className="flex flex-col gap-section"
      >
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
        {dangTraCuu ? <StateView loai="DANG_TAI" /> : null}
        {!dangTraCuu && sinhVien !== null ? (
          <HoSoSinhVien sinhVien={sinhVien} />
        ) : null}
      </div>

      {sinhVien !== null && !dangTraCuu ? (
        <div className="flex justify-end gap-actions">
          <Button onClick={quetTheKhac}>Quét thẻ khác</Button>
          <Button
            bienThe="primary"
            disabled={sinhVien.hopLe !== true}
            onClick={() => router.push(TUYEN_BUOC[1])}
          >
            Tiếp tục
          </Button>
        </div>
      ) : null}
    </KhungBuocGiao>
  );
}

function HoSoSinhVien({ sinhVien: sv }: { sinhVien: SinhVienTheInfo }) {
  const dsBienBan = sv.dsBienBanChuaNop ?? [];
  const hopLe = sv.hopLe === true;
  return (
    <>
      <Card bienThe="TINH" tieuDe="Hồ sơ sinh viên">
        <KeyValueList
          dsDong={[
            { ten: "Mã số SV", giaTri: sv.maSV, laMa: true },
            { ten: "Họ tên", giaTri: sv.hoTen },
            { ten: "Lớp", giaTri: sv.lop },
            { ten: "Khoa", giaTri: sv.khoa },
            { ten: "Nhóm sinh viên", giaTri: sv.tenNhom },
            { ten: "Hạn thẻ", giaTri: formatNgay(sv.hanThe) },
            {
              ten: "Trạng thái học",
              giaTri:
                sv.trangThaiHoc !== undefined ? (
                  <StatusBadge ho="TRANG_THAI_HOC" ma={sv.trangThaiHoc} />
                ) : null,
            },
            {
              ten: "Quyền mượn",
              giaTri:
                sv.trangThaiQuyenMuon !== undefined ? (
                  <StatusBadge ho="QUYEN_MUON" ma={sv.trangThaiQuyenMuon} />
                ) : null,
            },
            {
              ten: "Đang mượn",
              giaTri:
                sv.soThietBiDangMuon !== undefined &&
                sv.soThietBiToiDa !== undefined
                  ? `${sv.soThietBiDangMuon} / ${sv.soThietBiToiDa} thiết bị`
                  : null,
            },
          ]}
        />
      </Card>

      {dsBienBan.length > 0 ? (
        <Card bienThe="TINH" tieuDe="Biên bản chưa nộp">
          <ul className="flex flex-col gap-2">
            {dsBienBan.map((bb) => (
              <li
                key={bb.maBienBan}
                className="flex flex-wrap items-center gap-x-4 gap-y-1 type-body text-text-primary"
              >
                <span className="type-code">{bb.maBienBan}</span>
                <span>{formatTien(bb.tongTien)}</span>
                <span className="text-text-secondary">
                  Lập {formatNgayGio(bb.ngayLap)}
                </span>
              </li>
            ))}
          </ul>
          <TextLink
            href={{
              pathname: BAN_DO_TUYEN["SCR-NV-11"].tuyen,
              query: { maSV: sv.maSV },
            }}
          >
            Mở ở Biên bản và thu phạt
          </TextLink>
        </Card>
      ) : null}

      <Banner
        tong={hopLe ? "success" : "danger"}
        ketLuan={hopLe ? "HOP_LE" : "KHONG_HOP_LE"}
        dsLyDo={(sv.dsLyDoKhongHopLe ?? []).map((lyDo) => lyDo.thongDiep)}
      />
    </>
  );
}
