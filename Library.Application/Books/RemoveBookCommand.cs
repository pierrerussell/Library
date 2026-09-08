using Library.Domain.Books;
using MediatR;

namespace Library.Application.Books;

public record RemoveBookCommand(Guid bookId) : IRequest;

public class RemoveBookHandler : IRequestHandler<RemoveBookCommand>
{
    private readonly IBookRepository _repository;

    public RemoveBookHandler(IBookRepository repository)
    {
        _repository = repository;
    }
    
    public async Task Handle(RemoveBookCommand request, CancellationToken cancellationToken)
    {
        var book = await _repository.GetByIdAsync(request.bookId);
        if (book is null)
            throw new InvalidOperationException($"Book with id {request.bookId} does not exist");
        
        // cannot delete a book that still has copies on loan
        if (book.Copies.Count(c => c.Status == BookCopyStatus.OnLoan) > 0)
            throw new InvalidOperationException("Cannot delete a book that has copies on loan");

        await _repository.DeleteAsync(book);
        await _repository.SaveChangesAsync();

    }
}