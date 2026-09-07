namespace Library.Domain.Books;

public interface IBookRepository
{
    Task<Book?> GetByIdAsync(Guid id);
    Task<List<Book>> GetAllAsync();
    Task AddAsync(Book book);

    Task DeleteAsync(Book book);
    
    Task SaveChangesAsync();
    
}