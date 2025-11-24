using MyShop.Domain.Entities;

namespace MyShop.Application.Abstractions;

public interface IProductRepository : IRepository<Product>
{
    // Product’a özel ek query’ler varsa buraya
}
