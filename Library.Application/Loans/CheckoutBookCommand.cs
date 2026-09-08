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
    private readonly IPaymentRepository _paymentRepository;
    
    
    public CheckoutBookHandler(ILoanRepository loanRepository, ILoanPolicyRepository loanPolicyRepository,
        IBookRepository bookRepository,  IPaymentRepository paymentRepository)
    {
        _loanRepository = loanRepository;
        _loanPolicyRepository = loanPolicyRepository;
        _bookRepository = bookRepository;
        _paymentRepository = paymentRepository;
        
    }
    
    
    public async Task<Guid> Handle(CheckoutBookCommand request, CancellationToken cancellationToken)
    {
        var book = await _bookRepository.GetByIdAsync(request.BookId) ??
                   throw new InvalidOperationException("Book not found.");
        
        var copy = book.GetAvailableCopy() ??
                   throw new InvalidOperationException("No available copies.");
        
        // check if member has any outstanding loans not paid
        var memberLoans = await _loanRepository.GetByMemberIdAsync(request.MemberId);
        var memberActiveLoans = memberLoans.Where(l => l.Status == LoanStatus.Active);
        var memberPayments = await _paymentRepository.GetByMemberIdAsync(request.MemberId);

        foreach (var memberLoan in memberActiveLoans)
        {
            // for each loan, check if sum of payments made for that loan is greater than or equal to the outstanding fee
            var outstandingFee = memberLoan.CalculateLateFee(DateTimeOffset.UtcNow);
            var paymentsMadeForLoan = memberPayments.Where(p => p.LoanId == memberLoan.Id).ToList();
            if (paymentsMadeForLoan.Sum(p => p.Amount.Amount) < outstandingFee.Amount)
            {
                throw new InvalidOperationException("Member has outstanding loan(s) not paid.");
            }
        }
        
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