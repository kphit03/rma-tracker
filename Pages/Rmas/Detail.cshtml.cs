using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Data.SqlClient;
using Dapper;

namespace RmaTracker.Pages.Rmas;

public class DetailModel : PageModel
{
    private readonly SqlConnection _connection;

    public DetailModel(SqlConnection connection)
    {
        _connection = connection;
    }

    [FromRoute]
    public int Id { get; set; }

    // Shapes must match stored proc column names
    public class RmaHeader
    {
        public int id { get; set; }
        public string rma_number { get; set; } = string.Empty;
        public int customer_id { get; set; }
        public string customer_name { get; set; } = string.Empty;
        public string status { get; set; } = string.Empty;
        public DateTime opened_at { get; set; }
        public string? notes { get; set; }
    }

    public class RmaItem
    {
        public int id { get; set; }
        public int product_id { get; set; }
        public string sku { get; set; } = string.Empty;
        public string product_name { get; set; } = string.Empty;
        public int qty { get; set; }
        public string? reason_code { get; set; }
        public string? resolution { get; set; }
        public DateTime? closed_at { get; set; }
    }

    public RmaHeader? Header { get; private set; }
    public List<RmaItem> Items { get; private set; } = new();

    public async Task<IActionResult> OnGet()
    {
        await LoadAsync();
        return Page();
    }

    // POST: Close RMA (header-level)
    public async Task<IActionResult> OnPostClose()
    {
        await _connection.OpenAsync();

        await _connection.ExecuteAsync(
            "sp_CloseRma",
            new { rmaId = Id },
            commandType: System.Data.CommandType.StoredProcedure
        );

        TempData["Flash"] = $"RMA closed.";
        return RedirectToPage("/Rmas/Detail", new { id = Id });
    }

    // POST: Delete RMA
    public async Task<IActionResult> OnPostDelete()
    {
        await _connection.OpenAsync();

        await _connection.ExecuteAsync(
            "sp_DeleteRma",
            new { rmaId = Id },
            commandType: System.Data.CommandType.StoredProcedure
        );

        TempData["Flash"] = $"RMA deleted.";
        return RedirectToPage("/Rmas/Index");
    }

    // POST: Update a single item's resolution 
    public async Task<IActionResult> OnPostUpdateItem(int itemId, string? resolution, bool closeNow = false)
    {   
        //open sql connection
        await _connection.OpenAsync();

        // Normalize resolution input: allow null/empty to clear
        var res = string.IsNullOrWhiteSpace(resolution) ? null : resolution;

        // validate allowed values when provided (UI level)
        if (res is not null)
        {
            var allowed = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
                { "refund", "replace", "repair", "deny" };
            if (!allowed.Contains(res))
            {
                TempData["Flash"] = "Invalid resolution value.";
                return RedirectToPage("/Rmas/Detail", new { id = Id });
            }
        }

        //call procedure with parameters
        await _connection.ExecuteAsync(
            "sp_UpdateRmaItemResolution",
            new { itemId, resolution = res, closeNow },
            commandType: System.Data.CommandType.StoredProcedure
        );
        //success toast for UI
        TempData["Flash"] = $"Item updated.";
        return RedirectToPage("/Rmas/Detail", new { id = Id });
    }

    public async Task<IActionResult> OnPostUpdateStatus(int id, string status)
    {
        // Allow only open/pending here; 'closed' should be done via the Close RMA button
        var allowed = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "open", "pending" };
        if (!allowed.Contains(status))
        {
            TempData["Flash"] = "Invalid status. Use Close RMA to set status to 'closed'.";
            return RedirectToPage("/Rmas/Detail", new { id });
        }

        await _connection.OpenAsync();

        // Call proc that updates status safely
        await _connection.ExecuteAsync(
            "sp_UpdateRmaStatus",
            new { rmaId = id, status },
            commandType: System.Data.CommandType.StoredProcedure
        );

        TempData["Flash"] = $"Status updated to '{status}'.";
        return RedirectToPage("/Rmas/Detail", new { id });
    }


    private async Task LoadAsync()
    {
        await _connection.OpenAsync();

        using var grid = await _connection.QueryMultipleAsync(
            "sp_GetRmaDetail",
            new { rmaId = Id },
            commandType: System.Data.CommandType.StoredProcedure
        );

        Header = await grid.ReadFirstOrDefaultAsync<RmaHeader>();
        Items = (await grid.ReadAsync<RmaItem>()).ToList();
    }
}
