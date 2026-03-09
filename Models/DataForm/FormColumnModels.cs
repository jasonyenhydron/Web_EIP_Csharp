namespace Web_EIP_Csharp.Models.DataForm
{
    public enum FormColumnType
    {
        Text,
        Number,
        Date,
        Select,
        Checkbox,
        Radio,
        Lov,
        Readonly,
        Textarea,
        Hidden
    }

    public class FormColumn
    {
        public string FieldName { get; set; } = string.Empty;
        public string Caption { get; set; } = string.Empty;
        public string CaptionAlignment { get; set; } = "left";
        public FormColumnType ColumnType { get; set; } = FormColumnType.Text;

        public int ColSpan { get; set; } = 1;
        public bool NewLine { get; set; } = false;

        public bool AlwaysReadOnly { get; set; } = false;
        public bool Required { get; set; } = false;
        public bool IsPrimaryKey { get; set; } = false;
        public bool Hidden { get; set; } = false;

        public string? DefaultValue { get; set; }
        public string Placeholder { get; set; } = string.Empty;
        public int? MaxLength { get; set; }
        public decimal? Min { get; set; }
        public decimal? Max { get; set; }

        public string Options { get; set; } = string.Empty;
        public string OptionsApi { get; set; } = string.Empty;

        public string LovTitle { get; set; } = string.Empty;
        public string LovApi { get; set; } = string.Empty;
        public string LovColumns { get; set; } = string.Empty;
        public string LovFields { get; set; } = string.Empty;
        public string LovKeyValue { get; set; } = string.Empty;
        public string LovKeyDisplay { get; set; } = string.Empty;
        public string LovDisplayFormat { get; set; } = string.Empty;
        public string LovOnConfirm { get; set; } = string.Empty;

        public string ValidateFn { get; set; } = string.Empty;
        public string ValidateMessage { get; set; } = string.Empty;

        public string OnChange { get; set; } = string.Empty;
    }

    public class FormToolItem
    {
        public string Action { get; set; } = string.Empty;
        public string Label { get; set; } = string.Empty;
        public string Icon { get; set; } = string.Empty;
        public string CssClass { get; set; } = string.Empty;
        public string OnClick { get; set; } = string.Empty;
        public bool RequireSelection { get; set; } = false;
    }

    public class RelationColumn
    {
        public string MasterField { get; set; } = string.Empty;
        public string DetailField { get; set; } = string.Empty;
    }
}
