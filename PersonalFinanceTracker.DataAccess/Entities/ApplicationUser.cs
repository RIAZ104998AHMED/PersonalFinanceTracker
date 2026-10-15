using Microsoft.AspNetCore.Identity;

namespace PersonalFinanceTracker.DataAccess.Entities;

public class ApplicationUser : IdentityUser<Guid>
{
    public string Name { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; }
}