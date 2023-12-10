namespace azdiff;

class SubscriptionDiffCommand(ArmTemplateComparer Comparer, IAzureTemplateLoader TemplateLoader) : BaseDiffCommand
{
    internal async Task<int> CompareSubscriptions(string sourceSubscriptionId, string targetSubscriptionId, DirectoryInfo outputFolder, IEnumerable<string> typesToIgnore, FileInfo? replaceStringsFile)
    {
        if (string.IsNullOrEmpty(sourceSubscriptionId))
        {
            Console.WriteLine("Source subscription id is invalid.");
            return 2;
        }

        if (string.IsNullOrEmpty(targetSubscriptionId))
        {
            Console.WriteLine("Target subscription id is invalid.");
            return 3;
        }

        var sourceRgs = await TemplateLoader.GetSubscriptionTemplatesAsync(sourceSubscriptionId);
        var targetRgs = await TemplateLoader.GetSubscriptionTemplatesAsync(targetSubscriptionId);
        outputFolder.Create();

        var (replaceStrings, resultCode) = PrepareReplaceStrings(replaceStringsFile);

        if (resultCode != 0 || replaceStrings == null)
            return resultCode;

        Console.WriteLine("Starting comparison.");

        var normalizedTargetRgs = targetRgs
            .ToDictionary(x => Utilities.ReplaceText(x.Key, ReplaceTextTarget.Name, replaceStrings ?? []), x => x.Value);

        foreach (var sourceRg in sourceRgs)
        {
            Console.WriteLine($"Comparing {sourceRg.Key}...");

            var sourceRgName = Utilities.ReplaceText(sourceRg.Key, ReplaceTextTarget.Name, replaceStrings ?? []);
            if (normalizedTargetRgs.TryGetValue(sourceRgName, out var targetRg))
            {
                var result = Comparer.DiffArmTemplates(sourceRg.Value, targetRg, typesToIgnore, replaceStrings ?? []);

                Console.WriteLine("Found {0} diffs. Writing to files...", result.Count());

                var rgDirectory = outputFolder.CreateSubdirectory(sourceRgName);

                await WriteResultToFiles(result, rgDirectory);
            }
            else
            {
                Console.WriteLine("Resource group {0} does not exist in target subscription.", sourceRg.Key);
            }
        }

        return 0;
    }
}