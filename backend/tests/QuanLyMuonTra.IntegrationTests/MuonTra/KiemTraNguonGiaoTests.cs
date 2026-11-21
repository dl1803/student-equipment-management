using Microsoft.EntityFrameworkCore;
using QuanLyMuonTra.Modules.DanhMuc.Infrastructure;
using QuanLyMuonTra.Modules.MuonTra.Application;
using QuanLyMuonTra.Modules.MuonTra.Infrastructure;
using QuanLyMuonTra.Modules.QuyDinh.Infrastructure;
using QuanLyMuonTra.Modules.TaiKhoan.Contracts;
using QuanLyMuonTra.SharedKernel;

namespace QuanLyMuonTra.IntegrationTests.MuonTra;

public class KiemTraNguonGiaoTests
{
    private const string YcHopLe = "YC-261120-998001";
    private const string YcDungHan = "YC-261120-998002";
    private const string YcQuaHan = "YC-261120-998003";
    private const string YcChoDuyet = "YC-261120-998004";
    private const string YcTheHetHan = "YC-261120-998005";
    private const string YcTamKhoa = "YC-261120-998006";
    private const string YcPhongKhac = "YC-261120-998007";
    private const string DkHopLe = "DK-261120-998001";
    private const string DkDungHan = "DK-261120-998002";
    private const string DkDaNhan = "DK-261120-998003";
    private static readonly Guid Nv = Guid.Parse("019c0000-0000-7000-8000-0000000d8a01");
    private static readonly Guid P1 = Guid.Parse("019c0000-0000-7000-8000-0000000d8b01");
    private static readonly Guid P2 = Guid.Parse("019c0000-0000-7000-8000-0000000d8b02");
    private static readonly Guid X = Guid.Parse("019c0000-0000-7000-8000-0000000d8c01");
    private static readonly Guid Y = Guid.Parse("019c0000-0000-7000-8000-0000000d8c02");
    private static readonly Guid LoaiKhongCo = Guid.Parse("019c0000-0000-7000-8000-0000000d8cff");
    private static readonly DateTimeOffset BayGio = new(2026, 11, 20, 3, 0, 0, TimeSpan.Zero);
    private static readonly CancellationToken Ct = CancellationToken.None;

    private sealed class DongHoCoDinh(DateTimeOffset thoiDiem) : IClock
    {
        public DateTimeOffset UtcNow => thoiDiem;
    }

    private sealed class QuyenPhongGia(IReadOnlyCollection<Guid> dsPhong) : IQuyenPhongService
    {
        public Task<IReadOnlyCollection<Guid>> LayDsMaPhongAsync(Guid maNguoiDung, CancellationToken ct)
        {
            return Task.FromResult(dsPhong);
        }

        public Task EnsureAsync(Guid maNguoiDung, Guid maPhong, CancellationToken ct)
        {
            if (!dsPhong.Contains(maPhong))
            {
                throw new LoiNghiepVuException("PHONG_CHUA_CAP_QUYEN", "Phòng chưa được cấp quyền.");
            }

            return Task.CompletedTask;
        }
    }

    private static async Task<YeuCauMuonSvTests.YeuCauMuonTestContext> TaoContextAsync(GiaoDichTestRollback gd)
    {
        await gd.NapDuLieuThuAsync("D08-V-du-lieu-thu.sql");
        await gd.NapDuLieuThuAsync("D08-V-nguon-du-lieu-thu.sql");
        var options = new DbContextOptionsBuilder<YeuCauMuonSvTests.YeuCauMuonTestContext>().UseNpgsql(gd.KetNoi).Options;
        var db = new YeuCauMuonSvTests.YeuCauMuonTestContext(options);
        await db.Database.UseTransactionAsync(gd.GiaoDich, Ct);
        return db;
    }

    private static KiemTraNguonGiaoHandler TaoBo(DbContext db, params Guid[] phongDuocCap)
    {
        var dongHo = new DongHoCoDinh(BayGio);
        return new KiemTraNguonGiaoHandler(
            db,
            dongHo,
            new DanhMucReader(db),
            new QuyDinhReader(db),
            new MuonTraReader(db),
            new AvailabilityService(db, dongHo),
            new QuyenPhongGia(phongDuocCap));
    }

    private static async Task<KetQuaKiemTraNguonDto> TheoPhieuYeuCauAsync(string maYeuCau)
    {
        await using var gd = await GiaoDichTestRollback.BatDauAsync();
        await using var db = await TaoContextAsync(gd);
        return await TaoBo(db, P1).KiemTraPhieuYeuCauAsync(Nv, maYeuCau, Ct);
    }

    private static async Task<KetQuaKiemTraNguonDto> TheoDangKyChoAsync(string maDangKy)
    {
        await using var gd = await GiaoDichTestRollback.BatDauAsync();
        await using var db = await TaoContextAsync(gd);
        return await TaoBo(db, P1).KiemTraDangKyChoAsync(Nv, maDangKy, Ct);
    }

    private static async Task<KetQuaKiemTraNguonDto> TrucTiepAsync(string maSv, Guid maPhong, params LoaiVaSoLuongYeuCau[] dsLoai)
    {
        await using var gd = await GiaoDichTestRollback.BatDauAsync();
        await using var db = await TaoContextAsync(gd);
        return await TaoBo(db, P1).KiemTraTrucTiepAsync(Nv, maSv, maPhong, dsLoai, Ct);
    }

    private static async Task<LoiNghiepVuException> LoiAsync(Func<KiemTraNguonGiaoHandler, Task> thucHien)
    {
        await using var gd = await GiaoDichTestRollback.BatDauAsync();
        await using var db = await TaoContextAsync(gd);
        return await Assert.ThrowsAsync<LoiNghiepVuException>(() => thucHien(TaoBo(db, P1)));
    }

    [CanKetNoiTestFact]
    public async Task UC07_KiemNguon_PhieuYeuCauHopLe_TraSinhVienCuaNguonLoaiVaViTri()
    {
        var ketQua = await TheoPhieuYeuCauAsync(YcHopLe);

        Assert.True(ketQua.HopLe);
        Assert.Empty(ketQua.DsLyDo);
        Assert.Equal("D08-SV-A", ketQua.SinhVien.MaSV);
        Assert.Equal("Sinh viên D08 A", ketQua.SinhVien.HoTen);
        Assert.Equal(P1, ketQua.MaPhong);
        Assert.Equal("D08 Phong P1", ketQua.TenPhong);
        Assert.Equal(2, ketQua.DsLoaiVaSoLuong.Count);
        Assert.All(ketQua.DsLoaiVaSoLuong, d => Assert.Equal(1, d.SoLuong));
        var viTriX = ketQua.DsViTriLayHang.Where(v => v.MaLoai == X).ToList();
        Assert.Equal(3, viTriX.Count);
        Assert.Equal("K1", viTriX[0].ViTriKe);
        Assert.Equal("T1", viTriX[0].ViTriTu);
        Assert.Equal(2, viTriX[0].SoThietBiSanSang);
        Assert.All(viTriX, v => Assert.Equal("D08 Phong P1", v.TenPhong));
        Assert.Equal(2, ketQua.DsViTriLayHang.Count(v => v.MaLoai == Y));
    }

    [CanKetNoiTestFact]
    public async Task UC07_KiemNguon_PhieuYeuCau_DungThoiDiemHetHanGiuCho_LaHetHan()
    {
        var ketQua = await TheoPhieuYeuCauAsync(YcDungHan);

        Assert.False(ketQua.HopLe);
        Assert.Equal("GIU_CHO_HET_HAN", Assert.Single(ketQua.DsLyDo).MaLoi);
    }

    [CanKetNoiTestFact]
    public async Task UC07_KiemNguon_PhieuYeuCau_QuaHanGiuCho_LaHetHan()
    {
        var ketQua = await TheoPhieuYeuCauAsync(YcQuaHan);

        Assert.False(ketQua.HopLe);
        Assert.Equal("GIU_CHO_HET_HAN", Assert.Single(ketQua.DsLyDo).MaLoi);
    }

    [CanKetNoiTestFact]
    public async Task UC07_KiemNguon_PhieuYeuCau_ChuaDuyet_TrangThaiKhongHopLe()
    {
        var ketQua = await TheoPhieuYeuCauAsync(YcChoDuyet);

        Assert.False(ketQua.HopLe);
        Assert.Equal("TRANG_THAI_KHONG_HOP_LE", Assert.Single(ketQua.DsLyDo).MaLoi);
    }

    [CanKetNoiTestFact]
    public async Task UC07_KiemNguon_PhieuYeuCau_TheSinhVienHetHan_CoLyDo()
    {
        var ketQua = await TheoPhieuYeuCauAsync(YcTheHetHan);

        Assert.False(ketQua.HopLe);
        Assert.Equal("SV_THE_HET_HAN", Assert.Single(ketQua.DsLyDo).MaLoi);
    }

    [CanKetNoiTestFact]
    public async Task UC07_KiemNguon_PhieuYeuCau_SinhVienBiKhoa_CoLyDo()
    {
        var ketQua = await TheoPhieuYeuCauAsync(YcTamKhoa);

        Assert.False(ketQua.HopLe);
        Assert.Equal("SV_BI_KHOA_QUYEN_MUON", Assert.Single(ketQua.DsLyDo).MaLoi);
    }

    [CanKetNoiTestFact]
    public async Task UC07_KiemNguon_PhieuYeuCau_KhongTonTai_Tra404()
    {
        var loi = await LoiAsync(bo => bo.KiemTraPhieuYeuCauAsync(Nv, "YC-261120-999999", Ct));

        Assert.Equal("KHONG_TIM_THAY", loi.MaLoi);
        Assert.Equal(404, loi.MaHttp);
    }

    [CanKetNoiTestFact]
    public async Task UC07_KiemNguon_PhieuYeuCau_SaiDinhDang_Tra400()
    {
        var loi = await LoiAsync(bo => bo.KiemTraPhieuYeuCauAsync(Nv, "YC-1", Ct));

        Assert.Equal("DU_LIEU_KHONG_HOP_LE", loi.MaLoi);
    }

    [CanKetNoiTestFact]
    public async Task UC07_KiemNguon_PhieuYeuCau_PhongChuaCapQuyen_Tra403()
    {
        var loi = await LoiAsync(bo => bo.KiemTraPhieuYeuCauAsync(Nv, YcPhongKhac, Ct));

        Assert.Equal("PHONG_CHUA_CAP_QUYEN", loi.MaLoi);
        Assert.Equal(403, loi.MaHttp);
    }

    [CanKetNoiTestFact]
    public async Task UC07_KiemNguon_DangKyChoHopLe_TraSinhVienLoaiMotThietBi()
    {
        var ketQua = await TheoDangKyChoAsync(DkHopLe);

        Assert.True(ketQua.HopLe);
        Assert.Equal("D08-SV-B", ketQua.SinhVien.MaSV);
        var dong = Assert.Single(ketQua.DsLoaiVaSoLuong);
        Assert.Equal(Y, dong.MaLoai);
        Assert.Equal(1, dong.SoLuong);
        Assert.Equal(2, ketQua.DsViTriLayHang.Count);
    }

    [CanKetNoiTestFact]
    public async Task UC07_KiemNguon_DangKyCho_DungThoiDiemHetHanGiuCho_LaHetHan()
    {
        var ketQua = await TheoDangKyChoAsync(DkDungHan);

        Assert.False(ketQua.HopLe);
        Assert.Equal("GIU_CHO_HET_HAN", Assert.Single(ketQua.DsLyDo).MaLoi);
    }

    [CanKetNoiTestFact]
    public async Task UC07_KiemNguon_DangKyCho_DaNhan_TrangThaiKhongHopLe()
    {
        var ketQua = await TheoDangKyChoAsync(DkDaNhan);

        Assert.False(ketQua.HopLe);
        Assert.Equal("TRANG_THAI_KHONG_HOP_LE", Assert.Single(ketQua.DsLyDo).MaLoi);
    }

    [CanKetNoiTestFact]
    public async Task UC07_KiemNguon_DangKyCho_KhongTonTaiHoacSaiDinhDang()
    {
        var khongCo = await LoiAsync(bo => bo.KiemTraDangKyChoAsync(Nv, "DK-261120-999999", Ct));
        var saiDinhDang = await LoiAsync(bo => bo.KiemTraDangKyChoAsync(Nv, "DK-1", Ct));

        Assert.Equal("KHONG_TIM_THAY", khongCo.MaLoi);
        Assert.Equal("DU_LIEU_KHONG_HOP_LE", saiDinhDang.MaLoi);
    }

    [CanKetNoiTestFact]
    public async Task UC07_KiemNguon_TrucTiepHopLe_DuKhaDung()
    {
        var ketQua = await TrucTiepAsync("D08-SV-A", P1, new LoaiVaSoLuongYeuCau(X, 1));

        Assert.True(ketQua.HopLe);
        Assert.Equal("D08-SV-A", ketQua.SinhVien.MaSV);
        Assert.Equal("D08 Loai X", Assert.Single(ketQua.DsLoaiVaSoLuong).TenLoai);
        Assert.Equal(3, ketQua.DsViTriLayHang.Count);
    }

    [CanKetNoiTestFact]
    public async Task UC07_KiemNguon_TrucTiep_KhongDuKhaDung_CoLyDo()
    {
        var ketQua = await TrucTiepAsync("D08-SV-A", P1, new LoaiVaSoLuongYeuCau(X, 2));

        Assert.False(ketQua.HopLe);
        Assert.Equal("SO_LUONG_KHA_DUNG_THAY_DOI", Assert.Single(ketQua.DsLyDo).MaLoi);
    }

    [CanKetNoiTestFact]
    public async Task UC07_KiemNguon_TrucTiep_VuotGioiHan_CoLyDo()
    {
        var ketQua = await TrucTiepAsync("D08-SV-B", P1, new LoaiVaSoLuongYeuCau(X, 4));

        Assert.False(ketQua.HopLe);
        Assert.Contains(ketQua.DsLyDo, d => d.MaLoi == "VUOT_GIOI_HAN_THIET_BI");
    }

    [CanKetNoiTestFact]
    public async Task UC07_KiemNguon_TrucTiep_TheHetHan_CoLyDo()
    {
        var ketQua = await TrucTiepAsync("D08-SV-HET-HAN", P1, new LoaiVaSoLuongYeuCau(X, 1));

        Assert.False(ketQua.HopLe);
        Assert.Equal("SV_THE_HET_HAN", Assert.Single(ketQua.DsLyDo).MaLoi);
    }

    [CanKetNoiTestFact]
    public async Task UC07_KiemNguon_TrucTiep_SinhVienKhongTonTai_Tra404()
    {
        var loi = await LoiAsync(bo => bo.KiemTraTrucTiepAsync(Nv, "D08-SV-KHONG-CO", P1, [new LoaiVaSoLuongYeuCau(X, 1)], Ct));

        Assert.Equal("KHONG_TIM_THAY", loi.MaLoi);
    }

    [CanKetNoiTestFact]
    public async Task UC07_KiemNguon_TrucTiep_PhongChuaCapQuyen_Tra403()
    {
        var loi = await LoiAsync(bo => bo.KiemTraTrucTiepAsync(Nv, "D08-SV-A", P2, [new LoaiVaSoLuongYeuCau(X, 1)], Ct));

        Assert.Equal("PHONG_CHUA_CAP_QUYEN", loi.MaLoi);
    }

    [CanKetNoiTestFact]
    public async Task UC07_KiemNguon_TrucTiep_LoaiKhongTonTai_Tra400()
    {
        var loi = await LoiAsync(bo => bo.KiemTraTrucTiepAsync(Nv, "D08-SV-A", P1, [new LoaiVaSoLuongYeuCau(LoaiKhongCo, 1)], Ct));

        Assert.Equal("DU_LIEU_KHONG_HOP_LE", loi.MaLoi);
    }

    [CanKetNoiTestFact]
    public async Task UC07_KiemNguon_KhongGhiCoSoDuLieu()
    {
        await using var gd = await GiaoDichTestRollback.BatDauAsync();
        await using var db = await TaoContextAsync(gd);
        var bo = TaoBo(db, P1);

        await bo.KiemTraPhieuYeuCauAsync(Nv, YcHopLe, Ct);
        await bo.KiemTraDangKyChoAsync(Nv, DkHopLe, Ct);
        await bo.KiemTraTrucTiepAsync(Nv, "D08-SV-A", P1, [new LoaiVaSoLuongYeuCau(X, 1)], Ct);

        var trangThaiYc = await db.Database.SqlQuery<string>($"""
            SELECT trang_thai AS "Value" FROM muon_tra.phieu_yeu_cau_muon WHERE ma_yeu_cau = {YcHopLe}
            """).SingleAsync(Ct);
        var trangThaiDk = await db.Database.SqlQuery<string>($"""
            SELECT trang_thai AS "Value" FROM muon_tra.dang_ky_cho WHERE ma_dang_ky = {DkHopLe}
            """).SingleAsync(Ct);
        var soPhieuMuon = await db.Database.SqlQuery<int>($"""
            SELECT count(*)::integer AS "Value" FROM muon_tra.phieu_muon
            """).SingleAsync(Ct);
        Assert.Equal("DA_DUYET", trangThaiYc);
        Assert.Equal("DA_THONG_BAO", trangThaiDk);
        Assert.Equal(0, soPhieuMuon);
        Assert.False(db.ChangeTracker.HasChanges());
    }
}
