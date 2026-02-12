using System.CommandLine;

namespace azdiff;

public static class Program
{
    public static int Main(string[] args)
    {
        var rootCommand = new RootCommand();
        var rgCommand = new Command("rg", "Compares two Azure Resource Groups");
        rootCommand.Add(rgCommand);

        var armCommand = new Command("arm", "Compare two ARM template files");
        rootCommand.Add(armCommand);

        var sourceFileOption = new Option<FileInfo>("--sourceFile")
        {
            Description = "The comparison source json file. It should be an exported ARM template.",
            Required = true
        };

        var targetFileOption = new Option<FileInfo>("--targetFile")
        {
            Description = "The comparison target json file. It should be an exported ARM template.",
            Required = true
        };

        var sourceResourceGroupId = new Option<string>("--sourceResourceGroupId")
        {
            Description = "The comparison source json file.",
            Required = true
        };

        var targetResourceGroupId = new Option<string>("--targetResourceGroupId")
        {
            Description = "The comparison target Resource Group.",
            Required = true
        };

        var outputFolderOption = new Option<DirectoryInfo>("--outputFolder")
        {
            Description = "The folder path for output.",
            DefaultValueFactory = _ => new DirectoryInfo("diffs")
        };

        var ignoreTypeOption = new Option<IEnumerable<string>>("--ignoreType")
        {
            Description = "A list of types to ignore in the ARM comparison."
        };

        var replaceStringsFileOption = new Option<FileInfo?>("--replaceStringsFile")
        {
            Description = "Replacement strings file."
        };

        var authenticationMethod = new Option<CredentialType>("--authenticationMethod")
        {
            Description = "Replacement strings file."
        };

        armCommand.Options.Add(sourceFileOption);
        armCommand.Options.Add(targetFileOption);
        armCommand.Options.Add(outputFolderOption);
        armCommand.Options.Add(ignoreTypeOption);
        armCommand.Options.Add(replaceStringsFileOption);

        rgCommand.Options.Add(sourceResourceGroupId);
        rgCommand.Options.Add(targetResourceGroupId);
        rgCommand.Options.Add(outputFolderOption);
        rgCommand.Options.Add(ignoreTypeOption);
        rgCommand.Options.Add(replaceStringsFileOption);
        rgCommand.Options.Add(authenticationMethod);

        armCommand.SetAction(async parseResult =>
        {
            var source = parseResult.GetValue(sourceFileOption);
            var target = parseResult.GetValue(targetFileOption);
            var outputFolder = parseResult.GetValue(outputFolderOption);
            var typesToIgnore = parseResult.GetValue(ignoreTypeOption);
            var replaceStringsFile = parseResult.GetValue(replaceStringsFileOption);
            return await CompareArmTemplateFiles(source!, target!, outputFolder!, typesToIgnore!, replaceStringsFile);
        });

        rgCommand.SetAction(async parseResult =>
        {
            var sourceRg = parseResult.GetValue(sourceResourceGroupId);
            var targetRg = parseResult.GetValue(targetResourceGroupId);
            var outputFolder = parseResult.GetValue(outputFolderOption);
            var typesToIgnore = parseResult.GetValue(ignoreTypeOption);
            var replaceStringsFile = parseResult.GetValue(replaceStringsFileOption);
            var credentialType = parseResult.GetValue(authenticationMethod);
            return await CompareResourceGroups(sourceRg!, targetRg!, outputFolder!, typesToIgnore!, replaceStringsFile, credentialType);
        });

        return rootCommand.Parse(args).Invoke();
    }

    internal static ArmTemplateComparer ArmTemplateComparer { get; set; } = new();
    internal static IAzureTemplateLoader AzureTemplateLoader { get; set; } = new AzureTemplateLoader();

    static async Task<int> CompareArmTemplateFiles(FileInfo source, FileInfo target, DirectoryInfo outputFolder, IEnumerable<string> typesToIgnore, FileInfo? replaceStringsFile)
    {
        var comparer = new JsonFileDiffCommand(ArmTemplateComparer);
        return await comparer.CompareJsonFiles(source, target, outputFolder, typesToIgnore, replaceStringsFile);
    }

    static async Task<int> CompareResourceGroups(string sourceResourceGroupId, string targetResourceGroupId, DirectoryInfo outputFolder, IEnumerable<string> typesToIgnore, FileInfo? replaceStringsFile, CredentialType credentialType)
    {
        var comparer = new ResourceGroupDiffCommand(ArmTemplateComparer, AzureTemplateLoader);
        return await comparer.CompareResourceGroups(sourceResourceGroupId, targetResourceGroupId, outputFolder, typesToIgnore, replaceStringsFile, credentialType);
    }
}