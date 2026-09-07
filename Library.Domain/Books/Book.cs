using Library.Domain.Common;

namespace Library.Domain.Books;

public class Book : AggregateRoot
{
    public string Title { get; private set; } = string.Empty;
    public string Author { get; private set; } = string.Empty;
    public string Genre { get; private set; } = string.Empty;

    private readonly List<BookCopy> _copies = new();
    public IReadOnlyCollection<BookCopy> Copies => _copies.AsReadOnly();
    
    private Book() { }
    public Book(string title, string author, string genre)
    {
        Title = title;
        Author = author;
        Genre = genre;
    }

    public BookCopy AddCopy()
    {
        var copy = new BookCopy(Id);
        _copies.Add(copy);
        return copy;
    }

    public void RemoveCopy(Guid bookCopyId)
    {
        var copy = _copies.FirstOrDefault(c => c.Id == bookCopyId);
        if (copy is null)
            throw new InvalidOperationException($"Book copy with id {bookCopyId} does not exist.");
        if (copy.Status != BookCopyStatus.Available)
            throw new InvalidOperationException(
                $"Book copy with id {bookCopyId} is not available to be removed."
            );
        
        _copies.Remove(copy);
    }

    public bool HasAvailableCopy() => _copies.Any(c => c.Status == BookCopyStatus.Available);
    public BookCopy? GetAvailableCopy() => _copies.FirstOrDefault(c => c.Status == BookCopyStatus.Available);


}