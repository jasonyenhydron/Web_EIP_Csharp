'use strict';

window.gDataForm = function (formId, initialFields) {
  return {
    formId,
    mode: 'view',
    formData: { ...initialFields },
    originalData: {},
    errors: {},
    selectOptions: {},
    hasSelection: false,
    loading: false,
    alwaysReadOnly: false,

    init() {
      const el = document.getElementById(this.formId);
      if (!el) return;
      this.alwaysReadOnly = el.dataset.alwaysReadonly === '1';

      const parentId = el.dataset.parentId;
      if (parentId) {
        document.addEventListener(`gDataForm:selected:${parentId}`, (e) => {
          this._loadByRelation(el, e.detail);
        });
      }

      const api = el.dataset.api;
      if (api && !parentId) {
        this._loadData(el);
      }

      const cb = el.dataset.onLoadSuccess;
      if (cb && typeof window[cb] === 'function') {
        this.$nextTick(() => window[cb](this.formData));
      }
    },

    handleToolAction(action, customFn, formIdValue) {
      if (action === 'custom' && customFn && typeof window[customFn] === 'function') {
        window[customFn](this.formData);
        return;
      }
      const el = document.getElementById(formIdValue);
      if (!el) return;

      switch (action) {
        case 'add': this.openAdd(formIdValue, el); break;
        case 'edit': this.openEdit(formIdValue, el); break;
        case 'delete': this.openDelete(formIdValue, el); break;
      }
    },

    openAdd(formIdValue, el) {
      this.mode = 'add';
      this.errors = {};
      const defaults = this._getDefaults(el);
      this.formData = { ...defaults };
      this._applyRelationDefaults(el);
      this._openFlowbiteModal(`${formIdValue}_modal`);
    },

    openEdit(formIdValue) {
      if (!this.hasSelection) {
        gDataFormToast.warn('請先選取一筆資料');
        return;
      }
      this.mode = 'edit';
      this.errors = {};
      this.originalData = { ...this.formData };
      this._openFlowbiteModal(`${formIdValue}_modal`);
    },

    openDelete(formIdValue) {
      if (!this.hasSelection) {
        gDataFormToast.warn('請先選取一筆資料');
        return;
      }
      this._openFlowbiteModal(`${formIdValue}_del_modal`);
    },

    async confirmDelete(formIdValue, modalId) {
      const el = document.getElementById(formIdValue);
      if (!el) return;
      const api = el.dataset.api;
      if (!api) return;

      this.loading = true;
      try {
        const pkValue = this._getPrimaryKeyValue(el);
        const url = pkValue ? `${api}/${pkValue}` : api;
        const res = await fetch(url, { method: 'DELETE', headers: this._headers() });
        const json = await res.json().catch(() => ({}));

        if (res.ok && (json.status === 'success' || res.status === 200)) {
          gDataFormToast.success('刪除成功');
          this.formData = this._getDefaults(el);
          this.hasSelection = false;
          this.closeModal(modalId);
          this._triggerCallback(el, 'onApplied', null);
          this._notifyChain(el);
        } else {
          gDataFormToast.error(json.message || '刪除失敗');
        }
      } catch (err) {
        gDataFormToast.error(`刪除失敗：${err.message}`);
      } finally {
        this.loading = false;
      }
    },

    async submitForm(formIdValue, modalId) {
      const el = document.getElementById(formIdValue);
      if (!el) return;
      const api = el.dataset.api;
      if (!api) return;

      if (this._triggerCallback(el, 'onBeforeValidate', this.formData) === false) return;

      const validateStyle = el.dataset.validateStyle || 'hint';
      const isValid = this._validate(el);
      if (!isValid) {
        if (validateStyle === 'dialog') {
          const msgs = Object.values(this.errors).filter(Boolean).join('\n');
          alert(`請修正以下錯誤：\n${msgs}`);
        }
        return;
      }

      if (this._triggerCallback(el, 'onApply', this.formData) === false) return;

      const dupCheck = el.dataset.duplicateCheck === '1';
      if (dupCheck && this.mode === 'add') {
        const isDup = await this._checkDuplicate(api);
        if (isDup) {
          gDataFormToast.error('資料重複，請確認後再送出');
          return;
        }
      }

      this.loading = true;
      try {
        const isAdd = this.mode === 'add';
        const pkVal = this._getPrimaryKeyValue(el);
        const url = isAdd ? api : `${api}/${pkVal}`;
        const method = isAdd ? 'POST' : 'PUT';

        const res = await fetch(url, {
          method,
          headers: this._headers(),
          body: JSON.stringify(this.formData)
        });
        const json = await res.json().catch(() => ({}));

        if (res.ok && (json.status === 'success' || res.status === 200 || res.status === 201)) {
          gDataFormToast.success(isAdd ? '新增成功' : '儲存成功');

          if (el.dataset.showApplyButton === '1' && json.data) {
            this.formData = { ...this.formData, ...json.data };
          }

          this._triggerCallback(el, 'onApplied', json.data || this.formData);

          const continueAdd = el.dataset.continueAdd === '1';
          if (continueAdd && isAdd) {
            this.formData = this._getDefaults(el);
            this.errors = {};
          } else {
            this.closeModal(modalId);
            if (json.data) this.formData = { ...this.formData, ...json.data };
            this.hasSelection = true;
            this.mode = 'view';
          }

          this._notifyChain(el);

          if (el.dataset.autoPageClose === '1') {
            setTimeout(() => window.close(), 800);
          }
        } else {
          gDataFormToast.error(json.message || '儲存失敗');
        }
      } catch (err) {
        gDataFormToast.error(`儲存失敗：${err.message}`);
      } finally {
        this.loading = false;
      }
    },

    cancelForm(formIdValue, modalId) {
      const el = document.getElementById(formIdValue);
      if (this.mode === 'edit') {
        this.formData = { ...this.originalData };
      }
      this.errors = {};
      this.mode = 'view';
      this._triggerCallback(el, 'onCancel', null);
      this.closeModal(modalId);
    },

    async _loadData(el, extraParams) {
      const api = el.dataset.api;
      if (!api) return;

      const params = extraParams ? '?' + new URLSearchParams(extraParams).toString() : '';

      try {
        const res = await fetch(`${api}${params}`, { headers: this._headers() });
        const json = await res.json().catch(() => ({}));
        if (res.ok && json.data) {
          this.formData = { ...this.formData, ...json.data };
          this.hasSelection = true;
          this._triggerCallback(el, 'onLoadSuccess', this.formData);
        }
      } catch (_) {
      }
    },

    _loadByRelation(el, masterData) {
      const relJson = el.dataset.relationColumns;
      if (!relJson) return;
      try {
        const relations = JSON.parse(relJson);
        const params = {};
        relations.forEach((r) => {
          if (r.MasterField && r.DetailField && masterData[r.MasterField] != null) {
            params[r.DetailField] = masterData[r.MasterField];
          }
        });
        this._loadData(el, params);
      } catch (_) {
      }
    },

    selectRow(data) {
      this.formData = { ...data };
      this.hasSelection = true;
      this.mode = 'view';
      const el = document.getElementById(this.formId);
      this._notifyChain(el);
      document.dispatchEvent(new CustomEvent(`gDataForm:selected:${this.formId}`, { detail: data }));
    },

    async loadSelectOptions(apiUrl, fieldName) {
      if (!apiUrl || this.selectOptions[fieldName]) return;
      try {
        const res = await fetch(apiUrl, { headers: this._headers() });
        const json = await res.json().catch(() => ({}));
        if (res.ok && Array.isArray(json.data)) {
          this.selectOptions[fieldName] = json.data.map((item) => ({
            value: item.value ?? item.id ?? item.code ?? '',
            label: item.label ?? item.name ?? item.text ?? ''
          }));
        }
      } catch (_) {
      }
    },

    _validate(el) {
      this.errors = {};
      const fields = el.querySelectorAll('[data-validate="1"]');
      let valid = true;

      fields.forEach((f) => {
        const name = f.dataset.fieldName;
        const required = f.dataset.required === '1';
        const validateFn = f.dataset.validateFn;
        const validateMsg = f.dataset.validateMsg;
        const val = this.formData[name];

        if (required && (val === null || val === undefined || String(val).trim() === '')) {
          this.errors[name] = validateMsg || `${f.dataset.caption || name} 為必填`;
          valid = false;
          return;
        }

        if (validateFn && typeof window[validateFn] === 'function') {
          const result = window[validateFn](val, this.formData);
          if (result !== true) {
            this.errors[name] = typeof result === 'string' ? result : (validateMsg || '驗證失敗');
            valid = false;
          }
        }
      });

      return valid;
    },

    async _checkDuplicate(api) {
      try {
        const url = `${api}/check-duplicate`;
        const res = await fetch(url, {
          method: 'POST',
          headers: this._headers(),
          body: JSON.stringify(this.formData)
        });
        const json = await res.json().catch(() => ({}));
        return json.isDuplicate === true;
      } catch (_) {
        return false;
      }
    },

    _getDefaults(el) {
      const defaults = {};
      el.querySelectorAll('[data-field-default]').forEach((f) => {
        defaults[f.dataset.fieldName] = f.dataset.fieldDefault ?? '';
      });
      return defaults;
    },

    _applyRelationDefaults(el) {
      const parentId = el.dataset.parentId;
      if (!parentId) return;
      const parentEl = document.getElementById(parentId);
      if (!parentEl || !parentEl._x_dataStack) return;

      try {
        const parentData = Alpine.$data(parentEl)?.formData;
        const relJson = el.dataset.relationColumns;
        if (!parentData || !relJson) return;
        const relations = JSON.parse(relJson);
        relations.forEach((r) => {
          if (r.MasterField && r.DetailField && parentData[r.MasterField] != null) {
            this.formData[r.DetailField] = parentData[r.MasterField];
          }
        });
      } catch (_) {
      }
    },

    _getPrimaryKeyValue(el) {
      const pkField = el.querySelector('[data-is-pk="1"]');
      if (pkField) return this.formData[pkField.dataset.fieldName];
      const keys = Object.keys(this.formData);
      const idField = keys.find((k) => k.toLowerCase().endsWith('_id') || k.toLowerCase() === 'id');
      return idField ? this.formData[idField] : null;
    },

    _notifyChain(el) {
      if (!el) return;
      const chainId = el.dataset.chainId;
      if (!chainId) return;
      document.dispatchEvent(new CustomEvent(`gDataForm:selected:${this.formId}`, {
        detail: this.formData
      }));
    },

    _triggerCallback(el, dataAttr, payload) {
      if (!el) return;
      const attrMap = {
        onLoadSuccess: 'onLoadSuccess',
        onApply: 'onApply',
        onApplied: 'onApplied',
        onCancel: 'onCancel',
        onBeforeValidate: 'onBeforeValidate'
      };
      const key = attrMap[dataAttr] || dataAttr;
      const cbName = el.dataset[key];
      if (!cbName) return;
      const fn = window[cbName];
      if (typeof fn === 'function') return fn(payload);
    },

    _headers() {
      const token = document.querySelector('meta[name="csrf-token"]')?.content;
      const h = { 'Content-Type': 'application/json', 'Accept': 'application/json' };
      if (token) h['X-CSRF-TOKEN'] = token;
      return h;
    },

    _openFlowbiteModal(modalId) {
      const el = document.getElementById(modalId);
      if (!el) return;
      if (window.FlowbiteInstances) {
        const instance = window.FlowbiteInstances.getInstance('Modal', modalId);
        if (instance) {
          instance.show();
          return;
        }
      }
      el.classList.remove('hidden');
      el.classList.add('flex');
    },

    closeModal(modalId) {
      const el = document.getElementById(modalId);
      if (!el) return;
      if (window.FlowbiteInstances) {
        const instance = window.FlowbiteInstances.getInstance('Modal', modalId);
        if (instance) {
          instance.hide();
          return;
        }
      }
      el.classList.add('hidden');
      el.classList.remove('flex');
    }
  };
};

window.gDataFormToast = (() => {
  function show(message, type) {
    const colors = {
      success: { bg: 'bg-green-50 border-green-300', icon: 'text-green-500', path: 'M5 13l4 4L19 7' },
      error: { bg: 'bg-red-50 border-red-300', icon: 'text-red-500', path: 'M6 18L18 6M6 6l12 12' },
      warn: { bg: 'bg-yellow-50 border-yellow-300', icon: 'text-yellow-500', path: 'M12 9v4m0 4h.01M10.29 3.86L1.82 18a2 2 0 001.71 3h16.94a2 2 0 001.71-3L13.71 3.86a2 2 0 00-3.42 0z' }
    };
    const c = colors[type] || colors.success;

    let container = document.getElementById('_gdf_toast_container');
    if (!container) {
      container = document.createElement('div');
      container.id = '_gdf_toast_container';
      container.className = 'fixed top-4 right-4 z-[200] flex flex-col gap-2';
      document.body.appendChild(container);
    }

    const toast = document.createElement('div');
    toast.className = `flex items-center gap-3 px-4 py-3 rounded-xl border shadow-md text-sm font-medium text-slate-700 ${c.bg} transition-all duration-300 opacity-0 translate-x-4`;
    toast.innerHTML = `
      <svg class="w-5 h-5 flex-shrink-0 ${c.icon}" fill="none" stroke="currentColor" viewBox="0 0 24 24">
        <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="${c.path}"/>
      </svg>
      <span>${message}</span>
      <button onclick="this.closest('div[data-toast]').remove()" class="ml-2 text-slate-400 hover:text-slate-600">
        <svg class="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M6 18L18 6M6 6l12 12"/></svg>
      </button>`;
    toast.setAttribute('data-toast', '1');
    container.appendChild(toast);

    requestAnimationFrame(() => {
      toast.classList.remove('opacity-0', 'translate-x-4');
    });

    setTimeout(() => {
      toast.classList.add('opacity-0', 'translate-x-4');
      setTimeout(() => toast.remove(), 300);
    }, 3500);
  }

  return {
    success: (msg) => show(msg, 'success'),
    error: (msg) => show(msg, 'error'),
    warn: (msg) => show(msg, 'warn')
  };
})();

document.addEventListener('DOMContentLoaded', () => {
  if (typeof initFlowbite === 'function') initFlowbite();
});
