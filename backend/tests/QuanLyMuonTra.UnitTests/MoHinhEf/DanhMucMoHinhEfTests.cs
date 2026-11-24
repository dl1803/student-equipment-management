using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using QuanLyMuonTra.Modules.DanhMuc.Domain;
using QuanLyMuonTra.Modules.DanhMuc.Infrastructure;

namespace QuanLyMuonTra.UnitTests.MoHinhEf;

public sealed class DanhMucMoHinhEfTests
{
    private static readonly IModel MoHinh =
        MoHinhEfKiemTra.LayMoHinh<DanhMucMoHinhDbContext>(o => new DanhMucMoHinhDbContext(o));

    [Fact]
    public void UC12_PhongThietBi_KhopDdl()
    {
        var entity = MoHinhEfKiemTra.LayBang(MoHinh, typeof(PhongThietBi), "danh_muc", "phong_thiet_bi");
        MoHinhEfKiemTra.KiemTraKhoaChinh(entity, "pk_phong_thiet_bi", "ma_phong");
        MoHinhEfKiemTra.KiemTraCot(entity, "ma_phong:NOT NULL", "ten_phong:NOT NULL", "co_so:NOT NULL");
        MoHinhEfKiemTra.KiemTraChiMuc(entity);
        MoHinhEfKiemTra.KiemTraKhoaNgoai(entity);
        MoHinhEfKiemTra.KiemTraRangBuocCheck(entity);
    }

    [Fact]
    public void UC12_LoaiThietBi_KhopDdl()
    {
        var entity = MoHinhEfKiemTra.LayBang(MoHinh, typeof(LoaiThietBi), "danh_muc", "loai_thiet_bi");
        MoHinhEfKiemTra.KiemTraKhoaChinh(entity, "pk_loai_thiet_bi", "ma_loai");
        MoHinhEfKiemTra.KiemTraCot(
            entity,
            "ma_loai:NOT NULL",
            "ten_loai:NOT NULL",
            "mo_ta:NULL",
            "hang_san_xuat:NULL",
            "chu_ky_bao_tri:NOT NULL",
            "phi_tre_moi_ngay:NOT NULL",
            "ngung_su_dung:NOT NULL");
        MoHinhEfKiemTra.KiemTraChiMuc(entity);
        MoHinhEfKiemTra.KiemTraKhoaNgoai(entity);
        MoHinhEfKiemTra.KiemTraRangBuocCheck(entity, "ck_ltb_chu_ky_bao_tri", "ck_ltb_phi_tre_moi_ngay");
    }

    [Fact]
    public void UC12_NhaCungCap_KhopDdl()
    {
        var entity = MoHinhEfKiemTra.LayBang(MoHinh, typeof(NhaCungCap), "danh_muc", "nha_cung_cap");
        MoHinhEfKiemTra.KiemTraKhoaChinh(entity, "pk_nha_cung_cap", "ma_ncc");
        MoHinhEfKiemTra.KiemTraCot(entity, "ma_ncc:NOT NULL", "ten_ncc:NOT NULL", "thong_tin_lien_he:NULL");
        MoHinhEfKiemTra.KiemTraChiMuc(entity);
        MoHinhEfKiemTra.KiemTraKhoaNgoai(entity);
        MoHinhEfKiemTra.KiemTraRangBuocCheck(entity);
    }

    [Fact]
    public void UC12_ThietBi_KhopDdl()
    {
        var entity = MoHinhEfKiemTra.LayBang(MoHinh, typeof(ThietBi), "danh_muc", "thiet_bi");
        MoHinhEfKiemTra.KiemTraKhoaChinh(entity, "pk_thiet_bi", "ma_thiet_bi");
        MoHinhEfKiemTra.KiemTraCot(
            entity,
            "ma_thiet_bi:NOT NULL",
            "ma_loai:NOT NULL",
            "ma_phong:NOT NULL",
            "ten_thiet_bi:NOT NULL",
            "mo_ta:NULL",
            "thong_so_ky_thuat:NULL",
            "ngay_nhap_kho:NOT NULL",
            "thoi_han_bao_hanh:NULL",
            "tinh_trang:NOT NULL",
            "vi_tri_ke:NULL",
            "vi_tri_tu:NULL",
            "ngung_su_dung:NOT NULL",
            "xmin:NOT NULL");
        MoHinhEfKiemTra.KiemTraChiMuc(
            entity,
            "ix_tb_phong_loai_tinh_trang|ma_phong,ma_loai,tinh_trang||",
            "ix_tb_loai|ma_loai||");
        MoHinhEfKiemTra.KiemTraKhoaNgoai(
            entity,
            "fk_tb_loai|ma_loai|danh_muc.loai_thiet_bi|NoAction",
            "fk_tb_phong|ma_phong|danh_muc.phong_thiet_bi|NoAction");
        MoHinhEfKiemTra.KiemTraRangBuocCheck(entity, "ck_tb_ma_thiet_bi_khong_rong", "ck_tb_tinh_trang");
    }

    [Fact]
    public void UC12_LichSuGiaTri_KhopDdl()
    {
        var entity = MoHinhEfKiemTra.LayBang(MoHinh, typeof(LichSuGiaTri), "danh_muc", "lich_su_gia_tri");
        MoHinhEfKiemTra.KiemTraKhoaChinh(entity, "pk_lich_su_gia_tri", "ma_lich_su");
        MoHinhEfKiemTra.KiemTraCot(
            entity,
            "ma_lich_su:NOT NULL",
            "ma_thiet_bi:NOT NULL",
            "gia_tri:NOT NULL",
            "ngay_cap_nhat:NOT NULL");
        MoHinhEfKiemTra.KiemTraChiMuc(entity, "ix_lsgt_thiet_bi_ngay|ma_thiet_bi,ngay_cap_nhat||");
        MoHinhEfKiemTra.KiemTraKhoaNgoai(entity, "fk_lsgt_thiet_bi|ma_thiet_bi|danh_muc.thiet_bi|Cascade");
        MoHinhEfKiemTra.KiemTraRangBuocCheck(entity, "ck_lsgt_gia_tri");
    }

    [Fact]
    public void UC12_ThietBiNhaCungCap_KhopDdl()
    {
        var entity = MoHinhEfKiemTra.LayBang(MoHinh, typeof(ThietBiNhaCungCap), "danh_muc", "thiet_bi_nha_cung_cap");
        MoHinhEfKiemTra.KiemTraKhoaChinh(entity, "pk_thiet_bi_nha_cung_cap", "ma_thiet_bi", "ma_ncc");
        MoHinhEfKiemTra.KiemTraCot(entity, "ma_thiet_bi:NOT NULL", "ma_ncc:NOT NULL");
        MoHinhEfKiemTra.KiemTraChiMuc(entity, "ix_tbncc_ncc|ma_ncc||");
        MoHinhEfKiemTra.KiemTraKhoaNgoai(
            entity,
            "fk_tbncc_thiet_bi|ma_thiet_bi|danh_muc.thiet_bi|Cascade",
            "fk_tbncc_ncc|ma_ncc|danh_muc.nha_cung_cap|NoAction");
        MoHinhEfKiemTra.KiemTraRangBuocCheck(entity);
    }

    [Fact]
    public void UC12_LoaiThietBiNhaCungCap_KhopDdl()
    {
        var entity = MoHinhEfKiemTra.LayBang(MoHinh, typeof(LoaiThietBiNhaCungCap), "danh_muc", "loai_thiet_bi_nha_cung_cap");
        MoHinhEfKiemTra.KiemTraKhoaChinh(entity, "pk_loai_thiet_bi_nha_cung_cap", "ma_loai", "ma_ncc");
        MoHinhEfKiemTra.KiemTraCot(entity, "ma_loai:NOT NULL", "ma_ncc:NOT NULL");
        MoHinhEfKiemTra.KiemTraChiMuc(entity, "ix_lncc_ncc|ma_ncc||");
        MoHinhEfKiemTra.KiemTraKhoaNgoai(
            entity,
            "fk_lncc_loai|ma_loai|danh_muc.loai_thiet_bi|Cascade",
            "fk_lncc_ncc|ma_ncc|danh_muc.nha_cung_cap|NoAction");
        MoHinhEfKiemTra.KiemTraRangBuocCheck(entity);
    }

    [Theory]
    [InlineData(TinhTrangThietBi.SanSang, "SAN_SANG")]
    [InlineData(TinhTrangThietBi.DangDuocMuon, "DANG_DUOC_MUON")]
    [InlineData(TinhTrangThietBi.DangBaoTriSuaChua, "DANG_BAO_TRI_SUA_CHUA")]
    [InlineData(TinhTrangThietBi.DaThanhLy, "DA_THANH_LY")]
    [InlineData(TinhTrangThietBi.DaBaoMat, "DA_BAO_MAT")]
    public void UC12_ThietBi_TinhTrangChuyenSangMaChuoi(TinhTrangThietBi tinhTrang, string ma)
    {
        var entity = MoHinh.FindEntityType(typeof(ThietBi))!;
        var converter = entity.FindProperty(nameof(ThietBi.TinhTrang))!.GetValueConverter();
        Assert.NotNull(converter);
        Assert.Equal(ma, converter!.ConvertToProvider(tinhTrang));
        Assert.Equal(tinhTrang, converter.ConvertFromProvider(ma));
    }

    [Fact]
    public void UC12_ThietBi_XminLaConcurrencyToken()
    {
        var entity = MoHinh.FindEntityType(typeof(ThietBi))!;
        var xmin = entity.FindProperty("xmin");
        Assert.NotNull(xmin);
        Assert.True(xmin!.IsConcurrencyToken);
        Assert.Equal("xid", xmin.GetColumnType(MoHinhEfKiemTra.Dinh(entity)));
        Assert.Equal(ValueGenerated.OnAddOrUpdate, xmin.ValueGenerated);
    }

    [Theory]
    [InlineData(typeof(LoaiThietBi), nameof(LoaiThietBi.PhiTreMoiNgay))]
    [InlineData(typeof(LichSuGiaTri), nameof(LichSuGiaTri.GiaTri))]
    public void UC12_Tien_LaNumeric14Khong(Type kieu, string thuocTinh)
    {
        var entity = MoHinh.FindEntityType(kieu)!;
        var property = entity.FindProperty(thuocTinh)!;
        Assert.Equal("numeric(14,0)", property.GetColumnType(MoHinhEfKiemTra.Dinh(entity)));
    }

    [Theory]
    [InlineData(typeof(LoaiThietBi))]
    [InlineData(typeof(ThietBi))]
    public void UC12_NgungSuDung_MacDinhFalse(Type kieu)
    {
        var entity = MoHinh.FindEntityType(kieu)!;
        var property = entity.FindProperty("NgungSuDung")!;
        Assert.Equal((object)false, property.GetDefaultValue(MoHinhEfKiemTra.Dinh(entity)));
    }

    [Fact]
    public void UC12_ThietBi_CoMacDinhTinhTrang()
    {
        var entity = MoHinh.FindEntityType(typeof(ThietBi))!;
        var property = entity.FindProperty(nameof(ThietBi.TinhTrang))!;
        Assert.NotNull(property.GetDefaultValue(MoHinhEfKiemTra.Dinh(entity)));
    }

    [Theory]
    [InlineData(typeof(PhongThietBi))]
    [InlineData(typeof(LoaiThietBi))]
    [InlineData(typeof(NhaCungCap))]
    [InlineData(typeof(LichSuGiaTri))]
    public void UC12_KhoaUuid_UngDungTuSinh(Type kieu)
    {
        var entity = MoHinh.FindEntityType(kieu)!;
        Assert.Equal(ValueGenerated.Never, entity.FindPrimaryKey()!.Properties[0].ValueGenerated);
    }
}

internal sealed class DanhMucMoHinhDbContext(DbContextOptions<DanhMucMoHinhDbContext> options) : DbContext(options)
{
    protected override void OnModelCreating(ModelBuilder modelBuilder) => modelBuilder.ApplyDanhMucConfigurations();
}
