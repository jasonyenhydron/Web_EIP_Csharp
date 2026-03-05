using Microsoft.AspNetCore.Razor.TagHelpers; namespace Web_EIP_Csharp.Views.Components
{ [HtmlTargetElement("g-multi-language")] public class GMultiLanguageTagHelper : GLegacyPlaceholderTagHelperBase { protected override string DefaultTitle => "MultiLanguage"; }
} 
