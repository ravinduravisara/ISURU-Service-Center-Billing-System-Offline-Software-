using System.ComponentModel.DataAnnotations;

namespace ServiceStationBillingWeb.Models;

public sealed class SupplierListViewModel
{
    public string Search { get; init; } = string.Empty;
    public IReadOnlyList<SupplierViewModel> Suppliers { get; init; } = [];
}

public sealed class SupplierViewModel
{
    public string SupplierCode { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public string Contact { get; init; } = string.Empty;
    public decimal CashTotal { get; init; }
    public decimal CreditTotal { get; init; }
    public decimal ChequeTotal { get; init; }
    public string Invoice { get; init; } = string.Empty;
}

public sealed class SupplierFormViewModel
{
    public string OriginalSupplierCode { get; set; } = string.Empty;

    [Required, StringLength(80)]
    [Display(Name = "Supplier code")]
    public string SupplierCode { get; set; } = string.Empty;

    [Required, StringLength(160)]
    public string Name { get; set; } = string.Empty;

    [StringLength(80)]
    public string Contact { get; set; } = string.Empty;

    [StringLength(160)]
    public string Invoice { get; set; } = string.Empty;
}