INSERT INTO danh_muc.phong_thiet_bi (ma_phong, ten_phong, co_so)
VALUES
    ('019c0000-0000-7000-8000-0000000d8b01', 'D08 Phong P1', 'D08 Co so'),
    ('019c0000-0000-7000-8000-0000000d8b02', 'D08 Phong P2', 'D08 Co so');

INSERT INTO danh_muc.loai_thiet_bi (ma_loai, ten_loai, chu_ky_bao_tri, phi_tre_moi_ngay, ngung_su_dung)
VALUES
    ('019c0000-0000-7000-8000-0000000d8c01', 'D08 Loai X', 30, 50000, false),
    ('019c0000-0000-7000-8000-0000000d8c02', 'D08 Loai Y', 30, 30000, false);

INSERT INTO danh_muc.thiet_bi (ma_thiet_bi, ma_loai, ma_phong, ten_thiet_bi, ngay_nhap_kho, tinh_trang, vi_tri_ke, vi_tri_tu, ngung_su_dung)
VALUES
    ('D08-TB-01', '019c0000-0000-7000-8000-0000000d8c01', '019c0000-0000-7000-8000-0000000d8b01', 'X 01 san sang', DATE '2026-01-01', 'SAN_SANG', 'K1', 'T1', false),
    ('D08-TB-02', '019c0000-0000-7000-8000-0000000d8c01', '019c0000-0000-7000-8000-0000000d8b01', 'X 02 dang muon', DATE '2026-01-01', 'DANG_DUOC_MUON', 'K1', 'T1', false),
    ('D08-TB-03', '019c0000-0000-7000-8000-0000000d8c01', '019c0000-0000-7000-8000-0000000d8b01', 'X 03 ngung su dung', DATE '2026-01-01', 'SAN_SANG', 'K1', 'T1', true),
    ('D08-TB-04', '019c0000-0000-7000-8000-0000000d8c01', '019c0000-0000-7000-8000-0000000d8b02', 'X 04 phong P2', DATE '2026-01-01', 'SAN_SANG', 'K2', 'T2', false),
    ('D08-TB-05', '019c0000-0000-7000-8000-0000000d8c02', '019c0000-0000-7000-8000-0000000d8b01', 'Y 05 loai Y', DATE '2026-01-01', 'SAN_SANG', 'K1', 'T1', false),
    ('D08-TB-06', '019c0000-0000-7000-8000-0000000d8c01', '019c0000-0000-7000-8000-0000000d8b01', 'X 06 bao tri', DATE '2026-01-01', 'DANG_BAO_TRI_SUA_CHUA', 'K1', 'T1', false);

INSERT INTO danh_muc.lich_su_gia_tri (ma_lich_su, ma_thiet_bi, gia_tri)
VALUES
    ('019c0000-0000-7000-8000-0000000d8e01', 'D08-TB-01', 15000000),
    ('019c0000-0000-7000-8000-0000000d8e02', 'D08-TB-02', 15000000),
    ('019c0000-0000-7000-8000-0000000d8e03', 'D08-TB-03', 15000000),
    ('019c0000-0000-7000-8000-0000000d8e04', 'D08-TB-04', 15000000),
    ('019c0000-0000-7000-8000-0000000d8e05', 'D08-TB-05', 8000000),
    ('019c0000-0000-7000-8000-0000000d8e06', 'D08-TB-06', 15000000);
