-- =============================================================================
-- schema.sql - He thong quan ly muon tra thiet bi hoc tap cho sinh vien (SE100 - Nhom 13)
-- CSDL: PostgreSQL (Supabase). Phien ban 1.1 - ngay lap 27/10/2026.
-- Can cu: SRS.md, Usecase.md v1.2, BusinessRule.md, Authorization.md,
--         backend-architecture.md v1.3 (muc 5), Backend-Conventions.md (muc 7).
-- Mo ta chi tiet va truy vet: DatabaseDesign.md. Nhan [DX-DB-xx]: quyet dinh bo sung cua
-- DatabaseDesign.md (muc 14), da duoc nhom duyet.
--
-- CACH DUNG: chay MOT LAN tren CSDL moi, bang tai khoan co quyen tao schema va role
-- (vi du role postgres cua Supabase). Toan bo script nam trong mot giao dich: loi o buoc nao
-- thi khong co gi duoc tao. Script KHONG dung de nang cap CSDL da co (dung migration EF Core).
-- Sau khi chay: dat mat khau cho role ung dung ngoai kho ma nguon, vi du
--   ALTER ROLE app_backend LOGIN PASSWORD '<mat-khau-ngau-nhien-dai>';
-- =============================================================================

BEGIN;

-- 0. Chan chay lai tren CSDL da khoi tao
DO $$
BEGIN
  IF EXISTS (SELECT 1 FROM pg_namespace WHERE nspname IN ('tai_khoan', 'sinh_vien', 'danh_muc', 'quy_dinh', 'muon_tra', 'vi_pham', 'bao_tri', 'thong_bao', 'nhat_ky', 'luu_tru', 'truong_gia_lap')) THEN
    RAISE EXCEPTION 'Schema ung dung da ton tai: schema.sql chi dung de khoi tao CSDL moi';
  END IF;
END
$$;

-- 1. Role ung dung quyen toi thieu (KT 5.1); mat khau/LOGIN dat ngoai file nay
DO $$
BEGIN
  IF NOT EXISTS (SELECT 1 FROM pg_roles WHERE rolname = 'app_backend') THEN
    CREATE ROLE app_backend NOLOGIN;
  END IF;
END
$$;

-- 2. Schema: moi module mot schema; khong tao bang o schema public (KT 5.1)

CREATE SCHEMA tai_khoan;
CREATE SCHEMA sinh_vien;
CREATE SCHEMA danh_muc;
CREATE SCHEMA quy_dinh;
CREATE SCHEMA muon_tra;
CREATE SCHEMA vi_pham;
CREATE SCHEMA bao_tri;
CREATE SCHEMA thong_bao;
CREATE SCHEMA nhat_ky;
CREATE SCHEMA luu_tru;
CREATE SCHEMA truong_gia_lap;

-- 3. Sequence sinh ma chung tu {tienTo}-{yyMMdd}-{6 chu so}; chap nhan khoang trong so (KT 5.1, DX-CV-06)
CREATE SEQUENCE muon_tra.seq_ma_yeu_cau AS bigint START WITH 1 INCREMENT BY 1 NO CYCLE;
CREATE SEQUENCE muon_tra.seq_ma_dang_ky AS bigint START WITH 1 INCREMENT BY 1 NO CYCLE;
CREATE SEQUENCE muon_tra.seq_ma_phieu AS bigint START WITH 1 INCREMENT BY 1 NO CYCLE;
CREATE SEQUENCE vi_pham.seq_ma_bien_ban AS bigint START WITH 1 INCREMENT BY 1 NO CYCLE;
CREATE SEQUENCE bao_tri.seq_ma_quyet_dinh AS bigint START WITH 1 INCREMENT BY 1 NO CYCLE;

-- 4. Bang (khoa ngoai o muc 5, chi muc o muc 6)

-- Schema tai_khoan: Module TaiKhoan: tài khoản NV/QT, quyền theo phòng, phiên đăng nhập
-- DM-01 tai_khoan.nguoi_dung_he_thong
CREATE TABLE tai_khoan.nguoi_dung_he_thong (
    ma_nguoi_dung uuid NOT NULL,
    ten_dang_nhap text NOT NULL,
    mat_khau_bam text NOT NULL,
    ho_ten text NOT NULL,
    email text,
    trang_thai_khoa text NOT NULL DEFAULT 'HOAT_DONG',
    so_lan_dang_nhap_sai integer NOT NULL DEFAULT 0,
    CONSTRAINT pk_nguoi_dung_he_thong PRIMARY KEY (ma_nguoi_dung),
    CONSTRAINT uq_ndht_ten_dang_nhap UNIQUE (ten_dang_nhap),
    CONSTRAINT ck_ndht_ten_dang_nhap_khong_rong CHECK (btrim(ten_dang_nhap) <> ''),
    CONSTRAINT ck_ndht_trang_thai_khoa CHECK (trang_thai_khoa IN ('HOAT_DONG','TAM_KHOA_DANG_NHAP','BI_KHOA')),
    CONSTRAINT ck_ndht_so_lan_dang_nhap_sai CHECK (so_lan_dang_nhap_sai >= 0),
    CONSTRAINT ck_ndht_email CHECK (email IS NULL OR position('@' in email) > 1)
);

-- DM-02 tai_khoan.nhan_vien_quan_ly
CREATE TABLE tai_khoan.nhan_vien_quan_ly (
    ma_nguoi_dung uuid NOT NULL,
    ma_nv text NOT NULL,
    CONSTRAINT pk_nhan_vien_quan_ly PRIMARY KEY (ma_nguoi_dung),
    CONSTRAINT uq_nvql_ma_nv UNIQUE (ma_nv),
    CONSTRAINT ck_nvql_ma_nv_khong_rong CHECK (btrim(ma_nv) <> '')
);

-- DM-03 tai_khoan.quan_tri_vien
CREATE TABLE tai_khoan.quan_tri_vien (
    ma_nguoi_dung uuid NOT NULL,
    ma_qt text NOT NULL,
    CONSTRAINT pk_quan_tri_vien PRIMARY KEY (ma_nguoi_dung),
    CONSTRAINT uq_qtv_ma_qt UNIQUE (ma_qt),
    CONSTRAINT ck_qtv_ma_qt_khong_rong CHECK (btrim(ma_qt) <> '')
);

-- (quan hệ 11) tai_khoan.nhan_vien_phong
CREATE TABLE tai_khoan.nhan_vien_phong (
    ma_nguoi_dung uuid NOT NULL,
    ma_phong uuid NOT NULL,
    CONSTRAINT pk_nhan_vien_phong PRIMARY KEY (ma_nguoi_dung, ma_phong)
);

-- (KT 5.5) tai_khoan.refresh_token
CREATE TABLE tai_khoan.refresh_token (
    ma uuid NOT NULL,
    chu_so_huu_loai text NOT NULL,
    chu_so_huu_ma text NOT NULL,
    ma_bam text NOT NULL,
    het_han timestamptz NOT NULL,
    da_thu_hoi boolean NOT NULL DEFAULT false,
    thay_the_boi uuid,
    CONSTRAINT pk_refresh_token PRIMARY KEY (ma),
    CONSTRAINT uq_rt_ma_bam UNIQUE (ma_bam),
    CONSTRAINT ck_rt_chu_so_huu_loai CHECK (chu_so_huu_loai IN ('SINH_VIEN','NHAN_VIEN','QUAN_TRI')),
    CONSTRAINT ck_rt_chu_so_huu_ma_khong_rong CHECK (btrim(chu_so_huu_ma) <> ''),
    CONSTRAINT ck_rt_thay_the_boi CHECK (thay_the_boi IS NULL OR thay_the_boi <> ma)
);


-- Schema sinh_vien: Module SinhVien: dữ liệu SV và nhóm do nhà trường cung cấp
-- DM-05 sinh_vien.nhom_sinh_vien
CREATE TABLE sinh_vien.nhom_sinh_vien (
    ma_nhom text NOT NULL,
    ten_nhom text NOT NULL,
    CONSTRAINT pk_nhom_sinh_vien PRIMARY KEY (ma_nhom),
    CONSTRAINT ck_nsv_khong_rong CHECK (btrim(ma_nhom) <> '' AND btrim(ten_nhom) <> '')
);

-- DM-04 sinh_vien.sinh_vien
CREATE TABLE sinh_vien.sinh_vien (
    ma_sv text NOT NULL,
    ho_ten text NOT NULL,
    lop text,
    khoa text,
    email text NOT NULL,
    ma_nhom text NOT NULL,
    han_the date NOT NULL,
    trang_thai_hoc text NOT NULL,
    hoc_ky text NOT NULL,
    trang_thai_quyen_muon text NOT NULL DEFAULT 'BINH_THUONG',
    CONSTRAINT pk_sinh_vien PRIMARY KEY (ma_sv),
    CONSTRAINT ck_sv_ma_sv_khong_rong CHECK (btrim(ma_sv) <> ''),
    CONSTRAINT ck_sv_trang_thai_hoc CHECK (trang_thai_hoc IN ('DANG_HOC','TOT_NGHIEP','THOI_HOC','DINH_CHI')),
    CONSTRAINT ck_sv_trang_thai_quyen_muon CHECK (trang_thai_quyen_muon IN ('BINH_THUONG','TAM_KHOA','CAM_MUON_DAI_HAN'))
);

-- (KT 5.5) sinh_vien.dong_bo_trang_thai
CREATE TABLE sinh_vien.dong_bo_trang_thai (
    id smallint NOT NULL DEFAULT 1,
    moc_dong_bo_gan_nhat timestamptz,
    CONSTRAINT pk_dong_bo_trang_thai PRIMARY KEY (id),
    CONSTRAINT ck_dbtt_mot_dong CHECK (id = 1)
);


-- Schema danh_muc: Module DanhMuc: phòng, loại thiết bị, thiết bị, giá trị, nhà cung cấp
-- DM-08 danh_muc.phong_thiet_bi
CREATE TABLE danh_muc.phong_thiet_bi (
    ma_phong uuid NOT NULL,
    ten_phong text NOT NULL,
    co_so text NOT NULL,
    CONSTRAINT pk_phong_thiet_bi PRIMARY KEY (ma_phong)
);

-- DM-09 danh_muc.loai_thiet_bi
CREATE TABLE danh_muc.loai_thiet_bi (
    ma_loai uuid NOT NULL,
    ten_loai text NOT NULL,
    mo_ta text,
    hang_san_xuat text,
    chu_ky_bao_tri integer NOT NULL,
    phi_tre_moi_ngay numeric(14,0) NOT NULL,
    ngung_su_dung boolean NOT NULL DEFAULT false,
    CONSTRAINT pk_loai_thiet_bi PRIMARY KEY (ma_loai),
    CONSTRAINT ck_ltb_chu_ky_bao_tri CHECK (chu_ky_bao_tri > 0),
    CONSTRAINT ck_ltb_phi_tre_moi_ngay CHECK (phi_tre_moi_ngay >= 0)
);

-- DM-12 danh_muc.nha_cung_cap
CREATE TABLE danh_muc.nha_cung_cap (
    ma_ncc uuid NOT NULL,
    ten_ncc text NOT NULL,
    thong_tin_lien_he text,
    CONSTRAINT pk_nha_cung_cap PRIMARY KEY (ma_ncc)
);

-- DM-10 danh_muc.thiet_bi
CREATE TABLE danh_muc.thiet_bi (
    ma_thiet_bi text NOT NULL,
    ma_loai uuid NOT NULL,
    ma_phong uuid NOT NULL,
    ten_thiet_bi text NOT NULL,
    mo_ta text,
    thong_so_ky_thuat text,
    ngay_nhap_kho date NOT NULL,
    thoi_han_bao_hanh date,
    tinh_trang text NOT NULL DEFAULT 'SAN_SANG',
    vi_tri_ke text,
    vi_tri_tu text,
    ngung_su_dung boolean NOT NULL DEFAULT false,
    CONSTRAINT pk_thiet_bi PRIMARY KEY (ma_thiet_bi),
    CONSTRAINT ck_tb_ma_thiet_bi_khong_rong CHECK (btrim(ma_thiet_bi) <> ''),
    CONSTRAINT ck_tb_tinh_trang CHECK (tinh_trang IN ('SAN_SANG','DANG_DUOC_MUON','DANG_BAO_TRI_SUA_CHUA','DA_THANH_LY','DA_BAO_MAT'))
);

-- DM-11 danh_muc.lich_su_gia_tri
CREATE TABLE danh_muc.lich_su_gia_tri (
    ma_lich_su uuid NOT NULL,
    ma_thiet_bi text NOT NULL,
    gia_tri numeric(14,0) NOT NULL,
    ngay_cap_nhat timestamptz NOT NULL DEFAULT now(),
    CONSTRAINT pk_lich_su_gia_tri PRIMARY KEY (ma_lich_su),
    CONSTRAINT ck_lsgt_gia_tri CHECK (gia_tri >= 0)
);

-- (quan hệ 9) danh_muc.thiet_bi_nha_cung_cap
CREATE TABLE danh_muc.thiet_bi_nha_cung_cap (
    ma_thiet_bi text NOT NULL,
    ma_ncc uuid NOT NULL,
    CONSTRAINT pk_thiet_bi_nha_cung_cap PRIMARY KEY (ma_thiet_bi, ma_ncc)
);

-- (quan hệ 10) danh_muc.loai_thiet_bi_nha_cung_cap
CREATE TABLE danh_muc.loai_thiet_bi_nha_cung_cap (
    ma_loai uuid NOT NULL,
    ma_ncc uuid NOT NULL,
    CONSTRAINT pk_loai_thiet_bi_nha_cung_cap PRIMARY KEY (ma_loai, ma_ncc)
);


-- Schema quy_dinh: Module QuyDinh: quy định mượn, tham số hệ thống, khung phạt
-- DM-07 quy_dinh.tham_so_he_thong
CREATE TABLE quy_dinh.tham_so_he_thong (
    id smallint NOT NULL DEFAULT 1,
    so_ngay_khoa integer NOT NULL DEFAULT 3,
    thoi_gian_giu_cho_phut integer NOT NULL DEFAULT 30,
    an_han_phut integer NOT NULL DEFAULT 15,
    so_gio_gia_han_toi_da integer NOT NULL DEFAULT 2,
    nguong_vi_pham integer NOT NULL DEFAULT 3,
    so_lan_dang_nhap_sai_toi_da integer NOT NULL DEFAULT 5,
    gio_mo_cua time,
    gio_dong_cua time,
    CONSTRAINT pk_tham_so_he_thong PRIMARY KEY (id),
    CONSTRAINT ck_tshs_mot_dong CHECK (id = 1),
    CONSTRAINT ck_tshs_so_ngay_khoa CHECK (so_ngay_khoa >= 0),
    CONSTRAINT ck_tshs_thoi_gian_giu_cho CHECK (thoi_gian_giu_cho_phut > 0),
    CONSTRAINT ck_tshs_an_han CHECK (an_han_phut >= 0),
    CONSTRAINT ck_tshs_so_gio_gia_han CHECK (so_gio_gia_han_toi_da > 0),
    CONSTRAINT ck_tshs_nguong_vi_pham CHECK (nguong_vi_pham > 0),
    CONSTRAINT ck_tshs_so_lan_dang_nhap_sai CHECK (so_lan_dang_nhap_sai_toi_da > 0),
    CONSTRAINT ck_tshs_gio_quay CHECK ((gio_mo_cua IS NULL AND gio_dong_cua IS NULL)
            OR (gio_mo_cua IS NOT NULL AND gio_dong_cua IS NOT NULL AND gio_mo_cua < gio_dong_cua))
);

-- DM-21 quy_dinh.khung_phat
CREATE TABLE quy_dinh.khung_phat (
    ma_khung uuid NOT NULL,
    ten_khung text NOT NULL,
    ty_le_phan_tram numeric(5,2) NOT NULL,
    dang_su_dung boolean NOT NULL DEFAULT true,
    CONSTRAINT pk_khung_phat PRIMARY KEY (ma_khung),
    CONSTRAINT ck_kp_ten_khung_khong_rong CHECK (btrim(ten_khung) <> ''),
    CONSTRAINT ck_kp_ty_le CHECK (ty_le_phan_tram > 0 AND ty_le_phan_tram <= 100)
);

-- DM-06 quy_dinh.quy_dinh_muon
CREATE TABLE quy_dinh.quy_dinh_muon (
    ma_quy_dinh uuid NOT NULL,
    ma_nhom text NOT NULL,
    ma_loai uuid,
    so_thiet_bi_toi_da integer,
    thoi_gian_muon_toi_da_gio integer NOT NULL,
    CONSTRAINT pk_quy_dinh_muon PRIMARY KEY (ma_quy_dinh),
    CONSTRAINT ck_qdm_chung_hoac_ngoai_le CHECK ((ma_loai IS NULL AND so_thiet_bi_toi_da IS NOT NULL)
            OR (ma_loai IS NOT NULL AND so_thiet_bi_toi_da IS NULL)),
    CONSTRAINT ck_qdm_so_thiet_bi CHECK (so_thiet_bi_toi_da IS NULL OR so_thiet_bi_toi_da > 0),
    CONSTRAINT ck_qdm_thoi_gian CHECK (thoi_gian_muon_toi_da_gio > 0)
);


-- Schema muon_tra: Module MuonTra: yêu cầu, danh sách chờ, phiếu mượn, nhận trả
-- DM-13 muon_tra.phieu_yeu_cau_muon
CREATE TABLE muon_tra.phieu_yeu_cau_muon (
    ma_yeu_cau text NOT NULL DEFAULT ('YC-' || to_char(now() AT TIME ZONE 'Asia/Ho_Chi_Minh', 'YYMMDD') || '-' || to_char(nextval('muon_tra.seq_ma_yeu_cau'), 'FM999999000000')),
    ma_sv text NOT NULL,
    ma_phong uuid NOT NULL,
    ma_nv_duyet uuid,
    thoi_gian_gui timestamptz NOT NULL DEFAULT now(),
    thoi_gian_muon_du_kien_gio integer NOT NULL,
    trang_thai text NOT NULL DEFAULT 'CHO_DUYET',
    ly_do_tu_choi text,
    thoi_diem_duyet timestamptz,
    han_giu_cho timestamptz,
    CONSTRAINT pk_phieu_yeu_cau_muon PRIMARY KEY (ma_yeu_cau),
    CONSTRAINT ck_yc_ma_yeu_cau CHECK (ma_yeu_cau ~ '^YC-[0-9]{6}-[0-9]{6,}$'),
    CONSTRAINT ck_yc_trang_thai CHECK (trang_thai IN ('CHO_DUYET','DA_DUYET','TU_CHOI','DA_HUY','HET_HAN','DA_NHAN')),
    CONSTRAINT ck_yc_thoi_gian_muon CHECK (thoi_gian_muon_du_kien_gio > 0),
    CONSTRAINT ck_yc_ly_do_tu_choi CHECK ((trang_thai = 'TU_CHOI' AND btrim(coalesce(ly_do_tu_choi, '')) <> '')
            OR (trang_thai <> 'TU_CHOI' AND ly_do_tu_choi IS NULL)),
    CONSTRAINT ck_yc_duyet CHECK ((trang_thai = 'CHO_DUYET' AND ma_nv_duyet IS NULL AND thoi_diem_duyet IS NULL AND han_giu_cho IS NULL)
            OR (trang_thai IN ('DA_DUYET','DA_NHAN') AND ma_nv_duyet IS NOT NULL AND thoi_diem_duyet IS NOT NULL AND han_giu_cho IS NOT NULL)
            OR (trang_thai = 'TU_CHOI' AND ma_nv_duyet IS NOT NULL AND han_giu_cho IS NULL)
            OR (trang_thai IN ('DA_HUY','HET_HAN') AND ((ma_nv_duyet IS NULL AND thoi_diem_duyet IS NULL AND han_giu_cho IS NULL)
            OR (ma_nv_duyet IS NOT NULL AND thoi_diem_duyet IS NOT NULL AND han_giu_cho IS NOT NULL)))),
    CONSTRAINT ck_yc_han_giu_cho CHECK (han_giu_cho IS NULL
            OR (thoi_diem_duyet IS NOT NULL AND han_giu_cho > thoi_diem_duyet))
);

-- DM-14 muon_tra.chi_tiet_yeu_cau_muon
CREATE TABLE muon_tra.chi_tiet_yeu_cau_muon (
    ma_yeu_cau text NOT NULL,
    ma_loai uuid NOT NULL,
    so_luong integer NOT NULL,
    CONSTRAINT pk_chi_tiet_yeu_cau_muon PRIMARY KEY (ma_yeu_cau, ma_loai),
    CONSTRAINT ck_ctyc_so_luong CHECK (so_luong > 0)
);

-- DM-15 muon_tra.dang_ky_cho
CREATE TABLE muon_tra.dang_ky_cho (
    ma_dang_ky text NOT NULL DEFAULT ('DK-' || to_char(now() AT TIME ZONE 'Asia/Ho_Chi_Minh', 'YYMMDD') || '-' || to_char(nextval('muon_tra.seq_ma_dang_ky'), 'FM999999000000')),
    ma_sv text NOT NULL,
    ma_phong uuid NOT NULL,
    ma_loai uuid NOT NULL,
    thoi_diem_dang_ky timestamptz NOT NULL DEFAULT now(),
    so_thu_tu_cho integer,
    trang_thai text NOT NULL DEFAULT 'DANG_CHO',
    han_giu_cho timestamptz,
    CONSTRAINT pk_dang_ky_cho PRIMARY KEY (ma_dang_ky),
    CONSTRAINT ck_dk_ma_dang_ky CHECK (ma_dang_ky ~ '^DK-[0-9]{6}-[0-9]{6,}$'),
    CONSTRAINT ck_dk_trang_thai CHECK (trang_thai IN ('DANG_CHO','DA_THONG_BAO','DA_NHAN','DA_HUY','HET_HAN')),
    CONSTRAINT ck_dk_so_thu_tu CHECK (so_thu_tu_cho IS NULL OR so_thu_tu_cho > 0),
    CONSTRAINT ck_dk_dang_cho_co_so_thu_tu CHECK (trang_thai <> 'DANG_CHO' OR so_thu_tu_cho IS NOT NULL),
    CONSTRAINT ck_dk_han_giu_cho CHECK ((trang_thai = 'DANG_CHO' AND han_giu_cho IS NULL)
            OR (trang_thai = 'DA_THONG_BAO' AND han_giu_cho IS NOT NULL) OR trang_thai IN ('DA_NHAN','DA_HUY','HET_HAN'))
);

-- DM-16 muon_tra.phieu_muon
CREATE TABLE muon_tra.phieu_muon (
    ma_phieu text NOT NULL DEFAULT ('PM-' || to_char(now() AT TIME ZONE 'Asia/Ho_Chi_Minh', 'YYMMDD') || '-' || to_char(nextval('muon_tra.seq_ma_phieu'), 'FM999999000000')),
    ma_sv text NOT NULL,
    ma_nv_lap uuid NOT NULL,
    ma_yeu_cau text,
    ma_dang_ky text,
    ngay_gio_muon timestamptz NOT NULL DEFAULT now(),
    ngay_gio_hen_tra timestamptz NOT NULL,
    so_lan_gia_han integer NOT NULL DEFAULT 0,
    trang_thai text NOT NULL DEFAULT 'DANG_MUON',
    CONSTRAINT pk_phieu_muon PRIMARY KEY (ma_phieu),
    CONSTRAINT uq_pm_ma_yeu_cau UNIQUE (ma_yeu_cau),
    CONSTRAINT uq_pm_ma_dang_ky UNIQUE (ma_dang_ky),
    CONSTRAINT ck_pm_ma_phieu CHECK (ma_phieu ~ '^PM-[0-9]{6}-[0-9]{6,}$'),
    CONSTRAINT ck_pm_trang_thai CHECK (trang_thai IN ('DANG_MUON','QUA_HAN','DA_TRA')),
    CONSTRAINT ck_pm_so_lan_gia_han CHECK (so_lan_gia_han BETWEEN 0 AND 1),
    CONSTRAINT ck_pm_han_tra CHECK (ngay_gio_hen_tra > ngay_gio_muon),
    CONSTRAINT ck_pm_mot_nguon CHECK (ma_yeu_cau IS NULL OR ma_dang_ky IS NULL)
);

-- DM-17 muon_tra.chi_tiet_phieu_muon
CREATE TABLE muon_tra.chi_tiet_phieu_muon (
    ma_chi_tiet uuid NOT NULL,
    ma_phieu text NOT NULL,
    ma_thiet_bi text NOT NULL,
    tinh_trang_ban_dau text NOT NULL,
    phi_tre_moi_ngay numeric(14,0) NOT NULL,
    tinh_trang_khi_tra text,
    ket_qua_kiem_tra text,
    ngay_gio_tra_thuc_te timestamptz,
    ma_nv_nhan_tra uuid,
    ma_khung uuid,
    CONSTRAINT pk_chi_tiet_phieu_muon PRIMARY KEY (ma_chi_tiet),
    CONSTRAINT uq_ctpm_phieu_thiet_bi UNIQUE (ma_phieu, ma_thiet_bi),
    CONSTRAINT ck_ctpm_phi_tre CHECK (phi_tre_moi_ngay >= 0),
    CONSTRAINT ck_ctpm_tinh_trang_ban_dau CHECK (btrim(tinh_trang_ban_dau) <> ''),
    CONSTRAINT ck_ctpm_ket_qua CHECK (ket_qua_kiem_tra IS NULL OR ket_qua_kiem_tra IN ('NGUYEN_VEN','HU_HONG','THIEU_PHU_KIEN','MAT')),
    CONSTRAINT ck_ctpm_ket_qua_dong_bo CHECK ((ket_qua_kiem_tra IS NULL AND ma_nv_nhan_tra IS NULL AND ngay_gio_tra_thuc_te IS NULL AND ma_khung IS NULL AND tinh_trang_khi_tra IS NULL)
            OR (ket_qua_kiem_tra IS NOT NULL AND ket_qua_kiem_tra = 'NGUYEN_VEN' AND ma_nv_nhan_tra IS NOT NULL AND ngay_gio_tra_thuc_te IS NOT NULL AND ma_khung IS NULL)
            OR (ket_qua_kiem_tra IS NOT NULL AND ket_qua_kiem_tra IN ('HU_HONG','THIEU_PHU_KIEN') AND ma_nv_nhan_tra IS NOT NULL AND ngay_gio_tra_thuc_te IS NOT NULL AND ma_khung IS NOT NULL AND btrim(coalesce(tinh_trang_khi_tra, '')) <> '')
            OR (ket_qua_kiem_tra IS NOT NULL AND ket_qua_kiem_tra = 'MAT' AND ma_nv_nhan_tra IS NOT NULL AND ngay_gio_tra_thuc_te IS NULL AND ma_khung IS NULL))
);

-- DM-24 muon_tra.phan_hoi_tinh_trang
CREATE TABLE muon_tra.phan_hoi_tinh_trang (
    ma_phan_hoi uuid NOT NULL,
    ma_chi_tiet uuid NOT NULL,
    ma_sv text NOT NULL,
    noi_dung text NOT NULL,
    thoi_diem_gui timestamptz NOT NULL DEFAULT now(),
    CONSTRAINT pk_phan_hoi_tinh_trang PRIMARY KEY (ma_phan_hoi),
    CONSTRAINT ck_pht_noi_dung CHECK (btrim(noi_dung) <> '')
);


-- Schema vi_pham: Module ViPham: vi phạm, biên bản, chi tiết biên bản
-- DM-19 vi_pham.bien_ban_vi_pham
CREATE TABLE vi_pham.bien_ban_vi_pham (
    ma_bien_ban text NOT NULL DEFAULT ('BB-' || to_char(now() AT TIME ZONE 'Asia/Ho_Chi_Minh', 'YYMMDD') || '-' || to_char(nextval('vi_pham.seq_ma_bien_ban'), 'FM999999000000')),
    ma_phieu_muon text NOT NULL,
    ma_nv_lap uuid NOT NULL,
    ngay_lap timestamptz NOT NULL DEFAULT now(),
    tong_tien numeric(14,0) NOT NULL,
    trang_thai text NOT NULL DEFAULT 'CHUA_NOP',
    thoi_diem_nop timestamptz,
    CONSTRAINT pk_bien_ban_vi_pham PRIMARY KEY (ma_bien_ban),
    CONSTRAINT uq_bb_ma_bien_ban_phieu UNIQUE (ma_bien_ban, ma_phieu_muon),
    CONSTRAINT ck_bb_ma_bien_ban CHECK (ma_bien_ban ~ '^BB-[0-9]{6}-[0-9]{6,}$'),
    CONSTRAINT ck_bb_trang_thai CHECK (trang_thai IN ('CHUA_NOP','DA_NOP','DA_HUY')),
    CONSTRAINT ck_bb_tong_tien CHECK (tong_tien >= 0),
    CONSTRAINT ck_bb_thoi_diem_nop CHECK ((trang_thai = 'CHUA_NOP' AND thoi_diem_nop IS NULL)
            OR (trang_thai = 'DA_NOP' AND thoi_diem_nop IS NOT NULL) OR trang_thai = 'DA_HUY')
);

-- DM-20 vi_pham.chi_tiet_bien_ban
CREATE TABLE vi_pham.chi_tiet_bien_ban (
    ma_bien_ban text NOT NULL,
    ma_phieu_muon text NOT NULL,
    ma_thiet_bi text NOT NULL,
    loai_vi_pham text NOT NULL,
    so_ngay_tre integer,
    so_tien numeric(14,0) NOT NULL,
    ma_khung uuid,
    bien_ban_da_huy boolean NOT NULL DEFAULT false,
    CONSTRAINT pk_chi_tiet_bien_ban PRIMARY KEY (ma_bien_ban, ma_thiet_bi, loai_vi_pham),
    CONSTRAINT ck_ctbb_loai_vi_pham CHECK (loai_vi_pham IN ('TRA_TRE','HU_HONG','MAT')),
    CONSTRAINT ck_ctbb_so_tien CHECK (so_tien >= 0),
    CONSTRAINT ck_ctbb_so_ngay_tre CHECK ((loai_vi_pham = 'TRA_TRE' AND so_ngay_tre IS NOT NULL AND so_ngay_tre >= 1)
            OR (loai_vi_pham <> 'TRA_TRE' AND so_ngay_tre IS NULL)),
    CONSTRAINT ck_ctbb_khung_phat CHECK ((loai_vi_pham = 'HU_HONG' AND ma_khung IS NOT NULL)
            OR (loai_vi_pham <> 'HU_HONG' AND ma_khung IS NULL))
);

-- DM-18 vi_pham.vi_pham
CREATE TABLE vi_pham.vi_pham (
    ma_vi_pham uuid NOT NULL,
    ma_sv text NOT NULL,
    loai_vi_pham text NOT NULL,
    thoi_diem_ghi_nhan timestamptz NOT NULL DEFAULT now(),
    hoc_ky text NOT NULL,
    ma_bien_ban text,
    ma_yeu_cau text,
    ma_dang_ky text,
    CONSTRAINT pk_vi_pham PRIMARY KEY (ma_vi_pham),
    CONSTRAINT ck_vp_loai_vi_pham CHECK (loai_vi_pham IN ('TRA_TRE','HU_HONG','MAT','KHONG_DEN_NHAN')),
    CONSTRAINT ck_vp_dung_mot_chung_tu CHECK (((ma_bien_ban IS NOT NULL)::int + (ma_yeu_cau IS NOT NULL)::int + (ma_dang_ky IS NOT NULL)::int) = 1),
    CONSTRAINT ck_vp_loai_theo_chung_tu CHECK ((loai_vi_pham IN ('TRA_TRE','HU_HONG','MAT') AND ma_bien_ban IS NOT NULL)
            OR (loai_vi_pham = 'KHONG_DEN_NHAN' AND ma_bien_ban IS NULL))
);


-- Schema bao_tri: Module BaoTri: bảo trì/sửa chữa, quyết định thanh lý
-- DM-22 bao_tri.bao_tri_sua_chua
CREATE TABLE bao_tri.bao_tri_sua_chua (
    ma_bao_tri uuid NOT NULL,
    ma_thiet_bi text NOT NULL,
    ma_nv_thuc_hien uuid NOT NULL,
    ma_phieu_muon text,
    loai_thuc_hien text NOT NULL,
    ngay_lap timestamptz NOT NULL DEFAULT now(),
    noi_dung_thuc_hien text,
    ket_qua text,
    ngay_hoan_thanh timestamptz,
    CONSTRAINT pk_bao_tri_sua_chua PRIMARY KEY (ma_bao_tri),
    CONSTRAINT ck_bt_loai_thuc_hien CHECK (loai_thuc_hien IN ('BAO_TRI_DINH_KY','SUA_CHUA')),
    CONSTRAINT ck_bt_ket_qua CHECK (ket_qua IS NULL OR ket_qua IN ('DUA_VE_SAN_SANG','DE_XUAT_THANH_LY')),
    CONSTRAINT ck_bt_hoan_thanh_dong_bo CHECK ((ket_qua IS NULL) = (ngay_hoan_thanh IS NULL)),
    CONSTRAINT ck_bt_noi_dung CHECK (ket_qua IS NULL OR btrim(coalesce(noi_dung_thuc_hien, '')) <> ''),
    CONSTRAINT ck_bt_ngay_hoan_thanh CHECK (ngay_hoan_thanh IS NULL OR ngay_hoan_thanh >= ngay_lap),
    CONSTRAINT ck_bt_phieu_chi_cho_sua_chua CHECK (loai_thuc_hien = 'SUA_CHUA' OR ma_phieu_muon IS NULL)
);

-- DM-23 bao_tri.quyet_dinh_thanh_ly
CREATE TABLE bao_tri.quyet_dinh_thanh_ly (
    ma_quyet_dinh text NOT NULL DEFAULT ('QD-' || to_char(now() AT TIME ZONE 'Asia/Ho_Chi_Minh', 'YYMMDD') || '-' || to_char(nextval('bao_tri.seq_ma_quyet_dinh'), 'FM999999000000')),
    ma_qt_lap uuid NOT NULL,
    ngay_thanh_ly date NOT NULL,
    CONSTRAINT pk_quyet_dinh_thanh_ly PRIMARY KEY (ma_quyet_dinh),
    CONSTRAINT ck_qd_ma_quyet_dinh CHECK (ma_quyet_dinh ~ '^QD-[0-9]{6}-[0-9]{6,}$')
);

-- (quan hệ 45) bao_tri.chi_tiet_thanh_ly
CREATE TABLE bao_tri.chi_tiet_thanh_ly (
    ma_quyet_dinh text NOT NULL,
    ma_thiet_bi text NOT NULL,
    CONSTRAINT pk_chi_tiet_thanh_ly PRIMARY KEY (ma_quyet_dinh, ma_thiet_bi),
    CONSTRAINT uq_cttl_ma_thiet_bi UNIQUE (ma_thiet_bi)
);


-- Schema thong_bao: Module ThongBao: hàng đợi thông báo (outbox)
-- DM-25 thong_bao.thong_bao
CREATE TABLE thong_bao.thong_bao (
    ma_thong_bao uuid NOT NULL,
    loai_thong_bao text NOT NULL,
    noi_dung text NOT NULL,
    kenh_gui text NOT NULL,
    thoi_diem_gui timestamptz NOT NULL DEFAULT now(),
    ma_sv_nhan text,
    ma_nv_nhan uuid,
    trang_thai_gui text NOT NULL DEFAULT 'CHO_GUI',
    so_lan_thu integer NOT NULL DEFAULT 0,
    lan_thu_cuoi timestamptz,
    khoa_trung_lap text,
    doi_tuong_loai text,
    doi_tuong_id text,
    CONSTRAINT pk_thong_bao PRIMARY KEY (ma_thong_bao),
    CONSTRAINT ck_tb_loai_thong_bao CHECK (loai_thong_bao IN ('NHAC_HAN_TRA','QUA_HAN','THIET_BI_CHO_SAN_SANG','DUYET_HOAC_TU_CHOI','VI_PHAM_PHI_PHAT','NHAC_NHO_NV','NHAC_BAO_TRI')),
    CONSTRAINT ck_tb_kenh_gui CHECK (kenh_gui IN ('UNG_DUNG','EMAIL','SMS')),
    CONSTRAINT ck_tb_trang_thai_gui CHECK (trang_thai_gui IN ('CHO_GUI','DANG_GUI','DA_GUI','LOI','LOI_VINH_VIEN')),
    CONSTRAINT ck_tb_so_lan_thu CHECK (so_lan_thu >= 0),
    CONSTRAINT ck_tb_dung_mot_nguoi_nhan CHECK (((ma_sv_nhan IS NOT NULL)::int + (ma_nv_nhan IS NOT NULL)::int) = 1),
    CONSTRAINT ck_tb_doi_tuong_dong_bo CHECK ((doi_tuong_loai IS NULL) = (doi_tuong_id IS NULL))
);


-- Schema nhat_ky: Module NhatKy: nhật ký kiểm toán chỉ thêm
-- DM-26 nhat_ky.nhat_ky_he_thong
CREATE TABLE nhat_ky.nhat_ky_he_thong (
    ma_nhat_ky uuid NOT NULL,
    thoi_diem timestamptz NOT NULL DEFAULT now(),
    ma_nguoi_dung uuid,
    ma_sv text,
    thao_tac text NOT NULL,
    doi_tuong text NOT NULL,
    doi_tuong_id text,
    gia_tri_truoc jsonb,
    gia_tri_sau jsonb,
    CONSTRAINT pk_nhat_ky_he_thong PRIMARY KEY (ma_nhat_ky),
    CONSTRAINT ck_nk_toi_da_mot_nguoi_thuc_hien CHECK (ma_nguoi_dung IS NULL OR ma_sv IS NULL),
    CONSTRAINT ck_nk_thao_tac_khong_rong CHECK (btrim(thao_tac) <> '' AND btrim(doi_tuong) <> '')
);


-- Schema luu_tru: Hạ tầng: siêu dữ liệu ảnh tải lên (tệp nằm ở Supabase Storage)
-- (KT 5.5) luu_tru.anh_tai_len
CREATE TABLE luu_tru.anh_tai_len (
    ma_anh uuid NOT NULL,
    bucket text NOT NULL,
    khoa_luu_tru text NOT NULL,
    loai_noi_dung text NOT NULL,
    kich_thuoc integer NOT NULL,
    nguoi_tai_len uuid NOT NULL,
    thoi_diem timestamptz NOT NULL DEFAULT now(),
    doi_tuong_loai text,
    doi_tuong_id text,
    giai_doan text,
    CONSTRAINT pk_anh_tai_len PRIMARY KEY (ma_anh),
    CONSTRAINT uq_anh_bucket_khoa UNIQUE (bucket, khoa_luu_tru),
    CONSTRAINT ck_anh_bucket CHECK (bucket IN ('thiet-bi-anh','tinh-trang-anh')),
    CONSTRAINT ck_anh_loai_noi_dung CHECK (loai_noi_dung IN ('image/jpeg','image/png','image/webp')),
    CONSTRAINT ck_anh_kich_thuoc CHECK (kich_thuoc > 0 AND kich_thuoc <= 2097152),
    CONSTRAINT ck_anh_giai_doan CHECK (giai_doan IS NULL OR giai_doan IN ('BAN_DAU','KHI_TRA','THIET_BI')),
    CONSTRAINT ck_anh_gan_doi_tuong CHECK ((doi_tuong_loai IS NULL AND doi_tuong_id IS NULL AND giai_doan IS NULL)
            OR (doi_tuong_loai IS NOT NULL AND doi_tuong_loai = 'THIET_BI' AND doi_tuong_id IS NOT NULL AND giai_doan IS NOT NULL AND giai_doan = 'THIET_BI' AND bucket = 'thiet-bi-anh')
            OR (doi_tuong_loai IS NOT NULL AND doi_tuong_loai = 'CHI_TIET_PHIEU_MUON' AND doi_tuong_id IS NOT NULL AND giai_doan IS NOT NULL AND giai_doan IN ('BAN_DAU','KHI_TRA') AND bucket = 'tinh-trang-anh'))
);


-- Schema truong_gia_lap: Mô phỏng hệ thống nhà trường (KT-05), chỉ dùng cho đồ án
-- (KT-05) truong_gia_lap.nhom_sinh_vien_truong
CREATE TABLE truong_gia_lap.nhom_sinh_vien_truong (
    ma_nhom text NOT NULL,
    ten_nhom text NOT NULL,
    CONSTRAINT pk_nhom_sinh_vien_truong PRIMARY KEY (ma_nhom)
);

-- (KT-05) truong_gia_lap.sinh_vien_truong
CREATE TABLE truong_gia_lap.sinh_vien_truong (
    ma_sv text NOT NULL,
    mat_khau_bam text NOT NULL,
    ho_ten text NOT NULL,
    lop text,
    khoa text,
    email text NOT NULL,
    ma_nhom text NOT NULL,
    han_the date NOT NULL,
    trang_thai_hoc text NOT NULL,
    hoc_ky text NOT NULL,
    cap_nhat_luc timestamptz NOT NULL DEFAULT now(),
    CONSTRAINT pk_sinh_vien_truong PRIMARY KEY (ma_sv),
    CONSTRAINT ck_svt_trang_thai_hoc CHECK (trang_thai_hoc IN ('DANG_HOC','TOT_NGHIEP','THOI_HOC','DINH_CHI'))
);

-- 5. Khoa ngoai (khong dung ON DELETE CASCADE tru bang noi va lich su gia tri, KT 6.5; DX-DB-14)
ALTER TABLE tai_khoan.nhan_vien_quan_ly
  ADD CONSTRAINT fk_nvql_nguoi_dung FOREIGN KEY (ma_nguoi_dung)
  REFERENCES tai_khoan.nguoi_dung_he_thong (ma_nguoi_dung);
ALTER TABLE tai_khoan.quan_tri_vien
  ADD CONSTRAINT fk_qtv_nguoi_dung FOREIGN KEY (ma_nguoi_dung)
  REFERENCES tai_khoan.nguoi_dung_he_thong (ma_nguoi_dung);
ALTER TABLE tai_khoan.nhan_vien_phong
  ADD CONSTRAINT fk_nvp_nhan_vien FOREIGN KEY (ma_nguoi_dung)
  REFERENCES tai_khoan.nhan_vien_quan_ly (ma_nguoi_dung);
ALTER TABLE tai_khoan.nhan_vien_phong
  ADD CONSTRAINT fk_nvp_phong FOREIGN KEY (ma_phong)
  REFERENCES danh_muc.phong_thiet_bi (ma_phong);
ALTER TABLE tai_khoan.refresh_token
  ADD CONSTRAINT fk_rt_thay_the_boi FOREIGN KEY (thay_the_boi)
  REFERENCES tai_khoan.refresh_token (ma);
ALTER TABLE sinh_vien.sinh_vien
  ADD CONSTRAINT fk_sv_nhom FOREIGN KEY (ma_nhom)
  REFERENCES sinh_vien.nhom_sinh_vien (ma_nhom);
ALTER TABLE danh_muc.thiet_bi
  ADD CONSTRAINT fk_tb_loai FOREIGN KEY (ma_loai)
  REFERENCES danh_muc.loai_thiet_bi (ma_loai);
ALTER TABLE danh_muc.thiet_bi
  ADD CONSTRAINT fk_tb_phong FOREIGN KEY (ma_phong)
  REFERENCES danh_muc.phong_thiet_bi (ma_phong);
ALTER TABLE danh_muc.lich_su_gia_tri
  ADD CONSTRAINT fk_lsgt_thiet_bi FOREIGN KEY (ma_thiet_bi)
  REFERENCES danh_muc.thiet_bi (ma_thiet_bi) ON DELETE CASCADE;
ALTER TABLE danh_muc.thiet_bi_nha_cung_cap
  ADD CONSTRAINT fk_tbncc_thiet_bi FOREIGN KEY (ma_thiet_bi)
  REFERENCES danh_muc.thiet_bi (ma_thiet_bi) ON DELETE CASCADE;
ALTER TABLE danh_muc.thiet_bi_nha_cung_cap
  ADD CONSTRAINT fk_tbncc_ncc FOREIGN KEY (ma_ncc)
  REFERENCES danh_muc.nha_cung_cap (ma_ncc);
ALTER TABLE danh_muc.loai_thiet_bi_nha_cung_cap
  ADD CONSTRAINT fk_lncc_loai FOREIGN KEY (ma_loai)
  REFERENCES danh_muc.loai_thiet_bi (ma_loai) ON DELETE CASCADE;
ALTER TABLE danh_muc.loai_thiet_bi_nha_cung_cap
  ADD CONSTRAINT fk_lncc_ncc FOREIGN KEY (ma_ncc)
  REFERENCES danh_muc.nha_cung_cap (ma_ncc);
ALTER TABLE quy_dinh.quy_dinh_muon
  ADD CONSTRAINT fk_qdm_nhom FOREIGN KEY (ma_nhom)
  REFERENCES sinh_vien.nhom_sinh_vien (ma_nhom);
ALTER TABLE quy_dinh.quy_dinh_muon
  ADD CONSTRAINT fk_qdm_loai FOREIGN KEY (ma_loai)
  REFERENCES danh_muc.loai_thiet_bi (ma_loai);
ALTER TABLE muon_tra.phieu_yeu_cau_muon
  ADD CONSTRAINT fk_yc_sv FOREIGN KEY (ma_sv)
  REFERENCES sinh_vien.sinh_vien (ma_sv);
ALTER TABLE muon_tra.phieu_yeu_cau_muon
  ADD CONSTRAINT fk_yc_phong FOREIGN KEY (ma_phong)
  REFERENCES danh_muc.phong_thiet_bi (ma_phong);
ALTER TABLE muon_tra.phieu_yeu_cau_muon
  ADD CONSTRAINT fk_yc_nv_duyet FOREIGN KEY (ma_nv_duyet)
  REFERENCES tai_khoan.nhan_vien_quan_ly (ma_nguoi_dung);
ALTER TABLE muon_tra.chi_tiet_yeu_cau_muon
  ADD CONSTRAINT fk_ctyc_phieu FOREIGN KEY (ma_yeu_cau)
  REFERENCES muon_tra.phieu_yeu_cau_muon (ma_yeu_cau);
ALTER TABLE muon_tra.chi_tiet_yeu_cau_muon
  ADD CONSTRAINT fk_ctyc_loai FOREIGN KEY (ma_loai)
  REFERENCES danh_muc.loai_thiet_bi (ma_loai);
ALTER TABLE muon_tra.dang_ky_cho
  ADD CONSTRAINT fk_dk_sv FOREIGN KEY (ma_sv)
  REFERENCES sinh_vien.sinh_vien (ma_sv);
ALTER TABLE muon_tra.dang_ky_cho
  ADD CONSTRAINT fk_dk_loai FOREIGN KEY (ma_loai)
  REFERENCES danh_muc.loai_thiet_bi (ma_loai);
ALTER TABLE muon_tra.dang_ky_cho
  ADD CONSTRAINT fk_dk_phong FOREIGN KEY (ma_phong)
  REFERENCES danh_muc.phong_thiet_bi (ma_phong);
ALTER TABLE muon_tra.phieu_muon
  ADD CONSTRAINT fk_pm_sv FOREIGN KEY (ma_sv)
  REFERENCES sinh_vien.sinh_vien (ma_sv);
ALTER TABLE muon_tra.phieu_muon
  ADD CONSTRAINT fk_pm_nv_lap FOREIGN KEY (ma_nv_lap)
  REFERENCES tai_khoan.nhan_vien_quan_ly (ma_nguoi_dung);
ALTER TABLE muon_tra.phieu_muon
  ADD CONSTRAINT fk_pm_yeu_cau FOREIGN KEY (ma_yeu_cau)
  REFERENCES muon_tra.phieu_yeu_cau_muon (ma_yeu_cau);
ALTER TABLE muon_tra.phieu_muon
  ADD CONSTRAINT fk_pm_dang_ky FOREIGN KEY (ma_dang_ky)
  REFERENCES muon_tra.dang_ky_cho (ma_dang_ky);
ALTER TABLE muon_tra.chi_tiet_phieu_muon
  ADD CONSTRAINT fk_ctpm_phieu FOREIGN KEY (ma_phieu)
  REFERENCES muon_tra.phieu_muon (ma_phieu);
ALTER TABLE muon_tra.chi_tiet_phieu_muon
  ADD CONSTRAINT fk_ctpm_thiet_bi FOREIGN KEY (ma_thiet_bi)
  REFERENCES danh_muc.thiet_bi (ma_thiet_bi);
ALTER TABLE muon_tra.chi_tiet_phieu_muon
  ADD CONSTRAINT fk_ctpm_khung FOREIGN KEY (ma_khung)
  REFERENCES quy_dinh.khung_phat (ma_khung);
ALTER TABLE muon_tra.chi_tiet_phieu_muon
  ADD CONSTRAINT fk_ctpm_nv_nhan_tra FOREIGN KEY (ma_nv_nhan_tra)
  REFERENCES tai_khoan.nhan_vien_quan_ly (ma_nguoi_dung);
ALTER TABLE muon_tra.phan_hoi_tinh_trang
  ADD CONSTRAINT fk_pht_chi_tiet FOREIGN KEY (ma_chi_tiet)
  REFERENCES muon_tra.chi_tiet_phieu_muon (ma_chi_tiet);
ALTER TABLE muon_tra.phan_hoi_tinh_trang
  ADD CONSTRAINT fk_pht_sv FOREIGN KEY (ma_sv)
  REFERENCES sinh_vien.sinh_vien (ma_sv);
ALTER TABLE vi_pham.bien_ban_vi_pham
  ADD CONSTRAINT fk_bb_phieu FOREIGN KEY (ma_phieu_muon)
  REFERENCES muon_tra.phieu_muon (ma_phieu);
ALTER TABLE vi_pham.bien_ban_vi_pham
  ADD CONSTRAINT fk_bb_nv_lap FOREIGN KEY (ma_nv_lap)
  REFERENCES tai_khoan.nhan_vien_quan_ly (ma_nguoi_dung);
ALTER TABLE vi_pham.chi_tiet_bien_ban
  ADD CONSTRAINT fk_ctbb_bien_ban FOREIGN KEY (ma_bien_ban, ma_phieu_muon)
  REFERENCES vi_pham.bien_ban_vi_pham (ma_bien_ban, ma_phieu_muon);
ALTER TABLE vi_pham.chi_tiet_bien_ban
  ADD CONSTRAINT fk_ctbb_chi_tiet_phieu FOREIGN KEY (ma_phieu_muon, ma_thiet_bi)
  REFERENCES muon_tra.chi_tiet_phieu_muon (ma_phieu, ma_thiet_bi);
ALTER TABLE vi_pham.chi_tiet_bien_ban
  ADD CONSTRAINT fk_ctbb_khung FOREIGN KEY (ma_khung)
  REFERENCES quy_dinh.khung_phat (ma_khung);
ALTER TABLE vi_pham.vi_pham
  ADD CONSTRAINT fk_vp_sv FOREIGN KEY (ma_sv)
  REFERENCES sinh_vien.sinh_vien (ma_sv);
ALTER TABLE vi_pham.vi_pham
  ADD CONSTRAINT fk_vp_bien_ban FOREIGN KEY (ma_bien_ban)
  REFERENCES vi_pham.bien_ban_vi_pham (ma_bien_ban);
ALTER TABLE vi_pham.vi_pham
  ADD CONSTRAINT fk_vp_yeu_cau FOREIGN KEY (ma_yeu_cau)
  REFERENCES muon_tra.phieu_yeu_cau_muon (ma_yeu_cau);
ALTER TABLE vi_pham.vi_pham
  ADD CONSTRAINT fk_vp_dang_ky FOREIGN KEY (ma_dang_ky)
  REFERENCES muon_tra.dang_ky_cho (ma_dang_ky);
ALTER TABLE bao_tri.bao_tri_sua_chua
  ADD CONSTRAINT fk_bt_thiet_bi FOREIGN KEY (ma_thiet_bi)
  REFERENCES danh_muc.thiet_bi (ma_thiet_bi);
ALTER TABLE bao_tri.bao_tri_sua_chua
  ADD CONSTRAINT fk_bt_nv FOREIGN KEY (ma_nv_thuc_hien)
  REFERENCES tai_khoan.nhan_vien_quan_ly (ma_nguoi_dung);
ALTER TABLE bao_tri.bao_tri_sua_chua
  ADD CONSTRAINT fk_bt_phieu FOREIGN KEY (ma_phieu_muon)
  REFERENCES muon_tra.phieu_muon (ma_phieu);
ALTER TABLE bao_tri.quyet_dinh_thanh_ly
  ADD CONSTRAINT fk_qd_qt FOREIGN KEY (ma_qt_lap)
  REFERENCES tai_khoan.quan_tri_vien (ma_nguoi_dung);
ALTER TABLE bao_tri.chi_tiet_thanh_ly
  ADD CONSTRAINT fk_cttl_quyet_dinh FOREIGN KEY (ma_quyet_dinh)
  REFERENCES bao_tri.quyet_dinh_thanh_ly (ma_quyet_dinh);
ALTER TABLE bao_tri.chi_tiet_thanh_ly
  ADD CONSTRAINT fk_cttl_thiet_bi FOREIGN KEY (ma_thiet_bi)
  REFERENCES danh_muc.thiet_bi (ma_thiet_bi);
ALTER TABLE thong_bao.thong_bao
  ADD CONSTRAINT fk_tb_sv_nhan FOREIGN KEY (ma_sv_nhan)
  REFERENCES sinh_vien.sinh_vien (ma_sv);
ALTER TABLE thong_bao.thong_bao
  ADD CONSTRAINT fk_tb_nv_nhan FOREIGN KEY (ma_nv_nhan)
  REFERENCES tai_khoan.nhan_vien_quan_ly (ma_nguoi_dung);
ALTER TABLE nhat_ky.nhat_ky_he_thong
  ADD CONSTRAINT fk_nk_nguoi_dung FOREIGN KEY (ma_nguoi_dung)
  REFERENCES tai_khoan.nguoi_dung_he_thong (ma_nguoi_dung);
ALTER TABLE nhat_ky.nhat_ky_he_thong
  ADD CONSTRAINT fk_nk_sv FOREIGN KEY (ma_sv)
  REFERENCES sinh_vien.sinh_vien (ma_sv);
ALTER TABLE luu_tru.anh_tai_len
  ADD CONSTRAINT fk_anh_nguoi_tai_len FOREIGN KEY (nguoi_tai_len)
  REFERENCES tai_khoan.nguoi_dung_he_thong (ma_nguoi_dung);
ALTER TABLE truong_gia_lap.sinh_vien_truong
  ADD CONSTRAINT fk_svt_nhom FOREIGN KEY (ma_nhom)
  REFERENCES truong_gia_lap.nhom_sinh_vien_truong (ma_nhom);

-- 6. Chi muc (KT 5.6 va chi muc ho tro khoa ngoai, DX-DB-15)
CREATE INDEX ix_rt_chu_so_huu ON tai_khoan.refresh_token (chu_so_huu_loai, chu_so_huu_ma);
CREATE INDEX ix_nvp_phong ON tai_khoan.nhan_vien_phong (ma_phong);
CREATE INDEX ix_sv_nhom ON sinh_vien.sinh_vien (ma_nhom);
CREATE INDEX ix_tb_phong_loai_tinh_trang ON danh_muc.thiet_bi (ma_phong, ma_loai, tinh_trang);
CREATE INDEX ix_tb_loai ON danh_muc.thiet_bi (ma_loai);
CREATE INDEX ix_lsgt_thiet_bi_ngay ON danh_muc.lich_su_gia_tri (ma_thiet_bi, ngay_cap_nhat DESC);
CREATE INDEX ix_tbncc_ncc ON danh_muc.thiet_bi_nha_cung_cap (ma_ncc);
CREATE INDEX ix_lncc_ncc ON danh_muc.loai_thiet_bi_nha_cung_cap (ma_ncc);
CREATE UNIQUE INDEX ux_qdm_chung ON quy_dinh.quy_dinh_muon (ma_nhom) WHERE ma_loai IS NULL;
CREATE UNIQUE INDEX ux_qdm_ngoai_le ON quy_dinh.quy_dinh_muon (ma_nhom, ma_loai) WHERE ma_loai IS NOT NULL;
CREATE INDEX ix_qdm_loai ON quy_dinh.quy_dinh_muon (ma_loai) WHERE ma_loai IS NOT NULL;
CREATE INDEX ix_yc_phong_trang_thai_han ON muon_tra.phieu_yeu_cau_muon (ma_phong, trang_thai, han_giu_cho);
CREATE INDEX ix_yc_sv_thoi_gian ON muon_tra.phieu_yeu_cau_muon (ma_sv, thoi_gian_gui DESC);
CREATE INDEX ix_ctyc_loai ON muon_tra.chi_tiet_yeu_cau_muon (ma_loai);
CREATE INDEX ix_dk_hang_cho ON muon_tra.dang_ky_cho (ma_phong, ma_loai, trang_thai, thoi_diem_dang_ky);
CREATE INDEX ix_dk_sv_trang_thai ON muon_tra.dang_ky_cho (ma_sv, trang_thai);
CREATE INDEX ix_pm_trang_thai_han_tra ON muon_tra.phieu_muon (trang_thai, ngay_gio_hen_tra);
CREATE INDEX ix_pm_sv_trang_thai ON muon_tra.phieu_muon (ma_sv, trang_thai);
CREATE UNIQUE INDEX ux_ctpm_thiet_bi_dang_muon ON muon_tra.chi_tiet_phieu_muon (ma_thiet_bi) WHERE ket_qua_kiem_tra IS NULL;
CREATE INDEX ix_ctpm_thiet_bi ON muon_tra.chi_tiet_phieu_muon (ma_thiet_bi);
CREATE INDEX ix_ctpm_khung ON muon_tra.chi_tiet_phieu_muon (ma_khung) WHERE ma_khung IS NOT NULL;
CREATE INDEX ix_pht_chi_tiet ON muon_tra.phan_hoi_tinh_trang (ma_chi_tiet);
CREATE INDEX ix_bb_phieu ON vi_pham.bien_ban_vi_pham (ma_phieu_muon);
CREATE INDEX ix_bb_chua_nop ON vi_pham.bien_ban_vi_pham (ma_phieu_muon) WHERE trang_thai = 'CHUA_NOP';
CREATE UNIQUE INDEX ux_ctbb_khoan ON vi_pham.chi_tiet_bien_ban (ma_phieu_muon, ma_thiet_bi, loai_vi_pham) WHERE NOT bien_ban_da_huy;
CREATE INDEX ix_ctbb_khung ON vi_pham.chi_tiet_bien_ban (ma_khung) WHERE ma_khung IS NOT NULL;
CREATE INDEX ix_vp_sv_hoc_ky ON vi_pham.vi_pham (ma_sv, hoc_ky);
CREATE UNIQUE INDEX ux_vp_bien_ban_loai ON vi_pham.vi_pham (ma_bien_ban, loai_vi_pham) WHERE ma_bien_ban IS NOT NULL;
CREATE UNIQUE INDEX ux_vp_yeu_cau ON vi_pham.vi_pham (ma_yeu_cau) WHERE ma_yeu_cau IS NOT NULL;
CREATE UNIQUE INDEX ux_vp_dang_ky ON vi_pham.vi_pham (ma_dang_ky) WHERE ma_dang_ky IS NOT NULL;
CREATE INDEX ix_bt_thiet_bi_hoan_thanh ON bao_tri.bao_tri_sua_chua (ma_thiet_bi, loai_thuc_hien, ngay_hoan_thanh DESC);
CREATE UNIQUE INDEX ux_bt_mot_ban_ghi_mo ON bao_tri.bao_tri_sua_chua (ma_thiet_bi) WHERE ket_qua IS NULL;
CREATE UNIQUE INDEX ux_tb_khoa_trung_lap ON thong_bao.thong_bao (khoa_trung_lap, kenh_gui) WHERE khoa_trung_lap IS NOT NULL;
CREATE INDEX ix_tb_gui_cho ON thong_bao.thong_bao (kenh_gui, trang_thai_gui, lan_thu_cuoi) WHERE trang_thai_gui IN ('CHO_GUI','LOI');
CREATE INDEX ix_tb_sv_thoi_diem ON thong_bao.thong_bao (ma_sv_nhan, thoi_diem_gui DESC) WHERE ma_sv_nhan IS NOT NULL;
CREATE INDEX ix_nk_thoi_diem ON nhat_ky.nhat_ky_he_thong (thoi_diem);
CREATE INDEX ix_nk_doi_tuong ON nhat_ky.nhat_ky_he_thong (doi_tuong, doi_tuong_id);
CREATE INDEX ix_anh_doi_tuong ON luu_tru.anh_tai_len (doi_tuong_loai, doi_tuong_id, giai_doan) WHERE doi_tuong_id IS NOT NULL;
CREATE INDEX ix_anh_mo_coi ON luu_tru.anh_tai_len (thoi_diem) WHERE doi_tuong_id IS NULL;
CREATE INDEX ix_svt_cap_nhat_luc ON truong_gia_lap.sinh_vien_truong (cap_nhat_luc);

-- 7. Ham va trigger
-- 7.1 Nhat ky chi them: chan UPDATE, DELETE, TRUNCATE (NFR-02, BR-79, KT 12.3)
CREATE OR REPLACE FUNCTION nhat_ky.chan_sua_xoa() RETURNS trigger
LANGUAGE plpgsql AS $$
BEGIN
  RAISE EXCEPTION 'Nhat ky he thong chi duoc them moi, khong duoc sua hoac xoa';
END;
$$;

CREATE TRIGGER trg_nhat_ky_chan_sua_xoa
  BEFORE UPDATE OR DELETE ON nhat_ky.nhat_ky_he_thong
  FOR EACH ROW EXECUTE FUNCTION nhat_ky.chan_sua_xoa();

CREATE TRIGGER trg_nhat_ky_chan_truncate
  BEFORE TRUNCATE ON nhat_ky.nhat_ky_he_thong
  FOR EACH STATEMENT EXECUTE FUNCTION nhat_ky.chan_sua_xoa();

-- 7.2 Phia truong gia lap: tu dat moc thay doi de LayThayDoiTuAsync keo thay doi (KT 7.1)
CREATE OR REPLACE FUNCTION truong_gia_lap.dat_cap_nhat_luc() RETURNS trigger
LANGUAGE plpgsql AS $$
BEGIN
  NEW.cap_nhat_luc := now();
  RETURN NEW;
END;
$$;

CREATE TRIGGER trg_sinh_vien_truong_cap_nhat
  BEFORE UPDATE ON truong_gia_lap.sinh_vien_truong
  FOR EACH ROW EXECUTE FUNCTION truong_gia_lap.dat_cap_nhat_luc();

-- 7.3 chi_tiet_bien_ban.bien_ban_da_huy luon bang trang thai huy cua bien ban (KT 5.6, DX-DB-07)
CREATE OR REPLACE FUNCTION vi_pham.dat_bien_ban_da_huy() RETURNS trigger
LANGUAGE plpgsql AS $$
BEGIN
  NEW.bien_ban_da_huy := EXISTS (
    SELECT 1 FROM vi_pham.bien_ban_vi_pham b
    WHERE b.ma_bien_ban = NEW.ma_bien_ban AND b.trang_thai = 'DA_HUY');
  RETURN NEW;
END;
$$;

CREATE TRIGGER trg_ctbb_dat_da_huy
  BEFORE INSERT ON vi_pham.chi_tiet_bien_ban
  FOR EACH ROW EXECUTE FUNCTION vi_pham.dat_bien_ban_da_huy();

CREATE OR REPLACE FUNCTION vi_pham.dong_bo_bien_ban_da_huy() RETURNS trigger
LANGUAGE plpgsql AS $$
BEGIN
  UPDATE vi_pham.chi_tiet_bien_ban
     SET bien_ban_da_huy = (NEW.trang_thai = 'DA_HUY')
   WHERE ma_bien_ban = NEW.ma_bien_ban
     AND bien_ban_da_huy IS DISTINCT FROM (NEW.trang_thai = 'DA_HUY');
  RETURN NULL;
END;
$$;

CREATE TRIGGER trg_bien_ban_dong_bo_da_huy
  AFTER UPDATE OF trang_thai ON vi_pham.bien_ban_vi_pham
  FOR EACH ROW
  WHEN (OLD.trang_thai IS DISTINCT FROM NEW.trang_thai)
  EXECUTE FUNCTION vi_pham.dong_bo_bien_ban_da_huy();

-- 7.4 Rang buoc hop thanh 1..* va toan ven tien, kiem tra khi COMMIT (DEFERRED), DX-DB-13
CREATE OR REPLACE FUNCTION vi_pham.kiem_tra_bien_ban() RETURNS trigger
LANGUAGE plpgsql AS $$
DECLARE
  v_ma text;
  v_tong numeric;
  v_so_dong integer;
  v_tong_chi_tiet numeric;
BEGIN
  IF TG_OP = 'DELETE' THEN
    v_ma := OLD.ma_bien_ban;
  ELSE
    v_ma := NEW.ma_bien_ban;
  END IF;

  SELECT tong_tien INTO v_tong FROM vi_pham.bien_ban_vi_pham WHERE ma_bien_ban = v_ma;
  IF NOT FOUND THEN
    RETURN NULL;
  END IF;

  SELECT count(*), COALESCE(sum(so_tien), 0)
    INTO v_so_dong, v_tong_chi_tiet
    FROM vi_pham.chi_tiet_bien_ban WHERE ma_bien_ban = v_ma;

  IF v_so_dong = 0 THEN
    RAISE EXCEPTION 'Bien ban % phai co it nhat mot chi tiet', v_ma USING ERRCODE = '23514';
  END IF;
  IF v_tong <> v_tong_chi_tiet THEN
    RAISE EXCEPTION 'Bien ban %: tong_tien (%) khong bang tong so_tien cac chi tiet (%)',
      v_ma, v_tong, v_tong_chi_tiet USING ERRCODE = '23514';
  END IF;
  RETURN NULL;
END;
$$;

CREATE CONSTRAINT TRIGGER trg_bien_ban_kiem_tra
  AFTER INSERT OR UPDATE OF tong_tien ON vi_pham.bien_ban_vi_pham
  DEFERRABLE INITIALLY DEFERRED
  FOR EACH ROW EXECUTE FUNCTION vi_pham.kiem_tra_bien_ban();

CREATE CONSTRAINT TRIGGER trg_ctbb_kiem_tra
  AFTER INSERT OR UPDATE OR DELETE ON vi_pham.chi_tiet_bien_ban
  DEFERRABLE INITIALLY DEFERRED
  FOR EACH ROW EXECUTE FUNCTION vi_pham.kiem_tra_bien_ban();

CREATE OR REPLACE FUNCTION muon_tra.kiem_tra_phieu_yeu_cau_co_chi_tiet() RETURNS trigger
LANGUAGE plpgsql AS $$
BEGIN
  IF NOT EXISTS (SELECT 1 FROM muon_tra.chi_tiet_yeu_cau_muon WHERE ma_yeu_cau = NEW.ma_yeu_cau) THEN
    RAISE EXCEPTION 'Phieu yeu cau % phai co it nhat mot chi tiet', NEW.ma_yeu_cau USING ERRCODE = '23514';
  END IF;
  RETURN NULL;
END;
$$;

CREATE CONSTRAINT TRIGGER trg_yc_co_chi_tiet
  AFTER INSERT ON muon_tra.phieu_yeu_cau_muon
  DEFERRABLE INITIALLY DEFERRED
  FOR EACH ROW EXECUTE FUNCTION muon_tra.kiem_tra_phieu_yeu_cau_co_chi_tiet();

CREATE OR REPLACE FUNCTION muon_tra.kiem_tra_phieu_muon_co_chi_tiet() RETURNS trigger
LANGUAGE plpgsql AS $$
BEGIN
  IF NOT EXISTS (SELECT 1 FROM muon_tra.chi_tiet_phieu_muon WHERE ma_phieu = NEW.ma_phieu) THEN
    RAISE EXCEPTION 'Phieu muon % phai co it nhat mot chi tiet', NEW.ma_phieu USING ERRCODE = '23514';
  END IF;
  RETURN NULL;
END;
$$;

CREATE CONSTRAINT TRIGGER trg_pm_co_chi_tiet
  AFTER INSERT ON muon_tra.phieu_muon
  DEFERRABLE INITIALLY DEFERRED
  FOR EACH ROW EXECUTE FUNCTION muon_tra.kiem_tra_phieu_muon_co_chi_tiet();

CREATE OR REPLACE FUNCTION bao_tri.kiem_tra_quyet_dinh_co_chi_tiet() RETURNS trigger
LANGUAGE plpgsql AS $$
BEGIN
  IF NOT EXISTS (SELECT 1 FROM bao_tri.chi_tiet_thanh_ly WHERE ma_quyet_dinh = NEW.ma_quyet_dinh) THEN
    RAISE EXCEPTION 'Quyet dinh thanh ly % phai co it nhat mot thiet bi', NEW.ma_quyet_dinh USING ERRCODE = '23514';
  END IF;
  RETURN NULL;
END;
$$;

CREATE CONSTRAINT TRIGGER trg_qd_co_chi_tiet
  AFTER INSERT ON bao_tri.quyet_dinh_thanh_ly
  DEFERRABLE INITIALLY DEFERRED
  FOR EACH ROW EXECUTE FUNCTION bao_tri.kiem_tra_quyet_dinh_co_chi_tiet();

CREATE OR REPLACE FUNCTION danh_muc.kiem_tra_thiet_bi_co_gia_tri() RETURNS trigger
LANGUAGE plpgsql AS $$
BEGIN
  IF NOT EXISTS (SELECT 1 FROM danh_muc.lich_su_gia_tri WHERE ma_thiet_bi = NEW.ma_thiet_bi) THEN
    RAISE EXCEPTION 'Thiet bi % phai co it nhat mot gia tri trong lich_su_gia_tri', NEW.ma_thiet_bi USING ERRCODE = '23514';
  END IF;
  RETURN NULL;
END;
$$;

CREATE CONSTRAINT TRIGGER trg_tb_co_gia_tri
  AFTER INSERT ON danh_muc.thiet_bi
  DEFERRABLE INITIALLY DEFERRED
  FOR EACH ROW EXECUTE FUNCTION danh_muc.kiem_tra_thiet_bi_co_gia_tri();

-- 8. Chu thich bang va cot
COMMENT ON TABLE tai_khoan.nguoi_dung_he_thong IS 'DM-01: Lớp cha (trừu tượng) của NV và QT: tài khoản do hệ thống cấp, mật khẩu băm, có thể bị khóa.';
COMMENT ON COLUMN tai_khoan.nguoi_dung_he_thong.ma_nguoi_dung IS 'Khóa chính, định danh chung của NV và QT (uuid v7 do ứng dụng sinh).';
COMMENT ON COLUMN tai_khoan.nguoi_dung_he_thong.ten_dang_nhap IS 'Tên đăng nhập, duy nhất; trùng thì từ chối.';
COMMENT ON COLUMN tai_khoan.nguoi_dung_he_thong.mat_khau_bam IS 'Mật khẩu đã băm (PBKDF2 qua PasswordHasher); không bao giờ lưu dạng rõ; bị loại khỏi JSON nhật ký.';
COMMENT ON COLUMN tai_khoan.nguoi_dung_he_thong.ho_ten IS 'Họ tên người dùng.';
COMMENT ON COLUMN tai_khoan.nguoi_dung_he_thong.email IS 'Địa chỉ nhận email (nhắc bảo trì cho NV). Cho phép rỗng vì tài khoản QT khởi tạo chỉ có tên đăng nhập và mật khẩu.';
COMMENT ON COLUMN tai_khoan.nguoi_dung_he_thong.trang_thai_khoa IS 'Hoạt động / Tạm khóa đăng nhập (hệ thống đặt khi nhập sai quá mức) / Bị khóa (QT khóa).';
COMMENT ON COLUMN tai_khoan.nguoi_dung_he_thong.so_lan_dang_nhap_sai IS 'Số lần nhập sai liên tiếp; về 0 khi đăng nhập đúng.';
COMMENT ON TABLE tai_khoan.nhan_vien_quan_ly IS 'DM-02: Lớp con của NguoiDungHeThong (kiểu bảng riêng TPT): nhân viên quản lý thiết bị.';
COMMENT ON COLUMN tai_khoan.nhan_vien_quan_ly.ma_nguoi_dung IS 'Khóa chính và khóa ngoại tới nguoi_dung_he_thong.';
COMMENT ON COLUMN tai_khoan.nhan_vien_quan_ly.ma_nv IS 'Mã nhân viên, duy nhất; là thuộc tính, không phải khóa chính.';
COMMENT ON TABLE tai_khoan.quan_tri_vien IS 'DM-03: Lớp con của NguoiDungHeThong (TPT): quản trị viên.';
COMMENT ON COLUMN tai_khoan.quan_tri_vien.ma_nguoi_dung IS 'Khóa chính và khóa ngoại tới nguoi_dung_he_thong.';
COMMENT ON COLUMN tai_khoan.quan_tri_vien.ma_qt IS 'Mã quản trị viên, duy nhất; là thuộc tính, không phải khóa chính.';
COMMENT ON TABLE tai_khoan.nhan_vien_phong IS '(quan hệ 11): Bảng nối n-n: phòng thiết bị mà NV được QT cấp quyền. Thu hồi quyền = xóa dòng (lịch sử nằm ở nhật ký).';
COMMENT ON COLUMN tai_khoan.nhan_vien_phong.ma_nguoi_dung IS 'NV được cấp quyền.';
COMMENT ON COLUMN tai_khoan.nhan_vien_phong.ma_phong IS 'Phòng thiết bị được cấp quyền.';
COMMENT ON TABLE tai_khoan.refresh_token IS '(KT 5.5): Refresh token (chỉ lưu giá trị băm), xoay vòng mỗi lần làm mới; dùng cho cả SV, NV, QT.';
COMMENT ON COLUMN tai_khoan.refresh_token.ma IS 'Khóa chính của token.';
COMMENT ON COLUMN tai_khoan.refresh_token.chu_so_huu_loai IS 'Loại chủ sở hữu theo mã vai trò: SINH_VIEN, NHAN_VIEN, QUAN_TRI.';
COMMENT ON COLUMN tai_khoan.refresh_token.chu_so_huu_ma IS 'Mã chủ sở hữu: ma_sv (SV) hoặc ma_nguoi_dung dạng chuỗi (NV, QT). Không có FK vì đa hình.';
COMMENT ON COLUMN tai_khoan.refresh_token.ma_bam IS 'Giá trị băm của token; không lưu token gốc.';
COMMENT ON COLUMN tai_khoan.refresh_token.het_han IS 'Thời điểm hết hạn (7 ngày kể từ khi cấp).';
COMMENT ON COLUMN tai_khoan.refresh_token.da_thu_hoi IS 'Đã thu hồi (đăng xuất, khóa tài khoản, phát hiện dùng lại token cũ).';
COMMENT ON COLUMN tai_khoan.refresh_token.thay_the_boi IS 'Token mới thay thế khi xoay vòng (để thu hồi cả chuỗi).';
COMMENT ON TABLE sinh_vien.nhom_sinh_vien IS 'DM-05: Nhóm sinh viên do nhà trường cung cấp (đại học; cao học; làm đồ án tốt nghiệp). QT không tạo nhóm.';
COMMENT ON COLUMN sinh_vien.nhom_sinh_vien.ma_nhom IS 'Khóa chính, mã nhóm do nhà trường cung cấp (khóa nghiệp vụ, không phải uuid).';
COMMENT ON COLUMN sinh_vien.nhom_sinh_vien.ten_nhom IS 'Tên nhóm.';
COMMENT ON TABLE sinh_vien.sinh_vien IS 'DM-04: Dữ liệu SV do nhà trường cung cấp (đồng bộ). Không có cột mật khẩu. trang_thai_quyen_muon do hệ thống này sở hữu, không bị đồng bộ ghi đè.';
COMMENT ON COLUMN sinh_vien.sinh_vien.ma_sv IS 'Khóa chính, mã số sinh viên (khóa nghiệp vụ); cũng là nội dung mã thẻ quét.';
COMMENT ON COLUMN sinh_vien.sinh_vien.ho_ten IS 'Họ tên.';
COMMENT ON COLUMN sinh_vien.sinh_vien.lop IS 'Lớp (chỉ để hiển thị).';
COMMENT ON COLUMN sinh_vien.sinh_vien.khoa IS 'Khoa (chỉ để hiển thị).';
COMMENT ON COLUMN sinh_vien.sinh_vien.email IS 'Email do nhà trường cung cấp, dùng gửi thông báo.';
COMMENT ON COLUMN sinh_vien.sinh_vien.ma_nhom IS 'Nhóm sinh viên quyết định giới hạn mượn.';
COMMENT ON COLUMN sinh_vien.sinh_vien.han_the IS 'Ngày hết hiệu lực của thẻ; thẻ hết hạn thì không đặt trước/mượn.';
COMMENT ON COLUMN sinh_vien.sinh_vien.trang_thai_hoc IS 'Đang học / Tốt nghiệp / Thôi học / Đình chỉ.';
COMMENT ON COLUMN sinh_vien.sinh_vien.hoc_ky IS 'Học kỳ hiện tại theo hệ thống nhà trường (định dạng do nhà trường quy định).';
COMMENT ON COLUMN sinh_vien.sinh_vien.trang_thai_quyen_muon IS 'Bình thường / Tạm khóa / Cấm mượn dài hạn.';
COMMENT ON TABLE sinh_vien.dong_bo_trang_thai IS '(KT 5.5): Một dòng duy nhất: mốc đồng bộ gần nhất để job DongBoSinhVien kéo thay đổi từ sau mốc đó.';
COMMENT ON COLUMN sinh_vien.dong_bo_trang_thai.id IS 'Luôn bằng 1.';
COMMENT ON COLUMN sinh_vien.dong_bo_trang_thai.moc_dong_bo_gan_nhat IS 'Mốc đồng bộ gần nhất; rỗng nghĩa là chưa đồng bộ lần nào.';
COMMENT ON TABLE danh_muc.phong_thiet_bi IS 'DM-08: Phòng thiết bị: thuộc một cơ sở; kệ và tủ là vị trí lưu trữ của thiết bị.';
COMMENT ON COLUMN danh_muc.phong_thiet_bi.ma_phong IS 'Khóa chính (uuid v7).';
COMMENT ON COLUMN danh_muc.phong_thiet_bi.ten_phong IS 'Tên phòng.';
COMMENT ON COLUMN danh_muc.phong_thiet_bi.co_so IS 'Cơ sở mà phòng thuộc về.';
COMMENT ON TABLE danh_muc.loai_thiet_bi IS 'DM-09: Nhóm phân loại thiết bị; có chu kỳ bảo trì và phí trễ mỗi ngày. Đã có lịch sử thì chỉ ngưng sử dụng.';
COMMENT ON COLUMN danh_muc.loai_thiet_bi.ma_loai IS 'Khóa chính (uuid v7).';
COMMENT ON COLUMN danh_muc.loai_thiet_bi.ten_loai IS 'Tên loại thiết bị.';
COMMENT ON COLUMN danh_muc.loai_thiet_bi.mo_ta IS 'Mô tả.';
COMMENT ON COLUMN danh_muc.loai_thiet_bi.hang_san_xuat IS 'Hãng sản xuất.';
COMMENT ON COLUMN danh_muc.loai_thiet_bi.chu_ky_bao_tri IS 'Chu kỳ bảo trì (ngày), lớn hơn 0; không có giá trị mặc định.';
COMMENT ON COLUMN danh_muc.loai_thiet_bi.phi_tre_moi_ngay IS 'Phí trả trễ (VNĐ/ngày) của loại, không âm; không có mức mặc định.';
COMMENT ON COLUMN danh_muc.loai_thiet_bi.ngung_su_dung IS 'Cờ ngưng sử dụng thay cho xóa cứng khi đã có lịch sử.';
COMMENT ON TABLE danh_muc.nha_cung_cap IS 'DM-12: Nhà cung cấp; một thiết bị hoặc loại thiết bị có thể do nhiều nhà cung cấp. Giai đoạn 1 chỉ đọc và gán.';
COMMENT ON COLUMN danh_muc.nha_cung_cap.ma_ncc IS 'Khóa chính (uuid v7).';
COMMENT ON COLUMN danh_muc.nha_cung_cap.ten_ncc IS 'Tên nhà cung cấp.';
COMMENT ON COLUMN danh_muc.nha_cung_cap.thong_tin_lien_he IS 'Thông tin liên hệ.';
COMMENT ON TABLE danh_muc.thiet_bi IS 'DM-10: Một đơn vị vật lý có mã riêng, thuộc một loại và một phòng. Giá trị hiện hành là bản ghi mới nhất của lich_su_gia_tri; ảnh danh mục nằm ở luu_tru.anh_tai_len.';
COMMENT ON COLUMN danh_muc.thiet_bi.ma_thiet_bi IS 'Khóa chính, mã thiết bị in chìm (khóa nghiệp vụ, duy nhất); bất biến sau khi tạo.';
COMMENT ON COLUMN danh_muc.thiet_bi.ma_loai IS 'Loại thiết bị.';
COMMENT ON COLUMN danh_muc.thiet_bi.ma_phong IS 'Phòng lưu giữ thiết bị.';
COMMENT ON COLUMN danh_muc.thiet_bi.ten_thiet_bi IS 'Tên thiết bị.';
COMMENT ON COLUMN danh_muc.thiet_bi.mo_ta IS 'Mô tả.';
COMMENT ON COLUMN danh_muc.thiet_bi.thong_so_ky_thuat IS 'Thông số kỹ thuật.';
COMMENT ON COLUMN danh_muc.thiet_bi.ngay_nhap_kho IS 'Ngày nhập kho (mốc tính hạn bảo trì khi chưa có lần bảo trì định kỳ).';
COMMENT ON COLUMN danh_muc.thiet_bi.thoi_han_bao_hanh IS 'Ngày hết hạn bảo hành; rỗng nếu không có bảo hành.';
COMMENT ON COLUMN danh_muc.thiet_bi.tinh_trang IS 'Tình trạng hiện tại (5 giá trị); chỉ đổi qua máy trạng thái ở Domain.';
COMMENT ON COLUMN danh_muc.thiet_bi.vi_tri_ke IS 'Kệ lưu trữ.';
COMMENT ON COLUMN danh_muc.thiet_bi.vi_tri_tu IS 'Tủ lưu trữ.';
COMMENT ON COLUMN danh_muc.thiet_bi.ngung_su_dung IS 'Cờ ngưng sử dụng; thiết bị ngưng sử dụng không tính vào số lượng khả dụng.';
COMMENT ON TABLE danh_muc.lich_su_gia_tri IS 'DM-11: Lịch sử giá trị thiết bị, chỉ thêm. Giá trị hiện hành là dòng có ngay_cap_nhat mới nhất; dùng tính bồi thường.';
COMMENT ON COLUMN danh_muc.lich_su_gia_tri.ma_lich_su IS 'Khóa chính (uuid v7), cũng cho thứ tự thời gian ổn định.';
COMMENT ON COLUMN danh_muc.lich_su_gia_tri.ma_thiet_bi IS 'Thiết bị.';
COMMENT ON COLUMN danh_muc.lich_su_gia_tri.gia_tri IS 'Giá trị (VNĐ), không âm.';
COMMENT ON COLUMN danh_muc.lich_su_gia_tri.ngay_cap_nhat IS 'Thời điểm QT cập nhật giá trị.';
COMMENT ON TABLE danh_muc.thiet_bi_nha_cung_cap IS '(quan hệ 9): Bảng nối n-n thiết bị - nhà cung cấp.';
COMMENT ON COLUMN danh_muc.thiet_bi_nha_cung_cap.ma_thiet_bi IS 'Thiết bị.';
COMMENT ON COLUMN danh_muc.thiet_bi_nha_cung_cap.ma_ncc IS 'Nhà cung cấp.';
COMMENT ON TABLE danh_muc.loai_thiet_bi_nha_cung_cap IS '(quan hệ 10): Bảng nối n-n loại thiết bị - nhà cung cấp.';
COMMENT ON COLUMN danh_muc.loai_thiet_bi_nha_cung_cap.ma_loai IS 'Loại thiết bị.';
COMMENT ON COLUMN danh_muc.loai_thiet_bi_nha_cung_cap.ma_ncc IS 'Nhà cung cấp.';
COMMENT ON TABLE quy_dinh.tham_so_he_thong IS 'DM-07: Bộ tham số dùng chung, một dòng duy nhất; thay đổi chỉ áp dụng cho giao dịch phát sinh sau (không hồi tố).';
COMMENT ON COLUMN quy_dinh.tham_so_he_thong.id IS 'Luôn bằng 1.';
COMMENT ON COLUMN quy_dinh.tham_so_he_thong.so_ngay_khoa IS 'Số ngày quá hạn vượt quá thì khóa quyền mượn mới (soNgayKhoa).';
COMMENT ON COLUMN quy_dinh.tham_so_he_thong.thoi_gian_giu_cho_phut IS 'Thời gian giữ chỗ (phút) (thoiGianGiuChoPhut).';
COMMENT ON COLUMN quy_dinh.tham_so_he_thong.an_han_phut IS 'Ân hạn sau hạn trả (phút) (anHanPhut).';
COMMENT ON COLUMN quy_dinh.tham_so_he_thong.so_gio_gia_han_toi_da IS 'Số giờ gia hạn tối đa (soGioGiaHanToiDa).';
COMMENT ON COLUMN quy_dinh.tham_so_he_thong.nguong_vi_pham IS 'Ngưỡng vi phạm mỗi học kỳ (nguongViPham).';
COMMENT ON COLUMN quy_dinh.tham_so_he_thong.so_lan_dang_nhap_sai_toi_da IS 'Số lần nhập sai mật khẩu tối đa của NV/QT, lớn hơn 0 (ĐX-02).';
COMMENT ON COLUMN quy_dinh.tham_so_he_thong.gio_mo_cua IS 'Giờ mở cửa quầy; rỗng = chưa cấu hình (không có mặc định, ĐX-03).';
COMMENT ON COLUMN quy_dinh.tham_so_he_thong.gio_dong_cua IS 'Giờ đóng cửa quầy; rỗng = chưa cấu hình (không có mặc định, ĐX-03).';
COMMENT ON TABLE quy_dinh.khung_phat IS 'DM-21: Khung phạt hư hỏng theo % giá trị hiện hành. Khung đã dùng chỉ được tắt (dang_su_dung = false), không xóa.';
COMMENT ON COLUMN quy_dinh.khung_phat.ma_khung IS 'Khóa chính (uuid v7).';
COMMENT ON COLUMN quy_dinh.khung_phat.ten_khung IS 'Tên khung (mặc định Nhẹ, Vừa, Nặng).';
COMMENT ON COLUMN quy_dinh.khung_phat.ty_le_phan_tram IS 'Tỷ lệ bồi thường (% giá trị hiện hành), trong khoảng (0, 100].';
COMMENT ON COLUMN quy_dinh.khung_phat.dang_su_dung IS 'Cờ bật/tắt; không phải dấu hiệu ''đã dùng''.';
COMMENT ON TABLE quy_dinh.quy_dinh_muon IS 'DM-06: Quy định mượn theo nhóm SV. Dòng chung (ma_loai rỗng): số thiết bị tối đa (tính trên tổng) và thời gian tối đa. Dòng ngoại lệ theo loại: chỉ đặt thời gian.';
COMMENT ON COLUMN quy_dinh.quy_dinh_muon.ma_quy_dinh IS 'Khóa chính (uuid v7).';
COMMENT ON COLUMN quy_dinh.quy_dinh_muon.ma_nhom IS 'Nhóm sinh viên áp dụng.';
COMMENT ON COLUMN quy_dinh.quy_dinh_muon.ma_loai IS 'Rỗng = quy định chung của nhóm; có giá trị = ngoại lệ theo loại thiết bị.';
COMMENT ON COLUMN quy_dinh.quy_dinh_muon.so_thiet_bi_toi_da IS 'Số thiết bị tối đa mượn cùng lúc (tính trên tổng). Chỉ có ở dòng chung.';
COMMENT ON COLUMN quy_dinh.quy_dinh_muon.thoi_gian_muon_toi_da_gio IS 'Thời gian mượn tối đa (giờ).';
COMMENT ON TABLE muon_tra.phieu_yeu_cau_muon IS 'DM-13: Phiếu yêu cầu mượn (đặt trước). Chờ duyệt: chưa giữ chỗ, chưa gán thiết bị. Duyệt thì giữ chỗ đến han_giu_cho.';
COMMENT ON COLUMN muon_tra.phieu_yeu_cau_muon.ma_yeu_cau IS 'Khóa chính, mã chứng từ dạng YC-yyMMdd-nnnnnn do sequence sinh.';
COMMENT ON COLUMN muon_tra.phieu_yeu_cau_muon.ma_sv IS 'SV gửi phiếu.';
COMMENT ON COLUMN muon_tra.phieu_yeu_cau_muon.ma_phong IS 'Phòng thiết bị nhận yêu cầu.';
COMMENT ON COLUMN muon_tra.phieu_yeu_cau_muon.ma_nv_duyet IS 'NV duyệt hoặc từ chối.';
COMMENT ON COLUMN muon_tra.phieu_yeu_cau_muon.thoi_gian_gui IS 'Thời điểm SV gửi.';
COMMENT ON COLUMN muon_tra.phieu_yeu_cau_muon.thoi_gian_muon_du_kien_gio IS 'Thời gian mượn dự kiến (giờ).';
COMMENT ON COLUMN muon_tra.phieu_yeu_cau_muon.trang_thai IS 'Trạng thái phiếu (6 giá trị).';
COMMENT ON COLUMN muon_tra.phieu_yeu_cau_muon.ly_do_tu_choi IS 'Lý do từ chối, bắt buộc khi TU_CHOI.';
COMMENT ON COLUMN muon_tra.phieu_yeu_cau_muon.thoi_diem_duyet IS 'Thời điểm duyệt.';
COMMENT ON COLUMN muon_tra.phieu_yeu_cau_muon.han_giu_cho IS 'Hạn giữ chỗ lưu lúc duyệt (= thời điểm duyệt + thoiGianGiuChoPhut), không đổi khi tham số đổi sau.';
COMMENT ON TABLE muon_tra.chi_tiet_yeu_cau_muon IS 'DM-14: Một dòng của phiếu yêu cầu: loại thiết bị và số lượng (một dòng cho mỗi loại).';
COMMENT ON COLUMN muon_tra.chi_tiet_yeu_cau_muon.ma_yeu_cau IS 'Phiếu yêu cầu.';
COMMENT ON COLUMN muon_tra.chi_tiet_yeu_cau_muon.ma_loai IS 'Loại thiết bị yêu cầu.';
COMMENT ON COLUMN muon_tra.chi_tiet_yeu_cau_muon.so_luong IS 'Số lượng, lớn hơn 0.';
COMMENT ON TABLE muon_tra.dang_ky_cho IS 'DM-15: Đăng ký chờ một loại thiết bị đã hết tại một phòng; xếp theo thứ tự đăng ký trước. Mỗi đăng ký giữ một thiết bị khi đến lượt.';
COMMENT ON COLUMN muon_tra.dang_ky_cho.ma_dang_ky IS 'Khóa chính, mã chứng từ DK-yyMMdd-nnnnnn.';
COMMENT ON COLUMN muon_tra.dang_ky_cho.ma_sv IS 'SV đăng ký.';
COMMENT ON COLUMN muon_tra.dang_ky_cho.ma_phong IS 'Phòng thiết bị (danh sách chờ theo từng loại tại từng phòng).';
COMMENT ON COLUMN muon_tra.dang_ky_cho.ma_loai IS 'Loại thiết bị được chờ.';
COMMENT ON COLUMN muon_tra.dang_ky_cho.thoi_diem_dang_ky IS 'Thời điểm đăng ký; thứ tự phục vụ theo cột này.';
COMMENT ON COLUMN muon_tra.dang_ky_cho.so_thu_tu_cho IS 'Số thứ tự chờ để hiển thị; cập nhật khi người phía trước rời. Bắt buộc khi DANG_CHO.';
COMMENT ON COLUMN muon_tra.dang_ky_cho.trang_thai IS 'Trạng thái đăng ký (5 giá trị).';
COMMENT ON COLUMN muon_tra.dang_ky_cho.han_giu_cho IS 'Hạn giữ chỗ lưu lúc đến lượt (thông báo); rỗng khi còn đang chờ.';
COMMENT ON TABLE muon_tra.phieu_muon IS 'DM-16: Phiếu mượn lập khi giao thiết bị; hạn trả, gia hạn và việc trả tính cho cả phiếu. Hạn trả lưu tại phiếu (không hồi tố).';
COMMENT ON COLUMN muon_tra.phieu_muon.ma_phieu IS 'Khóa chính, mã chứng từ PM-yyMMdd-nnnnnn.';
COMMENT ON COLUMN muon_tra.phieu_muon.ma_sv IS 'SV mượn.';
COMMENT ON COLUMN muon_tra.phieu_muon.ma_nv_lap IS 'NV lập phiếu (giao thiết bị).';
COMMENT ON COLUMN muon_tra.phieu_muon.ma_yeu_cau IS 'Phiếu yêu cầu dẫn đến phiếu mượn (nguồn giao theo phiếu yêu cầu).';
COMMENT ON COLUMN muon_tra.phieu_muon.ma_dang_ky IS 'Đăng ký chờ dẫn đến phiếu mượn (nguồn giao từ danh sách chờ).';
COMMENT ON COLUMN muon_tra.phieu_muon.ngay_gio_muon IS 'Giờ giao thực tế.';
COMMENT ON COLUMN muon_tra.phieu_muon.ngay_gio_hen_tra IS 'Hạn trả của cả phiếu = min(giờ giao + thời gian mượn, giờ đóng cửa cùng ngày).';
COMMENT ON COLUMN muon_tra.phieu_muon.so_lan_gia_han IS 'Số lần đã gia hạn (tối đa 1).';
COMMENT ON COLUMN muon_tra.phieu_muon.trang_thai IS 'Đang mượn / Quá hạn / Đã trả.';
COMMENT ON TABLE muon_tra.chi_tiet_phieu_muon IS 'DM-17: Từng thiết bị trong phiếu mượn: tình trạng lúc giao, phí trễ/ngày lưu lúc giao, kết quả kiểm tra lúc trả. Ảnh lúc giao/trả ở luu_tru.anh_tai_len.';
COMMENT ON COLUMN muon_tra.chi_tiet_phieu_muon.ma_chi_tiet IS 'Khóa chính (uuid v7); làm doi_tuong_id cho ảnh đính kèm.';
COMMENT ON COLUMN muon_tra.chi_tiet_phieu_muon.ma_phieu IS 'Phiếu mượn.';
COMMENT ON COLUMN muon_tra.chi_tiet_phieu_muon.ma_thiet_bi IS 'Thiết bị được giao.';
COMMENT ON COLUMN muon_tra.chi_tiet_phieu_muon.tinh_trang_ban_dau IS 'Mô tả tình trạng lúc giao (bắt buộc).';
COMMENT ON COLUMN muon_tra.chi_tiet_phieu_muon.phi_tre_moi_ngay IS 'Phí trễ/ngày của loại thiết bị tại thời điểm giao; cấu hình đổi sau không ảnh hưởng.';
COMMENT ON COLUMN muon_tra.chi_tiet_phieu_muon.tinh_trang_khi_tra IS 'Mô tả tình trạng lúc trả; bắt buộc khi hư hỏng hoặc thiếu phụ kiện.';
COMMENT ON COLUMN muon_tra.chi_tiet_phieu_muon.ket_qua_kiem_tra IS 'Nguyên vẹn / Hư hỏng / Thiếu phụ kiện / Mất; rỗng trước khi trả.';
COMMENT ON COLUMN muon_tra.chi_tiet_phieu_muon.ngay_gio_tra_thuc_te IS 'Giờ trả thực tế; rỗng với kết quả Mất.';
COMMENT ON COLUMN muon_tra.chi_tiet_phieu_muon.ma_nv_nhan_tra IS 'NV nhận trả (NV đang đăng nhập), ghi khi có kết quả kiểm tra.';
COMMENT ON COLUMN muon_tra.chi_tiet_phieu_muon.ma_khung IS 'Khung phạt NV chọn khi hư hỏng hoặc thiếu phụ kiện.';
COMMENT ON TABLE muon_tra.phan_hoi_tinh_trang IS 'DM-24: Phản hồi của SV về tình trạng thiết bị sau khi bàn giao tại quầy (UC-21, giai đoạn sau; bảng tạo sẵn, trống ở giai đoạn 1).';
COMMENT ON COLUMN muon_tra.phan_hoi_tinh_trang.ma_phan_hoi IS 'Khóa chính (uuid v7).';
COMMENT ON COLUMN muon_tra.phan_hoi_tinh_trang.ma_chi_tiet IS 'Chi tiết phiếu mượn (gắn mã thiết bị và phiếu mượn).';
COMMENT ON COLUMN muon_tra.phan_hoi_tinh_trang.ma_sv IS 'SV gửi phản hồi.';
COMMENT ON COLUMN muon_tra.phan_hoi_tinh_trang.noi_dung IS 'Nội dung phản hồi.';
COMMENT ON COLUMN muon_tra.phan_hoi_tinh_trang.thoi_diem_gui IS 'Thời điểm gửi.';
COMMENT ON TABLE vi_pham.bien_ban_vi_pham IS 'DM-19: Biên bản vi phạm do NV lập (trả trễ, hư hỏng, mất). Số tiền không đổi sau khi lập; chỉ QT được hủy hoặc điều chỉnh.';
COMMENT ON COLUMN vi_pham.bien_ban_vi_pham.ma_bien_ban IS 'Khóa chính, mã chứng từ BB-yyMMdd-nnnnnn.';
COMMENT ON COLUMN vi_pham.bien_ban_vi_pham.ma_phieu_muon IS 'Phiếu mượn phát sinh biên bản.';
COMMENT ON COLUMN vi_pham.bien_ban_vi_pham.ma_nv_lap IS 'NV lập biên bản.';
COMMENT ON COLUMN vi_pham.bien_ban_vi_pham.ngay_lap IS 'Thời điểm lập.';
COMMENT ON COLUMN vi_pham.bien_ban_vi_pham.tong_tien IS 'Tổng tiền (VNĐ) = tổng so_tien các chi tiết; được kiểm tra khi commit.';
COMMENT ON COLUMN vi_pham.bien_ban_vi_pham.trang_thai IS 'Chưa nộp / Đã nộp / Đã hủy.';
COMMENT ON COLUMN vi_pham.bien_ban_vi_pham.thoi_diem_nop IS 'Thời điểm NV ghi nhận đã nộp.';
COMMENT ON TABLE vi_pham.chi_tiet_bien_ban IS 'DM-20: Khoản tiền theo từng thiết bị của biên bản. Thiếu phụ kiện được xử lý như hư hỏng (HU_HONG).';
COMMENT ON COLUMN vi_pham.chi_tiet_bien_ban.ma_bien_ban IS 'Biên bản.';
COMMENT ON COLUMN vi_pham.chi_tiet_bien_ban.ma_phieu_muon IS 'Phiếu mượn (sao chép từ biên bản, đảm bảo bằng khóa ngoại ghép) để chống lập trùng khoản.';
COMMENT ON COLUMN vi_pham.chi_tiet_bien_ban.ma_thiet_bi IS 'Thiết bị (phải thuộc phiếu mượn, khóa ngoại ghép).';
COMMENT ON COLUMN vi_pham.chi_tiet_bien_ban.loai_vi_pham IS 'Trả trễ / Hư hỏng / Mất.';
COMMENT ON COLUMN vi_pham.chi_tiet_bien_ban.so_ngay_tre IS 'Số ngày trễ (>= 1), chỉ có với TRA_TRE.';
COMMENT ON COLUMN vi_pham.chi_tiet_bien_ban.so_tien IS 'Số tiền của khoản (VNĐ).';
COMMENT ON COLUMN vi_pham.chi_tiet_bien_ban.ma_khung IS 'Khung phạt áp dụng, bắt buộc với HU_HONG.';
COMMENT ON COLUMN vi_pham.chi_tiet_bien_ban.bien_ban_da_huy IS 'Bản sao trạng thái hủy của biên bản; do trigger quản lý, không ghi từ ứng dụng.';
COMMENT ON TABLE vi_pham.vi_pham IS 'DM-18: Bản ghi mỗi lần vi phạm để thống kê và đếm theo học kỳ. Mỗi dòng gắn đúng một chứng từ phát sinh.';
COMMENT ON COLUMN vi_pham.vi_pham.ma_vi_pham IS 'Khóa chính (uuid v7).';
COMMENT ON COLUMN vi_pham.vi_pham.ma_sv IS 'SV bị ghi nhận.';
COMMENT ON COLUMN vi_pham.vi_pham.loai_vi_pham IS 'Trả trễ / Hư hỏng / Mất / Không đến nhận.';
COMMENT ON COLUMN vi_pham.vi_pham.thoi_diem_ghi_nhan IS 'Thời điểm ghi nhận.';
COMMENT ON COLUMN vi_pham.vi_pham.hoc_ky IS 'Học kỳ của SV tại thời điểm ghi nhận (sao chép từ sinh_vien.hoc_ky).';
COMMENT ON COLUMN vi_pham.vi_pham.ma_bien_ban IS 'Biên bản (với Trả trễ, Hư hỏng, Mất).';
COMMENT ON COLUMN vi_pham.vi_pham.ma_yeu_cau IS 'Phiếu yêu cầu hết hạn giữ chỗ (Không đến nhận).';
COMMENT ON COLUMN vi_pham.vi_pham.ma_dang_ky IS 'Đăng ký chờ hết hạn giữ chỗ (Không đến nhận).';
COMMENT ON TABLE bao_tri.bao_tri_sua_chua IS 'DM-22: Lịch và kết quả bảo trì định kỳ hoặc sửa chữa. Bản ghi còn mở (ket_qua rỗng) = đang thực hiện.';
COMMENT ON COLUMN bao_tri.bao_tri_sua_chua.ma_bao_tri IS 'Khóa chính (uuid v7).';
COMMENT ON COLUMN bao_tri.bao_tri_sua_chua.ma_thiet_bi IS 'Thiết bị được bảo trì/sửa.';
COMMENT ON COLUMN bao_tri.bao_tri_sua_chua.ma_nv_thuc_hien IS 'NV thực hiện.';
COMMENT ON COLUMN bao_tri.bao_tri_sua_chua.ma_phieu_muon IS 'Phiếu mượn có thiết bị hư hỏng phát sinh sửa chữa.';
COMMENT ON COLUMN bao_tri.bao_tri_sua_chua.loai_thuc_hien IS 'Bảo trì định kỳ / Sửa chữa.';
COMMENT ON COLUMN bao_tri.bao_tri_sua_chua.ngay_lap IS 'Thời điểm lập lịch hoặc tiếp nhận.';
COMMENT ON COLUMN bao_tri.bao_tri_sua_chua.noi_dung_thuc_hien IS 'Nội dung thực hiện, ghi khi hoàn thành.';
COMMENT ON COLUMN bao_tri.bao_tri_sua_chua.ket_qua IS 'Đưa về sẵn sàng / Đề xuất thanh lý; rỗng khi đang thực hiện.';
COMMENT ON COLUMN bao_tri.bao_tri_sua_chua.ngay_hoan_thanh IS 'Thời điểm hoàn thành; mốc tính hạn bảo trì kế tiếp.';
COMMENT ON TABLE bao_tri.quyet_dinh_thanh_ly IS 'DM-23: Quyết định thanh lý của một đợt: mã số riêng, ngày thanh lý, QT lập; danh sách thiết bị ở chi_tiet_thanh_ly.';
COMMENT ON COLUMN bao_tri.quyet_dinh_thanh_ly.ma_quyet_dinh IS 'Khóa chính, mã chứng từ QD-yyMMdd-nnnnnn.';
COMMENT ON COLUMN bao_tri.quyet_dinh_thanh_ly.ma_qt_lap IS 'QT lập quyết định.';
COMMENT ON COLUMN bao_tri.quyet_dinh_thanh_ly.ngay_thanh_ly IS 'Ngày thanh lý.';
COMMENT ON TABLE bao_tri.chi_tiet_thanh_ly IS '(quan hệ 45): Bảng nối quyết định - thiết bị; mỗi thiết bị tối đa một quyết định thanh lý.';
COMMENT ON COLUMN bao_tri.chi_tiet_thanh_ly.ma_quyet_dinh IS 'Quyết định thanh lý.';
COMMENT ON COLUMN bao_tri.chi_tiet_thanh_ly.ma_thiet_bi IS 'Thiết bị được thanh lý.';
COMMENT ON TABLE thong_bao.thong_bao IS 'DM-25: Thông báo kiêm outbox: ghi cùng giao dịch nghiệp vụ, job GuiEmail gửi sau. Mỗi sự kiện sinh một dòng cho mỗi kênh; mỗi dòng đúng một người nhận.';
COMMENT ON COLUMN thong_bao.thong_bao.ma_thong_bao IS 'Khóa chính (uuid v7).';
COMMENT ON COLUMN thong_bao.thong_bao.loai_thong_bao IS 'Loại thông báo (7 giá trị).';
COMMENT ON COLUMN thong_bao.thong_bao.noi_dung IS 'Nội dung (chỉ thông tin cần thiết).';
COMMENT ON COLUMN thong_bao.thong_bao.kenh_gui IS 'Ứng dụng / Email / SMS (giai đoạn 1 không dùng SMS).';
COMMENT ON COLUMN thong_bao.thong_bao.thoi_diem_gui IS 'Thời điểm phát sinh thông báo (dùng sắp xếp danh sách cho SV); thời điểm gửi email thực tế là lan_thu_cuoi khi DA_GUI.';
COMMENT ON COLUMN thong_bao.thong_bao.ma_sv_nhan IS 'SV nhận (khóa ngoại rỗng được).';
COMMENT ON COLUMN thong_bao.thong_bao.ma_nv_nhan IS 'NV nhận (khóa ngoại rỗng được).';
COMMENT ON COLUMN thong_bao.thong_bao.trang_thai_gui IS 'Trạng thái outbox (5 giá trị).';
COMMENT ON COLUMN thong_bao.thong_bao.so_lan_thu IS 'Số lần thử gửi (tối đa 5 lần, giãn cách 1, 5, 15, 60 phút).';
COMMENT ON COLUMN thong_bao.thong_bao.lan_thu_cuoi IS 'Thời điểm thử gần nhất.';
COMMENT ON COLUMN thong_bao.thong_bao.khoa_trung_lap IS 'Khóa chống trùng của thông báo tự động, dạng {LOAI}:{maDoiTuong}[:{moc}]; rỗng với nhắc nhở thủ công.';
COMMENT ON COLUMN thong_bao.thong_bao.doi_tuong_loai IS 'Loại đối tượng nguồn phát sinh (giá trị do ứng dụng quy định).';
COMMENT ON COLUMN thong_bao.thong_bao.doi_tuong_id IS 'Mã đối tượng nguồn phát sinh.';
COMMENT ON TABLE nhat_ky.nhat_ky_he_thong IS 'DM-26: Nhật ký kiểm toán chỉ thêm: trigger chặn UPDATE, DELETE, TRUNCATE; role ứng dụng bị thu hồi quyền. Cả hai cột người thực hiện rỗng = Hệ thống tự động.';
COMMENT ON COLUMN nhat_ky.nhat_ky_he_thong.ma_nhat_ky IS 'Khóa chính (uuid v7).';
COMMENT ON COLUMN nhat_ky.nhat_ky_he_thong.thoi_diem IS 'Thời điểm thao tác.';
COMMENT ON COLUMN nhat_ky.nhat_ky_he_thong.ma_nguoi_dung IS 'NV hoặc QT thực hiện.';
COMMENT ON COLUMN nhat_ky.nhat_ky_he_thong.ma_sv IS 'SV thực hiện.';
COMMENT ON COLUMN nhat_ky.nhat_ky_he_thong.thao_tac IS 'Thao tác (giá trị do ứng dụng quy định).';
COMMENT ON COLUMN nhat_ky.nhat_ky_he_thong.doi_tuong IS 'Đối tượng bị tác động.';
COMMENT ON COLUMN nhat_ky.nhat_ky_he_thong.doi_tuong_id IS 'Mã đối tượng bị tác động.';
COMMENT ON COLUMN nhat_ky.nhat_ky_he_thong.gia_tri_truoc IS 'Giá trị trước (JSON), đã loại cột nhạy cảm.';
COMMENT ON COLUMN nhat_ky.nhat_ky_he_thong.gia_tri_sau IS 'Giá trị sau (JSON), đã loại cột nhạy cảm.';
COMMENT ON TABLE luu_tru.anh_tai_len IS '(KT 5.5): Siêu dữ liệu ảnh (tệp ở Supabase Storage). Ảnh chưa gắn đối tượng sau 24 giờ bị job DonAnhMoCo xóa; ảnh của phiếu đã lập không xóa.';
COMMENT ON COLUMN luu_tru.anh_tai_len.ma_anh IS 'Khóa chính (uuid v7); client nhận về dưới tên maAnh.';
COMMENT ON COLUMN luu_tru.anh_tai_len.bucket IS 'thiet-bi-anh (công khai chỉ đọc) hoặc tinh-trang-anh (riêng tư).';
COMMENT ON COLUMN luu_tru.anh_tai_len.khoa_luu_tru IS 'Khóa tệp trong Storage do server sinh ({loai}/{yyyy}/{mm}/{uuid}).';
COMMENT ON COLUMN luu_tru.anh_tai_len.loai_noi_dung IS 'MIME đã kiểm tra magic bytes: jpeg, png, webp.';
COMMENT ON COLUMN luu_tru.anh_tai_len.kich_thuoc IS 'Kích thước (byte), tối đa 2 MB sau nén.';
COMMENT ON COLUMN luu_tru.anh_tai_len.nguoi_tai_len IS 'NV hoặc QT tải lên.';
COMMENT ON COLUMN luu_tru.anh_tai_len.thoi_diem IS 'Thời điểm tải lên.';
COMMENT ON COLUMN luu_tru.anh_tai_len.doi_tuong_loai IS 'THIET_BI hoặc CHI_TIET_PHIEU_MUON; rỗng khi chưa gắn.';
COMMENT ON COLUMN luu_tru.anh_tai_len.doi_tuong_id IS 'ma_thiet_bi (THIET_BI) hoặc ma_chi_tiet dạng chuỗi (CHI_TIET_PHIEU_MUON).';
COMMENT ON COLUMN luu_tru.anh_tai_len.giai_doan IS 'BAN_DAU, KHI_TRA hoặc THIET_BI; rỗng khi chưa gắn.';
COMMENT ON TABLE truong_gia_lap.nhom_sinh_vien_truong IS '(KT-05): Phía nhà trường mô phỏng: nhóm sinh viên.';
COMMENT ON COLUMN truong_gia_lap.nhom_sinh_vien_truong.ma_nhom IS 'Khóa chính, mã nhóm.';
COMMENT ON COLUMN truong_gia_lap.nhom_sinh_vien_truong.ten_nhom IS 'Tên nhóm.';
COMMENT ON TABLE truong_gia_lap.sinh_vien_truong IS '(KT-05): Phía nhà trường mô phỏng: SV mẫu kèm mật khẩu băm (chỉ nằm ở đây, không có ở schema ứng dụng).';
COMMENT ON COLUMN truong_gia_lap.sinh_vien_truong.ma_sv IS 'Khóa chính, MSSV.';
COMMENT ON COLUMN truong_gia_lap.sinh_vien_truong.mat_khau_bam IS 'Mật khẩu SV mẫu đã băm.';
COMMENT ON COLUMN truong_gia_lap.sinh_vien_truong.ho_ten IS 'Họ tên.';
COMMENT ON COLUMN truong_gia_lap.sinh_vien_truong.lop IS 'Lớp.';
COMMENT ON COLUMN truong_gia_lap.sinh_vien_truong.khoa IS 'Khoa.';
COMMENT ON COLUMN truong_gia_lap.sinh_vien_truong.email IS 'Email.';
COMMENT ON COLUMN truong_gia_lap.sinh_vien_truong.ma_nhom IS 'Nhóm sinh viên.';
COMMENT ON COLUMN truong_gia_lap.sinh_vien_truong.han_the IS 'Hạn thẻ.';
COMMENT ON COLUMN truong_gia_lap.sinh_vien_truong.trang_thai_hoc IS 'Trạng thái học.';
COMMENT ON COLUMN truong_gia_lap.sinh_vien_truong.hoc_ky IS 'Học kỳ.';
COMMENT ON COLUMN truong_gia_lap.sinh_vien_truong.cap_nhat_luc IS 'Thời điểm thay đổi gần nhất (trigger tự đặt) để LayThayDoiTuAsync kéo thay đổi.';

-- 9. Bao mat co so du lieu (KT 5.1, Conventions 7.5; DX-DB-09)
-- 9.1 Thu hoi quyen cua anon/authenticated (neu ton tai, tren Supabase) va cua PUBLIC
DO $$
DECLARE
  v_role text;
  v_schema text;
BEGIN
  FOREACH v_schema IN ARRAY ARRAY['tai_khoan', 'sinh_vien', 'danh_muc', 'quy_dinh', 'muon_tra', 'vi_pham', 'bao_tri', 'thong_bao', 'nhat_ky', 'luu_tru', 'truong_gia_lap'] LOOP
    EXECUTE format('REVOKE ALL ON SCHEMA %I FROM PUBLIC', v_schema);
  END LOOP;
  FOREACH v_role IN ARRAY ARRAY['anon', 'authenticated'] LOOP
    IF EXISTS (SELECT 1 FROM pg_roles WHERE rolname = v_role) THEN
      FOREACH v_schema IN ARRAY ARRAY['tai_khoan', 'sinh_vien', 'danh_muc', 'quy_dinh', 'muon_tra', 'vi_pham', 'bao_tri', 'thong_bao', 'nhat_ky', 'luu_tru', 'truong_gia_lap'] LOOP
        EXECUTE format('REVOKE ALL ON SCHEMA %I FROM %I', v_schema, v_role);
        EXECUTE format('REVOKE ALL ON ALL TABLES IN SCHEMA %I FROM %I', v_schema, v_role);
        EXECUTE format('REVOKE ALL ON ALL SEQUENCES IN SCHEMA %I FROM %I', v_schema, v_role);
        EXECUTE format('REVOKE ALL ON ALL FUNCTIONS IN SCHEMA %I FROM %I', v_schema, v_role);
      END LOOP;
    END IF;
  END LOOP;
END
$$;

-- 9.2 Quyen toi thieu cho role app_backend (KT 5.1, Conventions 7.5)
GRANT USAGE ON SCHEMA tai_khoan, sinh_vien, danh_muc, quy_dinh, muon_tra, vi_pham, bao_tri, thong_bao, nhat_ky, luu_tru, truong_gia_lap TO app_backend;

GRANT SELECT, INSERT, UPDATE, DELETE ON ALL TABLES IN SCHEMA tai_khoan, sinh_vien, danh_muc, quy_dinh, muon_tra, vi_pham, bao_tri, thong_bao, luu_tru TO app_backend;
GRANT SELECT, INSERT ON ALL TABLES IN SCHEMA nhat_ky TO app_backend;
GRANT SELECT ON ALL TABLES IN SCHEMA truong_gia_lap TO app_backend;
GRANT USAGE, SELECT ON ALL SEQUENCES IN SCHEMA muon_tra, vi_pham, bao_tri TO app_backend;

REVOKE UPDATE, DELETE, TRUNCATE ON nhat_ky.nhat_ky_he_thong FROM app_backend;

ALTER DEFAULT PRIVILEGES IN SCHEMA tai_khoan, sinh_vien, danh_muc, quy_dinh, muon_tra, vi_pham, bao_tri, thong_bao, luu_tru
  GRANT SELECT, INSERT, UPDATE, DELETE ON TABLES TO app_backend;
ALTER DEFAULT PRIVILEGES IN SCHEMA nhat_ky GRANT SELECT, INSERT ON TABLES TO app_backend;
ALTER DEFAULT PRIVILEGES IN SCHEMA truong_gia_lap GRANT SELECT ON TABLES TO app_backend;
ALTER DEFAULT PRIVILEGES IN SCHEMA muon_tra, vi_pham, bao_tri
  GRANT USAGE, SELECT ON SEQUENCES TO app_backend;

-- 9.3 Bat RLS cho moi bang; chi role app_backend co policy (anon/authenticated khong co policy nao)
DO $$
DECLARE
  t record;
BEGIN
  FOR t IN
    SELECT schemaname, tablename FROM pg_tables
    WHERE schemaname IN ('tai_khoan', 'sinh_vien', 'danh_muc', 'quy_dinh', 'muon_tra', 'vi_pham', 'bao_tri', 'thong_bao', 'nhat_ky', 'luu_tru', 'truong_gia_lap')
  LOOP
    EXECUTE format('ALTER TABLE %I.%I ENABLE ROW LEVEL SECURITY', t.schemaname, t.tablename);
    IF t.schemaname = 'nhat_ky' THEN
      EXECUTE format('CREATE POLICY app_backend_doc ON %I.%I AS PERMISSIVE FOR SELECT TO app_backend USING (true)',
                     t.schemaname, t.tablename);
      EXECUTE format('CREATE POLICY app_backend_them ON %I.%I AS PERMISSIVE FOR INSERT TO app_backend WITH CHECK (true)',
                     t.schemaname, t.tablename);
    ELSIF t.schemaname = 'truong_gia_lap' THEN
      EXECUTE format('CREATE POLICY app_backend_doc ON %I.%I AS PERMISSIVE FOR SELECT TO app_backend USING (true)',
                     t.schemaname, t.tablename);
    ELSE
      EXECUTE format('CREATE POLICY app_backend_toan_quyen ON %I.%I AS PERMISSIVE FOR ALL TO app_backend USING (true) WITH CHECK (true)',
                     t.schemaname, t.tablename);
    END IF;
  END LOOP;
END
$$;

-- 10. Du lieu khoi tao bat buoc (KT 5.7, Conventions 7.4)
-- 10.1 Mot dong tham so he thong voi gia tri mac dinh SRS 3.3 (gio_mo_cua, gio_dong_cua de NULL theo DX-03)
INSERT INTO quy_dinh.tham_so_he_thong (id) VALUES (1) ON CONFLICT (id) DO NOTHING;

-- 10.2 Mot dong moc dong bo SV
INSERT INTO sinh_vien.dong_bo_trang_thai (id) VALUES (1) ON CONFLICT (id) DO NOTHING;

-- 10.3 Ba khung phat mac dinh (FR-37)
INSERT INTO quy_dinh.khung_phat (ma_khung, ten_khung, ty_le_phan_tram, dang_su_dung)
SELECT gen_random_uuid(), v.ten, v.ty_le, true
FROM (VALUES ('Nhẹ', 15), ('Vừa', 40), ('Nặng', 80)) AS v(ten, ty_le)
WHERE NOT EXISTS (SELECT 1 FROM quy_dinh.khung_phat k WHERE k.ten_khung = v.ten);

-- 10.4 Ba nhom sinh vien (nha truong cung cap; ma nhom la quyet dinh DX-DB-05) o ca hai phia
INSERT INTO truong_gia_lap.nhom_sinh_vien_truong (ma_nhom, ten_nhom) VALUES
  ('DAI_HOC', 'Sinh viên đại học'),
  ('CAO_HOC', 'Học viên cao học'),
  ('DO_AN_TOT_NGHIEP', 'Sinh viên làm đồ án tốt nghiệp')
ON CONFLICT (ma_nhom) DO NOTHING;

INSERT INTO sinh_vien.nhom_sinh_vien (ma_nhom, ten_nhom)
SELECT ma_nhom, ten_nhom FROM truong_gia_lap.nhom_sinh_vien_truong
ON CONFLICT (ma_nhom) DO NOTHING;

-- 10.5 Quy dinh muon mac dinh theo nhom (FR-20, DX-01): dai hoc 2 thiet bi 5 gio; cao hoc va do an 3 thiet bi 10 gio
INSERT INTO quy_dinh.quy_dinh_muon (ma_quy_dinh, ma_nhom, ma_loai, so_thiet_bi_toi_da, thoi_gian_muon_toi_da_gio)
SELECT gen_random_uuid(), v.ma_nhom, NULL::uuid, v.so_tb, v.gio
FROM (VALUES ('DAI_HOC', 2, 5), ('CAO_HOC', 3, 10), ('DO_AN_TOT_NGHIEP', 3, 10)) AS v(ma_nhom, so_tb, gio)
WHERE NOT EXISTS (SELECT 1 FROM quy_dinh.quy_dinh_muon q WHERE q.ma_nhom = v.ma_nhom AND q.ma_loai IS NULL);

-- Khong seed trong file nay (xem DatabaseDesign.md muc 12):
--  * Tai khoan QT dau tien: ung dung tao tu bien moi truong Seed__AdminUser/Seed__AdminPassword (can bam mat khau bang .NET).
--  * SV mau trong truong_gia_lap.sinh_vien_truong: can mat khau bam bang PasswordHasher, do seed demo cua ung dung tao.
--  * Danh muc thiet bi va muc phi: he thong khong co mac dinh (DX-05).
--  * gio_mo_cua, gio_dong_cua: QT thiet lap truoc khi su dung (DX-03). Chi de DEMO, bo comment dong duoi va DOI gia tri:
-- UPDATE quy_dinh.tham_so_he_thong SET gio_mo_cua = '07:30', gio_dong_cua = '17:00' WHERE id = 1;

COMMIT;
