using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using MyShop.Application.Abstractions;
using MyShop.Application.Services;
using MyShop.Domain.Entities;
using MyShop.Persistence;
using MyShop.Persistence.Repositories;
using MyShop.Persistence.UnitOfWork;

var builder = WebApplication.CreateBuilder(args);

// Connection string (appsettings.json'dan)
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
if (string.IsNullOrWhiteSpace(connectionString))
{
    throw new InvalidOperationException("Connection string 'DefaultConnection' is not configured.");
}

// DbContext
builder.Services.AddDbContext<MyShopDbContext>(options =>
{
    options.UseSqlServer(connectionString);

#if DEBUG
    options.LogTo(Console.WriteLine);
    options.EnableSensitiveDataLogging();
    options.EnableDetailedErrors();
#endif
});

// UnitOfWork
builder.Services.AddScoped<IUnitOfWork, EfUnitOfWork>();

// Repositories
builder.Services.AddScoped<IProductRepository, ProductRepository>();
builder.Services.AddScoped<ICategoryRepository, CategoryRepository>();

// IRepository<T> mappings - delegate to specific repositories
builder.Services.AddScoped<IRepository<Product>>(sp => sp.GetRequiredService<IProductRepository>());
builder.Services.AddScoped<IRepository<Category>, CategoryRepository>();

// Include Expression Factory (Singleton - stateless)
builder.Services.AddSingleton<IIncludeExpressionFactory, MyShop.Persistence.Specifications.IncludeExpressionFactory>();

// Services
builder.Services.AddScoped<ProductService>();
builder.Services.AddScoped<CategoryService>();

// Controllers
builder.Services.AddControllers();

// Swagger / OpenAPI
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "MyShop API",
        Version = "v1",
        Description = "Backend API for MyShop sample application"
    });
});

var app = builder.Build();

// Initialize IncludeBuilder with factory from DI
using (var scope = app.Services.CreateScope())
{
    var factory = scope.ServiceProvider.GetRequiredService<IIncludeExpressionFactory>();
    MyShop.Application.Specifications.Base.IncludeBuilder.Initialize(factory);
}


// HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "MyShop API v1");
    });
}

app.UseHttpsRedirection();

// E?er ileride auth ekleyeceksen buray? a�ars?n
// app.UseAuthentication();
// app.UseAuthorization();

app.MapControllers();

app.Run();

