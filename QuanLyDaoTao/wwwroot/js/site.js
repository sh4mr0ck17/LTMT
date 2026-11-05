// C500 - Học viện An ninh Nhân dân: Script Biểu đồ Chart.js & Hệ thống Hướng Dẫn Tương Tác Qua Shadow Box (PC & Điện Thoại Workspace)

// Biểu đồ trang chủ: số liệu THẬT do server tính (không có số giả); màu lấy từ hệ màu đối tượng (mau-doi-tuong.css)
// để cùng một đối tượng có cùng màu ở thẻ thống kê, cây tổ chức và biểu đồ.
window.khoiTaoBieuDoTrangChu = function (duLieuJson) {
    if (typeof Chart === 'undefined') return;
    let d;
    try { d = JSON.parse(duLieuJson || '{}'); } catch (e) { console.error('Lỗi dữ liệu biểu đồ', e); return; }
    const css = getComputedStyle(document.documentElement);
    const mau = ten => css.getPropertyValue(ten).trim() || '#64748b';
    const nhat = (hex, a) => {
        const m = /^#?([\da-f]{2})([\da-f]{2})([\da-f]{2})$/i.exec(hex);
        return m ? `rgba(${parseInt(m[1], 16)},${parseInt(m[2], 16)},${parseInt(m[3], 16)},${a})` : hex;
    };
    Chart.defaults.font.family = getComputedStyle(document.body).fontFamily;
    Chart.defaults.font.size = 11;
    Chart.defaults.color = '#475569';
    const veMoi = (id, cauHinh) => {
        const el = document.getElementById(id);
        if (!el) return;
        Chart.getChart(el)?.destroy();
        new Chart(el, cauHinh);
    };
    const chung = { responsive: true, maintainAspectRatio: false, animation: { duration: 500 } };
    // Bảng màu phân loại cho danh mục (trình độ, chức vụ): lần lượt theo các màu đối tượng dễ phân biệt.
    const bangPhanLoai = ['--mau-khoa', '--mau-lop', '--mau-can-bo', '--mau-hoc-vien', '--mau-he', '--mau-nien-khoa', '--mau-mon-hoc', '--mau-bo-mon'].map(mau);

    // 1. Học viên & lớp theo khoa (2 trục vì đơn vị chênh lệch lớn)
    const khoa = d.theoKhoa || [];
    veMoi('chartHocVienTheoKhoa', {
        type: 'bar',
        data: {
            labels: khoa.map(k => k.ten),
            datasets: [
                { label: 'Học viên', data: khoa.map(k => k.hocVien), backgroundColor: mau('--mau-hoc-vien'), borderRadius: 4, maxBarThickness: 36, yAxisID: 'y' },
                { label: 'Lớp', data: khoa.map(k => k.lop), backgroundColor: mau('--mau-lop'), borderRadius: 4, maxBarThickness: 36, yAxisID: 'y1' }
            ]
        },
        options: { ...chung, plugins: { legend: { position: 'top', align: 'end' } },
            scales: { y: { beginAtZero: true, ticks: { precision: 0 }, title: { display: true, text: 'Học viên' } },
                      y1: { beginAtZero: true, position: 'right', grid: { drawOnChartArea: false }, ticks: { precision: 0 }, title: { display: true, text: 'Lớp' } } } }
    });

    // 2. Trình độ cán bộ (doughnut)
    const trinhDo = d.trinhDo || [];
    veMoi('chartTrinhDoChuyenMon', {
        type: 'doughnut',
        data: { labels: trinhDo.map(t => t.ten), datasets: [{ data: trinhDo.map(t => t.soLuong), backgroundColor: trinhDo.map((_, i) => bangPhanLoai[i % bangPhanLoai.length]), borderColor: '#fff', borderWidth: 2 }] },
        options: { ...chung, cutout: '58%', plugins: { legend: { position: 'right' } } }
    });

    // 3. Cán bộ theo chức vụ (ngang)
    const chucVu = d.chucVu || [];
    veMoi('chartChucVuNhanSu', {
        type: 'bar',
        data: { labels: chucVu.map(c => c.ten), datasets: [{ label: 'Cán bộ', data: chucVu.map(c => c.soLuong), backgroundColor: mau('--mau-can-bo'), borderRadius: 4, maxBarThickness: 28 }] },
        options: { ...chung, indexAxis: 'y', plugins: { legend: { display: false } }, scales: { x: { beginAtZero: true, ticks: { precision: 0 } } } }
    });

    // 4. Tỉ lệ lấp đầy từng lớp: màu theo trạng thái (đủ chỗ / gần đầy / vượt)
    const lop = d.siSoLop || [];
    const mauTrangThai = p => p > 100 ? mau('--tt-nguy') : p >= 90 ? mau('--tt-canh-bao') : mau('--tt-tot');
    veMoi('chartSiSoLopHoc', {
        type: 'bar',
        data: {
            labels: lop.map(l => l.ten),
            datasets: [
                { label: 'Sĩ số', data: lop.map(l => l.siSo), backgroundColor: lop.map(l => mauTrangThai(l.chiTieu ? l.siSo * 100 / l.chiTieu : 0)), borderRadius: 3, order: 1 },
                { label: 'Chỉ tiêu', data: lop.map(l => l.chiTieu), backgroundColor: nhat(mau('--tt-trung-tinh'), 0.18), borderColor: nhat(mau('--tt-trung-tinh'), 0.5), borderWidth: 1, borderRadius: 3, grouped: false, order: 2 }
            ]
        },
        options: { ...chung, plugins: { legend: { position: 'top', align: 'end' },
                tooltip: { callbacks: { afterBody: items => { const l = lop[items[0].dataIndex]; return l.chiTieu ? `Lấp đầy: ${Math.round(l.siSo * 100 / l.chiTieu)}%` : ''; } } } },
            scales: { x: { ticks: { autoSkip: false, maxRotation: 60, minRotation: 0 } }, y: { beginAtZero: true, ticks: { precision: 0 } } } }
    });
};

/* ==========================================================================
   HỆ THỐNG HƯỚNG DẪN TƯƠNG TÁC QUA SHADOW BOX (INTERACTIVE SPOTLIGHT TOUR)
   Hoạt động đồng bộ trên cả Máy Tính (Desktop) & Điện Thoại (Mobile Workspace)
   ========================================================================== */
(function () {
    let danhSachBuocTour = [];
    let chiSoBuocHienTai = 0;
    let dangChayTour = false;

    function laManHinhDienThoai() {
        return window.innerWidth < 992;
    }

    function xacDinhCacBuocTheoUser(roleLabel) {
        const isMobile = laManHinhDienThoai();
        const badgeEl = document.getElementById('lblWorkspaceRoleBadge') || document.getElementById('lblUserRoleBadge');
        const tenQuyen = roleLabel || (badgeEl ? badgeEl.innerText.trim() : 'Workspace');
        const steps = [];

        // Bước 1: Giới thiệu Không gian làm việc (Workspace) trên cả PC & Điện thoại
        if (document.getElementById('pnlCaNhanHoaWorkspace')) {
            steps.push({
                selector: '#pnlCaNhanHoaWorkspace',
                title: '1. Không Gian Làm Việc Cá Nhân Hóa (' + tenQuyen + ')',
                desc: 'Chào mừng bạn! Đây là Workspace đã được tối ưu riêng theo đúng quyền hạn tài khoản của bạn trên cả máy tính và điện thoại, loại bỏ các thông tin dư thừa.',
                needSidebar: false
            });
        } else if (document.getElementById('lblTieuDe')) {
            steps.push({
                selector: '#lblTieuDe',
                title: '1. Khu Vực Quản Lý Nghiệp Vụ Hiện Tại',
                desc: 'Hiển thị dữ liệu thời gian thực kèm công cụ tìm kiếm, lọc đa chiều và xuất báo cáo Excel.',
                needSidebar: false
            });
        }

        // Bước 2: Điều hướng Menu (Tự động thích ứng PC vs Điện thoại)
        if (isMobile && document.getElementById('btnMobileMenuToggle')) {
            steps.push({
                selector: '#btnMobileMenuToggle',
                title: '2. Nút Mở Menu Trên Điện Thoại',
                desc: 'Trên điện thoại, chạm vào biểu tượng 3 gạch này để mở ngăn kéo danh mục hệ thống. Các mục con sẽ mở dạng danh sách dọc vừa vặn màn hình cảm ứng.',
                needSidebar: false
            });
        } else if (!isMobile && document.getElementById('btnToggleNav')) {
            steps.push({
                selector: '#btnToggleNav',
                title: '2. Nút Thu Gọn Side Panel Về Icon (74px)',
                desc: 'Trên máy tính, bấm nút mũi tên tròn trên mép phải để thu gọn thanh menu về dạng biểu tượng mượt mà, không đè vào Logo trường. Rê chuột vào mục lớn để mở cửa sổ nổi.',
                needSidebar: true
            });
        }

        // Bước 3: Danh mục điều hướng trong Side Panel (Mở ngăn kéo tự động nếu đang ở Điện thoại)
        if (document.querySelector('.sidebar-tree')) {
            steps.push({
                selector: '.sidebar-tree',
                title: '3. Hệ Thống Danh Mục Nghiệp Vụ',
                desc: isMobile
                    ? 'Tại ngăn kéo menu điện thoại, bạn chạm vào từng nhóm (Cơ cấu đơn vị, Quản lý đào tạo, Hồ sơ cán bộ...) để xổ xuống các trang chức năng.'
                    : 'Tại thanh menu bên trái, bạn rê chuột vào từng mục lớn để mở ngay cửa sổ nổi (Flyout Menu) bên phải và chọn chức năng cần thao tác.',
                needSidebar: true
            });
        }

        // Bước 4: Nội dung & Công cụ trọng tâm trên Workspace theo từng loại tài khoản
        if (document.getElementById('pnlWorkspaceGiangVien')) {
            steps.push({
                selector: '#pnlLopQuanLyCaNhan',
                title: '4. Lớp Học Quản Lý & Chủ Nhiệm (GVCN)',
                desc: 'Dành riêng cho Giảng viên: Hệ thống tự động đối chiếu mã nhân sự để hiển thị các Lớp học bạn đang quản lý hoặc làm Giáo viên chủ nhiệm.',
                needSidebar: false
            });
            if (document.getElementById('pnlLichGiangDayCaNhan')) {
                steps.push({
                    selector: '#pnlLichGiangDayCaNhan',
                    title: '5. Môn Học Giảng Dạy & Lịch Phân Công',
                    desc: 'Theo dõi danh sách các môn học, số tín chỉ và học kỳ giảng dạy được phân công cho riêng bạn.',
                    needSidebar: false
                });
            }
        } else {
            if (document.getElementById('btnXemStudio')) {
                steps.push({
                    selector: '#btnXemStudio',
                    title: '4. Đặc Quyền Superadmin: Live CRUD Studio',
                    desc: 'Duy nhất Superadmin có nút này! Bấm vào để thêm/bớt dòng [+]/[-], tạo thêm cột tùy chỉnh động (như Chứng chỉ, Dân tộc) và lưu trực tiếp xuống SQLite.',
                    needSidebar: false
                });
            } else if (document.getElementById('btnCapTaiKhoanGv')) {
                steps.push({
                    selector: '#btnCapTaiKhoanGv',
                    title: '4. Tạo Tài Khoản Giáo Viên / Cán Bộ',
                    desc: 'Dành cho Quản trị viên: Cấp phát tài khoản mới, gán quyền Workspace và tự động khởi tạo/liên kết hồ sơ Nhân sự.',
                    needSidebar: false
                });
            } else if (document.getElementById('btnMoDialogLoc')) {
                steps.push({
                    selector: '#btnMoDialogLoc',
                    title: '4. Bộ Lọc Đa Chiều & Sắp Xếp Nâng Cao',
                    desc: 'Bấm vào đây để lọc dữ liệu theo từng trường chuyên sâu và sắp xếp danh sách tăng/giảm dần.',
                    needSidebar: false
                });
            } else if (document.getElementById('pnlBieuDoThongKe')) {
                steps.push({
                    selector: '#pnlBieuDoThongKe',
                    title: '4. Biểu Đồ Phân Tích Quy Mô Thời Gian Thực',
                    desc: 'Trực quan hóa số liệu học viên, lớp học, trình độ học vị và tỷ lệ lấp đầy sĩ số trên cả máy tính và điện thoại.',
                    needSidebar: false
                });
            }
        }

        // Bước cuối: Hồ sơ cá nhân, đổi Avatar mặt người & Đăng xuất
        if (document.getElementById('pnlTaiKhoanGocTrai')) {
            steps.push({
                selector: '#pnlTaiKhoanGocTrai',
                title: (steps.length + 1) + '. Hồ Sơ Cá Nhân, Avatar Mặt Người & Đăng Xuất',
                desc: 'Ở góc dưới bên trái menu: Bấm vào hình tròn Avatar để đổi ảnh chân dung mặt người, bấm "Hồ sơ" để cập nhật thông tin cá nhân hoặc bấm "Đăng xuất" để thoát an toàn.',
                needSidebar: true
            });
        }

        return steps;
    }

    function dongBoTrangThaiSidebarKhiChayTour(needSidebar) {
        const isMobile = laManHinhDienThoai();
        const sidebar = document.getElementById('appSidebar');
        const backdrop = document.getElementById('appSidebarBackdrop');
        if (!sidebar) return;

        // Tắt tạm thời transition để đo tọa độ getBoundingClientRect() chính xác tuyệt đối ngay lập tức
        sidebar.style.transition = 'none';
        if (isMobile) {
            if (needSidebar) {
                sidebar.classList.add('show');
                if (backdrop) backdrop.classList.add('show');
            } else {
                sidebar.classList.remove('show');
                if (backdrop) backdrop.classList.remove('show');
            }
        } else {
            if (needSidebar && sidebar.classList.contains('sidebar-collapsed')) {
                sidebar.dataset.wasCollapsedBeforeTour = 'true';
                sidebar.classList.remove('sidebar-collapsed');
            }
        }
        // Ép trình duyệt cập nhật layout ngay lập tức
        void sidebar.offsetHeight;
        sidebar.style.transition = '';
    }

    function dongTourHuongDan() {
        dangChayTour = false;
        const overlay = document.getElementById('pnlTourOverlay');
        if (overlay) {
            overlay.remove();
        }
        const sidebar = document.getElementById('appSidebar');
        const backdrop = document.getElementById('appSidebarBackdrop');
        if (sidebar) {
            if (laManHinhDienThoai()) {
                sidebar.classList.remove('show');
                if (backdrop) backdrop.classList.remove('show');
            } else if (sidebar.dataset.wasCollapsedBeforeTour === 'true') {
                sidebar.classList.add('sidebar-collapsed');
                delete sidebar.dataset.wasCollapsedBeforeTour;
            }
        }
    }

    function capNhatToaDoSpotlightVaCard() {
        if (!dangChayTour || chiSoBuocHienTai < 0 || chiSoBuocHienTai >= danhSachBuocTour.length) return;

        const step = danhSachBuocTour[chiSoBuocHienTai];
        const targetEl = document.querySelector(step.selector);
        const spotlight = document.getElementById('pnlTourSpotlight');
        const card = document.getElementById('pnlTourCard');
        if (!targetEl || !spotlight || !card) return;

        const rect = targetEl.getBoundingClientRect();
        const isMobile = laManHinhDienThoai();
        const pad = isMobile ? 4 : 6;

        const spotTop = Math.max(4, rect.top - pad);
        const spotLeft = Math.max(4, rect.left - pad);
        const spotWidth = Math.min(window.innerWidth - 8, Math.max(32, rect.width + pad * 2));
        const spotHeight = Math.min(window.innerHeight - 8, Math.max(32, rect.height + pad * 2));

        spotlight.style.top = spotTop + 'px';
        spotlight.style.left = spotLeft + 'px';
        spotlight.style.width = spotWidth + 'px';
        spotlight.style.height = spotHeight + 'px';

        const cardWidth = Math.min(345, window.innerWidth - 20);
        card.style.width = cardWidth + 'px';
        const cardHeight = card.offsetHeight || 215;

        let cardTop = spotTop + spotHeight + 12;
        let cardLeft = spotLeft;

        if (!isMobile && spotLeft < 265 && spotWidth <= 275) {
            // Mục tiêu nằm trên thanh Sidebar máy tính -> Đặt hộp thoại ngay bên phải Sidebar
            cardLeft = spotLeft + spotWidth + 16;
            cardTop = Math.max(14, Math.min(spotTop, window.innerHeight - cardHeight - 14));
        } else {
            // Trên Điện thoại hoặc vùng nội dung chính -> Đặt hộp thoại bên dưới hoặc bên trên mục tiêu, luôn căn vừa màn hình
            if (cardTop + cardHeight > window.innerHeight - 12) {
                cardTop = Math.max(12, spotTop - cardHeight - 12);
            }
            if (isMobile) {
                cardLeft = Math.max(10, Math.round((window.innerWidth - cardWidth) / 2));
                // Nếu hộp thoại vẫn đè lên vùng sáng trên màn hình điện thoại nhỏ, đẩy xuống cuối hoặc đầu màn hình
                if (cardTop < spotTop + spotHeight && cardTop + cardHeight > spotTop) {
                    cardTop = spotTop > window.innerHeight / 2
                        ? Math.max(10, spotTop - cardHeight - 10)
                        : Math.min(window.innerHeight - cardHeight - 10, spotTop + spotHeight + 10);
                }
            } else if (cardLeft + cardWidth > window.innerWidth - 14) {
                cardLeft = Math.max(14, window.innerWidth - cardWidth - 14);
            }
        }

        card.style.top = cardTop + 'px';
        card.style.left = cardLeft + 'px';
    }

    function hienThiBuocTour(index) {
        if (index < 0 || index >= danhSachBuocTour.length) {
            dongTourHuongDan();
            return;
        }

        chiSoBuocHienTai = index;
        const step = danhSachBuocTour[index];

        dongBoTrangThaiSidebarKhiChayTour(step.needSidebar);

        const targetEl = document.querySelector(step.selector);
        if (!targetEl) {
            if (index + 1 < danhSachBuocTour.length) {
                hienThiBuocTour(index + 1);
            } else {
                dongTourHuongDan();
            }
            return;
        }

        try {
            targetEl.scrollIntoView({ behavior: 'instant', block: 'nearest' });
        } catch (e) {
            targetEl.scrollIntoView();
        }

        document.getElementById('lblTourStepBadge').innerText = `Bước ${index + 1} / ${danhSachBuocTour.length}`;
        document.getElementById('lblTourStepTitle').innerText = step.title;
        document.getElementById('lblTourStepDesc').innerText = step.desc;

        const dotsContainer = document.getElementById('pnlTourDots');
        dotsContainer.innerHTML = '';
        for (let i = 0; i < danhSachBuocTour.length; i++) {
            const d = document.createElement('span');
            d.className = 'tour-dot' + (i === index ? ' active' : '');
            dotsContainer.appendChild(d);
        }

        const btnPrev = document.getElementById('btnTourPrev');
        const btnNext = document.getElementById('btnTourNext');
        btnPrev.style.display = index > 0 ? 'inline-flex' : 'none';
        btnNext.innerHTML = index === danhSachBuocTour.length - 1
            ? '<i class="bi bi-check2-circle me-1"></i>Hoàn tất'
            : 'Tiếp tục <i class="bi bi-arrow-right ms-1"></i>';

        capNhatToaDoSpotlightVaCard();
    }

    window.batDauTourHuongDan = function (roleLabel) {
        dongTourHuongDan();
        danhSachBuocTour = xacDinhCacBuocTheoUser(roleLabel);
        if (danhSachBuocTour.length === 0) return;

        dangChayTour = true;
        const overlay = document.createElement('div');
        overlay.id = 'pnlTourOverlay';
        overlay.className = 'tour-overlay-container';
        overlay.innerHTML = `
            <div id="pnlTourSpotlight" class="tour-spotlight-box"></div>
            <div id="pnlTourCard" class="tour-popover-card">
                <div class="d-flex justify-content-between align-items-center mb-2">
                    <span id="lblTourStepBadge" class="tour-step-badge">Bước 1</span>
                    <button type="button" class="btn-close small" id="btnTourCloseX" title="Bỏ qua hướng dẫn"></button>
                </div>
                <h6 id="lblTourStepTitle" class="fw-bold text-primary mb-2">Tiêu đề hướng dẫn</h6>
                <p id="lblTourStepDesc" class="text-muted small mb-3" style="line-height: 1.55;"></p>
                <div class="d-flex justify-content-between align-items-center pt-2 border-top">
                    <button type="button" class="btn btn-sm btn-link text-muted text-decoration-none p-0 small" id="btnTourSkip">
                        Bỏ qua hướng dẫn
                    </button>
                    <div id="pnlTourDots" class="tour-dots d-none d-sm-flex"></div>
                    <div class="d-flex align-items-center gap-1">
                        <button type="button" class="btn btn-sm btn-outline-secondary py-1 px-2" id="btnTourPrev">
                            <i class="bi bi-arrow-left"></i> Trước
                        </button>
                        <button type="button" class="btn btn-sm btn-primary py-1 px-3 fw-semibold" id="btnTourNext">
                            Tiếp tục <i class="bi bi-arrow-right ms-1"></i>
                        </button>
                    </div>
                </div>
            </div>
        `;
        document.body.appendChild(overlay);

        document.getElementById('btnTourCloseX').addEventListener('click', dongTourHuongDan);
        document.getElementById('btnTourSkip').addEventListener('click', dongTourHuongDan);
        document.getElementById('btnTourPrev').addEventListener('click', function () {
            hienThiBuocTour(chiSoBuocHienTai - 1);
        });
        document.getElementById('btnTourNext').addEventListener('click', function () {
            if (chiSoBuocHienTai + 1 < danhSachBuocTour.length) {
                hienThiBuocTour(chiSoBuocHienTai + 1);
            } else {
                dongTourHuongDan();
            }
        });

        hienThiBuocTour(0);
    };

    window.addEventListener('resize', function () {
        if (dangChayTour) {
            capNhatToaDoSpotlightVaCard();
        }
    });
})();

// Cuộn ngang mượt tới cuối vùng (cây dạng cột: luôn thấy cột vừa mở).
window.cuonCuoiNgang = function (el) {
    if (!el) return;
    const giam = window.matchMedia && window.matchMedia('(prefers-reduced-motion: reduce)').matches;
    el.scrollTo({ left: el.scrollWidth, behavior: giam ? 'auto' : 'smooth' });
};
