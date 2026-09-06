using System.ComponentModel.DataAnnotations;

namespace ServiceStationBillingWeb.Models;

public sealed class CustomerListViewModel
{
    public string Search { get; init; } = string.Empty;
    public IReadOnlyList<CustomerViewModel> Customers { get; init; } = [];
}

public sealed class CustomerViewModel
{
    public int Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string Phone { get; init; } = string.Empty;
    public int Vehicles { get; init; }
}

public sealed class CustomerFormViewModel
{
    public int Id { get; set; }

    [Required]
    [StringLength(120)]
    [Display(Name = "Customer name")]
    public string Name { get; set; } = string.Empty;

    [StringLength(40)]
    public string Phone { get; set; } = string.Empty;
}