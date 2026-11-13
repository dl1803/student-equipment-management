import { defineConfig, globalIgnores } from "eslint/config";
import nextVitals from "eslint-config-next/core-web-vitals";
import nextTs from "eslint-config-next/typescript";

// Điều cấm theo Frontend-Conventions 5.11 và biên giới import theo mục 4.1.
const CAM_IMPORT_CHUNG = [
  { name: "next/image", message: "Dùng thẻ <img> thường (ĐX-FA-11)." },
  { name: "clsx", message: "Dùng classNames ở shared/components (FC 5.7)." },
  {
    name: "classnames",
    message: "Dùng classNames ở shared/components (FC 5.7).",
  },
];
const CAM_AXIOS = {
  name: "axios",
  message: "Chỉ shared/api/ được dùng axios (FC 4.1).",
};
const CAM_THU_VIEN_UI = {
  group: [
    "@mui/*",
    "@chakra-ui/*",
    "antd",
    "@radix-ui/*",
    "@headlessui/*",
    "@mantine/*",
    "react-bootstrap",
  ],
  message:
    "Không dùng thư viện thành phần dựng sẵn; dùng shared/components theo Design-System (FA 2.5).",
};
const CAM_IMPORT_APP = {
  group: ["@/app/*"],
  message: "Không import từ app/ (FC 4.1).",
};
const CAM_IMPORT_FEATURES = {
  group: ["@/features/*"],
  message:
    "shared/ không import features/; features không import chéo tính năng khác (FC 4.1).",
};

function camImport({ choAxios = false, patterns = [] } = {}) {
  return [
    "error",
    {
      paths: choAxios ? CAM_IMPORT_CHUNG : [...CAM_IMPORT_CHUNG, CAM_AXIOS],
      patterns: [CAM_THU_VIEN_UI, ...patterns],
    },
  ];
}

// Gốc tính năng: features/xac-thuc, features/quay/<tính năng>, features/quan-tri/<tính năng>.
// Import tương đối chỉ trong cùng thư mục tính năng (FC 4.1): tệp nằm sâu d cấp dưới gốc tính năng
// không được leo quá d cấp "../". Luật ghi đè theo tệp nên mỗi độ sâu là một khối cấu hình.
const DO_SAU_TOI_DA = 4;
function camImportTuongDoiRaNgoaiTinhNang() {
  return Array.from({ length: DO_SAU_TOI_DA }, (_, doSau) => {
    const thuMucCon = "*/".repeat(doSau);
    return {
      files: [
        `src/features/xac-thuc/${thuMucCon}*.{ts,tsx}`,
        `src/features/{quay,quan-tri}/*/${thuMucCon}*.{ts,tsx}`,
      ],
      rules: {
        "no-restricted-imports": camImport({
          patterns: [
            CAM_IMPORT_APP,
            CAM_IMPORT_FEATURES,
            {
              regex: `^(\\.\\./){${doSau + 1},}`,
              message:
                "Import tương đối không được ra khỏi thư mục tính năng (FC 4.1).",
            },
          ],
        }),
      },
    };
  });
}

const CAM_LOCAL_STORAGE = {
  name: "localStorage",
  message: "Cấm localStorage (FA 4.13, FC 5.11).",
};
const CAM_SESSION_STORAGE = {
  name: "sessionStorage",
  message: "Chỉ shared/api/session/ được dùng sessionStorage (FC 5.10).",
};
const CAM_THUOC_TINH_STORAGE = [
  {
    object: "window",
    property: "localStorage",
    message: CAM_LOCAL_STORAGE.message,
  },
  {
    object: "globalThis",
    property: "localStorage",
    message: CAM_LOCAL_STORAGE.message,
  },
];

const eslintConfig = defineConfig([
  ...nextVitals,
  ...nextTs,
  globalIgnores([
    ".next/**",
    "out/**",
    "build/**",
    "next-env.d.ts",
    "src/shared/api/schema.d.ts",
  ]),
  {
    files: ["src/**/*.{ts,tsx}"],
    rules: {
      "no-console": ["error", { allow: ["warn", "error"] }],
      "no-restricted-globals": [
        "error",
        CAM_LOCAL_STORAGE,
        CAM_SESSION_STORAGE,
      ],
      "no-restricted-properties": [
        "error",
        ...CAM_THUOC_TINH_STORAGE,
        {
          object: "window",
          property: "sessionStorage",
          message: CAM_SESSION_STORAGE.message,
        },
        {
          object: "globalThis",
          property: "sessionStorage",
          message: CAM_SESSION_STORAGE.message,
        },
      ],
      "no-restricted-imports": camImport(),
      "no-restricted-syntax": [
        "error",
        {
          selector: "ExpressionStatement[directive='use server']",
          message: "Không dùng Server Action (ĐX-FA-01).",
        },
      ],
      "no-restricted-exports": [
        "error",
        {
          restrictDefaultExports: {
            direct: true,
            named: true,
            defaultFrom: true,
            namedFrom: true,
            namespaceFrom: true,
          },
        },
      ],
      "react/no-danger": "error",
      "react-hooks/exhaustive-deps": "error",
      "@typescript-eslint/no-explicit-any": "error",
      "@typescript-eslint/no-non-null-assertion": "error",
      "@typescript-eslint/consistent-type-imports": "error",
      "@typescript-eslint/consistent-type-assertions": [
        "error",
        { assertionStyle: "never" },
      ],
    },
  },
  {
    files: ["src/app/**/{page,layout,error,global-error,not-found}.tsx"],
    rules: { "no-restricted-exports": "off" },
  },
  {
    files: ["src/features/**/*.{ts,tsx}"],
    rules: {
      "no-restricted-imports": camImport({
        patterns: [CAM_IMPORT_APP, CAM_IMPORT_FEATURES],
      }),
    },
  },
  ...camImportTuongDoiRaNgoaiTinhNang(),
  {
    files: ["src/shared/**/*.{ts,tsx}"],
    rules: {
      "no-restricted-imports": camImport({
        patterns: [
          CAM_IMPORT_APP,
          CAM_IMPORT_FEATURES,
          {
            group: ["@/providers/*"],
            message: "shared/ không import providers/ (FC 4.1).",
          },
        ],
      }),
    },
  },
  {
    files: ["src/shared/api/**/*.{ts,tsx}"],
    rules: {
      "no-restricted-imports": camImport({
        choAxios: true,
        patterns: [
          CAM_IMPORT_APP,
          CAM_IMPORT_FEATURES,
          {
            group: ["@/providers/*"],
            message: "shared/ không import providers/ (FC 4.1).",
          },
        ],
      }),
    },
  },
  {
    files: ["src/shared/api/session/**/*.{ts,tsx}"],
    rules: {
      "no-restricted-globals": ["error", CAM_LOCAL_STORAGE],
      "no-restricted-properties": ["error", ...CAM_THUOC_TINH_STORAGE],
    },
  },
  {
    files: ["src/**/*.test.{ts,tsx}"],
    rules: { "@typescript-eslint/consistent-type-assertions": "off" },
  },
]);

export default eslintConfig;
