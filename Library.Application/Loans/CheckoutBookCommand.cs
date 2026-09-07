using Library.Domain.Books;
using Library.Domain.Loans;
using Library.Domain.Members;
using MediatR;

namespace Library.Application.Loans;

public record CheckoutBookCommand(Guid BookId, Guid MemberId) : IRequest<Guid>;

public class CheckoutBookHandler : IRequestHandler<CheckoutBookCommand, Guid>
{
    private readonly ILoanRepository _loanRepository;
    private readonly ILoanPolicyRepository _loanPolicyRepository;
    private readonly IBookRepository _bookRepository;

    public CheckoutBookHandler(ILoanRepository loanRepository, ILoanPolicyRepository loanPolicyRepository,
        IBookRepository bookRepository)
    {
        _loanRepository = loanRepository;
        _loanPolicyRepository = loanPolicyRepository;
        _bookRepository = bookRepository;
    }
    
    
    public async Task<Guid> Handle(CheckoutBookCommand request, CancellationToken cancellationToken)
    {
        var book = await _bookRepository.GetByIdAsync(request.BookId) ??
                   throw new InvalidOperationException("Book not found.");
        
        var copy = book.GetAvailableCopy() ??
                   throw new InvalidOperationException("No available copies.");
        
        copy.SetOnLoan();

        var checkoutDate = DateTimeOffset.UtcNow;
        var loanPolicy = await _loanPolicyRepository.GetLatestAsync() ??
                         throw new InvalidOperationException("No loan policy found.");
        
        var loan = new Loan(request.BookId, copy.Id, request.MemberId, checkoutDate, loanPolicy.LoanPeriodDays, loanPolicy.LateFeePerDay);
        await _loanRepository.AddAsync(loan);
        await _loanRepository.SaveChangesAsync();
        
        return loan.Id;


    }
}