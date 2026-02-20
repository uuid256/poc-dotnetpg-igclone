using InstaClone.Api.Data;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace InstaClone.Tests.Helpers;

public class InstaCloneAppFactory : WebApplicationFactory<Program>
{
    private string? _uploadsPath;

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        _uploadsPath = Path.Combine(Path.GetTempPath(), $"instaclone-tests-{Guid.NewGuid()}");

        builder.UseSetting("Uploads:Path", _uploadsPath);

        builder.ConfigureServices(services =>
        {
            services.RemoveAll<AppDbContext>();
            services.RemoveAll<DbContextOptions<AppDbContext>>();
            services.RemoveAll<DbContextOptions>();
            services.RemoveAll(typeof(IDbContextOptionsConfiguration<AppDbContext>));

            // Capture DB name outside the lambda so all scopes share the same store
            var dbName = $"TestDb_{Guid.NewGuid()}";
            services.AddDbContext<AppDbContext>(options =>
                options.UseInMemoryDatabase(dbName));
        });
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        if (_uploadsPath != null && Directory.Exists(_uploadsPath))
            Directory.Delete(_uploadsPath, recursive: true);
    }
}
