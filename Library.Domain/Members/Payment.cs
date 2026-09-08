using Library.Domain.Common;
using Library.Domain.Common.ValueObjects;

namespace Library.Domain.Members;

public class Payment : Entity
{
    public Guid MemberId { get; private set; }
    public Money Amount { get; private set; }
    public string Description { get; private set; } = string.Empty;
    public DateTimeOffset Date { get; private set; }
    public Guid? LoanId { get; private set; }
    
    private Payment() {}

    public Payment(Guid memberId, Money amount, string description, DateTimeOffset date, Guid? loanId = null)
    {
        MemberId = memberId;
        Amount = amount;
        Description = description;
        Date = date;
        LoanId = loanId;
    }
    
}