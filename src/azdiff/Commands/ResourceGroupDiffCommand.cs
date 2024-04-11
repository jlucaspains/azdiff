namespace azdiff;

class ResourceGroupDiffCommand(ArmTemplateComparer Comparer, IAzureTemplateLoader TemplateLoader) : BaseDiffCommand
{
    internal async Task<int> CompareResourceGroups(string sourceResourceGroupId, string targetResourceGroupId, DirectoryInfo outputFolder, 
        IEnumerable<string> typesToIgnore, FileInfo? replaceStringsFile, CredentialType credentialType)
    {
        if (string.IsNullOrEmpty(sourceResourceGroupId))
        {
            Utilities.WriteError("Source resource group id is invalid.");
            return 2;
        }

        if (string.IsNullOrEmpty(targetResourceGroupId))
        {
            Utilities.WriteError("Target resource group id is invalid.");
            return 3;
        }

        var (source, sourceResult) = await TemplateLoader.GetArmTemplateAsync(sourceResourceGroupId, credentialType);

        if (sourceResult != 0)
            return sourceResult;

        var (target, targetResult) = await TemplateLoader.GetArmTemplateAsync(targetResourceGroupId, credentialType);

        if (targetResult != 0)
            return targetResult;

        outputFolder.Create();

        var (replaceStrings, resultCode) = PrepareReplaceStrings(replaceStringsFile);

        if (resultCode != 0 || replaceStrings == null)
            return resultCode;

        Utilities.WriteInformation("Starting comparison...");

        var result = Comparer.DiffArmTemplates(source, target, typesToIgnore, replaceStrings ?? []);

        Utilities.WriteInformation("Found {0} resource differences. Writing to files...", result.Count());

        await WriteResultToFiles(result, outputFolder);

        Utilities.WriteSuccess("Diff is complete.");

        return 0;
    }
}