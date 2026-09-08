using Library.Domain.Books;
using Library.Domain.Common.ValueObjects;
using Library.Domain.Loans;
using Library.Domain.Loans.Events;
using Library.Domain.Members;
using MediatR;

namespace Library.Application.Loans;

public record ReturnBookCommand(Guid LoanId) : IRequest<Money>;

public class ReturnBookHandler : IRequestHandler<ReturnBookCommand, Money>
{
    private readonly ILoanRepository _loanRepository;
    private readonly IBookRepository _bookRepository;
    private readonly IPublisher _publisher;
    private readonly IPaymentRepository _paymentRepository;

    public ReturnBookHandler(ILoanRepository loanRepository, IBookRepository bookRepository, IPublisher publisher, IPaymentRepository paymentRepository)
    {
        _loanRepository = loanRepository;
        _bookRepository = bookRepository;
        _publisher = publisher;
        _paymentRepository = paymentRepository;
    }


    public async Task<Money> Handle(ReturnBookCommand request, CancellationToken cancellationToken)
    {
        var loan = await _loanRepository.GetByIdAsync(request.LoanId) ??
                   throw new InvalidOperationException("Loan not found.");
        
        var fee = loan.CalculateLateFee(DateTimeOffset.UtcNow);
        
        var book = await _bookRepository.GetByIdAsync(loan.BookId) ??
                   throw new InvalidOperationException("Book not found.");
        var copy = book.Copies.First(c => c.Id == loan.CopyId);

        if (fee.Amount > 0)
        {
            var payments = await _paymentRepository.GetByLoanIdAsync(loan.Id);
            if (payments.Count == 0)
                throw new InvalidOperationException("Payment not made for overdue fee.");
            
            if (payments.Sum(p => p.Amount.Amount) != fee.Amount)
            {
                throw new InvalidOperationException("Payment amount does not match overdue fee.");
            }
        }

        loan.Return(DateTimeOffset.UtcNow);
        
        copy.SetAvailable();
        foreach (var domainEvent in loan.DomainEvents)
        {
            switch (domainEvent)
            {
                case BookReturnedEvent e:
                    await _publisher.Publish(new BookReturnedNotification(
                        e.BookId, e.CopyId, e.MemberId, e.ReturnedOn),
                        cancellationToken);
                    break;
            }
        }
        loan.ClearDomainEvents();
        
        await _bookRepository.SaveChangesAsync();
        
        return fee;
    }
}


