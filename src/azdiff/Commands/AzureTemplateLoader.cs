using Azure.Identity;
using Azure.ResourceManager;
using Azure.Core;
using Azure.ResourceManager.Resources.Models;

namespace azdiff;

interface IAzureTemplateLoader {
    Task<string> GetArmTemplateAsync(string resourceGroupId);
}

class AzureTemplateLoader : IAzureTemplateLoader
{
    public async Task<string> GetArmTemplateAsync(string resourceGroupId)
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
}
