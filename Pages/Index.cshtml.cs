using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Data.SqlClient;
using Dapper;

namespace RmaTracker.Pages;

public class IndexModel : PageModel
{   
    //dependency injection
    private readonly SqlConnection _connection;

    // Expose simple properties the Razor page can read
    public int OpenCount { get; private set; }
    public int ClosedCount { get; private set; }
    public decimal? AvgResolutionDays { get; private set; }

    //dependency injection
    public IndexModel(SqlConnection connection)
    {
        _connection = connection; // injected SQL connection from Program.cs
    }

    // Row shape for sp_OpenRmasByAging (names match SQL columns exactly for easy Dapper mapping)
    public class AgingRow
    {
        public int id { get; set; }
        public string rma_number { get; set; } = string.Empty;
        public int customer_id { get; set; }
        public string status { get; set; } = string.Empty;
        public DateTime opened_at { get; set; }
        public int age_days { get; set; }
        public string customer_name { get; set; } = string.Empty;
    }

    // Will hold the Top N aging RMAs for the dashboard table
    public List<AgingRow> AgingTop { get; private set; } = new();

    //OnGet is similar to Spring Boot's @GetMapping annotation, except instead of returning JSON it sets the data
    public async Task OnGet()
    {
        await _connection.OpenAsync();

        var now = DateTime.UtcNow;
        var start = now.AddDays(-30);

        // Call stored procedure sp_RmaKpis using Dapper
        var result = await _connection.QuerySingleAsync<dynamic>(
            "sp_RmaKpis",
            new { startDate = start, endDate = now },
            commandType: System.Data.CommandType.StoredProcedure
        );

        // Map returned columns to our properties
        OpenCount = (int)result.OpenCount;
        ClosedCount = (int)result.ClosedCount;
        AvgResolutionDays = (decimal?)result.AvgResolutionDays;

        // Fetch Top 10 aging RMAs (no status filter) with the procedure "sp_OpenRmasByAging"
        var aging = await _connection.QueryAsync<AgingRow>(
            "sp_OpenRmasByAging",
            new { minDays = 0, maxDays = 9999, status = (string?)null, top = 10 },
            commandType: System.Data.CommandType.StoredProcedure
        );
        AgingTop = aging.ToList();

    }
}
