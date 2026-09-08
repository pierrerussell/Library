using Library.Domain.Loans;
using Microsoft.EntityFrameworkCore;

namespace Library.Blazor.Data.Repositories.Loans;

public class LoanRepository : ILoanRepository
{
    private readonly ApplicationDbContext _context;
    public LoanRepository(ApplicationDbContext context)
    {
        _context = context;
    }
    
    public async Task<Loan?> GetByIdAsync(Guid id)
    {
        return await _context.Loans
            .FirstOrDefaultAsync(l => l.Id == id);
    }

    public async Task<List<Loan>> GetByMemberIdAsync(Guid memberId)
    {
        var loans = await _context.Loans.Where(l => l.MemberId == memberId).ToListAsync();
        foreach (var loan in loans)
        {
            if (loan.Status == LoanStatus.Overdue || loan.Status == LoanStatus.Returned)
                continue;
            if (loan.DueDate < DateTimeOffset.UtcNow)
            {
                loan.SetOverdue(DateTimeOffset.UtcNow);
            }
        }

        await _context.SaveChangesAsync();
        
        return await _context.Loans
            .Where(l => l.MemberId == memberId)
            .OrderByDescending(l => l.CheckoutDate)
            .ToListAsync();
    }

    public async Task<List<Loan>> GetAllActiveAsync()
    {
        return await _context.Loans
            .Where(l => l.Status != LoanStatus.Returned)
            .OrderByDescending(l => l.CheckoutDate)
            .ToListAsync();
        
    }

    public async Task AddAsync(Loan loan)
    {
        await _context.Loans.AddAsync(loan);   
    }

    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();   
    }
}