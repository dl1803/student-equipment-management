using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuanLyMuonTra.Modules.TaiKhoan.Domain;

namespace QuanLyMuonTra.Modules.TaiKhoan.Infrastructure;

internal sealed class NhanVienQuanLyConfiguration : IEntityTypeConfiguration<NhanVienQuanLy>
{
    public void Configure(EntityTypeBuilder<NhanVienQuanLy> builder)
    {
        builder.ToTable("nhan_vien_quan_ly", "tai_khoan", table =>
            table.HasCheckConstraint("ck_nvql_ma_nv_khong_rong", "btrim(ma_nv) <> ''"));

        builder.Property(x => x.MaNv).HasColumnName("ma_nv");

        builder.HasIndex(x => x.MaNv).IsUnique().HasDatabaseName("uq_nvql_ma_nv");
    }
}

