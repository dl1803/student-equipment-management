using System.Globalization;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Diagnostics;
using QuanLyMuonTra.SharedKernel;

namespace QuanLyMuonTra.Infrastructure.Auditing;

public sealed class NhatKyInterceptor(ICurrentUser nguoiDung, IClock dongHo) : SaveChangesInterceptor
{
    public override InterceptionResult<int> SavingChanges(DbContextEventData eventData, InterceptionResult<int> result)
    {
        ThemNhatKy(eventData.Context);
        return result;
    }

    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        ThemNhatKy(eventData.Context);
        return ValueTask.FromResult(result);
    }

    private static string? LayKhoaChinh(EntityEntry dong)
    {
        var khoa = dong.Metadata.FindPrimaryKey();
        if (khoa is null)
        {
            return null;
        }

        var cacGiaTri = khoa.Properties.Select(p => dong.Property(p.Name)).ToList();
        if (cacGiaTri.Any(p => p.IsTemporary || p.CurrentValue is null))
        {
            return null;
        }

        return string.Join(",", cacGiaTri.Select(p => Convert.ToString(p.CurrentValue, CultureInfo.InvariantCulture)));
    }

    private void ThemNhatKy(DbContext? context)
    {
        if (context is null)
        {
            return;
        }

        var cacDong = context.ChangeTracker.Entries<IAuditable>()
            .Where(dong => dong.State is EntityState.Added or EntityState.Modified or EntityState.Deleted)
            .ToList();

        foreach (var dong in cacDong)
        {
            var nhatKy = TaoNhatKy(dong);
            if (nhatKy is not null)
            {
                context.Set<NhatKyHeThong>().Add(nhatKy);
            }
        }
    }

    private NhatKyHeThong? TaoNhatKy(EntityEntry dong)
    {
        var cacThuocTinh = dong.Properties
            .Where(p => !p.Metadata.IsConcurrencyToken && !p.IsTemporary)
            .ToList();

        var thayDoi = dong.State == EntityState.Modified
            ? cacThuocTinh.Where(p => p.IsModified).ToList()
            : cacThuocTinh;

        if (thayDoi.Count == 0)
        {
            return null;
        }

        var truoc = dong.State == EntityState.Added
            ? null
            : NhatKyJson.ChuyenSangJson(
                thayDoi.Select(p => KeyValuePair.Create<string, object?>(p.Metadata.Name, p.OriginalValue)));

        var sau = dong.State == EntityState.Deleted
            ? null
            : NhatKyJson.ChuyenSangJson(
                thayDoi.Select(p => KeyValuePair.Create<string, object?>(p.Metadata.Name, p.CurrentValue)));

        var bang = dong.Metadata.GetTableName() ?? dong.Metadata.ClrType.Name;
        var schema = dong.Metadata.GetSchema();

        return new NhatKyHeThong
        {
            MaNhatKy = Guid.CreateVersion7(),
            ThoiDiem = dongHo.UtcNow,
            MaNguoiDung = nguoiDung.MaNguoiDung,
            MaSv = nguoiDung.MaSv,
            ThaoTac = dong.State switch
            {
                EntityState.Added => "THEM",
                EntityState.Modified => "SUA",
                _ => "XOA"
            },
            DoiTuong = schema is null ? bang : $"{schema}.{bang}",
            DoiTuongId = LayKhoaChinh(dong),
            GiaTriTruoc = truoc,
            GiaTriSau = sau
        };
    }
}

