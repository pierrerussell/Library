using Library.Domain.Members;
using Microsoft.EntityFrameworkCore;

namespace Library.Blazor.Data.Repositories.Members;

public class MemberRepository : IMemberRepository
{
    private readonly ApplicationDbContext _context;
    
    public MemberRepository(ApplicationDbContext context)
    {
        _context = context;
    }
    
    public async Task<Member?> GetByIdentityUserIdAsync(Guid identityUserId)
    {
        return await _context.Members.FirstOrDefaultAsync(m => m.IdentityUserId == identityUserId);
    }

    public async Task<Member?> GetByIdAsync(Guid id)
    {
        return await _context.Members.FirstOrDefaultAsync(m => m.Id == id);
    }

    public async Task<List<Member>> GetAllAsync()
    {
        return await _context.Members.ToListAsync();
    }

    public async Task AddAsync(Member member)
    {
        await _context.Members.AddAsync(member);
    }

    public async Task DeleteAsync(Member member)
    {
        _context.Members.Remove(member);
    }

    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }
}