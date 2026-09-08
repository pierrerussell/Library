using Library.Domain.Members;
using Microsoft.EntityFrameworkCore;

namespace Library.Blazor.Data.Repositories.Members;

public class PaymentRepository : IPaymentRepository
{
    private readonly ApplicationDbContext _context;
    public PaymentRepository(ApplicationDbContext context)
    {
        _context = context;
    }
    public async Task<List<Payment>> GetByLoanIdAsync(Guid loanId)
    {
        return await _context.Payments.Where(x => x.LoanId == loanId).ToListAsync();

    }

    public async Task<List<Payment>> GetByMemberIdAsync(Guid memberId)
    {
        return await _context.Payments.Where(x => x.MemberId == memberId).ToListAsync();
        
    }

    public async Task AddAsync(Payment payment)
    {
        await _context.Payments.AddAsync(payment);
        
    }

    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
        
    }
}