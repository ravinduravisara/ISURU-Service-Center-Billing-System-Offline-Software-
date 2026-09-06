using System.ComponentModel.DataAnnotations;

namespace ServiceStationBillingWeb.Models;

public sealed class LoginViewModel
{
    public string? ReturnUrl { get; set; }

    [Required]
    public string Username { get; set; } = string.Empty;

    [Required]
    [DataType(DataType.Password)]
    public string Password { get; set; } = string.Empty;
}