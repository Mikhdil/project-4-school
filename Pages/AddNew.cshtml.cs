using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Orleans;
using TopSite.Grains;

namespace TopSite.Pages
{
  public class AddNewModel : PageModel
  {
    private readonly IGrainFactory grainFactory;

    public AddNewModel(IGrainFactory grainFactory)
    {
      this.grainFactory = grainFactory;
      this.CreatedGoods = new CreateGoodsInputModel();
    }

    [BindProperty]
    public CreateGoodsInputModel CreatedGoods { get; set; }

    public void OnGet()
    {
    }

    public async Task<IActionResult> OnPostAsync()
    {
      if (!this.ModelState.IsValid)
      {
        return this.Page();
      }

      var goods = this.grainFactory.GetGrain<IGoods>(Guid.NewGuid());
      await goods.Create(new CreateGoodsCommand(
        this.CreatedGoods.Name,
        this.CreatedGoods.Phone,
        string.Empty,
        this.CreatedGoods.Price));
      return this.RedirectToPage("Index");
    }
  }

  public class CreateGoodsInputModel
  {
    [Required]
    public string Name { get; set; }

    [Required]
    [Range(0, double.MaxValue)]
    public decimal Price { get; set; }

    [Required]
    [Phone]
    public string Phone { get; set; }
  }
}
