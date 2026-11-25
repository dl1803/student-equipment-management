using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using QuanLyMuonTra.Modules.BaoTri.Domain;
using QuanLyMuonTra.Modules.BaoTri.Infrastructure;

namespace QuanLyMuonTra.UnitTests.MoHinhEf;

public sealed class BaoTriMoHinhEfTests
{
    private static readonly IModel MoHinh =
        MoHinhEfKiemTra.LayMoHinh<BaoTriMoHinhDbContext>(o => new BaoTriMoHinhDbContext(o));

    [Fact]
    public void UC11_BaoTriSuaChua_KhopDdl()
    {
        var entity = MoHinhEfKiemTra.LayBang(MoHinh, typeof(BaoTriSuaChua), "bao_tri", "bao_tri_sua_chua");
        MoHinhEfKiemTra.KiemTraKhoaChinh(entity, "pk_bao_tri_sua_chua", "ma_bao_tri");
        MoHinhEfKiemTra.KiemTraCot(
            entity,
            "ma_bao_tri:NOT NULL",
            "ma_thiet_bi:NOT NULL",
            "ma_nv_thuc_hien:NOT NULL",
            "ma_phieu_muon:NULL",
            "loai_thuc_hien:NOT NULL",
            "ngay_lap:NOT NULL",
            "noi_dung_thuc_hien:NULL",
            "ket_qua:NULL",
            "ngay_hoan_thanh:NULL");
        MoHinhEfKiemTra.KiemTraChiMuc(
            entity,
            "ix_bt_thiet_bi_hoan_thanh|ma_thiet_bi,loai_thuc_hien,ngay_hoan_thanh||",
            "ux_bt_mot_ban_ghi_mo|ma_thiet_bi|UNIQUE|ket_qua IS NULL");
        MoHinhEfKiemTra.KiemTraKhoaNgoai(entity);
        MoHinhEfKiemTra.KiemTraRangBuocCheck(
            entity,
            "ck_bt_loai_thuc_hien",
            "ck_bt_ket_qua",
            "ck_bt_hoan_thanh_dong_bo",
            "ck_bt_noi_dung",
            "ck_bt_ngay_hoan_thanh",
            "ck_bt_phieu_chi_cho_sua_chua");
    }

    [Fact]
    public void UC11_BaoTriSuaChua_KhoaUuidDoUngDungSinh_KhongCoXmin()
    {
        var entity = MoHinh.FindEntityType(typeof(BaoTriSuaChua))!;

        Assert.Equal(ValueGenerated.Never, entity.FindProperty(nameof(BaoTriSuaChua.MaBaoTri))!.ValueGenerated);
        Assert.Null(entity.FindProperty("xmin"));
    }

    [Theory]
    [InlineData(LoaiThucHienBaoTri.BaoTriDinhKy, "BAO_TRI_DINH_KY")]
    [InlineData(LoaiThucHienBaoTri.SuaChua, "SUA_CHUA")]
    public void UC11_LoaiThucHien_ChuyenDoiKhopMaDdl(LoaiThucHienBaoTri loai, string ma)
    {
        var entity = MoHinh.FindEntityType(typeof(BaoTriSuaChua))!;
        var converter = entity.FindProperty(nameof(BaoTriSuaChua.LoaiThucHien))!.GetValueConverter();

        Assert.NotNull(converter);
        Assert.Equal(ma, converter!.ConvertToProvider(loai));
        Assert.Equal(loai, converter.ConvertFromProvider(ma));
    }
}

internal sealed class BaoTriMoHinhDbContext(DbContextOptions<BaoTriMoHinhDbContext> options) : DbContext(options)
{
    protected override void OnModelCreating(ModelBuilder modelBuilder) => modelBuilder.ApplyBaoTriConfigurations();
}
