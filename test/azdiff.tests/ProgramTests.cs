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
    public async Task CompareJsonFileCommandReturns0()
    {
        File.WriteAllText("CompareJsonFileCommandReturns0.json", "{}");
        var result = await Program.Main(["arm", "--sourceFile", "CompareJsonFileCommandReturns0.json", "--targetFile", "CompareJsonFileCommandReturns0.json"]);
        Assert.Equal(0, result);
    }

    [Fact]
    public async Task CompareResourceGroupCommandReturns0()
    {
        File.WriteAllText("CompareJsonFileCommandReturns0.json", "{}");
        
        var templateLoader = new TestAzureTemplateLoader();
        templateLoader.Result.Add("id", ("{}", 0));
        Program.AzureTemplateLoader = templateLoader;

        var result = await Program.Main(["rg", "--sourceResourceGroupId", "id", "--targetResourceGroupId", "id"]);
        Assert.Equal(0, result);
    }
}