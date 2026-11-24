using Microsoft.EntityFrameworkCore;

namespace QuanLyMuonTra.Infrastructure.Persistence;

public class AppDbContext(
    DbContextOptions<AppDbContext> options,
    IEnumerable<EfConfigurationAssemblies> danhSachCauHinh) : DbContext(options)
{
    public IEnumerable<EfConfigurationAssemblies> DanhSachCauHinh => danhSachCauHinh;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);

        foreach (var cauHinh in danhSachCauHinh)
        {
            foreach (var assembly in cauHinh.Assemblies)
            {
                modelBuilder.ApplyConfigurationsFromAssembly(assembly);
            }
        }
    }
}
