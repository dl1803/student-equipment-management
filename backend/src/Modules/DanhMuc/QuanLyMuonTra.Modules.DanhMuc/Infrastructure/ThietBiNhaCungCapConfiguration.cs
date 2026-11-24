using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuanLyMuonTra.Modules.DanhMuc.Domain;

namespace QuanLyMuonTra.Modules.DanhMuc.Infrastructure;

internal sealed class ThietBiNhaCungCapConfiguration : IEntityTypeConfiguration<ThietBiNhaCungCap>
{
    public void Configure(EntityTypeBuilder<ThietBiNhaCungCap> builder)
    {
        builder.ToTable("thiet_bi_nha_cung_cap", "danh_muc");

        builder.HasKey(x => new { x.MaThietBi, x.MaNCC }).HasName("pk_thiet_bi_nha_cung_cap");

        builder.Property(x => x.MaThietBi).HasColumnName("ma_thiet_bi");
        builder.Property(x => x.MaNCC).HasColumnName("ma_ncc");

        builder.HasIndex(x => x.MaNCC).HasDatabaseName("ix_tbncc_ncc");

        builder.HasOne<ThietBi>().WithMany().HasForeignKey(x => x.MaThietBi).HasConstraintName("fk_tbncc_thiet_bi").OnDelete(DeleteBehavior.Cascade);
        builder.HasOne<NhaCungCap>().WithMany().HasForeignKey(x => x.MaNCC).HasConstraintName("fk_tbncc_ncc").OnDelete(DeleteBehavior.NoAction);
    }
}
