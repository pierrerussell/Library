using Library.Domain.Reservations;
using Microsoft.EntityFrameworkCore;

namespace Library.Blazor.Data.Repositories.Reservations;

public class ReservationRepository : IReservationRepository
{
    private readonly ApplicationDbContext _context;
    public ReservationRepository(ApplicationDbContext context)
    {
        _context = context;
    }
    
    public async Task<Reservation?> GetByIdAsync(Guid id)
    {
        return await _context.Reservations.Where(x => x.Id == id).FirstOrDefaultAsync();
    }

    public async Task<Reservation?> GetOldestPendingForBookAsync(Guid bookId)
    {
        return await _context.Reservations
            .Where(x => x.BookId == bookId && x.Status == ReservationStatus.Pending)
            .OrderBy(x => x.ReservationDate)
            .FirstOrDefaultAsync();
    }

    public async Task<List<Reservation>> GetByMemberIdAsync(Guid memberId)
    {
        return await _context.Reservations.Where(x => x.MemberId == memberId).ToListAsync();
    }

    public async Task<List<Reservation>> GetAllActiveAsync()
    {
        return await _context.Reservations.Where(x => x.Status == ReservationStatus.Pending).ToListAsync();
    }

    public async Task AddAsync(Reservation reservation)
    {
        await _context.Reservations.AddAsync(reservation);
    }

    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }
}