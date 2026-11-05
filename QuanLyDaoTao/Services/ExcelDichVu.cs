using ClosedXML.Excel;
using Microsoft.EntityFrameworkCore;
using QuanLyDaoTao.Data;
using QuanLyDaoTao.Models;
using QuanLyDaoTao.Models.Excel;
using System.Collections.Concurrent;
using System.Globalization;
using System.Net.Mail;
using System.Text.RegularExpressions;

namespace QuanLyDaoTao.Services;

/// <summary>
/// Lớp triển khai dịch vụ xử lý tệp Excel toàn diện: Tạo template, kiểm tra nghiêm ngặt, preview, commit và xuất báo cáo.
/// </summary>
public class ExcelDichVu : IExcelDichVu
{
    private readonly QuanLyDaoTaoDbContext _coSoDuLieu;
    private readonly INhatKyDichVu _nhatKyDichVu;
    private readonly IPhanQuyenDichVu? _phanQuyen;
    private readonly IBusThayDoiDuLieu? _bus;

    /// <summary>
    /// Vùng quản lý tương ứng của từng module nhập/xuất Excel.
    /// </summary>
    public static string? LayVungCuaModule(string? tenModule) => (tenModule ?? string.Empty).ToLowerInvariant() switch
    {
        "khoa" or "bomon" or "nhansu" => VungQuanLy.ToChuc,
        "lophoc" or "hocvien" or "monhoc" or "phancong" or "phanconggiangday" => VungQuanLy.DaoTao,
        _ => null
    };

    /// <summary>
    /// Giới hạn kích thước tệp Excel tối đa cho phép tải lên (5 MiB).
    /// </summary>
    public const long KichThuocTepToiDa = 5 * 1024 * 1024;

    /// <summary>
    /// Giới hạn số lượng dòng dữ liệu tối đa cho phép trong một tệp (1000 dòng).
    /// </summary>
    public const int SoDongDuLieuToiDa = 1000;

    /// <summary>
    /// Bộ nhớ tạm lưu trữ các phiên xem trước (Preview Session) trên máy chủ với cơ chế an toàn đa luồng.
    /// </summary>
    private static readonly ConcurrentDictionary<string, PhienPreviewExcel> _danhSachPhienPreview = new();

    /// <summary>
    /// Khởi tạo dịch vụ Excel với DbContext và Dịch vụ Nhật ký.
    /// </summary>
    /// <param name="coSoDuLieu">Ngữ cảnh cơ sở dữ liệu.</param>
    /// <param name="nhatKyDichVu">Dịch vụ nhật ký kiểm toán.</param>
    /// <param name="phanQuyen">Dịch vụ phân quyền (tùy chọn; vắng mặt khi kiểm thử đơn vị).</param>
    /// <param name="bus">Kênh thông báo thay đổi dữ liệu (tùy chọn).</param>
    // Dịch vụ nghiệp vụ: khi có, mỗi dòng Excel được ghi qua CÙNG quy tắc với biểu mẫu (sĩ số, chỉ tiêu chức vụ,
    // trùng CCCD/SĐT, niên khóa, chỉ tiêu lãnh đạo…). Chỉ để trống trong kiểm thử đơn vị cũ.
    private readonly IHocVienDichVu? _hocVienDichVu;
    private readonly INhanSuDichVu? _nhanSuDichVu;
    private readonly ILopHocDichVu? _lopHocDichVu;

    public ExcelDichVu(
        QuanLyDaoTaoDbContext coSoDuLieu,
        INhatKyDichVu nhatKyDichVu,
        IPhanQuyenDichVu? phanQuyen = null,
        IBusThayDoiDuLieu? bus = null,
        IHocVienDichVu? hocVienDichVu = null,
        INhanSuDichVu? nhanSuDichVu = null,
        ILopHocDichVu? lopHocDichVu = null)
    {
        _hocVienDichVu = hocVienDichVu;
        _nhanSuDichVu = nhanSuDichVu;
        _lopHocDichVu = lopHocDichVu;
        ArgumentNullException.ThrowIfNull(coSoDuLieu);
        ArgumentNullException.ThrowIfNull(nhatKyDichVu);

        _coSoDuLieu = coSoDuLieu;
        _nhatKyDichVu = nhatKyDichVu;
        _phanQuyen = phanQuyen;
        _bus = bus;
    }

    /// <summary>
    /// Chuẩn hóa tên module sang tên bảng nội bộ chuẩn trong hệ thống.
    /// </summary>
    private static string ChuanHoaTenBangModule(string tenModule)
    {
        return QuanTriDuLieuDichVu.ChuanHoaTenBang(tenModule);
    }

    /// <summary>
    /// Lấy danh sách trường tùy chỉnh đang kích hoạt cho module/bảng tương ứng (đồng bộ).
    /// </summary>
    private List<TruongTuyChinh> LayDanhSachTruongTuyChinh(string tenModule)
    {
        var bangChuan = ChuanHoaTenBangModule(tenModule);
        try
        {
            return _coSoDuLieu.TruongTuyChinhs
                .AsNoTracking()
                .Where(t => t.TenBang == bangChuan && t.CoHoatDong)
                .OrderBy(t => t.ThuTuHienThi)
                .ToList();
        }
        catch
        {
            return new List<TruongTuyChinh>();
        }
    }

    /// <summary>
    /// Lấy danh sách trường tùy chỉnh đang kích hoạt cho module/bảng tương ứng (bất đồng bộ).
    /// </summary>
    private async Task<List<TruongTuyChinh>> LayDanhSachTruongTuyChinhAsync(string tenModule, CancellationToken ct = default)
    {
        var bangChuan = ChuanHoaTenBangModule(tenModule);
        try
        {
            return await _coSoDuLieu.TruongTuyChinhs
                .AsNoTracking()
                .Where(t => t.TenBang == bangChuan && t.CoHoatDong)
                .OrderBy(t => t.ThuTuHienThi)
                .ToListAsync(ct);
        }
        catch
        {
            return new List<TruongTuyChinh>();
        }
    }

    /// <summary>
    /// Chuyển đổi định nghĩa trường tùy chỉnh trong DB thành đối tượng định nghĩa cột Excel.
    /// </summary>
    private static DinhNghiaCotExcel ChuyenDoiSangDinhNghiaCot(TruongTuyChinh c)
    {
        var kieu = c.KieuDuLieu?.Trim().ToLowerInvariant() switch
        {
            "songuyen" => KieuDuLieuExcel.SoNguyen,
            "ngaythang" => KieuDuLieuExcel.NgayThang,
            "logic" => KieuDuLieuExcel.Logic,
            _ => KieuDuLieuExcel.VanBan
        };

        var viDu = !string.IsNullOrWhiteSpace(c.GiaTriMacDinh)
            ? c.GiaTriMacDinh
            : (kieu == KieuDuLieuExcel.NgayThang ? "20/10/2026" : (kieu == KieuDuLieuExcel.SoNguyen ? "10" : "Dữ liệu mẫu"));

        return new DinhNghiaCotExcel
        {
            TenCot = c.TenHienThi,
            CacTenGoiKhac = new[] { c.MaTruong, $"Custom_{c.MaTruong}", c.MaTruong.ToLowerInvariant() },
            LaBatBuoc = c.LaBatBuoc,
            KieuDuLieu = kieu,
            DoDaiToiDa = 500,
            MoTa = $"[Tùy chỉnh: {c.KieuDuLieu}] {c.TenHienThi}",
            ViDu = viDu
        };
    }

    /// <summary>
    /// Tạo tệp Excel mẫu chuẩn (.xlsx) có kèm dòng hướng dẫn và dữ liệu ví dụ cho module được chỉ định.
    /// </summary>
    public byte[] TaoTepMauExcel(string tenModule)
    {
        var schema = DinhNghiaSchemaExcel.LaySchema(tenModule);
        var truongTuyChinhs = LayDanhSachTruongTuyChinh(tenModule);
        foreach (var c in truongTuyChinhs)
        {
            if (!schema.Any(s => string.Equals(s.TenCot, c.TenHienThi, StringComparison.OrdinalIgnoreCase)))
            {
                schema.Add(ChuyenDoiSangDinhNghiaCot(c));
            }
        }

        using var workbook = new XLWorkbook();
        var tenSheet = $"Mau_{tenModule}";
        var worksheet = workbook.Worksheets.Add(tenSheet);

        // Dòng 1: Tiêu đề cột chuẩn (Header)
        for (int colIndex = 0; colIndex < schema.Count; colIndex++)
        {
            var colDef = schema[colIndex];
            var cell = worksheet.Cell(1, colIndex + 1);
            cell.Value = colDef.TenCot;
            cell.Style.Font.Bold = true;
            cell.Style.Font.FontColor = XLColor.White;
            bool laTruongDong = truongTuyChinhs.Any(t => string.Equals(t.TenHienThi, colDef.TenCot, StringComparison.OrdinalIgnoreCase));
            cell.Style.Fill.BackgroundColor = laTruongDong
                ? XLColor.FromArgb(13, 110, 253) // Màu xanh lam phân biệt trường tùy chỉnh Live CRUD
                : XLColor.FromArgb(4, 121, 32);  // Xanh lá đậm CAND (#047920)
            cell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            cell.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
        }
        worksheet.Row(1).Height = 28;

        // Dòng 2: Hướng dẫn / Mô tả yêu cầu của trường
        for (int colIndex = 0; colIndex < schema.Count; colIndex++)
        {
            var colDef = schema[colIndex];
            var cell = worksheet.Cell(2, colIndex + 1);
            var yeuCau = colDef.LaBatBuoc ? "[Bắt buộc] " : "[Tùy chọn] ";
            cell.Value = $"{yeuCau}{colDef.MoTa}";
            cell.Style.Font.Italic = true;
            cell.Style.Font.FontSize = 9;
            cell.Style.Font.FontColor = XLColor.FromArgb(108, 117, 125);
            cell.Style.Fill.BackgroundColor = XLColor.FromArgb(248, 249, 250);
        }
        worksheet.Row(2).Height = 22;

        // Dòng 3: Dữ liệu mẫu (Ví dụ hợp lệ)
        for (int colIndex = 0; colIndex < schema.Count; colIndex++)
        {
            var colDef = schema[colIndex];
            var cell = worksheet.Cell(3, colIndex + 1);
            cell.SetValue(colDef.ViDu);
        }
        worksheet.Row(3).Height = 20;

        worksheet.Columns().AdjustToContents(10.0, 50.0);

        using var memoryStream = new MemoryStream();
        workbook.SaveAs(memoryStream);
        return memoryStream.ToArray();
    }

    /// <summary>
    /// Đọc, kiểm tra tính toàn vẹn (Header, Định dạng dữ liệu, Ràng buộc khóa ngoại, Trùng lặp) và sinh báo cáo xem trước.
    /// </summary>
    public async Task<KetQuaKiemTraExcel> KiemTraVaDocTepExcelAsync(
        string tenModule,
        Stream luongDuLieu,
        string tenTepGoc,
        bool choPhepThieuTruong = false,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(luongDuLieu);
        var ketQua = new KetQuaKiemTraExcel
        {
            TenTep = tenTepGoc,
            TenModule = tenModule,
            ChoPhepThieuTruong = choPhepThieuTruong
        };

        // 1. Kiểm tra kích thước tệp
        if (luongDuLieu.Length > KichThuocTepToiDa)
        {
            ketQua.DanhSachLoi.Add(new LoiChiTietExcel
            {
                SoDong = 0,
                TenTruong = "Tệp Excel",
                MaLoi = "ERR_FILE_SIZE_LIMIT",
                MoTaLoi = $"Dung lượng tệp vượt quá giới hạn cho phép (Tối đa 5 MB, hiện tại: {luongDuLieu.Length / (1024 * 1024.0):F2} MB).",
                HuongXuLy = "Vui lòng giảm bớt dung lượng tệp hoặc chia nhỏ dữ liệu trước khi tải lên.",
                MucDoLoi = "Lỗi nghiêm trọng"
            });
            return ketQua;
        }

        // 2. Mở workbook và lấy schema (kết hợp trường tĩnh và trường động Live CRUD)
        var schema = DinhNghiaSchemaExcel.LaySchema(tenModule);
        var truongTuyChinhs = await LayDanhSachTruongTuyChinhAsync(tenModule, cancellationToken);
        foreach (var c in truongTuyChinhs)
        {
            if (!schema.Any(s => string.Equals(s.TenCot, c.TenHienThi, StringComparison.OrdinalIgnoreCase)))
            {
                schema.Add(ChuyenDoiSangDinhNghiaCot(c));
            }
        }
        XLWorkbook workbook;
        try
        {
            workbook = new XLWorkbook(luongDuLieu);
        }
        catch (Exception ex)
        {
            ketQua.DanhSachLoi.Add(new LoiChiTietExcel
            {
                SoDong = 0,
                TenTruong = "Cấu trúc tệp",
                MaLoi = "ERR_CORRUPTED_FILE",
                MoTaLoi = $"Tệp Excel không đúng định dạng .xlsx hoặc bị lỗi cấu trúc: {ex.Message}",
                HuongXuLy = "Hãy chắc chắn rằng tệp được lưu ở định dạng Microsoft Excel (.xlsx) chuẩn.",
                MucDoLoi = "Lỗi nghiêm trọng"
            });
            return ketQua;
        }

        using (workbook)
        {
            var worksheet = workbook.Worksheets.FirstOrDefault();
            if (worksheet == null)
            {
                ketQua.DanhSachLoi.Add(new LoiChiTietExcel
                {
                    SoDong = 0,
                    TenTruong = "Trang tính",
                    MaLoi = "ERR_NO_SHEET",
                    MoTaLoi = "Tệp Excel không chứa bất kỳ trang tính (sheet) nào.",
                    HuongXuLy = "Vui lòng sử dụng tệp mẫu tải từ hệ thống và không xóa trang tính.",
                    MucDoLoi = "Lỗi nghiêm trọng"
                });
                return ketQua;
            }

            ketQua.TenSheet = worksheet.Name;

            // 3. KIỂM TRA HEADER NGHIÊM NGẶT (Strict Header Matching)
            var danhSachHeaderDocDuoc = new List<string>();
            var viTriCotTheoTenHeader = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
            var headerTrungLap = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            var firstRow = worksheet.Row(1);
            var lastCellUsed = firstRow.LastCellUsed();
            if (lastCellUsed == null)
            {
                ketQua.DanhSachLoi.Add(new LoiChiTietExcel
                {
                    SoDong = 1,
                    TenTruong = "Dòng tiêu đề",
                    MaLoi = "ERR_EMPTY_HEADER_ROW",
                    MoTaLoi = "Dòng tiêu đề đầu tiên trong trang tính bị rỗng.",
                    HuongXuLy = "Vui lòng tải tệp mẫu chuẩn và điền dữ liệu dưới dòng tiêu đề.",
                    MucDoLoi = "Lỗi nghiêm trọng"
                });
                return ketQua;
            }

            int tongSoCotHeader = lastCellUsed.Address.ColumnNumber;
            for (int col = 1; col <= tongSoCotHeader; col++)
            {
                var cellValue = firstRow.Cell(col).GetString().Trim();
                if (string.IsNullOrWhiteSpace(cellValue))
                {
                    continue;
                }

                if (viTriCotTheoTenHeader.ContainsKey(cellValue))
                {
                    headerTrungLap.Add(cellValue);
                    ketQua.DanhSachLoi.Add(new LoiChiTietExcel
                    {
                        SoDong = 1,
                        TenTruong = cellValue,
                        DiaChiO = firstRow.Cell(col).Address.ToString(),
                        GiaTriDauVao = cellValue,
                        MaLoi = "ERR_DUPLICATE_HEADER",
                        MoTaLoi = $"Cột tiêu đề '{cellValue}' bị trùng lặp nhiều lần trong tệp.",
                        HuongXuLy = "Vui lòng xóa các cột trùng lặp hoặc đặt lại tên tiêu đề chính xác.",
                        MucDoLoi = "Lỗi nghiêm trọng"
                    });
                }
                else
                {
                    viTriCotTheoTenHeader[cellValue] = col;
                    danhSachHeaderDocDuoc.Add(cellValue);
                }
            }

            // Ánh xạ từng định nghĩa cột trong schema sang vị trí cột thực tế trong file
            var anhXaCotSangIndex = new Dictionary<DinhNghiaCotExcel, int>();
            foreach (var colDef in schema)
            {
                int matchedCol = -1;
                // Khớp chính xác tên chuẩn
                if (viTriCotTheoTenHeader.TryGetValue(colDef.TenCot, out int colIndex))
                {
                    matchedCol = colIndex;
                }
                else
                {
                    // Khớp alias nếu có
                    foreach (var alias in colDef.CacTenGoiKhac)
                    {
                        if (viTriCotTheoTenHeader.TryGetValue(alias, out int aliasColIndex))
                        {
                            matchedCol = aliasColIndex;
                            break;
                        }
                    }
                }

                if (matchedCol > 0)
                {
                    anhXaCotSangIndex[colDef] = matchedCol;
                    ketQua.DanhSachCot.Add(colDef.TenCot);
                }
                else if (colDef.LaBatBuoc)
                {
                    ketQua.DanhSachLoi.Add(new LoiChiTietExcel
                    {
                        SoDong = 1,
                        TenTruong = colDef.TenCot,
                        MaLoi = "ERR_MISSING_REQUIRED_HEADER",
                        MoTaLoi = $"Thiếu cột tiêu đề bắt buộc: '{colDef.TenCot}'.",
                        HuongXuLy = $"Vui lòng bổ sung cột '{colDef.TenCot}' vào dòng tiêu đề hoặc tải tệp mẫu mới.",
                        MucDoLoi = "Lỗi nghiêm trọng"
                    });
                }
            }

            // Phát hiện cột lạ không thuộc schema
            foreach (var headerDoc in danhSachHeaderDocDuoc)
            {
                bool laHopLe = schema.Any(s =>
                    string.Equals(s.TenCot, headerDoc, StringComparison.OrdinalIgnoreCase) ||
                    s.CacTenGoiKhac.Any(a => string.Equals(a, headerDoc, StringComparison.OrdinalIgnoreCase)));

                if (!laHopLe)
                {
                    ketQua.DanhSachLoi.Add(new LoiChiTietExcel
                    {
                        SoDong = 1,
                        TenTruong = headerDoc,
                        GiaTriDauVao = headerDoc,
                        MaLoi = "ERR_UNKNOWN_HEADER",
                        MoTaLoi = $"Phát hiện cột không nhận diện được: '{headerDoc}'.",
                        HuongXuLy = "Vui lòng xóa cột lạ này hoặc đổi tên khớp với mẫu quy định.",
                        MucDoLoi = "Lỗi nghiêm trọng"
                    });
                }
            }

            // Nếu có lỗi ở header thì dừng ngay, không tiếp tục đọc data
            if (ketQua.DanhSachLoi.Count > 0)
            {
                return ketQua;
            }

            // 4. KIỂM TRA VÀ ĐỌC TỪNG DÒNG DỮ LIỆU (Data Rows)
            int lastRowUsed = worksheet.LastRowUsed()?.RowNumber() ?? 1;

            // Xác định dòng bắt đầu chứa dữ liệu:
            // Nếu dòng 2 là dòng hướng dẫn (bắt đầu bằng [Bắt buộc] hoặc [Tùy chọn]), ta bỏ qua dòng 2
            int startRow = 2;
            var dong2Cell1 = worksheet.Cell(2, 1).GetString().Trim();
            if (dong2Cell1.StartsWith("[Bắt buộc]") || dong2Cell1.StartsWith("[Tùy chọn]"))
            {
                startRow = 3;
            }

            int tongSoDongData = lastRowUsed - startRow + 1;
            if (tongSoDongData <= 0)
            {
                ketQua.DanhSachLoi.Add(new LoiChiTietExcel
                {
                    SoDong = 0,
                    TenTruong = "Dữ liệu",
                    MaLoi = "ERR_NO_DATA_ROWS",
                    MoTaLoi = "Tệp Excel không chứa bất kỳ dòng dữ liệu nào để nhập.",
                    HuongXuLy = "Vui lòng điền các dòng dữ liệu vào tệp Excel trước khi tải lên.",
                    MucDoLoi = "Lỗi nghiêm trọng"
                });
                return ketQua;
            }

            if (tongSoDongData > SoDongDuLieuToiDa)
            {
                ketQua.DanhSachLoi.Add(new LoiChiTietExcel
                {
                    SoDong = 0,
                    TenTruong = "Giới hạn dòng",
                    MaLoi = "ERR_MAX_ROWS_EXCEEDED",
                    MoTaLoi = $"Số dòng dữ liệu ({tongSoDongData}) vượt quá giới hạn tối đa cho phép ({SoDongDuLieuToiDa} dòng).",
                    HuongXuLy = "Vui lòng chia nhỏ tệp dữ liệu ra các lô dưới 1000 dòng để xử lý.",
                    MucDoLoi = "Lỗi nghiêm trọng"
                });
                return ketQua;
            }

            // Tải trước dữ liệu tham chiếu trong DB để kiểm tra nhanh (tránh N+1 queries)
            var tapMaKhoaTrongDb = await _coSoDuLieu.Khoas.AsNoTracking().Select(k => k.MaKhoa).ToListAsync(cancellationToken);
            var tapMaBoMonTrongDb = await _coSoDuLieu.BoMons.AsNoTracking().Select(bm => bm.MaBoMon).ToListAsync(cancellationToken);
            var tapMaLopTrongDb = await _coSoDuLieu.LopHocs.AsNoTracking().Select(l => l.MaLop).ToListAsync(cancellationToken);
            var tapMaHocVienTrongDb = await _coSoDuLieu.HocViens.AsNoTracking().Select(hv => hv.MaHocVien).ToListAsync(cancellationToken);
            var tapMaNhanSuTrongDb = await _coSoDuLieu.NhanSus.AsNoTracking().Select(ns => ns.MaNhanSu).ToListAsync(cancellationToken);
            var tapEmailNhanSuTrongDb = await _coSoDuLieu.NhanSus.AsNoTracking().Select(ns => ns.Email).ToListAsync(cancellationToken);
            var tapMaChucVuTrongDb = await _coSoDuLieu.ChucVus.AsNoTracking().Select(cv => cv.MaChucVu).ToListAsync(cancellationToken);
            var tapMaTrinhDoTrongDb = await _coSoDuLieu.TrinhDoChuyenMons.AsNoTracking().Select(td => td.MaTrinhDo).ToListAsync(cancellationToken);
            var tapMaMonHocTrongDb = await _coSoDuLieu.MonHocs.AsNoTracking().Select(mh => mh.MaMonHoc).ToListAsync(cancellationToken);
            var tapMaPhanCongTrongDb = await _coSoDuLieu.PhanCongGiangDays.AsNoTracking().Select(pc => pc.MaPhanCong).ToListAsync(cancellationToken);

            // Tập lưu mã nghiệp vụ xuất hiện trong file để bắt trùng trong cùng 1 file
            var tapMaChinhTrongFile = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            var tapEmailTrongFile = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            for (int row = startRow; row <= lastRowUsed; row++)
            {
                var rowObj = worksheet.Row(row);
                // Kiểm tra dòng hoàn toàn rỗng thì bỏ qua
                bool dongRong = true;
                foreach (var kvp in anhXaCotSangIndex)
                {
                    if (!string.IsNullOrWhiteSpace(rowObj.Cell(kvp.Value).GetString()))
                    {
                        dongRong = false;
                        break;
                    }
                }
                if (dongRong)
                {
                    continue;
                }

                ketQua.TongSoDong++;
                var dongPreview = new DongDuLieuPreviewExcel
                {
                    SoDong = row,
                    LaHopLe = true
                };

                // Đọc và kiểm tra từng ô theo schema
                foreach (var colDef in schema)
                {
                    if (!anhXaCotSangIndex.TryGetValue(colDef, out int colIdx))
                    {
                        continue;
                    }

                    var cell = rowObj.Cell(colIdx);
                    // Đọc an toàn giữ nguyên số 0 đầu
                    string giaTriO = cell.GetString().Trim();
                    string diaChiO = cell.Address?.ToString() ?? $"R{row}C{colIdx}";

                    // A. Chuẩn hóa trước định dạng ngày tháng nếu là cột NgayThang
                    if (colDef.KieuDuLieu == KieuDuLieuExcel.NgayThang && !string.IsNullOrWhiteSpace(giaTriO))
                    {
                        if (KiemTraNgayThangHopLe(cell, out var ngayPhanTichPre))
                        {
                            giaTriO = ngayPhanTichPre.ToString("dd/MM/yyyy");
                        }
                    }

                    dongPreview.DuLieuCot[colDef.TenCot] = giaTriO;

                    // Kiểm tra trường bắt buộc
                    if (string.IsNullOrWhiteSpace(giaTriO))
                    {
                        if (colDef.LaBatBuoc)
                        {
                            if (choPhepThieuTruong && DuocTuBuDap(colDef.TenCot))
                            {
                                // Tự động bù đắp thông minh khi thiếu trường trong 1 hàng để tiếp tục nhập
                                string giaTriBuDap;
                                if (string.Equals(colDef.TenCot, "Mã học viên", StringComparison.OrdinalIgnoreCase))
                                {
                                    giaTriBuDap = $"HV_{DateTime.Now:yyyyMMdd}_{row:D4}";
                                    while (tapMaChinhTrongFile.Contains(giaTriBuDap) || tapMaHocVienTrongDb.Contains(giaTriBuDap))
                                    {
                                        giaTriBuDap = $"HV_{DateTime.Now:yyyyMMdd}_{Guid.NewGuid().ToString("N")[..4].ToUpper()}";
                                    }
                                }
                                else if (string.Equals(colDef.TenCot, "Mã nhân sự", StringComparison.OrdinalIgnoreCase))
                                {
                                    giaTriBuDap = $"CB_AUTOGEN_{row:D4}";
                                    while (tapMaChinhTrongFile.Contains(giaTriBuDap) || tapMaNhanSuTrongDb.Contains(giaTriBuDap))
                                    {
                                        giaTriBuDap = $"CB_AUTOGEN_{Guid.NewGuid().ToString("N")[..4].ToUpper()}";
                                    }
                                }
                                else if (string.Equals(colDef.TenCot, "Mã phân công", StringComparison.OrdinalIgnoreCase))
                                {
                                    giaTriBuDap = $"PC_{DateTime.Now:yyyyMMdd}_{row:D4}";
                                }
                                else
                                {
                                    giaTriBuDap = "Hoạt động";
                                }

                                giaTriO = giaTriBuDap;
                                dongPreview.DuLieuCot[colDef.TenCot] = giaTriO;
                                dongPreview.GhiNhanCanhBaoCot(colDef.TenCot, $"Trường '{colDef.TenCot}' để trống, đã tự động bù đắp: '{giaTriO}'");

                                ketQua.DanhSachLoi.Add(new LoiChiTietExcel
                                {
                                    SoDong = row,
                                    TenTruong = colDef.TenCot,
                                    DiaChiO = diaChiO,
                                    GiaTriDauVao = "(Trống)",
                                    MaLoi = "WARN_AUTO_FILLED",
                                    MoTaLoi = $"Trường '{colDef.TenCot}' để trống đã được tự động bù đắp giá trị '{giaTriO}' để tiếp tục nhập.",
                                    HuongXuLy = "Hệ thống tự động bù đắp dữ liệu hợp lệ.",
                                    MucDoLoi = "Cảnh báo"
                                });

                                if (LaCotKhoaChinh(tenModule, colDef.TenCot))
                                {
                                    tapMaChinhTrongFile.Add(giaTriO);
                                }
                            }
                            else
                            {
                                var loi = new LoiChiTietExcel
                                {
                                    SoDong = row,
                                    TenTruong = colDef.TenCot,
                                    DiaChiO = diaChiO,
                                    GiaTriDauVao = "(Trống)",
                                    MaLoi = "ERR_REQUIRED_FIELD_EMPTY",
                                    MoTaLoi = $"Trường '{colDef.TenCot}' là bắt buộc nhưng đang bị để trống.",
                                    HuongXuLy = $"Vui lòng nhập giá trị hợp lệ cho trường '{colDef.TenCot}'.",
                                    MucDoLoi = "Lỗi nghiêm trọng"
                                };
                                ketQua.DanhSachLoi.Add(loi);
                                dongPreview.GhiNhanLoiCot(colDef.TenCot, loi.MoTaLoi);
                                continue;
                            }
                        }
                        else
                        {
                            continue;
                        }
                    }

                    // B. Kiểm tra độ dài
                    if (giaTriO.Length > colDef.DoDaiToiDa)
                    {
                        var loi = new LoiChiTietExcel
                        {
                            SoDong = row,
                            TenTruong = colDef.TenCot,
                            DiaChiO = diaChiO,
                            GiaTriDauVao = giaTriO,
                            MaLoi = "ERR_MAX_LENGTH_EXCEEDED",
                            MoTaLoi = $"Độ dài của '{colDef.TenCot}' ({giaTriO.Length} ký tự) vượt quá giới hạn cho phép ({colDef.DoDaiToiDa} ký tự).",
                            HuongXuLy = $"Vui lòng rút ngắn nội dung không vượt quá {colDef.DoDaiToiDa} ký tự.",
                            MucDoLoi = "Lỗi nghiêm trọng"
                        };
                        ketQua.DanhSachLoi.Add(loi);
                        dongPreview.GhiNhanLoiCot(colDef.TenCot, loi.MoTaLoi);
                    }

                    // C. Kiểm tra Giới tính (nếu cột là Giới tính)
                    if (string.Equals(colDef.TenCot, "Giới tính", StringComparison.OrdinalIgnoreCase) ||
                        colDef.CacTenGoiKhac.Any(a => string.Equals(a, "GioiTinh", StringComparison.OrdinalIgnoreCase)))
                    {
                        var gt = giaTriO.Trim();
                        if (!string.Equals(gt, "Nam", StringComparison.OrdinalIgnoreCase) &&
                            !string.Equals(gt, "Nữ", StringComparison.OrdinalIgnoreCase))
                        {
                            if (choPhepThieuTruong)
                            {
                                string gtSuyLuan = SuyLuanGioiTinh(dongPreview.DuLieuCot.TryGetValue("Họ và tên", out var hvt) ? hvt : "");
                                dongPreview.DuLieuCot[colDef.TenCot] = gtSuyLuan;
                                dongPreview.GhiNhanCanhBaoCot(colDef.TenCot, $"Giới tính '{giaTriO}' chưa chuẩn, tự động nhận diện thành '{gtSuyLuan}'");
                                ketQua.DanhSachLoi.Add(new LoiChiTietExcel
                                {
                                    SoDong = row,
                                    TenTruong = colDef.TenCot,
                                    DiaChiO = diaChiO,
                                    GiaTriDauVao = giaTriO,
                                    MaLoi = "WARN_GENDER_AUTO_FIXED",
                                    MoTaLoi = $"Giới tính '{giaTriO}' được tự động chuẩn hóa thành '{gtSuyLuan}'.",
                                    HuongXuLy = "Hệ thống tự động chuẩn hóa giới tính hợp lệ.",
                                    MucDoLoi = "Cảnh báo"
                                });
                            }
                            else
                            {
                                var loi = new LoiChiTietExcel
                                {
                                    SoDong = row,
                                    TenTruong = colDef.TenCot,
                                    DiaChiO = diaChiO,
                                    GiaTriDauVao = giaTriO,
                                    MaLoi = "ERR_INVALID_GENDER",
                                    MoTaLoi = $"Giới tính '{giaTriO}' không hợp lệ (chỉ chấp nhận 'Nam' hoặc 'Nữ').",
                                    HuongXuLy = "Vui lòng nhập chính xác 'Nam' hoặc 'Nữ'.",
                                    MucDoLoi = "Lỗi nghiêm trọng"
                                };
                                ketQua.DanhSachLoi.Add(loi);
                                dongPreview.GhiNhanLoiCot(colDef.TenCot, loi.MoTaLoi);
                            }
                        }
                    }

                    // D. Kiểm tra kiểu dữ liệu và định dạng chi tiết
                    switch (colDef.KieuDuLieu)
                    {
                        case KieuDuLieuExcel.NgayThang:
                            if (!KiemTraNgayThangHopLe(cell, out var ngayPhanTich))
                            {
                                if (choPhepThieuTruong)
                                {
                                    string ngayBuDap = "01/01/2007";
                                    dongPreview.DuLieuCot[colDef.TenCot] = ngayBuDap;
                                    dongPreview.GhiNhanCanhBaoCot(colDef.TenCot, $"Ngày sinh '{giaTriO}' không đúng định dạng, tự động gán '{ngayBuDap}'");
                                    ketQua.DanhSachLoi.Add(new LoiChiTietExcel
                                    {
                                        SoDong = row,
                                        TenTruong = colDef.TenCot,
                                        DiaChiO = diaChiO,
                                        GiaTriDauVao = giaTriO,
                                        MaLoi = "WARN_DATE_AUTO_FIXED",
                                        MoTaLoi = $"Ngày tháng '{giaTriO}' không đúng chuẩn dd/MM/yyyy, đã tự động gán '{ngayBuDap}' để tiếp tục nhập.",
                                        HuongXuLy = "Hệ thống tự động bù đắp ngày mặc định.",
                                        MucDoLoi = "Cảnh báo"
                                    });
                                }
                                else
                                {
                                    var loi = new LoiChiTietExcel
                                    {
                                        SoDong = row,
                                        TenTruong = colDef.TenCot,
                                        DiaChiO = diaChiO,
                                        GiaTriDauVao = giaTriO,
                                        MaLoi = "ERR_INVALID_DATE_FORMAT",
                                        MoTaLoi = $"Giá trị '{giaTriO}' không phải là ngày tháng hợp lệ.",
                                        HuongXuLy = "Vui lòng nhập ngày tháng theo đúng định dạng ngày/tháng/năm (ví dụ: 25/12/2000).",
                                        MucDoLoi = "Lỗi nghiêm trọng"
                                    };
                                    ketQua.DanhSachLoi.Add(loi);
                                    dongPreview.GhiNhanLoiCot(colDef.TenCot, loi.MoTaLoi);
                                }
                            }
                            else
                            {
                                dongPreview.DuLieuCot[colDef.TenCot] = ngayPhanTich.ToString("dd/MM/yyyy");
                            }
                            break;

                        case KieuDuLieuExcel.Email:
                            if (!KiemTraEmailHopLe(giaTriO))
                            {
                                var loi = new LoiChiTietExcel
                                {
                                    SoDong = row,
                                    TenTruong = colDef.TenCot,
                                    DiaChiO = diaChiO,
                                    GiaTriDauVao = giaTriO,
                                    MaLoi = "ERR_INVALID_EMAIL_FORMAT",
                                    MoTaLoi = $"Địa chỉ email '{giaTriO}' không đúng định dạng.",
                                    HuongXuLy = "Vui lòng kiểm tra lại địa chỉ email (ví dụ: user@quanlydaotao.local).",
                                    MucDoLoi = "Lỗi nghiêm trọng"
                                };
                                ketQua.DanhSachLoi.Add(loi);
                                dongPreview.GhiNhanLoiCot(colDef.TenCot, loi.MoTaLoi);
                            }
                            else if (string.Equals(colDef.TenCot, "Email công việc", StringComparison.OrdinalIgnoreCase) ||
                                     string.Equals(colDef.TenCot, "Email", StringComparison.OrdinalIgnoreCase))
                            {
                                if (string.Equals(tenModule, "NhanSu", StringComparison.OrdinalIgnoreCase))
                                {
                                    if (tapEmailTrongFile.Contains(giaTriO))
                                    {
                                        var loi = new LoiChiTietExcel
                                        {
                                            SoDong = row,
                                            TenTruong = colDef.TenCot,
                                            DiaChiO = diaChiO,
                                            GiaTriDauVao = giaTriO,
                                            MaLoi = "ERR_DUPLICATE_EMAIL_IN_FILE",
                                            MoTaLoi = $"Email '{giaTriO}' bị trùng lặp với dòng khác trong cùng tệp.",
                                            HuongXuLy = "Mỗi nhân sự phải có email duy nhất trong tệp.",
                                            MucDoLoi = "Lỗi nghiêm trọng"
                                        };
                                        ketQua.DanhSachLoi.Add(loi);
                                        dongPreview.GhiNhanLoiCot(colDef.TenCot, loi.MoTaLoi);
                                    }
                                    else
                                    {
                                        tapEmailTrongFile.Add(giaTriO);
                                    }

                                    if (tapEmailNhanSuTrongDb.Contains(giaTriO, StringComparer.OrdinalIgnoreCase))
                                    {
                                        var loi = new LoiChiTietExcel
                                        {
                                            SoDong = row,
                                            TenTruong = colDef.TenCot,
                                            DiaChiO = diaChiO,
                                            GiaTriDauVao = giaTriO,
                                            MaLoi = "ERR_DUPLICATE_EMAIL_IN_DB",
                                            MoTaLoi = $"Email '{giaTriO}' đã tồn tại trong cơ sở dữ liệu hệ thống.",
                                            HuongXuLy = "Vui lòng sử dụng địa chỉ email chưa được đăng ký.",
                                            MucDoLoi = "Lỗi nghiêm trọng"
                                        };
                                        ketQua.DanhSachLoi.Add(loi);
                                        dongPreview.GhiNhanLoiCot(colDef.TenCot, loi.MoTaLoi);
                                    }
                                }
                            }
                            break;

                        case KieuDuLieuExcel.SoDienThoai:
                            var phoneCleaned = Regex.Replace(giaTriO, @"[\s\.\-\(\)]", "");
                            if (!Regex.IsMatch(phoneCleaned, @"^0\d{9,10}$"))
                            {
                                var loi = new LoiChiTietExcel
                                {
                                    SoDong = row,
                                    TenTruong = colDef.TenCot,
                                    DiaChiO = diaChiO,
                                    GiaTriDauVao = giaTriO,
                                    MaLoi = "ERR_INVALID_PHONE_NUMBER",
                                    MoTaLoi = $"Số điện thoại '{giaTriO}' không đúng định dạng (yêu cầu số bắt đầu bằng số 0 và có từ 10 đến 11 chữ số).",
                                    HuongXuLy = "Vui lòng kiểm tra lại số điện thoại (ví dụ: 0912345678).",
                                    MucDoLoi = "Lỗi nghiêm trọng"
                                };
                                ketQua.DanhSachLoi.Add(loi);
                                dongPreview.GhiNhanLoiCot(colDef.TenCot, loi.MoTaLoi);
                            }
                            else
                            {
                                dongPreview.DuLieuCot[colDef.TenCot] = phoneCleaned;
                            }
                            break;

                        case KieuDuLieuExcel.Logic:
                            var logicStr = giaTriO.Trim();
                            bool laLogicHopLe = string.Equals(logicStr, "Hoạt động", StringComparison.OrdinalIgnoreCase) ||
                                                string.Equals(logicStr, "Đã khóa", StringComparison.OrdinalIgnoreCase) ||
                                                string.Equals(logicStr, "Có", StringComparison.OrdinalIgnoreCase) ||
                                                string.Equals(logicStr, "Không", StringComparison.OrdinalIgnoreCase) ||
                                                string.Equals(logicStr, "1", StringComparison.OrdinalIgnoreCase) ||
                                                string.Equals(logicStr, "0", StringComparison.OrdinalIgnoreCase) ||
                                                string.Equals(logicStr, "True", StringComparison.OrdinalIgnoreCase) ||
                                                string.Equals(logicStr, "False", StringComparison.OrdinalIgnoreCase) ||
                                                string.Equals(logicStr, "Đang học", StringComparison.OrdinalIgnoreCase) ||
                                                string.Equals(logicStr, "Đã tốt nghiệp", StringComparison.OrdinalIgnoreCase) ||
                                                string.Equals(logicStr, "Thôi học", StringComparison.OrdinalIgnoreCase);
                            if (!laLogicHopLe)
                            {
                                var loi = new LoiChiTietExcel
                                {
                                    SoDong = row,
                                    TenTruong = colDef.TenCot,
                                    DiaChiO = diaChiO,
                                    GiaTriDauVao = giaTriO,
                                    MaLoi = "ERR_INVALID_LOGIC_VALUE",
                                    MoTaLoi = $"Giá trị '{giaTriO}' không hợp lệ cho trường trạng thái/logic.",
                                    HuongXuLy = "Vui lòng nhập giá trị hợp lệ: Hoạt động, Đã khóa, Có, Không, 1 hoặc 0.",
                                    MucDoLoi = "Lỗi nghiêm trọng"
                                };
                                ketQua.DanhSachLoi.Add(loi);
                                dongPreview.GhiNhanLoiCot(colDef.TenCot, loi.MoTaLoi);
                            }
                            break;

                        case KieuDuLieuExcel.SoNguyen:
                            if (!int.TryParse(giaTriO, out int soNguyen) || soNguyen < 0)
                            {
                                var loi = new LoiChiTietExcel
                                {
                                    SoDong = row,
                                    TenTruong = colDef.TenCot,
                                    DiaChiO = diaChiO,
                                    GiaTriDauVao = giaTriO,
                                    MaLoi = "ERR_INVALID_INTEGER",
                                    MoTaLoi = $"Giá trị '{giaTriO}' không phải là số nguyên dương hợp lệ.",
                                    HuongXuLy = "Vui lòng nhập một số nguyên không âm.",
                                    MucDoLoi = "Lỗi nghiêm trọng"
                                };
                                ketQua.DanhSachLoi.Add(loi);
                                dongPreview.GhiNhanLoiCot(colDef.TenCot, loi.MoTaLoi);
                            }
                            break;

                        case KieuDuLieuExcel.KhoaNgoai:
                            KiemTraKhoaNgoaiTonTai(tenModule, colDef.TenCot, giaTriO, row, diaChiO,
                                tapMaKhoaTrongDb, tapMaBoMonTrongDb, tapMaLopTrongDb, tapMaNhanSuTrongDb,
                                tapMaChucVuTrongDb, tapMaTrinhDoTrongDb, tapMaMonHocTrongDb,
                                ketQua, dongPreview, choPhepThieuTruong);
                            break;
                    }

                    // E. Kiểm tra khóa chính / mã duy nhất của bản ghi
                    if (LaCotKhoaChinh(tenModule, colDef.TenCot))
                    {
                        // Kiểm tra trùng trong file
                        if (tapMaChinhTrongFile.Contains(giaTriO))
                        {
                            if (choPhepThieuTruong)
                            {
                                string maMoi = $"{giaTriO}_{row:D3}";
                                dongPreview.DuLieuCot[colDef.TenCot] = maMoi;
                                dongPreview.GhiNhanCanhBaoCot(colDef.TenCot, $"Mã '{giaTriO}' trùng trong tệp, tự đổi thành '{maMoi}' để tiếp tục");
                                tapMaChinhTrongFile.Add(maMoi);
                                ketQua.DanhSachLoi.Add(new LoiChiTietExcel
                                {
                                    SoDong = row,
                                    TenTruong = colDef.TenCot,
                                    DiaChiO = diaChiO,
                                    GiaTriDauVao = giaTriO,
                                    MaLoi = "WARN_DUPLICATE_KEY_AUTO_FIXED",
                                    MoTaLoi = $"Mã '{giaTriO}' bị trùng lặp trong tệp, đã tự động bù đắp mã mới '{maMoi}' để tiếp tục nhập.",
                                    HuongXuLy = "Hệ thống tự động cấp mã duy nhất.",
                                    MucDoLoi = "Cảnh báo"
                                });
                            }
                            else
                            {
                                var loi = new LoiChiTietExcel
                                {
                                    SoDong = row,
                                    TenTruong = colDef.TenCot,
                                    DiaChiO = diaChiO,
                                    GiaTriDauVao = giaTriO,
                                    MaLoi = "ERR_DUPLICATE_KEY_IN_FILE",
                                    MoTaLoi = $"Mã '{giaTriO}' bị trùng lặp với dòng khác trong cùng tệp.",
                                    HuongXuLy = "Vui lòng đảm bảo mỗi mã nghiệp vụ là duy nhất trong tệp.",
                                    MucDoLoi = "Lỗi nghiêm trọng"
                                };
                                ketQua.DanhSachLoi.Add(loi);
                                dongPreview.GhiNhanLoiCot(colDef.TenCot, loi.MoTaLoi);
                            }
                        }
                        else
                        {
                            tapMaChinhTrongFile.Add(giaTriO);
                        }

                        // Kiểm tra trùng trong DB
                        if (KiemTraMaTonTaiTrongDb(tenModule, giaTriO,
                            tapMaKhoaTrongDb, tapMaBoMonTrongDb, tapMaLopTrongDb, tapMaHocVienTrongDb,
                            tapMaNhanSuTrongDb, tapMaMonHocTrongDb, tapMaPhanCongTrongDb))
                        {
                            if (choPhepThieuTruong)
                            {
                                string maMoi = $"{giaTriO}_{Guid.NewGuid().ToString("N")[..4].ToUpper()}";
                                dongPreview.DuLieuCot[colDef.TenCot] = maMoi;
                                dongPreview.GhiNhanCanhBaoCot(colDef.TenCot, $"Mã '{giaTriO}' đã có trong DB, tự đổi thành '{maMoi}' để tiếp tục");
                                tapMaChinhTrongFile.Add(maMoi);
                                ketQua.DanhSachLoi.Add(new LoiChiTietExcel
                                {
                                    SoDong = row,
                                    TenTruong = colDef.TenCot,
                                    DiaChiO = diaChiO,
                                    GiaTriDauVao = giaTriO,
                                    MaLoi = "WARN_DUPLICATE_DB_AUTO_FIXED",
                                    MoTaLoi = $"Mã '{giaTriO}' đã tồn tại trong CSDL, đã tự động bù đắp mã mới '{maMoi}' để tiếp tục nhập.",
                                    HuongXuLy = "Hệ thống tự động cấp mã duy nhất.",
                                    MucDoLoi = "Cảnh báo"
                                });
                            }
                            else
                            {
                                var loi = new LoiChiTietExcel
                                {
                                    SoDong = row,
                                    TenTruong = colDef.TenCot,
                                    DiaChiO = diaChiO,
                                    GiaTriDauVao = giaTriO,
                                    MaLoi = "ERR_DUPLICATE_KEY_IN_DB",
                                    MoTaLoi = $"Mã '{giaTriO}' đã tồn tại trong cơ sở dữ liệu hệ thống.",
                                    HuongXuLy = "Vui lòng đổi sang một mã mới chưa tồn tại trong hệ thống.",
                                    MucDoLoi = "Lỗi nghiêm trọng"
                                };
                                ketQua.DanhSachLoi.Add(loi);
                                dongPreview.GhiNhanLoiCot(colDef.TenCot, loi.MoTaLoi);
                            }
                        }
                    }
                }

                if (dongPreview.LaHopLe)
                {
                    ketQua.SoDongHopLe++;
                }
                else
                {
                    ketQua.SoDongLoi++;
                }

                ketQua.DanhSachDongDuLieu.Add(dongPreview);
            }
        }

        return ketQua;
    }

    /// <summary>
    /// Thực hiện ghi các dòng hợp lệ vào cơ sở dữ liệu trong một Transaction an toàn (All-or-nothing rollback).
    /// </summary>
    public async Task<KetQuaCommitExcel> CommitDuLieuExcelAsync(
        string tenModule,
        List<DongDuLieuPreviewExcel> danhSachDongHopLe,
        string? nguoiThucHien = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(danhSachDongHopLe);
        var ketQua = new KetQuaCommitExcel();

        if (danhSachDongHopLe.Count == 0)
        {
            ketQua.ThanhCong = false;
            ketQua.ThongDiep = "Không có dòng dữ liệu hợp lệ nào để ghi vào cơ sở dữ liệu.";
            return ketQua;
        }

        // Kiểm tra quyền tại dịch vụ: nhập Khoa cần quản trị viên; module khác cần quyền ghi toàn vùng
        // (Manager bị giới hạn một khoa không nhập hàng loạt vì không kiểm soát được khoa của từng dòng).
        if (_phanQuyen is not null)
        {
            if (string.Equals(tenModule, "khoa", StringComparison.OrdinalIgnoreCase))
            {
                await _phanQuyen.DamBaoQuanTriAsync();
            }
            else
            {
                var vung = LayVungCuaModule(tenModule)
                    ?? throw new InvalidOperationException($"Module '{tenModule}' không được hỗ trợ nhập dữ liệu.");
                await _phanQuyen.DamBaoQuyenGhiAsync(vung, null);
            }
        }

        // Bắt đầu Transaction nếu cơ sở dữ liệu hỗ trợ giao dịch quan hệ
        var coHoTroGiaoDich = _coSoDuLieu.Database.IsRelational();
        using var giaoDich = coHoTroGiaoDich ? await _coSoDuLieu.Database.BeginTransactionAsync(cancellationToken) : null;
        try
        {
            switch (tenModule.ToLowerInvariant())
            {
                case "khoa":
                    await CommitKhoaAsync(danhSachDongHopLe, cancellationToken);
                    break;
                case "bomon":
                    await CommitBoMonAsync(danhSachDongHopLe, cancellationToken);
                    break;
                case "lophoc":
                    await CommitLopHocAsync(danhSachDongHopLe, cancellationToken);
                    break;
                case "hocvien":
                    await CommitHocVienAsync(danhSachDongHopLe, cancellationToken);
                    break;
                case "nhansu":
                    await CommitNhanSuAsync(danhSachDongHopLe, nguoiThucHien, cancellationToken);
                    break;
                case "monhoc":
                    await CommitMonHocAsync(danhSachDongHopLe, cancellationToken);
                    break;
                case "phanconggiangday":
                case "phancong":
                    await CommitPhanCongGiangDayAsync(danhSachDongHopLe, cancellationToken);
                    break;
                default:
                    throw new InvalidOperationException($"Module '{tenModule}' không được hỗ trợ nhập dữ liệu.");
            }

            await _coSoDuLieu.SaveChangesAsync(cancellationToken);

            // Tự động lưu/cập nhật toàn bộ các trường tùy chỉnh (Dynamic Custom Fields) trong cùng một Transaction
            await CommitTruongTuyChinhChoDongAsync(tenModule, danhSachDongHopLe, cancellationToken);
            await _coSoDuLieu.SaveChangesAsync(cancellationToken);
            if (giaoDich != null)
            {
                await giaoDich.CommitAsync(cancellationToken);
            }

            ketQua.ThanhCong = true;
            ketQua.SoBanGhiDaThem = danhSachDongHopLe.Count;
            ketQua.ThongDiep = $"Đã nhập thành công {danhSachDongHopLe.Count} bản ghi vào hệ thống!";

            await _nhatKyDichVu.GhiNhatKyAsync(
                nguoiThucHien ?? "Hệ thống",
                "Nhập Excel",
                tenModule,
                null,
                null,
                $"Số bản ghi: {danhSachDongHopLe.Count}",
                "Thành công",
                null,
                cancellationToken);
            _bus?.PhatThayDoi(ChuanHoaTenBangModule(tenModule));
        }
        catch (Exception ex)
        {
            if (giaoDich != null)
            {
                await giaoDich.RollbackAsync(cancellationToken);
            }
            ketQua.ThanhCong = false;
            ketQua.ThongDiep = $"Lỗi khi lưu dữ liệu vào cơ sở dữ liệu: {ex.Message}";
            ketQua.DanhSachLoi.Add(ex.Message);

            await _nhatKyDichVu.GhiNhatKyAsync(
                nguoiThucHien ?? "Hệ thống",
                "Nhập Excel thất bại",
                tenModule,
                null,
                null,
                ex.Message,
                "Thất bại",
                null,
                cancellationToken);
        }

        return ketQua;
    }

    /// <summary>
    /// Xuất tệp Excel báo cáo lỗi chi tiết dựa trên kết quả kiểm tra để người dùng tải về sửa chữa.
    /// </summary>
    public byte[] XuatBaoCaoLoiExcel(KetQuaKiemTraExcel ketQuaKiemTra)
    {
        ArgumentNullException.ThrowIfNull(ketQuaKiemTra);

        using var workbook = new XLWorkbook();
        var worksheet = workbook.Worksheets.Add("BaoCaoLoi");

        // Tiêu đề báo cáo
        worksheet.Cell(1, 1).Value = "BÁO CÁO KẾT QUẢ KIỂM TRA TỆP EXCEL";
        worksheet.Cell(1, 1).Style.Font.Bold = true;
        worksheet.Cell(1, 1).Style.Font.FontSize = 14;
        worksheet.Range(1, 1, 1, 8).Merge();

        worksheet.Cell(2, 1).Value = $"Tệp: {ketQuaKiemTra.TenTep} | Module: {ketQuaKiemTra.TenModule} | Tổng số dòng: {ketQuaKiemTra.TongSoDong} | Dòng hợp lệ: {ketQuaKiemTra.SoDongHopLe} | Dòng lỗi: {ketQuaKiemTra.SoDongLoi}";
        worksheet.Range(2, 1, 2, 8).Merge();

        // Header bảng lỗi
        string[] headers = { "STT", "Dòng", "Trường", "Địa chỉ ô", "Giá trị đầu vào", "Mã lỗi", "Mô tả lỗi", "Hướng xử lý" };
        for (int i = 0; i < headers.Length; i++)
        {
            var cell = worksheet.Cell(4, i + 1);
            cell.Value = headers[i];
            cell.Style.Font.Bold = true;
            cell.Style.Fill.BackgroundColor = XLColor.FromArgb(220, 53, 69); // Danger Red
            cell.Style.Font.FontColor = XLColor.White;
            cell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
        }

        int rowIdx = 5;
        int stt = 1;
        foreach (var loi in ketQuaKiemTra.DanhSachLoi)
        {
            worksheet.Cell(rowIdx, 1).Value = stt++;
            worksheet.Cell(rowIdx, 2).Value = loi.SoDong > 0 ? loi.SoDong.ToString() : "Toàn tệp";
            worksheet.Cell(rowIdx, 3).Value = loi.TenTruong;
            worksheet.Cell(rowIdx, 4).Value = loi.DiaChiO ?? "-";
            worksheet.Cell(rowIdx, 5).SetValue(loi.GiaTriDauVao ?? "-");
            worksheet.Cell(rowIdx, 6).Value = loi.MaLoi;
            worksheet.Cell(rowIdx, 7).Value = loi.MoTaLoi;
            worksheet.Cell(rowIdx, 8).Value = loi.HuongXuLy;

            worksheet.Row(rowIdx).Height = 20;
            rowIdx++;
        }

        worksheet.Columns().AdjustToContents(10.0, 60.0);

        using var memoryStream = new MemoryStream();
        workbook.SaveAs(memoryStream);
        return memoryStream.ToArray();
    }

    /// <summary>
    /// Xuất danh sách toàn bộ dữ liệu hiện có của module ra tệp Excel chuẩn, an toàn chống CSV/Formula Injection.
    /// </summary>
    public async Task<byte[]> XuatDanhSachDuLieuExcelAsync(string tenModule, CancellationToken cancellationToken = default)
    {
        var bangChuan = ChuanHoaTenBangModule(tenModule);
        var schema = DinhNghiaSchemaExcel.LaySchema(tenModule);
        var soCotChuan = schema.Count;
        var truongTuyChinhs = await LayDanhSachTruongTuyChinhAsync(tenModule, cancellationToken);
        foreach (var c in truongTuyChinhs)
        {
            if (!schema.Any(s => string.Equals(s.TenCot, c.TenHienThi, StringComparison.OrdinalIgnoreCase)))
            {
                schema.Add(ChuyenDoiSangDinhNghiaCot(c));
            }
        }

        // Người xuất không quản lý vùng tương ứng sẽ nhận tệp đã che thông tin cá nhân.
        var nguoiXuat = _phanQuyen is null ? null : await _phanQuyen.LayNguoiDungAsync();

        using var workbook = new XLWorkbook();
        var worksheet = workbook.Worksheets.Add($"DanhSach_{tenModule}");

        // Ghi dòng tiêu đề
        for (int col = 0; col < schema.Count; col++)
        {
            var cell = worksheet.Cell(1, col + 1);
            cell.Value = schema[col].TenCot;
            cell.Style.Font.Bold = true;
            cell.Style.Font.FontColor = XLColor.White;
            bool laTruongDong = truongTuyChinhs.Any(t => string.Equals(t.TenHienThi, schema[col].TenCot, StringComparison.OrdinalIgnoreCase));
            cell.Style.Fill.BackgroundColor = laTruongDong
                ? XLColor.FromArgb(13, 110, 253) // Màu xanh lam phân biệt trường tùy chỉnh Live CRUD
                : XLColor.FromArgb(25, 135, 84);  // Success Green
            cell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
        }

        // Tải sẵn dữ liệu giá trị tùy chỉnh của bảng để nối vào dòng
        var dsGiaTriCustom = await _coSoDuLieu.GiaTriTruongTuyChinhs
            .AsNoTracking()
            .Where(g => g.TenBang == bangChuan)
            .ToListAsync(cancellationToken);
        var mapGiaTriCustom = dsGiaTriCustom.ToDictionary(g => $"{g.TruongTuyChinhId}_{g.KhoaBanGhi}", g => g.GiaTri ?? "");

        void GhiCacCotTuyChinh(int row, string khoaBanGhi, int startCol)
        {
            for (int i = 0; i < truongTuyChinhs.Count; i++)
            {
                var truong = truongTuyChinhs[i];
                var key = $"{truong.TruongTuyChinhId}_{khoaBanGhi}";
                var giaTri = mapGiaTriCustom.TryGetValue(key, out var val) ? val : "";
                GhiOAnToan(worksheet.Cell(row, startCol + i), giaTri);
            }
        }

        // Ghi theo tên cột của lược đồ để không lệch cột khi lược đồ thay đổi.
        void GhiTheoTenCot(int row, string khoaBanGhi, IReadOnlyDictionary<string, string?> giaTriTheoCot)
        {
            for (int col = 0; col < soCotChuan; col++)
            {
                var giaTri = giaTriTheoCot.TryGetValue(schema[col].TenCot, out var val) ? val : null;
                GhiOAnToan(worksheet.Cell(row, col + 1), giaTri ?? "");
            }

            GhiCacCotTuyChinh(row, khoaBanGhi, soCotChuan + 1);
        }

        // Lấy dữ liệu và ghi từng dòng
        int rowIdx = 2;
        switch (tenModule.ToLowerInvariant())
        {
            case "khoa":
                var danhSachKhoa = await _coSoDuLieu.Khoas.AsNoTracking().OrderBy(k => k.MaKhoa).ToListAsync(cancellationToken);
                foreach (var k in danhSachKhoa)
                {
                    GhiOAnToan(worksheet.Cell(rowIdx, 1), k.MaKhoa);
                    GhiOAnToan(worksheet.Cell(rowIdx, 2), k.TenKhoa);
                    GhiOAnToan(worksheet.Cell(rowIdx, 3), k.MoTa ?? "");
                    GhiOAnToan(worksheet.Cell(rowIdx, 4), k.SoDienThoai ?? "");
                    GhiOAnToan(worksheet.Cell(rowIdx, 5), k.Email ?? "");
                    GhiOAnToan(worksheet.Cell(rowIdx, 6), k.DiaDiem ?? "");
                    GhiOAnToan(worksheet.Cell(rowIdx, 7), k.CoHoatDong ? "Hoạt động" : "Đã khóa");
                    GhiCacCotTuyChinh(rowIdx, k.KhoaId.ToString(), 8);
                    rowIdx++;
                }
                break;

            case "bomon":
                var danhSachBoMon = await _coSoDuLieu.BoMons.Include(b => b.Khoa).AsNoTracking().OrderBy(b => b.MaBoMon).ToListAsync(cancellationToken);
                foreach (var bm in danhSachBoMon)
                {
                    GhiOAnToan(worksheet.Cell(rowIdx, 1), bm.MaBoMon);
                    GhiOAnToan(worksheet.Cell(rowIdx, 2), bm.TenBoMon);
                    GhiOAnToan(worksheet.Cell(rowIdx, 3), bm.MoTa ?? "");
                    GhiOAnToan(worksheet.Cell(rowIdx, 4), bm.Khoa?.MaKhoa ?? "");
                    GhiOAnToan(worksheet.Cell(rowIdx, 5), bm.CoHoatDong ? "Hoạt động" : "Đã khóa");
                    GhiCacCotTuyChinh(rowIdx, bm.BoMonId.ToString(), 6);
                    rowIdx++;
                }
                break;

            case "lophoc":
                var danhSachLop = await _coSoDuLieu.LopHocs.Include(l => l.Khoa).AsNoTracking().OrderBy(l => l.MaLop).ToListAsync(cancellationToken);
                foreach (var l in danhSachLop)
                {
                    GhiTheoTenCot(rowIdx, l.LopHocId.ToString(), new Dictionary<string, string?>
                    {
                        ["Mã lớp"] = l.MaLop,
                        ["Tên lớp"] = l.TenLop,
                        ["Niên khóa"] = l.NienKhoa,
                        ["Mã khoa"] = l.Khoa?.MaKhoa,
                        ["Hệ đào tạo"] = l.HeDaoTao,
                        ["Khóa"] = l.KhoaHoc,
                        ["Chuyên ngành"] = l.ChuyenNganh,
                        ["Số lượng tối đa"] = l.SoLuongToiDa.ToString(CultureInfo.InvariantCulture),
                        ["Trạng thái"] = l.CoHoatDong ? "Hoạt động" : "Đã khóa"
                    });
                    rowIdx++;
                }
                break;

            case "hocvien":
                var danhSachHocVien = await _coSoDuLieu.HocViens.Include(h => h.LopHoc).AsNoTracking().OrderBy(h => h.MaHocVien).ToListAsync(cancellationToken);
                foreach (var hv in danhSachHocVien)
                {
                    if (!(nguoiXuat?.CoTheXemThongTinCaNhan(VungQuanLy.DaoTao, hv.LopHoc?.KhoaId) ?? true))
                    {
                        HocVienDichVu.CheThongTinCaNhan(hv);
                    }

                    GhiTheoTenCot(rowIdx, hv.HocVienId.ToString(), new Dictionary<string, string?>
                    {
                        ["Mã học viên"] = hv.MaHocVien,
                        ["Họ và tên"] = hv.HoVaTen,
                        ["Ngày sinh"] = hv.NgaySinh.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture),
                        ["Giới tính"] = hv.GioiTinh,
                        ["Email"] = hv.Email,
                        ["Số điện thoại"] = hv.SoDienThoai,
                        ["Số CCCD"] = hv.SoCCCD,
                        ["Địa chỉ"] = hv.DiaChi,
                        ["Mã lớp"] = hv.LopHoc?.MaLop,
                        ["Chức vụ"] = hv.ChucVuLop,
                        ["Trạng thái"] = hv.CoHoatDong ? "Hoạt động" : "Đã khóa"
                    });
                    rowIdx++;
                }
                break;

            case "nhansu":
                var danhSachNhanSu = await _coSoDuLieu.NhanSus
                    .Include(n => n.Khoa)
                    .Include(n => n.BoMon)
                    .Include(n => n.ChucVu)
                    .Include(n => n.TrinhDoChuyenMon)
                    .AsNoTracking()
                    .OrderBy(n => n.MaNhanSu)
                    .ToListAsync(cancellationToken);
                foreach (var ns in danhSachNhanSu)
                {
                    if (!(nguoiXuat?.CoTheXemThongTinCaNhan(VungQuanLy.ToChuc, ns.KhoaId) ?? true))
                    {
                        NhanSuDichVu.CheThongTinCaNhan(ns);
                    }

                    GhiTheoTenCot(rowIdx, ns.NhanSuId.ToString(), new Dictionary<string, string?>
                    {
                        ["Mã nhân sự"] = ns.MaNhanSu,
                        ["Họ và tên"] = ns.HoVaTen,
                        ["Ngày sinh"] = ns.NgaySinh.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture),
                        ["Giới tính"] = ns.GioiTinh,
                        ["Số CCCD"] = ns.SoCCCD,
                        ["Số điện thoại"] = ns.SoDienThoai,
                        ["Email"] = ns.Email,
                        ["Địa chỉ"] = ns.DiaChi,
                        ["Mã khoa"] = ns.Khoa?.MaKhoa,
                        ["Mã bộ môn"] = ns.BoMon?.MaBoMon,
                        ["Mã chức vụ"] = ns.ChucVu?.MaChucVu,
                        ["Mã trình độ"] = ns.TrinhDoChuyenMon?.MaTrinhDo,
                        ["Chuyên môn đào tạo"] = ns.ChuyenMonDaoTao,
                        ["Cấp bậc hàm"] = ns.CapBacHam,
                        ["Học hàm"] = ns.HocHam,
                        ["Quê quán"] = ns.QueQuan,
                        ["Trạng thái"] = ns.TrangThai,
                        ["Ghi chú"] = ns.GhiChu
                    });
                    rowIdx++;
                }
                break;

            case "monhoc":
                var danhSachMonHoc = await _coSoDuLieu.MonHocs.Include(m => m.BoMon).AsNoTracking().OrderBy(m => m.MaMonHoc).ToListAsync(cancellationToken);
                foreach (var mh in danhSachMonHoc)
                {
                    GhiOAnToan(worksheet.Cell(rowIdx, 1), mh.MaMonHoc);
                    GhiOAnToan(worksheet.Cell(rowIdx, 2), mh.TenMonHoc);
                    GhiOAnToan(worksheet.Cell(rowIdx, 3), mh.SoTinChi.ToString());
                    GhiOAnToan(worksheet.Cell(rowIdx, 4), mh.SoTietLyThuyet.ToString());
                    GhiOAnToan(worksheet.Cell(rowIdx, 5), mh.SoTietThucHanh.ToString());
                    GhiOAnToan(worksheet.Cell(rowIdx, 6), mh.BoMon?.MaBoMon ?? "");
                    GhiOAnToan(worksheet.Cell(rowIdx, 7), mh.CoHoatDong ? "Hoạt động" : "Đã khóa");
                    GhiCacCotTuyChinh(rowIdx, mh.MonHocId.ToString(), 8);
                    rowIdx++;
                }
                break;

            case "phanconggiangday":
            case "phancong":
                var danhSachPhanCong = await _coSoDuLieu.PhanCongGiangDays
                    .Include(p => p.NhanSu)
                    .Include(p => p.MonHoc)
                    .Include(p => p.LopHoc)
                    .AsNoTracking()
                    .OrderBy(p => p.MaPhanCong)
                    .ToListAsync(cancellationToken);
                foreach (var pc in danhSachPhanCong)
                {
                    GhiOAnToan(worksheet.Cell(rowIdx, 1), pc.MaPhanCong);
                    GhiOAnToan(worksheet.Cell(rowIdx, 2), pc.NhanSu?.MaNhanSu ?? "");
                    GhiOAnToan(worksheet.Cell(rowIdx, 3), pc.MonHoc?.MaMonHoc ?? "");
                    GhiOAnToan(worksheet.Cell(rowIdx, 4), pc.LopHoc?.MaLop ?? "");
                    GhiOAnToan(worksheet.Cell(rowIdx, 5), pc.HocKy);
                    GhiOAnToan(worksheet.Cell(rowIdx, 6), pc.NamHoc);
                    GhiOAnToan(worksheet.Cell(rowIdx, 7), pc.VaiTroGiangDay);
                    GhiOAnToan(worksheet.Cell(rowIdx, 8), pc.LaChuNhiem ? "Có" : "Không");
                    GhiOAnToan(worksheet.Cell(rowIdx, 9), pc.GhiChu ?? "");
                    GhiCacCotTuyChinh(rowIdx, pc.PhanCongId.ToString(), 10);
                    rowIdx++;
                }
                break;
        }

        worksheet.Columns().AdjustToContents(10.0, 50.0);

        using var memoryStream = new MemoryStream();
        workbook.SaveAs(memoryStream);
        return memoryStream.ToArray();
    }

    /// <summary>
    /// Ghi giá trị vào ô một cách an toàn, bảo vệ chống Formula Injection (CSV / Excel Injection).
    /// </summary>
    private static void GhiOAnToan(IXLCell cell, string giaTri)
    {
        if (string.IsNullOrEmpty(giaTri))
        {
            cell.SetValue(string.Empty);
            return;
        }

        // Chống Formula Injection: nếu chuỗi bắt đầu bằng '=', '+', '-', '@' thì thêm dấu nháy đơn
        char firstChar = giaTri[0];
        if (firstChar == '=' || firstChar == '+' || firstChar == '-' || firstChar == '@')
        {
            giaTri = "'" + giaTri;
        }

        cell.SetValue(giaTri);
    }

    /// <summary>
    /// Lưu trữ kết quả kiểm tra vào bộ nhớ máy chủ để phục vụ bước xác nhận commit sau xem trước (Preview).
    /// </summary>
    public PhienPreviewExcel LuuPhienPreview(string tenModule, KetQuaKiemTraExcel ketQua, string nguoiTao)
    {
        // Dọn dẹp các phiên đã hết hạn
        DondepPhienHetHan();

        var phien = new PhienPreviewExcel
        {
            TenModule = tenModule,
            KetQuaKiemTra = ketQua,
            NguoiTao = nguoiTao
        };

        _danhSachPhienPreview[phien.MaPhien] = phien;
        return phien;
    }

    /// <summary>
    /// Lấy lại thông tin phiên xem trước từ bộ nhớ máy chủ theo mã phiên.
    /// </summary>
    public PhienPreviewExcel? LayPhienPreview(string maPhien)
    {
        if (string.IsNullOrWhiteSpace(maPhien))
        {
            return null;
        }

        if (_danhSachPhienPreview.TryGetValue(maPhien, out var phien))
        {
            if (phien.ConHieuLuc)
            {
                return phien;
            }
            _danhSachPhienPreview.TryRemove(maPhien, out _);
        }

        return null;
    }

    /// <summary>
    /// Hủy bỏ và xóa phiên xem trước khỏi bộ nhớ máy chủ.
    /// </summary>
    public void XoaPhienPreview(string maPhien)
    {
        if (!string.IsNullOrWhiteSpace(maPhien))
        {
            _danhSachPhienPreview.TryRemove(maPhien, out _);
        }
    }

    private static void DondepPhienHetHan()
    {
        var bayGio = DateTime.UtcNow;
        foreach (var kvp in _danhSachPhienPreview)
        {
            if (kvp.Value.ThoiGianHetHan < bayGio)
            {
                _danhSachPhienPreview.TryRemove(kvp.Key, out _);
            }
        }
    }

    // ==========================================
    // CÁC HÀM TIỆN ÍCH KIỂM TRA HỢP LỆ DỮ LIỆU
    // ==========================================

    private static bool KiemTraNgayThangHopLe(IXLCell cell, out DateTime ngayKetQua)
    {
        // Trường hợp ô có định dạng Date gốc của Excel
        if (cell.DataType == XLDataType.DateTime)
        {
            ngayKetQua = cell.GetDateTime();
            return true;
        }

        string chuoiNgay = cell.GetString().Trim();
        string[] cacDinhDang = { "dd/MM/yyyy", "d/M/yyyy", "yyyy-MM-dd", "dd-MM-yyyy", "MM/dd/yyyy" };
        if (DateTime.TryParseExact(chuoiNgay, cacDinhDang, CultureInfo.InvariantCulture, DateTimeStyles.None, out ngayKetQua))
        {
            return true;
        }

        return DateTime.TryParse(chuoiNgay, new CultureInfo("vi-VN"), DateTimeStyles.None, out ngayKetQua);
    }

    private static bool KiemTraEmailHopLe(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
        {
            return false;
        }
        try
        {
            var mailAddress = new MailAddress(email);
            return mailAddress.Address == email && Regex.IsMatch(email, @"^[^@\s]+@[^@\s]+\.[^@\s]+$");
        }
        catch
        {
            return false;
        }
    }

    private static bool LaCotKhoaChinh(string tenModule, string tenCot)
    {
        return tenModule.ToLowerInvariant() switch
        {
            "khoa" => string.Equals(tenCot, "Mã khoa", StringComparison.OrdinalIgnoreCase),
            "bomon" => string.Equals(tenCot, "Mã bộ môn", StringComparison.OrdinalIgnoreCase),
            "lophoc" => string.Equals(tenCot, "Mã lớp", StringComparison.OrdinalIgnoreCase),
            "hocvien" => string.Equals(tenCot, "Mã học viên", StringComparison.OrdinalIgnoreCase),
            "nhansu" => string.Equals(tenCot, "Mã nhân sự", StringComparison.OrdinalIgnoreCase),
            "monhoc" => string.Equals(tenCot, "Mã môn học", StringComparison.OrdinalIgnoreCase),
            "phanconggiangday" or "phancong" => string.Equals(tenCot, "Mã phân công", StringComparison.OrdinalIgnoreCase),
            _ => false
        };
    }

    private static bool KiemTraMaTonTaiTrongDb(
        string tenModule,
        string ma,
        List<string> dsKhoa,
        List<string> dsBoMon,
        List<string> dsLop,
        List<string> dsHocVien,
        List<string> dsNhanSu,
        List<string> dsMonHoc,
        List<string> dsPhanCong)
    {
        return tenModule.ToLowerInvariant() switch
        {
            "khoa" => dsKhoa.Contains(ma, StringComparer.OrdinalIgnoreCase),
            "bomon" => dsBoMon.Contains(ma, StringComparer.OrdinalIgnoreCase),
            "lophoc" => dsLop.Contains(ma, StringComparer.OrdinalIgnoreCase),
            "hocvien" => dsHocVien.Contains(ma, StringComparer.OrdinalIgnoreCase),
            "nhansu" => dsNhanSu.Contains(ma, StringComparer.OrdinalIgnoreCase),
            "monhoc" => dsMonHoc.Contains(ma, StringComparer.OrdinalIgnoreCase),
            "phanconggiangday" or "phancong" => dsPhanCong.Contains(ma, StringComparer.OrdinalIgnoreCase),
            _ => false
        };
    }

    private static string SuyLuanGioiTinh(string hoVaTen)
    {
        if (string.IsNullOrWhiteSpace(hoVaTen)) return "Nam";
        var words = hoVaTen.ToLowerInvariant().Split(' ', StringSplitOptions.RemoveEmptyEntries);
        if (words.Length == 0) return "Nam";

        if (words.Contains("thị")) return "Nữ";

        var lastWord = words[^1];
        var femaleLastWords = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "vân", "hân", "quỳnh", "chi", "nhi", "thư", "yến", "ly", "hoa", "huệ", "hạnh", "huyền", "phượng", "trang"
        };

        return femaleLastWords.Contains(lastWord) ? "Nữ" : "Nam";
    }

    /// <summary>Chỉ mã hệ thống và trạng thái mặc định mới được tự bù khi để trống; dữ liệu định danh và tham chiếu thì không.</summary>
    /// <summary>Ghi một dòng qua dịch vụ nghiệp vụ; lỗi quy tắc được gắn số dòng để người dùng sửa đúng chỗ trong tệp.</summary>
    private static async Task GhiTungDongAsync<T>(int soDong, Func<Task<T>> ghi)
    {
        try
        {
            await ghi();
        }
        catch (Exception ex) when (ex is InvalidOperationException or UnauthorizedAccessException)
        {
            throw new InvalidOperationException($"Dòng {soDong}: {ex.Message}", ex);
        }
    }

    /// <summary>Đưa trạng thái học viên trong tệp về danh sách chuẩn ("Hoạt động" = Đang học, "Đã khóa" = Thôi học).</summary>
    private static string ChuanHoaTrangThaiHocVien(string giaTri)
    {
        var tt = (giaTri ?? string.Empty).Trim();
        if (QuyTacNghiepVu.DanhSachTrangThaiHocVien.Contains(tt)) { return tt; }
        return tt.Equals("Đã khóa", StringComparison.OrdinalIgnoreCase) ? "Thôi học" : "Đang học";
    }

    private static bool DuocTuBuDap(string tenCot) =>
        tenCot is "Mã học viên" or "Mã nhân sự" or "Mã phân công" or "Trạng thái";

    /// <summary>Tham chiếu bắt buộc của từng phân hệ: thiếu hoặc sai là lỗi, không được đoán.</summary>
    private static bool LaThamChieuBatBuoc(string tenModule, string tenCot) => (tenModule.ToLowerInvariant(), tenCot) switch
    {
        ("bomon", "Mã khoa") => true,
        ("lophoc", "Mã khoa") => true,
        ("hocvien", "Mã lớp") => true,
        ("phanconggiangday" or "phancong", "Mã lớp" or "Mã nhân sự" or "Mã môn học") => true,
        _ => false
    };

    private static void KiemTraKhoaNgoaiTonTai(
        string tenModule,
        string tenCot,
        string giaTri,
        int row,
        string diaChiO,
        List<string> dsKhoa,
        List<string> dsBoMon,
        List<string> dsLop,
        List<string> dsNhanSu,
        List<string> dsChucVu,
        List<string> dsTrinhDo,
        List<string> dsMonHoc,
        KetQuaKiemTraExcel ketQua,
        DongDuLieuPreviewExcel dongPreview,
        bool choPhepThieuTruong = false)
    {
        bool tonTai = true;
        string doiTuong = "";

        if (string.Equals(tenCot, "Mã khoa", StringComparison.OrdinalIgnoreCase))
        {
            tonTai = dsKhoa.Contains(giaTri, StringComparer.OrdinalIgnoreCase);
            doiTuong = "Khoa";
        }
        else if (string.Equals(tenCot, "Mã bộ môn", StringComparison.OrdinalIgnoreCase))
        {
            tonTai = dsBoMon.Contains(giaTri, StringComparer.OrdinalIgnoreCase);
            doiTuong = "Bộ môn";
        }
        else if (string.Equals(tenCot, "Mã lớp", StringComparison.OrdinalIgnoreCase))
        {
            tonTai = dsLop.Contains(giaTri, StringComparer.OrdinalIgnoreCase);
            doiTuong = "Lớp học";
        }
        else if (string.Equals(tenCot, "Mã nhân sự", StringComparison.OrdinalIgnoreCase))
        {
            tonTai = dsNhanSu.Contains(giaTri, StringComparer.OrdinalIgnoreCase);
            doiTuong = "Nhân sự";
        }
        else if (string.Equals(tenCot, "Mã chức vụ", StringComparison.OrdinalIgnoreCase))
        {
            tonTai = dsChucVu.Contains(giaTri, StringComparer.OrdinalIgnoreCase);
            doiTuong = "Chức vụ";
        }
        else if (string.Equals(tenCot, "Mã trình độ", StringComparison.OrdinalIgnoreCase))
        {
            tonTai = dsTrinhDo.Contains(giaTri, StringComparer.OrdinalIgnoreCase);
            doiTuong = "Trình độ";
        }
        else if (string.Equals(tenCot, "Mã môn học", StringComparison.OrdinalIgnoreCase))
        {
            tonTai = dsMonHoc.Contains(giaTri, StringComparer.OrdinalIgnoreCase);
            doiTuong = "Môn học";
        }

        if (!tonTai)
        {
            if (choPhepThieuTruong && !LaThamChieuBatBuoc(tenModule, tenCot))
            {
                dongPreview.DuLieuCot[tenCot] = string.Empty;
                dongPreview.GhiNhanCanhBaoCot(tenCot, $"{doiTuong} '{giaTri}' chưa có trong hệ thống, trường này sẽ để trống");
                ketQua.DanhSachLoi.Add(new LoiChiTietExcel
                {
                    SoDong = row,
                    TenTruong = tenCot,
                    DiaChiO = diaChiO,
                    GiaTriDauVao = giaTri,
                    MaLoi = "WARN_FOREIGN_KEY_EMPTY",
                    MoTaLoi = $"{doiTuong} '{giaTri}' chưa tồn tại; trường tùy chọn này sẽ để trống (không tự gán sang bản ghi khác).",
                    HuongXuLy = $"Bổ sung {doiTuong} vào danh mục rồi cập nhật lại nếu cần.",
                    MucDoLoi = "Cảnh báo"
                });
            }
            else
            {
                var loi = new LoiChiTietExcel
                {
                    SoDong = row,
                    TenTruong = tenCot,
                    DiaChiO = diaChiO,
                    GiaTriDauVao = giaTri,
                    MaLoi = "ERR_FOREIGN_KEY_NOT_FOUND",
                    MoTaLoi = $"Không tìm thấy {doiTuong} có mã '{giaTri}' trong hệ thống.",
                    HuongXuLy = $"Vui lòng chọn một mã {doiTuong} đã tồn tại trong danh mục hệ thống.",
                    MucDoLoi = "Lỗi nghiêm trọng"
                };
                ketQua.DanhSachLoi.Add(loi);
                dongPreview.GhiNhanLoiCot(tenCot, loi.MoTaLoi);
            }
        }
    }

    // ==========================================
    // CÁC HÀM COMMIT CỤ THỂ TỪNG MODULE
    // ==========================================

    private static string? LayGiaTriHoacNull(Dictionary<string, string> dict, string key)
    {
        if (dict.TryGetValue(key, out var val) && !string.IsNullOrWhiteSpace(val))
        {
            return val;
        }
        return null;
    }

    private static string LayGiaTri(Dictionary<string, string> dict, string key, string macDinh = "")
    {
        if (dict.TryGetValue(key, out var val) && !string.IsNullOrWhiteSpace(val))
        {
            return val;
        }
        return macDinh;
    }

    private async Task CommitKhoaAsync(List<DongDuLieuPreviewExcel> danhSach, CancellationToken ct)
    {
        var entities = new List<Khoa>();
        foreach (var dong in danhSach)
        {
            var d = dong.DuLieuCot;
            var khoa = new Khoa
            {
                MaKhoa = LayGiaTri(d, "Mã khoa", ""),
                TenKhoa = LayGiaTri(d, "Tên khoa", ""),
                MoTa = LayGiaTriHoacNull(d, "Mô tả"),
                SoDienThoai = LayGiaTriHoacNull(d, "Số điện thoại"),
                Email = LayGiaTriHoacNull(d, "Email"),
                DiaDiem = LayGiaTriHoacNull(d, "Địa điểm"),
                CoHoatDong = !string.Equals(LayGiaTri(d, "Trạng thái", "Hoạt động"), "Đã khóa", StringComparison.OrdinalIgnoreCase),
                NgayTao = DateTime.UtcNow
            };
            entities.Add(khoa);
        }
        await _coSoDuLieu.Khoas.AddRangeAsync(entities, ct);
    }

    private async Task CommitBoMonAsync(List<DongDuLieuPreviewExcel> danhSach, CancellationToken ct)
    {
        var mapKhoa = await _coSoDuLieu.Khoas.ToDictionaryAsync(k => k.MaKhoa, k => k.KhoaId, StringComparer.OrdinalIgnoreCase, ct);
        var entities = new List<BoMon>();
        foreach (var dong in danhSach)
        {
            var d = dong.DuLieuCot;
            var maKhoa = LayGiaTri(d, "Mã khoa", "");
            if (string.IsNullOrEmpty(maKhoa) || !mapKhoa.TryGetValue(maKhoa, out int khoaId))
            {
                throw new InvalidOperationException($"Dòng {dong.SoDong}: không tìm thấy khoa '{maKhoa}'.");
            }
            var boMon = new BoMon
            {
                MaBoMon = LayGiaTri(d, "Mã bộ môn", ""),
                TenBoMon = LayGiaTri(d, "Tên bộ môn", ""),
                MoTa = LayGiaTriHoacNull(d, "Mô tả"),
                KhoaId = khoaId,
                CoHoatDong = !string.Equals(LayGiaTri(d, "Trạng thái", "Hoạt động"), "Đã khóa", StringComparison.OrdinalIgnoreCase),
                NgayTao = DateTime.UtcNow
            };
            entities.Add(boMon);
        }
        await _coSoDuLieu.BoMons.AddRangeAsync(entities, ct);
    }

    private async Task CommitLopHocAsync(List<DongDuLieuPreviewExcel> danhSach, CancellationToken ct)
    {
        var mapKhoa = await _coSoDuLieu.Khoas.ToDictionaryAsync(k => k.MaKhoa, k => k.KhoaId, StringComparer.OrdinalIgnoreCase, ct);
        var entities = new List<LopHoc>();
        var soDongCua = new Dictionary<LopHoc, int>(ReferenceEqualityComparer.Instance);
        foreach (var dong in danhSach)
        {
            var d = dong.DuLieuCot;
            var maKhoa = LayGiaTri(d, "Mã khoa", "");
            int.TryParse(LayGiaTri(d, "Số lượng tối đa", "50"), out int soLuong);
            if (string.IsNullOrEmpty(maKhoa) || !mapKhoa.TryGetValue(maKhoa, out int khoaId))
            {
                throw new InvalidOperationException($"Dòng {dong.SoDong}: không tìm thấy khoa '{maKhoa}'.");
            }
            var maLop = QuyTacNghiepVu.ChuanHoaMa(LayGiaTri(d, "Mã lớp", ""));
            var heDaoTao = LayGiaTri(d, "Hệ đào tạo", "Chính quy");
            var lop = new LopHoc
            {
                MaLop = maLop,
                TenLop = LayGiaTri(d, "Tên lớp", ""),
                NienKhoa = LayGiaTri(d, "Niên khóa", ""),
                KhoaId = khoaId,
                HeDaoTao = heDaoTao,
                KhoaHoc = LayGiaTriHoacNull(d, "Khóa") ?? (heDaoTao == "Chính quy" ? QuyTacNghiepVu.SuyRaKhoaHoc(maLop) : null),
                ChuyenNganh = LayGiaTriHoacNull(d, "Chuyên ngành"),
                SoLuongToiDa = soLuong > 0 ? soLuong : 50,
                CoHoatDong = !string.Equals(LayGiaTri(d, "Trạng thái", "Hoạt động"), "Đã khóa", StringComparison.OrdinalIgnoreCase),
                NgayTao = DateTime.UtcNow
            };
            entities.Add(lop);
            soDongCua[lop] = dong.SoDong;
        }
        if (_lopHocDichVu is null) { await _coSoDuLieu.LopHocs.AddRangeAsync(entities, ct); return; }
        foreach (var lop in entities)
        {
            await GhiTungDongAsync(soDongCua[lop], () => _lopHocDichVu.ThemLopHocAsync(lop, ct));
        }
    }

    private async Task CommitHocVienAsync(List<DongDuLieuPreviewExcel> danhSach, CancellationToken ct)
    {
        var mapLop = await _coSoDuLieu.LopHocs.ToDictionaryAsync(l => l.MaLop, l => l.LopHocId, StringComparer.OrdinalIgnoreCase, ct);
        var entities = new List<HocVien>();
        var soDongCua = new Dictionary<HocVien, int>(ReferenceEqualityComparer.Instance);
        foreach (var dong in danhSach)
        {
            var d = dong.DuLieuCot;
            var maLop = LayGiaTri(d, "Mã lớp", "");
            if (string.IsNullOrEmpty(maLop) || !mapLop.TryGetValue(maLop, out int lopHocId))
            {
                throw new InvalidOperationException($"Dòng {dong.SoDong}: không tìm thấy lớp '{maLop}'.");
            }

            if (!DateTime.TryParseExact(LayGiaTri(d, "Ngày sinh", ""), "dd/MM/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out var ngaySinh) || ngaySinh == default)
            {
                throw new InvalidOperationException($"Dòng {dong.SoDong}: ngày sinh không hợp lệ.");
            }

            var maHocVien = LayGiaTri(d, "Mã học viên", "");
            if (string.IsNullOrWhiteSpace(maHocVien))
            {
                maHocVien = $"HV_{DateTime.Now:yyyyMMdd}_{Guid.NewGuid().ToString("N")[..6].ToUpper()}";
            }

            var hoVaTen = LayGiaTri(d, "Họ và tên", "");

            var hv = new HocVien
            {
                MaHocVien = maHocVien,
                HoVaTen = hoVaTen,
                NgaySinh = ngaySinh,
                GioiTinh = LayGiaTri(d, "Giới tính", "Nam"),
                Email = LayGiaTriHoacNull(d, "Email"),
                SoDienThoai = LayGiaTriHoacNull(d, "Số điện thoại"),
                SoCCCD = LayGiaTriHoacNull(d, "Số CCCD"),
                ChucVuLop = QuyTacNghiepVu.GhepChucVuLop(QuyTacNghiepVu.TachChucVuLop(LayGiaTriHoacNull(d, "Chức vụ"))),
                DiaChi = LayGiaTriHoacNull(d, "Địa chỉ"),
                LopHocId = lopHocId,
                TrangThai = ChuanHoaTrangThaiHocVien(LayGiaTri(d, "Trạng thái", "Đang học")),
                NgayTao = DateTime.UtcNow
            };
            entities.Add(hv);
            soDongCua[hv] = dong.SoDong;
        }
        if (_hocVienDichVu is null) { await _coSoDuLieu.HocViens.AddRangeAsync(entities, ct); return; }
        foreach (var hv in entities)
        {
            await GhiTungDongAsync(soDongCua[hv], () => _hocVienDichVu.ThemHocVienAsync(hv, ct));
        }
    }

    private async Task CommitNhanSuAsync(List<DongDuLieuPreviewExcel> danhSach, string? nguoiThucHien, CancellationToken ct)
    {
        var mapKhoa = await _coSoDuLieu.Khoas.ToDictionaryAsync(k => k.MaKhoa, k => k.KhoaId, StringComparer.OrdinalIgnoreCase, ct);
        var mapBoMon = await _coSoDuLieu.BoMons.ToDictionaryAsync(b => b.MaBoMon, b => b.BoMonId, StringComparer.OrdinalIgnoreCase, ct);
        var mapChucVu = await _coSoDuLieu.ChucVus.ToDictionaryAsync(c => c.MaChucVu, c => c.ChucVuId, StringComparer.OrdinalIgnoreCase, ct);
        var mapTrinhDo = await _coSoDuLieu.TrinhDoChuyenMons.ToDictionaryAsync(t => t.MaTrinhDo, t => t.TrinhDoChuyenMonId, StringComparer.OrdinalIgnoreCase, ct);

        var entities = new List<NhanSu>();
        var soDongCua = new Dictionary<NhanSu, int>(ReferenceEqualityComparer.Instance);
        foreach (var dong in danhSach)
        {
            var d = dong.DuLieuCot;
            DateTime.TryParseExact(LayGiaTri(d, "Ngày sinh", ""), "dd/MM/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out var ngaySinh);

            var maKhoa = LayGiaTriHoacNull(d, "Mã khoa");
            var maBoMon = LayGiaTriHoacNull(d, "Mã bộ môn");
            var maChucVu = LayGiaTriHoacNull(d, "Mã chức vụ");
            var maTrinhDo = LayGiaTriHoacNull(d, "Mã trình độ");

            var ns = new NhanSu
            {
                MaNhanSu = LayGiaTri(d, "Mã nhân sự", ""),
                HoVaTen = LayGiaTri(d, "Họ và tên", ""),
                NgaySinh = ngaySinh,
                GioiTinh = LayGiaTri(d, "Giới tính", "Nam"),
                Email = LayGiaTri(d, "Email", LayGiaTri(d, "Email công việc", "")),
                SoDienThoai = LayGiaTriHoacNull(d, "Số điện thoại"),
                SoCCCD = LayGiaTriHoacNull(d, "Số CCCD"),
                DiaChi = LayGiaTriHoacNull(d, "Địa chỉ"),
                KhoaId = !string.IsNullOrEmpty(maKhoa) && mapKhoa.ContainsKey(maKhoa) ? mapKhoa[maKhoa] : null,
                BoMonId = !string.IsNullOrEmpty(maBoMon) && mapBoMon.ContainsKey(maBoMon) ? mapBoMon[maBoMon] : null,
                ChucVuId = !string.IsNullOrEmpty(maChucVu) && mapChucVu.ContainsKey(maChucVu) ? mapChucVu[maChucVu] : null,
                TrinhDoChuyenMonId = !string.IsNullOrEmpty(maTrinhDo) && mapTrinhDo.ContainsKey(maTrinhDo) ? mapTrinhDo[maTrinhDo] : null,
                ChuyenMonDaoTao = LayGiaTriHoacNull(d, "Chuyên môn đào tạo"),
                CapBacHam = LayGiaTriHoacNull(d, "Cấp bậc hàm"),
                HocHam = LayGiaTriHoacNull(d, "Học hàm"),
                QueQuan = LayGiaTriHoacNull(d, "Quê quán"),
                TrangThai = LayGiaTri(d, "Trạng thái", LayGiaTri(d, "Trạng thái công tác", "Đang làm việc")),
                GhiChu = LayGiaTriHoacNull(d, "Ghi chú"),
                NgayTao = DateTime.UtcNow,
                NguoiTao = nguoiThucHien
            };
            entities.Add(ns);
            soDongCua[ns] = dong.SoDong;
        }
        if (_nhanSuDichVu is null) { await _coSoDuLieu.NhanSus.AddRangeAsync(entities, ct); return; }
        foreach (var ns in entities)
        {
            await GhiTungDongAsync(soDongCua[ns], () => _nhanSuDichVu.ThemNhanSuAsync(ns, nguoiThucHien, ct));
        }
    }

    private async Task CommitMonHocAsync(List<DongDuLieuPreviewExcel> danhSach, CancellationToken ct)
    {
        var mapBoMon = await _coSoDuLieu.BoMons.ToDictionaryAsync(b => b.MaBoMon, b => b.BoMonId, StringComparer.OrdinalIgnoreCase, ct);
        var entities = new List<MonHoc>();
        foreach (var dong in danhSach)
        {
            var d = dong.DuLieuCot;
            int.TryParse(LayGiaTri(d, "Số tín chỉ", "3"), out int soTinChi);
            int.TryParse(LayGiaTri(d, "Số tiết lý thuyết", "30"), out int soLyThuyet);
            int.TryParse(LayGiaTri(d, "Số tiết thực hành", "15"), out int soThucHanh);

            var maBoMon = LayGiaTriHoacNull(d, "Mã bộ môn");

            var mh = new MonHoc
            {
                MaMonHoc = LayGiaTri(d, "Mã môn học", ""),
                TenMonHoc = LayGiaTri(d, "Tên môn học", ""),
                SoTinChi = soTinChi > 0 ? soTinChi : 3,
                SoTietLyThuyet = soLyThuyet >= 0 ? soLyThuyet : 30,
                SoTietThucHanh = soThucHanh >= 0 ? soThucHanh : 15,
                BoMonId = !string.IsNullOrEmpty(maBoMon) && mapBoMon.ContainsKey(maBoMon) ? mapBoMon[maBoMon] : null,
                CoHoatDong = !string.Equals(LayGiaTri(d, "Trạng thái", "Hoạt động"), "Đã khóa", StringComparison.OrdinalIgnoreCase)
            };
            entities.Add(mh);
        }
        await _coSoDuLieu.MonHocs.AddRangeAsync(entities, ct);
    }

    private async Task CommitPhanCongGiangDayAsync(List<DongDuLieuPreviewExcel> danhSach, CancellationToken ct)
    {
        var mapNhanSu = await _coSoDuLieu.NhanSus.ToDictionaryAsync(ns => ns.MaNhanSu, ns => ns.NhanSuId, StringComparer.OrdinalIgnoreCase, ct);
        var mapMonHoc = await _coSoDuLieu.MonHocs.ToDictionaryAsync(m => m.MaMonHoc, m => m.MonHocId, StringComparer.OrdinalIgnoreCase, ct);
        var mapLop = await _coSoDuLieu.LopHocs.ToDictionaryAsync(l => l.MaLop, l => l.LopHocId, StringComparer.OrdinalIgnoreCase, ct);

        var entities = new List<PhanCongGiangDay>();
        foreach (var dong in danhSach)
        {
            var d = dong.DuLieuCot;
            var maNhanSu = LayGiaTri(d, "Mã nhân sự", "");
            var maMonHoc = LayGiaTri(d, "Mã môn học", "");
            var maLop = LayGiaTri(d, "Mã lớp", "");

            var chuNhiemStr = LayGiaTri(d, "Chủ nhiệm", "Không");
            bool laChuNhiem = string.Equals(chuNhiemStr, "Có", StringComparison.OrdinalIgnoreCase) ||
                              string.Equals(chuNhiemStr, "1", StringComparison.OrdinalIgnoreCase) ||
                              string.Equals(chuNhiemStr, "True", StringComparison.OrdinalIgnoreCase);

            var pc = new PhanCongGiangDay
            {
                MaPhanCong = LayGiaTri(d, "Mã phân công", ""),
                NhanSuId = mapNhanSu[maNhanSu],
                MonHocId = mapMonHoc[maMonHoc],
                LopHocId = mapLop[maLop],
                HocKyNamHoc = LayGiaTri(d, "Học kỳ năm học", $"{LayGiaTri(d, "Học kỳ", "HK1")} ({LayGiaTri(d, "Năm học", "2026-2027")})"),
                VaiTroGiangDay = LayGiaTri(d, "Vai trò giảng dạy", "Giảng viên chính"),
                LaChuNhiem = laChuNhiem,
                GhiChu = LayGiaTriHoacNull(d, "Ghi chú"),
                NgayTao = DateTime.UtcNow
            };
            entities.Add(pc);
        }
        await _coSoDuLieu.PhanCongGiangDays.AddRangeAsync(entities, ct);
    }

    private async Task CommitTruongTuyChinhChoDongAsync(
        string tenModule,
        List<DongDuLieuPreviewExcel> danhSachDongHopLe,
        CancellationToken ct)
    {
        var bangChuan = ChuanHoaTenBangModule(tenModule);
        var truongTuyChinhs = await LayDanhSachTruongTuyChinhAsync(tenModule, ct);
        if (truongTuyChinhs.Count == 0)
        {
            return;
        }

        // Tạo bản đồ tra cứu mã nghiệp vụ -> Khóa chính trong DB
        var mapMaSangKhoaChinh = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        switch (bangChuan)
        {
            case "Khoa":
                var khoas = await _coSoDuLieu.Khoas.AsNoTracking().Select(k => new { k.MaKhoa, k.KhoaId }).ToListAsync(ct);
                foreach (var item in khoas)
                {
                    mapMaSangKhoaChinh[item.MaKhoa] = item.KhoaId.ToString();
                }
                break;

            case "BoMon":
                var bomons = await _coSoDuLieu.BoMons.AsNoTracking().Select(b => new { b.MaBoMon, b.BoMonId }).ToListAsync(ct);
                foreach (var item in bomons)
                {
                    mapMaSangKhoaChinh[item.MaBoMon] = item.BoMonId.ToString();
                }
                break;

            case "LopHoc":
                var lophocs = await _coSoDuLieu.LopHocs.AsNoTracking().Select(l => new { l.MaLop, l.LopHocId }).ToListAsync(ct);
                foreach (var item in lophocs)
                {
                    mapMaSangKhoaChinh[item.MaLop] = item.LopHocId.ToString();
                }
                break;

            case "HocVien":
                var hocviens = await _coSoDuLieu.HocViens.AsNoTracking().Select(h => new { h.MaHocVien, h.HocVienId }).ToListAsync(ct);
                foreach (var item in hocviens)
                {
                    mapMaSangKhoaChinh[item.MaHocVien] = item.HocVienId.ToString();
                }
                break;

            case "NhanSu":
                var nhansus = await _coSoDuLieu.NhanSus.AsNoTracking().Select(n => new { n.MaNhanSu, n.NhanSuId }).ToListAsync(ct);
                foreach (var item in nhansus)
                {
                    mapMaSangKhoaChinh[item.MaNhanSu] = item.NhanSuId.ToString();
                }
                break;

            case "MonHoc":
                var monhocs = await _coSoDuLieu.MonHocs.AsNoTracking().Select(m => new { m.MaMonHoc, m.MonHocId }).ToListAsync(ct);
                foreach (var item in monhocs)
                {
                    mapMaSangKhoaChinh[item.MaMonHoc] = item.MonHocId.ToString();
                }
                break;

            case "PhanCongGiangDay":
                var phancongs = await _coSoDuLieu.PhanCongGiangDays.AsNoTracking().Select(p => new { p.MaPhanCong, p.PhanCongId }).ToListAsync(ct);
                foreach (var item in phancongs)
                {
                    mapMaSangKhoaChinh[item.MaPhanCong] = item.PhanCongId.ToString();
                }
                break;
        }

        // Tải các giá trị hiện có trong GiaTriTruongTuyChinhs của bảng này
        var dsGiaTriCu = await _coSoDuLieu.GiaTriTruongTuyChinhs
            .Where(g => g.TenBang == bangChuan)
            .ToListAsync(ct);
        var mapGiaTriCu = dsGiaTriCu.ToDictionary(g => $"{g.TruongTuyChinhId}_{g.KhoaBanGhi}");

        foreach (var dong in danhSachDongHopLe)
        {
            var d = dong.DuLieuCot;
            // Tìm mã chính của dòng
            string maNghiepVu = bangChuan switch
            {
                "Khoa" => LayGiaTri(d, "Mã khoa", ""),
                "BoMon" => LayGiaTri(d, "Mã bộ môn", ""),
                "LopHoc" => LayGiaTri(d, "Mã lớp", ""),
                "HocVien" => LayGiaTri(d, "Mã học viên", ""),
                "NhanSu" => LayGiaTri(d, "Mã nhân sự", ""),
                "MonHoc" => LayGiaTri(d, "Mã môn học", ""),
                "PhanCongGiangDay" => LayGiaTri(d, "Mã phân công", ""),
                _ => ""
            };

            if (string.IsNullOrWhiteSpace(maNghiepVu) || !mapMaSangKhoaChinh.TryGetValue(maNghiepVu, out var khoaBanGhi))
            {
                continue;
            }

            foreach (var truong in truongTuyChinhs)
            {
                // Kiểm tra xem dòng có dữ liệu cho trường này không (theo TenHienThi hoặc MaTruong hoặc Custom_MaTruong)
                string? giaTri = null;
                if (d.TryGetValue(truong.TenHienThi, out var v1) && !string.IsNullOrWhiteSpace(v1))
                {
                    giaTri = v1.Trim();
                }
                else if (d.TryGetValue(truong.MaTruong, out var v2) && !string.IsNullOrWhiteSpace(v2))
                {
                    giaTri = v2.Trim();
                }
                else if (d.TryGetValue($"Custom_{truong.MaTruong}", out var v3) && !string.IsNullOrWhiteSpace(v3))
                {
                    giaTri = v3.Trim();
                }

                if (giaTri != null)
                {
                    var keyMap = $"{truong.TruongTuyChinhId}_{khoaBanGhi}";
                    if (mapGiaTriCu.TryGetValue(keyMap, out var entityGiaTri))
                    {
                        entityGiaTri.GiaTri = giaTri;
                        entityGiaTri.NgayCapNhat = DateTime.UtcNow;
                    }
                    else
                    {
                        var giaTriMoi = new GiaTriTruongTuyChinh
                        {
                            TruongTuyChinhId = truong.TruongTuyChinhId,
                            TenBang = bangChuan,
                            KhoaBanGhi = khoaBanGhi,
                            GiaTri = giaTri,
                            NgayCapNhat = DateTime.UtcNow
                        };
                        await _coSoDuLieu.GiaTriTruongTuyChinhs.AddAsync(giaTriMoi, ct);
                        mapGiaTriCu[keyMap] = giaTriMoi;
                    }
                }
            }
        }
    }
}
