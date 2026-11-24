using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuanLyMuonTra.Modules.DanhMuc.Domain;

namespace QuanLyMuonTra.Modules.DanhMuc.Infrastructure;

internal sealed class NhaCungCapConfiguration : IEntityTypeConfiguration<NhaCungCap>
{
    public void Configure(EntityTypeBuilder<NhaCungCap> builder)
    {
        builder.ToTable("nha_cung_cap", "danh_muc");

        builder.HasKey(x => x.MaNCC).HasName("pk_nha_cung_cap");

        builder.Property(x => x.MaNCC).HasColumnName("ma_ncc").ValueGeneratedNever();
        builder.Property(x => x.TenNCC).HasColumnName("ten_ncc");
        builder.Property(x => x.ThongTinLienHe).HasColumnName("thong_tin_lien_he");
    }
}
