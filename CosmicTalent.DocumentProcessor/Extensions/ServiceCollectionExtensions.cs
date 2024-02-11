using Azure.Identity;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

namespace CosmicTalent.DocumentProcessor.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IFunctionsConfigurationBuilder AddAzureKeyVaultConnection(this IFunctionsConfigurationBuilder builder)
        {
            var env = builder.GetContext().EnvironmentName;
            if (env != Environments.Development)
            {

                var cred = new ClientSecretCredential(Environment.GetEnvironmentVariable("KeyVault__TenantId"),
                                         Environment.GetEnvironmentVariable("KeyVault__ClientId"),
                                         Environment.GetEnvironmentVariable("KeyVault__Secret"));
                builder.ConfigurationBuilder.AddAzureKeyVault(new Uri(Environment.GetEnvironmentVariable("KeyVault__Uri")), cred);
            }            
            return builder;
        }
    }
}
