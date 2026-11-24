namespace QuanLyMuonTra.IntegrationTests.Database;

public sealed class CanBundleMigrationFactAttribute : FactAttribute
{
    public CanBundleMigrationFactAttribute()
    {
        if (BundleMigration.LayDuongDan() is null)
        {
            Skip = "Chưa có bundle: đặt MigrationBundle:Path (user-secrets của IntegrationTests) hoặc biến môi trường MigrationBundle__Path.";
        }
    }
}

public sealed class CanBundlePhienBanTruocFactAttribute : FactAttribute
{
    public CanBundlePhienBanTruocFactAttribute()
    {
        if (BundleMigration.LayDuongDan() is null || BundleMigration.LayPhienBanTruoc() is null)
        {
            Skip = "Chưa có bundle hoặc phiên bản trước: đặt MigrationBundle:Path và MigrationBundle:PhienBanTruoc (user-secrets hoặc biến môi trường MigrationBundle__...).";
        }
    }
}

public class MigrationBundleTests
{
    [CanBundleMigrationFact]
    public async Task Migration_CsdlTrong_BundleTaoDuMuoiMotSchemaVaBaMuoiLamBang()
    {
        await using var csdl = await CsdlTamThoi.TaoAsync();

        await ChayVaKiemTraAsync(BundleMigration.BatBuocDuongDan(), csdl, null);

        await using var ketNoi = await csdl.MoAsync();
        Assert.Equal(11, await LuocDoCsdl.DemSchemaAsync(ketNoi));
        Assert.Equal(35, await LuocDoCsdl.DemBangAsync(ketNoi));
    }

    [CanBundlePhienBanTruocFact]
    public async Task Migration_CsdlPhienBanTruoc_NangCapRaCungLuocDoVoiCsdlTrong()
    {
        var bundle = BundleMigration.BatBuocDuongDan();
        var phienBanTruoc = BundleMigration.BatBuocPhienBanTruoc();
        await using var csdlMoi = await CsdlTamThoi.TaoAsync();
        await using var csdlNangCap = await CsdlTamThoi.TaoAsync();

        await ChayVaKiemTraAsync(bundle, csdlMoi, null);
        await ChayVaKiemTraAsync(bundle, csdlNangCap, phienBanTruoc);
        await ChayVaKiemTraAsync(bundle, csdlNangCap, null);

        await using var ketNoiMoi = await csdlMoi.MoAsync();
        await using var ketNoiNangCap = await csdlNangCap.MoAsync();
        Assert.Equal(11, await LuocDoCsdl.DemSchemaAsync(ketNoiNangCap));
        Assert.Equal(35, await LuocDoCsdl.DemBangAsync(ketNoiNangCap));
        var chenhLech = LuocDoCsdl.SoSanh(
            await LuocDoCsdl.LayDauVanTayAsync(ketNoiMoi),
            await LuocDoCsdl.LayDauVanTayAsync(ketNoiNangCap));
        if (chenhLech.Count > 0)
        {
            Assert.Fail("Lược đồ nâng cấp khác CSDL trống:" + Environment.NewLine + string.Join(Environment.NewLine, chenhLech.Take(30)));
        }
    }

    private static async Task ChayVaKiemTraAsync(string bundle, CsdlTamThoi csdl, string? phienBan)
    {
        var ketQua = await BundleMigration.ChayAsync(bundle, csdl.ChuoiKetNoi, phienBan);
        if (ketQua.MaThoat != 0)
        {
            Assert.Fail($"Bundle trả mã thoát {ketQua.MaThoat} (phiên bản: {phienBan ?? "mới nhất"}):{Environment.NewLine}{ketQua.DauRa}");
        }
    }
}
