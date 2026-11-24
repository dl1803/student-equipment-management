using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuanLyMuonTra.Modules.MuonTra.Domain;

namespace QuanLyMuonTra.Modules.MuonTra.Infrastructure;

internal sealed class DangKyChoConfiguration : IEntityTypeConfiguration<DangKyCho>
{
    public void Configure(EntityTypeBuilder<DangKyCho> builder)
    {
        builder.ToTable("dang_ky_cho", "muon_tra");
        builder.HasKey(d => d.MaDangKy);
        builder.Property(d => d.MaDangKy).HasColumnName("ma_dang_ky");
        builder.Property(d => d.MaSv).HasColumnName("ma_sv");
        builder.Property(d => d.MaPhong).HasColumnName("ma_phong");
        builder.Property(d => d.MaLoai).HasColumnName("ma_loai");
        builder.Property(d => d.ThoiDiemDangKy).HasColumnName("thoi_diem_dang_ky");
        builder.Property(d => d.SoThuTuCho).HasColumnName("so_thu_tu_cho");
        builder.Property(d => d.TrangThai).HasColumnName("trang_thai").HasConversion(new MaTrangThaiConverter<TrangThaiDangKyCho>());
        builder.Property(d => d.HanGiuCho).HasColumnName("han_giu_cho");
        builder.Property<uint>("xmin").HasColumnName("xmin").HasColumnType("xid").ValueGeneratedOnAddOrUpdate().IsConcurrencyToken();
    }
}
