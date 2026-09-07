using Library.Domain.Loans;
using MediatR;

namespace Library.Application.Loans;

public record AddLoanPolicyCommand(int LoanPeriodDays, decimal LateFeePerDay, DateTime ValidFrom) : IRequest;

public class AddLoanPolicyHandler : IRequestHandler<AddLoanPolicyCommand>
{
    private readonly ILoanPolicyRepository _loanPolicyRepository;

    public AddLoanPolicyHandler(ILoanPolicyRepository loanPolicyRepository)
    {
        _loanPolicyRepository = loanPolicyRepository;
    }

    public async Task Handle(AddLoanPolicyCommand request, CancellationToken ct)
    {
        var policy = new LoanPolicy(request.LoanPeriodDays, request.LateFeePerDay, request.ValidFrom);
        await _loanPolicyRepository.AddAsync(policy);
        await _loanPolicyRepository.SaveChangesAsync();
    }
}