using Library.Domain.Common.ValueObjects;
using Library.Domain.Loans;
using Library.Domain.Members;
using MediatR;

namespace Library.Application.Members;

public record RecordPaymentCommand(Guid MemberId, Guid LoanId) : IRequest;

public class RecordPaymentHandler : IRequestHandler<RecordPaymentCommand>
{
    private readonly IPaymentRepository _paymentRepository;
    private readonly ILoanRepository _loanRepository;
    
    public RecordPaymentHandler(IPaymentRepository paymentRepository, ILoanRepository loanRepository)
    {
        _paymentRepository = paymentRepository;
        _loanRepository = loanRepository;
    }
    
    public async Task Handle(RecordPaymentCommand request, CancellationToken cancellationToken)
    {
        // late fee should be total amount of days overdue * late fee per day - total amount paid so far
        
        var loan = await _loanRepository.GetByIdAsync(request.LoanId) ??
                   throw new InvalidOperationException("Loan not found.");
        var totalFee = loan.CalculateLateFee(DateTimeOffset.UtcNow);
        var totalPaid = await _paymentRepository.GetByLoanIdAsync(loan.Id);
        var outstandingFee = new Money(totalFee.Amount - totalPaid.Sum(p => p.Amount.Amount));
        var payment = new Payment(request.MemberId, outstandingFee, "Overdue Payment", DateTimeOffset.UtcNow, loan.Id);
        await _paymentRepository.AddAsync(payment);
        await _paymentRepository.SaveChangesAsync();
        

    }
}