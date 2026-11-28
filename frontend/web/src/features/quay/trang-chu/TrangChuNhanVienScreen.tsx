"use client";

// SCR-NV-01 Trang chủ nhân viên (UI-Screens 6.1). Sở hữu: Thịnh.
// Bố cục (Wireframes SCR-NV-01, ĐX-WF-21): ngày và lời chào theo giờ; ba thẻ số liệu (tongSo do máy
// chủ đếm, "—" khi đang tải hoặc lỗi); phòng được cấp quyền dạng thẻ bo tròn; lối vào chức năng có
// biểu tượng giống menu bên.
import { useEffect, useState } from "react";
import { Banner } from "@/shared/components/Banner";
import { IconAssignmentLate } from "@/shared/components/icons/IconAssignmentLate";
import { IconHourglassEmpty } from "@/shared/components/icons/IconHourglassEmpty";
import { IconMeetingRoom } from "@/shared/components/icons/IconMeetingRoom";
import { IconPendingActions } from "@/shared/components/icons/IconPendingActions";
import { TheLoiVao, TheSoLieu } from "@/shared/components/TheLoiVao";
import { useAuth } from "@/shared/hooks/useAuth";
import { BAN_DO_TUYEN } from "@/shared/tuyen/banDoTuyen";
import { loiVaoCua } from "@/shared/tuyen/menuTheoVaiTro";
import { loiChao, useSoLieuTrangChu } from "./soLieu";

const DINH_DANG_NGAY = new Intl.DateTimeFormat("vi-VN", {
  weekday: "long",
  day: "2-digit",
  month: "2-digit",
  year: "numeric",
  timeZone: "Asia/Ho_Chi_Minh",
});

/** Lời chào và ngày theo đồng hồ máy người dùng; chỉ tính sau khi gắn để không lệch với HTML dựng sẵn. */
function useLoiChaoHomNay() {
  const [giaTri, setGiaTri] = useState<{ chao: string; ngay: string } | null>(
    null,
  );
  useEffect(() => {
    const bayGio = new Date();
    const gio = Number(
      new Intl.DateTimeFormat("en-GB", {
        hour: "numeric",
        // h23 thay vì hour12: false: có engine định dạng nửa đêm thành "24" với hour12: false.
        hourCycle: "h23",
        timeZone: "Asia/Ho_Chi_Minh",
      }).format(bayGio),
    );
    // eslint-disable-next-line react-hooks/set-state-in-effect -- đọc đồng hồ máy một lần sau khi gắn.
    setGiaTri({ chao: loiChao(gio), ngay: DINH_DANG_NGAY.format(bayGio) });
  }, []);
  return giaTri;
}

export function TrangChuNhanVienScreen() {
  const { trangThai } = useAuth();
  const soLieu = useSoLieuTrangChu();
  const homNay = useLoiChaoHomNay();
  if (trangThai.loai !== "DA_DANG_NHAP") return null;
  const { hoTen, dsPhong } = trangThai.nguoiDung;

  return (
    <main className="flex flex-col gap-8">
      <div className="flex flex-col gap-1">
        <p className="type-label text-text-secondary first-letter:uppercase">
          {homNay?.ngay ?? " "}
        </p>
        <h1 className="type-title-screen text-text-primary">
          {homNay?.chao ?? "Xin chào"}, {hoTen}
        </h1>
      </div>

      <section aria-label="Số liệu hôm nay" className="grid grid-cols-3 gap-6">
        <TheSoLieu
          nhan="Phiếu chờ duyệt"
          giaTri={soLieu.phieuChoDuyet}
          href={BAN_DO_TUYEN["SCR-NV-02"].tuyen}
          bieuTuong={IconPendingActions}
          nhanManh
        />
        <TheSoLieu
          nhan="Phiếu chưa trả quá 30 ngày"
          giaTri={soLieu.quaHan30Ngay}
          href={BAN_DO_TUYEN["SCR-NV-12"].tuyen}
          bieuTuong={IconAssignmentLate}
        />
        <TheSoLieu
          nhan="Đăng ký trong danh sách chờ"
          giaTri={soLieu.danhSachCho}
          href={BAN_DO_TUYEN["SCR-NV-13"].tuyen}
          bieuTuong={IconHourglassEmpty}
        />
      </section>

      <section aria-labelledby="tieu-de-phong" className="flex flex-col gap-3">
        <h2 id="tieu-de-phong" className="type-title-section text-text-primary">
          Phòng được cấp quyền
        </h2>
        {dsPhong.length === 0 ? (
          <Banner tong="warning">
            Bạn chưa được cấp quyền phòng thiết bị nào; liên hệ quản trị viên
          </Banner>
        ) : (
          <ul className="flex flex-wrap gap-3">
            {dsPhong.map((phong) => (
              <li
                key={phong.maPhong}
                className="flex items-center gap-3 rounded-full border border-border-subtle bg-surface-default py-2 pr-4 pl-2"
              >
                <span className="grid size-control-sm place-items-center rounded-full bg-surface-selected text-text-link">
                  <IconMeetingRoom kichThuoc="md" />
                </span>
                <span className="flex flex-col">
                  <span className="type-label text-text-primary">
                    {phong.tenPhong}
                  </span>
                  <span className="type-caption text-text-secondary">
                    {phong.coSo}
                  </span>
                </span>
              </li>
            ))}
          </ul>
        )}
      </section>

      <section
        aria-labelledby="tieu-de-chuc-nang"
        className="flex flex-col gap-3"
      >
        <h2
          id="tieu-de-chuc-nang"
          className="type-title-section text-text-primary"
        >
          Chức năng
        </h2>
        <div className="grid grid-cols-3 gap-4">
          {loiVaoCua("NHAN_VIEN")
            .flatMap((nhom) => nhom.dsMuc)
            .map((muc) => (
              <TheLoiVao
                key={muc.href}
                nhan={muc.nhan}
                href={muc.href}
                {...(muc.bieuTuong !== undefined
                  ? { bieuTuong: muc.bieuTuong }
                  : {})}
              />
            ))}
        </div>
      </section>
    </main>
  );
}
