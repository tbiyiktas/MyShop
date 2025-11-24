namespace MyShop.Contracts.Common;

public sealed class SortCriterion
{
    public string PropertyPath { get; }
    public bool Descending { get; }

    public SortCriterion(string propertyPath, bool descending = false)
    {
        PropertyPath = propertyPath ?? throw new ArgumentNullException(nameof(propertyPath));
        Descending = descending;
    }

    /// <summary>
    /// sortBy = "Price,Name"  sortDir = "desc,asc"
    /// gibi string’leri List&lt;SortCriterion&gt;’a çevirir.
    /// </summary>
    public static IReadOnlyList<SortCriterion> CreateMany(
        string? sortBy,
        string? sortDir)
    {
        if (string.IsNullOrWhiteSpace(sortBy))
            return Array.Empty<SortCriterion>();

        var props = sortBy.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        var dirs = (sortDir ?? string.Empty)
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

        var result = new List<SortCriterion>();

        for (var i = 0; i < props.Length; i++)
        {
            var prop = props[i];
            var dir = (i < dirs.Length ? dirs[i] : "asc") ?? "asc";

            var desc = dir.Equals("desc", StringComparison.OrdinalIgnoreCase) ||
                       dir.Equals("descending", StringComparison.OrdinalIgnoreCase);

            result.Add(new SortCriterion(prop, desc));
        }

        return result;
    }
}
