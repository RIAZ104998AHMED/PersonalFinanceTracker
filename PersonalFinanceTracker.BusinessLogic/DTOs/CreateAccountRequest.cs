using System.ComponentModel.DataAnnotations;

namespace PersonalFinanceTracker.BusinessLogic.DTOs;

public class CreateAccountRequest
{
    [Required]
    public string Name { get; set; } = string.Empty;

    [Required]
    [RegularExpression(
        "^[A-Z]{3}$",
        ErrorMessage = "Currency must be a three-letter uppercase code.")]
    public string Currency { get; set; } = string.Empty;

    public decimal StartAmount { get; set; }
}