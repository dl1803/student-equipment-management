using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuanLyMuonTra.Modules.DanhMuc.Domain;

namespace QuanLyMuonTra.Modules.DanhMuc.Infrastructure;

internal sealed class LoaiThietBiConfiguration : IEntityTypeConfiguration<LoaiThietBi>
{
    public void Configure(EntityTypeBuilder<LoaiThietBi> builder)
    {
        builder.ToTable("loai_thiet_bi", "danh_muc", table =>
        {
            table.HasCheckConstraint("ck_ltb_chu_ky_bao_tri", "chu_ky_bao_tri > 0");
            table.HasCheckConstraint("ck_ltb_phi_tre_moi_ngay", "phi_tre_moi_ngay >= 0");
        });

        builder.HasKey(x => x.MaLoai).HasName("pk_loai_thiet_bi");

        builder.Property(x => x.MaLoai).HasColumnName("ma_loai").ValueGeneratedNever();
        builder.Property(x => x.TenLoai).HasColumnName("ten_loai");
        builder.Property(x => x.MoTa).HasColumnName("mo_ta");
        builder.Property(x => x.HangSanXuat).HasColumnName("hang_san_xuat");
        builder.Property(x => x.ChuKyBaoTri).HasColumnName("chu_ky_bao_tri");
        builder.Property(x => x.PhiTreMoiNgay).HasColumnName("phi_tre_moi_ngay").HasColumnType("numeric(14,0)");
        builder.Property(x => x.NgungSuDung).HasColumnName("ngung_su_dung").HasDefaultValue(false);
    }
}
