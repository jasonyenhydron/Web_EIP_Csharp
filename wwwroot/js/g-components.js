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
    const sortEnabled = opts.sortEnabled === true;

    const state = {
        page: 1,
        hasMore: true,
        loading: false,
        query: "",
        rows: [],
        selected: null,
        sortEnabled: sortEnabled,
        sortKey: "",
        sortDir: "asc"
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
                ${columns.map((c, i) => {
                    const key = fields[i];
                    const sortable = sortEnabled && !!key;
                    return `<th data-sort-idx="${i}" class="text-left px-3 py-2 font-semibold text-slate-600 ${sortable ? "cursor-pointer select-none hover:bg-blue-50" : ""}">
                        <span class="inline-flex items-center gap-1">
                            <span>${escapeHtml(c)}</span>
                            ${sortable ? `<span class="text-[11px] text-slate-400" data-sort-indicator="${i}">↕</span>` : ``}
                        </span>
                    </th>`;
                }).join("")}
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
    const headerRow = ov.querySelector("thead tr");

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
        const source = Array.isArray(rows) ? rows : state.rows;
        const sortKey = state.sortKey;
        const sortDir = state.sortDir === "desc" ? -1 : 1;
        const renderData = !sortKey
            ? source
            : [...source].sort((a, b) => {
                const av = a?.[sortKey] ?? "";
                const bv = b?.[sortKey] ?? "";
                if (av === bv) return 0;
                return av < bv ? -sortDir : sortDir;
            });
        const startIndex = tbody.children.length;
        renderData.forEach((row, localIdx) => {
            const rowIndex = startIndex + localIdx;
            const tr = document.createElement("tr");
            const rowBaseClass = `border-b border-slate-100 hover:bg-blue-50 cursor-pointer ${(rowIndex % 2 === 0) ? "bg-white" : "bg-slate-50/70"}`;
            tr.className = rowBaseClass;
            tr.dataset.rowBaseClass = rowBaseClass;
            tr.innerHTML = fields.map((f) => `<td class="px-3 py-2">${escapeHtml(row[f] ?? "")}</td>`).join("");
            tr.addEventListener("click", () => {
                state.selected = row;
                tbody.querySelectorAll("tr").forEach((r) => {
                    r.className = r.dataset.rowBaseClass || "border-b border-slate-100 hover:bg-blue-50 cursor-pointer bg-white";
                });
                tr.classList.add("bg-blue-100");
            });
            tr.addEventListener("dblclick", () => {
                state.selected = row;
                tbody.querySelectorAll("tr").forEach((r) => {
                    r.className = r.dataset.rowBaseClass || "border-b border-slate-100 hover:bg-blue-50 cursor-pointer bg-white";
                });
                tr.classList.add("bg-blue-100");
                commitSelection();
            });
            tbody.appendChild(tr);
        });
        emptyEl.classList.toggle("hidden", tbody.children.length !== 0);
    }

    function updateHeaderSortIndicators() {
        if (!headerRow || !state.sortEnabled) return;
        headerRow.querySelectorAll("[data-sort-indicator]").forEach((el) => {
            const idx = Number(el.getAttribute("data-sort-indicator"));
            const key = fields[idx];
            const isCurrent = state.sortKey === key;
            el.textContent = isCurrent ? (state.sortDir === "asc" ? "▲" : "▼") : "↕";
            el.classList.toggle("text-blue-600", isCurrent);
            el.classList.toggle("text-slate-400", !isCurrent);
        });
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

    if (state.sortEnabled && headerRow) {
        headerRow.querySelectorAll("[data-sort-idx]").forEach((th) => {
            th.addEventListener("click", () => {
                const idx = Number(th.getAttribute("data-sort-idx"));
                const key = fields[idx];
                if (!key) return;
                if (state.sortKey === key) {
                    state.sortDir = state.sortDir === "asc" ? "desc" : "asc";
                } else {
                    state.sortKey = key;
                    state.sortDir = "asc";
                }
                updateHeaderSortIndicators();
                renderRows(true);
            });
        });
        updateHeaderSortIndicators();
    }

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

function initGFileUploaders(root) {
    const host = root || document;
    const uploaders = host.querySelectorAll('[data-g-file-uploader="1"]');
    uploaders.forEach((el) => {
        if (el.dataset.gfuInitialized === "1") return;
        if (el.dataset.gfuAuto === "false") return;
        el.dataset.gfuInitialized = "1";
        setupGFileUploader(el);
    });
}

function setupGFileUploader(el) {
    const input = el.querySelector('[data-role="pick"]') ? el.querySelector("input[type='file']") : null;
    const pickBtn = el.querySelector('[data-role="pick"]');
    const uploadBtn = el.querySelector('[data-role="upload"]');
    const clearBtn = el.querySelector('[data-role="clear"]');
    const listEl = el.querySelector('[data-role="list"]');
    const emptyEl = el.querySelector('[data-role="empty"]');
    const dropzone = el.querySelector('[data-role="dropzone"]');
    const messageEl = el.querySelector('[data-role="message"]');
    if (!input || !pickBtn || !dropzone) return;

    const mode = (el.dataset.uploadMode || "instantly");
    const fieldName = el.dataset.name || input.name || "files";
    const allowMultiple = (el.dataset.multiple || "true") === "true";
    const maxFileSize = Number(el.dataset.maxFileSize || "0");
    const minFileSize = Number(el.dataset.minFileSize || "0");
    const allowedExts = (el.dataset.allowedExtensions || "")
        .split(",")
        .map((x) => x.trim().toLowerCase())
        .filter((x) => !!x);
    const uploadUrl = el.dataset.uploadUrl || "/api/files/upload";
    const folder = el.dataset.folder || "";
    const resultInputId = el.dataset.resultInputId || "";
    const resultValueField = (el.dataset.resultValueField || "relativePath").trim();
    const columnName = (el.dataset.columnName || "").trim();
    const tableId = (el.dataset.tableId || "").trim();
    const onValueChangedName = el.dataset.onValueChanged || "";
    const onUploadedName = el.dataset.onUploaded || "";
    const onUploadErrorName = el.dataset.onUploadError || "";
    const showFileList = (el.dataset.showFileList || "true") === "true";
    const disabled = (el.getAttribute("aria-disabled") || "false") === "true";

    const state = {
        files: [],
        uploading: false,
        uploadedResults: []
    };

    if (disabled) {
        input.disabled = true;
        pickBtn.disabled = true;
        if (uploadBtn) uploadBtn.disabled = true;
        if (clearBtn) clearBtn.disabled = true;
    }

    pickBtn.addEventListener("click", () => {
        if (disabled) return;
        input.click();
    });

    input.addEventListener("change", () => {
        if (!input.files) return;
        const picked = Array.from(input.files);
        applySelectedFiles(picked);
    });

    if (uploadBtn) {
        uploadBtn.addEventListener("click", async () => {
            await uploadSelectedFiles();
        });
    }

    if (clearBtn) {
        clearBtn.addEventListener("click", () => {
            clearSelection(true);
        });
    }

    dropzone.addEventListener("dragover", (e) => {
        if (disabled) return;
        e.preventDefault();
        dropzone.classList.add("border-blue-400", "bg-blue-50");
    });
    dropzone.addEventListener("dragleave", () => {
        dropzone.classList.remove("border-blue-400", "bg-blue-50");
    });
    dropzone.addEventListener("drop", (e) => {
        if (disabled) return;
        e.preventDefault();
        dropzone.classList.remove("border-blue-400", "bg-blue-50");
        const dropped = Array.from(e.dataTransfer?.files || []);
        applySelectedFiles(dropped);
    });

    renderFiles();

    function applySelectedFiles(files) {
        clearMessage();
        const validated = validateFiles(files);
        if (!validated.ok) {
            showMessage(validated.message || "Invalid file", "error");
            callNamedFunction(onUploadErrorName, { message: validated.message || "Invalid file", element: el });
            return;
        }

        state.files = allowMultiple ? validated.files : validated.files.slice(0, 1);
        state.uploadedResults = [];
        renderFiles();
        callNamedFunction(onValueChangedName, {
            files: state.files,
            count: state.files.length,
            element: el
        });

        if (mode === "instantly" && state.files.length > 0) {
            uploadSelectedFiles();
        }
    }

    function validateFiles(files) {
        if (!files || files.length === 0) {
            return { ok: true, files: [] };
        }
        const normalized = [];
        for (const f of files) {
            if (maxFileSize > 0 && f.size > maxFileSize) {
                return { ok: false, message: `${f.name} exceeds max file size` };
            }
            if (minFileSize > 0 && f.size < minFileSize) {
                return { ok: false, message: `${f.name} is smaller than min file size` };
            }
            if (allowedExts.length > 0) {
                const dotIdx = f.name.lastIndexOf(".");
                const ext = dotIdx >= 0 ? f.name.substring(dotIdx).toLowerCase() : "";
                if (!allowedExts.includes(ext)) {
                    return { ok: false, message: `${f.name} extension is not allowed` };
                }
            }
            normalized.push(f);
        }
        return { ok: true, files: normalized };
    }

    async function uploadSelectedFiles() {
        if (state.uploading || state.files.length === 0 || mode === "useForm") return;
        clearMessage();
        state.uploading = true;
        setUploadingState(true);
        try {
            const formData = new FormData();
            state.files.forEach((f) => formData.append(fieldName, f));
            if (folder) formData.append("folder", folder);

            const extraDataRaw = el.dataset.extraData || "";
            if (extraDataRaw) {
                try {
                    const extraData = JSON.parse(extraDataRaw);
                    if (extraData && typeof extraData === "object") {
                        Object.keys(extraData).forEach((k) => {
                            formData.append(k, `${extraData[k] ?? ""}`);
                        });
                    }
                } catch {
                    // Ignore invalid JSON in data-extra-data.
                }
            }

            const res = await fetch(uploadUrl, {
                method: "POST",
                body: formData
            });
            let json = null;
            try {
                json = await res.json();
            } catch {
                json = null;
            }
            if (!res.ok) {
                const err = (json && (json.message || json.error)) || `Upload failed: HTTP ${res.status}`;
                throw new Error(err);
            }

            const filesResult = Array.isArray(json?.files)
                ? json.files
                : (Array.isArray(json?.data) ? json.data : []);
            state.uploadedResults = filesResult;
            const resultValue = buildResultValue(filesResult);
            writeResultInput(resultValue);
            setTargetColumnValue(resultValue, filesResult);
            showMessage("Upload success", "success");
            callNamedFunction(onUploadedName, {
                files: filesResult,
                value: resultValue,
                raw: json,
                element: el
            });
        } catch (err) {
            const msg = err && err.message ? err.message : "Upload failed";
            showMessage(msg, "error");
            callNamedFunction(onUploadErrorName, { message: msg, element: el });
        } finally {
            state.uploading = false;
            setUploadingState(false);
        }
    }

    function setUploadingState(uploading) {
        if (uploadBtn) uploadBtn.disabled = uploading || disabled || state.files.length === 0;
        if (clearBtn) clearBtn.disabled = uploading || disabled;
        pickBtn.disabled = uploading || disabled;
        input.disabled = uploading || disabled;
    }

    function clearSelection(clearResult) {
        input.value = "";
        state.files = [];
        if (clearResult) {
            state.uploadedResults = [];
            writeResultInput("");
            setTargetColumnValue("", []);
        }
        clearMessage();
        renderFiles();
        callNamedFunction(onValueChangedName, {
            files: state.files,
            count: 0,
            element: el
        });
    }

    function renderFiles() {
        if (!listEl || !emptyEl) return;
        listEl.innerHTML = "";
        if (!showFileList || state.files.length === 0) {
            emptyEl.classList.toggle("hidden", !showFileList ? true : false);
            if (!showFileList && state.files.length > 0) {
                emptyEl.classList.remove("hidden");
                emptyEl.textContent = `${state.files.length} file(s) selected`;
            } else {
                emptyEl.textContent = "No file selected";
            }
            return;
        }

        emptyEl.classList.add("hidden");
        state.files.forEach((f) => {
            const row = document.createElement("div");
            row.className = "flex items-center justify-between rounded border border-slate-200 bg-white px-2 py-1";
            const sizeKb = (f.size / 1024).toFixed(1);
            row.innerHTML = `<span class="truncate pr-2">${escapeHtml(f.name)}</span><span class="text-xs text-slate-400">${sizeKb} KB</span>`;
            listEl.appendChild(row);
        });
    }

    function buildResultValue(filesResult) {
        const rows = Array.isArray(filesResult) ? filesResult : [];
        const pick = (item) => {
            if (!item || typeof item !== "object") return "";
            return `${item?.[resultValueField] ?? item?.relativePath ?? item?.storedName ?? item?.fileName ?? ""}`;
        };
        if (allowMultiple) {
            return JSON.stringify(rows.map((x) => pick(x)).filter((x) => !!x));
        }
        return rows.length > 0 ? pick(rows[0]) : "";
    }

    function writeResultInput(value) {
        if (!resultInputId) return;
        const target = document.getElementById(resultInputId);
        if (!target) return;
        target.value = value ?? "";
        target.dispatchEvent(new Event("input", { bubbles: true }));
        target.dispatchEvent(new Event("change", { bubbles: true }));
    }

    function setTargetColumnValue(value, filesResult) {
        if (!columnName) return;
        const v = value ?? "";

        // 1) Priority: update same table row input/select/textarea by name/data-field
        const row = el.closest("tr");
        if (row) {
            const selector = `[name="${cssEscape(columnName)}"], [data-column="${cssEscape(columnName)}"], [data-field="${cssEscape(columnName)}"]`;
            const target = row.querySelector(selector);
            if (target) {
                target.value = v;
                target.dispatchEvent(new Event("input", { bubbles: true }));
                target.dispatchEvent(new Event("change", { bubbles: true }));
            }
        }

        // 2) Optional: update gDataGrid selected row when table-id is provided
        if (tableId) {
            const tableEl = document.getElementById(tableId);
            const grid = tableEl?.gDataGrid;
            if (grid) {
                if (grid.selectedRow && typeof grid.selectedRow === "object") {
                    grid.selectedRow[columnName] = v;
                }
                if (grid.editRowData && typeof grid.editRowData === "object") {
                    grid.editRowData[columnName] = v;
                }
                tableEl.dispatchEvent(new CustomEvent("gfu:column-updated", {
                    bubbles: true,
                    detail: { columnName, value: v, files: filesResult || [] }
                }));
            }
        }
    }

    function clearMessage() {
        if (!messageEl) return;
        messageEl.classList.add("hidden");
        messageEl.textContent = "";
        messageEl.classList.remove("text-red-600", "text-emerald-600");
    }

    function showMessage(message, type) {
        if (!messageEl) return;
        messageEl.classList.remove("hidden");
        messageEl.textContent = message || "";
        messageEl.classList.remove("text-red-600", "text-emerald-600");
        messageEl.classList.add(type === "success" ? "text-emerald-600" : "text-red-600");
    }
}

function cssEscape(v) {
    if (window.CSS && typeof window.CSS.escape === "function") {
        return window.CSS.escape(v);
    }
    return `${v}`.replace(/([ #;?%&,.+*~\':"!^$[\]()=>|\/@])/g, "\\$1");
}

function callNamedFunction(fnName, payload) {
    const name = (fnName || "").trim();
    if (!name) return;
    const path = name.split(".");
    let target = window;
    for (let i = 0; i < path.length; i++) {
        const key = path[i];
        target = target?.[key];
    }
    if (typeof target === "function") {
        target(payload);
    }
}

function initGCardViews(root) {
    const host = root || document;
    const cardViews = host.querySelectorAll('[data-g-cardview="1"]');
    cardViews.forEach((el) => {
        if (el.dataset.gcvInitialized === "1") return;
        el.dataset.gcvInitialized = "1";
        setupGCardView(el);
    });
}

function setupGCardView(el) {
    const loadingEl = el.querySelector('[data-role="loading"]');
    const emptyEl = el.querySelector('[data-role="empty"]');
    const cardsEl = el.querySelector('[data-role="cards"]');
    const searchEl = el.querySelector('[data-role="search"]');
    const sortFieldEl = el.querySelector('[data-role="sort-field"]');
    const sortDirEl = el.querySelector('[data-role="sort-dir"]');
    const prevEl = el.querySelector('[data-role="prev"]');
    const nextEl = el.querySelector('[data-role="next"]');
    const pageInfoEl = el.querySelector('[data-role="page-info"]');
    const totalCountEl = el.querySelector('[data-role="total-count"]');
    const pagerWrapEl = el.querySelector('[data-role="pager-wrap"]');

    const state = {
        allRows: [],
        filteredRows: [],
        pageSize: Math.max(1, Number(el.dataset.pageSize || "8")),
        currentPage: 1,
        sortField: (el.dataset.defaultSortField || "").trim(),
        sortDir: (el.dataset.defaultSortDir || "asc") === "desc" ? "desc" : "asc",
        search: ""
    };

    const api = (el.dataset.api || "").trim();
    const sourceVar = (el.dataset.sourceVar || "").trim();
    const keyField = el.dataset.keyField || "ID";
    const coverField = el.dataset.coverField || "";
    const coverAltField = el.dataset.coverAltField || "";
    const titleField = el.dataset.titleField || "";
    const subtitleField = el.dataset.subtitleField || "";
    const descriptionField = el.dataset.descriptionField || "";
    const metaFields = parseCsv(el.dataset.metaFields || "");
    let searchFields = parseCsv(el.dataset.searchFields || "");
    const sortFields = parseFieldLabelPairs(el.dataset.sortFields || "");
    const cardsPerRow = (el.dataset.cardsPerRow || "auto").toLowerCase();
    const cardMinWidth = Math.max(180, Number(el.dataset.cardMinWidth || "280"));
    const primaryActionText = el.dataset.primaryActionText || "";
    const primaryActionIcon = el.dataset.primaryActionIcon || "arrow-right";
    const secondaryActionText = el.dataset.secondaryActionText || "";
    const secondaryActionIcon = el.dataset.secondaryActionIcon || "message";
    const onCardClick = el.dataset.onCardClick || "";
    const onPrimaryAction = el.dataset.onPrimaryAction || "";
    const onSecondaryAction = el.dataset.onSecondaryAction || "";
    const showPager = (el.dataset.showPager || "true") === "true";

    if (cardsPerRow === "auto") {
        cardsEl.style.gridTemplateColumns = `repeat(auto-fill, minmax(${cardMinWidth}px, 1fr))`;
    } else {
        const n = Math.max(1, Number(cardsPerRow || "1"));
        cardsEl.style.gridTemplateColumns = `repeat(${n}, minmax(0, 1fr))`;
    }

    bindSortFields();
    bindEvents();
    fetchData();

    function bindSortFields() {
        if (!sortFieldEl) return;
        const rowsFromSortFields = sortFields.length > 0
            ? sortFields
            : defaultSortFields();
        sortFieldEl.innerHTML = `<option value="">預設排序</option>`;
        rowsFromSortFields.forEach((item) => {
            const opt = document.createElement("option");
            opt.value = item.field;
            opt.textContent = item.label;
            sortFieldEl.appendChild(opt);
        });
        if (state.sortField) {
            sortFieldEl.value = state.sortField;
        }
        if (sortDirEl) {
            sortDirEl.textContent = state.sortDir === "desc" ? "▼" : "▲";
        }
    }

    function bindEvents() {
        if (searchEl) {
            searchEl.addEventListener("input", () => {
                state.search = (searchEl.value || "").trim();
                state.currentPage = 1;
                applyAndRender();
            });
        }
        if (sortFieldEl) {
            sortFieldEl.addEventListener("change", () => {
                state.sortField = sortFieldEl.value || "";
                state.currentPage = 1;
                applyAndRender();
            });
        }
        if (sortDirEl) {
            sortDirEl.addEventListener("click", () => {
                state.sortDir = state.sortDir === "asc" ? "desc" : "asc";
                sortDirEl.textContent = state.sortDir === "desc" ? "▼" : "▲";
                applyAndRender();
            });
        }
        if (prevEl) {
            prevEl.addEventListener("click", () => {
                if (state.currentPage > 1) {
                    state.currentPage -= 1;
                    renderCards();
                }
            });
        }
        if (nextEl) {
            nextEl.addEventListener("click", () => {
                if (state.currentPage < totalPages()) {
                    state.currentPage += 1;
                    renderCards();
                }
            });
        }
        if (pagerWrapEl && !showPager) {
            pagerWrapEl.classList.add("hidden");
        }
    }

    async function fetchData() {
        setLoading(true);
        try {
            let rows = [];
            if (api) {
                const res = await fetch(api);
                const json = await res.json();
                if (!res.ok) {
                    const msg = json?.message || json?.error || `HTTP ${res.status}`;
                    throw new Error(msg);
                }
                rows = Array.isArray(json) ? json : (Array.isArray(json?.data) ? json.data : []);
            } else if (sourceVar) {
                const sourceValue = resolveNamedValue(sourceVar);
                if (Array.isArray(sourceValue)) {
                    rows = sourceValue;
                } else if (Array.isArray(sourceValue?.data)) {
                    rows = sourceValue.data;
                }
            }

            state.allRows = Array.isArray(rows) ? rows : [];
            if (!searchFields.length) {
                const autoFields = inferSearchFields(state.allRows, [titleField, subtitleField, descriptionField, ...metaFields]);
                searchFields = autoFields.length > 0 ? autoFields : [];
            }
            applyAndRender();
        } catch (err) {
            cardsEl.innerHTML = "";
            showEmpty(err?.message || "載入失敗");
        } finally {
            setLoading(false);
        }
    }

    function applyAndRender() {
        const q = state.search.toLowerCase();
        const filtered = !q
            ? [...state.allRows]
            : state.allRows.filter((row) => {
                const fields = searchFields.length ? searchFields : Object.keys(row || {});
                return fields.some((k) => `${row?.[k] ?? ""}`.toLowerCase().includes(q));
            });

        if (state.sortField) {
            const dir = state.sortDir === "desc" ? -1 : 1;
            filtered.sort((a, b) => {
                const av = a?.[state.sortField] ?? "";
                const bv = b?.[state.sortField] ?? "";
                if (av === bv) return 0;
                return av < bv ? -dir : dir;
            });
        }

        state.filteredRows = filtered;
        if (state.currentPage > totalPages()) {
            state.currentPage = totalPages();
        }
        if (state.currentPage < 1) state.currentPage = 1;
        renderCards();
    }

    function renderCards() {
        cardsEl.innerHTML = "";
        const rows = pagedRows();
        if (rows.length === 0) {
            showEmpty(el.dataset.emptyText || "目前無資料");
        } else {
            hideEmpty();
            rows.forEach((row) => {
                const card = createCard(row);
                cardsEl.appendChild(card);
            });
        }
        updatePager();
    }

    function createCard(row) {
        const card = document.createElement("article");
        card.className = "group rounded-xl border border-slate-200 bg-white shadow-sm hover:shadow-md transition-all duration-200 overflow-hidden";
        const cover = coverField ? `${row?.[coverField] ?? ""}` : "";
        const coverAlt = coverAltField ? `${row?.[coverAltField] ?? ""}` : "cover";
        const title = titleField ? `${row?.[titleField] ?? ""}` : `${row?.[keyField] ?? ""}`;
        const subtitle = subtitleField ? `${row?.[subtitleField] ?? ""}` : "";
        const description = descriptionField ? `${row?.[descriptionField] ?? ""}` : "";

        const metaHtml = metaFields
            .map((f) => {
                const value = `${row?.[f] ?? ""}`.trim();
                if (!value) return "";
                return `<span class="inline-flex items-center px-2 py-0.5 rounded-full text-[11px] bg-slate-100 text-slate-600">${escapeHtml(value)}</span>`;
            })
            .filter((x) => !!x)
            .join("");

        card.innerHTML = `
            ${cover ? `<div class="h-36 bg-slate-100 overflow-hidden"><img src="${escapeHtml(cover)}" alt="${escapeHtml(coverAlt)}" class="w-full h-full object-cover"></div>` : ""}
            <div class="p-4 space-y-2">
                <div class="min-h-10">
                    <h4 class="text-sm font-bold text-slate-800 truncate">${escapeHtml(title || "(未命名)")}</h4>
                    ${subtitle ? `<p class="text-xs text-slate-500 truncate">${escapeHtml(subtitle)}</p>` : ""}
                </div>
                ${description ? `<p class="text-xs text-slate-600 line-clamp-3 min-h-12">${escapeHtml(description)}</p>` : ""}
                ${metaHtml ? `<div class="flex flex-wrap gap-1">${metaHtml}</div>` : ""}
                ${(primaryActionText || secondaryActionText) ? `
                    <div class="pt-2 flex items-center gap-2">
                        ${primaryActionText ? `<button type="button" data-role="primary-action" class="px-2.5 py-1.5 rounded-lg bg-blue-600 text-white text-xs font-semibold hover:bg-blue-700 inline-flex items-center gap-1">${iconHtml(primaryActionIcon)}<span>${escapeHtml(primaryActionText)}</span></button>` : ""}
                        ${secondaryActionText ? `<button type="button" data-role="secondary-action" class="px-2.5 py-1.5 rounded-lg bg-slate-100 text-slate-700 text-xs font-semibold hover:bg-slate-200 inline-flex items-center gap-1">${iconHtml(secondaryActionIcon)}<span>${escapeHtml(secondaryActionText)}</span></button>` : ""}
                    </div>
                ` : ""}
            </div>
        `;

        card.addEventListener("click", () => {
            callNamedFunction(onCardClick, { row, key: row?.[keyField], element: el });
        });

        const primaryBtn = card.querySelector('[data-role="primary-action"]');
        if (primaryBtn) {
            primaryBtn.addEventListener("click", (event) => {
                event.stopPropagation();
                callNamedFunction(onPrimaryAction, { row, key: row?.[keyField], element: el });
            });
        }

        const secondaryBtn = card.querySelector('[data-role="secondary-action"]');
        if (secondaryBtn) {
            secondaryBtn.addEventListener("click", (event) => {
                event.stopPropagation();
                callNamedFunction(onSecondaryAction, { row, key: row?.[keyField], element: el });
            });
        }

        return card;
    }

    function pagedRows() {
        const start = (state.currentPage - 1) * state.pageSize;
        return state.filteredRows.slice(start, start + state.pageSize);
    }

    function totalPages() {
        return Math.max(1, Math.ceil(state.filteredRows.length / state.pageSize));
    }

    function updatePager() {
        const total = state.filteredRows.length;
        if (totalCountEl) totalCountEl.textContent = `${total}`;
        if (pageInfoEl) pageInfoEl.textContent = `${state.currentPage} / ${totalPages()}`;
        if (prevEl) prevEl.disabled = state.currentPage <= 1;
        if (nextEl) nextEl.disabled = state.currentPage >= totalPages();
    }

    function setLoading(loading) {
        if (loadingEl) loadingEl.classList.toggle("hidden", !loading);
    }

    function showEmpty(message) {
        if (!emptyEl) return;
        emptyEl.textContent = message;
        emptyEl.classList.remove("hidden");
    }

    function hideEmpty() {
        if (!emptyEl) return;
        emptyEl.classList.add("hidden");
    }

    function defaultSortFields() {
        const set = new Set();
        [titleField, subtitleField, ...metaFields].forEach((f) => {
            if (f) set.add(f);
        });
        return [...set].map((f) => ({ field: f, label: f }));
    }
}

function parseCsv(raw) {
    return (raw || "")
        .split(",")
        .map((x) => x.trim())
        .filter((x) => !!x);
}

function parseFieldLabelPairs(raw) {
    return parseCsv(raw).map((part) => {
        const idx = part.indexOf(":");
        if (idx < 0) return { field: part, label: part };
        const field = part.substring(0, idx).trim();
        const label = part.substring(idx + 1).trim() || field;
        return { field, label };
    }).filter((x) => !!x.field);
}

function inferSearchFields(rows, preferred) {
    const p = (preferred || []).filter((x) => !!x);
    if (p.length > 0) return [...new Set(p)];
    if (!Array.isArray(rows) || rows.length === 0) return [];
    const first = rows.find((r) => r && typeof r === "object");
    if (!first) return [];
    return Object.keys(first).slice(0, 8);
}

function resolveNamedValue(pathName) {
    const path = (pathName || "").split(".").filter((x) => !!x);
    let target = window;
    for (const p of path) {
        target = target?.[p];
    }
    return target;
}

function iconHtml(icon) {
    const name = (icon || "").toLowerCase();
    if (name === "message") {
        return `<svg class="w-3.5 h-3.5" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M8 10h8m-8 4h5m8-2a9 9 0 11-18 0 9 9 0 0118 0z"></path></svg>`;
    }
    return `<svg class="w-3.5 h-3.5" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M9 5l7 7-7 7"></path></svg>`;
}

document.addEventListener("DOMContentLoaded", () => {
    initGFileUploaders(document);
    initGCardViews(document);
});

window.initGFileUploaders = initGFileUploaders;
window.initGCardViews = initGCardViews;

