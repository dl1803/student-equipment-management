using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuanLyMuonTra.Modules.TaiKhoan.Domain;

namespace QuanLyMuonTra.Modules.TaiKhoan.Infrastructure;

internal sealed class RefreshTokenConfiguration : IEntityTypeConfiguration<RefreshToken>
{
    public void Configure(EntityTypeBuilder<RefreshToken> builder)
    {
        builder.ToTable("refresh_token", "tai_khoan", table =>
        {
            table.HasCheckConstraint("ck_rt_chu_so_huu_loai", "chu_so_huu_loai IN ('SINH_VIEN','NHAN_VIEN','QUAN_TRI')");
            table.HasCheckConstraint("ck_rt_chu_so_huu_ma_khong_rong", "btrim(chu_so_huu_ma) <> ''");
            table.HasCheckConstraint("ck_rt_thay_the_boi", "thay_the_boi IS NULL OR thay_the_boi <> ma");
        });

        builder.HasKey(x => x.Ma).HasName("pk_refresh_token");

        builder.Property(x => x.Ma).HasColumnName("ma").ValueGeneratedNever();
        builder.Property(x => x.ChuSoHuuLoai).HasColumnName("chu_so_huu_loai");
        builder.Property(x => x.ChuSoHuuMa).HasColumnName("chu_so_huu_ma");
        builder.Property(x => x.MaBam).HasColumnName("ma_bam");
        builder.Property(x => x.HetHan).HasColumnName("het_han");
        builder.Property(x => x.DaThuHoi).HasColumnName("da_thu_hoi").HasDefaultValue(false);
        builder.Property(x => x.ThayTheBoi).HasColumnName("thay_the_boi");

        builder.HasIndex(x => x.MaBam).IsUnique().HasDatabaseName("uq_rt_ma_bam");
        builder.HasIndex(x => new { x.ChuSoHuuLoai, x.ChuSoHuuMa }).HasDatabaseName("ix_rt_chu_so_huu");

        builder.HasOne<RefreshToken>()
            .WithMany()
            .HasForeignKey(x => x.ThayTheBoi)
            .HasConstraintName("fk_rt_thay_the_boi")
            .OnDelete(DeleteBehavior.NoAction);
    }
}

