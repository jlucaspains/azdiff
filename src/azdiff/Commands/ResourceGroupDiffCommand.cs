namespace azdiff;

class ResourceGroupDiffCommand(ArmTemplateComparer Comparer, IAzureTemplateLoader TemplateLoader) : BaseDiffCommand
{
    internal async Task<int> CompareResourceGroups(string sourceResourceGroupId, string targetResourceGroupId, DirectoryInfo outputFolder, IEnumerable<string> typesToIgnore, FileInfo? replaceStringsFile)
    {
        if (string.IsNullOrEmpty(sourceResourceGroupId))
        {
            Console.WriteLine("Source resource group does not exist.");
            return 2;
        }

        if (string.IsNullOrEmpty(targetResourceGroupId))
        {
            Console.WriteLine("Target resource group does not exist.");
            return 3;
        }

        var source = await TemplateLoader.GetArmTemplateAsync(sourceResourceGroupId);
        var target = await TemplateLoader.GetArmTemplateAsync(targetResourceGroupId);
        outputFolder.Create();

        var (replaceStrings, resultCode) = PrepareReplaceStrings(replaceStringsFile);

        if (resultCode != 0 || replaceStrings == null)
            return resultCode;

        Console.WriteLine("Starting comparison.");

        var result = Comparer.DiffArmTemplates(source, target, typesToIgnore, replaceStrings ?? []);

        Console.WriteLine("Found {0}.resource diffs. Writing to files...", result.Count());

        await WriteResultToFiles(result, outputFolder);

        return 0;
    }
}