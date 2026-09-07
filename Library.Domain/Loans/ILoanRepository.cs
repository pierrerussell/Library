namespace Library.Domain.Loans;

public interface ILoanRepository
{
    Task<Loan?> GetByIdAsync(Guid id);
    Task<List<Loan>> GetByMemberIdAsync(Guid memberId);
    Task<List<Loan>> GetAllActiveAsync();
    Task AddAsync(Loan loan);
    Task SaveChangesAsync();
    
}