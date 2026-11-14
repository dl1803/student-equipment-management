using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuanLyMuonTra.Modules.MuonTra.Domain;

namespace QuanLyMuonTra.Modules.MuonTra.Infrastructure;

internal sealed class ChiTietYeuCauMuonConfiguration : IEntityTypeConfiguration<ChiTietYeuCauMuon>
{
    public void Configure(EntityTypeBuilder<ChiTietYeuCauMuon> builder)
    {
        builder.ToTable("chi_tiet_yeu_cau_muon", "muon_tra");
        builder.HasKey(c => new { c.MaYeuCau, c.MaLoai });
        builder.Property(c => c.MaYeuCau).HasColumnName("ma_yeu_cau");
        builder.Property(c => c.MaLoai).HasColumnName("ma_loai");
        builder.Property(c => c.SoLuong).HasColumnName("so_luong");
    }
}
