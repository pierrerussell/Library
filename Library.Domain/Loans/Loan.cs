using Library.Domain.Common;
using Library.Domain.Common.ValueObjects;
using Library.Domain.Loans.Events;

namespace Library.Domain.Loans;

public enum LoanStatus
{
    Active,
    Returned,
    Overdue
}

public class Loan : AggregateRoot
{
    public Guid BookId { get; private set; }
    public Guid CopyId { get; private set; }
    public Guid MemberId { get; private set; }
    public DateTimeOffset CheckoutDate { get; private set; }
    public DateTimeOffset DueDate { get; private set; }
    public DateTimeOffset? ReturnDate { get; private set; }
    public LoanStatus Status { get; private set; }
    public decimal LateFeePerDay { get; private set; }
    
    private Loan() {}
    
    public Loan(Guid bookId, Guid copyId, Guid memberId, DateTimeOffset checkoutDate, int loanPeriodDays, decimal lateFeePerDay)
    {
        BookId = bookId;
        CopyId = copyId;
        MemberId = memberId;
        CheckoutDate = checkoutDate;
        DueDate = checkoutDate.AddDays(loanPeriodDays);
        Status = LoanStatus.Active;
        LateFeePerDay = lateFeePerDay;
    }

    public Money Return(DateTimeOffset returnDate)
    {
        if (Status == LoanStatus.Returned)
            throw new InvalidOperationException("Loan has already been returned.");
        
        ReturnDate = returnDate;
        Status = LoanStatus.Returned;
        var fee = CalculateLateFee(returnDate);
        
        RaiseDomainEvent(new BookReturnedEvent(BookId, CopyId, MemberId, returnDate));
        return fee;
    }

    public Money CalculateLateFee(DateTimeOffset returnDate)
    {
        if (returnDate < DueDate)
            return Money.Zero;
        var daysLate = (returnDate.Date - DueDate.Date).Days;
        return new Money(daysLate * LateFeePerDay);
    }

    public void SetOverdue(DateTimeOffset asOf)
    {
        if (Status == LoanStatus.Active && asOf > DueDate)
            Status = LoanStatus.Overdue;
    }
    
    
    
    
}