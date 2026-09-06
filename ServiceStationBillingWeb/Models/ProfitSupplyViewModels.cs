using System.ComponentModel.DataAnnotations;

namespace ServiceStationBillingWeb.Models;

public sealed class ProfitStatementViewModel
{
    public string From { get; init; } = string.Empty;
    public string To { get; init; } = string.Empty;
    public decimal ServiceRevenue { get; init; }
    public decimal PartsRevenue { get; init; }
    public decimal Cogs { get; init; }
    public decimal GrossProfit { get; init; }
    public decimal Purchases { get; init; }
    public decimal NetProfit { get; init; }
    public IReadOnlyList<ProfitLineViewModel> Lines { get; init; } = [];
}

public sealed class ProfitLineViewModel
{
    public string Date { get; init; } = string.Empty;
    public string InvoiceNo { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public string ItemId { get; init; } = string.Empty;
    public decimal Quantity { get; init; }
    public decimal Revenue { get; init; }
    public decimal Cost { get; init; }
    public decimal Profit { get; init; }
}

public sealed class SupplyInvoiceListViewModel
{
    public string Search { get; init; } = string.Empty;
    public string PaymentStatus { get; init; } = string.Empty;
    public IReadOnlyList<SupplyInvoiceViewModel> Invoices { get; init; } = [];
}

public sealed class SupplyInvoiceViewModel
{
    public long Id { get; init; }
    public string InvoiceNo { get; init; } = string.Empty;
    public string SupplierCode { get; init; } = string.Empty;
    public string ItemId { get; init; } = string.Empty;
    public decimal BuyingPrice { get; init; }
    public decimal Quantity { get; init; }
    public decimal Total { get; init; }
    public string PaymentStatus { get; init; } = string.Empty;
    public string CreatedAt { get; init; } = string.Empty;
    public string ChequeNo { get; init; } = string.Empty;
    public string BranchCode { get; init; } = string.Empty;
    public string BankCode { get; init; } = string.Empty;
    public string ValueDate { get; init; } = string.Empty;
    public string SettledAt { get; init; } = string.Empty;
}

public sealed class SupplyInvoiceFormViewModel
{
    [Required, StringLength(80)]
    [Display(Name = "Invoice number")]
    public string InvoiceNo { get; set; } = string.Empty;

    [Required, StringLength(80)]
    [Display(Name = "Supplier code")]
    public string SupplierCode { get; set; } = string.Empty;

    [Required, StringLength(80)]
    [Display(Name = "Item ID")]
    public string ItemId { get; set; } = string.Empty;

    [Range(0.01, double.MaxValue)]
    [Display(Name = "Buying price")]
    public decimal BuyingPrice { get; set; }

    [Range(0.01, double.MaxValue)]
    public decimal Quantity { get; set; }

    [Required]
    [Display(Name = "Payment status")]
    public string PaymentStatus { get; set; } = "Cash";

    [StringLength(80)]
    [Display(Name = "Cheque number")]
    public string ChequeNo { get; set; } = string.Empty;

    [StringLength(40)]
    [Display(Name = "Branch code")]
    public string BranchCode { get; set; } = string.Empty;

    [StringLength(40)]
    [Display(Name = "Bank code")]
    public string BankCode { get; set; } = string.Empty;

    [DataType(DataType.Date)]
    [Display(Name = "Value date")]
    public DateTime? ValueDate { get; set; }
}