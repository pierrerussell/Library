using Library.Domain.Books;
using MediatR;

namespace Library.Application.Books;

public record RemoveCopyCommand(Guid BookId, Guid BookCopyId) : IRequest<Guid>;

public class RemoveCopyHandler : IRequestHandler<RemoveCopyCommand, Guid>
{
    private readonly IBookRepository _bookRepository;
    public RemoveCopyHandler(IBookRepository bookRepository)
    {
        _bookRepository = bookRepository;
    }
    
    public async Task<Guid> Handle(RemoveCopyCommand request, CancellationToken cancellationToken)
    {
        var book = await _bookRepository.GetByIdAsync(request.BookId);
        if (book is null)
            throw new InvalidOperationException($"Book with id {request.BookId} does not exist.");
        book.RemoveCopy(request.BookCopyId);
        await _bookRepository.SaveChangesAsync();
        return book.Id;
        
    }
}

