namespace QuanLyDaoTao.Models.ToChuc;

/// <summary>Lọc bản ghi nhưng luôn giữ đường dẫn cấp trên để không mất ngữ cảnh.</summary>
public static class BoLocCayToChuc
{
    /// <summary>Tìm bản ghi theo từ khóa, loại, trạng thái; tên đơn vị khớp bao gồm cả phạm vi trực thuộc.</summary>
    public static (HashSet<string> HienThi, HashSet<string> Khop) Loc(
        NutToChuc goc, string tuKhoa = "", string loai = "", string trangThai = "")
    {
        ArgumentNullException.ThrowIfNull(goc);
        var hienThi = new HashSet<string>();
        var khop = new HashSet<string>();
        var coBoLoc = !string.IsNullOrWhiteSpace(tuKhoa) || loai != "" || trangThai != "";

        bool LocNhanh(NutToChuc nut, bool chaKhopTuKhoa)
        {
            var khopTuKhoa = chaKhopTuKhoa || nut.KhopTuKhoa(tuKhoa);
            var coLaBanGhi = nut.Loai != "Nhóm" && nut.Loai != "Học viện";
            var khopTrangThai = trangThai switch
            {
                "hoat-dong" => nut.CoHoatDong == true,
                "ngung" => nut.CoHoatDong == false,
                "ra-soat" => nut.CanhBao != null,
                _ => true
            };
            var coKhop = coLaBanGhi && khopTuKhoa && (loai == "" || nut.Loai == loai) && khopTrangThai;
            if (coKhop) { khop.Add(nut.KhoaNut); }
            var coConKhop = false;
            foreach (var con in nut.Con)
            {
                // Nhãn nhóm không mở rộng tìm kiếm, nhưng giữ kết quả khớp từ khoa/bộ môn phía trên.
                coConKhop = LocNhanh(con, chaKhopTuKhoa || (coLaBanGhi && khopTuKhoa)) || coConKhop;
            }
            if (!coBoLoc || coKhop || coConKhop) { hienThi.Add(nut.KhoaNut); return true; }
            return false;
        }

        LocNhanh(goc, false);
        return (hienThi, khop);
    }
}
