namespace azdiff;

class JsonFileDiffCommand(ArmTemplateComparer Comparer) : BaseDiffCommand
{
    internal async Task<int> CompareJsonFiles(FileInfo source, FileInfo target, DirectoryInfo outputFolder, IEnumerable<string> typesToIgnore, FileInfo? replaceStringsFile)
    {
        if (!source.Exists)
        {
            Utilities.WriteError("Source file does not exist.");
            return 2;
        }

        if (!target.Exists)
        {
            Utilities.WriteError("Target file does not exist.");
            return 3;
        }

        var targetContents = await File.ReadAllTextAsync(target.FullName);
        var sourceContents = await File.ReadAllTextAsync(source.FullName);
        outputFolder.Create();

        var (replaceStrings, resultCode) = PrepareReplaceStrings(replaceStringsFile);

        if (resultCode != 0 || replaceStrings == null)
            return resultCode;

        Utilities.WriteInformation("Starting comparison...");

        var result = Comparer.DiffArmTemplates(sourceContents, targetContents, typesToIgnore, replaceStrings ?? []);

        Utilities.WriteInformation("Found {0} resource differences. Writing to files...", result.Count());

        await WriteResultToFiles(result, outputFolder);

        Utilities.WriteSuccess("Diff is complete.");

        return 0;
    }
}