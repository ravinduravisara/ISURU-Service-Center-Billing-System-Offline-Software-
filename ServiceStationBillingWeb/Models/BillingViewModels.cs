using System.ComponentModel.DataAnnotations;

namespace ServiceStationBillingWeb.Models;

public sealed class BillingViewModel
{
    [Required]
    [Display(Name = "Bill number")]
    public string BillNo { get; set; } = $"INV-{DateTime.Now:yyyyMMddHHmmss}";

    [Required]
    [Display(Name = "Customer")]
    public int CustomerId { get; set; }

    [Display(Name = "Customer name")]
    public string CustomerName { get; set; } = string.Empty;

    [Display(Name = "Contact")]
    public string CustomerPhone { get; set; } = string.Empty;

    [Required]
    [Display(Name = "Vehicle number")]
    public string VehicleNo { get; set; } = string.Empty;

    [Display(Name = "Vehicle type")]
    public string VehicleType { get; set; } = string.Empty;

    [Range(0, double.MaxValue)]
    public decimal Discount { get; set; }

    public decimal Subtotal { get; set; }
    public decimal TotalAmount { get; set; }

    [Display(Name = "Payment method")]
    public string PaymentMethod { get; set; } = "Cash";

    public IReadOnlyList<CustomerOption> Customers { get; set; } = [];
    public IReadOnlyList<ServiceOption> Services { get; set; } = [];
    public List<BillingLineItemViewModel> Items { get; set; } = [new()];
}

public sealed class CustomerOption
{
    public int Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string Phone { get; init; } = string.Empty;
}

public sealed class ServiceOption
{
    public int Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public decimal Price { get; init; }
    public string Description { get; init; } = string.Empty;
}

public sealed class BillingLineItemViewModel
{
    public string Service { get; set; } = string.Empty;
    public string ItemId { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;

    [Range(0, double.MaxValue)]
    public decimal Quantity { get; set; } = 1;

    [Range(0, double.MaxValue)]
    public decimal Rate { get; set; }

    public decimal Amount { get; set; }
}

public sealed class InvoiceListViewModel
{
    public string Search { get; init; } = string.Empty;
    public string From { get; init; } = string.Empty;
    public string To { get; init; } = string.Empty;
    public string PaymentMethod { get; init; } = string.Empty;
    public bool CreditorsOnly { get; init; }
    public decimal FilteredTotal { get; init; }
    public decimal CashTotal { get; init; }
    public decimal ChequeTotal { get; init; }
    public decimal CreditTotal { get; init; }
    public IReadOnlyList<InvoiceViewModel> Invoices { get; init; } = [];
}

public sealed class InvoiceViewModel
{
    public int Id { get; init; }
    public string BillNo { get; init; } = string.Empty;
    public string CustomerName { get; init; } = string.Empty;
    public string VehicleNo { get; init; } = string.Empty;
    public string DateTime { get; init; } = string.Empty;
    public decimal TotalAmount { get; init; }
    public decimal Subtotal { get; init; }
    public decimal Discount { get; init; }
    public string PaymentMethod { get; init; } = string.Empty;
    public string PaymentStatus { get; init; } = string.Empty;
    public string PaymentSettledAt { get; init; } = string.Empty;
}

public sealed class InvoiceDetailViewModel
{
    public InvoiceViewModel Invoice { get; init; } = new();
    public string CustomerPhone { get; init; } = string.Empty;
    public string VehicleType { get; init; } = string.Empty;
    public IReadOnlyList<BillingLineItemViewModel> Items { get; init; } = [];
}