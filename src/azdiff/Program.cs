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

        var sbCommand = new Command("sb", "Compares two Azure Subscriptions");
        rootCommand.Add(sbCommand);

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
                    description: "The comparison source Resource Group.")
        { IsRequired = true };

        var targetResourceGroupId = new Option<string>(
                    name: "--targetResourceGroupId",
                    description: "The comparison target Resource Group.")
        { IsRequired = true };

        var sourceSubscriptionId = new Option<string>(
                    name: "--sourceSubscriptionId",
                    description: "The comparison source Subscription.")
        { IsRequired = true };

        var targetSubscriptionId = new Option<string>(
                    name: "--targetSubscriptionId",
                    description: "The comparison target Subscription.")
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

        rgCommand.AddOption(sourceResourceGroupId);
        rgCommand.AddOption(targetResourceGroupId);
        rgCommand.AddOption(outputFolderOption);
        rgCommand.AddOption(ignoreTypeOption);
        rgCommand.AddOption(replaceStringsFileOption);

        sbCommand.AddOption(sourceSubscriptionId);
        sbCommand.AddOption(targetSubscriptionId);
        sbCommand.AddOption(outputFolderOption);
        sbCommand.AddOption(ignoreTypeOption);
        sbCommand.AddOption(replaceStringsFileOption);

        armCommand.SetHandler(CompareArmTemplateFiles,
                    sourceFileOption, targetFileOption, outputFolderOption, ignoreTypeOption, replaceStringsFileOption);

        rgCommand.SetHandler(CompareResourceGroups,
                    sourceResourceGroupId, targetResourceGroupId, outputFolderOption, ignoreTypeOption, replaceStringsFileOption);

        sbCommand.SetHandler(CompareSubscriptions,
                    sourceSubscriptionId, targetSubscriptionId, outputFolderOption, ignoreTypeOption, replaceStringsFileOption);

        return await rootCommand.InvokeAsync(args);
    }

    internal static ArmTemplateComparer ArmTemplateComparer { get; set; } = new();
    internal static IAzureTemplateLoader AzureTemplateLoader { get; set; } = new AzureTemplateLoader();

    static async Task<int> CompareArmTemplateFiles(FileInfo source, FileInfo target, DirectoryInfo outputFolder, IEnumerable<string> typesToIgnore, FileInfo? replaceStringsFile)
    {
        var comparer = new JsonFileDiffCommand(ArmTemplateComparer);
        return await comparer.CompareJsonFiles(source, target, outputFolder, typesToIgnore, replaceStringsFile);
    }

    static async Task<int> CompareResourceGroups(string sourceResourceGroupId, string targetResourceGroupId, DirectoryInfo outputFolder, IEnumerable<string> typesToIgnore, FileInfo? replaceStringsFile)
    {
        var comparer = new ResourceGroupDiffCommand(ArmTemplateComparer, AzureTemplateLoader);
        return await comparer.CompareResourceGroups(sourceResourceGroupId, targetResourceGroupId, outputFolder, typesToIgnore, replaceStringsFile);
    }
    
    static async Task<int> CompareSubscriptions(string sourceSubscriptionId, string targetSubscriptionId, DirectoryInfo outputFolder, IEnumerable<string> typesToIgnore, FileInfo? replaceStringsFile)
    {
        var comparer = new SubscriptionDiffCommand(ArmTemplateComparer, AzureTemplateLoader);
        return await comparer.CompareSubscriptions(sourceSubscriptionId, targetSubscriptionId, outputFolder, typesToIgnore, replaceStringsFile);
    }
}