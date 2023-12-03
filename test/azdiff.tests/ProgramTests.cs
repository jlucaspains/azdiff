namespace azdiff.tests;

public class ProgramTests
{
    [Fact]
    public async Task NoParametersReturn1()
    {
        var result = await Program.Main([]);

        Assert.Equal(1, result);
    }

    [Fact]
    public async Task RunWithSourceNotExisting()
    {
        var result = await Program.Main(["arm", "--sourceFile", "source.json", "--targetFile", "target.json"]);
        Assert.Equal(2, result);
    }

    [Fact]
    public async Task RunWithTargetNotExisting()
    {
        File.Create("RunWithTargetNotExisting.json").Close();
        var result = await Program.Main(["arm", "--sourceFile", "RunWithTargetNotExisting.json", "--targetFile", "target.json"]);
        Assert.Equal(3, result);
    }

    [Fact]
    public async Task RunWithReplaceStringsNotExisting()
    {
        File.Create("RunWithReplaceStringsNotExisting.json").Close();
        var result = await Program.Main([
            "arm",
            "--sourceFile", "RunWithReplaceStringsNotExisting.json",
            "--targetFile", "RunWithReplaceStringsNotExisting.json",
            "--replaceStringsFile", "invalid.json"
        ]);
        Assert.Equal(4, result);
    }

    [Fact]
    public async Task RunWithInvalidReplaceStrings()
    {
        File.WriteAllText("RunWithInvalidReplaceStrings.json", "{");
        var result = await Program.Main([
            "arm",
            "--sourceFile", "RunWithInvalidReplaceStrings.json",
            "--targetFile", "RunWithInvalidReplaceStrings.json",
            "--replaceStringsFile", "RunWithInvalidReplaceStrings.json"
        ]);
        Assert.Equal(5, result);
    }

    [Fact]
    public async Task RunWithEmptyReplaceStrings()
    {
        File.WriteAllText("RunWithEmptyReplaceStrings.json", "");
        var result = await Program.Main([
            "arm",
            "--sourceFile", "RunWithEmptyReplaceStrings.json",
            "--targetFile", "RunWithEmptyReplaceStrings.json",
            "--replaceStringsFile", "RunWithEmptyReplaceStrings.json"
        ]);
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
        var result = await Program.Main([
                    "arm",
            "--sourceFile", "AllValidParameters_source.json",
            "--targetFile", "AllValidParameters_target.json",
            "--replaceStringsFile", "AllValidParameters_replace.json"
                ]);
        Assert.Equal(0, result);
    }
}