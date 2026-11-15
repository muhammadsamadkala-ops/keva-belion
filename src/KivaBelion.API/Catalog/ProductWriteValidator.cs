using System.Text.RegularExpressions;

namespace KivaBelion.API.Catalog;

/// <summary>Checks a product request without touching the database, so it is easy to test.</summary>
public static class ProductWriteValidator
{
    private static readonly Regex Sku = new("^[A-Za-z0-9._-]+$", RegexOptions.Compiled);
    private static readonly Regex Slug = new("^[a-z0-9]+(?:-[a-z0-9]+)*$", RegexOptions.Compiled);
    private static readonly Regex Label = new("^[a-z]{2,30}$", RegexOptions.Compiled);
    // A site path such as assets/products/noir.jpg, or a full https:// address. Never javascript: or data:.
    private static readonly Regex Image = new(@"^(https://[^\s]+|[A-Za-z0-9_./-]+)$", RegexOptions.Compiled);

    public static readonly string[] Genders = { "men", "women", "unisex" };

    public static Dictionary<string, string[]> Validate(ProductWriteRequest? r)
    {
        var errors = new Dictionary<string, string[]>();
        void Add(string key, string message) => errors[key] = new[] { message };

        if (r is null) { Add("request", "The product is empty."); return errors; }

        if (r.CategoryId is null or < 1) Add("categoryId", "Please choose a category.");

        var sku = r.Sku?.Trim() ?? "";
        if (sku.Length < 2 || sku.Length > 50 || !Sku.IsMatch(sku)) Add("sku", "SKU needs 2 to 50 letters, digits, dot, underscore or hyphen.");

        var name = r.Name?.Trim() ?? "";
        if (name.Length < 2 || name.Length > 150) Add("name", "Name needs 2 to 150 characters.");

        var slug = r.Slug?.Trim() ?? "";
        if (slug.Length < 2 || slug.Length > 160 || !Slug.IsMatch(slug)) Add("slug", "Slug may contain only lower-case letters, digits and single hyphens.");

        if (r.SizeMl < 1 || r.SizeMl > 5000) Add("sizeMl", "Size must be between 1 and 5000 ml.");
        if (r.Price < 0 || r.Price > 9_999_999m) Add("price", "Price must be between 0 and 9,999,999.");

        if (Len(r.ShortDescription) > 300) Add("shortDescription", "Short description can be at most 300 characters.");
        if (Len(r.Description) > 4000) Add("description", "Description can be at most 4000 characters.");
        if (Len(r.Notes) > 300) Add("notes", "Notes can be at most 300 characters.");
        if (Len(r.TopNotes) > 300) Add("topNotes", "Top notes can be at most 300 characters.");
        if (Len(r.HeartNotes) > 300) Add("heartNotes", "Heart notes can be at most 300 characters.");
        if (Len(r.BaseNotes) > 300) Add("baseNotes", "Base notes can be at most 300 characters.");

        var image = r.ImageUrl?.Trim() ?? "";
        if (image.Length > 0 && (image.Length > 300 || !Image.IsMatch(image) || image.Contains("..")))
            Add("imageUrl", "Image must be a path like assets/products/noir.jpg or an https:// address.");

        var gender = r.Gender?.Trim() ?? "";
        if (gender.Length > 0 && !Genders.Contains(gender)) Add("gender", "Gender must be men, women or unisex.");

        var family = r.Family?.Trim() ?? "";
        if (family.Length > 0 && !Label.IsMatch(family)) Add("family", "Family must be one lower-case word, for example woody.");

        var intensity = r.Intensity?.Trim() ?? "";
        if (intensity.Length > 0 && !Label.IsMatch(intensity)) Add("intensity", "Intensity must be one lower-case word, for example bold.");

        if (r.SortOrder < 0 || r.SortOrder > 100000) Add("sortOrder", "Sort order must be between 0 and 100000.");

        return errors;
    }

    public static string? Clean(string? v) => string.IsNullOrWhiteSpace(v) ? null : v.Trim();

    private static int Len(string? v) => v?.Trim().Length ?? 0;
}
