INSERT INTO sinh_vien.nhom_sinh_vien (ma_nhom, ten_nhom)
VALUES ('DAI_HOC', 'Sinh viên đại học')
ON CONFLICT (ma_nhom) DO NOTHING;

INSERT INTO sinh_vien.sinh_vien (ma_sv, ho_ten, lop, khoa, email, ma_nhom, han_the, trang_thai_hoc, hoc_ky)
VALUES ('D05-SV-A', 'Sinh viên D05 A', 'L1', 'K1', 'd05-a@example.test', 'DAI_HOC', DATE '2030-12-31', 'DANG_HOC', '2026-1');

INSERT INTO tai_khoan.nguoi_dung_he_thong (ma_nguoi_dung, ten_dang_nhap, mat_khau_bam, ho_ten)
VALUES ('019c0000-0000-7000-8000-0000000d5a01', 'd05-nv', 'khong-dung-de-dang-nhap', 'Nhân viên D05');

INSERT INTO tai_khoan.nhan_vien_quan_ly (ma_nguoi_dung, ma_nv)
VALUES ('019c0000-0000-7000-8000-0000000d5a01', 'D05-NV');

INSERT INTO danh_muc.phong_thiet_bi (ma_phong, ten_phong, co_so)
VALUES
    ('019c0000-0000-7000-8000-0000000d5b01', 'D05V Phong A', 'D05V Co so 1'),
    ('019c0000-0000-7000-8000-0000000d5b02', 'D05V Phong B', 'D05V Co so 1'),
    ('019c0000-0000-7000-8000-0000000d5b03', 'D05V Phong C', 'D05V Co so 2'),
    ('019c0000-0000-7000-8000-0000000d5b04', 'D05V Phong A', 'D05V Co so 2');

INSERT INTO danh_muc.loai_thiet_bi (ma_loai, ten_loai, mo_ta, hang_san_xuat, chu_ky_bao_tri, phi_tre_moi_ngay, ngung_su_dung)
VALUES
    ('019c0000-0000-7000-8000-0000000d5c01', 'D05V Projector', 'Mo ta projector', 'Epson', 30, 50000, false),
    ('019c0000-0000-7000-8000-0000000d5c02', 'D05V Laptop', NULL, NULL, 30, 30000, false),
    ('019c0000-0000-7000-8000-0000000d5c03', 'D05V Speaker', NULL, NULL, 30, 10000, true),
    ('019c0000-0000-7000-8000-0000000d5c04', 'D05V Micro', NULL, 'Shure', 30, 20000, false);

INSERT INTO danh_muc.thiet_bi (ma_thiet_bi, ma_loai, ma_phong, ten_thiet_bi, thong_so_ky_thuat, ngay_nhap_kho, tinh_trang, ngung_su_dung)
VALUES
    ('D05-TB-01', '019c0000-0000-7000-8000-0000000d5c01', '019c0000-0000-7000-8000-0000000d5b01', 'Projector 01', 'HDMI 3000lm', DATE '2026-01-01', 'SAN_SANG', false),
    ('D05-TB-02', '019c0000-0000-7000-8000-0000000d5c01', '019c0000-0000-7000-8000-0000000d5b01', 'Projector 02', 'HDMI 3000lm', DATE '2026-01-01', 'SAN_SANG', false),
    ('D05-TB-03', '019c0000-0000-7000-8000-0000000d5c01', '019c0000-0000-7000-8000-0000000d5b01', 'Projector 03', 'VGA', DATE '2026-01-01', 'SAN_SANG', false),
    ('D05-TB-04', '019c0000-0000-7000-8000-0000000d5c01', '019c0000-0000-7000-8000-0000000d5b01', 'Projector 04', NULL, DATE '2026-01-01', 'SAN_SANG', false),
    ('D05-TB-05', '019c0000-0000-7000-8000-0000000d5c01', '019c0000-0000-7000-8000-0000000d5b01', 'Projector 05', '   ', DATE '2026-01-01', 'SAN_SANG', false),
    ('D05-TB-06', '019c0000-0000-7000-8000-0000000d5c02', '019c0000-0000-7000-8000-0000000d5b01', 'Laptop 06', NULL, DATE '2026-01-01', 'SAN_SANG', false),
    ('D05-TB-07', '019c0000-0000-7000-8000-0000000d5c02', '019c0000-0000-7000-8000-0000000d5b01', 'Laptop 07 bao tri', NULL, DATE '2026-01-01', 'DANG_BAO_TRI_SUA_CHUA', false),
    ('D05-TB-08', '019c0000-0000-7000-8000-0000000d5c03', '019c0000-0000-7000-8000-0000000d5b01', 'Speaker 08', NULL, DATE '2026-01-01', 'SAN_SANG', false),
    ('D05-TB-09', '019c0000-0000-7000-8000-0000000d5c02', '019c0000-0000-7000-8000-0000000d5b04', 'Laptop 09', NULL, DATE '2026-01-01', 'SAN_SANG', false),
    ('D05-TB-10', '019c0000-0000-7000-8000-0000000d5c01', '019c0000-0000-7000-8000-0000000d5b02', 'Projector 10 ngung', NULL, DATE '2026-01-01', 'SAN_SANG', true),
    ('D05-TB-11', '019c0000-0000-7000-8000-0000000d5c04', '019c0000-0000-7000-8000-0000000d5b02', 'Micro 11 dang muon', NULL, DATE '2026-01-01', 'DANG_DUOC_MUON', false),
    ('D05-TB-12', '019c0000-0000-7000-8000-0000000d5c04', '019c0000-0000-7000-8000-0000000d5b02', 'Micro 12 dang muon', NULL, DATE '2026-01-01', 'DANG_DUOC_MUON', false),
    ('D05-TB-13', '019c0000-0000-7000-8000-0000000d5c04', '019c0000-0000-7000-8000-0000000d5b02', 'Micro 13 bao tri', NULL, DATE '2026-01-01', 'DANG_BAO_TRI_SUA_CHUA', false);

INSERT INTO muon_tra.phieu_yeu_cau_muon (ma_yeu_cau, ma_sv, ma_phong, ma_nv_duyet, thoi_gian_gui, thoi_gian_muon_du_kien_gio, trang_thai, thoi_diem_duyet, han_giu_cho)
VALUES
    ('YC-261114-995001', 'D05-SV-A', '019c0000-0000-7000-8000-0000000d5b01', '019c0000-0000-7000-8000-0000000d5a01', TIMESTAMPTZ '2026-11-14 01:50:00+00', 2, 'DA_DUYET', TIMESTAMPTZ '2026-11-14 02:00:00+00', TIMESTAMPTZ '2026-11-14 02:30:00+00'),
    ('YC-261114-995002', 'D05-SV-A', '019c0000-0000-7000-8000-0000000d5b01', '019c0000-0000-7000-8000-0000000d5a01', TIMESTAMPTZ '2026-11-14 00:50:00+00', 2, 'DA_DUYET', TIMESTAMPTZ '2026-11-14 01:00:00+00', TIMESTAMPTZ '2026-11-14 01:30:00+00'),
    ('YC-261114-995003', 'D05-SV-A', '019c0000-0000-7000-8000-0000000d5b01', NULL, TIMESTAMPTZ '2026-11-14 01:50:00+00', 2, 'CHO_DUYET', NULL, NULL);

INSERT INTO muon_tra.chi_tiet_yeu_cau_muon (ma_yeu_cau, ma_loai, so_luong)
VALUES
    ('YC-261114-995001', '019c0000-0000-7000-8000-0000000d5c01', 1),
    ('YC-261114-995002', '019c0000-0000-7000-8000-0000000d5c01', 2),
    ('YC-261114-995003', '019c0000-0000-7000-8000-0000000d5c01', 1);

INSERT INTO muon_tra.dang_ky_cho (ma_dang_ky, ma_sv, ma_phong, ma_loai, thoi_diem_dang_ky, so_thu_tu_cho, trang_thai, han_giu_cho)
VALUES
    ('DK-261114-995001', 'D05-SV-A', '019c0000-0000-7000-8000-0000000d5b01', '019c0000-0000-7000-8000-0000000d5c01', TIMESTAMPTZ '2026-11-14 01:00:00+00', NULL, 'DA_THONG_BAO', TIMESTAMPTZ '2026-11-14 02:30:00+00');

INSERT INTO muon_tra.phieu_muon (ma_phieu, ma_sv, ma_nv_lap, ngay_gio_muon, ngay_gio_hen_tra, trang_thai)
VALUES
    ('PM-261114-995001', 'D05-SV-A', '019c0000-0000-7000-8000-0000000d5a01', TIMESTAMPTZ '2026-11-12 08:00:00+00', TIMESTAMPTZ '2026-11-13 10:00:00+00', 'QUA_HAN'),
    ('PM-261114-995002', 'D05-SV-A', '019c0000-0000-7000-8000-0000000d5a01', TIMESTAMPTZ '2026-11-14 01:00:00+00', TIMESTAMPTZ '2026-11-15 10:00:00+00', 'DANG_MUON'),
    ('PM-261114-995003', 'D05-SV-A', '019c0000-0000-7000-8000-0000000d5a01', TIMESTAMPTZ '2026-11-10 08:00:00+00', TIMESTAMPTZ '2026-11-12 10:00:00+00', 'DA_TRA');

INSERT INTO muon_tra.chi_tiet_phieu_muon (ma_chi_tiet, ma_phieu, ma_thiet_bi, tinh_trang_ban_dau, phi_tre_moi_ngay)
VALUES
    ('019c0000-0000-7000-8000-0000000d5e01', 'PM-261114-995001', 'D05-TB-11', 'Tốt', 20000),
    ('019c0000-0000-7000-8000-0000000d5e02', 'PM-261114-995002', 'D05-TB-12', 'Tốt', 20000);

INSERT INTO muon_tra.chi_tiet_phieu_muon (ma_chi_tiet, ma_phieu, ma_thiet_bi, tinh_trang_ban_dau, phi_tre_moi_ngay, tinh_trang_khi_tra, ket_qua_kiem_tra, ngay_gio_tra_thuc_te, ma_nv_nhan_tra)
VALUES
    ('019c0000-0000-7000-8000-0000000d5e03', 'PM-261114-995003', 'D05-TB-13', 'Tốt', 20000, NULL, 'NGUYEN_VEN', TIMESTAMPTZ '2026-11-12 09:00:00+00', '019c0000-0000-7000-8000-0000000d5a01');
