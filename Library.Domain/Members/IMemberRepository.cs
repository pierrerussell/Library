namespace Library.Domain.Members;

public interface IMemberRepository
{
    Task<Member?> GetByIdentityUserIdAsync(Guid identityUserId);
    Task<Member?> GetByIdAsync(Guid id);
    
    Task<List<Member>> GetAllAsync();
    Task AddAsync(Member member);
    Task DeleteAsync(Member member);
    
    Task SaveChangesAsync();
    
}