using System.Globalization;
using QuanLyMuonTra.IntegrationTests.Database;
using QuanLyMuonTra.IntegrationTests.MuonTra;
using QuanLyMuonTra.Modules.BaoTri.Application;
using QuanLyMuonTra.SharedKernel;

namespace QuanLyMuonTra.IntegrationTests.BaoTri;

public sealed class BaoTriDsCsdlTests
{
    private static readonly Guid MaNv = Guid.Parse(BaoTriTestSupport.MaNv);
    private static readonly Guid PhongP1 = Guid.Parse(BaoTriTestSupport.MaPhongP1);
    private static readonly Guid PhongP2 = Guid.Parse(BaoTriTestSupport.MaPhongP2);

    [CanKetNoiTestFact]
    public async Task UC11_DsDenHan_PhongDuocCapQuyen_KhopOracleD12()
    {
        await using var mt = await BaoTriMoiTruong.TaoAsync();
        var oracle = await OracleCsv.DocAsync("BaoTri", "D12-N-oracle-bao-tri.csv");
        var kyVong = oracle
            .Where(d => d["MaPhong"] == BaoTriTestSupport.MaPhongP1 && d["ThuTuDenHanTai20261123"] != string.Empty)
            .OrderBy(d => int.Parse(d["ThuTuDenHanTai20261123"], CultureInfo.InvariantCulture))
            .Select(d => d["MaThietBi"] + "|" + d["NgayDenHan"])
            .ToList();

        var ketQua = await mt.Lay<DsThietBiDenHanBaoTriHandler>().XuLyAsync(MaNv, null, 1, 100, CancellationToken.None);

        Assert.NotEmpty(kyVong);
        Assert.Equal(kyVong.Count, ketQua.TongSo);
        Assert.Equal(
            kyVong,
            ketQua.DanhSach.Select(d => d.MaThietBi + "|" + d.NgayDenHan.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture)).ToList());
    }

    [CanKetNoiTestFact]
    public async Task UC11_DsDenHan_ThietBiDangMuonCoMatNhungKhongGomDangBaoTri()
    {
        await using var mt = await BaoTriMoiTruong.TaoAsync();

        var ketQua = await mt.Lay<DsThietBiDenHanBaoTriHandler>().XuLyAsync(MaNv, null, 1, 100, CancellationToken.None);

        var dsMa = ketQua.DanhSach.Select(d => d.MaThietBi).ToList();
        Assert.Contains("D12-TB-05", dsMa);
        Assert.Equal("DANG_DUOC_MUON", ketQua.DanhSach.Single(d => d.MaThietBi == "D12-TB-05").TinhTrang);
        Assert.DoesNotContain("D12-TB-06", dsMa);
        Assert.DoesNotContain("D12-TB-04", dsMa);
    }

    [CanKetNoiTestFact]
    public async Task UC11_DsDenHan_PhongKhongDuocCapQuyen_TraRongKhongLoThietBi()
    {
        await using var mt = await BaoTriMoiTruong.TaoAsync();
        var handler = mt.Lay<DsThietBiDenHanBaoTriHandler>();

        var theoP2 = await handler.XuLyAsync(MaNv, PhongP2, 1, 20, CancellationToken.None);
        var tatCa = await handler.XuLyAsync(MaNv, null, 1, 100, CancellationToken.None);

        Assert.Equal(0, theoP2.TongSo);
        Assert.Empty(theoP2.DanhSach);
        Assert.DoesNotContain(tatCa.DanhSach, d => d.MaPhong == PhongP2);
        Assert.DoesNotContain(tatCa.DanhSach, d => d.MaThietBi == "D12-TB-12");
    }

    [CanKetNoiTestFact]
    public async Task UC11_DsDenHan_PhanTrangOnDinhVaGiuTongSo()
    {
        await using var mt = await BaoTriMoiTruong.TaoAsync();
        var handler = mt.Lay<DsThietBiDenHanBaoTriHandler>();

        var tatCa = await handler.XuLyAsync(MaNv, PhongP1, 1, 100, CancellationToken.None);
        var trang1 = await handler.XuLyAsync(MaNv, PhongP1, 1, 2, CancellationToken.None);
        var trang2 = await handler.XuLyAsync(MaNv, PhongP1, 2, 2, CancellationToken.None);
        var quaCuoi = await handler.XuLyAsync(MaNv, PhongP1, 99, 2, CancellationToken.None);

        Assert.Equal(tatCa.TongSo, trang1.TongSo);
        Assert.Equal(tatCa.TongSo, trang2.TongSo);
        Assert.Equal(tatCa.DanhSach.Take(2).Select(d => d.MaThietBi), trang1.DanhSach.Select(d => d.MaThietBi));
        Assert.Equal(tatCa.DanhSach.Skip(2).Take(2).Select(d => d.MaThietBi), trang2.DanhSach.Select(d => d.MaThietBi));
        Assert.Equal(tatCa.TongSo, quaCuoi.TongSo);
        Assert.Empty(quaCuoi.DanhSach);
    }

    [CanKetNoiTestFact]
    public async Task UC11_DsChoTiepNhan_KhopOracleVaCoMaPhieuMuon()
    {
        await using var mt = await BaoTriMoiTruong.TaoAsync();
        var oracle = await OracleCsv.DocAsync("BaoTri", "D12-N-oracle-bao-tri.csv");
        var kyVong = oracle
            .Where(d => d["MaPhong"] == BaoTriTestSupport.MaPhongP1 && d["ChoTiepNhanSuaChua"] == "CO")
            .Select(d => d["MaThietBi"])
            .OrderBy(d => d, StringComparer.Ordinal)
            .ToList();

        var ketQua = await mt.Lay<DsChoTiepNhanSuaChuaHandler>().XuLyAsync(MaNv, null, 1, 100, CancellationToken.None);

        Assert.Equal(kyVong, ketQua.DanhSach.Select(d => d.MaThietBi).ToList());
        Assert.Equal(kyVong.Count, ketQua.TongSo);
        Assert.Equal("PM-261123-991203", ketQua.DanhSach.Single(d => d.MaThietBi == "D12-TB-08").MaPhieuMuon);
    }

    [CanKetNoiTestFact]
    public async Task UC11_DsChoTiepNhan_LocTheoPhongVaKhongLoPhongKhacQuyen()
    {
        await using var mt = await BaoTriMoiTruong.TaoAsync();
        var handler = mt.Lay<DsChoTiepNhanSuaChuaHandler>();

        var theoP2 = await handler.XuLyAsync(MaNv, PhongP2, 1, 20, CancellationToken.None);
        var theoP1 = await handler.XuLyAsync(MaNv, PhongP1, 1, 20, CancellationToken.None);

        Assert.Equal(0, theoP2.TongSo);
        Assert.Equal(1, theoP1.TongSo);
    }

    [CanKetNoiTestFact]
    public async Task UC11_DsBaoTri_TrangNgoaiKhoang_Nem400_KhongChamCsdl()
    {
        await using var mt = await BaoTriMoiTruong.TaoAsync();

        var loi = await Assert.ThrowsAsync<LoiNghiepVuException>(
            () => mt.Lay<DsChoTiepNhanSuaChuaHandler>().XuLyAsync(MaNv, null, 0, 20, CancellationToken.None));

        Assert.Equal(400, loi.MaHttp);
    }
}
