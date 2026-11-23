// Tóm tắt phiếu trong hộp xác nhận Duyệt, Từ chối (UI-Screens 6.3, ĐX-UF-05: nêu SV, phòng, các loại
// và số lượng). Sở hữu: Thịnh. Dữ liệu lấy từ dòng đã chọn ở SCR-NV-02.
import type { PhieuYeuCauMuonQuay } from "@/shared/api/types";
import { KeyValueList } from "@/shared/components/KeyValueList";

export function TomTatPhieu({ phieu }: { phieu: PhieuYeuCauMuonQuay }) {
  return (
    <KeyValueList
      dsDong={[
        { ten: "Mã phiếu", giaTri: phieu.maYeuCau, laMa: true },
        {
          ten: "Sinh viên",
          giaTri: `${phieu.sinhVien.hoTen} (${phieu.sinhVien.maSV})`,
        },
        { ten: "Phòng", giaTri: phieu.tenPhong },
        {
          ten: "Loại và số lượng",
          giaTri: (
            <ul className="flex flex-col">
              {phieu.dsLoaiVaSoLuong.map((l) => (
                <li key={l.maLoai}>
                  {l.tenLoai} × {l.soLuong}
                </li>
              ))}
            </ul>
          ),
        },
      ]}
    />
  );
}
