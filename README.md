# Hệ thống Quản lý Thiết bị Sinh viên

> **Đồ án:** Phát triển phần mềm hướng đối tượng (SE100) — Nhóm 13  
> **Mục tiêu:** Hệ thống quản lý toàn diện quy trình mượn, trả, kiểm tra, bảo trì và xử lý vi phạm thiết bị học tập cho sinh viên đại học.

---

## Cấu trúc Thư mục Dự án

```text
student-equipment-management/
├── backend/                           # Phân hệ Backend (ASP.NET Core .NET 8)
│   └── docs/                          # 📚 Tài liệu kỹ thuật Backend
│       ├── README.md                  # Hướng dẫn điều hướng & Bản đồ tài liệu Backend
│       ├── 01-requirements/           # Yêu cầu nghiệp vụ & Phân tích hệ thống
│       │   ├── SRS.md                 # Đặc tả yêu cầu phần mềm (SRS)
│       │   ├── Usecase.md             # Mô hình 19 Use cases chi tiết
│       │   └── BusinessRule.md        # 81 Quy tắc nghiệp vụ (BR-01..81)
│       ├── 02-architecture/           # Kiến trúc hệ thống & Quy chuẩn
│       │   ├── backend-architecture.md# Kiến trúc Modular Monolith & Phân tầng
│       │   ├── Backend-Conventions.md # Quy ước lập trình C# & Git/CI-CD
│       │   └── Authorization.md       # Phân quyền & Bảo mật (JWT, RBAC)
│       ├── 03-database/               # Thiết kế Cơ sở dữ liệu (PostgreSQL)
│       │   ├── DatabaseDesign.md      # Thiết kế CSDL vật lý (35 bảng)
│       │   └── schema.sql             # Script DDL khởi tạo toàn bộ CSDL
│       └── 04-api/                    # Đặc tả Hợp đồng API
│           ├── API-Notes.md           # Hướng dẫn API, mã lỗi & máy trạng thái
│           └── openapi.yaml           # Hợp đồng chuẩn OpenAPI 3.0.3 (88 endpoints)
│
├── frontend/                          # Phân hệ Frontend (Web & Mobile)
│   └── docs/                          # 📚 Tài liệu thiết kế & Kỹ thuật Frontend
│       ├── README.md                  # Hướng dẫn điều hướng & Bản đồ tài liệu Frontend
│       ├── 01-design/                 # Thiết kế UI/UX & Giao diện
│       │   ├── User-Flows.md          # Luồng thao tác người dùng (User Flows)
│       │   ├── Wireframes.md          # Khung giao diện phác thảo (Wireframes)
│       │   ├── UI-Screens.md          # Đặc tả chi tiết từng màn hình Web & Mobile
│       │   └── Design-System.md       # Hệ thống Design Tokens, Typography, Colors
│       └── 02-architecture/           # Kiến trúc Kỹ thuật & Quy chuẩn
│           ├── Frontend-Architecture.md# Kiến trúc Next.js 14 & Android Native Java
│           └── Frontend-Conventions.md # Quy ước lập trình, Naming & State Management
│
└── README.md                          # Tài liệu giới thiệu tổng quan đồ án
```

---

## Hướng dẫn Truy cập Tài liệu

Hệ thống tài liệu dự án được xây dựng đầy đủ và phân loại trực quan theo từng phân hệ:

- **[Tài liệu Backend](backend/docs/README.md)**: Bao gồm toàn bộ đặc tả yêu cầu (SRS, Usecase, Business Rule), kiến trúc Modular Monolith, thiết kế CSDL (schema.sql), và đặc tả API (OpenAPI 3.0).
- **[Tài liệu Frontend](frontend/docs/README.md)**: Bao gồm hệ thống thiết kế (Design System), luồng người dùng (User Flows), bản vẽ giao diện (Wireframes, UI Screens), và kiến trúc 2 nền tảng (Next.js cho Quản trị/Nhân viên và Android Java cho Sinh viên).
