using Ecommerce.Models;

namespace Ecommerce.Data
{
    public static class DbInitializer
    {
        public static void Seed(ApplicationDbContext context)
        {

            if (!context.Categories.Any())
            {
                var categories = new Category[]
                {
                    new Category { Name = "Electronics" },
                    new Category { Name = "Fashion" },
                    new Category { Name = "Home & Kitchen" },
                    new Category { Name = "Books" },
                    new Category { Name = "Beauty & Personal Care" },
                    new Category { Name = "Sports & Fitness" },
                    new Category { Name = "Toys & Games" },
                    new Category { Name = "Automotive" },
                    new Category { Name = "Grocery" },
                    new Category { Name = "Office Supplies" }
                };
                context.Categories.AddRange(categories);
                context.SaveChanges();
            }

            if (!context.Products.Any())
            {
                var dbCategories = context.Categories.ToList();
                var getCatId = (string name) => dbCategories.FirstOrDefault(c => c.Name == name)?.CategoryId ?? dbCategories[0].CategoryId;

                var products = new Product[]
                {
                    // Electronics (8)
                    new Product { Name = "MacBook Air M2 13-inch", SKU = "EL-AP-MBA13", Price = 999.99m, StockQuantity = 7, CategoryId = getCatId("Electronics") },
                    new Product { Name = "Samsung Galaxy S24 Ultra", SKU = "EL-SS-S24U", Price = 1199.99m, StockQuantity = 10, CategoryId = getCatId("Electronics") },
                    new Product { Name = "LG 65 Inch OLED TV", SKU = "EL-LG-OLED65", Price = 1499.99m, StockQuantity = 4, CategoryId = getCatId("Electronics") },
                    new Product { Name = "JBL Bluetooth Speaker Charge 5", SKU = "EL-JB-CH5", Price = 179.99m, StockQuantity = 18, CategoryId = getCatId("Electronics") },
                    new Product { Name = "Dell 27 Inch Monitor 144Hz", SKU = "EL-DL-MN27", Price = 349.99m, StockQuantity = 9, CategoryId = getCatId("Electronics") },
                    new Product { Name = "iPhone 15 Pro", SKU = "EL-AP-IP15", Price = 1099.99m, StockQuantity = 15, CategoryId = getCatId("Electronics") },
                    new Product { Name = "Sony WH-1000XM5 Headphones", SKU = "EL-SN-WH5", Price = 349.99m, StockQuantity = 20, CategoryId = getCatId("Electronics") },
                    new Product { Name = "Sony DualSense Controller", SKU = "EL-SN-PS5C", Price = 69.99m, StockQuantity = 25, CategoryId = getCatId("Electronics") },

                    // Fashion (5)
                    new Product { Name = "Adidas Hoodie Essentials", SKU = "FS-AD-HOOD", Price = 64.99m, StockQuantity = 25, CategoryId = getCatId("Fashion") },
                    new Product { Name = "Women's Leather Handbag Black", SKU = "FS-WM-HB", Price = 89.99m, StockQuantity = 14, CategoryId = getCatId("Fashion") },
                    new Product { Name = "Casio Stainless Steel Watch", SKU = "FS-CS-WT", Price = 59.99m, StockQuantity = 20, CategoryId = getCatId("Fashion") },
                    new Product { Name = "Men's Leather Jacket", SKU = "FS-MN-LJ", Price = 149.99m, StockQuantity = 10, CategoryId = getCatId("Fashion") },
                    new Product { Name = "Nike Air Zoom Pegasus Running Shoes", SKU = "FS-NK-RUN", Price = 129.99m, StockQuantity = 20, CategoryId = getCatId("Fashion") },

                    // Home & Kitchen (5)
                    new Product { Name = "Air Fryer 5L Digital", SKU = "HK-PH-AF", Price = 119.99m, StockQuantity = 12, CategoryId = getCatId("Home & Kitchen") },
                    new Product { Name = "Espresso Coffee Machine 15-Bar", SKU = "HK-DL-EM", Price = 249.99m, StockQuantity = 5, CategoryId = getCatId("Home & Kitchen") },
                    new Product { Name = "Microwave Oven 30L Digital", SKU = "HK-MW-30L", Price = 159.99m, StockQuantity = 8, CategoryId = getCatId("Home & Kitchen") },
                    new Product { Name = "Blender 1000W High Speed", SKU = "HK-BL-1000", Price = 69.99m, StockQuantity = 15, CategoryId = getCatId("Home & Kitchen") },
                    new Product { Name = "Vacuum Cleaner Bagless", SKU = "HK-VC-BG", Price = 199.99m, StockQuantity = 11, CategoryId = getCatId("Home & Kitchen") },

                    // Books (4)
                    new Product { Name = "The Lean Startup by Eric Ries", SKU = "BK-BS-LS", Price = 16.99m, StockQuantity = 50, CategoryId = getCatId("Books") },
                    new Product { Name = "Atomic Habits by James Clear", SKU = "BK-SF-AH", Price = 18.99m, StockQuantity = 45, CategoryId = getCatId("Books") },
                    new Product { Name = "Deep Work by Cal Newport", SKU = "BK-PD-DW", Price = 15.99m, StockQuantity = 40, CategoryId = getCatId("Books") },
                    new Product { Name = "Clean Code by Robert C. Martin", SKU = "BK-PR-CC", Price = 39.99m, StockQuantity = 30, CategoryId = getCatId("Books") },

                    // Beauty & Personal Care (3)
                    new Product { Name = "Hair Dryer Ionic 2200W", SKU = "BT-HD-2200", Price = 49.99m, StockQuantity = 28, CategoryId = getCatId("Beauty & Personal Care") },
                    new Product { Name = "Men's Beard Grooming Kit", SKU = "BT-BG-MN", Price = 34.99m, StockQuantity = 19, CategoryId = getCatId("Beauty & Personal Care") },
                    new Product { Name = "Skin Care Routine Kit", SKU = "BT-OR-SK", Price = 59.99m, StockQuantity = 30, CategoryId = getCatId("Beauty & Personal Care") },

                    // Sports & Fitness (5)
                    new Product { Name = "Adjustable Dumbbells 20KG", SKU = "SP-DB-20", Price = 79.99m, StockQuantity = 25, CategoryId = getCatId("Sports & Fitness") },
                    new Product { Name = "Electric Treadmill Foldable", SKU = "SP-TM-EL", Price = 499.99m, StockQuantity = 6, CategoryId = getCatId("Sports & Fitness") },
                    new Product { Name = "Yoga Mat Anti-Slip", SKU = "SP-YG-MAT", Price = 24.99m, StockQuantity = 40, CategoryId = getCatId("Sports & Fitness") },
                    new Product { Name = "Protein Powder Whey 2KG", SKU = "SP-WH-2KG", Price = 54.99m, StockQuantity = 18, CategoryId = getCatId("Sports & Fitness") },
                    new Product { Name = "Exercise Bike Magnetic", SKU = "SP-EB-MG", Price = 349.99m, StockQuantity = 5, CategoryId = getCatId("Sports & Fitness") },

                    // Toys & Games (4)
                    new Product { Name = "LEGO Classic Box", SKU = "TY-LG-CL", Price = 39.99m, StockQuantity = 40, CategoryId = getCatId("Toys & Games") },
                    new Product { Name = "Remote Control Car High Speed", SKU = "TY-RC-CAR", Price = 49.99m, StockQuantity = 22, CategoryId = getCatId("Toys & Games") },
                    new Product { Name = "Monopoly Classic Board Game", SKU = "TY-MN-CL", Price = 24.99m, StockQuantity = 30, CategoryId = getCatId("Toys & Games") },
                    new Product { Name = "Puzzle 1000 Pieces Landscape", SKU = "TY-PZ-1000", Price = 14.99m, StockQuantity = 35, CategoryId = getCatId("Toys & Games") },

                    // Automotive (4)
                    new Product { Name = "Car Vacuum Cleaner Portable", SKU = "AU-VC-01", Price = 44.99m, StockQuantity = 30, CategoryId = getCatId("Automotive") },
                    new Product { Name = "Car Phone Holder Dashboard Mount", SKU = "AU-PH-02", Price = 14.99m, StockQuantity = 50, CategoryId = getCatId("Automotive") },
                    new Product { Name = "Car Air Compressor Portable", SKU = "AU-AC-01", Price = 39.99m, StockQuantity = 22, CategoryId = getCatId("Automotive") },
                    new Product { Name = "Dash Cam Full HD 1080p", SKU = "AU-DC-1080", Price = 69.99m, StockQuantity = 16, CategoryId = getCatId("Automotive") },

                    // Grocery (8)
                    new Product { Name = "Basmati Rice 5KG", SKU = "GR-RC-5KG", Price = 12.99m, StockQuantity = 50, CategoryId = getCatId("Grocery") },
                    new Product { Name = "Sunflower Cooking Oil 2L", SKU = "GR-OIL-2L", Price = 6.99m, StockQuantity = 75, CategoryId = getCatId("Grocery") },
                    new Product { Name = "Granulated Sugar 1KG", SKU = "GR-SG-1KG", Price = 2.49m, StockQuantity = 120, CategoryId = getCatId("Grocery") },
                    new Product { Name = "Full Cream Milk 1L", SKU = "GR-ML-1L", Price = 1.99m, StockQuantity = 90, CategoryId = getCatId("Grocery") },
                    new Product { Name = "Spaghetti Pasta 500g", SKU = "GR-PT-500", Price = 1.49m, StockQuantity = 80, CategoryId = getCatId("Grocery") },
                    new Product { Name = "Canned Tuna in Oil 170g", SKU = "GR-TN-170", Price = 2.99m, StockQuantity = 60, CategoryId = getCatId("Grocery") },
                    new Product { Name = "Tomato Paste 300g", SKU = "GR-TM-300", Price = 0.99m, StockQuantity = 110, CategoryId = getCatId("Grocery") },
                    new Product { Name = "Tea Bags 100pcs", SKU = "GR-TEA-100", Price = 4.99m, StockQuantity = 55, CategoryId = getCatId("Grocery") },

                    // Office Supplies (8)
                    new Product { Name = "A4 Copy Paper 500 Sheets", SKU = "OS-PP-A4", Price = 8.99m, StockQuantity = 40, CategoryId = getCatId("Office Supplies") },
                    new Product { Name = "Ballpoint Pen Pack (10pcs)", SKU = "OS-PN-10", Price = 4.99m, StockQuantity = 85, CategoryId = getCatId("Office Supplies") },
                    new Product { Name = "Office Stapler Heavy Duty", SKU = "OS-ST-HD", Price = 9.99m, StockQuantity = 25, CategoryId = getCatId("Office Supplies") },
                    new Product { Name = "Spiral Notebook A5", SKU = "OS-NB-A5", Price = 3.99m, StockQuantity = 70, CategoryId = getCatId("Office Supplies") },
                    new Product { Name = "Plastic Ruler 30cm", SKU = "OS-RL-30", Price = 1.49m, StockQuantity = 100, CategoryId = getCatId("Office Supplies") },
                    new Product { Name = "Highlighters Set (5 Colors)", SKU = "OS-HL-5C", Price = 5.99m, StockQuantity = 60, CategoryId = getCatId("Office Supplies") },
                    new Product { Name = "Sticky Notes 3x3 100 Sheets", SKU = "OS-SN-3x3", Price = 2.99m, StockQuantity = 90, CategoryId = getCatId("Office Supplies") },
                    new Product { Name = "Desk Organizer Tray", SKU = "OS-DT-ORG", Price = 14.99m, StockQuantity = 30, CategoryId = getCatId("Office Supplies") },
                };

                context.Products.AddRange(products);
                context.SaveChanges();
            }
        }
    }
}
