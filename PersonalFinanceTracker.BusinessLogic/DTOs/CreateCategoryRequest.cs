using System.ComponentModel.DataAnnotations;
using PersonalFinanceTracker.DataAccess.Entities;

namespace PersonalFinanceTracker.BusinessLogic.DTOs;

public class CreateCategoryRequest
{
    [Required]
    public string Name { get; set; } = string.Empty;

    [Required]
    public OperationType? Type { get; set; }
}