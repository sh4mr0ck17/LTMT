# Phân tích – phản biện thiết kế bản hợp nhất v4

> Hệ thống Quản lý Đào tạo – Học viện An ninh nhân dân (C500)
> Phạm vi: bản hợp nhất từ hai hướng làm song song của nhóm (repo `LTMT` và `LTMT-demo`), nhánh `feat/hop-nhat-v4`.
> Cập nhật: 05/11/2026

Tài liệu này ghi lại **vì sao** chọn từng hướng, **đã đánh đổi gì**, và **những điểm còn yếu** để cả nhóm thống nhất trước khi báo cáo.

---

## 1. Bài toán thật sự là gì

Hệ thống phục vụ hai nhóm dữ liệu có vòng đời và người phụ trách khác nhau:

| | Vùng Tổ chức – Cán bộ | Vùng Đào tạo |
|---|---|---|
| Cấu trúc | Học viện → Khoa → Bộ môn → Cán bộ | Học viện → Hệ → Khóa → Lớp → Học viên |
| Thay đổi | Chậm (theo quyết định tổ chức) | Nhanh (mỗi khóa tuyển sinh, chuyển lớp, tốt nghiệp) |
| Người phụ trách | Phòng Tổ chức cán bộ | Phòng Đào tạo / quản lý học viên |
| Dữ liệu nhạy cảm | CCCD, SĐT, chức vụ, trình độ | CCCD, SĐT, kết quả học tập |

**Kết luận thiết kế:** tách thành hai *vùng quản lý* (`VungQuanLy.ToChuc`, `VungQuanLy.DaoTao`) nhưng **vẫn giữ quan hệ Khoa ↔ Lớp**. Lớp nào cũng do một khoa quản lý về chuyên môn, nên lớp vẫn có khóa ngoại tới khoa. Chỉ có *quyền ghi* và *cách hiển thị* là tách theo vùng.

> Phản biện: “Bỏ hẳn Khoa khỏi phía đào tạo cho gọn?” — Không nên. Báo cáo theo khoa (sĩ số, phân công giảng dạy) sẽ mất căn cứ, và thực tế quản lý không như vậy. Giải pháp là tách vùng **quyền**, không tách vùng **dữ liệu**.

---

## 2. Lấy gì từ mỗi bản

| Hạng mục | Bản `LTMT` (nền) | Bản `LTMT-demo` | Quyết định v4 |
|---|---|---|---|
| Kiến trúc | 3 tầng, service + interface, test đầy đủ | Gọn hơn, ít test | Giữ kiến trúc và bộ test của `LTMT` |
| Phân quyền | Vai trò phẳng | Tách vùng Khoa/Tổ và Đào tạo | Gộp: 4 vai trò × vùng × khoa phụ trách |
| Giao diện | Nhiều thẻ thống kê, header to | Dễ theo dõi, bảng chiếm phần lớn màn hình | Theo kiểu `LTMT-demo`: header một dòng, chip thống kê nhỏ |
| CRUD | Trang riêng | Hộp thoại ngay trên bảng | Hộp thoại (không mất chức năng sửa, chỉ bớt chuyển trang) |
| Thông báo | Cơ bản | Hai cột, ảnh, chọn Hệ → Khóa → Lớp | Lấy của demo, bổ sung thể thức văn bản hành chính |
| Cây tổ chức | Canvas vẽ toàn bộ | Không có | Giữ canvas nhưng chỉ vẽ phần đang quan tâm (mục 4) |

---

## 3. Mô hình phân quyền

**Ba lớp kiểm soát**, kiểm tra ở tầng service (`PhanQuyenDichVu`), không chỉ ẩn nút trên giao diện:

1. **Vai trò:** Siêu quản trị, Quản trị viên, Quản lý, Người xem.
2. **Vùng:** Quản lý chỉ được ghi trong vùng của mình (`qldt:vung`).
3. **Khoa phụ trách:** Quản lý vùng Tổ chức có thể bị giới hạn vào một khoa (`qldt:khoa`).

Tài khoản cũ được ánh xạ tự động (`AnhXaVaiTroCu`): `DataManager` → Quản lý vùng Đào tạo, `CanBo` → Quản lý vùng Tổ chức, `GiaoVien`/`User` → Người xem.

**Kiểm soát bổ sung:**
- Chống trùng hồ sơ chéo phân hệ: một CCCD/SĐT không thể vừa là học viên vừa là cán bộ khác người (`KiemTraTrungHoSo`).
- Che thông tin nhạy cảm với người không có quyền (`QuyTacNghiepVu.CheThongTin`).
- Nhật ký mọi thao tác ghi, có giá trị cũ → mới, lọc theo bảng/người/kết quả.

> Phản biện: quyền nằm trong claim của cookie, vậy đổi quyền của người đang đăng nhập thì sao? Khi đổi vai trò, vùng, khóa tài khoản hoặc đặt lại mật khẩu, hệ thống đổi `SecurityStamp`. Stamp được kiểm tra lại mỗi phút (`Program.cs`), nên phiên cũ hết hiệu lực trong vòng khoảng 1 phút. Độ trễ này là đánh đổi có chủ ý để không phải truy vấn CSDL ở mọi request.

---

## 4. Cây tổ chức: chỉ vẽ phần đang quan tâm

**Vấn đề gốc:** canvas từng được dùng để hiển thị *toàn bộ* dữ liệu. Khoảng 78 thẻ phải thu về 17% mới vừa màn hình, chữ không đọc được. Phóng to hay thu nhỏ đều không giải quyết được, vì canvas chỉ đọc tốt khoảng 20–40 thẻ cùng lúc.

**Đã làm:**

| # | Giải pháp | Kết quả đo được |
|---|---|---|
| 1 | Mặc định thu gọn; mỗi nhánh tối đa 8 thẻ, phần dư gom vào thẻ “+ N mục nữa”; “Mở cấp 2” dừng ở 60 thẻ | Mới vào trang: 7–15 thẻ |
| 2 | Cán bộ và học viên **không** vẽ thành thẻ. Thẻ cha chỉ có badge số lượng; danh sách nằm ở panel phải, có tìm kiếm và phân trang | Cây không còn phình theo số người |
| 3 | Mức chi tiết theo độ zoom: zoom xa thì ẩn chữ phụ, phóng to tên; không cho thu dưới 50%. Sơ đồ lớn hơn thì căn theo nút đang chọn thay vì thu nhỏ cả cây | Luôn đọc được tên đơn vị |
| 4 | Chế độ “Tập trung” và breadcrumb bấm được để đi lên | Một khoa ≈ 10 thẻ |
| 5 | Tách **hai sơ đồ** (Tổ chức – Cán bộ / Đào tạo), thêm cây thư mục bên trái (ẩn/hiện được), ô tìm kiếm nhảy tới nút | Hai cây độc lập, mỗi cây nhỏ |

Thẻ được thu gọn từ 256×152 xuống 240×112 px. Nhờ vậy cả hai sơ đồ mở ở 50%, và chọn một khóa thì phóng lên 75%.

> Phản biện:
> - **Vẫn tải cả cây một lần** (`LayCayAsync`, `LayCayDaoTaoAsync`). Với vài trăm đơn vị và vài nghìn học viên thì ổn. Nếu lên hàng chục nghìn bản ghi thì cần API trả con theo từng nút (tải lười).
> - **Bố cục tự tính tọa độ** (cây ngang đơn giản). Chưa cần `d3-hierarchy` hay ELK vì số thẻ hiển thị đã giới hạn ở mức nhỏ.
> - Canvas là để **xem tổng quan**. Thao tác hằng ngày (tìm, sửa, xuất Excel) vẫn nên làm trên các trang danh sách. Đừng biến canvas thành nơi nhập liệu.

---

## 5. Thông báo theo thể thức văn bản hành chính

Bố cục tham khảo các cổng thông tin cơ quan nhà nước: chuyên mục bên trái, danh sách có ô ngày ban hành, trang chi tiết có bảng thuộc tính văn bản. Các trường lấy theo thể thức văn bản hành chính:

| Trường | Ý nghĩa |
|---|---|
| Loại văn bản | Thông báo, Kế hoạch, Quyết định, Công văn, Hướng dẫn, Lịch công tác (= chuyên mục) |
| Số / ký hiệu | Ví dụ `125/KH-C500`; form gợi ý ký hiệu theo loại |
| Ngày ban hành, cơ quan ban hành, người ký | Hiện trong bảng thuộc tính |
| Trích yếu | Tóm tắt một dòng, hiện ở danh sách |
| Mức độ | Thường / Quan trọng / Khẩn |
| Hiệu lực hiển thị | Hết hạn thì tự ẩn với người xem |
| Nơi nhận | Toàn hệ thống, hoặc chọn nhanh Hệ → Khóa → Lớp |
| Tệp đính kèm | Tối đa 5 tệp × 10 MB; kiểm tra chữ ký tệp phía máy chủ; tải về luôn dạng *attachment*, bắt buộc đăng nhập |

**Tăng kiểm soát:** ghim lên đầu, theo dõi đã đọc / chưa đọc, người đăng xem được ai đã đọc, badge chưa đọc trên menu, trang chủ chỉ hiện 3 thông báo ưu tiên nhất. Có bản in sạch (ẩn menu và nút bấm).

> Phản biện:
> - **Tệp lưu dạng BLOB trong SQLite.** Đơn giản, sao lưu một file là đủ, phù hợp quy mô bài tập. Nếu dùng thật thì nên chuyển sang lưu tệp trên đĩa hoặc object storage, CSDL chỉ giữ metadata.
> - Chưa có **quy trình duyệt** (soạn → duyệt → ban hành). Nếu cần thì thêm trạng thái `ChoDuyet` cùng vai trò người duyệt; mô hình dữ liệu hiện tại mở rộng được mà không phá vỡ.
> - “Đã đọc” chỉ có nghĩa là *đã mở trang chi tiết*, không phải *đã xác nhận*. Với văn bản Khẩn có thể thêm nút “Xác nhận đã nhận”.

---

## 6. Rủi ro và việc nên làm tiếp

| Mức | Rủi ro | Đề xuất |
|---|---|---|
| Cao | SQLite ghi tuần tự; nhiều người nhập Excel cùng lúc sẽ phải chờ nhau | Đủ cho demo. Nếu triển khai thật thì đổi provider sang SQL Server/PostgreSQL (EF Core chỉ đổi cấu hình) |
| Thấp | Đổi quyền có độ trễ tối đa ~1 phút (mục 3) | Chấp nhận; giảm `ValidationInterval` nếu cần chặt hơn |
| Trung bình | Cây tổ chức tải toàn bộ (mục 4) | Thêm API lấy con theo nút khi dữ liệu lớn |
| Thấp | Tệp đính kèm nằm trong CSDL (mục 5) | Tách ra lưu trữ tệp |
| Thấp | Chưa có quy trình duyệt thông báo | Thêm trạng thái và vai trò duyệt |

**Thứ tự ưu tiên trước khi nộp:** chạy toàn bộ test → demo theo ba vai trò (Admin, Quản lý từng vùng, Người xem) → chuẩn bị câu trả lời cho các mục “Phản biện” ở trên, vì đó là những câu giảng viên dễ hỏi nhất.

---

## 7. Nguyên tắc giao diện và dữ liệu (chốt 05/11/2026)

**Hai loại trang, hai mục đích:**

| | Trang nghiệp vụ (Khoa, Lớp, Học viên, Cán bộ…) | “Dữ liệu & danh mục” |
|---|---|---|
| Ai dùng | Mọi vai trò; Quản lý sửa trong vùng của mình, Người xem chỉ xem | Quản trị viên (danh mục), Siêu quản trị (mọi bảng) |
| Cách sửa | Biểu mẫu có kiểm tra nghiệp vụ (trùng CCCD, chỉ tiêu lãnh đạo, sĩ số…) | Lưới kiểu bảng tính, ghi theo lô khi bấm Lưu |
| Mục đích | Công việc hằng ngày, tìm kiếm, thống kê | Danh mục dùng chung (Chức vụ, Trình độ) và sửa dữ liệu gốc |

> Phản biện: “Sao không dồn mọi chỉnh sửa vào một trang CRUD?” — Vì trang CRUD là công cụ quản trị: không phân vùng, ít kiểm tra nghiệp vụ. Nếu Quản lý phải sửa ở đó thì mất kiểm soát phạm vi. Vì vậy chỉ danh mục dùng chung mới chuyển hẳn vào đó; trang Chức vụ / Trình độ còn lại là trang tra cứu.

**Ghi dữ liệu an toàn trong “Dữ liệu & danh mục”:** thay đổi chỉ được đánh dấu, bấm Lưu mới ghi; xóa phải xác nhận; dòng lỗi giữ lại kèm thông báo; nhật ký kiểm toán chỉ xem (chặn cả ở tầng service). Bản cũ ghi ngay bản ghi rác khi bấm “Thêm dòng” — các khoa “Khoa Mới 805/835/914” sinh ra từ lỗi đó và đã được xóa (có ghi nhật ký).

**Màu theo đối tượng** (`wwwroot/css/mau-doi-tuong.css`, ánh xạ ở `Models/MauDoiTuong.cs`): mỗi loại đối tượng một màu cố định trên thẻ chỉ số, cây tổ chức, chip, biểu đồ; màu trạng thái (tốt / cảnh báo / vượt) là nhóm riêng. Thêm loại mới thì khai báo ở hai chỗ này, không viết màu trực tiếp trong trang.

**Số liệu trang chủ là số thật**, tính từ hồ sơ (bản cũ có hai biểu đồ dùng số mẫu viết cứng). Chart.js được đóng gói tại `wwwroot/lib/chartjs` để demo không cần mạng.

**Đi từ ngoài vào trong:** Đào tạo duyệt Niên khóa → Hệ → Lớp → Học viên; Cây tổ chức cho “xem riêng nhánh” (bấm đúp) kèm “Lên một cấp”.

**Một trường cho một thông tin, một nguồn quy tắc cho mọi đường ghi (đợt 7):**

| Vấn đề phát hiện | Cách xử lý |
|---|---|
| Model Cán bộ có 19 trường `[NotMapped]` (học vị, dân tộc, hợp đồng…) — form MVC cũ cho nhập nhưng **không lưu**, mất dữ liệu âm thầm | Gỡ bỏ; gỡ các trang MVC cũ của Cán bộ (chuyển hướng sang trang Blazor) |
| 9 tên bí danh cho cùng một cột (`EmailCongViec`=`Email`, `SoGiayTo`=`SoCCCD`, `TrangThaiCongTac`=`TrangThai`…) | Chỉ giữ tên thật. Studio từng đọc theo bí danh nên **sửa email/CCCD/trạng thái cán bộ bị bỏ qua** — đã sửa |
| Trình độ lưu ở 3 chỗ, chuyên môn ở 2 chỗ | Trình độ = danh mục `TrinhDoChuyenMon`; chuyên môn = `ChuyenMonDaoTao` |
| Giới tính, trạng thái cán bộ / học viên mỗi nơi một danh sách | Gom về `QuyTacNghiepVu` (`DanhSachGioiTinh`, `DanhSachTrangThaiCanBo`, `DanhSachTrangThaiHocVien`) và kiểm tra ở service |
| Studio ghi thẳng Cán bộ / Học viên / Lớp, bỏ qua CCCD, trùng hồ sơ, chỉ tiêu lãnh đạo, sĩ số | Studio gọi service nghiệp vụ; bộ cột Studio khớp biểu mẫu |
| Nhập Excel “linh hoạt” tự gán học viên vào lớp đầu tiên, khoa đầu tiên, bịa ngày sinh / họ tên | Chỉ tự sinh mã hệ thống và trạng thái mặc định; thiếu/sai tham chiếu hoặc định danh là lỗi. Mỗi dòng ghi qua service nghiệp vụ, lỗi kèm số dòng |
| Niên khóa nhập tự do trên từng lớp | Chuẩn hóa `YYYY-YYYY`; lớp cùng khóa phải cùng niên khóa (để trống thì tự lấy theo khóa); hệ đào tạo theo danh mục |

**Cây tổ chức:** mặc định là **dạng cột** (mỗi cấp một cột danh sách, đường đi tô sáng), dễ theo dõi hơn canvas khi cây sâu; canvas giữ ở chế độ “Sơ đồ”.

**Thông báo:** bố cục cổng tin — chuyên mục ngang, dải chủ đề (Khẩn / Quan trọng), tiêu điểm + tin mới cập nhật, khối từng chuyên mục, cột “Xem nhiều” và “Khẩn – còn hiệu lực”; trang chi tiết có sapo, dòng chia sẻ (sao chép liên kết, email, in), ký tên, “Cùng chuyên mục” và “Các tin khác”.

**Đợt 8 — tách hẳn “xem” và “sửa”:**

- Trang Khoa, Bộ môn, Cán bộ, Lớp, Học viên, Môn học chỉ còn **tra cứu, lọc, thống kê, xuất Excel**; nút “Chỉnh sửa dữ liệu” mở thẳng bảng tương ứng trong *Dữ liệu & danh mục*.
- *Dữ liệu & danh mục* là nơi chỉnh sửa duy nhất, mở cho cả Quản lý (chỉ thấy bảng thuộc vùng của mình; dòng ngoài khoa phụ trách bị khóa). Mọi bảng ghi qua dịch vụ nghiệp vụ nên quyền theo vùng / khoa được kiểm tra ở máy chủ, không chỉ ở giao diện.
- Tính năng: sắp xếp theo cột, lọc nhanh theo khóa ngoại / trạng thái, chọn nhiều dòng để gán giá trị hoặc đánh dấu xóa, biểu mẫu chi tiết (dùng lại biểu mẫu nghiệp vụ), cố định cột định danh, đánh dấu ô bắt buộc còn trống, chỉ gửi các ô thực sự thay đổi khi lưu, Nhập / Xuất Excel theo bảng.
- Phân công giảng dạy tạm ẩn khỏi menu và Studio (sẽ thiết kế thành bài toán riêng).
- **Mỗi chức năng một nơi:** tra cứu → *Cây tổ chức & tra cứu*; chỉnh sửa → *Dữ liệu & danh mục*; số liệu → *Không gian làm việc*; tài khoản, nhật ký → *Quản trị hệ thống*. Menu bỏ hai nhóm “Tổ chức – Cán bộ”, “Đào tạo” (trùng với trang chỉnh sửa); các trang danh sách vẫn mở được từ khung chi tiết của cây và thẻ chỉ số trang chủ.
- **Danh mục thông minh:** mỗi bảng có nhóm “Cần xử lý” (thiếu CCCD/SĐT, chưa gán bộ môn, chưa khai báo khóa, ngừng hoạt động…) kèm số lượng; mục “Cần chú ý” ở trang chủ dẫn thẳng tới đúng nhóm đó (người không có quyền sửa được dẫn tới trang xem).
- Font: chồng font hệ thống, Segoe UI đứng đầu (không tải webfont, chạy offline, dấu tiếng Việt chuẩn), chữ số đều cột trong bảng.

---

## 8. Trạng thái kiểm thử (05/11/2026)

- 211/211 test .NET qua; 11/11 test JS của canvas qua; `dotnet build --warnaserror` không có cảnh báo.
- Kiểm tra trên trình duyệt thật: đăng văn bản có số ký hiệu và tệp PDF, tải tệp (200, dạng attachment; khách chưa đăng nhập bị chuyển về trang đăng nhập), hai sơ đồ cây, ẩn/hiện cây thư mục. Không có lỗi console.
