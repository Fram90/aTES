using Coravel;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace aTES.Common.Shared.Db;

public static class DbExtensions
{
    public static void RunOutboxPublisher<TContext>(this WebApplication app) where TContext : OutboxContext<TContext>
    {
        app.Services.UseScheduler(scheduler => scheduler.Schedule<OutboxPublisher<TContext>>().EverySecond());
    }

    public static void AddOutboxPublisher<TContext>(this IServiceCollection services) where TContext : OutboxContext<TContext>
    {
        services.AddScoped<OutboxPublisher<TContext>>();
    }
}