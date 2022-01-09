using System.Collections.Immutable;
using Orleans;
using Orleans.Runtime;

namespace TopSite.Grains
{
  public interface IGoodsList : IGrainWithGuidKey
  {
    Task Add(long goodsId);

    Task<ImmutableArray<long>> ListAll();
  }

  public class GoodsListGrain : Grain, IGoodsList
  {
    public IPersistentState<HashSet<long>> state;

    public GoodsListGrain([PersistentState("GoodsList")] IPersistentState<HashSet<long>> goodsListState)
    {
      this.state = goodsListState;
    }

    public Task Add(long goodsId)
    {
      this.state.State.Add(goodsId);
      return this.state.WriteStateAsync();
    }

    public Task<ImmutableArray<long>> ListAll()
    {
      return Task.FromResult(this.state.State.ToImmutableArray());
    }
  }
}
