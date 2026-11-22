using QuanLyMuonTra.SharedKernel;

namespace QuanLyMuonTra.Modules.DanhMuc.Application;

internal readonly record struct ThayDoiAnhThietBi(IReadOnlyList<Guid> CanGo, IReadOnlyList<Guid> CanGan);

internal static class AnhThietBi
{
    internal static ThayDoiAnhThietBi TinhThayDoi(IReadOnlyCollection<Guid> hienCo, IReadOnlyCollection<Guid> moi) =>
        new(hienCo.Except(moi).ToList(), moi.Except(hienCo).ToList());

    internal static Task GanMoiAsync(IAnhService anh, string maThietBi, IReadOnlyList<Guid>? dsMaAnh, CancellationToken ct) =>
        dsMaAnh is { Count: > 0 }
            ? anh.GanAsync(dsMaAnh, MaDoiTuongAnh.ThietBi, maThietBi, MaGiaiDoanAnh.ThietBi, ct)
            : Task.CompletedTask;

    internal static async Task ThayAsync(IAnhService anh, string maThietBi, IReadOnlyList<Guid>? dsMaAnhMoi, CancellationToken ct)
    {
        if (dsMaAnhMoi is null)
        {
            return;
        }

        if (dsMaAnhMoi.Count > 5)
        {
            throw new LoiNghiepVuException("DU_LIEU_KHONG_HOP_LE", "Mỗi thiết bị tối đa 5 ảnh.");
        }

        if (dsMaAnhMoi.Distinct().Count() != dsMaAnhMoi.Count)
        {
            throw new LoiNghiepVuException("DU_LIEU_KHONG_HOP_LE", "Danh sách mã ảnh có mã trùng.");
        }

        var dsHienCo = await LayMaHienCoAsync(anh, maThietBi, ct);
        var thayDoi = TinhThayDoi(dsHienCo, dsMaAnhMoi);
        if (thayDoi.CanGo.Count > 0)
        {
            await anh.GoAnhThietBiAsync(maThietBi, thayDoi.CanGo, ct);
        }

        if (thayDoi.CanGan.Count > 0)
        {
            await anh.GanAsync(thayDoi.CanGan, MaDoiTuongAnh.ThietBi, maThietBi, MaGiaiDoanAnh.ThietBi, ct);
        }
    }

    internal static async Task GoTatCaAsync(IAnhService anh, string maThietBi, CancellationToken ct)
    {
        var dsHienCo = await LayMaHienCoAsync(anh, maThietBi, ct);
        if (dsHienCo.Count > 0)
        {
            await anh.GoAnhThietBiAsync(maThietBi, dsHienCo, ct);
        }
    }

    private static async Task<IReadOnlyList<Guid>> LayMaHienCoAsync(IAnhService anh, string maThietBi, CancellationToken ct)
    {
        var dsAnh = await anh.LayDsAnhCongKhaiAsync(MaDoiTuongAnh.ThietBi, maThietBi, ct);
        return dsAnh.Select(a => a.MaAnh).ToList();
    }
}
