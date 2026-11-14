using Microsoft.EntityFrameworkCore;
using Npgsql;
using QuanLyMuonTra.Modules.MuonTra.Domain;
using QuanLyMuonTra.Modules.MuonTra.Infrastructure;

namespace QuanLyMuonTra.IntegrationTests.MuonTra;

public class MuonTraDuLieuThuTests
{
    private static readonly Guid P1 = Guid.Parse("019c0000-0000-7000-8000-0000000d0b01");
    private static readonly Guid P2 = Guid.Parse("019c0000-0000-7000-8000-0000000d0b02");
    private static readonly Guid P3 = Guid.Parse("019c0000-0000-7000-8000-0000000d0b03");
    private static readonly Guid L1 = Guid.Parse("019c0000-0000-7000-8000-0000000d0c01");
    private static readonly Guid L2 = Guid.Parse("019c0000-0000-7000-8000-0000000d0c02");
    private static readonly Guid L3 = Guid.Parse("019c0000-0000-7000-8000-0000000d0c03");
    private static readonly Guid KhungDaDung = Guid.Parse("019c0000-0000-7000-8000-0000000d0d01");
    private static readonly Guid KhungChuaDung = Guid.Parse("019c0000-0000-7000-8000-0000000d0d02");
    private static readonly Guid MaNv = Guid.Parse("019c0000-0000-7000-8000-0000000d0a01");
    private static readonly DateTimeOffset ThoiDiem = new(2026, 11, 14, 8, 0, 0, TimeSpan.Zero);

    [CanKetNoiTestFact]
    public async Task BR24_DemThietBiDangCam_TheoSinhVienXuyenPhongVaKhongBiNoPhongKhacLamLech()
    {
        await using var gd = await GiaoDichTestRollback.BatDauAsync();
        await gd.NapDuLieuThuAsync();
        var reader = new MuonTraReader(gd.Db);

        Assert.Equal(3, await reader.DemThietBiDangCamAsync("D02-SV-A", CancellationToken.None));
        Assert.Equal(1, await reader.DemThietBiDangCamAsync("D02-SV-B", CancellationToken.None));
        Assert.Equal(0, await reader.DemThietBiDangCamAsync("D02-SV-C", CancellationToken.None));
        Assert.Equal(0, await reader.DemThietBiDangCamAsync("D02-SV-D", CancellationToken.None));
        Assert.Equal(0, await reader.DemThietBiDangCamAsync("D02-SV-KHONG-TON-TAI", CancellationToken.None));

        await using var lenh = new NpgsqlCommand(
            "SELECT count(*) FROM vi_pham.bien_ban_vi_pham b JOIN muon_tra.phieu_muon p ON p.ma_phieu = b.ma_phieu_muon WHERE p.ma_sv = 'D02-SV-C' AND b.trang_thai = 'CHUA_NOP'",
            gd.KetNoi,
            gd.GiaoDich);
        Assert.Equal(1L, (long)(await lenh.ExecuteScalarAsync())!);
    }

    [CanKetNoiTestFact]
    public async Task KT65_ThamChieuMuonTra_PhongLoaiThietBiKhungPhat_DungVoiDuLieuThu()
    {
        await using var gd = await GiaoDichTestRollback.BatDauAsync();
        await gd.NapDuLieuThuAsync();
        var provider = new ThamChieuMuonTraProvider(gd.Db);
        var ct = CancellationToken.None;

        Assert.True(await provider.CoThamChieuPhongAsync(P1, ct));
        Assert.True(await provider.CoThamChieuPhongAsync(P2, ct));
        Assert.False(await provider.CoThamChieuPhongAsync(P3, ct));
        Assert.False(await provider.CoThamChieuPhongAsync(Guid.NewGuid(), ct));

        Assert.True(await provider.CoThamChieuLoaiAsync(L1, ct));
        Assert.True(await provider.CoThamChieuLoaiAsync(L2, ct));
        Assert.False(await provider.CoThamChieuLoaiAsync(L3, ct));

        Assert.True(await provider.CoThamChieuThietBiAsync("D02-TB-01", ct));
        Assert.True(await provider.CoThamChieuThietBiAsync("D02-TB-07", ct));
        Assert.False(await provider.CoThamChieuThietBiAsync("D02-TB-08", ct));

        Assert.True(await provider.CoThamChieuKhungPhatAsync(KhungDaDung, ct));
        Assert.False(await provider.CoThamChieuKhungPhatAsync(KhungChuaDung, ct));
        Assert.True(await provider.CoThamChieuKhungAsync(KhungDaDung, ct));
        Assert.False(await provider.CoThamChieuKhungAsync(KhungChuaDung, ct));
    }

    [CanKetNoiTestFact]
    public async Task DDL_AnhXaEf_CotCuaNamBangKhopInformationSchemaTrenDbThat()
    {
        await using var gd = await GiaoDichTestRollback.BatDauAsync();

        var cacBang = new Dictionary<Type, string>
        {
            [typeof(PhieuYeuCauMuon)] = "phieu_yeu_cau_muon",
            [typeof(ChiTietYeuCauMuon)] = "chi_tiet_yeu_cau_muon",
            [typeof(DangKyCho)] = "dang_ky_cho",
            [typeof(PhieuMuon)] = "phieu_muon",
            [typeof(ChiTietPhieuMuon)] = "chi_tiet_phieu_muon"
        };

        foreach (var (kieu, tenBang) in cacBang)
        {
            var bang = Microsoft.EntityFrameworkCore.Metadata.StoreObjectIdentifier.Table(tenBang, "muon_tra");
            var cotEf = gd.Db.Model.FindEntityType(kieu)!
                .GetProperties()
                .Select(p => p.GetColumnName(bang))
                .Where(c => c != "xmin")
                .Select(c => c!)
                .OrderBy(c => c, StringComparer.Ordinal)
                .ToList();

            var cotDdl = new List<string>();
            await using var lenh = new NpgsqlCommand(
                "SELECT column_name FROM information_schema.columns WHERE table_schema = 'muon_tra' AND table_name = @bang",
                gd.KetNoi,
                gd.GiaoDich);
            lenh.Parameters.AddWithValue("bang", tenBang);
            await using (var doc = await lenh.ExecuteReaderAsync())
            {
                while (await doc.ReadAsync())
                {
                    cotDdl.Add(doc.GetString(0));
                }
            }

            cotDdl.Sort(StringComparer.Ordinal);
            Assert.Equal(cotDdl, cotEf);
        }
    }

    [CanKetNoiTestFact]
    public async Task TT01_YeuCau_DuyetRoiHuy_LuuQuaEfThoaCheckCuaDdl()
    {
        await using var gd = await GiaoDichTestRollback.BatDauAsync();
        await gd.NapDuLieuThuAsync();
        var hanGiuCho = ThoiDiem.AddMinutes(30);

        var phieu = await gd.Db.Set<PhieuYeuCauMuon>().SingleAsync(p => p.MaYeuCau == "YC-261114-990001");
        phieu.Duyet(MaNv, ThoiDiem, hanGiuCho);
        await gd.Db.SaveChangesAsync();
        gd.Db.ChangeTracker.Clear();

        var daDuyet = await gd.Db.Set<PhieuYeuCauMuon>().SingleAsync(p => p.MaYeuCau == "YC-261114-990001");
        Assert.Equal(TrangThaiYeuCauMuon.DaDuyet, daDuyet.TrangThai);
        Assert.Equal(MaNv, daDuyet.MaNvDuyet);
        Assert.Equal(ThoiDiem, daDuyet.ThoiDiemDuyet);
        Assert.Equal(hanGiuCho, daDuyet.HanGiuCho);

        daDuyet.Huy();
        await gd.Db.SaveChangesAsync();
        gd.Db.ChangeTracker.Clear();

        var daHuy = await gd.Db.Set<PhieuYeuCauMuon>().SingleAsync(p => p.MaYeuCau == "YC-261114-990001");
        Assert.Equal(TrangThaiYeuCauMuon.DaHuy, daHuy.TrangThai);
        Assert.Equal(hanGiuCho, daHuy.HanGiuCho);
    }

    [CanKetNoiTestFact]
    public async Task TT05_DangKyCho_ThongBaoRoiRoiHuy_LuuQuaEfThoaCheckCuaDdl()
    {
        await using var gd = await GiaoDichTestRollback.BatDauAsync();
        await gd.NapDuLieuThuAsync();
        var hanGiuCho = ThoiDiem.AddMinutes(30);

        var dangKy = await gd.Db.Set<DangKyCho>().SingleAsync(d => d.MaDangKy == "DK-261114-990001");
        dangKy.ThongBaoDenLuot(hanGiuCho);
        await gd.Db.SaveChangesAsync();
        gd.Db.ChangeTracker.Clear();

        var daThongBao = await gd.Db.Set<DangKyCho>().SingleAsync(d => d.MaDangKy == "DK-261114-990001");
        Assert.Equal(TrangThaiDangKyCho.DaThongBao, daThongBao.TrangThai);
        Assert.Equal(hanGiuCho, daThongBao.HanGiuCho);

        daThongBao.Roi();
        await gd.Db.SaveChangesAsync();
        gd.Db.ChangeTracker.Clear();

        var daHuy = await gd.Db.Set<DangKyCho>().SingleAsync(d => d.MaDangKy == "DK-261114-990001");
        Assert.Equal(TrangThaiDangKyCho.DaHuy, daHuy.TrangThai);
    }

    [CanKetNoiTestFact]
    public async Task TT09_PhieuMuon_QuaHanVaXetDaTra_LuuQuaEfVaDocDuocKetQuaKiemTra()
    {
        await using var gd = await GiaoDichTestRollback.BatDauAsync();
        await gd.NapDuLieuThuAsync();

        var phieuQuaHan = await gd.Db.Set<PhieuMuon>().SingleAsync(p => p.MaPhieu == "PM-261114-990001");
        phieuQuaHan.DanhDauQuaHan();
        await gd.Db.SaveChangesAsync();

        var phieuDaTra = await gd.Db.Set<PhieuMuon>().Include(p => p.ChiTiet).SingleAsync(p => p.MaPhieu == "PM-261114-990004");
        Assert.Equal(KetQuaKiemTra.HuHong, phieuDaTra.ChiTiet.Single().KetQuaKiemTra);
        phieuDaTra.XetDaTra();
        await gd.Db.SaveChangesAsync();
        gd.Db.ChangeTracker.Clear();

        var daLuuQuaHan = await gd.Db.Set<PhieuMuon>().SingleAsync(p => p.MaPhieu == "PM-261114-990001");
        var daLuuDaTra = await gd.Db.Set<PhieuMuon>().SingleAsync(p => p.MaPhieu == "PM-261114-990004");
        Assert.Equal(TrangThaiPhieuMuon.QuaHan, daLuuQuaHan.TrangThai);
        Assert.Equal(TrangThaiPhieuMuon.DaTra, daLuuDaTra.TrangThai);
    }
}
