using System.Text.RegularExpressions;

namespace Blog.Shared.Utils;

/// <summary>
/// Markdown 内嵌资源（本地文件或外部超链接）
/// </summary>
/// <param name="Url">资源原始地址</param>
/// <param name="FileName">本地资源文件名（外部超链接为 null）</param>
/// <param name="IsHyperLink">是否为外部超链接</param>
public sealed record MarkdownResource(string Url, string? FileName, bool IsHyperLink)
{
    /// <summary>
    /// 资源比较键：本地资源按文件名，外部资源按Url
    /// </summary>
    public string Key => IsHyperLink ? Url : FileName ?? Url;
}

/// <summary>
/// Markdown 内嵌资源解析工具
/// </summary>
public static class MarkdownResourceUtil
{
    private const string UrlPattern = @"<?(?<url>[^)\s>]+)>?";

    /// <summary>
    /// Markdown 图片：![alt](url "title")
    /// </summary>
    private static readonly Regex MarkdownImageRegex = new(
        @"(?<!\\)!\[[^\]]*\]\(\s*" + UrlPattern + @"(?:\s+(?:""[^""]*""|'[^']*'))?\s*\)",
        RegexOptions.Compiled);

    /// <summary>
    /// Markdown 链接：text(url "title")（用于内嵌的本地文件资源）
    /// </summary>
    private static readonly Regex MarkdownLinkRegex = new(
        @"(?<![!\\])\[[^\]]*\]\(\s*" + UrlPattern + @"(?:\s+(?:""[^""]*""|'[^']*'))?\s*\)",
        RegexOptions.Compiled);

    /// <summary>
    /// HTML 内嵌资源：img / video / audio / source 的 src
    /// </summary>
    private static readonly Regex HtmlResourceRegex = new(
        @"<(?:img|video|audio|source)\b[^>]*?\bsrc\s*=\s*(?:""(?<url>[^""]*)""|'(?<url>[^']*)'|(?<url>[^\s>]+))",
        RegexOptions.Compiled | RegexOptions.IgnoreCase);

    /// <summary>
    /// 提取 Markdown 中所有内嵌资源（同时包含本地资源与外部超链接），已按资源键去重
    /// </summary>
    /// <param name="content">Markdown 内容</param>
    /// <param name="resolveLocalFileName">
    /// 可选：将形如 http://host/files/Source/{userId}/{fileName} 的站内静态资源地址解析为本地文件名。
    /// 返回非空文件名时按本地资源处理，否则仍按外部超链接处理。
    /// </param>
    public static IReadOnlyList<MarkdownResource> ExtractResourceEntries(string? content,
        Func<string, string?>? resolveLocalFileName = null)
    {
        if (string.IsNullOrWhiteSpace(content))
            return Array.Empty<MarkdownResource>();

        var resources = new List<MarkdownResource>();
        var exists = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var regex in new[] { MarkdownImageRegex, MarkdownLinkRegex, HtmlResourceRegex })
        {
            foreach (Match match in regex.Matches(content))
            {
                var url = match.Groups["url"].Value.Trim();
                if (string.IsNullOrEmpty(url) || IsIgnoredResource(url))
                    continue;

                var isHyperLink = IsExternalResource(url);
                string? fileName = null;
                // 上传后的资源通过静态文件访问，虽然以 URL 形式出现，但尾部带文件名，
                // 若能被解析为真实的本地资源，则按本地资源登记而非外部超链接。
                if (isHyperLink)
                {
                    var resolvedFileName = resolveLocalFileName?.Invoke(url);
                    if (!string.IsNullOrEmpty(resolvedFileName))
                    {
                        isHyperLink = false;
                        fileName = resolvedFileName;
                    }
                }

                if (isHyperLink)
                    fileName = null;
                else
                    fileName ??= GetFileName(url);

                if (!isHyperLink && string.IsNullOrEmpty(fileName))
                    continue;

                var resource = new MarkdownResource(url, fileName, isHyperLink);
                if (exists.Add(resource.Key))
                    resources.Add(resource);
            }
        }

        return resources;
    }

    /// <summary>
    /// 提取内容中所有本地内嵌资源的地址
    /// </summary>
    /// <param name="content">Markdown 内容</param>
    /// <param name="resolveLocalFileName">可选：将站内静态资源地址解析为本地文件名</param>
    public static IReadOnlyList<string> ExtractResources(string? content,
        Func<string, string?>? resolveLocalFileName = null)
    {
        return ExtractResourceEntries(content, resolveLocalFileName)
            .Where(r => !r.IsHyperLink)
            .Select(r => r.Url)
            .ToArray();
    }

    /// <summary>
    /// 提取内容中所有本地内嵌资源的文件名（去掉路径、查询参数与锚点）
    /// </summary>
    /// <param name="content">Markdown 内容</param>
    /// <param name="resolveLocalFileName">可选：将站内静态资源地址解析为本地文件名</param>
    public static IReadOnlyList<string> ExtractResourceFileNames(string? content,
        Func<string, string?>? resolveLocalFileName = null)
    {
        return ExtractResourceEntries(content, resolveLocalFileName)
            .Where(r => !r.IsHyperLink)
            .Select(r => r.FileName!)
            .Where(name => !string.IsNullOrEmpty(name))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();
    }

    /// <summary>
    /// 提取内容中所有外部超链接资源地址
    /// </summary>
    /// <param name="content">Markdown 内容</param>
    /// <param name="resolveLocalFileName">可选：将站内静态资源地址解析为本地文件名</param>
    public static IReadOnlyList<string> ExtractHyperLinkResources(string? content,
        Func<string, string?>? resolveLocalFileName = null)
    {
        return ExtractResourceEntries(content, resolveLocalFileName)
            .Where(r => r.IsHyperLink)
            .Select(r => r.Url)
            .ToArray();
    }

    /// <summary>
    /// 比较原内容与新内容，取出新内容中已不再引用的资源
    /// </summary>
    /// <param name="originalContent">原内容</param>
    /// <param name="currentContent">新内容</param>
    /// <param name="resolveLocalFileName">可选：将站内静态资源地址解析为本地文件名</param>
    public static IReadOnlyList<MarkdownResource> ExtractRemovedResources(string? originalContent,
        string? currentContent, Func<string, string?>? resolveLocalFileName = null)
    {
        var originalResources = ExtractResourceEntries(originalContent, resolveLocalFileName);
        if (originalResources.Count == 0)
            return Array.Empty<MarkdownResource>();

        var currentKeys = new HashSet<string>(
            ExtractResourceEntries(currentContent, resolveLocalFileName).Select(r => r.Key),
            StringComparer.OrdinalIgnoreCase);
        return originalResources.Where(r => !currentKeys.Contains(r.Key)).ToArray();
    }

    /// <summary>
    /// 比较原内容与新内容，取出新内容中新增引用的资源
    /// </summary>
    /// <param name="originalContent">原内容</param>
    /// <param name="currentContent">新内容</param>
    /// <param name="resolveLocalFileName">可选：将站内静态资源地址解析为本地文件名</param>
    public static IReadOnlyList<MarkdownResource> ExtractAddedResources(string? originalContent,
        string? currentContent, Func<string, string?>? resolveLocalFileName = null)
    {
        var currentResources = ExtractResourceEntries(currentContent, resolveLocalFileName);
        if (currentResources.Count == 0)
            return Array.Empty<MarkdownResource>();

        var originalKeys = new HashSet<string>(
            ExtractResourceEntries(originalContent, resolveLocalFileName).Select(r => r.Key),
            StringComparer.OrdinalIgnoreCase);
        return currentResources.Where(r => !originalKeys.Contains(r.Key)).ToArray();
    }

    /// <summary>
    /// 是否为需要忽略的地址（锚点、data 内联、邮件、电话等，本身不是可登记的资源）
    /// </summary>
    private static bool IsIgnoredResource(string url)
    {
        return url.StartsWith("#", StringComparison.Ordinal)
               || url.StartsWith("data:", StringComparison.OrdinalIgnoreCase)
               || url.StartsWith("mailto:", StringComparison.OrdinalIgnoreCase)
               || url.StartsWith("tel:", StringComparison.OrdinalIgnoreCase)
               || url.StartsWith("javascript:", StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// 是否为外部超链接资源
    /// </summary>
    private static bool IsExternalResource(string url)
    {
        return url.StartsWith("http://", StringComparison.OrdinalIgnoreCase)
               || url.StartsWith("https://", StringComparison.OrdinalIgnoreCase)
               || url.StartsWith("//", StringComparison.Ordinal);
    }

    /// <summary>
    /// 从资源地址中取出文件名
    /// </summary>
    public static string GetFileName(string url)
    {
        var endIndex = url.IndexOfAny(new[] { '?', '#' });
        if (endIndex >= 0)
            url = url[..endIndex];
        url = url.Replace('\\', '/');
        var separatorIndex = url.LastIndexOf('/');
        if (separatorIndex >= 0)
            url = url[(separatorIndex + 1)..];
        return Uri.UnescapeDataString(url);
    }
}
