using System.Text;
using System.Text.RegularExpressions;
using DiffPlex.DiffBuilder;
using DiffPlex.DiffBuilder.Model;
using Newtonsoft.Json;

namespace azdiff;

class ArmTemplateComparer
{
    public IEnumerable<DiffResult> DiffArmTemplates(string source, string target, IEnumerable<string> typesToIgnore, IEnumerable<ReplaceText> replaceStringsFile)
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

                leftJson = Utilities.ReplaceText(leftJson, ReplaceTextTarget.Body, replaceStringsFile);
                rightJson = Utilities.ReplaceText(rightJson, ReplaceTextTarget.Body, replaceStringsFile);

                var diff = InlineDiffBuilder.Diff(leftJson, rightJson, true, true);

                if (diff.HasDifferences)
                    result.Add(new DiffResult(DiffType.Diff, Utilities.MakeFileNameSafe(cleanName), Utilities.MakeDiffResult(diff)));

                itemsRight.Remove(leftKey);
            }
            else
            {
                var leftJson = JsonConvert.SerializeObject(left, Formatting.Indented);
                result.Add(new DiffResult(DiffType.MissingOnTarget, Utilities.MakeFileNameSafe(cleanName), leftJson));
            }
        }

        foreach (var (_, item) in itemsRight)
        {
            item.TryGetValue("name", out var cleanNameToken);
            var cleanName = cleanNameToken?.ToString() ?? string.Empty;
            var rightJson = JsonConvert.SerializeObject(item, Formatting.Indented);

            result.Add(new DiffResult(DiffType.ExtraOnTarget, Utilities.MakeFileNameSafe(cleanName), rightJson));
        }

        return result;
    }

    List<Newtonsoft.Json.Linq.JObject> TryDeserializeArmJson(string json)
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

            var cleanName = Utilities.ReplaceText(name?.ToString() ?? string.Empty, ReplaceTextTarget.Name, replaceStrings);
            name?.Replace(cleanName);
            result.Add($"{type}/{cleanName}", item);
        }

        return result;
    }
}