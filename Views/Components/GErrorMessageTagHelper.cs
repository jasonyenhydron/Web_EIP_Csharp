using Microsoft.AspNetCore.Razor.TagHelpers;

namespace Web_EIP_Csharp.Views.Components
{
    [HtmlTargetElement("g-error-message", TagStructure = TagStructure.WithoutEndTag)]
    public class GErrorMessageTagHelper : TagHelper
    {
        public string Title { get; set; } = "系統錯誤";
        public bool AutoCapture { get; set; } = true;
        public bool CaptureFetch { get; set; } = true;
        public bool CaptureWindowError { get; set; } = true;
        public bool CaptureUnhandledRejection { get; set; } = true;

        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            output.TagName = null;
            output.Content.SetHtmlContent($@"
<div id=""gErrorMessageRoot"" class=""hidden fixed inset-0 z-[120] bg-slate-900/50 backdrop-blur-sm items-center justify-center p-4"">
  <div class=""w-full max-w-3xl bg-white border border-slate-200 rounded-xl shadow-2xl overflow-hidden"">
    <div class=""px-4 py-3 border-b border-slate-200 bg-slate-50 flex items-center justify-between"">
      <div class=""text-sm font-bold text-slate-800"">{System.Net.WebUtility.HtmlEncode(Title)}</div>
      <button type=""button"" class=""p-1.5 rounded hover:bg-slate-200 text-slate-500"" onclick=""gHideErrorMessage()"" title=""關閉"">
        <svg class=""w-4 h-4"" fill=""none"" stroke=""currentColor"" viewBox=""0 0 24 24""><path stroke-linecap=""round"" stroke-linejoin=""round"" stroke-width=""2"" d=""M6 18L18 6M6 6l12 12""></path></svg>
      </button>
    </div>
    <div class=""p-4 space-y-3"">
      <div class=""text-sm text-slate-700"">
        <div><span class=""font-semibold"">訊息：</span><span id=""gErrMsgText""></span></div>
        <div><span class=""font-semibold"">來源：</span><span id=""gErrMsgSource""></span></div>
        <div><span class=""font-semibold"">行號：</span><span id=""gErrMsgLine""></span></div>
      </div>
      <div>
        <label class=""block text-xs text-slate-500 mb-1"">詳細內容</label>
        <pre id=""gErrMsgDetail"" class=""max-h-72 overflow-auto text-xs bg-slate-900 text-slate-100 rounded-lg p-3 border border-slate-700 whitespace-pre-wrap""></pre>
      </div>
      <div class=""flex justify-end gap-2"">
        <button type=""button"" onclick=""gCopyErrorMessage()"" class=""px-3 py-1.5 text-xs rounded border border-slate-300 bg-white hover:bg-slate-50 text-slate-700"">複製</button>
        <button type=""button"" onclick=""gHideErrorMessage()"" class=""px-3 py-1.5 text-xs rounded bg-blue-600 text-white hover:bg-blue-700"">關閉</button>
      </div>
    </div>
  </div>
</div>

<script>
(function() {{
  if (window.__gErrorMessageInited) return;
  window.__gErrorMessageInited = true;

  const cfg = {{
    autoCapture: {(AutoCapture ? "true" : "false")},
    captureFetch: {(CaptureFetch ? "true" : "false")},
    captureWindowError: {(CaptureWindowError ? "true" : "false")},
    captureUnhandledRejection: {(CaptureUnhandledRejection ? "true" : "false")}
  }};

  let currentErrorText = '';

  function toText(v) {{
    if (v == null) return '';
    if (typeof v === 'string') return v;
    try {{ return JSON.stringify(v, null, 2); }} catch {{ return String(v); }}
  }}

  function normalizeError(input) {{
    if (typeof input === 'string') {{
      return {{ message: input, source: '', lineNumber: '', detail: input }};
    }}
    if (!input || typeof input !== 'object') {{
      const t = toText(input);
      return {{ message: t || '未知錯誤', source: '', lineNumber: '', detail: t }};
    }}
    return {{
      message: input.message || input.title || '系統發生錯誤',
      source: input.source || input.fileName || input.url || '',
      lineNumber: input.lineNumber || input.lineno || input.line || '',
      detail: input.detail || input.stack || toText(input)
    }};
  }}

  window.gShowErrorMessage = function(input) {{
    const e = normalizeError(input);
    const root = document.getElementById('gErrorMessageRoot');
    if (!root) return;
    document.getElementById('gErrMsgText').textContent = e.message || '';
    document.getElementById('gErrMsgSource').textContent = e.source || '-';
    document.getElementById('gErrMsgLine').textContent = e.lineNumber || '-';
    document.getElementById('gErrMsgDetail').textContent = e.detail || e.message || '';

    currentErrorText =
      `訊息: ${{e.message || ''}}\\n來源: ${{e.source || '-'}}\\n行號: ${{e.lineNumber || '-'}}\\n\\n${{e.detail || e.message || ''}}`;

    root.classList.remove('hidden');
    root.classList.add('flex');
  }};

  window.gHideErrorMessage = function() {{
    const root = document.getElementById('gErrorMessageRoot');
    if (!root) return;
    root.classList.add('hidden');
    root.classList.remove('flex');
  }};

  window.gCopyErrorMessage = async function() {{
    if (!currentErrorText) return;
    try {{
      await navigator.clipboard.writeText(currentErrorText);
      if (window.gToast) window.gToast('錯誤內容已複製', 'success');
    }} catch {{
      const ta = document.createElement('textarea');
      ta.value = currentErrorText;
      document.body.appendChild(ta);
      ta.select();
      document.execCommand('copy');
      ta.remove();
      if (window.gToast) window.gToast('錯誤內容已複製', 'success');
    }}
  }};

  if (!cfg.autoCapture) return;

  if (cfg.captureWindowError) {{
    window.addEventListener('error', function(ev) {{
      window.gShowErrorMessage({{
        message: ev.message || '前端執行錯誤',
        source: ev.filename || '',
        lineNumber: ev.lineno || '',
        detail: ev.error?.stack || ev.message || ''
      }});
    }});
  }}

  if (cfg.captureUnhandledRejection) {{
    window.addEventListener('unhandledrejection', function(ev) {{
      const reason = ev.reason || {{}};
      window.gShowErrorMessage({{
        message: reason.message || '非預期 Promise 錯誤',
        source: '',
        lineNumber: '',
        detail: reason.stack || toText(reason)
      }});
    }});
  }}

  if (cfg.captureFetch && !window.__gFetchWrapped) {{
    window.__gFetchWrapped = true;
    const rawFetch = window.fetch.bind(window);
    window.fetch = async function(...args) {{
      try {{
        const res = await rawFetch(...args);
        if (!res.ok) {{
          let payload = null;
          try {{
            const ct = res.headers.get('content-type') || '';
            if (ct.includes('application/json')) payload = await res.clone().json();
            else payload = {{ message: await res.clone().text() }};
          }} catch {{}}

          window.gShowErrorMessage({{
            message: payload?.message || `HTTP ${{res.status}} ${{res.statusText}}`,
            source: args?.[0]?.toString?.() || '',
            lineNumber: payload?.lineNumber || '',
            detail: payload?.detail || payload?.stack || toText(payload) || ''
          }});
        }}
        return res;
      }} catch (err) {{
        window.gShowErrorMessage({{
          message: err?.message || '網路錯誤',
          source: args?.[0]?.toString?.() || '',
          lineNumber: '',
          detail: err?.stack || toText(err)
        }});
        throw err;
      }}
    }};
  }}
}})();
</script>");
        }
    }
}

