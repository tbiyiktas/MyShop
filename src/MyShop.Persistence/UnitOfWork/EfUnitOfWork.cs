using MyShop.Application.Abstractions;

namespace MyShop.Persistence.UnitOfWork;

public class EfUnitOfWork : IUnitOfWork
{
    private readonly MyShopDbContext _dbContext;

    public EfUnitOfWork(MyShopDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        => _dbContext.SaveChangesAsync(cancellationToken);
}
