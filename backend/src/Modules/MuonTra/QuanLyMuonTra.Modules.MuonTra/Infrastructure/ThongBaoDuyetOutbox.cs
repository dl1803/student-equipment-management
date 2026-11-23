using System.Globalization;
using QuanLyMuonTra.Modules.MuonTra.Application;
using QuanLyMuonTra.Modules.ThongBao.Contracts;
using QuanLyMuonTra.SharedKernel;

namespace QuanLyMuonTra.Modules.MuonTra.Infrastructure;

public sealed class ThongBaoDuyetOutbox(IThongBaoOutbox outbox) : IThongBaoDuyetOutbox
{
    public const string TienToKhoaTrungLap = "DUYET";
    public const string DoiTuongLoai = "PHIEU_YEU_CAU";

    public Task GhiAsync(ThongBaoDuyetHoacTuChoi thongBao, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(thongBao);

        var yeuCau = new YeuCauGhiThongBao(
            MaLoaiThongBao.DuyetHoacTuChoi,
            thongBao.MaSv,
            null,
            new Dictionary<string, string>
            {
                [ThamSoThongBao.MaYeuCau] = thongBao.MaYeuCau,
                [ThamSoThongBao.KetQua] = thongBao.DaDuyet ? "đã được duyệt" : "đã bị từ chối",
                [ThamSoThongBao.ChiTiet] = TaoChiTiet(thongBao)
            },
            DoiTuongLoai,
            thongBao.MaYeuCau,
            $"{TienToKhoaTrungLap}:{thongBao.MaYeuCau}");

        return outbox.GhiAsync(yeuCau, ct);
    }

    private static string TaoChiTiet(ThongBaoDuyetHoacTuChoi thongBao)
    {
        if (!thongBao.DaDuyet)
        {
            if (string.IsNullOrWhiteSpace(thongBao.LyDoTuChoi))
            {
                throw new InvalidOperationException("Thông báo từ chối thiếu lý do.");
            }

            return $"Lý do: {thongBao.LyDoTuChoi}";
        }

        var han = thongBao.HanGiuCho
            ?? throw new InvalidOperationException("Thông báo duyệt thiếu hạn giữ chỗ.");

        return string.Create(
            CultureInfo.InvariantCulture,
            $"Thiết bị được giữ chỗ đến {VnTime.GioLich(han):HH:mm} ngày {VnTime.NgayLich(han):dd/MM/yyyy}. Vui lòng đến quầy nhận thiết bị trước thời điểm này.");
    }
}
