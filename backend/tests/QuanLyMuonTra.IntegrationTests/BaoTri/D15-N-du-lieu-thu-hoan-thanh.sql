INSERT INTO quy_dinh.tham_so_he_thong (id, gio_mo_cua, gio_dong_cua)
VALUES (1, TIME '07:00', TIME '17:00')
ON CONFLICT (id) DO NOTHING;

INSERT INTO sinh_vien.nhom_sinh_vien (ma_nhom, ten_nhom)
VALUES ('D15-NHOM', 'Nhóm D15');

INSERT INTO sinh_vien.sinh_vien (ma_sv, ho_ten, lop, khoa, email, ma_nhom, han_the, trang_thai_hoc, trang_thai_quyen_muon, hoc_ky)
VALUES
    ('D15-SV-A', 'Sinh viên D15 A', 'L1', 'K1', 'd15-a@example.test', 'D15-NHOM', DATE '2030-12-31', 'DANG_HOC', 'BINH_THUONG', '2026-1'),
    ('D15-SV-B', 'Sinh viên D15 B', 'L1', 'K1', 'd15-b@example.test', 'D15-NHOM', DATE '2030-12-31', 'DANG_HOC', 'BINH_THUONG', '2026-1'),
    ('D15-SV-C', 'Sinh viên D15 C', 'L1', 'K1', 'd15-c@example.test', 'D15-NHOM', DATE '2030-12-31', 'DANG_HOC', 'BINH_THUONG', '2026-1');

INSERT INTO tai_khoan.nguoi_dung_he_thong (ma_nguoi_dung, ten_dang_nhap, mat_khau_bam, ho_ten)
VALUES ('019c0000-0000-7000-8000-0000000c1521', 'd15-nv', 'khong-dung-de-dang-nhap', 'Nhân viên D15');

INSERT INTO tai_khoan.nhan_vien_quan_ly (ma_nguoi_dung, ma_nv)
VALUES ('019c0000-0000-7000-8000-0000000c1521', 'D15-NV');

INSERT INTO danh_muc.phong_thiet_bi (ma_phong, ten_phong, co_so)
VALUES
    ('019c0000-0000-7000-8000-0000000c1501', 'Phòng D15 P1', 'Cơ sở D15'),
    ('019c0000-0000-7000-8000-0000000c1502', 'Phòng D15 P2', 'Cơ sở D15');

INSERT INTO tai_khoan.nhan_vien_phong (ma_nguoi_dung, ma_phong)
VALUES ('019c0000-0000-7000-8000-0000000c1521', '019c0000-0000-7000-8000-0000000c1501');

INSERT INTO danh_muc.loai_thiet_bi (ma_loai, ten_loai, chu_ky_bao_tri, phi_tre_moi_ngay, ngung_su_dung)
VALUES
    ('019c0000-0000-7000-8000-0000000c1511', 'D15 Loại X', 30, 50000, false),
    ('019c0000-0000-7000-8000-0000000c1512', 'D15 Loại Y', 30, 30000, false),
    ('019c0000-0000-7000-8000-0000000c1513', 'D15 Loại W', 30, 20000, false);

INSERT INTO danh_muc.thiet_bi (ma_thiet_bi, ma_loai, ma_phong, ten_thiet_bi, ngay_nhap_kho, tinh_trang, ngung_su_dung)
VALUES
    ('D15-TB-01', '019c0000-0000-7000-8000-0000000c1511', '019c0000-0000-7000-8000-0000000c1501', 'D15 TB 01 định kỳ', DATE '2026-01-01', 'DANG_BAO_TRI_SUA_CHUA', false),
    ('D15-TB-02', '019c0000-0000-7000-8000-0000000c1512', '019c0000-0000-7000-8000-0000000c1501', 'D15 TB 02 có người chờ', DATE '2026-01-01', 'DANG_BAO_TRI_SUA_CHUA', false),
    ('D15-TB-03', '019c0000-0000-7000-8000-0000000c1511', '019c0000-0000-7000-8000-0000000c1501', 'D15 TB 03 sửa chữa', DATE '2026-01-01', 'DANG_BAO_TRI_SUA_CHUA', false),
    ('D15-TB-04', '019c0000-0000-7000-8000-0000000c1513', '019c0000-0000-7000-8000-0000000c1501', 'D15 TB 04 đề xuất thanh lý', DATE '2026-01-01', 'DANG_BAO_TRI_SUA_CHUA', false),
    ('D15-TB-05', '019c0000-0000-7000-8000-0000000c1511', '019c0000-0000-7000-8000-0000000c1501', 'D15 TB 05 đã hoàn thành', DATE '2026-01-01', 'SAN_SANG', false),
    ('D15-TB-06', '019c0000-0000-7000-8000-0000000c1511', '019c0000-0000-7000-8000-0000000c1502', 'D15 TB 06 phòng chưa cấp quyền', DATE '2026-01-01', 'DANG_BAO_TRI_SUA_CHUA', false);

INSERT INTO danh_muc.lich_su_gia_tri (ma_lich_su, ma_thiet_bi, gia_tri)
VALUES
    ('019c0000-0000-7000-8000-0000000c1541', 'D15-TB-01', 10000000),
    ('019c0000-0000-7000-8000-0000000c1542', 'D15-TB-02', 10000000),
    ('019c0000-0000-7000-8000-0000000c1543', 'D15-TB-03', 10000000),
    ('019c0000-0000-7000-8000-0000000c1544', 'D15-TB-04', 10000000),
    ('019c0000-0000-7000-8000-0000000c1545', 'D15-TB-05', 10000000),
    ('019c0000-0000-7000-8000-0000000c1546', 'D15-TB-06', 10000000);

INSERT INTO bao_tri.bao_tri_sua_chua (ma_bao_tri, ma_thiet_bi, ma_nv_thuc_hien, ma_phieu_muon, loai_thuc_hien, ngay_lap, noi_dung_thuc_hien, ket_qua, ngay_hoan_thanh)
VALUES
    ('019c0000-0000-7000-8000-0000000c1531', 'D15-TB-01', '019c0000-0000-7000-8000-0000000c1521', NULL, 'BAO_TRI_DINH_KY', TIMESTAMPTZ '2026-10-15 09:00:00+07', NULL, NULL, NULL),
    ('019c0000-0000-7000-8000-0000000c1532', 'D15-TB-02', '019c0000-0000-7000-8000-0000000c1521', NULL, 'SUA_CHUA', TIMESTAMPTZ '2026-11-18 09:00:00+07', NULL, NULL, NULL),
    ('019c0000-0000-7000-8000-0000000c1533', 'D15-TB-03', '019c0000-0000-7000-8000-0000000c1521', NULL, 'SUA_CHUA', TIMESTAMPTZ '2026-11-18 09:00:00+07', NULL, NULL, NULL),
    ('019c0000-0000-7000-8000-0000000c1534', 'D15-TB-04', '019c0000-0000-7000-8000-0000000c1521', NULL, 'SUA_CHUA', TIMESTAMPTZ '2026-11-18 09:00:00+07', NULL, NULL, NULL),
    ('019c0000-0000-7000-8000-0000000c1535', 'D15-TB-05', '019c0000-0000-7000-8000-0000000c1521', NULL, 'BAO_TRI_DINH_KY', TIMESTAMPTZ '2026-10-01 09:00:00+07', 'Bảo trì định kỳ đã xong', 'DUA_VE_SAN_SANG', TIMESTAMPTZ '2026-10-02 10:00:00+07'),
    ('019c0000-0000-7000-8000-0000000c1536', 'D15-TB-06', '019c0000-0000-7000-8000-0000000c1521', NULL, 'SUA_CHUA', TIMESTAMPTZ '2026-11-18 09:00:00+07', NULL, NULL, NULL);

INSERT INTO muon_tra.dang_ky_cho (ma_dang_ky, ma_sv, ma_phong, ma_loai, thoi_diem_dang_ky, so_thu_tu_cho, trang_thai, han_giu_cho)
VALUES
    ('DK-261123-915001', 'D15-SV-A', '019c0000-0000-7000-8000-0000000c1501', '019c0000-0000-7000-8000-0000000c1512', TIMESTAMPTZ '2026-11-22 01:00:00+00', 1, 'DANG_CHO', NULL),
    ('DK-261123-915002', 'D15-SV-B', '019c0000-0000-7000-8000-0000000c1501', '019c0000-0000-7000-8000-0000000c1512', TIMESTAMPTZ '2026-11-22 01:05:00+00', 2, 'DANG_CHO', NULL),
    ('DK-261123-915003', 'D15-SV-C', '019c0000-0000-7000-8000-0000000c1501', '019c0000-0000-7000-8000-0000000c1513', TIMESTAMPTZ '2026-11-22 01:00:00+00', 1, 'DANG_CHO', NULL);
