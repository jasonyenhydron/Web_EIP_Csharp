namespace Web_EIP_Csharp.Views.Components
{
    internal static class TagHelperClassResolver
    {
        /// <summary>
        /// Class priority:
        /// 1) overrideClass (if provided) -> replace default class completely
        /// 2) defaultClass (+ extraClass if provided)
        /// </summary>
        public static string Resolve(string defaultClass, string overrideClass, string extraClass = "")
        {
            if (!string.IsNullOrWhiteSpace(overrideClass))
            {
                return overrideClass.Trim();
            }

            if (string.IsNullOrWhiteSpace(extraClass))
            {
                return defaultClass?.Trim() ?? string.Empty;
            }

            return $"{defaultClass} {extraClass}".Trim();
        }
    }
}

