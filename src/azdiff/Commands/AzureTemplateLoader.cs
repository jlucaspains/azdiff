using Azure.Identity;
using Azure.ResourceManager;
using Azure.Core;
using Azure.ResourceManager.Resources.Models;
using System.Security.Permissions;

namespace azdiff;

interface IAzureTemplateLoader
{
    Task<(string Result, int Code)> GetArmTemplateAsync(string resourceGroupId);
}

class AzureTemplateLoader : IAzureTemplateLoader
{
    public async Task<(string Result, int Code)> GetArmTemplateAsync(string resourceGroupId)
    {
        try
        {
            Utilities.WriteInformation("Getting ARM template for resource group {0}...", resourceGroupId);
            var credential = new DefaultAzureCredential(includeInteractiveCredentials: true);
            var client = new ArmClient(credential);
            var rg = client.GetResourceGroupResource(new ResourceIdentifier(resourceGroupId));

            var request = new ExportTemplate
            {
                Options = "SkipAllParameterization"
            };
            request.Resources.Add("*");
            var armtemplate = await rg.ExportTemplateAsync(Azure.WaitUntil.Completed, request);

            return (armtemplate.Value.Template.ToString(), 0);

        }
        catch (Exception ex)
        {
            Utilities.WriteError("Error getting ARM template for resource group {0}: {1}", resourceGroupId, ex.Message);
            return (string.Empty, 6);
        }
    }
}
