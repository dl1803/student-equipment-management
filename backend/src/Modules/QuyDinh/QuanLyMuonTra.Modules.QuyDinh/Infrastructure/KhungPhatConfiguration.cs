using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuanLyMuonTra.Modules.QuyDinh.Domain;

namespace QuanLyMuonTra.Modules.QuyDinh.Infrastructure;

internal sealed class KhungPhatConfiguration : IEntityTypeConfiguration<KhungPhat>
{
    public void Configure(EntityTypeBuilder<KhungPhat> builder)
    {
        builder.ToTable("khung_phat", "quy_dinh", table =>
        {
            table.HasCheckConstraint("ck_kp_ten_khung_khong_rong", "btrim(ten_khung) <> ''");
            table.HasCheckConstraint("ck_kp_ty_le", "ty_le_phan_tram > 0 AND ty_le_phan_tram <= 100");
        });

        builder.HasKey(x => x.MaKhung).HasName("pk_khung_phat");

        builder.Property(x => x.MaKhung).HasColumnName("ma_khung").ValueGeneratedNever();
        builder.Property(x => x.TenKhung).HasColumnName("ten_khung");
        builder.Property(x => x.TyLePhanTram).HasColumnName("ty_le_phan_tram").HasColumnType("numeric(5,2)");
        builder.Property(x => x.DangSuDung).HasColumnName("dang_su_dung").HasDefaultValue(true).HasSentinel(true);
    }
}
