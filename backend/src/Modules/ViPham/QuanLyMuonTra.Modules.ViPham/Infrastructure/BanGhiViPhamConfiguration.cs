using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuanLyMuonTra.Modules.ViPham.Domain;

namespace QuanLyMuonTra.Modules.ViPham.Infrastructure;

internal sealed class BanGhiViPhamConfiguration : IEntityTypeConfiguration<BanGhiViPham>
{
    public void Configure(EntityTypeBuilder<BanGhiViPham> builder)
    {
        builder.ToTable("vi_pham", "vi_pham", table =>
        {
            table.HasCheckConstraint("ck_vp_loai_vi_pham", "loai_vi_pham IN ('TRA_TRE','HU_HONG','MAT','KHONG_DEN_NHAN')");
            table.HasCheckConstraint(
                "ck_vp_dung_mot_chung_tu",
                "((ma_bien_ban IS NOT NULL)::int + (ma_yeu_cau IS NOT NULL)::int + (ma_dang_ky IS NOT NULL)::int) = 1");
            table.HasCheckConstraint(
                "ck_vp_loai_theo_chung_tu",
                "(loai_vi_pham IN ('TRA_TRE','HU_HONG','MAT') AND ma_bien_ban IS NOT NULL) OR (loai_vi_pham = 'KHONG_DEN_NHAN' AND ma_bien_ban IS NULL)");
        });

        builder.HasKey(x => x.MaViPham).HasName("pk_vi_pham");

        builder.Property(x => x.MaViPham).HasColumnName("ma_vi_pham").ValueGeneratedNever();
        builder.Property(x => x.MaSv).HasColumnName("ma_sv");
        builder.Property(x => x.LoaiViPham).HasColumnName("loai_vi_pham");
        builder.Property(x => x.ThoiDiemGhiNhan).HasColumnName("thoi_diem_ghi_nhan");
        builder.Property(x => x.HocKy).HasColumnName("hoc_ky");
        builder.Property(x => x.MaBienBan).HasColumnName("ma_bien_ban");
        builder.Property(x => x.MaYeuCau).HasColumnName("ma_yeu_cau");
        builder.Property(x => x.MaDangKy).HasColumnName("ma_dang_ky");

        builder.HasIndex(x => new { x.MaSv, x.HocKy }).HasDatabaseName("ix_vp_sv_hoc_ky");
        builder.HasIndex(x => new { x.MaBienBan, x.LoaiViPham })
            .IsUnique()
            .HasFilter("ma_bien_ban IS NOT NULL")
            .HasDatabaseName("ux_vp_bien_ban_loai");
        builder.HasIndex(x => x.MaYeuCau)
            .IsUnique()
            .HasFilter("ma_yeu_cau IS NOT NULL")
            .HasDatabaseName("ux_vp_yeu_cau");
        builder.HasIndex(x => x.MaDangKy)
            .IsUnique()
            .HasFilter("ma_dang_ky IS NOT NULL")
            .HasDatabaseName("ux_vp_dang_ky");
    }
}

