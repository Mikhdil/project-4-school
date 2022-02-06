using Microsoft.Extensions.DependencyInjection.Extensions;
using Orleans;
using Orleans.Hosting;
using Orleans.Providers;
using Orleans.Runtime;
using Orleans.Storage;

namespace TopSite.Grains
{
  public static class FileSiloBuilderExtensions
  {
    public static ISiloBuilder AddFileGrainStorageAsDefault(this ISiloBuilder builder, Action<FileGrainStorageOptions> configureOptions)
    {
      return builder.AddFileGrainStorage(ProviderConstants.DEFAULT_STORAGE_PROVIDER_NAME, configureOptions);
    }

    public static ISiloBuilder AddFileGrainStorage(this ISiloBuilder builder, string providerName, Action<FileGrainStorageOptions> options)
    {
      return builder.ConfigureServices(services => services.AddFileGrainStorage(providerName, options));
    }

    public static IServiceCollection AddFileGrainStorage(this IServiceCollection services, string providerName, Action<FileGrainStorageOptions> options)
    {
      services.AddOptions<FileGrainStorageOptions>(providerName).Configure(options);

      if (string.Equals(providerName, ProviderConstants.DEFAULT_STORAGE_PROVIDER_NAME, StringComparison.Ordinal))
      {
        services.TryAddSingleton(sp => sp.GetServiceByName<IGrainStorage>(ProviderConstants.DEFAULT_STORAGE_PROVIDER_NAME));
      }
      return services
          .AddSingletonNamedService(providerName, FileGrainStorageFactory.Create)
          .AddSingletonNamedService(providerName, (s, n) => (ILifecycleParticipant<ISiloLifecycle>)s.GetRequiredServiceByName<IGrainStorage>(n));
    }
  }
}
