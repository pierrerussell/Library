using Library.Domain.Books;
using Library.Domain.Common.ValueObjects;
using Library.Domain.Loans;
using Library.Domain.Loans.Events;
using MediatR;

namespace Library.Application.Loans;

public record ReturnBookCommand(Guid LoanId) : IRequest<Money>;

public class ReturnBookHandler : IRequestHandler<ReturnBookCommand, Money>
{
    private readonly ILoanRepository _loanRepository;
    private readonly IBookRepository _bookRepository;
    private readonly IPublisher _publisher;

    public ReturnBookHandler(ILoanRepository loanRepository, IBookRepository bookRepository, IPublisher publisher)
    {
        _loanRepository = loanRepository;
        _bookRepository = bookRepository;
        _publisher = publisher;
    }


    public async Task<Money> Handle(ReturnBookCommand request, CancellationToken cancellationToken)
    {
        var loan = await _loanRepository.GetByIdAsync(request.LoanId) ??
                   throw new InvalidOperationException("Loan not found.");
        
        var fee = loan.Return(DateTimeOffset.UtcNow);
        
        var book = await _bookRepository.GetByIdAsync(loan.BookId) ??
                   throw new InvalidOperationException("Book not found.");
        var copy = book.Copies.First(c => c.Id == loan.CopyId);
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


