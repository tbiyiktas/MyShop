using Microsoft.AspNetCore.Mvc;
using MyShop.Application.Services;
using MyShop.Contracts.Common;
using MyShop.Contracts.Products;
namespace MyShop.WebApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProductsController : ControllerBase
{
    private readonly ProductService _productService;

    public ProductsController(ProductService productService)
    {
        _productService = productService;
    }

    // GET: api/products
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<PaginatedResponse<ProductDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<PaginatedResponse<ProductDto>>>> GetAll(
        [FromQuery] int pageIndex = 1,
        [FromQuery] int pageSize = 20)
    {
        var page = await _productService.GetAllProductsAsync(pageIndex, pageSize);
        var dtoPage = page.ToDtoPage();

        var response = ApiResponse<PaginatedResponse<ProductDto>>.SuccessResponse(dtoPage);
        return Ok(response);
    }

    // GET: api/products/low-stock
    [HttpGet("low-stock")]
    [ProducesResponseType(typeof(ApiResponse<PaginatedResponse<ProductDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<PaginatedResponse<ProductDto>>>> GetLowStock(
        [FromQuery] int threshold = 10,
        [FromQuery] int pageIndex = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] bool orderByPriceDescending = false)
    {
        var page = await _productService.GetLowStockProductsAsync(
            threshold,
            pageIndex,
            pageSize,
            orderByPriceDescending);

        var dtoPage = page.ToDtoPage();
        var response = ApiResponse<PaginatedResponse<ProductDto>>.SuccessResponse(dtoPage);

        return Ok(response);
    }

    // GET: api/products/{id}
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(ApiResponse<ProductDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<ProductDto>), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<ProductDto>>> GetById(int id)
    {
        var product = await _productService.GetByIdAsync(id);
        if (product is null)
        {
            var notFoundResponse = ApiResponse<ProductDto>.ErrorResponse($"Product with id {id} not found.");
            return NotFound(notFoundResponse);
        }

        var dto = product.ToDto();
        var response = ApiResponse<ProductDto>.SuccessResponse(dto);

        return Ok(response);
    }

    // POST: api/products
    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<ProductDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<ProductDto>), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse<ProductDto>>> Create(
        [FromBody] CreateProductRequest request,
        CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            var errors = ModelState
                .Where(x => x.Value?.Errors.Count > 0)
                .SelectMany(x => x.Value!.Errors.Select(e => e.ErrorMessage))
                .ToList();

            var errorResponse = ApiResponse<ProductDto>.ErrorResponse(errors);
            return BadRequest(errorResponse);
        }

        var product = await _productService.CreateAsync(
            request.Name,
            request.Price,
            request.StockQuantity,
            request.CategoryId,
            cancellationToken);

        var dto = product.ToDto();
        var response = ApiResponse<ProductDto>.SuccessResponse(dto);

        return CreatedAtAction(
            nameof(GetById),
            new { id = dto.Id },
            response);
    }

    // PUT: api/products/{id}
    [HttpPut("{id:int}")]
    [ProducesResponseType(typeof(ApiResponse<ProductDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<ProductDto>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<ProductDto>), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse<ProductDto>>> Update(
        int id,
        [FromBody] UpdateProductRequest request,
        CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            var errors = ModelState
                .Where(x => x.Value?.Errors.Count > 0)
                .SelectMany(x => x.Value!.Errors.Select(e => e.ErrorMessage))
                .ToList();

            var errorResponse = ApiResponse<ProductDto>.ErrorResponse(errors);
            return BadRequest(errorResponse);
        }

        var updated = await _productService.UpdateAsync(
            id,
            request.Name,
            request.Price,
            request.StockQuantity,
            request.CategoryId,
            request.IsActive,
            cancellationToken);

        if (updated is null)
        {
            var notFoundResponse = ApiResponse<ProductDto>.ErrorResponse($"Product with id {id} not found.");
            return NotFound(notFoundResponse);
        }

        var dto = updated.ToDto();
        var response = ApiResponse<ProductDto>.SuccessResponse(dto);

        return Ok(response);
    }

    // DELETE: api/products/{id}
    [HttpDelete("{id:int}")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<object>>> Delete(
        int id,
        CancellationToken cancellationToken)
    {
        var deleted = await _productService.DeleteAsync(id, cancellationToken);
        if (!deleted)
        {
            var notFoundResponse = ApiResponse<object>.ErrorResponse($"Product with id {id} not found.");
            return NotFound(notFoundResponse);
        }

        var response = ApiResponse<object>.SuccessResponse(null!);
        return Ok(response);
    }

    [HttpGet("low-stock-expensive-non-electronics")]
    public async Task<ActionResult<ApiResponse<PaginatedResponse<ProductDto>>>> GetLowStockExpensiveNonElectronics(
    [FromQuery] int threshold = 10,
    [FromQuery] decimal minPrice = 1000m,
    [FromQuery] int pageIndex = 1,
    [FromQuery] int pageSize = 20)
    {
        var page = await _productService.GetLowStockExpensiveNonElectronicsAsync(threshold, minPrice, pageIndex, pageSize);

        var dtoPage = page.ToDtoPage();
        var response = ApiResponse<PaginatedResponse<ProductDto>>.SuccessResponse(dtoPage);

        return Ok(response);
    }

    [HttpGet("search")]
    [ProducesResponseType(typeof(ApiResponse<PaginatedResponse<ProductDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<PaginatedResponse<ProductDto>>>> Search(
     [FromQuery] string? search,
     [FromQuery] bool? isActive,
     [FromQuery] decimal? minPrice,
     [FromQuery] decimal? maxPrice,
     [FromQuery] string? categoryName,
     [FromQuery] string? sortBy,   // örn: "Price,Name,Category.Name"
     [FromQuery] string? sortDir,  // örn: "desc,asc,asc"
     [FromQuery] int pageIndex = 1,
     [FromQuery] int pageSize = 20)
    {
        var sorts = SortCriterion.CreateMany(sortBy, sortDir);

        var page = await _productService.SearchAsync(
            search,
            isActive,
            minPrice,
            maxPrice,
            categoryName,
            sorts,
            pageIndex,
            pageSize);

        var dtoPage = page.ToDtoPage();
        var response = ApiResponse<PaginatedResponse<ProductDto>>.SuccessResponse(dtoPage);
        return Ok(response);
    }

    [HttpPost("grid-search")]
    [ProducesResponseType(typeof(ApiResponse<PaginatedResponse<ProductDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<PaginatedResponse<ProductDto>>>> GridSearch(
    [FromBody] GridFilterRequestDto request,
    CancellationToken cancellationToken)
    {
        // Application service: ExpressionCombiner + GridFilterExpressionBuilder kullanan versiyon
        var page = await _productService.SearchWithGridAsync(request);

        var dtoPage = page.ToDtoPage();
        var response = ApiResponse<PaginatedResponse<ProductDto>>.SuccessResponse(dtoPage);

        return Ok(response);
    }

}
