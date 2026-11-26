INSERT INTO quy_dinh.tham_so_he_thong (id, gio_mo_cua, gio_dong_cua)
VALUES (1, TIME '07:00', TIME '17:00')
ON CONFLICT (id) DO UPDATE SET gio_mo_cua = EXCLUDED.gio_mo_cua, gio_dong_cua = EXCLUDED.gio_dong_cua;

INSERT INTO sinh_vien.nhom_sinh_vien (ma_nhom, ten_nhom)
VALUES ('D13-NHOM', 'Nhóm D13');

INSERT INTO quy_dinh.quy_dinh_muon (ma_quy_dinh, ma_nhom, ma_loai, so_thiet_bi_toi_da, thoi_gian_muon_toi_da_gio)
VALUES ('019c0000-0000-7000-8000-0000000d13d1', 'D13-NHOM', NULL, 4, 5);

INSERT INTO sinh_vien.sinh_vien (ma_sv, ho_ten, lop, khoa, email, ma_nhom, han_the, trang_thai_hoc, trang_thai_quyen_muon, hoc_ky)
VALUES
    ('D13-SV-A', 'Sinh viên D13 A', 'L1', 'K1', 'd13-a@example.test', 'D13-NHOM', DATE '2030-12-31', 'DANG_HOC', 'BINH_THUONG', '2026-1'),
    ('D13-SV-B', 'Sinh viên D13 B', 'L1', 'K1', 'd13-b@example.test', 'D13-NHOM', DATE '2030-12-31', 'DANG_HOC', 'BINH_THUONG', '2026-1'),
    ('D13-SV-C', 'Sinh viên D13 C', 'L1', 'K1', 'd13-c@example.test', 'D13-NHOM', DATE '2030-12-31', 'DANG_HOC', 'BINH_THUONG', '2026-1'),
    ('D13-SV-D', 'Sinh viên D13 D', 'L1', 'K1', 'd13-d@example.test', 'D13-NHOM', DATE '2030-12-31', 'DANG_HOC', 'BINH_THUONG', '2026-1'),
    ('D13-SV-E', 'Sinh viên D13 E', 'L1', 'K1', 'd13-e@example.test', 'D13-NHOM', DATE '2030-12-31', 'DANG_HOC', 'BINH_THUONG', '2026-1'),
    ('D13-SV-F', 'Sinh viên D13 F', 'L1', 'K1', 'd13-f@example.test', 'D13-NHOM', DATE '2030-12-31', 'DANG_HOC', 'BINH_THUONG', '2026-1'),
    ('D13-SV-G', 'Sinh viên D13 G', 'L1', 'K1', 'd13-g@example.test', 'D13-NHOM', DATE '2030-12-31', 'DANG_HOC', 'BINH_THUONG', '2026-1'),
    ('D13-SV-H', 'Sinh viên D13 H', 'L1', 'K1', 'd13-h@example.test', 'D13-NHOM', DATE '2030-12-31', 'DANG_HOC', 'BINH_THUONG', '2026-1'),
    ('D13-SV-I', 'Sinh viên D13 I', 'L1', 'K1', 'd13-i@example.test', 'D13-NHOM', DATE '2030-12-31', 'DANG_HOC', 'BINH_THUONG', '2026-1');

INSERT INTO tai_khoan.nguoi_dung_he_thong (ma_nguoi_dung, ten_dang_nhap, mat_khau_bam, ho_ten)
VALUES ('019c0000-0000-7000-8000-0000000d13a1', 'd13-nv1', 'khong-dung-de-dang-nhap', 'Nhân viên D13 1');

INSERT INTO tai_khoan.nhan_vien_quan_ly (ma_nguoi_dung, ma_nv)
VALUES ('019c0000-0000-7000-8000-0000000d13a1', 'D13-NV1');

INSERT INTO danh_muc.phong_thiet_bi (ma_phong, ten_phong, co_so)
VALUES
    ('019c0000-0000-7000-8000-0000000d13b1', 'D13 Phong P1', 'D13 Co so'),
    ('019c0000-0000-7000-8000-0000000d13b2', 'D13 Phong P2', 'D13 Co so');

INSERT INTO danh_muc.loai_thiet_bi (ma_loai, ten_loai, chu_ky_bao_tri, phi_tre_moi_ngay, ngung_su_dung)
VALUES
    ('019c0000-0000-7000-8000-0000000d13c1', 'D13 Loai X', 30, 50000, false),
    ('019c0000-0000-7000-8000-0000000d13c2', 'D13 Loai Y', 30, 30000, false),
    ('019c0000-0000-7000-8000-0000000d13c3', 'D13 Loai W', 30, 20000, false);

INSERT INTO danh_muc.thiet_bi (ma_thiet_bi, ma_loai, ma_phong, ten_thiet_bi, ngay_nhap_kho, tinh_trang, ngung_su_dung)
VALUES
    ('D13-TB-01', '019c0000-0000-7000-8000-0000000d13c1', '019c0000-0000-7000-8000-0000000d13b1', 'X 01 P1 san sang', DATE '2026-01-01', 'SAN_SANG', false),
    ('D13-TB-02', '019c0000-0000-7000-8000-0000000d13c1', '019c0000-0000-7000-8000-0000000d13b1', 'X 02 P1 dang muon', DATE '2026-01-01', 'DANG_DUOC_MUON', false),
    ('D13-TB-03', '019c0000-0000-7000-8000-0000000d13c1', '019c0000-0000-7000-8000-0000000d13b2', 'X 03 P2 san sang', DATE '2026-01-01', 'SAN_SANG', false),
    ('D13-TB-05', '019c0000-0000-7000-8000-0000000d13c2', '019c0000-0000-7000-8000-0000000d13b1', 'Y 05 P1 dang muon', DATE '2026-01-01', 'DANG_DUOC_MUON', false),
    ('D13-TB-06', '019c0000-0000-7000-8000-0000000d13c3', '019c0000-0000-7000-8000-0000000d13b1', 'W 06 P1 san sang', DATE '2026-01-01', 'SAN_SANG', false),
    ('D13-TB-07', '019c0000-0000-7000-8000-0000000d13c2', '019c0000-0000-7000-8000-0000000d13b2', 'Y 07 P2 san sang', DATE '2026-01-01', 'SAN_SANG', false),
    ('D13-TB-08', '019c0000-0000-7000-8000-0000000d13c3', '019c0000-0000-7000-8000-0000000d13b2', 'W 08 P2 san sang', DATE '2026-01-01', 'SAN_SANG', false);

INSERT INTO danh_muc.lich_su_gia_tri (ma_lich_su, ma_thiet_bi, gia_tri)
VALUES
    ('019c0000-0000-7000-8000-0000000d13e1', 'D13-TB-01', 10000000),
    ('019c0000-0000-7000-8000-0000000d13e2', 'D13-TB-02', 10000000),
    ('019c0000-0000-7000-8000-0000000d13e3', 'D13-TB-03', 10000000),
    ('019c0000-0000-7000-8000-0000000d13e5', 'D13-TB-05', 10000000),
    ('019c0000-0000-7000-8000-0000000d13e6', 'D13-TB-06', 10000000),
    ('019c0000-0000-7000-8000-0000000d13e7', 'D13-TB-07', 10000000),
    ('019c0000-0000-7000-8000-0000000d13e8', 'D13-TB-08', 10000000);

INSERT INTO muon_tra.dang_ky_cho (ma_dang_ky, ma_sv, ma_phong, ma_loai, thoi_diem_dang_ky, so_thu_tu_cho, trang_thai, han_giu_cho)
VALUES
    ('DK-261125-913001', 'D13-SV-B', '019c0000-0000-7000-8000-0000000d13b1', '019c0000-0000-7000-8000-0000000d13c1', TIMESTAMPTZ '2026-11-25 01:00:00+00', 1, 'DANG_CHO', NULL),
    ('DK-261125-913002', 'D13-SV-A', '019c0000-0000-7000-8000-0000000d13b1', '019c0000-0000-7000-8000-0000000d13c1', TIMESTAMPTZ '2026-11-25 01:00:00+00', 2, 'DANG_CHO', NULL),
    ('DK-261125-913003', 'D13-SV-C', '019c0000-0000-7000-8000-0000000d13b1', '019c0000-0000-7000-8000-0000000d13c1', TIMESTAMPTZ '2026-11-25 01:05:00+00', 3, 'DANG_CHO', NULL),
    ('DK-261125-913004', 'D13-SV-D', '019c0000-0000-7000-8000-0000000d13b2', '019c0000-0000-7000-8000-0000000d13c1', TIMESTAMPTZ '2026-11-25 00:30:00+00', 1, 'DANG_CHO', NULL),
    ('DK-261125-913005', 'D13-SV-E', '019c0000-0000-7000-8000-0000000d13b1', '019c0000-0000-7000-8000-0000000d13c2', TIMESTAMPTZ '2026-11-25 01:00:00+00', 1, 'DANG_CHO', NULL),
    ('DK-261125-913006', 'D13-SV-F', '019c0000-0000-7000-8000-0000000d13b1', '019c0000-0000-7000-8000-0000000d13c3', TIMESTAMPTZ '2026-11-25 00:30:00+00', NULL, 'DA_THONG_BAO', TIMESTAMPTZ '2026-11-25 03:10:00+00'),
    ('DK-261125-913007', 'D13-SV-G', '019c0000-0000-7000-8000-0000000d13b1', '019c0000-0000-7000-8000-0000000d13c3', TIMESTAMPTZ '2026-11-25 01:00:00+00', 1, 'DANG_CHO', NULL),
    ('DK-261125-913008', 'D13-SV-H', '019c0000-0000-7000-8000-0000000d13b2', '019c0000-0000-7000-8000-0000000d13c3', TIMESTAMPTZ '2026-11-25 00:30:00+00', NULL, 'DA_THONG_BAO', TIMESTAMPTZ '2026-11-25 03:10:00+00'),
    ('DK-261125-913009', 'D13-SV-E', '019c0000-0000-7000-8000-0000000d13b1', '019c0000-0000-7000-8000-0000000d13c1', TIMESTAMPTZ '2026-11-25 00:10:00+00', NULL, 'DA_HUY', NULL),
    ('DK-261125-913010', 'D13-SV-E', '019c0000-0000-7000-8000-0000000d13b1', '019c0000-0000-7000-8000-0000000d13c1', TIMESTAMPTZ '2026-11-25 00:05:00+00', NULL, 'HET_HAN', TIMESTAMPTZ '2026-11-25 00:40:00+00');
