using Library.Domain.Loans;
using Library.Domain.Members;
using Library.Domain.Reservations;

namespace Library.Application.Members;

public record MemberDashboardDto(
    Member Member,
    List<Loan> ActiveLoans,
    List<Loan> PastLoans,
    List<Reservation> Reservations,
    List<Payment> Payments);