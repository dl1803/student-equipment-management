using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuanLyMuonTra.Modules.ViPham.Domain;

namespace QuanLyMuonTra.Modules.ViPham.Infrastructure;

internal sealed class BienBanViPhamConfiguration : IEntityTypeConfiguration<BienBanViPham>
{
    public void Configure(EntityTypeBuilder<BienBanViPham> builder)
    {
        builder.ToTable("bien_ban_vi_pham", "vi_pham", table =>
        {
            table.HasCheckConstraint("ck_bb_ma_bien_ban", "ma_bien_ban ~ '^BB-[0-9]{6}-[0-9]{6,}$'");
            table.HasCheckConstraint("ck_bb_trang_thai", "trang_thai IN ('CHUA_NOP','DA_NOP','DA_HUY')");
            table.HasCheckConstraint("ck_bb_tong_tien", "tong_tien >= 0");
            table.HasCheckConstraint(
                "ck_bb_thoi_diem_nop",
                "(trang_thai = 'CHUA_NOP' AND thoi_diem_nop IS NULL) OR (trang_thai = 'DA_NOP' AND thoi_diem_nop IS NOT NULL) OR trang_thai = 'DA_HUY'");
        });

        builder.HasKey(x => x.MaBienBan).HasName("pk_bien_ban_vi_pham");
        builder.HasAlternateKey(x => new { x.MaBienBan, x.MaPhieuMuon }).HasName("uq_bb_ma_bien_ban_phieu");

        builder.Property(x => x.MaBienBan).HasColumnName("ma_bien_ban");
        builder.Property(x => x.MaPhieuMuon).HasColumnName("ma_phieu_muon");
        builder.Property(x => x.MaNvLap).HasColumnName("ma_nv_lap");
        builder.Property(x => x.NgayLap).HasColumnName("ngay_lap");
        builder.Property(x => x.TongTien).HasColumnName("tong_tien").HasColumnType("numeric(14,0)");
        builder.Property(x => x.TrangThai).HasColumnName("trang_thai");
        builder.Property(x => x.ThoiDiemNop).HasColumnName("thoi_diem_nop");
        builder.Property<uint>("xmin").HasColumnName("xmin").HasColumnType("xid").ValueGeneratedOnAddOrUpdate().IsConcurrencyToken();

        builder.HasIndex(x => x.MaPhieuMuon, "ix_bb_phieu").HasDatabaseName("ix_bb_phieu");
        builder.HasIndex(x => x.MaPhieuMuon, "ix_bb_chua_nop")
            .HasFilter("trang_thai = 'CHUA_NOP'")
            .HasDatabaseName("ix_bb_chua_nop");
    }
}

