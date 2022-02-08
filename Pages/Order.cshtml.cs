using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Orleans;
using TopSite.Grains;

namespace TopSite.Pages
{
  public class OrderModel : PageModel
  {
    private readonly IGrainFactory grainFactory;

    public OrderModel(IGrainFactory grainFactory)
    {
      this.grainFactory = grainFactory;
      this.Order = new CreateOrderInputModel();
      this.Error = String.Empty;
    }

    [BindProperty]
    public CreateOrderInputModel Order { get; set; }

    public string Error { get; set; }

    public async Task<IActionResult> OnGetAsync(long goodsId)
    {
      if (goodsId == default)
        return this.RedirectToPage("Index");

      var goodsRepository = this.grainFactory.GetGrain<IGoodsRepository>(Guid.Empty);
      var goods = await goodsRepository.GetBySurrogateId(goodsId);
      if (goods == null)
        return this.RedirectToPage("Index");

      this.Order.GoodsId = goodsId;
      var goodsInfo = await goods.Info();
      this.Order.GoodsName = goodsInfo.Name;

      return this.Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
      if (!this.ModelState.IsValid)
      {
        this.Error = "Заполните все поля формы.";
        return this.Page();
      }

      var goodsRepository = this.grainFactory.GetGrain<IGoodsRepository>(Guid.Empty);
      var goods = await goodsRepository.GetBySurrogateId(this.Order.GoodsId);
      if (goods == null)
        return this.RedirectToPage("Index");

      await goods.Delete();

      return this.RedirectToPage("Index");
    }
  }

  public class CreateOrderInputModel
  {
    [Required]
    public string PersonFullName { get; set; }

    [Required]
    public string Address { get; set; }

    [Required]
    public string DeliveryType { get; set; }

    [Required]
    public long GoodsId { get; set; }

    public string GoodsName { get; set; }

    [Required]
    public string PolicyAgree { get; set; }
  }
}
