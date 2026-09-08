namespace Library.Domain.Members;

public interface IPaymentRepository
{
    Task<List<Payment>> GetByLoanIdAsync(Guid loanId);
    Task<List<Payment>> GetByMemberIdAsync(Guid memberId);
    Task AddAsync(Payment payment);
    Task SaveChangesAsync();
    
}