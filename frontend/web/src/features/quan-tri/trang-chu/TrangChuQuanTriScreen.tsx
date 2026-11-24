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
      {loiVaoCua("QUAN_TRI").map((nhom, chiSo) => {
        const luoi = (
          <div className="grid grid-cols-3 gap-6">
            {nhom.dsMuc.map((muc) => (
              <Card
                key={muc.href}
                bienThe="TUONG_TAC"
                tieuDe={muc.nhan}
                href={muc.href}
              />
            ))}
          </div>
        );
        // Nhóm có tiêu đề là một khu vực có tên; nhóm không tiêu đề chỉ là khối bố cục.
        if (nhom.tieuDe === undefined) {
          return (
            <div key={`khac-${chiSo}`} className="flex flex-col gap-3">
              {luoi}
            </div>
          );
        }
        const idTieuDe = `nhom-loi-vao-${chiSo}`;
        return (
          <section
            key={nhom.tieuDe}
            aria-labelledby={idTieuDe}
            className="flex flex-col gap-3"
          >
            <h2 id={idTieuDe} className="type-title-section text-text-primary">
              {nhom.tieuDe}
            </h2>
            {luoi}
          </section>
        );
      })}
    </main>
  );
}
