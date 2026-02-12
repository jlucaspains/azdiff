using Azure.Identity;
using Azure.ResourceManager;
using Azure.Core;
using Azure.ResourceManager.Resources.Models;
using System.Security.Permissions;

namespace azdiff;

enum CredentialType
{
    DefaultAzureCredential,
    EnvironmentCredential,
    WorkloadIdentityCredential,
    ManagedIdentityCredential,
    SharedTokenCacheCredential,
    VisualStudioCredential,
    VisualStudioCodeCredential,
    AzureCliCredential,
    AzurePowerShellCredential,
    AzureDeveloperCliCredential,
    InteractiveBrowserCredential
}

interface IAzureTemplateLoader
{
    Task<(string Result, int Code)> GetArmTemplateAsync(string resourceGroupId, CredentialType credentialType);
}

class AzureTemplateLoader : IAzureTemplateLoader
{
    public async Task<(string Result, int Code)> GetArmTemplateAsync(string resourceGroupId, CredentialType credentialType)
    {
        try
        {
            Utilities.WriteInformation("Getting ARM template for resource group {0}...", resourceGroupId);
            TokenCredential credential = credentialType switch
            {
                CredentialType.DefaultAzureCredential => new DefaultAzureCredential(),
                CredentialType.EnvironmentCredential => new EnvironmentCredential(),
                CredentialType.WorkloadIdentityCredential => new WorkloadIdentityCredential(),
                CredentialType.ManagedIdentityCredential => new ManagedIdentityCredential(),
#pragma warning disable CS0618 // Type or member is obsolete
                CredentialType.SharedTokenCacheCredential => new SharedTokenCacheCredential(),
#pragma warning restore CS0618 // Type or member is obsolete
                CredentialType.VisualStudioCodeCredential => new VisualStudioCodeCredential(),
                CredentialType.VisualStudioCredential => new VisualStudioCredential(),
                CredentialType.InteractiveBrowserCredential => new InteractiveBrowserCredential(),
                CredentialType.AzureCliCredential => new AzureCliCredential(),
                CredentialType.AzurePowerShellCredential => new AzurePowerShellCredential(),
                CredentialType.AzureDeveloperCliCredential => new AzureDeveloperCliCredential(),
                _ => throw new NotImplementedException()
            };
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
