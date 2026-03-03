/**
 * g-components.js  嚗? eip-components.js嚗?絞銝?寧 g ?韌嚗? * ================================================
 * ?? Views/Components G* Tag Helpers 雿輻???JS
 * 撘?孵?嚗? *   <script src="~/js/g-components.js" asp-append-version="true"></script>
 * ================================================
 */

// ?? g-panel嚗??撅? ??????????????????????????
function gPanelToggle(panelId) {
    const body  = document.getElementById(panelId);
    const arrow = document.getElementById(`${panelId}-arrow`);
    if (!body) return;
    const hidden = body.classList.contains('hidden');
    body.classList.toggle('hidden', !hidden);
    if (arrow) arrow.classList.toggle('rotate-180', !hidden);
}

// ?? g-dialog嚗???/ ?? ?????????????????????????
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

// ?? g-tree嚗?暺??撅? ??????????????????????????
function gTreeToggle(nodeId) {
    const body  = document.getElementById(nodeId);
    const arrow = document.getElementById(`${nodeId}-arrow`);
    if (!body) return;
    const hidden = body.classList.contains('hidden');
    body.classList.toggle('hidden', !hidden);
    if (arrow) arrow.classList.toggle('rotate-90', hidden);
}

// ?? g-layout嚗est / East ?嗅? ??????????????????????
function gLayoutToggle(panelId) {
    const panel = document.getElementById(panelId);
    const icon  = document.getElementById(`${panelId}-icon`);
    if (!panel) return;
    const collapsed = panel.style.width === '0px' || panel.style.width === '';
    if (collapsed) {
        // 撅?嚗??祝摨佗?敺?data-width ??憪潘?
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

// ?? gToast嚗??Toast ? ????????????????????????
// gToast('?脣???', 'success')   type: success|error|warning|info
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

// ?? gConfirm嚗Ⅱ隤?閰望?嚗romise嚗??????????????????
// if (!(await gConfirm('蝣箏??芷嚗?))) return;
function gConfirm(message, title = '蝣箄???') {
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
                    <button id="${id}-cancel" class="px-4 py-2 text-sm font-semibold rounded-lg bg-slate-100 hover:bg-slate-200 text-slate-700 transition-colors">??</button>
                    <button id="${id}-ok"     class="px-4 py-2 text-sm font-semibold rounded-lg bg-blue-600 hover:bg-blue-700 text-white transition-colors">蝣箄?</button>
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

// ???詨捆嚗? eip* ?迂?亙?嚗?????澆?喳蝘駁嚗?window.eipToast         = gToast;
window.eipConfirm       = gConfirm;
window.eipDialogOpen    = gDialogOpen;
window.eipDialogClose   = gDialogClose;
window.eipPanelToggle   = gPanelToggle;

// Generic LOV modal (for <g-lov-input>)
// Signature:
// openGenericLov(title, api, columns, fields, map, displayFormatter, onConfirm, options)
function openGenericLov(title, api, columns, fields, map, displayFormatter, onConfirm, options) {
    const lovId = `_lov_${Date.now()}`;
    const opts = options || {};
    const pageSize = Number(opts.pageSize || 50) > 0 ? Number(opts.pageSize || 50) : 50;
    // 2 擇 1: bufferView=true 使用滾動續載；bufferView=false 使用分頁按鈕。
    const bufferView = opts.bufferView !== false;

    const state = {
        page: 1,
        hasMore: true,
        loading: false,
        query: "",
        rows: [],
        selected: null
    };

    const ov = document.createElement("div");
    ov.className = "fixed inset-0 bg-slate-900/50 backdrop-blur-sm z-[950] flex items-center justify-center p-4";
    ov.innerHTML = `
      <div class="bg-white rounded-2xl shadow-2xl border border-slate-200 w-full max-w-3xl h-[75vh] flex flex-col overflow-hidden">
        <div class="px-4 py-3 border-b border-slate-200 flex items-center justify-between bg-blue-600 text-white">
          <h3 class="text-base font-bold">${escapeHtml(title || "查詢")}</h3>
          <button type="button" id="${lovId}_close" class="p-1 rounded hover:bg-white/10">✕</button>
        </div>
        <div class="p-3 border-b border-slate-100 flex items-center gap-2">
          <input id="${lovId}_q" type="text" class="w-full px-3 py-2 border border-slate-300 rounded text-sm" placeholder="輸入關鍵字後按 Enter 或點查詢">
          <button id="${lovId}_search" class="px-4 py-2 text-sm rounded bg-slate-700 text-white hover:bg-slate-800">查詢</button>
        </div>
        <div class="flex-1 min-h-0 overflow-auto" id="${lovId}_scroll">
          <table class="w-full text-sm">
            <thead class="sticky top-0 bg-slate-50 border-b border-slate-200">
              <tr>
                ${columns.map((c) => `<th class="text-left px-3 py-2 font-semibold text-slate-600">${escapeHtml(c)}</th>`).join("")}
              </tr>
            </thead>
            <tbody id="${lovId}_tbody"></tbody>
          </table>
          <div id="${lovId}_loading" class="hidden px-3 py-2 text-xs text-slate-500">載入中...</div>
          <div id="${lovId}_empty" class="hidden px-3 py-4 text-sm text-slate-500">查無資料</div>
        </div>
        <div class="px-4 py-3 border-t border-slate-200 flex items-center justify-between gap-3">
          <div id="${lovId}_pager" class="${bufferView ? "hidden" : "flex"} items-center gap-2 text-xs text-slate-600">
            <button id="${lovId}_prev" type="button" class="px-2 py-1 rounded border border-slate-300 bg-white hover:bg-slate-50 disabled:opacity-50 disabled:cursor-not-allowed">上一頁</button>
            <span id="${lovId}_pageText">第 1 頁</span>
            <button id="${lovId}_next" type="button" class="px-2 py-1 rounded border border-slate-300 bg-white hover:bg-slate-50 disabled:opacity-50 disabled:cursor-not-allowed">下一頁</button>
          </div>
          <div class="flex items-center gap-2 ml-auto">
            <button id="${lovId}_ok" class="px-4 py-2 rounded bg-blue-600 text-white hover:bg-blue-700 text-sm">確定</button>
            <button id="${lovId}_cancel" class="px-4 py-2 rounded bg-slate-100 text-slate-700 hover:bg-slate-200 text-sm">取消</button>
          </div>
        </div>
      </div>`;
    document.body.appendChild(ov);

    const qEl = document.getElementById(`${lovId}_q`);
    const tbody = document.getElementById(`${lovId}_tbody`);
    const scrollEl = document.getElementById(`${lovId}_scroll`);
    const loadingEl = document.getElementById(`${lovId}_loading`);
    const emptyEl = document.getElementById(`${lovId}_empty`);
    const pageTextEl = document.getElementById(`${lovId}_pageText`);
    const prevEl = document.getElementById(`${lovId}_prev`);
    const nextEl = document.getElementById(`${lovId}_next`);

    function closeLov() {
        ov.remove();
    }

    function setLoading(isLoading) {
        state.loading = isLoading;
        if (loadingEl) loadingEl.classList.toggle("hidden", !isLoading);
        if (!bufferView) {
            if (prevEl) prevEl.disabled = isLoading || state.page <= 1;
            if (nextEl) nextEl.disabled = isLoading || !state.hasMore;
        }
    }

    function renderRows(reset, rows) {
        if (reset) tbody.innerHTML = "";
        const renderData = Array.isArray(rows) ? rows : state.rows;
        for (const row of renderData) {
            const tr = document.createElement("tr");
            tr.className = "border-b border-slate-100 hover:bg-blue-50 cursor-pointer";
            tr.innerHTML = fields.map((f) => `<td class="px-3 py-2">${escapeHtml(row[f] ?? "")}</td>`).join("");
            tr.addEventListener("click", () => {
                state.selected = row;
                tbody.querySelectorAll("tr").forEach((r) => r.classList.remove("bg-blue-100"));
                tr.classList.add("bg-blue-100");
            });
            tr.addEventListener("dblclick", () => {
                state.selected = row;
                tbody.querySelectorAll("tr").forEach((r) => r.classList.remove("bg-blue-100"));
                tr.classList.add("bg-blue-100");
                commitSelection();
            });
            tbody.appendChild(tr);
        }
        emptyEl.classList.toggle("hidden", tbody.children.length !== 0);
    }

    function appendQuery(url, query, page, size) {
        const sep = url.includes("?") ? "&" : "?";
        return `${url}${sep}query=${encodeURIComponent(query || "")}&page=${page}&pageSize=${size}`;
    }

    function updatePager() {
        if (bufferView || !pageTextEl) return;
        pageTextEl.textContent = `第 ${state.page} 頁`;
        if (prevEl) prevEl.disabled = state.loading || state.page <= 1;
        if (nextEl) nextEl.disabled = state.loading || !state.hasMore;
    }

    async function fetchPage(reset, targetPage) {
        if (state.loading) return;
        if (bufferView && !state.hasMore && !reset) return;
        if (reset) {
            state.page = 1;
            state.hasMore = true;
            state.rows = [];
            state.selected = null;
            renderRows(true, []);
        }

        if (!bufferView && Number.isInteger(targetPage) && targetPage > 0) {
            state.page = targetPage;
        }

        setLoading(true);
        try {
            const url = appendQuery(api, state.query, state.page, pageSize);
            const res = await fetch(url);
            const data = await res.json();
            if (!res.ok) throw new Error((data && (data.message || data.error)) || `HTTP ${res.status}`);

            const pageRows = Array.isArray(data) ? data : (Array.isArray(data.data) ? data.data : []);
            if (bufferView) {
                state.rows = reset ? pageRows : [...state.rows, ...pageRows];
                renderRows(false, pageRows);
            } else {
                state.rows = pageRows;
                renderRows(true, pageRows);
            }

            state.hasMore = typeof data?.hasMore === "boolean" ? data.hasMore : pageRows.length >= pageSize;
            if (bufferView) state.page += 1;
            updatePager();
        } catch (e) {
            if (tbody.children.length === 0) {
                tbody.innerHTML = `<tr><td class="px-3 py-3 text-red-600" colspan="${fields.length}">${escapeHtml(e.message || String(e))}</td></tr>`;
            }
            state.hasMore = false;
            updatePager();
        } finally {
            setLoading(false);
        }
    }

    function assignValue(elId, value) {
        if (!elId) return;
        const el = document.getElementById(elId);
        if (!el) return;
        el.value = value ?? "";
        el.dispatchEvent(new Event("input", { bubbles: true }));
        el.dispatchEvent(new Event("change", { bubbles: true }));
    }

    function commitSelection() {
        if (!state.selected) {
            gToast("請先選擇一筆資料", "warning");
            return;
        }

        const selected = state.selected;
        if (map && typeof map === "object") {
            Object.keys(map).forEach((key) => {
                const targetId = map[key];
                if (key === "FORMATTED_DISPLAY") {
                    const displayVal = typeof displayFormatter === "function"
                        ? displayFormatter(selected)
                        : fields.map((f) => selected[f] ?? "").join(" - ");
                    assignValue(targetId, displayVal);
                } else {
                    assignValue(targetId, selected[key]);
                }
            });
        }

        if (typeof onConfirm === "function") {
            onConfirm(selected);
        }
        closeLov();
    }

    document.getElementById(`${lovId}_close`).addEventListener("click", closeLov);
    document.getElementById(`${lovId}_cancel`).addEventListener("click", closeLov);
    document.getElementById(`${lovId}_ok`).addEventListener("click", commitSelection);
    document.getElementById(`${lovId}_search`).addEventListener("click", () => {
        state.query = (qEl.value || "").trim();
        fetchPage(true);
    });
    qEl.addEventListener("keydown", (e) => {
        if (e.key === "Enter") {
            state.query = (qEl.value || "").trim();
            fetchPage(true);
        }
    });

    if (bufferView) {
        scrollEl.addEventListener("scroll", () => {
            const nearBottom = scrollEl.scrollTop + scrollEl.clientHeight >= scrollEl.scrollHeight - 40;
            if (nearBottom) fetchPage(false);
        });
    } else {
        if (prevEl) {
            prevEl.addEventListener("click", () => {
                if (state.page <= 1 || state.loading) return;
                fetchPage(false, state.page - 1);
            });
        }
        if (nextEl) {
            nextEl.addEventListener("click", () => {
                if (!state.hasMore || state.loading) return;
                fetchPage(false, state.page + 1);
            });
        }
        updatePager();
    }

    ov.addEventListener("click", (e) => {
        if (e.target === ov) closeLov();
    });

    fetchPage(true);
}
function escapeHtml(v) {
    return String(v ?? "")
        .replaceAll("&", "&amp;")
        .replaceAll("<", "&lt;")
        .replaceAll(">", "&gt;")
        .replaceAll("\"", "&quot;")
        .replaceAll("'", "&#39;");
}

window.openGenericLov = openGenericLov;

