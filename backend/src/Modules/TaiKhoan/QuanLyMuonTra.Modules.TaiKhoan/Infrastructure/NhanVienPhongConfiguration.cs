using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuanLyMuonTra.Modules.TaiKhoan.Domain;

namespace QuanLyMuonTra.Modules.TaiKhoan.Infrastructure;

internal sealed class NhanVienPhongConfiguration : IEntityTypeConfiguration<NhanVienPhong>
{
    public void Configure(EntityTypeBuilder<NhanVienPhong> builder)
    {
        builder.ToTable("nhan_vien_phong", "tai_khoan");

        builder.HasKey(x => new { x.MaNguoiDung, x.MaPhong }).HasName("pk_nhan_vien_phong");

        builder.Property(x => x.MaNguoiDung).HasColumnName("ma_nguoi_dung");
        builder.Property(x => x.MaPhong).HasColumnName("ma_phong");

        builder.HasIndex(x => x.MaPhong).HasDatabaseName("ix_nvp_phong");

        builder.HasOne<NhanVienQuanLy>()
            .WithMany()
            .HasForeignKey(x => x.MaNguoiDung)
            .HasConstraintName("fk_nvp_nhan_vien")
            .OnDelete(DeleteBehavior.NoAction);
    }
}

