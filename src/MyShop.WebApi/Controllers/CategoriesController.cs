using Microsoft.AspNetCore.Mvc;
using MyShop.Application.Services;
using MyShop.Contracts.Categories;
using MyShop.Contracts.Common;
using MyShop.Domain.Entities;

namespace MyShop.WebApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CategoriesController : ControllerBase
{
    private readonly CategoryService _categoryService;

    public CategoriesController(CategoryService categoryService)
    {
        _categoryService = categoryService;
    }

    // GET: api/categories
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<PaginatedResponse<CategoryResponse>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<PaginatedResponse<CategoryResponse>>>> GetAll(
        [FromQuery] int pageIndex = 1,
        [FromQuery] int pageSize = 20)
    {
        var page = await _categoryService.GetAllCategoriesAsync(pageIndex, pageSize);
        
        var categoryResponses = page.Items.Select(c => new CategoryResponse
        {
            Id = c.Id,
            Name = c.Name
        }).ToList();

        var dtoPage = PaginatedResponse<CategoryResponse>.Create(
            categoryResponses,
            page.TotalCount,
            page.PageIndex,
            page.PageSize);

        var response = ApiResponse<PaginatedResponse<CategoryResponse>>.SuccessResponse(dtoPage);
        return Ok(response);
    }

    // GET: api/categories/lookup
    [HttpGet("lookup")]
    [ProducesResponseType(typeof(ApiResponse<List<CategoryResponse>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<List<CategoryResponse>>>> GetLookup()
    {
        // For lookup, we want all categories. Let's fetch a large page for now.
        var page = await _categoryService.GetAllCategoriesAsync(1, 1000);
        
        var list = page.Items.Select(c => new CategoryResponse
        {
            Id = c.Id,
            Name = c.Name
        }).ToList();

        var response = ApiResponse<List<CategoryResponse>>.SuccessResponse(list);
        return Ok(response);
    }

    // GET: api/categories/{id}
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(ApiResponse<CategoryResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<CategoryResponse>), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<CategoryResponse>>> GetById(int id)
    {
        var category = await _categoryService.GetByIdAsync(id);
        if (category is null)
        {
            var notFoundResponse = ApiResponse<CategoryResponse>.ErrorResponse($"Category with id {id} not found.");
            return NotFound(notFoundResponse);
        }

        var dto = new CategoryResponse
        {
            Id = category.Id,
            Name = category.Name
        };

        var response = ApiResponse<CategoryResponse>.SuccessResponse(dto);
        return Ok(response);
    }

    // POST: api/categories
    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<CategoryResponse>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<CategoryResponse>), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse<CategoryResponse>>> Create(
        [FromBody] CreateCategoryRequest request,
        CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            var errors = ModelState
                .Where(x => x.Value?.Errors.Count > 0)
                .SelectMany(x => x.Value!.Errors.Select(e => e.ErrorMessage))
                .ToList();

            var errorResponse = ApiResponse<CategoryResponse>.ErrorResponse(errors);
            return BadRequest(errorResponse);
        }

        var result = await _categoryService.CreateAsync(request.Name, cancellationToken);

        if (result.HasError)
        {
            var errorResponse = ApiResponse<CategoryResponse>.ErrorResponse(result.Errors);
            return BadRequest(errorResponse);
        }

        var category = result.Value!;
        var dto = new CategoryResponse
        {
            Id = category.Id,
            Name = category.Name
        };

        var response = ApiResponse<CategoryResponse>.SuccessResponse(dto);

        return CreatedAtAction(
            nameof(GetById),
            new { id = dto.Id },
            response);
    }

    // PUT: api/categories/{id}
    [HttpPut("{id:int}")]
    [ProducesResponseType(typeof(ApiResponse<CategoryResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<CategoryResponse>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<CategoryResponse>), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse<CategoryResponse>>> Update(
        int id,
        [FromBody] UpdateCategoryRequest request,
        CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            var errors = ModelState
                .Where(x => x.Value?.Errors.Count > 0)
                .SelectMany(x => x.Value!.Errors.Select(e => e.ErrorMessage))
                .ToList();

            var errorResponse = ApiResponse<CategoryResponse>.ErrorResponse(errors);
            return BadRequest(errorResponse);
        }

        var result = await _categoryService.UpdateAsync(id, request.Name, cancellationToken);

        if (result.HasError)
        {
            if (result.Errors.Any(e => e.Contains("not found")))
            {
                var notFoundResponse = ApiResponse<CategoryResponse>.ErrorResponse(result.Errors);
                return NotFound(notFoundResponse);
            }

            var errorResponse = ApiResponse<CategoryResponse>.ErrorResponse(result.Errors);
            return BadRequest(errorResponse);
        }

        var category = result.Value!;
        var dto = new CategoryResponse
        {
            Id = category.Id,
            Name = category.Name
        };

        var response = ApiResponse<CategoryResponse>.SuccessResponse(dto);
        return Ok(response);
    }

    // DELETE: api/categories/{id}
    [HttpDelete("{id:int}")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<object>>> Delete(
        int id,
        CancellationToken cancellationToken)
    {
        var result = await _categoryService.DeleteAsync(id, cancellationToken);

        if (result.HasError)
        {
            var notFoundResponse = ApiResponse<object>.ErrorResponse(result.Errors);
            return NotFound(notFoundResponse);
        }

        var response = ApiResponse<object>.SuccessResponse(null!);
        return Ok(response);
    }
}
