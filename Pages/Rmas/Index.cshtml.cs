using Dapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Data.SqlClient;

namespace RmaTracker.Pages.Rmas;

public class IndexModel : PageModel
{
    private readonly SqlConnection _connection;

    // Row shape returned by sp_OpenRmasByAging (names match SQL columns)
    public class RmaListRow
    {
        public int id { get; set; }
        public string rma_number { get; set; } = string.Empty;
        public int customer_id { get; set; }
        public string customer_name { get; set; } = string.Empty;
        public string status { get; set; } = string.Empty;
        public DateTime opened_at { get; set; }
        public int age_days { get; set; }
    }

    // Exposed to the Razor page
    public List<RmaListRow> Rows { get; private set; } = new();

    [BindProperty(SupportsGet = true)]
    public string? Status { get; set; }

    [BindProperty(SupportsGet = true)]
    public int? MinDays { get; set; }

    [BindProperty(SupportsGet = true)]
    public int? MaxDays { get; set; }

    public IndexModel(SqlConnection connection)
    {
        _connection = connection;
    }


    public async Task OnGet()
    {
        await _connection.OpenAsync();

        // Initial load: Top 50, all statuses, any age
        var status = string.IsNullOrWhiteSpace(Status) ? null : Status;
        var min = MinDays ?? 0;
        var max = MaxDays ?? 9999;

        var data = await _connection.QueryAsync<RmaListRow>(
            "sp_OpenRmasByAging",
            new { minDays = min, maxDays = max, status, top = 50 },
            commandType: System.Data.CommandType.StoredProcedure
        );

        Rows = data.ToList();

    }
}
