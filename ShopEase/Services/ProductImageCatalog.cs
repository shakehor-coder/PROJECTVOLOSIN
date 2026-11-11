using System.Text.Json;

namespace Ecommerce.Services;
public sealed record ProductImageAsset(string Sku, string Url, string Title, string Author,
    string License, string Source, string LicenseUrl);

public sealed class ProductImageCatalog
{
    private readonly IReadOnlyDictionary<string, ProductImageAsset> _images;
    public ProductImageCatalog(IWebHostEnvironment environment)
    {
        var path = Path.Combine(environment.WebRootPath, "images", "product-images.json");
        var images = JsonSerializer.Deserialize<List<ProductImageAsset>>(File.ReadAllText(path),
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? [];
        _images = images.ToDictionary(i => i.Sku, StringComparer.OrdinalIgnoreCase);
    }
    public ProductImageAsset? Find(string sku) => _images.GetValueOrDefault(sku);
    public IEnumerable<ProductImageAsset> All => _images.Values.OrderBy(i => i.Sku);
}
