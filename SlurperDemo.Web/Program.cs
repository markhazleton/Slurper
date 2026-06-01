using ApiTestSpark;
using Microsoft.AspNetCore.OpenApi;
using WebSpark.Slurper.Extensions;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();
builder.Services.AddSlurper();

builder.Services.AddOpenApi(options =>
{
    options.AddDocumentTransformer((document, context, cancellationToken) =>
    {
        document.Info.Title = "WebSpark Slurper Demo API";
        document.Info.Version = "v1";
        document.Info.Description =
            "Demonstrates **WebSpark.Slurper** — a library for extracting structured data from " +
            "XML, JSON, CSV, and HTML using dynamic objects.\n\n" +
            "### Endpoints\n" +
            "- **Slurper: Extraction** — parse product catalogs from each supported format\n\n" +
            "All product data is loaded from sample files in `wwwroot/data/`.";
        document.Info.Contact = new()
        {
            Name = "Mark Hazleton",
            Url = new Uri("https://markhazleton.com")
        };
        return Task.CompletedTask;
    });
});

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.MapOpenApi();

app.MapApiTestSpark(options =>
{
    options.OpenApiUrl = "/openapi/v1.json";
    options.Environments = ["Development", "Test", "Production"];
    options.EnableDemoIntegrations = false;
});

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
