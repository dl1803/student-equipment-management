using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace QuanLyMuonTra.Infrastructure.Auditing;

public class NhatKyHeThongConfiguration : IEntityTypeConfiguration<NhatKyHeThong>
{
    public void Configure(EntityTypeBuilder<NhatKyHeThong> builder)
    {
        builder.ToTable("nhat_ky_he_thong", "nhat_ky");
        builder.HasKey(n => n.MaNhatKy);
        builder.Property(n => n.MaNhatKy).HasColumnName("ma_nhat_ky").ValueGeneratedNever();
        builder.Property(n => n.ThoiDiem).HasColumnName("thoi_diem");
        builder.Property(n => n.MaNguoiDung).HasColumnName("ma_nguoi_dung");
        builder.Property(n => n.MaSv).HasColumnName("ma_sv");
        builder.Property(n => n.ThaoTac).HasColumnName("thao_tac");
        builder.Property(n => n.DoiTuong).HasColumnName("doi_tuong");
        builder.Property(n => n.DoiTuongId).HasColumnName("doi_tuong_id");
        builder.Property(n => n.GiaTriTruoc).HasColumnName("gia_tri_truoc").HasColumnType("jsonb");
        builder.Property(n => n.GiaTriSau).HasColumnName("gia_tri_sau").HasColumnType("jsonb");
    }
}

