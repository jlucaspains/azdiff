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

        var sourceResourceGroupId = new Option<string>(
                    name: "--sourceResourceGroupId",
                    description: "The comparison source json file.")
        { IsRequired = true };

        var targetResourceGroupId = new Option<string>(
                    name: "--targetResourceGroupId",
                    description: "The comparison target Resource Group.")
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

        var authenticationMethod = new Option<CredentialType>(
                    name: "--authenticationMethod",
                    description: "Replacement strings file.");

        armCommand.AddOption(sourceFileOption);
        armCommand.AddOption(targetFileOption);
        armCommand.AddOption(outputFolderOption);
        armCommand.AddOption(ignoreTypeOption);
        armCommand.AddOption(replaceStringsFileOption);

        rgCommand.AddOption(sourceResourceGroupId);
        rgCommand.AddOption(targetResourceGroupId);
        rgCommand.AddOption(outputFolderOption);
        rgCommand.AddOption(ignoreTypeOption);
        rgCommand.AddOption(replaceStringsFileOption);
        rgCommand.AddOption(authenticationMethod);

        armCommand.SetHandler(CompareArmTemplateFiles,
                    sourceFileOption, targetFileOption, outputFolderOption, ignoreTypeOption, replaceStringsFileOption);

        rgCommand.SetHandler(CompareResourceGroups,
                    sourceResourceGroupId, targetResourceGroupId, outputFolderOption, ignoreTypeOption, replaceStringsFileOption, authenticationMethod);

        return await rootCommand.InvokeAsync(args);
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