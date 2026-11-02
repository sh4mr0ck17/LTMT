// C500 - Học viện An ninh Nhân dân: Script khởi tạo biểu đồ Chart.js với bảng màu Xanh Lá Đậm (Dark Forest Green) & Trắng

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

    const greenPalette = {
        forestDark: '#0b4627',
        forestMedium: '#15803d',
        emerald: '#059669',
        sage: '#34d399',
        slate: '#64748b',
        slateLight: '#cbd5e1'
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
                        backgroundColor: greenPalette.forestDark,
                        borderColor: greenPalette.forestDark,
                        borderWidth: 1,
                        borderRadius: 6
                    },
                    {
                        label: 'Lớp học',
                        data: dsKhoa.map(k => k.SoLop || k.soLop || 0),
                        backgroundColor: greenPalette.emerald,
                        borderColor: greenPalette.emerald,
                        borderWidth: 1,
                        borderRadius: 6
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
                        '#0b4627',
                        '#15803d',
                        '#059669',
                        '#64748b'
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
                    backgroundColor: '#15803d',
                    borderColor: '#0b4627',
                    borderWidth: 1,
                    borderRadius: 5
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
                        backgroundColor: '#059669',
                        borderColor: '#059669',
                        borderWidth: 1,
                        borderRadius: 5
                    },
                    {
                        label: 'Chỉ tiêu tối đa',
                        data: dsLop.map(l => l.SoLuongToiDa || l.soLuongToiDa || 50),
                        backgroundColor: '#cbd5e1',
                        borderColor: '#94a3b8',
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
};
