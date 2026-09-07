namespace Library.Domain.Loans;

public interface ILoanPolicyRepository
{
    Task<LoanPolicy> GetEffectiveAsync(DateTimeOffset asOf);
    Task<LoanPolicy> GetLatestAsync();
    Task<List<LoanPolicy>> GetAllAsync();
    Task AddAsync(LoanPolicy loanPolicy);
    Task SaveChangesAsync();
}