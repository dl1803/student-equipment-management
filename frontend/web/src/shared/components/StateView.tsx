// StateView (CMP-25) — trạng thái đang tải, rỗng, lỗi của một vùng dữ liệu (DS 8.5, FC 3.4, 5.5).
// Sở hữu: Thịnh (shared core). D01-T: chốt hợp đồng props và chữ chuẩn (DS 9.8);
// biểu tượng, kiểu dáng token và thông điệp "Hệ thống đang khởi động" sau 8 giây thuộc task sau.

export type HanhDongStateView = {
  nhan: string;
  onClick: () => void;
};

export type StateViewProps =
  | { loai: "DANG_TAI" }
  | {
      loai: "RONG";
      /** Thông điệp riêng của từng danh sách, do màn hình truyền vào. */
      thongDiep: string;
      /** Rỗng do bộ lọc thì dùng biểu tượng search_off và thường kèm "Xóa bộ lọc". */
      doBoLoc?: boolean;
      hanhDong?: HanhDongStateView;
    }
  | { loai: "LOI_MANG"; onThuLai: () => void }
  | { loai: "LOI_HE_THONG"; traceId?: string; onThuLai: () => void };

export function StateView(props: StateViewProps) {
  switch (props.loai) {
    case "DANG_TAI":
      return (
        <div aria-busy="true" role="status">
          Đang tải
        </div>
      );
    case "RONG":
      return (
        <div role="status">
          <p>{props.thongDiep}</p>
          {props.hanhDong ? (
            <button type="button" onClick={props.hanhDong.onClick}>
              {props.hanhDong.nhan}
            </button>
          ) : null}
        </div>
      );
    case "LOI_MANG":
      return (
        <div role="alert">
          <p>Không tải được dữ liệu. Kiểm tra kết nối rồi thử lại.</p>
          <button type="button" onClick={props.onThuLai}>
            Thử lại
          </button>
        </div>
      );
    case "LOI_HE_THONG":
      return (
        <div role="alert">
          <p>Đã xảy ra lỗi hệ thống.</p>
          {props.traceId ? (
            <p>
              Mã theo dõi: <code>{props.traceId}</code>
            </p>
          ) : null}
          <button type="button" onClick={props.onThuLai}>
            Thử lại
          </button>
        </div>
      );
  }
}
