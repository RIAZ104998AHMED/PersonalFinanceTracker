using System.ComponentModel.DataAnnotations;

namespace PersonalFinanceTracker.BusinessLogic.DTOs;

public class UpdateUserRequest
{
    [Required]
    public string Name { get; set; } = string.Empty;
}