using MediatR;

namespace Library.Application.Loans;

public record BookReturnedNotification(Guid BookId, Guid CopyId, Guid MemberId, DateTimeOffset ReturnedOn)
    : INotification;
