using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Orleans;
using TopSite.Grains;

namespace TopSite.Pages
{
  public class AddNewModel : PageModel
  {
    private IWebHostEnvironment _environment;

    private readonly IGrainFactory grainFactory;

    public AddNewModel(IGrainFactory grainFactory, IWebHostEnvironment environment)
    {
      this.grainFactory = grainFactory;
      this.CreatedGoods = new CreateGoodsInputModel();
      this._environment = environment;
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

      var picturePath = Path.Combine(_environment.WebRootPath, "images");
      if (!Directory.Exists(picturePath))
        Directory.CreateDirectory(picturePath);

      var goodId = Guid.NewGuid();

      var photoName = $"{goodId}-{this.CreatedGoods.Picture.FileName}";
      var file = Path.Combine(picturePath, photoName);
      using (var fileStream = new FileStream(file, FileMode.Create))
      {
        await this.CreatedGoods.Picture.CopyToAsync(fileStream);
      }

      var goods = this.grainFactory.GetGrain<IGoods>(goodId);
      await goods.Create(new CreateGoodsCommand(
        this.CreatedGoods.Name,
        this.CreatedGoods.Phone,
        Path.Combine("images", photoName),
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

    public IFormFile Picture { get; set; }
  }
}
