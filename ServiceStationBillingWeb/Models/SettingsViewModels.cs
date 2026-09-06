using System.ComponentModel.DataAnnotations;

namespace ServiceStationBillingWeb.Models;

public sealed class SettingsViewModel
{
    [Required, StringLength(120)]
    [Display(Name = "Admin username")]
    public string AdminUsername { get; set; } = string.Empty;

    [Required, StringLength(160)]
    [DataType(DataType.Password)]
    [Display(Name = "Admin password")]
    public string AdminPassword { get; set; } = string.Empty;

    [Required, StringLength(120)]
    [Display(Name = "User username")]
    public string UserUsername { get; set; } = string.Empty;

    [Required, StringLength(160)]
    [DataType(DataType.Password)]
    [Display(Name = "User password")]
    public string UserPassword { get; set; } = string.Empty;
}