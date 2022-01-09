using Microsoft.AspNetCore.Mvc.RazorPages;
using Orleans;
using TopSite.Grains;

namespace TopSite.Pages
{
  public class IndexModel : PageModel
  {
    private readonly ILogger<IndexModel> logger;
    private readonly IGrainFactory grainFactory;

    public IndexModel(ILogger<IndexModel> logger, IGrainFactory grainFactory)
    {
      this.logger = logger;
      this.grainFactory = grainFactory;
      this.Goods = new List<GoodsViewModel>();
    }

    public List<GoodsViewModel> Goods { get; set; }

    public async Task OnGetAsync()
    {
      var list = this.grainFactory.GetGrain<IGoodsRepository>(Guid.Empty);
      var allGoodsRows = await list.ListAll();
      if (allGoodsRows.Length == 0)
      {
        await this.FillSampleData();
        allGoodsRows = await list.ListAll();
      }

      var goodsStatesTaskList = new Dictionary<long, Task<GoodsState>>();
      foreach (var goodsRow in allGoodsRows)
      {
        var goodsGrain = this.grainFactory.GetGrain<IGoods>(goodsRow.Id);
        goodsStatesTaskList.Add(goodsRow.SurrogateId, goodsGrain.Info());
      }

      Task.WaitAll(goodsStatesTaskList.Values.ToArray());
      foreach (var goodsTask in goodsStatesTaskList)
      {
        var goods = goodsTask.Value.Result;
        this.Goods.Add(new GoodsViewModel()
        {
          Id = goodsTask.Key,
          Name = goods.Name,
          Phone = goods.Phone,
          Photo = goods.Photo,
          Price = goods.Price
        });
      }
    }

    private async Task FillSampleData()
    {
      var goods = this.grainFactory.GetGrain<IGoods>(Guid.NewGuid());
      await goods.Create(new CreateGoodsCommand("Ноутбук 16 гб оперотивной памяти", "81231234567", "img.jpg", 30000));

      goods = this.grainFactory.GetGrain<IGoods>(Guid.NewGuid());
      await goods.Create(new CreateGoodsCommand("Чехлы для телефоноф", "81231234567", "img.jpg", 800));

      goods = this.grainFactory.GetGrain<IGoods>(Guid.NewGuid());
      await goods.Create(new CreateGoodsCommand("Смартфон", "81231234567", "img.jpg", 10000));

      goods = this.grainFactory.GetGrain<IGoods>(Guid.NewGuid());
      await goods.Create(new CreateGoodsCommand("Планшет", "81231234567", "img.jpg", 20000));

      goods = this.grainFactory.GetGrain<IGoods>(Guid.NewGuid());
      await goods.Create(new CreateGoodsCommand("Самокат", "81231234567", "img.jpg", 10000));
    }
  }

  public class GoodsViewModel
  {
    public long Id { get; set; }

    public string Name { get; set; }

    public string Photo { get; set; }

    public string Phone { get; set; }

    public decimal Price { get; set; }
  }
}