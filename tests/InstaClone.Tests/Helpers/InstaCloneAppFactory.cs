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
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
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
}
