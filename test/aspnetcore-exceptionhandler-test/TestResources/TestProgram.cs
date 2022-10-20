using System.IO;
using Microsoft.AspNetCore.Builder;

var builder = WebApplication.CreateBuilder(new WebApplicationOptions
{
    ApplicationName = typeof(Program).Assembly.FullName,
    ContentRootPath = Path.GetFullPath(Directory.GetCurrentDirectory()),
    WebRootPath = "wwwroot",
    Args = args
});

// ... Configure services, routes, etc.

builder.Build().Run();

public partial class Program { }