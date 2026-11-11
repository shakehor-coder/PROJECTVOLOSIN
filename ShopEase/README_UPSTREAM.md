# ShopEase — ASP.NET Core MVC E-Commerce Application

A full-featured e-commerce web application built with ASP.NET Core 8 MVC, Entity Framework Core, and ASP.NET Core Identity. It supports a customer-facing storefront with a session-based shopping cart, a full checkout flow, and a dedicated admin panel for managing products, categories, and orders.

---

## Table of Contents

- [Features](#features)
- [Tech Stack](#tech-stack)
- [Project Structure](#project-structure)
- [Architecture](#architecture)
- [Domain Models](#domain-models)
- [Repository Layer](#repository-layer)
- [Getting Started](#getting-started)
  - [Prerequisites](#prerequisites)
  - [Database Setup](#database-setup)
  - [Running the Application](#running-the-application)
- [Default Credentials](#default-credentials)
- [Customer Features](#customer-features)
- [Admin Panel](#admin-panel)
- [Session Cart](#session-cart)
- [Authentication & Authorization](#authentication--authorization)
- [Database Schema](#database-schema)
- [Seeded Data](#seeded-data)

---

## Features

### Customer-Facing
- Browse products by category with search, sort, and pagination
- Product detail pages with category info
- Session-based shopping cart (add, update quantity, remove)
- Secure checkout with saved or new shipping addresses
- Atomic order placement with real-time stock validation and decrement
- Order history and detailed order view with status tracking

### Admin Panel
- Full CRUD for **Products** (name, SKU, price, stock, category, active flag)
- Full CRUD for **Categories** (flat or hierarchical with parent/child support)
- Order management: list all orders, view details, update order status
  (Pending → Processing → Shipped → Delivered → Cancelled)

### Auth & Identity
- User registration with Full Name, email, and password
- Automatic role assignment (`Customer`) on registration
- Role-based authorization (`Admin` vs `Customer`)
- Admin account seeded automatically at startup

---

## Tech Stack

| Layer | Technology |
|---|---|
| Framework | ASP.NET Core 8 MVC |
| ORM | Entity Framework Core 8 (SQL Server) |
| Identity | ASP.NET Core Identity |
| Database | SQL Server (Express) |
| Frontend | Bootstrap 5, jQuery |
| Session | ASP.NET Core Session + System.Text.Json |
| Architecture | Repository Pattern (Generic Base + Specialized) |

### NuGet Packages

| Package | Version |
|---|---|
| `Microsoft.AspNetCore.Identity.EntityFrameworkCore` | 8.0.24 |
| `Microsoft.AspNetCore.Identity.UI` | 8.0.24 |
| `Microsoft.EntityFrameworkCore.SqlServer` | 8.0.24 |
| `Microsoft.EntityFrameworkCore.Tools` | 8.0.24 |
| `Microsoft.AspNetCore.Diagnostics.EntityFrameworkCore` | 8.0.24 |
| `Microsoft.VisualStudio.Web.CodeGeneration.Design` | 8.0.7 |

---

## Project Structure

```
Ecommerce/
├── Areas/
│   ├── Admin/
│   │   ├── Controllers/
│   │   │   ├── CategoriesController.cs   # Admin CRUD for categories
│   │   │   ├── OrdersController.cs       # Admin order list + status update
│   │   │   └── ProductsController.cs     # Admin CRUD for products
│   │   └── Views/
│   │       ├── Categories/               # Index, Create, Edit, Delete
│   │       ├── Orders/                   # Index, Details
│   │       └── Products/                 # Index, Create, Edit, Delete
│   └── Identity/
│       └── Pages/
│           └── Account/
│               └── Register.cshtml       # Scaffolded with FullName field
├── Controllers/
│   ├── CartController.cs                 # Session cart management
│   ├── CatalogController.cs              # Product listing + details
│   ├── CheckoutController.cs             # Checkout + atomic order placement
│   ├── HomeController.cs                 # Home page + featured products
│   └── OrdersController.cs              # Customer order history + details
├── Data/
│   ├── ApplicationDbContext.cs           # EF Core context + Fluent API config
│   ├── DbInitializer.cs                  # Seeds categories and products
│   └── Migrations/
├── Helpers/
│   └── CartHelper.cs                     # Session cart serialization helper
├── Models/
│   ├── Address.cs
│   ├── AppUser.cs
│   ├── Category.cs
│   ├── Order.cs
│   ├── OrderItem.cs
│   └── Product.cs
├── Repositories/
│   ├── EntityRepository.cs               # Generic base repository
│   ├── CategoryRepository.cs
│   ├── ProductRepository.cs
│   ├── OrderRepository.cs
│   └── AddressRepository.cs
├── ViewModels/
│   ├── CartVM.cs
│   ├── CategoryVM.cs
│   ├── CheckoutVM.cs
│   ├── OrderDetailsVM.cs
│   ├── ProductDetailsVM.cs
│   └── ProductListVM.cs
├── Views/
│   ├── Cart/Index.cshtml
│   ├── Catalog/Index.cshtml + Details.cshtml
│   ├── Checkout/Index.cshtml
│   ├── Home/Index.cshtml
│   ├── Orders/Index.cshtml + Details.cshtml
│   └── Shared/_Layout.cshtml
├── appsettings.json
├── Program.cs
└── Ecommerce.csproj
```

---

## Architecture

```
┌─────────────────────────────────────────────────────────────────┐
│                       ASP.NET Core 8 MVC                        │
├──────────────────────────────┬──────────────────────────────────┤
│    Customer-Facing Area      │          Admin Area               │
│  /Controllers                │  /Areas/Admin/Controllers         │
│  • HomeController            │  • ProductsController (CRUD)      │
│  • CatalogController         │  • CategoriesController (CRUD)    │
│  • CartController            │  • OrdersController               │
│  • CheckoutController [Auth] │    (list + status update)         │
│  • OrdersController  [Auth]  │    [Auth: Role = Admin]           │
├──────────────────────────────┴──────────────────────────────────┤
│                       Repository Layer                           │
│  IEntityRepository<T, TKey>  (generic base)                     │
│  ICategoryRepository  •  IProductRepository                     │
│  IOrderRepository     •  IAddressRepository                     │
├─────────────────────────────────────────────────────────────────┤
│            Entity Framework Core 8  (SQL Server)                │
│            ApplicationDbContext : IdentityDbContext<AppUser>     │
├─────────────────────────────────────────────────────────────────┤
│                        Domain Models                             │
│   AppUser • Category • Product • Order • OrderItem • Address    │
├─────────────────────────────────────────────────────────────────┤
│                   Cross-Cutting Concerns                         │
│  • ASP.NET Core Identity   (roles: Admin, Customer)             │
│  • Session Cart            (CartHelper + System.Text.Json)      │
│  • DB Transaction          (atomic checkout)                    │
│  • DbInitializer           (seed on startup)                    │
└─────────────────────────────────────────────────────────────────┘
```

### Key Design Decisions

- **Repository Pattern** — a generic `EntityRepository<T, TKey>` base class handles common CRUD operations. Specialized repositories (e.g., `ProductRepository`) override `GetAll`/`GetById` to include navigation properties via eager loading.
- **Session-Based Cart** — no database cart table. The cart is a `List<CartItemVM>` serialized to JSON in the ASP.NET Core session (in-memory).
- **Atomic Checkout** — `CheckoutController` wraps the entire order placement in a `BeginTransaction()` / `Commit()` / `Rollback()` block to ensure stock decrement and order creation are all-or-nothing.
- **Soft Delete for Products** — an `IsActive` flag allows hiding products from the catalog without deleting them from the database.
- **Hierarchical Categories** — `Category` supports a self-referencing `ParentCategoryId` for nested subcategories.
- **Unique Constraints** — enforced on `Category.Name`, `Product.SKU`, and `Order.OrderNumber`.

---

## Domain Models

### `AppUser`
Extends `IdentityUser` with:
- `FullName` (string)
- Navigation: `ICollection<Address>`, `ICollection<Order>`

### `Category`
| Property | Type | Notes |
|---|---|---|
| `CategoryId` | int | PK |
| `Name` | string | Unique |
| `ParentCategoryId` | int? | Self-referencing FK |
| `ParentCategory` | Category? | Nav property |
| `SubCategories` | ICollection\<Category\> | Nav property |
| `Products` | ICollection\<Product\> | Nav property |

### `Product`
| Property | Type | Notes |
|---|---|---|
| `ProductId` | int | PK |
| `CategoryId` | int | FK |
| `Name` | string | |
| `SKU` | string | Unique |
| `Price` | decimal(18,2) | |
| `StockQuantity` | int | Decremented on checkout |
| `IsActive` | bool | Soft-delete flag |
| `CreatedAt` | DateTime | Default: UtcNow |

### `Order`
| Property | Type | Notes |
|---|---|---|
| `OrderId` | int | PK |
| `UserId` | string | FK → AppUser |
| `ShippingAddressId` | int | FK → Address |
| `OrderNumber` | string | Unique |
| `Status` | OrderStatus | Enum (0–4) |
| `OrderDate` | DateTime | Default: UtcNow |
| `TotalAmount` | decimal(18,2) | |

**OrderStatus enum:** `Pending = 0`, `Processing = 1`, `Shipped = 2`, `Delivered = 3`, `Cancelled = 4`

### `OrderItem`
| Property | Type | Notes |
|---|---|---|
| `OrderItemId` | int | PK |
| `OrderId` | int | FK → Order (Cascade delete) |
| `ProductId` | int | FK → Product (Restrict) |
| `UnitPrice` | decimal(18,2) | Price at time of order |
| `Quantity` | int | |
| `LineTotal` | decimal(18,2) | UnitPrice × Quantity |

### `Address`
| Property | Type | |
|---|---|---|
| `AddressId` | int | PK |
| `UserId` | string | FK → AppUser |
| `Country` | string | |
| `City` | string | |
| `Street` | string | |
| `Zip` | string | |
| `IsDefault` | bool | |

---

## Repository Layer

### Generic Base

```csharp
IEntityRepository<T, TKey>
  GetAll()            → IEnumerable<T>
  GetById(id)         → T?
  Add(entity)         → void
  Update(entity)      → void
  Delete(id)          → void
  FindBy(predicate)   → IEnumerable<T>
  SaveChanges()       → void
```

### Specialized Repositories

| Repository | Extra Methods |
|---|---|
| `CategoryRepository` | `GetAll()` includes `ParentCategory`; `GetById()` includes `ParentCategory` + `Products` |
| `ProductRepository` | `GetFiltered(categoryId, search, sort, page, pageSize)` and `GetFilteredCount(categoryId, search)` |
| `OrderRepository` | `GetByUserId(userId)` — orders for a specific user, newest first |
| `AddressRepository` | `GetByUserId(userId)` — all addresses for a specific user |

`ProductRepository.GetFiltered` supports:
- Filter by category (`categoryId`)
- Text search on product name
- Sorting: `price_asc`, `price_desc`, `name`, or default (newest)
- Skip/take pagination

---

## Getting Started

### Prerequisites

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [SQL Server Express](https://www.microsoft.com/en-us/sql-server/sql-server-downloads) (or any SQL Server edition)
- [Visual Studio 2022](https://visualstudio.microsoft.com/) or VS Code with the C# extension

### Database Setup

1. Update the connection string in `appsettings.json` to point to your SQL Server instance:

```json
"ConnectionStrings": {
  "DefaultConnection": "Server=YOUR_SERVER\\SQLEXPRESS;Initial Catalog=EcommerceDB;Integrated Security=True;Encrypt=False;Trust Server Certificate=True"
}
```

2. Apply the Entity Framework migrations to create the database schema:

```bash
dotnet ef database update
```

The database `EcommerceDB` will be created with all tables. On first run, the application automatically seeds:
- Roles: `Admin`, `Customer`
- Admin user account
- 10 product categories and 54 products

### Running the Application

```bash
cd "d:\study\MVC_Project\Ecommerce"
dotnet run
```

Or press **F5** in Visual Studio to start with the debugger.

The app will be accessible at `https://localhost:PORT` (check the terminal output or `launchSettings.json` for the exact port).

---

## Default Credentials

An admin account is seeded automatically at startup:

| Field | Value |
|---|---|
| Email | `[configure SeedAdmin:Email]` |
| Password | `[configure SeedAdmin:Password]` |
| Role | `Admin` |

> **Note:** Change these credentials before deploying to any non-local environment.

New users who register via `/Identity/Account/Register` are automatically assigned the `Customer` role.

---

## Customer Features

### Home Page (`/`)
- Hero banner with a call-to-action
- "Shop by Category" grid showing all top-level categories
- "New Arrivals" section showing the 8 most recently added products

### Product Catalog (`/Catalog`)
- Filter by category using a dropdown
- Full-text search by product name
- Sort by: Newest, Price Low→High, Price High→Low, Name A→Z
- Paginated results (8 products per page)

### Product Details (`/Catalog/Details/{id}`)
- Product name, category, price, stock status
- "Add to Cart" button

### Shopping Cart (`/Cart`)
- View all items with per-line totals and cart grand total
- Update item quantities inline
- Remove individual items
- Proceed to Checkout

### Checkout (`/Checkout`) — *Requires login*
- Select a previously saved shipping address **or** enter a new one
- Order summary with all cart items and total
- Atomic order placement:
  - Validates stock availability for every item
  - Decrements stock for every item
  - Creates `Order` and `OrderItem` records
  - Clears the session cart on success

### My Orders (`/Orders`) — *Requires login*
- Lists all past orders with order number, date, status badge, and total
- Drill into any order for full details (items, quantities, prices, shipping address)

---

## Admin Panel

Accessible at `/Admin/*` — requires the `Admin` role.

The Admin dropdown in the navbar is only visible to admin users.

### Products (`/Admin/Products`)
- List all products (including inactive)
- Create new product: name, SKU, price, stock quantity, category, active flag
- Edit existing products
- Delete products (with confirmation page)

### Categories (`/Admin/Categories`)
- List all categories with their parent category
- Create new category with optional parent
- Edit categories (self is excluded from the parent dropdown to prevent circular references)
- Delete categories

### Orders (`/Admin/Orders`)
- List all orders across all customers
- View order details: customer info, shipping address, all line items
- Update order status from a dropdown (Pending / Processing / Shipped / Delivered / Cancelled)

---

## Session Cart

The shopping cart is stored entirely in the ASP.NET Core session using `CartHelper`:

```
Session key: "ShoppingCart"
Value:        JSON-serialized List<CartItemVM>
```

`CartHelper` provides three static methods:
- `GetCart(session)` — deserializes cart from session, or returns an empty list
- `SaveCart(session, cart)` — serializes and saves cart to session
- `ClearCart(session)` — removes the cart key from the session

The session uses an **in-memory distributed cache** (no Redis or SQL session backing). Cart data is lost if the server restarts.

---

## Authentication & Authorization

| Area | Requirement |
|---|---|
| Home, Catalog, Cart | Public (no login required) |
| Checkout, My Orders | `[Authorize]` — any logged-in user |
| Admin Panel | `[Authorize(Roles = "Admin")]` |

Identity pages (Login, Logout, Manage account, etc.) are provided by the `Microsoft.AspNetCore.Identity.UI` package. Only the **Register** page is scaffolded to include the custom `FullName` field.

Cookie configuration:
- Login path: `/Identity/Account/Login`
- Access denied path: `/Identity/Account/AccessDenied`

---

## Database Schema

```
AppUser (IdentityUser)
  └── Addresses (1:N)
  └── Orders    (1:N)

Category
  └── ParentCategory (self-ref, optional)
  └── SubCategories  (1:N)
  └── Products       (1:N, Restrict on delete)

Product
  └── Category   (N:1, Restrict on delete)
  └── OrderItems (1:N)

Order
  └── AppUser         (N:1, Restrict on delete)
  └── ShippingAddress (N:1, Restrict on delete)
  └── OrderItems      (1:N, Cascade on delete)

OrderItem
  └── Order   (N:1)
  └── Product (N:1)

Address
  └── AppUser (N:1)
  └── Orders  (1:N)
```

---

## Seeded Data

`DbInitializer.Seed()` runs at startup and inserts data only when tables are empty.

### Categories (10 top-level)
Electronics, Fashion, Home & Kitchen, Books, Beauty & Personal Care, Sports & Fitness, Toys & Games, Automotive, Grocery, Office Supplies

### Products (54 total)

| Category | Count |
|---|---|
| Electronics | 8 |
| Fashion | 5 |
| Home & Kitchen | 5 |
| Books | 4 |
| Beauty & Personal Care | 3 |
| Sports & Fitness | 5 |
| Toys & Games | 4 |
| Automotive | 4 |
| Grocery | 8 |
| Office Supplies | 8 |

Each product has a unique SKU, realistic price, and initial stock quantity.
