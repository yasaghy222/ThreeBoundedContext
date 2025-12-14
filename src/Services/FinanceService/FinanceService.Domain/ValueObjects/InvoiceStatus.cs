namespace FinanceService.Domain.ValueObjects;

public enum InvoiceStatus
{
    Pending = 0,
    Paid = 1,
    Cancelled = 2,
    Overdue = 3
}
