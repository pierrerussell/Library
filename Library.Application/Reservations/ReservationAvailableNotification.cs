using Library.Domain.Reservations;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Library.Application.Reservations;

public record ReservationAvailableNotification(Guid ReservationId, Guid MemberId, Guid BookId)
    : INotification;

public class ReservationAvailableNotificationHandler : INotificationHandler<ReservationAvailableNotification>
{
    private readonly ILogger<ReservationAvailableNotificationHandler> _logger;
    public ReservationAvailableNotificationHandler(ILogger<ReservationAvailableNotificationHandler> logger)
    {
        _logger = logger;
    }
    
    public Task Handle(ReservationAvailableNotification notification, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Reservation {ReservationId} for book {BookId} is available for member {MemberId}",
            notification.ReservationId, notification.BookId, notification.MemberId);
        return Task.CompletedTask;
    }
}