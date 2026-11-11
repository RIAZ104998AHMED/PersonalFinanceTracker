using System.ComponentModel.DataAnnotations;

namespace PersonalFinanceTracker.BusinessLogic.DTOs;

public class CreateTransferRequest
{
    [Required]
    public Guid? FromAccountId { get; set; }

    [Required]
    public Guid? ToAccountId { get; set; }

    public decimal Amount { get; set; }

    [Required]
    public DateOnly? Date { get; set; }

    [StringLength(500)]
    public string? Comment { get; set; }
}