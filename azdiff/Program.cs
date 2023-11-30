using Azure.ResourceManager.Resources.Models;
using Azure.Core;
using Azure.ResourceManager;
using Azure.Identity;
using Newtonsoft.Json;
using DiffPlex.DiffBuilder;
using DiffPlex.DiffBuilder.Model;
using System.Text;
using System.Text.RegularExpressions;

Console.WriteLine("Starting compares");

string[] typesToIgnore = [
    "microsoft.insights/components/ProactiveDetectionConfigs",
    "Microsoft.OperationalInsights/workspaces/tables",
    "Microsoft.OperationalInsights/workspaces/savedSearches",
    "Microsoft.Web/sites/snapshots",
    "Microsoft.Sql/servers/databases/advisors",
    "Microsoft.Cache/Redis/privateEndpointConnections"
];

(string input, string replacement)[] replacements = [
    ("ststest.synqor.com", "stsenv.synqor.com"),
    ("sts.synqor.com", "stsenv.synqor.com"),
    ("e0b31020-b98a-4fc4-9a5a-a5d972f8ab2b", "sub-id"),
    ("169a845e-1a84-48b8-b62d-4afd521350fe", "sub-id"),
    ("-dev", "-env"),
    ("-test", "-env"),
    ("-prod", "-env"),
    ("proudfield-a0abe991.eastus.azurecontainerapps.io", "envapp.eastus.azurecontainerapps.io"),
    ("politefield-d61e7a17.eastus.azurecontainerapps.io", "envapp.eastus.azurecontainerapps.io")
];

var anonymous = new { resources = new List<Newtonsoft.Json.Linq.JObject>() };

// var resourcesRight = await GetResourceGroupResources("/subscriptions/e0b31020-b98a-4fc4-9a5a-a5d972f8ab2b/resourceGroups/rg-eastus-sts-test", new CancellationToken());
// var resultRight = JsonConvert.DeserializeAnonymousType(resourcesRight.Template.ToString(), anonymous);

var resourcesRight = await File.ReadAllTextAsync("prod.json");
var resultRight = JsonConvert.DeserializeAnonymousType(resourcesRight, anonymous);

if (resultRight == null)
{
    return;
}

var itemsRight = new Dictionary<string, Newtonsoft.Json.Linq.JObject>();
foreach (var item in resultRight.resources)
{
    item.TryGetValue("type", out var type);
    item.TryGetValue("name", out var name);

    if (typesToIgnore.Contains(type?.ToString()))
    {
        continue;
    }

    var cleanName = ReplaceEnvironmentInName(name?.ToString() ?? string.Empty);
    name?.Replace(cleanName);
    itemsRight.Add($"{type}/{cleanName}", item);
}

// var resourcesLeft = await GetResourceGroupResources("/subscriptions/8d7d08cf-e17f-448d-9c2d-ab5eb8dd65fe/resourceGroups/rg-eastus-sts-dev", new CancellationToken());
// var resultLeft = JsonConvert.DeserializeAnonymousType(resourcesLeft.Template.ToString(), anonymous);

var resourcesLeft = await File.ReadAllTextAsync("test.json");
var resultLeft = JsonConvert.DeserializeAnonymousType(resourcesLeft, anonymous);

if (resultLeft == null)
{
    return;
}

foreach (var item in resultLeft.resources)
{
    item.TryGetValue("type", out var type);
    item.TryGetValue("name", out var name);

    if (typesToIgnore.Contains(type?.ToString()))
    {
        continue;
    }

    var cleanName = ReplaceEnvironmentInName(name?.ToString() ?? string.Empty);

    name?.Replace(cleanName);

    var key = $"{type}/{cleanName}";

    if (itemsRight.TryGetValue(key, out Newtonsoft.Json.Linq.JObject? value))
    {
        var right = value;
        var left = item;

        right.Remove("tags");
        left.Remove("tags");

        var rightJson = JsonConvert.SerializeObject(right, Formatting.Indented);
        var leftJson = JsonConvert.SerializeObject(item, Formatting.Indented);

        leftJson = ReplaceEnvironmentInBody(leftJson);
        rightJson = ReplaceEnvironmentInBody(rightJson);

        var diff = InlineDiffBuilder.Diff(leftJson, rightJson, true, true);

        if (diff.HasDifferences)
        {
            await WriteDiffToFile(diff, $"diffs/diff_{MakeFileNameSafe(cleanName)}.diff");
        }
        else
        {
            Console.WriteLine($"No diff for {key}");
        }

        itemsRight.Remove(key);
    }
    else
    {
        Console.WriteLine($"Missing in right {key}");
        var leftJson = JsonConvert.SerializeObject(item, Formatting.Indented);
        await WriteToFile(leftJson, $"diffs/missing_{MakeFileNameSafe(cleanName)}.diff");
    }
}

foreach (var (key, item) in itemsRight)
{
    var cleanName = ReplaceEnvironmentInName(key);
    var rightJson = JsonConvert.SerializeObject(item, Formatting.Indented);

    await WriteToFile(rightJson, $"diffs/extra_{MakeFileNameSafe(cleanName)}.diff");
}

async Task<ResourceGroupExportResult> GetResourceGroupResources(string id, CancellationToken cancellationToken)
{
    var armClient = new ArmClient(new AzureCliCredential());
    var rg = armClient.GetResourceGroupResource(new ResourceIdentifier(id));

    var request = new ExportTemplate
    {
        Options = "SkipAllParameterization"
    };
    request.Resources.Add("*");

    var armtemplate = await rg.ExportTemplateAsync(Azure.WaitUntil.Completed, request, cancellationToken);

    return armtemplate.Value;
}

// static void WriteDiff(DiffPaneModel diff)
// {
//     var savedColor = Console.ForegroundColor;
//     foreach (var line in diff.Lines)
//     {
//         switch (line.Type)
//         {
//             case ChangeType.Inserted:
//                 Console.ForegroundColor = ConsoleColor.Green;
//                 Console.Write("+ ");
//                 break;
//             case ChangeType.Deleted:
//                 Console.ForegroundColor = ConsoleColor.Red;
//                 Console.Write("- ");
//                 break;
//             default:
//                 Console.ForegroundColor = ConsoleColor.Gray;
//                 Console.Write("  ");
//                 break;
//         }

//         Console.WriteLine(line.Text);
//     }
//     Console.ForegroundColor = savedColor;
//     Console.ReadLine();
// }

static async Task WriteDiffToFile(DiffPaneModel diff, string fileName)
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

    await WriteToFile(content.ToString(), fileName);
}

static async Task WriteToFile(string text, string fileName)
{
    await File.WriteAllTextAsync(fileName, text);
}

static string MakeFileNameSafe(string fileName)
{
    return Path.GetInvalidFileNameChars().Aggregate(fileName, (current, c) => current.Replace(c, '_'));
}

static string ReplaceEnvironmentInName(string input)
{
    return Regex.Replace(input, @"(development|production|dev|test|prod)", "env", RegexOptions.IgnoreCase);
}

string ReplaceEnvironmentInBody(string input)
{
    foreach (var (item, replacement) in replacements)
    {
        input = Regex.Replace(input, item, replacement, RegexOptions.IgnoreCase);
    }

    return input;
}