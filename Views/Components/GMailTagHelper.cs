using Microsoft.AspNetCore.Razor.TagHelpers; namespace Web_EIP_Csharp.Views.Components
{ [HtmlTargetElement("g-mail")] public class GMailTagHelper : GLegacyPlaceholderTagHelperBase { protected override string DefaultTitle => "Mail"; }
} 
