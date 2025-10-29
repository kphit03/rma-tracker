using Microsoft.Data.SqlClient;
using Dapper;

var builder = WebApplication.CreateBuilder(args);

// Read connection string from appsettings.json
string? connectionString = builder.Configuration.GetConnectionString("Default");

// Register factory for creating SQL connections where needed
builder.Services.AddScoped((sp) => new SqlConnection(connectionString));

builder.Services.AddRazorPages();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();
app.MapRazorPages();
app.Run();
