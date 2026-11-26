using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using QuanLyMuonTra.Modules.ViPham.Domain;
using QuanLyMuonTra.Modules.ViPham.Infrastructure;

namespace QuanLyMuonTra.UnitTests.MoHinhEf;

public sealed class ViPhamMoHinhEfTests
{
    private static readonly IModel MoHinh =
        MoHinhEfKiemTra.LayMoHinh<ViPhamMoHinhDbContext>(o => new ViPhamMoHinhDbContext(o));

    [Fact]
    public void UC09_BanGhiViPham_KhopDdl_KhongKhaiBaoFk()
    {
        var entity = MoHinhEfKiemTra.LayBang(MoHinh, typeof(BanGhiViPham), "vi_pham", "vi_pham");
        MoHinhEfKiemTra.KiemTraKhoaChinh(entity, "pk_vi_pham", "ma_vi_pham");
        MoHinhEfKiemTra.KiemTraCot(
            entity,
            "ma_vi_pham:NOT NULL",
            "ma_sv:NOT NULL",
            "loai_vi_pham:NOT NULL",
            "thoi_diem_ghi_nhan:NOT NULL",
            "hoc_ky:NOT NULL",
            "ma_bien_ban:NULL",
            "ma_yeu_cau:NULL",
            "ma_dang_ky:NULL");
        MoHinhEfKiemTra.KiemTraChiMuc(
            entity,
            "ix_vp_sv_hoc_ky|ma_sv,hoc_ky||",
            "ux_vp_bien_ban_loai|ma_bien_ban,loai_vi_pham|UNIQUE|ma_bien_ban IS NOT NULL",
            "ux_vp_yeu_cau|ma_yeu_cau|UNIQUE|ma_yeu_cau IS NOT NULL",
            "ux_vp_dang_ky|ma_dang_ky|UNIQUE|ma_dang_ky IS NOT NULL");
        MoHinhEfKiemTra.KiemTraKhoaNgoai(entity);
        MoHinhEfKiemTra.KiemTraRangBuocCheck(
            entity,
            "ck_vp_loai_vi_pham",
            "ck_vp_dung_mot_chung_tu",
            "ck_vp_loai_theo_chung_tu");
    }

    [Fact]
    public void UC09_BanGhiViPham_MaViPhamKhongTuSinh()
    {
        var entity = MoHinh.FindEntityType(typeof(BanGhiViPham))!;

        Assert.Equal(ValueGenerated.Never, entity.FindPrimaryKey()!.Properties[0].ValueGenerated);
    }

    [Fact]
    public void UC09_BienBanViPham_KhopDdl_KhongKhaiBaoFk()
    {
        var entity = MoHinhEfKiemTra.LayBang(MoHinh, typeof(BienBanViPham), "vi_pham", "bien_ban_vi_pham");
        MoHinhEfKiemTra.KiemTraKhoaChinh(entity, "pk_bien_ban_vi_pham", "ma_bien_ban");
        MoHinhEfKiemTra.KiemTraCot(
            entity,
            "ma_bien_ban:NOT NULL",
            "ma_phieu_muon:NOT NULL",
            "ma_nv_lap:NOT NULL",
            "ngay_lap:NOT NULL",
            "tong_tien:NOT NULL",
            "trang_thai:NOT NULL",
            "thoi_diem_nop:NULL",
            "xmin:NOT NULL");
        MoHinhEfKiemTra.KiemTraChiMuc(
            entity,
            "ix_bb_phieu|ma_phieu_muon||",
            "ix_bb_chua_nop|ma_phieu_muon||trang_thai = 'CHUA_NOP'");
        MoHinhEfKiemTra.KiemTraKhoaNgoai(entity);
        MoHinhEfKiemTra.KiemTraRangBuocCheck(
            entity,
            "ck_bb_ma_bien_ban",
            "ck_bb_trang_thai",
            "ck_bb_tong_tien",
            "ck_bb_thoi_diem_nop");
    }

    [Fact]
    public void UC09_BienBanViPham_KhoaPhuXminVaKieuTien()
    {
        var entity = MoHinh.FindEntityType(typeof(BienBanViPham))!;
        var bang = MoHinhEfKiemTra.Dinh(entity);
        var khoaPhu = entity.GetKeys().Single(k => k != entity.FindPrimaryKey());

        Assert.Equal("uq_bb_ma_bien_ban_phieu", khoaPhu.GetName(bang));
        Assert.Equal(["ma_bien_ban", "ma_phieu_muon"], khoaPhu.Properties.Select(p => p.GetColumnName(bang)!));
        Assert.True(entity.FindProperty("xmin")!.IsConcurrencyToken);
        Assert.Equal("numeric(14,0)", entity.FindProperty(nameof(BienBanViPham.TongTien))!.GetColumnType(bang));
    }

    [Fact]
    public void UC09_ChiTietBienBan_KhopDdl_KhongAnhXaCotDoTriggerQuanLy()
    {
        var entity = MoHinhEfKiemTra.LayBang(MoHinh, typeof(ChiTietBienBan), "vi_pham", "chi_tiet_bien_ban");
        MoHinhEfKiemTra.KiemTraKhoaChinh(entity, "pk_chi_tiet_bien_ban", "ma_bien_ban", "ma_thiet_bi", "loai_vi_pham");
        MoHinhEfKiemTra.KiemTraCot(
            entity,
            "ma_bien_ban:NOT NULL",
            "ma_phieu_muon:NOT NULL",
            "ma_thiet_bi:NOT NULL",
            "loai_vi_pham:NOT NULL",
            "so_ngay_tre:NULL",
            "so_tien:NOT NULL",
            "ma_khung:NULL");
        MoHinhEfKiemTra.KiemTraChiMuc(entity, "ix_ctbb_khung|ma_khung||ma_khung IS NOT NULL");
        MoHinhEfKiemTra.KiemTraKhoaNgoai(entity);
        MoHinhEfKiemTra.KiemTraRangBuocCheck(
            entity,
            "ck_ctbb_loai_vi_pham",
            "ck_ctbb_so_tien",
            "ck_ctbb_so_ngay_tre",
            "ck_ctbb_khung_phat");
    }

    [Fact]
    public void UC09_ChiTietBienBan_SoTienLaNumeric14Khong()
    {
        var entity = MoHinh.FindEntityType(typeof(ChiTietBienBan))!;
        var bang = MoHinhEfKiemTra.Dinh(entity);

        Assert.Equal("numeric(14,0)", entity.FindProperty(nameof(ChiTietBienBan.SoTien))!.GetColumnType(bang));
    }
}

internal sealed class ViPhamMoHinhDbContext(DbContextOptions<ViPhamMoHinhDbContext> options) : DbContext(options)
{
    protected override void OnModelCreating(ModelBuilder modelBuilder) => modelBuilder.ApplyViPhamConfigurations();
}

