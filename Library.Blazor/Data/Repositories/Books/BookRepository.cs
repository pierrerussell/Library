using Library.Domain.Books;
using Microsoft.EntityFrameworkCore;

namespace Library.Blazor.Data.Repositories.Books;

public class BookRepository : IBookRepository
{
    private readonly ApplicationDbContext _context;
    
    public BookRepository(ApplicationDbContext context)
    {
        _context = context;
    }
    
    public async Task<Book?> GetByIdAsync(Guid id)
    {
        return await _context.Books
            .Include(b => b.Copies)
            .FirstOrDefaultAsync(b => b.Id == id);
    }

    public async Task<List<Book>> GetAllAsync()
    {
        return await _context.Books
            .Include(b => b.Copies)
            .ToListAsync();
    }

    public async Task AddAsync(Book book)
    {
        await _context.Books.AddAsync(book);
    }

    public async Task DeleteAsync(Book book)
    {
        _context.Books.Remove(book);
    }


    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }
}