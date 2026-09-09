using Library.Domain.Reservations;
using MediatR;

namespace Library.Application.Reservations;

public record CancelReservationCommand(Guid reservationId) : IRequest;

public class CancelReservationHandler : IRequestHandler<CancelReservationCommand>
{
    private readonly IReservationRepository _reservationRepository;

    public CancelReservationHandler(IReservationRepository reservationRepository)
    {
        _reservationRepository = reservationRepository;
    }
    
    public async Task Handle(CancelReservationCommand request, CancellationToken cancellationToken)
    {
        var reservation = await _reservationRepository.GetByIdAsync(request.reservationId) ??
                          throw new InvalidOperationException($"Reservation with id {request.reservationId} does not exist.");
        
        reservation.Cancel();
        await _reservationRepository.SaveChangesAsync();
        
    }
}