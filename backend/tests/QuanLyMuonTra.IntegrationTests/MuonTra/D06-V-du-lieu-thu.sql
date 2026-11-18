INSERT INTO sinh_vien.nhom_sinh_vien (ma_nhom, ten_nhom)
VALUES ('D06-NHOM', 'Nhóm D06');

INSERT INTO quy_dinh.quy_dinh_muon (ma_quy_dinh, ma_nhom, ma_loai, so_thiet_bi_toi_da, thoi_gian_muon_toi_da_gio)
VALUES ('019c0000-0000-7000-8000-0000000d6d01', 'D06-NHOM', NULL, 2, 5);

INSERT INTO sinh_vien.sinh_vien (ma_sv, ho_ten, lop, khoa, email, ma_nhom, han_the, trang_thai_hoc, trang_thai_quyen_muon, hoc_ky)
VALUES
    ('D06-SV-A', 'Sinh viên D06 A', 'L1', 'K1', 'd06-a@example.test', 'D06-NHOM', DATE '2030-12-31', 'DANG_HOC', 'BINH_THUONG', '2026-1'),
    ('D06-SV-B', 'Sinh viên D06 B', 'L1', 'K1', 'd06-b@example.test', 'D06-NHOM', DATE '2030-12-31', 'DANG_HOC', 'BINH_THUONG', '2026-1'),
    ('D06-SV-C', 'Sinh viên D06 C', 'L1', 'K1', 'd06-c@example.test', 'D06-NHOM', DATE '2030-12-31', 'DANG_HOC', 'BINH_THUONG', '2026-1'),
    ('D06-SV-HET-HAN', 'Sinh viên D06 hết hạn thẻ', 'L1', 'K1', 'd06-hh@example.test', 'D06-NHOM', DATE '2026-11-17', 'DANG_HOC', 'BINH_THUONG', '2026-1'),
    ('D06-SV-HOM-NAY', 'Sinh viên D06 hạn thẻ hôm nay', 'L1', 'K1', 'd06-hn@example.test', 'D06-NHOM', DATE '2026-11-18', 'DANG_HOC', 'BINH_THUONG', '2026-1'),
    ('D06-SV-TOT-NGHIEP', 'Sinh viên D06 tốt nghiệp', 'L1', 'K1', 'd06-tn@example.test', 'D06-NHOM', DATE '2030-12-31', 'TOT_NGHIEP', 'BINH_THUONG', '2026-1'),
    ('D06-SV-TAM-KHOA', 'Sinh viên D06 tạm khóa', 'L1', 'K1', 'd06-tk@example.test', 'D06-NHOM', DATE '2030-12-31', 'DANG_HOC', 'TAM_KHOA', '2026-1');

INSERT INTO tai_khoan.nguoi_dung_he_thong (ma_nguoi_dung, ten_dang_nhap, mat_khau_bam, ho_ten)
VALUES ('019c0000-0000-7000-8000-0000000d6a01', 'd06-nv', 'khong-dung-de-dang-nhap', 'Nhân viên D06');

INSERT INTO tai_khoan.nhan_vien_quan_ly (ma_nguoi_dung, ma_nv)
VALUES ('019c0000-0000-7000-8000-0000000d6a01', 'D06-NV');

INSERT INTO danh_muc.phong_thiet_bi (ma_phong, ten_phong, co_so)
VALUES ('019c0000-0000-7000-8000-0000000d6b01', 'D06 Phong P1', 'D06 Co so');

INSERT INTO danh_muc.loai_thiet_bi (ma_loai, ten_loai, chu_ky_bao_tri, phi_tre_moi_ngay, ngung_su_dung)
VALUES
    ('019c0000-0000-7000-8000-0000000d6c01', 'D06 Loai X', 30, 50000, false),
    ('019c0000-0000-7000-8000-0000000d6c02', 'D06 Loai Y', 30, 30000, false),
    ('019c0000-0000-7000-8000-0000000d6c03', 'D06 Loai W', 30, 20000, false),
    ('019c0000-0000-7000-8000-0000000d6c04', 'D06 Loai Z ngung', 30, 10000, true);

INSERT INTO quy_dinh.quy_dinh_muon (ma_quy_dinh, ma_nhom, ma_loai, so_thiet_bi_toi_da, thoi_gian_muon_toi_da_gio)
VALUES ('019c0000-0000-7000-8000-0000000d6d02', 'D06-NHOM', '019c0000-0000-7000-8000-0000000d6c02', NULL, 2);

INSERT INTO danh_muc.thiet_bi (ma_thiet_bi, ma_loai, ma_phong, ten_thiet_bi, ngay_nhap_kho, tinh_trang, ngung_su_dung)
VALUES
    ('D06-TB-01', '019c0000-0000-7000-8000-0000000d6c01', '019c0000-0000-7000-8000-0000000d6b01', 'X 01', DATE '2026-01-01', 'SAN_SANG', false),
    ('D06-TB-02', '019c0000-0000-7000-8000-0000000d6c01', '019c0000-0000-7000-8000-0000000d6b01', 'X 02', DATE '2026-01-01', 'SAN_SANG', false),
    ('D06-TB-03', '019c0000-0000-7000-8000-0000000d6c01', '019c0000-0000-7000-8000-0000000d6b01', 'X 03', DATE '2026-01-01', 'SAN_SANG', false),
    ('D06-TB-04', '019c0000-0000-7000-8000-0000000d6c01', '019c0000-0000-7000-8000-0000000d6b01', 'X 04 dang muon', DATE '2026-01-01', 'DANG_DUOC_MUON', false),
    ('D06-TB-05', '019c0000-0000-7000-8000-0000000d6c02', '019c0000-0000-7000-8000-0000000d6b01', 'Y 05', DATE '2026-01-01', 'SAN_SANG', false),
    ('D06-TB-06', '019c0000-0000-7000-8000-0000000d6c03', '019c0000-0000-7000-8000-0000000d6b01', 'W 06', DATE '2026-01-01', 'SAN_SANG', false),
    ('D06-TB-07', '019c0000-0000-7000-8000-0000000d6c03', '019c0000-0000-7000-8000-0000000d6b01', 'W 07', DATE '2026-01-01', 'SAN_SANG', false),
    ('D06-TB-08', '019c0000-0000-7000-8000-0000000d6c03', '019c0000-0000-7000-8000-0000000d6b01', 'W 08', DATE '2026-01-01', 'SAN_SANG', false),
    ('D06-TB-09', '019c0000-0000-7000-8000-0000000d6c04', '019c0000-0000-7000-8000-0000000d6b01', 'Z 09', DATE '2026-01-01', 'SAN_SANG', true);

INSERT INTO muon_tra.phieu_muon (ma_phieu, ma_sv, ma_nv_lap, ngay_gio_muon, ngay_gio_hen_tra, trang_thai)
VALUES ('PM-261118-996001', 'D06-SV-C', '019c0000-0000-7000-8000-0000000d6a01', TIMESTAMPTZ '2026-11-18 01:00:00+00', TIMESTAMPTZ '2026-11-18 06:00:00+00', 'DANG_MUON');

INSERT INTO muon_tra.chi_tiet_phieu_muon (ma_chi_tiet, ma_phieu, ma_thiet_bi, tinh_trang_ban_dau, phi_tre_moi_ngay)
VALUES ('019c0000-0000-7000-8000-0000000d6e01', 'PM-261118-996001', 'D06-TB-04', 'Tốt', 50000);

INSERT INTO muon_tra.phieu_yeu_cau_muon (ma_yeu_cau, ma_sv, ma_phong, ma_nv_duyet, thoi_gian_gui, thoi_gian_muon_du_kien_gio, trang_thai, thoi_diem_duyet, han_giu_cho)
VALUES ('YC-261118-996001', 'D06-SV-B', '019c0000-0000-7000-8000-0000000d6b01', '019c0000-0000-7000-8000-0000000d6a01', TIMESTAMPTZ '2026-11-18 02:00:00+00', 2, 'DA_DUYET', TIMESTAMPTZ '2026-11-18 02:30:00+00', TIMESTAMPTZ '2026-11-18 04:00:00+00');

INSERT INTO muon_tra.chi_tiet_yeu_cau_muon (ma_yeu_cau, ma_loai, so_luong)
VALUES ('YC-261118-996001', '019c0000-0000-7000-8000-0000000d6c03', 2);
