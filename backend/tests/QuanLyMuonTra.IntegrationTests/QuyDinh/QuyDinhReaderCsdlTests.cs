using QuanLyMuonTra.IntegrationTests.C3;
using QuanLyMuonTra.IntegrationTests.MuonTra;
using QuanLyMuonTra.Modules.QuyDinh.Contracts;
using QuanLyMuonTra.Modules.QuyDinh.Infrastructure;

namespace QuanLyMuonTra.IntegrationTests.QuyDinh;

[Collection(C3BoSuuTap.Ten)]
public sealed class QuyDinhReaderCsdlTests
{
    private const string MaLoaiLaptop = "019c0000-0000-7000-8000-000000000011";
    private const string MaLoaiMayChieu = "019c0000-0000-7000-8000-000000000012";
    private const string MaKhungDangDung = "019c0000-0000-7000-8000-000000000401";
    private const string MaKhungDaTat = "019c0000-0000-7000-8000-000000000402";

    [CanKetNoiTestFact]
    public async Task UC13_LayThamSo_GiaTriDaDatVaGioQuay_AnhXaDungKieuThoiGian()
    {
        await using var gd = await GiaoDichTestRollback.BatDauAsync();
        await C3TestSupport.ThucThiAsync(gd, "INSERT INTO quy_dinh.tham_so_he_thong (id) VALUES (1) ON CONFLICT (id) DO NOTHING");
        await C3TestSupport.ThucThiAsync(
            gd,
            "UPDATE quy_dinh.tham_so_he_thong SET so_ngay_khoa = 4, thoi_gian_giu_cho_phut = 25, an_han_phut = 10, so_gio_gia_han_toi_da = 3, nguong_vi_pham = 4, so_lan_dang_nhap_sai_toi_da = 6, gio_mo_cua = TIME '07:30', gio_dong_cua = TIME '17:00' WHERE id = 1");
        await using var db = await C3TestSupport.TaoContextAsync(gd);
        var reader = new QuyDinhReader(db);

        var thamSo = await reader.LayThamSoAsync(CancellationToken.None);

        Assert.Equal(new ThamSoHeThongDto(4, 25, 10, 3, 4, 6, new TimeOnly(7, 30), new TimeOnly(17, 0)), thamSo);
    }

    [CanKetNoiTestFact]
    public async Task UC13_LayThamSo_GiaTriMacDinh_GioQuayChuaCauHinhLaNull()
    {
        await using var gd = await GiaoDichTestRollback.BatDauAsync();
        await C3TestSupport.ThucThiAsync(gd, "INSERT INTO quy_dinh.tham_so_he_thong (id) VALUES (1) ON CONFLICT (id) DO NOTHING");
        await C3TestSupport.ThucThiAsync(
            gd,
            "UPDATE quy_dinh.tham_so_he_thong SET so_ngay_khoa = DEFAULT, thoi_gian_giu_cho_phut = DEFAULT, an_han_phut = DEFAULT, so_gio_gia_han_toi_da = DEFAULT, nguong_vi_pham = DEFAULT, so_lan_dang_nhap_sai_toi_da = DEFAULT, gio_mo_cua = DEFAULT, gio_dong_cua = DEFAULT WHERE id = 1");
        await using var db = await C3TestSupport.TaoContextAsync(gd);
        var reader = new QuyDinhReader(db);

        var thamSo = await reader.LayThamSoAsync(CancellationToken.None);

        Assert.Equal(new ThamSoHeThongDto(3, 30, 15, 2, 3, 5, null, null), thamSo);
    }

    [CanKetNoiTestFact]
    public async Task UC13_LayQuyDinhNhom_ChungVaNgoaiLeThoiGianTheoLoai()
    {
        await using var gd = await GiaoDichTestRollback.BatDauAsync();
        await C3TestSupport.NapQuyDinhBatBuocAsync(gd);
        await C3TestSupport.NapDuLieuThuAsync(gd);
        await C3TestSupport.ThucThiAsync(
            gd,
            "UPDATE quy_dinh.quy_dinh_muon SET so_thiet_bi_toi_da = 2, thoi_gian_muon_toi_da_gio = 5 WHERE ma_nhom = 'DAI_HOC' AND ma_loai IS NULL");
        await using var db = await C3TestSupport.TaoContextAsync(gd);
        var reader = new QuyDinhReader(db);

        var ketQua = await reader.LayQuyDinhNhomAsync("DAI_HOC", CancellationToken.None);

        Assert.NotNull(ketQua);
        var nhom = ketQua!;
        Assert.Equal("DAI_HOC", nhom.MaNhom);
        Assert.Equal(2, nhom.SoThietBiToiDa);
        Assert.Equal(5, nhom.ThoiGianMuonToiDaGio);
        Assert.Contains(nhom.DsNgoaiLeTheoLoai, x => x.MaLoai == Guid.Parse(MaLoaiMayChieu) && x.ThoiGianMuonToiDaGio == 3);
        Assert.Equal(3, nhom.ThoiGianToiDaGio(new[] { Guid.Parse(MaLoaiMayChieu) }));
        Assert.Equal(5, nhom.ThoiGianToiDaGio(new[] { Guid.NewGuid() }));
    }

    [CanKetNoiTestFact]
    public async Task UC13_LayQuyDinhNhom_NhomKhongCoQuyDinhChung_TraNull()
    {
        await using var gd = await GiaoDichTestRollback.BatDauAsync();
        await C3TestSupport.NapDuLieuThuAsync(gd);
        await C3TestSupport.ThucThiAsync(gd, "INSERT INTO sinh_vien.nhom_sinh_vien (ma_nhom, ten_nhom) VALUES ('NHOM_THU_C3', 'Nhóm thử C3')");
        await C3TestSupport.ThucThiAsync(
            gd,
            $"INSERT INTO quy_dinh.quy_dinh_muon (ma_quy_dinh, ma_nhom, ma_loai, so_thiet_bi_toi_da, thoi_gian_muon_toi_da_gio) VALUES ('019c0000-0000-7000-8000-0000000f0b01', 'NHOM_THU_C3', '{MaLoaiLaptop}', NULL, 2)");
        await using var db = await C3TestSupport.TaoContextAsync(gd);
        var reader = new QuyDinhReader(db);
        var ct = CancellationToken.None;

        Assert.Null(await reader.LayQuyDinhNhomAsync("NHOM_THU_C3", ct));
        Assert.Null(await reader.LayQuyDinhNhomAsync("NHOM_KHONG_TON_TAI", ct));
    }

    [CanKetNoiTestFact]
    public async Task UC13_KhungPhat_LayTheoMaVaDanhSachChiGomKhungDangDung()
    {
        await using var gd = await GiaoDichTestRollback.BatDauAsync();
        await C3TestSupport.NapDuLieuThuAsync(gd);
        await using var db = await C3TestSupport.TaoContextAsync(gd);
        var reader = new QuyDinhReader(db);
        var ct = CancellationToken.None;

        var dangDung = await reader.LayKhungPhatAsync(Guid.Parse(MaKhungDangDung), ct);
        var daTat = await reader.LayKhungPhatAsync(Guid.Parse(MaKhungDaTat), ct);
        var danhSach = await reader.DsKhungPhatDangSuDungAsync(ct);

        Assert.Equal(new KhungPhatDto(Guid.Parse(MaKhungDangDung), "Thử 12,5%", 12.5m, true), dangDung);
        Assert.Equal(new KhungPhatDto(Guid.Parse(MaKhungDaTat), "Thử đã tắt", 5m, false), daTat);
        Assert.Null(await reader.LayKhungPhatAsync(Guid.NewGuid(), ct));
        Assert.Contains(danhSach, k => k.MaKhung == Guid.Parse(MaKhungDangDung));
        Assert.DoesNotContain(danhSach, k => k.MaKhung == Guid.Parse(MaKhungDaTat));
        Assert.Equal(danhSach.OrderBy(k => k.TyLePhanTram).ThenBy(k => k.MaKhung).ToList(), danhSach.ToList());
    }
}
