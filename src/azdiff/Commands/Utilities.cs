using System.Text;
using System.Text.RegularExpressions;
using DiffPlex.DiffBuilder.Model;

namespace azdiff;

class Utilities
{
    public static string ReplaceText(string input, ReplaceTextTarget target, IEnumerable<ReplaceText> replacements)
    {
        foreach (var (_, replacementInput, replacement)
            in replacements.Where(item => (item.Target & target) == target))
        {
            input = Regex.Replace(input, replacementInput, replacement, RegexOptions.IgnoreCase);
        }

        return input;
    }

    public static string MakeDiffResult(DiffPaneModel diff)
    {
        var content = new StringBuilder();
        foreach (var line in diff.Lines)
        {
            var prefix = line.Type switch
            {
                ChangeType.Inserted => "+ ",
                ChangeType.Deleted => "- ",
                _ => "  "
            };
            content.AppendLine($"{prefix}{line.Text}");
        }

        return content.ToString();
    }

    public static string MakeFileNameSafe(string fileName)
    {
        return Path.GetInvalidFileNameChars()
            .Aggregate(fileName, (current, c) => current.Replace(c, '_'));
    }
}