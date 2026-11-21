using System.Net;
using System.Text.RegularExpressions;
using QuanLyMuonTra.Modules.ThongBao.Contracts;

namespace QuanLyMuonTra.Modules.ThongBao.Application;

public static class MauThongBao
{
    private static readonly Regex ThamSoRegex = new(@"\{(?<ten>[A-Za-z]+)\}", RegexOptions.Compiled);

    private static readonly Dictionary<string, Mau> CacMau = new()
    {
        [MaLoaiThongBao.NhacHanTra] = new(
            "Nhắc hạn trả thiết bị",
            "Phiếu mượn {maPhieu} sắp đến hạn trả lúc {hanTra}. Vui lòng trả thiết bị đúng hạn."),
        [MaLoaiThongBao.QuaHan] = new(
            "Phiếu mượn đã quá hạn",
            "Phiếu mượn {maPhieu} đã quá hạn trả (hạn trả: {hanTra}). Vui lòng mang thiết bị đến quầy để trả."),
        [MaLoaiThongBao.ThietBiChoSanSang] = new(
            "Thiết bị bạn đăng ký chờ đã sẵn sàng",
            "Thiết bị {tenLoai} bạn đăng ký chờ đã sẵn sàng. Thiết bị được giữ chỗ đến {hanGiuCho}, vui lòng đến quầy nhận trước thời điểm này."),
        [MaLoaiThongBao.DuyetHoacTuChoi] = new(
            "Kết quả xử lý yêu cầu mượn",
            "Yêu cầu mượn {maYeuCau} {ketQua}. {chiTiet}"),
        [MaLoaiThongBao.ViPhamPhiPhat] = new(
            "Biên bản vi phạm mới",
            "Biên bản vi phạm {maBienBan} đã được lập với tổng số tiền {tongTien}. Vui lòng nộp tiền trực tiếp tại quầy."),
        [MaLoaiThongBao.NhacBaoTri] = new(
            "Thiết bị đến hạn bảo trì",
            "Thiết bị {maThietBi} đến hạn bảo trì định kỳ vào ngày {ngayDenHan}.")
    };

    public static string TieuDe(string loaiThongBao) => LayMau(loaiThongBao).TieuDe;

    public static string TaoNoiDungUngDung(string loaiThongBao, IReadOnlyDictionary<string, string> thamSo)
    {
        var mau = LayMau(loaiThongBao);

        return ThamSoRegex.Replace(mau.NoiDung, khop =>
        {
            var ten = khop.Groups["ten"].Value;
            if (!thamSo.TryGetValue(ten, out var giaTri) || string.IsNullOrWhiteSpace(giaTri))
            {
                throw new ArgumentException($"Thiếu tham số '{ten}' cho mẫu thông báo {loaiThongBao}.", nameof(thamSo));
            }

            return giaTri.Trim();
        });
    }

    public static string TaoNoiDungEmailHtml(string loaiThongBao, IReadOnlyDictionary<string, string> thamSo) =>
        $"<p>{WebUtility.HtmlEncode(TaoNoiDungUngDung(loaiThongBao, thamSo)).Replace("\r\n", "<br/>").Replace("\n", "<br/>")}</p>";

    private static Mau LayMau(string loaiThongBao) =>
        CacMau.TryGetValue(loaiThongBao, out var mau)
            ? mau
            : throw new ArgumentException($"Loại thông báo không có mẫu: {loaiThongBao}.", nameof(loaiThongBao));

    private sealed record Mau(string TieuDe, string NoiDung);
}

