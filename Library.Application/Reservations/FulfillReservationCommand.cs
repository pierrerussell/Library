using Library.Application.Loans;
using Library.Domain.Reservations;
using MediatR;

namespace Library.Application.Reservations;

public record FulfillReservationCommand(Guid reservationId) : IRequest;

public class FulfillReservationHandler : IRequestHandler<FulfillReservationCommand>
{
    private readonly IReservationRepository _reservationRepository;
    private readonly IMediator _mediator;

    public FulfillReservationHandler(IReservationRepository reservationRepository,  IMediator mediator)
    {
        _reservationRepository = reservationRepository;
        _mediator = mediator;
    }
    
    public async Task Handle(FulfillReservationCommand request, CancellationToken cancellationToken)
    {
        var reservation = await _reservationRepository.GetByIdAsync(request.reservationId);
        if (reservation is null)
            throw new InvalidOperationException($"Reservation with id {request.reservationId} not found");
        
        // fulfilling reservation means loaning a book to the person
        try
        {
            await _mediator.Send(new CheckoutBookCommand(reservation.BookId, reservation.MemberId));
        }
        catch (InvalidOperationException e)
        {
            throw new InvalidOperationException(e.Message);
        }
        
        reservation.Fulfill();
        
        await _reservationRepository.SaveChangesAsync();
    }
}