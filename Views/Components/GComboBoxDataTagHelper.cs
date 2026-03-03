using System.Text;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Razor.TagHelpers;
using Oracle.ManagedDataAccess.Client;
using Web_EIP_Csharp.Helpers;

namespace Web_EIP_Csharp.Views.Components
{
    [HtmlTargetElement("g-combobox")]
    public class GComboBoxDataTagHelper : TagHelper
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public GComboBoxDataTagHelper(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public string Id { get; set; } = "";
        public string Name { get; set; } = "";
        public string Label { get; set; } = "";
        public string Value { get; set; } = "";
        public string Placeholder { get; set; } = "全部";
        public string Items { get; set; } = ""; // value:text,value:text
        public string Sql { get; set; } = "";   // SELECT value,text FROM ...
        public string ValueField { get; set; } = "";
        public string TextField { get; set; } = "";
        [HtmlAttributeName("x-model")]
        public string AlpineModel { get; set; } = "";
        public bool Required { get; set; } = false;
        public bool Disabled { get; set; } = false;
        public int ColSpan { get; set; } = 1;
        public string Class { get; set; } = "";
        public string InputClass { get; set; } = "block w-20 px-2.5 py-1.5 border border-slate-300 rounded-lg text-sm focus:ring-blue-500 focus:border-blue-500";
        public string Onchange { get; set; } = "";

        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            var inputId = string.IsNullOrWhiteSpace(Id) ? $"gcb_{Guid.NewGuid():N}" : Id;
            var colClass = ColSpan switch { 2 => "col-span-2", 3 => "col-span-3", 4 => "col-span-4", _ => "col-span-1" };
            var requiredMark = Required ? @"<span class=""text-red-500 ml-0.5 font-bold"">*</span>" : "";
            var disAttr = Disabled ? " disabled" : "";
            var reqAttr = Required ? " required" : "";
            var xmodel = string.IsNullOrWhiteSpace(AlpineModel) ? "" : $@" x-model=""{HtmlEncoder.Default.Encode(AlpineModel)}""";
            var onchange = string.IsNullOrWhiteSpace(Onchange) ? "" : $@" onchange=""{HtmlEncoder.Default.Encode(Onchange)}""";

            var optionHtml = new StringBuilder();
            optionHtml.Append($@"<option value="""">{HtmlEncoder.Default.Encode(Placeholder)}</option>");
            AppendItemsOptions(optionHtml);
            AppendSqlOptions(optionHtml);

            var labelHtml = string.IsNullOrWhiteSpace(Label)
                ? ""
                : $@"<label for=""{inputId}"" class=""text-xs font-bold text-slate-600"">{HtmlEncoder.Default.Encode(Label)}{requiredMark}</label>";

            output.TagName = "div";
            output.Attributes.SetAttribute("class", $"flex flex-col gap-1 {colClass} {Class}".Trim());
            output.Content.SetHtmlContent($@"
                {labelHtml}
                <select id=""{inputId}"" name=""{Name}"" class=""{InputClass}""{disAttr}{reqAttr}{xmodel}{onchange}>
                    {optionHtml}
                </select>
            ");
        }

        private void AppendItemsOptions(StringBuilder optionHtml)
        {
            if (string.IsNullOrWhiteSpace(Items)) return;

            foreach (var item in Items.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
            {
                var parts = item.Split(':', 2, StringSplitOptions.TrimEntries);
                var val = parts[0];
                var text = parts.Length > 1 ? parts[1] : val;
                var selected = string.Equals(val, Value, StringComparison.OrdinalIgnoreCase) ? " selected" : "";
                optionHtml.Append($@"<option value=""{HtmlEncoder.Default.Encode(val)}""{selected}>{HtmlEncoder.Default.Encode(text)}</option>");
            }
        }

        private void AppendSqlOptions(StringBuilder optionHtml)
        {
            if (string.IsNullOrWhiteSpace(Sql)) return;

            try
            {
                var ctx = _httpContextAccessor.HttpContext;
                var username = ctx?.Session.GetString("username");
                var password = ctx?.Session.GetString("password");
                var tns = ctx?.Session.GetString("tns");
                if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password) || string.IsNullOrWhiteSpace(tns))
                {
                    return;
                }

                using var connection = OracleDbHelper.GetConnection(username, password, tns);
                connection.Open();

                using var command = connection.CreateCommand();
                command.CommandText = Sql;
                command.BindByName = true;

                using var reader = command.ExecuteReader();
                while (reader.Read())
                {
                    var rawValue = ResolveColumn(reader, ValueField, 0);
                    var rawText = ResolveColumn(reader, TextField, 1);
                    var val = rawValue?.ToString() ?? "";
                    var text = rawText?.ToString() ?? val;
                    var selected = string.Equals(val, Value, StringComparison.OrdinalIgnoreCase) ? " selected" : "";
                    optionHtml.Append($@"<option value=""{HtmlEncoder.Default.Encode(val)}""{selected}>{HtmlEncoder.Default.Encode(text)}</option>");
                }
            }
            catch
            {
                // ignore SQL load failure to keep page rendering
            }
        }

        private static object ResolveColumn(OracleDataReader reader, string fieldName, int fallbackIndex)
        {
            if (!string.IsNullOrWhiteSpace(fieldName))
            {
                try { return reader[fieldName]; } catch { }
            }

            if (reader.FieldCount > fallbackIndex) return reader.GetValue(fallbackIndex);
            if (reader.FieldCount > 0) return reader.GetValue(0);
            return "";
        }
    }
}

