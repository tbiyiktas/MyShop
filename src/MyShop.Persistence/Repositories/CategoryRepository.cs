using MyShop.Application.Abstractions;
using MyShop.Domain.Entities;

namespace MyShop.Persistence.Repositories;

public class CategoryRepository : EfRepository<Category>, ICategoryRepository
{
    public CategoryRepository(MyShopDbContext dbContext)
        : base(dbContext)
    {
    }
}
