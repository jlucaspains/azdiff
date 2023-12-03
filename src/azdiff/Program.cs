using Newtonsoft.Json;
using System.CommandLine;

namespace azdiff;

public static class Program
{
    public static async Task<int> Main(string[] args)
    {
        var rootCommand = new RootCommand();
        var rgCommand = new Command("rg", "Compares two Azure Resource Groups");
        rootCommand.Add(rgCommand);

        var armCommand = new Command("arm", "Compare two ARM template files");
        rootCommand.Add(armCommand);

        var sourceFileOption = new Option<FileInfo>(
                    name: "--sourceFile",
                    description: "The comparison source json file. It should be an exported ARM template.")
        { IsRequired = true };

        var targetFileOption = new Option<FileInfo>(
                    name: "--targetFile",
                    description: "The comparison target json file. It should be an exported ARM template.")
        { IsRequired = true };

        var outputFolderOption = new Option<DirectoryInfo>(
                    name: "--outputFolder",
                    description: "The folder path for output.",
                    getDefaultValue: () => new DirectoryInfo("diffs"));

        var ignoreTypeOption = new Option<IEnumerable<string>>(
                    name: "--ignoreType",
                    description: "A list of types to ignore in the ARM comparison.");

        var replaceStringsFileOption = new Option<FileInfo?>(
                    name: "--replaceStringsFile",
                    description: "Replacement strings file.");

        armCommand.AddOption(sourceFileOption);
        armCommand.AddOption(targetFileOption);
        armCommand.AddOption(outputFolderOption);
        armCommand.AddOption(ignoreTypeOption);
        armCommand.AddOption(replaceStringsFileOption);

        armCommand.SetHandler(CompareArmTemplateFiles,
                    sourceFileOption, targetFileOption, outputFolderOption, ignoreTypeOption, replaceStringsFileOption);

        return await rootCommand.InvokeAsync(args);
    }

    static async Task<int> CompareArmTemplateFiles(FileInfo source, FileInfo target, DirectoryInfo outputFolder, IEnumerable<string> typesToIgnore, FileInfo? replaceStringsFile)
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

        List<ReplaceText>? replaceStrings = null;

        if (replaceStringsFile != null)
        {
            if (!replaceStringsFile.Exists)
            {
                Console.WriteLine("Replace strings file does not exist.");
                return 4;
            }

            replaceStrings = GetReplaceStrings(replaceStringsFile);

            if (replaceStrings.Count == 0)
            {
                Console.WriteLine("Replace strings file is invalid.");
                return 5;
            }
        }

        var result = ArmComparer.DiffArmTemplates(sourceContents, targetContents, typesToIgnore, replaceStrings ?? []);

        await WriteResultToFiles(result, outputFolder);

        return 0;
    }

    static List<ReplaceText> GetReplaceStrings(FileInfo replaceStringsFile)
    {
        try
        {
            var replaceStringsContent = File.ReadAllText(replaceStringsFile.FullName);

            return JsonConvert.DeserializeObject<List<ReplaceText>>(replaceStringsContent)
                ?? [];
        }
        catch
        {
            return [];
        }
    }
    static async Task WriteResultToFiles(IEnumerable<DiffResult> diffResultItems, DirectoryInfo outputFolder)
    {
        foreach (var item in diffResultItems)
        {
            var prefix = item.DiffType switch
            {
                DiffType.Diff => "diff",
                DiffType.MissingOnTarget => "new",
                DiffType.ExtraOnTarget => "extra",
                _ => throw new NotImplementedException(),
            };

            await File.WriteAllTextAsync($"diffs/{prefix}_{item.Name}.diff", item.Result);
        }
    }
}