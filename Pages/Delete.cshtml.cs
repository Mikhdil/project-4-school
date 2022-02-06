using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Orleans;
using TopSite.Grains;

namespace TopSite.Pages
{
  public class DeleteModel : PageModel
  {
    private readonly IGrainFactory grainFactory;

    public DeleteGoodsInputModel DeleteInputModel { get; set; }

    public DeleteModel(IGrainFactory grainFactory)
    {
      this.grainFactory = grainFactory;
      this.DeleteInputModel = new DeleteGoodsInputModel();
    }

    public async Task<IActionResult> OnGetAsync(long goodsId)
    {
      if (goodsId == default)
        return this.RedirectToPage("Index");

      var goodsRepository = this.grainFactory.GetGrain<IGoodsRepository>(Guid.Empty);
      var goods = await goodsRepository.GetBySurrogateId(goodsId);
      if (goods == null)
        return this.RedirectToPage("Index");

      this.DeleteInputModel.GoodsId = goodsId;
      var goodsInfo = await goods.Info();

      this.DeleteInputModel.GoodsName = goodsInfo.Name;
      this.DeleteInputModel.Photo = goodsInfo.Photo;
      this.DeleteInputModel.Price = goodsInfo.Price;
      this.DeleteInputModel.Phone = goodsInfo.Phone;

      return this.Page();
    }

    public async Task<IActionResult> OnPostAsync(long goodsId)
    {
      if (goodsId == default)
        return this.RedirectToPage("Index");

      var goodsRepository = this.grainFactory.GetGrain<IGoodsRepository>(Guid.Empty);
      var goods = await goodsRepository.GetBySurrogateId(goodsId);
      if (goods == null)
        return this.RedirectToPage("Index");

      await goods.Delete();
      return this.RedirectToPage("Index");
    }
  }

  public class DeleteGoodsInputModel
  {
    [Required]
    public long GoodsId { get; set; }

    public string GoodsName { get; set; }

    public string Photo { get; internal set; }

    public decimal Price { get; internal set; }

    public string Phone { get; internal set; }
  }
}
