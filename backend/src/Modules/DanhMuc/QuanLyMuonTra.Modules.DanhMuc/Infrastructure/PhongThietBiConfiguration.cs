using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuanLyMuonTra.Modules.DanhMuc.Domain;

namespace QuanLyMuonTra.Modules.DanhMuc.Infrastructure;

internal sealed class PhongThietBiConfiguration : IEntityTypeConfiguration<PhongThietBi>
{
    public void Configure(EntityTypeBuilder<PhongThietBi> builder)
    {
        builder.ToTable("phong_thiet_bi", "danh_muc");

        builder.HasKey(x => x.MaPhong).HasName("pk_phong_thiet_bi");

        builder.Property(x => x.MaPhong).HasColumnName("ma_phong").ValueGeneratedNever();
        builder.Property(x => x.TenPhong).HasColumnName("ten_phong");
        builder.Property(x => x.CoSo).HasColumnName("co_so");
    }
}
