INSERT INTO sinh_vien.nhom_sinh_vien (ma_nhom, ten_nhom)
VALUES ('D14-NHOM', 'Nhóm D14');

INSERT INTO sinh_vien.sinh_vien (ma_sv, ho_ten, lop, khoa, email, ma_nhom, han_the, trang_thai_hoc, trang_thai_quyen_muon, hoc_ky)
VALUES
    ('D14-SV-A', 'Sinh viên D14 A', 'L1', 'K1', 'd14-a@example.test', 'D14-NHOM', DATE '2030-12-31', 'DANG_HOC', 'BINH_THUONG', '2026-1'),
    ('D14-SV-B', 'Sinh viên D14 B', 'L1', 'K1', 'd14-b@example.test', 'D14-NHOM', DATE '2030-12-31', 'DANG_HOC', 'BINH_THUONG', '2026-1');

INSERT INTO tai_khoan.nguoi_dung_he_thong (ma_nguoi_dung, ten_dang_nhap, mat_khau_bam, ho_ten)
VALUES
    ('019c0000-0000-7000-8000-0000000d14a1', 'd14-nv1', 'khong-dung-de-dang-nhap', 'Nhân viên D14 1'),
    ('019c0000-0000-7000-8000-0000000d14a2', 'd14-nv2', 'khong-dung-de-dang-nhap', 'Nhân viên D14 2');

INSERT INTO tai_khoan.nhan_vien_quan_ly (ma_nguoi_dung, ma_nv)
VALUES
    ('019c0000-0000-7000-8000-0000000d14a1', 'D14-NV1'),
    ('019c0000-0000-7000-8000-0000000d14a2', 'D14-NV2');

INSERT INTO danh_muc.phong_thiet_bi (ma_phong, ten_phong, co_so)
VALUES
    ('019c0000-0000-7000-8000-0000000d14b1', 'D14 Phong P1', 'D14 Co so'),
    ('019c0000-0000-7000-8000-0000000d14b2', 'D14 Phong P2', 'D14 Co so');

INSERT INTO danh_muc.loai_thiet_bi (ma_loai, ten_loai, chu_ky_bao_tri, phi_tre_moi_ngay, ngung_su_dung)
VALUES
    ('019c0000-0000-7000-8000-0000000d14c1', 'D14 Loai X', 30, 50000, false),
    ('019c0000-0000-7000-8000-0000000d14c2', 'D14 Loai Y', 30, 30000, false);

INSERT INTO quy_dinh.khung_phat (ma_khung, ten_khung, ty_le_phan_tram)
VALUES ('019c0000-0000-7000-8000-0000000d14d1', 'Khung D14 nhẹ', 15);

INSERT INTO danh_muc.thiet_bi (ma_thiet_bi, ma_loai, ma_phong, ten_thiet_bi, ngay_nhap_kho, tinh_trang, ngung_su_dung)
VALUES
    ('D14-TB-T1', '019c0000-0000-7000-8000-0000000d14c1', '019c0000-0000-7000-8000-0000000d14b1', 'T1', DATE '2026-01-01', 'DANG_DUOC_MUON', false),
    ('D14-TB-T2', '019c0000-0000-7000-8000-0000000d14c1', '019c0000-0000-7000-8000-0000000d14b1', 'T2', DATE '2026-01-01', 'DANG_DUOC_MUON', false),
    ('D14-TB-T3', '019c0000-0000-7000-8000-0000000d14c2', '019c0000-0000-7000-8000-0000000d14b1', 'T3', DATE '2026-01-01', 'DANG_DUOC_MUON', false),
    ('D14-TB-T4', '019c0000-0000-7000-8000-0000000d14c2', '019c0000-0000-7000-8000-0000000d14b1', 'T4', DATE '2026-01-01', 'DANG_DUOC_MUON', false),
    ('D14-TB-T5', '019c0000-0000-7000-8000-0000000d14c1', '019c0000-0000-7000-8000-0000000d14b1', 'T5', DATE '2026-01-01', 'DANG_DUOC_MUON', false),
    ('D14-TB-T6', '019c0000-0000-7000-8000-0000000d14c2', '019c0000-0000-7000-8000-0000000d14b2', 'T6', DATE '2026-01-01', 'DANG_DUOC_MUON', false),
    ('D14-TB-T7', '019c0000-0000-7000-8000-0000000d14c1', '019c0000-0000-7000-8000-0000000d14b1', 'T7', DATE '2026-01-01', 'SAN_SANG', false),
    ('D14-TB-T8', '019c0000-0000-7000-8000-0000000d14c1', '019c0000-0000-7000-8000-0000000d14b1', 'T8', DATE '2026-01-01', 'SAN_SANG', false);

INSERT INTO danh_muc.lich_su_gia_tri (ma_lich_su, ma_thiet_bi, gia_tri)
VALUES
    ('019c0000-0000-7000-8000-0000000d14e1', 'D14-TB-T1', 10000000),
    ('019c0000-0000-7000-8000-0000000d14e2', 'D14-TB-T2', 10000000),
    ('019c0000-0000-7000-8000-0000000d14e3', 'D14-TB-T3', 10000000),
    ('019c0000-0000-7000-8000-0000000d14e4', 'D14-TB-T4', 10000000),
    ('019c0000-0000-7000-8000-0000000d14e5', 'D14-TB-T5', 10000000),
    ('019c0000-0000-7000-8000-0000000d14e6', 'D14-TB-T6', 10000000),
    ('019c0000-0000-7000-8000-0000000d14e7', 'D14-TB-T7', 10000000),
    ('019c0000-0000-7000-8000-0000000d14e8', 'D14-TB-T8', 10000000);

INSERT INTO muon_tra.phieu_muon (ma_phieu, ma_sv, ma_nv_lap, ngay_gio_muon, ngay_gio_hen_tra, trang_thai)
VALUES
    ('PM-261126-914001', 'D14-SV-A', '019c0000-0000-7000-8000-0000000d14a1', TIMESTAMPTZ '2026-11-26 01:00:00+00', TIMESTAMPTZ '2026-11-26 09:00:00+00', 'DANG_MUON'),
    ('PM-261126-914002', 'D14-SV-B', '019c0000-0000-7000-8000-0000000d14a1', TIMESTAMPTZ '2026-11-26 01:30:00+00', TIMESTAMPTZ '2026-11-26 09:00:00+00', 'DANG_MUON'),
    ('PM-261120-914003', 'D14-SV-A', '019c0000-0000-7000-8000-0000000d14a1', TIMESTAMPTZ '2026-11-20 01:00:00+00', TIMESTAMPTZ '2026-11-20 09:00:00+00', 'DA_TRA');

INSERT INTO muon_tra.chi_tiet_phieu_muon
    (ma_chi_tiet, ma_phieu, ma_thiet_bi, tinh_trang_ban_dau, phi_tre_moi_ngay, tinh_trang_khi_tra, ket_qua_kiem_tra, ngay_gio_tra_thuc_te, ma_nv_nhan_tra, ma_khung)
VALUES
    ('019c0000-0000-7000-8000-0000000d14f1', 'PM-261126-914001', 'D14-TB-T1', 'Còn tốt', 50000, NULL, NULL, NULL, NULL, NULL),
    ('019c0000-0000-7000-8000-0000000d14f2', 'PM-261126-914001', 'D14-TB-T2', 'Còn tốt', 50000, NULL, NULL, NULL, NULL, NULL),
    ('019c0000-0000-7000-8000-0000000d14f3', 'PM-261126-914001', 'D14-TB-T3', 'Còn tốt', 30000, NULL, NULL, NULL, NULL, NULL),
    ('019c0000-0000-7000-8000-0000000d14f4', 'PM-261126-914001', 'D14-TB-T4', 'Còn tốt', 30000, NULL, NULL, NULL, NULL, NULL),
    ('019c0000-0000-7000-8000-0000000d14f5', 'PM-261126-914002', 'D14-TB-T5', 'Còn tốt', 50000, NULL, NULL, NULL, NULL, NULL),
    ('019c0000-0000-7000-8000-0000000d14f6', 'PM-261126-914002', 'D14-TB-T6', 'Còn tốt', 30000, NULL, NULL, NULL, NULL, NULL),
    ('019c0000-0000-7000-8000-0000000d14f7', 'PM-261120-914003', 'D14-TB-T7', 'Còn tốt', 50000, 'Nguyên vẹn', 'NGUYEN_VEN', TIMESTAMPTZ '2026-11-20 08:00:00+00', '019c0000-0000-7000-8000-0000000d14a1', NULL);
