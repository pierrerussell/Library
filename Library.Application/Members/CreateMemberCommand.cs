using Library.Domain.Members;
using MediatR;

namespace Library.Application.Members;

public record CreateMemberCommand(Guid IdentityUserId, string Name, string Email): IRequest<Guid>;

public class CreateMemberHandler : IRequestHandler<CreateMemberCommand, Guid>
{
    private readonly IMemberRepository _memberRepository;
    public CreateMemberHandler(IMemberRepository memberRepository)
    {
        _memberRepository = memberRepository;
    }
    
    public async Task<Guid> Handle(CreateMemberCommand request, CancellationToken cancellationToken)
    {
        var member = new Member(request.IdentityUserId, request.Name, request.Email);
        await _memberRepository.AddAsync(member);
        await _memberRepository.SaveChangesAsync();
        return member.Id;
    }
}