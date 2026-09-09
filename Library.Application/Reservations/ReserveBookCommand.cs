using Library.Domain.Books;
using Library.Domain.Reservations;
using MediatR;

namespace Library.Application.Reservations;

public record ReserveBookCommand(Guid BookId, Guid MemberId) : IRequest<Guid>;

public class ReserveBookHandler : IRequestHandler<ReserveBookCommand, Guid>
{
    private readonly IReservationRepository _reservationRepository;
    private readonly IBookRepository _bookRepository;
    public ReserveBookHandler(IReservationRepository reservationRepository, IBookRepository bookRepository)
    {
        _reservationRepository = reservationRepository;
        _bookRepository = bookRepository;
    }
    public async Task<Guid> Handle(ReserveBookCommand request, CancellationToken cancellationToken)
    {
        var book = await _bookRepository.GetByIdAsync(request.BookId) ??
                   throw new InvalidOperationException("Book not found.");

        if (book.HasAvailableCopy())
            throw new InvalidOperationException("Reservation not needed - copies available.");
        
        // check if member already has a reservation for this book that is still active
        var memberReservations = await _reservationRepository.GetByMemberIdAsync(request.MemberId);
        if (memberReservations.Any(r => 
                r.BookId == request.BookId &&
                r.Status == ReservationStatus.Pending))
            throw new InvalidOperationException("Member already has a reservation for this book.");
        
        var reservation = new Reservation(request.BookId, request.MemberId, DateTimeOffset.UtcNow);
        await _reservationRepository.AddAsync(reservation);
        await _reservationRepository.SaveChangesAsync();
        
        return reservation.Id;
    }
}