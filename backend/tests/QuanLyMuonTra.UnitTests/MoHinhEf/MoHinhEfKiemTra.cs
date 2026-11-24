using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;

namespace QuanLyMuonTra.UnitTests.MoHinhEf;

internal static class MoHinhEfKiemTra
{
    public static IModel LayMoHinh<TContext>(Func<DbContextOptions<TContext>, TContext> taoContext)
        where TContext : DbContext
    {
        var options = new DbContextOptionsBuilder<TContext>()
            .UseNpgsql("Host=khong-ket-noi;Database=khong-ket-noi")
            .Options;
        using var context = taoContext(options);
        return context.GetService<IDesignTimeModel>().Model;
    }

    public static StoreObjectIdentifier Dinh(IEntityType entity) =>
        StoreObjectIdentifier.Table(entity.GetTableName()!, entity.GetSchema());

    public static IEntityType LayBang(IModel moHinh, Type kieu, string schema, string bang)
    {
        var entity = moHinh.FindEntityType(kieu);
        Assert.NotNull(entity);
        Assert.Equal(schema, entity!.GetSchema());
        Assert.Equal(bang, entity.GetTableName());
        return entity;
    }

    public static void KiemTraKhoaChinh(IEntityType entity, string ten, params string[] cot)
    {
        var bang = Dinh(entity);
        var khoa = entity.FindPrimaryKey();
        Assert.NotNull(khoa);
        Assert.Equal(ten, khoa!.GetName(bang));
        Assert.Equal(cot, khoa.Properties.Select(p => p.GetColumnName(bang)!));
    }

    public static void KiemTraCot(IEntityType entity, params string[] mongDoi)
    {
        var bang = Dinh(entity);
        var thucTe = entity.GetProperties()
            .Select(p => $"{p.GetColumnName(bang)}:{(p.IsNullable ? "NULL" : "NOT NULL")}");
        SoSanh(mongDoi, thucTe);
    }

    public static void KiemTraChiMuc(IEntityType entity, params string[] mongDoi)
    {
        var bang = Dinh(entity);
        var thucTe = entity.GetIndexes().Select(i =>
            $"{i.GetDatabaseName(bang)}|{string.Join(',', i.Properties.Select(p => p.GetColumnName(bang)))}|{(i.IsUnique ? "UNIQUE" : string.Empty)}|{i.GetFilter(bang)}");
        SoSanh(mongDoi, thucTe);
    }

    public static void KiemTraKhoaNgoai(IEntityType entity, params string[] mongDoi)
    {
        var bang = Dinh(entity);
        var thucTe = entity.GetForeignKeys().Select(fk =>
        {
            var chu = Dinh(fk.PrincipalEntityType);
            var cot = string.Join(',', fk.Properties.Select(p => p.GetColumnName(bang)));
            return $"{fk.GetConstraintName(bang, chu)}|{cot}|{chu.Schema}.{chu.Name}|{fk.DeleteBehavior}";
        });
        SoSanh(mongDoi, thucTe);
    }

    public static void KiemTraRangBuocCheck(IEntityType entity, params string[] mongDoi)
    {
        var thucTe = entity.GetCheckConstraints().Select(c => c.Name!);
        SoSanh(mongDoi, thucTe);
    }

    private static void SoSanh(IEnumerable<string> mongDoi, IEnumerable<string> thucTe) =>
        Assert.Equal(
            mongDoi.OrderBy(x => x, StringComparer.Ordinal),
            thucTe.OrderBy(x => x, StringComparer.Ordinal));
}
