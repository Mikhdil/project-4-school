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
      var list = this.grainFactory.GetGrain<IGoodsList>(Guid.Empty);
      var allGoodsIds = await list.ListAll();
      if (allGoodsIds.Length == 0)
      {
        await this.FillSampleData();
        allGoodsIds = await list.ListAll();
      }

      var goodsStatesTaskList = new List<Task<GoodsState>>();
      foreach (var goodsId in allGoodsIds)
      {
        var goodsGrain = this.grainFactory.GetGrain<IGoods>(goodsId);
        goodsStatesTaskList.Add(goodsGrain.Info());
      }

      var allGoods = await Task.WhenAll(goodsStatesTaskList);
      foreach (var goods in allGoods)
      {
        this.Goods.Add(new GoodsViewModel()
        {
          Name = goods.Name,
          Phone = goods.Phone,
          Photo = goods.Photo,
          Price = goods.Price
        });
      }
    }

    private async Task FillSampleData()
    {
      var i = 1;
      var goods = this.grainFactory.GetGrain<IGoods>(i);
      await goods.Create(new CreateGoodsCommand("Ноутбук 16 гб оперотивной памяти", "81231234567", "img.jpg", 30000));

      goods = this.grainFactory.GetGrain<IGoods>(++i);
      await goods.Create(new CreateGoodsCommand("Чехлы для телефоноф", "81231234567", "img.jpg", 800));

      goods = this.grainFactory.GetGrain<IGoods>(++i);
      await goods.Create(new CreateGoodsCommand("Смартфон", "81231234567", "img.jpg", 10000));

      goods = this.grainFactory.GetGrain<IGoods>(++i);
      await goods.Create(new CreateGoodsCommand("Планшет", "81231234567", "img.jpg", 20000));

      goods = this.grainFactory.GetGrain<IGoods>(++i);
      await goods.Create(new CreateGoodsCommand("Самокат", "81231234567", "img.jpg", 10000));
    }
  }

  public class GoodsViewModel
  {
    public string Name { get; set; }

    public string Photo { get; set; }

    public string Phone { get; set; }

    public decimal Price { get; set; }
  }
}