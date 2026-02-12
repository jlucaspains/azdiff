namespace azdiff.tests;

public class ProgramTests
{
    [Fact]
    public void NoParametersReturn1()
    {
        var result = Program.Main([]);

        Assert.Equal(1, result);
    }

    [Fact]
    public void CompareJsonFileCommandReturns0()
    {
        File.WriteAllText("CompareJsonFileCommandReturns0.json", "{}");
        var result = Program.Main(["arm", "--sourceFile", "CompareJsonFileCommandReturns0.json", "--targetFile", "CompareJsonFileCommandReturns0.json"]);
        Assert.Equal(0, result);
    }

    [Fact]
    public void CompareResourceGroupCommandReturns0()
    {
        File.WriteAllText("CompareJsonFileCommandReturns0.json", "{}");
        
        var templateLoader = new TestAzureTemplateLoader();
        templateLoader.Result.Add("id", ("{}", 0));
        Program.AzureTemplateLoader = templateLoader;

        var result = Program.Main(["rg", "--sourceResourceGroupId", "id", "--targetResourceGroupId", "id"]);
        Assert.Equal(0, result);
    }
}