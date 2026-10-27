using System.ComponentModel.DataAnnotations;

namespace PersonalFinanceTracker.BusinessLogic.DTOs;

public class UpdateCategoryRequest
{
    [Required]
    public string Name { get; set; } = string.Empty;
}