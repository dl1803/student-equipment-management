// Nhãn trạng thái nghiệp vụ (DS 7, FC 7.3). Sở hữu: Thịnh (shared core).
// Một nơi duy nhất ánh xạ (họ, mã API) → nhãn tiếng Việt, tông, mức nhấn. Màn hình không tự đặt
// nhãn hay màu; chỉ dùng StatusBadge. Mã lạ trả nguyên mã, tông neutral, không lỗi (DS 7.1).
import type {
  KetQuaBaoTri,
  KetQuaKiemTra,
  LoaiThongBao,
  LoaiThucHienBaoTri,
  LoaiViPham,
  NguonGiao,
  TinhTrangThietBi,
  TrangThaiBienBan,
  TrangThaiDangKyCho,
  TrangThaiHoc,
  TrangThaiKhoa,
  TrangThaiPhieuMuon,
  TrangThaiPhieuYeuCauMuon,
  TrangThaiQuyenMuon,
} from "@/shared/api/types";

export type Tong = "info" | "success" | "warning" | "danger" | "neutral";

export type ThongTinTrangThai = {
  nhan: string;
  tong: Tong;
  /** Mức nhấn đậm (DS 7.1): nền solid, chữ on-solid. */
  nhanDam: boolean;
  /** Mã không có trong bảng: hiển thị nguyên mã bằng type.code (DS 7.1). */
  laMaChuaBiet: boolean;
};

type Dong = { nhan: string; tong: Tong; nhanDam?: true };

// `satisfies Record<EnumAPI, Dong>`: thiếu mã của openapi.yaml thì tsc báo lỗi khi sinh lại kiểu.
const BANG = {
  TINH_TRANG_THIET_BI: {
    SAN_SANG: { nhan: "Sẵn sàng cho mượn", tong: "success" },
    DANG_DUOC_MUON: { nhan: "Đang được mượn", tong: "info" },
    DANG_BAO_TRI_SUA_CHUA: { nhan: "Đang bảo trì/sửa chữa", tong: "warning" },
    DA_THANH_LY: { nhan: "Đã thanh lý", tong: "neutral" },
    DA_BAO_MAT: { nhan: "Đã báo mất", tong: "danger" },
  } satisfies Record<TinhTrangThietBi, Dong>,
  PHIEU_YEU_CAU: {
    CHO_DUYET: { nhan: "Chờ duyệt", tong: "warning" },
    DA_DUYET: { nhan: "Đã duyệt", tong: "success" },
    TU_CHOI: { nhan: "Từ chối", tong: "danger" },
    DA_HUY: { nhan: "Đã hủy", tong: "neutral" },
    HET_HAN: { nhan: "Hết hạn", tong: "neutral" },
    DA_NHAN: { nhan: "Đã nhận", tong: "success" },
  } satisfies Record<TrangThaiPhieuYeuCauMuon, Dong>,
  PHIEU_MUON: {
    DANG_MUON: { nhan: "Đang mượn", tong: "info" },
    QUA_HAN: { nhan: "Quá hạn", tong: "danger", nhanDam: true },
    DA_TRA: { nhan: "Đã trả", tong: "success" },
  } satisfies Record<TrangThaiPhieuMuon, Dong>,
  DANG_KY_CHO: {
    DANG_CHO: { nhan: "Đang chờ", tong: "info" },
    DA_THONG_BAO: { nhan: "Đã thông báo", tong: "success" },
    DA_NHAN: { nhan: "Đã nhận", tong: "success" },
    DA_HUY: { nhan: "Đã hủy", tong: "neutral" },
    HET_HAN: { nhan: "Hết hạn", tong: "neutral" },
  } satisfies Record<TrangThaiDangKyCho, Dong>,
  KET_QUA_KIEM_TRA: {
    NGUYEN_VEN: { nhan: "Nguyên vẹn", tong: "success" },
    HU_HONG: { nhan: "Hư hỏng", tong: "danger" },
    THIEU_PHU_KIEN: { nhan: "Thiếu phụ kiện", tong: "warning" },
    MAT: { nhan: "Mất", tong: "danger", nhanDam: true },
  } satisfies Record<KetQuaKiemTra, Dong>,
  BIEN_BAN: {
    CHUA_NOP: { nhan: "Chưa nộp", tong: "warning" },
    DA_NOP: { nhan: "Đã nộp", tong: "success" },
    DA_HUY: { nhan: "Đã hủy", tong: "neutral" },
  } satisfies Record<TrangThaiBienBan, Dong>,
  LOAI_VI_PHAM: {
    TRA_TRE: { nhan: "Trả trễ", tong: "warning" },
    HU_HONG: { nhan: "Hư hỏng", tong: "danger" },
    MAT: { nhan: "Mất", tong: "danger" },
    KHONG_DEN_NHAN: { nhan: "Không đến nhận", tong: "warning" },
  } satisfies Record<LoaiViPham, Dong>,
  TRANG_THAI_HOC: {
    DANG_HOC: { nhan: "Đang học", tong: "success" },
    TOT_NGHIEP: { nhan: "Tốt nghiệp", tong: "neutral" },
    THOI_HOC: { nhan: "Thôi học", tong: "danger" },
    DINH_CHI: { nhan: "Đình chỉ", tong: "danger" },
  } satisfies Record<TrangThaiHoc, Dong>,
  QUYEN_MUON: {
    BINH_THUONG: { nhan: "Bình thường", tong: "success" },
    TAM_KHOA: { nhan: "Tạm khóa", tong: "warning", nhanDam: true },
    CAM_MUON_DAI_HAN: {
      nhan: "Cấm mượn dài hạn",
      tong: "danger",
      nhanDam: true,
    },
  } satisfies Record<TrangThaiQuyenMuon, Dong>,
  TAI_KHOAN: {
    HOAT_DONG: { nhan: "Hoạt động", tong: "success" },
    TAM_KHOA_DANG_NHAP: { nhan: "Tạm khóa đăng nhập", tong: "warning" },
    BI_KHOA: { nhan: "Bị khóa", tong: "danger" },
  } satisfies Record<TrangThaiKhoa, Dong>,
  LOAI_BAO_TRI: {
    BAO_TRI_DINH_KY: { nhan: "Bảo trì định kỳ", tong: "neutral" },
    SUA_CHUA: { nhan: "Sửa chữa", tong: "neutral" },
  } satisfies Record<LoaiThucHienBaoTri, Dong>,
  KET_QUA_BAO_TRI: {
    DUA_VE_SAN_SANG: { nhan: "Đưa về Sẵn sàng", tong: "success" },
    DE_XUAT_THANH_LY: { nhan: "Đề xuất thanh lý", tong: "warning" },
  } satisfies Record<KetQuaBaoTri, Dong>,
  NGUON_GIAO: {
    PHIEU_YEU_CAU: { nhan: "Phiếu yêu cầu", tong: "neutral" },
    DANG_KY_CHO: { nhan: "Danh sách chờ", tong: "neutral" },
    TRUC_TIEP: { nhan: "Mượn trực tiếp", tong: "neutral" },
  } satisfies Record<NguonGiao, Dong>,
  LOAI_THONG_BAO: {
    NHAC_HAN_TRA: { nhan: "Nhắc hạn trả", tong: "warning" },
    QUA_HAN: { nhan: "Quá hạn", tong: "danger" },
    THIET_BI_CHO_SAN_SANG: { nhan: "Thiết bị chờ sẵn sàng", tong: "success" },
    DUYET_HOAC_TU_CHOI: { nhan: "Duyệt hoặc từ chối", tong: "info" },
    VI_PHAM_PHI_PHAT: { nhan: "Vi phạm, phí phạt", tong: "danger" },
    NHAC_NHO_NV: { nhan: "Nhắc nhở của NV", tong: "info" },
    NHAC_BAO_TRI: { nhan: "Nhắc bảo trì", tong: "neutral" },
  } satisfies Record<LoaiThongBao, Dong>,
};

/** Họ trạng thái (FC 3.5); một mã có thể trùng giữa các họ nên luôn tra theo họ (DS 7.1). */
export type HoTrangThai = keyof typeof BANG;

/** Nhãn phát sinh từ dữ liệu không phải mã trạng thái (DS 7.3, FC 7.3). */
const BANG_PHAT_SINH = {
  HET_HANG: { nhan: "Hết hàng", tong: "danger" },
  KHONG_DU: { nhan: "Không đủ", tong: "danger" },
  HOP_LE: { nhan: "Hợp lệ", tong: "success", nhanDam: true },
  KHONG_HOP_LE: { nhan: "Không hợp lệ", tong: "danger", nhanDam: true },
  DUOC_DUYET: { nhan: "Được duyệt", tong: "success", nhanDam: true },
  KHONG_DUOC_DUYET: { nhan: "Không được duyệt", tong: "danger", nhanDam: true },
  DA_NGUNG_SU_DUNG: { nhan: "Đã ngưng sử dụng", tong: "neutral" },
  DANG_SU_DUNG: { nhan: "Đang sử dụng", tong: "success" },
  DA_TAT: { nhan: "Đã tắt", tong: "neutral" },
  CHUA_TRA: { nhan: "Chưa trả", tong: "neutral" },
  BAO_TRI: { nhan: "Bảo trì", tong: "neutral" },
  THANH_LY: { nhan: "Thanh lý", tong: "neutral" },
  TAM_TINH: { nhan: "Tạm tính", tong: "warning" },
} satisfies Record<string, Dong>;

export type NhanPhatSinh = keyof typeof BANG_PHAT_SINH;

function tuDong(dong: Dong): ThongTinTrangThai {
  return {
    nhan: dong.nhan,
    tong: dong.tong,
    nhanDam: dong.nhanDam === true,
    laMaChuaBiet: false,
  };
}

function traDong(bang: Record<string, Dong>, ma: string): Dong | undefined {
  return Object.prototype.hasOwnProperty.call(bang, ma) ? bang[ma] : undefined;
}

/**
 * Tra nhãn theo (họ, mã). `ma` là string, không phải union, để mã lạ từ máy chủ không làm vỡ
 * client (FC 5.1). Không bao giờ ném lỗi.
 */
export function thongTinTrangThai(
  ho: HoTrangThai,
  ma: string,
): ThongTinTrangThai {
  const bang: Record<string, Dong> = BANG[ho];
  const dong = traDong(bang, ma);
  if (dong === undefined) {
    return { nhan: ma, tong: "neutral", nhanDam: false, laMaChuaBiet: true };
  }
  return tuDong(dong);
}

export function nhanTrangThai(ho: HoTrangThai, ma: string): string {
  return thongTinTrangThai(ho, ma).nhan;
}

export function thongTinNhanPhatSinh(khoa: NhanPhatSinh): ThongTinTrangThai {
  return tuDong(BANG_PHAT_SINH[khoa]);
}
