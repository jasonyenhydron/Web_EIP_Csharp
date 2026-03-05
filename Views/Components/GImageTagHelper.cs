using Microsoft.AspNetCore.Razor.TagHelpers; namespace Web_EIP_Csharp.Views.Components
{ [HtmlTargetElement("g-image")] public class GImageTagHelper : GLegacyPlaceholderTagHelperBase { protected override string DefaultTitle => "Image"; }
} 
