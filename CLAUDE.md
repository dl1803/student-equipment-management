# CLAUDE.md — Luật dự án

Áp dụng cho mọi người và mọi agent khi sửa mã trong repo này. Khi luật ở đây khác tài liệu chi tiết, tài liệu đã duyệt trong `frontend/docs/` và `backend/docs/` là nguồn gốc; sửa tài liệu trước rồi mới sửa luật này.

## Web (`frontend/web/`) — Nhân viên và Quản trị viên

Nguồn: `frontend/docs/02-architecture/Frontend-Architecture.md` (FA), `Frontend-Conventions.md` (FC), `frontend/docs/01-design/UI-Screens.md`, `Design-System.md`.

### Công nghệ

- Next.js App Router + TypeScript (`strict`) + Tailwind CSS. Chỉ thêm các thư viện đã chốt ở FA 2.3 (React Hook Form, TanStack Query, Axios; công cụ: openapi-typescript, Vitest, React Testing Library, ESLint). Thư viện khác phải được nhóm duyệt và ghi vào FA trước.
- Mọi trang có dữ liệu hoặc trạng thái người dùng là Client Component. Không dùng Route Handler (`route.ts`), Server Action (`"use server"`), `middleware`/`proxy`, proxy API hay `next/image` (ĐX-FA-01, ĐX-FA-11).
- Phiên bản thư viện khóa bằng `package-lock.json`; cài đặt bằng `npm ci`.

### Hợp đồng API

- `frontend/web/openapi/openapi.yaml` là hợp đồng, trùng byte với `backend/docs/04-api/openapi.yaml`.
- `src/shared/api/schema.d.ts` do `npm run gen:api` sinh. **Không sửa tay.** Khi API đổi: chép `openapi.yaml` mới, chạy `gen:api`, sửa mã cho tới khi xanh, commit chung một PR.
- Kiểu dùng trong mã lấy qua bí danh ở `src/shared/api/types.ts`; không tự định nghĩa lại hình dạng yêu cầu/phản hồi. Hàm gọi API đặt tên trùng `operationId`.

### Phiên và lưu trữ

- `accessToken` chỉ nằm trong bộ nhớ (biến của module `src/shared/api/session/`).
- `refreshToken` chỉ nằm trong `sessionStorage`, chỉ module `src/shared/api/session/` được đọc/ghi.
- **Cấm `localStorage` và cookie** cho token hay bất kỳ dữ liệu nào. Không đưa token vào URL, log hay thông điệp lỗi.

### Dữ liệu từ máy chủ

- `QueryClient` mặc định: `retry: 0`, `staleTime: 0`, `refetchOnWindowFocus: false`; mutation `retry: 0`, `networkMode: "always"`. Không tự gửi lại thao tác ghi, không cập nhật lạc quan, không `resumePausedMutations`.
- Khóa truy vấn dựng từ `TIEN_TO_KHOA` ở `src/shared/api/tienToKhoa.ts`.
- Mọi lỗi ra khỏi `shared/api/` là `ApiError` (`src/shared/api/apiError.ts`); phân nhánh theo `maLoi`/`status`/`loai`, không theo `detail`.

### Nghiệp vụ

- **Client không tự tính** phí, số ngày trễ, hạn trả, hạn giữ chỗ, trạng thái hay điều kiện mượn. Chỉ định dạng giá trị máy chủ trả về. Kiểm tra phía client chỉ gồm bắt buộc nhập, định dạng và giới hạn khai báo trong `openapi.yaml`.

### Giao diện

- **Chỉ dùng CSS token** của Design-System (`src/app/tokens.css`, tiền tố `--ds-`, được thêm ở task Design-System); không dùng màu/cỡ/khoảng cách mặc định của Tailwind, không giá trị thô `[#…]`, `[13px]`, không `style` với giá trị thô.
- **Không cài thư viện UI** (MUI, shadcn/ui, Radix, Chakra, Ant Design…). Thành phần dùng chung tự viết trong `src/shared/components/` theo danh mục CMP của Design-System.
- Không có bố cục điện thoại (web từ 1024 px). Không `dangerouslySetInnerHTML`.

### Cấu trúc và tuyến

- `src/app/` chỉ ghép tuyến: mỗi `page.tsx` import đúng một `Screen` từ `src/features/`.
- `features/` chỉ import `shared/`; không import chéo giữa các tính năng. `shared/` không import `features/` hay `app/`. ESLint cưỡng chế các luật này.
- Bản đồ tuyến duy nhất: `src/shared/tuyen/banDoTuyen.ts` (28 màn hình giai đoạn 1). **Không thêm tuyến hay chức năng ngoài giai đoạn 1** (không UC-04, UC-18…). Kiểm thử `banDoTuyen.test.ts` thất bại khi có `page.tsx` ngoài bản đồ.
- Đầu mỗi tệp `Screen` ghi mã màn hình (`// SCR-NV-04 …`) để truy vết.

### Phân công sở hữu

| Người | Phạm vi |
|---|---|
| Thịnh | Shared core (`src/shared/**`, `src/app/layout.tsx`, các layout, `features/xac-thuc`, cấu hình dự án), SCR-CM-02, SCR-CM-03, SCR-NV-01..14, SCR-QT-01..04, SCR-QT-10..12 |
| Long | SCR-QT-05..09 (`features/quan-tri/cau-hinh/`, `features/quan-tri/tai-khoan-nhan-vien/`) từ ngày làm việc 20 |

Sửa phần thuộc người khác phải thống nhất với người đó trước. Thay đổi shared core do Thịnh review.

### Kiểm chứng trước khi tạo PR

Trong `frontend/web/`: `npm run gen:api` (không làm lệch `schema.d.ts`) → `npm run lint` → `npm run typecheck` → `npm run test` → `npm run build`. Tất cả phải xanh.

## Git

- Commit theo Conventional Commits, mô tả tiếng Việt: `{loai}({pham-vi}): {mô tả}`, ví dụ `feat(web): …`, `chore(android): …`.
- Không commit `.env*` thật (chỉ `.env.example`), `local.properties`, keystore, token, `node_modules/`, `.next/`, `build/`, `.idea/`.
