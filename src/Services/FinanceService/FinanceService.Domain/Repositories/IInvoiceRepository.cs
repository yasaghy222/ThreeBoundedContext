using FinanceService.Domain.Entities;

namespace FinanceService.Domain.Repositories;

public interface IInvoiceRepository
{
	Task<Invoice?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
	Task<Invoice?> GetByBookingIdAsync(Guid bookingId, CancellationToken cancellationToken = default);
	Task<IEnumerable<Invoice>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
	Task<bool> ExistsByBookingIdAsync(Guid bookingId, CancellationToken cancellationToken = default);
	Task AddAsync(Invoice invoice, CancellationToken cancellationToken = default);
	Task UpdateAsync(Invoice invoice, CancellationToken cancellationToken = default);
	Task<IEnumerable<Invoice>> GetAllAsync(CancellationToken cancellationToken = default);
}
