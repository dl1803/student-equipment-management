using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuanLyMuonTra.Modules.DanhMuc.Domain;

namespace QuanLyMuonTra.Modules.DanhMuc.Infrastructure;

internal sealed class ThietBiConfiguration : IEntityTypeConfiguration<ThietBi>
{
    public void Configure(EntityTypeBuilder<ThietBi> builder)
    {
        builder.ToTable("thiet_bi", "danh_muc", table =>
        {
            table.HasCheckConstraint("ck_tb_ma_thiet_bi_khong_rong", "btrim(ma_thiet_bi) <> ''");
            table.HasCheckConstraint("ck_tb_tinh_trang", "tinh_trang IN ('SAN_SANG','DANG_DUOC_MUON','DANG_BAO_TRI_SUA_CHUA','DA_THANH_LY','DA_BAO_MAT')");
        });

        builder.HasKey(x => x.MaThietBi).HasName("pk_thiet_bi");

        builder.Property(x => x.MaThietBi).HasColumnName("ma_thiet_bi");
        builder.Property(x => x.MaLoai).HasColumnName("ma_loai");
        builder.Property(x => x.MaPhong).HasColumnName("ma_phong");
        builder.Property(x => x.TenThietBi).HasColumnName("ten_thiet_bi");
        builder.Property(x => x.MoTa).HasColumnName("mo_ta");
        builder.Property(x => x.ThongSoKyThuat).HasColumnName("thong_so_ky_thuat");
        builder.Property(x => x.NgayNhapKho).HasColumnName("ngay_nhap_kho");
        builder.Property(x => x.ThoiHanBaoHanh).HasColumnName("thoi_han_bao_hanh");
        builder.Property(x => x.TinhTrang)
            .HasColumnName("tinh_trang")
            .HasConversion(v => TinhTrangThietBiMa.ToMa(v), s => TinhTrangThietBiMa.ToTinhTrang(s))
            .HasDefaultValue(TinhTrangThietBi.SanSang);
        builder.Property(x => x.ViTriKe).HasColumnName("vi_tri_ke");
        builder.Property(x => x.ViTriTu).HasColumnName("vi_tri_tu");
        builder.Property(x => x.NgungSuDung).HasColumnName("ngung_su_dung").HasDefaultValue(false);
        builder.Property<uint>("xmin").HasColumnType("xid").ValueGeneratedOnAddOrUpdate().IsConcurrencyToken();

        builder.HasIndex(x => new { x.MaPhong, x.MaLoai, x.TinhTrang }).HasDatabaseName("ix_tb_phong_loai_tinh_trang");
        builder.HasIndex(x => x.MaLoai).HasDatabaseName("ix_tb_loai");

        builder.HasOne<LoaiThietBi>().WithMany().HasForeignKey(x => x.MaLoai).HasConstraintName("fk_tb_loai").OnDelete(DeleteBehavior.NoAction);
        builder.HasOne<PhongThietBi>().WithMany().HasForeignKey(x => x.MaPhong).HasConstraintName("fk_tb_phong").OnDelete(DeleteBehavior.NoAction);
    }
}
