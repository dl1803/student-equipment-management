# Bàn giao shared core web (D04-T, cập nhật D05-T, D06-T, D07-T)

Người nhận: Long, phụ trách SCR-QT-05..09 (`features/quan-tri/cau-hinh/`, `features/quan-tri/tai-khoan-nhan-vien/`) từ ngày làm việc 20.
Từ cuối D04-T, các interface dưới đây là **public và ổn định**. Muốn đổi chữ ký, Thịnh báo trước và cập nhật tài liệu này trong cùng PR. Cần thêm gì thì tạo issue hoặc nhắn Thịnh, không sửa thẳng `src/shared/**`.

Luật cứng của dự án nằm ở [CLAUDE.md](../../CLAUDE.md). Mọi mục ở đây đều có căn cứ trong FA/FC.

## 1. Viết một màn hình

Mỗi tính năng có `api.ts` (hàm gọi API và khóa truy vấn), `hooks.ts` (truy vấn và thao tác ghi), `components/` và `{Ten}Screen.tsx` (FC 4.1). `page.tsx` đã có sẵn, chỉ import đúng một `Screen`, không cần sửa.

```ts
// features/quan-tri/cau-hinh/api.ts
import { apiClient } from "@/shared/api/client";
import { TIEN_TO_KHOA } from "@/shared/api/tienToKhoa";
import type { components } from "@/shared/api/types";

type ThamSoHeThong = components["schemas"]["ThamSoHeThong"];

export const khoaThamSo = {
  hienTai: () => [...TIEN_TO_KHOA.quanTriThamSo] as const,
};

// Tên hàm trùng operationId; đường dẫn KHÔNG có /api/v1 (baseURL đã có).
export async function qtLayThamSo(tuyChon?: { signal?: AbortSignal }) {
  const res = await apiClient.get<ThamSoHeThong>("/quan-tri/cau-hinh/tham-so", {
    signal: tuyChon?.signal,
  });
  return res.data;
}
```

```ts
// features/quan-tri/cau-hinh/hooks.ts
"use client";
import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import type { ApiError } from "@/shared/api/apiError";
import { TIEN_TO_KHOA } from "@/shared/api/tienToKhoa";

export function useThamSoQuery() {
  return useQuery({
    queryKey: khoaThamSo.hienTai(),
    queryFn: ({ signal }) => qtLayThamSo({ signal }),
  });
}

export function useLuuThamSoMutation() {
  const queryClient = useQueryClient();
  return useMutation<ThamSoHeThong, ApiError, ThamSoHeThong>({
    mutationFn: qtCapNhatThamSo, // hàm PUT viết tương tự qtLayThamSo
    onSuccess: () =>
      void queryClient.invalidateQueries({
        queryKey: TIEN_TO_KHOA.quanTriThamSo,
      }),
  });
}
```

Ở `Screen`:

- **Vùng dữ liệu:** đang tải dùng `<StateView loai="DANG_TAI" />`, lỗi dùng `LOI_MANG`/`LOI_HE_THONG` kèm `onThuLai={refetch}`, rỗng dùng `RONG` (FC 5.2).
- **Thao tác ghi:** dùng `useThaoTacGhi(mutation)`. Vô hiệu nút theo `biKhoa`. Khi `chuaBietKetQua` thì hiện "Chưa xác định được kết quả", tải lại dữ liệu rồi gọi `xacNhanDaKiemTra()` (FA 3.11).
- **Lỗi theo trường:** `ganLoiMayChu(loi, form)` trả về các thông điệp không gắn được vào trường nào; hiện chúng bằng `<Banner tong="danger" dsLyDo={...} />`.
- **Hộp xác nhận** cho thao tác ở DS 9.2: `<ConfirmDialog ... dangXuLy={dangGui} />`.

## 2. Interface đã ổn định

| Module                       | Xuất                                                                                              | Ghi chú                                                                                                                           |
| ---------------------------- | ------------------------------------------------------------------------------------------------- | --------------------------------------------------------------------------------------------------------------------------------- |
| `shared/api/client`          | `apiClient`                                                                                       | Axios chung; tự gắn token, tự làm mới khi 401 (một lần), mọi lỗi là `ApiError`                                                    |
| `shared/api/apiError`        | `ApiError`, `laApiError`, `laChuaBietKetQua`, `NhomLoi`                                           | FA 3.3                                                                                                                            |
| `shared/api/nhomLoi`         | `nhomCuaLoi(loi)`                                                                                 | Bảng FA 3.4 cho 32 `maLoi`; mã lạ thuộc `NGHIEP_VU_THEO_MAN_HINH`                                                                 |
| `shared/api/tienToKhoa`      | `TIEN_TO_KHOA`                                                                                    | Tiền tố khóa truy vấn; QT-05..09 có `quanTriQuyDinhMuon`, `quanTriPhiTre`, `quanTriKhungPhat`, `quanTriThamSo`, `quanTriNhanVien` |
| `shared/api/types`           | Bí danh kiểu từ `schema.d.ts`                                                                     | Thêm bí danh mới ở đây, không định nghĩa lại hình dạng                                                                            |
| `shared/hooks/useAuth`       | `useAuth()` → `GiaTriAuth`                                                                        | `trangThai`, `dangXuat`, `taiLaiThongTinToi`, …                                                                                   |
| `shared/hooks/useThaoTacGhi` | `useThaoTacGhi(mutation)`                                                                         | Khóa gửi đồng bộ + nhánh chưa biết kết quả                                                                                        |
| `shared/hooks/ganLoiMayChu`  | `ganLoiMayChu(loi, form)`, `chuyenKhoaLoiTruong`                                                  | `ds[0].x` → `ds.0.x`                                                                                                              |
| `shared/format/*`            | `formatNgayGio`, `formatNgay`, `formatGio`, `thanhChuoiGioVn`, `formatTien`, `formatTyLePhanTram` | FC 7.2                                                                                                                            |
| `shared/labels/trangThai`    | `thongTinTrangThai`, `nhanTrangThai`, `thongTinNhanPhatSinh`                                      | DS 7.2, 7.3                                                                                                                       |
| `shared/labels/thongDiepLoi` | `THONG_DIEP_LOI_CHUNG`, `thongDiepTheoNhom`, `thongBaoTheoLoi(loi)` → `ThongBao`                  | Thông điệp riêng của màn hình đặt ở `features/.../thongDiep.ts`; `ThongBao` = `{ tong, noiDung, traceId? }` cho `Banner`          |
| `shared/api/phanTrang`       | `KICH_THUOC_TRANG_MAC_DINH` (= 20), `soTrang`, `trangCanLuiVe`                                    | Tổng số giảm (xóa dòng cuối của trang cuối) thì lùi về trang cuối; xem `LoaiThietBiScreen`                                        |
| `shared/format/docSo`        | `docSoNguyen(chuoi)` → `number \| null`                                                           | Đọc ô số nguyên không âm (chỉ chữ số); kiểm tra giới hạn ở biểu mẫu                                                               |
| `shared/tuyen/banDoTuyen`    | `BAN_DO_TUYEN`, `tuyenCua`                                                                        | Không thêm tuyến ngoài 28 màn hình                                                                                                |

**Thành phần đã hiện thực** (`shared/components/`): `Button` (CMP-01), `IconButton` (02), `TextLink` (03), `TextField` + `FormField` (04), `Select` (06, chọn một), `AnhUploader` (12), `StatusBadge` (13), `KeyValueList` (14), `Card` (15), `DataTable` (18), `Pagination` (19), `Menu` (20), `AnhGallery` + `ImageViewer` (23), `Banner` (24), `StateView` (25), `Dialog` + `ConfirmDialog` (26), `Sidebar` (29), `Header` (30). Ví dụ dùng đầy đủ: `features/quan-tri/danh-muc/` (SCR-QT-02, QT-03).

- `Select` là `<select>` gốc: `{ id, dsLuaChon: LuaChon<T>[], giaTri: T | null, onChange(T | null), coMucTatCa?, goiY?, ... }`. Không có mục Tất cả và chưa chọn thì hiện mục gợi ý `goiY` (mặc định "Chọn"). **Khác hợp đồng ngày 4:** bỏ prop `cheDo`; chọn nhiều tách sang `SelectNhieuProps`.
- `DataTable`: `{ tieuDe, dsCot: CotBang<T>[], dsDong, khoaDong, trangThaiThan?, hanhDongDong?, dangTaiLai? }`. Cứ truyền đủ hành động (`HanhDongDong`: `nhan`, `onClick`, `huyBo?`, `an?`): tối đa 2 mục hiện thì là nút văn bản trực tiếp, từ 3 mục hiện thì **tự gom vào `Menu`** (`SO_HANH_DONG_TRUC_TIEP_TOI_DA`); mục `an` không tính. Screen không tự dựng Menu. Trạng thái tải/rỗng/lỗi truyền qua `trangThaiThan`.
- `Menu`: `{ nhanNutKichHoat?, dsMuc: HanhDongDong[] }`. Nút "Thao tác ▾"; mục `huyBo` tông danger, đặt cuối, cách bằng đường kẻ; bàn phím ↑/↓/Home/End/Enter/Esc/Tab theo DS CMP-20. Danh sách dùng `position: fixed` nên không bị khung cuộn của bảng cắt; bên dưới không đủ chỗ (dòng cuối khung nhìn) thì tự lật mở lên; cuộn trang thì menu đóng. Kiểu `HanhDongDong` định nghĩa ở `Menu`, `DataTable` xuất lại.
- `Button`: thêm `bieuTuongSau` (biểu tượng bên phải chữ).
- `Pagination`: `{ trang, kichThuoc, tongSo, onDoiTrang, dangTai? }`; tự ẩn khi chỉ có một trang và đang ở trang 1.
- `IconButton`: thêm `kichThuoc?: "sm" | "md"` (mặc định md 40px; sm 32px cho góc ô ảnh) và `ref`.
- `AnhUploader` (SCR-NV-06 `BAN_DAU`, SCR-NV-09 `KHI_TRA`, SCR-QT-04 `THIET_BI`): `{ giaiDoan, tenThietBi, dsAnhCoSan?, onThayDoi({ dsMaAnh, coTheGui }), lyDoVoHieu? }`. Tự kiểm định dạng và giới hạn 5 ảnh, nén (`shared/anh/nenAnh`), tải lên tuần tự `POST /anh` và giữ `maAnh`. Screen chỉ đưa `dsMaAnh` vào yêu cầu và **vô hiệu nút gửi khi `coTheGui === false`** (còn ảnh đang tải hoặc lỗi chưa gỡ). Không tự thử lại; người dùng bấm "Thử lại" ở từng ảnh. URL tạm được thu hồi khi gỡ ảnh hoặc khi component bị gỡ (gửi xong thì đóng/rời màn hình). SCR-NV-09 khi kết quả Mất: truyền `lyDoVoHieu="Kết quả Mất không có ảnh"`.
- `AnhGallery`: `{ dsAnh: AnhRef[], tenThietBi, onTaiLai? }`. Ảnh tình trạng (URL ký) truyền `onTaiLai={() => truyVanChiTiet.refetch()}`: ảnh lỗi thì tải lại chi tiết đúng một lần cho cả dải, vẫn lỗi thì hiện "Không tải được ảnh" + "Tải lại". Ảnh danh mục (URL công khai) bỏ `onTaiLai`. Không lưu URL ảnh vào storage, tuyến hay khóa truy vấn. Chọn ảnh mở `ImageViewer` (`{ url, nhanTruyCap, onDong }`).

**Props đã công bố, chưa hiện thực** (`shared/components/contracts.ts`): `SelectNhieu`, `Checkbox`, `RadioGroup` và phần mở rộng `DataTableMoRongProps` (chọn dòng, mở rộng dòng, làm nổi bật). Có thể viết `Screen` theo các props này ngay; thành phần thật sẽ khớp chữ ký.

## 3. Kiểm thử màn hình bằng fixture

`shared/testing/nguCanhKiemThu.tsx` dựng màn hình với `QueryClient` mặc định của dự án và `AuthContext` giả, không cần đăng nhập thật:

```tsx
// @vitest-environment jsdom
import { screen } from "@testing-library/react";
import { vi } from "vitest";
import {
  hienThiVoiNguCanh,
  QUAN_TRI_MAU,
} from "@/shared/testing/nguCanhKiemThu";

vi.mock("./api", () => ({
  qtLayThamSo: vi.fn().mockResolvedValue(THAM_SO_MAU),
}));

it("SCR-QT-08: hiện bộ tham số hiện tại", async () => {
  hienThiVoiNguCanh(<ThamSoScreen />, { nguoiDung: QUAN_TRI_MAU });
  expect(
    await screen.findByLabelText("Số lần đăng nhập sai tối đa"),
  ).toBeTruthy();
});
```

Ví dụ đang chạy được: [TrangChuNhanVienScreen.test.tsx](src/features/quay/trang-chu/TrangChuNhanVienScreen.test.tsx). Để giả lập HTTP thật thì thay `apiClient.defaults.adapter` như trong [client.test.ts](src/shared/api/client.test.ts) (FC 9.1 không cho thêm thư viện giả lập HTTP). `<dialog>` đã có polyfill trong `vitest.setup.ts`.

## 4. Những thứ không được làm (CLAUDE.md)

- Không gọi `axios` hay `fetch` ngoài `shared/api/`. Không đụng token, `sessionStorage`, `localStorage`.
- Không tự tính phí, hạn, trạng thái. Không cập nhật lạc quan. Không retry thao tác ghi.
- Không dùng màu hay cỡ mặc định của Tailwind, không giá trị thô `[#…]`/`[…px]`. Chỉ dùng lớp token (xem `src/app/globals.css`).
- Không import chéo giữa các tính năng. Cần dùng chung thì đề nghị đưa vào `shared/`.
