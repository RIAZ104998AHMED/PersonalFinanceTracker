using System.ComponentModel.DataAnnotations;

namespace PersonalFinanceTracker.BusinessLogic.DTOs;

public class CreateTransferRequest
{
    [Required]
    public Guid FromAccountId { get; set; }

    [Required]
    public Guid ToAccountId { get; set; }

    [Range(
        0.01,
        double.MaxValue,
        ErrorMessage = "Amount must be greater than zero.")]
    public decimal Amount { get; set; }

    [Required]
    public DateOnly Date { get; set; }

    public string? Comment { get; set; }
}