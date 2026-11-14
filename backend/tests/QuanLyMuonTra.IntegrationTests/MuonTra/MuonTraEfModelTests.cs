using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using QuanLyMuonTra.Modules.MuonTra.Domain;

namespace QuanLyMuonTra.IntegrationTests.MuonTra;

public class MuonTraEfModelTests
{
    private static IModel TaoModel()
    {
        var options = new DbContextOptionsBuilder<MuonTraTestContext>()
            .UseNpgsql("Host=localhost;Database=khong_ket_noi")
            .Options;
        using var db = new MuonTraTestContext(options);
        return db.Model;
    }

    [Fact]
    public void DDL_AnhXaEf_NamBangDungSchemaVaTenBang()
    {
        var model = TaoModel();

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
            var entity = model.FindEntityType(kieu);
            Assert.NotNull(entity);
            Assert.Equal("muon_tra", entity!.GetSchema());
            Assert.Equal(tenBang, entity.GetTableName());
        }
    }

    [Fact]
    public void KT5_AnhXaEf_BaAggregateDungXminLamConcurrencyToken()
    {
        var model = TaoModel();

        foreach (var kieu in new[] { typeof(PhieuYeuCauMuon), typeof(DangKyCho), typeof(PhieuMuon) })
        {
            var xmin = model.FindEntityType(kieu)!.FindProperty("xmin");
            Assert.NotNull(xmin);
            Assert.True(xmin!.IsConcurrencyToken);
        }
    }

    [Fact]
    public void DDL_AnhXaEf_CotTrangThaiVaKetQuaLuuDuoiDangChuoi()
    {
        var model = TaoModel();

        var cacCot = new[]
        {
            (typeof(PhieuYeuCauMuon), nameof(PhieuYeuCauMuon.TrangThai)),
            (typeof(DangKyCho), nameof(DangKyCho.TrangThai)),
            (typeof(PhieuMuon), nameof(PhieuMuon.TrangThai)),
            (typeof(ChiTietPhieuMuon), nameof(ChiTietPhieuMuon.KetQuaKiemTra))
        };

        foreach (var (kieu, tenCot) in cacCot)
        {
            var thuocTinh = model.FindEntityType(kieu)!.FindProperty(tenCot);
            Assert.NotNull(thuocTinh);
            Assert.Equal(typeof(string), thuocTinh!.GetValueConverter()!.ProviderClrType);
        }
    }
}
