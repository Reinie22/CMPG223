using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Nkanyezi.Data;
using Nkanyezi.Models;

namespace Nkanyezi_Tours.Pages.Maintain.Attractions;

/// <summary>
/// The attraction list screen.
///
/// THIS IS THE EXAMPLE TO COPY when you build your own list page. Notice what
/// is not here: no SQL, no connection string, no SqlConnection. The page asks
/// the repository for data and hands it to the view.
/// </summary>
public class IndexModel : PageModel
{
    private readonly AttractionRepository _attractions;
    private readonly LookupRepository _lookups;

    // The repositories arrive automatically because they were registered in
    // Program.cs. You never write "new AttractionRepository(...)" yourself.
    public IndexModel(AttractionRepository attractions, LookupRepository lookups)
    {
        _attractions = attractions;
        _lookups = lookups;
    }

    // Data for the view to display
    public List<Attraction> Attractions { get; set; } = new();
    public List<Lookup> Categories { get; set; } = new();
    public List<Lookup> Provinces { get; set; } = new();
    public string? ErrorMessage { get; set; }

    // The search box values. BindProperty with SupportsGet means they are read
    // straight off the query string, so the filters survive a page refresh.
    [BindProperty(SupportsGet = true)] public string? SearchName { get; set; }
    [BindProperty(SupportsGet = true)] public int? CategoryId { get; set; }
    [BindProperty(SupportsGet = true)] public int? ProvinceId { get; set; }

    public void OnGet()
    {
        try
        {
            Categories = _lookups.GetAttractionCategories();
            Provinces = _lookups.GetProvinces();

            // No filters typed in means show everything.
            var noFilters = string.IsNullOrWhiteSpace(SearchName)
                            && CategoryId == null
                            && ProvinceId == null;

            Attractions = noFilters
                ? _attractions.GetAll()
                : _attractions.Search(SearchName, CategoryId, ProvinceId);
        }
        catch (Exception ex)
        {
            // Shown on the page rather than as a yellow crash screen, because a
            // missing database is the most likely problem on a new machine.
            ErrorMessage = "Could not load attractions. Check that SQL Server is "
                         + "running, that the NkanyeziTours database exists, and that "
                         + "01-schema.sql, 03-seed-data.sql and the procedures have "
                         + "been run. Details: " + ex.Message;
        }
    }
}
