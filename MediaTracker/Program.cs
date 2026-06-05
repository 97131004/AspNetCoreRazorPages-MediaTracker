using MediaTracker.Modules.Media;
using MediaTracker.Modules.Media.Services;
using MediaTracker.Modules.MediaData;
using MediaTracker.Modules.MediaData.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorPages();

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

builder.Services.AddDbContext<MediaDbContext>(options =>
    options.UseSqlServer(connectionString));

builder.Services.AddDbContext<MediaDataDbContext>(options =>
    options.UseSqlServer(connectionString));

builder.Services.AddScoped<MediaService>();
builder.Services.AddScoped<MediaDataService>();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    try
    {
        scope.ServiceProvider.GetRequiredService<MediaDbContext>().Database.Migrate();
        scope.ServiceProvider.GetRequiredService<MediaDataDbContext>().Database.Migrate();
    }
    catch (Exception ex)
    {
        scope.ServiceProvider.GetRequiredService<ILogger<Program>>()
            .LogError(ex, "An error occurred while migrating the database.");
        throw;
    }
}

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
}
else
{
    app.UseDeveloperExceptionPage();
}

app.UseStaticFiles();
app.UseRouting();
app.MapRazorPages();

app.Run();

