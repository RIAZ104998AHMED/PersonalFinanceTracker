using System.ComponentModel.DataAnnotations;
using PersonalFinanceTracker.DataAccess.Entities;

namespace PersonalFinanceTracker.BusinessLogic.DTOs;

public class UpdateOperationRequest
{
    [Required]
    public Guid AccountId { get; set; }

    [Required]
    public Guid CategoryId { get; set; }

    [Required]
    public OperationType? Type { get; set; }

    [Range(
        0.01,
        double.MaxValue,
        ErrorMessage = "Amount must be greater than zero.")]
    public decimal Amount { get; set; }

    [Required]
    public DateOnly Date { get; set; }

    public string? Comment { get; set; }
}