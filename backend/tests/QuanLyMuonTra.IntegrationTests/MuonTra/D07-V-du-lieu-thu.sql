INSERT INTO sinh_vien.nhom_sinh_vien (ma_nhom, ten_nhom)
VALUES ('D07-NHOM', 'Nhóm D07');

INSERT INTO quy_dinh.quy_dinh_muon (ma_quy_dinh, ma_nhom, ma_loai, so_thiet_bi_toi_da, thoi_gian_muon_toi_da_gio)
VALUES ('019c0000-0000-7000-8000-0000000d7d01', 'D07-NHOM', NULL, 3, 5);

INSERT INTO sinh_vien.sinh_vien (ma_sv, ho_ten, lop, khoa, email, ma_nhom, han_the, trang_thai_hoc, trang_thai_quyen_muon, hoc_ky)
VALUES
    ('D07-SV-A', 'Sinh viên D07 A', 'L1', 'K1', 'd07-a@example.test', 'D07-NHOM', DATE '2030-12-31', 'DANG_HOC', 'BINH_THUONG', '2026-1'),
    ('D07-SV-B', 'Sinh viên D07 B', 'L1', 'K1', 'd07-b@example.test', 'D07-NHOM', DATE '2030-12-31', 'DANG_HOC', 'BINH_THUONG', '2026-1'),
    ('D07-SV-C', 'Sinh viên D07 C', 'L1', 'K1', 'd07-c@example.test', 'D07-NHOM', DATE '2030-12-31', 'DANG_HOC', 'BINH_THUONG', '2026-1'),
    ('D07-SV-D', 'Sinh viên D07 D', 'L1', 'K1', 'd07-d@example.test', 'D07-NHOM', DATE '2030-12-31', 'DANG_HOC', 'BINH_THUONG', '2026-1'),
    ('D07-SV-HET-HAN', 'Sinh viên D07 hết hạn thẻ', 'L1', 'K1', 'd07-hh@example.test', 'D07-NHOM', DATE '2026-11-17', 'DANG_HOC', 'BINH_THUONG', '2026-1'),
    ('D07-SV-TAM-KHOA', 'Sinh viên D07 tạm khóa', 'L1', 'K1', 'd07-tk@example.test', 'D07-NHOM', DATE '2030-12-31', 'DANG_HOC', 'TAM_KHOA', '2026-1');

INSERT INTO tai_khoan.nguoi_dung_he_thong (ma_nguoi_dung, ten_dang_nhap, mat_khau_bam, ho_ten)
VALUES
    ('019c0000-0000-7000-8000-0000000d7a01', 'd07-nv1', 'khong-dung-de-dang-nhap', 'Nhân viên D07 1'),
    ('019c0000-0000-7000-8000-0000000d7a02', 'd07-nv2', 'khong-dung-de-dang-nhap', 'Nhân viên D07 2');

INSERT INTO tai_khoan.nhan_vien_quan_ly (ma_nguoi_dung, ma_nv)
VALUES
    ('019c0000-0000-7000-8000-0000000d7a01', 'D07-NV1'),
    ('019c0000-0000-7000-8000-0000000d7a02', 'D07-NV2');

INSERT INTO danh_muc.phong_thiet_bi (ma_phong, ten_phong, co_so)
VALUES
    ('019c0000-0000-7000-8000-0000000d7b01', 'D07 Phong P1', 'D07 Co so'),
    ('019c0000-0000-7000-8000-0000000d7b02', 'D07 Phong P2', 'D07 Co so');

INSERT INTO danh_muc.loai_thiet_bi (ma_loai, ten_loai, chu_ky_bao_tri, phi_tre_moi_ngay, ngung_su_dung)
VALUES
    ('019c0000-0000-7000-8000-0000000d7c01', 'D07 Loai X', 30, 50000, false),
    ('019c0000-0000-7000-8000-0000000d7c02', 'D07 Loai Y', 30, 30000, false),
    ('019c0000-0000-7000-8000-0000000d7c03', 'D07 Loai Z', 30, 20000, false);

INSERT INTO danh_muc.thiet_bi (ma_thiet_bi, ma_loai, ma_phong, ten_thiet_bi, ngay_nhap_kho, tinh_trang, ngung_su_dung)
VALUES
    ('D07-TB-01', '019c0000-0000-7000-8000-0000000d7c01', '019c0000-0000-7000-8000-0000000d7b01', 'X 01', DATE '2026-01-01', 'SAN_SANG', false),
    ('D07-TB-02', '019c0000-0000-7000-8000-0000000d7c01', '019c0000-0000-7000-8000-0000000d7b01', 'X 02', DATE '2026-01-01', 'SAN_SANG', false),
    ('D07-TB-03', '019c0000-0000-7000-8000-0000000d7c01', '019c0000-0000-7000-8000-0000000d7b01', 'X 03', DATE '2026-01-01', 'SAN_SANG', false),
    ('D07-TB-04', '019c0000-0000-7000-8000-0000000d7c02', '019c0000-0000-7000-8000-0000000d7b01', 'Y 04', DATE '2026-01-01', 'SAN_SANG', false),
    ('D07-TB-05', '019c0000-0000-7000-8000-0000000d7c03', '019c0000-0000-7000-8000-0000000d7b01', 'Z 05', DATE '2026-01-01', 'SAN_SANG', false),
    ('D07-TB-06', '019c0000-0000-7000-8000-0000000d7c01', '019c0000-0000-7000-8000-0000000d7b01', 'X 06 dang muon', DATE '2026-01-01', 'DANG_DUOC_MUON', false),
    ('D07-TB-07', '019c0000-0000-7000-8000-0000000d7c01', '019c0000-0000-7000-8000-0000000d7b01', 'X 07 dang muon', DATE '2026-01-01', 'DANG_DUOC_MUON', false);

INSERT INTO danh_muc.lich_su_gia_tri (ma_lich_su, ma_thiet_bi, gia_tri)
VALUES
    ('019c0000-0000-7000-8000-0000000d7f01', 'D07-TB-01', 10000000),
    ('019c0000-0000-7000-8000-0000000d7f02', 'D07-TB-02', 10000000),
    ('019c0000-0000-7000-8000-0000000d7f03', 'D07-TB-03', 10000000),
    ('019c0000-0000-7000-8000-0000000d7f04', 'D07-TB-04', 10000000),
    ('019c0000-0000-7000-8000-0000000d7f05', 'D07-TB-05', 10000000),
    ('019c0000-0000-7000-8000-0000000d7f06', 'D07-TB-06', 10000000),
    ('019c0000-0000-7000-8000-0000000d7f07', 'D07-TB-07', 10000000);

INSERT INTO muon_tra.phieu_muon (ma_phieu, ma_sv, ma_nv_lap, ngay_gio_muon, ngay_gio_hen_tra, trang_thai)
VALUES ('PM-261118-997001', 'D07-SV-C', '019c0000-0000-7000-8000-0000000d7a01', TIMESTAMPTZ '2026-11-18 01:00:00+00', TIMESTAMPTZ '2026-11-18 06:00:00+00', 'DANG_MUON');

INSERT INTO muon_tra.chi_tiet_phieu_muon (ma_chi_tiet, ma_phieu, ma_thiet_bi, tinh_trang_ban_dau, phi_tre_moi_ngay)
VALUES
    ('019c0000-0000-7000-8000-0000000d7e01', 'PM-261118-997001', 'D07-TB-06', 'Tốt', 50000),
    ('019c0000-0000-7000-8000-0000000d7e02', 'PM-261118-997001', 'D07-TB-07', 'Tốt', 50000);

INSERT INTO muon_tra.phieu_yeu_cau_muon (ma_yeu_cau, ma_sv, ma_phong, thoi_gian_gui, thoi_gian_muon_du_kien_gio, trang_thai)
VALUES
    ('YC-261118-997001', 'D07-SV-A', '019c0000-0000-7000-8000-0000000d7b01', TIMESTAMPTZ '2026-11-18 01:00:00+00', 2, 'CHO_DUYET'),
    ('YC-261118-997002', 'D07-SV-B', '019c0000-0000-7000-8000-0000000d7b01', TIMESTAMPTZ '2026-11-18 01:10:00+00', 2, 'CHO_DUYET'),
    ('YC-261118-997003', 'D07-SV-A', '019c0000-0000-7000-8000-0000000d7b01', TIMESTAMPTZ '2026-11-18 01:20:00+00', 2, 'CHO_DUYET'),
    ('YC-261118-997004', 'D07-SV-D', '019c0000-0000-7000-8000-0000000d7b01', TIMESTAMPTZ '2026-11-18 01:30:00+00', 2, 'CHO_DUYET'),
    ('YC-261118-997005', 'D07-SV-HET-HAN', '019c0000-0000-7000-8000-0000000d7b01', TIMESTAMPTZ '2026-11-18 01:40:00+00', 2, 'CHO_DUYET'),
    ('YC-261118-997006', 'D07-SV-TAM-KHOA', '019c0000-0000-7000-8000-0000000d7b01', TIMESTAMPTZ '2026-11-18 01:50:00+00', 2, 'CHO_DUYET'),
    ('YC-261118-997007', 'D07-SV-C', '019c0000-0000-7000-8000-0000000d7b01', TIMESTAMPTZ '2026-11-18 02:00:00+00', 2, 'CHO_DUYET'),
    ('YC-261118-997008', 'D07-SV-A', '019c0000-0000-7000-8000-0000000d7b01', TIMESTAMPTZ '2026-11-18 02:10:00+00', 8, 'CHO_DUYET'),
    ('YC-261118-997009', 'D07-SV-B', '019c0000-0000-7000-8000-0000000d7b02', TIMESTAMPTZ '2026-11-18 02:20:00+00', 2, 'CHO_DUYET');

INSERT INTO muon_tra.phieu_yeu_cau_muon (ma_yeu_cau, ma_sv, ma_phong, ma_nv_duyet, thoi_gian_gui, thoi_gian_muon_du_kien_gio, trang_thai, thoi_diem_duyet, han_giu_cho)
VALUES ('YC-261118-997010', 'D07-SV-D', '019c0000-0000-7000-8000-0000000d7b01', '019c0000-0000-7000-8000-0000000d7a01', TIMESTAMPTZ '2026-11-18 02:30:00+00', 2, 'DA_DUYET', TIMESTAMPTZ '2026-11-18 02:35:00+00', TIMESTAMPTZ '2026-11-18 04:00:00+00');

INSERT INTO muon_tra.chi_tiet_yeu_cau_muon (ma_yeu_cau, ma_loai, so_luong)
VALUES
    ('YC-261118-997001', '019c0000-0000-7000-8000-0000000d7c01', 1),
    ('YC-261118-997002', '019c0000-0000-7000-8000-0000000d7c03', 1),
    ('YC-261118-997003', '019c0000-0000-7000-8000-0000000d7c02', 1),
    ('YC-261118-997004', '019c0000-0000-7000-8000-0000000d7c02', 1),
    ('YC-261118-997005', '019c0000-0000-7000-8000-0000000d7c01', 1),
    ('YC-261118-997006', '019c0000-0000-7000-8000-0000000d7c01', 1),
    ('YC-261118-997007', '019c0000-0000-7000-8000-0000000d7c01', 2),
    ('YC-261118-997008', '019c0000-0000-7000-8000-0000000d7c01', 1),
    ('YC-261118-997009', '019c0000-0000-7000-8000-0000000d7c02', 1),
    ('YC-261118-997010', '019c0000-0000-7000-8000-0000000d7c01', 1);
