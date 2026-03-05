using Microsoft.AspNetCore.Razor.TagHelpers; namespace Web_EIP_Csharp.Views.Components
{ [HtmlTargetElement("g-control-designer")] public class GControlDesignerTagHelper : GLegacyPlaceholderTagHelperBase { protected override string DefaultTitle => "ControlDesigner"; }
} 
