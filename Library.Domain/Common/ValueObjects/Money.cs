namespace Library.Domain.Common.ValueObjects;

public record Money(decimal Amount)
{
    public static Money Zero => new(0);

    public static Money operator +(Money a, Money b)
    {
        return new(a.Amount + b.Amount);
    }

    public override string ToString()
    {
        return $"{Amount:F}";
    }
};