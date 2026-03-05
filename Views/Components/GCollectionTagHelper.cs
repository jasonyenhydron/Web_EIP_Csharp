using Microsoft.AspNetCore.Razor.TagHelpers; namespace Web_EIP_Csharp.Views.Components
{ [HtmlTargetElement("g-collection")] public class GCollectionTagHelper : GLegacyPlaceholderTagHelperBase { protected override string DefaultTitle => "Collection"; }
} 
