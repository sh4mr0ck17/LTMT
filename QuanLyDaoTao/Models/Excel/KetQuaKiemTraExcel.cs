namespace QuanLyDaoTao.Models.Excel;

/// <summary>
/// Đại diện cho một dòng dữ liệu được đọc từ Excel để phục vụ xem trước (Preview).
/// </summary>
public class DongDuLieuPreviewExcel
{
    /// <summary>
    /// Số dòng Excel thực tế (tính từ 1).
    /// </summary>
    public int SoDong { get; set; }

    /// <summary>
    /// Xác định xem dòng dữ liệu này có hợp lệ hoàn toàn hay không.
    /// </summary>
    public bool LaHopLe { get; set; } = true;

    /// <summary>
    /// Bản đồ chứa tên cột và giá trị chuỗi tương ứng của dòng đó.
    /// </summary>
    public Dictionary<string, string> DuLieuCot { get; set; } = new(StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// Danh sách các thông điệp lỗi thuộc về dòng này nếu có.
    /// </summary>
    public List<string> DanhSachLoi { get; set; } = new();

    /// <summary>
    /// Bản đồ lưu danh sách lỗi chi tiết theo từng tên cột trong dòng.
    /// Cột có lỗi sẽ hiển thị MÀU ĐỎ trên giao diện.
    /// Cột thỏa mãn điều kiện hợp lệ sẽ hiển thị MÀU XANH LÁ trên giao diện.
    /// </summary>
    public Dictionary<string, List<string>> LoiTheoCot { get; set; } = new(StringComparer.OrdinalIgnoreCase);
    /// <summary>
    /// Bản đồ lưu danh sách cảnh báo chi tiết theo từng tên cột trong dòng (Cột được tự động bù đắp dữ liệu).
    /// </summary>
    public Dictionary<string, List<string>> CanhBaoTheoCot { get; set; } = new(StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// Danh sách toàn bộ các cảnh báo thuộc về dòng này.
    /// </summary>
    public List<string> DanhSachCanhBao { get; set; } = new();

    /// <summary>
    /// Ghi nhận lỗi cho một cột cụ thể trong dòng.
    /// </summary>
    /// <param name="tenCot">Tên cột tiêu đề.</param>
    /// <param name="thongDiepLoi">Mô tả lỗi phát hiện.</param>
    public void GhiNhanLoiCot(string tenCot, string thongDiepLoi)
    {
        if (string.IsNullOrWhiteSpace(tenCot)) return;
        if (!LoiTheoCot.ContainsKey(tenCot))
        {
            LoiTheoCot[tenCot] = new List<string>();
        }
        LoiTheoCot[tenCot].Add(thongDiepLoi);
        DanhSachLoi.Add(thongDiepLoi);
        LaHopLe = false;
    }

    /// <summary>
    /// Ghi nhận cảnh báo tự động bù đắp dữ liệu cho một cột (Vẫn cho phép dòng hợp lệ để tiếp tục nhập).
    /// </summary>
    /// <param name="tenCot">Tên cột tiêu đề.</param>
    /// <param name="thongDiepCanhBao">Mô tả bù đắp dữ liệu.</param>
    public void GhiNhanCanhBaoCot(string tenCot, string thongDiepCanhBao)
    {
        if (string.IsNullOrWhiteSpace(tenCot)) return;
        if (!CanhBaoTheoCot.ContainsKey(tenCot))
        {
            CanhBaoTheoCot[tenCot] = new List<string>();
        }
        CanhBaoTheoCot[tenCot].Add(thongDiepCanhBao);
        DanhSachCanhBao.Add(thongDiepCanhBao);
    }

    /// <summary>
    /// Kiểm tra xem cột có bị cảnh báo/tự động bù đắp hay không (phục vụ hiển thị MÀU VÀNG CAM).
    /// </summary>
    public bool CotCoCanhBao(string tenCot) => CanhBaoTheoCot.TryGetValue(tenCot, out var cb) && cb.Count > 0;

    /// <summary>
    /// Lấy chuỗi thông điệp cảnh báo của cột.
    /// </summary>
    public string? LayCanhBaoCot(string tenCot) => CanhBaoTheoCot.TryGetValue(tenCot, out var cb) && cb.Count > 0 ? string.Join("; ", cb) : null;

    /// <summary>
    /// Kiểm tra xem cột có bị lỗi hay không (phục vụ hiển thị MÀU ĐỎ).
    /// </summary>
    public bool CotCoLoi(string tenCot) => LoiTheoCot.TryGetValue(tenCot, out var loi) && loi.Count > 0;

    /// <summary>
    /// Lấy chuỗi thông điệp lỗi của cột để hiển thị tooltip / badge cảnh báo.
    /// </summary>
    public string? LayLoiCot(string tenCot) => LoiTheoCot.TryGetValue(tenCot, out var loi) && loi.Count > 0 ? string.Join("; ", loi) : null;

    /// <summary>
    /// Kiểm tra xem cột có thỏa mãn dữ liệu hợp lệ hay không (phục vụ hiển thị MÀU XANH LÁ).
    /// </summary>
    public bool CotThoaMan(string tenCot)
    {
        if (CotCoLoi(tenCot)) return false;
        return DuLieuCot.TryGetValue(tenCot, out var v) && !string.IsNullOrWhiteSpace(v);
    }
}

/// <summary>
/// Đại diện cho kết quả kiểm tra toàn bộ tệp Excel trước khi ghi vào cơ sở dữ liệu.
/// </summary>
public class KetQuaKiemTraExcel
{
    /// <summary>
    /// Tên tệp Excel tải lên.
    /// </summary>
    public string TenTep { get; set; } = string.Empty;

    /// <summary>
    /// Tên trang tính (sheet) được xử lý.
    /// </summary>
    public string TenSheet { get; set; } = string.Empty;

    /// <summary>
    /// Tên module nghiệp vụ đang nhập liệu (Khoa, LopHoc, NhanSu, ...).
    /// </summary>
    public string TenModule { get; set; } = string.Empty;

    /// <summary>
    /// Tổng số dòng dữ liệu đọc được từ Excel (không tính dòng tiêu đề).
    /// </summary>
    public int TongSoDong { get; set; }

    /// <summary>
    /// Số lượng dòng hoàn toàn hợp lệ và sẵn sàng nhập.
    /// </summary>
    public int SoDongHopLe { get; set; }

    /// <summary>
    /// Số lượng dòng có lỗi không thể nhập.
    /// </summary>
    public int SoDongLoi { get; set; }

    /// <summary>
    /// Xác định xem toàn bộ tệp có hợp lệ hay không (không có bất kỳ dòng nào lỗi).
    /// </summary>
    public bool LaHopLeToanBo => SoDongLoi == 0 && TongSoDong > 0 && DanhSachLoi.Count == 0;

    /// <summary>
    /// Danh sách toàn bộ các lỗi chi tiết theo từng ô / trường / dòng.
    /// </summary>
    public List<LoiChiTietExcel> DanhSachLoi { get; set; } = new();

    /// <summary>
    /// Danh sách các dòng dữ liệu phục vụ hiển thị xem trước (Preview).
    /// </summary>
    public List<DongDuLieuPreviewExcel> DanhSachDongDuLieu { get; set; } = new();

    /// <summary>
    /// Danh sách các tên cột theo đúng thứ tự tiêu đề.
    /// </summary>
    public List<string> DanhSachCot { get; set; } = new();

    /// <summary>
    /// Cho phép tiếp tục import dữ liệu ngay cả khi thiếu trường thông tin trong 1 hàng (Chế độ nhập linh hoạt).
    /// </summary>
    public bool ChoPhepThieuTruong { get; set; } = true;

    /// <summary>
    /// Số lượng dòng được tự động bù đắp dữ liệu khuyết thiếu.
    /// </summary>
    public int SoDongDuocBuDap => DanhSachDongDuLieu.Count(d => d.DanhSachCanhBao.Count > 0);
}
