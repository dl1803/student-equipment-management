INSERT INTO sinh_vien.nhom_sinh_vien (ma_nhom, ten_nhom)
VALUES ('DAI_HOC', 'Sinh viên đại học')
ON CONFLICT (ma_nhom) DO NOTHING;

INSERT INTO sinh_vien.sinh_vien (ma_sv, ho_ten, lop, khoa, email, ma_nhom, han_the, trang_thai_hoc, hoc_ky)
VALUES
    ('D02-SV-A', 'Sinh viên D02 A', 'L1', 'K1', 'd02-a@example.test', 'DAI_HOC', DATE '2030-12-31', 'DANG_HOC', '2026-1'),
    ('D02-SV-B', 'Sinh viên D02 B', 'L1', 'K1', 'd02-b@example.test', 'DAI_HOC', DATE '2030-12-31', 'DANG_HOC', '2026-1'),
    ('D02-SV-C', 'Sinh viên D02 C', 'L1', 'K1', 'd02-c@example.test', 'DAI_HOC', DATE '2030-12-31', 'DANG_HOC', '2026-1'),
    ('D02-SV-D', 'Sinh viên D02 D', 'L1', 'K1', 'd02-d@example.test', 'DAI_HOC', DATE '2030-12-31', 'DANG_HOC', '2026-1');

INSERT INTO tai_khoan.nguoi_dung_he_thong (ma_nguoi_dung, ten_dang_nhap, mat_khau_bam, ho_ten)
VALUES ('019c0000-0000-7000-8000-0000000d0a01', 'd02-nv', 'khong-dung-de-dang-nhap', 'Nhân viên D02');

INSERT INTO tai_khoan.nhan_vien_quan_ly (ma_nguoi_dung, ma_nv)
VALUES ('019c0000-0000-7000-8000-0000000d0a01', 'D02-NV');

INSERT INTO danh_muc.phong_thiet_bi (ma_phong, ten_phong, co_so)
VALUES
    ('019c0000-0000-7000-8000-0000000d0b01', 'Phòng D02 P1', 'Cơ sở D02'),
    ('019c0000-0000-7000-8000-0000000d0b02', 'Phòng D02 P2', 'Cơ sở D02'),
    ('019c0000-0000-7000-8000-0000000d0b03', 'Phòng D02 P3 chưa tham chiếu', 'Cơ sở D02');

INSERT INTO danh_muc.loai_thiet_bi (ma_loai, ten_loai, chu_ky_bao_tri, phi_tre_moi_ngay)
VALUES
    ('019c0000-0000-7000-8000-0000000d0c01', 'Loại D02 L1', 30, 50000),
    ('019c0000-0000-7000-8000-0000000d0c02', 'Loại D02 L2', 30, 30000),
    ('019c0000-0000-7000-8000-0000000d0c03', 'Loại D02 L3 chưa tham chiếu', 30, 10000);

INSERT INTO danh_muc.thiet_bi (ma_thiet_bi, ma_loai, ma_phong, ten_thiet_bi, ngay_nhap_kho)
VALUES
    ('D02-TB-01', '019c0000-0000-7000-8000-0000000d0c01', '019c0000-0000-7000-8000-0000000d0b01', 'Thiết bị 01', DATE '2026-01-01'),
    ('D02-TB-02', '019c0000-0000-7000-8000-0000000d0c01', '019c0000-0000-7000-8000-0000000d0b01', 'Thiết bị 02', DATE '2026-01-01'),
    ('D02-TB-03', '019c0000-0000-7000-8000-0000000d0c02', '019c0000-0000-7000-8000-0000000d0b02', 'Thiết bị 03', DATE '2026-01-01'),
    ('D02-TB-04', '019c0000-0000-7000-8000-0000000d0c01', '019c0000-0000-7000-8000-0000000d0b01', 'Thiết bị 04', DATE '2026-01-01'),
    ('D02-TB-05', '019c0000-0000-7000-8000-0000000d0c02', '019c0000-0000-7000-8000-0000000d0b02', 'Thiết bị 05', DATE '2026-01-01'),
    ('D02-TB-06', '019c0000-0000-7000-8000-0000000d0c02', '019c0000-0000-7000-8000-0000000d0b02', 'Thiết bị 06', DATE '2026-01-01'),
    ('D02-TB-07', '019c0000-0000-7000-8000-0000000d0c02', '019c0000-0000-7000-8000-0000000d0b02', 'Thiết bị 07', DATE '2026-01-01'),
    ('D02-TB-08', '019c0000-0000-7000-8000-0000000d0c03', '019c0000-0000-7000-8000-0000000d0b03', 'Thiết bị 08 chưa tham chiếu', DATE '2026-01-01');

INSERT INTO quy_dinh.khung_phat (ma_khung, ten_khung, ty_le_phan_tram)
VALUES
    ('019c0000-0000-7000-8000-0000000d0d01', 'Khung D02 đã dùng', 20),
    ('019c0000-0000-7000-8000-0000000d0d02', 'Khung D02 chưa dùng', 30);

INSERT INTO muon_tra.phieu_yeu_cau_muon (ma_yeu_cau, ma_sv, ma_phong, thoi_gian_muon_du_kien_gio)
VALUES ('YC-261114-990001', 'D02-SV-D', '019c0000-0000-7000-8000-0000000d0b01', 2);

INSERT INTO muon_tra.chi_tiet_yeu_cau_muon (ma_yeu_cau, ma_loai, so_luong)
VALUES ('YC-261114-990001', '019c0000-0000-7000-8000-0000000d0c01', 1);

INSERT INTO muon_tra.dang_ky_cho (ma_dang_ky, ma_sv, ma_phong, ma_loai, so_thu_tu_cho)
VALUES ('DK-261114-990001', 'D02-SV-D', '019c0000-0000-7000-8000-0000000d0b02', '019c0000-0000-7000-8000-0000000d0c02', 1);

INSERT INTO muon_tra.phieu_muon (ma_phieu, ma_sv, ma_nv_lap, ngay_gio_muon, ngay_gio_hen_tra)
VALUES
    ('PM-261114-990001', 'D02-SV-A', '019c0000-0000-7000-8000-0000000d0a01', TIMESTAMPTZ '2026-11-14 08:00:00+00', TIMESTAMPTZ '2026-11-14 10:00:00+00'),
    ('PM-261114-990002', 'D02-SV-A', '019c0000-0000-7000-8000-0000000d0a01', TIMESTAMPTZ '2026-11-14 08:00:00+00', TIMESTAMPTZ '2026-11-14 10:00:00+00'),
    ('PM-261114-990003', 'D02-SV-B', '019c0000-0000-7000-8000-0000000d0a01', TIMESTAMPTZ '2026-11-14 08:00:00+00', TIMESTAMPTZ '2026-11-14 10:00:00+00'),
    ('PM-261114-990004', 'D02-SV-C', '019c0000-0000-7000-8000-0000000d0a01', TIMESTAMPTZ '2026-11-10 08:00:00+00', TIMESTAMPTZ '2026-11-10 10:00:00+00');

INSERT INTO muon_tra.chi_tiet_phieu_muon (ma_chi_tiet, ma_phieu, ma_thiet_bi, tinh_trang_ban_dau, phi_tre_moi_ngay)
VALUES
    ('019c0000-0000-7000-8000-0000000d0e01', 'PM-261114-990001', 'D02-TB-01', 'Tốt', 50000),
    ('019c0000-0000-7000-8000-0000000d0e02', 'PM-261114-990001', 'D02-TB-02', 'Tốt', 50000),
    ('019c0000-0000-7000-8000-0000000d0e03', 'PM-261114-990002', 'D02-TB-03', 'Tốt', 30000),
    ('019c0000-0000-7000-8000-0000000d0e06', 'PM-261114-990003', 'D02-TB-06', 'Tốt', 30000);

INSERT INTO muon_tra.chi_tiet_phieu_muon (ma_chi_tiet, ma_phieu, ma_thiet_bi, tinh_trang_ban_dau, phi_tre_moi_ngay, tinh_trang_khi_tra, ket_qua_kiem_tra, ngay_gio_tra_thuc_te, ma_nv_nhan_tra)
VALUES
    ('019c0000-0000-7000-8000-0000000d0e04', 'PM-261114-990002', 'D02-TB-04', 'Tốt', 50000, NULL, 'NGUYEN_VEN', TIMESTAMPTZ '2026-11-14 09:00:00+00', '019c0000-0000-7000-8000-0000000d0a01');

INSERT INTO muon_tra.chi_tiet_phieu_muon (ma_chi_tiet, ma_phieu, ma_thiet_bi, tinh_trang_ban_dau, phi_tre_moi_ngay, ket_qua_kiem_tra, ma_nv_nhan_tra)
VALUES
    ('019c0000-0000-7000-8000-0000000d0e05', 'PM-261114-990002', 'D02-TB-05', 'Tốt', 30000, 'MAT', '019c0000-0000-7000-8000-0000000d0a01');

INSERT INTO muon_tra.chi_tiet_phieu_muon (ma_chi_tiet, ma_phieu, ma_thiet_bi, tinh_trang_ban_dau, phi_tre_moi_ngay, tinh_trang_khi_tra, ket_qua_kiem_tra, ngay_gio_tra_thuc_te, ma_nv_nhan_tra, ma_khung)
VALUES
    ('019c0000-0000-7000-8000-0000000d0e07', 'PM-261114-990004', 'D02-TB-07', 'Tốt', 10000, 'Vỡ màn hình', 'HU_HONG', TIMESTAMPTZ '2026-11-12 09:00:00+00', '019c0000-0000-7000-8000-0000000d0a01', '019c0000-0000-7000-8000-0000000d0d01');

INSERT INTO vi_pham.bien_ban_vi_pham (ma_bien_ban, ma_phieu_muon, ma_nv_lap, tong_tien)
VALUES ('BB-261114-990001', 'PM-261114-990004', '019c0000-0000-7000-8000-0000000d0a01', 20000);

INSERT INTO vi_pham.chi_tiet_bien_ban (ma_bien_ban, ma_phieu_muon, ma_thiet_bi, loai_vi_pham, so_tien, ma_khung)
VALUES ('BB-261114-990001', 'PM-261114-990004', 'D02-TB-07', 'HU_HONG', 20000, '019c0000-0000-7000-8000-0000000d0d01');
