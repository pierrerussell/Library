using Library.Domain.Books;
using MediatR;

namespace Library.Application.Books;

public record AddBookCommand(string Title, string Author, string Genre) : IRequest<Guid>;

public class AddBookHandler : IRequestHandler<AddBookCommand, Guid>
{
    private readonly IBookRepository _bookRepository;
    public AddBookHandler(IBookRepository bookRepository)
    {
        _bookRepository = bookRepository;
    }
    
    public async Task<Guid> Handle(AddBookCommand request, CancellationToken cancellationToken)
    {
        var book = new Book(request.Title, request.Author, request.Genre);
        await _bookRepository.AddAsync(book);
        await _bookRepository.SaveChangesAsync();
        return book.Id;
    }
}