using System.ComponentModel.DataAnnotations;

namespace PersonalFinanceTracker.BusinessLogic.DTOs;

public record CreateAccountRequest(
    [property: Required, MaxLength(100)]
    string Name,

    decimal Balance
);