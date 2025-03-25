using InventoryHub.Backend.Models;
using Microsoft.Extensions.Caching.Memory;

var builder = WebApplication.CreateBuilder(args);

// Add CORS policy to allow requests from the specified origin
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(builder =>
    {
        builder.WithOrigins("https://localhost:7118")
               .AllowAnyHeader()
               .AllowAnyMethod();
    });
});

// Add services for API endpoint exploration and Swagger generation
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Add in-memory caching services
builder.Services.AddMemoryCache();

var app = builder.Build();

// Use the configured CORS policy
app.UseCors();

// Define an endpoint to get the product list
app.MapGet("/api/productlist", (IMemoryCache cache) =>
{
    const string cacheKey = "productList";
    if (!cache.TryGetValue(cacheKey, out Product[] products))
    {
        // If the product list is not in the cache, create it
        products = GetProductList();

        // Set cache options
        var cacheEntryOptions = new MemoryCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5), // Cache for 5 minutes
            SlidingExpiration = TimeSpan.FromMinutes(2) // Reset expiration time if accessed within 2 minutes
        };

        // Save the product list in the cache
        cache.Set(cacheKey, products, cacheEntryOptions);
    }

    // Return the product list as a JSON response
    return Results.Ok(products);
});

// Enable Swagger UI in development environment
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Run the application
app.Run();

// Method to create the product list
Product[] GetProductList()
{
    return new[]
    {
        new Product
        {
            Id = 1,
            Name = "Laptop",
            Price = 1200.50,
            Stock = 25,
            Category = new Category { Id = 101, Name = "Electronics" }
        },
        new Product
        {
            Id = 2,
            Name = "Headphones",
            Price = 50.00,
            Stock = 100,
            Category = new Category { Id = 102, Name = "Accessories" }
        }
    };
}
