using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Orleans;
using Orleans.Configuration;
using Orleans.Runtime;
using Orleans.Serialization;
using Orleans.Storage;

namespace TopSite.Grains
{
  public class FileGrainStorage : IGrainStorage, ILifecycleParticipant<ISiloLifecycle>
  {
    private readonly string storageName;
    private readonly FileGrainStorageOptions options;
    private readonly ClusterOptions clusterOptions;
    private readonly IGrainFactory grainFactory;
    private readonly ITypeResolver typeResolver;
    private JsonSerializerSettings jsonSettings;

    public FileGrainStorage(
      string storageName,
      FileGrainStorageOptions options,
      IOptions<ClusterOptions> clusterOptions,
      IGrainFactory grainFactory,
      ITypeResolver typeResolver)
    {
      this.storageName = storageName;
      this.options = options;
      this.clusterOptions = clusterOptions.Value;
      this.grainFactory = grainFactory;
      this.typeResolver = typeResolver;
    }

    public Task ClearStateAsync(string grainType, GrainReference grainReference, IGrainState grainState)
    {
      var fName = GetKeyString(grainType, grainReference);
      var path = Path.Combine(this.options.RootDirectory, fName);

      var fileInfo = new FileInfo(path);
      if (fileInfo.Exists)
      {
        if (fileInfo.LastWriteTimeUtc.ToString() != grainState.ETag)
        {
          throw new InconsistentStateException($"Version conflict (ClearState): ServiceId={this.clusterOptions.ServiceId} ProviderName={this.storageName} GrainType={grainType} GrainReference={grainReference.ToKeyString()}.");
        }

        grainState.ETag = null;
        grainState.State = Activator.CreateInstance(grainState.State.GetType());
        fileInfo.Delete();
      }

      return Task.CompletedTask;
    }

    public async Task ReadStateAsync(string grainType, GrainReference grainReference, IGrainState grainState)
    {
      var fName = GetKeyString(grainType, grainReference);
      var path = Path.Combine(this.options.RootDirectory, fName);

      var fileInfo = new FileInfo(path);
      if (!fileInfo.Exists)
      {
        grainState.State = Activator.CreateInstance(grainState.State.GetType());
        return;
      }

      using (var stream = fileInfo.OpenText())
      {
        var storedData = await stream.ReadToEndAsync();
        grainState.State = JsonConvert.DeserializeObject(storedData, grainState.Type, this.jsonSettings);
      }

      grainState.ETag = fileInfo.LastWriteTimeUtc.ToString();
    }

    public async Task WriteStateAsync(string grainType, GrainReference grainReference, IGrainState grainState)
    {
      var storedData = JsonConvert.SerializeObject(grainState.State, this.jsonSettings);

      var fName = GetKeyString(grainType, grainReference);
      var path = Path.Combine(this.options.RootDirectory, fName);

      var fileInfo = new FileInfo(path);

      if (fileInfo.Exists && fileInfo.LastWriteTimeUtc.ToString() != grainState.ETag)
      {
        throw new InconsistentStateException($"Version conflict (WriteState): ServiceId={this.clusterOptions.ServiceId} ProviderName={this.storageName} GrainType={grainType} GrainReference={grainReference.ToKeyString()}.");
      }

      using (var stream = new StreamWriter(fileInfo.Open(FileMode.Create, FileAccess.Write)))
      {
        await stream.WriteAsync(storedData);
      }

      fileInfo.Refresh();
      grainState.ETag = fileInfo.LastWriteTimeUtc.ToString();
    }

    public void Participate(ISiloLifecycle lifecycle)
    {
      lifecycle.Subscribe(
        OptionFormattingUtilities.Name<FileGrainStorage>(this.storageName),
        ServiceLifecycleStage.ApplicationServices,
        this.Init);
    }

    private Task Init(CancellationToken ct)
    {
      // Settings could be made configurable from Options.
      this.jsonSettings = OrleansJsonSerializer.UpdateSerializerSettings(
        OrleansJsonSerializer.GetDefaultSerializerSettings(this.typeResolver, this.grainFactory),
        false, true, TypeNameHandling.Auto);

      var directory = new DirectoryInfo(this.options.RootDirectory);
      if (!directory.Exists)
        directory.Create();

      return Task.CompletedTask;
    }

    private string GetKeyString(string grainType, GrainReference grainReference)
    {
      return $"{this.clusterOptions.ServiceId}.{grainReference.ToKeyString()}.{grainType}";
    }
  }

  public class FileGrainStorageOptions
  {
    public string RootDirectory { get; set; }
  }
}
