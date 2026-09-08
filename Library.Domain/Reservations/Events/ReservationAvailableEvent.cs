using Library.Domain.Common;

namespace Library.Domain.Reservations.Events;

public record ReservationAvailableEvent(Guid ReservationId, Guid BookId, Guid MemberId): IDomainEvent;