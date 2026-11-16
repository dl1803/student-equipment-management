using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuanLyMuonTra.Modules.TaiKhoan.Domain;

namespace QuanLyMuonTra.Modules.TaiKhoan.Infrastructure;

internal sealed class QuanTriVienConfiguration : IEntityTypeConfiguration<QuanTriVien>
{
    public void Configure(EntityTypeBuilder<QuanTriVien> builder)
    {
        builder.ToTable("quan_tri_vien", "tai_khoan", table =>
            table.HasCheckConstraint("ck_qtv_ma_qt_khong_rong", "btrim(ma_qt) <> ''"));

        builder.Property(x => x.MaQt).HasColumnName("ma_qt");

        builder.HasIndex(x => x.MaQt).IsUnique().HasDatabaseName("uq_qtv_ma_qt");
    }
}

