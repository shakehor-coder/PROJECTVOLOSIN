using Ecommerce.Models;

namespace Ecommerce.Data
{
    public static class DbInitializer
    {
        public static void Seed(ApplicationDbContext context)
        {

            if (!context.Categories.Any())
            {
                var categories = SampleCatalog.CreateCategories();
                context.Categories.AddRange(categories);
                context.SaveChanges();
            }

            if (!context.Products.Any())
            {
                var dbCategories = context.Categories.ToList();
                var getCatId = (string name) => dbCategories.FirstOrDefault(c => c.Name == name)?.CategoryId ?? dbCategories[0].CategoryId;

                var products = SampleCatalog.CreateProducts(getCatId);

                context.Products.AddRange(products);
                context.SaveChanges();
            }
        }
    }
}
