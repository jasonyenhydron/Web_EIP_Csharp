/**
 * g-components.js  （原 eip-components.js，命名統一改為 g 前綴）
 * ================================================
 * 配合 Views/Components G* Tag Helpers 使用的共用 JS
 * 引用方式：
 *   <script src="~/js/g-components.js" asp-append-version="true"></script>
 * ================================================
 */

// ── g-panel：收合/展開 ──────────────────────────
function gPanelToggle(panelId) {
    const body  = document.getElementById(panelId);
    const arrow = document.getElementById(`${panelId}-arrow`);
    if (!body) return;
    const hidden = body.classList.contains('hidden');
    body.classList.toggle('hidden', !hidden);
    if (arrow) arrow.classList.toggle('rotate-180', !hidden);
}

// ── g-dialog：開啟 / 關閉 ─────────────────────────
function gDialogOpen(id) {
    const dlg = document.getElementById(id);
    const box = document.getElementById(`${id}-content`);
    if (!dlg) return;
    dlg.style.display = 'flex';
    document.body.style.overflow = 'hidden';
    requestAnimationFrame(() => requestAnimationFrame(() => {
        box?.classList.remove('scale-95', 'opacity-0');
        box?.classList.add('scale-100', 'opacity-100');
    }));
}

function gDialogClose(id) {
    const dlg = document.getElementById(id);
    const box = document.getElementById(`${id}-content`);
    if (!dlg) return;
    box?.classList.remove('scale-100', 'opacity-100');
    box?.classList.add('scale-95', 'opacity-0');
    setTimeout(() => { dlg.style.display = 'none'; document.body.style.overflow = ''; }, 180);
}

// ── g-tree：節點收合/展開 ──────────────────────────
function gTreeToggle(nodeId) {
    const body  = document.getElementById(nodeId);
    const arrow = document.getElementById(`${nodeId}-arrow`);
    if (!body) return;
    const hidden = body.classList.contains('hidden');
    body.classList.toggle('hidden', !hidden);
    if (arrow) arrow.classList.toggle('rotate-90', hidden);
}

// ── g-layout：West / East 收合 ──────────────────────
function gLayoutToggle(panelId) {
    const panel = document.getElementById(panelId);
    const icon  = document.getElementById(`${panelId}-icon`);
    if (!panel) return;
    const collapsed = panel.style.width === '0px' || panel.style.width === '';
    if (collapsed) {
        // 展開：還原寬度（從 data-width 取原始值）
        const w = panel.dataset.origWidth || '220px';
        panel.style.width = w;
        panel.style.overflow = 'auto';
        if (icon) icon.classList.toggle('rotate-180');
    } else {
        panel.dataset.origWidth = panel.style.width;
        panel.style.width = '0px';
        panel.style.overflow = 'hidden';
        if (icon) icon.classList.toggle('rotate-180');
    }
}

// ── gToast：全域 Toast 通知 ────────────────────────
// gToast('儲存成功', 'success')   type: success|error|warning|info
function gToast(message, type = 'success') {
    const colors = { success:'bg-green-600', error:'bg-red-600', warning:'bg-amber-500', info:'bg-blue-600' };
    const icons  = {
        success: '<path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M5 13l4 4L19 7"/>',
        error  : '<path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M6 18L18 6M6 6l12 12"/>',
        warning: '<path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 9v2m0 4h.01M21 12a9 9 0 11-18 0 9 9 0 0118 0z"/>',
        info   : '<path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M13 16h-1v-4h-1m1-4h.01M21 12a9 9 0 11-18 0 9 9 0 0118 0z"/>'
    };
    document.querySelectorAll('.g-toast').forEach(e => e.remove());
    const t = document.createElement('div');
    t.className = `g-toast fixed bottom-5 right-5 z-[999] flex items-center gap-3 px-5 py-3.5 rounded-xl shadow-2xl text-white text-sm font-semibold transition-all duration-300 ${colors[type]??colors.info} opacity-0 translate-y-4`;
    t.innerHTML = `<svg class="w-5 h-5 shrink-0" fill="none" stroke="currentColor" viewBox="0 0 24 24">${icons[type]??icons.info}</svg><span>${message}</span>`;
    document.body.appendChild(t);
    requestAnimationFrame(() => requestAnimationFrame(() => { t.classList.remove('opacity-0','translate-y-4'); t.classList.add('opacity-100','translate-y-0'); }));
    setTimeout(() => { t.classList.add('opacity-0','translate-y-4'); setTimeout(() => t.remove(), 350); }, 2800);
}

// ── gConfirm：確認對話框（Promise） ─────────────────
// if (!(await gConfirm('確定刪除？'))) return;
function gConfirm(message, title = '確認操作') {
    return new Promise(resolve => {
        const id = `_gc_${Date.now()}`;
        const ov = document.createElement('div');
        ov.className = 'fixed inset-0 bg-slate-900/50 backdrop-blur-sm z-[900] flex items-center justify-center p-4';
        ov.innerHTML = `
            <div class="bg-white rounded-2xl shadow-2xl border border-slate-200 w-full max-w-sm transform scale-95 opacity-0 transition-all duration-200" id="${id}-box">
                <div class="px-5 pt-5 pb-4">
                    <div class="flex items-center gap-3 mb-3">
                        <div class="w-10 h-10 rounded-full bg-amber-100 flex items-center justify-center shrink-0">
                            <svg class="w-5 h-5 text-amber-600" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 9v2m0 4h.01M21 12a9 9 0 11-18 0 9 9 0 0118 0z"/></svg>
                        </div>
                        <h3 class="text-base font-bold text-slate-800">${title}</h3>
                    </div>
                    <p class="text-sm text-slate-600 leading-relaxed">${message}</p>
                </div>
                <div class="flex justify-end gap-2 px-5 pb-5">
                    <button id="${id}-cancel" class="px-4 py-2 text-sm font-semibold rounded-lg bg-slate-100 hover:bg-slate-200 text-slate-700 transition-colors">取消</button>
                    <button id="${id}-ok"     class="px-4 py-2 text-sm font-semibold rounded-lg bg-blue-600 hover:bg-blue-700 text-white transition-colors">確認</button>
                </div>
            </div>`;
        document.body.appendChild(ov);
        const box = document.getElementById(`${id}-box`);
        requestAnimationFrame(() => requestAnimationFrame(() => { box.classList.remove('scale-95','opacity-0'); box.classList.add('scale-100','opacity-100'); }));
        const close = r => { box.classList.add('scale-95','opacity-0'); setTimeout(() => ov.remove(), 180); resolve(r); };
        document.getElementById(`${id}-ok`).onclick     = () => close(true);
        document.getElementById(`${id}-cancel`).onclick = () => close(false);
        ov.onclick = e => { if (e.target === ov) close(false); };
    });
}

// 向下相容：舊 eip* 名稱別名（不再用舊名呼叫即可移除）
window.eipToast         = gToast;
window.eipConfirm       = gConfirm;
window.eipDialogOpen    = gDialogOpen;
window.eipDialogClose   = gDialogClose;
window.eipPanelToggle   = gPanelToggle;
