using Microsoft.EntityFrameworkCore;

namespace QuanLyMuonTra.Modules.ViPham.Infrastructure;

public static class ViPhamModelBuilderExtensions
{
    public static ModelBuilder ApplyViPhamConfigurations(this ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfiguration(new BanGhiViPhamConfiguration());
        modelBuilder.ApplyConfiguration(new BienBanViPhamConfiguration());
        modelBuilder.ApplyConfiguration(new ChiTietBienBanConfiguration());
        return modelBuilder;
    }
}

