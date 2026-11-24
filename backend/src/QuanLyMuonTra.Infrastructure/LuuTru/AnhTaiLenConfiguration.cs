using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace QuanLyMuonTra.Infrastructure.LuuTru;

public class AnhTaiLenConfiguration : IEntityTypeConfiguration<AnhTaiLen>
{
    public void Configure(EntityTypeBuilder<AnhTaiLen> builder)
    {
        builder.ToTable("anh_tai_len", "luu_tru");
        builder.HasKey(a => a.MaAnh);
        builder.Property(a => a.MaAnh).HasColumnName("ma_anh").ValueGeneratedNever();
        builder.Property(a => a.Bucket).HasColumnName("bucket");
        builder.Property(a => a.KhoaLuuTru).HasColumnName("khoa_luu_tru");
        builder.Property(a => a.LoaiNoiDung).HasColumnName("loai_noi_dung");
        builder.Property(a => a.KichThuoc).HasColumnName("kich_thuoc");
        builder.Property(a => a.NguoiTaiLen).HasColumnName("nguoi_tai_len");
        builder.Property(a => a.ThoiDiem).HasColumnName("thoi_diem");
        builder.Property(a => a.DoiTuongLoai).HasColumnName("doi_tuong_loai");
        builder.Property(a => a.DoiTuongId).HasColumnName("doi_tuong_id");
        builder.Property(a => a.GiaiDoan).HasColumnName("giai_doan");
    }
}

