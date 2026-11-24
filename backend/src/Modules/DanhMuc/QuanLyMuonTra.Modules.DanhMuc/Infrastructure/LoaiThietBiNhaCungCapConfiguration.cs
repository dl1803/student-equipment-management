using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuanLyMuonTra.Modules.DanhMuc.Domain;

namespace QuanLyMuonTra.Modules.DanhMuc.Infrastructure;

internal sealed class LoaiThietBiNhaCungCapConfiguration : IEntityTypeConfiguration<LoaiThietBiNhaCungCap>
{
    public void Configure(EntityTypeBuilder<LoaiThietBiNhaCungCap> builder)
    {
        builder.ToTable("loai_thiet_bi_nha_cung_cap", "danh_muc");

        builder.HasKey(x => new { x.MaLoai, x.MaNCC }).HasName("pk_loai_thiet_bi_nha_cung_cap");

        builder.Property(x => x.MaLoai).HasColumnName("ma_loai");
        builder.Property(x => x.MaNCC).HasColumnName("ma_ncc");

        builder.HasIndex(x => x.MaNCC).HasDatabaseName("ix_lncc_ncc");

        builder.HasOne<LoaiThietBi>().WithMany().HasForeignKey(x => x.MaLoai).HasConstraintName("fk_lncc_loai").OnDelete(DeleteBehavior.Cascade);
        builder.HasOne<NhaCungCap>().WithMany().HasForeignKey(x => x.MaNCC).HasConstraintName("fk_lncc_ncc").OnDelete(DeleteBehavior.NoAction);
    }
}
