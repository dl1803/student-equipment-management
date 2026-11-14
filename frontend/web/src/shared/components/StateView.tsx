"use client";

// StateView — CMP-25 Khối trạng thái (DS 8.5, 9.3; FC 3.4, 5.5). Sở hữu: Thịnh (shared core).
// Trạng thái đang tải, tải chậm (sau 8 giây), rỗng, lỗi mạng, lỗi hệ thống của MỘT vùng dữ liệu.
// Rỗng không phải lỗi: tông trung tính, không dùng màu lỗi (DS 9.3).
import type { JSX, ReactNode } from "react";
import { Button } from "./Button";
import { ChiBaoQuay } from "./ChiBaoQuay";
import { IconCloudOff } from "./icons/IconCloudOff";
import { IconError } from "./icons/IconError";
import { IconInbox } from "./icons/IconInbox";
import { IconSearchOff } from "./icons/IconSearchOff";
import {
  THONG_DIEP_KHOI_DONG,
  useThongBaoKhoiDong,
} from "@/shared/hooks/useThongBaoKhoiDong";

export type HanhDongStateView = {
  nhan: string;
  onClick: () => void;
};

export type StateViewProps =
  | { loai: "DANG_TAI" }
  | {
      loai: "RONG";
      /** Thông điệp riêng của từng danh sách, do màn hình truyền vào (DS 9.3). */
      thongDiep: string;
      /** Rỗng do bộ lọc: biểu tượng search_off, thường kèm hành động "Xóa bộ lọc". */
      doBoLoc?: boolean;
      hanhDong?: HanhDongStateView;
    }
  | { loai: "LOI_MANG"; onThuLai: () => void }
  | { loai: "LOI_HE_THONG"; traceId?: string; onThuLai: () => void };

function Khung({
  children,
  ...aria
}: {
  children: ReactNode;
  role?: "status" | "alert";
  "aria-busy"?: boolean;
}) {
  return (
    <div
      {...aria}
      className="mx-auto flex w-full max-w-state-max flex-col items-center gap-3 py-state text-center"
    >
      {children}
    </div>
  );
}

function DangTai(): JSX.Element {
  const taiCham = useThongBaoKhoiDong(true);
  return (
    <Khung role="status" aria-busy>
      <ChiBaoQuay kichThuoc="lg" />
      <p className="type-body text-text-secondary">
        {taiCham ? THONG_DIEP_KHOI_DONG : "Đang tải"}
      </p>
    </Khung>
  );
}

export function StateView(props: StateViewProps): JSX.Element {
  switch (props.loai) {
    case "DANG_TAI":
      return <DangTai />;
    case "RONG": {
      const Icon = props.doBoLoc ? IconSearchOff : IconInbox;
      return (
        <Khung role="status">
          <Icon kichThuoc="lg" className="text-tone-neutral-icon" />
          <p className="type-body text-text-secondary">{props.thongDiep}</p>
          {props.hanhDong ? (
            <Button bienThe="tertiary" onClick={props.hanhDong.onClick}>
              {props.hanhDong.nhan}
            </Button>
          ) : null}
        </Khung>
      );
    }
    case "LOI_MANG":
      return (
        <Khung role="alert">
          <IconCloudOff kichThuoc="lg" className="text-text-secondary" />
          <p className="type-body text-text-primary">
            Không tải được dữ liệu. Kiểm tra kết nối rồi thử lại.
          </p>
          <Button bienThe="secondary" onClick={props.onThuLai}>
            Thử lại
          </Button>
        </Khung>
      );
    case "LOI_HE_THONG":
      return (
        <Khung role="alert">
          <IconError kichThuoc="lg" className="text-tone-danger-icon" />
          <p className="type-body text-text-primary">Đã xảy ra lỗi hệ thống.</p>
          {props.traceId ? (
            <p className="type-body text-text-secondary">
              Mã theo dõi:{" "}
              <code className="type-code select-all text-text-primary">
                {props.traceId}
              </code>
            </p>
          ) : null}
          <Button bienThe="secondary" onClick={props.onThuLai}>
            Thử lại
          </Button>
        </Khung>
      );
    default: {
      // Thêm loại mới vào StateViewProps mà chưa xử lý ở trên thì tsc báo lỗi tại đây.
      const chuaXuLy: never = props;
      return chuaXuLy;
    }
  }
}
