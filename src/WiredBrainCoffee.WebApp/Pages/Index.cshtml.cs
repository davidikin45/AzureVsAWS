using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.RazorPages;
using WiredBrainCoffee.Storage;

namespace WiredBrainCoffee.WebApp.Pages
{
  public class IndexModel : PageModel
  {
    private readonly ICoffeeVideoStorage _coffeeVideoStorage;

    public IndexModel(ICoffeeVideoStorage coffeeVideoStorage)
    {
      _coffeeVideoStorage = coffeeVideoStorage;
    }

    public IEnumerable<CoffeeVideoModel> CoffeeVideoModels { get; private set; }

    public async Task OnGet()
    {
      CoffeeVideoModels = await LoadCoffeeVideoModelsAsync();
    }

    private async Task<IEnumerable<CoffeeVideoModel>> LoadCoffeeVideoModelsAsync()
    {
      var coffeeVideoModels = new List<CoffeeVideoModel>();

      var cloudBlockBlobs = await _coffeeVideoStorage.ListVideoBlobsAsync();

      foreach (var cloudBlockBlob in cloudBlockBlobs)
      {
        var (title, description) = _coffeeVideoStorage.GetBlobMetadata(cloudBlockBlob);
        coffeeVideoModels.Add(new CoffeeVideoModel
        {
          Title = title,
          Description = description,
          BlobUri = _coffeeVideoStorage.GetBlobUriWithSasToken(cloudBlockBlob)
        });
      }

      return coffeeVideoModels;
    }
  }

  public class CoffeeVideoModel
  {
    public string Title { get; set; }
    public string Description { get; set; }
    public string BlobUri { get; set; }
  }
}
