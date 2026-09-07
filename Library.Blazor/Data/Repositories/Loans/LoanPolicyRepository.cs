using Library.Domain.Loans;
using Microsoft.EntityFrameworkCore;

namespace Library.Blazor.Data.Repositories.Loans;

public class LoanPolicyRepository : ILoanPolicyRepository
{
    private readonly ApplicationDbContext _context;
    public LoanPolicyRepository(ApplicationDbContext context)
    {
        _context = context;
    }
    
    public async Task<LoanPolicy> GetEffectiveAsync(DateTimeOffset asOf)
    {
        var policy = await _context.LoanPolicies
            .Where(p => p.ValidFrom <= asOf)
            .OrderByDescending(p => p.ValidFrom)
            .FirstOrDefaultAsync();
        
        return policy ??
               throw new InvalidOperationException("No loan policy found for the given date.");
    }

    public async Task<LoanPolicy?> GetLatestAsync()
    {
        return await _context.LoanPolicies.OrderByDescending(p => p.ValidFrom).FirstOrDefaultAsync();
        
    }

    public async Task<List<LoanPolicy>> GetAllAsync()
    {
        return await _context.LoanPolicies.OrderByDescending(p => p.ValidFrom).ToListAsync();
        
    }

    public async Task AddAsync(LoanPolicy loanPolicy) =>
        await _context.LoanPolicies.AddAsync(loanPolicy);
    
    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }
}