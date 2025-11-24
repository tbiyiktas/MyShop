using Microsoft.AspNetCore.Mvc;
using MyShop.Contracts.Common;
using MyShop.Contracts.Grid;

namespace MyShop.WebApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class MetadataController : ControllerBase
{
    [HttpGet("products-grid")]
    [ProducesResponseType(typeof(ApiResponse<GridMetadataDto>), StatusCodes.Status200OK)]
    public ActionResult<ApiResponse<GridMetadataDto>> GetProductsGridMetadata()
    {
        var metadata = new GridMetadataDto
        {
            EntityName = "Product",
            Columns = new()
            {
                new GridColumnMetadataDto
                {
                    PropertyPath = "Name",
                    DisplayName = "Name",
                    ColumnType = GridColumnType.String,
                    Filterable = true,
                    Sortable = true,
                    AllowedOperations = new()
                    {
                        FilterOperation.Equals,
                        FilterOperation.NotEquals,
                        FilterOperation.Contains,
                        FilterOperation.StartsWith,
                        FilterOperation.EndsWith
                    }
                },
                new GridColumnMetadataDto
                {
                    PropertyPath = "Price",
                    DisplayName = "Price",
                    ColumnType = GridColumnType.Number,
                    Filterable = true,
                    Sortable = true,
                    AllowedOperations = new()
                    {
                        FilterOperation.Equals,
                        FilterOperation.GreaterThan,
                        FilterOperation.GreaterThanOrEqual,
                        FilterOperation.LessThan,
                        FilterOperation.LessThanOrEqual
                    }
                },
                new GridColumnMetadataDto
                {
                    PropertyPath = "StockQuantity",
                    DisplayName = "Stock",
                    ColumnType = GridColumnType.Number,
                    Filterable = true,
                    Sortable = true,
                    AllowedOperations = new()
                    {
                        FilterOperation.Equals,
                        FilterOperation.GreaterThan,
                        FilterOperation.GreaterThanOrEqual,
                        FilterOperation.LessThan,
                        FilterOperation.LessThanOrEqual
                    }
                },
                new GridColumnMetadataDto
                {
                    PropertyPath = "CategoryId",
                    DisplayName = "Category",
                    ColumnType = GridColumnType.Number, // ID is number
                    Filterable = true,
                    Sortable = true,
                    InputType = "Dropdown",
                    LookupUrl = "/api/categories/lookup",
                    LookupValueField = "id",
                    LookupTextField = "name",
                    AllowedOperations = new()
                    {
                        FilterOperation.Equals,
                        FilterOperation.NotEquals
                    }
                },
                new GridColumnMetadataDto
                {
                    PropertyPath = "IsActive",
                    DisplayName = "Is Active",
                    ColumnType = GridColumnType.Boolean,
                    Filterable = true,
                    Sortable = true,
                    AllowedOperations = new()
                    {
                        FilterOperation.Equals,
                        FilterOperation.NotEquals
                    }
                }
            }
        };

        var response = ApiResponse<GridMetadataDto>.SuccessResponse(metadata);
        return Ok(response);
    }

    // İleride:
    // [HttpGet("orders-grid")] => Order için metadata
}
