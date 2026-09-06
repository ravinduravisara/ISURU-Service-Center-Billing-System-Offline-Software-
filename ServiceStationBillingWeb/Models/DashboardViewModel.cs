namespace ServiceStationBillingWeb.Models;

public sealed class DashboardViewModel
{
    public int Customers { get; init; }
    public int Vehicles { get; init; }
    public int Invoices { get; init; }
    public int InventoryItems { get; init; }
    public decimal Revenue { get; init; }
    public IReadOnlyList<RecentInvoice> RecentInvoices { get; init; } = [];
}

public sealed class RecentInvoice
{
    public string BillNo { get; init; } = string.Empty;
    public string CustomerName { get; init; } = string.Empty;
    public string DateTime { get; init; } = string.Empty;
    public decimal TotalAmount { get; init; }
    public string PaymentStatus { get; init; } = string.Empty;
}