using Microsoft.AspNetCore.Razor.TagHelpers; namespace Web_EIP_Csharp.Views.Components
{ [HtmlTargetElement("g-property-editor")] public class GPropertyEditorTagHelper : GLegacyPlaceholderTagHelperBase { protected override string DefaultTitle => "PropertyEditor"; }
} 
