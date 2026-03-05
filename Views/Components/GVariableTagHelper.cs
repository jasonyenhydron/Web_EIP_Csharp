using Microsoft.AspNetCore.Razor.TagHelpers; namespace Web_EIP_Csharp.Views.Components
{ [HtmlTargetElement("g-variable")] public class GVariableTagHelper : GLegacyPlaceholderTagHelperBase { protected override string DefaultTitle => "Variable"; }
} 
