using Library.Domain.Common;

namespace Library.Domain.Loans.Events;

public record BookReturnedEvent(Guid BookId, Guid CopyId, Guid MemberId, DateTimeOffset ReturnedOn) : IDomainEvent;