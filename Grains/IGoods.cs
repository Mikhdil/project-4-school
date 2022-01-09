using Orleans;
using Orleans.Runtime;

namespace TopSite.Grains
{
  public interface IGoods : IGrainWithIntegerKey
  {
    Task Create(CreateGoodsCommand createCommand);

    Task<GoodsState> Info();
  }

  public class GoodsGrain : Grain, IGoods
  {
    private IPersistentState<GoodsState> state;

    public GoodsGrain([PersistentState("Goods")] IPersistentState<GoodsState> goodsState)
    {
      this.state = goodsState;
    }

    public async Task Create(CreateGoodsCommand createCommand)
    {
      this.state.State = new GoodsState(
        createCommand.Name,
        createCommand.Phone,
        createCommand.Photo,
        createCommand.Price);

      var listGrain = this.GrainFactory.GetGrain<IGoodsList>(Guid.Empty);
      var goodsId = this.GetPrimaryKeyLong();
      await listGrain.Add(goodsId);

      await this.state.WriteStateAsync();
    }

    public Task<GoodsState> Info()
    {
      return Task.FromResult(this.state.State);
    }
  }

  public class GoodsState
  {
    public GoodsState(string name, string phone, string photo, decimal price)
    {
      this.Name = name;
      this.Phone = phone;
      this.Photo = photo;
      this.Price = price;
    }

    public string Name { get; private set; }

    public string Phone { get; private set; }

    public string Photo { get; private set; }

    public decimal Price { get; private set; }

    public GoodsState() { }
  }

  public record CreateGoodsCommand(string Name, string Phone, string Photo, decimal Price);
}
