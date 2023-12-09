namespace azdiff.tests;

public class JsonFileDiffCommandTests
{
    [Fact]
    public async Task RunWithSourceNotExisting()
    {
        DirectoryInfo outputFolder = new(".");
        var comparer = new JsonFileDiffCommand(new ArmTemplateComparer());
        var result = await comparer.CompareJsonFiles(new FileInfo("source.json"), new FileInfo("target.json"), outputFolder, [], null);

        Assert.Equal(2, result);
    }

    [Fact]
    public async Task RunWithTargetNotExisting()
    {
        DirectoryInfo outputFolder = new(".");
        var comparer = new JsonFileDiffCommand(new ArmTemplateComparer());
        File.Create("RunWithTargetNotExisting.json").Close();
        var result = await comparer.CompareJsonFiles(new FileInfo("RunWithTargetNotExisting.json"), new FileInfo("invalidtarget.json"), outputFolder, [], null);
        Assert.Equal(3, result);
    }

    [Fact]
    public async Task RunWithReplaceStringsNotExisting()
    {
        File.Create("RunWithReplaceStringsNotExisting.json").Close();
        DirectoryInfo outputFolder = new(".");
        var comparer = new JsonFileDiffCommand(new ArmTemplateComparer());
        var result = await comparer.CompareJsonFiles(new FileInfo("RunWithReplaceStringsNotExisting.json"), new FileInfo("RunWithReplaceStringsNotExisting.json"), outputFolder, [], new FileInfo("invalid.json"));

        Assert.Equal(4, result);
    }

    [Fact]
    public async Task RunWithInvalidReplaceStrings()
    {
        File.WriteAllText("RunWithInvalidReplaceStrings.json", "{");
        DirectoryInfo outputFolder = new(".");
        var comparer = new JsonFileDiffCommand(new ArmTemplateComparer());
        var result = await comparer.CompareJsonFiles(new FileInfo("RunWithInvalidReplaceStrings.json"), new FileInfo("RunWithInvalidReplaceStrings.json"), outputFolder, [], new FileInfo("RunWithInvalidReplaceStrings.json"));

        Assert.Equal(5, result);
    }

    [Fact]
    public async Task AllValidParameters()
    {
        File.WriteAllText("AllValidParameters_source.json", @"
{
    ""resources"": [
        {
            ""type"": ""Microsoft.Web/staticSites"",
            ""apiVersion"": ""2023-01-01"",
            ""name"": ""stapp-blog-centralus-dev-001"",
            ""location"": ""Central US"",
        }
    ]
}");

        File.WriteAllText("AllValidParameters_target.json", @"
{
    ""resources"": [
        {
            ""type"": ""Microsoft.Web/staticSites"",
            ""apiVersion"": ""2023-01-01"",
            ""name"": ""stapp-blog-centralus-dev-001"",
            ""location"": ""South Central US"",
        }
    ]
}");

        File.WriteAllText("AllValidParameters_replace.json", @"[{
    ""target"": ""Name"",
    ""input"": ""dev"",
    ""replacement"": ""env""
}]");
        DirectoryInfo outputFolder = new(".");
        var comparer = new JsonFileDiffCommand(new ArmTemplateComparer());
        var result = await comparer.CompareJsonFiles(new FileInfo("AllValidParameters_source.json"), new FileInfo("AllValidParameters_target.json"), outputFolder, [], new FileInfo("AllValidParameters_replace.json"));

        Assert.Equal(0, result);
    }
}