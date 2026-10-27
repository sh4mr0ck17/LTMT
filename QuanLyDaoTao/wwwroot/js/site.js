// C500 - Học viện An ninh Nhân dân: Script khởi tạo biểu đồ Chart.js cho Blazor & MVC

window.khoiTaoBieuDoDashboard = function (khoasJson, lopsJson) {
    if (typeof Chart === 'undefined') return;

    let dsKhoa = [];
    let dsLop = [];
    try {
        dsKhoa = JSON.parse(khoasJson || '[]');
        dsLop = JSON.parse(lopsJson || '[]');
    } catch (e) {
        console.error("Lỗi parse JSON biểu đồ", e);
    }

    const candColors = {
        green: '#047920',
        greenDark: '#025917',
        gold: '#FFD700',
        red: '#D91E18',
        blue: '#0d6efd',
        cyan: '#0dcaf0',
        gray: '#6c757d'
    };

    // 1. Biểu đồ Cột: Học viên & Lớp theo Khoa
    const ctxKhoa = document.getElementById('chartHocVienTheoKhoa');
    if (ctxKhoa) {
        const existing = Chart.getChart(ctxKhoa);
        if (existing) existing.destroy();

        new Chart(ctxKhoa, {
            type: 'bar',
            data: {
                labels: dsKhoa.map(k => k.TenKhoa || k.tenKhoa),
                datasets: [
                    {
                        label: 'Học viên',
                        data: dsKhoa.map(k => k.SoHocVien || k.soHocVien || 0),
                        backgroundColor: 'rgba(4, 121, 32, 0.85)',
                        borderColor: candColors.green,
                        borderWidth: 1,
                        borderRadius: 5
                    },
                    {
                        label: 'Lớp học',
                        data: dsKhoa.map(k => k.SoLop || k.soLop || 0),
                        backgroundColor: 'rgba(255, 215, 0, 0.85)',
                        borderColor: '#c59b08',
                        borderWidth: 1,
                        borderRadius: 5
                    }
                ]
            },
            options: {
                responsive: true,
                maintainAspectRatio: false,
                plugins: { legend: { position: 'top' } },
                scales: { y: { beginAtZero: true, ticks: { precision: 0 } } }
            }
        });
    }

    // 2. Biểu đồ Tròn: Cơ cấu Trình độ chuyên môn
    const ctxTrinhDo = document.getElementById('chartTrinhDoChuyenMon');
    if (ctxTrinhDo) {
        const existing = Chart.getChart(ctxTrinhDo);
        if (existing) existing.destroy();

        new Chart(ctxTrinhDo, {
            type: 'doughnut',
            data: {
                labels: ['Tiến sĩ', 'Thạc sĩ', 'Kỹ sư', 'Cử nhân'],
                datasets: [{
                    data: [12, 28, 15, 10],
                    backgroundColor: [
                        '#047920',
                        '#0b8a28',
                        '#FFD700',
                        '#0dcaf0'
                    ],
                    borderWidth: 2,
                    borderColor: '#ffffff'
                }]
            },
            options: {
                responsive: true,
                maintainAspectRatio: false,
                plugins: { legend: { position: 'right' } }
            }
        });
    }

    // 3. Biểu đồ Ngang: Phân bổ chức vụ
    const ctxChucVu = document.getElementById('chartChucVuNhanSu');
    if (ctxChucVu) {
        const existing = Chart.getChart(ctxChucVu);
        if (existing) existing.destroy();

        new Chart(ctxChucVu, {
            type: 'bar',
            data: {
                labels: ['Trưởng khoa', 'Phó trưởng khoa', 'Trưởng bộ môn', 'Giảng viên', 'Chuyên viên'],
                datasets: [{
                    axis: 'y',
                    label: 'Số lượng cán bộ',
                    data: [3, 5, 8, 32, 14],
                    backgroundColor: 'rgba(255, 193, 7, 0.85)',
                    borderColor: '#ffc107',
                    borderWidth: 1,
                    borderRadius: 4
                }]
            },
            options: {
                indexAxis: 'y',
                responsive: true,
                maintainAspectRatio: false,
                plugins: { legend: { display: false } },
                scales: { x: { beginAtZero: true, ticks: { precision: 0 } } }
            }
        });
    }

    // 4. Biểu đồ Nhóm: Sĩ số lớp học
    const ctxLop = document.getElementById('chartSiSoLopHoc');
    if (ctxLop) {
        const existing = Chart.getChart(ctxLop);
        if (existing) existing.destroy();

        new Chart(ctxLop, {
            type: 'bar',
            data: {
                labels: dsLop.map(l => l.TenLop || l.tenLop),
                datasets: [
                    {
                        label: 'Sĩ số thực tế',
                        data: dsLop.map(l => l.SoHocVien || l.soHocVien || 0),
                        backgroundColor: 'rgba(13, 202, 240, 0.85)',
                        borderColor: '#0dcaf0',
                        borderWidth: 1,
                        borderRadius: 4
                    },
                    {
                        label: 'Chỉ tiêu tối đa',
                        data: dsLop.map(l => l.SoLuongToiDa || l.soLuongToiDa || 50),
                        backgroundColor: 'rgba(108, 117, 125, 0.35)',
                        borderColor: '#6c757d',
                        borderWidth: 1,
                        borderRadius: 4
                    }
                ]
            },
            options: {
                responsive: true,
                maintainAspectRatio: false,
                plugins: { legend: { position: 'top' } },
                scales: { y: { beginAtZero: true, ticks: { precision: 0 } } }
            }
        });
    }
};
