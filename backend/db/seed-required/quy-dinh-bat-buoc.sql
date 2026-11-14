BEGIN;

INSERT INTO quy_dinh.tham_so_he_thong (id) VALUES (1) ON CONFLICT (id) DO NOTHING;

INSERT INTO quy_dinh.khung_phat (ma_khung, ten_khung, ty_le_phan_tram, dang_su_dung)
SELECT gen_random_uuid(), v.ten, v.ty_le, true
FROM (VALUES ('Nhẹ', 15), ('Vừa', 40), ('Nặng', 80)) AS v(ten, ty_le)
WHERE NOT EXISTS (SELECT 1 FROM quy_dinh.khung_phat k WHERE k.ten_khung = v.ten);

INSERT INTO quy_dinh.quy_dinh_muon (ma_quy_dinh, ma_nhom, ma_loai, so_thiet_bi_toi_da, thoi_gian_muon_toi_da_gio)
SELECT gen_random_uuid(), v.ma_nhom, NULL::uuid, v.so_tb, v.gio
FROM (VALUES ('DAI_HOC', 2, 5), ('CAO_HOC', 3, 10), ('DO_AN_TOT_NGHIEP', 3, 10)) AS v(ma_nhom, so_tb, gio)
WHERE NOT EXISTS (SELECT 1 FROM quy_dinh.quy_dinh_muon q WHERE q.ma_nhom = v.ma_nhom AND q.ma_loai IS NULL);

COMMIT;
