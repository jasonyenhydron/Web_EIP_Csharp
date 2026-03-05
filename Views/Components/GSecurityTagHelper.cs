using Microsoft.AspNetCore.Razor.TagHelpers; namespace Web_EIP_Csharp.Views.Components
{ [HtmlTargetElement("g-security")] public class GSecurityTagHelper : GLegacyPlaceholderTagHelperBase { protected override string DefaultTitle => "Security"; }
} 
