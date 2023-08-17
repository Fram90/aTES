namespace aTES.Accounting.Domain.Services;

/// <summary>
/// Можно было бы сделать отдельный сервис вызываемый по http, но раз уж мы решили не увеличивать глобальную сложность,
/// то просто сделаем локальную реализацию этого интерфейса, держа в уме то, что можно сделать другую реализацию, которая
/// вызывала бы другой сервис
/// </summary>
public interface IPriceProvider
{
    decimal GetTaskChargePrice();
    decimal GetTaskPaymentPrice();
}

class PriceProvider : IPriceProvider
{
    private static Random _random;

    static PriceProvider()
    {
        _random = new Random();
    }

    public decimal GetTaskChargePrice() => Convert.ToDecimal(_random.NextDouble() * -(20 - 10) + 10);

    public decimal GetTaskPaymentPrice() => Convert.ToDecimal(_random.NextDouble() * (40 - 20) + 20);
}