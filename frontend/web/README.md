# Web — Nhân viên và Quản trị viên

Next.js (App Router) + TypeScript + Tailwind CSS. Kiến trúc và quy ước: [Frontend-Architecture.md](../docs/02-architecture/Frontend-Architecture.md), [Frontend-Conventions.md](../docs/02-architecture/Frontend-Conventions.md). Luật cứng của dự án: [CLAUDE.md](../../CLAUDE.md).

## Chạy cục bộ

Yêu cầu Node.js 20.9 trở lên.

```bash
npm ci
cp .env.example .env.local   # sửa NEXT_PUBLIC_API_BASE_URL nếu cần
npm run dev
```

## Lệnh

| Lệnh                   | Việc                                                                                |
| ---------------------- | ----------------------------------------------------------------------------------- |
| `npm run gen:api`      | Sinh `src/shared/api/schema.d.ts` từ `openapi/openapi.yaml`. Không sửa tay tệp sinh |
| `npm run lint`         | ESLint, gồm các điều cấm ở FC 5.11 và biên giới import ở FC 4.1                     |
| `npm run format`       | Prettier định dạng lại toàn bộ (cấu hình mặc định, ĐX-FC-08)                        |
| `npm run format:check` | Prettier kiểm tra, không sửa                                                        |
| `npm run typecheck`    | `next typegen` rồi `tsc --noEmit`                                                   |
| `npm run test`         | Vitest; tệp kiểm thử component khai báo `// @vitest-environment jsdom`              |
| `npm run build`        | Build production                                                                    |

Thứ tự kiểm chứng trước khi tạo PR (FC 10.3): `gen:api` (không được làm lệch `schema.d.ts` đã commit) → `lint` và `format:check` → `typecheck` → `test` → `build`.

Dự án buộc ký tự xuống dòng LF qua `.gitattributes`, kể cả máy Windows bật `core.autocrlf`.

## Cấu trúc

- `openapi/openapi.yaml`: bản hợp đồng API, trùng byte với `backend/docs/04-api/openapi.yaml`.
- `src/app/`: chỉ ghép tuyến; mỗi `page.tsx` import đúng một `Screen` từ `features/`.
- `src/features/`: `xac-thuc`, `quay` (NV), `quan-tri` (QT).
- `src/shared/`: phần dùng chung (`api`, `components`, `format`, `hooks`, `labels`, `tuyen`).
- `src/app/tokens.css`: design token của Design-System (nơi duy nhất có giá trị thô); `src/app/globals.css` ánh xạ token vào theme Tailwind (thay thế bảng mặc định) và lớp kiểu chữ `type-*`.
- `src/shared/components/contracts.ts`: hợp đồng props của các thành phần chưa hiện thực (Select, Checkbox, Radio, Card, KeyValueList, Banner, DataTable, Pagination, Menu, Sidebar, Header).
- `src/shared/tuyen/banDoTuyen.ts`: bản đồ 28 tuyến giai đoạn 1 kèm chủ sở hữu; kiểm thử đi kèm chặn tuyến thiếu hoặc thừa.
