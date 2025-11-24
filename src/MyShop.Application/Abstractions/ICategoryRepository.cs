using MyShop.Domain.Entities;

namespace MyShop.Application.Abstractions;

public interface ICategoryRepository : IRepository<Category>
{
    // Category-specific repository methods can be added here if needed
}
