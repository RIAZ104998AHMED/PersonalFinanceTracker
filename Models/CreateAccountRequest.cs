using System.ComponentModel.DataAnnotations;

namespace PersonalFinanceTracker.Api.Models
{
    public record CreateAccountRequest(
        [property: Required, MaxLength(100)]
        string Name,

        decimal Balance
    );
}