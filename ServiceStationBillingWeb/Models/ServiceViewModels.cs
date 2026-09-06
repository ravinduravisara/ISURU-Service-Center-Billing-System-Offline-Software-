using System.ComponentModel.DataAnnotations;

namespace ServiceStationBillingWeb.Models;

public sealed class ServiceListViewModel
{
    public string Search { get; init; } = string.Empty;
    public IReadOnlyList<ServiceViewModel> Services { get; init; } = [];
}

public sealed class ServiceViewModel
{
    public int Id { get; init; }
    public string Category { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public decimal Price { get; init; }
    public string Description { get; init; } = string.Empty;
}

public sealed class ServiceFormViewModel
{
    public int Id { get; set; }

    [Required]
    [StringLength(80)]
    public string Category { get; set; } = "Others";

    [Required]
    [StringLength(120)]
    [Display(Name = "Service name")]
    public string Name { get; set; } = string.Empty;

    [Range(0, double.MaxValue)]
    public decimal Price { get; set; }

    [StringLength(500)]
    public string Description { get; set; } = string.Empty;
}