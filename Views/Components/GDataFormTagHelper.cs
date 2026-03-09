using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Razor.TagHelpers;
using System.Text;
using Web_EIP_Csharp.Models.DataForm;

namespace Web_EIP_Csharp.Views.Components
{
    /// <summary>
    /// g-dataform TagHelper
    /// </summary>
    [HtmlTargetElement("g-dataform")]
    [RestrictChildren("form-column")]
    public class GDataFormTagHelper : TagHelper
    {
        private const string RuntimeInjectedKey = "__g_dataform_runtime_injected";

        [Microsoft.AspNetCore.Mvc.ViewFeatures.ViewContext]
        [HtmlAttributeNotBound]
        public ViewContext? ViewContext { get; set; }

        public string Id { get; set; } = "";
        public string Title { get; set; } = "";

        [HtmlAttributeName("api")]
        public string Api { get; set; } = "";

        [HtmlAttributeName("member-id")]
        public string DataMember { get; set; } = "";

        public int HorizontalColumnsCount { get; set; } = 2;
        public int HorizontalGap { get; set; } = 4;
        public int VerticalGap { get; set; } = 4;

        [HtmlAttributeName("caption-alignment")]
        public string CaptionAlignment { get; set; } = "left";

        [HtmlAttributeName("extra-class")]
        public string ExtraClass { get; set; } = "";

        [HtmlAttributeName("always-read-only")]
        public bool AlwaysReadOnly { get; set; } = false;

        [HtmlAttributeName("continue-add")]
        public bool ContinueAdd { get; set; } = false;

        [HtmlAttributeName("is-auto-page-close")]
        public bool IsAutoPageClose { get; set; } = false;

        [HtmlAttributeName("duplicate-check")]
        public bool DuplicateCheck { get; set; } = false;

        [HtmlAttributeName("validate-style")]
        public string ValidateStyle { get; set; } = "Hint";

        [HtmlAttributeName("show-apply-button")]
        public bool ShowApplyButton { get; set; } = false;

        [HtmlAttributeName("chain-dataform-id")]
        public string ChainDataFormID { get; set; } = "";

        [HtmlAttributeName("parent-object-id")]
        public string ParentObjectID { get; set; } = "";

        [HtmlAttributeName("relation-columns")]
        public string RelationColumns { get; set; } = "";

        [HtmlAttributeName("tool-items")]
        public string ToolItems { get; set; } = "";

        [HtmlAttributeName("on-load-success")]
        public string OnLoadSuccess { get; set; } = "";

        [HtmlAttributeName("on-apply")]
        public string OnApply { get; set; } = "";

        [HtmlAttributeName("on-applied")]
        public string OnApplied { get; set; } = "";

        [HtmlAttributeName("on-cancel")]
        public string OnCancel { get; set; } = "";

        [HtmlAttributeName("on-before-validate")]
        public string OnBeforeValidate { get; set; } = "";

        public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
        {
            if (string.IsNullOrWhiteSpace(DataMember) &&
                context.AllAttributes.TryGetAttribute("data-member", out var memberAttr))
            {
                DataMember = memberAttr.Value?.ToString() ?? string.Empty;
            }

            var colCtx = new FormColumnContext();
            context.Items[typeof(FormColumnContext)] = colCtx;
            await output.GetChildContentAsync();

            var formId = string.IsNullOrWhiteSpace(Id)
                ? $"gdf_{Guid.NewGuid():N}"
                : Id.Trim();

            var gridCols = Math.Max(1, HorizontalColumnsCount);

            EnsureRuntimeInjected(output);

            output.TagName = "div";
            output.Attributes.SetAttribute("id", formId);
            output.Attributes.SetAttribute("data-g-dataform", "1");
            output.Attributes.SetAttribute("data-api", Api);
            output.Attributes.SetAttribute("data-member", DataMember);
            output.Attributes.SetAttribute("data-validate-style", ValidateStyle.ToLowerInvariant());
            output.Attributes.SetAttribute("data-duplicate-check", DuplicateCheck ? "1" : "0");
            output.Attributes.SetAttribute("data-continue-add", ContinueAdd ? "1" : "0");
            output.Attributes.SetAttribute("data-auto-page-close", IsAutoPageClose ? "1" : "0");
            output.Attributes.SetAttribute("data-show-apply-button", ShowApplyButton ? "1" : "0");
            output.Attributes.SetAttribute("data-always-readonly", AlwaysReadOnly ? "1" : "0");
            output.Attributes.SetAttribute("data-parent-id", ParentObjectID);
            output.Attributes.SetAttribute("data-chain-id", ChainDataFormID);
            output.Attributes.SetAttribute("data-on-load-success", OnLoadSuccess);
            output.Attributes.SetAttribute("data-on-apply", OnApply);
            output.Attributes.SetAttribute("data-on-applied", OnApplied);
            output.Attributes.SetAttribute("data-on-cancel", OnCancel);
            output.Attributes.SetAttribute("data-on-before-validate", OnBeforeValidate);
            if (!string.IsNullOrWhiteSpace(RelationColumns))
                output.Attributes.SetAttribute("data-relation-columns", RelationColumns);

            var baseClass = $"bg-white rounded-xl border border-slate-200 shadow-sm {ExtraClass}".Trim();
            output.Attributes.SetAttribute("class", baseClass);

            var alpineData = BuildAlpineData(formId, colCtx.Columns);
            output.Attributes.SetAttribute("x-data", alpineData);
            output.Attributes.SetAttribute("x-init", "init()");

            output.Content.SetHtmlContent(BuildHtml(formId, colCtx.Columns, gridCols));
        }

        private string BuildAlpineData(string formId, List<FormColumn> columns)
        {
            var fields = string.Join(",", columns
                .Where(c => !c.Hidden)
                .Select(c => $"'{JsEsc(c.FieldName)}':''"));

            return $"gDataForm('{formId}',{{{fields}}})";
        }

        private string BuildHtml(string formId, List<FormColumn> columns, int gridCols)
        {
            var sb = new StringBuilder();
            sb.Append(BuildToolbar(formId));

            sb.Append($"<div x-show=\"mode==='view'\" class=\"p-{VerticalGap}\">");
            sb.Append(BuildViewGrid(columns, gridCols));
            sb.Append("</div>");

            sb.Append(BuildModal(formId, columns, gridCols));
            sb.Append(BuildDeleteModal(formId));

            return sb.ToString();
        }

        private string BuildToolbar(string formId)
        {
            if (AlwaysReadOnly) return string.Empty;

            var defaultTools = string.IsNullOrWhiteSpace(ToolItems)
                ? "[{\"action\":\"add\",\"label\":\"新增\"},{\"action\":\"edit\",\"label\":\"修改\",\"requireSelection\":true},{\"action\":\"delete\",\"label\":\"刪除\",\"requireSelection\":true}]"
                : ToolItems;

            var sb = new StringBuilder();
            sb.Append("<div class=\"flex items-center justify-between px-4 py-2 border-b border-slate-200 bg-slate-50 rounded-t-xl\">");

            if (!string.IsNullOrWhiteSpace(Title))
                sb.Append($"<span class=\"text-sm font-bold text-slate-700\">{HtmlEnc(Title)}</span>");
            else
                sb.Append("<span></span>");

            sb.Append("<div class=\"flex gap-2\" x-data=\"{}\">");
            sb.Append($"<template x-for=\"tool in {HtmlAttr(defaultTools)}\" :key=\"tool.action\">");
            sb.Append("<button type=\"button\"");
            sb.Append(" :disabled=\"tool.requireSelection && !hasSelection\"");
            sb.Append($" @click=\"handleToolAction(tool.action, tool.onClick, '{formId}')\"");
            sb.Append(" :class=\"{'opacity-40 cursor-not-allowed': tool.requireSelection && !hasSelection}\"");
            sb.Append(" class=\"inline-flex items-center gap-1 px-3 py-1.5 text-xs font-semibold rounded-lg border border-slate-300 bg-white text-slate-700 hover:bg-blue-50 hover:text-blue-700 hover:border-blue-400 transition-colors shadow-sm\">");
            sb.Append("<span x-text=\"tool.label\"></span>");
            sb.Append("</button>");
            sb.Append("</template>");
            sb.Append("</div>");
            sb.Append("</div>");

            return sb.ToString();
        }

        private string BuildViewGrid(List<FormColumn> columns, int gridCols)
        {
            var sb = new StringBuilder();
            sb.Append($"<div class=\"grid gap-{HorizontalGap}\" style=\"grid-template-columns:repeat({gridCols},minmax(0,1fr));\">");

            foreach (var col in columns.Where(c => !c.Hidden && c.ColumnType != FormColumnType.Hidden))
            {
                var colSpan = Math.Max(1, col.ColSpan);
                var spanStyle = colSpan > 1 ? $" style=\"grid-column:span {colSpan}/span {colSpan};\"" : "";
                var alignClass = col.CaptionAlignment switch
                {
                    "center" => "text-center",
                    "right" => "text-right",
                    _ => "text-left"
                };

                sb.Append($"<div class=\"flex flex-col gap-1\"{spanStyle}>");
                sb.Append($"<label class=\"text-xs font-semibold text-slate-500 {alignClass}\">{HtmlEnc(col.Caption)}</label>");
                sb.Append($"<div class=\"min-h-9 px-3 py-2 rounded-lg border border-slate-200 bg-slate-50 text-sm text-slate-700 break-all\"");
                sb.Append($" x-text=\"(formData && formData['{JsEsc(col.FieldName)}'] != null) ? formData['{JsEsc(col.FieldName)}'] : ''\"></div>");
                sb.Append("</div>");
            }

            sb.Append("</div>");
            return sb.ToString();
        }

        private string BuildModal(string formId, List<FormColumn> columns, int gridCols)
        {
            var modalId = $"{formId}_modal";
            var sb = new StringBuilder();

            sb.Append($"<div id=\"{modalId}\" tabindex=\"-1\" aria-hidden=\"true\"");
            sb.Append(" class=\"hidden overflow-y-auto overflow-x-hidden fixed top-0 right-0 left-0 z-50 justify-center items-center w-full md:inset-0 h-[calc(100%-1rem)] max-h-full\">");
            sb.Append("<div class=\"relative p-4 w-full max-w-2xl max-h-full\">");
            sb.Append("<div class=\"relative bg-white rounded-xl shadow-xl border border-slate-200\">");

            sb.Append("<div class=\"flex items-center justify-between p-4 border-b border-slate-200 rounded-t-xl bg-blue-600\">");
            sb.Append($"<h3 class=\"text-sm font-bold text-white\" x-text=\"mode==='add' ? '新增 - {HtmlEnc(Title)}' : '修改 - {HtmlEnc(Title)}'\"></h3>");
            sb.Append($"<button type=\"button\" @click=\"closeModal('{modalId}')\"");
            sb.Append(" class=\"text-white hover:bg-blue-700 rounded-lg p-1 transition-colors\">");
            sb.Append("<svg class=\"w-4 h-4\" fill=\"none\" stroke=\"currentColor\" viewBox=\"0 0 24 24\"><path stroke-linecap=\"round\" stroke-linejoin=\"round\" stroke-width=\"2\" d=\"M6 18L18 6M6 6l12 12\"/></svg>");
            sb.Append("</button>");
            sb.Append("</div>");

            sb.Append("<div class=\"p-4\">");
            sb.Append($"<div class=\"grid gap-{HorizontalGap}\" style=\"grid-template-columns:repeat({gridCols},minmax(0,1fr));\">");

            foreach (var col in columns.Where(c => c.ColumnType != FormColumnType.Hidden))
            {
                if (col.Hidden) continue;
                var colSpan = Math.Max(1, col.ColSpan);
                var spanStyle = colSpan > 1 ? $" style=\"grid-column:span {colSpan}/span {colSpan};\"" : "";
                var req = col.Required ? " <span class=\"text-red-500\">*</span>" : "";
                var alignClass = (col.CaptionAlignment ?? CaptionAlignment) switch
                {
                    "center" => "text-center",
                    "right" => "text-right",
                    _ => "text-left"
                };

                sb.Append($"<div class=\"flex flex-col gap-1\"{spanStyle}>");
                sb.Append($"<label class=\"text-xs font-semibold text-slate-600 {alignClass}\">{HtmlEnc(col.Caption)}{req}</label>");
                sb.Append(BuildFormField(formId, col));

                if (ValidateStyle.Equals("Hint", StringComparison.OrdinalIgnoreCase))
                {
                    sb.Append($"<p class=\"text-xs text-red-500 hidden\" id=\"{formId}_err_{col.FieldName}\"");
                    sb.Append($" x-show=\"errors['{JsEsc(col.FieldName)}']\"");
                    sb.Append($" x-text=\"errors['{JsEsc(col.FieldName)}']\"></p>");
                }

                sb.Append("</div>");
            }

            foreach (var col in columns.Where(c => c.Hidden || c.ColumnType == FormColumnType.Hidden))
            {
                var defaultAttr = col.DefaultValue == null ? "" : $" data-field-default=\"{HtmlEnc(col.DefaultValue)}\"";
                sb.Append($"<input type=\"hidden\" name=\"{HtmlEnc(col.FieldName)}\" data-field-name=\"{HtmlEnc(col.FieldName)}\"{defaultAttr} :value=\"formData['{JsEsc(col.FieldName)}']\"/>");
            }

            sb.Append("</div>");
            sb.Append("</div>");

            sb.Append("<div class=\"flex items-center justify-end gap-3 p-4 border-t border-slate-200 bg-slate-50 rounded-b-xl\">");
            sb.Append($"<button type=\"button\" @click=\"submitForm('{formId}','{modalId}')\"");
            sb.Append(" class=\"px-5 py-2 text-sm font-semibold text-white bg-blue-600 rounded-lg hover:bg-blue-700 transition-colors shadow-sm\">");
            sb.Append("<span x-text=\"mode==='add' ? '新增' : '儲存'\"></span></button>");
            sb.Append($"<button type=\"button\" @click=\"cancelForm('{formId}','{modalId}')\"");
            sb.Append(" class=\"px-5 py-2 text-sm font-semibold text-slate-700 bg-white border border-slate-300 rounded-lg hover:bg-slate-100 transition-colors shadow-sm\">取消</button>");
            sb.Append("</div>");

            sb.Append("</div></div></div>");
            return sb.ToString();
        }

        private string BuildDeleteModal(string formId)
        {
            var delModalId = $"{formId}_del_modal";
            var sb = new StringBuilder();

            sb.Append($"<div id=\"{delModalId}\" tabindex=\"-1\" aria-hidden=\"true\"");
            sb.Append(" class=\"hidden overflow-y-auto overflow-x-hidden fixed top-0 right-0 left-0 z-50 justify-center items-center w-full md:inset-0 h-[calc(100%-1rem)] max-h-full\">");
            sb.Append("<div class=\"relative p-4 w-full max-w-md max-h-full\">");
            sb.Append("<div class=\"relative bg-white rounded-xl shadow-xl border border-slate-200 p-6 text-center\">");
            sb.Append("<svg class=\"mx-auto mb-4 text-red-400 w-12 h-12\" fill=\"none\" stroke=\"currentColor\" viewBox=\"0 0 24 24\"><path stroke-linecap=\"round\" stroke-linejoin=\"round\" stroke-width=\"1.5\" d=\"M12 9v3.75m-9.303 3.376c-.866 1.5.217 3.374 1.948 3.374h14.71c1.73 0 2.813-1.874 1.948-3.374L13.949 3.378c-.866-1.5-3.032-1.5-3.898 0L2.697 16.126zM12 15.75h.007v.008H12v-.008z\"/></svg>");
            sb.Append("<h3 class=\"mb-2 text-base font-bold text-slate-800\">確認刪除</h3>");
            sb.Append("<p class=\"mb-5 text-sm text-slate-500\">確定要刪除此筆資料嗎？此操作無法復原。</p>");
            sb.Append("<div class=\"flex justify-center gap-3\">");
            sb.Append($"<button type=\"button\" @click=\"confirmDelete('{formId}','{delModalId}')\"");
            sb.Append(" class=\"px-5 py-2 text-sm font-semibold text-white bg-red-600 rounded-lg hover:bg-red-700 transition-colors\">確認刪除</button>");
            sb.Append($"<button type=\"button\" @click=\"closeModal('{delModalId}')\"");
            sb.Append(" class=\"px-5 py-2 text-sm font-semibold text-slate-700 bg-white border border-slate-300 rounded-lg hover:bg-slate-100 transition-colors\">取消</button>");
            sb.Append("</div>");
            sb.Append("</div></div></div>");

            return sb.ToString();
        }

        private string BuildFormField(string formId, FormColumn col)
        {
            var f = JsEsc(col.FieldName);
            var isRo = col.AlwaysReadOnly || col.IsPrimaryKey;
            var baseInput = "px-3 py-2 border rounded-lg text-sm w-full focus:outline-none focus:ring-2 focus:ring-blue-400 focus:border-blue-400 transition-colors";
            var roAttr = isRo ? " :readonly=\"mode==='edit' || alwaysReadOnly\"" : "";
            var disAttr = isRo ? " :disabled=\"mode==='edit' || alwaysReadOnly\"" : "";
            var classBinding = isRo
                ? " :class=\"(mode==='edit' || alwaysReadOnly) ? 'bg-slate-100 text-slate-500 cursor-not-allowed border-slate-200' : 'bg-white text-slate-800 border-slate-300'\""
                : " :class=\"'bg-white text-slate-800 border-slate-300'\"";
            var onChange = !string.IsNullOrWhiteSpace(col.OnChange) ? $" @change=\"{HtmlAttr(col.OnChange)}($event,formData)\"" : "";
            var vModel = $" x-model=\"formData['{f}']\"";
            var validateAttrs = BuildValidateAttrs(col);

            return col.ColumnType switch
            {
                FormColumnType.Readonly => BuildReadonlyField(f),
                FormColumnType.Textarea => BuildTextareaField(f, col, baseInput, onChange, roAttr, classBinding, validateAttrs),
                FormColumnType.Select => BuildSelectField(f, col, baseInput, onChange, disAttr, classBinding, validateAttrs),
                FormColumnType.Checkbox => BuildCheckboxField(f, col, onChange, validateAttrs),
                FormColumnType.Radio => BuildRadioField(f, col, onChange, validateAttrs),
                FormColumnType.Date => $"<input type=\"date\"{vModel}{roAttr}{onChange} class=\"{baseInput}\"{classBinding}{validateAttrs}/>",
                FormColumnType.Number => BuildNumberField(f, col, baseInput, onChange, roAttr, classBinding, validateAttrs),
                FormColumnType.Lov => BuildLovField(formId, col),
                FormColumnType.Hidden => $"<input type=\"hidden\"{vModel}/>",
                _ => $"<input type=\"text\"{vModel}{roAttr}{onChange}"
                     + (col.MaxLength.HasValue ? $" maxlength=\"{col.MaxLength}\"" : "")
                     + (string.IsNullOrEmpty(col.Placeholder) ? "" : $" placeholder=\"{HtmlEnc(col.Placeholder)}\"")
                     + $" class=\"{baseInput}\"{classBinding}{validateAttrs}/>"
            };
        }

        private static string BuildReadonlyField(string f)
            => $"<div class=\"min-h-9 px-3 py-2 rounded-lg border border-slate-200 bg-slate-100 text-sm text-slate-600 break-all\""
             + $" x-text=\"formData['{f}'] ?? ''\"></div>";

        private static string BuildTextareaField(string f, FormColumn col, string baseInput, string onChange, string roAttr, string classBinding, string validateAttrs)
            => $"<textarea x-model=\"formData['{f}']\"{roAttr}{onChange}"
             + (col.MaxLength.HasValue ? $" maxlength=\"{col.MaxLength}\"" : "")
             + $" rows=\"3\" class=\"{baseInput} resize-y\"{classBinding}{validateAttrs}></textarea>";

        private static string BuildSelectField(string f, FormColumn col, string baseInput, string onChange, string disAttr, string classBinding, string validateAttrs)
        {
            if (!string.IsNullOrWhiteSpace(col.OptionsApi))
            {
                return $"<select x-model=\"formData['{f}']\"{disAttr}{onChange} class=\"{baseInput}\"{classBinding}{validateAttrs}"
                     + $" x-init=\"loadSelectOptions('{JsEsc(col.OptionsApi)}','{f}')\">"
                     + "<option value=\"\">-- 請選擇 --</option>"
                     + $"<template x-for=\"opt in (selectOptions['{f}'] || [])\" :key=\"opt.value\">"
                     + "<option :value=\"opt.value\" x-text=\"opt.label\"></option>"
                     + "</template></select>";
            }

            var sb = new StringBuilder();
            sb.Append($"<select x-model=\"formData['{f}']\"{disAttr}{onChange} class=\"{baseInput}\"{classBinding}{validateAttrs}>");
            sb.Append("<option value=\"\">-- 請選擇 --</option>");

            if (!string.IsNullOrWhiteSpace(col.Options))
            {
                foreach (var opt in col.Options.Split(',', StringSplitOptions.RemoveEmptyEntries))
                {
                    var parts = opt.Split(':', 2);
                    var val = HtmlEnc(parts[0].Trim());
                    var lbl = parts.Length > 1 ? HtmlEnc(parts[1].Trim()) : val;
                    sb.Append($"<option value=\"{val}\">{lbl}</option>");
                }
            }

            sb.Append("</select>");
            return sb.ToString();
        }

        private static string BuildCheckboxField(string f, FormColumn col, string onChange, string validateAttrs)
            => "<div class=\"flex items-center min-h-9\">"
             + $"<input type=\"checkbox\" x-model=\"formData['{f}']\"{onChange}"
             + $" class=\"w-4 h-4 text-blue-600 border-slate-300 rounded focus:ring-blue-400\"{validateAttrs}/>"
             + "</div>";

        private static string BuildRadioField(string f, FormColumn col, string onChange, string validateAttrs)
        {
            var sb = new StringBuilder("<div class=\"flex items-center gap-4 min-h-9\">");
            if (!string.IsNullOrWhiteSpace(col.Options))
            {
                foreach (var opt in col.Options.Split(',', StringSplitOptions.RemoveEmptyEntries))
                {
                    var parts = opt.Split(':', 2);
                    var val = HtmlEnc(parts[0].Trim());
                    var lbl = parts.Length > 1 ? HtmlEnc(parts[1].Trim()) : val;
                    sb.Append("<label class=\"flex items-center gap-1 text-sm text-slate-700 cursor-pointer\">");
                    sb.Append($"<input type=\"radio\" x-model=\"formData['{f}']\" value=\"{val}\"{onChange}");
                    sb.Append($" class=\"w-4 h-4 text-blue-600 border-slate-300 focus:ring-blue-400\"{validateAttrs}/>{lbl}</label>");
                }
            }
            sb.Append("</div>");
            return sb.ToString();
        }

        private static string BuildNumberField(string f, FormColumn col, string baseInput, string onChange, string roAttr, string classBinding, string validateAttrs)
        {
            var minAttr = col.Min.HasValue ? $" min=\"{col.Min}\"" : "";
            var maxAttr = col.Max.HasValue ? $" max=\"{col.Max}\"" : "";
            return $"<input type=\"number\" x-model=\"formData['{f}']\"{roAttr}{onChange}{minAttr}{maxAttr}"
                 + $" class=\"{baseInput}\"{classBinding}{validateAttrs}/>";
        }

        private string BuildLovField(string formId, FormColumn col)
        {
            var f = JsEsc(col.FieldName);
            var displayF = JsEsc(string.IsNullOrWhiteSpace(col.LovKeyDisplay) ? col.FieldName : col.LovKeyDisplay);
            var title = JsEsc(string.IsNullOrWhiteSpace(col.LovTitle) ? col.Caption : col.LovTitle);
            var cols = JsEsc(col.LovColumns);
            var fields = JsEsc(col.LovFields);
            var api = JsEsc(col.LovApi);
            var keyVal = JsEsc(col.LovKeyValue);
            var keyDisp = JsEsc(col.LovKeyDisplay);
            var fmt = JsEsc(col.LovDisplayFormat);
            var cb = string.IsNullOrWhiteSpace(col.LovOnConfirm) ? "null" : col.LovOnConfirm;
            var inputId = $"{formId}_lov_{col.FieldName}";
            var validateAttrs = BuildValidateAttrs(col);

            var openJs = $"gLov.open({{title:'{title}',api:'{api}',columns:['{cols}'.split(',')].flat(),"
                       + $"fields:['{fields}'.split(',')].flat(),"
                       + $"map:{{'{keyVal}':'{HtmlAttr(inputId)}_hidden','{keyDisp}':'{HtmlAttr(inputId)}'}},"
                       + $"formatDisplay:{(string.IsNullOrWhiteSpace(fmt) ? "null" : $"function(d){{return `{fmt.Replace("{", "${{d.").Replace("}", "}}")}`;}}")}"
                       + $",onConfirm:{cb}}})";

            return "<div class=\"flex\">"
                 + $"<input type=\"hidden\" id=\"{inputId}_hidden\" x-model=\"formData['{f}']\"{validateAttrs}/>"
                 + $"<input type=\"text\" id=\"{inputId}\" x-model=\"formData['{displayF}']\" readonly"
                 + $" placeholder=\"{HtmlEnc(col.Placeholder.Coalesce("請選擇..."))}\""
                 + " class=\"flex-1 px-3 py-2 border border-slate-300 rounded-l-lg text-sm bg-blue-600 text-white font-bold cursor-pointer focus:outline-none\"/>"
                 + $"<button type=\"button\" onclick=\"{HtmlAttr(openJs)}\""
                 + " class=\"px-3 border border-l-0 border-slate-300 rounded-r-lg bg-slate-100 hover:bg-slate-200 text-slate-600 hover:text-blue-600 transition-colors\">"
                 + "<svg class=\"w-4 h-4\" fill=\"none\" stroke=\"currentColor\" viewBox=\"0 0 24 24\"><path stroke-linecap=\"round\" stroke-linejoin=\"round\" stroke-width=\"2\" d=\"M21 21l-6-6m2-5a7 7 0 11-14 0 7 7 0 0114 0z\"/></svg>"
                 + "</button>"
                 + "</div>";
        }

        private static string BuildValidateAttrs(FormColumn col)
        {
            var required = col.Required ? "1" : "0";
            return $" data-validate=\"1\" data-field-name=\"{HtmlEnc(col.FieldName)}\" data-caption=\"{HtmlEnc(col.Caption)}\""
                 + $" data-required=\"{required}\" data-validate-fn=\"{HtmlEnc(col.ValidateFn)}\" data-validate-msg=\"{HtmlEnc(col.ValidateMessage)}\""
                 + (col.IsPrimaryKey ? " data-is-pk=\"1\"" : "")
                 + (col.DefaultValue == null ? "" : $" data-field-default=\"{HtmlEnc(col.DefaultValue)}\"");
        }

        private void EnsureRuntimeInjected(TagHelperOutput output)
        {
            var ctx = ViewContext?.HttpContext;
            if (ctx == null) return;
            if (ctx.Items.ContainsKey(RuntimeInjectedKey)) return;
            ctx.Items[RuntimeInjectedKey] = true;
            output.PostElement.AppendHtml("<script src=\"/js/g-dataform.js\"></script>");
        }

        private static string JsEsc(string s) => (s ?? "").Replace("\\", "\\\\").Replace("'", "\\'");
        private static string HtmlEnc(string s) => System.Net.WebUtility.HtmlEncode(s ?? "");
        private static string HtmlAttr(string s) => (s ?? "").Replace("\"", "&quot;");
    }

    public class FormColumnContext
    {
        public List<FormColumn> Columns { get; } = new();
    }
}
