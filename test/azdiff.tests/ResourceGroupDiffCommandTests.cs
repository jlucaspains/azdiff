namespace azdiff.tests;

internal class TestAzureTemplateLoader : IAzureTemplateLoader
{
    public Dictionary<string, (string, int)> Result { get; set; } = new();
    public Task<(string Result, int Code)> GetArmTemplateAsync(string resourceGroupId)
    {
        return Task.FromResult(Result[resourceGroupId]);
    }
}

public class ResourceGroupDiffCommandTests
{
    [Fact]
    public async Task ValidSourceTargetReturnsCode0()
    {
        DirectoryInfo outputFolder = new(".");
        var templateLoader = new TestAzureTemplateLoader();
        templateLoader.Result.Add("test1", ("{}", 0));
        templateLoader.Result.Add("test2", ("{}", 0));
        var comparer = new ResourceGroupDiffCommand(new ArmTemplateComparer(), templateLoader);
        var result = await comparer.CompareResourceGroups("test1", "test2", outputFolder, [], null);

        Assert.Equal(0, result);
    }

    [Fact]
    public async Task ValidSourceTargetReplaceStringsReturnsCode0()
    {
        DirectoryInfo outputFolder = new(".");
        var templateLoader = new TestAzureTemplateLoader();
        templateLoader.Result.Add("test1", ("{}", 0));
        templateLoader.Result.Add("test2", ("{}", 0));
        File.WriteAllText("replacestrings.json", "[{\"Target\":\"Name\",\"Input\": \"dev\",\"Replacement\":\"env\"}]");
        var comparer = new ResourceGroupDiffCommand(new ArmTemplateComparer(), templateLoader);
        var result = await comparer.CompareResourceGroups("test1", "test2", outputFolder, [], new FileInfo("replacestrings.json"));

        Assert.Equal(0, result);
    }

    [Fact]
    public async Task BadSourceIdReturnsErrorCode2()
    {
        DirectoryInfo outputFolder = new(".");
        var comparer = new ResourceGroupDiffCommand(new ArmTemplateComparer(), new TestAzureTemplateLoader());
        var result = await comparer.CompareResourceGroups("", "", outputFolder, [], null);

        Assert.Equal(2, result);
    }

    [Fact]
    public async Task BadTargetIdReturnsErrorCode2()
    {
        DirectoryInfo outputFolder = new(".");
        var templateLoader = new TestAzureTemplateLoader();
        templateLoader.Result.Add("id", ("{}", 0));
        var comparer = new ResourceGroupDiffCommand(new ArmTemplateComparer(), templateLoader);
        var result = await comparer.CompareResourceGroups("id", "", outputFolder, [], null);

        Assert.Equal(3, result);
    }

    [Fact]
    public async Task InvalidReplaceStringsReturnErrorCode()
    {
        DirectoryInfo outputFolder = new(".");
        var templateLoader = new TestAzureTemplateLoader();
        templateLoader.Result.Add("id", ("{}", 0));
        var comparer = new ResourceGroupDiffCommand(new ArmTemplateComparer(), templateLoader);
        var result = await comparer.CompareResourceGroups("id", "id", outputFolder, [], new FileInfo("idontexist.json"));

        Assert.Equal(4, result);
    }

    [Fact]
    public async Task InvalidContentReplaceStringsReturnErrorCode()
    {
        DirectoryInfo outputFolder = new(".");
        var templateLoader = new TestAzureTemplateLoader();
        templateLoader.Result.Add("id", ("{}", 0));
        File.WriteAllText("badreplacestrings.json", "");
        var comparer = new ResourceGroupDiffCommand(new ArmTemplateComparer(), templateLoader);
        var result = await comparer.CompareResourceGroups("id", "id", outputFolder, [], new FileInfo("badreplacestrings.json"));

        Assert.Equal(5, result);
    }
    
    [Fact]
    public async Task NonZeroResultLoadingSourceAzureTemplateStopsCompares()
    {
        DirectoryInfo outputFolder = new(".");
        var templateLoader = new TestAzureTemplateLoader();
        templateLoader.Result.Add("source", ("{}", 6));
        templateLoader.Result.Add("target", ("{}", 0));
        var comparer = new ResourceGroupDiffCommand(new ArmTemplateComparer(), templateLoader);
        var result = await comparer.CompareResourceGroups("source", "target", outputFolder, [], null);

        Assert.Equal(6, result);
    }
    
    [Fact]
    public async Task NonZeroResultLoadingTargetAzureTemplateStopsCompares()
    {
        DirectoryInfo outputFolder = new(".");
        var templateLoader = new TestAzureTemplateLoader();
        templateLoader.Result.Add("source", ("{}", 0));
        templateLoader.Result.Add("target", ("{}", 6));
        var comparer = new ResourceGroupDiffCommand(new ArmTemplateComparer(), templateLoader);
        var result = await comparer.CompareResourceGroups("source", "target", outputFolder, [], null);

        Assert.Equal(6, result);
    }
}