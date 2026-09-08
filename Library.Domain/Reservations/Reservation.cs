using Library.Domain.Common;
using Library.Domain.Reservations.Events;

namespace Library.Domain.Reservations;

public enum ReservationStatus
{
    Pending,
    Notified,
    Fulfilled,
    Cancelled
}

public class Reservation : AggregateRoot
{
    public Guid BookId { get; private set; }
    public Guid MemberId { get; private set; }
    public DateTimeOffset ReservationDate { get; private set; }
    public ReservationStatus Status { get; private set; }
    
    private Reservation() {}
    
    public Reservation(Guid bookId, Guid memberId, DateTimeOffset reservationDate)
    {
        BookId = bookId;
        MemberId = memberId;
        ReservationDate = reservationDate;
        Status = ReservationStatus.Pending;
    }

    public void NotifyAvailable()
    {
        if (Status != ReservationStatus.Pending)
            throw new InvalidOperationException("Reservation is not pending.");
        Status = ReservationStatus.Notified;
        RaiseDomainEvent(new ReservationAvailableEvent(Id, BookId, MemberId));
    }
    
    public void Fulfill()
    {
        Status = ReservationStatus.Fulfilled;
    }
    
    public void Cancel()
    {
        Status = ReservationStatus.Cancelled;
    }
    
}