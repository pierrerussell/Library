using Library.Domain.Loans;
using Library.Domain.Members;
using Library.Domain.Reservations;
using MediatR;

namespace Library.Application.Members;

public record GetMemberDashboardQuery(Guid MemberId) : IRequest<MemberDashboardDto>;

public class GetMemberDashboardHandler : IRequestHandler<GetMemberDashboardQuery, MemberDashboardDto>
{
    private readonly IMemberRepository _memberRepository;
    private readonly ILoanRepository _loanRepository;
    private readonly IReservationRepository _reservationRepository;
    private readonly IPaymentRepository _paymentRepository;
    
    public GetMemberDashboardHandler(IMemberRepository memberRepository, ILoanRepository loanRepository, IReservationRepository reservationRepository, IPaymentRepository paymentRepository)
    {
        _memberRepository = memberRepository;
        _loanRepository = loanRepository;
        _reservationRepository = reservationRepository;
        _paymentRepository = paymentRepository;
    }
    
    public async Task<MemberDashboardDto> Handle(GetMemberDashboardQuery request, CancellationToken cancellationToken)
    {
        var member = await _memberRepository.GetByIdAsync(request.MemberId) ??
                     throw new InvalidOperationException("Member not found.");
        
        var allLoans = await _loanRepository.GetByMemberIdAsync(request.MemberId);
        var activeLoans = allLoans.Where(l => l.Status != LoanStatus.Returned).ToList();
        var pastLoans = allLoans.Where(l => l.Status == LoanStatus.Returned).ToList();
        
        var reservations = await _reservationRepository.GetByMemberIdAsync(request.MemberId);
        var pendingReservations = reservations.Where(r => r.Status is ReservationStatus.Pending or ReservationStatus.Notified).ToList();
        
        var payments = await _paymentRepository.GetByMemberIdAsync(request.MemberId);
        
        return new MemberDashboardDto(member, activeLoans, pastLoans, pendingReservations, payments);
        
    }
}