using Microsoft.AspNetCore.Razor.TagHelpers; namespace Web_EIP_Csharp.Views.Components
{ [HtmlTargetElement("g-schedule")] public class GScheduleTagHelper : GLegacyPlaceholderTagHelperBase { protected override string DefaultTitle => "Schedule"; }
} 
