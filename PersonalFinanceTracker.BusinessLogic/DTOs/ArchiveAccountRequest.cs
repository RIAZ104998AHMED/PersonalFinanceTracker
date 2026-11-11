using System.ComponentModel.DataAnnotations;

namespace PersonalFinanceTracker.BusinessLogic.DTOs;

public class ArchiveAccountRequest
{
    [Required]
    public bool? IsArchived { get; set; }
}