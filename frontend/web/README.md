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

| Lệnh | Việc |
|---|---|
| `npm run gen:api` | Sinh `src/shared/api/schema.d.ts` từ `openapi/openapi.yaml`. Không sửa tay tệp sinh |
| `npm run lint` | ESLint, gồm các điều cấm ở FC 5.11 và biên giới import ở FC 4.1 |
| `npm run typecheck` | `next typegen` rồi `tsc --noEmit` |
| `npm run test` | Vitest |
| `npm run build` | Build production |

Thứ tự kiểm chứng trước khi tạo PR: `gen:api` (không được làm lệch `schema.d.ts` đã commit) → `lint` → `typecheck` → `test` → `build`.

## Cấu trúc

- `openapi/openapi.yaml`: bản hợp đồng API, trùng byte với `backend/docs/04-api/openapi.yaml`.
- `src/app/`: chỉ ghép tuyến; mỗi `page.tsx` import đúng một `Screen` từ `features/`.
- `src/features/`: `xac-thuc`, `quay` (NV), `quan-tri` (QT).
- `src/shared/`: phần dùng chung (`api`, `components`, `hooks`, `tuyen`).
- `src/shared/tuyen/banDoTuyen.ts`: bản đồ 28 tuyến giai đoạn 1 kèm chủ sở hữu; kiểm thử đi kèm chặn tuyến thiếu hoặc thừa.
