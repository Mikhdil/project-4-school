using Orleans.Hosting;
using TopSite.Grains;

var builder = WebApplication.CreateBuilder(args);
// Add services to the container.
builder.Services.AddRazorPages();

builder.Host.UseOrleans(builder =>
{
  builder.UseLocalhostClustering();
  builder.AddFileGrainStorageAsDefault(options =>
  {
    options.RootDirectory = Path.Combine(Environment.CurrentDirectory, "bin", "storage");
  });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
  app.UseExceptionHandler("/Error");
}
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapRazorPages();

app.Run();
