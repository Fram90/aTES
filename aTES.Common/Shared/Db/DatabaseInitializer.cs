using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;

namespace aTES.Common.Shared.Db;

public class DatabaseInitializer
{
    public static async Task Init<TContext>(WebApplication app) where TContext : DbContext
    {
        using (var scope = app.Services.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<TContext>();
            await context.Database.MigrateAsync();

            if (context.Database.GetDbConnection() is NpgsqlConnection npgsqlConnection)
            {
                await npgsqlConnection.OpenAsync();
                try
                {
                    await npgsqlConnection.ReloadTypesAsync();
                }
                finally
                {
                    await npgsqlConnection.CloseAsync();
                }
            }
        }
    }
}