using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuanLyMuonTra.Modules.QuyDinh.Domain;

namespace QuanLyMuonTra.Modules.QuyDinh.Infrastructure;

internal sealed class QuyDinhMuonConfiguration : IEntityTypeConfiguration<QuyDinhMuon>
{
    public void Configure(EntityTypeBuilder<QuyDinhMuon> builder)
    {
        builder.ToTable("quy_dinh_muon", "quy_dinh", table =>
        {
            table.HasCheckConstraint("ck_qdm_chung_hoac_ngoai_le", "(ma_loai IS NULL AND so_thiet_bi_toi_da IS NOT NULL) OR (ma_loai IS NOT NULL AND so_thiet_bi_toi_da IS NULL)");
            table.HasCheckConstraint("ck_qdm_so_thiet_bi", "so_thiet_bi_toi_da IS NULL OR so_thiet_bi_toi_da > 0");
            table.HasCheckConstraint("ck_qdm_thoi_gian", "thoi_gian_muon_toi_da_gio > 0");
        });

        builder.HasKey(x => x.MaQuyDinh).HasName("pk_quy_dinh_muon");

        builder.Property(x => x.MaQuyDinh).HasColumnName("ma_quy_dinh").ValueGeneratedNever();
        builder.Property(x => x.MaNhom).HasColumnName("ma_nhom");
        builder.Property(x => x.MaLoai).HasColumnName("ma_loai");
        builder.Property(x => x.SoThietBiToiDa).HasColumnName("so_thiet_bi_toi_da");
        builder.Property(x => x.ThoiGianMuonToiDaGio).HasColumnName("thoi_gian_muon_toi_da_gio");

        builder.HasIndex(x => x.MaNhom).IsUnique().HasFilter("ma_loai IS NULL").HasDatabaseName("ux_qdm_chung");
        builder.HasIndex(x => new { x.MaNhom, x.MaLoai }).IsUnique().HasFilter("ma_loai IS NOT NULL").HasDatabaseName("ux_qdm_ngoai_le");
        builder.HasIndex(x => x.MaLoai).HasFilter("ma_loai IS NOT NULL").HasDatabaseName("ix_qdm_loai");
    }
}
