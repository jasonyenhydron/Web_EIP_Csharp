using Microsoft.AspNetCore.Razor.TagHelpers;
using Web_EIP_Csharp.Models.DataForm;

namespace Web_EIP_Csharp.Views.Components
{
    [HtmlTargetElement("form-column", ParentTag = "g-dataform")]
    public class FormColumnTagHelper : TagHelper
    {
        [HtmlAttributeName("field-name")]
        public string FieldName { get; set; } = string.Empty;

        [HtmlAttributeName("caption")]
        public string Caption { get; set; } = string.Empty;

        [HtmlAttributeName("caption-alignment")]
        public string CaptionAlignment { get; set; } = "left";

        [HtmlAttributeName("column-type")]
        public FormColumnType ColumnType { get; set; } = FormColumnType.Text;

        [HtmlAttributeName("col-span")]
        public int ColSpan { get; set; } = 1;

        [HtmlAttributeName("new-line")]
        public bool NewLine { get; set; } = false;

        [HtmlAttributeName("always-read-only")]
        public bool AlwaysReadOnly { get; set; } = false;

        [HtmlAttributeName("required")]
        public bool Required { get; set; } = false;

        [HtmlAttributeName("is-primary-key")]
        public bool IsPrimaryKey { get; set; } = false;

        [HtmlAttributeName("hidden")]
        public bool Hidden { get; set; } = false;

        [HtmlAttributeName("default-value")]
        public string? DefaultValue { get; set; }

        [HtmlAttributeName("placeholder")]
        public string Placeholder { get; set; } = string.Empty;

        [HtmlAttributeName("max-length")]
        public int? MaxLength { get; set; }

        [HtmlAttributeName("min")]
        public decimal? Min { get; set; }

        [HtmlAttributeName("max")]
        public decimal? Max { get; set; }

        [HtmlAttributeName("options")]
        public string Options { get; set; } = string.Empty;

        [HtmlAttributeName("options-api")]
        public string OptionsApi { get; set; } = string.Empty;

        [HtmlAttributeName("lov-title")]
        public string LovTitle { get; set; } = string.Empty;

        [HtmlAttributeName("lov-api")]
        public string LovApi { get; set; } = string.Empty;

        [HtmlAttributeName("lov-columns")]
        public string LovColumns { get; set; } = string.Empty;

        [HtmlAttributeName("lov-fields")]
        public string LovFields { get; set; } = string.Empty;

        [HtmlAttributeName("lov-key-value")]
        public string LovKeyValue { get; set; } = string.Empty;

        [HtmlAttributeName("lov-key-display")]
        public string LovKeyDisplay { get; set; } = string.Empty;

        [HtmlAttributeName("lov-display-format")]
        public string LovDisplayFormat { get; set; } = string.Empty;

        [HtmlAttributeName("lov-on-confirm")]
        public string LovOnConfirm { get; set; } = string.Empty;

        [HtmlAttributeName("validate-fn")]
        public string ValidateFn { get; set; } = string.Empty;

        [HtmlAttributeName("validate-message")]
        public string ValidateMessage { get; set; } = string.Empty;

        [HtmlAttributeName("on-change")]
        public string OnChange { get; set; } = string.Empty;

        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            output.SuppressOutput();

            if (!context.Items.TryGetValue(typeof(FormColumnContext), out var ctx)) return;
            if (ctx is not FormColumnContext colCtx) return;
            if (string.IsNullOrWhiteSpace(FieldName)) return;

            colCtx.Columns.Add(new FormColumn
            {
                FieldName = FieldName,
                Caption = string.IsNullOrWhiteSpace(Caption) ? FieldName : Caption,
                CaptionAlignment = CaptionAlignment,
                ColumnType = ColumnType,
                ColSpan = ColSpan,
                NewLine = NewLine,
                AlwaysReadOnly = AlwaysReadOnly,
                Required = Required,
                IsPrimaryKey = IsPrimaryKey,
                Hidden = Hidden,
                DefaultValue = DefaultValue,
                Placeholder = Placeholder,
                MaxLength = MaxLength,
                Min = Min,
                Max = Max,
                Options = Options,
                OptionsApi = OptionsApi,
                LovTitle = LovTitle,
                LovApi = LovApi,
                LovColumns = LovColumns,
                LovFields = LovFields,
                LovKeyValue = LovKeyValue,
                LovKeyDisplay = LovKeyDisplay,
                LovDisplayFormat = LovDisplayFormat,
                LovOnConfirm = LovOnConfirm,
                ValidateFn = ValidateFn,
                ValidateMessage = ValidateMessage,
                OnChange = OnChange
            });
        }
    }
}
