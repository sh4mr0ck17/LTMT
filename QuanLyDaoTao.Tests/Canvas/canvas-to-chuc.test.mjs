import test from 'node:test';
import assert from 'node:assert/strict';
import { gioiHanTiLe, tinhVuaKhung, tinhPhongTai, khoiTao, tinhMucChiTiet, tinhTiLeVuaKhungGoc, GioiHan, NguongChiTiet } from '../../QuanLyDaoTao/wwwroot/js/canvas-to-chuc.js';

test('Thu phóng giữ nguyên điểm sơ đồ dưới con trỏ', () => {
    const cu = { x: -140, y: 70, tiLe: 0.7 };
    const diem = { x: 410, y: 230 };
    const moi = tinhPhongTai(cu, 1.4, diem.x, diem.y);
    assert.ok(Math.abs((diem.x - cu.x) / cu.tiLe - (diem.x - moi.x) / moi.tiLe) < 1e-9);
    assert.ok(Math.abs((diem.y - cu.y) / cu.tiLe - (diem.y - moi.y) / moi.tiLe) < 1e-9);
});

test('Thu rồi phóng ngược lại không làm khung nhìn trôi', () => {
    const cu = { x: 30, y: -50, tiLe: 1 };
    const thu = tinhPhongTai(cu, 0.5, 400, 250);
    const phong = tinhPhongTai(thu, 1, 400, 250);
    assert.deepEqual(phong, cu);
});

test('Tỉ lệ luôn trong giới hạn và vẫn neo đúng con trỏ khi chạm giới hạn', () => {
    assert.equal(gioiHanTiLe(0), GioiHan.nhoNhat);
    assert.ok(GioiHan.nhoNhat >= 0.45 && GioiHan.nhoNhat <= 0.55);
    assert.equal(gioiHanTiLe(10), 2);
    const cu = { x: 10, y: 30, tiLe: 1 };
    const moi = tinhPhongTai(cu, 10, 200, 100);
    assert.equal(moi.tiLe, 2);
    assert.equal((200 - moi.x) / moi.tiLe, 190);
    assert.equal((100 - moi.y) / moi.tiLe, 70);
});

test('Vừa khung hiển thị trọn sơ đồ và chừa lề', () => {
    const khung = tinhVuaKhung(1100, 800, 1300, 900);
    assert.ok(khung.x >= 31.99);
    assert.ok(khung.y >= 31.99);
    assert.ok(khung.tiLe > GioiHan.nhoNhat);
    assert.ok(khung.x + 1300 * khung.tiLe <= 1068.01);
    assert.ok(khung.y + 900 * khung.tiLe <= 768.01);
});

test('Sơ đồ nhỏ không tự phóng quá 100% khi căn khung', () => {
    const khung = tinhVuaKhung(1000, 600, 400, 200);
    assert.deepEqual(khung, { tiLe: 1, x: 300, y: 200 });
});

test('Mức chi tiết đổi theo tỉ lệ thu phóng', () => {
    assert.equal(tinhMucChiTiet(GioiHan.nhoNhat), 'thap');
    assert.equal(tinhMucChiTiet(NguongChiTiet.thap - 0.01), 'thap');
    assert.equal(tinhMucChiTiet(NguongChiTiet.thap), 'vua');
    assert.equal(tinhMucChiTiet(NguongChiTiet.cao - 0.01), 'vua');
    assert.equal(tinhMucChiTiet(NguongChiTiet.cao), 'cao');
    assert.equal(tinhMucChiTiet(1.5), 'cao');
});

test('Sơ đồ quá lớn được nhận biết để không thu nhỏ dưới mức đọc được', () => {
    assert.ok(tinhTiLeVuaKhungGoc(800, 500, 12000, 9000) < GioiHan.nhoNhat);
    assert.equal(tinhVuaKhung(800, 500, 12000, 9000).tiLe, GioiHan.nhoNhat);
});

test('Canvas trống và kích thước tạm bằng không không gây NaN', () => {
    const khung = tinhVuaKhung(0, 0, 0, 0);
    assert.ok(Object.values(khung).every(Number.isFinite));
});

// DOM tối thiểu để chạy chính các trình xử lý thao tác mà không cần cài trình duyệt.
function taoMoiTruong(t) {
    class PhanTu {
        constructor() {
            this.handlers = new Map(); this.style = {}; this.dataset = {}; this.attrs = {};
            this.classes = new Set(); this.clientWidth = 800; this.clientHeight = 500;
            this.classList = { add: (...cs) => cs.forEach(c => this.classes.add(c)), remove: (...cs) => cs.forEach(c => this.classes.delete(c)) };
        }
        addEventListener(ten, ham, options = {}) {
            const cacHam = this.handlers.get(ten) || new Set(); cacHam.add(ham); this.handlers.set(ten, cacHam);
            options.signal?.addEventListener('abort', () => cacHam.delete(ham), { once: true });
        }
        async phat(ten, props = {}) {
            const e = { target: this, preventDefault() {}, ...props };
            for (const ham of this.handlers.get(ten) || []) { await ham(e); }
        }
        closest() { return null; }
        getBoundingClientRect() { return { left: 0, top: 0, right: this.clientWidth, bottom: this.clientHeight }; }
        setAttribute(ten, giaTri) { this.attrs[ten] = giaTri; }
        focus() {}
        setPointerCapture(ma) { this.capture = ma; }
        hasPointerCapture(ma) { return this.capture === ma; }
        releasePointerCapture() { this.capture = null; }
    }
    const docCu = globalThis.document; const resizeCu = globalThis.ResizeObserver;
    const doc = new PhanTu(); globalThis.document = doc;
    let observer;
    globalThis.ResizeObserver = class {
        constructor(callback) { this.callback = callback; this.disconnected = false; observer = this; }
        observe() {}
        disconnect() { this.disconnected = true; }
    };
    t.after(() => { globalThis.document = docCu; globalThis.ResizeObserver = resizeCu; });
    const khung = new PhanTu(); const panel = new PhanTu(); const workspace = new PhanTu(); const san = new PhanTu();
    san.offsetWidth = 1200; san.offsetHeight = 800;
    const nut = new PhanTu(); Object.assign(nut, { offsetLeft: 700, offsetTop: 400, offsetWidth: 240, offsetHeight: 112 });
    nut.dataset.canvasNode = 'khoa';
    const cacNut = new Map(['phong-to', 'thu-nho', 'vua-khung', 'nut-dang-chon', 'toan-man-hinh'].map(ten => {
        const n = new PhanTu(); n.dataset.canvasAction = ten; n.closest = () => n; return [ten, n];
    }));
    const nhan = new PhanTu();
    panel.querySelector = q => q === '[data-canvas-zoom]' ? nhan : cacNut.get(q.match(/"([^"]+)"/)[1]);
    panel.closest = () => workspace;
    khung.closest = q => q === '[data-canvas-panel]' ? panel : null;
    khung.querySelector = () => san;
    san.querySelectorAll = () => [nut];
    workspace.requestFullscreen = async () => { doc.fullscreenElement = workspace; await doc.phat('fullscreenchange'); };
    doc.exitFullscreen = async () => { doc.fullscreenElement = null; await doc.phat('fullscreenchange'); };
    const dieuKhien = khoiTao(khung);
    const docViTri = () => san.style.transform.match(/-?[\d.]+/g).map(Number);
    const bam = ten => panel.phat('click', { target: cacNut.get(ten) });
    return { khung, san, doc, workspace, cacNut, dieuKhien, docViTri, bam, observer };
}

test('Kéo nền di chuyển ngay; render lại cùng trạng thái giữ nguyên khung nhìn', async t => {
    const mt = taoMoiTruong(t);
    mt.dieuKhien.capNhat('khoa', 1, 'bo-cuc');
    const cu = mt.docViTri();
    await mt.khung.phat('pointerdown', { button: 0, pointerId: 1, clientX: 100, clientY: 100 });
    await mt.khung.phat('pointermove', { pointerId: 1, clientX: 180, clientY: 145 });
    await mt.khung.phat('pointerup', { pointerId: 1 });
    const moi = mt.docViTri();
    assert.ok(Math.abs(moi[0] - cu[0] - 80) < 1e-9);
    assert.ok(Math.abs(moi[1] - cu[1] - 45) < 1e-9);
    mt.dieuKhien.capNhat('khoa', 1, 'bo-cuc');
    assert.deepEqual(mt.docViTri(), moi);
    assert.equal(mt.khung.classes.has('tc-canvas-dang-keo'), false);
    mt.dieuKhien.huy();
});

test('Nút định vị làm thẻ đọc được; đổi kích thước giữ tâm; toàn màn hình bao gồm vùng chi tiết', async t => {
    const mt = taoMoiTruong(t);
    mt.dieuKhien.capNhat('khoa', 1, 'bo-cuc');
    await mt.bam('nut-dang-chon');
    const cu = mt.docViTri();
    assert.ok(cu[2] >= 0.75);
    assert.ok(Math.abs(cu[0] + 820 * cu[2] - 400) < 1e-9);
    mt.khung.clientWidth = 600;
    mt.observer.callback();
    assert.ok(Math.abs(mt.docViTri()[0] - cu[0] + 100) < 1e-9);
    await mt.bam('toan-man-hinh');
    assert.equal(mt.doc.fullscreenElement, mt.workspace);
    assert.equal(mt.cacNut.get('toan-man-hinh').attrs['aria-pressed'], 'true');
    await mt.bam('toan-man-hinh');
    assert.equal(mt.doc.fullscreenElement, null);
    mt.dieuKhien.huy();
});

test('Phím tắt và cuộn hoạt động, rời trang gỡ toàn bộ sự kiện và theo dõi kích thước', async t => {
    const mt = taoMoiTruong(t);
    mt.dieuKhien.capNhat('khoa', 1, 'bo-cuc');
    const cu = mt.docViTri();
    await mt.khung.phat('keydown', { key: 'ArrowRight' });
    assert.equal(mt.docViTri()[0], cu[0] - 40);
    await mt.khung.phat('wheel', { deltaY: -60, clientX: 300, clientY: 200 });
    assert.ok(mt.docViTri()[2] > cu[2]);
    mt.dieuKhien.huy();
    const sauHuy = mt.docViTri();
    await mt.khung.phat('keydown', { key: '+' });
    assert.deepEqual(mt.docViTri(), sauHuy);
    assert.equal(mt.observer.disconnected, true);
    assert.equal(mt.khung.classes.has('tc-canvas-san-sang'), false);
});
