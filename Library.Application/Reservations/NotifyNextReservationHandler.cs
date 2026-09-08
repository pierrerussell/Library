using Library.Application.Loans;
using Library.Domain.Loans;
using Library.Domain.Loans.Events;
using Library.Domain.Reservations;
using Library.Domain.Reservations.Events;
using MediatR;

namespace Library.Application.Reservations;

public class NotifyNextReservationHandler : INotificationHandler<BookReturnedNotification>
{
    private readonly IReservationRepository _reservationRepository;
    private readonly IPublisher _publisher;
    
    public NotifyNextReservationHandler(IReservationRepository reservationRepository, IPublisher publisher)
    {
        _reservationRepository = reservationRepository;
        _publisher = publisher;
    }
    
    
    public async Task Handle(BookReturnedNotification notification, CancellationToken cancellationToken)
    {
        var reservation = await _reservationRepository.GetOldestPendingForBookAsync(notification.BookId);
        if (reservation is null) return;
        
        reservation.NotifyAvailable();

        foreach (var domainEvent in reservation.DomainEvents)
        {
            switch (domainEvent)
            {
                case ReservationAvailableEvent e:
                    await _publisher.Publish(
                        new ReservationAvailableNotification(e.ReservationId, e.BookId, e.MemberId), 
                        cancellationToken);
                    break;
            }
        }
        reservation.ClearDomainEvents();
        await _reservationRepository.SaveChangesAsync();
    }
}