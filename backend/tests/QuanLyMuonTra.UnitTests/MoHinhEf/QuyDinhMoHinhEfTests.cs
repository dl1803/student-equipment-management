using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using QuanLyMuonTra.Modules.QuyDinh.Domain;
using QuanLyMuonTra.Modules.QuyDinh.Infrastructure;

namespace QuanLyMuonTra.UnitTests.MoHinhEf;

public sealed class QuyDinhMoHinhEfTests
{
    private static readonly IModel MoHinh =
        MoHinhEfKiemTra.LayMoHinh<QuyDinhMoHinhDbContext>(o => new QuyDinhMoHinhDbContext(o));

    [Fact]
    public void UC13_ThamSoHeThong_KhopDdl()
    {
        var entity = MoHinhEfKiemTra.LayBang(MoHinh, typeof(ThamSoHeThong), "quy_dinh", "tham_so_he_thong");
        MoHinhEfKiemTra.KiemTraKhoaChinh(entity, "pk_tham_so_he_thong", "id");
        MoHinhEfKiemTra.KiemTraCot(
            entity,
            "id:NOT NULL",
            "so_ngay_khoa:NOT NULL",
            "thoi_gian_giu_cho_phut:NOT NULL",
            "an_han_phut:NOT NULL",
            "so_gio_gia_han_toi_da:NOT NULL",
            "nguong_vi_pham:NOT NULL",
            "so_lan_dang_nhap_sai_toi_da:NOT NULL",
            "gio_mo_cua:NULL",
            "gio_dong_cua:NULL");
        MoHinhEfKiemTra.KiemTraChiMuc(entity);
        MoHinhEfKiemTra.KiemTraKhoaNgoai(entity);
        MoHinhEfKiemTra.KiemTraRangBuocCheck(
            entity,
            "ck_tshs_mot_dong",
            "ck_tshs_so_ngay_khoa",
            "ck_tshs_thoi_gian_giu_cho",
            "ck_tshs_an_han",
            "ck_tshs_so_gio_gia_han",
            "ck_tshs_nguong_vi_pham",
            "ck_tshs_so_lan_dang_nhap_sai",
            "ck_tshs_gio_quay");
    }

    [Fact]
    public void UC13_ThamSoHeThong_MacDinhKhopSrs33_GioQuayKhongCoMacDinh()
    {
        var entity = MoHinh.FindEntityType(typeof(ThamSoHeThong))!;
        var bang = MoHinhEfKiemTra.Dinh(entity);
        var mongDoi = new Dictionary<string, object?>
        {
            ["id"] = (short)1,
            ["so_ngay_khoa"] = 3,
            ["thoi_gian_giu_cho_phut"] = 30,
            ["an_han_phut"] = 15,
            ["so_gio_gia_han_toi_da"] = 2,
            ["nguong_vi_pham"] = 3,
            ["so_lan_dang_nhap_sai_toi_da"] = 5,
            ["gio_mo_cua"] = null,
            ["gio_dong_cua"] = null,
        };

        foreach (var property in entity.GetProperties())
        {
            var cot = property.GetColumnName(bang)!;
            Assert.Equal(mongDoi[cot], property.GetDefaultValue(bang));
        }
    }

    [Fact]
    public void UC13_ThamSoHeThong_IdKhongTuSinh()
    {
        var entity = MoHinh.FindEntityType(typeof(ThamSoHeThong))!;
        Assert.Equal(ValueGenerated.Never, entity.FindProperty(nameof(ThamSoHeThong.Id))!.ValueGenerated);
    }

    [Fact]
    public void UC13_KhungPhat_KhopDdl()
    {
        var entity = MoHinhEfKiemTra.LayBang(MoHinh, typeof(KhungPhat), "quy_dinh", "khung_phat");
        MoHinhEfKiemTra.KiemTraKhoaChinh(entity, "pk_khung_phat", "ma_khung");
        MoHinhEfKiemTra.KiemTraCot(
            entity,
            "ma_khung:NOT NULL",
            "ten_khung:NOT NULL",
            "ty_le_phan_tram:NOT NULL",
            "dang_su_dung:NOT NULL");
        MoHinhEfKiemTra.KiemTraChiMuc(entity);
        MoHinhEfKiemTra.KiemTraKhoaNgoai(entity);
        MoHinhEfKiemTra.KiemTraRangBuocCheck(entity, "ck_kp_ten_khung_khong_rong", "ck_kp_ty_le");
    }

    [Fact]
    public void UC13_KhungPhat_TyLaLaNumeric5Hai_MacDinhDangSuDungTrue()
    {
        var entity = MoHinh.FindEntityType(typeof(KhungPhat))!;
        var bang = MoHinhEfKiemTra.Dinh(entity);
        Assert.Equal("numeric(5,2)", entity.FindProperty(nameof(KhungPhat.TyLePhanTram))!.GetColumnType(bang));
        Assert.Equal((object)true, entity.FindProperty(nameof(KhungPhat.DangSuDung))!.GetDefaultValue(bang));
    }

    [Fact]
    public void UC13_KhungPhat_MaKhungKhongTuSinh()
    {
        var entity = MoHinh.FindEntityType(typeof(KhungPhat))!;
        Assert.Equal(ValueGenerated.Never, entity.FindPrimaryKey()!.Properties[0].ValueGenerated);
    }

    [Fact]
    public void UC13_QuyDinhMuon_KhopDdl_KhongKhaiBaoFkLienModule()
    {
        var entity = MoHinhEfKiemTra.LayBang(MoHinh, typeof(QuyDinhMuon), "quy_dinh", "quy_dinh_muon");
        MoHinhEfKiemTra.KiemTraKhoaChinh(entity, "pk_quy_dinh_muon", "ma_quy_dinh");
        MoHinhEfKiemTra.KiemTraCot(
            entity,
            "ma_quy_dinh:NOT NULL",
            "ma_nhom:NOT NULL",
            "ma_loai:NULL",
            "so_thiet_bi_toi_da:NULL",
            "thoi_gian_muon_toi_da_gio:NOT NULL");
        MoHinhEfKiemTra.KiemTraChiMuc(
            entity,
            "ux_qdm_chung|ma_nhom|UNIQUE|ma_loai IS NULL",
            "ux_qdm_ngoai_le|ma_nhom,ma_loai|UNIQUE|ma_loai IS NOT NULL",
            "ix_qdm_loai|ma_loai||ma_loai IS NOT NULL");
        MoHinhEfKiemTra.KiemTraKhoaNgoai(entity);
        MoHinhEfKiemTra.KiemTraRangBuocCheck(
            entity,
            "ck_qdm_chung_hoac_ngoai_le",
            "ck_qdm_so_thiet_bi",
            "ck_qdm_thoi_gian");
    }
}

internal sealed class QuyDinhMoHinhDbContext(DbContextOptions<QuyDinhMoHinhDbContext> options) : DbContext(options)
{
    protected override void OnModelCreating(ModelBuilder modelBuilder) => modelBuilder.ApplyQuyDinhConfigurations();
}
