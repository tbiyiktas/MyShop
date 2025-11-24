using Microsoft.AspNetCore.Mvc;
using MyShop.Contracts.Categories;
using MyShop.Contracts.Common;
using MyShop.WebMvc.Services;

namespace MyShop.WebMvc.Controllers;

public class CategoriesController : Controller
{
    private readonly ApiClient _apiClient;

    public CategoriesController(ApiClient apiClient)
    {
        _apiClient = apiClient;
    }

    // GET: /Categories
    [HttpGet]
    public async Task<IActionResult> Index(
        int pageIndex = 1,
        int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        var queryParams = new Dictionary<string, object?>
        {
            ["pageIndex"] = pageIndex <= 0 ? 1 : pageIndex,
            ["pageSize"] = pageSize <= 0 ? 20 : pageSize
        };

        var apiResponse = await _apiClient.GetAsync<PaginatedResponse<CategoryResponse>>(
            "/api/Categories",
            queryParams,
            cancellationToken);

        if (!apiResponse.Success)
        {
            foreach (var err in apiResponse.Errors)
            {
                ModelState.AddModelError(string.Empty, err);
            }
        }

        ViewBag.Page = apiResponse.Data;
        return View();
    }

    // GET: /Categories/Details/5
    [HttpGet]
    public async Task<IActionResult> Details(int id, CancellationToken cancellationToken)
    {
        var apiResponse = await _apiClient.GetAsync<CategoryResponse>(
            $"/api/Categories/{id}",
            null,
            cancellationToken);

        if (!apiResponse.Success)
        {
            TempData["Error"] = string.Join(", ", apiResponse.Errors);
            return RedirectToAction(nameof(Index));
        }

        return View(apiResponse.Data);
    }

    // GET: /Categories/Create
    [HttpGet]
    public IActionResult Create()
    {
        return View();
    }

    // POST: /Categories/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(
        CreateCategoryRequest request,
        CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return View(request);
        }

        var apiResponse = await _apiClient.PostAsync<CreateCategoryRequest, CategoryResponse>(
            "/api/Categories",
            request,
            null,
            cancellationToken);

        if (!apiResponse.Success)
        {
            foreach (var err in apiResponse.Errors)
            {
                ModelState.AddModelError(string.Empty, err);
            }
            return View(request);
        }

        TempData["Success"] = "Category created successfully!";
        return RedirectToAction(nameof(Index));
    }

    // GET: /Categories/Edit/5
    [HttpGet]
    public async Task<IActionResult> Edit(int id, CancellationToken cancellationToken)
    {
        var apiResponse = await _apiClient.GetAsync<CategoryResponse>(
            $"/api/Categories/{id}",
            null,
            cancellationToken);

        if (!apiResponse.Success)
        {
            TempData["Error"] = string.Join(", ", apiResponse.Errors);
            return RedirectToAction(nameof(Index));
        }

        var updateRequest = new UpdateCategoryRequest
        {
            Name = apiResponse.Data!.Name
        };

        ViewBag.CategoryId = id;
        return View(updateRequest);
    }

    // POST: /Categories/Edit/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(
        int id,
        UpdateCategoryRequest request,
        CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            ViewBag.CategoryId = id;
            return View(request);
        }

        var apiResponse = await _apiClient.PutAsync<UpdateCategoryRequest, CategoryResponse>(
            $"/api/Categories/{id}",
            request,
            null,
            cancellationToken);

        if (!apiResponse.Success)
        {
            foreach (var err in apiResponse.Errors)
            {
                ModelState.AddModelError(string.Empty, err);
            }
            ViewBag.CategoryId = id;
            return View(request);
        }

        TempData["Success"] = "Category updated successfully!";
        return RedirectToAction(nameof(Index));
    }

    // GET: /Categories/Delete/5
    [HttpGet]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        var apiResponse = await _apiClient.GetAsync<CategoryResponse>(
            $"/api/Categories/{id}",
            null,
            cancellationToken);

        if (!apiResponse.Success)
        {
            TempData["Error"] = string.Join(", ", apiResponse.Errors);
            return RedirectToAction(nameof(Index));
        }

        return View(apiResponse.Data);
    }

    // POST: /Categories/Delete/5
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id, CancellationToken cancellationToken)
    {
        var apiResponse = await _apiClient.DeleteAsync<object>(
            $"/api/Categories/{id}",
            null,
            cancellationToken);

        if (!apiResponse.Success)
        {
            TempData["Error"] = string.Join(", ", apiResponse.Errors);
        }
        else
        {
            TempData["Success"] = "Category deleted successfully!";
        }

        return RedirectToAction(nameof(Index));
    }
}
