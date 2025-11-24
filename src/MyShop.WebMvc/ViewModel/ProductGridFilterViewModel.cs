namespace MyShop.WebMvc.ViewModel;


public sealed class ProductGridFilterViewModel
{
    public string? Search { get; set; }
    public decimal? MinPrice { get; set; }
    public decimal? MaxPrice { get; set; }
    public string? CategoryName { get; set; }

    public string? SortBy { get; set; }   // "Price,Name"
    public string? SortDir { get; set; }  // "desc,asc"

    public int PageIndex { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}