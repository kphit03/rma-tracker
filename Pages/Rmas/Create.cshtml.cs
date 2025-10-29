using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Data.SqlClient;
using Dapper;
using System.Data;

namespace RmaTracker.Pages.Rmas;

public class CreateModel : PageModel
{
    private readonly SqlConnection _connection;

    public CreateModel(SqlConnection connection)
    {
        _connection = connection;
    }

    // Data for dropdowns
    public List<CustomerOption> Customers { get; private set; } = new();
    public List<ProductOption> Products { get; private set; } = new();

    public string? ErrorMessage { get; private set; }

    public class CustomerOption { public int id { get; set; } public string name { get; set; } = ""; }
    public class ProductOption { public int id { get; set; } public string sku { get; set; } = ""; public string name { get; set; } = ""; }

    public async Task OnGet()
    {
        await _connection.OpenAsync();
        Customers = (await _connection.QueryAsync<CustomerOption>("SELECT id, name FROM customers ORDER BY name")).ToList();
        Products = (await _connection.QueryAsync<ProductOption>("SELECT id, sku, name FROM products ORDER BY name")).ToList();
    }

    public async Task<IActionResult> OnPost()
    {
        await _connection.OpenAsync();

        string rmaNumber = Request.Form["rma_number"]!;
        int customerId = int.Parse(Request.Form["customer_id"]!);
        string? notes = Request.Form["notes"];

        int productId = int.Parse(Request.Form["product_id"]!);
        int qty = int.Parse(Request.Form["qty"]!);
        string reasonCode = Request.Form["reason_code"]!;

        // Build the table-valued parameter
        var items = new System.Data.DataTable();
        items.Columns.Add("product_id", typeof(int));
        items.Columns.Add("qty", typeof(int));
        items.Columns.Add("reason_code", typeof(string));
        items.Rows.Add(productId, qty, reasonCode);

        var parms = new
        {
            rma_number = rmaNumber,
            customer_id = customerId,
            notes = notes,
            items = items.AsTableValuedParameter("dbo.RmaItemType")
        };

        try
        {
            int newId = await _connection.ExecuteScalarAsync<int>(
                "sp_CreateRma",
                parms,
                commandType: System.Data.CommandType.StoredProcedure
            );

            //  Set a one-time flash message for the next request
            TempData["Flash"] = $"RMA {rmaNumber}: created successfully.";

            // Redirect to the detail page where we’ll display the toast
            return RedirectToPage("/Rmas/Detail", new { id = newId });
        }
        catch (Exception ex)
        {
            ErrorMessage = ex.Message;
            await OnGet(); // reload dropdown lists on error
            return Page();
        }
    }

}
