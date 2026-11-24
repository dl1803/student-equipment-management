using Npgsql;
using QuanLyMuonTra.IntegrationTests.MuonTra;

namespace QuanLyMuonTra.IntegrationTests.Database;

public class CsdlTamThoiTests
{
    [CanKetNoiTestFact]
    public async Task Migration_CsdlTamThoi_TaoRoiXoaKhongConSot()
    {
        string ten;
        await using (var csdl = await CsdlTamThoi.TaoAsync())
        {
            ten = csdl.TenCsdl;
            Assert.True(await CsdlTamThoi.TonTaiAsync(ten));
            await using var ketNoi = await csdl.MoAsync();
            Assert.Equal(0, await LuocDoCsdl.DemBangAsync(ketNoi));
        }

        Assert.False(await CsdlTamThoi.TonTaiAsync(ten));
    }

    [CanKetNoiTestFact]
    public async Task Migration_DauVanTayLuocDo_GiongNhauThiKhongLechVaPhatHienCotMoiVaChiMucMoi()
    {
        await using var csdlA = await CsdlTamThoi.TaoAsync();
        await using var csdlB = await CsdlTamThoi.TaoAsync();
        const string ddl = "CREATE SCHEMA tai_khoan; CREATE TABLE tai_khoan.mau (id integer PRIMARY KEY, ten text NOT NULL);";
        await using var ketNoiA = await csdlA.MoAsync();
        await using var ketNoiB = await csdlB.MoAsync();
        await ThucThiAsync(ketNoiA, ddl);
        await ThucThiAsync(ketNoiB, ddl);

        var giongNhau = LuocDoCsdl.SoSanh(
            await LuocDoCsdl.LayDauVanTayAsync(ketNoiA),
            await LuocDoCsdl.LayDauVanTayAsync(ketNoiB));
        Assert.Empty(giongNhau);

        await ThucThiAsync(ketNoiB, "ALTER TABLE tai_khoan.mau ADD COLUMN them integer; CREATE INDEX ix_mau_ten ON tai_khoan.mau (ten);");
        var khacNhau = LuocDoCsdl.SoSanh(
            await LuocDoCsdl.LayDauVanTayAsync(ketNoiA),
            await LuocDoCsdl.LayDauVanTayAsync(ketNoiB));
        Assert.Contains(khacNhau, dong => dong.StartsWith("+ cot|tai_khoan.mau.them|", StringComparison.Ordinal));
        Assert.Contains(khacNhau, dong => dong.StartsWith("+ idx|tai_khoan.ix_mau_ten|", StringComparison.Ordinal));
    }

    private static async Task ThucThiAsync(NpgsqlConnection ketNoi, string sql)
    {
        await using var lenh = new NpgsqlCommand(sql, ketNoi);
        await lenh.ExecuteNonQueryAsync();
    }
}
