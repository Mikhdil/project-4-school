using Microsoft.Extensions.Options;
using Orleans.Configuration.Overrides;
using Orleans.Storage;

namespace TopSite.Grains
{
  public static class FileGrainStorageFactory
  {
    internal static IGrainStorage Create(IServiceProvider services, string name)
    {
      var optionsSnapshot = services.GetRequiredService<IOptionsMonitor<FileGrainStorageOptions>>();
      return ActivatorUtilities.CreateInstance<FileGrainStorage>(services, name, optionsSnapshot.Get(name), services.GetProviderClusterOptions(name));
    }
  }
}
