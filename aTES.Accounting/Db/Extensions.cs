using aTES.Accounting.Domain;
using Microsoft.EntityFrameworkCore;

namespace aTES.Accounting.Db;

public static class Extensions
{
    
    public static BillingCycle? GetCurrentBillingCycle(this AccountingDbContext ctx)
    {
        return ctx.BillingCycles.Include(x=>x.Transactions).FirstOrDefault(x => x.Status == BillingCycleStatus.Open);
    }

    public static BillingCycle GetOrAddCurrentBillingCycle(this AccountingDbContext ctx)
    {
        var billingCycle = ctx.GetCurrentBillingCycle();
        if (billingCycle == null)
        {   
            var now = DateTimeOffset.UtcNow;
            var todayStart = new DateTimeOffset(now.Year, now.Month, now.Day, 0, 0, 0, TimeSpan.Zero);
            var todayEnd = new DateTimeOffset(now.Year, now.Month, now.Day, 23, 59, 59, TimeSpan.Zero);
            var newCycle = new BillingCycle(todayStart, todayEnd, BillingCycleStatus.Open);
            ctx.BillingCycles.Add(newCycle);
            billingCycle = newCycle;
        }

        return billingCycle;
    }
}