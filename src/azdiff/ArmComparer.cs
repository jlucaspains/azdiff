using System.Text;
using System.Text.RegularExpressions;
using DiffPlex.DiffBuilder;
using DiffPlex.DiffBuilder.Model;
using Newtonsoft.Json;

class ArmComparer
{
    public static IEnumerable<DiffResult> DiffArmTemplates(string source, string target, IEnumerable<string> typesToIgnore, IEnumerable<ReplaceText> replaceStringsFile)
    {
        var result = new List<DiffResult>();

        var resourcesRight = TryDeserializeArmJson(target);
        var itemsRight = GetArmResourceDictionary(resourcesRight, typesToIgnore, replaceStringsFile);
        var resourcesLeft = TryDeserializeArmJson(source);
        var itemsLeft = GetArmResourceDictionary(resourcesLeft, typesToIgnore, replaceStringsFile);

        foreach (var (leftKey, left) in itemsLeft)
        {
            left.TryGetValue("name", out var cleanNameToken);
            var cleanName= cleanNameToken?.ToString() ?? string.Empty;

            if (itemsRight.TryGetValue(leftKey, out Newtonsoft.Json.Linq.JObject? value))
            {
                var right = value;

                right.Remove("tags");
                left.Remove("tags");

                var rightJson = JsonConvert.SerializeObject(right, Formatting.Indented);
                var leftJson = JsonConvert.SerializeObject(left, Formatting.Indented);

                leftJson = ReplaceText(leftJson, ReplaceTextTarget.Body, replaceStringsFile);
                rightJson = ReplaceText(rightJson, ReplaceTextTarget.Body, replaceStringsFile);

                var diff = InlineDiffBuilder.Diff(leftJson, rightJson, true, true);

                if (diff.HasDifferences)
                    result.Add(new DiffResult(DiffType.Diff, MakeFileNameSafe(cleanName), MakeDiffResult(diff)));

                itemsRight.Remove(leftKey);
            }
            else
            {
                var leftJson = JsonConvert.SerializeObject(left, Formatting.Indented);
                result.Add(new DiffResult(DiffType.MissingOnTarget, MakeFileNameSafe(cleanName), leftJson));
            }
        }

        foreach (var (_, item) in itemsRight)
        {
            item.TryGetValue("name", out var cleanNameToken);
            var cleanName = cleanNameToken?.ToString() ?? string.Empty;
            var rightJson = JsonConvert.SerializeObject(item, Formatting.Indented);

            result.Add(new DiffResult(DiffType.ExtraOnTarget, MakeFileNameSafe(cleanName), rightJson));
        }

        return result;
    }

    static string MakeDiffResult(DiffPaneModel diff)
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

    static string MakeFileNameSafe(string fileName)
    {
        return Path.GetInvalidFileNameChars()
            .Aggregate(fileName, (current, c) => current.Replace(c, '_'));
    }

    static string ReplaceText(string input, ReplaceTextTarget target, IEnumerable<ReplaceText> replacements)
    {
        foreach (var (_, replacementInput, replacement)
            in replacements.Where(item => (item.Target & target) == target))
        {
            input = Regex.Replace(input, replacementInput, replacement, RegexOptions.IgnoreCase);
        }

        return input;
    }

    static List<Newtonsoft.Json.Linq.JObject> TryDeserializeArmJson(string json)
    {
        try
        {
            var anonymous = new { resources = new List<Newtonsoft.Json.Linq.JObject>() };

            var result = JsonConvert.DeserializeAnonymousType(json, anonymous);

            return result?.resources ?? [];
        }
        catch
        {
            return [];
        }
    }

    static Dictionary<string, Newtonsoft.Json.Linq.JObject> GetArmResourceDictionary(IEnumerable<Newtonsoft.Json.Linq.JObject> resources, IEnumerable<string> typesToIgnore, IEnumerable<ReplaceText> replaceStrings)
    {
        var result = new Dictionary<string, Newtonsoft.Json.Linq.JObject>();
        foreach (var item in resources)
        {
            item.TryGetValue("type", out var type);
            item.TryGetValue("name", out var name);

            if (typesToIgnore.Contains(type?.ToString()))
                continue;

            var cleanName = ReplaceText(name?.ToString() ?? string.Empty, ReplaceTextTarget.Name, replaceStrings);
            name?.Replace(cleanName);
            result.Add($"{type}/{cleanName}", item);
        }

        return result;
    }
}