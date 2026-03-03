function toggleNode(element) {
    const treeItem = element;
    const treeNode = element.parentElement;
    const children = treeNode.querySelector('.tree-children');

    if (!children) return;

    if (children.style.display === 'none') {
        children.style.display = 'block';
        treeItem.classList.add('expanded');
    } else {
        children.style.display = 'none';
        treeItem.classList.remove('expanded');
    }
}

function toDateOnly(value) {
    if (!value || typeof value === 'object' || value === '-') return '-';
    const s = String(value);
    if (s.includes('T')) return s.split('T')[0];
    if (s.includes(' ')) return s.split(' ')[0];
    return s;
}

function openProgramModal(input) {
    let data;

    if (input instanceof HTMLElement) {
        data = {
            programNo: input.dataset.programNo,
            programName: input.dataset.programName,
            purpose: input.dataset.purpose,
            employeeId: input.dataset.employeeId,
            programType: input.dataset.programType,
            planStart: input.dataset.planStart,
            planFinish: input.dataset.planFinish,
            realStart: input.dataset.realStart,
            realFinish: input.dataset.realFinish,
            planHours: input.dataset.planHours,
            realHours: input.dataset.realHours
        };
    } else {
        data = {
            programNo: input.PROGRAM_NO,
            programName: input.PROGRAM_NAME,
            purpose: input.PURPOSE,
            employeeId: input.EMPLOYEE_ID,
            programType: input.PROGRAM_TYPE,
            planStart: input.PLAN_START_DEVELOP_DATE,
            planFinish: input.PLAN_FINISH_DEVELOP_DATE,
            realStart: input.REAL_START_DEVELOP_DATE,
            realFinish: input.REAL_FINISH_DEVELOP_DATE,
            planHours: input.PLAN_WORK_HOURS,
            realHours: input.REAL_WORK_HOURS
        };
    }

    document.getElementById('modal-program-no').textContent = data.programNo || '-';
    document.getElementById('modal-program-name').textContent = data.programName || '-';
    document.getElementById('modal-purpose').textContent = data.purpose || '-';
    document.getElementById('modal-employee-id').textContent = data.employeeId || '-';
    document.getElementById('modal-program-type').textContent = data.programType || '-';

    document.getElementById('modal-plan-start').textContent = toDateOnly(data.planStart);
    document.getElementById('modal-plan-finish').textContent = toDateOnly(data.planFinish);
    document.getElementById('modal-real-start').textContent = toDateOnly(data.realStart);
    document.getElementById('modal-real-finish').textContent = toDateOnly(data.realFinish);

    document.getElementById('modal-plan-hours').textContent = data.planHours || '-';
    document.getElementById('modal-real-hours').textContent = data.realHours || '-';

    const btnOpenProgram = document.getElementById('btnOpenProgram');
    if (btnOpenProgram) {
        btnOpenProgram.onclick = function (e) {
            e.preventDefault();
            const programNo = (data.programNo || '').toUpperCase();
            openExecutionModal(resolveProgramUrl(programNo), `${programNo} ${data.programName || ''}`.trim());
        };
    }

    const modal = document.getElementById('programModal');
    modal.style.display = 'flex';
    modal.classList.remove('hidden');

    const content = document.getElementById('modalContent');
    if (!content.classList.contains('w-screen')) {
        toggleMaximizeModal();
    }

    const modalBody = modal.querySelector('.overflow-y-auto');
    const header = modal.querySelector('.bg-gradient-to-r');
    if (modalBody && header) {
        modalBody.onscroll = function () {
            if (modalBody.scrollTop > 10) {
                header.classList.add('shadow-xl');
            } else {
                header.classList.remove('shadow-xl');
            }
        };
    }

    document.body.style.overflow = 'hidden';
}

function closeProgramModal() {
    const modal = document.getElementById('programModal');
    const content = document.getElementById('modalContent');

    modal.style.display = 'none';
    modal.classList.add('hidden');
    document.body.style.overflow = '';

    if (content.classList.contains('w-screen')) {
        toggleMaximizeModal();
    }
}

function toggleMaximizeModal() {
    const modal = document.getElementById('programModal');
    const content = document.getElementById('modalContent');
    const maxIcon = document.getElementById('maximizeIcon');
    const restoreIcon = document.getElementById('restoreIcon');
    const isMax = content.classList.contains('w-screen');

    if (isMax) {
        content.classList.remove('w-screen', 'h-screen', 'max-w-none', 'max-h-none', 'rounded-none', 'scale-100');
        content.classList.add('w-full', 'max-w-4xl', 'max-h-[90vh]', 'rounded-2xl', 'scale-95');
        modal.classList.add('p-4');
        maxIcon.classList.remove('hidden');
        restoreIcon.classList.add('hidden');
    } else {
        content.classList.add('w-screen', 'h-screen', 'max-w-none', 'max-h-none', 'rounded-none', 'scale-100');
        content.classList.remove('w-full', 'max-w-4xl', 'max-h-[90vh]', 'rounded-2xl', 'scale-95');
        modal.classList.remove('p-4');
        maxIcon.classList.add('hidden');
        restoreIcon.classList.remove('hidden');
    }
}

window.onclick = function (event) {
    const modal = document.getElementById('programModal');
    if (modal && event.target === modal) {
        closeProgramModal();
    }
};

let suggestionTimeout = null;
let currentFocus = -1;

document.addEventListener('DOMContentLoaded', () => {
    const input = document.querySelector('input[name="program_no"]');
    const list = document.getElementById('suggestionList');

    if (!input || !list) return;

    input.addEventListener('input', (e) => {
        const val = e.target.value.trim();
        clearTimeout(suggestionTimeout);

        if (val.length < 1) {
            hideSuggestions();
            return;
        }

        suggestionTimeout = setTimeout(() => fetchSuggestions(val), 280);
    });

    input.addEventListener('keydown', (e) => {
        const items = list.getElementsByClassName('suggestion-item');

        if (e.key === 'ArrowDown') {
            e.preventDefault();
            currentFocus++;
            addActive(items);
        } else if (e.key === 'ArrowUp') {
            e.preventDefault();
            currentFocus--;
            addActive(items);
        } else if (e.key === 'Enter') {
            if (currentFocus > -1 && items[currentFocus]) {
                items[currentFocus].click();
                e.preventDefault();
            }
        } else if (e.key === 'Escape') {
            hideSuggestions();
        }
    });

    document.addEventListener('click', (e) => {
        if (!list.contains(e.target) && e.target !== input) {
            hideSuggestions();
        }
    });

    const reposition = () => {
        if (list.classList.contains('show')) {
            positionSuggestions();
        }
    };

    window.addEventListener('scroll', reposition, true);
    window.addEventListener('resize', reposition);
});

function fetchSuggestions(query) {
    fetch(`/api/mis/programs/suggestions?q=${encodeURIComponent(query)}`)
        .then((r) => {
            if (!r.ok) throw new Error(`HTTP ${r.status}`);
            return r.json();
        })
        .then((data) => {
            console.log('[Suggestion] API 回傳:', data);
            showSuggestions(data);
        })
        .catch((err) => {
            console.error('[Suggestion] 載入失敗:', err);
        });
}

function positionSuggestions() {
    const input = document.querySelector('input[name="program_no"]');
    const list = document.getElementById('suggestionList');
    if (!input || !list) return;

    const rect = input.getBoundingClientRect();
    list.style.top = `${rect.bottom + 4}px`;
    list.style.left = `${rect.left}px`;
    list.style.width = `${rect.width}px`;
}

function showSuggestions(data) {
    const list = document.getElementById('suggestionList');
    list.innerHTML = '';
    currentFocus = -1;

    if (!data || data.length === 0) {
        hideSuggestions();
        return;
    }

    data.forEach((item) => {
        const no = item.program_no ?? item.PROGRAM_NO ?? '';
        const name = item.program_name ?? item.PROGRAM_NAME ?? '';

        const div = document.createElement('div');
        div.className = 'suggestion-item';
        div.innerHTML = `
            <span class="prog-no">${no}</span>
            <span class="prog-name">${name || '未命名'}</span>
        `;

        div.addEventListener('mousedown', (e) => {
            e.preventDefault();
            const input = document.querySelector('input[name="program_no"]');
            input.value = no;
            hideSuggestions();
        });

        list.appendChild(div);
    });

    positionSuggestions();
    list.classList.remove('hidden');
    list.classList.add('show');
}

function hideSuggestions() {
    const list = document.getElementById('suggestionList');
    if (!list) return;

    list.classList.remove('show');
    setTimeout(() => list.classList.add('hidden'), 200);
}

function addActive(items) {
    if (!items || !items.length) return;
    removeActive(items);

    if (currentFocus >= items.length) currentFocus = 0;
    if (currentFocus < 0) currentFocus = items.length - 1;

    items[currentFocus].classList.add('active');
    items[currentFocus].scrollIntoView({ block: 'nearest' });
}

function removeActive(items) {
    for (let i = 0; i < items.length; i++) {
        items[i].classList.remove('active');
    }
}

function runProgram() {
    const input = document.getElementsByName('program_no')[0];
    const programNo = (input?.value || '').trim().toUpperCase();

    if (!programNo) {
        alert('請先輸入程式代號');
        return;
    }

    openExecutionModal(resolveProgramUrl(programNo), programNo);
}

function resolveProgramUrl(programNo) {
    const code = String(programNo || '').trim().toUpperCase();
    if (!code) return '/mis/programs';

    const moduleMatch = code.match(/^([A-Z]{3})[A-Z0-9_]+$/);
    if (moduleMatch) {
        const module = moduleMatch[1].toLowerCase();
        const controller = module.charAt(0).toUpperCase() + module.slice(1);
        return `/${controller}/${code}`;
    }

    return `/mis/programs/${encodeURIComponent(code)}`;
}

function openExecutionModal(url, title) {
    const modal = document.getElementById('executionModal');
    const iframe = document.getElementById('executionIframe');
    const titleEl = document.getElementById('executionModalTitle');

    titleEl.textContent = title ? `${title} - 程式執行` : '程式執行';
    iframe.src = url;

    modal.classList.remove('hidden');
    modal.style.display = 'flex';

    setTimeout(() => {
        const content = document.getElementById('executionModalContent');
        content.classList.remove('scale-95');
        content.classList.add('scale-100');
    }, 10);

    closeProgramModal();
}

function closeExecutionModal() {
    const modal = document.getElementById('executionModal');
    const iframe = document.getElementById('executionIframe');
    const modalContent = document.getElementById('executionModalContent');

    modalContent.classList.remove('scale-100');
    modalContent.classList.add('scale-95');

    setTimeout(() => {
        modal.classList.add('hidden');
        modal.style.display = 'none';
        iframe.src = 'about:blank';
    }, 300);
}

function toggleExecutionMaximize() {
    const modalContent = document.getElementById('executionModalContent');
    const maxIcon = document.getElementById('execMaximizeIcon');
    const restoreIcon = document.getElementById('execRestoreIcon');

    if (modalContent.classList.contains('max-w-7xl')) {
        modalContent.classList.remove('max-w-7xl', 'h-[95vh]', 'rounded-2xl');
        modalContent.classList.add('w-full', 'h-screen', 'rounded-none');
        maxIcon.classList.add('hidden');
        restoreIcon.classList.remove('hidden');
    } else {
        modalContent.classList.add('max-w-7xl', 'h-[95vh]', 'rounded-2xl');
        modalContent.classList.remove('w-full', 'h-screen', 'rounded-none');
        maxIcon.classList.remove('hidden');
        restoreIcon.classList.add('hidden');
    }
}
