using Library.Domain.Common;

namespace Library.Domain.Loans;

public class LoanPolicy : AggregateRoot
{
    public int LoanPeriodDays { get; private set; }
    public decimal LateFeePerDay { get; private set; }
    public DateTimeOffset ValidFrom { get; private set; }
    
    private LoanPolicy() {}
    public LoanPolicy(int loanPeriodDays, decimal lateFeePerDay, DateTimeOffset validFrom)
    {
        if (loanPeriodDays <= 0)
            throw new ArgumentOutOfRangeException(nameof(loanPeriodDays), "Loan period must be greater than zero.");
        if (lateFeePerDay < 0)
            throw new ArgumentOutOfRangeException(nameof(lateFeePerDay), "Late fee must be greater than or equal to zero.");
        LoanPeriodDays = loanPeriodDays;
        LateFeePerDay = lateFeePerDay;
        ValidFrom = validFrom;
    }
    
    
}