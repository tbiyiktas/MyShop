using MyShop.Application.Specifications.Base;
using MyShop.Domain.Entities;

namespace MyShop.Application.Examples;

/// <summary>
/// Examples demonstrating advanced Include support with ThenInclude and AsSplitQuery.
/// </summary>
public static class AdvancedIncludeExamples
{
    /// <summary>
    /// Example 1: New ApplyInclude with fluent API
    /// </summary>
    public static void Example1_NewIncludeAPI()
    {
        var spec = SpecificationBuilder<Product>.Create()
            .Where(p => p.IsActive)
            .AsNoTracking()
            .Build();

        // Note: We can use ApplyInclude in custom specifications
        // See Example2 for how to create custom spec with ApplyInclude
    }

    /// <summary>
    /// Example 3: Custom Specification with ApplyInclude
    /// </summary>
    public class ProductWithCategorySpec : Specification<Product>
    {
        public ProductWithCategorySpec()
        {
            Criteria = p => p.IsActive;

            // NEW: Fluent include API
            ApplyInclude(builder =>
            {
                builder.Include(p => p.Category);
                // If Category had ParentCategory, we could do:
                // builder.Include(p => p.Category)
                //        .ThenInclude(c => c.ParentCategory);
            });

            ApplyAsNoTracking();
        }
    }

    /// <summary>
    /// Example 4: String-based includes (for dynamic scenarios)
    /// </summary>
    public class ProductWithStringIncludeSpec : Specification<Product>
    {
        public ProductWithStringIncludeSpec(string includePath)
        {
            Criteria = p => p.IsActive;

            // String-based include
            ApplyInclude(builder =>
            {
                builder.Include(includePath);
                // Example: "Category" or "Category.ParentCategory"
            });

            ApplyAsNoTracking();
        }
    }

    /// <summary>
    /// Example 5: AsSplitQuery to prevent cartesian explosion
    /// (Useful when Product has multiple collection navigations in the future)
    /// </summary>
    public class ProductWithSplitQuerySpec : Specification<Product>
    {
        public ProductWithSplitQuerySpec()
        {
            Criteria = p => p.IsActive;

            ApplyInclude(builder =>
            {
                builder.Include(p => p.Category);
                // If Product had Images and Reviews:
                // builder.Include(p => p.Images);
                // builder.Include(p => p.Reviews);
            });

            // Prevent cartesian explosion
            ApplyAsSplitQuery();
            ApplyAsNoTracking();
        }
    }

    /// <summary>
    /// Example 6: Future scenario - ThenInclude with nested navigation
    /// (This will work when Product/Category model is extended)
    /// </summary>
    public class FutureNestedIncludeSpec : Specification<Product>
    {
        public FutureNestedIncludeSpec()
        {
            Criteria = p => p.IsActive;

            ApplyInclude(builder =>
            {
                // Future: When Category has ParentCategory
                builder.Include(p => p.Category);
                    // .ThenInclude(c => c.ParentCategory);

                // Future: When Product has Supplier with Country
                // builder.Include(p => p.Supplier)
                //        .ThenInclude(s => s.Country);

                // Future: When Product has Reviews with User
                // builder.Include(p => p.Reviews)
                //        .ThenInclude(r => r.User);
            });

            ApplyAsSplitQuery();  // Important for multiple collections!
            ApplyAsNoTracking();
        }
    }
}
