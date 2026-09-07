using Library.Domain.Books;
using MediatR;

namespace Library.Application.Books;

public record AddCopyCommand(Guid BookId) : IRequest<Guid>;

public class AddCopyHandler : IRequestHandler<AddCopyCommand, Guid>
{
    private readonly IBookRepository _bookRepository;
    public AddCopyHandler(IBookRepository bookRepository)
    {
        _bookRepository = bookRepository;
    }
    
    public async Task<Guid> Handle(AddCopyCommand request, CancellationToken cancellationToken)
    {
        var book = await _bookRepository.GetByIdAsync(request.BookId);
        if (book is null)
            throw new InvalidOperationException($"Book with id {request.BookId} does not exist.");
        var copy = book.AddCopy();
        await _bookRepository.SaveChangesAsync();
        return copy.Id;
    }
}