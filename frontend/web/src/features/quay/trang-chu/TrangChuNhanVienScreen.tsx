"use client";

// SCR-NV-01 Trang chủ nhân viên (UI-Screens 6.1). Sở hữu: Thịnh.
// Dữ liệu lấy từ GET /auth/toi đã gọi khi đăng nhập/khôi phục (useAuth), không gọi thêm API.
import { Banner } from "@/shared/components/Banner";
import { Card } from "@/shared/components/Card";
import { useAuth } from "@/shared/hooks/useAuth";
import { loiVaoCua } from "@/shared/tuyen/menuTheoVaiTro";

export function TrangChuNhanVienScreen() {
  const { trangThai } = useAuth();
  if (trangThai.loai !== "DA_DANG_NHAP") return null;
  const { hoTen, dsPhong } = trangThai.nguoiDung;

  return (
    <main className="flex flex-col gap-section">
      <div className="flex flex-col gap-1">
        <h1 className="type-title-screen text-text-primary">Trang chủ</h1>
        <p className="type-body-strong text-text-primary">{hoTen}</p>
      </div>

      <section aria-labelledby="tieu-de-phong" className="flex flex-col gap-3">
        <h2 id="tieu-de-phong" className="type-title-section text-text-primary">
          Phòng được cấp quyền
        </h2>
        {dsPhong.length === 0 ? (
          <Banner tong="warning">
            Bạn chưa được cấp quyền phòng thiết bị nào; liên hệ quản trị viên
          </Banner>
        ) : (
          <ul className="flex flex-col gap-2">
            {dsPhong.map((phong) => (
              <li key={phong.maPhong} className="flex flex-col">
                <span className="type-body text-text-primary">
                  {phong.tenPhong}
                </span>
                <span className="type-caption text-text-secondary">
                  {phong.coSo}
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
              <Card
                key={muc.href}
                bienThe="TUONG_TAC"
                tieuDe={muc.nhan}
                href={muc.href}
              />
            ))}
        </div>
      </section>
    </main>
  );
}
