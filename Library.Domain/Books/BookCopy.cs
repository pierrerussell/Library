using Library.Domain.Common;

namespace Library.Domain.Books;

public enum BookCopyStatus
{
    Available,
    OnLoan,
}

public class BookCopy : Entity
{
    public Guid BookId { get; private set; }
    public BookCopyStatus Status { get; private set; }
    
    private BookCopy() {}

    public BookCopy(Guid bookId)
    {
        BookId = bookId;
        Id = Guid.NewGuid();
        Status = BookCopyStatus.Available;
    }

    public void SetOnLoan()
    {
        if (Status != BookCopyStatus.Available)
            throw new InvalidOperationException("Book copy is not available to be loaned.");
        Status = BookCopyStatus.OnLoan;
    }
    
    public void SetAvailable()
    {
        Status = BookCopyStatus.Available;
    }
    
    
}