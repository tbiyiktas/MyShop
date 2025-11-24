using MyShop.Application.Abstractions;
using MyShop.Domain.Entities;

namespace MyShop.Persistence.Repositories;

public class ProductRepository : EfRepository<Product>, IProductRepository
{
    public ProductRepository(MyShopDbContext dbContext)
        : base(dbContext)
    {
    }
}
