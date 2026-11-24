using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuanLyMuonTra.Modules.MuonTra.Domain;

namespace QuanLyMuonTra.Modules.MuonTra.Infrastructure;

internal sealed class PhieuMuonConfiguration : IEntityTypeConfiguration<PhieuMuon>
{
    public void Configure(EntityTypeBuilder<PhieuMuon> builder)
    {
        builder.ToTable("phieu_muon", "muon_tra");
        builder.HasKey(p => p.MaPhieu);
        builder.Property(p => p.MaPhieu).HasColumnName("ma_phieu");
        builder.Property(p => p.MaSv).HasColumnName("ma_sv");
        builder.Property(p => p.MaNvLap).HasColumnName("ma_nv_lap");
        builder.Property(p => p.MaYeuCau).HasColumnName("ma_yeu_cau");
        builder.Property(p => p.MaDangKy).HasColumnName("ma_dang_ky");
        builder.Property(p => p.NgayGioMuon).HasColumnName("ngay_gio_muon");
        builder.Property(p => p.NgayGioHenTra).HasColumnName("ngay_gio_hen_tra");
        builder.Property(p => p.SoLanGiaHan).HasColumnName("so_lan_gia_han");
        builder.Property(p => p.TrangThai).HasColumnName("trang_thai").HasConversion(new MaTrangThaiConverter<TrangThaiPhieuMuon>());
        builder.Property<uint>("xmin").HasColumnName("xmin").HasColumnType("xid").ValueGeneratedOnAddOrUpdate().IsConcurrencyToken();
        builder.HasMany(p => p.ChiTiet).WithOne().HasForeignKey(c => c.MaPhieu);
        builder.Navigation(p => p.ChiTiet).UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}
