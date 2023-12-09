namespace azdiff;

class JsonFileDiffCommand(ArmTemplateComparer Comparer) : BaseDiffCommand
{
    internal async Task<int> CompareJsonFiles(FileInfo source, FileInfo target, DirectoryInfo outputFolder, IEnumerable<string> typesToIgnore, FileInfo? replaceStringsFile)
    {
        if (!source.Exists)
        {
            Console.WriteLine("Source file does not exist.");
            return 2;
        }

        if (!target.Exists)
        {
            Console.WriteLine("Target file does not exist.");
            return 3;
        }

        var targetContents = await File.ReadAllTextAsync(target.FullName);
        var sourceContents = await File.ReadAllTextAsync(source.FullName);
        outputFolder.Create();

        var (replaceStrings, resultCode) = PrepareReplaceStrings(replaceStringsFile);

        if (resultCode != 0 || replaceStrings == null)
            return resultCode;

        var result = Comparer.DiffArmTemplates(sourceContents, targetContents, typesToIgnore, replaceStrings ?? []);

        await WriteResultToFiles(result, outputFolder);

        return 0;
    }
}