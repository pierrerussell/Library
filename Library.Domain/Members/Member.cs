using Library.Domain.Common;

namespace Library.Domain.Members;

public class Member : AggregateRoot
{
    public Guid IdentityUserId { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public string Email { get; private set; } = string.Empty;

    private List<string> _interests = new();
    public IReadOnlyCollection<string> Interests => _interests.AsReadOnly();
    
    private Member() { }
    public Member(Guid identityUserId, string name, string email)
    {
        IdentityUserId = identityUserId;
        Name = name;
        Email = email;
    }
    
    public void AddInterest(string interest)
    {
        _interests = _interests.Append(interest).ToList();
    }

    public void RemoveInterest(string interest)
    {
        _interests = _interests
            .Where(x => x != interest)
            .ToList();
    }
    
    public void UpdateName(string name) => Name = name;
    public void UpdateEmail(string email) => Email = email;
    
}