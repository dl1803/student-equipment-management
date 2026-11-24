using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuanLyMuonTra.Modules.DanhMuc.Domain;

namespace QuanLyMuonTra.Modules.DanhMuc.Infrastructure;

internal sealed class LichSuGiaTriConfiguration : IEntityTypeConfiguration<LichSuGiaTri>
{
    public void Configure(EntityTypeBuilder<LichSuGiaTri> builder)
    {
        builder.ToTable("lich_su_gia_tri", "danh_muc", table =>
        {
            table.HasCheckConstraint("ck_lsgt_gia_tri", "gia_tri >= 0");
        });

        builder.HasKey(x => x.MaLichSu).HasName("pk_lich_su_gia_tri");

        builder.Property(x => x.MaLichSu).HasColumnName("ma_lich_su").ValueGeneratedNever();
        builder.Property(x => x.MaThietBi).HasColumnName("ma_thiet_bi");
        builder.Property(x => x.GiaTri).HasColumnName("gia_tri").HasColumnType("numeric(14,0)");
        builder.Property(x => x.NgayCapNhat).HasColumnName("ngay_cap_nhat").HasDefaultValueSql("now()");

        builder.HasIndex(x => new { x.MaThietBi, x.NgayCapNhat }).IsDescending(false, true).HasDatabaseName("ix_lsgt_thiet_bi_ngay");

        builder.HasOne<ThietBi>().WithMany().HasForeignKey(x => x.MaThietBi).HasConstraintName("fk_lsgt_thiet_bi").OnDelete(DeleteBehavior.Cascade);
    }
}
