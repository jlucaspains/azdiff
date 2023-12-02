using Newtonsoft.Json;
using System.CommandLine;

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

static async Task CompareArmTemplateFiles(FileInfo source, FileInfo target, DirectoryInfo outputFolder, IEnumerable<string> typesToIgnore, FileInfo? replaceStringsFile)
{
    if (!source.Exists)
    {
        Console.WriteLine("Source file does not exist.");
        return;
    }

    if (!target.Exists)
    {
        Console.WriteLine("Target file does not exist.");
        return;
    }

    var targetContents = await File.ReadAllTextAsync(target.FullName);
    var sourceContents = await File.ReadAllTextAsync(source.FullName);
    outputFolder.Create();

    List<ReplaceText>? replaceStrings = null;

    if (replaceStringsFile != null)
        replaceStrings = GetReplaceStrings(replaceStringsFile);

    var result = ArmComparer.DiffArmTemplates(sourceContents, targetContents, typesToIgnore, replaceStrings ?? []);

    await WriteResultToFiles(result, outputFolder);
}

static List<ReplaceText> GetReplaceStrings(FileInfo replaceStringsFile)
{
    var replaceStringsContent = File.ReadAllText(replaceStringsFile.FullName);

    return JsonConvert.DeserializeObject<List<ReplaceText>>(replaceStringsContent)
        ?? [];
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