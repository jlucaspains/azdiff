using Azure.Identity;
using Azure.ResourceManager;
using Azure.Core;
using Azure.ResourceManager.Resources.Models;

namespace azdiff;

interface IAzureTemplateLoader {
    Task<string> GetResourceGroupTemplateAsync(string resourceGroupId);
    Task<Dictionary<string, string>> GetSubscriptionTemplatesAsync(string subscriptionId);
}

class AzureTemplateLoader : IAzureTemplateLoader
{
    public async Task<string> GetResourceGroupTemplateAsync(string resourceGroupId)
    {
        Console.WriteLine("Getting ARM template for resource group {0}", resourceGroupId);
        var credential = new DefaultAzureCredential();
        var client = new ArmClient(credential);
        var rg = client.GetResourceGroupResource(new ResourceIdentifier(resourceGroupId));

        var request = new ExportTemplate
        {
            Options = "SkipAllParameterization"
        };
        request.Resources.Add("*");
        var armtemplate = await rg.ExportTemplateAsync(Azure.WaitUntil.Completed, request);

        return armtemplate.Value.Template.ToString();
    }

    public async Task<Dictionary<string, string>> GetSubscriptionTemplatesAsync(string subscriptionId)
    {
        Console.WriteLine("Getting ARM template for subscription {0}", subscriptionId);

        if (!subscriptionId.StartsWith("/subscriptions/", StringComparison.InvariantCultureIgnoreCase))
            subscriptionId = $"/subscriptions/{subscriptionId}";

        var credential = new DefaultAzureCredential();
        var client = new ArmClient(credential);
        var sb = client.GetSubscriptionResource(new ResourceIdentifier(subscriptionId));
        var rgs = sb.GetResourceGroups();

        var request = new ExportTemplate
        {
            Options = "SkipAllParameterization"
        };
        request.Resources.Add("*");

        var result = new Dictionary<string, string>();
        foreach(var item in rgs)
        {
            var armtemplate = await item.ExportTemplateAsync(Azure.WaitUntil.Completed, request);
            result.Add(item.Data.Name, armtemplate.Value.Template.ToString());
        }

        return result;
    }
}
