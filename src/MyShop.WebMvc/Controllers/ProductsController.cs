using Microsoft.AspNetCore.Mvc;
using MyShop.Contracts.Categories;
using MyShop.Contracts.Common;
using MyShop.Contracts.Grid;
using MyShop.Contracts.Products;
using MyShop.WebMvc.Services;
using MyShop.WebMvc.ViewModel;

namespace MyShop.WebMvc.Controllers;

public class ProductsController : Controller
{
    private readonly ApiClient _apiClient;

    public ProductsController(ApiClient apiClient)
    {
        _apiClient = apiClient;
    }

    [HttpGet]
    public async Task<IActionResult> Index(ProductGridFilterViewModel model, CancellationToken cancellationToken)
    {
        // 1) Query param sözlüğünü hazırla
        var queryParams = new Dictionary<string, object?>();

        if (!string.IsNullOrWhiteSpace(model.Search))
            queryParams["search"] = model.Search;

        if (model.MinPrice.HasValue)
            queryParams["minPrice"] = model.MinPrice.Value;

        if (model.MaxPrice.HasValue)
            queryParams["maxPrice"] = model.MaxPrice.Value;

        if (!string.IsNullOrWhiteSpace(model.CategoryName))
            queryParams["categoryName"] = model.CategoryName;

        if (!string.IsNullOrWhiteSpace(model.SortBy))
            queryParams["sortBy"] = model.SortBy;

        if (!string.IsNullOrWhiteSpace(model.SortDir))
            queryParams["sortDir"] = model.SortDir;

        queryParams["pageIndex"] = model.PageIndex <= 0 ? 1 : model.PageIndex;
        queryParams["pageSize"] = model.PageSize <= 0 ? 20 : model.PageSize;

        // 2) Web API /api/products/search endpoint'ine git
        var apiResponse = await _apiClient.GetAsync<PaginatedResponse<ProductDto>>(
            "/api/Products/search",
            queryParams,
            cancellationToken);

        // 3) Hata varsa ModelState'e yaz
        if (!apiResponse.Success)
        {
            foreach (var err in apiResponse.Errors)
            {
                ModelState.AddModelError(string.Empty, err);
            }
        }

        // 4) Data'yı ViewBag ile gönder
        ViewBag.Page = apiResponse.Data;

        return View(model);
    }

    private static GridFilterRequestDto BuildGridFilterRequest(ProductGridFilterViewModel model)
    {
        var request = new GridFilterRequestDto
        {
            PageIndex = model.PageIndex <= 0 ? 1 : model.PageIndex,
            PageSize = model.PageSize <= 0 ? 20 : model.PageSize,
            GroupOperator = FilterLogicalOperator.And
        };

        var group = new FilterGroupDto
        {
            Operator = FilterLogicalOperator.And
        };

        if (!string.IsNullOrWhiteSpace(model.Search))
        {
            group.Conditions.Add(new FilterConditionDto
            {
                PropertyPath = "Name",
                Operation = FilterOperation.Contains,
                Value = model.Search,
                CaseInsensitive = true
            });
        }

        if (model.MinPrice.HasValue)
        {
            group.Conditions.Add(new FilterConditionDto
            {
                PropertyPath = "Price",
                Operation = FilterOperation.GreaterThanOrEqual,
                Value = model.MinPrice.Value.ToString(System.Globalization.CultureInfo.InvariantCulture)
            });
        }

        if (model.MaxPrice.HasValue)
        {
            group.Conditions.Add(new FilterConditionDto
            {
                PropertyPath = "Price",
                Operation = FilterOperation.LessThanOrEqual,
                Value = model.MaxPrice.Value.ToString(System.Globalization.CultureInfo.InvariantCulture)
            });
        }

        if (!string.IsNullOrWhiteSpace(model.CategoryName))
        {
            group.Conditions.Add(new FilterConditionDto
            {
                PropertyPath = "Category.Name",
                Operation = FilterOperation.Equals,
                Value = model.CategoryName,
                CaseInsensitive = true
            });
        }

        if (group.Conditions.Count > 0)
        {
            request.Groups.Add(group);
        }

        // Sort
        var sorts = SortCriterion.CreateMany(model.SortBy, model.SortDir)
            .Select(sc => new SortDto
            {
                PropertyPath = sc.PropertyPath,
                Descending = sc.Descending
            })
            .ToList();

        request.Sorts = sorts;

        return request;
    }

    [HttpGet]
    public IActionResult GridDesigner()
    {
        return View();
    }

    [HttpGet]
    public async Task<IActionResult> GetProductGridMetadata(CancellationToken cancellationToken)
    {
        var apiResponse = await _apiClient.GetAsync<GridMetadataDto>(
            "/api/metadata/products-grid",
            null,
            cancellationToken);

        return Json(apiResponse);
    }

    [HttpPost]
    public async Task<IActionResult> GridSearchApi(
        [FromBody] GridFilterRequestDto request,
        CancellationToken cancellationToken)
    {
        var apiResponse = await _apiClient.PostAsync<GridFilterRequestDto, PaginatedResponse<ProductDto>>(
            "/api/products/grid-search",
            request,
            null,
            cancellationToken);

        return Json(apiResponse);
    }

    [HttpGet]
    [Route("/api/categories/lookup")]
    public async Task<IActionResult> CategoriesLookup(CancellationToken cancellationToken)
    {
        var apiResponse = await _apiClient.GetAsync<List<CategoryResponse>>(
            "/api/categories/lookup",
            null,
            cancellationToken);

        return Json(apiResponse);
    }

    // GET: /Products/Details/5
    [HttpGet]
    public async Task<IActionResult> Details(int id, CancellationToken cancellationToken)
    {
        var apiResponse = await _apiClient.GetAsync<ProductDto>(
            $"/api/Products/{id}",
            null,
            cancellationToken);

        if (!apiResponse.Success)
        {
            TempData["Error"] = string.Join(", ", apiResponse.Errors);
            return RedirectToAction(nameof(Index));
        }

        return View(apiResponse.Data);
    }

    // GET: /Products/Create
    [HttpGet]
    public async Task<IActionResult> Create(CancellationToken cancellationToken)
    {
        await LoadCategoriesForDropdown(cancellationToken);
        return View();
    }

    // POST: /Products/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(
        CreateProductRequest request,
        CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            await LoadCategoriesForDropdown(cancellationToken);
            return View(request);
        }

        var apiResponse = await _apiClient.PostAsync<CreateProductRequest, ProductDto>(
            "/api/Products",
            request,
            null,
            cancellationToken);

        if (!apiResponse.Success)
        {
            foreach (var err in apiResponse.Errors)
            {
                ModelState.AddModelError(string.Empty, err);
            }
            await LoadCategoriesForDropdown(cancellationToken);
            return View(request);
        }

        TempData["Success"] = "Product created successfully!";
        return RedirectToAction(nameof(Index));
    }

    // GET: /Products/Edit/5
    [HttpGet]
    public async Task<IActionResult> Edit(int id, CancellationToken cancellationToken)
    {
        var apiResponse = await _apiClient.GetAsync<ProductDto>(
            $"/api/Products/{id}",
            null,
            cancellationToken);

        if (!apiResponse.Success)
        {
            TempData["Error"] = string.Join(", ", apiResponse.Errors);
            return RedirectToAction(nameof(Index));
        }

        var updateRequest = new UpdateProductRequest
        {
            Name = apiResponse.Data!.Name,
            Price = apiResponse.Data.Price,
            StockQuantity = apiResponse.Data.StockQuantity,
            CategoryId = apiResponse.Data.CategoryId,
            IsActive = apiResponse.Data.IsActive
        };

        ViewBag.ProductId = id;
        await LoadCategoriesForDropdown(cancellationToken);
        return View(updateRequest);
    }

    // POST: /Products/Edit/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(
        int id,
        UpdateProductRequest request,
        CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            ViewBag.ProductId = id;
            await LoadCategoriesForDropdown(cancellationToken);
            return View(request);
        }

        var apiResponse = await _apiClient.PutAsync<UpdateProductRequest, ProductDto>(
            $"/api/Products/{id}",
            request,
            null,
            cancellationToken);

        if (!apiResponse.Success)
        {
            foreach (var err in apiResponse.Errors)
            {
                ModelState.AddModelError(string.Empty, err);
            }
            ViewBag.ProductId = id;
            await LoadCategoriesForDropdown(cancellationToken);
            return View(request);
        }

        TempData["Success"] = "Product updated successfully!";
        return RedirectToAction(nameof(Index));
    }

    // GET: /Products/Delete/5
    [HttpGet]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        var apiResponse = await _apiClient.GetAsync<ProductDto>(
            $"/api/Products/{id}",
            null,
            cancellationToken);

        if (!apiResponse.Success)
        {
            TempData["Error"] = string.Join(", ", apiResponse.Errors);
            return RedirectToAction(nameof(Index));
        }

        return View(apiResponse.Data);
    }

    // POST: /Products/Delete/5
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id, CancellationToken cancellationToken)
    {
        var apiResponse = await _apiClient.DeleteAsync<object>(
            $"/api/Products/{id}",
            null,
            cancellationToken);

        if (!apiResponse.Success)
        {
            TempData["Error"] = string.Join(", ", apiResponse.Errors);
        }
        else
        {
            TempData["Success"] = "Product deleted successfully!";
        }

        return RedirectToAction(nameof(Index));
    }

    private async Task LoadCategoriesForDropdown(CancellationToken cancellationToken)
    {
        var categoriesResponse = await _apiClient.GetAsync<PaginatedResponse<CategoryResponse>>(
            "/api/Categories",
            new Dictionary<string, object?> { ["pageIndex"] = 1, ["pageSize"] = 100 },
            cancellationToken);

        ViewBag.Categories = categoriesResponse.Success && categoriesResponse.Data != null
            ? categoriesResponse.Data.Items
            : new List<CategoryResponse>();
    }
}