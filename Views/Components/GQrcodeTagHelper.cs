using Microsoft.AspNetCore.Razor.TagHelpers; namespace Web_EIP_Csharp.Views.Components
{ [HtmlTargetElement("g-qrcode")] public class GQrcodeTagHelper : GLegacyPlaceholderTagHelperBase { protected override string DefaultTitle => "Qrcode"; }
} 
