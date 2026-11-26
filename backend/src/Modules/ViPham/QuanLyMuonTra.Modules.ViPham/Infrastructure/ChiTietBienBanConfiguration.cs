using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuanLyMuonTra.Modules.ViPham.Domain;

namespace QuanLyMuonTra.Modules.ViPham.Infrastructure;

internal sealed class ChiTietBienBanConfiguration : IEntityTypeConfiguration<ChiTietBienBan>
{
    public void Configure(EntityTypeBuilder<ChiTietBienBan> builder)
    {
        builder.ToTable("chi_tiet_bien_ban", "vi_pham", table =>
        {
            table.HasCheckConstraint("ck_ctbb_loai_vi_pham", "loai_vi_pham IN ('TRA_TRE','HU_HONG','MAT')");
            table.HasCheckConstraint("ck_ctbb_so_tien", "so_tien >= 0");
            table.HasCheckConstraint(
                "ck_ctbb_so_ngay_tre",
                "(loai_vi_pham = 'TRA_TRE' AND so_ngay_tre IS NOT NULL AND so_ngay_tre >= 1) OR (loai_vi_pham <> 'TRA_TRE' AND so_ngay_tre IS NULL)");
            table.HasCheckConstraint(
                "ck_ctbb_khung_phat",
                "(loai_vi_pham = 'HU_HONG' AND ma_khung IS NOT NULL) OR (loai_vi_pham <> 'HU_HONG' AND ma_khung IS NULL)");
        });

        builder.HasKey(x => new { x.MaBienBan, x.MaThietBi, x.LoaiViPham }).HasName("pk_chi_tiet_bien_ban");

        builder.Property(x => x.MaBienBan).HasColumnName("ma_bien_ban");
        builder.Property(x => x.MaPhieuMuon).HasColumnName("ma_phieu_muon");
        builder.Property(x => x.MaThietBi).HasColumnName("ma_thiet_bi");
        builder.Property(x => x.LoaiViPham).HasColumnName("loai_vi_pham");
        builder.Property(x => x.SoNgayTre).HasColumnName("so_ngay_tre");
        builder.Property(x => x.SoTien).HasColumnName("so_tien").HasColumnType("numeric(14,0)");
        builder.Property(x => x.MaKhung).HasColumnName("ma_khung");

        builder.HasIndex(x => x.MaKhung)
            .HasFilter("ma_khung IS NOT NULL")
            .HasDatabaseName("ix_ctbb_khung");
    }
}

