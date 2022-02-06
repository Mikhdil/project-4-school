using System.Collections.Immutable;
using Orleans;
using Orleans.Runtime;

namespace TopSite.Grains
{
  public interface IGoodsRepository : IGrainWithGuidKey
  {
    Task Add(Guid goodsId);

    Task Delete(Guid goodsId);

    Task<ImmutableArray<GoodsRow>> ListAll();
    Task<IGoods> GetBySurrogateId(long surrogateId);
  }

  public class GoodsRepositoryGrain : Grain, IGoodsRepository
  {
    public IPersistentState<GoodsRepositoryGrainState> state;

    public GoodsRepositoryGrain([PersistentState("GoodsRepository")] IPersistentState<GoodsRepositoryGrainState> goodsListState)
    {
      this.state = goodsListState;
    }

    public Task Add(Guid goodsId)
    {
      var surrogateId = this.NextSurrogateId();
      this.state.State.Goods.Add(new(goodsId, surrogateId));
      return this.state.WriteStateAsync();
    }

    private long NextSurrogateId()
    {
      return this.state.State.Goods.Count > 0 ? this.state.State.Goods.Max(r => r.SurrogateId) + 1 : 1;
    }

    public Task<ImmutableArray<GoodsRow>> ListAll()
    {
      return Task.FromResult(this.state.State.Goods.ToImmutableArray());
    }

    public Task<IGoods> GetBySurrogateId(long surrogateId)
    {
      var goodsRow = this.state.State.Goods.SingleOrDefault(r => r.SurrogateId == surrogateId);
      if (goodsRow == null)
        return Task.FromResult<IGoods>(null);

      return Task.FromResult(this.GrainFactory.GetGrain<IGoods>(goodsRow.Id));
    }

    public Task Delete(Guid goodsId)
    {
      var goodsRow = this.state.State.Goods.SingleOrDefault(r => r.Id == goodsId);
      if (goodsRow == null)
        return Task.CompletedTask;

      this.state.State.Goods.Remove(goodsRow);
      return this.state.WriteStateAsync();
    }
  }

  public class GoodsRepositoryGrainState
  {
    public HashSet<GoodsRow> Goods { get; private set; }

    public GoodsRepositoryGrainState()
    {
      this.Goods = new HashSet<GoodsRow>();
    }
  }

  public record GoodsRow(Guid Id, long SurrogateId);
}
