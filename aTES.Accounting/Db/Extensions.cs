using aTES.Accounting.Domain;

namespace aTES.Accounting.Db;

public static class Extensions
{
    public static BillingCycle GetOrAddCurrentBillingCycle(this AccountingDbContext ctx)
    {
        var now = DateTimeOffset.UtcNow;
        var billingCycle = ctx.BillingCycles.FirstOrDefault(x => x.StartDate <= now && now <= x.EndDate);
        if (billingCycle == null)
        {
            var todayStart = new DateTimeOffset(now.Year, now.Month, now.Day, 0, 0, 0, TimeSpan.Zero);
            var todayEnd = new DateTimeOffset(now.Year, now.Month, now.Day, 23, 59, 59, TimeSpan.Zero);
            var newCycle = new BillingCycle(todayStart, todayEnd, BillingCycleStatus.Open);
            ctx.BillingCycles.Add(newCycle);
            billingCycle = newCycle;
        }

        return billingCycle;
    }
}