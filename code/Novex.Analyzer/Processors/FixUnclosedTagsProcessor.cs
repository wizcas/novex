using System.Text;
using System.Text.RegularExpressions;

namespace Novex.Analyzer.Processors;

/// <summary>
/// A processor to fix unclosed XML/HTML tags by appending missing closing tags.
/// </summary>
public class FixUnclosedTagsProcessor : ITransformationProcessor
{
    private static readonly Regex TagRegex = new Regex(@"<(/?)(\w+)([^>]*?)/?>", RegexOptions.Compiled);

    public Task<string> ProcessAsync(string input, Dictionary<string, object> parameters)
    {
        if (string.IsNullOrEmpty(input))
        {
            return Task.FromResult(string.Empty);
        }

        var onlyForEndingTags = GetBooleanParameter(parameters, "OnlyForEndingTags");

        if (onlyForEndingTags)
        {
            return FixOrphanedClosingTags(input);
        }
        else
        {
            return FixOrphanedOpeningTags(input);
        }
    }

    private static Task<string> FixOrphanedOpeningTags(string input)
    {
        var openTags = new Stack<string>();
        foreach (Match match in TagRegex.Matches(input))
        {
            if (match.Value.EndsWith("/>")) continue;

            var tagName = match.Groups[2].Value;
            var isClosingTag = !string.IsNullOrEmpty(match.Groups[1].Value);

            if (isClosingTag)
            {
                if (openTags.Contains(tagName, StringComparer.OrdinalIgnoreCase))
                {
                    // Pop until we find the matching opening tag, closing any tags in between implicitly.
                    while (openTags.Count > 0)
                    {
                        var openTag = openTags.Pop();
                        if (openTag.Equals(tagName, StringComparison.OrdinalIgnoreCase))
                        {
                            break; // Found the match
                        }
                    }
                }
                // If the closing tag has no corresponding open tag in the stack, ignore it.
            }
            else
            {
                openTags.Push(tagName);
            }
        }

        if (openTags.Count == 0) return Task.FromResult(input);

        var resultBuilder = new StringBuilder(input);
        while (openTags.Count > 0)
        {
            resultBuilder.Append($"</{openTags.Pop()}>");
        }
        return Task.FromResult(resultBuilder.ToString());
    }

    private static Task<string> FixOrphanedClosingTags(string input)
    {
        var openTags = new Stack<string>();
        var prependedTags = new List<string>();

        foreach (Match match in TagRegex.Matches(input))
        {
            if (match.Value.EndsWith("/>")) continue;

            var tagName = match.Groups[2].Value;
            var isClosingTag = !string.IsNullOrEmpty(match.Groups[1].Value);

            if (isClosingTag)
            {
                if (openTags.Count > 0 && openTags.Peek().Equals(tagName, StringComparison.OrdinalIgnoreCase))
                {
                    openTags.Pop();
                }
                else
                {
                    prependedTags.Add(tagName);
                }
            }
            else
            {
                openTags.Push(tagName);
            }
        }

        if (prependedTags.Count == 0) return Task.FromResult(input);

        var resultBuilder = new StringBuilder();
        // Prepend the necessary opening tags in reverse order of finding the closing ones
        for (int i = prependedTags.Count - 1; i >= 0; i--)
        {
            resultBuilder.Append($"<{prependedTags[i]}>");
        }
        resultBuilder.Append(input);

        return Task.FromResult(resultBuilder.ToString());
    }

    private bool GetBooleanParameter(Dictionary<string, object> parameters, string key)
    {
        if (!parameters.TryGetValue(key, out var value)) return false;
        if (value is bool boolValue) return boolValue;
        if (value is string stringValue) return string.Equals(stringValue, "true", StringComparison.OrdinalIgnoreCase);
        return false;
    }
}
