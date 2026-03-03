using Microsoft.AspNetCore.Razor.TagHelpers;
using System.Text;

namespace Web_EIP_Csharp.Views.Components
{
    [HtmlTargetElement("g-js")]
    public class GJsTagHelper : TagHelper
    {
        // Profile controls required scripts. Required scripts are always included.
        public string Profile { get; set; } = "none"; // none | popup | main | mis-programs

        // Optional extra scripts, separated by comma/semicolon/newline.
        public string Extras { get; set; } = "";

        // Optional CDN Alpine include.
        public bool IncludeAlpine { get; set; } = false;
        public string AlpineVersion { get; set; } = "3.x.x";

        // Optional local script version suffix, e.g. "20260303".
        public string LocalVersion { get; set; } = "";

        // Add defer to script tags by default.
        public bool Defer { get; set; } = false;

        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            output.TagName = null;

            var scripts = new List<string>();
            scripts.AddRange(GetRequiredScripts(Profile));
            scripts.AddRange(SplitScripts(Extras));

            // Keep order and remove duplicates.
            var uniqueScripts = new List<string>();
            var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (var s in scripts)
            {
                var normalized = NormalizeUrl(s);
                if (string.IsNullOrWhiteSpace(normalized)) continue;
                if (seen.Add(normalized)) uniqueScripts.Add(normalized);
            }

            var sb = new StringBuilder();

            if (IncludeAlpine)
            {
                var alpine = $"https://cdn.jsdelivr.net/npm/alpinejs@{AlpineVersion}/dist/cdn.min.js";
                sb.AppendLine($@"<script src=""{alpine}"" defer></script>");
            }

            foreach (var script in uniqueScripts)
            {
                var src = AppendVersion(script, LocalVersion);
                var deferAttr = Defer ? " defer" : "";
                sb.AppendLine($@"<script src=""{src}""{deferAttr}></script>");
            }

            output.Content.SetHtmlContent(sb.ToString());
        }

        private static IEnumerable<string> GetRequiredScripts(string profile)
        {
            switch ((profile ?? "").Trim().ToLowerInvariant())
            {
                case "popup":
                    return new[] { "~/js/g-components.js" };
                case "main":
                    return new[] { "~/js/sidebar.js" };
                case "mis-programs":
                    return new[] { "~/js/sidebar.js", "~/js/mis_programs.js" };
                default:
                    return Array.Empty<string>();
            }
        }

        private static IEnumerable<string> SplitScripts(string raw)
        {
            if (string.IsNullOrWhiteSpace(raw)) return Array.Empty<string>();
            return raw
                .Split(new[] { ',', ';', '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        }

        private static string NormalizeUrl(string url)
        {
            if (string.IsNullOrWhiteSpace(url)) return "";
            var u = url.Trim();
            if (u.StartsWith("~/", StringComparison.Ordinal)) return "/" + u.Substring(2);
            return u;
        }

        private static string AppendVersion(string url, string version)
        {
            if (string.IsNullOrWhiteSpace(version)) return url;
            return url.Contains('?') ? $"{url}&v={version}" : $"{url}?v={version}";
        }
    }
}

