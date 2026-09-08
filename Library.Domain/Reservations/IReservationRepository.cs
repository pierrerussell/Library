namespace Library.Domain.Reservations;

public interface IReservationRepository
{
    Task<Reservation?> GetByIdAsync(Guid id);
    Task<Reservation?> GetOldestPendingForBookAsync(Guid bookId);
    Task<List<Reservation>> GetByMemberIdAsync(Guid memberId);
    Task<List<Reservation>> GetAllActiveAsync();
    Task AddAsync(Reservation reservation);
    Task SaveChangesAsync();
    
}