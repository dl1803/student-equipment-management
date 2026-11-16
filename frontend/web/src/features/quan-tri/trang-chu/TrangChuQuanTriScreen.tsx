"use client";

// SCR-QT-01 Trang chủ quản trị viên (UI-Screens 7.1). Sở hữu: Thịnh.
// QT không bị giới hạn theo phòng và không có chức năng quầy (ĐX-PQ-02).
import { Card } from "@/shared/components/Card";
import { useAuth } from "@/shared/hooks/useAuth";
import { loiVaoCua } from "@/shared/tuyen/menuTheoVaiTro";

export function TrangChuQuanTriScreen() {
  const { trangThai } = useAuth();
  if (trangThai.loai !== "DA_DANG_NHAP") return null;

  return (
    <main className="flex flex-col gap-section">
      <div className="flex flex-col gap-1">
        <h1 className="type-title-screen text-text-primary">Trang chủ</h1>
        <p className="type-body-strong text-text-primary">
          {trangThai.nguoiDung.hoTen}
        </p>
      </div>
      {loiVaoCua("QUAN_TRI").map((nhom, chiSo) => (
        <section
          key={nhom.tieuDe ?? `khac-${chiSo}`}
          className="flex flex-col gap-3"
        >
          {nhom.tieuDe !== undefined ? (
            <h2 className="type-title-section text-text-primary">
              {nhom.tieuDe}
            </h2>
          ) : null}
          <div className="grid grid-cols-3 gap-4">
            {nhom.dsMuc.map((muc) => (
              <Card
                key={muc.href}
                bienThe="TUONG_TAC"
                tieuDe={muc.nhan}
                href={muc.href}
              />
            ))}
          </div>
        </section>
      ))}
    </main>
  );
}
