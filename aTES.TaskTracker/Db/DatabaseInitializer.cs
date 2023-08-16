using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace aTES.TaskTracker.Db;

public class DatabaseInitializer
{
    public static async Task Init(WebApplication app)
    {
        using (var scope = app.Services.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<TaskTrackerDbContext>();
            context.Database.Migrate();

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