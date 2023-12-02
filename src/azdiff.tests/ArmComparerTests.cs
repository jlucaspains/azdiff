namespace azdiff.tests;

public class ArmComparerTests
{
    [Fact]
    public void EmptySourceReturnsEmptyResult()
    {
        var result = ArmComparer.DiffArmTemplates("", "{}", [], []);

        Assert.Empty(result);
    }

    [Fact]
    public void EmptyTargetReturnsEmptyResult()
    {
        var result = ArmComparer.DiffArmTemplates("{}", "", [], []);

        Assert.Empty(result);
    }

    [Fact]
    public void CompareReturnsDifferences()
    {
        string left = @"
{
    ""resources"": [
        {
            ""type"": ""Microsoft.Web/staticSites"",
            ""apiVersion"": ""2023-01-01"",
            ""name"": ""stapp-blog-centralus-001"",
            ""location"": ""Central US"",
        }
    ]
}";

        string right = @"
{
    ""resources"": [
        {
            ""type"": ""Microsoft.Web/staticSites"",
            ""apiVersion"": ""2023-01-01"",
            ""name"": ""stapp-blog-centralus-001"",
            ""location"": ""South Central US"",
        }
    ]
}";
        var result = ArmComparer.DiffArmTemplates(left, right, [], []);

        Assert.True(result.Count() == 1);
        Assert.Equal("stapp-blog-centralus-001", result.First().Name);
        Assert.Equal(DiffType.Diff, result.First().DiffType);
        Assert.Contains("-   \"location\": \"Central US\"", result.First().Result);
        Assert.Contains("+   \"location\": \"South Central US\"", result.First().Result);
    }

    [Fact]
    public void CompareReturnsNew()
    {
        string left = @"
{
    ""resources"": [
        {
            ""type"": ""Microsoft.Web/staticSites"",
            ""apiVersion"": ""2023-01-01"",
            ""name"": ""stapp-blog-centralus-001"",
            ""location"": ""Central US"",
        }
    ]
}";

        string right = @"
{
    ""resources"": []
}";
        var result = ArmComparer.DiffArmTemplates(left, right, [], []);

        Console.WriteLine(result);
        Assert.True(result.Count() == 1);
        Assert.Equal("stapp-blog-centralus-001", result.First().Name);
        Assert.Equal(DiffType.MissingOnTarget, result.First().DiffType);
        Assert.Contains("  \"location\": \"Central US\"", result.First().Result);
    }
    
    [Fact]
    public void CompareReturnsExtra()
    {
        string left = @"
{
    ""resources"": []
}";

        string right = @"
{
    ""resources"": [
        {
            ""type"": ""Microsoft.Web/staticSites"",
            ""apiVersion"": ""2023-01-01"",
            ""name"": ""stapp-blog-centralus-001"",
            ""location"": ""South Central US"",
        }
    ]
}";
        var result = ArmComparer.DiffArmTemplates(left, right, [], []);

        Assert.True(result.Count() == 1);
        Assert.Equal("stapp-blog-centralus-001", result.First().Name);
        Assert.Equal(DiffType.ExtraOnTarget, result.First().DiffType);
        Assert.Contains("  \"location\": \"South Central US\"", result.First().Result);
    }

    [Fact]
    public void CompareWithReplacementsReturnsDifferences()
    {
        string left = @"
{
    ""resources"": [
        {
            ""type"": ""Microsoft.Web/staticSites"",
            ""apiVersion"": ""2023-01-01"",
            ""name"": ""stapp-blog-centralus-dev-001"",
            ""location"": ""Central US"",
        }
    ]
}";

        string right = @"
{
    ""resources"": [
        {
            ""type"": ""Microsoft.Web/staticSites"",
            ""apiVersion"": ""2023-01-01"",
            ""name"": ""stapp-blog-centralus-test-001"",
            ""location"": ""South Central US"",
        }
    ]
}";
        var result = ArmComparer.DiffArmTemplates(left, right, [], [
            new ReplaceText(ReplaceTextTarget.Name, "dev", "env"),
            new ReplaceText(ReplaceTextTarget.Name, "test", "env")
        ]);

        Assert.True(result.Count() == 1);
        Assert.Equal("stapp-blog-centralus-env-001", result.First().Name);
        Assert.Equal(DiffType.Diff, result.First().DiffType);
        Assert.Contains("-   \"location\": \"Central US\"", result.First().Result);
        Assert.Contains("+   \"location\": \"South Central US\"", result.First().Result);
    }

    [Fact]
    public void CompareWillIgnoreTypes()
    {
        string left = @"
{
    ""resources"": [
        {
            ""type"": ""Microsoft.Web/staticSites"",
            ""apiVersion"": ""2023-01-01"",
            ""name"": ""stapp-blog-centralus-dev-001"",
            ""location"": ""Central US"",
        }
    ]
}";

        string right = @"
{
    ""resources"": [
        {
            ""type"": ""Microsoft.Web/staticSites"",
            ""apiVersion"": ""2023-01-01"",
            ""name"": ""stapp-blog-centralus-test-001"",
            ""location"": ""South Central US"",
        }
    ]
}";
        var result = ArmComparer.DiffArmTemplates(left, right, [
            "Microsoft.Web/staticSites"
        ], []);

        Assert.Empty(result);
    }

    
    [Fact]
    public void CompareBadJson()
    {
        string left = @"
{
    ""resources"": 
}";

        string right = @"
{
    ""resources"":
}";
        var result = ArmComparer.DiffArmTemplates(left, right, [], []);

        Assert.Empty(result);
    }
}