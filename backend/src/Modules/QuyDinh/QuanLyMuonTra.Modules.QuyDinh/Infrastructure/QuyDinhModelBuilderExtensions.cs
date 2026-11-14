using Microsoft.EntityFrameworkCore;

namespace QuanLyMuonTra.Modules.QuyDinh.Infrastructure;

public static class QuyDinhModelBuilderExtensions
{
    public static ModelBuilder ApplyQuyDinhConfigurations(this ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfiguration(new ThamSoHeThongConfiguration());
        modelBuilder.ApplyConfiguration(new KhungPhatConfiguration());
        modelBuilder.ApplyConfiguration(new QuyDinhMuonConfiguration());
        return modelBuilder;
    }
}
