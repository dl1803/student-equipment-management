using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace QuanLyMuonTra.Infrastructure.Persistence;

public sealed class AppDbContextModelCacheKeyFactory : IModelCacheKeyFactory
{
    public object Create(DbContext context, bool designTime)
    {
        if (context is AppDbContext appDb)
        {
            var assemblyNames = string.Join(";", appDb.DanhSachCauHinh
                .SelectMany(c => c.Assemblies)
                .Select(a => a.FullName)
                .OrderBy(n => n));

            return (context.GetType(), assemblyNames, designTime);
        }

        return (context.GetType(), designTime);
    }
}

