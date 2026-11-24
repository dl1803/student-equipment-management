"use client";

// SCR-NV-01 Trang chủ nhân viên (UI-Screens 6.1). Sở hữu: Thịnh.
// Dữ liệu lấy từ GET /auth/toi đã gọi khi đăng nhập/khôi phục (useAuth), không gọi thêm API.
// Bố cục (Wireframes SCR-NV-01, ĐX-DS-17): phòng được cấp quyền là hàng thẻ tĩnh (tên phòng, cơ sở);
// lối vào chức năng là lưới thẻ tương tác. Không có thẻ số liệu: API không trả số tổng (NT-01).
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
          <ul className="grid grid-cols-3 gap-6">
            {dsPhong.map((phong) => (
              <li key={phong.maPhong}>
                <Card bienThe="TINH" tieuDe={phong.tenPhong}>
                  <span className="type-body text-text-secondary">
                    {phong.coSo}
                  </span>
                </Card>
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
        <div className="grid grid-cols-3 gap-6">
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
