using Microsoft.EntityFrameworkCore;

namespace QuanLyMuonTra.Modules.DanhMuc.Infrastructure;

public static class DanhMucModelBuilderExtensions
{
    public static ModelBuilder ApplyDanhMucConfigurations(this ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfiguration(new PhongThietBiConfiguration());
        modelBuilder.ApplyConfiguration(new LoaiThietBiConfiguration());
        modelBuilder.ApplyConfiguration(new NhaCungCapConfiguration());
        modelBuilder.ApplyConfiguration(new ThietBiConfiguration());
        modelBuilder.ApplyConfiguration(new LichSuGiaTriConfiguration());
        modelBuilder.ApplyConfiguration(new ThietBiNhaCungCapConfiguration());
        modelBuilder.ApplyConfiguration(new LoaiThietBiNhaCungCapConfiguration());
        return modelBuilder;
    }
}
