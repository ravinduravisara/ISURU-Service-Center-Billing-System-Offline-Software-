using System.ComponentModel.DataAnnotations;

namespace ServiceStationBillingWeb.Models;

public sealed class InventoryListViewModel
{
    public string Search { get; init; } = string.Empty;
    public IReadOnlyList<InventoryViewModel> Items { get; init; } = [];
}

public sealed class InventoryViewModel
{
    public int Id { get; init; }
    public string ItemId { get; init; } = string.Empty;
    public string ItemName { get; init; } = string.Empty;
    public string Brand { get; init; } = string.Empty;
    public decimal Quantity { get; init; }
    public decimal UnitPrice { get; init; }
    public decimal BuyingPrice { get; init; }
    public string UpdatedAt { get; init; } = string.Empty;
    public string SupplierCode { get; init; } = string.Empty;
    public string PaymentMethod { get; init; } = string.Empty;
}

public sealed class InventoryFormViewModel
{
    public int Id { get; set; }

    [StringLength(80)]
    [Display(Name = "Item ID")]
    public string ItemId { get; set; } = string.Empty;

    [Required]
    [StringLength(160)]
    [Display(Name = "Item name")]
    public string ItemName { get; set; } = string.Empty;

    [StringLength(120)]
    public string Brand { get; set; } = string.Empty;

    [Range(0, double.MaxValue)]
    public decimal Quantity { get; set; }

    [Range(0, double.MaxValue)]
    [Display(Name = "Unit price")]
    public decimal UnitPrice { get; set; }

    [Range(0, double.MaxValue)]
    [Display(Name = "Buying price")]
    public decimal BuyingPrice { get; set; }

    [StringLength(80)]
    [Display(Name = "Supplier code")]
    public string SupplierCode { get; set; } = string.Empty;

    [StringLength(40)]
    [Display(Name = "Payment method")]
    public string PaymentMethod { get; set; } = "Cash";
}